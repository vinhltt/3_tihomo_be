namespace Identity.Api.Services;

public interface ITokenVerificationService
{
    Task<SocialUserInfo?> VerifyGoogleTokenAsync(string token);
    Task<SocialUserInfo?> VerifyFacebookTokenAsync(string token);
    Task<SocialUserInfo?> VerifyTokenAsync(string provider, string token);
}

public class SocialUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public string Provider { get; set; } = string.Empty;
}