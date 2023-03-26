using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace ExpressionConverter;

public class DbHelper
{
    private const string ConnectionString = "Server=localhost,1433; Database=TestDB; User=sa; Password=MyP@ssword;";

    public static async Task GetDataForAdoNet(string query, List<object> queryParameters)
    {
        await using SqlConnection connection = new(ConnectionString);

        try
        {
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    public static async Task GetDataForDapper(string query, List<object> queryParameters)
    {
        using IDbConnection connection = new SqlConnection(ConnectionString);

        try
        {
            connection.Open();

            DynamicParameters dynamicParameters = new();

            for (int i = 0; i < queryParameters.Count; i++)
            {
                dynamicParameters.Add($"@p{i}", queryParameters[i]);
            }

            List<Persons> persons = (await connection.QueryAsync<Persons>(query, dynamicParameters)).ToList();

            foreach (Persons person in persons)
            {
                Console.WriteLine(
                    $" Id {person.Id};" +
                    $" FirstName {person.FirstName};" +
                    $" LastName {person.LastName};" +
                    $" Age {person.Age};" +
                    $" City {person.City}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            connection.Close();
        }
    }
}