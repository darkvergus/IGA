using Database.Context;
using Ingestion.Pipeline;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;

namespace Web.Controllers;

public class IngestionController(IngestionPipeline pipeline, IgaDbContext db, IWebHostEnvironment environment) : Controller
{
    [HttpGet]
    public IActionResult Upload() => View();

    [HttpPost, DisableRequestSizeLimit, IgnoreAntiforgeryToken]
    public async Task<IActionResult> Upload(string entity, IFormFile file, CancellationToken ct)
    {
        if (file is null || string.IsNullOrWhiteSpace(entity))
        {
            ViewBag.Status = "Entity and file required.";
            return View();
        }

        string tmp = Path.Combine(environment.ContentRootPath, "uploads", $"{Guid.NewGuid():N}.csv");
        await using (FileStream fs = System.IO.File.Create(tmp))
        {
            await file.CopyToAsync(fs, ct);
        }

        await IngestionExtensions.RunCsvAsync(entity, tmp, pipeline, db, ct);
        System.IO.File.Delete(tmp);

        ViewBag.Status = "Import complete";
        return View();
    }
}