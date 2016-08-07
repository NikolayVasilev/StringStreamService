using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StringStreamService.Engine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace StringStreamService.Tests
{
    [TestClass]
    public class TextProcessorTests
    {
        private static Random rand = new Random();
        private TextProcessor processor;
        private string largeFileName = "largeFile16G.txt";

        [TestInitialize]
        public void Initialize()
        {
            this.processor = new TextProcessor(Guid.NewGuid());
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.processor.ClearFiles();
        }

        [TestMethod]
        public void ShouldReturnSameCountItems_SmallAmount()
        {
            long sentLinesCount = 1000;

            for (long i = 0; i < sentLinesCount; i++)
            {
                this.processor.AppendLine(i.ToString());
            }

            var readLinesCount = this.GetReadLinesCount();

            Assert.AreEqual(sentLinesCount, readLinesCount);
        }

        [TestMethod]
        public void ShouldReturnSameCountItems_LargeAmount()
        {
            long sentLinesCount = 10000000;

            for (long i = 0; i < sentLinesCount; i++)
            {
                this.processor.AppendLine(i.ToString());
            }

            var readLinesCount = this.GetReadLinesCount();

            Assert.AreEqual(sentLinesCount, readLinesCount);
        }

        [TestMethod]
        public void ReadLinesShoudlMatchSentLinesAtAnyMoment()
        {
            long sentLinesCount = 1000000;
            long readLinesCount = 0;
            long currentSentLines = 0;

            for (long i = 0; i < sentLinesCount; i++)
            {
                if (currentSentLines % 100000 == 0)
                {
                    readLinesCount = this.GetReadLinesCount();
                    Assert.AreEqual(i, readLinesCount);
                }

                this.processor.AppendLine(i.ToString());
                currentSentLines++;
            }

            readLinesCount = this.GetReadLinesCount();

            Assert.AreEqual(sentLinesCount, readLinesCount);
        }

        [TestMethod]
        public void ReadLinesShouldBeSorted()
        {
            long sentLinesCount = 100000;
            List<string> sentLines = new List<string>();

            for (long i = 0; i < sentLinesCount; i++)
            {
                string stringToSend = (i % 20).ToString();
                this.processor.AppendLine(stringToSend);
                sentLines.Add(stringToSend);
            }

            var readLines = this.GetReadLines();

            Assert.AreEqual(sentLines.Count, readLines.Count);

            sentLines.Sort();

            for (int i = 0; i < readLines.Count; i++)
            {
                Assert.AreEqual(sentLines[i], readLines[i]);
            }
        }

        [TestMethod]
        public void ShouldBeAbleToProcess16GBFile()
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + this.largeFileName;

            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + this.largeFileName))
            {
                this.Generate16GBFile();
            }

            long sentLines = 0;

            using (var sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    this.processor.AppendLine(sr.ReadLine());
                    sentLines++;
                }
            }

            var readLines = this.GetReadLinesCount();

            Assert.AreEqual(sentLines, readLines);
        }


        private void Generate16GBFile()
        {
            List<string> existing = new List<string>();
            string fileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + this.largeFileName;
            for (int i = 0; i < 1000000; i++)
            {
                existing.Add(Guid.NewGuid().ToString());
                //existing.Add(i.ToString());
            }

            using (StreamWriter sr = new StreamWriter(fileName))
            {
                var fileInfo = new FileInfo(fileName);

                while (fileInfo.Length < 17180180000)
                //while (fileInfo.Length < 1295020000)
                //while (fileInfo.Length < 9295017985)
                //while (fileInfo.Length < 294967296)
                //while (fileInfo.Length < 17179)
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        var index = rand.Next(0, 1000000 - 1);

                        sr.Write(existing[index]);
                        sr.Write(existing[index]);
                        sr.Write(existing[index]);
                        sr.WriteLine(existing[index]);
                    }

                    fileInfo = new FileInfo(fileName);
                }
            }
        }

        private List<string> GetReadLines()
        {
            Stream stream = this.processor.GetSortedStream();
            List<string> readLines = new List<string>();

            if (stream.CanRead)
            {
                using (var sr = new StreamReader(stream))
                {
                    while (!sr.EndOfStream)
                    {
                        var read = sr.ReadLine();
                        readLines.Add(read);
                    }
                }
            }

            return readLines;
        }

        private long GetReadLinesCount()
        {
            Stream stream = this.processor.GetSortedStream();
            long linesCount = 0;

            if (stream.CanRead)
            {
                using (var sr = new StreamReader(stream))
                {
                    while (!sr.EndOfStream)
                    {
                        sr.ReadLine();
                        linesCount++;
                    }
                }
            }

            return linesCount;
        }
    }
}
