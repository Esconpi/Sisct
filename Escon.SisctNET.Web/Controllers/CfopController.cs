﻿using System;
using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Escon.SisctNET.Web.Controllers
{
    public class CfopController : ControllerBaseSisctNET
    {
        ICfopService _service;

        public CfopController(
            ICfopService service,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Cfop")
        {
            _service = service;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cfop")).FirstOrDefault() == null)
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
                    return View(null);
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cfop")).FirstOrDefault() == null)
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
        public IActionResult Create(Model.Cfop entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cfop")).FirstOrDefault() == null)
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cfop")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

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
        public IActionResult Edit(int id, Model.Cfop entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cfop")).FirstOrDefault() == null)
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cfop")).FirstOrDefault() == null)
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

        public IActionResult GetAll(int draw, int start)
        {


            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var cfopsAll = _service.FindAll(GetLog(Model.OccorenceLog.Read)).OrderBy(_ => _.Code);


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Cfop> cfops = new List<Cfop>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Cfop> cfopTemp = new List<Cfop>();
                cfopsAll.ToList().ForEach(s =>
                {
                    s.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Description);
                    s.Code = s.Code;
                    cfopTemp.Add(s);
                });

                var ids = cfopTemp.Where(c =>
                    c.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                cfops = cfopsAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var cfop = from r in cfops
                           where ids.ToArray().Contains(r.Id)
                           select new
                           {
                               Id = r.Id.ToString(),
                               Code = r.Code,
                               Description = r.Description

                           };

                return Ok(new { draw = draw, recordsTotal = cfops.Count(), recordsFiltered = cfops.Count(), data = cfop.Skip(start).Take(lenght) });

            }
            else
            {


                var cfop = from r in cfopsAll
                           select new
                           {
                               Id = r.Id.ToString(),
                               Code = r.Code,
                               Description = r.Description

                           };
                return Ok(new { draw = draw, recordsTotal = cfopsAll.Count(), recordsFiltered = cfopsAll.Count(), data = cfop.Skip(start).Take(lenght) });
            }

        }

    }
}