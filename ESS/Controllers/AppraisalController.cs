using AppraiseeAppraisalCommentsServiceReference;
using ApprovalGoalsServiceReference;
using DeftOData;
using DeftSoap_AppraisalCard;
using DeftSoap_OPS;
using ESS.Data;
using ESS.Helpers;
using ESS.Models;
using ESS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SupervisorCommentsServiceReference;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Net;
using System.Reflection.Emit;
using System.ServiceModel;
using System.ServiceModel.Channels;
using UnitSubjectServiceReference;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ESS.Controllers
{
	[Authorize]
	public class AppraisalController : Controller
	{
		private readonly ILogger<AppraisalController> _logger;
		private readonly IConfiguration _configuration;
		private readonly ApplicationDBContext _dbContext;

		public AppraisalController(IConfiguration configuration, ApplicationDBContext dBContext, ILogger<AppraisalController> logger)
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
			context.Credentials = new NetworkCredential(_configuration["BC_API:Username"], _configuration["BC_API:Password"]);


			//Create Query
			var appraisalQuery = context.CreateQuery<DeftOData.DEFT_AppraisalCard>("DEFT_AppraisalCard")
				.AddQueryOption("$filter", "Employee_No eq '" + EmpCode + "'");

			ViewBag.AppraisalQuery = appraisalQuery;

			return View();
		}

		public IActionResult RegisterId()
		{
			string EmpCode = User.Identity.Name;

			Guid Id = Guid.NewGuid();

			DeftAddAppraisal deftAddAppraisal = new DeftAddAppraisal();
			deftAddAppraisal.Id = Id;
			deftAddAppraisal.EmpCode = EmpCode;
			_dbContext.DeftAddAppraisal.Add(deftAddAppraisal);
			_dbContext.SaveChanges();

			return RedirectToAction(nameof(AddAppraisal), new { Id = Id });
		}

		public IActionResult AddAppraisal()
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

			ViewBag.EmployeeCard = employeeCard;

			//Create Query
			var appraisalPeriodsQuery = context.CreateQuery<DeftOData.DEFT_AppraisalPeriods>("DEFT_AppraisalPeriods")
				.AddQueryOption("$orderby", "Start_Date desc");

			ViewBag.AppraisalPeriodsQuery = appraisalPeriodsQuery;

			//Create Query
			var subjectUnits = context.CreateQuery<UnitSubjectServiceReference.unitsubject>("unitsubject")
				.AddQueryOption("$orderby", "Description desc");

			ViewBag.SubjectUnits = subjectUnits;

			AppraisalCardVM appraisalCardVM = new AppraisalCardVM();

			return View(appraisalCardVM);
		}

		[HttpPost]
		public IActionResult AddAppraisal(AppraisalCardVM appraisalCardVM)
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

			ViewBag.EmployeeCard = employeeCard;

			//Create Query
			var appraisalPeriodsQuery = context.CreateQuery<DeftOData.DEFT_AppraisalPeriods>("DEFT_AppraisalPeriods")
				.AddQueryOption("$orderby", "Start_Date desc"); ;

			ViewBag.AppraisalPeriodsQuery = appraisalPeriodsQuery;

			if (ModelState.IsValid)
			{

				BasicHttpBinding binding = new BasicHttpBinding();
				binding.Security.Mode = BasicHttpSecurityMode.Transport;
				//binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
				binding.Security.Transport.ClientCredentialType = DeftFunctions.getHttpClientCredentialType();

				// Appraisal Card
				EndpointAddress endpointAPC = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/DEFT_AppraisalCard");
				DeftSoap_AppraisalCard.DEFT_AppraisalCard_PortClient aPC_PortClient = new DeftSoap_AppraisalCard.DEFT_AppraisalCard_PortClient(binding, endpointAPC);

				if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
				{
					aPC_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
					aPC_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
				}
				else
				{
					aPC_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
				}

				DeftSoap_AppraisalCard.DEFT_AppraisalCard appraisalCard = new DeftSoap_AppraisalCard.DEFT_AppraisalCard();
				appraisalCard.Employee_No = EmpCode;
				appraisalCard.Appraisal_Subject = appraisalCardVM.Appraisal_Subject;

				try
				{
					aPC_PortClient.Create(ref appraisalCard);
					TempData["Message"] = "Appraisal header created successfully. Please use the header to complete your appraisal.";
				}
				catch (Exception ex)
				{
					TempData["Message"] = ex.Message;
				}

				return RedirectToAction(nameof(Index));

			}

			return View(appraisalCardVM);
		}

		public IActionResult AddAppraisalCard()
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

			ViewBag.EmployeeCard = employeeCard;

			//Create Query
			var appraisalPeriodsQuery = context.CreateQuery<DeftOData.DEFT_AppraisalPeriods>("DEFT_AppraisalPeriods")
				.AddQueryOption("$orderby", "Start_Date desc"); ;

			ViewBag.AppraisalPeriodsQuery = appraisalPeriodsQuery;

			BasicHttpBinding binding = new BasicHttpBinding();
			binding.Security.Mode = BasicHttpSecurityMode.Transport;
			//binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
			binding.Security.Transport.ClientCredentialType = DeftFunctions.getHttpClientCredentialType();

			// Appraisal Card
			EndpointAddress endpointAPC = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/DEFT_AppraisalCard");
			DeftSoap_AppraisalCard.DEFT_AppraisalCard_PortClient aPC_PortClient = new DeftSoap_AppraisalCard.DEFT_AppraisalCard_PortClient(binding, endpointAPC);

			if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
			{
				aPC_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
				aPC_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
			}
			else
			{
				aPC_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
			}

			DeftSoap_AppraisalCard.DEFT_AppraisalCard appraisalCard = new DeftSoap_AppraisalCard.DEFT_AppraisalCard();
			appraisalCard.Employee_No = EmpCode;

			try
			{
				aPC_PortClient.Create(ref appraisalCard);
				TempData["Message"] = "Appraisal header created successfully. Please use the header to complete your appraisal.";
			}
			catch (Exception ex)
			{
				TempData["Message"] = ex.Message;
			}

			return RedirectToAction(nameof(Index));

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
					.AddQueryOption("$filter", "Appraisal_No eq '" + Id + "' and Employee_No eq   '" + EmpCode + "'")
					.First();

				ViewBag.AppraisalCard = appraisalCard;
				 
				return View();
			}

			return RedirectToAction(nameof(Index));

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

			// Appraisal Card
			EndpointAddress endpointAPC = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Codeunit/DEFT_OnlinePortalServices");
			PopulateAppraisalGoals populateAppraisalGoals = new PopulateAppraisalGoals();

			populateAppraisalGoals.appNo = appNo;

			populateAppraisalGoals.employeeNo = empNo;

			DEFT_OnlinePortalServices_PortClient dEFT_OnlinePortalServices_PortClient = new DEFT_OnlinePortalServices_PortClient(binding, endpointAPC);

			if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
			{
				dEFT_OnlinePortalServices_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
				dEFT_OnlinePortalServices_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
			}
			else
			{
				dEFT_OnlinePortalServices_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
			}

			dEFT_OnlinePortalServices_PortClient.PopulateAppraisalGoals(populateAppraisalGoals.appNo, populateAppraisalGoals.employeeNo);

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

			EndpointAddress endpointSecondSuperVisorAppraisalGoals = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/SecondSuperVisorComments");

			SecondSuperVisorComments_PortClient secondSuperVisorComments_PortClient = new SecondSuperVisorComments_PortClient(binding, endpointSecondSuperVisorAppraisalGoals);

			if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
			{
				secondSuperVisorComments_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
				secondSuperVisorComments_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
			}
			else
			{
				secondSuperVisorComments_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
			}

			List<SecondSuperVisorComments_Filter> secondSuperVisorComments_Filters = new List<SecondSuperVisorComments_Filter>();

			SecondSuperVisorComments_Filter superVisorComments_Filter = new SecondSuperVisorComments_Filter();

			superVisorComments_Filter.Field = SecondSuperVisorComments_Fields.Appraisal_No;

			superVisorComments_Filter.Criteria = appNo;

			secondSuperVisorComments_Filters.Add(superVisorComments_Filter);

			var resultSupervisorComments = await secondSuperVisorComments_PortClient.ReadMultipleAsync(secondSuperVisorComments_Filters.ToArray(), null, 0);

			List<SecondSuperVisorComments> second = new List<SecondSuperVisorComments>();

			foreach (var resultSupervisor in resultSupervisorComments.ReadMultiple_Result1)
			{
				SecondSuperVisorComments superVisorComments = new SecondSuperVisorComments
				{
					Comments_on_Performance = resultSupervisor.Comments_on_Performance
				};

				second.Add(superVisorComments);
			}

			EndpointAddress endpointAppraiseeAppraisalAppraisalGoals = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/AppraiseeAppraisalComments");

			AppraiseeAppraisalComments_PortClient appraiseeAppraisalComments_PortClient = new AppraiseeAppraisalComments_PortClient(binding, endpointAppraiseeAppraisalAppraisalGoals);

			if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
			{
				appraiseeAppraisalComments_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
				appraiseeAppraisalComments_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
			}
			else
			{
				appraiseeAppraisalComments_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
			}

			List<AppraiseeAppraisalComments_Filter> appraiseeAppraisalComments_Filters = new List<AppraiseeAppraisalComments_Filter>();

			AppraiseeAppraisalComments_Filter appraisalComments_Filter = new AppraiseeAppraisalComments_Filter();

			appraisalComments_Filter.Field = AppraiseeAppraisalComments_Fields.Appraisal_No;

			appraisalComments_Filter.Criteria = appNo;

			appraiseeAppraisalComments_Filters.Add(appraisalComments_Filter);

			var resultAppraisalComments = await appraiseeAppraisalComments_PortClient.ReadMultipleAsync(appraiseeAppraisalComments_Filters.ToArray(), null, 0);

			List<AppraiseeAppraisalComments> appraiseeAppraisalComments = new List<AppraiseeAppraisalComments>();

			foreach (var resultSupervisor in resultSupervisorComments.ReadMultiple_Result1)
			{
				AppraiseeAppraisalComments appraisee = new AppraiseeAppraisalComments
				{
					Comments_on_Performance = resultSupervisor.Comments_on_Performance
				};

				appraiseeAppraisalComments.Add(appraisee);
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
			return RedirectToAction("AppraisalQuestions", "Appraisal", new { appNo = appNo });
		}

		[HttpGet]
		public IActionResult AppraisalQuestions(string appNo)
		{

			List<SecondSupervisorComment> secondSupervisorComments = new List<SecondSupervisorComment>();

			SecondSupervisorComment secondSupervisorComment = new SecondSupervisorComment
			{
				No = 1,
				Question = "1. List your outstanding achievements for the period under review.",
				key = "",
				comments_on_Performance = string.Empty,
				appraisal_No = "",
				person = SupervisorCommentsServiceReference.Person.Substantial_Achievements,
				personFieldSpecified = true,
				Table = "Second Supervisor Comments"
			};

			secondSupervisorComments.Add(secondSupervisorComment);

			SecondSupervisorComment secondSupervisorComment2 = new SecondSupervisorComment
			{
				No = 2,
				Question = "2. State the circumstances that affected your performance positively for the period under review (circumstances may be related to Supervisor/organization/personal/external etc.)",
				key = "",
				comments_on_Performance = string.Empty,
				appraisal_No = "",
				person = SupervisorCommentsServiceReference.Person.Significant_Positive_Issues,
				personFieldSpecified = true,
				Table = "Second Supervisor Comments"
			};

			secondSupervisorComments.Add(secondSupervisorComment2);

			SecondSupervisorComment secondSupervisorComment3 = new SecondSupervisorComment
			{
				No = 3,
				Question = "3. State the circumstances that affected your performance negatively for the period under review (circumstances may be related to Supervisor/organization/personal/external etc.)",
				key = "",
				comments_on_Performance = string.Empty,
				appraisal_No = "",
				person = SupervisorCommentsServiceReference.Person.Significant_Negative_Issues,
				personFieldSpecified = true,
				Table = "Second Supervisor Comments"
			};

			secondSupervisorComments.Add(secondSupervisorComment3);

			SecondSupervisorComment secondSupervisorComment4 = new SecondSupervisorComment
			{
				No = 4,
				Question = "4. Appraisee's Appraisal Comments On The Performance Appraisal",
				key = "",
				comments_on_Performance = string.Empty,
				appraisal_No = "",
				person = SupervisorCommentsServiceReference.Person.PAppraisee,
				personFieldSpecified = true,
				Table = "Appraisee's Appraisal Comments"
			};

			secondSupervisorComments.Add(secondSupervisorComment4);

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

			if(secondSupervisorComment.FirstOrDefault().appraisal_No != null && secondSupervisorComment.FirstOrDefault().appraisal_No != string.Empty)
			{
				dEFT_OnlinePortalServices_PortClient.SendForReview(secondSupervisorComment.FirstOrDefault().appraisal_No);
				TempData["Message"] = "Appraisal Sent for review";

				return RedirectToAction("Index");
			}

			return View();
		}
	}
}

