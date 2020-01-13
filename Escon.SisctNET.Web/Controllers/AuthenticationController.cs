﻿using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

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
            return View();
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

                    var accesses = _accessService.FindByActive(person.ProfileId);
                    foreach (var access in accesses)
                    {
                        if (access.FunctionalityId.Equals(1))
                        {
                            SessionManager.SetProfileInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(2))
                        {
                            SessionManager.SetNoteInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(3))
                        {
                            SessionManager.SetProductInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(4))
                        {
                            SessionManager.SetAccessInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(5))
                        {
                            SessionManager.SetUserInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(6))
                        {
                            SessionManager.SetHomeInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(7))
                        {
                            SessionManager.SetNcmInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(8))
                        {
                            SessionManager.SetAttachmentInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(9))
                        {
                            SessionManager.SetGroupInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(10))
                        {
                            SessionManager.SetCestInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(11))
                        {
                            SessionManager.SetCompanyInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(12))
                        {
                            SessionManager.SetConfigurationInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(13))
                        {
                            SessionManager.SetCfopInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(14))
                        {
                            SessionManager.SetStateInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(18))
                        {
                            SessionManager.SetTaxationInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(16))
                        {
                            SessionManager.SetProductNoteInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(17))
                        {
                            SessionManager.SetCompanyCfopInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(19))
                        {
                            SessionManager.SetDarInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(20))
                        {
                            SessionManager.SetAnnexInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(21))
                        {
                            SessionManager.SetNcmConvenioInSession(access.FunctionalityId);
                        }

                        if (access.FunctionalityId.Equals(22))
                        {
                            SessionManager.SetHomeExitInSession(access.FunctionalityId);
                        }

                    }
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
                SessionManager.SetLogoutInSession();
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}