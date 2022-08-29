using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Ticket
    {
        //Primary Key
        public int Id { get; set; }

        [Required]
        string Title { get; set; }

        [Required]
        string Description { get; set; }

        DateTime Created { get; set; }
        DateTime Updated { get; set; }

        bool Archived { get; set; }
        bool ArchivedByProject { get; set; }

        //Foreign Key
        int ProjectId { get; set; }
        int TicketTypeId { get; set; }
        int TicketStatusId { get; set; }
        int TicketPriorityId { get; set; }
        string? DeveloperUserId { get; set; }

        [Required]
        string? SubmitterUserId { get; set; }
        //

        //Navigation Properties 
        public virtual Project? Project { get; set; }
        public virtual TicketPriority? TicketPriority { get; set; }
        public virtual TicketType? TicketType { get; set; }
        public virtual TicketStatus? TicketStatus { get; set; }
        public virtual BTUser? DeveloperUser { get; set; }
        public virtual BTUser? SubmitterUser { get; set; }
        public virtual BTUser? History { get; set; }



    }
}
