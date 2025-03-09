using FluentResults;

namespace PaymentsDemo.Services;

public interface IPaymentsService
{
    public Task<Result> MakePaymentAsync(Guid userId);
}