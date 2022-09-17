using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class Project
    {
        //Primary Key
        public int Id { get; set; }

        //Foreign Keys
        public int CompanyId { get; set; }
        public int ProjectPriorityId { get; set; }
        //

        [Required]
        [DisplayName("Name")]
        [StringLength(250, ErrorMessage = "The {0} must be at least {2} and a max {1} characters long.", MinimumLength = 2)]
        public string? Name { get; set; }

        [Required]
        [DisplayName("Description")]
        [StringLength(2000, ErrorMessage = "The {0} must be at least {2} and a max {1} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Created")]
        public DateTime Created { get; set; }


        [DataType(DataType.Date)]
        [DisplayName("Project Start Date")]
        public DateTime StartDate { get; set; }


        [DataType(DataType.Date)]
        [DisplayName("Project End Date")]
        public DateTime EndDate { get; set; }


        

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFormFile { get; set; }

        [DisplayName("Project Image")]
        public byte[]? ImageFileData { get; set; }

        [DisplayName("File Name")]
        public string? ImageFileName { get; set; }

        [DisplayName("File Extension")]
        public string? ImageContentType { get; set; }

        public bool Archived { get; set; }

        //Navigation Properties
        public virtual Company? Company { get; set; }
        public virtual ProjectPriority? ProjectPriority { get; set; }
        public virtual ICollection<BTUser>? Members { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<Ticket>? Tickets { get; set; } = new HashSet<Ticket>();





    }
}
