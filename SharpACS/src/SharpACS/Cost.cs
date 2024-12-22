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
        // E.g. Clean Company Azure tenant
        public string Connection { get; set; }
        
        // E.g. AWS or Azure
        public string Service { get; set; }

        // Resource id
        public string ResourceId { get; set; }
        public string ResourceType { get; set; }

        public string Period { get; set; }

        // Cost name is used to match the cost to a resource 
        public string Name { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
