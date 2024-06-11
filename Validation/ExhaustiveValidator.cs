using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Validation;

public class ExhaustiveValidator(IServiceProvider serviceProvider) : IExhaustiveValidator
{
    private static readonly ConcurrentDictionary<Type, List<Func<object, IExhaustiveValidatingMachine>>> _machinesCache = new ();
    private static readonly ConcurrentDictionary<Type, Func<object, IExhaustiveValidatingMachine>> _machineBuilders = new ();

    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public async Task<bool> ExecuteAllValidationRulesAsync(object entity, CancellationToken cancellationToken)
    {
        var validatingMachines = ComposeAllValidatingMachines(entity, this.CanUseValidator);
        foreach (var validatingMachine in validatingMachines)
        {
            var validationResult = await validatingMachine.TriggerValidationAsync(this._serviceProvider, cancellationToken);
            if (!validationResult) return false;
        }

        return true;
    }

    private bool CanUseValidator(Type validatorType) => this._serviceProvider.GetService(validatorType) is not null;

    private static IEnumerable<IExhaustiveValidatingMachine> ComposeAllValidatingMachines(object entity, Func<Type, bool>? canUseValidator = null)
    {
        return ComposeAllValidatingMachineBuilders(entity, canUseValidator).Select(x => x.Invoke(entity));
    }

    private static IEnumerable<Func<object, IExhaustiveValidatingMachine>> ComposeAllValidatingMachineBuilders(object entity, Func<Type, bool>? canUseValidator = null)
    {
        var entityType = entity.GetType();
        if (_machinesCache.TryGetValue(entityType, out var cachedMachineBuilders)) return cachedMachineBuilders;

        var allValidatingMachineBuilders = new List<Func<object, IExhaustiveValidatingMachine>>();

        var typeHierarchy = new Stack<Type>();
        typeHierarchy.Push(entityType);

        var baseType = entityType.BaseType;
        while (baseType != null)
        {
            typeHierarchy.Push(baseType);
            baseType = baseType.BaseType;
        }

        var interfaces = entityType.GetInterfaces();
        foreach (var implementedInterfaceType in interfaces) AddValidatingMachineBuilders(implementedInterfaceType);
        while (typeHierarchy.Count > 0) AddValidatingMachineBuilders(typeHierarchy.Pop());

        _machinesCache[entityType] = allValidatingMachineBuilders;
        return allValidatingMachineBuilders;

        void AddValidatingMachineBuilders(Type validatedType)
        {
            if (canUseValidator is not null)
            {
                var validatorType = typeof(IExhaustiveValidator<>).MakeGenericType(validatedType);
                if (!canUseValidator(validatorType)) return;
            }
                
            var machineBuilder = BuildMachine(validatedType);
            allValidatingMachineBuilders.Add(machineBuilder);
        }
    }
        
    private static Func<object, IExhaustiveValidatingMachine> BuildMachine(Type dataType)
    {
        if (_machineBuilders.TryGetValue(dataType, out var preCompiledFunc)) return preCompiledFunc;

        var dataParameter = Expression.Parameter(typeof(object));

        var validatingMachineType = typeof(ExhaustiveValidatingMachine<>).MakeGenericType(dataType);
        var mappingMachineConstructor = validatingMachineType.GetConstructor(new[] { dataType });

        var buildMapper = Expression.Lambda<Func<object, IExhaustiveValidatingMachine>>(Expression.New(mappingMachineConstructor!, Expression.Convert(dataParameter, dataType)), dataParameter);
        var compiledFunc = buildMapper.Compile();

        _machineBuilders[dataType] = compiledFunc;
        return compiledFunc;
    }
        
    private interface IExhaustiveValidatingMachine
    {
        Task<bool> TriggerValidationAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }

    private sealed class ExhaustiveValidatingMachine<TEntity>(TEntity instanceToValidate) : IExhaustiveValidatingMachine
    {
        public Task<bool> TriggerValidationAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var validator = serviceProvider.GetService<IExhaustiveValidator<TEntity>>();
            if (validator is null) return Task.FromResult(false);
            return validator.ValidateAsync(instanceToValidate, cancellationToken);
        }
    }
}