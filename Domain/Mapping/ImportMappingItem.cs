using System.Xml.Serialization;

namespace Domain.Mapping;

public sealed class ImportMappingItem
{
    [XmlAttribute("source")] public string SourceFieldName { get; set; } = null!;
    [XmlAttribute("target")] public string TargetFieldName { get; set; } = null!;
}