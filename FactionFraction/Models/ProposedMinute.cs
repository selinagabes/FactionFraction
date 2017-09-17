using System.Collections.Generic;

namespace FactionFraction.Models
{
    public class SuggestedMinute
    {      
        public int GroupMemberId { get; set; }
        public int TaskId { get; set; }
        public int Length { get; set; }
    }

    public class EstimateViewModel
    {
        public EstimateViewModel()
        {
            TaskTitles = new List<string>();
            GroupMemberNames = new List<string>();
            ProposedMinutes = new List<SuggestedMinute>();
        }
        public List<string> GroupMemberNames { get; set; }
        public List<string> TaskTitles { get; set; }
        public List<SuggestedMinute> ProposedMinutes { get; set; }

    }
}