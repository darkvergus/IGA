namespace Core.Dynamic;

public static class DynamicAttributeRegistry
{
    private static readonly Dictionary<string, DynamicAttributeDefinition> ByName = new(StringComparer.OrdinalIgnoreCase);

    private static readonly ReaderWriterLockSlim Lock = new();

    /// <summary>Bulk-load all definitions once at app start.</summary>
    public static void WarmUp(IEnumerable<DynamicAttributeDefinition> definitions)
    {
        Lock.EnterWriteLock();

        try
        {
            ByName.Clear();

            foreach (DynamicAttributeDefinition attributeDefinition in definitions)
            {
                ByName[attributeDefinition.SystemName] = attributeDefinition;
            }
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    /// <summary>Get definition by SystemName (throws if unknown).</summary>
    public static DynamicAttributeDefinition Get(string sysName)
    {
        Lock.EnterReadLock();

        try
        {
            return ByName.TryGetValue(sysName, out DynamicAttributeDefinition? def) ? def : throw new InvalidOperationException($"Dynamic attribute “{sysName}” not registered.");
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }
}