using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using BugTracker.Models.ViewModels;
using BugTracker.Extensions;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketHistoryService _ticketHistoryService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTFileService _fileService;

        public TicketsController(ApplicationDbContext context,
                                 IImageService imageService,
                                 UserManager<BTUser> userManager,
                                  IBTTicketService ticketService,
                                  IBTRolesService rolesService,
                                  IBTProjectService projectService,
                                  IBTNotificationService notificationService,
                                  IBTTicketHistoryService ticketHistoryService,
                                  IBTFileService fileService)
        {
            _context = context;
            _imageService = imageService;
            _userManager = userManager;
            _ticketService = ticketService;
            _rolesService = rolesService;
            _projectService = projectService;
            _notificationService = notificationService;
            _ticketHistoryService = ticketHistoryService;
            _fileService = fileService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);

            return View(tickets);
        }










        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                //ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                ticketAttachment.UserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id, UserId, TicketId, Comment")] TicketComment ticketComment, int ticketId, string? commentBody)
        {

            ModelState.Remove("UserId");
            ModelState.Remove("Comment");



            if (ModelState.IsValid)
            {

                try
                {
                    Ticket ticket = await _ticketService.GetTicketByIdAsync(ticketId);

                    ticketComment.UserId = _userManager.GetUserId(User);
                    ticketComment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                    ticketComment.Comment = commentBody;

                    await _ticketService.AddTicketCommentAsync(ticketComment, ticketId);

                    await _ticketHistoryService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId);
                }
                catch (Exception)
                {

                    throw;
                }

            }

            return RedirectToAction(nameof(Index));

        }


     



        #region GET for Assign Developer
        //Get: AssignDeveloper

        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignDeveloper(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            AssignDevViewModel model = new();

            //int projectId = await _projectService.GetProjectByIdAsync(id.Value);

            model.Ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            //Get Current PM (if exists)
            string? currentDevId = (await _ticketService.GetDeveloperAsync(model.Ticket.Id)!)?.Id;



            //Service call to RoleService
            model.DevList = new SelectList(await _projectService.GetProjectMembersByRoleAsync(model.Ticket.ProjectId, nameof(BTRoles.Developer)), "Id", "FullName", currentDevId);


            return View(model);
        }
        #endregion

        #region POST for Assign Developer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDevViewModel model)
        {
            if (!string.IsNullOrEmpty(model.DevID))
            {

                await _ticketService.AddDeveloperAsync(model.Ticket!.Id, model.DevID);


                BTUser btUser = await _userManager.GetUserAsync(User);


                Notification notification = new()
                {
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id,
                    TicketId = model.Ticket!.Id,
                    Title = "New Ticket Added",
                    Message = $"New Ticket:{model.Ticket.Title} was created by {btUser.FullName}",
                    Created = DataUtility.GetPostGresDate(DateTime.UtcNow),
                    SenderId = btUser.Id,
                    RecipientId = model.DevID

                };
                await _notificationService.AddNotificationAsync(notification);
                await _notificationService.SendEmailNotificationAsync(notification, "Ticket Assigned");

                return RedirectToAction(nameof(Index));
            }






            ModelState.AddModelError("DevID", "No Developer chosen! Please Select a Developer.");

            //Get Current PM (if exists)
            string? currentDevId = (await _ticketService.GetDeveloperAsync(model.Ticket!.Id)!)?.Id;



            //Service call to RoleService
            model.DevList = new SelectList(await _projectService.GetProjectMembersByRoleAsync(model.Ticket.ProjectId, nameof(BTRoles.Developer)), "Id", "FullName", currentDevId);


            return View(model);


        }
        #endregion





        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.Comments)
                    .ThenInclude(t => t.User)
                .Include(t => t.History)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description");
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            //ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Id");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");

            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Created,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            ticket.SubmitterUserId = _userManager.GetUserId(User);

            ModelState.Remove("SubmitterUserId");

            if (ModelState.IsValid)
            {
                int statusId = (await _context.TicketStatuses.FirstOrDefaultAsync(s => s.Name == nameof(BTTicketStatuses.New)))!.Id;

                ticket.TicketStatusId = statusId;
                ticket.Created = DataUtility.GetPostGresDate(DateTime.Now);
                ticket.SubmitterUserId = _userManager.GetUserId(User);



                await _ticketService.AddTicketAsync(ticket);

                int company = User.Identity!.GetCompanyId();

                string userId = _userManager.GetUserId(User);



                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, company);
                await _ticketHistoryService.AddHistoryAsync(null!, newTicket, userId);



                //_context.Add(ticket);
                //await _context.SaveChangesAsync();
                BTUser btUser = await _userManager.GetUserAsync(User);
                BTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId)!;

                Notification notification = new()
                {
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes)))!.Id,
                    TicketId = ticket.Id,
                    Title = "New Ticket Added",
                    Message = $"New Ticket:{ticket.Title} was created by {btUser.FullName}",
                    Created = DataUtility.GetPostGresDate(DateTime.UtcNow),
                    SenderId = userId,
                    RecipientId = projectManager?.Id

                };


                await _notificationService.AddNotificationAsync(notification);
                if (projectManager != null)
                {
                    await _notificationService.SendEmailNotificationAsync(notification, $"New Ticket Added For Project: {ticket.Project!.Name}");
                }
                else
                {
                    notification.RecipientId = userId;
                    await _notificationService.SendEmailNotificationAsync(notification, $"New Ticket Added For Project: {ticket.Project!.Name}");

                }


                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            //ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                int companyId = User.Identity!.GetCompanyId();
                string userId = _userManager.GetUserId(User);
                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);
                try
                {



                    ticket.Updated = DataUtility.GetPostGresDate(ticket.Created);
                    ticket.Created = DataUtility.GetPostGresDate(DateTime.Now);
                    ticket.SubmitterUserId = _userManager.GetUserId(User);


                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                //Add History 
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);
                await _ticketHistoryService.AddHistoryAsync(oldTicket, newTicket, userId);


                //Add Notification 



                return RedirectToAction(nameof(Index));
            }
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            //ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }


        public async Task<IActionResult> UnassignedTickets()
        {

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            List<Ticket> unassignedTickets = await _ticketService.GetUnassignedTicketsAsync(companyId);

            return View(unassignedTickets);
        }





        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetArchivedTicketsByCompanyIdAsync(companyId);
            return View(tickets);

        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            //service call
            await _ticketService.ArchiveTicketAsync(id);

            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }




        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            await _ticketService.RestoreTicketAsync(id);
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            //string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData!;
            //string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename=");
            return File(fileData!, $"application/");
        }






    }
}
