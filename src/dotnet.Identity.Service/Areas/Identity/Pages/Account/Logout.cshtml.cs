using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet.Identity.Service.Entities;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace dotnet.Identity.Service.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IIdentityServerInteractionService interactionService;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger, IIdentityServerInteractionService interactionService)
        {
            _signInManager = signInManager;
            _logger = logger;
            this.interactionService = interactionService;
        }

        public async Task<IActionResult> OnGetAsync(string logoutid)
        {
            //Get contextual info of the current logout flow
            var context = await interactionService.GetLogoutContextAsync(logoutid);
            if (context?.ShowSignoutPrompt == false) return await this.OnPost(context.PostLogoutRedirectUri);

            //Display a page
            return Page();
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToPage();
            }
        }
    }
}
