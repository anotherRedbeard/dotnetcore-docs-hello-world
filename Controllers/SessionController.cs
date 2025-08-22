// Controllers/SessionController.cs
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Route("api/session")]
public class SessionController : ControllerBase
{
    public record SessionStartDto(string? token);

    [HttpPost("start")]
    public IActionResult Start([FromBody] SessionStartDto body)
    {
        if (body?.token != "123")
            return Unauthorized();

        // Issue a simple dev cookie. In prod, sign/encrypt and use a real session id.
        Response.Cookies.Append(
            "embed_session",
            "dev-ok",
            new CookieOptions {
                HttpOnly = true,
                Secure = true,        // required for SameSite=None
                SameSite = SameSiteMode.None,
                Path = "/",
                MaxAge = TimeSpan.FromHours(1)
            });

        return NoContent();
    }

    // Issues an embed token for the parent to pass into the iframe bootstrap.
    // For local/dev, if EMBED_JWT_PRIVATE_KEY is not set, returns the static token "123".
    // In production, set EMBED_JWT_PRIVATE_KEY to an RSA private key in PEM format.
    [HttpGet("embed-token")]
    public IActionResult GetEmbedToken()
    {
        // Try to discover user from claims (AAD tokens often carry oid/tid)
        var oid = User.FindFirst("oid")?.Value ?? "dev-oid";
        var tid = User.FindFirst("tid")?.Value ?? "dev-tenant";

        var audience = "app.redmancorp.com";
        var issuer = "your-tab-backend";

        var pem = Environment.GetEnvironmentVariable("EMBED_JWT_PRIVATE_KEY");
        string token;

        if (string.IsNullOrWhiteSpace(pem))
        {
            // Dev fallback: static token expected by the bootstrap for local testing
            token = "123";
        }
        else
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(pem.AsSpan());

            var creds = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

            var claims = new List<Claim>
            {
                new("sub", oid),
                new("tid", tid),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Aud, audience)
            };

            var handler = new JwtSecurityTokenHandler();
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = creds,
            };

            var st = handler.CreateToken(descriptor);
            token = handler.WriteToken(st);
        }

        return Ok(new
        {
            token,
            iframeOrigin = "https://app.redmancorp.com",
            parentOrigin = "https://your-tab-domain.example"
        });
    }
}