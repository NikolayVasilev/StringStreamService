using StringStreamService.Consumer.ServiceReference1;
using StringStreamService.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringStreamService.Consumer
{
    class Consumer
    {
        static void Main(string[] args)
        {
            TrySimpleStreaming();


            string fileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "largeFile_guidsManyValues.txt";

            //TryLargeFileStreaming(fileName);
            //TryLargeFileStreamingNoService(fileName);
        }

        private static long ReadStream(StringStreamServiceClient serviceClient, Guid guid)
        {
            var stream = serviceClient.GetSortedStream(guid);

            long sortedLines = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    reader.ReadLine();
                    sortedLines++;
                }
            }

            return sortedLines;
        }

        private static long UploadFile(string fileName, StringStreamServiceClient serviceClient, Guid guid)
        {
            long lines = 0;

            using (var rdr = new StreamReader(fileName))
            {
                List<string> readLines = new List<string>();

                while (!rdr.EndOfStream)
                {
                    var read = rdr.ReadLine();
                    lines++;

                    readLines.Add(read);

                    if (readLines.Count > 10000)
                    {
                        serviceClient.PutStreamData(guid, readLines.ToArray());
                        readLines.Clear();
                    }
                }

                serviceClient.PutStreamData(guid, readLines.ToArray());
                readLines.Clear();
            }

            return lines;
        }

        private static long UploadFileToEngine(string fileName, TextProcessor processor)
        {
            long lines = 0;
            using (var rdr = new StreamReader(fileName))
            {
                List<string> readLines = new List<string>();

                while (!rdr.EndOfStream)
                {
                    var read = rdr.ReadLine();
                    lines++;

                    processor.AppendLine(read);
                }
            }

            return lines;
        }

        private static long ReadStreamFromEngine(TextProcessor processor)
        {
            long sortedLines = 0;
            var stream = processor.GetSortedStream();

            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    reader.ReadLine();
                    sortedLines++;
                }
            }

            return sortedLines;
        }

        private static void TryLargeFileStreamingNoService(string fileName)
        {
            var processor = new TextProcessor(Guid.NewGuid());

            Console.WriteLine("StartUpload");
            var startUpload = DateTime.Now;
            var lines = UploadFileToEngine(fileName, processor);

            Console.WriteLine("End upload. Elapsed: {0} ms", (DateTime.Now - startUpload).TotalMilliseconds);



            Console.WriteLine("StartStreaming");
            var startStreaming = DateTime.Now;

            var sortedLines = ReadStreamFromEngine(processor);

            Console.WriteLine("End stream. Elapsed: {0} ms", (DateTime.Now - startStreaming).TotalMilliseconds);

            Console.WriteLine("All acounted for: {0}.  Expected: {1}, actual: {2}", sortedLines == lines, lines, sortedLines);
        }

        private static void TryLargeFileStreaming(string fileName)
        {
            var serviceClient = new ServiceReference1.StringStreamServiceClient();
            var guid = serviceClient.BeginStream();


            Console.WriteLine("StartUpload");
            var startUpload = DateTime.Now;

            long lines = UploadFile(fileName, serviceClient, guid);

            Console.WriteLine("End upload. Elapsed: {0} ms", (DateTime.Now - startUpload).TotalMilliseconds);

            var stream = serviceClient.GetSortedStream(guid);

            Console.WriteLine("StartStreaming");
            var startStreaming = DateTime.Now;

            long sortedLines = ReadStream(serviceClient, guid);

            Console.WriteLine("End stream. Elapsed: {0} ms", (DateTime.Now - startStreaming).TotalMilliseconds);

            Console.WriteLine("All acounted for: {0}.  Expected: {1}, actual: {2}", sortedLines == lines, lines, sortedLines);
        }

        private static void TrySimpleStreaming()
        {
            var serviceClient = new ServiceReference1.StringStreamServiceClient();

            var guid = serviceClient.BeginStream();

            List<string> strings = new List<string>();

            for (int i = 0; i < 1000; i++)
            {
                var guidString = i.ToString();

                strings.Add(guidString);
            }

            serviceClient.PutStreamData(guid, strings.ToArray());


            Stream a = serviceClient.GetSortedStream(guid);

            using (StreamReader reader = new StreamReader(a))
            {
                while (!reader.EndOfStream)
                {
                    Console.WriteLine(reader.ReadLine());
                }
            }

            serviceClient.EndStream(guid);

            serviceClient.Close();
        }
    }
}
