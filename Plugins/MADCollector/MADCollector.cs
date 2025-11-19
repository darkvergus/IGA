using System.DirectoryServices.Protocols;
using System.Net;
using Core.Dynamic;
using Database.Context;
using Domain.Mapping;
using Domain.Repository;
using Ingestion.Interfaces;
using Ingestion.Pipeline;
using MADCollector.Source;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MADCollector;

public sealed class MADCollector(IServiceScopeFactory scopeFactory) : ICollector
{
    public string Name => "MADCollector";
    
    private IConfiguration? configuration;
    private ILogger? logger;

    public void Initialize(IConfiguration cfg, ILogger log)
    {
        configuration = cfg;
        logger = log;
    }

    public async Task RunAsync(IReadOnlyDictionary<string, string> args, CancellationToken cancellationToken = default)
    {
        string host = args["Host"];
        int port = int.Parse(args.TryGetValue("Port", out string? p) ? p : "389");
        bool ssl = args.TryGetValue("UseSsl", out string? s) && bool.Parse(s);
        bool skip = args.TryGetValue("SkipCertCheck", out string? sc) && bool.Parse(sc);
        string bindDn = args["BindDn"];
        string bindPw = args["BindPw"];
        string baseDn = args["BaseDn"];
        string filter = args.TryGetValue("Filter", out string? f) ? f : "(objectClass=*)";
        string entity = args.TryGetValue("Entity", out string? e) ? e : "Identity";
        string authStr = args.TryGetValue("AuthType", out string? at) ? at : "Basic";
        AuthType auth = Enum.TryParse(authStr, true, out AuthType a) ? a : AuthType.Basic;
        string? domain = args.TryGetValue("Domain", out string? d) ? d : null;
        
        string pluginDir = Path.Combine(Path.GetDirectoryName(typeof(MADCollector).Assembly.Location)!, "MADCollector");

        ImportMapping? importMapping = MappingRepository.Get(Name, entity);

        if (importMapping == null)
        {
            logger?.LogError($"No mapping found for entity '{entity}'. Job aborted.");

            return;
        }

        using IServiceScope scope = scopeFactory.CreateScope();
        IgaDbContext context = scope.ServiceProvider.GetRequiredService<IgaDbContext>();
        
        IngestionPipeline pipeline = new(context);
        List<DynamicAttributeDefinition> attributeDefinitions = await context.DynamicAttributeDefinitions.ToListAsync(cancellationToken);

        logger?.LogInformation($"MADCollector connecting to {host}:{port} ssl={ssl}");

        using LdapConnection connection = new(host);
        connection.AuthType = auth;
        connection.SessionOptions.SecureSocketLayer = ssl;

        if (skip)
        {
            connection.SessionOptions.VerifyServerCertificate = static (_, _) => true;
        }

        connection.Credential = !string.IsNullOrWhiteSpace(domain) && auth == AuthType.Ntlm ? new(bindDn, bindPw, domain)
            : new NetworkCredential(bindDn, bindPw);

        connection.Bind();

        MADSource source = new(connection, baseDn, filter, pluginDir, scope.ServiceProvider.GetRequiredService<ILogger<MADSource>>());

        try
        {
            await pipeline.RunAsync(source, importMapping, attributeDefinitions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, $"MAD import failed for entity {entity}");
        }

        logger?.LogInformation($"MADCollector completed for entity {entity}");
    }
}