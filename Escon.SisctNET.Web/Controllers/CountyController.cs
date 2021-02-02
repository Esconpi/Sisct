using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class CountyController : ControllerBaseSisctNET
    {
        private readonly ICountyService _service;
        private readonly IStateService _stateService;

        public CountyController(
            ICountyService service,
            IStateService stateService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "County")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _stateService = stateService;
        }

        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("County")).FirstOrDefault().Active)
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("County")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var list_states = _stateService.FindAll(null).Where(_ => !_.UF.Equals("EXT")).OrderBy(_ => _.UF).ToList();
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
        public IActionResult Create(Model.County entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("County")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;
                var result = _service.Create(entity, GetLog(Model.OccorenceLog.Create));
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("County")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var list_states = _stateService.FindAll(null).Where(_ => !_.UF.Equals("EXT")).OrderBy(_ => _.UF).ToList();
                foreach (var s in list_states)
                {
                    s.Name = s.Name + " - " + s.UF;
                }

                SelectList states = new SelectList(list_states, "Id", "Name", null);
                ViewBag.StateId = states;

                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
        
        [HttpPost]
        public IActionResult Edit(int id, Model.County entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("County")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                result.Code = entity.Code;
                result.Name = entity.Name;
                result.StateId = entity.StateId;
                result.Updated = DateTime.Now;
                _service.Update(result, GetLog(Model.OccorenceLog.Create));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("County")).FirstOrDefault().Active)
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

            var countyAll = _service.FindAll(null).OrderBy(_ => _.State.Name).ThenBy(_ => _.Name).ToList();


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Model.County> counties = new List<Model.County>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Model.County> countyTemp = new List<Model.County>();
                countyAll.ToList().ForEach(s =>
                {
                    countyTemp.Add(s);
                });

                var ids = countyTemp.Where(c =>
                    c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.State.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    )
                .Select(s => s.Id).ToList();

                counties = countyAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var county = from r in counties
                          where ids.ToArray().Contains(r.Id)
                          select new
                          {
                              Id = r.Id.ToString(),
                              Code = r.Code,
                              Name = r.Name,
                              State = r.State.Name + "-" + r.State.UF
                          };

                return Ok(new { draw = draw, recordsTotal = counties.Count(), recordsFiltered = counties.Count(), data = county.Skip(start).Take(lenght) });

            }
            else
            {

                var county = from r in countyAll
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  Code = r.Code,
                                  Name = r.Name,
                                  State = r.State.Name + "-" + r.State.UF
                              };
                return Ok(new { draw = draw, recordsTotal = countyAll.Count(), recordsFiltered = countyAll.Count(), data = county.Skip(start).Take(lenght) });
            }

        }
    }
}
