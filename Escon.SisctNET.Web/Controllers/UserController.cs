using System;
using System.Linq;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Escon.SisctNET.Web.Controllers
{
    public class UserController : ControllerBaseSisctNET
    {
        private readonly IPersonService _service;
        private readonly IProfileService _profileService;

        public UserController(
            IPersonService service,
            IProfileService profileService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "User")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);

            _service = service;
            _profileService = profileService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("User")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindAll(GetLog(Model.OccorenceLog.Read));
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("User")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                ViewBag.ProfileId = new SelectList(_profileService.FindAll(GetLog(Model.OccorenceLog.Read)), "Id", "Name", null);
                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create(Model.Person entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("User")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {

                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;
                entity.Password = new Crypto.HashManager().GenerateHash(entity.Password);

                _service.Create(entity, GetLog(Model.OccorenceLog.Create));

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
      
        [HttpGet]
        public IActionResult Edit(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("User")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var person = _service.FindById(id, null);

                ViewBag.ProfileId = new SelectList(_profileService.FindAll(GetLog(Model.OccorenceLog.Read)), "Id", "Name", person.Profile);

                return View(person);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.Person person)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("User")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var _person = _service.FindById(id, null);
                person.Created = _person.Created;

                if (!string.IsNullOrEmpty(person.Password))
                {
                    person.Password = new Crypto.HashManager().GenerateHash(person.Password);
                }
                else
                {
                    person.Password = _person.Password;
                }

                person.Updated = DateTime.Now;
                _service.Update(person, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult EditNew(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var person = _service.FindById(id, null);
                return PartialView(person);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult EditNew(long id, Model.Person person)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var _person = _service.FindById(id, null);
                person.Created = _person.Created;

                if (!string.IsNullOrEmpty(person.Password))
                {
                    person.Password = new Crypto.HashManager().GenerateHash(person.Password);
                }
                else
                {
                    person.Password = _person.Password;
                }
                person.Updated = DateTime.Now;
                person.ProfileId = _person.ProfileId;
                person.Active = true;
                _service.Update(person, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Delete(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("User")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                _service.Delete(id, GetLog(Model.OccorenceLog.Delete));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateStatus([FromBody] Model.UpdateActive updateActive)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("User")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var entity = _service.FindById(updateActive.Id, null);
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
