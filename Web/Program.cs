using Database.Context;
using Microsoft.EntityFrameworkCore;
using Provisioning.Connectors;
using Provisioning.Interfaces;
using Provisioning.Options;
using Provisioning.Services;
using Ingestion.Interfaces;
using Ingestion.Resolver;
using Ingestion.Validation;
using Web;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    string xml = Path.Combine(AppContext.BaseDirectory, "Web.xml");

    if (File.Exists(xml))
    {
        o.IncludeXmlComments(xml);
    }
});

string? cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<IgaDbContext>(dbContextOptionsBuilder => dbContextOptionsBuilder.UseSqlServer(cs));

builder.Services.AddScoped<ProvisioningService>();
builder.Services.Configure<LdapOptions>(builder.Configuration.GetSection("Ldap"));
builder.Services.AddScoped<IProvisioningConnector, LdapConnector>();

WebApplication app = builder.Build();

app.MapProvisioningEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(ui =>
    {
        ui.DefaultModelsExpandDepth(-1);
        ui.SwaggerEndpoint("/swagger/v1/swagger.json", "IGA Provisioning v1");
    });
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapGet("/dbping", async (IgaDbContext db) =>
    await db.Database.CanConnectAsync() ? Results.Ok("DB OK") : Results.Problem("DB down"));

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();