// Controllers/SessionController.cs
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
[ApiController]
[Route("api/session")]
public class SessionController : ControllerBase
{
    private readonly ILogger<SessionController> _logger;
    private readonly string _tenantId;
    private readonly string _authority;
    private readonly string[] _expectedAudiences;
    private static readonly ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> _authorityMetadata = new();
    private readonly bool _debugBypassEnabled;
    private readonly string? _debugBypassSecret;

    public record SessionStartDto(string? token);

    public SessionController(ILogger<SessionController> logger, IConfiguration configuration)
    {
        _logger = logger;
        // Prefer hierarchical keys if you adopt them later (AAD:TenantId etc.)
        _tenantId = configuration["AAD_TENANT_ID"] ?? configuration["AAD:TenantId"] ?? string.Empty;
        var authorityConfig = configuration["AAD_AUTHORITY"] ?? configuration["AAD:Authority"];
        _authority = !string.IsNullOrWhiteSpace(authorityConfig)
            ? authorityConfig
            : (string.IsNullOrWhiteSpace(_tenantId) ? string.Empty : $"https://login.microsoftonline.com/{_tenantId}/v2.0");
        var audiencesRaw = configuration["EMBED_EXPECTED_AUDIENCES"] ?? configuration["Embed:ExpectedAudiences"] ?? string.Empty;
        _expectedAudiences = audiencesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    // Debug bypass configuration (optional)
    _debugBypassEnabled = bool.TryParse(configuration["Debug:EnableBypass"], out var be) && be;
    _debugBypassSecret = configuration["Debug:BypassSecret"];
        if (string.IsNullOrWhiteSpace(_authority))
            _logger.LogWarning("Authority is empty. Provide AAD_AUTHORITY or AAD_TENANT_ID in appsettings.");
        else
            _logger.LogInformation("SessionController configured authority {Authority} audiences {Audiences}", _authority, string.Join(',', _expectedAudiences));
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] SessionStartDto body)
    {
        var token = body?.token;
        if (string.IsNullOrWhiteSpace(token))
            return Unauthorized();

        _logger.LogDebug("/api/session/start invoked. Authority={Authority} Tenant={Tenant} AudienceCount={AudienceCount}", _authority, _tenantId, _expectedAudiences.Length);

        // Debug bypass: direct secret match (only when enabled)
        if (_debugBypassEnabled && !string.IsNullOrWhiteSpace(_debugBypassSecret) && token == _debugBypassSecret)
        {
            _logger.LogWarning("DEBUG BYPASS used to issue session cookie. Disable Debug:EnableBypass in production.");
            IssueSessionCookie("debug-user", "debug-tenant");
            return NoContent();
        }

        // Validate Entra ID (Azure AD) JWT
        try
        {
            var principal = await ValidateEntraJwtAsync(token);
            var sub = principal.FindFirst("sub")?.Value
                      ?? principal.FindFirst("oid")?.Value
                      ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? "unknown";
            var tid = principal.FindFirst("tid")?.Value ?? "unknown";

            IssueSessionCookie(sub, tid);
            return NoContent();
        }
        catch (SecurityTokenException ex)
        {
            return Unauthorized(new { error = "invalid_token", message = ex.Message });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { error = "token_validation_error", message = ex.Message });
        }
    }


    private ConfigurationManager<OpenIdConnectConfiguration> GetConfigManager()
    {
        if (string.IsNullOrWhiteSpace(_authority))
            throw new InvalidOperationException("Authority not configured.");
        return _authorityMetadata.GetOrAdd(_authority, auth =>
        {
            var metadata = $"{auth.TrimEnd('/')}/.well-known/openid-configuration";
            return new ConfigurationManager<OpenIdConnectConfiguration>(
                metadata,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = true });
        });
    }

    private async Task<ClaimsPrincipal> ValidateEntraJwtAsync(string jwt)
    {
        var cfg = GetConfigManager();
        var oidc = await cfg.GetConfigurationAsync(HttpContext.RequestAborted);

        var validIssuers = string.IsNullOrWhiteSpace(_tenantId)
            ? Array.Empty<string>()
            : new[]
            {
                $"https://login.microsoftonline.com/{_tenantId}/v2.0",
                $"https://login.microsoftonline.com/{_tenantId}/",
                $"https://sts.windows.net/{_tenantId}/"
            };

        if (_expectedAudiences.Length == 0)
            throw new SecurityTokenInvalidAudienceException("EMBED_EXPECTED_AUDIENCES not configured");

        var tvp = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = oidc.SigningKeys,

            ValidateIssuer = true,
            ValidIssuers = validIssuers,

            ValidateAudience = true,
            ValidAudiences = _expectedAudiences,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),

            ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 }
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(jwt, tvp, out var validated);
        if (validated is JwtSecurityToken j && !string.Equals(j.Header.Alg, SecurityAlgorithms.RsaSha256, StringComparison.Ordinal))
            throw new SecurityTokenInvalidAlgorithmException("Unexpected token alg");

        return principal;
    }

    private void IssueSessionCookie(string sub, string tid)
    {
        Response.Cookies.Append(
            "embed_session",
            $"ok:{sub}:{tid}",
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                MaxAge = TimeSpan.FromHours(1)
            });
}

}