using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IStateService _stateService;
        private readonly ICstService _cstService;
        private readonly ICsosnService _csosnService;
        private readonly IHostingEnvironment _appEnvironment;

        public HomeExitController(
            ICompanyService service,
            ICfopService cfopService,
            IStateService stateService,
            ICstService cstService,
            ICsosnService csosnService,
            IHostingEnvironment env,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Company")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _cfopService = cfopService;
            _stateService = stateService;
            _cstService = cstService;
            _csosnService = csosnService;
            _appEnvironment = env;
        }

        public IActionResult Index()
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                SessionManager.SetTipoInSession(1);
                return View(null);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult PisCofins(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

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
       
        public IActionResult Import(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        public IActionResult Sincronize(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Icms(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _service.FindById(id, null);
                var list_cfop = _cfopService.FindAll(null).OrderBy(_ => _.Code).ToList();
                list_cfop.Insert(0, new Model.Cfop() { Description = "Nennhum cfop", Id = 0 });
                foreach (var cfop in list_cfop)
                {
                    if(cfop.Id != 0)
                        cfop.Description = cfop.Code + " - " + cfop.Description;
                }
                SelectList cfops = new SelectList(list_cfop, "Id", "Description", null);
                ViewBag.CfopId = cfops;

                var list_cst = _cstService.FindByIdent(true).OrderBy(_ => _.Code).ToList();
                list_cst.Insert(0, new Model.Cst() { Description = "Nenhum CST", Id = 0 });
                foreach (var cst in list_cst)
                {
                    if (cst.Id != 0)
                        cst.Description = cst.Code + " - " + cst.Description;
                }
                SelectList csts = new SelectList(list_cst, "Id", "Description", null);
                ViewBag.CstId = csts;

                var list_csosn = _csosnService.FindAll(null).OrderBy(_ => _.Code).ToList();
                list_csosn.Insert(0, new Model.Csosn() { Name = "Nenhum Csosn", Id = 0 });
                foreach (var csosn in list_csosn)
                {
                    if (csosn.Id != 0)
                        csosn.Name = csosn.Code + " - " + csosn.Name;
                }
                SelectList csosns = new SelectList(list_csosn, "Id", "Name", null);
                ViewBag.CsosnId = csosns;

                var list_states = _stateService.FindAll(null).Where(_ => !_.UF.Equals("EXT")).OrderBy(_ => _.UF).ToList();
                list_states.Insert(0, new Model.State() { Name = "Nenhuma UF", Id = 0 });
                foreach (var state in list_states)
                {
                    if(state.Id != 0)
                        state.Name = state.Name + " - " + state.UF;
                }
                SelectList states = new SelectList(list_states, "Id", "Name", null);
                ViewBag.StateId = states;

                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Sequence(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

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

        public IActionResult CompareCancellation(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

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

    }
}
