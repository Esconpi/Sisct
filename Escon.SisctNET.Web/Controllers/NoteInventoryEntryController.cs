using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class NoteInventoryEntryController : ControllerBaseSisctNET
    {

        private readonly ICompanyService _companyService;

        public NoteInventoryEntryController(
            ICompanyService companyService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Note")
        {
            _companyService = companyService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(int id, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);

                ViewBag.Company = comp;

                SessionManager.SetCompanyIdInSession(id);
                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);

                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
