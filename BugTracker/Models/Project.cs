﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class Project
    {
        //Primary Key
        public int Id { get; set; }

        //Foreign Key
        public int CompanyId { get; set; }
        //

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Description { get; set; }

        public DateTime Created { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int ProjectPriorityId { get; set; }

        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }

        public string? ImageFileName { get; set; }
        public string? ImageFileType { get; set; }

        public bool Archived { get; set; }

        //Navigation Properties
        public virtual Company? Company { get; set; }
        public virtual ProjectPriority? ProjectPriority { get; set; }
        public virtual ICollection<BTUser>? Members { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<Ticket>? Tickets { get; set; } = new HashSet<Ticket>();





    }
}
