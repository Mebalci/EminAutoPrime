// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using EminAutoPrime.Data;
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
        private readonly UserManager<AplicationUser> _userManager;
        private readonly SignInManager<AplicationUser> _signInManager;
        private readonly IUserStore<AplicationUser> _userStore;
        private readonly IUserEmailStore<AplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<AplicationUser> userManager,
            IUserStore<AplicationUser> userStore,
            SignInManager<AplicationUser> signInManager,
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
            [Required(ErrorMessage = "Ad alan� zorunludur.")]
            [Display(Name = "Ad")]
            [RegularExpression(@"^[a-zA-Z������������\s]+$", ErrorMessage = "Ad yaln�zca harf, rakam ve bo�luk i�erebilir.")]
            public string KullaniciAdi { get; set; }

            [Required(ErrorMessage = "Soyad alan� zorunludur.")]
            [Display(Name = "Soyad")]
            [RegularExpression(@"^[a-zA-Z������������\s]+$", ErrorMessage = "Soyad yaln�zca harf, rakam ve bo�luk i�erebilir.")]
            public string KullaniciSoyAdi { get; set; }

            [Required(ErrorMessage = "E-posta alan� zorunludur.")]
            [EmailAddress(ErrorMessage = "Ge�erli bir e-posta adresi giriniz.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Telefon numaras� zorunludur.")]
            [Phone(ErrorMessage = "Ge�erli bir telefon numaras� giriniz.")]
            [Display(Name = "Telefon Numaras�")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "�ifre alan� zorunludur.")]
            [StringLength(15, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter uzunlu�unda olmal�d�r.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "�ifre")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "�ifre Onay�")]
            [Compare("Password", ErrorMessage = "�ifre ve �ifre onay� e�le�miyor.")]
            public string ConfirmPassword { get; set; }
        }

        private string FormatName(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var culture = new CultureInfo("tr-TR");
            text = text.ToLower(culture);
            return char.ToUpper(text[0], culture) + text.Substring(1);
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

                

                user.KullaniciAdi= (FormatName(Input.KullaniciAdi) ?? "Emin Auto Ad");
                user.KullaniciSoyadi = (FormatName(Input.KullaniciSoyAdi) ?? "Emin Auto Soyad");
                user.PhoneNumber = Input.PhoneNumber ?? string.Empty; ;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);                

                if (result.Succeeded)
                {
                    _logger.LogInformation("Yeni kullan�c� hesap �ifre ile olu�turuldu.");

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

                    await _emailSender.SendEmailAsync(Input.Email, "E-posta do�rulama",
                        $"L�tfen hesab�n�z� do�rulamak i�in <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>buraya t�klay�n</a>.");

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

        private AplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"'{nameof(AplicationUser)}' olu�turulamad�. " +
                    $"'{nameof(AplicationUser)}' soyut bir s�n�f olmamal� ve parametresiz bir kurucuya sahip olmal�d�r.");
            }
        }

        private IUserEmailStore<AplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("Varsay�lan UI, e-posta deste�i olan bir kullan�c� deposu gerektirir.");
            }
            return (IUserEmailStore<AplicationUser>)_userStore;
        }
    }
}
