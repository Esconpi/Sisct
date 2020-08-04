using System;
using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Escon.SisctNET.Web.Controllers
{
    public class GroupController : ControllerBaseSisctNET
    {
        private readonly IGroupService _service;
        private readonly IAttachmentService _attachmentService;

        public GroupController(
           IGroupService service,
           IAttachmentService attachmentService,
           IFunctionalityService functionalityService,
           IHttpContextAccessor httpContextAccessor)
           : base(functionalityService, "Group")
        {
            _service = service;
            _attachmentService = attachmentService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }


        [HttpGet]
        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Group")).FirstOrDefault() == null)
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
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Create()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Group")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                List<Model.Attachment> list_anexos = _attachmentService.FindAll(GetLog(Model.OccorenceLog.Read));
                list_anexos.Insert(0, new Model.Attachment() { Description = "Nennhum item selecionado", Id = 0 });
                SelectList anexos = new SelectList(list_anexos, "Id", "Description", null);
                ViewBag.AttachmentId = anexos;
                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create(Model.Group entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Group")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Group")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                ViewBag.AttachmentId = new SelectList(_attachmentService.FindAll(GetLog(Model.OccorenceLog.Read)), "Id", "Description", null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
        
        [HttpPost]
        public IActionResult Edit(int id , Model.Group entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Group")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                entity.Created = result.Created;
                entity.Updated = DateTime.Now;
                entity.Active = result.Active;
                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateStatus([FromBody] Model.UpdateActive updateActive)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Group")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var entity = _service.FindById(updateActive.Id, GetLog(Model.OccorenceLog.Read));
                entity.Active = updateActive.Active;

                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return Ok(new { requestcode = 200, message = "ok" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { requestcode = 500, message = ex.Message });
            }
        }
    }
}