using Ingestion.Interfaces;
using Ingestion.Services;
using Ingestion.Sources.Csv;
using Microsoft.Extensions.DependencyInjection;

namespace Ingestion.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIngestion(this IServiceCollection svcs)
    {
        svcs.AddSingleton<IIngestionService, IngestionService>();
        svcs.AddSingleton<IIngestionSource, CsvIngestionSource>();
        // register IMappingResolver implementation elsewhere
        return svcs;
    }
}