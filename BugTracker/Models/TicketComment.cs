using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        [Required]
        string? Comment { get; set; }

        DateTime Created { get; set; }

        //Foreign Keys
        int TicketId { get; set; }
        string? UserId { get; set; }

        //Navigation Properties
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }


    }
}
