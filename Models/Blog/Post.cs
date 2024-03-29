using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AppMVC.Models.Blog
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Title { get; set; }
        public string Description { get; set; }

        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} is from {1} to {2}")]
        [RegularExpression(@"^[a-z0-9-]*$")]
        [AllowNull]
        public string? Slug { get; set; }

        public string Content { get; set; }
        public bool Published { get; set; }
        public List<PostCategory>? PostCategories { set; get; }

        public string? AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public AppUser? Author { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}