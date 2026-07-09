using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hotel_System.Pages.RoomStaff
{
    [Authorize(Policy = "RoomStaffOnly")]
    public class TasksModel : PageModel
    {
        public void OnGet() { }
    }
}