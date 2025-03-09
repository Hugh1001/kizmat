namespace PaymentsDemo.Options;

public class DbOptions
{
    public const string SectionName = "Database";
    public string ConnectionString { get; set; } = string.Empty;
}