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
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using peakmotion.Data;
using peakmotion.Data.Services;
using peakmotion.Models;
using static peakmotion.Data.Services.ReCAPTCHA;

namespace peakmotion.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly PeakmotionContext _context;


        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IEmailService emailService,
            IConfiguration configuration,
            PeakmotionContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _emailService = emailService;
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            //first name of the user
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }


            //last name of the user
            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            //phone number of user
            // [Required]
            [Display(Name = "Phone")]
            public string Phone { get; set; }

            //address of the user
            // [Required]
            [Display(Name = "Address")]
            public string Address { get; set; }

            //city
            // [Required]
            [Display(Name = "City")]
            public string City { get; set; }

            //provience
            // [Required]
            [Display(Name = "Province")]
            public string Province { get; set; }

            //postal code
            // [Required]
            [Display(Name = "Postal Code")]
            public string PostalCode { get; set; }

            //country
            // [Required]
            [Display(Name = "Country")]
            public string Country { get; set; }


            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ViewData["SiteKey"] = _configuration["Recaptcha:SiteKey"];
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            string captchaResponse = Request.Form["g-Recaptcha-Response"];
            string secret = _configuration["Recaptcha:SecretKey"];
            ReCaptchaValidationResult resultCaptcha =
                ReCaptchaValidator.IsValid(secret, captchaResponse);

            //Invalidate the form if the captcha is invalid.
            if (!resultCaptcha.Success)
            {
                ViewData["SiteKey"] = _configuration["Recaptcha:SiteKey"];
                ModelState.AddModelError(string.Empty,
                    "The ReCaptcha is invalid.");
            }

            if (ModelState.IsValid)
            {

                var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "This email address is already registered.");
                    return Page();
                }

                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);
                var customUser = new Pmuser
                {
                    Firstname = Input.FirstName,
                    Lastname = Input.LastName,
                    // Phone = Input.Phone,
                    // Address = Input.Address,
                    // City = Input.City,
                    // Province = Input.Province,
                    // Postalcode = Input.PostalCode,
                    // Country = Input.Country,
                    Email = Input.Email,
                    Lastloggedin = DateOnly.FromDateTime(DateTime.Now)
                };
                _context.Pmusers.Add(customUser);
                await _context.SaveChangesAsync();

                // Automatic role assignment
                var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
                if (!roleResult.Succeeded)
                {
                    _logger.LogInformation("ERROR associating user with customer role");
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
                else
                {
                    _logger.LogInformation("User associated with customer role");
                }

                // Continue with registration process
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    // Professional Email Body
                    var body = $@"
                                <p>Dear {Input.Email},</p>
                                <p>Thank you for registering with <strong>PeakMotion</strong>! To complete your registration and activate your account, please confirm your email by clicking the link below:</p>
                                <p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='color: #007bff; text-decoration: none; font-weight: bold;'>Confirm Your Email</a></p>
                                <p>If you did not sign up for this account, you can safely ignore this email.</p>
                                
                                <br>
                                <p>Best regards,</p>
                                <p><strong>PeakMotion Support Team</strong></p>
                                <p>Email: <a href='mailto:support@peakmotion.com'>support@peakmotion.com</a></p>
                                <p>Phone: +1 (123) 456-7890</p>
                                <p>Website: <a href='https://www.peakmotion.com'>www.peakmotion.com</a></p>
                                <br>
                                <p style='color: gray; font-size: 12px;'><em>This is an automated message. Please do not reply to this email.</em></p>";

                    // SendGrid
                    var response = await _emailService.SendSingleEmail(new ComposeEmailModel
                    {
                        Subject = "Confirm your email",
                        Email = Input.Email,
                        Body = body,
                    });

                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });


                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}