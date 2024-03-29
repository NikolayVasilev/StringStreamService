﻿using Microsoft.Win32;
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
using System.Threading;
using StringStreamService.Engine;

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

        private long sentLines = 0;
        private TextProcessor textProcessor;
        private Task writeTask;

        public MainWindow()
        {
            InitializeComponent();

            this.client = new StringStreamServiceClient();
            this.guid = this.client.BeginStream();
            this.DataContext = this;

            this.textProcessor = new TextProcessor(Guid.NewGuid());
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
            if (this.writeTask != null)
            {
                this.writeTask.ContinueWith((argument) => { this.ReadExecute(); });
            }
            else
            {
                Task.Factory.StartNew((Action)(() =>
                {
                    ReadExecute();
                }));
            }
        }

        private void ReadExecute()
        {
            var stream = this.client.GetSortedStream(this.guid);
            //var stream = this.textProcessor.GetSortedStream();

            long sortedLines = 0;
            this.UpdateReadLines((long)sortedLines);

            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string read = reader.ReadLine();

                    //Debug.WriteLine(read);
                    sortedLines++;

                    if (sortedLines % 1000 == 0)
                    {
                        this.UpdateReadLines((long)sortedLines);
                    }
                    else if (sortedLines % 20000 == 0)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(10000));
                        Task.Delay(TimeSpan.FromMilliseconds(10000));
                    }
                }
            }

            this.UpdateReadLines((long)sortedLines);
        }

        private void UpdateReadLines(long sortedLines)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                this.ReceivedLinesLabel.Text = sortedLines.ToString();
            }));
        }

        private void SendLines(long linesToSend)
        {
            this.writeTask = Task.Factory.StartNew((Action)(() =>
            {
                long lines = 0;

                List<string> readLines = new List<string>();
                while (!this.fileReader.EndOfStream && lines < linesToSend)
                {
                    var read = this.fileReader.ReadLine();
                    //this.textProcessor.AppendLine(read);
                    lines++;
                    this.sentLines++;

                    readLines.Add(read);

                    if (readLines.Count > 10000)
                    {
                        this.client.PutStreamData(this.guid, readLines.ToArray());
                        readLines.Clear();
                    }

                    if (lines % 1000 == 0)
                    {
                        this.UpdateSentLines(this.sentLines);
                    }
                }

                this.client.PutStreamData(this.guid, readLines.ToArray());
                readLines.Clear();

                this.UpdateSentLines(this.sentLines);

                this.writeTask = null;
            }));

        }

        private void UpdateSentLines(long lines)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                this.SentLinesLabel.Text = lines.ToString();
            }));
        }

        internal void CleanUp()
        {
            if (this.fileReader != null)
            {
                this.fileReader.Close();
                this.fileReader.Dispose();
                this.fileReader = null;
            }

            this.client.EndStream(this.guid);
            this.client.Close();
            this.sentLines = 0;

            this.SentLinesLabel.Text = "0";
            this.ReceivedLinesLabel.Text = "0";

        }

        private void RestartBtn_Click(object sender, RoutedEventArgs e)
        {
            this.CleanUp();

            this.client = new StringStreamServiceClient();
            this.guid = this.client.BeginStream();
        }
    }
}
