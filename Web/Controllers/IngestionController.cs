using Host.Interfaces;
using Host.Job;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class IngestionController(IConnectorQueue queue) : Controller
{
    [HttpGet]
    public IActionResult Upload() => View();

    [HttpPost, DisableRequestSizeLimit, IgnoreAntiforgeryToken]
    public async Task<IActionResult> Upload(string entity, IFormFile file, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entity) || file is null || file.Length == 0)
        {
            ViewBag.Status = "Entity and file required.";

            return View();
        }
        
        string guid = Guid.NewGuid().ToString("N");
        string relPath = Path.Combine("CsvCollector", "Inbox", $"{guid}.csv").Replace(Path.DirectorySeparatorChar, '/');
        string absPath = Path.Combine(AppContext.BaseDirectory, "plugins", relPath);

        Directory.CreateDirectory(Path.GetDirectoryName(absPath)!);

        await using (FileStream fs = System.IO.File.Create(absPath))
        {
            await file.CopyToAsync(fs, cancellationToken);
        }

        Dictionary<string, string> parameters = new()
        {
            ["Path"] = relPath,
            ["Entity"] = entity,
            ["Delimiter"] = ","
        };

        queue.Enqueue(new CollectorJob("CsvCollector", parameters));

        ViewBag.Status = "Upload queued; background worker will import shortly.";

        return View();
    }
}