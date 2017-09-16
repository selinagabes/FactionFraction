namespace FactionFraction.Models
{
    public class ProposedMinute
    {
        public int Id { get; set; }
        public GroupMember GroupMember { get; set; }
        public AssignedTask AssignedTask { get; set; }
        public int Length { get; set; }
    }
}