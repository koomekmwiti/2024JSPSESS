using Microsoft.AspNetCore.Mvc;

namespace PensionPortalWeb.ViewComponents
{
    public class UserViewComponent : ViewComponent
    {
        public UserViewComponent()
        {
            
        }

        public IViewComponentResult Invoke() 
        { 
            return View(); 
        }
    }
}
