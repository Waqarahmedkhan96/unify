using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Unify.Erp.Contracts.Auth;
using Unify.Erp.Infrastructure.Identity;

namespace Unify.Erp.Infrastructure.Auth;

public sealed class JwtTokenFactory
{
    private readonly JwtOptions _options;
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtTokenFactory(IOptions<JwtOptions> options, UserManager<ApplicationUser> userManager)
    {
        _options = options.Value;
        _userManager = userManager;
    }

    public async Task<(string Token, DateTimeOffset ExpiresAtUtc)> CreateAccessTokenAsync(
        ApplicationUser user,
        Guid? organisationId,
        Guid? deviceId,
        DateTimeOffset nowUtc)
    {
        var expiresAtUtc = nowUtc.AddMinutes(_options.AccessTokenMinutes);
        var userClaims = await _userManager.GetClaimsAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(userClaims.Where(claim => claim.Type == PermissionNames.ClaimType));

        if (organisationId.HasValue)
        {
            claims.Add(new Claim("organisation_id", organisationId.Value.ToString()));
        }

        if (deviceId.HasValue)
        {
            claims.Add(new Claim("device_id", deviceId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            nowUtc.UtcDateTime,
            expiresAtUtc.UtcDateTime,
            credentials);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expiresAtUtc);
    }
}
