using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Taxation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

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
                var comp = _companyService.FindById(companyId,GetLog(Model.OccorenceLog.Read));
                ViewBag.SocialName = comp.SocialName;
                ViewBag.Document = comp.Document;
                ViewBag.Id = companyId;
                List<Model.Client> list_clients = _service.FindByCompanyId(companyId);

                foreach (var client in list_clients)
                {
                    client.Name = client.Document + " - " + client.Name;
                }

                list_clients.Insert(0, new Model.Client() { Name = "Nennhum item selecionado", Id = 0 });
                SelectList clients = new SelectList(list_clients, "Id", "Name", null);
                ViewBag.ClientId = clients;
                var result = _service.FindByCompanyId(companyId).TakeLast(1000);

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
                    string CNPJ = det.ContainsKey("CNPJ") ? det["CNPJ"] : "escon";
                    string indIEDest = det.ContainsKey("indIEDest") ? det["indIEDest"] : "escon";
                    string IE = det.ContainsKey("IE") ? det["IE"] : "escon";   

                    if (indIEDest == "1" && (IE != "escon" || IE != ""))
                    {
                        CNPJ = CNPJ.Substring(0, 8);
                        var existCnpj = _service.FindByDocumentCompany(id, CNPJ);

                        var CNPJRaiz = CNPJ.Substring(0, 8);

                        if (existCnpj == null)
                        {
                            var client = new Model.Client
                            {
                                Name = det["xNome"],
                                CompanyId = id,
                                Document = CNPJ,
                                CnpjRaiz = CNPJRaiz,
                                Ie = IE ,
                                Taxpayer = true,
                                Created = DateTime.Now,
                                Updated = DateTime.Now

                            };
                            _service.Create(entity: client,GetLog(Model.OccorenceLog.Create));
                            cont++;
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

        [HttpPost]
        public IActionResult Filter(int id)
        {
            try
            {
                ViewBag.Id = id;
                var result = _service.FindById(id,GetLog(Model.OccorenceLog.Read));
                return View(result);
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
                ViewBag.Id = id;
                ViewBag.Count = count;
                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateActive([FromBody] Model.UpdateActive updateActive)
        {
            try
            {
                var entity = _service.FindById(updateActive.Id, GetLog(Model.OccorenceLog.Read));
                entity.Taxpayer = updateActive.Active;

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
