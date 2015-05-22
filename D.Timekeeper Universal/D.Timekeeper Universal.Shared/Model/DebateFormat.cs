using System;
using System.Collections.Generic;
using System.Text;

namespace D.Timekeeper_Universal.Model
{
    public class DebateFormat
    {
        public String name { get; set; }
        public List<Speaker> speakers { get; set; }

        public DebateFormat(String name, List<Speaker> speakers)
        {
            this.name = name;
            this.speakers = speakers;
        }
    }
}
