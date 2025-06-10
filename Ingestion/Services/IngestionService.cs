using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks.Dataflow;
using Database.Context;
using Database.Extensions;
using Ingestion.Extensions;
using Ingestion.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLinq;

namespace Ingestion.Services;

public sealed class IngestionService : IIngestionService
{
    private static readonly MethodInfo BulkGeneric = typeof(IgaDbContextBulkExtensions).GetMethod(nameof(IgaDbContextBulkExtensions.BulkUpsertAsync))!;
    
    private readonly IReadOnlyDictionary<string, IIngestionSource> sources;
    private readonly IServiceProvider sp;
    private readonly ILogger<IngestionService> log;

    public IngestionService(IEnumerable<IIngestionSource> sources,  IServiceProvider sp, ILogger<IngestionService> log)
    {
        this.sources = sources.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
        this.sp = sp;
        this.log = log;
    }

    public async Task<ImportReport> IngestAsync(Type entityType, string sourceName, IEnumerable<IImportFilter> filters,
        CancellationToken cancellationToken = default)
    {
        if (!sources.TryGetValue(sourceName, out IIngestionSource? src))
        {
            throw new InvalidOperationException($"No ingestion source named '{sourceName}'.");
        }

        Stopwatch sw = Stopwatch.StartNew();
        int inCount = 0, outCount = 0, inserted = 0, updated = 0, skipped = 0, errors = 0;

        BufferBlock<object> buffer = new(new DataflowBlockOptions { BoundedCapacity = 10000 });

        TransformManyBlock<object, object> filterBlock = new(obj =>
        {
            Interlocked.Increment(ref inCount);

            if (ApplyFilters(obj, filters))
            {
                Interlocked.Increment(ref outCount);

                return [obj];
            }

            Interlocked.Increment(ref skipped);

            return [];
        }, new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 10000,
            MaxDegreeOfParallelism = Environment.ProcessorCount
        });
        
        BatchBlock<object> batchBlock = new(5000);
        
        ActionBlock<object[]> bulkBlock = new(async batchObjs =>
        {
            try
            {
                using IServiceScope scope = sp.CreateScope();
                IgaDbContext db = scope.ServiceProvider.GetRequiredService<IgaDbContext>();

                await BulkWriteAsync(entityType, batchObjs, db, cancellationToken);
                Interlocked.Add(ref inserted, batchObjs.Length);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Bulk write failed");
                Interlocked.Increment(ref errors);
            }
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

        DataflowLinkOptions linkOpts = new() { PropagateCompletion = true };
        buffer.LinkTo(filterBlock, linkOpts);
        filterBlock.LinkTo(batchBlock, linkOpts);
        batchBlock.LinkTo(bulkBlock, linkOpts);

        await foreach (object obj in src.FetchAsync(entityType, cancellationToken))
        {
            await buffer.SendAsync(obj, cancellationToken);
        }

        buffer.Complete();
        await bulkBlock.Completion.ConfigureAwait(false);

        sw.Stop();

        return new ImportReport(inCount, outCount, inserted, updated, skipped, errors, sw.Elapsed);
    }

    private static bool ApplyFilters(object obj, IEnumerable<IImportFilter> filters)
    {
        if (!filters.AsValueEnumerable().Any())
        {
            return true;
        }

        KeyValuePair<string, string>[] kvps = obj.ToKeyValuePairs();

        return filters.AsValueEnumerable().All(filter => filter.Accept(kvps));
    }

    private static Task BulkWriteAsync(Type entityType, object[] batch, IgaDbContext db, CancellationToken ct)
    {
        Type listType = typeof(List<>).MakeGenericType(entityType);
        IList list = (IList)Activator.CreateInstance(listType)!;
        foreach (object o in batch)
        {
            list.Add(o);
        }

        MethodInfo method = BulkGeneric.MakeGenericMethod(entityType);
        return (Task)method.Invoke(null, [db, list, ct])!;
    }
}