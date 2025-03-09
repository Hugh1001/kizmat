using FluentResults;

namespace PaymentsDemo.Services;

public interface ILoginService
{
    public Task<Result<string>> Login(string username, string password);
    
    public Task<Result> RegisterUser(string username, string password);
}