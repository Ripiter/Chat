using System;
using System.Diagnostics;
//using System.Windows.Shapes;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for RecordPage.xaml
    /// </summary>
    public partial class RecordPage : Window
    {
        private bool recorded = false;
        private bool recording = false;
        public bool Recorded { get => recorded; set => recorded = value; }

        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);

        string path = Path.GetTempPath();
        byte recordingLenght;
        DispatcherTimer timer = new DispatcherTimer();


        public RecordPage()
        {
            InitializeComponent();
            recordingLabel.Visibility = Visibility.Hidden;

            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += timer_Tick;
            
        }
        void timer_Tick(object sender, EventArgs e)
        {
            recordingLenght++;
            Debug.WriteLine(recordingLenght);
            recordingLabel.Content = string.Format("Recording: {0}",recordingLenght);
            if (recordingLenght == 10)
                StopRecordningMethod();
        }
        private void StartRecording(object sender, RoutedEventArgs e)
        {
            if (recording == true)
                return;

            recordingLabel.Visibility = Visibility.Visible;
            mciSendString("open new Type waveaudio Alias recsound", "", 0, 0);
            mciSendString("record recsound", "", 0, 0);
            recording = true;
            timer.Start();
        }

        private void StopRecording(object sender, RoutedEventArgs e)
        {
            StopRecordningMethod();
        }

        private void Play(object sender, RoutedEventArgs e)
        {
            if (recording == true)
                return;

            byte[] soundByte = File.ReadAllBytes(path + "result.wav");

            if (soundByte.Length == 0)
                return;

            using (MemoryStream ms = new MemoryStream(soundByte))
            {
                SoundPlayer player = new SoundPlayer(ms);

                player.Play();
            }
        }

        private void StopRecordningMethod()
        {
            recording = false;
            Recorded = true;
            timer.Stop();
            recordingLenght = 0;
            recordingLenght = 0;
            recordingLabel.Visibility = Visibility.Hidden;
            mciSendString("save recsound " + path + "result.wav", "", 0, 0);
            mciSendString("close recsound ", "", 0, 0);
        }
    }
}
