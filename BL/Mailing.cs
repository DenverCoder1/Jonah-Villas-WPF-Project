using BE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BL
{
    public static class Mailing
    {
        /// <summary>
        /// Send an email in the background
        /// </summary>
        /// <param name="order">Order details</param>
        public static void StartEmailBackgroundWorker(Order order, RunWorkerCompletedEventHandler RunWorkerCompleted = null)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += RunWorkerCompleted;
            worker.RunWorkerAsync(order);
        }

        /// <summary>
        /// Generate subject line
        /// </summary>
        /// <param name="owner">Owner who created offer</param>
        /// <returns>Subject line as string</returns>
        private static string MakeSubject(Host owner)
        {
            return $"Jonah's Villas : You have a new offer from {owner.FirstName}!";
        }

        /// <summary>
        /// Genrate HTML email body
        /// </summary>
        /// <param name="owner">Owner who created offer</param>
        /// <param name="hostingUnit">Hosting unit offered</param>
        /// <param name="request">Request from guest</param>
        /// <returns>HTML body of email as string</returns>
        private static string MakeBody(Host owner, HostingUnit hostingUnit, GuestRequest request)
        {
            StringBuilder body = new StringBuilder();
            body.AppendLine($"<h2>Jonah's Villas</h2>");
            body.AppendLine($"<p>Hi {request.FirstName},</p>");
            body.Append($"<p><span style='font-weight:bold;'>{owner.FirstName} {owner.LastName}</span> has an offer for you ");
            string unitCity = Regex.Replace(hostingUnit.UnitCity.ToString(), "([a-z])([A-Z])", "$1 $2");
            string unitDistrict = Regex.Replace(hostingUnit.UnitDistrict.ToString(), "([a-z])([A-Z])", "$1 $2");
            body.Append($"in <span style='font-weight:bold;'>{unitCity}, {unitDistrict}</span> ");
            body.Append($"from <span style='font-weight:bold;'>{request.EntryDate:dd.MM.yyyy} through {request.ReleaseDate:dd.MM.yyyy}</span>.</p>");
            body.AppendLine($"<p>{owner.FirstName} can be contacted by email at <span style='font-weight:bold;'>{owner.Email}</span> or by phone at <span style='font-weight:bold;'>{owner.PhoneNumber}</span>.</p>");
            body.AppendLine($"<p>Have a great day!<br/>");
            body.AppendLine($"- Jonah from Jonah's Villas</p>");
            return body.ToString();
        }

        /// <summary>
        /// Send email (called from within the DoWork method)
        /// </summary>
        /// <param name="e">worker arguments</param>
        private static object SendEmail(DoWorkEventArgs e)
        {
            IBL Bl = FactoryBL.GetBL();

            // Get order details
            Order order = (Order)e.Argument;
            GuestRequest request = Bl.GetGuestRequest(order.GuestRequestKey);
            HostingUnit hostingUnit = Bl.GetHostingUnit(order.HostingUnitKey);
            Host owner = Bl.GetHost(hostingUnit.Owner.HostKey);

            // Get details from configuration file
            Configuration oConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            MailSettingsSectionGroup mailSettings = oConfig.GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup;

            int port = mailSettings.Smtp.Network.Port;
            string from = mailSettings.Smtp.From;
            string netHost = mailSettings.Smtp.Network.Host;
            string pwd = mailSettings.Smtp.Network.Password;
            string uid = mailSettings.Smtp.Network.UserName;

            try
            {
                // Check that all entities exist
                if (order == null)
                    throw new ArgumentException("Email failed to send. Order cannot be null.");
                if (request == null)
                    throw new ArgumentException("Email failed to send. Guest request was not found.");
                if (hostingUnit == null)
                    throw new ArgumentException("Email failed to send. Hosting unit was not found.");
                if (owner == null)
                    throw new ArgumentException("Email failed to send. Host was not found.");
                if (mailSettings == null)
                    throw new Exception("Error with mail configuration.");
            }
            catch (Exception error)
            {
                e.Result = error;
                return e.Result;
            }

            // MailMessage Create an object
            MailMessage mail = new MailMessage();

            // address of recipient (more than one can be added)
            mail.To.Add(request.Email);

            // The address from which the email was sent
            mail.From = new MailAddress(@from);

            // Message Subject

            mail.Subject = MakeSubject(owner);

            // HTML Message content
            mail.Body = MakeBody(owner, hostingUnit, request);

            // Define format as HTML
            mail.IsBodyHtml = true;

            // create SmtpClient
            SmtpClient client = new SmtpClient
            {
                Host = netHost,
                Port = port,
                Credentials = new NetworkCredential(uid, pwd),
                EnableSsl = true
            };
            try
            {
                // Send message
                //client.Send(mail);
            }
            catch (Exception error)
            {
                e.Result = error;
                return e.Result;
            }
            e.Result = true;
            return e.Result;
        }

        /// <summary>
        /// Send an email and set background worker result
        /// </summary>
        public static void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                object result = SendEmail(e);
                if (result is Exception)
                {
                    // sleep 2 seconds and try again
                    Thread.Sleep(2000);
                }
                else
                {
                    // exit loop and end thread
                    break;
                }
            }
        }
    }
}
