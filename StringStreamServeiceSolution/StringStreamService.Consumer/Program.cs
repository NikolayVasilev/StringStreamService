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

            Console.WriteLine(serviceClient.GetSortedStreamFull(guid).FirstOrDefault());

            List<string> strings = new List<string>();

            for (int i = 0; i < 105000; i++)
            {
                var guidString = Guid.NewGuid().ToString();

                strings.Add(guidString);
            }

            for (int i = 0; i < 105000; i++)
            {
                var guidString = Guid.NewGuid().ToString();

                strings.Add(guidString);
            }

            serviceClient.PutStreamData(guid, strings.ToArray());

            serviceClient.EndStream(guid);

            //Stream result = serviceClient.GetSortedStream(guid);

            serviceClient.Close();
        }
    }
}
