using System;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Escon.SisctNET.Web.Controllers
{
    public class TaxationController : ControllerBaseSisctNET
    {
        private readonly ITaxationService _service;
        private readonly ICompanyService _companyService;

        public TaxationController(
            ITaxationService service,
            ICompanyService companyService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Taxation")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _companyService = companyService;

        }

        public IActionResult Index(int companyId)
        {
            try
            {
                var result = _service.FindByCompany(companyId);
                var company = _companyService.FindById(companyId, GetLog(Model.OccorenceLog.Read));
                ViewBag.Company = company.FantasyName;
                ViewBag.Document = company.Document;
                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }
    }
}
