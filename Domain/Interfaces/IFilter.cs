namespace Domain.Interfaces;

/// <summary>
/// Interface for filtering rules in the pipeline.
/// Implementations decide whether a given entity should be included or skipped.
/// </summary>
public interface IFilter<T>
{
    bool ShouldInclude(T item);
}