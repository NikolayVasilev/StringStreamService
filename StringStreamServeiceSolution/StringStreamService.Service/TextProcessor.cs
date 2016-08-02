using System;
using System.Collections.Generic;
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
        private int dumpNumber = 0;

        public TextProcessor(ISessionWorker sessionWorker)
        {
            this.sessionWorker = sessionWorker;
            this.CurrentBuffer = new List<string>(MaxBufferSize);
            this.CacheFilePaths = new List<string>();
        }


        internal void AppendLine(string line)
        {
            this.CurrentBuffer.Add(line);

            if(this.CurrentBuffer.Count > MaxBufferSize)
            {
                this.DumpBuffer();
            }
        }

        private void DumpBuffer()
        {
            var bufferCopy = this.CurrentBuffer.ToList();
            this.CurrentBuffer.Clear();

            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + FolderBase + this.sessionWorker.Id.ToString();
            var writePath = basePath  + "\\" + this.CacheFilePaths.Count + ".txt";

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            using (StreamWriter output = new StreamWriter(writePath))
            {
                var sortedCopy = bufferCopy.OrderBy(s => s);

                foreach(var line in sortedCopy)
                {
                    output.WriteLine(line);
                }

                this.CacheFilePaths.Add(writePath);
            }
        }

        internal void Clear()
        {
        }
    }
}
