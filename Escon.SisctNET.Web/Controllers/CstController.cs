using System;
using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Escon.SisctNET.Web.Controllers
{
    public class CstController : ControllerBaseSisctNET
    {
        private readonly ICstService _service;
        private readonly ITaxationTypeNcmService _taxationTypeNcmService;


        public CstController(
            ICstService service,
            ITaxationTypeNcmService taxationTypeNcmService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Cst")
        {
            _service = service;
            _taxationTypeNcmService = taxationTypeNcmService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);

        }

        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cst")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindAll(null);
                return View(result);
            }
            catch(Exception e)
            {
                return BadRequest(new { erro = 500, message = e.Message });
            }
            
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cst")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                List<Model.TaxationTypeNcm> list_tipos = _taxationTypeNcmService.FindAll(null);
                list_tipos.Insert(0, new Model.TaxationTypeNcm() { Description = "Nennhum item selecionado", Id = 0 });
                SelectList tipos = new SelectList(list_tipos, "Id", "Description", null);
                ViewBag.TaxationTypeNcmId = tipos;
                return View();
            }
            catch(Exception e)
            {
                return BadRequest(new { erro = 500, message = e.Message});
            }
        }

        [HttpPost]
        public IActionResult Create(Model.Cst entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cst")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;
                _service.Create(entity, GetLog(Model.OccorenceLog.Create));
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return BadRequest(new { erro = 500, message = e.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cst")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                ViewBag.TaxationTypeNcmId = new SelectList(_taxationTypeNcmService.FindAll(null), "Id", "Description", null);
                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.Cst entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cst")).FirstOrDefault().Active)
                return Unauthorized();

            try 
            { 


                var result = _service.FindById(id, null);
                entity.Updated = DateTime.Now;
                entity.Type = result.Type;
                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cst")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                _service.Delete(id, GetLog(Model.OccorenceLog.Delete));
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return BadRequest(new { erro = 500, message = e.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateStatus([FromBody] Model.UpdateActive updateActive)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cst")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var entity = _service.FindById(updateActive.Id, null);
                entity.Ident = updateActive.Active;

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