using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using TuColmadoRD.Core.Application.DTOs.Security;
using TuColmadoRD.Core.Application.Interfaces.Security;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Infrastructure.CrossCutting.Security;

public class LicenseVerifierService : ILicenseVerifier
{
    private readonly IDeviceIdentityStore _identityStore;
    private readonly ITimeGuard _timeGuard;

    public LicenseVerifierService(
        IDeviceIdentityStore identityStore,
        ITimeGuard timeGuard)
    {
        _identityStore = identityStore;
        _timeGuard = timeGuard;
    }

    public async Task<OperationResult<LicenseStatus, SubscriptionError>> VerifyAsync()
    {
        var identityResult = _identityStore.Read();
        if (!identityResult.TryGetResult(out var identity))
        {
            return OperationResult<LicenseStatus, SubscriptionError>.Bad(SubscriptionError.LicenseNotFound);
        }

        if (string.IsNullOrWhiteSpace(identity!.LicenseToken))
        {
            return OperationResult<LicenseStatus, SubscriptionError>.Bad(SubscriptionError.LicenseNotFound);
        }

        using var rsa = RSA.Create();
        try
        {
            rsa.ImportFromPem(identity.PublicLicenseKey);
        }
        catch
        {
            return OperationResult<LicenseStatus, SubscriptionError>.Bad(SubscriptionError.InvalidSignature);
        }

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsa),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            tokenHandler.ValidateToken(identity.LicenseToken, validationParameters, out var validatedToken);
            
            if (validatedToken is not JwtSecurityToken jwt)
            {
                return OperationResult<LicenseStatus, SubscriptionError>.Bad(SubscriptionError.InvalidSignature);
            }

            var terminalIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "terminal_id")?.Value;
            var validUntilClaim = jwt.Claims.FirstOrDefault(c => c.Type == "valid_until")?.Value;

            if (terminalIdClaim != identity.TerminalId.ToString())
            {
                return OperationResult<LicenseStatus, SubscriptionError>.Bad(SubscriptionError.TerminalMismatch);
            }

            if (!long.TryParse(validUntilClaim, out var validUntilUnix))
            {
                return OperationResult<LicenseStatus, SubscriptionError>.Bad(SubscriptionError.InvalidSignature);
            }

            var validUntil = DateTimeOffset.FromUnixTimeSeconds(validUntilUnix).UtcDateTime;

            var advanceResult = await _timeGuard.AdvanceTimeAsync(DateTime.UtcNow);
            if (!advanceResult.IsGood)
            {
                advanceResult.TryGetError(out var timeError);
                return OperationResult<LicenseStatus, SubscriptionError>.Bad(timeError!);
            }

            if (DateTime.UtcNow > validUntil)
            {
                return OperationResult<LicenseStatus, SubscriptionError>.Bad(SubscriptionError.SubscriptionExpired);
            }

            return OperationResult<LicenseStatus, SubscriptionError>.Good(
                new LicenseStatus(true, validUntil, null));
        }
        catch
        {
            return OperationResult<LicenseStatus, SubscriptionError>.Bad(SubscriptionError.InvalidSignature);
        }
    }
}
