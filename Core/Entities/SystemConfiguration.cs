using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace Core.Entities;

public sealed partial record SystemConfiguration : GuidEntity
{
    [Required]
    public string CollectorName { get; set; } = null!;

    public string? ProvisionerName { get; set; }

    [Required]
    public SystemDataType DataSelection { get; set; }

    public string? IdentityDataModelXml { get; set; }

    public string? PermissionDataModelXml { get; set; }

    public string? CollectorConnectionConfigurationJson { get; set; }

    public string? ProvisionerConnectionConfigurationJson { get; set; }

    public SystemConfiguration() { }

    public SystemConfiguration(Guid id) : base(id) { }
}