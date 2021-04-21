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

        public ProductNoteInventoryEntryController(
            IProductNoteInventoryEntryService service,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "ProductNote")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
        }

        public IActionResult Index(string id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var products = _service.FindByNote(id, null);

                return View(products);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
