﻿using System;
using System.Collections.Generic;
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
            ViewBag.AttachmentId = new SelectList(_attachmentService.FindAll(GetLog(Model.OccorenceLog.Read)), "Id", "Description", null);
            
            return View();
        }

        [HttpPost]
        public IActionResult Create(Model.Group entity)
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
    }
}