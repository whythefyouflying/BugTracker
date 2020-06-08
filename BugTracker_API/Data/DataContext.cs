using Microsoft.EntityFrameworkCore;

namespace BugTracker_API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }

        public DbSet<Issue> Issues { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .HasMany(project => project.Issues)
                .WithOne(issue => issue.Project)
                .IsRequired();

            modelBuilder.Entity<Issue>()
                .HasMany(i => i.Comments)
                .WithOne(c => c.Issue)
                .IsRequired();
        }
    }
}
