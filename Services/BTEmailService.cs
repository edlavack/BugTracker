using BugTracker.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;


namespace BugTracker.Services
{
    public class BTEmailService : IEmailSender 
    {
        private readonly MailSettings _mailSettings;

        public BTEmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string userEmail, string subject, string htmlMessage)
        {
            var configEmail = _mailSettings.EmailAddress ?? Environment.GetEnvironmentVariable("EmailAddress");
            string? password = _mailSettings.EmailPassword ?? Environment.GetEnvironmentVariable("EmailPassword");
            var port = _mailSettings.EmailPort != 0 ? _mailSettings.EmailPort : int.Parse(Environment.GetEnvironmentVariable("EmailPort")!);
            var host = _mailSettings.EmailHost ?? Environment.GetEnvironmentVariable("EmailHost");

            MimeMessage newEmail = new();


            //add all email addresses to the "TO" for the email
            newEmail.Sender = MailboxAddress.Parse(configEmail);
            newEmail.To.Add(MailboxAddress.Parse(userEmail));

            //add the subject for the email
            newEmail.Subject = subject;


            //add the body for the email
            var builder = new BodyBuilder { HtmlBody = htmlMessage };
            newEmail.Body = builder.ToMessageBody();

            //send the email
            try
            {
                
                using SmtpClient smtpClient = new();
                
                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);

                await smtpClient.AuthenticateAsync(configEmail, password);
                await smtpClient.SendAsync(newEmail);
                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }

    }
}
