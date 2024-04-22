using DeftOData;
using ESS.Data;
using ESS.Helpers;
using ESS.Migrations;
using ESS.Models;
using ESS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.ServiceModel;

namespace ESS.Controllers
{
    [Authorize]
    public class LeaveController : Controller
    {

        private readonly ILogger<LeaveController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDBContext _dbContext;

        public LeaveController(IConfiguration configuration, ApplicationDBContext dBContext, ILogger<LeaveController> logger)
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
            var leaveQuery = context.CreateQuery<DeftOData.DEFT_LeaveApplicationList>("DEFT_LeaveApplicationList")
                .AddQueryOption("$filter", "Employee_No eq '" + EmpCode + "'");

            ViewBag.LeaveQuery = leaveQuery;

            return View();

        }

        public IActionResult RegisterId()
        {
            string EmpCode = User.Identity.Name;

            Guid Id = Guid.NewGuid();

            DeftAddLeave deftAddLeave = new DeftAddLeave();
            deftAddLeave.Id = Id;
            deftAddLeave.EmpCode = EmpCode;
            _dbContext.DeftAddLeave.Add(deftAddLeave);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(SelectLeaveType), new { Id = Id });
        }

        public async Task<IActionResult> SelectLeaveType(Guid Id)
        {

            string EmpCode = User.Identity.Name;

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }


            BasicHttpBinding binding = new BasicHttpBinding();
            //binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
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

            string Msg = oPS_PortClient.LeaveExists(EmpCode);

            if (!Msg.IsNullOrEmpty())
            {
                TempData["Message"] = Msg;
            }

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
            var leaveTypesQuery = context.CreateQuery<DeftOData.DEFT_LeaveTypes>("DEFT_LeaveTypes");

            ViewBag.LeaveTypes = leaveTypesQuery;

            SelectLeaveTypeVM leaveTypeVM = new SelectLeaveTypeVM();

            DeftAddLeave deftAddLeave = await _dbContext.DeftAddLeave.SingleAsync(c => c.Id.Equals(Id));
            leaveTypeVM.Id = deftAddLeave.Id;

            return View(leaveTypeVM);

        }

        [HttpPost]
        public async Task<IActionResult> SelectLeaveType(SelectLeaveTypeVM leaveTypeVM)
        {
            string EmpCode = User.Identity.Name;

            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            //binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
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

            //EmployeeLeaveApplicationCard
            EndpointAddress endpointEAC = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/DEFT_EmployeeLeaveApplicationCard");
            DeftSoap_EmpApplicationCard.DEFT_EmployeeLeaveApplicationCard_PortClient eAP_PortClient = new DeftSoap_EmpApplicationCard.DEFT_EmployeeLeaveApplicationCard_PortClient(binding, endpointEAC);
           
            if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
            {
                eAP_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
                eAP_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
            }
            else
            {
                eAP_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
            }

            //Create Context
            var serviceRoot = _configuration["BC_API:ODATA_ServiceRoot"] + "Company('" + _configuration["Defaults:Company"] + "')/";
            var context = new DeftOData.NAV(new Uri(serviceRoot));
            context.Credentials = DeftFunctions.getNetworkCredential(_configuration);

            if (ModelState.IsValid)
            {
                try
                {
                    DeftAddLeave deftAddLeave = await _dbContext.DeftAddLeave.SingleAsync(c => c.Id.Equals(leaveTypeVM.Id));

                    string Msg = oPS_PortClient.LeaveExists(EmpCode);

                    if (Msg.IsNullOrEmpty())
                    {
                        DeftSoap_EmpApplicationCard.DEFT_EmployeeLeaveApplicationCard dEFT_EmployeeLeaveApplicationCard = new DeftSoap_EmpApplicationCard.DEFT_EmployeeLeaveApplicationCard();
                        eAP_PortClient.Create(ref dEFT_EmployeeLeaveApplicationCard);

                        dEFT_EmployeeLeaveApplicationCard.Employee_No = EmpCode;
                        dEFT_EmployeeLeaveApplicationCard.Leave_Code = leaveTypeVM.LeaveType;
                        eAP_PortClient.Update(ref dEFT_EmployeeLeaveApplicationCard);

                        deftAddLeave.ApplicationNo = dEFT_EmployeeLeaveApplicationCard.Application_No;

                        _dbContext.DeftAddLeave.Update(deftAddLeave);
                        _dbContext.SaveChanges();

                        return RedirectToAction(nameof(ApplyLeave), new { Id = deftAddLeave.Id });
                    }
                    else
                    {
                        TempData["Message"] = Msg;
                    }

                }
                catch (Exception ex)
                {
                    TempData["Message"] = ex.Message;
                }
            }

            //Create Query
            DeftOData.DEFT_EmployeeCard employeeCard = context.CreateQuery<DeftOData.DEFT_EmployeeCard>("DEFT_EmployeeCard")
                .AddQueryOption("$filter", "No eq '" + EmpCode + "'")
                .First();

            ViewBag.EmployeeCard = employeeCard;

            //Create Query
            var leaveTypesQuery = context.CreateQuery<DeftOData.DEFT_LeaveTypes>("DEFT_LeaveTypes");

            ViewBag.LeaveTypes = leaveTypesQuery;

            return View(leaveTypeVM);

        }

        public async Task<IActionResult> ApplyLeave(Guid Id)
        {

            string EmpCode = User.Identity.Name;

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

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
            var leaveTypesQuery = context.CreateQuery<DeftOData.DEFT_LeaveTypes>("DEFT_LeaveTypes");

            ViewBag.LeaveTypes = leaveTypesQuery;

            ApplyLeaveVM applyLeaveVM = new ApplyLeaveVM();

            DeftAddLeave deftAddLeave = await _dbContext.DeftAddLeave.SingleAsync(c => c.Id.Equals(Id));
            applyLeaveVM.Id = deftAddLeave.Id;

            //Create Query
            DeftOData.DEFT_EmployeeLeaveApplicationCard leaveApplicationCard = context.CreateQuery<DeftOData.DEFT_EmployeeLeaveApplicationCard>("DEFT_EmployeeLeaveApplicationCard")
                .AddQueryOption("$filter", "Application_No eq '" + deftAddLeave.ApplicationNo + "'")
                .First();

            ViewBag.LeaveApplicationCard = leaveApplicationCard;

            var employeesList = context.CreateQuery<DeftOData.DEFT_Employees>("DEFT_Employees")
                .AddQueryOption("$filter", "Global_Dimension_1_Code eq '" + employeeCard.Global_Dimension_1_Code + "'");

            ViewBag.EmployeesList = employeesList;

            return View(applyLeaveVM);

        }

        [HttpPost]
        public async Task<IActionResult> ApplyLeave(ApplyLeaveVM applyLeaveVM)
        {
            string EmpCode = User.Identity.Name;
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            //binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
            binding.Security.Transport.ClientCredentialType = DeftFunctions.getHttpClientCredentialType();

            //EmployeeLeaveApplicationCard
            EndpointAddress endpointEAC = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/DEFT_EmployeeLeaveApplicationCard");
            DeftSoap_EmpApplicationCard.DEFT_EmployeeLeaveApplicationCard_PortClient eAP_PortClient = new DeftSoap_EmpApplicationCard.DEFT_EmployeeLeaveApplicationCard_PortClient(binding, endpointEAC);

            if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
            {
                eAP_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
                eAP_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
            }
            else
            {
                eAP_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
            }

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

            //LeaveRelievers
            EndpointAddress endpointLR = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/DEFT_LeaveRelievers");
            DeftSoap_LeaveRelievers.DEFT_LeaveRelievers_PortClient lR_PortClient = new DeftSoap_LeaveRelievers.DEFT_LeaveRelievers_PortClient(binding, endpointLR);

            if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
            {
                lR_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
                lR_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
            }
            else
            {
                lR_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
            }

            //Create Context
            var serviceRoot = _configuration["BC_API:ODATA_ServiceRoot"] + "Company('" + _configuration["Defaults:Company"] + "')/";
            var context = new DeftOData.NAV(new Uri(serviceRoot));
            context.Credentials = DeftFunctions.getNetworkCredential(_configuration);

            DeftAddLeave deftAddLeave = await _dbContext.DeftAddLeave.SingleAsync(c => c.Id.Equals(applyLeaveVM.Id));

            if (ModelState.IsValid)
            {
                var relieverCard = context.CreateQuery<DeftOData.DEFT_Employees>("DEFT_Employees")
                .AddQueryOption("$filter", "No eq '" + applyLeaveVM.RelieverCode + "'")
                .First();

                try
                {
                    DeftSoap_EmpApplicationCard.DEFT_EmployeeLeaveApplicationCard dEFT_EmployeeLeaveApplicationCard = eAP_PortClient.Read(deftAddLeave.ApplicationNo);

                    dEFT_EmployeeLeaveApplicationCard.Employee_No = EmpCode;
                    dEFT_EmployeeLeaveApplicationCard.Start_Date = applyLeaveVM.StartDate;
                    dEFT_EmployeeLeaveApplicationCard.Days_Applied = applyLeaveVM.NumberOfDays;

                    eAP_PortClient.Update(ref dEFT_EmployeeLeaveApplicationCard);

                    DeftSoap_LeaveRelievers.DEFT_LeaveRelievers dEFT_LeaveRelievers = new DeftSoap_LeaveRelievers.DEFT_LeaveRelievers();
                    dEFT_LeaveRelievers.Leave_Code = deftAddLeave.ApplicationNo;
                    dEFT_LeaveRelievers.Staff_No = applyLeaveVM.RelieverCode;
                    dEFT_LeaveRelievers.Staff_Name = relieverCard.FullName;

                    lR_PortClient.Create(ref dEFT_LeaveRelievers);

                    oPS_PortClient.SendLeaveApproval(dEFT_EmployeeLeaveApplicationCard.Application_No);

                    deftAddLeave.IsUsed = true;

                    _dbContext.DeftAddLeave.Update(deftAddLeave);
                    _dbContext.SaveChanges();

                    TempData["Message"] = "Your leave application number " + deftAddLeave.ApplicationNo + " has been sent for appproval.";

                    return RedirectToAction(nameof(Index));

                }
                catch (Exception ex)
                {
                    TempData["Message"] = ex.Message;
                }
            }

            //Create Query
            DeftOData.DEFT_EmployeeCard employeeCard = context.CreateQuery<DeftOData.DEFT_EmployeeCard>("DEFT_EmployeeCard")
                .AddQueryOption("$filter", "No eq '" + EmpCode + "'")
                .First();

            ViewBag.EmployeeCard = employeeCard;

            //Create Query
            var leaveTypesQuery = context.CreateQuery<DeftOData.DEFT_LeaveTypes>("DEFT_LeaveTypes");

            ViewBag.LeaveTypes = leaveTypesQuery;

            //Create Query
            DeftOData.DEFT_EmployeeLeaveApplicationCard leaveApplicationCard = context.CreateQuery<DeftOData.DEFT_EmployeeLeaveApplicationCard>("DEFT_EmployeeLeaveApplicationCard")
                .AddQueryOption("$filter", "Application_No eq '" + deftAddLeave.ApplicationNo + "'")
                .First();

            ViewBag.LeaveApplicationCard = leaveApplicationCard;

            var employeesList = context.CreateQuery<DeftOData.DEFT_Employees>("DEFT_Employees")
                .AddQueryOption("$filter", "Global_Dimension_1_Code eq '" + employeeCard.Global_Dimension_1_Code + "'");

            ViewBag.EmployeesList = employeesList;

            return View(applyLeaveVM);

        }

        public IActionResult DetailsLeave(string Id)
        {
            string EmpCode = User.Identity.Name;

            //Create Context
            var serviceRoot = _configuration["BC_API:ODATA_ServiceRoot"] + "Company('" + _configuration["Defaults:Company"] + "')/";
            var context = new DeftOData.NAV(new Uri(serviceRoot));
            context.Credentials = DeftFunctions.getNetworkCredential(_configuration);


            //Create Query
            var leaveQuery = context.CreateQuery<DeftOData.DEFT_EmployeeLeaveApplicationCard>("DEFT_EmployeeLeaveApplicationCard")
                .AddQueryOption("$filter", "Application_No eq '" + Id + "' and Employee_No eq '" + EmpCode + "'");

            DeftOData.DEFT_EmployeeLeaveApplicationCard leave = leaveQuery.FirstOrDefault();

            return View(leave);
        }

        public IActionResult EditLeave(string Application_No)
        {
            string EmpCode = User.Identity.Name;

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }


            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
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
            var leaveTypesQuery = context.CreateQuery<DeftOData.DEFT_LeaveTypes>("DEFT_LeaveTypes");

            ViewBag.LeaveTypes = leaveTypesQuery;

            SelectLeaveTypeVM leaveTypeVM = new SelectLeaveTypeVM();

            DeftAddLeave deftAddLeave = new DeftAddLeave();

            var deftAddLeaves = _dbContext.DeftAddLeave.Where(c => c.EmpCode.Equals(EmpCode) & c.ApplicationNo.Equals(Application_No));
            if (deftAddLeaves.Any())
            {
                deftAddLeave = deftAddLeaves.First();
            }
            else
            {
                deftAddLeave.Id = Guid.NewGuid();
                deftAddLeave.EmpCode = EmpCode;

                _dbContext.DeftAddLeave.Add(deftAddLeave);
                _dbContext.SaveChanges();

            }

            deftAddLeave.ApplicationNo = Application_No;
            _dbContext.DeftAddLeave.Update(deftAddLeave);
            _dbContext.SaveChanges();

            leaveTypeVM.Id = deftAddLeave.Id;

            return View(leaveTypeVM);
        }

        [HttpPost]
        public async Task<IActionResult> EditLeave([FromForm]SelectLeaveTypeVM leaveTypeVM)
        {
            string EmpCode = User.Identity.Name;

            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
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

            //EmployeeLeaveApplicationCard
            EndpointAddress endpointEAC = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/DEFT_EmployeeLeaveApplicationCard");
            DeftSoap_EmpApplicationCard.DEFT_EmployeeLeaveApplicationCard_PortClient eAP_PortClient = new DeftSoap_EmpApplicationCard.DEFT_EmployeeLeaveApplicationCard_PortClient(binding, endpointEAC);

            if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
            {
                eAP_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
                eAP_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
            }
            else
            {
                eAP_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
            }

            //Create Context
            var serviceRoot = _configuration["BC_API:ODATA_ServiceRoot"] + "Company('" + _configuration["Defaults:Company"] + "')/";
            var context = new DeftOData.NAV(new Uri(serviceRoot));
            context.Credentials = DeftFunctions.getNetworkCredential(_configuration);

            if (ModelState.IsValid)
            {
                try
                {
                    DeftAddLeave deftAddLeave = await _dbContext.DeftAddLeave.SingleAsync(c => c.Id.Equals(leaveTypeVM.Id));

                    DeftSoap_EmpApplicationCard.DEFT_EmployeeLeaveApplicationCard dEFT_EmployeeLeaveApplicationCard = eAP_PortClient.Read(deftAddLeave.ApplicationNo);

                    //dEFT_EmployeeLeaveApplicationCard.Employee_No = EmpCode;
                    dEFT_EmployeeLeaveApplicationCard.Leave_Code = leaveTypeVM.LeaveType;
                    eAP_PortClient.Update(ref dEFT_EmployeeLeaveApplicationCard);

                    deftAddLeave.ApplicationNo = dEFT_EmployeeLeaveApplicationCard.Application_No;

                    _dbContext.DeftAddLeave.Update(deftAddLeave);
                    _dbContext.SaveChanges();

                    return RedirectToAction(nameof(EditApplication), new { Id = deftAddLeave.Id });

                }
                catch (Exception ex)
                {
                    TempData["Message"] = ex.Message;
                }
            }

            //Create Query
            DeftOData.DEFT_EmployeeCard employeeCard = context.CreateQuery<DeftOData.DEFT_EmployeeCard>("DEFT_EmployeeCard")
                .AddQueryOption("$filter", "No eq '" + EmpCode + "'")
                .First();

            ViewBag.EmployeeCard = employeeCard;

            //Create Query
            var leaveTypesQuery = context.CreateQuery<DeftOData.DEFT_LeaveTypes>("DEFT_LeaveTypes");

            ViewBag.LeaveTypes = leaveTypesQuery;

            return View(leaveTypeVM);
        }

        public async Task<IActionResult> EditApplication(Guid Id)
        {

            string EmpCode = User.Identity.Name;

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

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
            var leaveTypesQuery = context.CreateQuery<DeftOData.DEFT_LeaveTypes>("DEFT_LeaveTypes");

            ViewBag.LeaveTypes = leaveTypesQuery;

            ApplyLeaveVM applyLeaveVM = new ApplyLeaveVM();

            DeftAddLeave deftAddLeave = await _dbContext.DeftAddLeave.SingleAsync(c => c.Id.Equals(Id));
            applyLeaveVM.Id = deftAddLeave.Id;

            //Create Query
            DeftOData.DEFT_EmployeeLeaveApplicationCard leaveApplicationCard = context.CreateQuery<DeftOData.DEFT_EmployeeLeaveApplicationCard>("DEFT_EmployeeLeaveApplicationCard")
                .AddQueryOption("$filter", "Application_No eq '" + deftAddLeave.ApplicationNo + "'")
                .First();

            ViewBag.LeaveApplicationCard = leaveApplicationCard;

            var employeesList = context.CreateQuery<DeftOData.DEFT_Employees>("DEFT_Employees")
                .AddQueryOption("$filter", "Global_Dimension_1_Code eq '" + employeeCard.Global_Dimension_1_Code + "'");

            ViewBag.EmployeesList = employeesList;

            return View(applyLeaveVM);

        }

        [HttpPost]
        public async Task<IActionResult> EditApplication(ApplyLeaveVM applyLeaveVM)
        {
            string EmpCode = User.Identity.Name;
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;//For Http only
            binding.Security.Transport.ClientCredentialType = DeftFunctions.getHttpClientCredentialType();

            //EmployeeLeaveApplicationCard
            EndpointAddress endpointEAC = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/DEFT_EmployeeLeaveApplicationCard");
            DeftSoap_EmpApplicationCard.DEFT_EmployeeLeaveApplicationCard_PortClient eAP_PortClient = new DeftSoap_EmpApplicationCard.DEFT_EmployeeLeaveApplicationCard_PortClient(binding, endpointEAC);

            if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
            {
                eAP_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
                eAP_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
            }
            else
            {
                eAP_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
            }

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

            //LeaveRelievers
            EndpointAddress endpointLR = new EndpointAddress(_configuration["BC_API:SOAP_ServiceRoot"] + _configuration["Defaults:Company"] + "/Page/DEFT_LeaveRelievers");
            DeftSoap_LeaveRelievers.DEFT_LeaveRelievers_PortClient lR_PortClient = new DeftSoap_LeaveRelievers.DEFT_LeaveRelievers_PortClient(binding, endpointLR);

            if (DeftFunctions.deftLoginMode.Equals(DeftLoginMode.BASIC))
            {
                lR_PortClient.ClientCredentials.UserName.UserName = DeftFunctions.getUsername(_configuration);
                lR_PortClient.ClientCredentials.UserName.Password = DeftFunctions.getPassword(_configuration);
            }
            else
            {
                lR_PortClient.ClientCredentials.Windows.ClientCredential = DeftFunctions.getNetworkCredential(_configuration);
            }

            //Create Context
            var serviceRoot = _configuration["BC_API:ODATA_ServiceRoot"] + "Company('" + _configuration["Defaults:Company"] + "')/";
            var context = new DeftOData.NAV(new Uri(serviceRoot));
            context.Credentials = DeftFunctions.getNetworkCredential(_configuration);

            DeftAddLeave deftAddLeave = await _dbContext.DeftAddLeave.SingleAsync(c => c.Id.Equals(applyLeaveVM.Id));

            if (ModelState.IsValid)
            {
                var relieverCard = context.CreateQuery<DeftOData.DEFT_Employees>("DEFT_Employees")
                .AddQueryOption("$filter", "No eq '" + applyLeaveVM.RelieverCode + "'")
                .First();


                var relievers = context.CreateQuery<DeftOData.DEFT_LeaveRelievers>("DEFT_LeaveRelievers")
                    .AddQueryOption("$filter", "Leave_Code eq '" + deftAddLeave.ApplicationNo + "'")
                    .AddQueryOption("$count","true");

                int relieversCount = relievers.Count(); 

                foreach(var item in relievers)
                {
                    DeftSoap_LeaveRelievers.DEFT_LeaveRelievers deleteReliever = lR_PortClient.Read(deftAddLeave.ApplicationNo, deftAddLeave.EmpCode);
                    lR_PortClient.Delete(deleteReliever.Key);
                }

                try
                {
                    DeftSoap_EmpApplicationCard.DEFT_EmployeeLeaveApplicationCard dEFT_EmployeeLeaveApplicationCard = eAP_PortClient.Read(deftAddLeave.ApplicationNo);

                    //dEFT_EmployeeLeaveApplicationCard.Employee_No = EmpCode;
                    dEFT_EmployeeLeaveApplicationCard.Start_Date = applyLeaveVM.StartDate;
                    dEFT_EmployeeLeaveApplicationCard.Days_Applied = applyLeaveVM.NumberOfDays;

                    eAP_PortClient.Update(ref dEFT_EmployeeLeaveApplicationCard);

                    DeftSoap_LeaveRelievers.DEFT_LeaveRelievers dEFT_LeaveRelievers = new DeftSoap_LeaveRelievers.DEFT_LeaveRelievers();
                    dEFT_LeaveRelievers.Leave_Code = deftAddLeave.ApplicationNo;
                    dEFT_LeaveRelievers.Staff_No = applyLeaveVM.RelieverCode;
                    dEFT_LeaveRelievers.Staff_Name = relieverCard.FullName;

                    lR_PortClient.Create(ref dEFT_LeaveRelievers);

                    oPS_PortClient.SendLeaveApproval(dEFT_EmployeeLeaveApplicationCard.Application_No);

                    deftAddLeave.IsUsed = true;

                    _dbContext.DeftAddLeave.Update(deftAddLeave);
                    _dbContext.SaveChanges();

                    TempData["Message"] = "Your leave application number " + deftAddLeave.ApplicationNo + " has been sent for appproval.";

                    return RedirectToAction(nameof(Index));

                }
                catch (Exception ex)
                {
                    TempData["Message"] = ex.Message;
                }
            }

            //Create Query
            DeftOData.DEFT_EmployeeCard employeeCard = context.CreateQuery<DeftOData.DEFT_EmployeeCard>("DEFT_EmployeeCard")
                .AddQueryOption("$filter", "No eq '" + EmpCode + "'")
                .First();

            ViewBag.EmployeeCard = employeeCard;

            //Create Query
            var leaveTypesQuery = context.CreateQuery<DeftOData.DEFT_LeaveTypes>("DEFT_LeaveTypes");

            ViewBag.LeaveTypes = leaveTypesQuery;

            //Create Query
            DeftOData.DEFT_EmployeeLeaveApplicationCard leaveApplicationCard = context.CreateQuery<DeftOData.DEFT_EmployeeLeaveApplicationCard>("DEFT_EmployeeLeaveApplicationCard")
                .AddQueryOption("$filter", "Application_No eq '" + deftAddLeave.ApplicationNo + "'")
                .First();

            ViewBag.LeaveApplicationCard = leaveApplicationCard;

            var employeesList = context.CreateQuery<DeftOData.DEFT_Employees>("DEFT_Employees")
                .AddQueryOption("$filter", "Global_Dimension_1_Code eq '" + employeeCard.Global_Dimension_1_Code + "'");

            ViewBag.EmployeesList = employeesList;

            return View(applyLeaveVM);

        }

        public IActionResult ViewApprover(String Id)
        {
            string EmpCode = User.Identity.Name;

            //Create Context
            var serviceRoot = _configuration["BC_API:ODATA_ServiceRoot"] + "Company('" + _configuration["Defaults:Company"] + "')/";
            var context = new DeftOData.NAV(new Uri(serviceRoot));
            context.Credentials = DeftFunctions.getNetworkCredential(_configuration);


            //Create Query
            var leaveQuery = context.CreateQuery<DeftOData.DEFT_ApprovalEntries>("DEFT_ApprovalEntries")
                .AddQueryOption("$filter", "Document_No eq '" + Id + "'");

            return View(leaveQuery);
        }
        public IActionResult Planner()
        {
            string EmpCode = User.Identity.Name;

            //Create Context
            var serviceRoot = _configuration["BC_API:ODATA_ServiceRoot"] + "Company('" + _configuration["Defaults:Company"] + "')/";
            var context = new DeftOData.NAV(new Uri(serviceRoot));
            context.Credentials = DeftFunctions.getNetworkCredential(_configuration);


            //Create Query
            var plannerQuery = context.CreateQuery<DeftOData.DEFT_LeavePlanner>("DEFT_LeavePlanner")
                .AddQueryOption("$filter", "Employee_No eq '" + EmpCode + "'");

            ViewBag.PlannerQuery = plannerQuery;

            return View();

        }
    }
}
