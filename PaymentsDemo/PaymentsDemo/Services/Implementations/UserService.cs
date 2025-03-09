using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using PaymentsDemo.Models;
using PaymentsDemo.Options;

namespace PaymentsDemo.Services;

public class UserService(IOptions<DbOptions> dbOptions) : IUserService
{
    private const string SelectUser = "SELECT Id, Username, PasswordHash FROM Users WHERE Username = @Username";
    private const string InsertUser = "INSERT INTO Users (Id, Username, PasswordHash) VALUES (@Id, @Username, @PasswordHash)";

    private readonly string _connectionString = dbOptions.Value.ConnectionString;

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        const string sql = SelectUser;
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<User>(SelectUser, new {Username = username});
    }

    public async Task CreateUserAsync(string username, string password)
    {
        await using var connection = new SqlConnection(_connectionString);

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = hashedPassword
        };

        await connection.ExecuteAsync(InsertUser, newUser);
        
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, storedHash);
    }
}