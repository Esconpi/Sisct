using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class ProductNoteInventoryExitController : ControllerBaseSisctNET
    {
        private readonly IProductNoteInventoryExitService _service;
        private readonly INoteInventoryExitService _noteInventoryExitService;

        public ProductNoteInventoryExitController(
            IProductNoteInventoryExitService service,
            INoteInventoryExitService noteInventoryExitService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "ProductNote")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _noteInventoryExitService = noteInventoryExitService;
        }

        public IActionResult Index(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var note = _noteInventoryExitService.FindByNote(id, null);

                return View(note);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
