namespace Core.Enums;

/// <summary>Categorization of connector implementations.
/// </summary>
public enum ResourceType
{
    Unknown = 0,
    Ldap = 1,
    Database = 2,
    RestApi = 3,
    SaaS = 4
}