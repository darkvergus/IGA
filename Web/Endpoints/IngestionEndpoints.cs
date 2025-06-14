using System.Text.Json;
using Core.Entities;
using Database.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Endpoints;

public static class IngestionEndpoints
{
    public static void MapIngestionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/ingestion").WithTags("Ingestion");

        group.MapPost("/csv/{entity}", IngestCsv).WithName("CsvIngestion").WithSummary("Ingest a CSV file for the given entity type.")
            .DisableAntiforgery();
    }

    public static async Task<IResult> IngestCsv(string entity, IFormFile file, [FromServices] IgaDbContext db, [FromServices] IWebHostEnvironment env,
        CancellationToken cancellationToken)
    {
        string guid = Guid.NewGuid().ToString("N");
        string upload = Path.Combine(env.WebRootPath, "Uploads", $"{guid}.csv");
        Directory.CreateDirectory(Path.GetDirectoryName(upload)!);

        await using FileStream fileStream = File.Create(upload);
        await file.CopyToAsync(fileStream, cancellationToken);

        ConnectorConfig? cfg = await db.ConnectorConfigs.SingleOrDefaultAsync(config => config.ConnectorName == "CsvCollector", cancellationToken);

        string json = JsonSerializer.Serialize(new
        {
            Path = Path.GetRelativePath(env.WebRootPath, upload), Delimiter = ",", Entity = entity
        });

        if (cfg is null)
        {
            cfg = new ConnectorConfig
            {
                ConnectorName = "CsvCollector",
                ConnectorType = "Collector",
                IsEnabled = true,
                ConfigData = json,
                CreatedAt = DateTime.UtcNow
            };
            db.Add(cfg);
        }
        else
        {
            cfg.ConfigData = json;
            cfg.ModifiedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        
        return Results.Ok(new { Status = "Queued", Entity = entity });
    }
}