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
    public class NcmConvenioController : ControllerBaseSisctNET
    {
        private readonly INcmConvenioService _service;
        private readonly IAnnexService _annexService;

        public NcmConvenioController(
            INcmConvenioService service,
            IAnnexService annexService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "NcmConvenio")
        {
            _annexService = annexService;
            _service = service;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index()
        {
            if (!SessionManager.GetNcmConvenioInSession().Equals(21))
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
                    var result = _service.FindAll(GetLog(Model.OccorenceLog.Read));
                    return View(null);
                }
               
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
           
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!SessionManager.GetNcmConvenioInSession().Equals(21))
            {
                return Unauthorized();
            }

            try
            {
                List<Annex> list_annex = _annexService.FindAll(GetLog(Model.OccorenceLog.Read));
                foreach (var annex in list_annex)
                {
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
            if (!SessionManager.GetNcmConvenioInSession().Equals(21))
            {
                return Unauthorized();
            }
            try
            {
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;
                _service.Create(entity, GetLog(Model.OccorenceLog.Create));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!SessionManager.GetNcmConvenioInSession().Equals(21))
            {
                return Unauthorized();
            }
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                List<Annex> list_annex = _annexService.FindAll(GetLog(Model.OccorenceLog.Read));
                foreach (var annex in list_annex)
                {
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
        public IActionResult Edit(int id, Model.NcmConvenio entity)
        {
            if (!SessionManager.GetNcmConvenioInSession().Equals(21))
            {
                return Unauthorized();
            }
            try
            {
                var rst = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                entity.Created = rst.Created;
                entity.Updated = DateTime.Now;
                _service.Update(entity, GetLog(Model.OccorenceLog.Read));
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!SessionManager.GetNcmConvenioInSession().Equals(21))
            {
                return Unauthorized();
            }
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

            var ncmsAll = _service.FindAll(GetLog(Model.OccorenceLog.Read)).OrderBy(_ => _.AnnexId);



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
                              Anexx = r.Annex.Description + r.Annex.Convenio

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
                              Anexx = r.Annex.Description + r.Annex.Convenio

                          };
                return Ok(new { draw = draw, recordsTotal = ncmsAll.Count(), recordsFiltered = ncmsAll.Count(), data = ncm.Skip(start).Take(lenght) });
            }

        }

    }
}
