using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Escon.SisctNET.Web.Controllers
{
    public class NcmConvenioController : ControllerBaseSisctNET
    {
        private readonly INcmConvenioService _service;

        public NcmConvenioController(
            INcmConvenioService service,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "NcmConvenio")
        {
            _service = service;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index()
        {
            try
            {
                var result = _service.FindAll(GetLog(Model.OccorenceLog.Read));

                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
           
        }
    }
}
