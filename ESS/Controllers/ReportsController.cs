﻿using ESS.Data;
using ESS.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using System.Net;
using System.ServiceModel;

namespace ESS.Controllers
{
	[Authorize]
	public class ReportsController : Controller
    {

		private readonly ILogger<ReportsController> _logger;
		private readonly IConfiguration _configuration;
		private readonly ApplicationDBContext _dbContext;

        public ReportsController(IConfiguration configuration, ApplicationDBContext dBContext, ILogger<ReportsController> logger)
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
            var apQuery = context.CreateQuery<DeftOData.DEFT_PayPeriod>("DEFT_PayPeriod")
                .AddQueryOption("$filter", "Allow_Payslip_Generation_on_Portal eq true")
                .AddQueryOption("$orderby", "Starting_Date desc");
            ViewBag.PayPeriods = apQuery;

            ViewBag.SiteURL = _configuration["Defaults:SiteURL"];

            return View();
        }

        public IActionResult P9Form()
        {
            string EmpCode = User.Identity.Name;

            //Create Context
            var serviceRoot = _configuration["BC_API:ODATA_ServiceRoot"] + "Company('" + _configuration["Defaults:Company"] + "')/";
            var context = new DeftOData.NAV(new Uri(serviceRoot));
            context.Credentials = DeftFunctions.getNetworkCredential(_configuration);

            //Create Query
            var apQuery = context.CreateQuery<DeftOData.DEFT_PayPeriod>("DEFT_PayPeriod")
                .AddQueryOption("$filter", "New_Fiscal_Year eq true")
                .AddQueryOption("$orderby", "Starting_Date desc");
            ViewBag.PayPeriods = apQuery;

            ViewBag.SiteURL = _configuration["Defaults:SiteURL"];

            return View();
        }

        public IActionResult DownloadPayslip(string Period)
        {
            string EmpCode = User.Identity.Name;

            DateTime dt1 = DateTime.Parse(Period);

            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
            binding.Security.Transport.ClientCredentialType = DeftFunctions.getHttpClientCredentialType();

            EndpointAddress endpoint = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Codeunit/DEFT_OnlinePortalServices");
            DeftSoap_OPS.DEFT_OnlinePortalServices_PortClient oSP_PortClient = new DeftSoap_OPS.DEFT_OnlinePortalServices_PortClient(binding, endpoint);

            if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
            {
                oSP_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
                oSP_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
            }
            else
            {
                oSP_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
            }

            string fileName = oSP_PortClient.PrintPaySlip(EmpCode, _configuration["Defaults:SharedFolder"], dt1);
            string filePath = _configuration["Defaults:SharedFolder"] + fileName;

            object TempFile = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + fileName;
            string tempPath = TempFile.ToString() ?? "";

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            System.IO.File.Move(filePath, tempPath);

            var fileInfo = new FileInfo(tempPath);
            Response.ContentType = "application/pdf";

            Response.Headers.Add("Content-Disposition", "attachement;filename=\"" + fileInfo.Name + "\"");
            Response.Headers.Add("Content-Length", fileInfo.Length.ToString());

            //return File(System.IO.File.ReadAllBytes(tempPath), "application/pdf", fileInfo.Name);
            return File(System.IO.File.ReadAllBytes(tempPath), "application/pdf", "Pay slip.pdf");
        }

        public IActionResult DownloadP9Form(string period)
        {
            string EmpCode = User.Identity.Name;

            DateTime dt1 = DateTime.Parse(period);
            DateTime dt2 = dt1.AddMonths(11);
            var lastDayOfMonth = new DateTime(dt2.Year, dt2.Month, DateTime.DaysInMonth(dt2.Year, dt2.Month));

            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
            binding.Security.Transport.ClientCredentialType = DeftFunctions.getHttpClientCredentialType();

            EndpointAddress endpoint = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Codeunit/DEFT_OnlinePortalServices");
            DeftSoap_OPS.DEFT_OnlinePortalServices_PortClient oSP_PortClient = new DeftSoap_OPS.DEFT_OnlinePortalServices_PortClient(binding, endpoint);

            if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
            {
                oSP_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
                oSP_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
            }
            else
            {
                oSP_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
            }

            string fileName = oSP_PortClient.PrintP9(EmpCode, _configuration["Defaults:SharedFolder"], dt1, dt2);
            string filePath = _configuration["Defaults:SharedFolder"] + fileName;

            object TempFile = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + fileName;
            string tempPath = TempFile.ToString() ?? "";

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            System.IO.File.Move(filePath, tempPath);

            var fileInfo = new FileInfo(tempPath);
            Response.ContentType = "application/pdf";

            Response.Headers.Add("Content-Disposition", "attachement;filename=\"" + fileInfo.Name + "\"");
            Response.Headers.Add("Content-Length", fileInfo.Length.ToString());

            //return File(System.IO.File.ReadAllBytes(tempPath), "application/pdf", fileInfo.Name);
            return File(System.IO.File.ReadAllBytes(tempPath), "application/pdf", "P9 Form.pdf");
        }

    }
}
