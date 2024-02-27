using AppMVC.Helpers;
using AppMVC.Models;
using AppMVC.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMVC.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Authorize(Roles = "Admin")]
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
        public ActionResult Index(string categoryslug, [FromQuery(Name = "p")] int currentPage, int pageSize)
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

            if (category != null)
            {
                var ids = new List<int>();
                category.ChildrenCategoryIDs(null, ids);
                ids.Add(category.Id);

                posts = posts.Where(p => p.PostCategories.Where(pc => ids.Contains(pc.CategoryId)).Any());
            }

            int totalPost = posts.Count();
            if (pageSize <= 0) pageSize = 10;
            int countPage = (int)Math.Ceiling((double)totalPost / pageSize);

            if (currentPage > countPage) currentPage = countPage;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countPages = countPage,
                currentPage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesize = pageSize
                })
            };

            var postInPage = posts.Skip((currentPage - 1) * pageSize)
                           .Take(pageSize);

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPost = totalPost;

            ViewBag.category = category;
            return View(postInPage.ToList());
        }
        [Route("/post/{postslug}.html")]
        public IActionResult Detail(string postslug)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;

            var post = _context.Posts.Where(p => p.Slug == postslug)
                            .Include(p => p.Author)
                            .Include(p => p.PostCategories)
                            .ThenInclude(pc => pc.Category)
                            .FirstOrDefault();
            if (post == null)
            {
                return NotFound();
            }

            Category category = post.PostCategories.FirstOrDefault()?.Category;
            ViewBag.category = category;

            var otherPosts = _context.Posts.Where(p => p.PostCategories.Any(c => c.Category.Id == category.Id))
                                    .Where(p => p.PostId != post.PostId)
                                    .OrderByDescending(p => p.DateUpdated)
                                    .Take(5);
            ViewBag.otherPosts = otherPosts;
            return View(post);
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
