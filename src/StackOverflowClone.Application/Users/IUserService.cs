namespace StackOverflowClone.Application.Users;

public interface IUserService
{
    Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken);
    Task<CurrentUserResponse> GetCurrentAsync(Guid userId, CancellationToken cancellationToken);
    Task DeleteAccountAsync(Guid userId, DeleteAccountRequest request , CancellationToken cancellationToken);
}
