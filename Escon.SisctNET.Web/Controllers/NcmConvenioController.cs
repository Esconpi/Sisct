using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

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
                    return View(result);
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
                ViewBag.AnnexId = new SelectList(_annexService.FindAll(GetLog(Model.OccorenceLog.Read)), "Id", "Description", null);
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
                ViewBag.AnnexId = new SelectList(_annexService.FindAll(GetLog(Model.OccorenceLog.Read)), "Id", "Description", null);
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
    }
}
