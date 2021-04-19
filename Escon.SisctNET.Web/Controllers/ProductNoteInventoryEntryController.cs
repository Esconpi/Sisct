using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class ProductNoteInventoryEntryController : ControllerBaseSisctNET
    {
        private readonly IProductNoteInventoryEntryService _service;
        private readonly INoteInventoryEntryService _noteInventoryEntryService;

        public ProductNoteInventoryEntryController(
            IProductNoteInventoryEntryService service,
            INoteInventoryEntryService noteInventoryEntryService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "ProductNote")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _noteInventoryEntryService = noteInventoryEntryService;
        }

        public IActionResult Index(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var note = _noteInventoryEntryService.FindByNote(id, null);

                return View(note);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
