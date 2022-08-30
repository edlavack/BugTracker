﻿using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Ticket
    {
        //Primary Key
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        public bool Archived { get; set; }
        public bool ArchivedByProject { get; set; }

        //Foreign Keys
        public int ProjectId { get; set; }
        public int TicketTypeId { get; set; }
        public int TicketStatusId { get; set; }
        public int TicketPriorityId { get; set; }
        public string? DeveloperUserId { get; set; }
        [Required]
        public string? SubmitterUserId { get; set; }


        //Navigation Properties 
        public virtual Project? Project { get; set; }
        public virtual TicketPriority? TicketPriority { get; set; }
        public virtual TicketType? TicketType { get; set; }
        public virtual TicketStatus? TicketStatus { get; set; }
        public virtual BTUser? DeveloperUser { get; set; }
        public virtual BTUser? SubmitterUser { get; set; }
        public virtual ICollection<BTUser>? Comments { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<BTUser>? Attachments { get; set; } = new HashSet<BTUser>();
        public virtual TicketHistory? History { get; set; }




    }
}
