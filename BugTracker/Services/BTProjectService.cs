using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        public async Task AddProjectAsync(Project project)
        {
            try
            {
                await _context.AddAsync(project);

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            try
            {
                BTUser? currentPM = await GetProjectManagerAsync(projectId)!;
                BTUser? selectedPM = await _context.Users.FindAsync(userId);

                //remove current PM
                if (currentPM != null)
                {
                    await RemoveProjectManagerAsync(projectId);
                }

                //Add new PM
                try
                {
                    //Project? project = await GetProjectByIdAsync(projectId);
                    await AddUserToProjectAsync(selectedPM!, projectId);

                    //if(await _rolesService.IsUserInRoleAsync(selectedPM!, nameof(BTRoles.ProjectManager)))
                    //{
                    
                    //await _context.SaveChangesAsync();
                    //}

                 
                    return true;
                }
                catch (Exception)
                {

                    throw;
                }


            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddUserToProjectAsync(BTUser user, int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId);

                bool onProject = project.Members!.Any(m => m.Id == user.Id);

                if (!onProject)
                {
                    project.Members!.Add(user);
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

        public async Task ArchiveProjectAsync(int projectId)
        {
            try
            {
                Project project = await GetProjectByIdAsync(projectId);

                if (project != null)
                {
                    project!.Archived = true;

                    foreach (Ticket ticket in project.Tickets!)
                    {
                        ticket.ArchivedByProject = true;
                    }

                await _context.SaveChangesAsync();
                }


            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
        {
            List<Project> rawProjects = await GetAllProjectsByCompanyIdAsync(companyId);

            List<Project> projects = new();

            foreach (Project project in rawProjects)
            {
                if(await GetProjectManagerAsync(project.Id)! == null)
                {
                    projects.Add(project);
                }


            }
            return projects;
        }


      
        /// <summary>
        /// Gets all none archived projects by company Id
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects.Where(p=>p.CompanyId == companyId && !p.Archived)
                                                                .Include(p => p.Company)
                                                                .Include(p => p.ProjectPriority)
                                                                .ToListAsync();

                return projects;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects
                                                        .Where(p => p.CompanyId == companyId && p.Archived)
                                                       .Include(p => p.Company)
                                                        .Include(p => p.ProjectPriority)
                                                        .ToListAsync();
                return projects;
            }
            catch
            {
                throw;
            }

        }

        public async Task<Project> GetProjectByIdAsync(int projectId)
        {

          Project? project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.Tickets)
                .Include(p => p.ProjectPriority)
                .Include(p => p.Members)
                .FirstOrDefaultAsync(m => m.Id == projectId);

            return project!;

        }

        public async Task<BTUser>? GetProjectManagerAsync(int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId);

                foreach(BTUser member in project.Members!)
                {
                    if (await _rolesService.IsUserInRoleAsync(member,nameof(BTRoles.ProjectManager)))
                    {
                        return member;
                    }
                }

                return null!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId);

                if (project != null)
                {
                    //check to see if the user is a project member
                    return project.Members!.Any(m => m.Id == userId);
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId);

                foreach (BTUser member in project.Members!)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        // Remove BTUser from Project 
                        await RemoveUserFromProjectAsync(member, projectId);

                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromProjectAsync(BTUser user, int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId);

                

                //Check if User is on project
                if (await IsUserOnProjectAsync(user.Id, projectId))
                {
                    project.Members!.Remove(user);
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

        public async Task RestoreProjectAsync(int projectId)
        {
            try
            {
                Project project = await GetProjectByIdAsync(projectId);

                if (project != null)
                {
                    project!.Archived = false;

                    foreach (Ticket ticket in project.Tickets!)
                    {
                        ticket.ArchivedByProject = false;
                    }

                }


                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);

                await _context.SaveChangesAsync();
            }
            catch(Exception)
            {
                throw;
            }

        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string roleName)
        {
            try
            {
                Project? project = await _context.Projects.Include(p=>p.Members)
                                                          .FirstOrDefaultAsync(p => p.Id == projectId);

                List<BTUser> members = new();

                foreach (BTUser btUser in project!.Members!)
                {
                    if (await _rolesService.IsUserInRoleAsync(btUser, roleName))
                    {
                        members.Add(btUser);
                    }
                }

                return members;

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
   
   

}

