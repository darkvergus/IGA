using System.Xml.Serialization;

namespace Domain.Connection;

[XmlRoot("ConnectionDefinition")]
public sealed class ConnectionDefinition
{
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("Field")]
    public List<ConnectionFieldDefinition> Fields { get; set; } = [];
}