using System.ComponentModel.DataAnnotations;
using Core.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Models;

public sealed class SystemEditViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = null!;

    [Required]
    public string CollectorName { get; set; } = null!;

    public string? ProvisionerName { get; set; }

    [Required] 
    public SystemDataType DataSelection { get; set; }

    public string? CollectorConnectionConfigurationJson { get; set; }

    public string? ProvisionerConnectionConfigurationJson { get; set; }

    public List<SelectListItem> AvailableCollectors { get; set; } = [];

    public List<SelectListItem> AvailableProvisioners { get; set; } = [];
}