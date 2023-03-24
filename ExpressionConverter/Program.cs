using System.Linq.Expressions;
using ExpressionConverter;


Expression<Func<Persons, bool>> expression = c => c.City.Contains("Baku") && c.Age > 20;

(string query, List<object> queryParameters) =
    expression.Translate(page: -11, pageSize: 100, sortBy: "Age", sortOrder: "desc");

await DbHelper.GetData(query, queryParameters);