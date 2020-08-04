﻿using Escon.SisctNET.Model;
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
        private readonly ITypeClientService _typeClientService;

        public ClientController(
            IClientService service,
            ICompanyService companyService,
            IConfigurationService  configurationService,
            ITypeClientService typeClientService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Client")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _typeClientService = typeClientService;
        }

        [HttpGet]
        public IActionResult Index(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var comp = _companyService.FindById(id,GetLog(Model.OccorenceLog.Read));
                ViewBag.SocialName = comp.SocialName;
                ViewBag.Document = comp.Document;
                ViewBag.Id = id;
                List<Model.Client> list_clients = _service.FindByCompanyId(id);

                foreach (var client in list_clients)
                {
                    client.Name = client.Document + " - " + client.Name;
                }

                list_clients.Insert(0, new Model.Client() { Name = "Nennhum item selecionado", Id = 0 });
                SelectList clients = new SelectList(list_clients, "Id", "Name", null);
                ViewBag.ClientId = clients;
                var result = _service.FindByCompanyId(id).TakeLast(1000);
                SessionManager.SetCompanyIdInSession(id);
                return View(null);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Atualize(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                int cont = 0;
                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));

                var confDBSisctNfeSaida = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));
                var confDBSisctNfEntrada = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                string directoryNfeSaida = confDBSisctNfeSaida.Value + "\\" + comp.Document + "\\" + year + "\\" + month;
                string directoryNfeEntrada = confDBSisctNfEntrada.Value + "\\" + comp.Document + "\\" + year + "\\" + month;
                var import = new Import();

                List<Dictionary<string, string>> dets = new List<Dictionary<string, string>>();

                dets = import.Client(directoryNfeSaida);

                int tipoCliente = 1;

                if(comp.AnnexId.Equals(3)){
                    tipoCliente = 2;
                }

                foreach (var det in dets)
                {
                    
                    if (det.ContainsKey("CNPJ"))
                    {
                        string CNPJ = det["CNPJ"];
                        string indIEDest = det.ContainsKey("indIEDest") ? det["indIEDest"] : "";
                        string IE = det.ContainsKey("IE") ? det["IE"] : "";
                        if (!CNPJ.Equals("escon") || !CNPJ.Equals(""))
                        {
                            var existCnpj = _service.FindByDocumentCompany(id, CNPJ);

                            if (existCnpj == null)
                            {
                                var CNPJRaiz = CNPJ.Substring(0, 8);
                                var client = new Model.Client
                                {
                                    Name = det["xNome"],
                                    CompanyId = id,
                                    Document = CNPJ,
                                    CnpjRaiz = CNPJRaiz,
                                    Ie = IE,
                                    TypeClientId = tipoCliente,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now

                                };
                                _service.Create(entity: client,GetLog(Model.OccorenceLog.Create));
                                cont++;
                            }
                        }
                    }
                  
                }

                return RedirectToAction("Details",new {companyId = id,count = cont });
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Filter(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = _service.FindById(id,GetLog(Model.OccorenceLog.Read));
                ViewBag.Id = result.CompanyId;
                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Details(int companyId,int count)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                ViewBag.Id = companyId;
                var client = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.CompanyId.Equals(companyId)).Reverse();
                var result = client.Take(count).ToList();
                ViewBag.Count = count;
                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                ViewBag.TypeClientId = new SelectList(_typeClientService.FindAll(GetLog(Model.OccorenceLog.Read)), "Id", "Name", null);
                var result = _service.FindById(id, null);
                ViewBag.Company = result.CompanyId;
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(int id, Model.Client entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var client = _service.FindById(id, null);
                client.TypeClientId = entity.TypeClientId;
                if (entity.TypeClientId.Equals(3))
                {
                    client.Diferido = entity.Diferido;
                    client.Percentual = entity.Percentual;
                }
                _service.Update(client, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index", new { id = client.CompanyId});
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Client(int id, int count)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                ViewBag.TypeClientId = new SelectList(_typeClientService.FindAll(GetLog(Model.OccorenceLog.Read)), "Id", "Name", null);
                var result = _service.FindById(id, null);
                ViewBag.Company = result.CompanyId;
                ViewBag.Count = count;
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Client(int id, int count, Model.Client entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var client = _service.FindById(id, null);
                client.TypeClientId = entity.TypeClientId;
                if (entity.TypeClientId.Equals(3))
                {
                    client.Diferido = entity.Diferido;
                    client.Percentual = entity.Percentual;
                }
                _service.Update(client, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Details", new { companyId = client.CompanyId, count = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
        
        public IActionResult Contribuinte(int id)
        {
            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.SocialName = comp.SocialName;
                ViewBag.Document = comp.Document;
                var clients = _service.FindAll(null).Where(_ => _.CompanyId.Equals(id) && _.TypeClientId.Equals(1)).ToList();
                return View(clients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult GetAll(int draw, int start)
        {

            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var clintesAll = _service.FindByCompanyId(SessionManager.GetCompanyIdInSession());


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Client> clientes = new List<Client>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Client> clientTemp = new List<Client>();
                clintesAll.ToList().ForEach(s =>
                {
                    s.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.Name);
                    s.Document = s.Document;
                    s.Ie = s.Ie;
                    s.TypeClientId = s.TypeClientId;
                    clientTemp.Add(s);
                });

                var ids = clientTemp.Where(c =>
                    c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Document.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Ie.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                clientes = clintesAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var cfop = from r in clientes
                           where ids.ToArray().Contains(r.Id)
                           select new
                           {
                               Id = r.Id.ToString(),
                               Name = r.Name,
                               Document = r.Document,
                               Ie = r.Ie,
                               TypeClient = r.TypeClientId

                           };

                return Ok(new { draw = draw, recordsTotal = clientes.Count(), recordsFiltered = clientes.Count(), data = cfop.Skip(start).Take(lenght) });

            }
            else
            {


                var cfop = from r in clintesAll
                           select new
                           {
                               Id = r.Id.ToString(),
                               Name = r.Name,
                               Document = r.Document,
                               Ie = r.Ie,
                               TypeClient = r.TypeClientId

                           };
                return Ok(new { draw = draw, recordsTotal = clintesAll.Count(), recordsFiltered = clintesAll.Count(), data = cfop.Skip(start).Take(lenght) });
            }

        }

    }
} 
