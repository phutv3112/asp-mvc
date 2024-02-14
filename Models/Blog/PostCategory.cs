using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models.Blog
{
    public class PostCategory
    {
        public int PostId { set; get; }
        public int CategoryId { set; get; }
        [ForeignKey("PostId")]
        public Post Post { set; get; }
        [ForeignKey("CategoryId")]
        public Category Category { set; get; }
    }
}