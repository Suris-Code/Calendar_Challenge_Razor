using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string name) : base(name) { }


        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public string LastModifiedBy { get; set; } = string.Empty;

        [Required]
        public DateTime LastModified { get; set; }
    }
}
