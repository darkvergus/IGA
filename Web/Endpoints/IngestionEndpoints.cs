using Host.Interfaces; 
using Host.Job;
using Microsoft.AspNetCore.Mvc;

namespace Web.Endpoints;

public static class IngestionEndpoints
{
    public static void MapIngestionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/ingestion").WithTags("Ingestion");

        // POST /api/ingestion/{connector}/{entity}
        group.MapPost("/{connectorName}/{entity}", Ingest).WithName("GenericIngestion").WithSummary("Upload data for any connector").DisableAntiforgery();
    }

    private static async Task<IResult> Ingest(string connectorName, string entity,IFormFile? file, [FromServices] IWebHostEnvironment environment,
        [FromServices] IConnectorQueue queue, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(connectorName))
        {
            return Results.BadRequest("connectorName missing");
        }

        if (string.IsNullOrWhiteSpace(entity))
        {
            return Results.BadRequest("entity missing");
        }

        Dictionary<string, string> args = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Entity"] = entity
        };
        
        if (file is { Length: > 0 })
        {
            string inbox = Path.Combine(environment.ContentRootPath, "plugins", connectorName, "Inbox");
            Directory.CreateDirectory(inbox);

            string saved = Path.Combine(inbox, $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");

            await using FileStream fileStream = File.Create(saved);
            await file.CopyToAsync(fileStream, cancellationToken);
            
            args["Path"] = saved;
        }
        else if (connectorName.Equals("CsvCollector", StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest("CsvCollector requires a file upload.");
        }
        
        queue.Enqueue(new CollectorJob(connectorName, args));

        return Results.Ok(new
        {
            Status = "Queued",
            Connector = connectorName,
            Entity = entity,
            File = args.TryGetValue("Path", out string? arg) ? Path.GetFileName(arg) : null
        });
    }
}
