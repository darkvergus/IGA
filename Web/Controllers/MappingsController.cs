using System.Xml.Serialization;
using Domain.Mapping;
using Microsoft.AspNetCore.Mvc;
using Web.Mappings;

namespace Web.Controllers;

public class MappingsController : Controller
{
    private readonly string pluginsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");

    [HttpGet]
    public IActionResult Index()
    {
        List<MappingOverview> mappings = [];

        foreach (string pluginDir in Directory.GetDirectories(pluginsBasePath))
        {
            string context = Path.GetFileName(pluginDir);
            string mappingPath = Path.Combine(pluginDir, "Mappings");

            if (!Directory.Exists(mappingPath))
            {
                continue;
            }

            foreach (string file in Directory.GetFiles(mappingPath, "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    using FileStream stream = System.IO.File.OpenRead(file);
                    XmlSerializer serializer = new(typeof(ImportMapping));
                    ImportMapping mapping = (ImportMapping)serializer.Deserialize(stream)!;
                    
                    mappings.Add(new()
                    {
                        Context = context,
                        Entity = Path.GetFileNameWithoutExtension(file).Replace(".mapping", "", StringComparison.OrdinalIgnoreCase),
                        FilePath = file,
                        PrimaryKey = mapping.PrimaryKeyProperty,
                        FieldCount = mapping.FieldMappings.Count
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading mapping {file}: {ex.Message}");
                }
            }
        }

        return View(mappings);
    }

    [HttpGet]
    public IActionResult Edit(string context, string entity)
    {
        string filePath = Path.Combine(pluginsBasePath, context, "Mappings", $"{entity}.mapping.xml");

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        using FileStream stream = System.IO.File.OpenRead(filePath);
        XmlSerializer serializer = new(typeof(ImportMapping));
        ImportMapping mapping = (ImportMapping)serializer.Deserialize(stream)!;

        ViewBag.Context = context;
        ViewBag.Entity = entity;

        return View(mapping);
    }

    [HttpPost]
    public IActionResult Edit(string context, string entity, ImportMapping model)
    {
        string filePath = Path.Combine(pluginsBasePath, context, "Mappings", $"{entity}.mapping.xml");

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        using FileStream stream = System.IO.File.Create(filePath);
        XmlSerializer serializer = new(typeof(ImportMapping));
        serializer.Serialize(stream, model);

        return RedirectToAction("Index");
    }
}