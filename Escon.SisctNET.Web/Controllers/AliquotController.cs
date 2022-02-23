using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class AliquotController : ControllerBaseSisctNET
    {
        private readonly IAliquotService _service;
        private readonly IStateService _stateService;

        public AliquotController(
            IAliquotService service,
            IStateService stateService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Aliquot")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _stateService = stateService;
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

                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create(Model.Aliquot entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var aliq = _service.FindAll(null);
                foreach (var a in aliq)
                {
                    if (a.StateOrigemId.Equals(entity.StateOrigemId) && a.StateDestinoId.Equals(entity.StateDestinoId))
                    {
                        a.DateEnd = entity.DateStart.AddDays(-1);
                        _service.Update(a, GetLog(Model.OccorenceLog.Update));
                    }
                }
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;

                _service.Create(entity, GetLog(Model.OccorenceLog.Create));
                return RedirectToAction("Index");
            }
            catch(Exception ex)
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

                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.Aliquot entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var rst = _service.FindById(id, null);
                entity.Created = rst.Created;
                entity.Updated = DateTime.Now;
                var result = _service.Update(entity, GetLog(Model.OccorenceLog.Update));
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

            var aliquotAll = _service.FindByAllState(null);


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Model.Aliquot> aliquots = new List<Model.Aliquot>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Model.Aliquot> aliquotTemp = new List<Model.Aliquot>();
                aliquotAll.ToList().ForEach(s =>
                {
                    s.StateOrigem.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.StateOrigem.Name);
                    s.StateDestino.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.StateDestino.Name);
                    aliquotTemp.Add(s);
                });

                var ids = aliquotTemp.Where(c =>
                    c.StateOrigem.UF.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.StateOrigem.UF.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.StateOrigem.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.StateDestino.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) 
                    )
                .Select(s => s.Id).ToList();

                aliquots = aliquotAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var aliquot = from r in aliquots
                             where ids.ToArray().Contains(r.Id)
                             select new
                             {
                                 Id = r.Id.ToString(),
                                 StateOrigem = r.StateOrigem.Name + "-" + r.StateOrigem.UF,
                                 StateDestino = r.StateDestino.Name + "-" + r.StateDestino.UF,
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
                                 Aliquota = r.Aliquota,
                                 Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                                 Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")
                             };
                return Ok(new { draw = draw, recordsTotal = aliquotAll.Count(), recordsFiltered = aliquotAll.Count(), data = aliquot.Skip(start).Take(lenght) });
            }

        }
    }
}
