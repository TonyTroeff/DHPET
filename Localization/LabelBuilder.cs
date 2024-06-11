using System.Linq.Expressions;
using System.Reflection;
using TryAtSoftware.Extensions.Reflection;

namespace Localization;

public static class LabelBuilder
{
    public static Label? FromExpression(Expression<Func<ILabelProvider, string>> expression) => FromExpression(expression.Body);

    private static Label? FromExpression(Expression messageExpression)
        => messageExpression switch
        {
            MemberExpression memberExpression => FromMemberExpression(memberExpression),
            MethodCallExpression methodCallExpression => FromMethodCallExpression(methodCallExpression),
            _ => null
        };

    private static Label? FromMemberExpression(MemberExpression memberExpression)
    {
        var messageIdentifier = GetMessageIdentifier(memberExpression);
        if (string.IsNullOrWhiteSpace(messageIdentifier)) return null;
        return new Label(messageIdentifier);
    }

    private static Label? FromMethodCallExpression(MethodCallExpression methodCallExpression)
    {
        var messageIdentifier = GetMessageIdentifier(methodCallExpression);
        if (string.IsNullOrWhiteSpace(messageIdentifier)) return null;
        
        var labelInfo = new Label(messageIdentifier);

        var methodCallParameters = methodCallExpression.Method.GetParameters();
        for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
        {
            var currentParameter = methodCallParameters[i];
            var currentArgument = methodCallExpression.Arguments[i];

            var value = currentArgument switch
            {
                ConstantExpression constantExpressionArgument => constantExpressionArgument.Value,
                MemberExpression { Expression: ConstantExpression parent } memberExpressionArgument => memberExpressionArgument.Member.GetValue(parent.Value),
                MemberExpression { Expression: null } staticMemberExpressionArgument => staticMemberExpressionArgument.Member.GetValue(null),
                _ => throw new InvalidOperationException("You have provided an invalid method call expression.")
            };

            var parameterId = GetParameterIdentifier(currentParameter);
            labelInfo.Parameters[parameterId] = value;
        }

        return labelInfo;
    }

    private static string GetMessageIdentifier(Expression? expression)
        => expression switch
        {
            MemberExpression memberExpression => GetMessageIdentifier(memberExpression),
            MethodCallExpression methodCallExpression => GetMessageIdentifier(methodCallExpression),
            _ => string.Empty
        };

    private static string GetMessageIdentifier(MemberExpression memberExpression)
    {
        var iteratedMemberExpression = memberExpression;
        var members = new Stack<string>();
        while (iteratedMemberExpression != null)
        {
            var member = iteratedMemberExpression.Member;
            if (CanGetResourceName(member, out var resourceName) == false) return string.Empty;

            members.Push(resourceName);
            iteratedMemberExpression = iteratedMemberExpression.Expression as MemberExpression;
        }

        return CombineMessageIdentifiers(members.ToArray());
    }

    private static string GetMessageIdentifier(MethodCallExpression methodCallExpression)
    {
        if (CanGetResourceName(methodCallExpression.Method, out var resourceName) == false) return string.Empty;
        if (string.IsNullOrWhiteSpace(resourceName)) return string.Empty;

        var baseId = GetMessageIdentifier(methodCallExpression.Object);
        return CombineMessageIdentifiers(baseId, resourceName);
    }

    private static bool CanGetResourceName(MemberInfo member, out string resourceName)
    {
        resourceName = ResourcesCache.GetIdentifier(member);
        return string.IsNullOrWhiteSpace(resourceName) == false;
    }

    private static string GetParameterIdentifier(ParameterInfo parameter) => ResourcesCache.GetParameterIdentifier(parameter);

    private static string CombineMessageIdentifiers(params string[] identifiers) => string.Join('_', identifiers);
}