using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class Class
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and a max {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and a max {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }

        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        IFormFile? ImageFormFile { get; set; }

        string? ImageFileName { get; set; }
        string? ImageFileType { get; set; }

        //Foreign Key
        int CompanyId { get; set; }

        //Navigation Properties
        public virtual Company? Company { get; set; }
       public virtual ICollection<Project>? Projects { get; set; }




    }
}
