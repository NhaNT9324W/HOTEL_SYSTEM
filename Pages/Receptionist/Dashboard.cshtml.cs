using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hotel_System.Pages.Receptionist
{
    [Authorize(Policy = "Receptionist")]
    public class DashboardModel : PageModel
    {
        public void OnGet() { }
    }
}