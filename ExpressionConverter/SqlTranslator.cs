using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ExpressionConverter;

public class SqlTranslator
{
    private static readonly List<object> _parameters = new();
    private static int _parameterIndex;

    public static string Translate<T>(Expression<Func<T, bool>> predicate)
    {
        StringBuilder sb = new("SELECT * FROM ");

        sb.Append(typeof(T).Name);

        sb.Append(" WHERE ");

        Visit(predicate.Body, sb);

        return sb.ToString();
    }

    private static void Visit(Expression expression, StringBuilder sb)
    {
        while (true)
        {
            switch (expression)
            {
                case BinaryExpression binaryExpression:
                    VisitBinary(binaryExpression, sb);
                    break;
                case ConstantExpression constantExpression:
                    VisitConstant(constantExpression, sb);
                    break;
                case MemberExpression memberExpression:
                    VisitMember(memberExpression, sb);
                    break;
                case UnaryExpression unaryExpression:
                    expression = unaryExpression.Operand;
                    continue;
                default:
                    throw new NotSupportedException($"The expression type '{expression.GetType()}' is not supported.");
            }

            break;
        }
    }

    private static void VisitBinary(BinaryExpression expression, StringBuilder sb)
    {
        sb.Append('(');

        Visit(expression.Left, sb);

        sb.Append(GetSqlOperator(expression.NodeType));

        Visit(expression.Right, sb);

        sb.Append(')');
    }

    private static void VisitConstant(ConstantExpression expression, StringBuilder sb)
    {
        string parameterName = $"@p{_parameterIndex++}";

        sb.Append(parameterName);

        _parameters.Add(expression.Value!);
    }

    private static void VisitMember(MemberExpression expression, StringBuilder sb)
    {
        sb.Append(GetColumnName(expression));
    }

    private static string GetColumnName(MemberExpression expression)
    {
        if (!(expression.Member is PropertyInfo propertyInfo))
            throw new NotSupportedException("Only property members are supported for column names.");

        return propertyInfo.Name;
    }

    private static string GetSqlOperator(ExpressionType expressionType)
    {
        return expressionType switch
        {
            ExpressionType.Add => " + ",
            ExpressionType.AddChecked => " + ",
            ExpressionType.And => " & ",
            ExpressionType.AndAlso => " AND ",
            ExpressionType.Coalesce => " COALESCE ",
            ExpressionType.Divide => " / ",
            ExpressionType.Equal => " = ",
            ExpressionType.ExclusiveOr => " ^ ",
            ExpressionType.GreaterThan => " > ",
            ExpressionType.GreaterThanOrEqual => " >= ",
            ExpressionType.LeftShift => " << ",
            ExpressionType.LessThan => " < ",
            ExpressionType.LessThanOrEqual => " <= ",
            ExpressionType.Modulo => " % ",
            ExpressionType.Multiply => " * ",
            ExpressionType.MultiplyChecked => " * ",
            ExpressionType.NotEqual => " <> ",
            ExpressionType.Or => " | ",
            ExpressionType.OrElse => " OR ",
            ExpressionType.Power => " ^ ",
            ExpressionType.RightShift => " >> ",
            ExpressionType.Subtract => " - ",
            ExpressionType.SubtractChecked => " - ",
            _ => throw new NotSupportedException($"The expression type '{expressionType}' is not supported.")
        };
    }
}