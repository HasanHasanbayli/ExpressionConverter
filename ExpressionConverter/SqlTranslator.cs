using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ExpressionConverter;

public static class SqlTranslator
{
    private static readonly Dictionary<ExpressionType, string> SqlOperators = new()
    {
        [ExpressionType.Add] = " + ",
        [ExpressionType.AddChecked] = " + ",
        [ExpressionType.And] = " & ",
        [ExpressionType.AndAlso] = " AND ",
        [ExpressionType.Coalesce] = " COALESCE ",
        [ExpressionType.Divide] = " / ",
        [ExpressionType.Equal] = " = ",
        [ExpressionType.ExclusiveOr] = " ^ ",
        [ExpressionType.GreaterThan] = " > ",
        [ExpressionType.GreaterThanOrEqual] = " >= ",
        [ExpressionType.LeftShift] = " << ",
        [ExpressionType.LessThan] = " < ",
        [ExpressionType.LessThanOrEqual] = " <= ",
        [ExpressionType.Modulo] = " % ",
        [ExpressionType.Multiply] = " * ",
        [ExpressionType.MultiplyChecked] = " * ",
        [ExpressionType.NotEqual] = " <> ",
        [ExpressionType.Or] = " | ",
        [ExpressionType.OrElse] = " OR ",
        [ExpressionType.Power] = " ^ ",
        [ExpressionType.RightShift] = " >> ",
        [ExpressionType.Subtract] = " - ",
        [ExpressionType.SubtractChecked] = " - "
    };

    public static (string, List<object>) Translate<T>(
        this Expression<Func<T, bool>> predicate,
        int page = 1,
        int pageSize = 10,
        string sortBy = "Id",
        string sortOrder = "asc")
    {
        StringBuilder sb = new("SELECT * FROM ");
        sb.Append(typeof(T).Name);
        sb.Append(" WHERE ");

        List<object> parameters = new();
        Visit(predicate.Body, sb, parameters);

        sb.Append($" ORDER BY {sortBy} {sortOrder.ToUpper()} ");
        sb.Append($" OFFSET {(page - 1) * pageSize} ROWS ");
        sb.Append($" FETCH NEXT {pageSize} ROWS ONLY ");

        return (sb.ToString(), parameters);
    }


    private static void Visit(Expression expression, StringBuilder sb, List<object> parameters)
    {
        while (true)
        {
            switch (expression)
            {
                case BinaryExpression binaryExpression:
                    VisitBinary(binaryExpression, sb, parameters);
                    break;
                case ConstantExpression constantExpression:
                    VisitConstant(constantExpression, sb, parameters);
                    break;
                case MemberExpression memberExpression:
                    VisitMember(memberExpression, sb);
                    break;
                case UnaryExpression unaryExpression:
                    expression = unaryExpression.Operand;
                    continue;
                case MethodCallExpression methodCallExpression:
                    VisitMethodCall(methodCallExpression, sb, parameters);
                    break;
                default:
                    throw new NotSupportedException($"The expression type '{expression.GetType()}' is not supported.");
            }

            break;
        }
    }

    private static void VisitMethodCall(MethodCallExpression expression, StringBuilder sb, List<object> parameters)
    {
        if (expression.Method.Name != "Contains" || expression.Object?.Type != typeof(string))
            throw new NotSupportedException($"The method '{expression.Method.Name}' is not supported.");
      
        sb.Append('(');
        
        Visit(expression.Object, sb, parameters);
        
        sb.Append(" LIKE CONCAT('%',");
        
        Visit(expression.Arguments[0], sb, parameters);
        
        sb.Append(",'%'))");
    }

    private static void VisitBinary(BinaryExpression expression, StringBuilder sb, List<object> parameters)
    {
        sb.Append('(');

        Visit(expression.Left, sb, parameters);

        sb.Append(GetSqlOperator(expression.NodeType));

        Visit(expression.Right, sb, parameters);

        sb.Append(')');
    }

    private static void VisitConstant(ConstantExpression expression, StringBuilder sb, List<object> parameters)
    {
        string parameterName = $"@p{parameters.Count}";

        sb.Append(parameterName);

        parameters.Add(expression.Value!);
    }

    private static void VisitMember(MemberExpression expression, StringBuilder sb)
    {
        sb.Append(GetColumnName(expression));
    }

    private static string GetColumnName(MemberExpression expression)
    {
        if (expression.Member is not PropertyInfo propertyInfo)
            throw new NotSupportedException("Only property members are supported for column names.");

        return propertyInfo.Name;
    }

    private static string GetSqlOperator(ExpressionType expressionType)
    {
        if (SqlOperators.TryGetValue(expressionType, out string? sqlOperator)) return sqlOperator;

        throw new NotSupportedException($"The expression type '{expressionType}' is not supported.");
    }
}