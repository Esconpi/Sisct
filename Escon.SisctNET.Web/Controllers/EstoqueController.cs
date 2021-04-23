using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Escon.SisctNET.Web.Controllers
{
    public class EstoqueController : ControllerBaseSisctNET
    {
        private readonly ICompanyService _service;
        private readonly IProductNoteInventoryEntryService _productNoteInventoryEntryService;
        private readonly IProductNoteInventoryExitService _productNoteInventoryExitService;

        public EstoqueController(
            ICompanyService service,
            IProductNoteInventoryEntryService productNoteInventoryEntryService,
            IProductNoteInventoryExitService productNoteInventoryExitService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Company")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _productNoteInventoryEntryService = productNoteInventoryEntryService;
            _productNoteInventoryExitService = productNoteInventoryExitService;
        }

        public IActionResult Index(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _service.FindById(id, null);
                ViewBag.Company = comp;
                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }
    }
}
