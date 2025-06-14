namespace Core.Entities;

/// <summary>
/// Stores JSON-based settings for each collector or provisioner plugin.
/// </summary>
public sealed class ConnectorConfig
{
    public int Id { get; set; }
    public string ConnectorName { get; set; } = null!;
    public string ConnectorType { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public string ConfigData { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public string Version { get; set; } = null!;
}