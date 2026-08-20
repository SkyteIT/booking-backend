namespace Ube.Application.Common.Models;
public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool EnableSsl { get; set; } = true;
    public string ClientBaseUrl { get; set; } = "http://localhost:3000";
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
