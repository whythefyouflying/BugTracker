using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker_API.Models
{
    public class IssueDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
