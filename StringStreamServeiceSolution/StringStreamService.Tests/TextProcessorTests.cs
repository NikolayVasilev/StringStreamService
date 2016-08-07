using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StringStreamService.Engine;
using System.IO;
using System.Collections.Generic;

namespace StringStreamService.Tests
{
    [TestClass]
    public class TextProcessorTests
    {
        private TextProcessor processor;

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
                if (sentLinesCount % 10000 == 0)
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
