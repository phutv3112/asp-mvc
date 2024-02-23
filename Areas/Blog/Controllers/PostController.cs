using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppMVC.Models;
using AppMVC.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using AppMVC.Helpers;
using AppMVC.Areas.Blog.Models;
using Microsoft.AspNetCore.Identity;
using AppMVC.Utilities;

namespace AppMVC.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/post/[action]/{id?}")]
    [Authorize(Roles = "Admin")]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        [TempData]
        public string StatusMessage { get; set; }

        public PostController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Post
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int pageSize)
        {
            var posts = _context.Posts.Include(p => p.Author)
                        .OrderByDescending(p => p.DateUpdated);

            int totalPost = await posts.CountAsync();
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

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPost = totalPost;
            ViewBag.postIndex = (currentPage - 1) * pageSize;

            var postInPage = await posts.Skip((currentPage - 1) * pageSize)
                            .Take(pageSize)
                            .Include(p => p.PostCategories)
                            .ThenInclude(pc => pc.Category)
                            .ToListAsync();

            return View(postInPage);
        }

        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Post/Create
        public async Task<IActionResult> CreateAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoriesIDs")] CreatePostModel post)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug))
            {
                ModelState.AddModelError(string.Empty, "Enter another url");
                return View(post);
            }
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);

                post.DateCreated = post.DateUpdated = DateTime.Now;
                post.AuthorId = user.Id;

                _context.Add(post);

                if (post.CategoriesIDs != null)
                {
                    foreach (var cateId in post.CategoriesIDs)
                    {
                        _context.Add(new PostCategory()
                        {
                            CategoryId = cateId,
                            Post = post
                        });
                    }
                }
                await _context.SaveChangesAsync();
                StatusMessage = "Create post successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // var post = await _context.Posts.FindAsync(id);
            var post = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null)
            {
                return NotFound();
            }
            var postEdit = new CreatePostModel()
            {
                PostId = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                Published = post.Published,
                CategoriesIDs = post.PostCategories.Select(pc => pc.CategoryId).ToArray()
            };
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            return View(postEdit);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published,CategoriesIDs")] CreatePostModel post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && p.PostId != id))
            {
                ModelState.AddModelError(string.Empty, "Enter another url");
                return View(post);
            }


            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
                    if (postUpdate == null)
                    {
                        return NotFound();
                    }
                    postUpdate.Title = post.Title;
                    postUpdate.Content = post.Content;
                    postUpdate.Description = post.Description;
                    postUpdate.Published = post.Published;
                    postUpdate.Slug = post.Slug;
                    postUpdate.DateUpdated = DateTime.Now;

                    //update postcategory
                    if (post.CategoriesIDs == null)
                    {
                        Console.WriteLine("CategoriesIDs is null");
                        post.CategoriesIDs = new int[] { };
                    }

                    var oldCateIds = postUpdate.PostCategories.Select(c => c.CategoryId).ToArray();
                    var newCateIds = post.CategoriesIDs;

                    var removeCatePost = from pc in postUpdate.PostCategories
                                         where !newCateIds.Contains(pc.CategoryId)
                                         select pc;
                    _context.PostCategory.RemoveRange(removeCatePost);

                    var addCatePost = from pc in newCateIds
                                      where !oldCateIds.Contains(pc)
                                      select pc;
                    foreach (var cateId in addCatePost)
                    {
                        _context.PostCategory.Add(new PostCategory()
                        {
                            PostId = id,
                            CategoryId = cateId
                        });
                    }

                    _context.Update(postUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }


            return View(post);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }
    }
}
