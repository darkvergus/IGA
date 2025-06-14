using Core.Dynamic;
using Core.Entities;
using Database.Context;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Provisioning;
using Provisioning.Enums;
using Provisioning.Services;
using Web.DTO;
using Account = Core.Entities.Account;

namespace Web.Endpoints;

public static class ProvisioningEndpoints
{
    public static void MapProvisioningEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/provisioning").WithTags("Provisioning");

        // POST /api/provisioning/{connector}/create
        group.MapPost("{connector}/create", RunCreate).WithName("Provisioning-Create").WithSummary("Create an object through a provisioning connector");

        // POST /api/provisioning/{connector}/update
        group.MapPost("{connector}/update", RunUpdate).WithName("Provisioning-Update").WithSummary("Update an object through a provisioning connector");

        // POST /api/provisioning/{connector}/delete
        group.MapPost("{connector}/delete", RunDelete).WithName("Provisioning-Delete").WithSummary("Delete an object through a provisioning connector");
    }

    private static async Task<Results<Ok<ProvisioningResult>, NotFound<string>>>
        ExecuteAsync(ProvisioningOperation provisioningOperation, string connector, ProvisioningRequestDto requestDto, IgaDbContext db,
            ProvisioningService service, CancellationToken cancellationToken)
    {
        Account? account = await db.Accounts.FindAsync([requestDto.AccountId], cancellationToken);
        Resource? resource = await db.Resources.FindAsync([requestDto.ResourceId], cancellationToken);

        if (account is null)
        {
            return TypedResults.NotFound("Account not found.");
        }

        if (resource is null)
        {
            return TypedResults.NotFound("Resource not found.");
        }

        ProvisioningCommand cmd = new(provisioningOperation, requestDto.ExternalId, account, resource, requestDto.Delta);
        ProvisioningResult result = await service.ProvisionAsync(connector, cmd, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ProvisioningResult>, NotFound<string>>> RunCreate(string connector, ProvisioningRequestDto requestDto,
        [FromServices] IgaDbContext db, [FromServices] ProvisioningService service, CancellationToken cancellationToken)
    {
        Account? account = await db.Accounts.FindAsync([requestDto.AccountId, cancellationToken], cancellationToken: cancellationToken);

        if (account is null)
        {
            account = new Account
            {
                Attributes = new List<DynamicAttributeValue>()
            };
            db.Accounts.Add(account);
            await db.SaveChangesAsync(cancellationToken);
        }

        ProvisioningCommand cmd = new(ProvisioningOperation.Create, requestDto.ExternalId, account, null, requestDto.Delta);
        ProvisioningResult result = await service.ProvisionAsync(connector, cmd, cancellationToken);

        if (result.ExternalSid is not null)
        {
            //account.Attributes["sid"] = DynamicAttributeValue.From(result.ExternalSid);
        }

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(result);
    }

    private static Task<Results<Ok<ProvisioningResult>, NotFound<string>>> RunUpdate(string connector, ProvisioningRequestDto requestDto,
        [FromServices] IgaDbContext db, [FromServices] ProvisioningService service, CancellationToken cancellationToken)
        => ExecuteAsync(ProvisioningOperation.Update, connector, requestDto, db, service, cancellationToken);

    private static Task<Results<Ok<ProvisioningResult>, NotFound<string>>> RunDelete(string connector, ProvisioningRequestDto requestDto,
        [FromServices] IgaDbContext db, [FromServices] ProvisioningService service, CancellationToken cancellationToken)
        => ExecuteAsync(ProvisioningOperation.Delete, connector, requestDto, db, service, cancellationToken);
}