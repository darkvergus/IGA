using Domain.Connection;

namespace Web.Models;

public sealed class ConnectionFieldViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ConnectionFieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public bool IsSecret { get; set; }
    public string? Value { get; set; }
}