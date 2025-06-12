using Core.Domain.Dynamic;
using Database.Context;
using Ingestion.Pipeline;
using Microsoft.EntityFrameworkCore;
using Provisioning.Connectors;
using Provisioning.Interfaces;
using Provisioning.Options;
using Provisioning.Services;
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

builder.Services.Configure<LdapOptions>(builder.Configuration.GetSection("Ldap"));
builder.Services.AddScoped<IngestionPipeline>();
builder.Services.AddScoped<ProvisioningService>();
builder.Services.AddScoped<IProvisioningConnector, LdapConnector>();

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