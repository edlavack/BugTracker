using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        
        //Foreign Keys
        int TicketId { get; set; }
           

        string? PropertyName { get; set; }
        string? Description { get; set; }

        DateTime Created { get; set; }

        string? OldValue { get; set; }
        string? NewValue { get; set; }

        [Required]
        string? UserId { get; set; }

        //Navigation Properties 
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }


    }
}
