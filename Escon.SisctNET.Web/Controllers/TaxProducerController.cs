using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class TaxProducerController : ControllerBaseSisctNET
    {
        private readonly ITaxProducerService _service;
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly IClientService _clientService;
        private readonly IProviderService _providerService;

        public TaxProducerController(
            ITaxProducerService service,
            ICompanyService companyService,
            IConfigurationService configurationService,
            IClientService clientService,
            IProviderService providerService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Tax")
        {
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _clientService = clientService;
            _providerService = providerService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(int id, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Comp = comp;
                var rst = _service.FindByTaxs(id, month, year).OrderBy(_ => Convert.ToInt32(_.Nnf)).ToList();

                SessionManager.SetCompanyIdInSession(id);
                SessionManager.SetMonthInSession(month);
                SessionManager.SetYearInSession(year);

                return View(rst);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Import()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(SessionManager.GetCompanyIdInSession(), null);
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Import(string type)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                int companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var comp = _companyService.FindById(companyid, null);


                var NfeExit = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));
                var NfeEntry = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                var importDir = new Diretorio.Import();
                var importXml = new Xml.Import();

                string directoryNfeExit = "", arqui = "";
                string directoryNfeEntry = importDir.Entrada(comp,NfeEntry.Value,year,month);

                if (type.Equals("xmlE"))
                {
                    directoryNfeExit = importDir.SaidaEmpresa(comp, NfeExit.Value, year, month);
                    arqui = "XML EMPRESA";
                }
                else
                {
                    directoryNfeExit = importDir.SaidaSefaz(comp, NfeExit.Value, year, month);
                    arqui = "XML SEFAZ";
                }

                List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();        
                
                exitNotes = importXml.NFeAll(directoryNfeExit);
                entryNotes = importXml.NFeAll(directoryNfeEntry);

                var clientes = _clientService.FindByCompany(companyid);
                var providers = _providerService.FindByCompany(companyid);

                var clientesAll = clientes.Select(_ => _.Document).ToList();
                var providersAll = providers.Select(_ => _.Document).ToList();

                var clientesProdutor = clientes.Where(_ => _.TypeClientId.Equals(3)).Select(_ => _.Document).ToList();
                var providersProdutor = providers.Where(_ => _.TypeClientId.Equals(3)).Select(_ => _.Document).ToList();

                List<List<string>> notesProdutor = new List<List<string>>();

                for (int i = entryNotes.Count - 1; i >= 0; i--)
                {
                    if (!entryNotes[i][3]["CNPJ"].Equals(comp.Document) || entryNotes[i][1]["finNFe"] == "4")
                    {
                        entryNotes.RemoveAt(i);
                        continue;
                    }

                    if(!clientesAll.Contains(entryNotes[i][2]["CNPJ"]) && !providersAll.Contains(entryNotes[i][2]["CNPJ"]))
                    {
                        ViewBag.Erro = 1;
                        return View(comp);
                    }

                    if (exitNotes[i][2].ContainsKey("CPF"))
                    {
                        if (!clientesAll.Contains(exitNotes[i][2]["CPF"]) && !providersAll.Contains(exitNotes[i][2]["CPF"]))
                        {
                            ViewBag.Error = 2;
                            return View(comp);
                        }

                        if (clientesProdutor.Contains(exitNotes[i][2]["CPF"]) || providersProdutor.Contains(exitNotes[i][2]["CPF"]))
                        {
                            var clientTemp = clientes.Where(_ => _.Document.Equals(exitNotes[i][2]["CPF"])).FirstOrDefault();
                            var providerTemp = providers.Where(_ => _.Document.Equals(exitNotes[i][2]["CPF"])).FirstOrDefault();

                            decimal percentualIcms = 0, baseCalc = 0;

                            if (clientTemp != null)
                                percentualIcms = Convert.ToDecimal(clientTemp.Percentual);

                            if (providerTemp != null)
                                percentualIcms = Convert.ToDecimal(providerTemp.Percentual);

                            if (Convert.ToDecimal(exitNotes[i][exitNotes[i].Count() - 1]["vBC"]) == 0)
                                baseCalc = Convert.ToDecimal(exitNotes[i][exitNotes[i].Count() - 1]["vNF"]);
                            else
                                baseCalc = Convert.ToDecimal(exitNotes[i][exitNotes[i].Count() - 1]["vBC"]);

                            decimal icms = baseCalc * percentualIcms / 100;

                            List<string> nota = new List<string>();
                            nota.Add(exitNotes[i][0]["chave"]);
                            nota.Add(exitNotes[i][1]["nNF"]);
                            nota.Add(exitNotes[i][1]["dhEmi"]);
                            nota.Add(exitNotes[i][2]["CPF"]);
                            nota.Add(exitNotes[i][2]["xNome"]);
                            nota.Add(exitNotes[i][exitNotes[i].Count() - 1]["vNF"]);
                            nota.Add(exitNotes[i][exitNotes[i].Count() - 1]["vBC"]);
                            nota.Add(percentualIcms.ToString());
                            nota.Add(icms.ToString());
                            notesProdutor.Add(nota);
                        }
                    }

                    if (clientesProdutor.Contains(entryNotes[i][2]["CNPJ"]) || providersProdutor.Contains(entryNotes[i][2]["CNPJ"]))
                    {
                        var clientTemp = clientes.Where(_ => _.Document.Equals(entryNotes[i][2]["CNPJ"])).FirstOrDefault();
                        var providerTemp = providers.Where(_ => _.Document.Equals(entryNotes[i][2]["CNPJ"])).FirstOrDefault();

                        decimal percentualIcms = 0, baseCalc = 0;

                        if (clientTemp != null)
                            percentualIcms = Convert.ToDecimal(clientTemp.Percentual);

                        if (providerTemp != null)
                            percentualIcms = Convert.ToDecimal(providerTemp.Percentual);

                        if (Convert.ToDecimal(entryNotes[i][entryNotes[i].Count() - 1]["vBC"]) == 0)
                            baseCalc = Convert.ToDecimal(entryNotes[i][entryNotes[i].Count() - 1]["vNF"]);
                        else
                            baseCalc = Convert.ToDecimal(entryNotes[i][entryNotes[i].Count() - 1]["vBC"]);

                        decimal icms = baseCalc * percentualIcms / 100;

                        List<string> nota = new List<string>();
                        nota.Add(entryNotes[i][0]["chave"]);
                        nota.Add(entryNotes[i][1]["nNF"]);
                        nota.Add(entryNotes[i][1]["dhEmi"]);
                        nota.Add(entryNotes[i][2]["CNPJ"]);
                        nota.Add(entryNotes[i][2]["xNome"]);
                        nota.Add(entryNotes[i][entryNotes[i].Count()-1]["vNF"]);
                        nota.Add(entryNotes[i][entryNotes[i].Count() - 1]["vBC"]);
                        nota.Add(percentualIcms.ToString());
                        nota.Add(icms.ToString());                       
                        notesProdutor.Add(nota);
                    }

                }

                for (int i = exitNotes.Count - 1; i >= 0; i--)
                {

                    if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document) || exitNotes[i][1]["finNFe"] == "4" || exitNotes[i][1]["tpNF"] == "1")
                    {
                        exitNotes.RemoveAt(i);
                        continue;
                    }

                    if (exitNotes[i][3].ContainsKey("CPF"))
                    {
                        if (!clientesAll.Contains(exitNotes[i][3]["CPF"]) && !providersAll.Contains(exitNotes[i][3]["CPF"]))
                        {
                            ViewBag.Error = 2;
                            return View(comp);
                        }

                        if (clientesProdutor.Contains(exitNotes[i][3]["CPF"]) || providersProdutor.Contains(exitNotes[i][3]["CPF"]))
                        {
                            var clientTemp = clientes.Where(_ => _.Document.Equals(exitNotes[i][3]["CPF"])).FirstOrDefault();
                            var providerTemp = providers.Where(_ => _.Document.Equals(exitNotes[i][3]["CPF"])).FirstOrDefault();

                            decimal percentualIcms = 0, baseCalc = 0;

                            if (clientTemp != null)
                                percentualIcms = Convert.ToDecimal(clientTemp.Percentual);

                            if (providerTemp != null)
                                percentualIcms = Convert.ToDecimal(providerTemp.Percentual);

                            if (Convert.ToDecimal(exitNotes[i][exitNotes[i].Count() - 1]["vBC"]) == 0)
                                baseCalc = Convert.ToDecimal(exitNotes[i][exitNotes[i].Count() - 1]["vNF"]);
                            else
                                baseCalc = Convert.ToDecimal(exitNotes[i][exitNotes[i].Count() - 1]["vBC"]);

                            decimal icms = baseCalc * percentualIcms / 100;

                            List<string> nota = new List<string>();
                            nota.Add(exitNotes[i][0]["chave"]);
                            nota.Add(exitNotes[i][1]["nNF"]);
                            nota.Add(exitNotes[i][1]["dhEmi"]);
                            nota.Add(exitNotes[i][3]["CPF"]);
                            nota.Add(exitNotes[i][3]["xNome"]);
                            nota.Add(exitNotes[i][exitNotes[i].Count() - 1]["vNF"]);
                            nota.Add(exitNotes[i][exitNotes[i].Count() - 1]["vBC"]);
                            nota.Add(percentualIcms.ToString());
                            nota.Add(icms.ToString());
                            notesProdutor.Add(nota);
                        }
                    }

                    if (exitNotes[i][3].ContainsKey("CNPJ"))
                    {
                        if (!clientesAll.Contains(exitNotes[i][3]["CNPJ"]) && !providersAll.Contains(exitNotes[i][3]["CNPJ"]))
                        {
                            ViewBag.Error = 2;
                            return View(comp);
                        }

                        if (clientesProdutor.Contains(exitNotes[i][3]["CNPJ"]) || providersProdutor.Contains(exitNotes[i][3]["CNPJ"]))
                        {
                            var clientTemp = clientes.Where(_ => _.Document.Equals(exitNotes[i][3]["CNPJ"])).FirstOrDefault();
                            var providerTemp = providers.Where(_ => _.Document.Equals(exitNotes[i][3]["CNPJ"])).FirstOrDefault();

                            decimal percentualIcms = 0, baseCalc = 0;

                            if (clientTemp != null)
                                percentualIcms = Convert.ToDecimal(clientTemp.Percentual);

                            if (providerTemp != null)
                                percentualIcms = Convert.ToDecimal(providerTemp.Percentual);

                            if (Convert.ToDecimal(exitNotes[i][exitNotes[i].Count() - 1]["vBC"]) == 0)
                                baseCalc = Convert.ToDecimal(exitNotes[i][exitNotes[i].Count() - 1]["vNF"]);
                            else
                                baseCalc = Convert.ToDecimal(exitNotes[i][exitNotes[i].Count() - 1]["vBC"]);

                            decimal icms = baseCalc * percentualIcms / 100;

                            List<string> nota = new List<string>();
                            nota.Add(exitNotes[i][0]["chave"]);
                            nota.Add(exitNotes[i][1]["nNF"]);
                            nota.Add(exitNotes[i][1]["dhEmi"]);
                            nota.Add(exitNotes[i][3]["CNPJ"]);
                            nota.Add(exitNotes[i][3]["xNome"]);
                            nota.Add(exitNotes[i][exitNotes[i].Count() - 1]["vNF"]);
                            nota.Add(exitNotes[i][exitNotes[i].Count() - 1]["vBC"]);
                            nota.Add(percentualIcms.ToString());
                            nota.Add(icms.ToString());
                            notesProdutor.Add(nota);
                        }
                    }

                    
                }

                List<Model.TaxProducer> addProdutor = new List<Model.TaxProducer>();
                List<Model.TaxProducer> updateProdutor = new List<Model.TaxProducer>();

                var taxs = _service.FindByTaxs(companyid, month, year);

                foreach (var n in notesProdutor)
                {
                    var taxTemp = taxs.Where(_ => _.Chave.Equals(n[0])).FirstOrDefault();

                    if(taxTemp == null)
                    {
                        Model.TaxProducer npr = new Model.TaxProducer();
                        npr.Arquivo = arqui;
                        npr.CompanyId = companyid;
                        npr.AnoRef = year;
                        npr.MesRef = month;
                        npr.Chave = n[0];
                        npr.Nnf = n[1];
                        npr.Dhemi = Convert.ToDateTime(n[2]);
                        npr.Cnpj = n[3];
                        npr.Xnome = n[4];
                        npr.Vnf = Convert.ToDecimal(n[5]);
                        npr.Vbasecalc = Convert.ToDecimal(n[6]);
                        npr.Percentual = Convert.ToDecimal(n[7]);
                        npr.Icms = Convert.ToDecimal(n[8]);
                        npr.Created = DateTime.Now;
                        npr.Updated = npr.Created;
                        addProdutor.Add(npr);
                    }
                    else
                    {
                        taxTemp.Vnf = Convert.ToDecimal(n[5]);
                        taxTemp.Vbasecalc = Convert.ToDecimal(n[6]);
                        taxTemp.Percentual = Convert.ToDecimal(n[7]);
                        taxTemp.Icms = Convert.ToDecimal(n[8]);
                        taxTemp.Updated = DateTime.Now;
                        updateProdutor.Add(taxTemp);
                    }
                }

                _service.Create(addProdutor);
                _service.Update(updateProdutor);

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                return RedirectToAction("Index", new { id = companyid, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
