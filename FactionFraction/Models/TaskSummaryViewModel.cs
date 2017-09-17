using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactionFraction.Models
{
    public class TaskSummaryViewModel
    {
        public List<GroupMember> GroupMembers { get; set; }
        public List<AssignedTask> AssignedTasks { get; set; }
    }
}
