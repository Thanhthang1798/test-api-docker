using DemoDockerAPI2.Options;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text.Json;

namespace DemoDockerAPI2.Services;

public class EMailService : IEmailService
{

    private readonly EmailOptions _options;
    public EMailService(IOptions<DeliveryOptions> options)
    {
        _options = options.Value.Email;
    }

    public async Task SendMessage(
    string to,
    string subject,
    string body)
    {
        var message = new MimeMessage();
        Console.WriteLine($"Sending email to: {to}, subject: {subject}, body: {body}");
        Console.WriteLine(JsonSerializer.Serialize(_options));
        message.From.Add(
            MailboxAddress.Parse(_options.From));

        message.To.Add(
            MailboxAddress.Parse(to));

        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = body,
            TextBody = body
        }.ToMessageBody();

        using var client = new SmtpClient();

        await client.ConnectAsync(
            _options.SmtpHost,
            _options.SmtpPort,
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            _options.Username,
            ResolvePassword());

        await client.SendAsync(message);

        await client.DisconnectAsync(true);
    }

    private string ResolvePassword()
    {
        if (!string.IsNullOrWhiteSpace(_options.AppPassword))
        {
            return _options.AppPassword;
        }

        if (!string.IsNullOrWhiteSpace(_options.Password2))
        {
            return _options.Password2;
        }

        return _options.Password;
    }
}