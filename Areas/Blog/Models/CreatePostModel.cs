using AppMVC.Models.Blog;

namespace AppMVC.Areas.Blog.Models
{
    public class CreatePostModel : Post
    {
        public int[] CategoriesIDs { get; set; }
    }
}