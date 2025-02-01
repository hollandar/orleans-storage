namespace Webefinity.Module.Messaging.Abstractions;

public class SmtpOptions
{
    public string? Host { get; set; } = null;
    public int Port { get; set; } = 432;
    public bool RequiresAuthentication { get; set; } = true;
    public string? Username { get; set; } = null;
    public string? Password { get; set; } = null;
    public bool UseSsl { get; set; } = true;
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public interface ISmtpOptionsProvider
{
    SmtpOptions GetSmtpOptions();
}
