namespace Ingestion.Interfaces;

/// <summary>
/// Convert a raw data record (dynamic / expando from CsvHelper, dictionary from
/// REST, etc.) into a fully initialised domain entity.
/// </summary>
public interface IRowMapper
{
    object Convert(object rawRecord);
}