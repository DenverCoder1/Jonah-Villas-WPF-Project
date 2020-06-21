using BE;
using DS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DAL
{
    public class DAL_XML_Imp : IDAL
    {

        #region Private Fields

        // Singleton instance
        private static IDAL instance = null;

        // XML configuration
        private readonly XElement ConfigXElement;

        private readonly string GuestRequestPath = @"guestRequests.xml";
        private readonly string HostingUnitPath = @"hostingUnits.xml";
        private readonly string OrderPath = @"orders.xml";
        private readonly string HostPath = @"host.xml";
        private readonly string ConfigPath = @"config.xml";

        #endregion

        #region Singleton method

        // Get instance
        public static IDAL GetDAL()
        {
            if (instance == null)
                instance = new DAL_XML_Imp();
            return instance;
        }

        #endregion

        #region Constructor

        private DAL_XML_Imp()
        {
            // load or create guest requests file
            DataSource.GuestRequests = LoadList<GuestRequest>(GuestRequestPath);

            // load or create hosting units file
            DataSource.HostingUnits = LoadList<HostingUnit>(HostingUnitPath);

            // load or create orders file
            DataSource.Orders = LoadList<Order>(OrderPath);

            // load or create hosts file
            DataSource.Hosts = LoadList<Host>(HostPath);

            // check for config files
            if (!File.Exists(ConfigPath))
            {
                ConfigXElement = new XElement("CONFIG");
                ConfigXElement.Add(new XElement("INITIAL_ORDER_KEY", Config.INITIAL_ORDER_KEY));
                ConfigXElement.Add(new XElement("INITIAL_HOST_KEY", Config.INITIAL_HOST_KEY));
                ConfigXElement.Add(new XElement("INITIAL_HOSTING_UNIT_KEY", Config.INITIAL_HOSTING_UNIT_KEY));
                ConfigXElement.Add(new XElement("INITIAL_GUEST_REQUEST_KEY", Config.INITIAL_GUEST_REQUEST_KEY));
                ConfigXElement.Save(ConfigPath);
            }
            else
            {
                ConfigXElement = XElement.Load(ConfigPath);
                long.TryParse(ConfigXElement.Element("INITIAL_ORDER_KEY").Value, out Config.INITIAL_ORDER_KEY);
                long.TryParse(ConfigXElement.Element("INITIAL_HOST_KEY").Value, out Config.INITIAL_HOST_KEY);
                long.TryParse(ConfigXElement.Element("INITIAL_HOSTING_UNIT_KEY").Value, out Config.INITIAL_HOSTING_UNIT_KEY);
                long.TryParse(ConfigXElement.Element("INITIAL_GUEST_REQUEST_KEY").Value, out Config.INITIAL_GUEST_REQUEST_KEY);
            }

            SaveList(DataSource.GuestRequests, GuestRequestPath);
            SaveList(DataSource.HostingUnits, HostingUnitPath);
            SaveList(DataSource.Orders, OrderPath);
            SaveList(DataSource.Hosts, HostPath);
        }

        #endregion

        #region XML Read and Write

        /// <summary>
        /// Load list from xml file
        /// </summary>
        /// <typeparam name="T">entity type of list</typeparam>
        /// <param name="xmlPath">path of xml file</param>
        /// <returns>List of entities from file</returns>
        private static List<T> LoadList<T>(string xmlPath)
        {
            FileStream file = new FileStream(xmlPath, FileMode.OpenOrCreate);
            List<T> result = new List<T>();
            if (!File.Exists(xmlPath))
            {
                // create document
                XDocument newdoc = new XDocument();
                // add root
                newdoc.Add(new XElement(typeof(T).ToString() + "s"));
                // save
                newdoc.Save(xmlPath);
            }
            else
            {
                try
                {
                    // deserialize xml and get result as list
                    XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
                    result = (List<T>)serializer.Deserialize(file);
                    file.Close();
                }
                catch (Exception error)
                {
                    file.Close();
                    throw error;
                }
            }
            return result;
        }

        /// <summary>
        /// Save list in xml file
        /// <typeparam name="T">entity type of list</typeparam>
        /// <param name="list">list to save</param>
        /// <param name="xmlPath">path of xml file</param>
        private static void SaveList<T>(List<T> list, string xmlPath)
        {
            FileStream file = new FileStream(xmlPath, FileMode.Create);
            (new XmlSerializer(typeof(List<T>))).Serialize(file, list);
            file.Close();
        }

        #endregion

        #region GUEST REQUEST METHODS

        /// <summary>
        /// Place a new Guest Request into the data
        /// </summary>
        /// <param name="guestRequest">Guest request to add</param>
        /// <returns>True if successful</returns>
        bool IDAL.CreateGuestRequest(GuestRequest guestRequest)
        {
            // check if guest request is already in list
            GuestRequest oldGuestRequest = instance.GetGuestRequests().FirstOrDefault((GuestRequest gr) =>
                gr.GuestRequestKey == guestRequest.GuestRequestKey
            );

            // check if request id is unique
            if (oldGuestRequest == null)
            {
                // add to list
                DataSource.GuestRequests.Add(Cloning.Clone(guestRequest));
                // save xml
                SaveList(DataSource.GuestRequests, GuestRequestPath);
                ConfigXElement.Element("INITIAL_GUEST_REQUEST_KEY").Value = Config.stGuestRequestKey.ToString();
                ConfigXElement.Save(ConfigPath);
                return true;
            }

            throw new ArgumentException($"Guest Request with key {guestRequest.GuestRequestKey} already exists.");
        }

        /// <summary>
        /// Update Guest request in the data
        /// </summary>
        /// <param name="newGuestRequest">Updated Guest request</param>
        /// <returns>True if successful</returns>
        bool IDAL.UpdateGuestRequest(GuestRequest newGuestRequest)
        {
            // find guest request in list
            IEnumerable<GuestRequest> matches =
                from GuestRequest item in instance.GetGuestRequests()
                where item.GuestRequestKey == newGuestRequest.GuestRequestKey
                select item;

            // if not in list, add it to list
            if (matches.ToList().Count == 0)
                instance.CreateGuestRequest(newGuestRequest);

            GuestRequest oldGuestRequest = matches.ToList()[0];

            // find index of old guest request
            int index = DataSource.GuestRequests.FindIndex(item =>
                item.GuestRequestKey == oldGuestRequest.GuestRequestKey
            );

            // replace with the new guest request
            DataSource.GuestRequests[index] = Cloning.Clone(newGuestRequest);
            // save xml
            SaveList(DataSource.GuestRequests, GuestRequestPath);
            return true;
        }

        /// <summary>
        /// Return a deep copy of the guest request list
        /// </summary>
        List<GuestRequest> IDAL.GetGuestRequests()
        {
            try
            {
                return LoadList<GuestRequest>(GuestRequestPath);
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        #endregion

        #region HOSTING UNIT METHODS

        /// <summary>
        /// Place a new hosting unit into the data
        /// </summary>
        /// <param name="hostingUnit">Hosting unit to add</param>
        /// <returns>True when successful</returns>
        bool IDAL.CreateHostingUnit(HostingUnit hostingUnit)
        {
            // check if hosting unit is already in list
            HostingUnit oldHostingUnit = instance.GetHostingUnits().FirstOrDefault((HostingUnit hu) =>
                hu.HostingUnitKey == hostingUnit.HostingUnitKey
            );
            // Make sure the Hosting Unit is unique
            if (oldHostingUnit == null)
            {
                // add Hosting Unit to list
                DataSource.HostingUnits.Add(Cloning.Clone(hostingUnit));
                // save xml
                SaveList(DataSource.HostingUnits, HostingUnitPath);
                ConfigXElement.Element("INITIAL_HOSTING_UNIT_KEY").Value = Config.stHostingUnitKey.ToString();
                ConfigXElement.Save(ConfigPath);
                return true;
            }
            throw new ArgumentException($"Hosting Unit with key {hostingUnit.HostingUnitKey} already exists.");
        }
        /// <summary>
        /// Remove Hosting Unit from data given key
        /// </summary>
        /// <param name="hostingUnitKey">Key of Hosting unit</param>
        /// <returns>True when successful</returns>
        bool IDAL.DeleteHostingUnit(long hostingUnitKey)
        {
            // find hosting unit in list
            HostingUnit hostingUnitToDelete = DataSource.HostingUnits.FirstOrDefault(hu =>
                hu.HostingUnitKey == hostingUnitKey
            );

            if (hostingUnitToDelete != null)
            {
                // remove hosting unit
                DataSource.HostingUnits.Remove(hostingUnitToDelete);
                // save xml
                SaveList(DataSource.HostingUnits, HostingUnitPath);
                return true;
            }

            throw new Exception("Hosting unit was not found with given ID.");
        }

        /// <summary>
        /// Update Hosting unit in the data
        /// </summary>
        /// <param name="newHostingUnit">Updated Hosting unit</param>
        /// <returns>True if successful</returns>
        bool IDAL.UpdateHostingUnit(HostingUnit newHostingUnit)
        {
            // find hosting unit in list
            IEnumerable<HostingUnit> matches =
                from HostingUnit item in instance.GetHostingUnits()
                where item.HostingUnitKey == newHostingUnit.HostingUnitKey
                select item;

            // if not in list, add it to list
            if (matches.ToList().Count == 0)
                instance.CreateHostingUnit(newHostingUnit);

            HostingUnit oldHostingUnit = matches.ToList()[0];

            // find index of hosting unit
            int index = DataSource.HostingUnits.FindIndex(item =>
                item.HostingUnitKey == oldHostingUnit.HostingUnitKey
            );

            // replace with the new hosting unit
            DataSource.HostingUnits[index] = Cloning.Clone(newHostingUnit);
            // save xml
            SaveList(DataSource.HostingUnits, HostingUnitPath);
            return true;
        }

        /// <summary>
        /// Return a deep copy of the hosting units list
        /// </summary>
        List<HostingUnit> IDAL.GetHostingUnits()
        {
            try
            {
                return LoadList<HostingUnit>(HostingUnitPath);
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        #endregion

        #region ORDER METHODS

        /// <summary>
        /// Create an order in the list of orders
        /// </summary>
        /// <param name="order">Order to add</param>
        /// <returns>True if successful</returns>
        bool IDAL.CreateOrder(Order order)
        {
            // find order in list
            Order oldOrder = instance.GetOrders().FirstOrDefault((Order o) =>
                o.OrderKey == order.OrderKey
            );

            // Check that order is unique
            if (oldOrder == null)
            {
                // add Order to list
                DataSource.Orders.Add(Cloning.Clone(order));
                // save xml
                SaveList(DataSource.Orders, OrderPath);
                ConfigXElement.Element("INITIAL_ORDER_KEY").Value = Config.stOrderKey.ToString();
                ConfigXElement.Save(ConfigPath);
                return true;
            }

            throw new ArgumentException($"Order with key {order.OrderKey} already exists.");
        }

        /// <summary>
        /// Update order in the data
        /// </summary>
        /// <param name="newOrder">Updated order</param>
        /// <returns>True if successful</returns>
        bool IDAL.UpdateOrder(Order newOrder)
        {
            // find order in list
            IEnumerable<Order> matches =
                from Order item in instance.GetOrders()
                where item.OrderKey == newOrder.OrderKey
                select item;

            // if not in list, add it to list
            if (matches.ToList().Count == 0)
                instance.CreateOrder(newOrder);

            Order oldOrder = matches.ToList()[0];

            // find index of old order
            int index = DataSource.Orders.FindIndex(o =>
                o.OrderKey == oldOrder.OrderKey
            );

            // replace with the new order
            DataSource.Orders[index] = Cloning.Clone(newOrder);

            // save xml
            SaveList(DataSource.Orders, OrderPath);
            return true;
        }

        /// <summary>
        /// Return a deep copy of the orders list
        /// </summary>
        List<Order> IDAL.GetOrders()
        {
            try
            {
                return LoadList<Order>(OrderPath);
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        #endregion

        #region HOST METHODS

        /// <summary>
        /// Create a new host in the data
        /// </summary>
        bool IDAL.CreateHost(Host host)
        {
            // check if host is already in list
            Host oldHost = instance.GetHosts().FirstOrDefault((Host h) =>
                h.HostKey == host.HostKey
            );

            // make sure the host is unique
            if (oldHost == null)
            {
                // add to list
                DataSource.Hosts.Add(Cloning.Clone(host));
                // save xml
                SaveList(DataSource.Hosts, HostPath);
                ConfigXElement.Element("INITIAL_HOST_KEY").Value = Config.stHostKey.ToString();
                ConfigXElement.Save(ConfigPath);
                return true;
            }

            throw new ArgumentException($"Host with key {host.HostKey} already exists.");
        }

        /// <summary>
        /// Update host in the data
        /// </summary>
        /// <param name="newOrder">Updated order</param>
        /// <returns>True if successful</returns>
        bool IDAL.UpdateHost(Host newHost)
        {
            // find order in list
            Host oldHost = instance.GetHosts().FirstOrDefault(h => h.HostKey == newHost.HostKey);

            // if not in list, throw exception
            if (oldHost == null)
                throw new Exception("Host to update does not exist.");

            // find index of old order
            int index = DataSource.Hosts.FindIndex(h => h.HostKey == h.HostKey);

            // replace with the new order
            DataSource.Hosts[index] = Cloning.Clone(newHost);

            // save xml
            SaveList(DataSource.Hosts, HostPath);
            return true;
        }

        /// <summary>
        /// Get the list of hosts
        /// </summary>
        List<Host> IDAL.GetHosts()
        {
            try
            {
                return LoadList<Host>(HostPath);
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        #endregion
    }
}
