using System;
using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Escon.SisctNET.Web.Controllers
{
    public class TaxationController : ControllerBaseSisctNET
    {
        private readonly ITaxationService _service;
        private readonly ICompanyService _companyService;
        private readonly ITaxationTypeService _taxationTypeService;

        public TaxationController(
            ITaxationService service,
            ICompanyService companyService,
            ITaxationTypeService taxationTypeService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Taxation")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _companyService = companyService;
            _taxationTypeService = taxationTypeService;

        }

        public IActionResult Index(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Taxation")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var company = _companyService.FindById(id, null);
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
        public IActionResult Atualize(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Taxation")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {

                var list_taxation = _taxationTypeService.FindAll(null).Where(_ => _.Active).OrderBy(_ => _.Description).ToList();

                list_taxation.Insert(0, new TaxationType() { Description = "Nennhum item selecionado", Id = 0 });

                SelectList taxationtypes = new SelectList(list_taxation, "Id", "Description", null);
                ViewBag.TaxationTypeId = taxationtypes;

                var result = _service.FindById(id, null);

                return View(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Atualize(long id, Model.Taxation entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Taxation")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                if (result != null)
                 {
                     result.DateEnd = Convert.ToDateTime(entity.DateStart).AddDays(-1);
                     _service.Update(result, GetLog(Model.OccorenceLog.Update));
                 }

                Taxation taxation = new Taxation();
                taxation.TaxationTypeId = entity.TaxationTypeId;
                taxation.NcmId = result.NcmId;
                taxation.CompanyId = result.CompanyId;
                taxation.Code = result.Code;
                taxation.Uf = result.Uf;
                taxation.Cest = result.Cest;
                taxation.Picms = result.Picms;
                taxation.AliqInterna = entity.AliqInterna;
                taxation.MVA = entity.MVA;
                taxation.Fecop = entity.Fecop;
                taxation.BCR = entity.BCR;
                taxation.PercentualInciso = entity.PercentualInciso;
                taxation.AliqInternaCTe = entity.AliqInterna;
                taxation.EBcr = entity.EBcr;
                taxation.DateStart = entity.DateStart;

                _service.Create(taxation, GetLog(Model.OccorenceLog.Create));

                /*
                var taxations = _service.FindByCompany(SessionManager.GetCompanyIdInSession()).Where(_ => _.Fecop.Equals((decimal)1) && _.DateEnd.Equals(null)).ToList();

                List<Taxation> createTaxation = new List<Taxation>();
                List<Taxation> updateTaxation = new List<Taxation>();

                foreach (var taxation in taxations)
                {
                    taxation.Updated = DateTime.Now;
                    taxation.DateEnd = Convert.ToDateTime(entity.DateStart).AddDays(-1);

                    updateTaxation.Add(taxation);

                    Taxation taxationTemp = new Taxation();
                    taxationTemp.Created = DateTime.Now;
                    taxationTemp.Updated = taxationTemp.Created;
                    taxationTemp.TaxationTypeId = taxation.TaxationTypeId;
                    taxationTemp.NcmId = taxation.NcmId;
                    taxationTemp.CompanyId = taxation.CompanyId;
                    taxationTemp.Code = taxation.Code;
                    taxationTemp.Uf = taxation.Uf;
                    taxationTemp.Cest = taxation.Cest;
                    taxationTemp.Picms = taxation.Picms;
                    taxationTemp.AliqInterna = taxation.AliqInterna;
                    taxationTemp.MVA = taxation.MVA;
                    taxationTemp.Fecop = null;
                    taxationTemp.BCR = taxation.BCR;
                    taxationTemp.PercentualInciso = taxation.PercentualInciso;
                    taxationTemp.AliqInternaCTe = taxation.AliqInterna;
                    taxationTemp.EBcr = taxation.EBcr;
                    taxationTemp.DateStart = entity.DateStart;

                    createTaxation.Add(taxationTemp);
                }

                _service.Update(updateTaxation);
                _service.Create(createTaxation);*/

                return RedirectToAction("Index", new { id = SessionManager.GetCompanyIdInSession() });
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

            var taxationAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession()).OrderBy(_ => _.Ncm.Code).ToList();

            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Model.Taxation> taxation = new List<Model.Taxation>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());
                
                var ids = taxationAll.Where(c =>
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
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
                               Code = r.Code,
                               Ncm = r.Ncm.Description,
                               Cest = r.Cest,
                               Taxation = r.TaxationType.Description,
                               Picms = r.Picms,
                               Uf = r.Uf,
                               Inicio = r.DateStart.ToString("dd/MM/yyyy"),
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
                               Code = r.Code,
                               Ncm = r.Ncm.Description,
                               Cest = r.Cest,
                               Taxation = r.TaxationType.Description,
                               Picms = r.Picms,
                               Uf = r.Uf,
                               Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                               Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")
                           };
                return Ok(new { draw = draw, recordsTotal = taxationAll.Count(), recordsFiltered = taxationAll.Count(), data = taxation.Skip(start).Take(lenght) });
            }

        }
    }
}
