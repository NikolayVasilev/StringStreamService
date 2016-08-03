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
        private static Dictionary<Guid, ISessionWorker> sessionWorkers = new Dictionary<Guid, ISessionWorker>();

        public Guid BeginStream()
        {
            var newSessionWorker = new SessionWorker();

            sessionWorkers.Add(newSessionWorker.Id, newSessionWorker);

            return newSessionWorker.Id;
        }

        public void EndStream(Guid streamId)
        {
            var sessionWorker = this.GetSessionWorker(streamId);

            if (sessionWorker != null)
            {
                sessionWorker.Clear();
                sessionWorkers.Remove(sessionWorker.Id);
            }
        }

        public Stream GetSortedStream(Guid streamId)
        {
            var sessionWorker = this.GetSessionWorker(streamId);

            if (sessionWorker == null)
            {
                throw new ArgumentException("No Stream found for Id: " + streamId);
            }

            return sessionWorker.GetSortedStream();
        }

        public string[] GetSortedStreamFull(Guid streamId)
        {
            var sessionWorker = this.GetSessionWorker(streamId);

            if (sessionWorker == null)
            {
                throw new ArgumentException("No Stream found for Id: " + streamId);
            }

            return sessionWorker.GetSortedTextFull();
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

        private ISessionWorker GetSessionWorker(Guid streamId)
        {
            return sessionWorkers.ContainsKey(streamId) ? sessionWorkers[streamId] : null;
        }
    }
}
