using TestToken.Models;

namespace TestToken.Repositories.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
        RefreshToken GenerateRefreshToken();
    }
}
