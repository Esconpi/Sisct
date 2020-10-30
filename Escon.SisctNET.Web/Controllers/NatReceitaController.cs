using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class NatReceitaController : ControllerBaseSisctNET
    {
        private readonly INatReceitaService _service;

        public NatReceitaController(
            INatReceitaService service,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "NatReceita")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
        }

        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("NatReceita")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("NatReceita")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

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
        public IActionResult Create(Model.NatReceita entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("NatReceita")).FirstOrDefault() == null)
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("NatReceita")).FirstOrDefault() == null)
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
        public IActionResult Edit(int id, Model.NatReceita entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("NatReceita")).FirstOrDefault() == null)
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

            var natReceitaAll = _service.FindAll(GetLog(Model.OccorenceLog.Read)).OrderBy(_ => _.Cst);


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Model.NatReceita> natReceitas = new List<Model.NatReceita>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Model.NatReceita> natReceitasTemp = new List<Model.NatReceita>();
                natReceitaAll.ToList().ForEach(s =>
                {
                    s.CodigoAC = s.CodigoAC;
                    s.Code = s.Code;
                    s.Cst = s.Cst;
                    natReceitasTemp.Add(s);
                });

                var ids = natReceitasTemp.Where(c =>
                    c.CodigoAC.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Cst.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                natReceitas = natReceitaAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var cfop = from r in natReceitas
                           where ids.ToArray().Contains(r.Id)
                           select new
                           {
                               Id = r.Id.ToString(),
                               Code = r.Code,
                               CodigoAc = r.CodigoAC,
                               Cst = r.Cst,
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
                               Cst = r.Cst,
                               Description = r.Description

                           };
                return Ok(new { draw = draw, recordsTotal = natReceitaAll.Count(), recordsFiltered = natReceitaAll.Count(), data = cfop.Skip(start).Take(lenght) });
            }
        }
    }
}
