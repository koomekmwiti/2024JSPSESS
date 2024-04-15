using Microsoft.AspNetCore.Mvc;

namespace PensionPortalWeb.ViewComponents
{
    public class DCMenuKEViewComponent: ViewComponent
    {
        public DCMenuKEViewComponent()
        {
            
        }

        public IViewComponentResult Invoke()
        {
            string Role = HttpContext.Session.GetString("Role") ?? "Empty";
            return View<string>(Role);
        }
    }
}
