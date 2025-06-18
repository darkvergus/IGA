using System.Text.Json;
using Domain.Jobs;
using Host.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Web.Controllers;

public class IngestionController(JobService jobs, IWebHostEnvironment env) : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost, DisableRequestSizeLimit, IgnoreAntiforgeryToken]
    public async Task<IActionResult> Index(string entity, IFormFile? file, string connectorName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entity))
        {
            ViewBag.Status = "Entity name missing.";
            return View();
        }

        if (string.IsNullOrWhiteSpace(connectorName))
        {
            ViewBag.Status = "Connector name missing.";
            return View();
        }

        Dictionary<string, string> args = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Entity"] = entity,
            ["ConnectorName"] = connectorName
        };
        
        foreach (KeyValuePair<string, StringValues> kvp in Request.Form)
        {
            if (kvp.Key.StartsWith("__", StringComparison.Ordinal))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(kvp.Value))
            {
                continue;
            }

            if (connectorName.Equals("CsvCollector", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(kvp.Key, "Delimiter", StringComparison.OrdinalIgnoreCase))
                {
                    args["Delimiter"] = kvp.Value!;
                }
                else if (string.Equals(kvp.Key, "Path", StringComparison.OrdinalIgnoreCase))
                {
                    args["Path"] = kvp.Value!;
                }

                continue;
            }

            if (!args.ContainsKey(kvp.Key))
            {
                args[kvp.Key] = kvp.Value!;
            }
        }
        
        if (connectorName.Equals("CsvCollector", StringComparison.OrdinalIgnoreCase))
        {
            if (file is { Length: > 0 })
            {
                string inbox = Path.Combine(env.ContentRootPath, "plugins", connectorName, "Inbox");
                Directory.CreateDirectory(inbox);
                string saved = Path.Combine(inbox, $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");

                await using FileStream stream = System.IO.File.Create(saved);
                await file.CopyToAsync(stream, cancellationToken);

                args["Path"] = saved;
            }

            if (!args.ContainsKey("Path"))
            {
                ViewBag.Status = "CSV file missing or Path not provided.";
                return View();
            }
        }

        await jobs.EnqueueAsync(JobType.Ingestion, connectorName, 0, JsonSerializer.Serialize(args), cancellationToken);

        ViewBag.Status = "Job queued.";
        return View();
    }
}
