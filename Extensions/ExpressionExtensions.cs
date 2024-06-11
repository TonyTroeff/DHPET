using System.Linq.Expressions;

namespace Extensions;

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> Negate<T>(this Expression<Func<T, bool>> expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        return Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters);
    }

    public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        if (first is null) throw new ArgumentNullException(nameof(first));
        if (second is null) throw new ArgumentNullException(nameof(second));

        var parameter = Expression.Parameter(typeof(T), "t");
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(first.ReuseBody([parameter]), second.ReuseBody([parameter])), parameter);
    }

    /*public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        if (first is null) throw new ArgumentNullException(nameof(first));
        if (second is null) throw new ArgumentNullException(nameof(second));

        var parameter = Expression.Parameter(typeof(T), "t");
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(Expression.Invoke(first, parameter), Expression.Invoke(second, parameter)), parameter);
    }*/

    public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        if (first is null) throw new ArgumentNullException(nameof(first));
        if (second is null) throw new ArgumentNullException(nameof(second));

        var parameter = Expression.Parameter(typeof(T), "t");
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(first.ReuseBody([parameter]), second.ReuseBody([parameter])), parameter);
    }

    /*public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        if (first is null) throw new ArgumentNullException(nameof(first));
        if (second is null) throw new ArgumentNullException(nameof(second));

        var parameter = Expression.Parameter(typeof(T), "t");
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(Expression.Invoke(first, parameter), Expression.Invoke(second, parameter)), parameter);
    }*/
    
    public static Expression<Func<TEntity, TValue>> Condition<TEntity, TValue>(this Expression<Func<TEntity, bool>> conditionExpression, Expression<Func<TEntity, TValue>> ifExpression, Expression<Func<TEntity, TValue>> elseExpression)
    {
        if (conditionExpression is null) throw new ArgumentNullException(nameof(conditionExpression));

        var parameter = Expression.Parameter(typeof(TEntity), "t");

        Expression body = Expression.Condition(conditionExpression.ReuseBody([parameter]), ifExpression.ReuseBody([parameter]), elseExpression.ReuseBody([parameter]));
        return Expression.Lambda<Func<TEntity, TValue>>(body, parameter);
    }

    private static Expression ReuseBody<TDelegate>(this Expression<TDelegate> expression, ParameterExpression[] newParameters)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        if (newParameters.Length != expression.Parameters.Count) throw new InvalidOperationException($"Expected {expression.Parameters.Count} elements within the list of new parameters.");

        var body = expression.Body;

        for (var i = 0; i < newParameters.Length; i++)
        {
            var replaceVisitor = new ReplaceParameterVisitor(expression.Parameters[i], newParameters[i]);
            body = replaceVisitor.Visit(body);
        }

        return body;
    }
}