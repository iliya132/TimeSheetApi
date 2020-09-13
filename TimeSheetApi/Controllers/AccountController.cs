using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TimeSheetApi.Model.Identity;

namespace TimeSheetApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<TimeSheetUser> signInManager;

        public AccountController(SignInManager<TimeSheetUser> signInManager)
        {
            this.signInManager = signInManager;
        }
        [HttpGet]
        [Route(nameof(Login))]
        public async Task Login(string login, string passwrd)
        {
            var result = await signInManager.PasswordSignInAsync("timesheetuser", "DK_User1!", true, false);
            Debug.WriteLine(result.Succeeded);
        }

        [HttpGet]
        [Route(nameof(Logout))]
        public Task Logout()
        {
            return signInManager.SignOutAsync();
        }
    }
}
