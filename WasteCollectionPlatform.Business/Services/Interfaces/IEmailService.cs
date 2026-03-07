namespace WasteCollectionPlatform.Business.Services.Interfaces;

/// <summary>
/// Email service interface for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email asynchronously
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlMessage">HTML email body</param>
    Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
}
