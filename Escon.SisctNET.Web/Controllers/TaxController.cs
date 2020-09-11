﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{
    public class TaxController : ControllerBaseSisctNET
    {
        private readonly ITaxService _service;
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly ICompanyCfopService _companyCfopService;
        private readonly ITaxationNcmService _taxationNcmService;
        private readonly INcmConvenioService _ncmConvenioService;
        private readonly IClientService _clientService;
        private readonly ISuspensionService _suspensionService;
        private readonly IGrupoService _grupoService;
        private readonly IProductNoteService _itemService;
        private readonly IProductIncentivoService _productIncentivoService;
        private readonly IHostingEnvironment _appEnvironment;

        public TaxController(
            ITaxService service,
            ICompanyService companyService,
            IConfigurationService configurationService,
            ICompanyCfopService companyCfopService,
            ITaxationNcmService taxationNcmService,
            INcmConvenioService ncmConvenioService,
            IClientService clientService,
            ISuspensionService suspensionService,
            IGrupoService grupoService,
            IProductNoteService itemService,
            IProductIncentivoService productIncentivoService,
            IHostingEnvironment env,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Tax")
        {
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _companyCfopService = companyCfopService;
            _taxationNcmService = taxationNcmService;
            _ncmConvenioService = ncmConvenioService;
            _clientService = clientService;
            _suspensionService = suspensionService;
            _grupoService = grupoService;
            _itemService = itemService;
            _productIncentivoService = productIncentivoService;
            _appEnvironment = env;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(int id,string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var comp = _companyService.FindById(id, null);

                ViewBag.PercentualPetroleo = Convert.ToDecimal(comp.IRPJ1).ToString().Replace(".", ",");
                ViewBag.PercentualComercio = Convert.ToDecimal(comp.IRPJ2).ToString().Replace(".", ",");
                ViewBag.PercentualTransporte = Convert.ToDecimal(comp.IRPJ3).ToString().Replace(".", ",");
                ViewBag.PercentualServico = Convert.ToDecimal(comp.IRPJ4).ToString().Replace(".", ",");

                ViewBag.Id = comp.Id;
                ViewBag.Year = year;
                ViewBag.Month = month;
                ViewBag.SocialName = comp.SocialName;
                ViewBag.Document = comp.Document;
                ViewBag.Comp = comp;

            
                var result = _service.FindByMonth(id, month, year);

                decimal vendasGrupo = 0, devolucaoGrupo = 0;

                List<Grupo> grupos = new List<Grupo>();

                if(result != null)
                {
                    grupos = _grupoService.FindByGrupos(result.Id);

                    vendasGrupo = Convert.ToDecimal(grupos.Sum(_ => _.Vendas));
                    devolucaoGrupo = Convert.ToDecimal(grupos.Sum(_ => _.Devolucao));
                }

                ViewBag.Grupos = grupos;
                ViewBag.VendasGrupo = vendasGrupo;
                ViewBag.DevolucoesGrupo = devolucaoGrupo;
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Receita(int companyid, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var comp = _companyService.FindById(companyid, null);
                ViewBag.Id = comp.Id;
                ViewBag.Year = year;
                ViewBag.Month = month;
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Receita(int companyid, string year, string month, decimal receitaAF, decimal capitalIM, decimal outrasReceitas, decimal bonificacao)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var imp = _service.FindByMonth(companyid, month, year);

                Model.Tax tax = new Model.Tax();

                if (imp != null)
                {
                    imp.Bonificacao = bonificacao;
                    imp.CapitalIM = capitalIM;
                    imp.ReceitaAF = receitaAF;
                    imp.OutrasReceitas = outrasReceitas;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, null);
                }
                else
                {
                    tax.Bonificacao = bonificacao;
                    tax.CompanyId = companyid;
                    tax.MesRef = month;
                    tax.AnoRef = year;
                    tax.CapitalIM = capitalIM;
                    tax.ReceitaAF = receitaAF;
                    tax.OutrasReceitas = outrasReceitas;
                    tax.Created = DateTime.Now;
                    tax.Updated = tax.Created;

                    _service.Create(tax, null);
                }

                return RedirectToAction("Index", new { id = companyid, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Retention(int companyid, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var comp = _companyService.FindById(companyid, null);
                ViewBag.Id = comp.Id;
                ViewBag.Year = year;
                ViewBag.Month = month;
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Retention(int companyid, string year, string month, decimal pis, decimal cofins, decimal csll, decimal csllFonte,
            decimal irpj, decimal irpjFonteServico, decimal irpjFonteFinanceira)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var imp = _service.FindByMonth(companyid, month, year);

                Model.Tax tax = new Model.Tax();

                if (imp != null)
                {
                    imp.PisRetido = pis;
                    imp.CofinsRetido = cofins;
                    imp.CsllRetido = csll;
                    imp.CsllFonte = csllFonte;
                    imp.IrpjRetido = irpj;
                    imp.IrpjFonteServico = irpjFonteServico;
                    imp.IrpjFonteFinanceira = irpjFonteFinanceira;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, null);
                }
                else
                {
                    tax.PisRetido = pis;
                    tax.CofinsRetido = cofins;
                    tax.CompanyId = companyid;
                    tax.MesRef = month;
                    tax.AnoRef = year;
                    tax.CsllRetido = csll;
                    tax.CsllFonte = csllFonte;
                    tax.IrpjRetido = irpj;
                    tax.IrpjFonteServico = irpjFonteServico;
                    tax.IrpjFonteFinanceira = irpjFonteFinanceira;
                    tax.Created = DateTime.Now;
                    tax.Updated = tax.Created;

                    _service.Create(tax, null);
                }

                return RedirectToAction("Index", new { id = companyid, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Reduction(int companyid, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var comp = _companyService.FindById(companyid, null);
                ViewBag.Id = comp.Id;
                ViewBag.Year = year;
                ViewBag.Month = month;
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Reduction(int companyid, string year, string month, decimal reducaoIcms)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var imp = _service.FindByMonth(companyid, month, year);

                Model.Tax tax = new Model.Tax();

                if (imp != null)
                {
                    imp.ReducaoIcms = reducaoIcms;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, null);
                }
                else
                {
                    tax.ReducaoIcms = reducaoIcms;
                    tax.Created = DateTime.Now;
                    tax.Updated = tax.Created;

                    _service.Create(tax, null);
                }

                return RedirectToAction("Index", new { id = companyid, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Import(int companyid, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var comp = _companyService.FindById(companyid, null);
                ViewBag.Id = comp.Id;
                ViewBag.Year = year;
                ViewBag.Month = month;
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Import(int companyid, string year, string month, string imposto,string type, List<IFormFile> arquivo)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var comp = _companyService.FindById(companyid, null);

                var NfeExit = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));
                var NfeEntry = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                string directoryNfeExit = NfeExit.Value + "\\" + comp.Document + "\\" + year + "\\" + month;
                string directoryNfeEntry = NfeEntry.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                List<string> caminhos = new List<string>();
                 
                var importXml = new Xml.Import(_companyCfopService, _taxationNcmService);
                var importSped = new Sped.Import(_companyCfopService, _taxationNcmService);

                Model.Tax tax = new Model.Tax();

                List<Model.Grupo> addGrupos = new List<Grupo>();

                var imp = _service.FindByMonth(companyid,month,year);

                if(imp != null)
                {
                    imp.Updated = DateTime.Now;
                }
                else
                {
                    tax.CompanyId = companyid;
                    tax.MesRef = month;
                    tax.AnoRef = year;
                    tax.Created = DateTime.Now;
                    tax.Updated = tax.Created;
                }

                if (type.Equals("sped"))
                {
                    string caminho_WebRoot = _appEnvironment.WebRootPath;
                    string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Speds\\";

                    int indice = 1;

                    foreach (var a in arquivo)
                    {

                        string nomeArquivo = comp.Document + indice.ToString() + year + month + ".txt";

                        string caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                        string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivo);

                        if (System.IO.File.Exists(caminhoDestinoArquivoOriginal))
                        {
                            System.IO.File.Delete(caminhoDestinoArquivoOriginal);
                        }

                        var stream = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);

                        await a.CopyToAsync(stream);

                        stream.Close();

                        caminhos.Add(caminhoDestinoArquivoOriginal);

                        indice++;
                    }
                }

                if (imposto.Equals("icms"))
                {
                    if (type.Equals("sped"))
                    {
                        decimal credito = 0;

                        foreach (var cc in caminhos)
                        {
                            credito = importSped.SpedCredito(cc, comp.Id);
                        }

                        if (imp != null)
                        {
                            imp.CreditoEntrada = credito;
                        }
                        else
                        {
                            tax.CreditoEntrada = credito;
                        }
                    }
                    else if (type.Equals("xml"))
                    {
                        List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                        List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();

                        if (comp.TipoApuracao.Equals(true))
                        {
                            exitNotes = importXml.Nfe(directoryNfeExit);
                            entryNotes = importXml.Nfe(directoryNfeEntry);

                            var ncms = _ncmConvenioService.FindByAnnex(Convert.ToInt32(comp.AnnexId));
                            var clientesAll = _clientService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid)).Select(_ => _.Document).ToList();
                            var suspensions = _suspensionService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid)).ToList();


                            var contribuintes = _clientService.FindByContribuinte(companyid, "all");
                            var contribuintesRaiz = _clientService.FindByContribuinte(companyid, "raiz");

                            var cfopsVenda = _companyCfopService.FindByCfopActive(companyid, "venda", "venda").Select(_ => _.Cfop.Code);
                            var cfopsTransf = _companyCfopService.FindByCfopActive(companyid, "venda", "transferencia").Select(_ => _.Cfop.Code);
                            var cfopsDevo = _companyCfopService.FindByCfopActive(companyid, "venda", "devo").Select(_ => _.Cfop.Code);

                            decimal totalVendas = 0, totalNcm = 0, totalTranferencias = 0, totalSaida = 0, totalDevo = 0,
                                totalDevoAnexo = 0, totalDevoContribuinte = 0, totalVendasSuspensao = 0, totalTranferenciaInter = 0;
                            int contContribuintes = contribuintes.Count();
                            int contContribuintesRaiz = contribuintesRaiz.Count() + 1;

                            string[,] resumoCnpjs = new string[contContribuintes, 2];
                            string[,] resumoCnpjRaiz = new string[contContribuintesRaiz, 2];
                            string[,] resumoAllCnpjRaiz = new string[contContribuintesRaiz - 1, 3];

                            for (int i = 0; i < contContribuintes; i++)
                            {
                                resumoCnpjs[i, 0] = contribuintes[i];
                                resumoCnpjs[i, 1] = "0";
                            }

                            for (int i = 0; i < contContribuintesRaiz; i++)
                            {
                                if (i < contContribuintesRaiz - 1)
                                {
                                    resumoCnpjRaiz[i, 0] = contribuintesRaiz[i];
                                    resumoAllCnpjRaiz[i, 0] = contribuintesRaiz[i];
                                    resumoCnpjRaiz[i, 1] = "0";
                                    resumoAllCnpjRaiz[i, 1] = "0";
                                    resumoAllCnpjRaiz[i, 2] = "0";
                                }
                                else
                                {
                                    resumoCnpjRaiz[i, 0] = "Não contribuinte";
                                    resumoCnpjRaiz[i, 1] = "0";
                                }
                            }


                            // Transferência Entrada
                            for (int i = entryNotes.Count - 1; i >= 0; i--)
                            {
                                if (!entryNotes[i][3]["CNPJ"].Equals(comp.Document))
                                {
                                    entryNotes.RemoveAt(i);
                                    continue;
                                }

                                bool cfop = false;

                                for (int j = 0; j < entryNotes[i].Count; j++)
                                {
                                    if (entryNotes[i][j].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfopsTransf.Contains(entryNotes[i][j]["CFOP"]))
                                        {
                                            cfop = true;
                                        }

                                    }
                                    if (cfop == true)
                                    {
                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vProd"))
                                        {
                                            if (entryNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            }
                                            totalTranferencias += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                        }
                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vFrete"))
                                        {
                                            if (entryNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            }
                                            totalTranferencias += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                        }
                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vDesc"))
                                        {
                                            if (entryNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            }
                                            totalTranferencias -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                        }
                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vOutro"))
                                        {
                                            if (entryNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            }
                                            totalTranferencias += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                        }
                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vSeg"))
                                        {
                                            if (entryNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            }
                                            totalTranferencias += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                        }
                                    }
                                }

                            }

                            // Vendas 
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                                {
                                    exitNotes.RemoveAt(i);
                                    continue;
                                }

                                int posClienteRaiz = contContribuintesRaiz - 1, posCliente = -1;

                                bool status = false, suspenso = false, cfop = false;

                                if (exitNotes[i][1].ContainsKey("dhEmi"))
                                {
                                    foreach (var suspension in suspensions)
                                    {
                                        if (Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) >= Convert.ToDateTime(suspension.DateStart) && Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) < Convert.ToDateTime(suspension.DateEnd))
                                        {
                                            suspenso = true;
                                            break;
                                        }
                                    }
                                }

                                if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("IE") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][1]["mod"].Equals("55"))
                                {
                                    string CNPJ = exitNotes[i][3].ContainsKey("CNPJ") ? exitNotes[i][3]["CNPJ"] : "escon";
                                    string indIEDest = exitNotes[i][3].ContainsKey("indIEDest") ? exitNotes[i][3]["indIEDest"] : "escon";
                                    string IE = exitNotes[i][3].ContainsKey("IE") ? exitNotes[i][3]["IE"] : "escon";

                                    if (contribuintesRaiz.Contains(exitNotes[i][3]["CNPJ"].Substring(0, 8)))
                                    {
                                        posClienteRaiz = contribuintesRaiz.IndexOf(exitNotes[i][3]["CNPJ"].Substring(0, 8));
                                    }

                                    if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                    {
                                        posCliente = contribuintes.IndexOf(exitNotes[i][3]["CNPJ"]);
                                    }

                                    bool existe = false;

                                    if (clientesAll.Contains(exitNotes[i][3]["CNPJ"]))
                                    {
                                        existe = true;
                                    }

                                    if (existe == false)
                                    {
                                        throw new Exception("Há Clientes não Importados");
                                    }
                                }

                                for (int k = 0; k < exitNotes[i].Count(); k++)
                                {

                                    if (exitNotes[i][k].ContainsKey("NCM"))
                                    {
                                        status = false;

                                        for (int j = 0; j < ncms.Count(); j++)
                                        {
                                            int tamanho = ncms[j].Length;

                                            if (ncms[j].Equals(exitNotes[i][k]["NCM"].Substring(0, tamanho)))
                                            {
                                                status = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (exitNotes[i][k].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfopsVenda.Contains(exitNotes[i][k]["CFOP"]))
                                        {
                                            cfop = true;
                                        }

                                    }

                                    if (status == true && cfop == true)
                                    {

                                        if (exitNotes[i][k].ContainsKey("vProd") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                            totalNcm += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                            }

                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vFrete") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                            totalNcm += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                            }
                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vDesc") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                            totalNcm -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                            }
                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vOutro") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                            totalNcm += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                            }
                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vSeg") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                            totalNcm += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();

                                            }
                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                            }
                                        }
                                    }
                                    else if (status == false && cfop == true)
                                    {

                                        if (exitNotes[i][k].ContainsKey("vProd") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                            }
                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vFrete") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                            }
                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vDesc") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                            }
                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vOutro") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                            }
                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vSeg") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();

                                            }
                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                            }
                                        }
                                    }

                                }

                            }

                            // Devolução Saida
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                if (exitNotes[i][3].ContainsKey("CNPJ"))
                                {
                                    if (!exitNotes[i][3]["CNPJ"].Equals(comp.Document))
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }
                                }
                                else
                                {
                                    exitNotes.RemoveAt(i);
                                    continue;
                                }

                                if (exitNotes[i][1]["finNFe"] != "4")
                                {
                                    exitNotes.RemoveAt(i);
                                    continue;
                                }

                                bool existe = false, status = false, cfop = false;

                                if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("IE") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][1]["mod"].Equals("55"))
                                {

                                    if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                    {
                                        existe = true;
                                    }
                                }

                                for (int j = 0; j < exitNotes[i].Count(); j++)
                                {
                                    if (exitNotes[i][j].ContainsKey("NCM"))
                                    {
                                        status = false;

                                        for (int k = 0; k < ncms.Count(); k++)
                                        {
                                            int tamanho = ncms[k].Length;

                                            if (ncms[k].Equals(exitNotes[i][j]["NCM"].Substring(0, tamanho)))
                                            {
                                                status = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (exitNotes[i][j].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfopsDevo.Contains(exitNotes[i][j]["CFOP"]))
                                        {
                                            cfop = true;
                                        }

                                    }

                                    if (status == true && cfop == true)
                                    {
                                        if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            totalDevoAnexo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            }
                                        }
                                    }
                                    else if (status == false && cfop == true)
                                    {
                                        if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            }

                                        }

                                        if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                        {

                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            }
                                        }
                                    }

                                }

                            }

                            // Transferência Saida
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {

                                if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                                {
                                    exitNotes.RemoveAt(i);
                                    continue;
                                }

                                bool suspenso = false;

                                if (exitNotes[i][1].ContainsKey("dhEmi"))
                                {
                                    foreach (var suspension in suspensions)
                                    {
                                        if (Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) >= Convert.ToDateTime(suspension.DateStart) && Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) < Convert.ToDateTime(suspension.DateEnd))
                                        {
                                            suspenso = true;
                                            break;
                                        }
                                    }
                                }

                                int posClienteRaiz = contContribuintesRaiz - 1;

                                if (exitNotes[i][3].ContainsKey("CNPJ"))
                                {
                                    if (contribuintesRaiz.Contains(exitNotes[i][3]["CNPJ"].Substring(0, 8)))
                                    {
                                        posClienteRaiz = contribuintesRaiz.IndexOf(exitNotes[i][3]["CNPJ"].Substring(0, 8));
                                    }
                                }

                                bool cfop = false;
                                for (int j = 0; j < exitNotes[i].Count; j++)
                                {
                                    if (exitNotes[i][j].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfopsTransf.Contains(exitNotes[i][j]["CFOP"]))
                                        {
                                            cfop = true;
                                        }

                                    }

                                    if (cfop == true)
                                    {
                                        if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                            }
                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            }

                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();

                                            }
                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            }

                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferencias -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();

                                            }
                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            }

                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();

                                            }
                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            }
                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();

                                            }
                                            if (suspenso == true)
                                            {
                                                totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            }
                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            }
                                        }
                                    }

                                }

                            }

                            // Devolução Entrada
                            for (int i = entryNotes.Count - 1; i >= 0; i--)
                            {
                                if (entryNotes[i][3].ContainsKey("CNPJ"))
                                {
                                    if (!entryNotes[i][3]["CNPJ"].Equals(comp.Document))
                                    {
                                        entryNotes.RemoveAt(i);
                                        continue;
                                    }
                                }
                                else
                                {
                                    entryNotes.RemoveAt(i);
                                    continue;
                                }

                                if (entryNotes[i][1]["finNFe"] != "4")
                                {
                                    entryNotes.RemoveAt(i);
                                    continue;
                                }

                                bool existe = false;
                                int posClienteRaiz = contContribuintesRaiz - 1;

                                if (entryNotes[i][3].ContainsKey("CNPJ") && entryNotes[i][3].ContainsKey("IE") && entryNotes[i][3].ContainsKey("indIEDest") && entryNotes[i][1]["mod"].Equals("55"))
                                {

                                    if (contribuintes.Contains(entryNotes[i][3]["CNPJ"]))
                                    {
                                        existe = true;
                                    }

                                    string CNPJ = entryNotes[i][3].ContainsKey("CNPJ") ? entryNotes[i][3]["CNPJ"] : "escon";
                                    string indIEDest = entryNotes[i][3].ContainsKey("indIEDest") ? entryNotes[i][3]["indIEDest"] : "escon";
                                    string IE = entryNotes[i][3].ContainsKey("IE") ? entryNotes[i][3]["IE"] : "escon";

                                    if (contribuintesRaiz.Contains(entryNotes[i][3]["CNPJ"].Substring(0, 8)))
                                    {
                                        posClienteRaiz = contribuintesRaiz.IndexOf(entryNotes[i][3]["CNPJ"].Substring(0, 8));
                                    }
                                }

                                bool status = false, cfop = false;

                                for (int j = 0; j < entryNotes[i].Count(); j++)
                                {

                                    if (entryNotes[i][j].ContainsKey("NCM"))
                                    {
                                        status = false;

                                        for (int k = 0; k < ncms.Count(); k++)
                                        {
                                            int tamanho = ncms[k].Length;

                                            if (ncms[k].Equals(entryNotes[i][j]["NCM"].Substring(0, tamanho)))
                                            {
                                                status = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (entryNotes[i][j].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfopsDevo.Contains(entryNotes[i][j]["CFOP"]))
                                        {
                                            cfop = true;
                                        }

                                    }

                                    if (status == true && cfop == true)
                                    {
                                        if (entryNotes[i][j].ContainsKey("vProd") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            }
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("vFrete") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            }
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("vDesc") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            totalDevoAnexo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            }
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("vOutro") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            }

                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("vSeg") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            }
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();
                                            }
                                        }
                                    }
                                    else if (status == false && cfop == true)
                                    {
                                        if (entryNotes[i][j].ContainsKey("vProd") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            }

                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                            }

                                        }

                                        if (entryNotes[i][j].ContainsKey("vFrete") && entryNotes[i][j].ContainsKey("cProd"))
                                        {

                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            }

                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();
                                            }

                                        }

                                        if (entryNotes[i][j].ContainsKey("vDesc") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            }

                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("vOutro") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            }
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("vSeg") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            }

                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();
                                            }
                                        }
                                    }
                                }

                            }

                            decimal totalNcontribuinte = Convert.ToDecimal(resumoCnpjRaiz[contContribuintesRaiz - 1, 1]), baseCalc = totalVendas - totalDevo;
                            decimal totalContribuinte = totalVendas - totalNcontribuinte;
                            decimal baseCalcContribuinte = totalContribuinte - totalDevoContribuinte;
                            decimal totalDevoNContribuinte = totalDevo - totalDevoContribuinte;
                            decimal baseCalcNContribuinte = totalNcontribuinte - totalDevoNContribuinte;

                            totalSaida = baseCalc + totalTranferencias;
                            totalVendas = totalVendas + totalTranferencias;

                            decimal limiteGrupo = (totalSaida * Convert.ToDecimal(comp.VendaMGrupo)) / 100;

                            if (imp != null)
                            {
                                imp.Vendas = totalVendas;
                                imp.VendasNContribuinte = totalNcontribuinte;
                                imp.VendasNcm = totalNcm;
                                imp.TransferenciaInter = totalTranferenciaInter;
                                imp.Transferencia = totalTranferencias;
                                imp.Devolucao = totalDevo;
                                imp.DevolucaoNContribuinte = totalDevoNContribuinte;
                                imp.DevolucaoNcm = totalDevoAnexo;
                                imp.Suspensao = totalVendasSuspensao;

                                var grupoTemp = _grupoService.FindByGrupos(imp.Id);

                                if (grupoTemp != null)
                                {
                                    List<Model.Grupo> updateGrupos = new List<Grupo>();

                                    for (int i = 0; i < contContribuintesRaiz - 1; i++)
                                    {
                                        var vendaGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 1]);
                                        var devoGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 2]);
                                        var baseCalcGrupo = (vendaGrupo - devoGrupo);
                                        if (baseCalcGrupo > limiteGrupo)
                                        {
                                            List<string> grupoExcedente = new List<string>();
                                            var cnpjGrupo = resumoAllCnpjRaiz[i, 0];
                                            var clientGrupo = _clientService.FindByRaiz(cnpjGrupo);
                                            var nomeGrupo = clientGrupo.Name;
                                            var percentGrupo = Math.Round((baseCalcGrupo / totalSaida) * 100, 2);



                                            var gg = grupoTemp.Where(_ => _.Cnpj.Equals(cnpjGrupo)).FirstOrDefault();

                                            if (gg != null)
                                            {
                                                gg.Cnpj = cnpjGrupo;
                                                gg.Nome = nomeGrupo;
                                                gg.Vendas = vendaGrupo;
                                                gg.Devolucao = devoGrupo;
                                                gg.BaseCalculo = baseCalcGrupo;
                                                gg.Percentual = percentGrupo;
                                                gg.TaxId = imp.Id;
                                                gg.Updated = DateTime.Now;

                                                updateGrupos.Add(gg);
                                            }
                                            else
                                            {
                                                Model.Grupo grupo = new Model.Grupo();
                                                grupo.Cnpj = cnpjGrupo;
                                                grupo.Nome = nomeGrupo;
                                                grupo.Vendas = vendaGrupo;
                                                grupo.Devolucao = devoGrupo;
                                                grupo.BaseCalculo = baseCalcGrupo;
                                                grupo.Percentual = percentGrupo;
                                                grupo.TaxId = imp.Id;
                                                grupo.Created = DateTime.Now;
                                                grupo.Updated = grupo.Created;

                                                addGrupos.Add(grupo);
                                            }


                                        }
                                    }

                                    _grupoService.Create(addGrupos, null);
                                    _grupoService.Update(updateGrupos, null);
                                }
                                else
                                {
                                    for (int i = 0; i < contContribuintesRaiz - 1; i++)
                                    {
                                        var vendaGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 1]);
                                        var devoGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 2]);
                                        var baseCalcGrupo = (vendaGrupo - devoGrupo);
                                        if (baseCalcGrupo > limiteGrupo)
                                        {
                                            List<string> grupoExcedente = new List<string>();
                                            var cnpjGrupo = resumoAllCnpjRaiz[i, 0];
                                            var clientGrupo = _clientService.FindByRaiz(cnpjGrupo);
                                            var nomeGrupo = clientGrupo.Name;
                                            var percentGrupo = Math.Round((baseCalcGrupo / totalSaida) * 100, 2);

                                            Model.Grupo grupo = new Model.Grupo();
                                            grupo.Cnpj = cnpjGrupo;
                                            grupo.Nome = nomeGrupo;
                                            grupo.Vendas = vendaGrupo;
                                            grupo.Devolucao = devoGrupo;
                                            grupo.BaseCalculo = baseCalcGrupo;
                                            grupo.Percentual = percentGrupo;
                                            grupo.TaxId = imp.Id;
                                            grupo.Created = DateTime.Now;
                                            grupo.Updated = grupo.Created;

                                            addGrupos.Add(grupo);

                                        }
                                    }

                                }
                            }
                            else
                            {
                                tax.Vendas = totalVendas;
                                tax.VendasNContribuinte = totalNcontribuinte;
                                tax.VendasNcm = totalNcm;
                                tax.TransferenciaInter = totalTranferenciaInter;
                                tax.Transferencia = totalTranferencias;
                                tax.Devolucao = totalDevo;
                                tax.DevolucaoNContribuinte = totalDevoNContribuinte;
                                tax.DevolucaoNcm = totalDevoAnexo;
                                tax.Suspensao = totalVendasSuspensao;

                                for (int i = 0; i < contContribuintesRaiz - 1; i++)
                                {
                                    var vendaGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 1]);
                                    var devoGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 2]);
                                    var baseCalcGrupo = (vendaGrupo - devoGrupo);
                                    if (baseCalcGrupo > limiteGrupo)
                                    {
                                        List<string> grupoExcedente = new List<string>();
                                        var cnpjGrupo = resumoAllCnpjRaiz[i, 0];
                                        var clientGrupo = _clientService.FindByRaiz(cnpjGrupo);
                                        var nomeGrupo = clientGrupo.Name;
                                        var percentGrupo = Math.Round((baseCalcGrupo / totalSaida) * 100, 2);

                                        Model.Grupo grupo = new Model.Grupo();
                                        grupo.Cnpj = cnpjGrupo;
                                        grupo.Nome = nomeGrupo;
                                        grupo.Vendas = vendaGrupo;
                                        grupo.Devolucao = devoGrupo;
                                        grupo.BaseCalculo = baseCalcGrupo;
                                        grupo.Percentual = percentGrupo;
                                        grupo.Created = DateTime.Now;
                                        grupo.Updated = grupo.Created;

                                        addGrupos.Add(grupo);

                                    }
                                }
                            }

                        }
                        else
                        {
                            if (!comp.AnnexId.Equals(3))
                            {
                                if (comp.TypeCompany.Equals(true))
                                {
                                    var productincentivo = _productIncentivoService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.CompanyId.Equals(companyid)).ToList();

                                    var codeProdIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Code).ToList();
                                    var codeProdST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Code).ToList();
                                    var codeProdIsento = productincentivo.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Code).ToList();

                                    var cestIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Cest).ToList();
                                    var cestST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Cest).ToList();
                                    var cestIsento = productincentivo.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Cest).ToList();

                                    List<List<string>> icmsForaDoEstado = new List<List<string>>();

                                    var contribuintes = _clientService.FindByContribuinte(companyid, "all");
                                    var clientesAll = _clientService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid)).Select(_ => _.Document).ToList();

                                    exitNotes = importXml.Nfe(directoryNfeExit);


                                    var cfopsDevoCompra = _companyCfopService.FindByCfopActive(companyid, "incentivo", "devolucao de compra").Select(_ => _.Cfop.Code);
                                    var cfopsVendaST = _companyCfopService.FindByCfopActive(companyid, "incentivo", "vendaSt").Select(_ => _.Cfop.Code);
                                    var cfopsVenda = _companyCfopService.FindByCfopActive(companyid, "incentivo", "venda").Select(_ => _.Cfop.Code);
                                    var cfospDevoVenda = _companyCfopService.FindByCfopActive(companyid, "entrada", "devolução de venda").Select(_ => _.Cfop.Code);

                                    decimal totalVendas = 0, naoContribuinteIncentivo = 0, naoContribuinteNIncetivo = 0, vendaCfopSTNaoContribuinteNIncetivo = 0, NaoContribuiteIsento = 0,
                                        naoContriForaDoEstadoIncentivo = 0, naoContriForaDoEstadoNIncentivo = 0, vendaCfopSTNaoContriForaDoEstadoNIncentivo = 0, NaoContribuinteForaDoEstadoIsento = 0,
                                        ContribuintesNIncentivo = 0, ContribuintesIncentivoAliqM25 = 0, ContribuintesIncentivo = 0, vendaCfopSTContribuintesNIncentivo = 0,
                                        ContribuinteIsento = 0, creditosIcms = 0, debitosIcms = 0;

                                    // Vendas
                                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                                    {
                                        if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                                        {
                                            exitNotes.RemoveAt(i);
                                            continue;
                                        }

                                        int posCliente = -1;

                                        if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][3].ContainsKey("IE"))
                                        {
                                            string CNPJ = exitNotes[i][3]["CNPJ"];
                                            string indIEDest = exitNotes[i][3]["indIEDest"];
                                            string IE = exitNotes[i][3]["IE"];
                                            if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                            {
                                                posCliente = contribuintes.IndexOf(exitNotes[i][3]["CNPJ"]);
                                            }

                                            bool existe = false;
                                            if (clientesAll.Contains(exitNotes[i][3]["CNPJ"]))
                                            {
                                                existe = true;
                                            }

                                            if (existe == false)
                                            {
                                                throw new Exception("Há Clientes não Importados");
                                            }
                                        }

                                        int posUf = -1;
                                        if (exitNotes[i][3].ContainsKey("UF") && exitNotes[i][1]["idDest"].Equals("2"))
                                        {

                                            for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                            {
                                                if (icmsForaDoEstado[j][0].Equals(exitNotes[i][3]["UF"]))
                                                {
                                                    posUf = j;
                                                }
                                            }

                                            if (posUf < 0)
                                            {
                                                List<string> uf = new List<string>();
                                                uf.Add(exitNotes[i][3]["UF"]);
                                                uf.Add("0,00");
                                                uf.Add("0,00");
                                                icmsForaDoEstado.Add(uf);
                                            }

                                        }

                                        bool cfop = false;
                                        decimal valorProduto = 0;
                                        string cProd = null, cest = null;

                                        for (int k = 0; k < exitNotes[i].Count(); k++)
                                        {
                                            if (exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                cProd = exitNotes[i][k]["cProd"];
                                                cest = "";
                                                if (exitNotes[i][k].ContainsKey("CEST"))
                                                {
                                                    cest = exitNotes[i][k]["CEST"];
                                                }

                                                if (exitNotes[i][k].ContainsKey("CFOP"))
                                                {
                                                    cfop = false;
                                                    if (cfopsVenda.Contains(exitNotes[i][k]["CFOP"]))
                                                    {
                                                        cfop = true;
                                                    }

                                                }

                                                if (cfop == true)
                                                {
                                                    if (exitNotes[i][k].ContainsKey("vProd"))
                                                    {
                                                        valorProduto += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vFrete"))
                                                    {
                                                        valorProduto += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vDesc"))
                                                    {
                                                        valorProduto -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vOutro"))
                                                    {
                                                        valorProduto += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vSeg"))
                                                    {
                                                        valorProduto += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                    }
                                                }

                                            }

                                            if (exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                            {
                                                decimal aliquota = 0;
                                                if (exitNotes[i][k].ContainsKey("pICMS"))
                                                {
                                                    aliquota = Convert.ToDecimal(exitNotes[i][k]["pICMS"]);
                                                    debitosIcms += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                                }

                                                if (codeProdIncentivado.Contains(cProd) && cestIncentivado.Contains(cest))
                                                {

                                                    if (posCliente < 0)
                                                    {
                                                        naoContribuinteIncentivo += Convert.ToDecimal(valorProduto);

                                                        if (exitNotes[i][1]["idDest"].Equals("2"))
                                                        {
                                                            naoContriForaDoEstadoIncentivo += Convert.ToDecimal(valorProduto);
                                                            icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(valorProduto)).ToString();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (aliquota <= 25)
                                                        {
                                                            ContribuintesIncentivo += Convert.ToDecimal(valorProduto);
                                                        }
                                                        else
                                                        {
                                                            ContribuintesIncentivoAliqM25 += Convert.ToDecimal(valorProduto);
                                                        }

                                                    }
                                                    totalVendas += Convert.ToDecimal(valorProduto);

                                                }
                                                else if (codeProdST.Contains(cProd) && cestST.Contains(cest))
                                                {
                                                    if (posCliente < 0)
                                                    {
                                                        naoContribuinteNIncetivo += Convert.ToDecimal(valorProduto);

                                                        if (exitNotes[i][1]["idDest"].Equals("2"))
                                                        {
                                                            naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(valorProduto);
                                                            icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(valorProduto)).ToString();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ContribuintesNIncentivo += Convert.ToDecimal(valorProduto);
                                                    }
                                                    totalVendas += Convert.ToDecimal(valorProduto);

                                                }
                                                else if (codeProdIsento.Contains(cProd) && cestIsento.Contains(cest))
                                                {
                                                    if (posCliente < 0)
                                                    {
                                                        NaoContribuiteIsento += Convert.ToDecimal(valorProduto);

                                                        if (exitNotes[i][1]["idDest"].Equals("2"))
                                                        {
                                                            NaoContribuinteForaDoEstadoIsento += Convert.ToDecimal(valorProduto);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ContribuinteIsento += Convert.ToDecimal(valorProduto);
                                                    }
                                                    totalVendas += Convert.ToDecimal(valorProduto);
                                                }
                                                else
                                                {
                                                    throw new Exception("Há Produtos não Tributado");
                                                }
                                                cest = null;
                                                cProd = null;
                                                valorProduto = 0;
                                                aliquota = 0;
                                            }

                                            if (exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                            {
                                                debitosIcms += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                            }
                                        }

                                    }

                                    // Vendas ST
                                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                                    {
                                        int posCliente = -1;

                                        if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][3].ContainsKey("IE"))
                                        {
                                            string CNPJ = exitNotes[i][3]["CNPJ"];
                                            string indIEDest = exitNotes[i][3]["indIEDest"];
                                            string IE = exitNotes[i][3]["IE"];
                                            if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                            {
                                                posCliente = contribuintes.IndexOf(exitNotes[i][3]["CNPJ"]);
                                            }

                                            bool existe = false;
                                            if (clientesAll.Contains(exitNotes[i][3]["CNPJ"]))
                                            {
                                                existe = true;
                                            }

                                            if (existe == false)
                                            {
                                                throw new Exception("Há Clientes não Importados");
                                            }
                                        }

                                        int posUf = -1;
                                        if (exitNotes[i][3].ContainsKey("UF") && exitNotes[i][1]["idDest"].Equals("2"))
                                        {

                                            for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                            {
                                                if (icmsForaDoEstado[j][0].Equals(exitNotes[i][3]["UF"]))
                                                {
                                                    posUf = j;
                                                }
                                            }

                                            if (posUf < 0)
                                            {
                                                List<string> uf = new List<string>();
                                                uf.Add(exitNotes[i][3]["UF"]);
                                                uf.Add("0");
                                                uf.Add("0");
                                                icmsForaDoEstado.Add(uf);
                                            }

                                        }

                                        bool cfop = false;

                                        for (int k = 0; k < exitNotes[i].Count(); k++)
                                        {
                                            string cest = "";
                                            if (exitNotes[i][k].ContainsKey("CEST"))
                                            {
                                                cest = exitNotes[i][k]["CEST"];
                                            }

                                            if (exitNotes[i][k].ContainsKey("CFOP"))
                                            {
                                                cfop = false;
                                                if (cfopsVendaST.Contains(exitNotes[i][k]["CFOP"]))
                                                {
                                                    cfop = true;
                                                }

                                            }

                                            if (cfop == true)
                                            {
                                                if (exitNotes[i][k].ContainsKey("cProd"))
                                                {
                                                    if (exitNotes[i][k].ContainsKey("vProd"))
                                                    {
                                                        if (posCliente < 0)
                                                        {
                                                            vendaCfopSTNaoContribuinteNIncetivo += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                                            {
                                                                vendaCfopSTNaoContriForaDoEstadoNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                                icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();

                                                            }
                                                        }
                                                        else
                                                        {
                                                            vendaCfopSTContribuintesNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                        }
                                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vProd"]);

                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vFrete"))
                                                    {
                                                        if (posCliente < 0)
                                                        {
                                                            vendaCfopSTNaoContribuinteNIncetivo += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                                            {
                                                                vendaCfopSTNaoContriForaDoEstadoNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                                icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();

                                                            }
                                                        }
                                                        else
                                                        {
                                                            vendaCfopSTContribuintesNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                        }

                                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vDesc"))
                                                    {
                                                        if (posCliente < 0)
                                                        {
                                                            vendaCfopSTNaoContribuinteNIncetivo -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                                            {
                                                                vendaCfopSTNaoContriForaDoEstadoNIncentivo -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                                icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();

                                                            }
                                                        }
                                                        else
                                                        {
                                                            vendaCfopSTContribuintesNIncentivo -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                        }

                                                        totalVendas -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vOutro"))
                                                    {
                                                        if (posCliente < 0)
                                                        {
                                                            vendaCfopSTNaoContribuinteNIncetivo += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                                            {
                                                                vendaCfopSTNaoContriForaDoEstadoNIncentivo += Convert.ToDecimal(exitNotes[i][k]["Outro"]);
                                                                icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();

                                                            }
                                                        }
                                                        else
                                                        {
                                                            vendaCfopSTContribuintesNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                        }

                                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vSeg"))
                                                    {
                                                        if (posCliente < 0)
                                                        {
                                                            vendaCfopSTNaoContribuinteNIncetivo += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                                            {
                                                                vendaCfopSTNaoContriForaDoEstadoNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                                icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();

                                                            }
                                                        }
                                                        else
                                                        {
                                                            vendaCfopSTContribuintesNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                        }

                                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                    }
                                                }



                                            }

                                        }
                                    }

                                    // Devolução de Compra
                                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                                    {
                                        if (exitNotes[i][1]["finNFe"] != "4")
                                        {
                                            exitNotes.RemoveAt(i);
                                            continue;
                                        }

                                        bool cfop = false;
                                        for (int k = 0; k < exitNotes[i].Count; k++)
                                        {
                                            if (exitNotes[i][k].ContainsKey("CFOP"))
                                            {
                                                cfop = false;
                                                if (cfopsDevoCompra.Contains(exitNotes[i][k]["CFOP"]))
                                                {
                                                    cfop = true;
                                                }

                                            }

                                            if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                            {
                                                debitosIcms += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                            }
                                        }
                                    }

                                    // Devolução de Venda
                                    /*for (int i = exitNotes.Count - 1; i >= 0; i--)
                                    {
                                        if (exitNotes[i][3].ContainsKey("CNPJ"))
                                        {
                                            if (!exitNotes[i][3]["CNPJ"].Equals(comp.Document))
                                            {
                                                exitNotes.RemoveAt(i);
                                                continue;
                                            }
                                        }

                                        bool cfop = false;

                                        for (int k = 0; k < exitNotes[i].Count(); k++)
                                        {
                                            if (exitNotes[i][k].ContainsKey("CFOP"))
                                            {
                                                cfop = false;
                                                if (cfospDevoVenda.Contains(exitNotes[i][k]["CFOP"]))
                                                {
                                                    cfop = true;
                                                }
                                            }
                                            if (exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                            {
                                                creditosIcms += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                            }
                                        }
                                    }*/


                                    if (imp != null)
                                    {
                                        imp.Debito = debitosIcms;
                                        imp.CreditoSaida = creditosIcms;
                                        imp.VendasContribuinte1 = ContribuintesIncentivo;
                                        imp.VendasContribuinte2 = ContribuintesIncentivoAliqM25;
                                        imp.ReceitaNormal1 = ContribuintesNIncentivo;
                                        imp.ReceitaST1 = vendaCfopSTContribuintesNIncentivo;
                                        imp.VendasNContribuinte = naoContribuinteIncentivo;
                                        imp.ReceitaNormal2 = naoContribuinteNIncetivo;
                                        imp.ReceitaST2 = vendaCfopSTNaoContribuinteNIncetivo;
                                        imp.VendasNContribuinteFora = naoContriForaDoEstadoIncentivo;
                                        imp.ReceitaNormal3 = naoContriForaDoEstadoNIncentivo;
                                        imp.ReceitaST3 = vendaCfopSTNaoContriForaDoEstadoNIncentivo;

                                        var grupoTemp = _grupoService.FindByGrupos(imp.Id);

                                        if (grupoTemp != null)
                                        {
                                            List<Model.Grupo> updateGrupos = new List<Grupo>();

                                            for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                            {

                                                decimal icms = (Convert.ToDecimal(comp.IcmsNContribuinteFora) * Convert.ToDecimal(icmsForaDoEstado[j][1])) / 100;
                                                if (icms > 0)
                                                {
                                                    var gg = grupoTemp.Where(_ => _.Uf.Equals(icmsForaDoEstado[j][0])).FirstOrDefault();

                                                    if (gg != null)
                                                    {
                                                        gg.Icms = icms;
                                                        gg.Updated = DateTime.Now;
                                                        updateGrupos.Add(gg);
                                                    }
                                                    else
                                                    {
                                                        Model.Grupo grupo = new Model.Grupo();
                                                        grupo.Uf = icmsForaDoEstado[j][0];
                                                        grupo.Icms = icms;
                                                        grupo.TaxId = imp.Id;
                                                        grupo.Created = DateTime.Now;
                                                        grupo.Updated = grupo.Created;
                                                        addGrupos.Add(grupo);
                                                    }

                                                }
                                            }

                                            _grupoService.Create(addGrupos, null);
                                            _grupoService.Update(updateGrupos, null);
                                        }
                                        else
                                        {
                                            for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                            {

                                                decimal icms = (Convert.ToDecimal(comp.IcmsNContribuinteFora) * Convert.ToDecimal(icmsForaDoEstado[j][1])) / 100;
                                                if (icms > 0)
                                                {
                                                    Model.Grupo grupo = new Model.Grupo();
                                                    grupo.Uf = icmsForaDoEstado[j][0];
                                                    grupo.Icms = icms;
                                                    grupo.TaxId = imp.Id;
                                                    grupo.Created = DateTime.Now;
                                                    grupo.Updated = grupo.Created;
                                                    addGrupos.Add(grupo);
                                                }
                                            }

                                        }

                                    }
                                    else
                                    {
                                        tax.Debito = debitosIcms;
                                        tax.CreditoSaida = creditosIcms;
                                        tax.VendasContribuinte1 = ContribuintesIncentivo;
                                        tax.VendasContribuinte2 = ContribuintesIncentivoAliqM25;
                                        tax.ReceitaNormal1 = ContribuintesNIncentivo;
                                        tax.ReceitaST1 = vendaCfopSTContribuintesNIncentivo;
                                        tax.VendasNContribuinte = naoContribuinteIncentivo;
                                        tax.ReceitaNormal2 = naoContribuinteNIncetivo;
                                        tax.ReceitaST2 = vendaCfopSTNaoContribuinteNIncetivo;
                                        tax.VendasNContribuinteFora = naoContriForaDoEstadoIncentivo;
                                        tax.ReceitaNormal3 = naoContriForaDoEstadoNIncentivo;
                                        tax.ReceitaST3 = vendaCfopSTNaoContriForaDoEstadoNIncentivo;

                                        for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                        {

                                            decimal icms = (Convert.ToDecimal(comp.IcmsNContribuinteFora) * Convert.ToDecimal(icmsForaDoEstado[j][1])) / 100;
                                            if (icms > 0)
                                            {
                                                Model.Grupo grupo = new Model.Grupo();
                                                grupo.Uf = icmsForaDoEstado[j][0];
                                                grupo.Icms = icms;
                                                grupo.Created = DateTime.Now;
                                                grupo.Updated = grupo.Created;
                                                addGrupos.Add(grupo);
                                            }
                                        }
                                    }
                                
                                }
                                else
                                {
                                    List<ProductIncentivo> productincentivo = new List<ProductIncentivo>();
                                    List<string> codeProdIncentivado = new List<string>();
                                    List<string> codeProdST = new List<string>();
                                    List<string> codeProdIsento = new List<string>();
                                    List<string> cestIncentivado = new List<string>();
                                    List<string> cestST = new List<string>();
                                    List<string> cestIsento = new List<string>();
                                    List<List<string>> percentuaisIncentivado = new List<List<string>>();
                                    List<List<string>> percentuaisNIncentivado = new List<List<string>>();


                                    exitNotes = importXml.Nfe(directoryNfeExit);

                                    var cfopsDevoCompra = _companyCfopService.FindByCfopActive(companyid, "incentivo", "devolucao de compra").Select(_ => _.Cfop.Code);
                                    var cfopsVenda = _companyCfopService.FindByCfopActive(companyid, "incentivo", "venda").Select(_ => _.Cfop.Code);
                                    var cfospDevoVenda = _companyCfopService.FindByCfopActive(companyid, "entrada", "devolução de venda").Select(_ => _.Cfop.Code);

                                    var prodsIncentivo = _productIncentivoService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid)).ToList();

                                    decimal vendasIncentivada = 0, vendasNIncentivada = 0, debitoIncetivo = 0, debitoNIncentivo = 0, credito = 0;

                                    // Vendas
                                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                                    {
                                        if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                                        {
                                            exitNotes.RemoveAt(i);
                                            continue;
                                        }

                                        if (exitNotes[i][1].ContainsKey("dhEmi"))
                                        {
                                            //productincentivo = _productIncentivoService.FindByDate(comp.Id, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));
                                            productincentivo = _productIncentivoService.FindByDate(prodsIncentivo, comp.Id, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));

                                            codeProdIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Code).ToList();
                                            codeProdST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Code).ToList();
                                            codeProdIsento = productincentivo.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Code).ToList();

                                            cestIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Cest).ToList();
                                            cestST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Cest).ToList();
                                            cestIsento = productincentivo.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Cest).ToList();
                                        }

                                        int status = 3;
                                        decimal percent = 0;
                                        bool cfop = false;

                                        for (int k = 0; k < exitNotes[i].Count(); k++)
                                        {
                                            if (exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                status = 3;
                                                percent = 0;

                                                string cest = "";
                                                if (exitNotes[i][k].ContainsKey("CEST"))
                                                {
                                                    cest = exitNotes[i][k]["CEST"];
                                                }

                                                if (exitNotes[i][k].ContainsKey("CFOP"))
                                                {
                                                    cfop = false;
                                                    if (cfopsVenda.Contains(exitNotes[i][k]["CFOP"]))
                                                    {
                                                        cfop = true;
                                                    }

                                                }

                                                if (cfop == true)
                                                {
                                                    if (codeProdIncentivado.Contains(exitNotes[i][k]["cProd"]) && cestIncentivado.Contains(cest))
                                                    {
                                                        if (cestIncentivado.Contains(cest))
                                                        {
                                                            status = 1;
                                                            var percentualIncentivado = Convert.ToDecimal(productincentivo.Where(_ => _.Code.Equals(exitNotes[i][k]["cProd"])).ToList().Select(_ => _.Percentual).FirstOrDefault());
                                                            percent = percentualIncentivado;
                                                            if (percentualIncentivado < 100)
                                                            {
                                                                var percentualNIncentivado = 100 - percentualIncentivado;

                                                                if (exitNotes[i][k].ContainsKey("vProd"))
                                                                {
                                                                    vendasIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vProd"]) * percentualIncentivado) / 100);
                                                                    vendasNIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vProd"]) * percentualNIncentivado) / 100);
                                                                }

                                                                if (exitNotes[i][k].ContainsKey("vFrete"))
                                                                {
                                                                    vendasIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vFrete"]) * percentualIncentivado) / 100);
                                                                    vendasNIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vFrete"]) * percentualNIncentivado) / 100);
                                                                }

                                                                if (exitNotes[i][k].ContainsKey("vDesc"))
                                                                {
                                                                    vendasIncentivada -= ((Convert.ToDecimal(exitNotes[i][k]["vDesc"]) * percentualIncentivado) / 100);
                                                                    vendasNIncentivada -= ((Convert.ToDecimal(exitNotes[i][k]["vDesc"]) * percentualNIncentivado) / 100);
                                                                }

                                                                if (exitNotes[i][k].ContainsKey("vOutro"))
                                                                {
                                                                    vendasIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vOutro"]) * percentualIncentivado) / 100);
                                                                    vendasNIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vOutro"]) * percentualNIncentivado) / 100);
                                                                }

                                                                if (exitNotes[i][k].ContainsKey("vSeg"))
                                                                {
                                                                    vendasIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vSeg"]) * percentualIncentivado) / 100);
                                                                    vendasNIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vSeg"]) * percentualNIncentivado) / 100);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (exitNotes[i][k].ContainsKey("vProd"))
                                                                {
                                                                    vendasIncentivada += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                                }

                                                                if (exitNotes[i][k].ContainsKey("vFrete"))
                                                                {
                                                                    vendasIncentivada += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                                }

                                                                if (exitNotes[i][k].ContainsKey("vDesc"))
                                                                {
                                                                    vendasIncentivada -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                                }

                                                                if (exitNotes[i][k].ContainsKey("vOutro"))
                                                                {
                                                                    vendasIncentivada += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                                }

                                                                if (exitNotes[i][k].ContainsKey("vSeg"))
                                                                {
                                                                    vendasIncentivada += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                                }
                                                            }
                                                        }

                                                    }
                                                    else if (codeProdST.Contains(exitNotes[i][k]["cProd"]) && cestST.Contains(cest))
                                                    {
                                                        if (cestST.Contains(cest))
                                                        {
                                                            if (exitNotes[i][k].ContainsKey("vProd"))
                                                            {
                                                                vendasNIncentivada += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                            }

                                                            if (exitNotes[i][k].ContainsKey("vFrete"))
                                                            {
                                                                vendasNIncentivada += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                            }

                                                            if (exitNotes[i][k].ContainsKey("vDesc"))
                                                            {
                                                                vendasNIncentivada -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                            }

                                                            if (exitNotes[i][k].ContainsKey("vOutro"))
                                                            {
                                                                vendasNIncentivada += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                            }

                                                            if (exitNotes[i][k].ContainsKey("vSeg"))
                                                            {
                                                                vendasNIncentivada += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                            }

                                                            status = 2;
                                                            percent = 0;
                                                        }
                                                    }
                                                    else if (codeProdIsento.Contains(exitNotes[i][k]["cProd"]) && cestIsento.Contains(cest))
                                                    {
                                                        if (cestIsento.Contains(cest))
                                                        {
                                                            status = 3;
                                                            percent = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        throw new Exception("Há Produtos não Tributado");
                                                    }
                                                }


                                            }

                                            if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                            {
                                                if (status == 1)
                                                {
                                                    if (percent < 100)
                                                    {
                                                        var percentNIncentivado = 100 - percent;

                                                        int pos = -1;
                                                        for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                            {
                                                                pos = j;
                                                            }
                                                        }

                                                        if (pos < 0)
                                                        {
                                                            List<string> percIncentivado = new List<string>();
                                                            percIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100).ToString());
                                                            percIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                            percIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percent) / 100).ToString());
                                                            percentuaisIncentivado.Add(percIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100)).ToString();
                                                            percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percent) / 100)).ToString();
                                                        }

                                                        debitoIncetivo += (((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) / 100) * percent) / 100);

                                                        int indice = -1;
                                                        for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                            {
                                                                indice = j;
                                                            }
                                                        }

                                                        if (indice < 0)
                                                        {
                                                            List<string> percNIncentivado = new List<string>();
                                                            percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percentNIncentivado) / 100).ToString());
                                                            percNIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                            percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100).ToString());
                                                            percentuaisNIncentivado.Add(percNIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percentNIncentivado) / 100)).ToString();
                                                            percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100)).ToString();
                                                        }
                                                        debitoNIncentivo += ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100);

                                                    }
                                                    else
                                                    {
                                                        int pos = -1;
                                                        for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                            {
                                                                pos = j;
                                                            }
                                                        }

                                                        if (pos < 0)
                                                        {
                                                            List<string> percIncentivado = new List<string>();
                                                            percIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                            percIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                            percIncentivado.Add(exitNotes[i][k]["vICMS"]);
                                                            percentuaisIncentivado.Add(percIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                            percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + (Convert.ToDecimal(exitNotes[i][k]["vICMS"]))).ToString();
                                                        }

                                                        debitoIncetivo += (Convert.ToDecimal(exitNotes[i][k]["vICMS"]));

                                                    }
                                                }
                                                else if (status == 2)
                                                {
                                                    int pos = -1;
                                                    for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                    {
                                                        if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                        {
                                                            pos = j;
                                                        }
                                                    }

                                                    if (pos < 0)
                                                    {
                                                        List<string> percNIncentivado = new List<string>();
                                                        percNIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                        percNIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                        percNIncentivado.Add(exitNotes[i][k]["vICMS"]);
                                                        percentuaisNIncentivado.Add(percNIncentivado);
                                                    }
                                                    else
                                                    {
                                                        percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                        percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vICMS"])).ToString();
                                                    }
                                                    debitoNIncentivo += (Convert.ToDecimal(exitNotes[i][k]["vICMS"]));

                                                }
                                            }

                                            if (exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                            {
                                                if (status == 1)
                                                {
                                                    if (percent < 100)
                                                    {
                                                        var percentNIncentivado = 100 - percent;

                                                        int pos = -1;
                                                        for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                            {
                                                                pos = j;
                                                            }
                                                        }

                                                        if (pos < 0)
                                                        {
                                                            List<string> percIncentivado = new List<string>();
                                                            percIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percent) / 100).ToString());
                                                            percIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                            percIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vFCP"])) * percent) / 100).ToString());
                                                            percentuaisIncentivado.Add(percIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100)).ToString();
                                                            percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percent) / 100)).ToString();
                                                        }

                                                        debitoIncetivo += ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percent) / 100);

                                                        int indice = -1;
                                                        for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                            {
                                                                indice = j;
                                                            }
                                                        }

                                                        if (indice < 0)
                                                        {
                                                            List<string> percNIncentivado = new List<string>();
                                                            percNIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percentNIncentivado) / 100).ToString());
                                                            percNIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                            percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100).ToString());
                                                            percentuaisNIncentivado.Add(percNIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + (((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percentNIncentivado) / 100)).ToString();
                                                            percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100)).ToString();
                                                        }

                                                        debitoNIncentivo += ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100);

                                                    }
                                                    else
                                                    {
                                                        int pos = -1;
                                                        for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                            {
                                                                pos = j;
                                                            }
                                                        }

                                                        if (pos < 0)
                                                        {
                                                            List<string> percIncentivado = new List<string>();
                                                            percIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                            percIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                            percIncentivado.Add(exitNotes[i][k]["vFCP"]);
                                                            percentuaisIncentivado.Add(percIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                            percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vFCP"])).ToString();
                                                        }

                                                        debitoIncetivo += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                                    }
                                                }
                                                else if (status == 2)
                                                {
                                                    int pos = -1;
                                                    for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                    {
                                                        if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                        {
                                                            pos = j;
                                                        }
                                                    }

                                                    if (pos < 0)
                                                    {
                                                        List<string> percNIncentivado = new List<string>();
                                                        percNIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                        percNIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                        percNIncentivado.Add(exitNotes[i][k]["vFCP"]);

                                                        percentuaisNIncentivado.Add(percNIncentivado);
                                                    }
                                                    else
                                                    {
                                                        percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                        percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vFCP"])).ToString();
                                                    }

                                                    debitoNIncentivo += (Convert.ToDecimal(exitNotes[i][k]["vFCP"]));
                                                }
                                            }

                                        }
                                    }

                                    // Devolução de Compra
                                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                                    {
                                        if (exitNotes[i][1]["finNFe"] != "4")
                                        {
                                            exitNotes.RemoveAt(i);
                                            continue;
                                        }

                                        string CNPJ = exitNotes[i][2].ContainsKey("CNPJ") ? exitNotes[i][2]["CNPJ"] : "";

                                        if (exitNotes[i][1].ContainsKey("dhEmi"))
                                        {
                                            //productincentivo = _productIncentivoService.FindByDate(comp.Id, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));
                                            productincentivo = _productIncentivoService.FindByDate(prodsIncentivo, comp.Id, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));

                                            codeProdIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Code).ToList();
                                            codeProdST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Code).ToList();
                                            codeProdIsento = productincentivo.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Code).ToList();

                                            cestIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Cest).ToList();
                                            cestST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Cest).ToList();
                                            cestIsento = productincentivo.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Cest).ToList();
                                        }

                                        int status = 3;
                                        decimal percent = 0;

                                        bool cfop = false;

                                        for (int k = 0; k < exitNotes[i].Count(); k++)
                                        {
                                            if (exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                status = 3;
                                                percent = 0;
                                                string cest = "";
                                                if (exitNotes[i][k].ContainsKey("CEST"))
                                                {
                                                    cest = exitNotes[i][k]["CEST"];
                    
                                                }

                                                if (exitNotes[i][k].ContainsKey("CFOP"))
                                                {
                                                    cfop = false;
                                                    if (cfopsDevoCompra.Contains(exitNotes[i][k]["CFOP"]))
                                                    {
                                                        cfop = true;
                                                    }

                                                }

                                                if (cfop == true)
                                                {
                                                    if (codeProdIncentivado.Contains(exitNotes[i][k]["cProd"]))
                                                    {
                                                        if (cestIncentivado.Contains(cest))
                                                        {
                                                            status = 1;
                                                            var percentualIncentivado = Convert.ToDecimal(productincentivo.Where(_ => _.Code.Equals(exitNotes[i][k]["cProd"])).ToList().Select(_ => _.Percentual).FirstOrDefault());
                                                            percent = percentualIncentivado;
                                                        }
                                                    }
                                                    else if (codeProdST.Contains(exitNotes[i][k]["cProd"]))
                                                    {
                                                        if (cestST.Contains(cest))
                                                        {
                                                            status = 2;
                                                            percent = 0;
                                                        }
                                                    }
                                                    else if (codeProdIsento.Contains(exitNotes[i][k]["cProd"]))
                                                    {
                                                        if (cestIsento.Contains(cest))
                                                        {
                                                            status = 3;
                                                            percent = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        throw new Exception("Há Produtos não Tributado");
                                                    }
                                                }

                                            }

                                            if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true && CNPJ.Equals(comp.Document))
                                            {
                                                if (status == 1)
                                                {
                                                    if (percent < 100)
                                                    {
                                                        var percentNIncentivado = 100 - percent;

                                                        int pos = -1;
                                                        for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                            {
                                                                pos = j;
                                                            }
                                                        }

                                                        if (pos < 0)
                                                        {
                                                            List<string> percIncentivado = new List<string>();
                                                            percIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100).ToString());
                                                            percIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                            percIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percent) / 100).ToString());
                                                            percentuaisIncentivado.Add(percIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100)).ToString();
                                                            percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percent) / 100)).ToString();
                                                        }

                                                        debitoIncetivo += (((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) / 100) * percent) / 100);

                                                        int indice = -1;
                                                        for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                            {
                                                                indice = j;
                                                            }
                                                        }

                                                        if (indice < 0)
                                                        {
                                                            List<string> percNIncentivado = new List<string>();
                                                            percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percentNIncentivado) / 100).ToString());
                                                            percNIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                            percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100).ToString());
                                                            percentuaisNIncentivado.Add(percNIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percentNIncentivado) / 100)).ToString();
                                                            percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100)).ToString();
                                                        }
                                                        debitoNIncentivo += ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100);

                                                    }
                                                    else
                                                    {
                                                        int pos = -1;
                                                        for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                            {
                                                                pos = j;
                                                            }
                                                        }

                                                        if (pos < 0)
                                                        {
                                                            List<string> percIncentivado = new List<string>();
                                                            percIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                            percIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                            percIncentivado.Add(exitNotes[i][k]["vICMS"]);
                                                            percentuaisIncentivado.Add(percIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                            percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + (Convert.ToDecimal(exitNotes[i][k]["vICMS"]))).ToString();
                                                        }

                                                        debitoIncetivo += (Convert.ToDecimal(exitNotes[i][k]["vICMS"]));

                                                        pos = -1;
                                                    }
                                                }
                                                else if (status == 2)
                                                {
                                                    int pos = -1;
                                                    for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                    {
                                                        if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                        {
                                                            pos = j;
                                                        }
                                                    }

                                                    if (pos < 0)
                                                    {
                                                        List<string> percNIncentivado = new List<string>();
                                                        percNIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                        percNIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                        percNIncentivado.Add(exitNotes[i][k]["vICMS"]);
                                                        percentuaisNIncentivado.Add(percNIncentivado);
                                                    }
                                                    else
                                                    {
                                                        percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                        percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vICMS"])).ToString();
                                                    }
                                                    debitoNIncentivo += (Convert.ToDecimal(exitNotes[i][k]["vICMS"]));

                                                }
                                            }

                                            if (exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true && CNPJ.Equals(comp.Document))
                                            {
                                                if (status == 1)
                                                {
                                                    if (percent < 100)
                                                    {
                                                        var percentNIncentivado = 100 - percent;

                                                        int pos = -1;
                                                        for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                            {
                                                                pos = j;
                                                            }
                                                        }

                                                        if (pos < 0)
                                                        {
                                                            List<string> percIncentivado = new List<string>();
                                                            percIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percent) / 100).ToString());
                                                            percIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                            percIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vFCP"])) * percent) / 100).ToString());
                                                            percentuaisIncentivado.Add(percIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100)).ToString();
                                                            percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percent) / 100)).ToString();
                                                        }

                                                        debitoIncetivo += ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percent) / 100);

                                                        int indice = -1;
                                                        for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                            {
                                                                indice = j;
                                                            }
                                                        }

                                                        if (indice < 0)
                                                        {
                                                            List<string> percNIncentivado = new List<string>();
                                                            percNIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percentNIncentivado) / 100).ToString());
                                                            percNIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                            percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100).ToString());
                                                            percentuaisNIncentivado.Add(percNIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + (((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percentNIncentivado) / 100)).ToString();
                                                            percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100)).ToString();
                                                        }

                                                        debitoNIncentivo += ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100);

                                                    }
                                                    else
                                                    {
                                                        int pos = -1;
                                                        for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                            {
                                                                pos = j;
                                                            }
                                                        }

                                                        if (pos < 0)
                                                        {
                                                            List<string> percIncentivado = new List<string>();
                                                            percIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                            percIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                            percIncentivado.Add(exitNotes[i][k]["vFCP"]);
                                                            percentuaisIncentivado.Add(percIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                            percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vFCP"])).ToString();
                                                        }

                                                        debitoIncetivo += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                                    }
                                                }
                                                else if (status == 2)
                                                {
                                                    int pos = -1;
                                                    for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                    {
                                                        if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                        {
                                                            pos = j;
                                                        }
                                                    }

                                                    if (pos < 0)
                                                    {
                                                        List<string> percNIncentivado = new List<string>();
                                                        percNIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                        percNIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                        percNIncentivado.Add(exitNotes[i][k]["vFCP"]);

                                                        percentuaisNIncentivado.Add(percNIncentivado);
                                                    }
                                                    else
                                                    {
                                                        percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                        percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vFCP"])).ToString();
                                                    }

                                                    debitoNIncentivo += (Convert.ToDecimal(exitNotes[i][k]["vFCP"]));
                                                }
                                            }

                                        }
                                    }

                                    // Devolução de Venda
                                    /*for (int i = exitNotes.Count - 1; i >= 0; i--)
                                    {
                                        bool cfop = false;

                                        for (int k = 0; k < exitNotes[i].Count(); k++)
                                        {
                                            if (exitNotes[i][k].ContainsKey("CFOP"))
                                            {
                                                cfop = false;
                                                if (cfospDevoVenda.Contains(exitNotes[i][k]["CFOP"]))
                                                {
                                                    cfop = true;
                                                }
                                            }

                                            if (exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                            {
                                                credito += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                            }
                                        }
                                    }*/

                                    List<List<string>> valoresIncentivo = new List<List<string>>();

                                    for (int i = 0; i < percentuaisIncentivado.Count(); i++)
                                    {
                                        List<string> percentual = new List<string>();
                                        percentual.Add(percentuaisIncentivado[i][0]);
                                        percentual.Add(percentuaisIncentivado[i][1]);
                                        percentual.Add(percentuaisIncentivado[i][2]);
                                        valoresIncentivo.Add(percentual);
                                    }

                                    List<List<string>> valoresNIncentivo = new List<List<string>>();

                                    for (int i = 0; i < percentuaisNIncentivado.Count(); i++)
                                    {
                                        List<string> percentual = new List<string>();
                                        percentual.Add(percentuaisNIncentivado[i][0]);
                                        percentual.Add(percentuaisNIncentivado[i][1]);
                                        percentual.Add(percentuaisNIncentivado[i][2]);
                                        valoresNIncentivo.Add(percentual);
                                    }

                                    if (valoresIncentivo.Count() < valoresNIncentivo.Count())
                                    {
                                        int diferenca = valoresNIncentivo.Count() - valoresIncentivo.Count();
                                        for (int i = 0; i < diferenca; i++)
                                        {
                                            List<string> percentual = new List<string>();
                                            percentual.Add("0.00");
                                            percentual.Add("0.00");
                                            percentual.Add("0.00");
                                            valoresIncentivo.Add(percentual);
                                        }
                                    }
                                    else if (valoresIncentivo.Count() > valoresNIncentivo.Count())
                                    {
                                        int diferenca = valoresIncentivo.Count() - valoresNIncentivo.Count();
                                        for (int i = 0; i < diferenca; i++)
                                        {
                                            List<string> percentual = new List<string>();
                                            percentual.Add("0.00");
                                            percentual.Add("0.00");
                                            percentual.Add("0.00");
                                            valoresNIncentivo.Add(percentual);
                                        }
                                    }

                                    if (imp != null)
                                    {
                                        imp.CreditoSaida = credito;
                                        imp.VendasIncentivada = vendasIncentivada;
                                        imp.VendasNIncentivada = vendasNIncentivada;

                                        var grupoTemp = _grupoService.FindByGrupos(imp.Id);

                                        List<Model.Grupo> updateGrupos = new List<Model.Grupo>();

                                        if(grupoTemp != null)
                                        {
                                            for (int i = 0; i < valoresIncentivo.Count(); i++)
                                            {
                                                var gg = grupoTemp.Where(_ => _.Percentual.Equals(Convert.ToDecimal(valoresIncentivo[i][1]))).FirstOrDefault();

                                                if (gg != null)
                                                {
                                                    gg.BaseCalculo = Convert.ToDecimal(valoresIncentivo[i][0]);
                                                    gg.Percentual = Convert.ToDecimal(valoresIncentivo[i][1]);
                                                    gg.Icms = Convert.ToDecimal(valoresIncentivo[i][2]);
                                                    gg.BaseCalculoNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][0]);
                                                    gg.PercentualNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][1]);
                                                    gg.IcmsNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][2]);
                                                    gg.Updated = DateTime.Now;
                                                    updateGrupos.Add(gg);
                                                }
                                                else
                                                {
                                                    Model.Grupo grupo = new Model.Grupo();
                                                    grupo.BaseCalculo = Convert.ToDecimal(valoresIncentivo[i][0]);
                                                    grupo.Percentual = Convert.ToDecimal(valoresIncentivo[i][1]);
                                                    grupo.Icms = Convert.ToDecimal(valoresIncentivo[i][2]);
                                                    grupo.BaseCalculoNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][0]);
                                                    grupo.PercentualNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][1]);
                                                    grupo.IcmsNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][2]);
                                                    grupo.TaxId = imp.Id;
                                                    grupo.Created = DateTime.Now;
                                                    grupo.Updated = grupo.Created;
                                                    addGrupos.Add(grupo);
                                                }
                                            }
                                            _grupoService.Create(addGrupos, null);
                                            _grupoService.Update(updateGrupos, null);
                                        }
                                        else
                                        {
                                            for (int i = 0; i < valoresIncentivo.Count(); i++)
                                            {
                                                Model.Grupo grupo = new Model.Grupo();
                                                grupo.BaseCalculo = Convert.ToDecimal(valoresIncentivo[i][0]);
                                                grupo.Percentual = Convert.ToDecimal(valoresIncentivo[i][1]);
                                                grupo.Icms = Convert.ToDecimal(valoresIncentivo[i][2]);
                                                grupo.BaseCalculoNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][0]);
                                                grupo.PercentualNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][1]);
                                                grupo.IcmsNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][2]);
                                                grupo.TaxId = imp.Id;
                                                grupo.Created = DateTime.Now;
                                                grupo.Updated = grupo.Created;
                                                addGrupos.Add(grupo);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        tax.CreditoSaida = credito;
                                        tax.VendasIncentivada = vendasIncentivada;
                                        tax.VendasNIncentivada = vendasNIncentivada;

                                        for (int i = 0; i < valoresIncentivo.Count(); i++)
                                        {
                                            Model.Grupo grupo = new Model.Grupo();
                                            grupo.BaseCalculo = Convert.ToDecimal(valoresIncentivo[i][0]);
                                            grupo.Percentual = Convert.ToDecimal(valoresIncentivo[i][1]);
                                            grupo.Icms = Convert.ToDecimal(valoresIncentivo[i][2]);
                                            grupo.BaseCalculoNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][0]);
                                            grupo.PercentualNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][1]);
                                            grupo.IcmsNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][2]);
                                            grupo.Created = DateTime.Now;
                                            grupo.Updated = grupo.Created;
                                            addGrupos.Add(grupo);
                                        }
                                    }

                                }
                            }
                            else if (comp.AnnexId.Equals(3))
                            {
                                var clientesAll = _clientService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid)).ToList();
                                var nContribuintes = clientesAll.Where(_ => _.TypeClientId.Equals(2)).Select(_ => _.Document).ToList();
                                var suspensions = _suspensionService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid)).ToList();

                                var cfopVenda = _companyCfopService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid) && _.Active.Equals(true) && (_.CfopTypeId.Equals(1) || _.CfopTypeId.Equals(4) || _.CfopTypeId.Equals(5))).Select(_ => _.Cfop.Code).ToList();

                                if (comp.SectionId.Equals(2))
                                {
                                    exitNotes = importXml.Nfe(directoryNfeExit);

                                    decimal vendasInternasElencadas = 0, vendasInterestadualElencadas = 0, vendasInternasDeselencadas = 0, vendasInterestadualDeselencadas = 0,
                                        InternasElencadas = 0, InterestadualElencadas = 0, InternasElencadasPortaria = 0, InterestadualElencadasPortaria = 0,
                                        InternasDeselencadas = 0, InterestadualDeselencadas = 0, InternasDeselencadasPortaria = 0, InterestadualDeselencadasPortaria = 0,
                                        suspensao = 0, vendasClienteCredenciado = 0, vendas = 0;

                                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                                    {
                                        if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                                        {
                                            exitNotes.RemoveAt(i);
                                            continue;
                                        }


                                        bool clenteCredenciado = false, ncm = false, cfop = false, suspenso = false;

                                        if (exitNotes[i][1].ContainsKey("dhEmi"))
                                        {
                                            foreach (var suspension in suspensions)
                                            {
                                                if (Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) >= Convert.ToDateTime(suspension.DateStart) && Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) < Convert.ToDateTime(suspension.DateEnd))
                                                {
                                                    suspenso = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (exitNotes[i][3].ContainsKey("CNPJ"))
                                        {
                                            if (nContribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                            {
                                                clenteCredenciado = true;
                                            }

                                            bool existe = false;

                                            if (clientesAll.Select(_ => _.Document).Contains(exitNotes[i][3]["CNPJ"]))
                                            {
                                                existe = true;
                                            }

                                            if (existe == false)
                                            {
                                                throw new Exception("Há Clientes não Importados");
                                            }
                                        }

                                        for (int j = 0; j < exitNotes[i].Count; j++)
                                        {
                                            if (exitNotes[i][j].ContainsKey("NCM"))
                                            {
                                                ncm = _itemService.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId), exitNotes[i][j]["NCM"].ToString());
                                            }

                                            if (exitNotes[i][j].ContainsKey("CFOP"))
                                            {
                                                if (cfopVenda.Contains(exitNotes[i][j]["CFOP"]))
                                                {
                                                    cfop = true;
                                                }
                                            }

                                            if (clenteCredenciado == true)
                                            {
                                                if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasClienteCredenciado += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }

                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasClienteCredenciado += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasClienteCredenciado -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        vendas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasClienteCredenciado += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasClienteCredenciado += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualDeselencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualDeselencadasPortaria -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }

                                    if (imp != null)
                                    {
                                        imp.VendasClientes = vendasClienteCredenciado;
                                        imp.VendasInternas1 = vendasInternasElencadas;
                                        imp.VendasInterestadual1 = vendasInterestadualElencadas;
                                        imp.SaidaInterna1 = InternasElencadas;
                                        imp.SaidaInterestadual1 = InterestadualElencadas;
                                        imp.SaidaPortInterna1 = InternasElencadasPortaria;
                                        imp.saidaPortInterestadual1 = InterestadualElencadasPortaria;

                                        imp.VendasInternas2 = vendasInternasDeselencadas;
                                        imp.VendasInterestadual2 = vendasInterestadualDeselencadas;
                                        imp.SaidaInterna2 = InternasDeselencadas;
                                        imp.SaidaInterestadual2 = InterestadualDeselencadas;
                                        imp.SaidaPortInterna2 = InternasDeselencadasPortaria;
                                        imp.saidaPortInterestadual2 = InterestadualDeselencadasPortaria;
                                        imp.Suspensao = suspensao;
                                    }
                                    else
                                    {
                                        tax.VendasClientes = vendasClienteCredenciado;
                                        tax.VendasInternas1 = vendasInternasElencadas;
                                        tax.VendasInterestadual1 = vendasInterestadualElencadas;
                                        tax.SaidaInterna1 = InternasElencadas;
                                        tax.SaidaInterestadual1 = InterestadualElencadas;
                                        tax.SaidaPortInterna1 = InternasElencadasPortaria;
                                        tax.saidaPortInterestadual1 = InterestadualElencadasPortaria;

                                        tax.VendasInternas2 = vendasInternasDeselencadas;
                                        tax.VendasInterestadual2 = vendasInterestadualDeselencadas;
                                        tax.SaidaInterna2 = InternasDeselencadas;
                                        tax.SaidaInterestadual2 = InterestadualDeselencadas;
                                        tax.SaidaPortInterna2 = InternasDeselencadasPortaria;
                                        tax.saidaPortInterestadual2 = InterestadualDeselencadasPortaria;
                                        tax.Suspensao = suspensao;
                                    }
                                }
                            }
                        }

                    }
                }
                else if (imposto.Equals("pisCofins"))
                {
                    var companies = _companyService.FindByCompanies().Where(_ => _.Document.Substring(0, 8).Equals(comp.Document.Substring(0, 8))).ToList();
                    var ncmsCompany = _taxationNcmService.FindAll(null).Where(_ => _.Company.Document.Substring(0, 8).Equals(comp.Document.Substring(0, 8))).ToList();
                    var cfopsDevolucao = _companyCfopService.FindByCfopDevolucao(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();

                    if (type.Equals("sped") && comp.Sped.Equals(true))
                    {
                        decimal  devolucaoComercio = 0, devolucaoServico = 0, devolucaoPetroleo = 0, devolucaoTransporte = 0, devolucaoMono = 0;

                        foreach (var cc in caminhos)
                        {
                            var devolucoes = importSped.SpedDevolucao(cc, cfopsDevolucao, ncmsCompany);
                            devolucaoPetroleo += devolucoes[0];
                            devolucaoComercio += devolucoes[1];
                            devolucaoTransporte += devolucoes[2];
                            devolucaoServico += devolucoes[3];
                            devolucaoMono += devolucoes[4];
                        }


                        if (imp != null)
                        {
                            imp.Devolucao1Entrada = devolucaoPetroleo;
                            imp.Devolucao2Entrada = devolucaoComercio;
                            imp.Devolucao3Entrada = devolucaoTransporte;
                            imp.Devolucao4Entrada = devolucaoServico;
                            imp.DevolucaoMonoEntrada = devolucaoMono;
                        }
                        else
                        {
                            tax.Devolucao1Entrada = devolucaoPetroleo;
                            tax.Devolucao2Entrada = devolucaoComercio;
                            tax.Devolucao3Entrada = devolucaoTransporte;
                            tax.Devolucao4Entrada = devolucaoServico;
                            tax.DevolucaoMonoEntrada = devolucaoMono;
                        }
  

                    }
                    else if (type.Equals("xml"))
                    {
                        List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();
                        List<string> codeProdMono = new List<string>();
                        List<string> ncmMono = new List<string>();

                        var cfopsVenda = _companyCfopService.FindByCfopVendas(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();

                        var codeProdAll = ncmsCompany.Select(_ => _.CodeProduct).ToList();
                        var ncmAll = ncmsCompany.Select(_ => _.Ncm.Code).ToList();

                        var codeProd1 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(1)).Select(_ => _.CodeProduct).ToList();
                        var codeProd2 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(2)).Select(_ => _.CodeProduct).ToList();
                        var codeProd3 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(3)).Select(_ => _.CodeProduct).ToList();
                        var codeProd4 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(4)).Select(_ => _.CodeProduct).ToList();

                        var ncm1 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(1)).Select(_ => _.Ncm.Code).ToList();
                        var ncm2 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(2)).Select(_ => _.Ncm.Code).ToList();
                        var ncm3 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(3)).Select(_ => _.Ncm.Code).ToList();
                        var ncm4 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(4)).Select(_ => _.Ncm.Code).ToList();

                        decimal receitaComercio = 0, devolucaoComercio = 0, receitaServico = 0, devolucaoServico = 0, receitaPetroleo = 0, devolucaoPetroleo = 0,
                            receitaTransporte = 0, devolucaoTransporte = 0, receitaMono = 0, devolucaoMono = 0;

                        foreach (var c in companies)
                        {
                            List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                            List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();
                            
                            directoryNfeExit = NfeExit.Value + "\\" + c.Document + "\\" + year + "\\" + month;
                            directoryNfeEntry = NfeEntry.Value + "\\" + c.Document + "\\" + year + "\\" + month;

                            exitNotes = importXml.Nfe(directoryNfeExit);

                            if (c.Sped.Equals(false))
                            {
                                entryNotes = importXml.Nfe(directoryNfeExit);
                            }

                            // Receitas
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                if (!exitNotes[i][2]["CNPJ"].Equals(c.Document))
                                {
                                    exitNotes.RemoveAt(i);
                                    continue;
                                }

                                if (exitNotes[i][1].ContainsKey("dhEmi"))
                                {

                                    ncmsTaxation = _taxationNcmService.FindAllInDate(ncmsCompany, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));

                                    codeProdMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.CodeProduct).ToList();
                                    ncmMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.Ncm.Code).ToList();
                                }

                                bool cfop = false;

                                for (int j = 0; j < exitNotes[i].Count; j++)
                                {
                                    

                                    if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("NCM"))
                                    {
                                        if (exitNotes[i][j].ContainsKey("CFOP"))
                                        {
                                            if (cfopsVenda.Contains(exitNotes[i][j]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }

                                        if (codeProdAll.Contains(exitNotes[i][j]["cProd"]) && ncmAll.Contains(exitNotes[i][j]["NCM"]))
                                        {
                                            if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][1]["finNFe"] != "4" && cfop == true)
                                            {
                                                if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                }
                                                else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                }
                                                else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                }
                                                else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaServico += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                }

                                                if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaMono += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][1]["finNFe"] != "4" && cfop == true)
                                            {

                                                if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                }
                                                else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                }
                                                else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                }
                                                else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaServico += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                }


                                                if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaMono += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][1]["finNFe"] != "4" && cfop == true)
                                            {
                                                if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaPetroleo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                }
                                                else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaComercio -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                }
                                                else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaTransporte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                }
                                                else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaServico -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                }

                                                if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaMono -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][1]["finNFe"] != "4" && cfop == true)
                                            {
                                                if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                }
                                                else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                }
                                                else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                }
                                                else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaServico += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                }

                                                if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaMono += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][1]["finNFe"] != "4" && cfop == true)
                                            {
                                                if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                }
                                                else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                }
                                                else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                }
                                                else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaServico += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                }

                                                if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    receitaMono += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("Há Ncm não Tributado");
                                        }
                                    }


                                }
                            }

                            // Devoluções de Vendas
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                if (exitNotes[i][1]["finNFe"] != "4")
                                {
                                    exitNotes.RemoveAt(i);
                                    continue;
                                }

                                if (exitNotes[i][3].ContainsKey("CNPJ"))
                                {
                                    if (!exitNotes[i][3]["CNPJ"].Equals(comp.Document))
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }
                                }

                                if (exitNotes[i][1].ContainsKey("dhEmi"))
                                {
                                   
                                    ncmsTaxation = _taxationNcmService.FindAllInDate(ncmsCompany, Convert.ToDateTime(exitNotes[i][1]["dhEmi"])).Where(_ => _.Company.CountingTypeId.Equals(2) || _.Company.CountingTypeId.Equals(3)).ToList();
                                    
                                    codeProdMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.CodeProduct).ToList();
                                    ncmMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.Ncm.Code).ToList();
                                }

                                for (int j = 0; j < exitNotes[i].Count; j++)
                                {
                                    if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("NCM"))
                                    {
                                        if (codeProdAll.Contains(exitNotes[i][j]["cProd"]) && ncmAll.Contains(exitNotes[i][j]["NCM"]))
                                        {
                                            if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vProd"))
                                            {
                                                if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                }
                                                else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                }
                                                else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                }
                                                else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                }

                                                if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoMono += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vFrete"))
                                            {
                                                if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                }
                                                else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                }
                                                else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                }
                                                else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                }

                                                if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoMono += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vDesc"))
                                            {
                                                if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoPetroleo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                }
                                                else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoComercio -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                }
                                                else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoTransporte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                }
                                                else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoServico -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                }


                                                if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoMono -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vOutro"))
                                            {
                                                if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                }
                                                else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                }
                                                else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                }
                                                else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                }

                                                if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoMono += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vSeg"))
                                            {
                                                if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                }
                                                else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                }
                                                else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                }
                                                else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                }

                                                if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    devolucaoMono += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                }
                                            }
                                        
                                        }
                                        else
                                        {
                                            throw new Exception("Há Ncm não Tributado");
                                        }

                                    }
                                }
                            }

                            // Devoluções de Vendas
                            for (int i = entryNotes.Count - 1; i >= 0; i--)
                            {
                                if(entryNotes[i][1]["finNFe"] != "4" || !entryNotes[i][3]["CNPJ"].Equals(c.Document))
                                {
                                    entryNotes.RemoveAt(i);
                                    continue;
                                }

                                if (entryNotes[i][1].ContainsKey("dhEmi"))
                                {

                                    ncmsTaxation = _taxationNcmService.FindAllInDate(ncmsCompany, Convert.ToDateTime(entryNotes[i][1]["dhEmi"]));

                                    codeProdMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.CodeProduct).ToList();
                                    ncmMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.Ncm.Code).ToList();
                                }

                                for (int j = 0; j < entryNotes[i].Count; j++)
                                {
                                    if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vProd"))
                                    {
                                        if (codeProd1.Contains(entryNotes[i][j]["cProd"]) && ncm1.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                        }
                                        else if (codeProd2.Contains(entryNotes[i][j]["cProd"]) && ncm2.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                        }
                                        else if (codeProd3.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                        }
                                        else if (codeProd4.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                        }

                                        if (codeProdMono.Contains(entryNotes[i][j]["cProd"]) && ncmMono.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoMono += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                        }
                                    }

                                    if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vFrete"))
                                    {
                                        if (codeProd1.Contains(entryNotes[i][j]["cProd"]) && ncm1.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                        }
                                        else if (codeProd2.Contains(entryNotes[i][j]["cProd"]) && ncm2.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                        }
                                        else if (codeProd3.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                        }
                                        else if (codeProd4.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                        }

                                        if (codeProdMono.Contains(entryNotes[i][j]["cProd"]) && ncmMono.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoMono += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                        }
                                    }

                                    if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vDesc"))
                                    {
                                        if (codeProd1.Contains(entryNotes[i][j]["cProd"]) && ncm1.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoPetroleo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                        }
                                        else if (codeProd2.Contains(entryNotes[i][j]["cProd"]) && ncm2.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoComercio -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                        }
                                        else if (codeProd3.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoTransporte -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                        }
                                        else if (codeProd4.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoServico -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                        }

                                        if (codeProdMono.Contains(entryNotes[i][j]["cProd"]) && ncmMono.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoMono -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                        }
                                    }

                                    if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vOutro"))
                                    {
                                        if (codeProd1.Contains(entryNotes[i][j]["cProd"]) && ncm1.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                        }
                                        else if (codeProd2.Contains(entryNotes[i][j]["cProd"]) && ncm2.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                        }
                                        else if (codeProd3.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                        }
                                        else if (codeProd4.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                        }

                                        if (codeProdMono.Contains(entryNotes[i][j]["cProd"]) && ncmMono.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoMono += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                        }
                                    }

                                    if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vSeg"))
                                    {
                                        if (codeProd1.Contains(entryNotes[i][j]["cProd"]) && ncm1.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                        }
                                        else if (codeProd2.Contains(entryNotes[i][j]["cProd"]) && ncm2.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                        }
                                        else if (codeProd3.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                        }
                                        else if (codeProd4.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                        }

                                        if (codeProdMono.Contains(entryNotes[i][j]["cProd"]) && ncmMono.Contains(entryNotes[i][j]["NCM"]))
                                        {
                                            devolucaoMono += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                        }
                                    }

                                }
                            }
                        }


                        if (imp != null)
                        {
                            imp.Receita1 = receitaPetroleo;
                            imp.Receita2 = receitaComercio;
                            imp.Receita3 = receitaTransporte;
                            imp.Receita4 = receitaServico;
                            imp.ReceitaMono = receitaMono;
                            imp.Devolucao1Saida = devolucaoPetroleo;
                            imp.Devolucao2Saida = devolucaoComercio;
                            imp.Devolucao3Saida = devolucaoTransporte;
                            imp.Devolucao4Saida = devolucaoServico;
                            imp.DevolucaoMonoSaida = devolucaoMono;
                        }
                        else
                        {
                            tax.Receita1 = receitaPetroleo;
                            tax.Receita2 = receitaComercio;
                            tax.Receita3 = receitaTransporte;
                            tax.Receita4 = receitaServico;
                            tax.ReceitaMono = receitaMono;
                            tax.Devolucao1Saida = devolucaoPetroleo;
                            tax.Devolucao2Saida = devolucaoComercio;
                            tax.Devolucao3Saida = devolucaoTransporte;
                            tax.Devolucao4Saida = devolucaoServico;
                            tax.DevolucaoMonoSaida = devolucaoMono;
                        }
                    }
                }

                if (imp != null)
                {
                    _service.Update(imp, null);
                }
                else
                {
                    _service.Create(tax, null);
                    imp = _service.FindByMonth(companyid, month, year);

                    foreach(var g in addGrupos)
                    {
                        g.TaxId = imp.Id;
                    }
                    _grupoService.Create(addGrupos, null);
                }

                return RedirectToAction("Index", new { id = companyid, year = year,  month = month});
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    
    }
}
