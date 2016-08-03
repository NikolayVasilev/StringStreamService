using StringStreamService.Consumer.ServiceReference1;
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
            var serviceClient = new ServiceReference1.StringStreamServiceClient();

            var guid = serviceClient.BeginStream();

            List<string> strings = new List<string>();

            for (int i = 0; i < 92345; i++)
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
