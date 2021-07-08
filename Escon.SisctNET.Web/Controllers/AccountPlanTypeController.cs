using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{

    public class AccountPlanTypeController : ControllerBaseSisctNET
    {
        private readonly Service.IAccountPlanTypeService _service;
        private readonly Service.IAccountPlanTypeGroupService _accountPlanTypeGroupService;

        public AccountPlanTypeController(
            Service.IAccountPlanTypeService service,
            Service.IAccountPlanTypeGroupService accountPlanTypeGroupService,
            Service.IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor
            ) : base(functionalityService, "AccountPlanType")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _accountPlanTypeGroupService = accountPlanTypeGroupService;
        }

        [HttpGet]
        public IActionResult Index()
        {

            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("AccountPlanType")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindAll(null).OrderBy(_ => _.AccountPlanTypeGroupId).ToList();

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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("AccountPlanType")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                List<Model.AccountPlanTypeGroup> list_grupos = _accountPlanTypeGroupService.FindAll(null);

                foreach (var g in list_grupos)
                {
                    g.Name = g.Id + " - " + g.Name;
                }

                list_grupos.Insert(0, new Model.AccountPlanTypeGroup() { Name = "Nennhum item selecionado", Id = 0 });
                SelectList grupos = new SelectList(list_grupos, "Id", "Name", null);
                ViewBag.AccountPlanTypeGroupId = grupos;

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create(Model.AccountPlanType entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("AccountPlanType")).FirstOrDefault().Active)
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
        public IActionResult Edit(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("AccountPlanType")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);

                List<Model.AccountPlanTypeGroup> list_grupos = _accountPlanTypeGroupService.FindAll(null);
                foreach (var g in list_grupos)
                {
                    g.Name = g.Id + " - " + g.Name;
                }

                SelectList grupos = new SelectList(list_grupos, "Id", "Name", null);
                ViewBag.AccountPlanTypeGroupId = grupos;

                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.AccountPlanType entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("AccountPlanType")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var rst = _service.FindById(id, null);
                entity.Created = rst.Created;

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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("AccountPlanType")).FirstOrDefault().Active)
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

        [HttpPost]
        public IActionResult UpdateStatus([FromBody] Model.UpdateActive updateActive)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("AccountPlanType")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var entity = _service.FindById(updateActive.Id, null);
                entity.Active = updateActive.Active;

                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return Ok(new { requestcode = 200, message = "ok" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { requestcode = 500, message = ex.Message });
            }
        }

        public IActionResult GetAll()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("AccountPlanType")).FirstOrDefault().Active)
                return Unauthorized();

            List<OutputDropdownList> dropdown = new List<OutputDropdownList>();

            List<Model.AccountPlanType> accountPlanTypes = new List<Model.AccountPlanType>();

            if (SessionManager.GetAccountPlanTypeInSession() == null)
            {
                accountPlanTypes = _service.FindAll(null).OrderBy(_ => _.AccountPlanTypeGroupId).ToList();
                SessionManager.SetAccountPlanTypeInSession(accountPlanTypes);
            }

            if (!string.IsNullOrEmpty(Request.Query["registration[term]"].ToString()))
            {
                accountPlanTypes = SessionManager.GetAccountPlanTypeInSession().Where(a => a.Name.Contains(Request.Query["registration[term]"].ToString())).ToList();
            }
            else
            {
                accountPlanTypes = SessionManager.GetAccountPlanTypeInSession().OrderBy(_ => _.AccountPlanTypeGroupId).ToList();
            }

            foreach (var item in accountPlanTypes)
            {
                dropdown.Add(new OutputDropdownList() { id = item.Id, text = item.AccountPlanTypeGroup.Id + " - " + item.Name });
            }

            return Ok(new { results = dropdown, pagination = new { more = false } });
        }
    }
}
