using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models.ViewModels;
using BugTracker.Models.Enums;
using BugTracker.Extensions;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyService _companyService;

        public ProjectsController(ApplicationDbContext context, 
                                  IImageService imageService,
                                  UserManager<BTUser> userManager, 
                                  IBTProjectService projectService, 
                                  IBTRolesService rolesService, 
                                  IBTCompanyService companyService)
        {
            _context = context;
            _imageService = imageService;
            _userManager = userManager;
            _projectService = projectService;
            _rolesService = rolesService;
            _companyService = companyService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            //int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            int companyId = User.Identity!.GetCompanyId();


            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            return View(projects);

        }


        //Get: AssignProjectManager
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignProjectManager(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            AssignPMViewModel model = new();

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            model.Project = await _projectService.GetProjectByIdAsync(id.Value);

            //Get Current PM (if exists)
            string? currentPMId = (await _projectService.GetProjectManagerAsync(model.Project.Id)!)?.Id;



            //Service call to RoleService
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName", currentPMId);


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectManager(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PMID))
            {

                await _projectService.AddProjectManagerAsync(model.PMID, model.Project!.Id);

                
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(AssignProjectManager), new { id = model.Project!.Id });


            ModelState!.AddModelError("PMID", "No Project Manager chosen! Please Select a PM.");


            //Get CompanyId
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            model.Project = await _projectService.GetProjectByIdAsync(model.Project!.Id);


            string? currentPMId = (await _projectService.GetProjectManagerAsync(model.Project.Id)!)?.Id;

            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName", currentPMId);
            return View(model);


        }


        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> AddMembersToProject(int? id)
        {

            if(id == null)
            {
                return NotFound();
            }

            AddMemberViewModel model = new();

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            model.Project = await _projectService.GetProjectByIdAsync(id.Value);

            model.MemberList = new MultiSelectList(await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync(), "Id", "FullName");

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMembersToProject(AddMemberViewModel model)
        {

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            if (model.MemberIds != null)
            {

                //Loop over the ids
                foreach (string memberId in model.MemberIds)
                {
                    BTUser? btUser = (await _companyService.GetCompanyMembersAsync(companyId)).FirstOrDefault(m=>m.Id == memberId);

                    await _projectService.AddUserToProjectAsync(btUser!, model.Project!.Id);
                }


                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("MemberId", "No Member was chosen. Please Select A Member");

            model.Project = await _projectService.GetProjectByIdAsync(model.Project!.Id);

            model.MemberList = new MultiSelectList(await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync(), "id", "FullName");

            return RedirectToAction(nameof(AddMembersToProject), new {id = model.Project!.Id});

        }











        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }



            var project = await _projectService.GetProjectByIdAsync(id.Value);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Create()
        {
            //ToDo: Abstract the use of _context

            AssignPMViewModel model = new();

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            model.Project = new Project();

            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName");


            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
            return View(model);
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignPMViewModel model)
        {
            if (ModelState.IsValid)
            {


                model.Project!.Created = DataUtility.GetPostGresDate(DateTime.Now);
                model.Project.StartDate = DataUtility.GetPostGresDate(model.Project.StartDate);
                model.Project.EndDate = DataUtility.GetPostGresDate(model.Project.EndDate);

                //TODO: Make companyId retrevial more efficient
                //get company Id
                model.Project.CompanyId = (await _userManager.GetUserAsync(User)).CompanyId;

                //Image
                if (model.Project.ImageFormFile != null)
                {
                    model.Project.ImageFileData = await _imageService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                    model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;

                }

                //Service

                if (User.IsInRole(nameof(BTRoles.DemoUser)))
                {
                     await _projectService.AddProjectAsync(model.Project);
                }


                int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

                string? currentPMId = (await _projectService.GetProjectManagerAsync(model.Project.Id)!)?.Id;

                model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "id", "FullName", currentPMId);

                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", model.Project!.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", model.Project!.ProjectPriorityId);
            return View(model.Project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,ProjectPriorityId,Name,Description,Created,StartDate,EndDate,ImageFormFile,ImageContentType,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    project.Created = DataUtility.GetPostGresDate(project.Created);
                    project.StartDate = DataUtility.GetPostGresDate(project.StartDate);
                    project.EndDate = DataUtility.GetPostGresDate(project.EndDate);

                    if (project.ImageFormFile != null)
                    {
                        project.ImageFileData = await _imageService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageContentType = project.ImageFormFile.ContentType;

                    }

                    //Service call
                    await _projectService.UpdateProjectAsync(project);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        public async Task<IActionResult> UnassignedProjects()
        {

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            List<Project> unassignedProjects = await _projectService.GetUnassignedProjectsAsync(companyId);

            return View(unassignedProjects);
        }



        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            List<Project> projects = await _projectService.GetArchivedProjectsByCompanyIdAsync(companyId);
            return View(projects);

        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _projectService.GetProjectByIdAsync(id.Value);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }



        // POST: Projects/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            //service call
            await _projectService.ArchiveProjectAsync(id);

            return RedirectToAction(nameof(Index));
        }



        private bool ProjectExists(int id)
        {
            return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }




        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _projectService.GetProjectByIdAsync(id.Value);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }



        // POST: Projects/Delete/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            await _projectService.RestoreProjectAsync(id);
            return RedirectToAction(nameof(Index));
        }

        //private bool ProjectExists(int id)
        //{
        //    return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        //}


    }
}


