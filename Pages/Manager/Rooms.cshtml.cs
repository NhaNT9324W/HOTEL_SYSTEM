using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hotel_System.Pages.Manager
{
    [Authorize(Policy = "AdminOrManager")]
    public class RoomsModel : PageModel
    {
        public void OnGet() { }
    }
}