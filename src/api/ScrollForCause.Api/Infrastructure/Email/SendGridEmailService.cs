namespace ScrollForCause.Api.Infrastructure.Email;

public class SendGridEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task SendVerificationApprovedAsync(string toEmail, string organizationName)
    {
        var apiKey = _configuration["SendGrid:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning(
                "SendGrid API key not configured. Skipping verification approved email to {Email} for org {OrgName}.",
                toEmail, organizationName);
            return Task.CompletedTask;
        }

        _logger.LogInformation(
            "Sending verification approved email to {Email} for org {OrgName}.",
            toEmail, organizationName);

        // TODO: Implement actual SendGrid email sending when API key is configured
        return Task.CompletedTask;
    }

    public Task SendVerificationRejectedAsync(string toEmail, string organizationName, string reason)
    {
        var apiKey = _configuration["SendGrid:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning(
                "SendGrid API key not configured. Skipping verification rejected email to {Email} for org {OrgName}. Reason: {Reason}",
                toEmail, organizationName, reason);
            return Task.CompletedTask;
        }

        _logger.LogInformation(
            "Sending verification rejected email to {Email} for org {OrgName}. Reason: {Reason}",
            toEmail, organizationName, reason);

        // TODO: Implement actual SendGrid email sending when API key is configured
        return Task.CompletedTask;
    }
}
