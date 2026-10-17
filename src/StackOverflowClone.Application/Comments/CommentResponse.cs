using StackOverflowClone.Application.Common.Models;

namespace StackOverflowClone.Application.Comments;

public sealed record CommentResponse(
    int Id,
    string Content,
    int Score,
    DateTime CreatedAtUtc,
    AuthorDto Author);
