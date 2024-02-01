using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AppMVC.Models;

namespace App.Admin.USer
{
    // [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        public IndexModel(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public class UserAndRole : AppUser
        {
            public string RolesName { set; get; }
        }
        public List<UserAndRole> users { set; get; }
        [TempData]
        public string StatusMessage { set; get; }
        public const int ITEM_PER_PAGE = 5;
        [BindProperty(SupportsGet = true, Name = "p")]
        public int currentPage { set; get; }
        public int countPages { set; get; }
        public async Task OnGet()
        {
            // users = await _userManager.Users.ToListAsync();
            var qr = _userManager.Users.OrderBy(u => u.UserName);
            int totalUser = await qr.CountAsync();
            countPages = (int)Math.Ceiling((double)totalUser / ITEM_PER_PAGE);
            if (currentPage < 1) currentPage = 1;
            if (currentPage > countPages) currentPage = countPages;
            var query = qr.Skip((currentPage - 1) * ITEM_PER_PAGE)
                        .Take(ITEM_PER_PAGE)
                        .Select(u => new UserAndRole()
                        {
                            Id = u.Id,
                            UserName = u.UserName
                        });
            users = await query.ToListAsync();
            foreach (var user in users)
            {
                var role = await _userManager.GetRolesAsync(user);
                user.RolesName = string.Join(",", role);
            }
        }
        public void OnPost() => RedirectToPage(); // chuyen ve OnGet()
    }
}
