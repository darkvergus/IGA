﻿using System.Data;
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

string cs = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<IgaDbContext>(opt => opt.UseSqlServer(cs));
builder.Services.AddScoped<IDbConnection>(_ => new SqlConnection(cs));

string pluginFolder = Path.Combine(AppContext.BaseDirectory, "plugins");

builder.Services.AddSingleton<PluginRegistry>();
builder.Services.AddSingleton<InMemJobQueue>();
builder.Services.AddScoped<JobService>();

builder.Services.AddSingleton<PluginLoader>(sp =>
{
    ILoggerFactory log = sp.GetRequiredService<ILoggerFactory>();
    PluginRegistry reg = sp.GetRequiredService<PluginRegistry>();
    return new(pluginFolder, sp, log, reg);
});

builder.Services.AddHostedService<PipelineWorker>();

WebApplication application = builder.Build();

using (IServiceScope scope = application.Services.CreateScope())
{
    IServiceProvider sp = scope.ServiceProvider;

    IgaDbContext db = sp.GetRequiredService<IgaDbContext>();
    List<DynamicAttributeDefinition> defs = db.DynamicAttributeDefinitions.AsNoTracking().ToList();
    DynamicAttributeRegistry.WarmUp(defs);

    PluginLoader loader = sp.GetRequiredService<PluginLoader>();
    _ = loader.LoadCollectors().ToList();
    _ = loader.LoadProvisioners().ToList();
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

application.MapGet("/dbping", async (IgaDbContext db) => await db.Database.CanConnectAsync()
    ? Results.Ok("DB OK")
    : Results.Problem("DB down"));

application.Run();