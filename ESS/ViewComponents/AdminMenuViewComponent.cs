using Microsoft.AspNetCore.Mvc;

namespace PensionPortalWeb.ViewComponents
{
    public class AdminMenuViewComponent : ViewComponent
    {
        private readonly IConfiguration _configuration;

        public AdminMenuViewComponent(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IViewComponentResult Invoke()
        {
            string CompanyCode = HttpContext.Session.GetString(_configuration["Session:SchemeCode"]) ?? "Empty";
            string IsAdmin = HttpContext.Session.GetString(_configuration["Session:IsAdmin"]) ?? "No";
            ViewBag.IsAdmin = IsAdmin;
            return View<string>(CompanyCode);
        }

    }
}
