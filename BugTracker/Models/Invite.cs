using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Invite
    {
        //Primary Key
        int Id { get; set; }
        DateTime InviteDate { get; set; }
        DateTime JoinDate { get; set; }
        Guid CompanyToken { get; set; }
        //Foreign Key(s)
        [Required]
        string? InvitorId { get; set; }

        string? InviteeId { get; set; }

        [Required]
        string? InviteeEmail { get; set; }

        [Required]
        string? InviteeFirstName { get; set; }

        [Required]
         string? InviteeLastName { get; set; }

        string? Message { get; set; }
        bool IsValid { get; set; }

        //Navigation Properties 
        public virtual Company? Company { get; set; }
        public virtual Project? Project { get; set; }
        public virtual BTUser? Invitor { get; set; }
        public virtual BTUser? Invitee { get; set; }



    }
}
