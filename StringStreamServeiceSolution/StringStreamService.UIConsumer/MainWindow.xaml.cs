using Microsoft.Win32;
using StringStreamService.UIConsumer.StreamService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;

namespace StringStreamService.UIConsumer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string filePath = null;
        private StringStreamServiceClient client;
        private Guid guid;
        private StreamReader fileReader;

        public MainWindow()
        {
            InitializeComponent();

            this.client = new StringStreamServiceClient();
            this.guid = this.client.BeginStream();
            this.DataContext = this;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            this.CleanUp();
        }

        private void OpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Text Files (.txt)|*.txt";

            var result = dialog.ShowDialog();

            if (result != null && result.Value)
            {
                this.filePath = dialog.FileName;
            }
        }
        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            long linesToSend = 0;

            try
            {
                linesToSend = long.Parse(this.LinesToStreamTB.Text);
            }
            catch (Exception)
            {
                linesToSend = long.MaxValue;
                this.LinesToStreamTB.Text = linesToSend.ToString();
            }

            if (this.filePath != null && linesToSend > 0)
            {
                if (this.fileReader == null)
                {
                    this.fileReader = new StreamReader(this.filePath);
                }

                this.SendLines(linesToSend);
            }
        }


        private void ReadBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Read();
        }

        private void Read()
        {
            Task.Factory.StartNew(() =>
            {
                var stream = this.client.GetSortedStream(guid);

                long sortedLines = 0;
                UpdateReadLines(sortedLines);

                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string read = reader.ReadLine();

                        //Debug.WriteLine(read);
                        sortedLines++;

                        if (sortedLines % 1000 == 0)
                        {
                            UpdateReadLines(sortedLines);
                        }
                    }
                }

                UpdateReadLines(sortedLines);
            });
        }

        private void UpdateReadLines(long sortedLines)
        {
            Dispatcher.BeginInvoke((Action) (() =>
             {
                 this.ReceivedLinesLabel.Text = sortedLines.ToString();
             }));
        }

        private void SendLines(long linesToSend)
        {
            long lines = 0;

            List<string> readLines = new List<string>();

            while (!this.fileReader.EndOfStream && lines < linesToSend)
            {
                var read = this.fileReader.ReadLine();
                lines++;

                readLines.Add(read);

                if (readLines.Count > 10000)
                {
                    this.client.PutStreamData(guid, readLines.ToArray());
                    readLines.Clear();
                }
            }

            this.client.PutStreamData(guid, readLines.ToArray());
            readLines.Clear();

            this.SentLinesLabel.Text = (long.Parse(this.SentLinesLabel.Text) + lines).ToString();
        }

        internal void CleanUp()
        {
            if (this.fileReader != null)
            {
                this.fileReader.Close();
                this.fileReader.Dispose();
                this.fileReader = null;
            }

            this.client.Close();
        }

        private void RestartBtn_Click(object sender, RoutedEventArgs e)
        {
            this.CleanUp();

            this.client = new StringStreamServiceClient();
        }
    }
}
