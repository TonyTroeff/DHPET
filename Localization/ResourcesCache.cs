using System.Reflection;
using Localization.Attributes;

namespace Localization;

public static class ResourcesCache
{
    private static readonly object _lock = new object();

    private static readonly Dictionary<Type, Dictionary<string, MemberInfo>> _cacheByIdentifiers = new();
    private static readonly Dictionary<MemberInfo, string> _cacheByMember = new();
    private static readonly Dictionary<ParameterInfo, string> _parametersCache = new();
    private static bool _isInitialized;

    public static void Initialize()
    {
        lock (_lock)
        {
            if (_isInitialized) return;
            _isInitialized = true;

            Iterate(typeof(ILabelProvider));
        }
    }

    public static string GetIdentifier(MemberInfo memberInfo)
    {
        if (_cacheByMember.TryGetValue(memberInfo, out var identifier) == false) return string.Empty;
        return identifier;
    }

    public static MemberInfo? GetMemberInfo(Type parentType, string identifier)
    {
        if (!_cacheByIdentifiers.TryGetValue(parentType, out var cachedMembers)) return null;
        return cachedMembers.GetValueOrDefault(identifier);
    }

    public static string GetParameterIdentifier(ParameterInfo parameterInfo)
    {
        if (!_parametersCache.TryGetValue(parameterInfo, out var parameterName)) return string.Empty;
        return parameterName;
    }

    private static void Iterate(Type type)
    {
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            if (!TryRegisterMember(property)) continue;
            Iterate(property.PropertyType);
        }

        var methods = type.GetMethods().Where(m => !m.IsSpecialName);
        foreach (var method in methods)
        {
            TryRegisterMember(method);

            var parameters = method.GetParameters();
            foreach (var parameter in parameters) TryRegisterParameter(parameter);
        }
    }

    private static bool TryRegisterMember(MemberInfo member)
    {
        if (member.DeclaringType is null) return false;

        var resourceIdAttribute = member.GetCustomAttribute<ResourceIdAttribute>();
        var resourceIdentifier = resourceIdAttribute?.Name;
        if (string.IsNullOrWhiteSpace(resourceIdentifier)) return false;

        _cacheByMember[member] = resourceIdentifier;
        if (!_cacheByIdentifiers.TryGetValue(member.DeclaringType, out var typedIdentifiersCache))
        {
            typedIdentifiersCache = new Dictionary<string, MemberInfo>();
            _cacheByIdentifiers[member.DeclaringType] = typedIdentifiersCache;
        }

        typedIdentifiersCache[resourceIdentifier] = member;
        return true;
    }

    private static void TryRegisterParameter(ParameterInfo parameter)
    {
        var resourceParameterAttribute = parameter.GetCustomAttribute<ResourceParameterAttribute>();

        var parameterName = resourceParameterAttribute?.Name;
        if (string.IsNullOrWhiteSpace(parameterName)) parameterName = parameter.Name!;

        _parametersCache[parameter] = parameterName;
    }
}