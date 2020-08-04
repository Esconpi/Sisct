using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class AccessController : ControllerBaseSisctNET
    {
        private readonly IAccessService _service;
        private readonly IProfileService _profileService;

        public AccessController(
            IAccessService service,
            IFunctionalityService functionalityService,
            IProfileService profileService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Access")
        {

            _service = service;
            _profileService = profileService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(int profileId)
        {

            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Access")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var login = SessionManager.GetLoginInSession();

                if (login == null)
                {
                    return RedirectToAction("Index", "Authentication");
                }
                else
                {
                    var result = _service.FindByProfileId(profileId);
                    var profile = _profileService.FindById(profileId, GetLog(Model.OccorenceLog.Read));
                    ViewBag.Name = profile.Name;
                    return View(result);
                }
                
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpPost]
        public IActionResult UpdateStatus([FromBody] Model.UpdateActive updateActive)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Access")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var entity = _service.FindById(updateActive.Id, GetLog(Model.OccorenceLog.Read));
                entity.Active = updateActive.Active;

                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return Ok(new { requestcode = 200, message = "ok" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { requestcode = 500, message = ex.Message });
            }
        }


    }
}