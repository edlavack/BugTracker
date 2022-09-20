﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class AssignDevViewModel
    {
        public Ticket? Ticket { get; set; }
        public SelectList? DevList { get; set; }
        public string? DevID { get; set; }
    }
}
