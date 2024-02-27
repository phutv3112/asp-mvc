using AppMVC.Data;
using AppMVC.Models;
using AppMVC.Models.Blog;
using AppMVC.Models.Product;
using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509;

namespace AppMVC.Controllers
{
    [Area("Database")]
    public class DbManageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;

        public DbManageController(AppDbContext context, UserManager<AppUser> userManager,
                                RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }
        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet]
        [Authorize(Roles = RoleName.Admin)]
        public IActionResult DeleteDb()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = RoleName.Admin)]
        public async Task<IActionResult> DeleteDbAsync()
        {
            var success = await _context.Database.EnsureDeletedAsync();
            StatusMessage = success ? "Deleted Database successfully" : "Could not delete database";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> MigrationAsync()
        {
            await _context.Database.MigrateAsync();
            StatusMessage = "Migrated successfully";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> SeedDataAsync()
        {
            var roles = typeof(RoleName).GetFields().ToList();
            foreach (var role in roles)
            {
                var roleName = (string)role.GetRawConstantValue();
                var roleFound = _roleManager.FindByNameAsync(roleName);
                if (roleFound == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var userAdmin = await _userManager.FindByEmailAsync("admin@example.com");
            if (userAdmin == null)
            {
                userAdmin = new AppUser()
                {
                    UserName = "Admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(userAdmin, "admin123");
                await _userManager.AddToRoleAsync(userAdmin, RoleName.Admin);
                await _signInManager.SignInAsync(userAdmin, false);
                return RedirectToAction("SeedData");
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Forbid();
                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Any(r => r == RoleName.Admin))
                {
                    return Forbid();
                }
            }

            await SeedPostCategory();
            await SeedProductCategory();
            StatusMessage = "Seed Data Success";
            return RedirectToAction(nameof(Index));
        }

        [Route("/database-manage/[action]")]
        // GET: DbManage
        public ActionResult Index()
        {
            StatusMessage = "Hello index database";
            return View();
        }
        public async Task<IActionResult> SeedPostCategory()
        {
            _context.Categories.RemoveRange(_context.Categories.Where(c => c.Content.Contains("[fakeData]")));
            _context.Posts.RemoveRange(_context.Posts.Where(p => p.Content.Contains("[fakeData]")));
            _context.SaveChanges();

            var fakerCategory = new Faker<Category>();
            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"CM{cm++} " + fk.Lorem.Sentence(1, 2).Trim('.'));
            fakerCategory.RuleFor(c => c.Content, fk => fk.Lorem.Sentence(5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

            var cate1 = fakerCategory.Generate();
            var cate11 = fakerCategory.Generate();
            var cate12 = fakerCategory.Generate();

            var cate2 = fakerCategory.Generate();
            var cate21 = fakerCategory.Generate();
            var cate211 = fakerCategory.Generate();

            cate11.CategoryParent = cate1;
            cate12.CategoryParent = cate1;

            cate21.CategoryParent = cate2;
            cate211.CategoryParent = cate21;

            var categories = new Category[] { cate1, cate11, cate12, cate2, cate21, cate211 };
            _context.AddRange(categories);

            //Seed Post
            var rCateIndex = new Random();
            int bv = 1;

            var user = _userManager.GetUserAsync(this.User).Result;
            var fakerPost = new Faker<Post>();
            fakerPost.RuleFor(p => p.AuthorId, f => user.Id);
            fakerPost.RuleFor(p => p.Content, f => f.Lorem.Paragraph(7) + "[fakeData]");
            fakerPost.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2023, 12, 5), new DateTime(2024, 2, 20)));
            fakerPost.RuleFor(p => p.Description, f => f.Lorem.Sentence(3));
            fakerPost.RuleFor(p => p.Published, f => true);
            fakerPost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerPost.RuleFor(p => p.Title, f => $"Post {bv++} " + f.Lorem.Sentence(3, 4).Trim('.'));

            List<Post> posts = new List<Post>();
            List<PostCategory> postCategories = new List<PostCategory>();

            for (int i = 0; i < 40; i++)
            {
                var post = fakerPost.Generate();
                post.DateUpdated = post.DateCreated;
                posts.Add(post);
                postCategories.Add(new PostCategory()
                {
                    Post = post,
                    Category = categories[rCateIndex.Next(5)]
                });
            }

            _context.AddRange(posts);
            _context.AddRange(postCategories);

            _context.SaveChanges();
            StatusMessage = "Seed data successfully";
            // return Content("Seed Data");
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> SeedProductCategory()
        {
            _context.CategoryProducts.RemoveRange(_context.CategoryProducts.Where(c => c.Content.Contains("[fakeData]")));
            _context.Products.RemoveRange(_context.Products.Where(p => p.Content.Contains("[fakeData]")));
            _context.SaveChanges();

            var fakerCategory = new Faker<CategoryProduct>();
            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"CateProduct{cm++} " + fk.Lorem.Sentence(1, 2).Trim('.'));
            fakerCategory.RuleFor(c => c.Content, fk => fk.Lorem.Sentence(5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

            var cate1 = fakerCategory.Generate();
            var cate11 = fakerCategory.Generate();
            var cate12 = fakerCategory.Generate();

            var cate2 = fakerCategory.Generate();
            var cate21 = fakerCategory.Generate();
            var cate211 = fakerCategory.Generate();

            cate11.CategoryParent = cate1;
            cate12.CategoryParent = cate1;

            cate21.CategoryParent = cate2;
            cate211.CategoryParent = cate21;

            var categories = new CategoryProduct[] { cate1, cate11, cate12, cate2, cate21, cate211 };
            _context.AddRange(categories);

            //Seed Post
            var rCateIndex = new Random();
            int bv = 1;

            var user = _userManager.GetUserAsync(this.User).Result;
            var fakerProduct = new Faker<ProductModel>();
            fakerProduct.RuleFor(p => p.AuthorId, f => user.Id);
            fakerProduct.RuleFor(p => p.Content, f => f.Commerce.ProductDescription() + "[fakeData]");
            fakerProduct.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2023, 12, 5), new DateTime(2024, 2, 20)));
            fakerProduct.RuleFor(p => p.Description, f => f.Lorem.Sentence(3));
            fakerProduct.RuleFor(p => p.Published, f => true);
            fakerProduct.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerProduct.RuleFor(p => p.Title, f => $"Product {bv++} " + f.Commerce.ProductName());
            fakerProduct.RuleFor(p => p.Price, f => int.Parse(f.Commerce.Price(500, 1000, 0)));

            List<ProductModel> products = new List<ProductModel>();
            List<ProductCategory> productCategories = new List<ProductCategory>();

            for (int i = 0; i < 40; i++)
            {
                var product = fakerProduct.Generate();
                product.DateUpdated = product.DateCreated;
                products.Add(product);
                productCategories.Add(new ProductCategory()
                {
                    Product = product,
                    Category = categories[rCateIndex.Next(5)]
                });
            }

            _context.AddRange(products);
            _context.AddRange(productCategories);

            _context.SaveChanges();
            StatusMessage = "Seed data successfully";
            // return Content("Seed Data");
            return RedirectToAction("Index");
        }

    }
}
