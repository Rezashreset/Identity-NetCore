using System.Text;
using Identity.ViewModels;
using IdentityCodeYad.Tools;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IViewRenderService _viewRenderService;
        public readonly SignInManager<IdentityUser> _signInManager;
        public AccountController(UserManager<IdentityUser> userManager, IViewRenderService viewRenderService, IEmailSender emailSender, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _viewRenderService = viewRenderService;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.IsSent = false;
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            ViewBag.IsSent = false;
            if (!ModelState.IsValid)
                return View();

            var result = await _userManager.CreateAsync(new IdentityUser()
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.Phone,


            }, model.Password);


            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);  // in male asp-validation-summary="ModelOnly" dar view hast
                    return View();
                }
            }
            var user = await _userManager.FindByNameAsync(model.UserName);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            string? callBackUrl = Url.ActionLink("ConfirmEmail", "Account", new { userId = user.Id, token = token },
                Request.Scheme);
            string body = await _viewRenderService.RenderToStringAsync("_RegisterEmail", callBackUrl);
            await _emailSender.SendEmailAsync(new EmailModel(user.Email, "تایید حساب", body));
            ViewBag.IsSent = true;
            return View();

        }
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null) return BadRequest();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            ViewBag.IsConfirmed = result.Succeeded ? true : false;
            return View();
        }
        public async Task<IActionResult> Login(string returnUlr = null)
        {
            returnUlr ??= Url.Content("~/");

            ViewBag.ReturnUrl = returnUlr;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVm model, string returnUlr = null)
        {
            returnUlr ??= Url.Content("~/");
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "کاربری با این مشخصات وارده یافت نشد");
                return View(model);
            }
            var resualt = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

            if (resualt.Succeeded)
            {
                if (Url.IsLocalUrl(returnUlr))
                    return Redirect(returnUlr);
                else
                    return RedirectToAction("Index", "Home");

            }
            else if (resualt.RequiresTwoFactor)
            {
                return Redirect("LoginWieth2fa");
            }
            else if (resualt.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "حساب شما قفل شده است");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "تلاش برای ورود نامعتبر می باشد");
            return View();

        }
        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");

        }
        public IActionResult ForgotPassword()
        {
            ViewBag.IsSent = false;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            int FlagMany=0;
            ViewBag.IsSent = false;
            if (!ModelState.IsValid)
                return View(model);


            #region reza
            foreach (var users in _userManager.Users)
            {


                if (users.Email == model.Email)
                {
                    FlagMany = FlagMany + 1;
                }

            }
            #endregion reza

            if (FlagMany > 1)
            {
                ModelState.AddModelError(string.Empty, "بیش از یک کاربر با این ایمیل ثبت نام کرده است");
                return View();
            }


            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "تلاش برای ارسال ایمیل ناموفق بود کاربر یافت نشد");
                return View();
            }


            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            string? callBackUrl = Url.ActionLink("ResetPassword", "Account", new { email= user.Email, token = token },
                Request.Scheme);
            string body = await _viewRenderService.RenderToStringAsync("_ResetPasswordEmail", callBackUrl);
            await _emailSender.SendEmailAsync(new EmailModel(user.Email, "بازیابی کلمه عبور", body));
            ViewBag.IsSent = true;
            return View();

        }
        
        public IActionResult ResetPassword (string token,string email)
        {
            ViewBag.IsSent = false;
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(token)) return BadRequest();
            ResetPasswordVM model = new()
            {
                Token = token,
                Email = email
            };
            return View();
        }
        [HttpPost]
        public async Task< IActionResult> ResetPassword(ResetPasswordVM model)
        {
            ViewBag.IsSent = false;
            if (!ModelState.IsValid)  return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "تلاش برای بازیابی کلمه عبور ناموفق بود");
                return View();
            }
           var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var resault = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!resault.Succeeded) 
            {
                foreach (var err in resault.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);  // in male asp-validation-summary="ModelOnly" dar view hast
                    return View();
                }
            }
            ViewBag.IsSent = true;
            return View();
;        }
        [HttpPost]
        public async Task<IActionResult> IsAnyEmail(string email){

            bool any = await _userManager.Users.AnyAsync(u => u.Email == email);
            if (!any)
                return Json(true);

            return Json("این ایمیل از قبل ثبت شده است");

        }
        [HttpPost]
        public async Task<IActionResult> IsAnyUsername(string username)
        {

            bool any = await _userManager.Users.AnyAsync(u => u.UserName == username);
            if (!any)
                return Json(true);

            return Json("این نام کاربری از قبل ثبت شده است");

        }
    }
}
//public async Task<IActionResult> Register(RegisterVM model)
//{
//    ViewBag.IsSent = false;
//    IdentityResult result = null;

//    if (ModelState.IsValid)
//    {
//        result = await _userManager.CreateAsync(new IdentityUser()
//        {
//            UserName = model.UserName,
//            Email = model.Email,
//            PhoneNumber = model.Phone,


//        }, model.Password);
//        if (!result.Succeeded)
//        {
//            foreach (var err in result.Errors)
//            {
//                ModelState.AddModelError(string.Empty, err.Description);
//                return View();
//            }
//        }


//        var user = await _userManager.FindByNameAsync(model.UserName);

//        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
//        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
//        string? callBackUrl = Url.ActionLink("ConfirmEmail", "Account", new { userId = user.Id, token = token },
//            Request.Scheme);
//        string body = await _viewRenderService.RenderToStringAsync("_RegisterEmail", callBackUrl);
//        await _emailSender.SendEmailAsync(new EmailModel(user.Email, "تایید حساب", body));
//        ViewBag.IsSent = true;
//    }
//    else if (!ModelState.IsValid)
//    {

//        return View();

//    }

//    return View();
//}