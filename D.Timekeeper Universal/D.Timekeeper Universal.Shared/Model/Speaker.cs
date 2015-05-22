using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace D.Timekeeper_Universal.Model
{
    public class Speaker
    {
        public String name { get; set; }
        // Key   : Time in seconds a ring should be made.
        // Value : How many times it should ring.
        public List<KeyValuePair<int, int>> ringPairs { get; set; }

        public Speaker(String name, List<KeyValuePair<int, int>> ringPairs)
        {
            this.name = name;
            this.ringPairs = ringPairs;
        }

        public override string ToString()
        {
            return this.name;
        }
    }
}
