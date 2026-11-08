namespace Application.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; set; } = "";
    public int Port { get; set; }
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromName { get; set; } = "Amazon";
}