using AppMVC.Models;
using AppMVC.Models.Blog;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppMVC.Controllers
{
    [Area("Database")]
    public class DbManageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DbManageController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [TempData]
        public string StatusMessage { get; set; }

        [Route("/database-manage/[action]")]
        // GET: DbManage
        public ActionResult Index()
        {
            StatusMessage = "Hello index database";
            return View();
        }
        public async Task<IActionResult> SeedData()
        {
            _context.Categories.RemoveRange(_context.Categories.Where(c => c.Content.Contains("[fakeData]")));
            _context.Posts.RemoveRange(_context.Posts.Where(p => p.Content.Contains("[fakeData]")));

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

    }
}
