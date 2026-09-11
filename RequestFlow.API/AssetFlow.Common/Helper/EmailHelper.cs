using RequestFlow.Common.Settings;
using System.Net.Mail;

namespace RequestFlow.Common.Helper
{
    public class EmailHelper
    {
        private readonly EmailSettings emailSettings;

        public EmailHelper(EmailSettings emailSettings)
        {
            this.emailSettings = emailSettings;
        }

        public bool SendEmail(string subject, string userEmail, string link)
        {
            //using (var client = new AmazonSimpleEmailServiceClient(emailSettings.AccessKey, emailSettings.SecretAccessKey, RegionEndpoint.USEast1))
            //{
            //    var emailRequest = new SendEmailRequest()
            //    {
            //        Source = "ils.client.aws@gmail.com",
            //        Destination = new Destination(),
            //        Message = new Message()
            //    };

            //    emailRequest.Destination.ToAddresses.Add(userEmail);
            //    emailRequest.Message.Subject = new Content(subject);
            //    emailRequest.Message.Body = new Body { Html = new Content(link) };

            //    try
            //    {
            //        await client.SendEmailAsync(emailRequest);
            //        return true;
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new Exception(ex.Message);
            //    }
            //}


            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailSettings.UserEmail)
            };

            mailMessage.To.Add(new MailAddress(userEmail));
            mailMessage.Subject = subject;
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = link;

            var client = new SmtpClient
            {
                UseDefaultCredentials = false,
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential(emailSettings.UserEmail, emailSettings.Password),
                Host = emailSettings.Host,
                Port = emailSettings.Port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };

            try
            {
                client.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
