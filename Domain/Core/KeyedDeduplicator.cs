using Domain.Interfaces;

namespace Domain.Core;

public class KeyedDeduplicator<T, TKey>(Func<T, TKey> keySelector) : IDeduplicator<T>
{
    private readonly HashSet<TKey> seen = [];

    public bool IsDuplicate(T item)
    {
        TKey key = keySelector(item);
        return !seen.Add(key);
    }
}