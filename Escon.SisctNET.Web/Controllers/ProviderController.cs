using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class ProviderController : ControllerBaseSisctNET
    {
        private readonly IProviderService _service;
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly ITypeClientService _typeClientService;
        private readonly IClientService _clientService;

        public ProviderController(
            IProviderService service,
            ICompanyService companyService,
            IConfigurationService configurationService,
            ITypeClientService typeClientService,
            IClientService clientService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Client")
        {
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _typeClientService = typeClientService;
            _clientService = clientService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult IndexAll(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                ViewBag.Comp = comp;
                SessionManager.SetCompanyIdInSession(id);
                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Atualize(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
                return Unauthorized();

            try
            {
                var result = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Atualize(int id, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));

                var confDBSisctNfe = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                var importXml = new Xml.Import();
                var importDir = new Diretorio.Import();

                string directoryNfe = importDir.Entrada(comp, confDBSisctNfe.Value, year, month);

                List<Dictionary<string, string>> dets = new List<Dictionary<string, string>>();

                dets = importXml.Provider(directoryNfe);

                int tipoCliente = 1;

                if (Convert.ToInt32(comp.AnnexId).Equals(3))
                {
                    tipoCliente = 2;
                }

                List<Model.Provider> addProviders = new List<Model.Provider>();

                var providersAll = _service.FindByCompany(id);
                var clientesAll = _clientService.FindByCompany(id);

                foreach (var det in dets)
                {
                    if (det.ContainsKey("CPF"))
                    {
                        string CPF = det["CPF"];
                        string IE = det.ContainsKey("IE") ? det["IE"] : "";

                        if (!CPF.Equals("escon") || !CPF.Equals(""))
                        {
                            var existProvider = providersAll.Where(_ => _.Document.Equals(CPF)).FirstOrDefault();
                            var existClient = clientesAll.Where(_ => _.Document.Equals(CPF)).FirstOrDefault();

                            if (existProvider == null && existClient == null)
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


                                Model.Provider provider = new Model.Provider();
                                provider.Name = det["xNome"];
                                provider.CompanyId = id;
                                provider.Document = CPF;
                                provider.CnpjRaiz = CPF;
                                provider.Ie = IE;
                                provider.TypeClientId = tipoCliente;
                                provider.MesRef = month;
                                provider.AnoRef = year;
                                provider.Created = DateTime.Now;
                                provider.Updated = DateTime.Now;

                                addProviders.Add(provider);
                            }
                        }
                    }

                    if (det.ContainsKey("CNPJ"))
                    {
                        string CNPJ = det["CNPJ"];
                        string IE = det.ContainsKey("IE") ? det["IE"] : "";

                        if (!CNPJ.Equals("escon") || !CNPJ.Equals(""))
                        {
                            var existProvider = providersAll.Where(_ => _.Document.Equals(CNPJ)).FirstOrDefault();
                            var existClient = clientesAll.Where(_ => _.Document.Equals(CNPJ)).FirstOrDefault();
                            

                            if (existProvider == null && existClient == null)
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

                                Model.Provider provider = new Model.Provider();
                                provider.Name = det["xNome"];
                                provider.CompanyId = id;
                                provider.Document = CNPJ;
                                provider.CnpjRaiz = CNPJRaiz;
                                provider.Ie = IE;
                                provider.TypeClientId = tipoCliente;
                                provider.MesRef = month;
                                provider.AnoRef = year;
                                provider.Created = DateTime.Now;
                                provider.Updated = DateTime.Now;

                                addProviders.Add(provider);
                            }
                        }
                    }

                }

                _service.Create(addProviders);

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                return RedirectToAction("Index", new { companyId = id, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Index(int companyId, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
                return Unauthorized();

            try
            {
                SessionManager.SetCompanyIdInSession(companyId);
                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);
                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
                return Unauthorized();

            try
            {
                ViewBag.TypeClientId = new SelectList(_typeClientService.FindAll(GetLog(Model.OccorenceLog.Read)), "Id", "Name", null);
                var result = _service.FindById(id, null);
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
                return RedirectToAction("IndexAll", new { id = client.CompanyId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Provider(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
                return Unauthorized();

            try
            {
                ViewBag.TypeClientId = new SelectList(_typeClientService.FindAll(GetLog(Model.OccorenceLog.Read)), "Id", "Name", null);
                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Provider(int id, Model.Client entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
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

        public IActionResult Contribuinte(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Client")).FirstOrDefault() == null)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Comp = comp;
                var clients = _service.FindByCompany(id).Where(_ => _.TypeClientId.Equals(1)).ToList();
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

            var providerssAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession());


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Model.Provider> providers = new List<Model.Provider>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Model.Provider> providerTemp = new List<Model.Provider>();
                providerssAll.ToList().ForEach(s =>
                {
                    s.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.Name);
                    s.Document = s.Document;
                    s.Ie = s.Ie;
                    s.TypeClientId = s.TypeClientId;
                    providerTemp.Add(s);
                });

                var ids = providerTemp.Where(c =>
                    c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Document.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Ie.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                providers = providerssAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var cfop = from r in providers
                           where ids.ToArray().Contains(r.Id)
                           select new
                           {
                               Id = r.Id.ToString(),
                               Name = r.Name,
                               Document = r.Document,
                               Ie = r.Ie,
                               TypeClient = r.TypeClientId

                           };

                return Ok(new { draw = draw, recordsTotal = providers.Count(), recordsFiltered = providers.Count(), data = cfop.Skip(start).Take(lenght) });

            }
            else
            {


                var cfop = from r in providerssAll
                           select new
                           {
                               Id = r.Id.ToString(),
                               Name = r.Name,
                               Document = r.Document,
                               Ie = r.Ie,
                               TypeClient = r.TypeClientId

                           };
                return Ok(new { draw = draw, recordsTotal = providerssAll.Count(), recordsFiltered = providerssAll.Count(), data = cfop.Skip(start).Take(lenght) });
            }

        }

        public IActionResult GetAll(int draw, int start)
        {

            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var providersAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession(), SessionManager.GetYearInSession(), SessionManager.GetMonthInSession());


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Model.Provider> providers = new List<Model.Provider>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Model.Provider> providerTemp = new List<Model.Provider>();
                providersAll.ToList().ForEach(s =>
                {
                    s.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.Name);
                    s.Document = s.Document;
                    s.Ie = s.Ie;
                    s.TypeClientId = s.TypeClientId;
                    providerTemp.Add(s);
                });

                var ids = providerTemp.Where(c =>
                    c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Document.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Ie.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                providers = providersAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var cfop = from r in providers
                           where ids.ToArray().Contains(r.Id)
                           select new
                           {
                               Id = r.Id.ToString(),
                               Name = r.Name,
                               Document = r.Document,
                               Ie = r.Ie,
                               TypeClient = r.TypeClientId

                           };

                return Ok(new { draw = draw, recordsTotal = providers.Count(), recordsFiltered = providers.Count(), data = cfop.Skip(start).Take(lenght) });

            }
            else
            {


                var cfop = from r in providersAll
                           select new
                           {
                               Id = r.Id.ToString(),
                               Name = r.Name,
                               Document = r.Document,
                               Ie = r.Ie,
                               TypeClient = r.TypeClientId

                           };
                return Ok(new { draw = draw, recordsTotal = providersAll.Count(), recordsFiltered = providersAll.Count(), data = cfop.Skip(start).Take(lenght) });
            }

        }
    }
}
