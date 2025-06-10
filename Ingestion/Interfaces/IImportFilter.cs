namespace Ingestion.Interfaces;

/// <summary>
/// Accept/deny a raw record during import (before materialisation).
/// </summary>
public interface IImportFilter
{
    /// <summary>
    /// Return <c>true</c> to keep the record, <c>false</c> to drop it.
    /// </summary>
    bool Accept(ReadOnlySpan<KeyValuePair<string, string>> record);
}