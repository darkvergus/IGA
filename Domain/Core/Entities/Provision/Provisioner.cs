namespace Domain.Core.Entities.Provision;

/// <summary>
/// Stores global config and identification for a provisioner plugin.
/// </summary>
public sealed class Provisioner
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public string Version { get; set; } = null!;
    public ICollection<ProvisionerInstance>? Instances { get; set; } = [];
}