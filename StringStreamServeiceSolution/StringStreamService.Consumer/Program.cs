using System;
using System.Collections.Generic;
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

            var serviceClient = new StringStreamServiceNS.StringStreamServiceClient();

            Console.WriteLine(serviceClient.GetSortedStream(guid).FirstOrDefault());

            serviceClient.Close();
        }
    }
}
