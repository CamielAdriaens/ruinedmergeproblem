using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FileStorage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleTokenRequest request)
        {
            try
            {
                var clientId = "911031744599-l50od06i5t89bmdl4amjjhdvacsdonm7.apps.googleusercontent.com";

                // Validate the Google ID token
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                });

                // If token is valid, return user info
                return Ok(new
                {
                    Message = "Authentication successful",
                    UserId = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name
                });

            }
            catch (InvalidJwtException ex)
            {
                return BadRequest("Invalid Google token.");
            }

        }


    }


    // Model for receiving the token from the frontend
    public class GoogleTokenRequest
    {
        public string Token { get; set; }
    }
}
