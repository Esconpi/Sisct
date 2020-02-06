using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Escon.SisctNET.Web.Controllers
{
    public class ClientController : ControllerBaseSisctNET
    {
        private readonly IClientService _service;

        public ClientController(
            IClientService service,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Client")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
        }

        [HttpGet]
        public IActionResult Index(int companyId)
        {
            try
            {
                var result = _service.FindByCompanyId(companyId);
                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
