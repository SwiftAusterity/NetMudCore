using NetMudCore.DataStructure.System;
using System.Net.Mail;

namespace NetMudCore.Authentication
{
    public static class EmailUtility
    {
        public static void SendEmail(IGlobalConfig globalConfig, ApplicationUser user, string subject, string body)
        {
            SendEmail(globalConfig, user.Email ?? string.Empty, subject, body);
        }
        internal static void SendEmail(IGlobalConfig globalConfig, string email, string subject, string body)
        {
            MailMessage mm = new();
            mm.To.Add(email);
            mm.Subject = subject;
            mm.Body = body;
            mm.IsBodyHtml = false;
            mm.From = new MailAddress(globalConfig.SystemEmail);

            SmtpClient smtp = new("smtp.gmail.com")
            {
                Port = 587,
                UseDefaultCredentials = false,
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential(globalConfig.SystemEmail, globalConfig.SystemMailPassword)
            };

            smtp.Send(mm);
        }
    }
}
