using System.Xml.Serialization;
using Core.Enums;

namespace Ingestion.Mapping;

public class DataModelField
{
    [XmlAttribute("source")] public string Source { get; set; } = null!;
    [XmlAttribute("type")] public AttributeDataType Type { get; set; }
    [XmlAttribute("required")] public bool Required { get; set; }
}