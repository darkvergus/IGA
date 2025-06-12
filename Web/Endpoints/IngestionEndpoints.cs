using Core.Domain.Dynamic;
using Database.Context;
using Ingestion.Mapping;
using Ingestion.Pipeline;
using Ingestion.Repository;
using Ingestion.Source;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;

namespace Web.Endpoints;

public static class IngestionEndpoints
{
    public static void MapIngestionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/ingestion").WithTags("Ingestion");

        group.MapPost("/csv/{entity}", IngestCsv).WithName("CsvIngestion").WithSummary("Ingest a CSV file for the given entity type.")
            .DisableAntiforgery();
    }

    private static async Task<IResult> IngestCsv(string entity, IFormFile file, [FromServices] IngestionPipeline pipeline, [FromServices] IgaDbContext db, [FromServices] IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        string tmp = Path.Combine(environment.ContentRootPath, "uploads", $"{Guid.NewGuid():N}.csv");
        await using (FileStream fs = File.Create(tmp))
        {
            await file.CopyToAsync(fs, cancellationToken);
        }

        await IngestionExtensions.RunCsvAsync(entity, tmp, pipeline, db, cancellationToken);
        File.Delete(tmp);

        return Results.Ok(new { Status = "Imported", Entity = entity });
    }
}