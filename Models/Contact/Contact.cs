using System.ComponentModel.DataAnnotations;

namespace AppMVC.Models.Contacts
{
    public class Contact
    {
        [Key]
        public int Id { set; get; }
        [StringLength(50)]
        [Required]
        public string FullName { set; get; }
        [Required]
        [EmailAddress]
        public string Email { set; get; }
        public DateTime? DateSent { set; get; }
        public string? Message { set; get; }
        [StringLength(11)]
        [Phone]
        public string? Phone { set; get; }
    }
}