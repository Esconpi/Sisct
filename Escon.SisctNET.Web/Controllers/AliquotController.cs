using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = _service.FindAll(GetLog(Model.OccorenceLog.Read));
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var list_states = _stateService.FindAll(null).OrderBy(_ => _.UF).ToList();
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var aliq = _service.FindAll(GetLog(Model.OccorenceLog.Create));
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
        public IActionResult Edit(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var list_states = _stateService.FindAll(null).OrderBy(_ => _.UF).ToList();
                SelectList states = new SelectList(list_states, "Id", "Name", null);
                ViewBag.StateOrigemId = states;

                ViewBag.StateDestinoId = states;

                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(int id, Model.Aliquot entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var rst = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
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
        public IActionResult Delete(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Aliquot")).FirstOrDefault() == null)
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
    }
}
