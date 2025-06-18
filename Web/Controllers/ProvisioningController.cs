using System.Text.Json;
using Domain.Jobs;
using Host.Services;
using Microsoft.AspNetCore.Mvc;
using Provisioning;
using Provisioning.Enums;

namespace Web.Controllers;

[ApiController, Route("provision")]
public class ProvisioningController(JobService jobs) : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost(""), DisableRequestSizeLimit, IgnoreAntiforgeryToken]
    public async Task<IActionResult> Index(string connectorName, int instanceId, string operation, string dn, string? deltaJson,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(connectorName) || string.IsNullOrWhiteSpace(dn) || !Enum.TryParse(operation, true, out ProvisioningOperation op))
        {
            ViewBag.Status = "Invalid form values.";

            return Index();
        }

        Dictionary<string, string>? delta = null;

        if (!string.IsNullOrWhiteSpace(deltaJson))
        {
            try
            {
                delta = JsonSerializer.Deserialize<Dictionary<string, string>>(deltaJson);
            }
            catch
            {
                ViewBag.Status = "Delta JSON malformed.";

                return Index();
            }
        }

        ProvisioningCommand cmd = new(op, dn, delta);
        long id = await jobs.EnqueueAsync(JobType.Provisioning, connectorName, instanceId, JsonSerializer.Serialize(cmd), cancellationToken);

        ViewBag.Status = $"Provisioning job queued (Id {id}).";

        return Index();
    }

    [HttpPost("{connectorName}/{instanceId:int}/{operation}"), IgnoreAntiforgeryToken]
    public async Task<IActionResult> EnqueueApi(string connectorName, int instanceId, string operation, [FromBody] JsonElement body,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(operation, true, out ProvisioningOperation op))
        {
            return BadRequest("operation must be Create, Update, or Delete");
        }

        if (!body.TryGetProperty("externalId", out JsonElement ext) || string.IsNullOrWhiteSpace(ext.GetString()))
        {
            return BadRequest("externalId missing");
        }

        Dictionary<string, string>? delta = null;

        if (body.TryGetProperty("delta", out JsonElement element) && element.ValueKind == JsonValueKind.Object)
        {
            delta = JsonSerializer.Deserialize<Dictionary<string, string>>(element.GetRawText());
        }

        ProvisioningCommand cmd = new(op, ext.GetString()!, delta);
        long id = await jobs.EnqueueAsync(JobType.Provisioning, connectorName, instanceId, JsonSerializer.Serialize(cmd), cancellationToken);

        return Ok(new { Status = "Queued", JobId = id });
    }
}