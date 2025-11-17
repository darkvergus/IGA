using System.ComponentModel.DataAnnotations;
using Core.Attributes;
using Core.Extensions;

namespace Core.Entities;

public partial record Identity
{
    [MaxLength(64), Unique]
    public string IdentityID
    {
        get => this.GetAttr<string>("IDENTITYID");
        set => this.SetAttr("IDENTITYID", value);
    }
    
    [MaxLength(64)]
    public string BusinessKey
    {
        get => this.GetAttr<string>("BUSINESSKEY");
        set => this.SetAttr("BUSINESSKEY", value);
    }
    
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
    
    public DateTime ValidFrom
    {
        get => this.GetAttr<DateTime>("VALIDFROM");
        set => this.SetAttr("VALIDFROM", value);
    }
    
    public DateTime ValidTo
    {
        get => this.GetAttr<DateTime>("VALIDTO");
        set => this.SetAttr("VALIDTO", value);
    }
}