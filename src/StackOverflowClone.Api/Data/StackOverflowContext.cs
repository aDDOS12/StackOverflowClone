using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Api.Entities;
using StackOverflowClone.Api.Data.Seed;

namespace StackOverflowClone.Api.Data;

public class StackOverflowContext(DbContextOptions<StackOverflowContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<QuestionVote> QuestionVotes { get; set; }
    public DbSet<AnswerVote> AnswerVotes { get; set; }
    public DbSet<CommentVote> CommentVotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Question>(eb =>
        {
            eb.Property(q => q.Title).HasMaxLength(150);
            eb.Property(q => q.Content).HasMaxLength(30000);

            eb.HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

            eb.HasMany(q => q.Comments)
            .WithOne(c => c.Question)
            .HasForeignKey(c => c.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

            eb.HasMany(q => q.QuestionVotes)
            .WithOne(qv => qv.Question)
            .HasForeignKey(qv => qv.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

            eb.HasMany(q => q.Tags)
            .WithMany(t => t.Questions)
            .UsingEntity<QuestionTag>(
                q => q.HasOne(qt => qt.Tag)
                .WithMany()
                .HasForeignKey(qt => qt.TagId),

                q => q.HasOne(qt => qt.Question)
                .WithMany()
                .HasForeignKey(qt => qt.QuestionId),

                qt =>
                {
                    qt.ToTable("QuestionTags");
                    qt.HasKey(x => new { x.TagId, x.QuestionId });
                    qt.HasQueryFilter(QueryFilterNames.SoftDelete, x => x.Question.DeletedAt == null);
                }
                );

            eb.HasQueryFilter(QueryFilterNames.SoftDelete, q => q.DeletedAt == null);
        });

        modelBuilder.Entity<Answer>(eb =>
        {
            eb.Property(a => a.Content).HasMaxLength(30000);

            eb.HasMany(a => a.Comments)
            .WithOne(c => c.Answer)
            .HasForeignKey(c => c.AnswerId)
            .OnDelete(DeleteBehavior.Restrict);

            eb.HasMany(a => a.AnswerVotes)
            .WithOne(av => av.Answer)
            .HasForeignKey(av => av.AnswerId)
            .OnDelete(DeleteBehavior.Restrict);

            eb.HasQueryFilter(QueryFilterNames.SoftDelete, a => a.DeletedAt == null);
        });

        modelBuilder.Entity<Comment>(eb =>
        {
            eb.Property(c => c.Content).HasMaxLength(600);

            eb.HasMany(c => c.CommentVotes)
            .WithOne(cv => cv.Comment)
            .HasForeignKey(cv => cv.CommentId)
            .OnDelete(DeleteBehavior.Restrict);

            eb.ToTable(c => c.HasCheckConstraint(
                "CK_Comments_ExactlyOneParent",
                "([QuestionId] IS NOT NULL AND [AnswerId] IS NULL) OR ([QuestionId] IS NULL AND [AnswerId] IS NOT NULL)"));

            eb.HasQueryFilter(QueryFilterNames.SoftDelete, q => q.DeletedAt == null);
        });

        modelBuilder.Entity<User>(eb =>
        {
            eb.Property(u => u.Username).HasMaxLength(30);
            eb.Property(u => u.Email).HasMaxLength(254);
            eb.Property(u => u.PasswordHash).HasMaxLength(256);

            eb.HasMany(u => u.Questions)
            .WithOne(q => q.User)
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            eb.HasMany(u => u.Answers)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            eb.HasMany(u => u.Comments)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            eb.HasMany(u => u.QuestionVotes)
            .WithOne(qv => qv.User)
            .HasForeignKey(qv => qv.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            eb.HasMany(u => u.AnswerVotes)
            .WithOne(av => av.User)
            .HasForeignKey(av => av.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            eb.HasMany(u => u.CommentVotes)
            .WithOne(cv => cv.User)
            .HasForeignKey(cv => cv.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AnswerVote>(eb =>
        {
            eb.HasKey(av => new { av.UserId, av.AnswerId });
            eb.HasQueryFilter(QueryFilterNames.SoftDelete, av => av.Answer.DeletedAt == null);
        });

        modelBuilder.Entity<QuestionVote>(eb =>
        {
            eb.HasKey(qv => new { qv.UserId, qv.QuestionId });
            eb.HasQueryFilter(QueryFilterNames.SoftDelete, qv => qv.Question.DeletedAt == null);
        });

        modelBuilder.Entity<CommentVote>(eb =>
        {
            eb.HasKey(cv => new { cv.UserId, cv.CommentId });
            eb.HasQueryFilter(QueryFilterNames.SoftDelete, cv => cv.Comment.DeletedAt == null);
        });

        modelBuilder.Entity<Tag>(eb =>
        {
            eb.Property(t => t.TagName).HasMaxLength(35);
            eb.HasIndex(t => t.TagName).IsUnique();

            eb.HasData(TagSeed.GetTags());
        });
    }
}
