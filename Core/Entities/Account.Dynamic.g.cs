using System.ComponentModel.DataAnnotations;
using Core.Attributes;
using Core.Extensions;

namespace Core.Entities;

public partial record Account
{
    [MaxLength(64)]
    public string FirstName
    {
        get => this.GetAttr<string>("FIRSTNAME");
        set => this.SetAttr("FIRSTNAME", value);
    }

    [MaxLength(64)]
    public string LastName
    {
        get => this.GetAttr<string>("LASTNAME");
        set => this.SetAttr("LASTNAME", value);
    }

    [Unique]
    public Guid Identity
    {
        get => this.GetAttr<Guid>("IDENTITYREF");
        set => this.SetAttr("IDENTITYREF", value);
    }
}