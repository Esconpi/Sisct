using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    SessionManager.SetCompanyIdInSession(companyId);
                    return View(null);
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

        public IActionResult GetAll(int draw, int start)
        {


            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var cfopsAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession());


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<CompanyCfop> cfops = new List<CompanyCfop>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<CompanyCfop> cfopTemp = new List<CompanyCfop>();
                cfopsAll.ToList().ForEach(s =>
                {
                    s.Cfop.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Cfop.Description);
                    s.Cfop.Code = s.Cfop.Code;
                    cfopTemp.Add(s);
                });

                var ids = cfopTemp.Where(c =>
                    c.Cfop.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Cfop.Code.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                cfops = cfopsAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var cfop = from r in cfops
                           where ids.ToArray().Contains(r.Id)
                           select new
                           {
                               Id = r.Id.ToString(),
                               Code = r.Cfop.Code,
                               Description = r.Cfop.Description,
                               Active = r.Active,
                               CfopTypeId = r.CfopTypeId,
                               CfopType = r.CfopType

                           };

                return Ok(new { draw = draw, recordsTotal = cfops.Count(), recordsFiltered = cfops.Count(), data = cfop.Skip(start).Take(lenght) });

            }
            else
            {


                var cfop = from r in cfopsAll
                           select new
                           {
                               Id = r.Id.ToString(),
                               Code = r.Cfop.Code,
                               Description = r.Cfop.Description,
                               Active = r.Active,
                               CfopTypeId = r.CfopTypeId,
                               CfopType = r.CfopType

                           };
                return Ok(new { draw = draw, recordsTotal = cfopsAll.Count(), recordsFiltered = cfopsAll.Count(), data = cfop.Skip(start).Take(lenght) });
            }

        }
    }
}
