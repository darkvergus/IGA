using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Core.Dynamic;
using Database.Context;
using Ingestion.Interfaces;
using Ingestion.Mapping;
using Ingestion.Pipeline;
using LDAPCollector.Repository;
using LDAPCollector.Source;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LDAPCollector;

public sealed class LDAPCollector(IServiceProvider root) : ICollector
{
    public string ConnectorName => "LDAPCollector";

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
        
        using IServiceScope scope = root.CreateScope();
        IgaDbContext context = scope.ServiceProvider.GetRequiredService<IgaDbContext>();
        IngestionPipeline pipeline = new(context);
        List<DynamicAttributeDefinition> attributeDefinitions = await context.DynamicAttributeDefinitions.ToListAsync(cancellationToken);
        
        ImportMapping? mapping = LDAPMappingRepository.Get(entity);

        if (mapping == null)
        {
            logger?.LogError($"No mapping found for entity '{entity}'. Job aborted.");

            return;
        }
        
        logger?.LogInformation($"LDAPCollector connecting to {host}:{port} ssl={ssl}");

        using LdapConnection connection = new(host);
        connection.AuthType = auth;
        connection.SessionOptions.SecureSocketLayer = ssl;

        if (skip)
        {
            connection.SessionOptions.VerifyServerCertificate = IgnoreCert;
        }

        if (!string.IsNullOrWhiteSpace(domain) && auth == AuthType.Ntlm)
        {
            connection.Credential = new NetworkCredential(bindDn, bindPw, domain);
        }
        else
        {
            connection.Credential = new NetworkCredential(bindDn, bindPw);
        }

        connection.Bind();

        LDAPSource source = new(connection, baseDn, filter);

        try
        {
            await pipeline.RunAsync(source, mapping, attributeDefinitions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, $"LDAP import failed for entity {entity}");
            return;
        }

        logger?.LogInformation($"LDAPCollector completed for entity {entity}");
    }

    private static bool IgnoreCert(LdapConnection _, X509Certificate __) => true;
}