namespace Ingestion.Interfaces;

public interface IValidator
{
    ValueTask ValidateAsync(object entity, CancellationToken cancellationToken = default);
}