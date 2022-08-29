using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketAttachment
    {
        int Id { get; set; }
        string? Description { get; set; }

        DateTime? Created { get; set; }

        //Foreign Key
        int TicketId { get; set; }

        [Required]
        string? UserId { get; set; }

        IFormFile? FormFile { get; set; }
        byte[]? FileData { get; set; }
        string? FileType { get; set; }

        //Navigation Properties 
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
