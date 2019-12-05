using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Escon.SisctNET.Web.Controllers
{
    public class AttachmentController : ControllerBaseSisctNET
    {
        private readonly IAttachmentService _service;

        public AttachmentController(
           IAttachmentService service,
           IFunctionalityService functionalityService,
           IHttpContextAccessor httpContextAccessor)
           : base(functionalityService, "Attachment")
        {
            _service = service;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }


        [HttpGet]
        public IActionResult Index()
        {
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
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                return View();
            }catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
            
        }


        [HttpPost]
        public IActionResult Create(Model.Attachment entity)
        {
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
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(int id, Model.Attachment entity)
        {
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                entity.Created = result.Created;
                entity.Updated = DateTime.Now;
                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}