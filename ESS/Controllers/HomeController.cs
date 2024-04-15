using ESS.Data;
using ESS.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Diagnostics.Metrics;
using System.Net;

namespace ESS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDBContext _dbContext;

        public HomeController(IConfiguration configuration, ApplicationDBContext dBContext, ILogger<HomeController> logger)
        {
            _configuration = configuration;
            _dbContext = dBContext;
            _logger = logger;
        }
        public IActionResult Index()
        {
            string EmpCode = User.Identity.Name;

            //Create Context
            var serviceRoot = _configuration["BC_API:ODATA_ServiceRoot"] + "Company('" + _configuration["Defaults:Company"] + "')/";
            var context = new DeftOData.NAV(new Uri(serviceRoot));
            context.Credentials = DeftFunctions.getNetworkCredential(_configuration);

			//Create Query
			DeftOData.DEFT_EmployeeCard employeeCard = context.CreateQuery<DeftOData.DEFT_EmployeeCard>("DEFT_EmployeeCard")
                .AddQueryOption("$filter","No eq '" + EmpCode + "'")
                .First();

            ViewBag.EmployeeCard = employeeCard;

            return View();
        }
    }
}
