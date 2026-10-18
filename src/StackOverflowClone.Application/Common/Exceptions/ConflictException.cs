namespace StackOverflowClone.Application.Common.Exceptions;

public sealed class ConflictException(string message, string? errorCode = null) : Exception(message)
{
    public string? ErrorCode { get; } = errorCode;
}
