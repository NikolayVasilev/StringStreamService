using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace StringStreamService.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class StringStreamService : IStringStreamService
    {
        public Guid BeginStream()
        {
            throw new NotImplementedException();
        }

        public void EndStream(Guid streamId)
        {
            throw new NotImplementedException();
        }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public string[] GetSortedStream(Guid streamId)
        {
            return new string[] { string.Format("You entered: {0}", streamId) };
        }

        public void PutStreamData(Guid streamId, string[] text)
        {
            throw new NotImplementedException();
        }
    }
}
