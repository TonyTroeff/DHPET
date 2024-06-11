using Demo.Models;
using Validation;

namespace Demo.Validation;

public class BaseSettingValidator : IExhaustiveValidator<BaseSetting>
{
    public Task<bool> ValidateAsync(BaseSetting entity, CancellationToken cancellationToken)
    {
        Console.WriteLine("Hello from BaseSettingValidator");
        return Task.FromResult(true);
    }
}