using System.Linq.Expressions;
using System.Text;

namespace Mathematics;

public static class FormulaParser
{
    // (<operator> <operand1> <operand2>)
    public static Expression<Func<int, int>> Parse(string formulaExpression)
    {
        var stack = new Stack<Operation>();
        var stringBuilder = new StringBuilder();

        Operation? lastOperation = null;
        var expectTermination = false;

        var parameter = Expression.Parameter(typeof(int), "x");

        int index = 0;
        while (index < formulaExpression.Length)
        {
            if (char.IsWhiteSpace(formulaExpression[index])) index++;
            else if (expectTermination) throw new InvalidOperationException($"Invalid character at index {index}. Expected termination.");
            else if (formulaExpression[index] == '(')
            {
                index++;
                while (index < formulaExpression.Length && !char.IsWhiteSpace(formulaExpression[index]))
                    stringBuilder.Append(formulaExpression[index++]);

                var @operator = stringBuilder.ToString();
                if (@operator.Length == 0) throw new InvalidOperationException("An operator must follow the opening brace.");

                stringBuilder.Length = 0;
                var nextOperation = new Operation { Operator = @operator };
                stack.Push(nextOperation);
            }
            else if (formulaExpression[index] == ')')
            {
                if (!stack.TryPop(out lastOperation)) throw new InvalidOperationException("Too many closing braces.");

                if (stack.TryPeek(out var prevOperation)) SetOperand(prevOperation, MaterializeOperation(lastOperation));
                else expectTermination = true;

                index++;
            }
            else if (formulaExpression[index] == '-' || char.IsNumber(formulaExpression[index]))
            {
                if (!stack.TryPeek(out var prevOperation)) throw new InvalidOperationException("A numeric literal is used outside of a formula expression.");

                var shouldBeNegative = formulaExpression[index] == '-';
                if (shouldBeNegative) index++;
                
                var number = 0;
                while (index < formulaExpression.Length && char.IsDigit(formulaExpression[index]))
                    number = number * 10 + (formulaExpression[index++] - '0');

                if (shouldBeNegative) number *= -1;
                SetOperand(prevOperation, Expression.Constant(number));
            }
            else if (char.IsLetter(formulaExpression[index]))
            {
                if (!stack.TryPeek(out var prevOperation)) throw new InvalidOperationException("Variable is used outside of a formula expression.");

                while (index < formulaExpression.Length && char.IsLetter(formulaExpression[index]))
                    stringBuilder.Append(formulaExpression[index++]);

                var paramName = stringBuilder.ToString();
                stringBuilder.Length = 0;

                if (paramName != "x") throw new InvalidOperationException($"The parameter [{paramName}] is not recognized");
                SetOperand(prevOperation, parameter);
            }
            else throw new InvalidOperationException($"Unexpected character on index {index}");
        }

        if (!expectTermination || lastOperation is null) throw new InvalidOperationException("An empty formula expression cannot be processed.");

        return Expression.Lambda<Func<int, int>>(MaterializeOperation(lastOperation), [parameter]);
    }

    private static Expression MaterializeOperation(Operation operation)
    {
        if (operation.Left is null || operation.Right is null) throw new InvalidOperationException("Each operation must have two operands.");

        return operation.Operator switch
        {
            "+" => Expression.Add(operation.Left, operation.Right),
            "-" => Expression.Subtract(operation.Left, operation.Right),
            "*" => Expression.Multiply(operation.Left, operation.Right),
            "/" => Expression.Divide(operation.Left, operation.Right),
            _ => throw new InvalidOperationException($"The operator [{operation.Operator}] cannot be processed.")
        };
    }

    private static void SetOperand(Operation operation, Expression operand)
    {
        if (operation.Left is null) operation.Left = operand;
        else if (operation.Right is null) operation.Right = operand;
        else throw new InvalidOperationException("Operations cannot have more than two operands");
    }

    private class Operation
    {
        public required string Operator { get; init; }
        public Expression? Left { get; set; }
        public Expression? Right { get; set; }
    }
}