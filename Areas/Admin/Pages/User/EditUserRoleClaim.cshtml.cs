using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuGet.Versioning;
using Org.BouncyCastle.Bcpg.OpenPgp;
using AppMVC.Models;

namespace App.Admin.User
{
    public class EditUserRoleClaimModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public EditUserRoleClaimModel(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [TempData]
        public string StatusMessage { set; get; }
        public class InputModel
        {
            [Display(Name = "Claim Type")]
            [StringLength(255), MinLength(3)]
            public string ClaimType { get; set; }

            [Display(Name = "Claim Value")]
            [StringLength(255), MinLength(3)]
            public string ClaimValue { get; set; }
        }
        [BindProperty]
        public InputModel Input { set; get; }
        public NotFoundObjectResult OnGet() => NotFound("Access denied");
        public AppUser user { set; get; }
        public async Task<IActionResult> OnGetAddClaimAsync(string userid)
        {
            user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("User not found");
            return Page();
        }
        public IdentityUserClaim<string> userClaim { get; set; }
        public async Task<IActionResult> OnGetEditClaimAsync(int? claimid)
        {
            if (claimid == null) return NotFound("User Claim not found");
            userClaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            user = await _userManager.FindByIdAsync(userClaim.UserId);
            if (user == null) return NotFound("User not found");
            Input = new InputModel()
            {
                ClaimType = userClaim.ClaimType,
                ClaimValue = userClaim.ClaimValue
            };
            return Page();
        }
        public async Task<IActionResult> OnPostEditClaimAsync(int? claimid)
        {
            if (claimid == null) return NotFound("User Claim not found");
            userClaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            user = await _userManager.FindByIdAsync(userClaim.UserId);
            if (user == null) return NotFound("User not found");
            if (!ModelState.IsValid) return Page();

            if (_context.UserClaims.Any(c => c.UserId == user.Id
            && c.ClaimType == Input.ClaimType
            && c.ClaimValue == Input.ClaimValue
            && c.Id != claimid))
            {
                ModelState.AddModelError(string.Empty, "Claim is  existing");
                return Page();
            }
            userClaim.ClaimType = Input.ClaimType;
            userClaim.ClaimValue = Input.ClaimValue;

            await _context.SaveChangesAsync();
            StatusMessage = "Update user claim success";
            return RedirectToPage("./AddRole", new { id = user.Id });
        }
        public async Task<IActionResult> OnPostDeleteClaimAsync(int? claimid)
        {
            if (claimid == null) return NotFound("User Claim not found");
            userClaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            user = await _userManager.FindByIdAsync(userClaim.UserId);
            if (user == null) return NotFound("User not found");

            await _userManager.RemoveClaimAsync(user, new Claim(userClaim.ClaimType, userClaim.ClaimValue));
            StatusMessage = "Delete user claim success";
            return RedirectToPage("./AddRole", new { id = user.Id });
        }
        public async Task<IActionResult> OnPostAddClaimAsync(string userid)
        {
            user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("User not found");
            if (!ModelState.IsValid) return Page();
            var claims = _context.UserClaims.Where(c => c.UserId == user.Id);
            if (claims.Any(c => c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue))
            {
                ModelState.AddModelError(string.Empty, "Claim is exist");
                return Page();
            }
            await _userManager.AddClaimAsync(user, new Claim(Input.ClaimType, Input.ClaimValue));
            StatusMessage = "Add claim for user success";
            return RedirectToPage("./AddRole", new { id = user.Id });
        }
    }
}
