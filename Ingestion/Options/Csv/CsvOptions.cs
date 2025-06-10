namespace Ingestion.Options.Csv;

public sealed class CsvOptions
{
    /// <summary>Key = entity CLR type name; Value = absolute or relative CSV file path.</summary>
    public Dictionary<string, string> Paths { get; init; } = new();
    public Guid MappingId { get; init; }
}