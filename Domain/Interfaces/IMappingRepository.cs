using Domain.Mapping;

namespace Domain.Interfaces;

public interface IMappingRepository
{
    ImportMapping? Get(string entity);
}