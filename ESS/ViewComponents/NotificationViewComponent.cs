using ESS.Data;
using ESS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PensionPortalWeb.ViewComponents
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly ApplicationDBContext _context;

        public NotificationViewComponent(ApplicationDBContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke(string username) 
        {
            //IQueryable<GLNotification> Notifications = _context.GLNotification.Include(c => c.GLUserScheme).Where(c => c.GLUserScheme.Email.Equals(username) && c.IsRead.Equals(false));

            //return View(Notifications);  
            return View();

        }
    }
}
