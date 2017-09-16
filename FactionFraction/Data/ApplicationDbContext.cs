using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FactionFraction.Models;

namespace FactionFraction.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<AssignedTask> AssignedTasks { get; set; }
        public DbSet<ProposedMinute> ProposedMinutes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

          

            builder.Entity<GroupMember>()
                .HasMany(a => a.AssignedTasks)
                .WithOne(g => g.GroupMember);

            builder.Entity<GroupMember>()
                .HasMany(g => g.ProposedMinutes)
                .WithOne(m => m.GroupMember)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); //Delete proposed mintues if a task is deleted

            builder.Entity<AssignedTask>()
                .HasMany(m => m.ProposedMinutes)
                .WithOne(a => a.AssignedTask)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade); //Delete proposed mintues if a task is deleted
            
        }
    }
}
