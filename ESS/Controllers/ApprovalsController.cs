using ApprovalEntriesServiceReference;
using ESS.Data;
using ESS.Helpers;
using ESS.Models;
using ESS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using System.Net;
using System.Reflection.Emit;
using System.ServiceModel;
using System.ServiceModel.Channels;

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
		public async Task<IActionResult> Index()
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

		[HttpPost]
		public async Task<IActionResult> ApproveLeave(string docNo)
		{
			string EmpCode = User.Identity.Name;

			//Create Context
			var serviceRoot = _configuration["BC_API:ODATA_ServiceRoot"] + "Company('" + _configuration["Defaults:Company"] + "')/";
			var context = new DeftOData.NAV(new Uri(serviceRoot));
			context.Credentials = new NetworkCredential(_configuration["BC_API:Username"], _configuration["BC_API:Password"]);

			BasicHttpBinding binding = new BasicHttpBinding();
			binding.Security.Mode = BasicHttpSecurityMode.Transport;
			binding.Security.Transport.ClientCredentialType = DeftFunctions.getHttpClientCredentialType();

			//OnlinePortalServices
			EndpointAddress endpointOPS = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Codeunit/DEFT_OnlinePortalServices");
			DeftSoap_OPS.DEFT_OnlinePortalServices_PortClient oPS_PortClient = new DeftSoap_OPS.DEFT_OnlinePortalServices_PortClient(binding, endpointOPS);


			if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
			{
				oPS_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
				oPS_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
			}
			else
			{
				oPS_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
			}

			//EndpointAddress endpointOPS1 = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/DEFT_ApprovalEntries");
			//ApprovalEntriesServiceReference.DEFT_ApprovalEntries_PortClient oPS_PortClient1 = new ApprovalEntriesServiceReference.DEFT_ApprovalEntries_PortClient(binding, endpointOPS);

			//List<DEFT_ApprovalEntries> approvalEntries = new List<DEFT_ApprovalEntries>();
			//List<DEFT_ApprovalEntries_Filter> dEFT_ApprovalEntries_Filters = new List<DEFT_ApprovalEntries_Filter>();
			//DEFT_ApprovalEntries_Filter dEFT_ApprovalEntries_Filter = new DEFT_ApprovalEntries_Filter();
			//dEFT_ApprovalEntries_Filter.Field = DEFT_ApprovalEntries_Fields.Approver_ID;
			//dEFT_ApprovalEntries_Filter.Criteria = ;
			//dEFT_ApprovalEntries_Filters.Add(dEFT_ApprovalEntries_Filter);

			//var result = await oPS_PortClient1.ReadMultipleAsync(dEFT_ApprovalEntries_Filters.ToArray(), null, 0);

			//Create Query
			var query = context.DEFT_ApprovalEntries;
			var r = query.ToArray();
			var recordId = r.Where(x => x.Document_No == docNo).FirstOrDefault();
			var approverId = recordId.Approver_ID;
			//DeftOData.DEFT_ApprovalEntries approvalEntries = new DeftOData.DEFT_ApprovalEntries()
			//{
			//    Approver_ID = 
			//};

			//var leaveQuery = context.CreateQuery<DeftOData.DEFT_ApprovalEntries>("DEFT_ApprovalEntries")
			//	.AddQueryOption("$filter", "Approver_ID eq '" + query);


			try
			{
				var result1 = oPS_PortClient.ApproveRequestAsync(docNo, approverId);

				if (result1 != null)
				{
					return RedirectToAction(nameof(Index));
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred");
			}

			return View("Error");
		}

		//[HttpGet]
		//public async Task<IActionResult> RejectLeave()
		//{
		//    return View();
		//}

		[HttpPost]
		public async Task<IActionResult> RejectLeave(string docNo, string rejectreason)
		{
			string EmpCode = User.Identity.Name;

			//Create Context
			var serviceRoot = _configuration["BC_API:ODATA_ServiceRoot"] + "Company('" + _configuration["Defaults:Company"] + "')/";
			var context = new DeftOData.NAV(new Uri(serviceRoot));
			context.Credentials = new NetworkCredential(_configuration["BC_API:Username"], _configuration["BC_API:Password"]);

			BasicHttpBinding binding = new BasicHttpBinding();
			binding.Security.Mode = BasicHttpSecurityMode.Transport;
			binding.Security.Transport.ClientCredentialType = DeftFunctions.getHttpClientCredentialType();

			//OnlinePortalServices
			EndpointAddress endpointOPS = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Codeunit/DEFT_OnlinePortalServices");
			DeftSoap_OPS.DEFT_OnlinePortalServices_PortClient oPS_PortClient = new DeftSoap_OPS.DEFT_OnlinePortalServices_PortClient(binding, endpointOPS);

			if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
			{
				oPS_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
				oPS_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
			}
			else
			{
				oPS_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
			}

			//try
			//{
			if (rejectreason == null)
			{
				TempData["FailMessage"] = "Reason for rejecting leave can not be empty!";
				return RedirectToAction("ViewDocument", "Approvals", new { Id = docNo});
			}
			var result = await oPS_PortClient.RejectRequestAsync(docNo, EmpCode, rejectreason);
			if (result != null)
			{
				TempData["SuccessMessage"] = "Leave request rejected successfully.";
				return RedirectToAction(nameof(Index));
			}

			return RedirectToAction(nameof(ViewDocument));

			//}
			//catch (Exception ex)
			//{
			//_logger.LogError(ex, "An error occurred");
			//}


			//return View("Error");
		}

		public IActionResult Appraisal()
		{
			return View();
		}

	}
}
