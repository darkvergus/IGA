using System.Text.Json;
using Domain.Jobs;
using Host.Services;
using Microsoft.AspNetCore.Mvc;
using Provisioning;
using Provisioning.Enums;

namespace Web.Endpoints;

public static class ProvisioningEndpoints
{
    public static void MapProvisioningEndpoints(this IEndpointRouteBuilder app)
    {
        /* POST /api/provisioning/{connector}/{instanceId}/{op} */
        app.MapPost("/api/provisioning/{connectorName}/{instanceId:int}/{operation}", Enqueue)
            .WithName("ProvisioningJob")
            .WithSummary("Queue provisioning command for any connector instance")
            .DisableAntiforgery();
    }

    private static async Task<IResult> Enqueue(string connectorName, int instanceId, string operation, [FromBody] JsonElement body, [FromServices] JobService jobs,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(operation, true, out ProvisioningOperation op))
        {
            return Results.BadRequest("Operation must be Create, Update, or Delete");
        }

        if (!body.TryGetProperty("externalId", out JsonElement ext) ||
            string.IsNullOrWhiteSpace(ext.GetString()))
        {
            return Results.BadRequest("externalId missing");
        }

        Dictionary<string, string>? delta = body.TryGetProperty("delta", out JsonElement element) && element.ValueKind == JsonValueKind.Object
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(element.GetRawText())
            : null;

        ProvisioningCommand command = new(op, ext.GetString()!, delta);

        long id = await jobs.EnqueueAsync(JobType.Provisioning, connectorName, instanceId, JsonSerializer.Serialize(command), cancellationToken);

        return Results.Ok(new { Status = "Queued", JobId = id });
    }
}