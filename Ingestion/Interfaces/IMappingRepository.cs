using Ingestion.Mapping;

namespace Ingestion.Interfaces;

public interface IMappingRepository
{
    ImportMapping? Get(string entity);
}