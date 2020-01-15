using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Escon.SisctNET.Web.Controllers
{
    public class HomeController : ControllerBaseSisctNET
    {

        private readonly ICompanyService _service;
        public HomeController(
            ICompanyService service,
            Service.IFunctionalityService functionalityService, 
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Home")
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
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
            
        }
    }
}
