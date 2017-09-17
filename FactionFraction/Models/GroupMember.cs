using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FactionFraction.Models
{
    public class GroupMember
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public float DesiredGrade { get; set; }
        public float FinalGrade { get; set; }
        public ICollection<AssignedTask> AssignedTasks { get; set; }
        public string AspNetUserId { get; set; }
        [EmailAddress]
        public string Email { get; set; }
    }
}
