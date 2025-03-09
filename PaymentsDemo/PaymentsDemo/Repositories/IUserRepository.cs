using PaymentsDemo.Models;

namespace PaymentsDemo.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task CreateUserAsync(string username, string password);
}