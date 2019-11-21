using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Escon.SisctNET.Web.Controllers
{
    public class StateController : ControllerBaseSisctNET
    {
        private readonly IStateService _service;

        public StateController(
            IStateService service,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "State")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
        }

        public IActionResult Index()
        {
            var result = _service.FindAll(GetLog(Model.OccorenceLog.Read));
            return View(result);
        }
    }
}
