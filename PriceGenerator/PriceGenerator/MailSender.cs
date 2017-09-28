using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace PriceGenerator
{
    public class MailSender
    {
        private readonly string FromAddress;
        private readonly string ToAddress;
        private readonly string FromPassword;
        private readonly string Subject;

        public MailSender(string fromAddress, string toAddress, string fromPassword, string subject)
        {
            FromAddress = fromAddress;
            ToAddress = toAddress;
            FromPassword = fromPassword;
            Subject = subject;
        }

        public MailSender()
        : this("descrr@gmail.com", "descrr@gmail.com", "gva212gva212!", "Price V+")
        {
        }
        public void SendMail(string body, List<string> attachFiles)
        {
            var smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(FromAddress, FromPassword)
            };

            using (var message = new MailMessage(FromAddress, ToAddress, Subject, body))
            {
                foreach (var file in attachFiles)
                {
                    Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);
                    
                    // Add time stamp information for the file.
                    ContentDisposition disposition = data.ContentDisposition;
                    disposition.CreationDate = System.IO.File.GetCreationTime(file);
                    disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
                    disposition.ReadDate = System.IO.File.GetLastAccessTime(file);
                    
                    // Add the file attachment to this e-mail message.
                    message.Attachments.Add(data);
                }

                smtpClient.Send(message);
            }
        }
    }
}
