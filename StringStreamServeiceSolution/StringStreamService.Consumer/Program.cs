using StringStreamService.Consumer.ServiceReference1;
using StringStreamService.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringStreamService.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            //TrySimpleStreaming();

            TryLargeFileStreaming();
            //TryLargeFileStreamingNoService();
        }

        private static void TryLargeFileStreamingNoService()
        {
            string fileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "largeFile_guids16G.txt";

            //var serviceClient = new ServiceReference1.StringStreamServiceClient();
            var processor = new TextProcessor(new SessionWorker());

            var guid = new Guid();

            long lines = 0;
            Console.WriteLine("StartUpload");
            var startUpload = DateTime.Now;
            using (var rdr = new StreamReader(fileName))
            {
                List<string> readLines = new List<string>();

                while (!rdr.EndOfStream)
                {
                    var read = rdr.ReadLine();
                    lines++;

                    //readLines.Add(read);
                    processor.AppendLine(read);

                    //if (readLines.Count > 10000)
                    //{
                    //    processor.PutStreamData(guid, readLines.ToArray());
                    //    readLines.Clear();
                    //}
                }

                //serviceClient.PutStreamData(guid, readLines.ToArray());
                //readLines.Clear();
            }

            Console.WriteLine("End upload. Elapsed: {0} ms", (DateTime.Now - startUpload).TotalMilliseconds);

            var stream = processor.GetSortedStream();

            long sortedLines = 0;

            Console.WriteLine("StartStreaming");
            var startStreaming = DateTime.Now;

            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    //Console.WriteLine(reader.ReadLine());
                    reader.ReadLine();
                    sortedLines++;
                }
            }

            Console.WriteLine("End stream. Elapsed: {0} ms", (DateTime.Now - startStreaming).TotalMilliseconds);

            Console.WriteLine("All acounted for: {0}.  Expected: {1}, actual: {2}", sortedLines == lines, lines, sortedLines);
        }

        private static void TryLargeFileStreaming()
        {
            string fileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "largeFile_guidsManyValues.txt";

            var serviceClient = new ServiceReference1.StringStreamServiceClient();

            var guid = serviceClient.BeginStream();

            long lines = 0;
            Console.WriteLine("StartUpload");
            var startUpload = DateTime.Now;
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

            Console.WriteLine("End upload. Elapsed: {0} ms", (DateTime.Now - startUpload).TotalMilliseconds);

            var stream = serviceClient.GetSortedStream(guid);

            long sortedLines = 0;

            Console.WriteLine("StartStreaming");
            var startStreaming = DateTime.Now;

            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    //Console.WriteLine(reader.ReadLine());
                    reader.ReadLine();
                    sortedLines++;
                }
            }

            Console.WriteLine("End stream. Elapsed: {0} ms", (DateTime.Now - startStreaming).TotalMilliseconds);

            Console.WriteLine("All acounted for: {0}.  Expected: {1}, actual: {2}", sortedLines == lines, lines, sortedLines);
        }


        private static void TrySimpleStreaming()
        {
            var serviceClient = new ServiceReference1.StringStreamServiceClient();

            var guid = serviceClient.BeginStream();

            List<string> strings = new List<string>();

            for (int i = 0; i < 10; i++)
            //for (int i = 0; i < 1000; i++)
            {
                //var guidString = Guid.NewGuid().ToString();
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
