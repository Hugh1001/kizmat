using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using PaymentsDemo.Models;
using PaymentsDemo.Options;

namespace PaymentsDemo.Services;

public class BalanceService(IOptions<DbOptions> dbOptions) : IBalanceService
{
    private const string SelectUserBalance = "SELECT UserId, Amount, Version FROM UserBalances WHERE UserId = @UserId";
    private const string AddUserBalance = "INSERT INTO UserBalances (UserId, Amount, Version) VALUES (@UserId, @Amount, @Version)";
    private const string UpdateUserBalance = $@"
        UPDATE UserBalances
        SET Amount = Amount + @DeductionAmount, Version = Version + 1
        WHERE UserId = @UserId AND Amount + @DeductionAmount >= 0;

        SELECT @@ROWCOUNT";
    
    private readonly string _connectionString = dbOptions.Value.ConnectionString;

    public async Task<UserBalance?> GetBalanceAsync(Guid userId)
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<UserBalance>(SelectUserBalance, new { UserId = userId });
    }

    public async Task CreateUserBalanceAsync(Guid userId, decimal initialBalance)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(AddUserBalance, new { UserId = userId, Amount = initialBalance, Version = 1 });
    }

    public async Task<bool> UpdateBalanceAsync(Guid userId, decimal amount)
    {
        await using var connection = new SqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteScalarAsync<int>(UpdateUserBalance, new { UserId = userId, DeductionAmount = amount });

        return rowsAffected == 1;
    }
}