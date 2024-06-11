using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Queries.Attributes;
using TryAtSoftware.Extensions.Reflection;

namespace Queries;

public class QueryAnalyzer
{
    public static string Analyze<T>(Expression<T> expression)
    {
        var analyzer = new ExpressionAnalyzingVisitor();
        analyzer.Visit(expression);

        return analyzer.BuildResult();
    }
}

public class ExpressionAnalyzingVisitor : ExpressionVisitor
{
    private static readonly Dictionary<ExpressionType, string> _operatorByExpressionType = new() { { ExpressionType.AndAlso, " and " }, { ExpressionType.OrElse, " or " }, { ExpressionType.GreaterThan, " > " }, { ExpressionType.GreaterThanOrEqual, " >= " }, { ExpressionType.LessThan, " < " }, { ExpressionType.LessThanOrEqual, " <= " }, { ExpressionType.Equal, " equals " } };

    private readonly StringBuilder _buffer = new();

    public string BuildResult() => this._buffer.ToString();

    protected override Expression VisitBinary(BinaryExpression node)
    {
        this._buffer.Append('(');
        this.Visit(node.Left);
        this._buffer.Append(_operatorByExpressionType[node.NodeType]);
        this.Visit(node.Right);
        this._buffer.Append(')');

        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        this._buffer.Append(node.Value);
        return node;
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        this._buffer.Append('(');
        for (var i = 0; i < node.Parameters.Count; i++)
        {
            if (i > 0) this._buffer.Append(", ");
            this._buffer.Append(node.Parameters[i].Name);
        }

        this._buffer.Append(')');
        this._buffer.Append(" => ");
        this.Visit(node.Body);

        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression is not null)
        {
            this.Visit(node.Expression);
            this._buffer.Append("->");
        }

        var queryNameAttribute = node.Member.GetCustomAttribute<QueryNameAttribute>();
        if (queryNameAttribute is not null)
        {
            this._buffer.Append('`');
            this._buffer.Append(queryNameAttribute.Name);
            this._buffer.Append('`');
        }
        else
        {
            this._buffer.Append(node.Member.Name);
        }

        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Object is not null)
        {
            this.Visit(node.Object);
            this._buffer.Append("->");
        }

        this._buffer.Append(node.Method.Name);
        this._buffer.Append('(');

        for (var i = 0; i < node.Arguments.Count; i++)
        {
            if (i != 0) this._buffer.Append(", ");
            this.Visit(node.Arguments[i]);
        }

        this._buffer.Append(')');
        
        return node;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        this._buffer.Append('[');
        if (!string.IsNullOrEmpty(node.Name))
            this._buffer.Append(node.Name);
        this._buffer.Append(']');

        return node;
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        if (node.NodeType == ExpressionType.Convert)
        {
            this.Visit(node.Operand);
            this._buffer.Append(" as ");
            this._buffer.Append(TypeNames.Get(node.Type));

            return node;
        }

        return base.VisitUnary(node);
    }
}