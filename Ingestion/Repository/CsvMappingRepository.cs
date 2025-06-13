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
                    new ImportMappingItem(sourceFieldName: "BusinessKey", targetFieldName: "BUSINESSKEY"),
                    new ImportMappingItem(sourceFieldName: "FirstName", targetFieldName: "FIRSTNAME"),
                    new ImportMappingItem(sourceFieldName: "LastName", targetFieldName: "LASTNAME"),
                    new ImportMappingItem(sourceFieldName: "Email", targetFieldName: "EMAIL")
                ]
            };
        }

        throw new NotSupportedException($"No mapping registered for entity: {entity}");
    }
}