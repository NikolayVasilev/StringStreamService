using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;

namespace StringStreamService.Service
{
    public class MergedStream : Stream
    {
        static int streamedLines = 0;

        private List<string> fileList;
        private List<StreamReader> streamReaders;
        private Dictionary<StreamReader, StringEncounter> readLines = new Dictionary<StreamReader, StringEncounter>();
        private List<byte> readBytes = new List<byte>();

        private string minFromStreams = null;
        private StreamReader minStream = null;

        public MergedStream(List<string> list)
        {
            this.fileList = list.ToList();
            streamReaders = this.GetStreamReadersForFiles(fileList);
        }


        public override bool CanRead
        {
            get
            {
                return this.HasData();
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return this.GetTotalLength();
            }
        }

        public override long Position
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        public override void Flush()
        {
            throw new InvalidOperationException("Stream is Read-only");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(this.readBytes.Count <= count * 10)
            {
                this.FillReadBytes(count * 10);
            }

            int readBytesCount = 0;

            for (int i = 0; i < Math.Min(count, this.readBytes.Count); i++)
            {
                buffer[i] = this.readBytes[i];
                readBytesCount++;
            }

            this.readBytes.RemoveRange(0, readBytesCount);

            return readBytesCount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException("Stream is Read-only");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("Stream is Read-only");
        }

        private void FillReadBytes(int countNeeded)
        {
            while (this.readBytes.Count < countNeeded && this.HasData())
            {
                foreach (var stream in this.streamReaders)
                {
                    if (!this.readLines.ContainsKey(stream) && !stream.EndOfStream)
                    {
                        this.readLines.Add(stream, null);
                    }

                    if (this.readLines.ContainsKey(stream) && this.readLines[stream] == null && stream.EndOfStream)
                    {
                        this.readLines.Remove(stream);
                        continue;
                    }

                    if (this.readLines.ContainsKey(stream) && this.readLines[stream] == null)
                    {
                        readLines[stream] = new StringEncounter(stream.ReadLine());
                    }

                    if (this.readLines.ContainsKey(stream) && (this.minFromStreams == null || this.readLines[stream].String.CompareTo(this.minFromStreams) < 0))
                    {
                        this.minStream = stream;
                        this.minFromStreams = readLines[stream].String;
                    }
                }

                var resultToAdd = this.minFromStreams + Environment.NewLine;

                for (long i = 0; i < this.readLines[this.minStream].Count; i++)
                {
                    streamedLines++;
                    this.readBytes.AddRange(System.Text.Encoding.UTF8.GetBytes(resultToAdd));
                }

                this.readLines[this.minStream] = null;

                this.minStream = null;
                this.minFromStreams = null;
            }
        }

        private long GetTotalLength()
        {
            return this.streamReaders.Sum(sr => sr.BaseStream.Length);
        }

        private bool HasData()
        {
            return this.streamReaders.Any(sr => !sr.EndOfStream);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            foreach (var streamReader in this.streamReaders)
            {
                streamReader.Close();
                streamReader.Dispose();
            }
        }

        private List<StreamReader> GetStreamReadersForFiles(List<string> fileList)
        {
            List<StreamReader> result = new List<StreamReader>();
            foreach (var path in this.fileList)
            {
                result.Add(new StreamReader(path));
            }

            return result;
        }
    }
}
