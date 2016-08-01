using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace StringStreamService.Service
{
    public class StringStreamService : IStringStreamService
    {
        private static List<SessionWorker> sessionWorkers = new List<SessionWorker>();

        public Guid BeginStream()
        {
            var newSessionWorker = new SessionWorker();

            sessionWorkers.Add(newSessionWorker);

            return newSessionWorker.Id;
        }

        public void EndStream(Guid streamId)
        {
            var sessionWorker = this.GetSessionWorker(streamId);

            if (sessionWorker != null)
            {
                sessionWorker.Clear();
                sessionWorkers.Remove(sessionWorker);
            }
        }

        public Stream GetSortedStream(Guid streamId)
        {
            var sessionWorker = this.GetSessionWorker(streamId);

            if (sessionWorker == null)
            {
                throw new ArgumentException("No Stream found for Id: " + streamId);
            }

            return Stream.Null;
        }

        public string[] GetSortedStreamFull(Guid streamId)
        {
            var sessionWorker = this.GetSessionWorker(streamId);

            if (sessionWorker == null)
            {
                throw new ArgumentException("No Stream found for Id: " + streamId);
            }

            return new string[] { string.Format("You entered: {0}", streamId) };
        }

        public void PutStreamData(Guid streamId, string[] text)
        {
            var sessionWorker = this.GetSessionWorker(streamId);

            if (sessionWorker == null)
            {
                throw new ArgumentException("No Stream found for Id: " + streamId);
            }

            sessionWorker.Process(text);
        }

        private SessionWorker GetSessionWorker(Guid streamId)
        {
            return sessionWorkers.FirstOrDefault(sw => sw.Id == streamId);
        }
    }
}
