using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
    public interface IBTTicketService
    {
        public Task AddTicketCommentAsync(TicketComment ticketComment, int ticketId);

        public Task<List<Ticket>> GetArchivedTicketsByProjectIdAsync(int ticketId);

        public Task AddTicketAsync(Ticket ticket);

        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

        public Task<Ticket> GetTicketByIdAsync(int ticketId);

        public Task ArchiveTicketAsync(int ticketId);

        public Task RestoreTicketAsync(int ticketId);

        public Task<bool> AddDeveloperAsync(int ticketId, string userId);

        public Task<BTUser>? GetDeveloperAsync(int ticketId);

        public Task RemoveDeveloperAsync(int ticketId);

        public Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId, int companyId);

        public Task<bool> AddUserToTicketAsync(BTUser user, int ticketId);

        public Task<bool> RemoveUserFromTicketAsync(BTUser user, int ticketId);
        public Task UpdateTicketAsync(Ticket ticket);
        public Task<List<Ticket>> GetAllTicketsByProjectIdAsync(int projectId);

        public Task<List<Ticket>> GetUnassignedTicketsAsync(int projectId);

        public Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId);

        public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);

        public Task<List<Ticket>> GetArchivedTicketsByCompanyIdAsync(int companyId);

        public Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId);
    }
}
