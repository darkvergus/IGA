using System.ComponentModel.DataAnnotations;
using Core.Extensions;

namespace Core.Entities;

public sealed partial record SystemConfiguration
{
    [MaxLength(64)]
    public string Name
    {
        get => this.GetAttr<string>("NAME");
        set => this.SetAttr("NAME", value);
    }
}