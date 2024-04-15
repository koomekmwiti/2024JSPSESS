using ESS.Data;
using ESS.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ESS.Controllers
{

	[Authorize]
	public class ApprovalsController : Controller
    {

		private readonly ILogger<ApprovalsController> _logger;
		private readonly IConfiguration _configuration;
		private readonly ApplicationDBContext _dbContext;
		public ApprovalsController(IConfiguration configuration, ApplicationDBContext dBContext, ILogger<ApprovalsController> logger)
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
			var leaveQuery = context.CreateQuery<DeftOData.DEFT_ApprovalEntries>("DEFT_ApprovalEntries")
				.AddQueryOption("$filter", "Approver_Staff_No eq '" + EmpCode + "' and Document_Type eq 'LeaveApplication' and Status eq 'Open'");

			ViewBag.LeaveQuery = leaveQuery;

			return View();
		}

		public IActionResult ViewDocument(string Id)
		{
			string EmpCode = User.Identity.Name;

			//Create Context
			var serviceRoot = _configuration["BC_API:ODATA_ServiceRoot"] + "Company('" + _configuration["Defaults:Company"] + "')/";
			var context = new DeftOData.NAV(new Uri(serviceRoot));
			context.Credentials = new NetworkCredential(_configuration["BC_API:Username"], _configuration["BC_API:Password"]);

			//Create Query
			var approvalQuery = context.CreateQuery<DeftOData.DEFT_ApprovalEntries>("DEFT_ApprovalEntries")
				.AddQueryOption("$filter", "Approver_Staff_No eq '" + EmpCode + "' and Document_No eq '" + Id + "'")
				.AddQueryOption("$count", "true");

			int approvalCount = approvalQuery.Count();

			if (approvalCount > 0)
			{
				//Create Query
				var leaveQuery = context.CreateQuery<DeftOData.DEFT_EmployeeLeaveApplicationCard>("DEFT_EmployeeLeaveApplicationCard")
					.AddQueryOption("$filter", "Application_No eq '" + Id + "'");

				DeftOData.DEFT_EmployeeLeaveApplicationCard leave = leaveQuery.FirstOrDefault();

				return View(leave);
			}
			else
			{
				return RedirectToAction(nameof(Index));
			}
		}


		public IActionResult Appraisal() { 
			return View(); 
		}

	}
}
