using FluentResults;
using PaymentsDemo.Repositories;

namespace PaymentsDemo.Services;

public class PaymentsService(IUserBalanceRepository userBalanceRepository) : IPaymentsService
{
    const decimal PaymentAmount = 1.0m;

    public async Task<Result> MakePaymentAsync(Guid userId)
    {
        var userBalance = await userBalanceRepository.GetBalanceAsync(userId);
        if (userBalance is null)
        {
            await userBalanceRepository.CreateUserBalanceAsync(userId, 8);
        }

        var newBalance = userBalance.Balance - PaymentAmount;
        if (newBalance < 0)
        {
            return Result.Fail("Insufficient balance");
        }

        var success = await userBalanceRepository.UpdateBalanceAsync(userId, newBalance, userBalance.Version);

        return success ? Result.Ok() : Result.Fail("Concurrent update conflict occurred.");
    }
}