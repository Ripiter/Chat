using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        NetworkStream ns;
        bool connected = false;
        int amountConnected = 0;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void ConnectServer(object sender, RoutedEventArgs e)
        {
            // Stops fro connecting multible times
            if (amountConnected > 0)
                return;
            // To do: add box to add custom ip
            // Ip of the server
            IPAddress ip = IPAddress.Parse("10.109.169.64");
            int port = 5000;
            TcpClient client = new TcpClient();

            try
            {
                client.Connect(ip, port);
                MessageBox.Show("client connected!!");
            }
            catch (Exception ee)
            {
                MessageBox.Show("Error " + ee.Message);
            }

            ns = client.GetStream();
            Thread thread = new Thread(o => ReceiveData((TcpClient)o));

            thread.Start(client);
            connected = true;
            amountConnected++;
        }


        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (connected == false)
                return;

            // Getting byte[] from xml
            Encoding encoding = Encoding.ASCII;
            XElement element = new XElement("Root",
                             new XElement("Name", "Steve"),
                             new XElement("Time", DateTime.Now.ToString("t")));

            byte[] buffer = ConvertXmlToByteArray(element, encoding);


            ns.Write(buffer, 0, buffer.Length);
        }

        void ReceiveData(TcpClient client)
        {
            if (connected == false)
                return;

            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;
            string temp = string.Empty;
            string name = string.Empty;
            string time = string.Empty;
            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                
                XmlDocument el = ConvertByteArrayToXml(receivedBytes, Encoding.ASCII);

                // Creates list of nodes inside of xml element between root
                XmlNodeList both = el.SelectNodes("/Root");

                foreach (XmlNode item in both)
                {
                    name = item["Name"].InnerText;
                    time = item["Time"].InnerText;
                }

                temp = string.Format("{0} by user {1}",time,name);
                
                // Update list from another thread 
                OutputMessage.Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    // Insert item on top of the list
                    OutputMessage.Items.Insert(0, temp);
                }));
            }
        }


        byte[] ConvertXmlToByteArray(XElement xml, Encoding encoding)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                // Add formatting and other writer options here if desired
                settings.Encoding = encoding;
                settings.OmitXmlDeclaration = true; // No prolog
                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    xml.Save(writer);
                }
                return stream.ToArray();
            }
        }

        XmlDocument ConvertByteArrayToXml(byte[] data, Encoding encoding)
        {
            XmlDocument doc = new XmlDocument();
            MemoryStream ms = new MemoryStream(data);
            doc.Load(ms);
            return doc;
        }
    }
}
