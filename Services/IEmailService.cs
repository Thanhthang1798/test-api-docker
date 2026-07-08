namespace DemoDockerAPI2.Services;

public interface IEmailService
{
    Task SendMessage(string to, string subject, string body);
}