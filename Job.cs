using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Web_Crawler {

    [DebuggerDisplay("{Title}, {Company}, {Salary}")]
    class Job {

        public string Title { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public string Salary { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }

    }
}
