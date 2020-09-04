using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{
    public class BilletDarController : ControllerBaseSisctNET
    {
        private readonly ICompanyService _companyService;
        private readonly IDarService _darService;
        private readonly IDarDocumentService _darDocumentService;
        private readonly IEmailResponsibleService _emailResponsibleService;
        private readonly IEmailService _serviceEmail;
        private readonly IEmailConfiguration _emailConfiguration;

        public BilletDarController(
            ICompanyService companyService,
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
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();
            FillPeriodReference(null);
            await FillDar();
            await FillCompany(null);

            var result = await _darDocumentService.ListFull();

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormCollection forms)
        {

            try
            {
                if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

                bool? canceled = null, paidout = null;
                int? darid = null, period = null, companyid = null;

                var sit = forms["Situation"].ToString();
                var pa = forms["PaidOut"].ToString();
                var dar = forms["DarId"].ToString();
                var pe = forms["PeriodId"].ToString();
                var company = forms["CompanyId"].ToString();

                switch (sit)
                {
                    case "0":
                        canceled = true;
                        break;
                    case "1":
                        canceled = false;
                        break;
                    case "2":
                        canceled = null;
                        break;
                    default:
                        canceled = null;
                        break;
                }

                switch (pa)
                {
                    case "0":
                        paidout = false;
                        break;
                    case "1":
                        paidout = true;
                        break;
                    case "2":
                        paidout = null;
                        break;
                    default:
                        canceled = null;
                        break;
                }

                if (!dar.Equals("0"))
                    darid = Convert.ToInt32(dar);

                if (!pe.Equals("0"))
                    period = Convert.ToInt32(pe);

                if (!company.Equals("0"))
                    companyid = Convert.ToInt32(company);

                FillPeriodReference(null);
                await FillDar();
                await FillCompany(company);

                var result = await _darDocumentService.SearchAsync(canceled, paidout, period, darid, companyid);

                return View("Index", result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { code = 500, message = "Houve uma falha na consulta do documentos. " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Watcher()
        {
            var currentPeriod = Convert.ToInt32(DateTime.Now.Year.ToString() + "" + DateTime.Now.Month.ToString("00"));

            ViewBag.Year = DateTime.Now.Year.ToString();
            ViewBag.Month = GetStringMonth(DateTime.Now.Month);

            FillPeriodReference(null);
            await FillCompany(null);

            var result = await _darDocumentService.FindByPeriodReferenceAsync(currentPeriod, null);

            ViewBag.Period = DateTime.Now.Month.ToString("00") + "/" + DateTime.Now.Year.ToString();
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Watcher(IFormCollection forms)
        {
            var pe = forms["PeriodId"].ToString();
            var company = forms["CompanyId"].ToString();

            int? companyid = null;

            if (!company.Equals("0"))
                companyid = Convert.ToInt32(company);

            FillPeriodReference(pe);
            await FillCompany(company);

            var result = await _darDocumentService.FindByPeriodReferenceAsync(Convert.ToInt32(pe), companyid);

            ViewBag.Period = pe.Substring(4, 2) + "/" + pe.Substring(0, 4);
            ViewBag.PeriodSelected = pe;
            ViewBag.CompanySelected = company;

            ViewBag.Year = pe.Substring(0, 4);
            ViewBag.Month = GetStringMonth(Convert.ToInt32(pe.Substring(4, 2)));

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

        private async Task FillDar()
        {
            List<Object> source = new List<object>();

            List<Model.Dar> list_dar = await _darService.FindAllAsync(GetLog(Model.OccorenceLog.Read));

            source.Add(new { Text = "Todos", Value = "0" });
            list_dar.ForEach(x => source.Add(new { Text = string.Format("[{0}] - {1}", x.Code, x.Description), Value = x.Id }));

            SelectList dar = new SelectList(source, "Value", "Text", null);
            ViewBag.DarId = dar;
        }

        private void FillPeriodReference(string periodSelected, bool ignoreAll = false)
        {
            List<object> itens = new List<object>();

            if (ignoreAll)
                itens.Add(new SelectListItem("Todos", "0"));

            for (int i = 2020; i < 2030; i++)
                for (int z = 1; z <= 12; z++)
                    itens.Add(new { Text = string.Format("{0}/{1}", z.ToString("00"), i.ToString()), Value = string.Format("{0}{1}", i.ToString("0000"), z.ToString("00")) });

            SelectList selectPeriods = new SelectList(itens, "Value", "Text", periodSelected);

            ViewBag.PeriodId = selectPeriods;
        }

        private async Task FillCompany(string companyId)
        {
            List<Object> source = new List<object>();
            List<Model.Company> list_dar = await _companyService.ListAllActiveAsync(GetLog(Model.OccorenceLog.Read));

            source.Add(new { Text = "Todos", Value = "0" });
            list_dar.ForEach(x => source.Add(new { Text = string.Format("[{0}] - [{1}] - {2}", x.Code, x.Document, x.SocialName), Value = x.Id }));

            SelectList dar = new SelectList(source, "Value", "Text", companyId);
            ViewBag.CompanyId = dar;
        }

        private string GetStringMonth(int month)
        {
            switch (month)
            {
                case 1:
                    return "Janeiro";
                case 2:
                    return "Fevereiro";
                case 3:
                    return "Março";
                case 4:
                    return "Abril";
                case 5:
                    return "Maio";
                case 6:
                    return "Junho";
                case 7:
                    return "Julho";
                case 8:
                    return "Agosto";
                case 9:
                    return "Setembro";
                case 10:
                    return "Outubro";
                case 11:
                    return "Novembro";
                case 12:
                    return "Dezembro";
                default:
                    return "None";
            }
        }
    }
}