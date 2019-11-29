using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Escon.SisctNET.Web.Controllers
{
    public class CompanyCfopController : ControllerBaseSisctNET
    {
        private readonly ICompanyCfopService _service;
        private readonly ICompanyService _companyService;
        private readonly ICfopService _cfopService;

        public CompanyCfopController(
            ICompanyCfopService service,
            ICfopService cfopService,
            ICompanyService companyService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "CompanyCfop")
        {
            _service = service;
            _cfopService = cfopService;
            _companyService = companyService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Sincronize(int companyId)
        {
            try
            {
                var cfops = _cfopService.FindAll(GetLog(Model.OccorenceLog.Read));
                foreach (var cfop in cfops)
                {
                    var companycfop = _service.FindByCompanyCfop(companyId, cfop.Id);
                    if (companycfop == null)
                    {
                        var cc = new Model.CompanyCfop
                        {
                            CompanyId = companyId,
                            CfopId = cfop.Id,
                            Active = false
                        };

                        var result = _service.Create(entity:cc, GetLog(Model.OccorenceLog.Create));
                    }
                }
                return RedirectToAction("Index", new { companyId = companyId});
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        public IActionResult Index(int companyId)
        {
            try
            {
                ViewBag.Id = companyId;
                var company = _companyService.FindById(companyId, GetLog(Model.OccorenceLog.Read));
                ViewBag.Document = company.Document;
                ViewBag.Name = company.SocialName;
                var result = _service.FindByCompany(companyId);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
            
        }

        [HttpPost]
        public IActionResult UpdateStatus([FromBody] Model.UpdateActive updateActive)
        {
            try
            {
                var entity = _service.FindById(updateActive.Id, GetLog(Model.OccorenceLog.Read));
                entity.Active = updateActive.Active;

                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return Ok(new { requestcode = 200, message = "ok" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { requestcode = 500, message = ex.Message });
            }
        }
    }
}
