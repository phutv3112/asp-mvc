using AppMVC.Models;
using AppMVC.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMVC.Areas.Blog.Controllers
{
    [Area("Blog")]
    public class ViewPostController : Controller
    {
        private readonly ILogger<ViewPostController> _logger;
        private readonly AppDbContext _context;

        public ViewPostController(ILogger<ViewPostController> logger, AppDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ViewPostController
        [Route("/post/{categoryslug?}")]
        public ActionResult Index(string categoryslug, int page)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.categoryslug = categoryslug;
            Category category = null;
            if (!string.IsNullOrEmpty(categoryslug))
            {
                category = _context.Categories.Where(c => c.Slug == categoryslug)
                            .Include(c => c.CategoryChildren)
                            .FirstOrDefault();
                if (category == null)
                {
                    return NotFound();
                }
            }
            var posts = _context.Posts.Include(p => p.Author)
                                    .Include(p => p.PostCategories)
                                    .ThenInclude(p => p.Category)
                                    .AsQueryable();
            posts.OrderByDescending(p => p.DateUpdated);
            ViewBag.category = category;
            return View(posts.ToList());
        }
        [Route("/post/{postslug}.html")]
        public IActionResult Detail(string postslug)
        {
            return View();
        }
        private List<Category> GetCategories()
        {
            var categories = _context.Categories.Include(c => c.CategoryChildren)
                                            .AsEnumerable()
                                            .Where(c => c.CategoryParent == null)
                                            .ToList();
            return categories;
        }

    }
}
