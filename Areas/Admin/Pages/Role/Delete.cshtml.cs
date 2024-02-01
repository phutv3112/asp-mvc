using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Bcpg;
using AppMVC.Models;

namespace App.Admin.Role
{
    public class DeleteModel : RolePageModel
    {
        public DeleteModel(RoleManager<IdentityRole> roleManager, AppDbContext context) : base(roleManager, context)
        {
        }

        public IdentityRole role { set; get; }
        public async Task<IActionResult> OnGetAsync(string roleid)
        {
            if (roleid == null) return NotFound("Role not found");
            role = await _roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                return NotFound("Role not found");

            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string roleid)
        {
            if (roleid == null) return NotFound("Role not found");
            role = await _roleManager.FindByIdAsync(roleid);
            if (role == null) return NotFound("Role not found");

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                StatusMessage = $"Updated role {role.Name}";
                return RedirectToPage("./Index");
            }
            else
            {
                result.Errors.ToList().ForEach(e =>
                {
                    ModelState.AddModelError(string.Empty, e.Description);
                });
            }
            return Page();
        }
    }
}
