using AppMVC.Models.Blog;
using AppMVC.Models.Contacts;
using AppMVC.Models.Product;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AppMVC.Models
{
    //AppMVC.Models.AppDbContext
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            // create indexer for slug in category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Slug).IsUnique();
            });
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasIndex(p => p.Slug).IsUnique();
            });
            modelBuilder.Entity<PostCategory>(entity =>
            {
                entity.HasKey(p => new { p.PostId, p.CategoryId });
            });

            modelBuilder.Entity<CategoryProduct>(entity =>
            {
                entity.HasIndex(c => c.Slug).IsUnique();
            });
            modelBuilder.Entity<ProductModel>(entity =>
            {
                entity.HasIndex(p => p.Slug).IsUnique();
            });
            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.HasKey(p => new { p.ProductId, p.CategoryId });
            });

        }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostCategory> PostCategory { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<CategoryProduct> CategoryProducts { get; set; }
        public DbSet<ProductPhoto> ProductPhotos { get; set; }
    }
}