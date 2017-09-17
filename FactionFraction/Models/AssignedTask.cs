using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FactionFraction.Models
{
    public enum Completion
    {
        ToDo, InProgress, Completed
    }
    public class AssignedTask
    {
       
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public Completion Completion { get; set; }
        public int EstimatedMinutes { get; set; }
        public GroupMember GroupMember { get; set; }
        public string AspNetUserId { get; set; }
    }
}