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
            var guid = Guid.NewGuid();

            var serviceClient = new ServiceReference1.StringStreamServiceClient();

            guid = serviceClient.BeginStream();

            Console.WriteLine(serviceClient.GetSortedStreamFull(guid).FirstOrDefault());

            serviceClient.EndStream(guid);


            //Stream result = serviceClient.GetSortedStream(guid);

            serviceClient.Close();
        }
    }
}
