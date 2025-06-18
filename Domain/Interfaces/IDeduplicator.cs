namespace Domain.Interfaces;

public interface IDeduplicator<T>
{
    bool IsDuplicate(T item);
}