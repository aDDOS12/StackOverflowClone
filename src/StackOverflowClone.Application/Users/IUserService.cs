namespace StackOverflowClone.Application.Users;

public interface IUserService
{
    Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken);
    Task<CurrentUserResponse> GetCurrentAsync(CancellationToken cancellationToken);
    Task DeleteAccountAsync(DeleteAccountRequest request , CancellationToken cancellationToken);
}
