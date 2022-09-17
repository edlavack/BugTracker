using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {

        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTTicketService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }
        public async Task<bool> AddDeveloperAsync(int ticketId, string userId)
        {



            try
            {
                Ticket? ticket = await _context.Tickets.FindAsync(ticketId);
                ticket!.DeveloperUserId = userId;

                await _context.SaveChangesAsync();


                return true;
            }
            catch (Exception)
            {

                throw;
            }





        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            try
            {
                await _context.AddAsync(ticket);

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddUserToTicketAsync(BTUser user, int ticketId)
        {
            try
            {
                Ticket? ticket = await GetTicketByIdAsync(ticketId);

                bool onTicket = ticket.Project!.Members!.Any(m => m.Id == user.Id);

                if (onTicket)
                {
                    ticket.Project.Members!.Add(user);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ArchiveTicketAsync(int ticketId)
        {
            try
            {
                Ticket ticket = await GetTicketByIdAsync(ticketId);

                if (ticket != null)
                {
                    ticket!.Archived = true;

                }


                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsByProjectIdAsync(int projectId)
        {


            try
            {
                List<Ticket> tickets = await _context.Tickets
                                                        .Where(p => p.ProjectId == projectId && p.Archived)
                                                       .Include(p => p.Project)
                                                        .Include(p => p.TicketPriority)
                                                        .Include(p => p.TicketType)
                                                        .Include(p => p.TicketStatus)
                                                        .Include(p => p.DeveloperUser)
                                                        .Include(p => p.SubmitterUser)
                                                        .ToListAsync();
                return tickets;
            }
            catch
            {
                throw;
            }

        }

        public async Task<BTUser>? GetDeveloperAsync(int ticketId)
        {
            try
            {
                Ticket? ticket = await GetTicketByIdAsync(ticketId);

                BTUser developer = ticket.DeveloperUser!;

                if (developer != null)
                {
                    return developer;
                }



                return null!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            Ticket? ticket = await _context.Tickets!
                                             .Include(t => t.DeveloperUser)
                                             .Include(t => t.Attachments)
                                             .Include(t => t.Project)
                                             .Include(t => t.History)
                                             .Include(t => t.Comments)
                                                .ThenInclude(c=>c.User)
                                             .Include(t => t.TicketType)
                                             .Include(t => t.TicketStatus)
                                             .Include(t => t.TicketPriority)
                                             .Include(t => t.SubmitterUser)
                                            .FirstOrDefaultAsync(t => t.Id == ticketId)!;

            return ticket!;
        }

        public async Task<bool> IsUserOnTicketAsync(string id, int ticketId)
        {
            try
            {
                Ticket? ticket = await GetTicketByIdAsync(ticketId);

                if (ticket != null)
                {
                    if (ticket.DeveloperUserId != null)
                    {
                        return true;

                    }
                    return false;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RestoreTicketAsync(int ticketId)
        {
            try
            {
                Ticket ticket = await GetTicketByIdAsync(ticketId);

                if (ticket != null)
                {
                    ticket!.Archived = false;



                }


                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveDeveloperAsync(int ticketId)
        {
            try
            {
                Ticket? ticket = await GetTicketByIdAsync(ticketId);

                BTUser developer = ticket.DeveloperUser!;

                if (developer == null)
                {
                    // Remove BTUser from Project 
                    await RemoveUserFromTicketAsync(developer!, ticketId);

                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromTicketAsync(BTUser user, int ticketId)
        {
            try
            {
                Ticket? ticket = await GetTicketByIdAsync(ticketId);

                //Check if User is on ticket
                if (await IsUserOnTicketAsync(user.Id, ticketId))
                {

                    return true;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId, int companyId)
        {
            try
            {
                Ticket? ticket = new();
                ticket = await _context.Projects
                                              .Where(p => p.CompanyId == companyId && !p.Archived)
                                              .SelectMany(p => p.Tickets!)
                                              .Include(t => t.TicketType)
                                              .Include(t => t.TicketStatus)
                                              .Include(t => t.TicketPriority)
                                              .Include(t => t.DeveloperUser)
                                              .Include(t => t.SubmitterUser)
                                              .Include(t => t.Attachments)
                                              .Include(t => t.Project)
                                              .Include(t => t.Comments)
                                              .Include(t => t.History)
                                              .Where(t => t.Archived && !t.ArchivedByProject)
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(t => t.Id == ticketId);

                return ticket!;


            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByProjectIdAsync(int projectId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets.Where(p => p.ProjectId == projectId && !p.Archived)
                                                                .Include(p => p.Project)
                                                                .Include(p => p.TicketPriority)
                                                                .Include(p => p.TicketType)
                                                                .Include(p => p.TicketStatus)
                                                                .Include(p => p.DeveloperUser)
                                                                .Include(p => p.SubmitterUser)
                                                                .ToListAsync();

                return tickets;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int projectId)
        {
            List<Ticket> rawTickets = await GetAllTicketsByProjectIdAsync(projectId);

            List<Ticket> tickets = new();

            foreach (Ticket ticket in rawTickets)
            {
                if (await GetDeveloperAsync(ticket.Id)! == null)
                {
                    tickets.Add(ticket);
                }


            }
            return tickets;
        }

        public async Task AddTicketCommentAsync(TicketComment ticketComment, int ticketId)
        {

            try
            {
                Ticket ticket = await GetTicketByIdAsync(ticketId);

                await _context.AddAsync(ticketComment);
                await _context.SaveChangesAsync();
                
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment? ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.User)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets.Where(t => t.Archived == true || t.ArchivedByProject == true && t.Project!.CompanyId == companyId).Include(t => t.Project)
                                                             .Include(t => t.TicketPriority)
                                                             .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Projects
                                              .Where(p => p.CompanyId == companyId && !p.Archived)
                                              .SelectMany(p => p.Tickets!)
                                              .Include(t => t.TicketType)
                                              .Include(t => t.TicketStatus)
                                              .Include(t => t.TicketPriority)
                                              .Include(t => t.DeveloperUser)
                                              .Include(t => t.SubmitterUser)
                                              .Include(t => t.Attachments)
                                              .Include(t => t.Project)
                                              .Include(t => t.Comments)
                                              .Include(t => t.History)
                                              .Where(t => !t.Archived)
                                              .ToListAsync();

                return tickets;
            }
            catch
            {
                throw;
            }
        }
    }
}
