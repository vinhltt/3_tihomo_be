using Identity.Domain.Dtos.Authentication;

namespace Identity.Application.Common.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleUserInfo> VerifyGoogleTokenAsync(string idToken);
}