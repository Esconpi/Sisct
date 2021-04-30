using System;
using System.Collections.Generic;
using System.Linq;
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

        public IActionResult Index(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Taxation")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindByCompany(id);
                var company = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                ViewBag.Company = company;
                SessionManager.SetCompanyIdInSession(id);
                return View(null);

            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Delete(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Taxation")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                SessionManager.SetCompanyIdInSession(comp.CompanyId);
                _service.Delete(id, GetLog(Model.OccorenceLog.Delete));
                return RedirectToAction("Index", new { id = SessionManager.GetCompanyIdInSession()});
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Details(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Taxation")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                return PartialView(result);
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

            var taxationAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession()).OrderBy(_ => _.Ncm.Code).ToList();


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Model.Taxation> taxation = new List<Model.Taxation>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());
                
                var ids = taxationAll.Where(c =>
                    c.Ncm.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Ncm.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.TaxationType.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Uf.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    )
                .Select(s => s.Id).ToList();

                taxation = taxationAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var cfop = from r in taxation
                           where ids.ToArray().Contains(r.Id)
                           select new
                           {
                               Id = r.Id.ToString(),
                               Ncm = r.Ncm.Description,
                               Cest = r.Cest,
                               Taxation = r.TaxationType.Description,
                               Picms = r.Picms,
                               Uf = r.Uf,

                           };

                return Ok(new { draw = draw, recordsTotal = taxation.Count(), recordsFiltered = taxation.Count(), data = cfop.Skip(start).Take(lenght) });

            }
            else
            {


                var taxation = from r in taxationAll
                           select new
                           {
                               Id = r.Id.ToString(),
                               Ncm = r.Ncm.Description,
                               Cest = r.Cest,
                               Taxation = r.TaxationType.Description,
                               Picms = r.Picms,
                               Uf = r.Uf,

                           };
                return Ok(new { draw = draw, recordsTotal = taxationAll.Count(), recordsFiltered = taxationAll.Count(), data = taxation.Skip(start).Take(lenght) });
            }

        }
    }
}
