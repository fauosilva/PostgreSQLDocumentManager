using ApplicationCore.Constants;
using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PostgreSQLDocumentManager.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PostgreSQLDocumentManager.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion(1.0)]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILoginService loginService;
        private readonly IOptions<JwtConfiguration> options;

        public AuthenticationController(ILoginService loginService, IOptions<JwtConfiguration> options)
        {
            this.loginService = loginService;
            this.options = options;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize]
        public async Task<IActionResult> Login(LoginRequest loginRequest, CancellationToken cancellationToken)
        {
            var user = await loginService.AuthenticateUserAsync(loginRequest, cancellationToken);

            if (user != null)
            {
                var claims = new List<Claim>() {
                    new Claim(UserClaimsConstants.UserId, user.Id.ToString()),
                    new Claim(UserClaimsConstants.UserName, user.Username),                    
                    new Claim(UserClaimsConstants.Role, user.Role)
                };

                var issuer = options.Value.Issuer;
                var audience = options.Value.Audience;
                var expiry = DateTime.Now.AddMinutes(30);
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.Key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(issuer: issuer, audience: audience,
                    expires: expiry, signingCredentials: credentials, claims: claims);
                var tokenHandler = new JwtSecurityTokenHandler();
                var stringToken = tokenHandler.WriteToken(token);
                return Ok(new LoginResponse(stringToken));
            }

            return Unauthorized();
        }
    }
}
