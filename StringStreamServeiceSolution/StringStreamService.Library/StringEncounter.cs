using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringStreamService.Engine
{
    public class StringEncounter
    {
        public StringEncounter(string line)
        {
            var firstSpaceIndex = line.IndexOf(" ");

            var countString = line.Substring(0, firstSpaceIndex);

            this.Count = long.Parse(countString);
            this.String = line.Substring(firstSpaceIndex + 1);
        }

        public long Count { get; set; }
        public string String { get; set; }
    }
}
