using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Taxation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Web.Controllers
{
    public class ClientController : ControllerBaseSisctNET
    {
        private readonly IClientService _service;
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;

        public ClientController(
            IClientService service,
            ICompanyService companyService,
            IConfigurationService  configurationService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Client")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
        }

        [HttpGet]
        public IActionResult Index(int companyId)
        {
            try
            {
                ViewBag.Id = companyId;
                var result = _service.FindByCompanyId(companyId);
                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Atualize(int id)
        {
            try
            {
                var result = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                return PartialView(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Atualize(int id,string year,string month)
        {
            try
            {
                int cont = 0;
                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));

                var confDBSisctNfe = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));

                string directoryNfe = confDBSisctNfe.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                var import = new Import();

                List<Dictionary<string, string>> dets = new List<Dictionary<string, string>>();

                dets = import.Client(directoryNfe);

                foreach (var det in dets)
                {
                    string indIEDest = "escon";
                    string CNPJ = "escon";

                    if (det.ContainsKey("CNPJ"))
                    {
                        CNPJ = det["CNPJ"];
                    }

                    if (det.ContainsKey("indIEDest"))
                    {
                        indIEDest = det["indIEDest"];
                    }

                    if(CNPJ != "escon")
                    {
                        if (indIEDest == "1")
                        {
                            var existCnpj = _service.FindByDocumentCompany(id, CNPJ);

                            if (existCnpj == null)
                            {
                                var client = new Model.Client
                                {
                                    Name = det["xNome"],
                                    CompanyId = id,
                                    Document = CNPJ,
                                    Taxpayer = true,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now

                                };
                                _service.Create(entity: client,GetLog(Model.OccorenceLog.Create));
                                cont++;
                            }
                        }
                    }
                    
                }

                return RedirectToAction("Details",new {id = id,count = cont });
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Details(int id,int count)
        {
            try
            {
                var result = _service.FindByCompanyId(id);
                ViewBag.Count = result.Count;
                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
