using System.Text.Json;
using Domain.Jobs;
using Host.Services;
using Microsoft.AspNetCore.Mvc;

public static class IngestionEndpoints
{
    public static void MapIngestionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/ingestion").WithTags("Ingestion");

        group.MapPost("/{connectorName}/{entity}", Ingest).WithName("GenericIngestion").WithSummary("Upload data for any connector").DisableAntiforgery();
    }

    private static async Task<IResult> Ingest(string connectorName, string entity, IFormFile? file, [FromServices] IWebHostEnvironment env, [FromServices] JobService jobs, 
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(connectorName))
        {
            return Results.BadRequest("connectorName missing");
        }

        if (string.IsNullOrWhiteSpace(entity))
        {
            return Results.BadRequest("entity missing");
        }

        Dictionary<string, string> args = new(StringComparer.OrdinalIgnoreCase) {["Entity"] = entity};

        if (file is {Length: > 0})
        {
            string inbox = Path.Combine(env.ContentRootPath, "plugins", connectorName, "Inbox");
            Directory.CreateDirectory(inbox);
            string saved = Path.Combine(inbox, $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
            await using FileStream fs = File.Create(saved);
            await file.CopyToAsync(fs, cancellationToken);
            args["Path"] = saved;
        }
        else if (connectorName.Equals("CsvCollector", StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest("CsvCollector requires a file upload.");
        }

        long id = await jobs.EnqueueAsync(JobType.Ingestion, 0, JsonSerializer.Serialize(args), cancellationToken);

        return Results.Ok(new
        {
            Status = "Queued",
            JobId = id,
            Connector = connectorName,
            Entity = entity,
            File = args.TryGetValue("Path", out string? value) ? Path.GetFileName(value) : null
        });
    }
}