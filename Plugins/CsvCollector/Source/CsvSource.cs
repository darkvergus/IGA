using System.Text;
using Ingestion.Interfaces;

namespace CsvCollector.Source;

public class CsvSource(string filePath, char delimiter = ',') : IDataSource
{
   public IEnumerable<IDictionary<string, string>> ReadRecords()
    {
        using StreamReader sr = new(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 16 * 1024);
        
        string? headerLine = sr.ReadLine();
        if (headerLine is null)
        {
            yield break;
        }

        string[] columns = SplitLineToStrings(headerLine.AsSpan(), delimiter);

        while (sr.ReadLine() is { } line)
        {
            if (line.Length == 0)
            {
                continue;
            }

            Dictionary<string, string> dict = new(columns.Length, StringComparer.OrdinalIgnoreCase);
            PopulateDictionary(line.AsSpan(), delimiter, columns, dict);
            yield return dict; 
        }
    }
   
    private static string[] SplitLineToStrings(ReadOnlySpan<char> line, char delim)
    {
        List<string> parts = new(16);
        int start = 0;
        for (int i = 0; i <= line.Length; i++)
        {
            if (i == line.Length || line[i] == delim)
            {
                parts.Add(line.Slice(start, i - start).ToString());
                start = i + 1;
            }
        }
        return parts.ToArray();
    }
    
    private static void PopulateDictionary(ReadOnlySpan<char> line, char delim, string[] columns, Dictionary<string, string> dict)
    {
        int colIdx = 0;
        int start = 0;
        for (int i = 0; i <= line.Length; i++)
        {
            if (i == line.Length || line[i] == delim)
            {
                if (colIdx < columns.Length)
                {
                    dict[columns[colIdx]] = line.Slice(start, i - start).ToString();
                }
                colIdx++;
                start = i + 1;
            }
        }
        
        for (int i = colIdx; i < columns.Length; i++)
        {
            dict[columns[i]] = string.Empty;
        }
    }
}