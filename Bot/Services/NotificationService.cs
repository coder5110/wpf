using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Services
{
    public static class NotificationService
    {
        #region Methods

        public static async Task<bool> SendNotification(string to, string message)
        {
            bool res = true;

            SmtpClient notificationClient = new SmtpClient()
            {
                Port = 25,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = m_emailHost,
                Credentials = new NetworkCredential(m_notificationEmail, m_notificationPassword)
            };

            MailMessage mailMessage = new MailMessage(m_notificationEmail, to);
            mailMessage.Subject = "Success!";
            mailMessage.Body = message;

            try
            {
                await notificationClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                res = false;
            }

            notificationClient.Dispose();

            return res;
        }

        public static async Task<bool> SendStatistic(string message, string subject)
        {
            bool res = true;

            SmtpClient statisticClient = new SmtpClient()
            {
                Port = 25,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = m_emailHost,
                Credentials = new NetworkCredential(m_statisticsEmail, m_statisticsPassword)
            };

            MailMessage mailMessage = new MailMessage(m_statisticsEmail, "statistic@projectdestroyer.com");
            mailMessage.Subject = subject;
            mailMessage.Body = message;

            try
            {
                await statisticClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                res = false;
            }

            statisticClient.Dispose();

            return res;
        }

        #endregion

        #region Fields

        private static string m_emailHost = "smtp.projectdestroyer.com";
        private static string m_notificationEmail = "notification@projectdestroyer.com";
        private static string m_notificationPassword = "NotificationPassword123!";
        private static string m_statisticsEmail = "statistic@projectdestroyer.com";
        private static string m_statisticsPassword = "StatisticPassword123!";

        #endregion
    }
}
