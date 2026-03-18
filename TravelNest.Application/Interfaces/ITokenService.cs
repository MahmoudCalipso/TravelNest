
using TravelNest.Domain.Entities;

namespace TravelNest.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateTokenAndGetUserId(string token);
}
