using System.Text.Json;
using Domain.Jobs;
using Host.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Web.Endpoints;

public static class IngestionEndpoints
{
    public static void MapIngestionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/ingestion").WithTags("Ingestion");

        group.MapPost("/{connectorName}/{entity}", Ingest).WithName("GenericIngestion").WithSummary("Upload data for any connector").DisableAntiforgery();
    }

    private static async Task<IResult> Ingest(string connectorName, string entity, IFormFile? file, HttpRequest request, [FromServices] IWebHostEnvironment environment,
        [FromServices] JobService jobs, CancellationToken cancellationToken)
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
            ["Entity"] = entity,
            ["connectorName"] = connectorName
        };

        foreach ((string key, StringValues val) in request.Form)
        {
            if (key.StartsWith("__", StringComparison.Ordinal))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(val))
            {
                continue;
            }

            if (connectorName.Equals("CsvCollector", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(key, "Delimiter", StringComparison.OrdinalIgnoreCase))
                {
                    args["Delimiter"] = val!;
                }
                else if (string.Equals(key, "Path", StringComparison.OrdinalIgnoreCase))
                {
                    args["Path"] = val!;
                }

                continue;
            }

            if (!args.ContainsKey(key))
            {
                args[key] = val!;
            }
        }

        if (connectorName.Equals("CsvCollector", StringComparison.OrdinalIgnoreCase))
        {
            if (file is { Length: > 0 })
            {
                string inbox = Path.Combine(environment.ContentRootPath, "plugins", connectorName, "Inbox");
                Directory.CreateDirectory(inbox);
                string saved = Path.Combine(inbox, $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");

                await using FileStream stream = File.Create(saved);
                await file.CopyToAsync(stream, cancellationToken);

                args["Path"] = saved;
            }

            if (!args.ContainsKey("Path"))
            {
                return Results.BadRequest("CsvCollector requires a file upload or Path field.");
            }
        }

        long id = await jobs.EnqueueAsync(JobType.Ingestion, connectorName, 0, JsonSerializer.Serialize(args), cancellationToken);

        return Results.Ok(new
        {
            Status = "Queued",
            JobId = id,
            Connector = connectorName,
            Entity = entity,
            File = args.TryGetValue("Path", out string? path) ? Path.GetFileName(path) : null
        });
    }
}