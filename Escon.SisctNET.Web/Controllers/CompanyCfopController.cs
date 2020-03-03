using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Web.Controllers
{
    public class CompanyCfopController : ControllerBaseSisctNET
    {
        private readonly ICompanyCfopService _service;
        private readonly ICompanyService _companyService;
        private readonly ICfopService _cfopService;
        private readonly ICfopTypeService _cfopTypeService;

        public CompanyCfopController(
            ICompanyCfopService service,
            ICfopService cfopService,
            ICompanyService companyService,
            ICfopTypeService cfopTypeService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "CompanyCfop")
        {
            _service = service;
            _cfopService = cfopService;
            _companyService = companyService;
            _cfopTypeService = cfopTypeService;
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
                        var companyCfop = new Model.CompanyCfop
                        {
                            CompanyId = companyId,
                            CfopId = cfop.Id,
                            Active = false,
                            Created = DateTime.Now,
                            Updated = DateTime.Now
                        };

                        var result = _service.Create(entity:companyCfop, GetLog(Model.OccorenceLog.Create));
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
                var login = SessionManager.GetLoginInSession();

                if (login == null)
                {
                    return RedirectToAction("Index", "Authentication");
                }
                else
                {
                    ViewBag.Id = companyId;
                    var company = _companyService.FindById(companyId, GetLog(Model.OccorenceLog.Read));
                    ViewBag.Document = company.Document;
                    ViewBag.Name = company.SocialName;

                    var cfopType = _cfopTypeService.FindAll(GetLog(Model.OccorenceLog.Read));

                    List<CfopType> cfopsTypes = new List<CfopType>();
                    cfopsTypes.Insert(0, new CfopType() { Id = 0, Name = "Nenhum" });

                    foreach (var item in cfopType)
                    {
                        cfopsTypes.Add(new CfopType() { Id = item.Id, Name = item.Name });
                    }

                    SelectList cfopTypes = new SelectList(cfopsTypes, "Id", "Name", null);
                    ViewBag.ListTypes = cfopTypes;

                    ViewBag.CompanyName = company.SocialName;
                    ViewBag.Document = company.Document;
                    ViewBag.CompanyId = company.Id;

                    var result = _service.FindByCompany(companyId);
                    return View(result);
                }
               
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
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateCfopType([FromBody] Model.UpdateCfopType updateCfopType)
        {

            try
            {
                var entity = _service.FindById(updateCfopType.CompanyCfopId, GetLog(Model.OccorenceLog.Read));

                if (updateCfopType.CfopTypeId.Equals(0))
                {
                    entity.CfopTypeId = null;
                }
                else
                {
                    entity.CfopTypeId = updateCfopType.CfopTypeId;
                }
               
                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return Ok(new { requestcode = 200, message = "ok" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

    }
}
