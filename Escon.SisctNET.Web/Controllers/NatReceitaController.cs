﻿using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class NatReceitaController : ControllerBaseSisctNET
    {
        private readonly INatReceitaService _service;
        private readonly ICstService _cstService;

        public NatReceitaController(
            INatReceitaService service,
            ICstService cstService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "NatReceita")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _cstService = cstService;
        }

        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("NatReceita")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("NatReceita")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                List<Model.Cst> list_cst = _cstService.FindAll(null);
                SelectList cst = new SelectList(list_cst, "Id", "Code", null);
                ViewBag.CstId = cst;

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create(Model.NatReceita entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("NatReceita")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("NatReceita")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                List<Model.Cst> list_cst = _cstService.FindAll(null);
                SelectList cst = new SelectList(list_cst, "Id", "Code", null);
                ViewBag.CstId = cst;

                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.NatReceita entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("NatReceita")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var rst = _service.FindById(id, null);
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("NatReceita")).FirstOrDefault().Active)
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

        public IActionResult GetAll(int draw, int start)
        {
            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var natReceitaAll = _service.FindAll(null).OrderBy(_ => _.Cst.Code);


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Model.NatReceita> natReceitas = new List<Model.NatReceita>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Model.NatReceita> natReceitasTemp = new List<Model.NatReceita>();
                natReceitaAll.ToList().ForEach(s =>
                {
                    s.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Description);
                    natReceitasTemp.Add(s);
                });

                var ids = natReceitasTemp.Where(c =>
                    c.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.CodigoAC.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Cst.Code.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                natReceitas = natReceitaAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var cfop = from r in natReceitas
                           where ids.ToArray().Contains(r.Id)
                           select new
                           {
                               Id = r.Id.ToString(),
                               Code = r.Code,
                               CodigoAc = r.CodigoAC,
                               Cst = r.Cst.Code,
                               Description = r.Description

                           };

                return Ok(new { draw = draw, recordsTotal = natReceitas.Count(), recordsFiltered = natReceitas.Count(), data = cfop.Skip(start).Take(lenght) });

            }
            else
            {


                var cfop = from r in natReceitaAll
                           select new
                           {
                               Id = r.Id.ToString(),
                               Code = r.Code,
                               CodigoAc = r.CodigoAC,
                               Cst = r.Cst.Code,
                               Description = r.Description

                           };
                return Ok(new { draw = draw, recordsTotal = natReceitaAll.Count(), recordsFiltered = natReceitaAll.Count(), data = cfop.Skip(start).Take(lenght) });
            }
        }
    }
}
