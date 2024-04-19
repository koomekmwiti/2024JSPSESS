using ESS.Data;
using ESS.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost]
        public async Task<IActionResult> ApproveLeave(string docNo)
        {
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

            try
            {
                var result = oPS_PortClient.SendLeaveApprovalAsync(docNo);

                if (result != null)
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



        [HttpPost]
        public async Task<IActionResult> RejectLeave(string docNo)
        {
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

            try
            {
                var result = oPS_PortClient.CancelLeaveApprovalAsync(docNo);

                if (result != null)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred");
            }

            return View("Index");
        }


        public IActionResult RejectLeave()
        {
            return View();
        }


        public IActionResult Appraisal()
        {
            return View();
        }

    }
}
