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
    public class InternalAliquotController : ControllerBaseSisctNET
    {
        private readonly IInternalAliquotService _service;
        private readonly IStateService _stateService;

        public InternalAliquotController(
            IInternalAliquotService service,
            IStateService stateService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "InternalAliquot")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _stateService = stateService;
        }

        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("InternalAliquot")).FirstOrDefault().Active)
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("InternalAliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var list_states = _stateService.FindAll(null).OrderBy(_ => _.UF).ToList();
                foreach (var s in list_states)
                {
                    s.Name = s.Name + " - " + s.UF;
                }

                SelectList states = new SelectList(list_states, "Id", "Name", null);
                ViewBag.StateId = states;

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create(Model.InternalAliquot entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("InternalAliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var item = _service.FindByAliquot(entity.StateId);

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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("InternalAliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var list_states = _stateService.FindAll(null).OrderBy(_ => _.UF).ToList();
                foreach (var s in list_states)
                {
                    s.Name = s.Name + " - " + s.UF;
                }

                SelectList states = new SelectList(list_states, "Id", "Name", null);
                ViewBag.StateId = states;

                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.InternalAliquot entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("InternalAliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var rst = _service.FindById(id, null);
                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
               
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("InternalAliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var list_states = _stateService.FindAll(null).OrderBy(_ => _.UF).ToList();
                foreach (var s in list_states)
                {
                    s.Name = s.Name + " - " + s.UF;
                }

                SelectList states = new SelectList(list_states, "Id", "Name", null);
                ViewBag.StateId = states;

                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Atualize(long id, Model.InternalAliquot entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("InternalAliquot")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                if (result != null)
                {
                    result.DateEnd = Convert.ToDateTime(entity.DateStart).AddDays(-1);
                    _service.Update(result, GetLog(Model.OccorenceLog.Update));
                }

                InternalAliquot aliquot = new InternalAliquot();
                aliquot.StateId = result.StateId;
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("InternalAliquot")).FirstOrDefault().Active)
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
                List<Model.InternalAliquot> aliquots = new List<Model.InternalAliquot>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Model.InternalAliquot> aliquotTemp = new List<Model.InternalAliquot>();
                aliquotAll.ToList().ForEach(s =>
                {
                    s.State.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.State.Name);
                    aliquotTemp.Add(s);
                });

                var ids = aliquotTemp.Where(c =>
                    c.State.UF.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.State.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                aliquots = aliquotAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var aliquot = from r in aliquots
                              where ids.ToArray().Contains(r.Id)
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  State = r.State.Name + "-" + r.State.UF,
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
                                  State = r.State.Name + "-" + r.State.UF,
                                  Aliquota = r.Aliquota,
                                  Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                                  Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")
                              };
                return Ok(new { draw = draw, recordsTotal = aliquotAll.Count(), recordsFiltered = aliquotAll.Count(), data = aliquot.Skip(start).Take(lenght) });
            }

        }
    }
}
