using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Escon.SisctNET.Service.Implementation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class NcmConvenioController : ControllerBaseSisctNET
    {
        private readonly INcmConvenioService _service;
        private readonly IAnnexService _annexService;

        public NcmConvenioController(
            INcmConvenioService service,
            IAnnexService annexService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Ncm")
        {
            _annexService = annexService;
            _service = service;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                return View(null);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
           
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                List<Annex> list_annex = _annexService.FindAll(null);
                foreach (var annex in list_annex)
                {
                    if (annex.Convenio.Equals("") || annex.Convenio.Equals(null))
                        annex.Description = annex.Description;
                    else
                        annex.Description = annex.Description + " - " + annex.Convenio;
                }
                list_annex.Insert(0, new Annex() { Description = "Nennhum anexo selecionado", Id = 0 });
                SelectList annexs = new SelectList(list_annex, "Id", "Description", null);
                ViewBag.AnnexId = annexs;
                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create(Model.NcmConvenio entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                if (entity.Ncm != null)
                    entity.Ncm = entity.Ncm.Trim();
                else
                    entity.Ncm = "";

                _service.Create(entity, GetLog(Model.OccorenceLog.Create));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
               
                var list_annex = _annexService.FindAll(null);
                foreach (var annex in list_annex)
                {
                    if (annex.Convenio.Equals("") || annex.Convenio.Equals(null))
                        annex.Description = annex.Description;
                    else
                        annex.Description = annex.Description + " - " + annex.Convenio;
                }
                list_annex.Insert(0, new Annex() { Description = "Nennhum anexo selecionado", Id = 0 });
                SelectList annexs = new SelectList(list_annex, "Id", "Description", null);
                ViewBag.AnnexId = annexs;
                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.NcmConvenio entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var rst = _service.FindById(id, null);

                if (entity.Ncm != null)
                    entity.Ncm = entity.Ncm.Trim();
                else
                    entity.Ncm = "";

                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Atualize(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
                return Unauthorized();
            try
            {

                var list_annex = _annexService.FindAll(null);
                foreach (var annex in list_annex)
                {
                    if (annex.Convenio.Equals("") || annex.Convenio.Equals(null))
                        annex.Description = annex.Description;
                    else
                        annex.Description = annex.Description + " - " + annex.Convenio;
                }
                list_annex.Insert(0, new Annex() { Description = "Nennhum anexo selecionado", Id = 0 });
                SelectList annexs = new SelectList(list_annex, "Id", "Description", null);
                ViewBag.AnnexId = annexs;

                var result = _service.FindById(id, null);

                return View(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Atualize(long id, Model.NcmConvenio entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                if (result != null)
                {
                    result.DateEnd = Convert.ToDateTime(entity.DateStart).AddDays(-1);
                    _service.Update(result, GetLog(Model.OccorenceLog.Update));
                }

                NcmConvenio taxation = new NcmConvenio();
                taxation.Ncm = result.Ncm;
                taxation.Cest = result.Cest;
                taxation.Description = result.Description;
                taxation.AnnexId = result.AnnexId;
                taxation.DateStart = entity.DateStart;

                _service.Create(taxation, GetLog(Model.OccorenceLog.Create));

                return RedirectToAction("Index", new { id = SessionManager.GetCompanyIdInSession() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }


        [HttpGet]
        public IActionResult Delete(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                _service.Delete(id, GetLog(Model.OccorenceLog.Delete));
                return RedirectToAction("Index");
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

            var ncmsAll = _service.FindAll(null).OrderBy(_ => _.AnnexId);

            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<NcmConvenio> ncms = new List<NcmConvenio>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<NcmConvenio> ncmTemp = new List<NcmConvenio>();
                ncmsAll.ToList().ForEach(s =>
                {
                    s.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Description);
                    s.Ncm = s.Ncm;
                    s.Annex.Description = s.Annex.Description;
                    ncmTemp.Add(s);
                });

                var ids = ncmTemp.Where(c =>
                    c.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Ncm.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Annex.Description.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                ncms = ncmsAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var ncm = from r in ncms
                          where ids.ToArray().Contains(r.Id)
                          select new
                          {
                              Id = r.Id.ToString(),
                              Cest = r.Cest,
                              Code = r.Ncm,
                              Description = r.Description,
                              Anexx = r.Annex.Convenio == null || r.Annex.Convenio == "" ? r.Annex.Description : r.Annex.Description + " - " + r.Annex.Convenio,
                              Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                              Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")
                          };

                return Ok(new { draw = draw, recordsTotal = ncms.Count(), recordsFiltered = ncms.Count(), data = ncm.Skip(start).Take(lenght) });

            }
            else
            {


                var ncm = from r in ncmsAll
                          select new
                          {
                              Id = r.Id.ToString(),
                              Cest = r.Cest,
                              Code = r.Ncm,
                              Description = r.Description,
                              Anexx = r.Annex.Convenio == null || r.Annex.Convenio == "" ? r.Annex.Description : r.Annex.Description + " - " + r.Annex.Convenio,
                              Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                              Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")
                          };
                return Ok(new { draw = draw, recordsTotal = ncmsAll.Count(), recordsFiltered = ncmsAll.Count(), data = ncm.Skip(start).Take(lenght) });
            }

        }

    }
}
