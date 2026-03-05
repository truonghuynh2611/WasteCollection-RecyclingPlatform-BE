namespace WasteCollectionPlatform.Common.Constants;

/// <summary>
/// Email configuration settings from appsettings.json
/// </summary>
public class EmailSettings
{
    public string SmtpServer { get; set; } = null!;
    public int Port { get; set; }
    public string SenderEmail { get; set; } = null!;
    public string AppPassword { get; set; } = null!;
    public string SenderName { get; set; } = null!;
}
