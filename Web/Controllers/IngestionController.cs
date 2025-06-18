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

        if (connectorName == "CsvCollector" && (file == null || file.Length == 0))
        {
            ViewBag.Status = "CSV file missing for CsvCollector.";

            return View();
        }

        Dictionary<string, string> args = new() {["Entity"] = entity};

        foreach (KeyValuePair<string, StringValues> kvp in Request.Form)
        {
            if (!args.ContainsKey(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value))
            {
                args[kvp.Key] = kvp.Value!;
            }
        }

        if (file is {Length: > 0})
        {
            string inbox = Path.Combine(env.ContentRootPath, "plugins", connectorName, "Inbox");
            Directory.CreateDirectory(inbox);
            string saved = Path.Combine(inbox, $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
            await using FileStream stream = System.IO.File.Create(saved);
            await file.CopyToAsync(stream, cancellationToken);
            args["Path"] = saved;
        }

        await jobs.EnqueueAsync(JobType.Ingestion, 0, JsonSerializer.Serialize(args), cancellationToken);
        ViewBag.Status = "Job queued.";

        return View();
    }
}