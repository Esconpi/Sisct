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
            : base(functionalityService, "Cfop")
        {
            _service = service;
            _cfopService = cfopService;
            _companyService = companyService;
            _cfopTypeService = cfopTypeService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Sincronize(long companyId)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cfop")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var cfops = _cfopService.FindAll(null);
                var cfopsCompany = _service.FindByCompany(companyId);

                List<CompanyCfop> addCompanyCfop = new List<CompanyCfop>();
                foreach (var cfop in cfops)
                {
                    var companyCfop = cfopsCompany.Where(_ => _.CfopId.Equals(Convert.ToInt32(cfop.Id))).FirstOrDefault();
                    if (companyCfop == null)
                    {
                        CompanyCfop cc = new CompanyCfop();
                        cc.CompanyId = companyId;
                        cc.CfopId = cfop.Id;
                        cc.Active = false;
                        cc.CfopTypeId = 11;
                        cc.Created = DateTime.Now;
                        cc.Updated = cc.Created;
                        addCompanyCfop.Add(cc);
                    }
                }
                _service.Create(addCompanyCfop, GetLog(OccorenceLog.Create));
                return RedirectToAction("Index", new { id = companyId});
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        public IActionResult Index(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cfop")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                ViewBag.Id = id;
                var company = _companyService.FindById(id, null);
                ViewBag.Company = company;

                var cfopType = _cfopTypeService.FindAll(null);

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
                var result = _service.FindByCompany(id);
                SessionManager.SetCompanyIdInSession(id);
                return View(null);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
            
        }

        [HttpPost]
        public IActionResult UpdateStatus([FromBody] Model.UpdateActive updateActive)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cfop")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var entity = _service.FindById(updateActive.Id, null);
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cfop")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                if(updateCfopType != null)
                {
                    var entity = _service.FindById(updateCfopType.CompanyCfopId, null);

                    if (updateCfopType.CfopTypeId.Equals(0))
                    {
                        entity.CfopTypeId = null;
                    }
                    else
                    {
                        entity.CfopTypeId = updateCfopType.CfopTypeId;
                    }

                    _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                }
                
             
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

            var cfopsAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession()).OrderBy(_ => Convert.ToInt32(_.Cfop.Code)).ToList();


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<CompanyCfop> cfops = new List<CompanyCfop>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<CompanyCfop> cfopTemp = new List<CompanyCfop>();
                cfopsAll.ToList().ForEach(s =>
                {
                    s.Cfop.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Cfop.Description);
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
                               CfopType = r.CfopType.Name

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
                               CfopType = r.CfopType.Name

                           };
                return Ok(new { draw = draw, recordsTotal = cfopsAll.Count(), recordsFiltered = cfopsAll.Count(), data = cfop.Skip(start).Take(lenght) });
            }

        }
    }
}
