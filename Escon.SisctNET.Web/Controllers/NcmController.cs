﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class NcmController : ControllerBaseSisctNET
    {
        private readonly INcmService _service;
        private readonly ICstService _cstService;

        public NcmController(
            INcmService service,
            ICstService cstService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Ncm")
        {
            _service = service;
            _cstService = cstService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }


        [HttpGet]
        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var rst = _service.FindAll(null);

                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Create()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpPost]
        public IActionResult Create(Model.Ncm entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
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
        public IActionResult Edit(long id, Model.Ncm entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var rst = _service.FindById(id, null);
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
        public IActionResult Delete(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Ncm")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var entity = _service.FindById(updateActive.Id, null);
                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return Ok(new { requestcode = 200, message = "ok" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { requestcode = 500, message = ex.Message });
            }
        }

        public IActionResult GetAll(int draw, int start)
        {
            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var ncmsAll = _service.FindAll(null).OrderBy(_ => _.Code).ToList();



            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Ncm> ncms = new List<Ncm>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Ncm> ncmTemp = new List<Ncm>();
                ncmsAll.ToList().ForEach(s =>
                {
                    s.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Description);
                    ncmTemp.Add(s);
                });

                var ids = ncmTemp.Where(c =>
                    c.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                ncms = ncmsAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var ncm = from r in ncms
                          where ids.ToArray().Contains(r.Id)
                          select new
                          {
                              Id = r.Id.ToString(),
                              Code = r.Code,
                              Description = r.Description

                          };

                return Ok(new { draw = draw, recordsTotal = ncms.Count(), recordsFiltered = ncms.Count(), data = ncm.Skip(start).Take(lenght) });

            }
            else
            {


                var ncm = from r in ncmsAll
                          select new
                          {
                              Id = r.Id.ToString(),
                              Code = r.Code,
                              Description = r.Description

                          };
                return Ok(new { draw = draw, recordsTotal = ncmsAll.Count(), recordsFiltered = ncmsAll.Count(), data = ncm.Skip(start).Take(lenght) });
            }

        }
    }
}