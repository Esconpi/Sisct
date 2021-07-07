﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{

    public class AccountPlanTypeController : ControllerBaseSisctNET
    {
        private readonly Service.IAccountPlanTypeService _service;

        public AccountPlanTypeController(
            Service.IAccountPlanTypeService service,
            Service.IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor
            ) : base(functionalityService, "AccountPlanType")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
        }

        [HttpGet]
        public IActionResult Index()
        {

            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("AccountPlanType")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindAll(null);

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

            return View();
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
                var rst = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
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
                accountPlanTypes = _service.FindAll(null);
                SessionManager.SetAccountPlanTypeInSession(accountPlanTypes);
            }

            if (!string.IsNullOrEmpty(Request.Query["registration[term]"].ToString()))
            {
                accountPlanTypes = SessionManager.GetAccountPlanTypeInSession().Where(a => a.Name.Contains(Request.Query["registration[term]"].ToString())).ToList();
            }
            else
            {
                accountPlanTypes = SessionManager.GetAccountPlanTypeInSession();
            }

            foreach (var item in accountPlanTypes)
            {
                dropdown.Add(new OutputDropdownList() { id = item.Id, text = item.Name });
            }

            return Ok(new { results = dropdown, pagination = new { more = false } });
        }
    }
}
