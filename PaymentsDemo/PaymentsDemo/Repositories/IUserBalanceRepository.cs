using PaymentsDemo.Models;

namespace PaymentsDemo.Repositories;

public interface IUserBalanceRepository
{
    Task<bool> UpdateBalanceAsync(Guid userId, decimal newBalance, int version);
    Task<UserBalance?> GetBalanceAsync(Guid userId);
    Task CreateUserBalanceAsync(Guid userId, decimal initialBalance);
}
