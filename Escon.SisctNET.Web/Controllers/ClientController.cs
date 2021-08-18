using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
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
        private readonly IProviderService _providerService;

        public ClientController(
            IClientService service,
            ICompanyService companyService,
            IConfigurationService  configurationService,
            ITypeClientService typeClientService,
            IProviderService providerService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Client")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _typeClientService = typeClientService;
            _providerService = providerService;
        }

        [HttpGet]
        public IActionResult IndexAll(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Company = comp;
                SessionManager.SetCompanyIdInSession(id);
                return View(null);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Atualize(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _companyService.FindById(id, null);
                return PartialView(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Atualize(long id,string year,string month, string archive) 
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);

                var confDBSisctNfe = _configurationService.FindByName("NFe Saida", null);

                var importXml = new Xml.Import();
                var importDir = new Diretorio.Import();

                string directoryNfe = "";

                if (archive.Equals("xmlE"))
                    directoryNfe = importDir.SaidaEmpresa(comp, confDBSisctNfe.Value, year, month);
                else
                    directoryNfe = importDir.SaidaSefaz(comp, confDBSisctNfe.Value, year, month);

                List<Dictionary<string, string>> dets = new List<Dictionary<string, string>>();

                dets = importXml.NFeClient(directoryNfe);

                long tipoCliente = 1;

                if(Convert.ToInt32(comp.AnnexId).Equals(3))
                    tipoCliente = 2;

                List<Model.Client> addClientes = new List<Model.Client>();

                var clientesAll = _service.FindByCompany(id);
                var providersAll = _providerService.FindByCompany(id);

                foreach (var det in dets)
                {
                    if (det.ContainsKey("CPF"))
                    {
                        string CPF = det["CPF"];
                        string IE = det.ContainsKey("IE") ? det["IE"] : "";

                        if (!CPF.Equals("escon") || !CPF.Equals(""))
                        {
                            var existClient = clientesAll.Where(_ => _.Document.Equals(CPF)).FirstOrDefault();
                            var existProvider = providersAll.Where(_ => _.Document.Equals(CPF)).FirstOrDefault();
                            

                            if (existClient == null && existProvider == null)
                            {
                                tipoCliente = 2;

                                Model.Client client = new Model.Client();
                                client.Name = det["xNome"];
                                client.CompanyId = id;
                                client.Document = CPF;
                                client.CnpjRaiz = CPF;
                                client.Ie = IE;
                                client.TypeClientId = tipoCliente;
                                client.MesRef = month;
                                client.AnoRef = year;
                                client.Created = DateTime.Now;
                                client.Updated = DateTime.Now;

                                addClientes.Add(client);
                            }
                        }
                    }

                    if (det.ContainsKey("CNPJ"))
                    {
                        string CNPJ = det["CNPJ"];
                        string IE = det.ContainsKey("IE") ? det["IE"] : "";

                        if (!CNPJ.Equals("escon") || !CNPJ.Equals(""))
                        {
                            var existClient = clientesAll.Where(_ => _.Document.Equals(CNPJ)).FirstOrDefault();
                            var existProvider = providersAll.Where(_ => _.Document.Equals(CNPJ)).FirstOrDefault();

                            if (existClient == null && existProvider == null)
                            {
                                tipoCliente = 1;

                                if (Convert.ToInt32(comp.AnnexId).Equals(3))
                                {
                                    tipoCliente = 2;
                                }

                                if (IE.Equals(""))
                                {
                                    tipoCliente = 2;
                                }

                                var CNPJRaiz = CNPJ.Substring(0, 8);

                                Model.Client client = new Model.Client();
                                client.Name = det["xNome"];
                                client.CompanyId = id;
                                client.Document = CNPJ;
                                client.CnpjRaiz = CNPJRaiz;
                                client.Ie = IE;
                                client.TypeClientId = tipoCliente;
                                client.MesRef = month;
                                client.AnoRef = year;
                                client.Created = DateTime.Now;
                                client.Updated = DateTime.Now;

                                addClientes.Add(client);
                            }
                        }
                    }

                }

                _service.Create(addClientes, GetLog(OccorenceLog.Create));

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                return RedirectToAction("Index",new {companyId = id, year = year, month = month });
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Index(long companyId, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                SessionManager.SetCompanyIdInSession(companyId);
                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);
                var comp = _companyService.FindById(companyId, null);
                ViewBag.Company = comp;
                return View(null);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                ViewBag.TypeClientId = new SelectList(_typeClientService.FindAll(null), "Id", "Name", null);
                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.Client entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var client = _service.FindById(id, null);
                client.TypeClientId = entity.TypeClientId;
                if (entity.TypeClientId.Equals(3))
                {
                    client.Diferido = entity.Diferido;
                    client.Percentual = entity.Percentual;
                }
                client.Updated = DateTime.Now;
                _service.Update(client, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("IndexAll", new { id = client.CompanyId});
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Client(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                ViewBag.TypeClientId = new SelectList(_typeClientService.FindAll(null), "Id", "Name", null);
                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Client(long id, Model.Client entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var client = _service.FindById(id, null);
                client.TypeClientId = entity.TypeClientId;
                if (entity.TypeClientId.Equals(3))
                {
                    client.Diferido = entity.Diferido;
                    client.Percentual = entity.Percentual;
                }
                client.Updated = DateTime.Now;
                _service.Update(client, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index", new { companyId = client.CompanyId, year = client.AnoRef, month = client.MesRef });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
        
        public IActionResult Contribuinte(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Company = comp;
                var clients = _service.FindByCompany(id).Where(_ => _.TypeClientId.Equals((long)1)).ToList();
                return View(clients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult NContribuinte(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Company = comp;
                var clients = _service.FindByCompany(id).Where(_ => _.TypeClientId.Equals((long)2)).OrderBy(_ => _.Ie).ToList();
                return View(clients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult ClienteIE(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Company = comp;
                var clients = _service.FindByCompany(id).Where(_ => _.Ie != null && _.Ie != "").ToList();
                return View(clients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult ContribuinteMonth(long id, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Company = comp;
                var clients = _service.FindByCompany(id).Where(_ => _.MesRef.Equals(month) && _.AnoRef.Equals(year) && _.TypeClientId.Equals((long)1)).ToList();
                return View(clients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult NContribuinteMonth(long id, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Company = comp;
                var clients = _service.FindByCompany(id).Where(_ => _.MesRef.Equals(month) && _.AnoRef.Equals(year) && _.TypeClientId.Equals((long)2)).OrderBy(_ => _.Ie).ToList();
                return View(clients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult ClienteIEMonth(long id, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Company = comp;
                var clients = _service.FindByCompany(id).Where(_ => _.MesRef.Equals(month) && _.AnoRef.Equals(year) && _.Ie != null && _.Ie != "").ToList();
                return View(clients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult GetAllCompany(int draw, int start)
        {

            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var clintesAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession()).OrderBy(_ => _.Ie).ToList();


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Client> clientes = new List<Client>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Client> clientTemp = new List<Client>();
                clintesAll.ToList().ForEach(s =>
                {
                    s.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.Name);
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

        public IActionResult GetAll(int draw, int start)
        {

            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var clintesAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession(),SessionManager.GetYearInSession(),SessionManager.GetMonthInSession()).OrderBy(_ => _.Ie).ToList();

            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Client> clientes = new List<Client>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Client> clientTemp = new List<Client>();
                clintesAll.ToList().ForEach(s =>
                {
                    s.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.Name);
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
