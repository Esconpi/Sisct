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
        INcmService _service;

        public NcmController(
            INcmService service,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Ncm")
        {
            _service = service;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }


        [HttpGet]
        public IActionResult Index(int page = 1)
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
                    var rst = _service.FindAll(GetLog(Model.OccorenceLog.Read));
                    //if (ncm.Equals(""))
                    //{
                    //    result = result.Take(500).ToList();
                    //}
                    //else if (!ncm.Equals(""))
                    //{
                    //    List<Model.Ncm> lista = new List<Model.Ncm>();
                    //    foreach (var code in result)
                    //    {
                    //        if(code.Code.Contains(ncm))
                    //        {
                    //            lista.Add(code);
                    //        }
                    //    }
                    //    result = lista;
                    //}
                    int contaPage = rst.Count() / 1000;
                    if(rst.Count() % 1000 > 0)
                    {
                        contaPage++;
                    }
                    int final = page * 1000;
                    int inicio = final - 1000;
                    var result = rst.Where(_ => _.Id > inicio && _.Id <= final).ToList();
                    
                    ViewBag.ContaPage = contaPage;

<<<<<<< HEAD
=======
                    //var result = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Take(500);
                    //foreach(var rst in result)
                    //{
                    //    if (rst.Description.Length > 50)
                    //    {
                    //        rst.Description = rst.Description.Substring(0, 50);
                    //    }
                    //}

>>>>>>> origin/Tiago
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
        public IActionResult Edit(int id, Model.Ncm entity)
        {
            try
            {
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
            try
            {
                var entity = _service.FindById(updateActive.Id, GetLog(Model.OccorenceLog.Read));
                entity.Active = updateActive.Active;

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