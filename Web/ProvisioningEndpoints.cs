using Core.Domain.Dynamic;
using Core.Domain.Entities;
using Database.Context;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Provisioning;
using Provisioning.Enums;
using Provisioning.Services;
using Web.DTO;

namespace Web;

public static class ProvisioningEndpoints
{
    public static void MapProvisioningEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/provisioning")
                                     .WithTags("Provisioning");

        // POST /api/provisioning/{connector}/create
        group.MapPost("{connector}/create", RunCreate)
             .WithName("Provisioning-Create")
             .WithSummary("Create an object through a provisioning connector");

        // POST /api/provisioning/{connector}/update
        group.MapPost("{connector}/update", RunUpdate)
             .WithName("Provisioning-Update")
             .WithSummary("Update an object through a provisioning connector");

        // POST /api/provisioning/{connector}/delete
        group.MapPost("{connector}/delete", RunDelete)
             .WithName("Provisioning-Delete")
             .WithSummary("Delete an object through a provisioning connector");
    }

    private static async Task<Results<Ok<ProvisioningResult>, NotFound<string>>>
    ExecuteAsync(ProvisioningOperation op,
                 string connector,
                 ProvisioningRequestDto req,
                 IgaDbContext db,
                 ProvisioningService svc,
                 CancellationToken ct)
    {
        Account? account = await db.Accounts.FindAsync([req.AccountId], ct);
        Resource? resource = await db.Resources.FindAsync([req.GroupId], ct);

        if (account is null)
        {
            return TypedResults.NotFound("Account not found.");
        }

        if (resource is null)
        {
            return TypedResults.NotFound("Resource not found.");
        }

        ProvisioningCommand cmd = new(op, req.ExternalId, account, resource, req.Delta);
        ProvisioningResult result = await svc.ProvisionAsync(connector, cmd, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ProvisioningResult>, NotFound<string>>> RunCreate(
         string connector,
         ProvisioningRequestDto req,
         [FromServices] IgaDbContext db,
         [FromServices] ProvisioningService svc,
         CancellationToken ct)
    {
        Account? account = await db.Accounts.FindAsync(req.AccountId, ct);
        if (account is null)
        {
            account = new Account
            {
                Attributes = new List<DynamicAttributeValue>()
            };
            db.Accounts.Add(account);
            await db.SaveChangesAsync(ct);
        }

        ProvisioningCommand cmd = new(ProvisioningOperation.Create, req.ExternalId, account, null ,req.Delta);
        ProvisioningResult result = await svc.ProvisionAsync(connector, cmd, ct);

        if (result.ExternalSid is not null)
        {
            //account.Attributes["sid"] = DynamicAttributeValue.From(result.ExternalSid);
        }

        await db.SaveChangesAsync(ct);

        return TypedResults.Ok(result);
    }


    private static Task<Results<Ok<ProvisioningResult>, NotFound<string>>> RunUpdate(
        string connector,
        ProvisioningRequestDto req,
        [FromServices] IgaDbContext db,
        [FromServices] ProvisioningService svc,
        CancellationToken ct)
        => ExecuteAsync(ProvisioningOperation.Update, connector, req, db, svc, ct);

    private static Task<Results<Ok<ProvisioningResult>, NotFound<string>>> RunDelete(
        string connector,
        ProvisioningRequestDto req,
        [FromServices] IgaDbContext db,
        [FromServices] ProvisioningService svc,
        CancellationToken ct)
        => ExecuteAsync(ProvisioningOperation.Delete, connector, req, db, svc, ct);
}
