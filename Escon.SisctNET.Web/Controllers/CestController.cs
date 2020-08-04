using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class CestController : ControllerBaseSisctNET
    {
        ICestService _service;
        public CestController(
            ICestService service,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Cest")
        {
            _service = service;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cest")).FirstOrDefault() == null)
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cest")).FirstOrDefault() == null)
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
        public IActionResult Create(Model.Cest entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cest")).FirstOrDefault() == null)
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cest")).FirstOrDefault() == null)
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
        public IActionResult Edit(int id, Model.Cest entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cest")).FirstOrDefault() == null)
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Cest")).FirstOrDefault() == null)
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

            var cestsAll = _service.FindAll(GetLog(Model.OccorenceLog.Read)).OrderBy(_ => _.Code);



            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Cest> cests = new List<Cest>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Cest> cestTemp = new List<Cest>();
                cestsAll.ToList().ForEach(s =>
                {
                    s.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Description);
                    s.Code = s.Code;
                    cestTemp.Add(s);
                });

                var ids = cestTemp.Where(c =>
                    c.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                cests = cestsAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var cest = from r in cests
                          where ids.ToArray().Contains(r.Id)
                          select new
                          {
                              Id = r.Id.ToString(),
                              Code = r.Code,
                              Description = r.Description

                          };

                return Ok(new { draw = draw, recordsTotal = cests.Count(), recordsFiltered = cests.Count(), data = cest.Skip(start).Take(lenght) });

            }
            else
            {


                var cest = from r in cestsAll
                          select new
                          {
                              Id = r.Id.ToString(),
                              Code = r.Code,
                              Description = r.Description

                          };
                return Ok(new { draw = draw, recordsTotal = cestsAll.Count(), recordsFiltered = cestsAll.Count(), data = cest.Skip(start).Take(lenght) });
            }

        }
    }
}