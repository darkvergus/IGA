using Ingestion.Interfaces;
using System.DirectoryServices.Protocols;

namespace LDAPCollector.Source;

public sealed class LDAPSource(LdapConnection connection, string baseDn, string filter) : IDataSource
{
    public IEnumerable<IDictionary<string, string>> ReadRecords()
    {
        SearchRequest request = new(baseDn, filter, SearchScope.Subtree, null);
        SearchResponse response = (SearchResponse)connection.SendRequest(request);

        foreach (SearchResultEntry entry in response.Entries)
        {
            Dictionary<string, string> dict = new(StringComparer.OrdinalIgnoreCase);

            foreach (DirectoryAttribute attr in entry.Attributes.Values)
            {
                dict[attr.Name] = attr[0]?.ToString() ?? string.Empty;
            }

            yield return dict;
        }
    }
}