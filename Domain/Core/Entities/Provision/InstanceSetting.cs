namespace Domain.Core.Entities.Provision;

/// <summary>
/// Generic key–value pair belonging to one ProvisionConfig.
/// </summary>
public sealed class InstanceSetting
{
    public int Id { get; set; }
    public int InstanceId { get; set; }
    public ProvisionerInstance Instance { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}