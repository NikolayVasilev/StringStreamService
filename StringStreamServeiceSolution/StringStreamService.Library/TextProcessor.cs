using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringStreamService.Service
{
    public class TextProcessor
    {
        private static readonly long MaxBufferSize = 1000000;
        private static readonly string FolderBase = @"\StringStreamService\TextsCache\";
        private long currentMaxEncounters = 0;
        private int bufferSize = 1000;

        internal List<string> CurrentBuffer { get; private set; }
        internal List<string> CacheFilePaths { get; private set; }

        internal Dictionary<string, long> LinesCounts { get; private set; }

        private ISessionWorker sessionWorker;
        private long currentDumpsCount = 0;

        public TextProcessor(ISessionWorker sessionWorker)
        {
            this.sessionWorker = sessionWorker;
            this.CurrentBuffer = new List<string>(bufferSize);
            this.CacheFilePaths = new List<string>();
            this.LinesCounts = new Dictionary<string, long>();
        }

        public Stream GetSortedStream()
        {
            if (this.CurrentBuffer.Count > 0)
            {
                this.DumpBuffer();
            }

            if(this.LinesCounts.Keys.Count > 0)
            {
                this.DumpBuffer();
            }

            return new MergedStream(this.CacheFilePaths.ToList());

            //return new FileStream(this.CacheFilePaths.FirstOrDefault(), FileMode.Open);
        }

        public void AppendLine(string line)
        {
            //this.CurrentBuffer.Add(line);

            if (!this.LinesCounts.ContainsKey(line))
            {
                this.LinesCounts.Add(line, 0);
            }

            this.LinesCounts[line]++;

            if(this.LinesCounts[line] > this.currentMaxEncounters)
            {
                this.currentMaxEncounters = this.LinesCounts[line];
            }

            if(this.currentMaxEncounters < 100 && this.bufferSize < MaxBufferSize)
            {
                this.bufferSize++;
            }

            if(this.LinesCounts.Keys.Count >= bufferSize)
            {
                this.DumpBuffer();
            }

            if (this.CurrentBuffer.Count >= bufferSize)
            {
                this.DumpBuffer();
            }
        }

        public string[] GetSortedTextFull()
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

                    if (readLines.ContainsKey(streamR) && streamR.EndOfStream)
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
            var dictionary = this.LinesCounts;

            this.LinesCounts = new Dictionary<string, long>();
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

                //foreach (var line in sortedCopy)
                //{
                //    output.WriteLine(line);
                //}

                var sortedKeys = dictionary.Keys.OrderBy(k => k);

                foreach (var item in sortedKeys)
                {
                    //for (int i = 0; i < dictionary[item]; i++)
                    //{
                    //    output.WriteLine(item);
                    //}

                    output.WriteLine(string.Format("{0} {1}", dictionary[item], item));
                }

                this.CacheFilePaths.Add(writePath);
            }
            //});
        }

        public void Clear()
        {
        }
    }
}
