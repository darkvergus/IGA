using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public sealed partial record SystemConfiguration : GuidEntity
{
    [Required]
    public string Type { get; set; } = null!;
    
    [Required]
    public string CollectorName { get; set; } = null!;

    public string? ProvisionerName { get; set; }
    
    public SystemConfiguration() { }
    
    public SystemConfiguration(Guid id) : base(id) { }
}