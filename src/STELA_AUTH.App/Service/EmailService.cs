using System.Net;
using MailKit.Net.Smtp;
using MimeKit;
using STELA_AUTH.Core.IService;

namespace STELA_AUTH.App.Service
{
    public class EmailService : IEmailService
    {
        private readonly MailboxAddress _senderEmail;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderPassword;

        public EmailService(
            string senderName,
            string senderEmail,
            string smtpServer,
            int smtpPort,
            string senderPassword
        )
        {
            _senderEmail = new MailboxAddress(senderName, senderEmail);
            _senderPassword = senderPassword;
            _smtpPort = smtpPort;
            _smtpServer = smtpServer;
        }

        public async Task<HttpStatusCode> SendMessage(string recipientEmail, string subject, string message)
        {
            HttpStatusCode code = HttpStatusCode.OK;

            try
            {
                using var emailMessage = new MimeMessage();
                emailMessage.Subject = subject;
                emailMessage.From.Add(_senderEmail);
                emailMessage.To.Add(new MailboxAddress("", recipientEmail));
                emailMessage.Body = new TextPart()
                {
                    Text = message,
                };

                using var client = new SmtpClient();
                client.CheckCertificateRevocation = false;
                await client.ConnectAsync(_smtpServer, _smtpPort, false);
                await client.AuthenticateAsync(_senderEmail.Address, _senderPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                code = HttpStatusCode.NotFound;
            }

            return code;
        }


    }
}