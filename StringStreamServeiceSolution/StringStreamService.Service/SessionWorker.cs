﻿using StringStreamService.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringStreamService.Service
{
    internal class SessionWorker : ISessionWorker
    {
        public Guid Id { get; private set; }

        private readonly TextProcessor processor;

        internal SessionWorker()
        {
            this.Id = Guid.NewGuid();
            this.processor = new TextProcessor(this.Id);
        }

        public void Clear()
        {
            this.processor.Clear();
        }

        public void Process(string[] text)
        {
            foreach (var line in text)
            {
                this.processor.AppendLine(line);
            }
        }

        public Stream GetSortedStream()
        {
            return this.processor.GetSortedStream();
        }

        public string[] GetSortedTextFull()
        {
            return this.processor.GetSortedTextFull();
        }
    }
}
