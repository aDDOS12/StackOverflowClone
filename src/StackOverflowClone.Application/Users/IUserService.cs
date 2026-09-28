namespace StackOverflowClone.Application.Users;

public interface IUserService
{
    Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken);
}
