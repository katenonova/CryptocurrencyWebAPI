using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebAPI.Entities;
using WebAPI.Services;

namespace WebAPI.Helpers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserService _userService;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserService userService)
            : base(options, logger, encoder, clock)
        {
            this._userService = userService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            this.Response.Headers.Add("WWW-Authenticate", "Basic");
            if (!this.Request.Headers.ContainsKey("Authorization"))
            {
                this.Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return AuthenticateResult.Fail("Missing authorization header");
            }
            User user = null;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(this.Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                var username = credentials[0];
                var password = credentials[1];

                user = await this._userService.Authenticate(username, password);
            }
            catch
            {
                this.Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return AuthenticateResult.Fail("Invalid authorization header");
            }

            if (user == null)
            {
                this.Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return AuthenticateResult.Fail("Invalid username or password");
            }

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
            };
            var identity = new ClaimsIdentity(claims, this.Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}