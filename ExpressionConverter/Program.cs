﻿using System.Data.SqlClient;
using System.Linq.Expressions;
using ExpressionConverter;

const string connectionString = "Server=localhost,1433; Database=TestDB; User=sa; Password=MyP@ssword;";

Expression<Func<Persons, bool>> expression = c => c.City.Contains("Baku") && c.Age > 15 && c.Age < 20;

(string query, List<object> queryParameters) = expression.Translate(page: 1, pageSize: 10, sortBy: "Age", sortOrder: "desc");

await using SqlConnection connection = new(connectionString);

await connection.OpenAsync();

await using SqlCommand command = new(query, connection);

for (int i = 0; i < queryParameters.Count; i++)
{
    command.Parameters.Add(new SqlParameter($"@p{i}", queryParameters[i]));
}

await using SqlDataReader reader = await command.ExecuteReaderAsync();

while (await reader.ReadAsync())
    Console.WriteLine(
        $" Id {reader[0]};" +
        $" FirstName {reader[1]};" +
        $" LastName {reader[2]};" +
        $" Age {reader[3]}; " +
        $" City {reader[4]}");