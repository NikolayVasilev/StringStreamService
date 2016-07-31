using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringStreamService.Library
{
    public interface IStringStreamService
    {
        Guid BeginStream();
        void PutStreamData(Guid streamId, string[] text);
        string[] GetSortedStream(Guid streamId);
        void EndStream(Guid streamId);
    }
}
