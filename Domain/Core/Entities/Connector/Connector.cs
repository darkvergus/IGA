namespace Domain.Core.Entities.Connector;

/// <summary>
/// Stores JSON-based settings for each collector or provisioner plugin.
/// </summary>
public sealed class Connector
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public string ConfigData { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = null!;
    public DateTime? ModifiedAt { get; set; }
}