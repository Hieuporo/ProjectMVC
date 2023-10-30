using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectMVC.Data;
using ProjectMVC.Models;
using ProjectMVC.Models.ViewModels;
using System.Security.Claims;
using Azure.Core;

namespace ProjectMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _db;
        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            DataContext db
            )
        {
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }
      
        public IActionResult Login()
        {
            var model = new SignInViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult>  Login(SignInViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if(user == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    var claims = new List<Claim>() {
                        new Claim(ClaimTypes.Email, model.Email),
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.NameIdentifier, user.Id)
                    };

                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties()
                    {
                        IsPersistent = false
                    });
                }
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }


        public IActionResult Register()
        {
            var model = new RegisterViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber
                }, model.Password);

                if (result.Succeeded)
                {
                    ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == model.Email);
                    await _userManager.AddToRoleAsync(user, "User");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View(model);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
