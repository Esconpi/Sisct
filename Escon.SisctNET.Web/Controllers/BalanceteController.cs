using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Escon.SisctNET.Web.Controllers
{
    public class BalanceteController : ControllerBaseSisctNET
    {
        private readonly ICompanyService _companyService;
        private readonly Fortes.ISDOService _sDOService;
        private readonly IConfigurationService _configurationService;
        private readonly IAccountPlanService _accountPlanService;

        public BalanceteController(
            ICompanyService companyService,
            Fortes.ISDOService sDOService,
            IConfigurationService configurationService,
            IAccountPlanService accountPlanService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Company")
        {
            _companyService = companyService;
            _sDOService = sDOService;
            _configurationService = configurationService;
            _accountPlanService = accountPlanService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Relatory(long companyId, DateTime inicio, DateTime fim)
        {

            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _companyService.FindById(companyId, null);
                var confDbFortes = _configurationService.FindByName("DataBaseFortes", null);

                var accs = _accountPlanService.FindByCompanyActive(companyId);

                var disponibilidadeFinanceira = _sDOService.GetDisponibilidadeFinanceira(accs, comp, inicio, fim, confDbFortes.Value);
                var despesasOperacionais = _sDOService.GetDespesasOperacionais(accs, comp, inicio, fim, confDbFortes.Value);
                var mercadoriasMercadorias = _sDOService.GetEstoqueMercadorias(accs, comp, inicio, fim, confDbFortes.Value);

                ViewBag.Company = comp;
                ViewBag.Inicio = inicio;
                ViewBag.Fim = fim;
                ViewBag.DisponibilidadeFinanceira = disponibilidadeFinanceira;
                ViewBag.DespesasOperacionais = despesasOperacionais;
                ViewBag.EstoqueMercadoria = mercadoriasMercadorias;

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
