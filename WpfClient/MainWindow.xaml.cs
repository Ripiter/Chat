using Microsoft.Win32;
using System;
using System.Windows;
using System.IO;
using System.Windows.Media;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClientSide clientSide = new ClientSide();
        static readonly object _lock = new object();
        string userName = "";
        string path = "";
        string fileName = "";
        string file;
        bool connected = false;
        int amountConnected = 0;

        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(string username)
        {
            InitializeComponent();
            this.userName = username;
        }
        private void ConnectServer(object sender, RoutedEventArgs e)
        {
            // Stops from connecting multible times
            if (amountConnected > 0)
                return;

            // Initialize our connection to serwer
            clientSide.Initialize();
            clientSide.MessageRecived += ReceiveData;

            connected = true;
            amountConnected++;
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (connected == false)
                return;

            if (path == string.Empty)
                clientSide.SendMessage(InputMessage.Text, userName);
            else
            {
                clientSide.SendMessage(InputMessage.Text, userName,path, fileName);
                path = "";
                file = "";
                fileName = "";
            }
        }

        void ReceiveData(object sender, HoldingVariable e)
        {
            if (connected == false)
                return;

            string temp = "";

            // Receive values from serwer  
            if (e.File.Length == 0)
            {
                temp = string.Format("{0} by user {1}: {2}", e.Time, e.Name, e.Message);
            }
            else
            {
                file = e.File;
                //OutputMessage.Foreground = Brushes.Blue;
                temp = string.Format("{0} {1} send file {2} with message {3}", e.Time, e.Name,e.FileName, e.Message);
            }

            // Update list from another thread 
            OutputMessage.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                // Insert item on top of the list
                lock (_lock)
                    OutputMessage.Items.Insert(0, temp);

              
            }));
        }

        private void UploadFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "select file";
            op.Filter = "Exe file | *exe";
            if (op.ShowDialog() == true)
            {
                path = op.FileName;
                fileName = System.IO.Path.GetFileName(path);
            }
            
        }

        private void Selected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            string temp = OutputMessage.SelectedItem.ToString();
            if (temp.Contains("file"))
            {
                byte[] buffer = Convert.FromBase64String(file);
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Exe file |*.exe";
                if (save.ShowDialog() == true)
                    File.WriteAllBytes(save.FileName,buffer);
            }

        }
    }
}
