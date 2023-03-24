using System.Data.SqlClient;

namespace ExpressionConverter;

public class DbHelper
{
    private const string ConnectionString = "Server=localhost,1433; Database=TestDB; User=sa; Password=MyP@ssword;";

    public static async Task GetData(string query, List<object> queryParameters)
    {
        await using SqlConnection connection = new(ConnectionString);

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
                $" Age {reader[3]};" +
                $" City {reader[4]}");

        await connection.CloseAsync();
    }
}