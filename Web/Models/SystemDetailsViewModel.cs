using Core.Entities;
using Domain.Mapping;

namespace Web.Models;

public sealed class SystemDetailsViewModel
{
    public SystemConfiguration SystemConfiguration { get; set; } = null!;
    public PluginDataModel? IdentityDataModel { get; set; }
    public List<ConnectionFieldViewModel> CollectorConnectionFields { get; set; } = [];
    public List<ConnectionFieldViewModel> ProvisionerConnectionFields { get; set; } = [];
}