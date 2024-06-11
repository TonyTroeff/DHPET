namespace Validation;

public interface IExhaustiveValidator
{
    Task<bool> ExecuteAllValidationRulesAsync(object entity, CancellationToken cancellationToken);
}

public interface IExhaustiveValidator<in TEntity>
{
    Task<bool> ValidateAsync(TEntity entity, CancellationToken cancellationToken);
}