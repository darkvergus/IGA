using System.Xml.Serialization;

namespace Domain.Connection;

public sealed class ConnectionFieldDefinition
{
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("label")]
    public string Label { get; set; } = string.Empty;

    [XmlAttribute("description")]
    public string Description { get; set; } = string.Empty;

    [XmlAttribute("type")]
    public ConnectionFieldType FieldType { get; set; } = ConnectionFieldType.Text;

    [XmlAttribute("required")]
    public bool IsRequired { get; set; }

    [XmlAttribute("secret")]
    public bool IsSecret { get; set; }

    [XmlAttribute("default")]
    public string? DefaultValue { get; set; }
}