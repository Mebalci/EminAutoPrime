// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace EminAutoPrime.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Ad Soyad alaný zorunludur.")]
            [Display(Name = "Ad Soyad")]
            [RegularExpression(@"^[a-zA-Z0-9çÇðÐýÝöÖþÞüÜ\s]+$", ErrorMessage = "Ad Soyad yalnýzca harf, rakam ve boþluk içerebilir.")]
            public string UserName { get; set; }

            [Required(ErrorMessage = "E-posta alaný zorunludur.")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Telefon numarasý zorunludur.")]
            [Phone(ErrorMessage = "Geçerli bir telefon numarasý giriniz.")]
            [Display(Name = "Telefon Numarasý")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Þifre alaný zorunludur.")]
            [StringLength(15, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter uzunluðunda olmalýdýr.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Þifre")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Þifre Onayý")]
            [Compare("Password", ErrorMessage = "Þifre ve þifre onayý eþleþmiyor.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {               

                var user = CreateUser();

                user.PhoneNumber = Input.PhoneNumber ?? string.Empty; ;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);                

                if (result.Succeeded)
                {
                    _logger.LogInformation("Yeni kullanýcý hesap þifre ile oluþturuldu.");

                    var roleAssignResult = await _userManager.AddToRoleAsync(user, "Musteri");
                    if (!roleAssignResult.Succeeded)
                    {
                        foreach (var error in roleAssignResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return Page();
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "E-posta doðrulama",
                        $"Lütfen hesabýnýzý doðrulamak için <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>buraya týklayýn</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }               
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }           

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
                throw new InvalidOperationException($"'{nameof(IdentityUser)}' oluþturulamadý. " +
                    $"'{nameof(IdentityUser)}' soyut bir sýnýf olmamalý ve parametresiz bir kurucuya sahip olmalýdýr.");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("Varsayýlan UI, e-posta desteði olan bir kullanýcý deposu gerektirir.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
