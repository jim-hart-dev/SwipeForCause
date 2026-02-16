namespace SwipeForCause.Api.Infrastructure.Email;

public interface IEmailService
{
    Task SendVerificationApprovedAsync(string toEmail, string organizationName);
    Task SendVerificationRejectedAsync(string toEmail, string organizationName, string reason);
}
