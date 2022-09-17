using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class AddMemberViewModel
    {
        public Project? Project { get; set; }
        public MultiSelectList? MemberList { get; set; }
        public List<string>? MemberIds { get; set; }


    }
}
