using PaymentsDemo.Models;

namespace PaymentsDemo.Services;

public interface IBalanceService
{
    Task<bool> UpdateBalanceAsync(Guid userId, decimal amount);
    Task<UserBalance?> GetBalanceAsync(Guid userId);
    Task CreateUserBalanceAsync(Guid userId, decimal initialBalance);
}
