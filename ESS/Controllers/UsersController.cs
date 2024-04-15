using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESS.Controllers
{
	[Authorize]
	public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
