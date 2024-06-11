using Demo.Models;
using Validation;

namespace Demo.Validation;

public class StringSettingValidator : IExhaustiveValidator<StringSetting>
{
    public Task<bool> ValidateAsync(StringSetting entity, CancellationToken cancellationToken)
    {
        Console.WriteLine("Hello from StringSettingValidator");
        return Task.FromResult(true);
    }
}