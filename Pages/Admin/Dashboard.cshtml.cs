using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hotel_System.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class DashboardModel : PageModel
    {
        public void OnGet() { }
    }
}