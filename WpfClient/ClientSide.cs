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
    class ClientSide
    {
        static readonly object _lock = new object();
        NetworkStream ns;

        public void Initialize()
        {
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
            Thread thread = new Thread(o => GetMessages((TcpClient)o));

            thread.Start(client);
        }
        public void SendMessage(string text,string userName)
        {
            // Getting byte[] from xml
            Encoding encoding = Encoding.ASCII;
            XElement element = new XElement("Root",
                             new XElement("Name", userName),
                             new XElement("Message", text),
                             new XElement("File", null),
                             new XElement("FileName",null),
                             new XElement("Time", DateTime.Now.ToString("t")));

            byte[] buffer = ConvertXmlToByteArray(element, encoding);

            ns.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Sends message with a file
        /// </summary>
        public void SendMessage(string text,string userName,string filePath, string fileName)
        {
            byte[] fileBuffer = File.ReadAllBytes(filePath);
            string base64Enc = Convert.ToBase64String(fileBuffer);
            // Getting byte[] from xml
            Encoding encoding = Encoding.ASCII;
            XElement element = new XElement("Root",
                             new XElement("Name", userName),
                             new XElement("Message", text),
                             new XElement("File",base64Enc),
                             new XElement("FileName",fileName),
                             new XElement("Time", DateTime.Now.ToString("t")));

            byte[] buffer = ConvertXmlToByteArray(element, encoding);

            ns.Write(buffer, 0, buffer.Length);
        }

        public void GetMessages(TcpClient client)
        {
            HoldingVariable values = new HoldingVariable();
            NetworkStream ns = client.GetStream();
            XmlDocument el;

            byte[] receivedBytes = new byte[32768];
            int byte_count;
            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                lock (_lock)
                    el = ConvertByteArrayToXml(receivedBytes);

                // Creates list of nodes inside of xml element between root
                XmlNodeList both = el.SelectNodes("/Root");

                foreach (XmlNode item in both)
                {
                    values.Name    = item["Name"].InnerText;
                    values.Time    = item["Time"].InnerText;
                    values.Message = item["Message"].InnerText;

                    if(item["File"].InnerText != null)
                    {
                        values.File    = item["File"].InnerText;
                        values.FileName = item["FileName"].InnerText;
                    }
                }

                GotMessage(values);
            }
        }
        public void GotMessage(HoldingVariable e)
        {
            MessageRecived(this, e);
        }

        public event EventHandler<HoldingVariable> MessageRecived;

        #region Xml Region
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

        XmlDocument ConvertByteArrayToXml(byte[] data)
        {
            XmlDocument doc = new XmlDocument();
            MemoryStream ms = new MemoryStream(data);
            doc.Load(ms);
            return doc;
        }
        #endregion
    }

    public class HoldingVariable
    {
        public string Name { get; set; }
        public string Time { get; set; }
        public string Message { get; set; }
        public string File { get; set; }
        public string FileName { get; set; }
    }

}
