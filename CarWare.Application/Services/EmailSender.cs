using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CarWare.Application.Interfaces;

namespace CarWare.Application.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("abdoabdel3ziz188@gmail.com", "aobd rdkm lqap manh"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("abdoabdel3ziz188@gmail.com", "CarWare Support"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
