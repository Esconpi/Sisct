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

        public IActionResult Index(int id)
        {
            if (!SessionManager.GetTaxationInSession().Equals(18))
            {
                return Unauthorized();
            }

            try
            {
                var login = SessionManager.GetLoginInSession();

                if (login == null)
                {
                    return RedirectToAction("Index", "Authentication");
                }
                else
                {
                    var result = _service.FindByCompany(id);
                    var company = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                    ViewBag.Company = company.SocialName;
                    ViewBag.Document = company.Document;
                    SessionManager.SetCompanyIdInSession(id);
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
            if (!SessionManager.GetTaxationInSession().Equals(18))
            {
                return Unauthorized();
            }
            try
            {
                var comp = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                SessionManager.SetCompanyIdInSession(comp.CompanyId);
                _service.Delete(id, GetLog(Model.OccorenceLog.Delete));
                return RedirectToAction("Index", new { companyId = SessionManager.GetCompanyIdInSession()});
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            if (!SessionManager.GetTaxationInSession().Equals(18))
            {
                return Unauthorized();
            }
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
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

            var taxationAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession()); ;


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Model.Taxation> taxation = new List<Model.Taxation>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());
                
                List<Model.Taxation> taxationTemp = new List<Model.Taxation>();
                taxationAll.ToList().ForEach(s =>
                {
                    s.Ncm.Code = Helpers.CharacterEspecials.RemoveDiacritics(s.Ncm.Code);
                    s.TaxationType.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.TaxationType.Description);
                    s.Uf = s.Uf;
                    taxationTemp.Add(s);
                });

                var ids = taxationTemp.Where(c =>
                    c.Ncm.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
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
                               Code = r.Ncm.Code,
                               cest = r.Cest,
                               Description = r.TaxationType.Description,
                               AliqInterna = r.AliqInterna,
                               Mva = r.MVA,
                               Bcr = r.BCR,
                               Picms = r.Picms,
                               Uf = r.Uf,
                               Inicio = Convert.ToDateTime(r.DateStart).ToString("dd/MM/yyyy"),
                               Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")

                           };

                return Ok(new { draw = draw, recordsTotal = taxation.Count(), recordsFiltered = taxation.Count(), data = cfop.Skip(start).Take(lenght) });

            }
            else
            {


                var taxation = from r in taxationAll
                           select new
                           {
                               Id = r.Id.ToString(),
                               Code = r.Ncm.Code,
                               cest = r.Cest,
                               Description = r.TaxationType.Description,
                               AliqInterna = r.AliqInterna,
                               Mva = r.MVA,
                               Bcr = r.BCR,
                               Picms = r.Picms,
                               Uf = r.Uf,
                               Inicio = Convert.ToDateTime(r.DateStart).ToString("dd/MM/yyyy"),
                               Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")


                           };
                return Ok(new { draw = draw, recordsTotal = taxationAll.Count(), recordsFiltered = taxationAll.Count(), data = taxation.Skip(start).Take(lenght) });
            }

        }
    }
}
