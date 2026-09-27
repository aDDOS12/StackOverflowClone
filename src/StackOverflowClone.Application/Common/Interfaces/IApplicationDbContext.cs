using Microsoft.EntityFrameworkCore;
using StackOverflowClone.Domain.Entities;

namespace StackOverflowClone.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Question> Questions { get; }
    DbSet<Answer> Answers { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Tag> Tags { get; }
    DbSet<QuestionVote> QuestionVotes { get; }
    DbSet<AnswerVote> AnswerVotes { get; }
    DbSet<CommentVote> CommentVotes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
