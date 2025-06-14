using System.Collections.Concurrent;
using ZLinq;

namespace Core.Extensions;

public static class ExtensionMethods
{
    public static object SyncRoot = new();
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="itemsList"></param>
    /// <param name="maxDegreeOfParallelism"></param>
    /// <param name="action"></param>
    /// <param name="thrownOnFirstError"></param>
    public static void AsParallel<T>(this IEnumerable<T> itemsList, int maxDegreeOfParallelism, Action<T> action, bool thrownOnFirstError = false)
    {
        T[] enumerable = itemsList as T[] ?? itemsList.AsValueEnumerable().ToArray();
        if (enumerable.Length == 0)
        {
            return;
        }

        ConcurrentQueue<Exception> exceptions = new();
        if (maxDegreeOfParallelism > enumerable.Length)
        {
            maxDegreeOfParallelism = enumerable.Length;
        }

        OrderablePartitioner<Tuple<int, int>> sampledDataPartitioned = Partitioner.Create(0, enumerable.Length);
        Parallel.ForEach(sampledDataPartitioned, new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            },
            (range, loopState) =>
            {
                (int item1, int item2) = range;
                for (int i = item1; i < item2; i++)
                {
                    try
                    {
                        T item = enumerable[i];
                        action(item);
                    }
                    catch (Exception e)
                    {
                        exceptions.Enqueue(e.GetBaseException());
                        if (thrownOnFirstError)
                        {
                            loopState.Break();
                        }
                    }
                }
            });
        if (!exceptions.IsEmpty)
        {
            throw new AggregateException(exceptions);
        }
    }
    
    /// <summary>
    /// Safes the add.
    /// </summary>
    /// <param name="set">The set.</param>
    /// <param name="item">The item.</param>
    public static bool SafeAdd(this HashSet<byte[]>? set, byte[] item)
    {
        lock (SyncRoot)
        {
            return set != null && !set.Contains(item) && set.Add(item);
        }
    }
    
    /// <summary>
    /// Safes the contains.
    /// </summary>
    /// <param name="set">The set.</param>
    /// <param name="item">The item.</param>
    /// <returns></returns>
    public static bool SafeContains(this HashSet<byte[]> set, byte[] item)
    {
        lock (SyncRoot)
        {
            return set.Contains(item);
        }
    }
    
    /// <summary>
    /// Safes the remove.
    /// </summary>
    /// <param name="set">The set.</param>
    /// <param name="item">The item.</param>
    public static bool SafeRemove(this HashSet<byte[]> set, byte[] item)
    {
        if (!set.SafeContains(item))
        {
            return true;
        }

        lock (SyncRoot)
        {
            bool result = set.Remove(item);
            return result;
        }

    }

    /// <summary>
    /// Gets the original exception.
    /// </summary>
    /// <param name="ex">The ex.</param>
    /// <returns></returns>
    public static Exception GetOriginalException(this Exception? ex) => (ex?.InnerException == null ? ex : ex.InnerException.GetOriginalException()) ?? throw new InvalidOperationException();
}