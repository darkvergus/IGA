using System.Data;
using Core.Dynamic;
using Database.Context;
using Host.Core;
using Host.Interfaces;
using Ingestion.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Provisioning.Interfaces;
using Web.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    string xml = Path.Combine(AppContext.BaseDirectory, "Web.xml");

    if (File.Exists(xml))
    {
        options.IncludeXmlComments(xml);
    }
});

string? cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<IgaDbContext>(opt => opt.UseSqlServer(cs));
builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(cs));

string pluginFolder = Path.Combine(AppContext.BaseDirectory, "plugins");
builder.Services.AddSingleton<PluginLoader>(serviceProvider =>
{
    ILoggerFactory logFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

    return new PluginLoader(pluginFolder, serviceProvider, logFactory);
});

builder.Services.AddSingleton<IConnectorQueue, InMemConnectorQueue>();

builder.Services.AddSingleton(provider =>
{
    PluginLoader loader = provider.GetRequiredService<PluginLoader>();
    Dictionary<string, ICollector> collectors = loader.LoadCollectors().ToDictionary(collector => collector.ConnectorName, StringComparer.OrdinalIgnoreCase);

    foreach (string key in collectors.Keys)
    {
        provider.GetRequiredService<ILogger<Program>>().LogInformation($"Loaded collector {key}");
    }

    return collectors;
});

builder.Services.AddSingleton(provider =>
{
    PluginLoader loader = provider.GetRequiredService<PluginLoader>();
    Dictionary<string, IProvisioner> provisioners = loader.LoadProvisioners().ToDictionary(provisioner => provisioner.ConnectorName, StringComparer.OrdinalIgnoreCase);

    return provisioners;
});

builder.Services.AddHostedService<Worker>();

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    IgaDbContext db = scope.ServiceProvider.GetRequiredService<IgaDbContext>();
    List<DynamicAttributeDefinition> attributeDefinitions = db.DynamicAttributeDefinitions.AsNoTracking().ToList();
    DynamicAttributeRegistry.WarmUp(attributeDefinitions);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(ui =>
    {
        ui.DefaultModelsExpandDepth(-1);
        ui.SwaggerEndpoint("/swagger/v1/swagger.json", "IGA Provisioning v1");
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Ingestion}/{action=Upload}/{id?}");

app.MapProvisioningEndpoints();
app.MapIngestionEndpoints();

app.MapStaticAssets();

app.MapGet("/dbping", async (IgaDbContext db) => await db.Database.CanConnectAsync() ? Results.Ok("DB OK") : Results.Problem("DB down"));

app.Run();