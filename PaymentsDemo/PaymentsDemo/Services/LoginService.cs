using FluentResults;
using Microsoft.Extensions.Caching.Memory;
using PaymentsDemo.Repositories;

namespace PaymentsDemo.Services;

public class LoginService(IUserRepository userRepository, IJwtService jwtService, IMemoryCache cache) : ILoginService
{
    public async Task<Result<string>> Login(string username, string password)
    {
        var user = await userRepository.GetUserByUsernameAsync(username);
        var hash = GenerateHash(password);
        if (user is null || !Verify(password, user.PasswordHash))
        {
            return Result.Fail("Invalid username or password");
        }

        return Result.Ok(jwtService.GenerateToken(user.Id, user.Username));
    }

    public async Task<Result> RegisterUser(string username, string password)
    {
        var userExists = await userRepository.GetUserByUsernameAsync(username);

        if (userExists is not null)
        {
            return Result.Fail("User already exists");
        }

        await userRepository.CreateUserAsync(username, GenerateHash(password));

        return Result.Ok();
    }

    static bool Verify(string password, string passwordHash)
        => BCrypt.Net.BCrypt.Verify(password, passwordHash);

    static string GenerateHash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);
}