using System.ComponentModel.DataAnnotations;
using Core.Extensions;

namespace Core.Entities;

public partial record OrganizationUnit
{
    [MaxLength(64)]
    public string Name
    {
        get => this.GetAttr<string>("NAME");
        set => this.SetAttr("NAME", value);
    }

    public Guid Manager
    {
        get => this.GetAttr<Guid>("MANAGER");
        set => this.SetAttr("MANAGER", value);
    }
}