using Host.Interfaces;
using Host.Job;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Web.Controllers;

public class IngestionController(IConnectorQueue queue, IWebHostEnvironment environment) : Controller
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

        if (connectorName == "CsvCollector" && (file == null || file.Length == 0))
        {
            ViewBag.Status = "CSV file missing for CsvCollector.";

            return View();
        }

        Dictionary<string, string> args = new()
        {
            ["Entity"] = entity
        };

        foreach (KeyValuePair<string, StringValues> kvp in Request.Form)
        {
            if (!args.ContainsKey(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value))
            {
                args[kvp.Key] = kvp.Value!;
            }
        }

        if (file is { Length: > 0 })
        {
            string inbox = Path.Combine(environment.ContentRootPath, "plugins", connectorName, "Inbox");
            Directory.CreateDirectory(inbox);
            string saved = Path.Combine(inbox, $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
            await using FileStream fileStream = System.IO.File.Create(saved);
            await file.CopyToAsync(fileStream, cancellationToken);

            args["Path"] = saved;
        }

        queue.Enqueue(new CollectorJob(connectorName, args));

        ViewBag.Status = "Job queued.";

        return View();
    }
}