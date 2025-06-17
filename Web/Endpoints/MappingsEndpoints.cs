using Ingestion.Mapping;
using Microsoft.AspNetCore.Mvc;
using Web.Mappings;
using Web.Plugins;
using ZLinq;

namespace Web.Endpoints;

public static class MappingsEndpoints
{
    public static void MapMappingsEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/mappings").WithTags("Mappings");

        group.MapGet("/", GetMappings).WithName("GetMappings").WithSummary("List all available plugin mappings");
        group.MapGet("/{plugin}/{entity}", GetMapping).WithName("GetMapping").WithSummary("Get a specific mapping");
        group.MapPost("/{plugin}/{entity}", SaveMapping).WithName("SaveMapping").WithSummary("Save or update a plugin mapping");
    }

    private static IResult GetMappings([FromServices] PluginMappingManager manager)
    {
        IEnumerable<PluginMappingInfo> mappings = manager.ListMappings();

        return Results.Ok(mappings);
    }

    private static IResult GetMapping(string plugin, string entity, [FromServices] PluginMappingManager manager)
    {
        PluginMappingInfo? mapping = manager.ListMappings().AsValueEnumerable().FirstOrDefault(mappingInfo =>
            mappingInfo.PluginName.Equals(plugin, StringComparison.OrdinalIgnoreCase)
            && mappingInfo.EntityName.Equals(entity, StringComparison.OrdinalIgnoreCase));

        return mapping != null ? Results.Ok(mapping.MappingData) : Results.NotFound();
    }

    private static IResult SaveMapping(string plugin, string entity, [FromBody] ImportMapping model, [FromServices] PluginMappingManager manager)
    {
        manager.SaveMapping(plugin, entity, model);

        return Results.Ok();
    }
}