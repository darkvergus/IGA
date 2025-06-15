using System.Xml.Serialization;

namespace Ingestion.Mapping;

[XmlRoot("DataModel")]
public class PluginDataModel
{
    [XmlAttribute("targetType")] public string TargetType { get; set; } = null!;
    [XmlElement("Attribute")] public List<DataModelField> Attributes { get; set; } = new();
}