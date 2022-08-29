using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Notification
    {
        //Primary Key
        int Id { get; set; }

        //Foreign Key(s)
        int ProjectId { get; set; } 
        int TicketId { get; set; }

        [Required]
        string? SenderId { get; set; }

        [Required]
        string? RecipientId  { get; set; }

        //

        [Required]
        public string? Title { get; set; }

        [Required]
        string? Message { get; set; }

        DateTime Created { get; set; }
        int NotificationTypeId  { get; set; }
        bool HasBeenViewed { get; set; }

        //Navigation Properties 
        public virtual NotificationType? NotificationType { get; set; }
        public virtual Ticket? Ticket { get; set; }
        public virtual Project? Project { get; set; }
        public virtual BTUser? Sender { get; set; }
        public virtual BTUser? Recipient { get; set; }
        

    }
}
