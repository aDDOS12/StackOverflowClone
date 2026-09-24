using Microsoft.EntityFrameworkCore;

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
            .WithOne(q => q.Question)
            .HasForeignKey(q => q.QuestionId);

            eb.HasMany(q => q.Comments)
            .WithOne(q => q.Question)
            .HasForeignKey(q => q.QuestionId);

            eb.HasMany(q => q.QuestionVotes)
            .WithOne(q => q.Question)
            .HasForeignKey(q => q.QuestionId);

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
        });

        modelBuilder.Entity<Answer>(eb =>
        {
            eb.Property(a => a.Content).HasColumnType("nvarchar(max)");
            eb.Property(a => a.Score).HasDefaultValue(0);
            eb.Property(a => a.IsAccepted).HasDefaultValue(false);
            eb.Property(a => a.CreatedAt).HasDefaultValueSql("getutcdate()");
            eb.Property(a => a.UpdatedAt).ValueGeneratedOnUpdate();

            eb.HasMany(a => a.Comments)
            .WithOne(a => a.Answer)
            .HasForeignKey(a => a.AnswerId);

            eb.HasMany(a => a.AnswerVotes)
            .WithOne(a => a.Answer)
            .HasForeignKey(a => a.AnswerId);
        });

        modelBuilder.Entity<Comment>(eb =>
        {
            eb.Property(c => c.Content).HasColumnType("nvarchar(600)");
            eb.Property(c => c.Score).HasDefaultValue(0);
            eb.Property(c => c.CreatedAt).HasDefaultValueSql("getutcdate");
            eb.Property(c => c.UpdatedAt).ValueGeneratedOnUpdate();

            eb.HasMany(c => c.CommentVotes)
            .WithOne(c => c.Comment)
            .HasForeignKey(c => c.CommentId);
        });

        modelBuilder.Entity<User>(eb =>
        {
            eb.Property(u => u.Username).HasColumnType("varchar(30)");
            eb.Property(u => u.Email).HasColumnType("varchar(254)");
            eb.Property(u => u.PasswordHash).HasColumnType("nvarchar(256)");
            eb.Property(u => u.AccountCreated).HasDefaultValueSql("getutcdate()");

            eb.HasMany(u => u.Questions)
            .WithOne(u => u.User)
            .HasForeignKey(u => u.UserId);

            eb.HasMany(u => u.Answers)
            .WithOne(u => u.User)
            .HasForeignKey(u => u.UserId);

            eb.HasMany(u => u.Comments)
            .WithOne(u => u.User)
            .HasForeignKey(u => u.UserId);

            eb.HasMany(u => u.QuestionVotes)
            .WithOne(u => u.User)
            .HasForeignKey(u => u.UserId);

            eb.HasMany(u => u.AnswerVotes)
            .WithOne(u => u.User)
            .HasForeignKey(u => u.UserId);

            eb.HasMany(u => u.CommentVotes)
            .WithOne(u => u.User)
            .HasForeignKey(u => u.UserId);
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
            eb.HasData(new Tag() { },
                new Tag() { },
                new Tag() { },
                new Tag() { },
                new Tag() { },
                new Tag() { },
                new Tag() { },
                new Tag() { },
                new Tag() { },
                new Tag() { });
        });
    }
}
