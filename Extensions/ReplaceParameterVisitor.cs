using System.Linq.Expressions;

namespace Extensions;

public class ReplaceParameterVisitor(ParameterExpression prev, ParameterExpression next) : ExpressionVisitor
{
    private readonly ParameterExpression _prev = prev ?? throw new ArgumentNullException(nameof(prev));
    private readonly ParameterExpression _next = next ?? throw new ArgumentNullException(nameof(next));

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (node == this._prev) return this._next;
        return base.VisitParameter(node);
    }
}