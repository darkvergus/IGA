using System.Text.Json;
using Domain.Jobs;
using Host.Core;
using Host.Services;
using Microsoft.AspNetCore.Mvc;
using Provisioning;
using Provisioning.Enums;
using ZLinq;

namespace Web.Controllers;

[ApiController, Route("provision")]
public class ProvisioningController(JobService jobs, PluginRegistry registry) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        List<string> list = registry.GetAllProvisioners().AsValueEnumerable().Select(provisioner => provisioner.Name).OrderBy(name => name).ToList();

        ViewBag.Provisioners = list;

        return View();
    }

    [HttpPost(""), IgnoreAntiforgeryToken]
    public async Task<IActionResult> Index([FromForm] string connectorName, [FromForm] string operation, [FromForm] string? externalId,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(operation, true, out ProvisioningOperation op))
        {
            ViewBag.Status = "Operation must be Create, Update, or Delete.";
            return await ReloadAsync();
        }

        ProvisioningCommand command = new(op, externalId ?? string.Empty, Delta: null);

        long id = await jobs.EnqueueAsync(JobType.Provisioning, connectorName, instanceId: 0, payload: JsonSerializer.Serialize(command), cancellationToken);

        ViewBag.Status = $"Provisioning job queued (Id {id}).";

        return View();
    }
    
    private async Task<IActionResult> ReloadAsync()
    {
        if (ViewBag.Provisioners == null)
        {
            ViewBag.Provisioners = registry.GetAllProvisioners().AsValueEnumerable().Select(provisioner => provisioner.Name).OrderBy(name => name).ToList();
        }
        return await Task.FromResult(View("Index"));
    }
}