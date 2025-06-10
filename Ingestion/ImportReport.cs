namespace Ingestion;

public sealed record ImportReport(int RecordsIn, int RecordsOut, int Inserted, int Updated, int Skipped, int Errors, TimeSpan Duration);