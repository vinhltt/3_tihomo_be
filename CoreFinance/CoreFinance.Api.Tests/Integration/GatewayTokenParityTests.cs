using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Tests.Shared.Helpers;

namespace CoreFinance.Api.Tests.Integration;

/// <summary>
///     Characterization: login through the gateway issues a JWT that protected gateway routes accept (EN)<br/>
///     Characterization: đăng nhập qua gateway phát JWT được route bảo vệ chấp nhận (VI)
/// </summary>
/// <remarks>
///     Baseline login does not verify passwords; no assertion pins or denies that. Only issuance/routing/validation.
/// </remarks>
[Trait("Category", "Characterization")]
public class GatewayTokenParityTests
{
    private const string ProtectedRoute = "/api/core-finance/Account/selections";

    [Fact]
    public async Task Login_Through_Gateway_Should_Issue_Token_With_Configured_Claims()
    {
        var username = ExternalBackend.NewUser("gateway");

        var token = new JwtSecurityTokenHandler().ReadJwtToken(await ExternalBackend.LoginThroughGatewayAsync(username));

        token.Issuer.Should().Be(ExternalBackend.Require("TIHOMO_TEST_JWT_ISSUER"));
        token.Audiences.Should().ContainSingle().Which.Should().Be(ExternalBackend.Require("TIHOMO_TEST_JWT_AUDIENCE"));
        token.ValidTo.Should().BeAfter(DateTime.UtcNow).And.BeBefore(DateTime.UtcNow.AddDays(1));
        Guid.TryParse(token.Claims.Single(c => c.Type == "nameid").Value, out _).Should().BeTrue();
        token.Claims.Single(c => c.Type == "email").Value.Should().Be(username);
    }

    [Fact]
    public async Task Protected_Gateway_Route_Should_Accept_Issued_Token()
    {
        var token = await ExternalBackend.LoginThroughGatewayAsync(ExternalBackend.NewUser("gateway"));
        using var gateway = ExternalBackend.Client("gateway");

        var response = await Send(gateway, token);

        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Protected_Gateway_Route_Should_Reject_Missing_Malformed_And_Expired_Tokens()
    {
        using var gateway = ExternalBackend.Client("gateway");

        (await Send(gateway, null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await Send(gateway, "abc.def.ghi")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await Send(gateway, ExpiredToken())).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static Task<HttpResponseMessage> Send(HttpClient client, string? token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, ProtectedRoute);
        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client.SendAsync(request);
    }

    /// <summary>Correctly signed but expired well beyond any clock skew.</summary>
    private static string ExpiredToken() => new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Guid.CreateVersion7().ToString())]),
        Issuer = ExternalBackend.Require("TIHOMO_TEST_JWT_ISSUER"),
        Audience = ExternalBackend.Require("TIHOMO_TEST_JWT_AUDIENCE"),
        NotBefore = DateTime.UtcNow.AddHours(-2),
        IssuedAt = DateTime.UtcNow.AddHours(-2),
        Expires = DateTime.UtcNow.AddHours(-1),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ExternalBackend.Require("TIHOMO_TEST_JWT_SECRET"))),
            SecurityAlgorithms.HmacSha256)
    });
}
