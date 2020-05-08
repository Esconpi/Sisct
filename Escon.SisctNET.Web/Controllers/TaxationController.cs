using System;
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
                    var result = _service.FindByCompany(companyId);
                    var company = _companyService.FindById(companyId, GetLog(Model.OccorenceLog.Read));
                    ViewBag.Company = company.SocialName;
                    ViewBag.Document = company.Document;
                    SessionManager.SetCompanyIdInSession(companyId);
                    return View(null);
                }
               
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            try
            {
                var comp = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                _service.Delete(id, GetLog(Model.OccorenceLog.Delete));
                return RedirectToAction("Index", new { companyId = comp.CompanyId});
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

            var taxationAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession()); ;


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Taxation> taxation = new List<Taxation>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Taxation> taxationTemp = new List<Taxation>();
                taxationAll.ToList().ForEach(s =>
                {
                    s.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Description);
                    s.Code = s.Code;
                    taxationTemp.Add(s);
                });

                var ids = taxationTemp.Where(c =>
                    c.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                taxation = taxationAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var cfop = from r in taxation
                           where ids.ToArray().Contains(r.Id)
                           select new
                           {
                               Id = r.Id.ToString(),
                               Code = r.Code,
                               Description = r.Description

                           };

                return Ok(new { draw = draw, recordsTotal = taxation.Count(), recordsFiltered = taxation.Count(), data = cfop.Skip(start).Take(lenght) });

            }
            else
            {


                var cfop = from r in taxationAll
                           select new
                           {
                               Id = r.Id.ToString(),
                               Code = r.Code,
                               Description = r.Description

                           };
                return Ok(new { draw = draw, recordsTotal = taxationAll.Count(), recordsFiltered = taxationAll.Count(), data = cfop.Skip(start).Take(lenght) });
            }

        }
    }
}
