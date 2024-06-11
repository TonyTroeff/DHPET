using Demo.Models;
using Validation;

namespace Demo.Validation;

public class Int32SettingValidator : IExhaustiveValidator<Int32Setting>
{
    public Task<bool> ValidateAsync(Int32Setting entity, CancellationToken cancellationToken)
    {
        Console.WriteLine("Hello from Int32SettingValidator");
        return Task.FromResult(true);
    }
}