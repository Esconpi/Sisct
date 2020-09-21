using System;
using System.Linq;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Escon.SisctNET.Web.Controllers
{
    public class HomeExitController : ControllerBaseSisctNET
    {
        private readonly ICompanyService _service;
        private readonly IHostingEnvironment _appEnvironment;

        public HomeExitController(
            ICompanyService service,
            IHostingEnvironment env,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "HomeExit")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
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
                var login = SessionManager.GetLoginInSession();

                if (login == null)
                {
                    return RedirectToAction("Index", "Authentication");
                }
                else
                {
                    var result = _service.FindByCompanies();
                    SessionManager.SetTipoInSession(1);
                    return View(null);
                }
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
                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
       
        public IActionResult Import(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault() == null)
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

    }
}
