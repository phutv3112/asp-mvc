using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace AppMVC.Models
{
    public class AppUser : IdentityUser
    {
        public AppUser()
        {
        }

        [Column(TypeName = "nvarchar")]
        [StringLength(255)]
        public string? HomeAddress { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
    }
}