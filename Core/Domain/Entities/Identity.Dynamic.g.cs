using System.ComponentModel.DataAnnotations;
using Core.Domain.Extensions;

namespace Core.Domain.Entities;

public partial record Identity
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
    
    [MaxLength(256)]
    public string Email
    {
        get => this.GetAttr<string>("EMAIL");
        set => this.SetAttr("EMAIL", value);
    }
    
    public Guid OrganizationUnit
    {
        get => this.GetAttr<Guid>("OUREF");
        set => this.SetAttr("OUREF", value);
    }
}