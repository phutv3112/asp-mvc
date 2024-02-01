using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Crypto.Engines;
using AppMVC.Models;

namespace App.Admin.Role
{
    public class AddRoleClaimModel : RolePageModel
    {
        public AddRoleClaimModel(RoleManager<IdentityRole> roleManager, AppDbContext context) : base(roleManager, context)
        {
        }
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
        public IdentityRole role { set; get; }


        public async Task<IActionResult> OnGetAsync(string roleid)
        {
            role = await _roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                return NotFound("Role not exist!");
            }

            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string roleid)
        {
            role = await _roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                return NotFound("Role not exist!");
            }
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if ((await _roleManager.GetClaimsAsync(role)).Any(c => c.Type == Input.ClaimType && c.Value == Input.ClaimValue))
            {
                ModelState.AddModelError(string.Empty, "Role Claims type is invalid!");
                return Page();
            }
            var newClaim = new Claim(Input.ClaimType, Input.ClaimValue);
            var result = await _roleManager.AddClaimAsync(role, newClaim);
            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(e =>
                {
                    ModelState.AddModelError(string.Empty, e.Description);
                });
                return Page();
            }
            StatusMessage = "Add role claim successfully";

            return RedirectToPage("./Edit", new { roleid = role.Id });
        }
    }
}
