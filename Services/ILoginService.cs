using AuthenticationService.Models;

namespace AuthenticationService.Services
{
    public interface ILoginService
    {
        RespondModel GetLoginUser(string? userId, string? password);
    }
}
