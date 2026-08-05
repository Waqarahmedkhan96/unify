using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Unify.Erp.Application.Auth;
using Unify.Erp.Contracts.Auth;
using Unify.Erp.Infrastructure.Identity;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Auth;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;
    private readonly JwtTokenFactory _jwtTokenFactory;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthenticationService(
        ApplicationDbContext dbContext,
        IOptions<JwtOptions> jwtOptions,
        JwtTokenFactory jwtTokenFactory,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
        _jwtTokenFactory = jwtTokenFactory;
        _userManager = userManager;
    }

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return AuthenticationResult.Failure(AuthenticationError.InvalidCredentials);
        }

        if (user.IsDisabled)
        {
            return AuthenticationResult.Failure(AuthenticationError.DisabledUser);
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return AuthenticationResult.Failure(AuthenticationError.InvalidCredentials);
        }

        return await CreateSessionAsync(user, request.OrganisationId, request.DeviceId, Guid.NewGuid(), cancellationToken);
    }

    public async Task<AuthenticationResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenHasher.Hash(request.RefreshToken);
        var nowUtc = DateTimeOffset.UtcNow;

        var existingToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null)
        {
            return AuthenticationResult.Failure(AuthenticationError.InvalidRefreshToken);
        }

        if (existingToken.RevokedAtUtc is not null)
        {
            await RevokeTokenFamilyAsync(existingToken.FamilyId, nowUtc, cancellationToken);

            return AuthenticationResult.Failure(AuthenticationError.ReusedRefreshToken);
        }

        if (existingToken.ExpiresAtUtc <= nowUtc)
        {
            return AuthenticationResult.Failure(AuthenticationError.ExpiredRefreshToken);
        }

        var user = await _userManager.FindByIdAsync(existingToken.UserId.ToString());
        if (user is null)
        {
            return AuthenticationResult.Failure(AuthenticationError.InvalidRefreshToken);
        }

        if (user.IsDisabled)
        {
            return AuthenticationResult.Failure(AuthenticationError.DisabledUser);
        }

        var result = await CreateSessionAsync(
            user,
            existingToken.OrganisationId,
            existingToken.DeviceId,
            existingToken.FamilyId,
            cancellationToken);

        if (result.Tokens is null)
        {
            return result;
        }

        var replacement = await _dbContext.RefreshTokens
            .SingleAsync(token => token.TokenHash == RefreshTokenHasher.Hash(result.Tokens.RefreshToken), cancellationToken);

        existingToken.Revoke(nowUtc, replacement.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenHasher.Hash(refreshToken);
        var nowUtc = DateTimeOffset.UtcNow;
        var existingToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null || existingToken.RevokedAtUtc is not null)
        {
            return;
        }

        existingToken.Revoke(nowUtc, null);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task LogoutAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var activeTokens = await _dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(nowUtc, null);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AuthSessionResponse>> ListSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTimeOffset.UtcNow;

        return await _dbContext.RefreshTokens
            .Where(token => token.UserId == userId)
            .OrderByDescending(token => token.CreatedAtUtc)
            .Select(token => new AuthSessionResponse(
                token.Id,
                token.OrganisationId,
                token.DeviceId,
                token.CreatedAtUtc,
                token.ExpiresAtUtc,
                token.RevokedAtUtc == null && token.ExpiresAtUtc > nowUtc))
            .ToListAsync(cancellationToken);
    }

    private async Task<AuthenticationResult> CreateSessionAsync(
        ApplicationUser user,
        Guid? organisationId,
        Guid? deviceId,
        Guid familyId,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var accessToken = _jwtTokenFactory.CreateAccessToken(user, organisationId, deviceId, nowUtc);
        var refreshToken = RefreshTokenGenerator.CreateToken();
        var refreshTokenExpiresAtUtc = nowUtc.AddDays(_jwtOptions.RefreshTokenDays);
        var refreshTokenRecord = new RefreshTokenRecord(
            Guid.NewGuid(),
            user.Id,
            organisationId,
            deviceId,
            familyId,
            RefreshTokenHasher.Hash(refreshToken),
            nowUtc,
            refreshTokenExpiresAtUtc);

        _dbContext.RefreshTokens.Add(refreshTokenRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return AuthenticationResult.Success(new AuthTokenResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshToken,
            refreshTokenExpiresAtUtc));
    }

    private async Task RevokeTokenFamilyAsync(Guid familyId, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var familyTokens = await _dbContext.RefreshTokens
            .Where(token => token.FamilyId == familyId && token.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in familyTokens)
        {
            token.Revoke(nowUtc, null);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
