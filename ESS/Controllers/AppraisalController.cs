using DeftOData;
using DeftSoap_AppraisalCard;
using ESS.Data;
using ESS.Helpers;
using ESS.Models;
using ESS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using System.Net;
using System.Reflection.Emit;
using System.ServiceModel;
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
                .AddQueryOption("$orderby", "Start_Date desc"); ;

            ViewBag.AppraisalPeriodsQuery = appraisalPeriodsQuery;

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
                binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
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
                    TempData["Message"] ="Appraisal header created successfully. Please use the header to complete your appraisal.";
                }
                catch(Exception ex)
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
            binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
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
    }
}
