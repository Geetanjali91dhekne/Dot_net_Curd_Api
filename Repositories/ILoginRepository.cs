using AuthenticationService.Models;

namespace AuthenticationService.Repositories
{
    public interface ILoginRepository
    {
        RespondModel GetLoginUser(string? userId, string? password);
    }
}
