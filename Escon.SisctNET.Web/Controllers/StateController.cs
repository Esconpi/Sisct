using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class StateController : ControllerBaseSisctNET
    {
        private readonly IStateService _service;

        public StateController(
            IStateService service,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "State")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
        }

        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("State")).FirstOrDefault() == null)
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("State")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create(Model.State entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("State")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var aliq = _service.FindAll(GetLog(Model.OccorenceLog.Create));
                foreach (var a in aliq)
                {
                    if (a.UfOrigem.Equals(entity.UfOrigem) && a.UfDestino.Equals(entity.UfDestino))
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
        public IActionResult Delete(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("State")).FirstOrDefault() == null)
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
