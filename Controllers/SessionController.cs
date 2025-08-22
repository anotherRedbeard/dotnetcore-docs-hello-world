// Controllers/SessionController.cs
using Microsoft.AspNetCore.Mvc;

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
}