using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public string? Description { get; set; }

        public DateTime? Created { get; set; }

        //Foreign Keys
        public int TicketId { get; set; }
        [Required]
        public string? UserId { get; set; }


        public IFormFile? FormFile { get; set; }
        public byte[]? FileData { get; set; }
        public string? FileType { get; set; }

        //Navigation Properties 
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
