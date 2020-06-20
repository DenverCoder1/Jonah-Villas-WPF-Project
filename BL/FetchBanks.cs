using BE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace BL
{
    public class FetchBanks
    {
        /// <summary>
        /// Fetch bank branches in the background
        /// </summary>
        public static void GetBankBranches(RunWorkerCompletedEventHandler RunWorkerCompleted = null)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Send an email and set background worker result
        /// </summary>
        public static void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // create a variable for storing the xml
            string xml = "";

            // create new web client
            WebClient webClient = new WebClient();

            try
            {
                // get data from Bank of Israel website
                string xmlServerPath = @"https://www.boi.org.il/en/BankingSupervision/BanksAndBranchLocations/Lists/BoiBankBranchesDocs/snifim_en.xml";
                xml = webClient.DownloadString(xmlServerPath);

            }
            catch (Exception)
            {
                // get data from secondary source if first fails
                // data was backed up to a server on 20.06.2020
                string xmlServerPath = @"http://lev.net/jonah/villas/snifim_en.xml";
                xml = webClient.DownloadString(xmlServerPath);
            }
            finally
            {
                // clean up web client
                webClient.Dispose();
                // keep result for use in completion function
                e.Result = xml;
            }
        }
    }
}
