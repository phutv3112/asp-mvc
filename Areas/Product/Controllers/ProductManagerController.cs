using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppMVC.Models;
using AppMVC.Models.Product;
using Microsoft.AspNetCore.Authorization;
using AppMVC.Helpers;
using Microsoft.AspNetCore.Identity;
using AppMVC.Utilities;
using AppMVC.Areas.Product.Models;
using System.ComponentModel.DataAnnotations;

namespace AppMVC.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("admin/product/[action]/{id?}")]
    [Authorize(Roles = "Admin")]
    public class ProductManagerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        [TempData]
        public string StatusMessage { get; set; }

        public ProductManagerController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Post
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int pageSize)
        {
            var products = _context.Products.Include(p => p.Author)
                        .OrderByDescending(p => p.DateUpdated);

            int totalProducts = await products.CountAsync();
            if (pageSize <= 0) pageSize = 10;
            int countPage = (int)Math.Ceiling((double)totalProducts / pageSize);

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
            ViewBag.totalProducts = totalProducts;
            ViewBag.postIndex = (currentPage - 1) * pageSize;

            var productInPage = await products.Skip((currentPage - 1) * pageSize)
                            .Take(pageSize)
                            .Include(p => p.ProductCategories)
                            .ThenInclude(pc => pc.Category)
                            .ToListAsync();

            return View(productInPage);
        }

        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Post/Create
        public async Task<IActionResult> CreateAsync()
        {
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoriesIDs,DiscountPercent,Price")] CreateProductModel product)
        {
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            if (product.Slug == null)
            {
                product.Slug = AppUtilities.GenerateSlug(product.Title);
            }

            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug))
            {
                ModelState.AddModelError(string.Empty, "Enter another url");
                return View(product);
            }
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);

                product.DateCreated = product.DateUpdated = DateTime.Now;
                product.AuthorId = user.Id;

                _context.Add(product);

                if (product.CategoriesIDs != null)
                {
                    foreach (var cateId in product.CategoriesIDs)
                    {
                        _context.Add(new ProductCategory()
                        {
                            CategoryId = cateId,
                            Product = product
                        });
                    }
                }
                await _context.SaveChangesAsync();
                StatusMessage = "Create product successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // var post = await _context.Posts.FindAsync(id);
            var product = await _context.Products.Include(p => p.ProductCategories).FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            var productEdit = new CreateProductModel()
            {
                ProductId = product.ProductId,
                Title = product.Title,
                Content = product.Content,
                Description = product.Description,
                Slug = product.Slug,
                Published = product.Published,
                Price = product.Price,
                DiscountPercent = product.DiscountPercent,
                CategoriesIDs = product.ProductCategories.Select(pc => pc.CategoryId).ToArray()
            };
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            return View(productEdit);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Title,Description,Slug,Content,Published,CategoriesIDs,DiscountPercent,Price")] CreateProductModel product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            if (product.Slug == null)
            {
                product.Slug = AppUtilities.GenerateSlug(product.Title);
            }

            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug && p.ProductId != id))
            {
                ModelState.AddModelError(string.Empty, "Enter another url");
                return View(product);
            }


            if (ModelState.IsValid)
            {
                try
                {
                    var productUpdate = await _context.Products.Include(p => p.ProductCategories)
                    .FirstOrDefaultAsync(p => p.ProductId == id);
                    if (productUpdate == null)
                    {
                        return NotFound();
                    }
                    productUpdate.Title = product.Title;
                    productUpdate.Content = product.Content;
                    productUpdate.Description = product.Description;
                    productUpdate.Published = product.Published;
                    productUpdate.Slug = product.Slug;
                    productUpdate.Price = product.Price;
                    productUpdate.DateUpdated = DateTime.Now;
                    productUpdate.DiscountPercent = product.DiscountPercent;

                    //update postcategory
                    if (product.CategoriesIDs == null)
                    {
                        Console.WriteLine("CategoriesIDs is null");
                        product.CategoriesIDs = new int[] { };
                    }

                    var oldCateIds = productUpdate.ProductCategories.Select(c => c.CategoryId).ToArray();
                    var newCateIds = product.CategoriesIDs;

                    var removeCateProduct = from pc in productUpdate.ProductCategories
                                            where !newCateIds.Contains(pc.CategoryId)
                                            select pc;
                    _context.ProductCategories.RemoveRange(removeCateProduct);

                    var addCateProduct = from pc in newCateIds
                                         where !oldCateIds.Contains(pc)
                                         select pc;
                    foreach (var cateId in addCateProduct)
                    {
                        _context.ProductCategories.Add(new ProductCategory()
                        {
                            ProductId = id,
                            CategoryId = cateId
                        });
                    }

                    _context.Update(productUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
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


            return View(product);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public class UploadOneFile
        {
            [Required]
            [DataType(DataType.Upload)]
            [FileExtensions(Extensions = "png,jpg,jpeg,gif")]
            public IFormFile FileUpload { get; set; }
        }
        [HttpGet]
        public async Task<IActionResult> UploadPhoto(int id)
        {
            var product = _context.Products.Where(p => p.ProductId == id)
                                    .Include(p => p.Photos)
                                    .FirstOrDefault();
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.product = product;
            return View(new UploadOneFile());
        }
        [HttpPost, ActionName("UploadPhoto")]
        public async Task<IActionResult> UploadPhotoAsync(int id, [Bind("FileUpload")] UploadOneFile f)
        {
            var product = _context.Products.Where(p => p.ProductId == id)
                                    .Include(p => p.Photos)
                                    .FirstOrDefault();
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.product = product;

            if (f != null)
            {
                var file1 = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                            + Path.GetExtension(f.FileUpload.FileName);
                var file = Path.Combine("Uploads", "Products", file1);
                using (var filestream = new FileStream(file, FileMode.Create))
                {
                    await f.FileUpload.CopyToAsync(filestream);
                }
                _context.Add(new ProductPhoto()
                {
                    ProductId = product.ProductId,
                    FileName = file1
                });
                await _context.SaveChangesAsync();
            }
            return View(f);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
        [HttpPost]
        public IActionResult ListPhotos(int id)
        {
            var product = _context.Products.Where(p => p.ProductId == id)
                                    .Include(p => p.Photos)
                                    .FirstOrDefault();
            if (product == null)
            {
                return Json(new
                {
                    success = 0,
                    message = "Product not found"
                });
            }
            var listPhotos = product.Photos.Select(p => new
            {
                id = p.Id,
                path = "/contents/Products/" + p.FileName
            });
            return Json(new
            {
                success = 1,
                photos = listPhotos
            });
        }

        public IActionResult DeletePhoto(int id)
        {
            var photo = _context.ProductPhotos.Where(p => p.Id == id).FirstOrDefault();
            if (photo != null)
            {
                _context.Remove(photo);
                _context.SaveChanges();
                var file = "Uploads/Products/" + photo.FileName;
                System.IO.File.Delete(file);

            }
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> UploadPhotoApi(int id, [Bind("FileUpload")] UploadOneFile f)
        {
            var product = _context.Products.Where(e => e.ProductId == id)
                .Include(p => p.Photos)
                .FirstOrDefault();

            if (product == null)
            {
                return NotFound("Product not found");
            }


            if (f != null)
            {
                var file1 = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                            + Path.GetExtension(f.FileUpload.FileName);

                var file = Path.Combine("Uploads", "Products", file1);

                using (var filestream = new FileStream(file, FileMode.Create))
                {
                    await f.FileUpload.CopyToAsync(filestream);
                }

                _context.Add(new ProductPhoto()
                {
                    ProductId = product.ProductId,
                    FileName = file1
                });

                await _context.SaveChangesAsync();
            }


            return Ok();
        }

    }
}
