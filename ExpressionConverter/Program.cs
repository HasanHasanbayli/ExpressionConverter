using System.Linq.Expressions;
using ExpressionConverter;

Expression<Func<Person, bool>> expression = p =>
    (p.Id != 1 && p.Age >= 20) ||
    (p.Age <= 30 && p.City == "Baku");

string query = SqlTranslator.Translate(expression);

Console.WriteLine(query);