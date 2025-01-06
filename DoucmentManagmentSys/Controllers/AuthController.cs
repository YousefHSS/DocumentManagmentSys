using DoucmentManagmentSys.ModelBinders;
using DoucmentManagmentSys.Models;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using LoginRequest = Microsoft.AspNetCore.Identity.Data.LoginRequest;
using RegisterRequest = DoucmentManagmentSys.Requests.Auth.RegisterRequest;

namespace DoucmentManagmentSys.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase 
    {
        private readonly UserManager<PrimacyUser> _userManager;
        private readonly SignInManager<PrimacyUser> _signInManager;
        private readonly IEmailSender<PrimacyUser> _emailSender;
        private readonly LinkGenerator _linkGenerator;
        private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;
        private readonly TimeProvider _timeProvider;
        private static readonly EmailAddressAttribute _emailAddressAttribute = new();

        public AuthController(
            UserManager<PrimacyUser> userManager,
            SignInManager<PrimacyUser> signInManager,
            IEmailSender<PrimacyUser> emailSender,
            LinkGenerator linkGenerator,
            IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
            TimeProvider timeProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _linkGenerator = linkGenerator;
            _bearerTokenOptions = bearerTokenOptions;
            _timeProvider = timeProvider;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([ModelBinder(BinderType = typeof(JsonOrFormDataModelBinder))] RegisterRequest registration)
        {
            if (!_userManager.SupportsUserEmail)
            {
                return BadRequest("Email support is required.");
            }

            if (string.IsNullOrEmpty(registration.Email) || !_emailAddressAttribute.IsValid(registration.Email))
            {
                return ValidationProblem(new ValidationProblemDetails
                {
                    Errors = { ["Email"] = new[] { "Invalid email address." } }
                });
            }

            var user = new PrimacyUser()
            {
                Name = registration.Name,
                Surname = registration.Surname
            };
            //Do not Set it to name because username must be unique for some reason
            
            await _userManager.SetEmailAsync(user, registration.Email);
            await _userManager.SetUserNameAsync(user, registration.Email);
            
            var result = await _userManager.CreateAsync(user, registration.Password);
            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
                {
                    
                }//Email already exists
                return ValidationProblem(new ValidationProblemDetails { Errors = result.Errors.Where(e=> e.Code != "DuplicateUserName").ToDictionary(e => e.Code  , e => new[] { e.Description }) });
            }

            await SendConfirmationEmailAsync(user, registration.Email);
            return Ok("Email confirmation link sent.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([ModelBinder(BinderType = typeof(JsonOrFormDataModelBinder))] LoginRequest login, [FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies)
        {
            var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
            var isPersistent = (useCookies == true) && (useSessionCookies != true);

            _signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

            var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                   
                string errorMessage;
                if (result.IsLockedOut)
                {
                    errorMessage = "Your account is locked out. Please try again later.";
                }
                else if (result.IsNotAllowed)
                {
                    errorMessage = "cannot sign in without a confirmed account.";
                }
                else if (result.RequiresTwoFactor)
                {
                    errorMessage = "Two-factor authentication is required. Please check your authentication app or email for the code.";
                }
                else
                {
                    errorMessage = "Invalid login attempt. Please check your email and password and try again.";
                }
                return Unauthorized(new { errors = errorMessage });

            }

            return Ok("Logged in successfully.");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([ModelBinder(BinderType = typeof(JsonOrFormDataModelBinder))] RefreshRequest refreshRequest)
        {
            var refreshTokenProtector = _bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
            var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

            if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
                _timeProvider.GetUtcNow() >= expiresUtc ||
                await _signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not PrimacyUser user)
            {
                return Unauthorized();
            }

            var newPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
            return SignIn(newPrincipal, IdentityConstants.BearerScheme);
        }

        [HttpGet("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string code, [FromQuery] string? changedEmail)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (FormatException)
            {
                return Unauthorized();
            }

            IdentityResult result = string.IsNullOrEmpty(changedEmail)
                ? await _userManager.ConfirmEmailAsync(user, code)
                : await _userManager.ChangeEmailAsync(user, changedEmail, code);

            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            return Content("Thank you for confirming your email.");
        }

        [HttpPost("resendConfirmationEmail")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest resendRequest)
        {
            var user = await _userManager.FindByEmailAsync(resendRequest.Email);
            if (user != null)
            {
                await SendConfirmationEmailAsync(user, resendRequest.Email);
            }

            return Ok();
        }

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest resetRequest)
        {
            var user = await _userManager.FindByEmailAsync(resetRequest.Email);
            if (user != null && await _userManager.IsEmailConfirmedAsync(user))
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                await _emailSender.SendPasswordResetCodeAsync(user, resetRequest.Email, HtmlEncoder.Default.Encode(code));
            }

            return Ok();
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetRequest)
        {
            var user = await _userManager.FindByEmailAsync(resetRequest.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return ValidationProblem(new ValidationProblemDetails
                {
                    Errors = { ["Token"] = new[] { "Invalid token." } }
                });
            }

            try
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
                var result = await _userManager.ResetPasswordAsync(user, code, resetRequest.NewPassword);

                if (!result.Succeeded)
                {
                    return ValidationProblem(new ValidationProblemDetails { Errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }) });
                }
            }
            catch (FormatException)
            {
                return ValidationProblem(new ValidationProblemDetails
                {
                    Errors = { ["Token"] = new[] { "Invalid token." } }
                });
            }

            return Ok();
        }

        private async Task SendConfirmationEmailAsync(PrimacyUser user, string email)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var userId = await _userManager.GetUserIdAsync(user);
            var callbackUrl = Url.Action(
                nameof(ConfirmEmail),
                "Auth",
                new { userId, code },
                Request.Scheme);

            await _emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(callbackUrl));
        }
    }

}
