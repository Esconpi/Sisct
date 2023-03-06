using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Escon.SisctNET.Model;

namespace Escon.SisctNET.Web.Controllers
{
    public class AliquotConfazController : ControllerBaseSisctNET
    {
        private readonly IAliquotConfazService _service;
        private readonly IStateService _stateService;
        private readonly IAnnexService _annexService;

        public AliquotConfazController(
            IAliquotConfazService service,
            IStateService stateService,
            IAnnexService annexService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Aliquot")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _stateService = stateService;
            _annexService = annexService;
        }

        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var list_states = _stateService.FindAll(null).OrderBy(_ => _.UF).ToList();
                foreach (var s in list_states)
                {
                    s.Name = s.Name + " - " + s.UF;
                }

                SelectList states = new SelectList(list_states, "Id", "Name", null);
                ViewBag.StateOrigemId = states;

                ViewBag.StateDestinoId = states;

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
        public IActionResult Create(Model.AliquotConfaz entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var item = _service.FindByAliquot(entity.StateOrigemId, entity.StateDestinoId, entity.AnnexId);

                if (item != null)
                {
                    item.DateEnd = entity.DateStart.AddDays(-1);
                    _service.Update(item, null);
                }

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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var list_states = _stateService.FindAll(null).OrderBy(_ => _.UF).ToList();
                foreach (var s in list_states)
                {
                    s.Name = s.Name + " - " + s.UF;
                }

                SelectList states = new SelectList(list_states, "Id", "Name", null);
                ViewBag.StateOrigemId = states;

                ViewBag.StateDestinoId = states;

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

                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.AliquotConfaz entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Atualize(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var list_states = _stateService.FindAll(null).OrderBy(_ => _.UF).ToList();
                foreach (var s in list_states)
                {
                    s.Name = s.Name + " - " + s.UF;
                }

                SelectList states = new SelectList(list_states, "Id", "Name", null);
                ViewBag.StateOrigemId = states;

                ViewBag.StateDestinoId = states;

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

                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Atualize(long id, Model.AliquotConfaz entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                if (result != null)
                {
                    result.DateEnd = Convert.ToDateTime(entity.DateStart).AddDays(-1);
                    _service.Update(result, GetLog(Model.OccorenceLog.Update));
                }

                AliquotConfaz aliquot = new AliquotConfaz();
                aliquot.StateOrigemId = result.StateOrigemId;
                aliquot.StateDestinoId = result.StateDestinoId;
                aliquot.AnnexId = result.AnnexId;
                aliquot.Aliquota = entity.Aliquota;
                aliquot.DateStart = entity.DateStart;

                _service.Create(aliquot, GetLog(Model.OccorenceLog.Create));

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Delete(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault().Active)
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

            var aliquotAll = _service.FindByAllState(null).OrderBy(_ => _.StateOrigem.Name).ToList();


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Model.AliquotConfaz> aliquots = new List<Model.AliquotConfaz>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Model.AliquotConfaz> aliquotTemp = new List<Model.AliquotConfaz>();
                aliquotAll.ToList().ForEach(s =>
                {
                    s.StateOrigem.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.StateOrigem.Name);
                    s.StateDestino.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.StateDestino.Name);
                    s.Annex.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Annex.Description);
                    aliquotTemp.Add(s);
                });

                var ids = aliquotTemp.Where(c =>
                    c.StateOrigem.UF.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.StateOrigem.UF.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.StateOrigem.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.StateDestino.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Annex.Description.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                aliquots = aliquotAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var aliquot = from r in aliquots
                              where ids.ToArray().Contains(r.Id)
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  StateOrigem = r.StateOrigem.Name + "-" + r.StateOrigem.UF,
                                  StateDestino = r.StateDestino.Name + "-" + r.StateDestino.UF,
                                  Annex = r.Annex.Description + "-" + r.Annex.Convenio,
                                  Aliquota = r.Aliquota,
                                  Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                                  Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")
                              };

                return Ok(new { draw = draw, recordsTotal = aliquots.Count(), recordsFiltered = aliquots.Count(), data = aliquot.Skip(start).Take(lenght) });

            }
            else
            {

                var aliquot = from r in aliquotAll
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  StateOrigem = r.StateOrigem.Name + "-" + r.StateOrigem.UF,
                                  StateDestino = r.StateDestino.Name + "-" + r.StateDestino.UF,
                                  Annex = r.Annex.Description + "-" + r.Annex.Convenio,
                                  Aliquota = r.Aliquota,
                                  Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                                  Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")
                              };
                return Ok(new { draw = draw, recordsTotal = aliquotAll.Count(), recordsFiltered = aliquotAll.Count(), data = aliquot.Skip(start).Take(lenght) });
            }

        }
    }
}
