using Core.Entities;
using Domain.Mapping;

namespace Web.Models;

public sealed class SystemDetailsViewModel
{
    public SystemConfiguration SystemConfiguration { get; set; } = null!;
    public PluginDataModel? IdentityDataModel { get; set; }
}