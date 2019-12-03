using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Escon.SisctNET.Web.Controllers
{
    public class AnnexController : ControllerBaseSisctNET
    {

        private readonly IAnnexService _service;

        public AnnexController(
            IAnnexService service,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Annex")
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

            }catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
