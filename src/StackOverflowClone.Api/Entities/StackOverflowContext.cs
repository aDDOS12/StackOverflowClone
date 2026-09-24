using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Api.Data;
using StackOverflowClone.Api.Data.Seed;

namespace StackOverflowClone.Api.Entities;

public class StackOverflowContext : DbContext
{
    public StackOverflowContext(DbContextOptions<StackOverflowContext> options) : base(options)
    {
        
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<QuestionVote> QuestionVote { get; set; }
    public DbSet<AnswerVote> AnswerVote { get; set; }
    public DbSet<CommentVote> CommentVote { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Question>(eb =>
        {
            eb.Property(q => q.Title).HasColumnType("nvarchar(150)");
            eb.Property(q => q.Content).HasColumnType("nvarchar(max)");
            eb.Property(q => q.Score).HasDefaultValue(0);
            eb.Property(q => q.CreatedAt).HasDefaultValueSql("getutcdate()");
            eb.Property(q => q.UpdatedAt).ValueGeneratedOnUpdate();

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
                    qt.HasKey(x => new { x.TagId, x.QuestionId });
                }
                );

            eb.HasQueryFilter(QueryFilterNames.SoftDelete, q => q.DeletedAt == null);
        });

        modelBuilder.Entity<Answer>(eb =>
        {
            eb.Property(a => a.Content).HasColumnType("nvarchar(max)");
            eb.Property(a => a.Score).HasDefaultValue(0);
            eb.Property(a => a.IsAccepted).HasDefaultValue(false);
            eb.Property(a => a.CreatedAt).HasDefaultValueSql("getutcdate()");
            eb.Property(a => a.UpdatedAt).ValueGeneratedOnUpdate();

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
            eb.Property(c => c.Content).HasColumnType("nvarchar(600)");
            eb.Property(c => c.Score).HasDefaultValue(0);
            eb.Property(c => c.CreatedAt).HasDefaultValueSql("getutcdate()");
            eb.Property(c => c.UpdatedAt).ValueGeneratedOnUpdate();

            eb.HasMany(c => c.CommentVotes)
            .WithOne(cv => cv.Comment)
            .HasForeignKey(cv => cv.CommentId)
            .OnDelete(DeleteBehavior.Restrict);

            eb.ToTable(c => c.HasCheckConstraint(
                "CK_Comments_ExactlyOneParent",
                "([QuestionId] IS NOT NULL AND [AnswerId] IS NULL) OR ([QuestionId] IS NULL AND [AnswerId] IS NOT NULL"));

            eb.HasQueryFilter(QueryFilterNames.SoftDelete, q => q.DeletedAt == null);
        });

        modelBuilder.Entity<User>(eb =>
        {
            eb.Property(u => u.Username).HasColumnType("varchar(30)");
            eb.Property(u => u.Email).HasColumnType("varchar(254)");
            eb.Property(u => u.PasswordHash).HasColumnType("nvarchar(256)");
            eb.Property(u => u.AccountCreated).HasDefaultValueSql("getutcdate()");

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
        });

        modelBuilder.Entity<QuestionVote>(eb =>
        {
            eb.HasKey(qv => new { qv.UserId, qv.QuestionId });
        });

        modelBuilder.Entity<CommentVote>(eb =>
        {
            eb.HasKey(cv => new { cv.UserId, cv.CommentId });
        });

        modelBuilder.Entity<Tag>(eb =>
        {
            eb.Property(t => t.TagName).HasMaxLength(35);
            eb.HasIndex(t => t.TagName).IsUnique();

            eb.HasData(TagSeed.GetTags());
        });
    }
}
