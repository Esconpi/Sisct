using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Escon.SisctNET.Web.Controllers
{
    public class IcmsEntryController : ControllerBaseSisctNET
    {
        private readonly IHostingEnvironment _appEnvironment;

        public IcmsEntryController(
            IFunctionalityService functionalityService,
            IHostingEnvironment env,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Company")
        {
            _appEnvironment = env;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Relatory()
        {
            return View();
        }
    }
}
