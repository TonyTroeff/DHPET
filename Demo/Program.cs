using System.Linq.Expressions;
using Demo.Models;
using Demo.Validation;
using Extensions;
using Localization;
using Mathematics;
using Microsoft.Extensions.DependencyInjection;
using Queries;
using Validation;

namespace Demo;

public static class Program
{
    private static readonly List<Person> _people =
    [
        new("George", 35, false),
        new("John", 27, true),
        new("Michael", 16, false),
        new("Smith", 17, true),
        new("Sharon", 22, false),
        new("Bob", 22, true)
    ];
    
    public static async Task Main()
    {
        // x + 3
        var formula1 = FormulaParser.Parse("(+ x 3)");
        Console.WriteLine(formula1);

        // 2x + 3
        var formula2 = FormulaParser.Parse("(+ (* x 2) 3)");
        Console.WriteLine(formula2);

        // x^2 + x/3 + 7
        var formula3 = FormulaParser.Parse("(+ (+ (* x x) (/ x 3)) 7)");
        Console.WriteLine(formula3);

        Expression<Func<Person, bool>> isAdult = p => p.Age > 18;
        Expression<Func<Person, bool>> isEmployed = p => p.IsEmployed;
        var e1 = isAdult.OrElse(isEmployed);
        QueryPeople(e1);

        var e2 = isAdult.AndAlso(isEmployed.Negate());
        QueryPeople(e2);

        var e3 = isAdult.Condition(p => $"{p.Name} is adult", p => $"{p.Name} is still a kid");
        Console.WriteLine(e3);

        Expression<Func<Person, bool>> e4 = p => p.Age > 18 && p.Name.StartsWith("Tony") && p.Name.Count(c => c == 'a') > 10;

        Console.WriteLine(e4.ToString());
        Console.WriteLine(QueryAnalyzer.Analyze(e4));

        ResourcesCache.Initialize();
        var labelProvider = new LabelProvider();

        var label1 = LabelBuilder.FromExpression(lp => lp.Jobs.QA);
        ArgumentNullException.ThrowIfNull(label1);
        Console.WriteLine(label1);

        var consumer1 = LabelConsumers.Build(label1);
        ArgumentNullException.ThrowIfNull(consumer1);
        Console.WriteLine(consumer1.GetLabel(labelProvider));

        var label2 = LabelBuilder.FromExpression(lp => lp.Jobs.Dev("Senior"));
        ArgumentNullException.ThrowIfNull(label2);
        Console.WriteLine(label2);

        var consumer2 = LabelConsumers.Build(label2);
        ArgumentNullException.ThrowIfNull(consumer2);
        Console.WriteLine(consumer2.GetLabel(labelProvider));

        var label3 = LabelBuilder.FromExpression(lp => lp.Technologies.CSharp(true, true, false));
        ArgumentNullException.ThrowIfNull(label3);
        Console.WriteLine(label3);

        var consumer3 = LabelConsumers.Build(label3);
        ArgumentNullException.ThrowIfNull(consumer3);
        Console.WriteLine(consumer3.GetLabel(labelProvider));


        var label4 = LabelBuilder.FromExpression(lp => lp.Jobs.Test);
        ArgumentNullException.ThrowIfNull(label4);
        Console.WriteLine(label4);

        var consumer4 = LabelConsumers.Build(label4);
        ArgumentNullException.ThrowIfNull(consumer4);
        Console.WriteLine(consumer4.GetLabel(labelProvider));

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IExhaustiveValidator, ExhaustiveValidator>();
        serviceCollection.AddScoped<IExhaustiveValidator<BaseSetting>, BaseSettingValidator>();
        serviceCollection.AddScoped<IExhaustiveValidator<StringSetting>, StringSettingValidator>();
        serviceCollection.AddScoped<IExhaustiveValidator<Int32Setting>, Int32SettingValidator>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var stringSetting = new StringSetting { Name = "Test1", Value = "Hello, world" };
        var int32Setting = new Int32Setting { Name = "Test2", Value = 13 };

        var exhaustiveValidator = scope.ServiceProvider.GetRequiredService<IExhaustiveValidator>();
        await exhaustiveValidator.ExecuteAllValidationRulesAsync(stringSetting, CancellationToken.None);
        await exhaustiveValidator.ExecuteAllValidationRulesAsync(int32Setting, CancellationToken.None);

    }
    
    private static void QueryPeople(Expression<Func<Person, bool>> expression)
    {
        var matchingPeople = _people.AsQueryable().Where(expression).ToList();
        Console.WriteLine($"Expression: {expression}");
        Console.WriteLine($"Matching people: {string.Join(", ", matchingPeople.Select(p => p.Name))}");
        Console.WriteLine();
    }
}