using peakmotion.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace peakmotion.Data.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Response> SendSingleEmail(ComposeEmailModel payload)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("osk.watson@gmail.com",
                                        "Craig Watson");
            var to = new EmailAddress(payload.Email);

            var msg = MailHelper.CreateSingleEmail(from, to, payload.Subject,
                                                   payload.Body, payload.Body);

            return await client.SendEmailAsync(msg);
        }
    }
}