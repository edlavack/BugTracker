using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Project
    {
        //Primary Key
        int Id { get; set; }

        //Foreign Key
        int CompanyId { get; set; }

        [Required]
        string? Name { get; set; }

        [Required]
        string? Description { get; set; }

        DateTime? Created { get; set; }
        DateTime StartDate { get; set; }
        DateTime? EndDate   { get; set; }

        int ProjectPriorityId { get; set; }

        IFormFile? ImageFormFile { get; set; }

        string? ImageFileName { get; set; }
        string? ImageFileType { get; set; }

        bool Archived { get; set; }

        //Navigation Properties
        public virtual Company? Company { get; set; }
        public virtual ProjectPriority? ProjectPriority { get; set; }
        public virtual ICollection<BTUser>? Members { get; set; }
        public virtual ICollection<Ticket>? Tickets { get; set; }





    }
}
