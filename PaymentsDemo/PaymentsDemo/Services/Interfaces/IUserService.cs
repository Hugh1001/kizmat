using PaymentsDemo.Models;

namespace PaymentsDemo.Services;

public interface IUserService
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task CreateUserAsync(string username, string password);
    bool VerifyPassword(string password, string storedHash);
}