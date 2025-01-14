using peakmotion.Models;
using SendGrid;

namespace peakmotion.Data.Services
{
    public interface IEmailService
    {
        Task<Response> SendSingleEmail(ComposeEmailModel payload);
    }
}