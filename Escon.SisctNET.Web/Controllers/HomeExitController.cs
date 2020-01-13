using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Escon.SisctNET.Web.Controllers
{
    public class HomeExitController : ControllerBaseSisctNET
    {
        private readonly ICompanyService _service;
        public HomeExitController(
            ICompanyService service,
            Service.IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "HomeExit")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
        }

        public IActionResult Index()
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
                    var result = _service.FindByCompanies();
                    return View(result);
                }
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
