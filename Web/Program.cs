using System.Data;
using Core.Dynamic;
using Database.Context;
using Host.Core;
using Host.Services;
using Host.Workers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Web.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    string xml = Path.Combine(AppContext.BaseDirectory, "Web.xml");
    if (File.Exists(xml))
    {
        opt.IncludeXmlComments(xml);
    }
});

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<IgaDbContext>(opt => opt.UseSqlServer(connectionString));
builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(connectionString));

string pluginFolder = Path.Combine(AppContext.BaseDirectory, "plugins");

builder.Services.AddSingleton<PluginRegistry>();
builder.Services.AddSingleton<InMemJobQueue>();
builder.Services.AddScoped<JobService>();

builder.Services.AddSingleton<PluginLoader>(serviceProvider =>
{
    ILoggerFactory logFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    PluginRegistry registry = serviceProvider.GetRequiredService<PluginRegistry>();
    return new(pluginFolder, serviceProvider, logFactory, registry);
});

builder.Services.AddHostedService<PipelineWorker>();

WebApplication application = builder.Build();

using (IServiceScope scope = application.Services.CreateScope())
{
    IgaDbContext db = scope.ServiceProvider.GetRequiredService<IgaDbContext>();
    List<DynamicAttributeDefinition> attributeDefinitions = db.DynamicAttributeDefinitions.AsNoTracking().ToList();
    DynamicAttributeRegistry.WarmUp(attributeDefinitions);
}

if (application.Environment.IsDevelopment())
{
    application.UseSwagger();
    application.UseSwaggerUI(ui =>
    {
        ui.DefaultModelsExpandDepth(-1);
        ui.SwaggerEndpoint("/swagger/v1/swagger.json", "IGA v1");
    });
}
else
{
    application.UseExceptionHandler("/Home/Error");
    application.UseHsts();
}

application.UseHttpsRedirection();
application.UseStaticFiles();
application.UseRouting();
application.UseAuthentication();
application.UseAuthorization();
application.UseAntiforgery();

application.MapControllers();
application.MapControllerRoute("default", "{controller=Ingestion}/{action=Index}/{id?}");

application.MapProvisioningEndpoints();
application.MapIngestionEndpoints();
application.MapMappingsEndpoints();
application.MapStaticAssets();

application.MapGet("/dbping", async (IgaDbContext db) => await db.Database.CanConnectAsync() ? Results.Ok("DB OK") : Results.Problem("DB down"));

application.Run();