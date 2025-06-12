using System.ComponentModel.DataAnnotations;
using Core.Domain.Extensions;

namespace Core.Domain.Entities;

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

    public Guid Identity
    {
        get => this.GetAttr<Guid>("IDENTITYREF");
        set => this.SetAttr("IDENTITYREF", value);
    }
}