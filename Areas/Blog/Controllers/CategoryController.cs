using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppMVC.Models;
using AppMVC.Models.Blog;

namespace AppMVC.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/category/[action]/{id?}")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var qr = (from c in _context.Categories select c)
                    .Include(c => c.CategoryParent)
                    .Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync()).Where(c => c.CategoryParent == null)
            .ToList();
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.CategoryParent)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        private void CreateSelectItems(List<Category> source, List<Category> des, int level)
        {
            string prefix = string.Concat(Enumerable.Repeat("*", level));
            foreach (var c in source)
            {
                // c.Slug = prefix + c.Slug;
                des.Add(new Category()
                {
                    Id = c.Id,
                    Slug = prefix + c.Slug
                });
                if (c.CategoryChildren?.Count > 0)
                {
                    CreateSelectItems(c.CategoryChildren.ToList(), des, level + 1);
                }
            }
        }

        // GET: Category/Create
        public async Task<IActionResult> Create()
        {
            var qr = (from c in _context.Categories select c)
                    .Include(c => c.CategoryChildren)
                    .Include(c => c.CategoryParent);

            var categories = (await qr.ToListAsync())
                            .Where(c => c.CategoryParent == null)
                            .ToList();
            categories.Insert(0, new Category()
            {
                Id = -1,
                Slug = "No Parent"
            });
            var items = new List<Category>();
            CreateSelectItems(categories, items, 0);
            var selectList = new SelectList(items, "Id", "Slug");
            ViewData["ParentId"] = selectList;
            return View();
        }

        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ParentId,Title,Content,Slug")] Category category)
        {
            Console.WriteLine("Id:=== " + category.Id);
            Console.WriteLine("ParentId:== " + category.ParentId);
            Console.WriteLine("Title: " + category.Title);
            Console.WriteLine("Content: " + category.Content);
            Console.WriteLine("Slug: " + category.Slug);
            Console.WriteLine("Parent: " + category.CategoryParent);
            // if (!ModelState.IsValid)
            // {
            //     return View(category);

            // }
            var qr = (from c in _context.Categories select c)
                   .Include(c => c.CategoryChildren)
                   .Include(c => c.CategoryParent);

            var categories = (await qr.ToListAsync())
                            .Where(c => c.CategoryParent == null)
                            .ToList();
            categories.Insert(0, new Category()
            {
                Id = -1,
                Slug = "No Parent"
            });
            var selectList = new SelectList(categories, "Id", "Slug");
            ViewData["ParentId"] = selectList;
            if (category.ParentId == -1) category.ParentId = null;
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            // ViewData["ParentId"] = new SelectList(_context.Categories, "Id", "Slug", category.ParentId);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound("Id not found");
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound("Category not found");
            }
            var qr = (from c in _context.Categories select c)
                   .Include(c => c.CategoryChildren)
                   .Include(c => c.CategoryParent);

            var categories = (await qr.ToListAsync())
                            .Where(c => c.CategoryParent == null)
                            .ToList();
            categories.Insert(0, new Category()
            {
                Id = -1,
                Slug = "No Parent"
            });
            var items = new List<Category>();
            CreateSelectItems(categories, items, 0);
            var selectList = new SelectList(items, "Id", "Slug");
            ViewData["ParentId"] = selectList;
            // ViewData["ParentId"] = new SelectList(_context.Categories, "Id", "Slug", category.ParentId);
            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ParentId,Title,Content,Slug")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound("Id not found!!");
            }
            if (category.ParentId == -1) category.ParentId = null;
            if (category.ParentId == category.Id)
            {
                ModelState.AddModelError(string.Empty, "choose another");
                return Content("Please select another category");
            }
            // if (!ModelState.IsValid)
            // {

            //     return View(category);
            // }
            ViewData["ParentId"] = new SelectList(_context.Categories, "Id", "Slug", category.ParentId);
            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.Id))
                {
                    return NotFound("Category not found!!");
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.CategoryParent)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = _context.Categories
                            .Include(c => c.CategoryChildren)
                            .FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            foreach (var c in category.CategoryChildren)
            {
                c.ParentId = category.ParentId;
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
