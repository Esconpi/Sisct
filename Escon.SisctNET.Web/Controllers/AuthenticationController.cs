using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Escon.SisctNET.Web.Controllers
{
    public class AuthenticationController : ControllerBaseSisctNET
    {
        private readonly IAuthentication _service;
        private readonly IPersonService _personService;
        private readonly IAccessService _accessService;

        public AuthenticationController(
            IAuthentication service,
            IPersonService personService,
            IAccessService accessService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) : base(functionalityService, "Login")
        {
            _service = service;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _personService = personService;
            _accessService = accessService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Index(IFormCollection login)
        {
            try
            {
                
                var auth = _service.FindByLogin(new Model.Login() { Email = login["Email"], Password = new Crypto.HashManager().GenerateHash(login["Password"]) });

                if (!auth.Authenticated)
                {
                    ViewBag.CodeErro = 2; //Usuário ou senha inválido
                    return View("Index");
                }

                var person = _personService.FindById(auth.Id, null);
                var Password = new Crypto.HashManager().GenerateHash(login["Password"]);
                if (person.Email.Equals(login["Email"]) && person.Password.Equals(Password))
                {

                    SessionManager.SetLoginInSession(new Model.Login() { AccessKey = auth.AccessToken, Email = auth.Email, Id = auth.Id});
                    SessionManager.SetUserIdInSession(auth.Id);
                    SessionManager.SetPersonInSession(person);

                    var accesses = _accessService.FindByProfileId(person.ProfileId);

                    SessionManager.SetAccessesInSession(accesses);

                    SessionManager._httpContextAccessor.HttpContext.Request.Headers.Add("Authorization", "bearer " + auth.AccessToken);
                    SessionManager._httpContextAccessor.HttpContext.Response.Headers.Add("Authorization", "bearer " + auth.AccessToken);
                  

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.CodeErro = 2; //Usuário ou senha inválido
                    return View("Index");
                }

                

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            try
            {
                //SessionManager.SetLogoutInSession();
                SessionManager.SetAccessesInSession(null);
                SessionManager.SetLoginInSession(null);
                SessionManager.SetUserIdInSession(0);
                SessionManager.SetPersonInSession(null);
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}