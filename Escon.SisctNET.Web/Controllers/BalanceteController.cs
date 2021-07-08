using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Escon.SisctNET.Web.Controllers
{
    public class BalanceteController : ControllerBaseSisctNET
    {
        private readonly ICompanyService _companyService;
        private readonly Fortes.ISDOService _cONService;
        private readonly IConfigurationService _configurationService;

        public BalanceteController(
            ICompanyService companyService,
            Fortes.ISDOService cONService,
            IConfigurationService configurationService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Company")
        {
            _companyService = companyService;
            _cONService = cONService;
            _configurationService = configurationService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Relatory(long companyId, DateTime inicio, DateTime fim)
        {

            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _companyService.FindById(companyId, null);
                var confDbFortes = _configurationService.FindByName("DataBaseFortes", null);

                var balancete = _cONService.GetBalancete(comp, inicio, fim, confDbFortes.Value);

                ViewBag.Company = comp;

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
