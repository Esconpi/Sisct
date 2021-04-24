using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

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

        public IActionResult Relatory(int id, string year, string inicio, string fim)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _service.FindById(id, null);
                ViewBag.Company = comp;

                var importPeriod = new Period.Month();

                var entradas = _productNoteInventoryEntryService.FindByNotes(id, year);
                var saidas = _productNoteInventoryExitService.FindByNotes(id, year);

                var meses = importPeriod.Months(inicio, fim);

                foreach(var mes in meses)
                {
                    for(int i = 1; i <= 31; i++)
                    {

                    }
                }

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }
    }
}
