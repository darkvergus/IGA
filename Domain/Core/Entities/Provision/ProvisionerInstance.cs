namespace Domain.Core.Entities.Provision;

public sealed class ProvisionerInstance
{
    public int Id { get; set; }
    public int ProvisionerId { get; set; }
    public Provisioner Provisioner { get; set; } = null!;
    public string InstanceName { get; set; } = null!;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<InstanceSetting> Settings { get; set; } = [];
}