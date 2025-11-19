using System.DirectoryServices.Protocols;
using Domain.Interfaces;
using Domain.Mapping;
using Microsoft.Extensions.Logging;
using ZLinq;

namespace MADProvisioner.Source;

public sealed class MADSource(LdapConnection connection, string baseDn, string filter, string pluginDir, ILogger<MADSource> log) : IDataSource
{
    private readonly PluginDataModel? model = XmlDataModelLoader.Load(pluginDir, "identity");

    private bool IsRowValid(IDictionary<string, string> row)
    {
        if (model == null)
        {
            return true;
        }

        foreach (DataModelField modelField in model.Attributes.AsValueEnumerable().Where(modelField => modelField.Required))
        {
            if (row.TryGetValue(modelField.Source, out string? value) && !string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            log.LogWarning($"MAD row skipped – missing '{modelField.Source}'");

            return false;
        }

        return true;
    }

    public IEnumerable<IDictionary<string, string>> ReadAsync(CancellationToken cancellationToken)
    {
        SearchRequest request = new(baseDn, filter, SearchScope.Subtree, null);
        SearchResponse? response = (SearchResponse)connection.SendRequest(request);

        foreach (SearchResultEntry entry in response.Entries)
        {
            Dictionary<string, string> row = new(StringComparer.OrdinalIgnoreCase);

            foreach (DirectoryAttribute attr in entry.Attributes.Values)
            {
                row[attr.Name] = attr[0]?.ToString() ?? string.Empty;
            }

            if (!IsRowValid(row))
            {
                continue;
            }

            yield return row;
        }
    }
}