using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Org.BouncyCastle.Bcpg;
using AppMVC.Models;

namespace App.Admin.Role
{
    public class EditRoleClaimModel : RolePageModel
    {
        public EditRoleClaimModel(RoleManager<IdentityRole> roleManager, AppDbContext context) : base(roleManager, context)
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
        public IdentityRoleClaim<string> claim { set; get; }

        public async Task<IActionResult> OnGetAsync(int? claimid)
        {
            if (claimid == null) return NotFound("Role claim not found");
            claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound("Role claim not found");
            role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role == null)
            {
                return NotFound("Role not exist!");
            }
            Input = new InputModel()
            {
                ClaimType = claim.ClaimType,
                ClaimValue = claim.ClaimValue
            };
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(int? claimid)
        {
            if (claimid == null) return NotFound("Role claim not found");
            claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound("Role claim not found");
            role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role == null)
            {
                return NotFound("Role not exist!");
            }
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (_context.RoleClaims.Any(c => c.RoleId == role.Id && c.ClaimType == Input.ClaimType
            && c.ClaimValue == Input.ClaimValue && c.Id != claimid))
            {
                ModelState.AddModelError(string.Empty, "Role Claims type is invalid!");
                return Page();
            }
            claim.ClaimType = Input.ClaimType;
            claim.ClaimValue = Input.ClaimValue;

            await _context.SaveChangesAsync();

            StatusMessage = "Edit role claim successfully";

            return RedirectToPage("./Edit", new { roleid = role.Id });
        }
        public async Task<IActionResult> OnPostDeleteAsync(int? claimid)
        {
            if (claimid == null) return NotFound("Role claim not found");
            claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound("Role claim not found");
            role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role == null)
            {
                return NotFound("Role not exist!");
            }


            await _roleManager.RemoveClaimAsync(role, new Claim(claim.ClaimType, claim.ClaimValue));
            StatusMessage = "Delete role claim successfully";

            return RedirectToPage("./Edit", new { roleid = role.Id });
        }
    }
}
