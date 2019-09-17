using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Threading;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClientSide clientSide = new ClientSide();
        static readonly object _lock = new object();
        RecordPage recordPage;
        #region Variables 
        string userName = "";
        string path = "";
        string fileName = "";
        string file;
        bool connected = false;
        int amountConnected = 0;
        byte clickedTimes = 0;
        int clickDelay;
        #endregion
        public MainWindow()
        {
            InitializeComponent();

            MessageDelay.Visibility = Visibility.Hidden;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        public MainWindow(string username)
        {
            InitializeComponent();
            this.userName = username;
            MessageDelay.Visibility = Visibility.Hidden;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += timer_Tick;
            timer.Start();

        }

        /// <summary>
        /// Gets called once a sec
        /// </summary>
        void timer_Tick(object sender, EventArgs e)
        {
            clickDelay++;
        }

        private void ConnectServer(object sender, RoutedEventArgs e)
        {
            // Stops from connecting multible times
            if (amountConnected > 0)
                return;

            // Initialize our connection to serwer
            MessageBox.Show(clientSide.Initialize());
            clientSide.MessageRecived += ReceiveData;

            connected = true;
            amountConnected++;
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (connected == false)
                return;

            #region Click Delay
            clickedTimes++;
            if (clickedTimes > 5)
            {
                MessageDelay.Visibility = Visibility.Visible;
                if (clickDelay <= 3)
                {
                    return;
                }
                clickedTimes = 0;
                MessageDelay.Visibility = Visibility.Hidden;
            }
            else
            {
                clickDelay = 0;
            }
            #endregion

            if (path == string.Empty && recordPage.Recorded == false)
            {
                clientSide.SendMessage(InputMessage.Text, userName);
                path = string.Empty;
                file = string.Empty;
                fileName = string.Empty;
            }
            else if(recordPage.Recorded == false)
            {
                if (clientSide.SendFile(InputMessage.Text, userName, path, fileName) == "Fail")
                    MessageBox.Show("File To Big");

                path = string.Empty;
                file = string.Empty;
                fileName = string.Empty;
            }
            else
            {
                if (clientSide.SendVoiceMessage(InputMessage.Text, userName, recordPage.SoundByte, fileName) == "Fail")
                    MessageBox.Show("File To Big");

                path = string.Empty;
                file = string.Empty;
                fileName = string.Empty;
                recordPage.Recorded = false;
            }
        }

        void ReceiveData(object sender, HoldingVariable e)
        {
            if (connected == false)
                return;
           
            Test data = null;
            string temp = "";

            if (e.File.Length == 0)
            {
                temp = string.Format("{0} by user {1}: {2}", e.Time, e.Name, e.Message);
                data = new NormalMessage() { SpecialMessage = "Normal", Name = temp };
            }
            else if(e.FileName.Contains(".exe"))
            {
                file = e.File;
                temp = string.Format("{0} {1} send file {2} with message {3}", e.Time, e.Name, e.FileName, e.Message);
                data = new GotFile() { SpecialMessage = "File", Name = temp };
            }
            else if (e.FileName.Contains("sound"))
            {
                file = e.File;
                temp = string.Format("{0} {1} send voice message {2} with message {3}", e.Time, e.Name, e.FileName, e.Message);
                data = new VoiceMessage() { SpecialMessage = "Voice", Name = temp };
            }

            // Update list from another thread 
            OutputMessage.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                // Insert item on top of the list
                lock (_lock)
                    OutputMessage.Items.Insert(0, data);
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

        /// <summary>
        /// Gets selected item from the list
        /// </summary>
        private void Selected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string temp = OutputMessage.SelectedItem.ToString();
            if (temp.Contains("GotFile"))
            {
                byte[] buffer = Convert.FromBase64String(file);
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Exe file |*.exe";
                if (save.ShowDialog() == true)
                    System.IO.File.WriteAllBytes(save.FileName, buffer);
            }
            else if (temp.Contains("VoiceMessage"))
            {
                byte[] buffer = Convert.FromBase64String(file);

                if (buffer.Length == 0)
                    return;

                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    SoundPlayer player = new SoundPlayer(ms);

                    player.Play();
                }
            }
        }

        private void OpenRecordMenu(object sender, RoutedEventArgs e)
        {
            recordPage = new RecordPage();
            recordPage.Show();
        }
    }

    #region Color Selector
    public class GotFile : Test
    {

    }
    public class NormalMessage : Test
    {

    }
    public class VoiceMessage : Test
    {

    }
    public class Test
    {
        public string Name { get; set; }
        public string SpecialMessage { get; set; }
    }
    #endregion
}
