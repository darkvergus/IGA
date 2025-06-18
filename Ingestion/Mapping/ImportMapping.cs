using System.Collections;
using System.Xml.Serialization;

namespace Ingestion.Mapping;

/// <summary>
/// XML‑serialisable definition of how source fields map to a Core entity.
/// </summary>
[XmlRoot("ImportMapping")]
public sealed class ImportMapping
{
    [XmlAttribute("targetType")] public string TargetType { get; set; } = null!;
    [XmlAttribute("primaryKeyProperty")] public string? PrimaryKeyProperty { get; set; }

    [XmlElement("Field")] public List<ImportMappingItem> FieldMappings { get; set; } = [];

    [XmlIgnore] public Type? TargetEntityType { get; set; }

    public ImportMapping() { }

    public ImportMapping(Type entity) => TargetEntityType = entity;
}