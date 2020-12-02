using System;
using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Escon.SisctNET.Web.Controllers
{
    public class HomeExitController : ControllerBaseSisctNET
    {
        private readonly ICompanyService _service;
        private readonly ICfopService _cfopService;
        private readonly IHostingEnvironment _appEnvironment;

        public HomeExitController(
            ICompanyService service,
            ICfopService cfopService,
            IHostingEnvironment env,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "HomeExit")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _cfopService = cfopService;
            _appEnvironment = env;
        }

        public IActionResult Index()
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }

            try
            {
                var result = _service.FindByCompanies();
                SessionManager.SetTipoInSession(1);
                return View(null);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult PisCofins(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }

            try
            {
                var result = _service.FindById(id, null);
                List<Cfop> list_cfop = _cfopService.FindAll(null);
                foreach (var cfop in list_cfop)
                {
                    cfop.Description = cfop.Code + " - " + cfop.Description;
                }
                SelectList cfops = new SelectList(list_cfop, "Id", "Description", null);
                ViewBag.CfopId = cfops;
                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
       
        public IActionResult Import(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        public IActionResult Sincronize(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Icms(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }
            try
            {
                var comp = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                List<Cfop> list_cfop = _cfopService.FindAll(null);
                foreach (var cfop in list_cfop)
                {
                    cfop.Description = cfop.Code + " - " + cfop.Description;
                }
                SelectList cfops = new SelectList(list_cfop, "Id", "Description", null);
                ViewBag.CfopId = cfops;
                ViewBag.TypeCompany = comp.TypeCompany;
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Sequence(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
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

        public IActionResult CompareCancellation(int id)
        {

            if (SessionManager.GetLoginInSession().Equals(null))
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
    }
}
