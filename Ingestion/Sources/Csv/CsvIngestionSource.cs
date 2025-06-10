using System.Globalization;
using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;
using Ingestion.Interfaces;
using Ingestion.Options.Csv;
using Microsoft.Extensions.Options;

namespace Ingestion.Sources.Csv;

public sealed class CsvIngestionSource(IOptions<CsvOptions> opts, IMappingResolver resolver) : IIngestionSource
{
    private readonly CsvOptions opts = opts.Value;

    public string Name => "csv";

    public async IAsyncEnumerable<object> FetchAsync(Type entityType, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!opts.Paths.TryGetValue(entityType.Name, out string? path))
        {
            throw new InvalidOperationException($"No CSV path configured for '{entityType.Name}'.");
        }

        IRowMapper map = await resolver.ResolveAsync(entityType, opts.MappingId, ct);

        using StreamReader reader = new(path);
        using CsvReader csv = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            MissingFieldFound = null,
            BadDataFound = null,
            DetectDelimiter = true,
        });

        while (await csv.ReadAsync())
        {
            dynamic rec = csv.GetRecord<dynamic>();
            dynamic? entity = map.Convert(rec);
            yield return entity;
        }
    }
}