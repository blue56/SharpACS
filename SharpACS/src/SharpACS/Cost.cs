using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpACS
{
    public class Cost
    {
        // E.g. AWS or Azure
        public string Service { get; set; }

        // Resource id
        public string ResourceId { get; set; }
        public string ResourceType { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        // Cost name is used to match the cost to a resource 
        public string Name { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; }

        // Final = true if the cost will not change. 
        // E.g. if the period has ended
        // The provider is the only one to decide if it is finale
        public bool Final { get; set; }

        public string Provider {get; set;}
    }
}
