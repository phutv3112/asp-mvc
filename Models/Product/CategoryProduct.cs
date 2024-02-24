using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMVC.Models.Product
{
    public class CategoryProduct
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
        public ICollection<CategoryProduct> CategoryChildren { set; get; }
        [ForeignKey("ParentId")]
        public CategoryProduct CategoryParent { set; get; }

        public void ChildrenCategoryIDs(ICollection<CategoryProduct> childrencates, List<int> list)
        {
            if (childrencates == null)
            {
                childrencates = this.CategoryChildren;
            }
            foreach (CategoryProduct cate in childrencates)
            {
                list.Add(cate.Id);
                ChildrenCategoryIDs(cate.CategoryChildren, list);
            }
        }
        public List<CategoryProduct> ListParents()
        {
            List<CategoryProduct> list = new List<CategoryProduct>();
            var parent = this.CategoryParent;
            while (parent != null)
            {
                list.Add(parent);
                parent = parent.CategoryParent;
            }
            list.Reverse();
            return list;
        }
    }
}