using Core.Domain.Records;
using Database.Context;
using Provisioning;
using Provisioning.Enums;
using Provisioning.Services;
using Web.DTO;

namespace Web;

public static class ProvisioningEndpoints
{
    public static void MapProvisioningEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/provisioning");

        group.MapPost("{connector}/{operation}", async (
            string connector,
            string operation,
            ProvisioningRequestDto req,
            IgaDbContext db,
            ProvisioningService svc,
            CancellationToken ct) =>
        {
            if (!Enum.TryParse(operation, true, out ProvisioningOperation op))
            {
                return Results.BadRequest("Unknown provisioning operation.");
            }
            
            Account? account  = await db.Accounts.FindAsync([req.AccountId], ct);
            Resource? resource = await db.Resources.FindAsync([req.ResourceId], ct);
            if (account  is null || resource is null)
            {
                return Results.NotFound("Account or Resource not found.");
            }

            ProvisioningCommand cmd = new(op, req.ExternalId, account, resource, req.Delta);

            ProvisioningResult result = await svc.ProvisionAsync(connector, cmd, ct);
            return Results.Ok(result);
        });
    }
}
