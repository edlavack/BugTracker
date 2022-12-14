using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        //Foreign Keys
        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }
        //

        public string? PropertyName { get; set; }
        public string? Description { get; set; }


        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        public string? OldValue { get; set; }
        public string? NewValue { get; set; }


        //Navigation Properties 
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }


    }
}
