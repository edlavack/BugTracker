﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        [DisplayName("File Description")]
        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime Created { get; set; }

        //Foreign Keys
        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }

        [NotMapped]
        public IFormFile? FormFile { get; set; }

        public byte[]? FileData { get; set; }
        public string? FileContentType { get; set; }

        //Navigation Properties 
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
