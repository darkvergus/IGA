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
    
    public Guid AccountId
    {
        get => this.GetAttr<Guid>("ACCOUNTREF");
        set => this.SetAttr("ACCOUNTREF", value);
    }
    
    public Guid OrganizationUnitId
    {
        get => this.GetAttr<Guid>("OUREF");
        set => this.SetAttr("OUREF", value);
    }
}