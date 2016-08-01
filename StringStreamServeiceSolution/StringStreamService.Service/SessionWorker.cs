using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringStreamService.Service
{
    internal class SessionWorker
    {
        internal Guid Id { get; private set; }

        internal SessionWorker()
        {
            this.Id = Guid.NewGuid();
        }

        internal void Clear()
        {
        }

        internal void Process(string[] text)
        {

        }
    }
}
