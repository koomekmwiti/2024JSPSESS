using ApprovalGoalsServiceReference;
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
using System.ServiceModel.Channels;
using System.ServiceModel;
using DeftOData;
using AppraisalListServiceReference;
using AppraiseeAppraisalCommentsServiceReference;
using DeftSoap_OPS;
using SupervisorCommentsServiceReference;

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
				return RedirectToAction("ViewDocument", "Approvals", new { Id = docNo });
			}
			var result = await oPS_PortClient.RejectRequestAsync(docNo, EmpCode, rejectreason);
			if (result != null)
			{
				TempData["SuccessMessage"] = "Leave request rejected successfully.";
				return RedirectToAction(nameof(Index));
			}
			return RedirectToAction(nameof(Index));
		}


		public async Task<IActionResult> Appraisal()
		{
			string empCode = User.Identity.Name;

			List<AppraisalListServiceReference.DEFT_AppraisalList> appraisalLists = new List<AppraisalListServiceReference.DEFT_AppraisalList>();
			BasicHttpBinding binding = new BasicHttpBinding();
			binding.Security.Mode = BasicHttpSecurityMode.Transport;
			//binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
			binding.Security.Transport.ClientCredentialType = DeftFunctions.getHttpClientCredentialType();

			EndpointAddress endpointAppraisalGoals = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/DEFT_AppraisalList");

			DEFT_AppraisalList_PortClient dEFT_AppraisalList_PortClient = new DEFT_AppraisalList_PortClient(binding, endpointAppraisalGoals);

			if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
			{
				dEFT_AppraisalList_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
				dEFT_AppraisalList_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
			}
			else
			{
				dEFT_AppraisalList_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
			}

			List<AppraisalListServiceReference.DEFT_AppraisalList_Filter> filters = new List<AppraisalListServiceReference.DEFT_AppraisalList_Filter>();

			DEFT_AppraisalList_Filter appraisalList_Filter = new DEFT_AppraisalList_Filter();

			appraisalList_Filter.Field = AppraisalListServiceReference.DEFT_AppraisalList_Fields.Appraiser_No;
			appraisalList_Filter.Criteria = empCode;

			filters.Add(appraisalList_Filter);

			DEFT_AppraisalList_Filter appraisalList_Filter1 = new DEFT_AppraisalList_Filter();

			appraisalList_Filter1.Field = AppraisalListServiceReference.DEFT_AppraisalList_Fields.Appraisal_Status;
			appraisalList_Filter1.Criteria = AppraisalListServiceReference.Appraisal_Status.Set.ToString();

			filters.Add(appraisalList_Filter1);

			var result = await dEFT_AppraisalList_PortClient.ReadMultipleAsync(filters.ToArray(), null, 0);

			foreach (var appraisalGoalsEntry in result.ReadMultiple_Result1)
			{
				AppraisalListServiceReference.DEFT_AppraisalList appraisalList = new AppraisalListServiceReference.DEFT_AppraisalList
				{
					Key = appraisalGoalsEntry.Key,
					Appraisal_No = appraisalGoalsEntry.Appraisal_No,
					Employee_No = appraisalGoalsEntry.Employee_No,
					Appraisee_Name = appraisalGoalsEntry.Appraisee_Name,
					Appraisees_Job_Title = appraisalGoalsEntry.Appraisees_Job_Title,
					Appraiser_No = appraisalGoalsEntry.Appraiser_No
				};

				appraisalLists.Add(appraisalList);
			}
			return View(appraisalLists);
		}

		public IActionResult Details(string Id)
		{
			string EmpCode = User.Identity.Name;

			if (ModelState.IsValid)
			{
				//Create Context
				var serviceRoot = _configuration["BC_API:ODATA_ServiceRoot"] + "Company('" + _configuration["Defaults:Company"] + "')/";
				var context = new DeftOData.NAV(new Uri(serviceRoot));
				context.Credentials = DeftFunctions.getNetworkCredential(_configuration);

				//Create Query
				DeftOData.DEFT_AppraisalCard appraisalCard = context.CreateQuery<DeftOData.DEFT_AppraisalCard>("DEFT_AppraisalCard")
					.AddQueryOption("$filter", "Appraisal_No eq '" + Id + "'")
					.First();

				ViewBag.AppraisalCard = appraisalCard;

				return View();
			}

			return RedirectToAction(nameof(ViewDocument));

			//}
			//catch (Exception ex)
			//{
			//_logger.LogError(ex, "An error occurred");
			//}
		}


		[HttpGet]
		public IActionResult PopulateAppraisalGoals()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> PopulateAppraisalGoals(string appNo, string empNo)
		{
			List<AppraisalGoalsEntries> appraisalGoalsEntries = new List<AppraisalGoalsEntries>();
			BasicHttpBinding binding = new BasicHttpBinding();
			binding.Security.Mode = BasicHttpSecurityMode.Transport;
			//binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
			binding.Security.Transport.ClientCredentialType = DeftFunctions.getHttpClientCredentialType();

			EndpointAddress endpointAppraisalGoals = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/AppraisalGoalsEntries");

			AppraisalGoalsEntries_PortClient appraisalGoalsEntries_PortClient = new AppraisalGoalsEntries_PortClient(binding, endpointAppraisalGoals);

			if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
			{
				appraisalGoalsEntries_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
				appraisalGoalsEntries_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
			}
			else
			{
				appraisalGoalsEntries_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
			}

			List<AppraisalGoalsEntries_Filter> filters = new List<AppraisalGoalsEntries_Filter>();

			AppraisalGoalsEntries_Filter appraisalGoalsEntries_Filter = new AppraisalGoalsEntries_Filter();

			appraisalGoalsEntries_Filter.Field = AppraisalGoalsEntries_Fields.Appraisal_No;
			appraisalGoalsEntries_Filter.Criteria = appNo;
			filters.Add(appraisalGoalsEntries_Filter);
			var result = await appraisalGoalsEntries_PortClient.ReadMultipleAsync(filters.ToArray(), null, 0);

			foreach (var appraisalGoalsEntry in result.ReadMultiple_Result1)
			{
				AppraisalGoalsEntries appraisalGoals = new AppraisalGoalsEntries
				{
					Key = appraisalGoalsEntry.Key,
					Line_No = appraisalGoalsEntry.Line_No,
					Line_NoSpecified = true,
					Objective_Code = appraisalGoalsEntry.Objective_Code,
					Key_Responsibility = appraisalGoalsEntry.Key_Responsibility,
					Initiative_code = appraisalGoalsEntry.Initiative_code,
					Description = appraisalGoalsEntry.Description,
					Self_Rating = appraisalGoalsEntry.Self_Rating,
					Self_RatingSpecified = true,
					Rating = appraisalGoalsEntry.Rating,
					RatingSpecified = true,
					Maximum_Score = appraisalGoalsEntry.Maximum_Score,
					Maximum_ScoreSpecified = true,
					Agreed_perfomance_targets = appraisalGoalsEntry.Agreed_perfomance_targets
				};

				appraisalGoalsEntries.Add(appraisalGoals);
			}

			ViewBag.AppNo = appNo;

			ViewBag.EmpNo = empNo;

			return View(appraisalGoalsEntries);
		}

		[HttpPost]
		public async Task<IActionResult> SubmitAppraisalGoals(List<AppraisalGoalsEntries> appraisalGoalsEntries, string appNo)
		{
			BasicHttpBinding binding = new BasicHttpBinding();
			binding.Security.Mode = BasicHttpSecurityMode.Transport;
			//binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
			binding.Security.Transport.ClientCredentialType = DeftFunctions.getHttpClientCredentialType();


			EndpointAddress endpointAPC = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/AppraisalGoalsEntries");
			AppraisalGoalsEntries_PortClient appraisalGoalsEntries_PortClient = new AppraisalGoalsEntries_PortClient(binding, endpointAPC);

			if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
			{
				appraisalGoalsEntries_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
				appraisalGoalsEntries_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
			}
			else
			{
				appraisalGoalsEntries_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
			}

			var updateMultiple = new ApprovalGoalsServiceReference.UpdateMultiple
			{
				AppraisalGoalsEntries_List = appraisalGoalsEntries.ToArray()
			};

			await appraisalGoalsEntries_PortClient.UpdateMultipleAsync(updateMultiple);

			//appraisalGoalsEntries_PortClient.UpdateAsync(appraisalGoalsEntries);
			return RedirectToAction("AppraisalQuestions", "Approvals", new { appNo = appNo });
		}

		[HttpGet]
		public IActionResult AppraisalQuestions(string appNo)
		{
			List<SecondSupervisorComment> secondSupervisorComments = new List<SecondSupervisorComment>();

			SecondSupervisorComment secondSupervisorComment4 = new SecondSupervisorComment
			{
				No = 4,
				Question = "1. Appraisee's Appraisal Comments On The Performance Appraisal",
				key = "",
				comments_on_Performance = string.Empty,
				appraisal_No = "",
				person = SupervisorCommentsServiceReference.Person.PAppraiser,
				personFieldSpecified = true,
				Table = "Appraisee's Appraisal Comments"
			};

			secondSupervisorComments.Add(secondSupervisorComment4);

			SecondSupervisorComment secondSupervisorComment5 = new SecondSupervisorComment
			{
				No = 5,
				Question = "2. Appraiser's Comments On The Performance Appraisal",
				key = "",
				comments_on_Performance = string.Empty,
				appraisal_No = "",
				person = SupervisorCommentsServiceReference.Person.PAppraiser,
				personFieldSpecified = true,
				Table = "Second Supervisor Comments"
			};

			secondSupervisorComments.Add(secondSupervisorComment5);

			SecondSupervisorComment secondSupervisorComment6 = new SecondSupervisorComment
			{
				No = 6,
				Question = "3.Departmental Head's Comments (If not the APPRAISER)",
				key = "",
				comments_on_Performance = string.Empty,
				appraisal_No = "",
				person = SupervisorCommentsServiceReference.Person.Second_Supervisor,
				personFieldSpecified = true,
				Table = "Second Supervisor Comments"
			};

			secondSupervisorComments.Add(secondSupervisorComment6);

			SecondSupervisorComment secondSupervisorComment7 = new SecondSupervisorComment
			{
				No = 7,
				Question = "4. Developmental Action To Be Taken",
				key = "",
				comments_on_Performance = string.Empty,
				appraisal_No = "",
				person = SupervisorCommentsServiceReference.Person.Dev_Action,
				personFieldSpecified = true,
				Table = "Second Supervisor Comments"
			};

			secondSupervisorComments.Add(secondSupervisorComment7);

			ViewBag.AppNo = appNo;

			return View(secondSupervisorComments);
		}

		[HttpPost]
		public async Task<IActionResult> SubmitAppraisalQuestions(List<SecondSupervisorComment> secondSupervisorComment)
		{
			BasicHttpBinding binding = new BasicHttpBinding();
			binding.Security.Mode = BasicHttpSecurityMode.Transport;
			//binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
			binding.Security.Transport.ClientCredentialType = DeftFunctions.getHttpClientCredentialType();


			EndpointAddress endpointAPC = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/SecondSuperVisorComments");

			SecondSuperVisorComments_PortClient secondSuperVisorComments_PortClient = new SecondSuperVisorComments_PortClient(binding, endpointAPC);

			if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
			{
				secondSuperVisorComments_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
				secondSuperVisorComments_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
			}
			else
			{
				secondSuperVisorComments_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
			}

			EndpointAddress eppraiseeAppraisalCommentsendpointAPC = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/AppraiseeAppraisalComments");

			AppraiseeAppraisalComments_PortClient appraiseeAppraisalComments_PortClient = new AppraiseeAppraisalComments_PortClient(binding, eppraiseeAppraisalCommentsendpointAPC);

			if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
			{
				appraiseeAppraisalComments_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
				appraiseeAppraisalComments_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
			}
			else
			{
				appraiseeAppraisalComments_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
			}

			List<AppraisalGoalsEntries> appraisalGoalsEntries = new List<AppraisalGoalsEntries>();
			binding.Security.Mode = BasicHttpSecurityMode.Transport;
			//binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
			binding.Security.Transport.ClientCredentialType = DeftFunctions.getHttpClientCredentialType();

			// Appraisal Card
			EndpointAddress endpointOnlinePortalService = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Codeunit/DEFT_OnlinePortalServices");

			DEFT_OnlinePortalServices_PortClient dEFT_OnlinePortalServices_PortClient = new DEFT_OnlinePortalServices_PortClient(binding, endpointOnlinePortalService);

			if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
			{
				dEFT_OnlinePortalServices_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
				dEFT_OnlinePortalServices_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
			}
			else
			{
				dEFT_OnlinePortalServices_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
			}

			foreach (var item in secondSupervisorComment)
			{
				if (item.Table == "Second Supervisor Comments")
				{
					SecondSuperVisorComments second = new SecondSuperVisorComments
					{
						Key = item.key,
						Comments_on_Performance = item.comments_on_Performance,
						Appraisal_No = item.appraisal_No,
						Person = item.person,
						PersonSpecified = true
					};

					var create = new SupervisorCommentsServiceReference.Create
					{
						SecondSuperVisorComments = second,
					};

					await secondSuperVisorComments_PortClient.CreateAsync(create);
				}
				else if (item.Table == "Appraisee's Appraisal Comments")
				{
					AppraiseeAppraisalComments second = new AppraiseeAppraisalComments
					{
						Key = item.key,
						Comments_on_Performance = item.comments_on_Performance,
						Appraisal_No = item.appraisal_No,
						Person = AppraiseeAppraisalCommentsServiceReference.Person.PAppraisee,
						PersonSpecified = true
					};

					var appraiseeAppraisalCommentCreate = new AppraiseeAppraisalCommentsServiceReference.Create
					{
						AppraiseeAppraisalComments = second,
					};

					await appraiseeAppraisalComments_PortClient.CreateAsync(appraiseeAppraisalCommentCreate);
				}
			}

			if (secondSupervisorComment.FirstOrDefault().appraisal_No != null && secondSupervisorComment.FirstOrDefault().appraisal_No != string.Empty)
			{
				DEFT_OnlinePortalServiceReference.DEFT_OnlinePortalServices_PortClient portClient = new DEFT_OnlinePortalServiceReference.DEFT_OnlinePortalServices_PortClient(binding, endpointOnlinePortalService);

				if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
				{
					portClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
					portClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
				}
				else
				{
					portClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
				}

				await portClient.SendForFurtherReviewAsync(secondSupervisorComment.FirstOrDefault().appraisal_No);

				TempData["Message"] = "Appraisal Sent for further review";

				return RedirectToAction("Appraisal");
			}

			return View();
		}

	}
}

