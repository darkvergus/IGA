using Core.Domain.Entities;
using Ingestion.Mapping;

namespace Ingestion.Repository;

public static class CsvMappingRepository
{
    public static ImportMapping Get(string entity)
    {
        if (entity.Equals("identity", StringComparison.OrdinalIgnoreCase))
        {
            return new ImportMapping(typeof(Identity))
            {
                FieldMappings =
                [
                    new ImportMappingItem(sourceFieldName: "firstName", targetFieldName: "FIRSTNAME"),
                    new ImportMappingItem(sourceFieldName: "lastName", targetFieldName: "LASTNAME"),
                    new ImportMappingItem(sourceFieldName: "email", targetFieldName: "EMAIL")
                ]
            };
        }

        throw new NotSupportedException($"No mapping registered for entity: {entity}");
    }
}