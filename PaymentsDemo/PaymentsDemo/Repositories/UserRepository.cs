using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using PaymentsDemo.Models;
using PaymentsDemo.Options;

namespace PaymentsDemo.Repositories;

public class UserRepository(IOptions<DbOptions> dbOptions) : IUserRepository
{
    const string SelectUser =
        "SELECT Id, Username, PasswordHash FROM Users WHERE Username = @Username";
    const string InsertUser =
        "INSERT INTO Users (Id, Username, PasswordHash) VALUES (@Id, @Username, @PasswordHash)";

    readonly string _connectionString = dbOptions.Value.ConnectionString;

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<User>(
            SelectUser,
            new { Username = username });
    }

    public async Task CreateUserAsync(string username, string passwordHash)
    {
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = passwordHash
        };

        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(InsertUser, newUser);
    }
}