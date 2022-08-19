using System;
using System.Linq;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Escon.SisctNET.Web.Controllers
{
    public class AnnexController : ControllerBaseSisctNET
    {

        private readonly IAnnexService _service;

        public AnnexController(
            IAnnexService service,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Attachment")
        {
            _service = service;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Attachment")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindAll(null);
                return View(result);

            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Attachment")).FirstOrDefault().Active)
                return Unauthorized();

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
        public IActionResult Create(Model.Annex entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Attachment")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;

                if (entity.Convenio.Equals(null))
                    entity.Convenio = "";

                _service.Create(entity,GetLog(Model.OccorenceLog.Create));
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Attachment")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.Annex entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Attachment")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var rst = _service.FindById(id, null);
                entity.Created = rst.Created;
                entity.Updated = DateTime.Now;

                if (entity.Convenio.Equals(null))
                    entity.Convenio = "";

                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Delete(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Attachment")).FirstOrDefault().Active)
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
    }
}
