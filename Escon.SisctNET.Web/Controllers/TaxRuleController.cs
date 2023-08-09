using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using System.Collections.Generic;
using Escon.SisctNET.Model;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Escon.SisctNET.Web.Controllers
{
    public class TaxRuleController : ControllerBaseSisctNET
    {
        private readonly ITaxRuleService _service;
        private readonly ICompanyService _companyService;
        private readonly ITaxationTypeNcmService _taxationTypeNcmService;

        public TaxRuleController(
            ITaxRuleService service,
            ICompanyService companyService,
            ITaxationTypeNcmService taxationTypeNcmService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "TaxRule")
        {
            _service = service; 
            _companyService = companyService;
            _taxationTypeNcmService = taxationTypeNcmService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxRule")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                SessionManager.SetCompanyIdInSession(id);
                var company = _companyService.FindById(id, null);
                ViewBag.Company = company;
                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxRule")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                ViewBag.TaxationTypeNcmId = new SelectList(_taxationTypeNcmService.FindAll(null), "Id", "Description", null);

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create(Model.TaxRule entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxRule")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                entity.CompanyId = SessionManager.GetCompanyIdInSession();
                _service.Create(entity, GetLog(OccorenceLog.Create));

                return RedirectToAction("Index", new { id = SessionManager.GetCompanyIdInSession() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxRule")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                ViewBag.TaxationTypeNcmId = new SelectList(_taxationTypeNcmService.FindAll(null), "Id", "Description", null);

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.TaxRule entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxRule")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);

                entity.CompanyId = result.CompanyId;
                _service.Create(entity, GetLog(OccorenceLog.Create));

                return RedirectToAction("Index", new { id = SessionManager.GetCompanyIdInSession() });
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

            var taxationAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession());

            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Model.TaxRule> taxation = new List<Model.TaxRule>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<TaxRule> taxs = new List<TaxRule>();

                taxationAll.ToList().ForEach(s =>
                {
                    s.DescriptionNcm = Helpers.CharacterEspecials.RemoveDiacritics(s.DescriptionNcm);
                    taxs.Add(s);
                });

                var ids = taxs.Where(c =>
                        c.CodeNcm.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        c.DescriptionNcm.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.Id).ToList();

                taxation = taxationAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var result = from r in taxation
                             where ids.ToArray().Contains(r.Id)
                             select new
                             {
                                 Id = r.Id.ToString(),
                                 Ncm = r.CodeNcm + " - " + r.DescriptionNcm,
                                 Ex = r.CodeException + " - " + r.NameException,
                                 Type = r.TaxationTypeNcm.Description,
                                 Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                                 Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")
                             };

                return Ok(new { draw = draw, recordsTotal = taxation.Count(), recordsFiltered = taxation.Count(), data = result.Skip(start).Take(lenght) });

            }
            else
            {


                var taxation = from r in taxationAll
                               select new
                               {
                                   Id = r.Id.ToString(),
                                   Ncm = r.CodeNcm + " - " + r.DescriptionNcm,
                                   Ex = r.CodeException + " - " + r.NameException,
                                   Type = r.TaxationTypeNcm.Description,
                                   Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                                   Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")
                               };
                return Ok(new { draw = draw, recordsTotal = taxationAll.Count(), recordsFiltered = taxationAll.Count(), data = taxation.Skip(start).Take(lenght) });
            }

        }
    }
}
