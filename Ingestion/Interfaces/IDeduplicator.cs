namespace Ingestion.Interfaces;

public interface IDeduplicator<T>
{
    bool IsDuplicate(T item);
}