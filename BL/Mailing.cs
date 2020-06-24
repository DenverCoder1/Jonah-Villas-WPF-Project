using BE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Reflection;
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
        public static void StartEmailBackgroundWorker(Order order, RunWorkerCompletedEventHandler RunWorkerCompleted, int delay = 0)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += RunWorkerCompleted;
            worker.RunWorkerAsync(new List<object>{ order, delay });
        }

        /// <summary>
        /// Generate subject line
        /// </summary>
        /// <param name="owner">Owner who created offer</param>
        /// <returns>Subject line as string</returns>
        private static string MakeSubject(Host owner)
        {
            return $"Jonah's Rentals : You have a new offer from {owner.FirstName}!";
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
            body.AppendLine($"<h2>Jonah's Rentals</h2>");
            body.AppendLine($"<p>Hi {request.FirstName},</p>");
            body.Append($"<p><span style='font-weight:bold;'>{owner.FirstName} {owner.LastName}</span> has an offer for you ");
            string unitCity = Regex.Replace(hostingUnit.UnitCity.ToString(), "([a-z])([A-Z])", "$1 $2");
            string unitDistrict = Regex.Replace(hostingUnit.UnitDistrict.ToString(), "([a-z])([A-Z])", "$1 $2");
            body.Append($"in <span style='font-weight:bold;'>{unitCity}, {unitDistrict}</span> ");
            body.Append($"from <span style='font-weight:bold;'>{request.EntryDate:dd.MM.yyyy} through {request.ReleaseDate:dd.MM.yyyy}</span>.</p>");
            body.AppendLine($"<p>{owner.FirstName} can be contacted by email at <span style='font-weight:bold;'>{owner.Email}</span> or by phone at <span style='font-weight:bold;'>{owner.PhoneNumber}</span>.</p>");
            body.AppendLine($"<p>Have a great day!<br/>");
            body.AppendLine($"- Jonah from Jonah's Rentals</p>");
            return body.ToString();
        }

        /// <summary>
        /// Send email (called from within the DoWork method)
        /// </summary>
        /// <param name="e">worker arguments</param>
        private static object SendEmail(Order order)
        {
            IBL Bl = FactoryBL.GetBL();

            // Get order details
            GuestRequest request = Bl.GetGuestRequest(order.GuestRequestKey);
            HostingUnit hostingUnit = Bl.GetHostingUnit(order.HostingUnitKey);
            Host owner = Bl.GetHost(hostingUnit.OwnerHostID);

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
                return error;
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
                client.Send(mail);
            }
            catch (Exception error)
            {
                return error;
            }
            return true;
        }

        /// <summary>
        /// Send an email and set background worker result
        /// </summary>
        public static void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // read arguments
            Order order = (Order)((List<object>)e.Argument)[0];
            int delay = (int)((List<object>)e.Argument)[1];
            // delay (only when retrying)
            Thread.Sleep(delay);
            // send email and get back true if successful, otherwise error.
            // return the order and the result
            e.Result = new List<object>
            {
                order, // original order
                SendEmail(order) // get result from sending email
            };
        }
    }
}
