using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringStreamService.Engine
{
    public interface ISessionWorker
    {
        Guid Id { get; }

        void Clear();
        Stream GetSortedStream();
        string[] GetSortedTextFull();
        void Process(string[] text);
    }
}
