namespace PaymentsDemo.Services;

public interface IJwtService
{
    string GenerateToken(Guid userId, string username);
}