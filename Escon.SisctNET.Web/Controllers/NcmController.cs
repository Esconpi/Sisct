using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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
                    int contaPage = rst.Count() / 1000;
                    if(rst.Count() % 1000 > 0)
                    {
                        contaPage++;
                    }
                    int final = page * 1000;
                    int inicio = final - 1000;
                    var result = rst.Where(_ => _.Id > inicio && _.Id <= final).ToList();
                    ViewBag.ContaPage = contaPage;
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
            return View();
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
    }
}