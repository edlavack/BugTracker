﻿using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTCompanyService : IBTCompanyService
    {
       private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        

        public BTCompanyService(ApplicationDbContext context, UserManager<BTUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<Company> GetCompanyInfoAsync(int? companyId)
        {
            try
            {
                Company? company = new();

                if(companyId != null)
                {
                    company = await _context.Companies
                                            .Include(c => c.Members)
                                            .Include(c => c.Projects)
                                            .Include(c => c.Invites)
                                            .FirstOrDefaultAsync(c => c.Id == companyId);
                }
                return company!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetCompanyMembersAsync(int companyId)
        {
            try
            {
                List<BTUser>? members = new();

                members = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();

                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
