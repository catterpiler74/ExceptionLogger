using System;
using System.Net;
using System.Net.Mail;

namespace Utilities
{
  /// <summary>Sends error logs as emails</summary>
  public class EmailLogger : LoggerImplementation
  {
    /// <summary>Logs the specified error.</summary>
    /// <param name="error">The error to log.</param>
    public override void LogError(string error)
    {
      // check all properties have been set
      if (string.IsNullOrEmpty(EmailFrom))
        throw new ArgumentException("EmailFrom has not been set");
      if (string.IsNullOrEmpty(EmailTo))
        throw new ArgumentException("EmailTo has not been set");
      if (string.IsNullOrEmpty(EmailServer))
        throw new ArgumentException("EmailServer has not been set");

      var message = new MailMessage(EmailFrom, EmailTo, "Unhandled exception report", error);
      var client = new SmtpClient(EmailServer);
      // Add credentials if the SMTP server requires them.
      client.Credentials = CredentialCache.DefaultNetworkCredentials;
      client.Send(message);
    }

    /// <summary>
    /// Specifies the email server that the exception information email will be sent via
    /// </summary>
    public string EmailServer { get; set; }

    /// <summary>
    /// Specifies the email address that the exception information will be sent from
    /// </summary>
    public string EmailFrom { get; set; }

    /// <summary>
    /// Specifies the email address that the exception information will be sent to
    /// </summary>
    public string EmailTo { get; set; }
  }
}
