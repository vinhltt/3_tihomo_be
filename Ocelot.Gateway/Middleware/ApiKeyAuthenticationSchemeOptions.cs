using Microsoft.AspNetCore.Authentication;

namespace Ocelot.Gateway.Middleware
{
    public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ApiKey";
    }
}