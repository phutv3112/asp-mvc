using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models.Blog
{
    public class Category
    {
        [Key]
        public int Id { set; get; }

        public int? ParentId { set; get; }
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Title { set; get; }
        [DataType(DataType.Text)]
        public string Content { set; get; }
        // Chuoi url
        [Required(ErrorMessage = "Required url")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} is from {1} to {2}")]
        [RegularExpression(@"^[a-z0-9-]*$")]
        public string Slug { set; get; }
        public ICollection<Category> CategoryChildren { set; get; }
        [ForeignKey("ParentId")]
        public Category CategoryParent { set; get; }
    }
}