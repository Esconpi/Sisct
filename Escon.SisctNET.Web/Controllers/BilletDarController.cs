using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{
    public class BilletDarController : ControllerBaseSisctNET
    {
        private readonly IDarService _darService;
        private readonly IDarDocumentService _darDocumentService;
        private readonly IEmailResponsibleService _emailResponsibleService;
        private readonly IEmailService _serviceEmail;
        private readonly IEmailConfiguration _emailConfiguration;

        public BilletDarController(
            IDarService darService,
            IDarDocumentService darDocumentService,
            IEmailResponsibleService emailResponsibleService,
            IFunctionalityService functionalityService,
            IEmailService serviceEmail,
            IEmailConfiguration emailConfiguration,
            IHttpContextAccessor httpContextAccessor) : base(functionalityService, "Group")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);

            _darDocumentService = darDocumentService;
            _emailResponsibleService = emailResponsibleService;
            _darService = darService;
            _serviceEmail = serviceEmail;
            _emailConfiguration = emailConfiguration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();
            var result = await _darDocumentService.ListFull();
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Index(FormCollection forms)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();
            var result = await _darDocumentService.ListFull();
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> SendBillet(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            var billet = _darDocumentService.FindById(id, GetLog(Model.OccorenceLog.Read));
            var responsibles = await _emailResponsibleService.GetByCompanyAsync(billet.CompanyId);
            var dar = _darService.FindAll(GetLog(Model.OccorenceLog.Read));
            var darBillet = dar.FirstOrDefault(x => x.Id.Equals(billet.DarId));

            //Enviar Email
            var subject = $"Boleto ESCONPI {darBillet.Description}";
            var body = $@"Boleto de {darBillet.Code} - {darBillet.Description} 
                                  referente ao período {string.Format("{0}-{1}", billet.PeriodReference.ToString().Substring(4, 2), billet.PeriodReference.ToString().Substring(0, 4))} com data de vencimento para {billet.DueDate.ToString("dd/MM/yyyy")}";

            var emailFrom = _emailConfiguration.SmtpUsername;

            List<EmailAddress> emailto = new List<EmailAddress>();
            foreach (var to in await _emailResponsibleService.GetByCompanyAsync(billet.CompanyId))
                emailto.Add(new EmailAddress() { Address = to.Email, Name = "" });

            EmailMessage email = new EmailMessage()
            {
                Content = body,
                FromAddresses = new List<EmailAddress>() { new EmailAddress() { Address = _emailConfiguration.SmtpUsername, Name = "Sistems SisCT - ESCONPI" } },
                Subject = subject,
                ToAddresses = emailto
            };

            var dirOutput = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot/Billets");
            var fileOutput = Path.Combine(dirOutput, billet.BilletPath);

            _serviceEmail.Send(email, new string[] { fileOutput });

            return Ok(new { code = 200, message = "ok" });

        }
    }
}