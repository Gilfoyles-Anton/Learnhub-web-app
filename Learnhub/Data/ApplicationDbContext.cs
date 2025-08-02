using Learnhub.Models;
using Microsoft.EntityFrameworkCore;

namespace Learnhub.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<AdminActivationCode> AdminActivationCodes { get; set; }
    public DbSet<Course> Courses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Course>()
            .Property(c => c.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<User>()
            .Property(u => u.WalletBalance)
            .HasPrecision(18, 2);
    }

    public DbSet<UserCourseInteraction> UserCourseInteractions { get; set; }
    

    public DbSet<WalletActivationCode> WalletActivationCodes { get; set; }

    public DbSet<ForumTopic> ForumTopics { get; set; }

    public DbSet<ComplaintTopic> ComplaintTopics { get; set; }

    public DbSet<CourseComment> CourseComments { get; set; }
    public DbSet<CourseCommentLike> CourseCommentLikes { get; set; }

    public DbSet<ForumComment> ForumComments { get; set; }
    public DbSet<ForumCommentLike> ForumCommentLikes { get; set; }

    public DbSet<UserForumTopicInteraction> UserForumTopicInteractions { get; set; }

    public DbSet<ComplaintComment> ComplaintComments { get; set; }
    public DbSet<ComplaintCommentLike> ComplaintCommentLikes { get; set; }
    public DbSet<UserComplaintTopicInteraction> UserComplaintTopicInteractions { get; set; }






}


