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

            //Console.WriteLine(serviceClient.GetSortedStreamFull(guid).FirstOrDefault());

            List<string> strings = new List<string>();

            for (int i = 0; i < 1000000; i++)
            {
                //var guidString = Guid.NewGuid().ToString();
                var guidString = i.ToString();

                strings.Add(guidString);
            }

            serviceClient.PutStreamData(guid, strings.ToArray());

            string[] sorted = serviceClient.GetSortedStreamFull(guid);

            //foreach (var str in sorted)
            //{
                Console.WriteLine(sorted.Count());
            //}

            serviceClient.EndStream(guid);

            serviceClient.Close();
        }

        static async Task GetSorted(StringStreamServiceClient clent)
        {

        }
    }
}
