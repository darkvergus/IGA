using Ingestion.Interfaces;

namespace Ingestion.Source;

public class CsvSource(string filePath, char delimiter = ',') : IDataSource
{
    public IEnumerable<IDictionary<string, string>> ReadRecords()
    {
        using StreamReader reader = new(filePath);
        string? header = reader.ReadLine();
        if (header == null)
        {
            yield break;
        }

        string[] columns = header.Split(delimiter);
        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] values = line.Split(delimiter);
            Dictionary<string, string> dict = new(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < columns.Length; i++)
            {
                dict[columns[i]] = i < values.Length ? values[i] : string.Empty;
            }

            yield return dict;
        }
    }
}