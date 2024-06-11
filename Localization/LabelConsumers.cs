using System.Linq.Expressions;
using System.Reflection;

namespace Localization;

public class LabelConsumers
{
    private delegate ILabelConsumer LabelConsumerFactory(IDictionary<string, object?> parameters);

    private static readonly Dictionary<string, LabelConsumerFactory> _consumerFactories = new();
    private static readonly MethodInfo _getValueMethod = typeof(ParametersHelper).GetMethod(nameof(ParametersHelper.GetValueOrDefault), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static ILabelConsumer? Build(Label label)
    {
        var consumerFactory = EnsureFactory(label.Identifier);
        return consumerFactory?.Invoke(label.Parameters);
    }

    private static LabelConsumerFactory? EnsureFactory(string labelId)
    {
        if (string.IsNullOrWhiteSpace(labelId)) return null;
        if (_consumerFactories.TryGetValue(labelId, out var factory)) return factory;

        var labelConsumer = PrepareConsumerFactory(labelId);
        _consumerFactories.Add(labelId, labelConsumer);
        return labelConsumer;
    }

    private static LabelConsumerFactory PrepareConsumerFactory(string labelId)
    {
        var labelParts = labelId.Split('_', StringSplitOptions.RemoveEmptyEntries);

        var labelProviderParameter = Expression.Parameter(typeof(ILabelProvider));

        var path = new List<PropertyInfo>(capacity: labelParts.Length - 1);
        var parentType = typeof(ILabelProvider);
        for (var i = 0; i < labelParts.Length - 1; i++)
        {
            var member = ResourcesCache.GetMemberInfo(parentType, labelParts[i]);
            if (member is null) throw new InvalidOperationException("Invalid label identifier. Member not found.");
            if (!(member is PropertyInfo propertyInfo)) throw new InvalidOperationException("Invalid label identifier. Member is not a property.");

            path.Add(propertyInfo);
            parentType = propertyInfo.PropertyType;
        }

        var instance = path.Aggregate<PropertyInfo, Expression>(labelProviderParameter, Expression.Property);
        var lastMember = ResourcesCache.GetMemberInfo(parentType, labelParts[^1]);

        return lastMember switch
        {
            PropertyInfo labelProperty => PrepareConsumerFactory(instance, labelProperty, labelProviderParameter),
            MethodInfo labelMethod => PrepareConsumerFactory(instance, labelMethod, labelProviderParameter),
            _ => throw new InvalidOperationException("A label consumer cannot be instantiated.")
        };
    }

    private static LabelConsumerFactory PrepareConsumerFactory(Expression instance, MethodInfo labelMethod, ParameterExpression labelProviderParameter)
    {
        var parameters = labelMethod.GetParameters();

        var dictionaryParameter = Expression.Parameter(typeof(IDictionary<string, object?>));
        var variables = new List<ParameterExpression>();
        var assignments = new List<Expression>();

        foreach (var parameterInfo in parameters)
        {
            var parameterId = ResourcesCache.GetParameterIdentifier(parameterInfo);
            var variable = Expression.Variable(parameterInfo.ParameterType, parameterId);
            variables.Add(variable);

            var setValue = Expression.Assign(variable, Expression.Call(null, _getValueMethod.MakeGenericMethod(parameterInfo.ParameterType), dictionaryParameter, Expression.Constant(parameterId)));
            assignments.Add(setValue);
        }

        var callExpression = Expression.Call(instance, labelMethod, variables);
        var expressionBody = Expression.Block(variables, [..assignments, callExpression]);
        var prepareConsumerExpression = Expression.Lambda<Func<ILabelProvider, IDictionary<string, object?>, string>>(expressionBody, labelProviderParameter, dictionaryParameter);
        var prepareConsumerFunction = prepareConsumerExpression.Compile();
        return invocationParams => new DynamicLabelConsumer(labelProvider => prepareConsumerFunction(labelProvider, invocationParams));
    }

    private static LabelConsumerFactory PrepareConsumerFactory(Expression instance, PropertyInfo labelProperty, ParameterExpression labelProviderParameter)
    {
        var accessLabel = Expression.Property(instance, labelProperty);
        var propertyAccessingExpression = Expression.Lambda<Func<ILabelProvider, string>>(accessLabel, labelProviderParameter);
        var propertyAccessingFunction = propertyAccessingExpression.Compile();

        return _ => new DynamicLabelConsumer(propertyAccessingFunction);
    }
}