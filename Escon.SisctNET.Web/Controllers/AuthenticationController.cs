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
        private readonly IConfigurationService _configurationService;
        private readonly IIncentiveService _incentiveService;

        public AuthenticationController(
            IAuthentication service,
            IPersonService personService,
            IAccessService accessService,
            IConfigurationService configurationService,
            IIncentiveService incentiveService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) : base(functionalityService, "Login")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _personService = personService;
            _accessService = accessService;
            _configurationService = configurationService;
            _incentiveService = incentiveService;
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
                    if (!person.Active)
                    {
                        ViewBag.CodeErro = 1; //Usuário Inativo
                        return View("Index");
                    }

                    SessionManager.SetLoginInSession(new Model.Login() { AccessKey = auth.AccessToken, Email = auth.Email, Id = auth.Id});
                    SessionManager.SetUserIdInSession(auth.Id);
                    SessionManager.SetPersonInSession(person);

                    var configMin = _configurationService.FindByName("DiasAvisoMínimoIncentivo");
                    var configMax = _configurationService.FindByName("DiasAvisoMáximoIncentivo");
                    var accesses = _accessService.FindByProfileId(person.ProfileId);
                    var incentives = _incentiveService.FindByPeriod(Convert.ToInt32(configMin.Value));

                    SessionManager.SetAccessesInSession(accesses);
                    SessionManager.SetIncitiveInSession(incentives);
                    SessionManager.SetMin(Convert.ToInt32(configMin.Value));
                    SessionManager.SetMax(Convert.ToInt32(configMax.Value));

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