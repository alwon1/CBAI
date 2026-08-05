using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace CBAI.Tests.Shared;

/// <summary>
/// Helpers for building fake authentication state for bUnit tests that render components
/// behind <c>[Authorize]</c> or that use <c>&lt;AuthorizeView&gt;</c>.
/// </summary>
public static class TestAuthenticationStateHelpers
{
    public static ClaimsPrincipal CreatePrincipal(string userName, params string[] roles)
    {
        var identity = new ClaimsIdentity(authenticationType: "TestAuth");
        identity.AddClaim(new Claim(ClaimTypes.Name, userName));

        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(identity);
    }

    public static ClaimsPrincipal CreateAnonymousPrincipal()
        => new(new ClaimsIdentity());

    /// <summary>
    /// A minimal <see cref="AuthenticationStateProvider"/> that always reports a fixed
    /// user (or anonymous, when no user name is supplied) for use with bUnit's
    /// <c>TestContext.Services.AddSingleton&lt;AuthenticationStateProvider&gt;</c>.
    /// </summary>
    public sealed class FakeAuthenticationStateProvider(string? userName = null, params string[] roles) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var principal = userName is null
                ? CreateAnonymousPrincipal()
                : CreatePrincipal(userName, roles);

            return Task.FromResult(new AuthenticationState(principal));
        }
    }
}
