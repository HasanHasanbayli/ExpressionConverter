using System.Linq.Expressions;
using ExpressionConverter;

Expression<Func<Persons, bool>> expression = c => c.Age >= 10 && c.Age <= 20;

(string query, List<object> queryParameters) =
    expression.Translate(page: 1, pageSize: 100, sortBy: "Age", sortOrder: "Desc");

await DbHelper.GetDataForDapper(query, queryParameters);

await DbHelper.GetDataForAdoNet(query, queryParameters);