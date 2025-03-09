namespace PaymentsDemo.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Secret { get; set; } = string.Empty;
    public int TokenLifetimeMinutes { get; set; } = 60;
}