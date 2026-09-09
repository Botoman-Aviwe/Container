// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using PrescribingSystem.Data;
using System.Net;
using System.Net.Mail;
using PrescribingSystem.Models;

namespace PrescribingSystem.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender, IConfiguration configuration)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Health Council Registration Number")]
            public string HealthCouncilRegistrationNumber { get; set; }

            [Required]
            [Display(Name = "Identity Number")]
            public string IdentityNumber { get; set; }

            [Required]
            [Display(Name = "Role")]
            public string Role { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            Input = new InputModel()
            {
                RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            string generatedPassword = GeneratePassword();

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                HealthCouncilRegistrationNumber = Input.HealthCouncilRegistrationNumber,
                IdentityNumber = Input.IdentityNumber
            };

            var result = await _userManager.CreateAsync(user, generatedPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Input.Role);
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code },
                    protocol: Request.Scheme);

                // Build professional email body
                string encodedUrl = HtmlEncoder.Default.Encode(callbackUrl);
                string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; margin: 0; padding: 20px;"">
    <div style=""max-width: 600px; margin: 0 auto; background: #fff; padding: 20px; border-radius: 5px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);"">
        <h2 style=""color: #0056b3; margin-top: 0;"">Welcome to Transnet MIS</h2>
        <p>An account has been created for you. Please confirm your email address to activate your account.</p>
        
        <!-- Call to action button -->
        <p style=""text-align: center; margin: 25px 0;"">
            <a href='{encodedUrl}' style=""display: inline-block; padding: 12px 24px; background-color: #28a745; color: #fff; text-decoration: none; border-radius: 5px; font-weight: bold;"">Confirm Email</a>
        </p>
        
        <!-- Fallback text link -->
        <p style=""margin-bottom: 20px;"">If the button above doesn't work, copy and paste this link into your browser:<br>
        <a href='{encodedUrl}' style=""color: #0056b3; word-break: break-all;"">{encodedUrl}</a></p>
        
        <hr style=""border: 0; border-top: 1px solid #eee; margin: 25px 0;"">
        
        <h3 style=""color: #0056b3;"">Your login details</h3>
        <p><strong>Health Council Registration Number:</strong> {user.HealthCouncilRegistrationNumber}</p>
        <p><strong>Password:</strong> {generatedPassword}</p>
        
        <!-- Important note box -->
        <p style=""color: #856404; background-color: #fff3cd; border: 1px solid #ffeeba; padding: 12px; border-radius: 5px; font-size: 0.95em;"">
            <strong>Note:</strong> You cannot log in until your email is confirmed. Please use the details above after confirmation.
        </p>
        
        <hr style=""border: 0; border-top: 1px solid #eee; margin: 25px 0;"">
        
        <p style=""font-size: 0.9em; color: #777; margin-bottom: 0;"">If you did not request this account, please ignore this email.</p>
    </div>
</body>
</html>";

                await _emailSender.SendEmailAsync(user.Email, "Confirm Your Email", emailBody);
                return RedirectToPage("/Account/RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        private string GeneratePassword()
        {
            return $"IBP{Guid.NewGuid().ToString().Substring(0, 9)}!";
        }

        public async Task<IActionResult> OnGetConfirmEmailAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                await _userManager.UpdateAsync(user);
                return RedirectToPage("/Account/ConfirmEmailSuccess");
            }

            return RedirectToPage("Account/ConfirmEmailFailure");
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
            using (SmtpClient smtp = new SmtpClient(smtpSettings.Server, smtpSettings.Port))
            {
                smtp.UseDefaultCredentials = false;
                smtp.EnableSsl = true;
                smtp.Port = 587;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);

                MailMessage message = new MailMessage
                {
                    To = { to },
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);
                try
                {
                    await smtp.SendMailAsync(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email sending failed: {ex}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                }
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}