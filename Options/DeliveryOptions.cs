namespace DemoDockerAPI2.Options;

public sealed class DeliveryOptions
{
    public EmailOptions Email { get; set; } = new();

    public SlackOptions Slack { get; set; } = new();
}


public sealed class EmailOptions
{
    public bool Enabled { get; set; }

    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    public bool UseStartTls { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string AppPassword { get; set; } = string.Empty;

    public string Password2 { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public List<string> To { get; set; } = [];
}

public sealed class SlackOptions
{
    public bool Enabled { get; set; }

    public string WebhookUrl { get; set; } = string.Empty;
}
