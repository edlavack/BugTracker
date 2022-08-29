using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Company
    {
        //Foreign Key
        int Id { get; set; }

        [Required]
        string? Name { get; set; }
        string? Description { get; set; }
        IFormFile? ImageFormFile { get; set; }
        string? ImageFileName { get; set; }
        string? ImageFileType { get; set; }

        //Navigation Properties 
        public virtual ICollection<Project>? Projects { get; set; }
        public virtual ICollection<BTUser>? Members { get; set; }
        public virtual ICollection<Invite>? Invites { get; set; }




    }
}
