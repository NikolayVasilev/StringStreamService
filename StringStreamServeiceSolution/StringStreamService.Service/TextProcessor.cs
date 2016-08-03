using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringStreamService.Service
{
    internal class TextProcessor
    {
        private static readonly int MaxBufferSize = 100000;
        private static readonly string FolderBase = @"\StringStreamService\TextsCache\";

        internal List<string> CurrentBuffer { get; private set; }
        internal List<string> CacheFilePaths { get; private set; }
        private ISessionWorker sessionWorker;
        private int currentDumpsCount = 0;

        public TextProcessor(ISessionWorker sessionWorker)
        {
            this.sessionWorker = sessionWorker;
            this.CurrentBuffer = new List<string>(MaxBufferSize);
            this.CacheFilePaths = new List<string>();
        }

        internal Stream GetSortedStream()
        {
            if(this.CurrentBuffer.Count > 0)
            {
                this.DumpBuffer();
            }

            return new MergedStream(this.CacheFilePaths.ToList());
        }

        internal void AppendLine(string line)
        {
            this.CurrentBuffer.Add(line);

            if (this.CurrentBuffer.Count >= MaxBufferSize)
            {
                this.DumpBuffer();
            }
        }

        internal string[] GetSortedTextFull()
        {
            List<string> result = new List<string>();

            List<string> currentFiles = this.CacheFilePaths.ToList();

            List<StreamReader> streamReaders = this.GetStreamReadersForFiles(currentFiles);

            Dictionary<StreamReader, string> readLines = new Dictionary<StreamReader, string>();

            bool shouldContinue = true;

            var start = DateTime.Now;

            while (shouldContinue)
            {
                string minFromStreams = null;
                StreamReader minStream = null;
                foreach (var streamR in streamReaders)
                {
                    if (!readLines.ContainsKey(streamR) && !streamR.EndOfStream)
                    {
                        readLines.Add(streamR, null);
                    }

                    if(readLines.ContainsKey(streamR) && streamR.EndOfStream)
                    {
                        readLines.Remove(streamR);
                    }

                    if (readLines.ContainsKey(streamR) && readLines[streamR] == null)
                    {
                        readLines[streamR] = streamR.ReadLine();
                    }

                    if (readLines.ContainsKey(streamR) && (minFromStreams == null || readLines[streamR].CompareTo(minFromStreams) <= 0))
                    {
                        minStream = streamR;
                        minFromStreams = readLines[streamR];
                    }
                }

                shouldContinue = streamReaders.Any(sr => !sr.EndOfStream);

                readLines[minStream] = null;

                result.Add(minFromStreams);
            }

            Debug.WriteLine("MergeTime :" + (DateTime.Now - start).TotalMilliseconds + "ms");

            return result.ToArray();
        }

        private List<StreamReader> GetStreamReadersForFiles(List<string> files)
        {
            List<StreamReader> result = new List<StreamReader>();
            foreach (var path in files)
            {
                result.Add(new StreamReader(path));
            }

            return result;
        }

        private void DumpBuffer()
        {
            var bufferCopy = this.CurrentBuffer.ToList();
            this.CurrentBuffer.Clear();


            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + FolderBase + this.sessionWorker.Id.ToString();
            var writePath = basePath + "\\" + this.currentDumpsCount++ + ".txt";

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            var sortedCopy = bufferCopy.OrderBy(s => s).ToList();

            //Task.Factory.StartNew(() =>
            //{
                using (StreamWriter output = new StreamWriter(writePath))
                {

                    foreach (var line in sortedCopy)
                    {
                        output.WriteLine(line);
                    }

                    this.CacheFilePaths.Add(writePath);
                }
            //});
        }

        internal void Clear()
        {
        }
    }
}
