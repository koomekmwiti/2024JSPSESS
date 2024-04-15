using ESS.Data;
using ESS.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Net;

namespace ESS.Controllers
{

	[Authorize]
	public class NoticeBoardController : Controller
    {
        private readonly ILogger<NoticeBoardController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDBContext _dbContext;

        public NoticeBoardController(IConfiguration configuration, ApplicationDBContext dBContext, ILogger<NoticeBoardController> logger)
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
                .AddQueryOption("$filter", "No eq '" + EmpCode + "'")
                .First();

            ViewBag.DEFT_EmployeeCard = employeeCard;

            //Create Query
            var activeNoticeQuery = context.CreateQuery<DeftOData.DEFT_CompanyActivitiesCard>("DEFT_CompanyActivitiesCard")
                .AddQueryOption("$filter", "Posted_To_ESS eq true and Recalled eq false and Expired eq false");

            var recalledNoticeQuery = context.CreateQuery<DeftOData.DEFT_CompanyActivitiesCard>("DEFT_CompanyActivitiesCard")
                .AddQueryOption("$filter", "Posted_To_ESS eq true and Recalled eq true and Expired eq false");

            var elapsedNoticeQuery = context.CreateQuery<DeftOData.DEFT_CompanyActivitiesCard>("DEFT_CompanyActivitiesCard")
                .AddQueryOption("$filter", "Posted_To_ESS eq true and Recalled eq false and Expired eq true");

            ViewBag.ActiveNoticeQuery   = activeNoticeQuery;
            ViewBag.RecalledNoticeQuery = recalledNoticeQuery;
            ViewBag.ElapsedNoticeQuery  = elapsedNoticeQuery;

            return View();
        }

        public IActionResult Details(string No)
        {
			string EmpCode = User.Identity.Name;

			//Create Context
			var serviceRoot = _configuration["BC_API:ODATA_ServiceRoot"] + "Company('" + _configuration["Defaults:Company"] + "')/";
			var context = new DeftOData.NAV(new Uri(serviceRoot));
			context.Credentials = DeftFunctions.getNetworkCredential(_configuration);

			//Create Query
			DeftOData.DEFT_EmployeeCard employeeCard = context.CreateQuery<DeftOData.DEFT_EmployeeCard>("DEFT_EmployeeCard")
				.AddQueryOption("$filter", "No eq '" + EmpCode + "'")
				.First();

			ViewBag.DEFT_EmployeeCard = employeeCard;

			//Create Query
			var noticeQuery = context.CreateQuery<DeftOData.DEFT_CompanyActivitiesCard>("DEFT_CompanyActivitiesCard")
				.AddQueryOption("$filter", "Code eq '" + No + "'");

			ViewBag.DEFT_CompanyActivitiesCard = noticeQuery.First();

			//Create Query
			var activitiesLinesQuery = context.CreateQuery<DeftOData.DEFT_CompanyActivitiesLines>("DEFT_CompanyActivitiesLines")
				.AddQueryOption("$filter", "No eq '" + No + "'");

            return View(activitiesLinesQuery);

		}

    }
}
