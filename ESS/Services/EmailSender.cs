using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;

namespace ESS.Services
{
    public class EmailSender : IEmailSender
    {
        string Username;
        string Password;
        string MailFrom;
        string ReplyTo;
        string Host;
        int Port;
        bool EnableSsl;

        private readonly ILogger<EmailSender> _logger;
        private readonly IConfiguration _configuration;

        public EmailSender(ILogger<EmailSender> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            this.MailFrom = _configuration["Email:From"];
            this.Username = _configuration["Email:Username"];
            this.Password = _configuration["Email:Password"];
            this.ReplyTo = _configuration["Email:ReplyTo"];
            this.Host = _configuration["Email:Host"];
            this.Port = Convert.ToInt32(_configuration["Email:Port"]);
            this.EnableSsl = Convert.ToBoolean(_configuration["Email:EnableSsl"]);
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.Subject = subject;
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = htmlMessage;
                mailMessage.From = new MailAddress(MailFrom);
                mailMessage.ReplyTo = new MailAddress(ReplyTo);
                mailMessage.To.Add(email);
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = Host;
                smtpClient.EnableSsl = EnableSsl;
                smtpClient.Port = Port;
                smtpClient.Credentials = new System.Net.NetworkCredential(Username, Password);
                smtpClient.Send(mailMessage);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Task.CompletedTask;
            }
        }
    }
}
