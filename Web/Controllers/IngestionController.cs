using System.Text.Json;
using Domain.Jobs;
using Host.Core;
using Host.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ZLinq;

namespace Web.Controllers;

public class IngestionController(JobService jobs, IWebHostEnvironment env,  PluginRegistry registry) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        List<string> list = registry.GetAllCollectors().AsValueEnumerable().Select(collector => collector.Name).OrderBy(name => name).ToList();

        ViewBag.Collectors = list;

        return View();
    }

    [HttpPost, DisableRequestSizeLimit, IgnoreAntiforgeryToken]
    public async Task<IActionResult> Index(string entity, IFormFile? file, string connectorName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entity))
        {
            ViewBag.Status = "Entity name missing.";
            return await ReloadAsync();
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

        long id = await jobs.EnqueueAsync(JobType.Ingestion, connectorName, 0, JsonSerializer.Serialize(args), cancellationToken);

        ViewBag.Status = $"Collector job queued (Id {id}).";
        
        ViewBag.Collectors = registry.GetAllCollectors().Select(collector => collector.Name).OrderBy(name => name).ToList();
        
        return View();
    }

    private async Task<IActionResult> ReloadAsync()
    {
        if (ViewBag.Collectors == null)
        {
            ViewBag.Collectors = registry.GetAllCollectors().AsValueEnumerable().Select(collector => collector.Name).OrderBy(name => name).ToList();
        }

        return await Task.FromResult(View("Index"));
    }
}
