using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using PaymentsDemo.Models;
using PaymentsDemo.Options;

namespace PaymentsDemo.Repositories;

public class UserBalanceRepository(IOptions<DbOptions> dbOptions) : IUserBalanceRepository
{
    private const string SelectUserBalance =
        "SELECT UserId, Balance, Version FROM UserBalances WHERE UserId = @UserId";
    private const string AddUserBalance =
        "INSERT INTO UserBalances (UserId, Balance, Version) VALUES (@UserId, @Balance, @Version)";
    private const string UpdateUserBalance = """
        UPDATE UserBalances
        SET Balance = @NewBalance, Version = @Version + 1
        WHERE UserId = @UserId AND Version = @Version;

        SELECT @@ROWCOUNT
        """;
    
    private readonly string _connectionString = dbOptions.Value.ConnectionString;

    public async Task<UserBalance?> GetBalanceAsync(Guid userId)
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<UserBalance>(SelectUserBalance, new { UserId = userId });
    }

    public async Task CreateUserBalanceAsync(Guid userId, decimal initialBalance)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(AddUserBalance, new { UserId = userId, Balance = initialBalance, Version = 1 });
    }

    public async Task<bool> UpdateBalanceAsync(Guid userId, decimal newBalance, int version)
    {
        await using var connection = new SqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            UpdateUserBalance, 
            new
                {
                    UserId = userId,
                    NewBalance = newBalance,
                    Version = version
                });

        return rowsAffected == 1;
    }
}