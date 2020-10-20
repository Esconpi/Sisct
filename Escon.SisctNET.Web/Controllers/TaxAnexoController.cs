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
    public class TaxAnexoController : ControllerBaseSisctNET
    {
        private readonly ITaxAnexoService _service;
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly INcmConvenioService _ncmConvenioService;
        private readonly ICompanyCfopService _companyCfopService;
        private readonly ICompraAnexoService _compraAnexoService;
        private readonly IDevoClienteService _devoClienteService;
        private readonly IDevoFornecedorService _devoFornecedorService;
        private readonly IVendaAnexoService _vendaAnexoService;
        private readonly IHostingEnvironment _appEnvironment;

        public TaxAnexoController(
            ITaxAnexoService service,
            ICompanyService companyService,
            IConfigurationService configurationService,
            INcmConvenioService ncmConvenioService,
            ICompanyCfopService companyCfopService,
            ICompraAnexoService compraAnexoService,
            IDevoClienteService devoClienteService,
            IDevoFornecedorService devoFornecedorService,
            IVendaAnexoService vendaAnexoService,
            IHostingEnvironment env,
            IFunctionalityService functionalityService,
             IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Taxation")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _ncmConvenioService = ncmConvenioService;
            _companyCfopService = companyCfopService;
            _compraAnexoService = compraAnexoService;
            _devoClienteService = devoClienteService;
            _devoFornecedorService = devoFornecedorService;
            _vendaAnexoService = vendaAnexoService;
            _appEnvironment = env;
        }

        public IActionResult Index(int id, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Taxation")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var company = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                ViewBag.Comp = company;
                ViewBag.Id = company.Id;
                ViewBag.Company = company.SocialName;
                ViewBag.Document = company.Document;
                ViewBag.Month = month;
                ViewBag.Year = year;

                var result = _service.FindByMonth(id,month,year);

                List<Model.VendaAnexo> vendas = new List<Model.VendaAnexo>();
                List<Model.DevoFornecedor> devoFornecedors = new List<Model.DevoFornecedor>();
                List<Model.CompraAnexo> compras = new List<Model.CompraAnexo>();
                List<Model.DevoCliente> devoClientes = new List<Model.DevoCliente>();

                if (result != null)
                {
                    vendas = _vendaAnexoService.FindByVendasTax(result.Id);
                    devoFornecedors = _devoFornecedorService.FindByDevoTax(result.Id);
                    compras = _compraAnexoService.FindByComprasTax(result.Id);
                    devoClientes = _devoClienteService.FindByDevoTax(result.Id);
                }
               

                ViewBag.VendasInternas = vendas;
                ViewBag.DevoClienteInternas = devoClientes;
                ViewBag.ComprasInternas = compras;
                ViewBag.DevoFornecedorInternas = devoFornecedors;

                return View(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Import(int companyid, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Taxation")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var company = _companyService.FindById(companyid, GetLog(Model.OccorenceLog.Read));
                ViewBag.Id = company.Id;
                ViewBag.Month = month;
                ViewBag.Year = year;
                return View(company);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Import(int companyid, string year, string month, string type, IFormFile arquivo)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Taxation")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var comp = _companyService.FindById(companyid, GetLog(Model.OccorenceLog.Read));

                var NfeExit = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));
                var NfeEntry = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                string directoryNfeExit = NfeExit.Value + "\\" + comp.Document + "\\" + year + "\\" + month;
                string directoryNfeEntry = NfeEntry.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                var importXml = new Xml.Import(_companyCfopService);
                var importSped = new Sped.Import(_companyCfopService);

                List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();

                var cfopsVenda = _companyCfopService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid) && _.Active.Equals(true) && (_.CfopTypeId.Equals(1) || _.CfopTypeId.Equals(2) || _.CfopTypeId.Equals(4) || _.CfopTypeId.Equals(5))).Select(_ => _.Cfop.Code).ToList();
                var cfopsDevo = _companyCfopService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid) && _.Active.Equals(true) && (_.CfopTypeId.Equals(3) || _.CfopTypeId.Equals(7))).Select(_ => _.Cfop.Code).ToList();

                var ncms = _ncmConvenioService.FindByAnnex(Convert.ToInt32(comp.AnnexId));

                var imp = _service.FindByMonth(companyid, month, year);

                Model.TaxAnexo taxAnexo = new Model.TaxAnexo();


                if (imp != null)
                {
                    imp.Updated = DateTime.Now;
                }
                else
                {
                    taxAnexo.CompanyId = companyid;
                    taxAnexo.MesRef = month;
                    taxAnexo.AnoRef = year;
                    taxAnexo.Created = DateTime.Now;
                    taxAnexo.Updated = taxAnexo.Created;
                }

                if (type.Equals("sped"))
                {
                    string filedir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Speds");

                    if (!Directory.Exists(filedir))
                    {
                        Directory.CreateDirectory(filedir);
                    }

                    string caminho_WebRoot = _appEnvironment.WebRootPath;
                    string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Speds\\";

                    string nomeArquivo = comp.Document + year + month + ".txt";

                    string caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                    string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivo);

                    if (System.IO.File.Exists(caminhoDestinoArquivoOriginal))
                    {
                        System.IO.File.Delete(caminhoDestinoArquivoOriginal);
                    }

                    var stream = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);

                    await arquivo.CopyToAsync(stream);

                    stream.Close();
                }
                else
                {
                    if (comp.AnnexId.Equals(1))
                    {
                        exitNotes = importXml.Nfe(directoryNfeExit);

                        decimal baseCalcVendaInterestadual4 = 0, baseCalcVendaInterestadual7 = 0, baseCalcVendaInterestadual12 = 0,
                           icmsVendaInterestadual4 = 0, icmsVendaInterestadual7 = 0, icmsVendaInterestadual12 = 0,
                           baseCalcDevoFornecedorInterestadual4 = 0, baseCalcDevoFornecedorInterestadual7 = 0, baseCalcDevoFornecedorInterestadual12 = 0,
                           icmsDevoFornecedorInterestadual4 = 0, icmsDevoFornecedorInterestadual7 = 0, icmsDevoFornecedorInterestadual12 = 0;

                        List<List<string>> devoFornecedorInterna = new List<List<string>>();
                        List<List<string>> vendaInterna = new List<List<string>>();

                        // Vendas
                        for (int i = exitNotes.Count - 1; i >= 0; i--)
                        {
                            if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                            {
                                exitNotes.RemoveAt(i);
                                continue;
                            }

                            bool ncm = false;

                            for (int k = 0; k < exitNotes[i].Count(); k++)
                            {
                                if (exitNotes[i][k].ContainsKey("NCM"))
                                {
                                    ncm = false;

                                    for (int j = 0; j < ncms.Count(); j++)
                                    {
                                        int tamanho = ncms[j].Length;

                                        if (ncms[j].Equals(exitNotes[i][k]["NCM"].Substring(0, tamanho)))
                                        {
                                            ncm = true;
                                            break;
                                        }
                                    }
                                }

                                if (exitNotes[i][k].ContainsKey("pICMS") && !exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && exitNotes[i][1]["finNFe"] != "4" && ncm == false)
                                {
                                    if (exitNotes[i][1]["idDest"].Equals("1"))
                                    {
                                        int pos = -1;
                                        for (int j = 0; j < vendaInterna.Count(); j++)
                                        {
                                            if (vendaInterna[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                            {
                                                pos = j;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> cc = new List<string>();
                                            cc.Add(exitNotes[i][k]["vBC"]);
                                            cc.Add(exitNotes[i][k]["pICMS"]);
                                            cc.Add(exitNotes[i][k]["vICMS"]);
                                            vendaInterna.Add(cc);
                                        }
                                        else
                                        {
                                            vendaInterna[pos][0] = (Convert.ToDecimal(vendaInterna[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                            vendaInterna[pos][2] = (Convert.ToDecimal(vendaInterna[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vICMS"])).ToString();
                                        }

                                    }
                                    else
                                    {
                                        if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(4))
                                        {
                                            baseCalcVendaInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsVendaInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(7))
                                        {
                                            baseCalcVendaInterestadual7 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsVendaInterestadual7 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(12))
                                        {
                                            baseCalcVendaInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsVendaInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                    }
                                }

                                if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && exitNotes[i][1]["finNFe"] != "4" && ncm == false)
                                {
                                    if (exitNotes[i][1]["idDest"].Equals("1"))
                                    {
                                        int pos = -1;
                                        for (int j = 0; j < vendaInterna.Count(); j++)
                                        {
                                            if (vendaInterna[j][1].Equals((Math.Round(Convert.ToDecimal(exitNotes[i][k]["pICMS"]) + Convert.ToDecimal(exitNotes[i][k]["pFCP"]), 2)).ToString()))
                                            {
                                                pos = j;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> cc = new List<string>();
                                            cc.Add(exitNotes[i][k]["vBC"]);
                                            cc.Add((Math.Round(Convert.ToDecimal(exitNotes[i][k]["pICMS"]) + Convert.ToDecimal(exitNotes[i][k]["pFCP"]), 2)).ToString());
                                            cc.Add(exitNotes[i][k]["vICMS"]);
                                            vendaInterna.Add(cc);
                                        }
                                        else
                                        {
                                            vendaInterna[pos][0] = (Convert.ToDecimal(vendaInterna[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                            vendaInterna[pos][2] = (Convert.ToDecimal(vendaInterna[pos][2]) + (Convert.ToDecimal(exitNotes[i][k]["vICMS"]) + Convert.ToDecimal(exitNotes[i][k]["vFCP"]))).ToString();
                                        }

                                    }
                                    else
                                    {
                                        if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(4))
                                        {
                                            baseCalcVendaInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsVendaInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(7))
                                        {
                                            baseCalcVendaInterestadual7 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsVendaInterestadual7 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(12))
                                        {
                                            baseCalcVendaInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsVendaInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                    }
                                }
                            }
                        }

                        // Devolução a Fornecedor
                        for (int i = exitNotes.Count - 1; i >= 0; i--)
                        {
                            if (exitNotes[i][1]["finNFe"] != "4" || exitNotes[i][1]["tpNF"] == "0")
                            {
                                exitNotes.RemoveAt(i);
                                continue;
                            }

                            string CNPJ = exitNotes[i][3].ContainsKey("CNPJ") ? exitNotes[i][3]["CNPJ"] : "";

                            for (int k = 0; k < exitNotes[i].Count(); k++)
                            {
                                if (exitNotes[i][k].ContainsKey("pICMS") && !exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig"))
                                {
                                    if (exitNotes[i][1]["idDest"].Equals("1"))
                                    {
                                        int pos = -1;
                                        for (int j = 0; j < devoFornecedorInterna.Count(); j++)
                                        {
                                            if (devoFornecedorInterna[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                            {
                                                pos = j;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> cc = new List<string>();
                                            cc.Add(exitNotes[i][k]["vBC"]);
                                            cc.Add(exitNotes[i][k]["pICMS"]);
                                            cc.Add(exitNotes[i][k]["vICMS"]);
                                            devoFornecedorInterna.Add(cc);
                                        }
                                        else
                                        {
                                            devoFornecedorInterna[pos][0] = (Convert.ToDecimal(devoFornecedorInterna[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                            devoFornecedorInterna[pos][2] = (Convert.ToDecimal(devoFornecedorInterna[pos][2]) + (Convert.ToDecimal(exitNotes[i][k]["vICMS"]))).ToString();
                                        }

                                    }
                                    else
                                    {
                                        if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(4))
                                        {
                                            baseCalcDevoFornecedorInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsDevoFornecedorInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(7))
                                        {
                                            baseCalcDevoFornecedorInterestadual7 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsDevoFornecedorInterestadual7 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(12))
                                        {
                                            baseCalcDevoFornecedorInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsDevoFornecedorInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                    }
                                }

                                if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig"))
                                {
                                    if (exitNotes[i][1]["idDest"].Equals("1"))
                                    {
                                        int pos = -1;
                                        for (int j = 0; j < devoFornecedorInterna.Count(); j++)
                                        {
                                            if (devoFornecedorInterna[j][1].Equals((Math.Round(Convert.ToDecimal(exitNotes[i][k]["pICMS"]) + Convert.ToDecimal(exitNotes[i][k]["pFCP"]), 2)).ToString()))
                                            {
                                                pos = j;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> cc = new List<string>();
                                            cc.Add(exitNotes[i][k]["vBC"]);
                                            cc.Add((Math.Round(Convert.ToDecimal(exitNotes[i][k]["pICMS"]) + Convert.ToDecimal(exitNotes[i][k]["pFCP"]), 2)).ToString());
                                            cc.Add(exitNotes[i][k]["vICMS"]);
                                            devoFornecedorInterna.Add(cc);
                                        }
                                        else
                                        {
                                            devoFornecedorInterna[pos][0] = (Convert.ToDecimal(devoFornecedorInterna[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                            devoFornecedorInterna[pos][2] = (Convert.ToDecimal(devoFornecedorInterna[pos][2]) + (Convert.ToDecimal(exitNotes[i][k]["vICMS"]) + Convert.ToDecimal(exitNotes[i][k]["vFCP"]))).ToString();
                                        }

                                    }
                                    else
                                    {
                                        if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(4))
                                        {
                                            baseCalcDevoFornecedorInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsDevoFornecedorInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(7))
                                        {
                                            baseCalcDevoFornecedorInterestadual7 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsDevoFornecedorInterestadual7 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(12))
                                        {
                                            baseCalcDevoFornecedorInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsDevoFornecedorInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                    }
                                }

                            }
                        }


                        if (imp != null)
                        {
                            imp.BaseVenda4 = baseCalcVendaInterestadual4;
                            imp.BaseVenda7 = baseCalcVendaInterestadual7;
                            imp.BaseVenda12 = baseCalcVendaInterestadual12;
                            imp.IcmsVenda4 = icmsVendaInterestadual4;
                            imp.IcmsVenda7 = icmsVendaInterestadual7;
                            imp.IcmsVenda12 = icmsVendaInterestadual12;

                            imp.BaseDevoFornecedor4 = baseCalcDevoFornecedorInterestadual4;
                            imp.BaseDevoFornecedor7 = baseCalcDevoFornecedorInterestadual7;
                            imp.BaseDevoFornecedor12 = baseCalcDevoFornecedorInterestadual12;
                            imp.IcmsDevoFornecedor4 = icmsDevoFornecedorInterestadual4;
                            imp.IcmsDevoFornecedor7 = icmsDevoFornecedorInterestadual7;
                            imp.IcmsDevoFornecedor12 = icmsDevoFornecedorInterestadual12;

                            _service.Update(imp, null);

                            var vendas = _vendaAnexoService.FindByVendasTax(imp.Id);
                            var devoFornecedors = _devoFornecedorService.FindByDevoTax(imp.Id);

                            List<Model.VendaAnexo> vendaAnexosAdd = new List<Model.VendaAnexo>();
                            List<Model.VendaAnexo> vendaAnexosUpdate = new List<Model.VendaAnexo>();

                            List<Model.DevoFornecedor> devoFornecedorsAdd = new List<Model.DevoFornecedor>();
                            List<Model.DevoFornecedor> devoFornecedorsUpdate = new List<Model.DevoFornecedor>();

                            for (int i = 0; i < vendaInterna.Count(); i++)
                            {
                                Model.VendaAnexo vendaAnexo = new Model.VendaAnexo();
                                var venda = vendas.Where(_ => _.Aliquota.Equals(Convert.ToDecimal(vendaInterna[i][1]))).FirstOrDefault();

                                if (venda == null)
                                {
                                    vendaAnexo.TaxAnexoId = imp.Id;
                                    vendaAnexo.Base = Convert.ToDecimal(vendaInterna[i][0]);
                                    vendaAnexo.Aliquota = Convert.ToDecimal(vendaInterna[i][1]);
                                    vendaAnexo.Icms = Convert.ToDecimal(vendaInterna[i][2]);
                                    vendaAnexo.Created = DateTime.Now;
                                    vendaAnexo.Updated = vendaAnexo.Created;
                                    vendaAnexosAdd.Add(vendaAnexo);
                                }
                                else
                                {
                                    venda.Base = Convert.ToDecimal(vendaInterna[i][0]);
                                    venda.Aliquota = Convert.ToDecimal(vendaInterna[i][1]);
                                    venda.Icms = Convert.ToDecimal(vendaInterna[i][2]);
                                    venda.Updated = DateTime.Now;
                                    vendaAnexosUpdate.Add(venda);
                                }
                            }

                            _vendaAnexoService.Create(vendaAnexosAdd);
                            _vendaAnexoService.Update(vendaAnexosUpdate);

                            for (int i = 0; i < devoFornecedorInterna.Count(); i++)
                            {
                                Model.DevoFornecedor devoFornecedor = new Model.DevoFornecedor();
                                var devoForne = devoFornecedors.Where(_ => _.Aliquota.Equals(Convert.ToDecimal(devoFornecedorInterna[i][1]))).FirstOrDefault();

                                if (devoForne == null)
                                {
                                    devoFornecedor.TaxAnexoId = imp.Id;
                                    devoFornecedor.Base = Convert.ToDecimal(devoFornecedorInterna[i][0]);
                                    devoFornecedor.Aliquota = Convert.ToDecimal(devoFornecedorInterna[i][1]);
                                    devoFornecedor.Icms = Convert.ToDecimal(devoFornecedorInterna[i][2]);
                                    devoFornecedor.Created = DateTime.Now;
                                    devoFornecedor.Updated = devoFornecedor.Created;
                                    devoFornecedorsAdd.Add(devoFornecedor);
                                }
                                else
                                {
                                    devoForne.Base = Convert.ToDecimal(devoFornecedorInterna[i][0]);
                                    devoForne.Aliquota = Convert.ToDecimal(devoFornecedorInterna[i][1]);
                                    devoForne.Icms = Convert.ToDecimal(devoFornecedorInterna[i][2]);
                                    devoForne.Updated = DateTime.Now;
                                    devoFornecedorsUpdate.Add(devoForne);
                                }
                            }

                            _devoFornecedorService.Create(devoFornecedorsAdd);
                            _devoFornecedorService.Update(devoFornecedorsUpdate);

                        }
                        else
                        {
                            taxAnexo.BaseVenda4 = baseCalcVendaInterestadual4;
                            taxAnexo.BaseVenda7 = baseCalcVendaInterestadual7;
                            taxAnexo.BaseVenda12 = baseCalcVendaInterestadual12;
                            taxAnexo.IcmsVenda4 = icmsVendaInterestadual4;
                            taxAnexo.IcmsVenda7 = icmsVendaInterestadual7;
                            taxAnexo.IcmsVenda12 = icmsVendaInterestadual12;

                            taxAnexo.BaseDevoFornecedor4 = baseCalcDevoFornecedorInterestadual4;
                            taxAnexo.BaseDevoFornecedor7 = baseCalcDevoFornecedorInterestadual7;
                            taxAnexo.BaseDevoFornecedor12 = baseCalcDevoFornecedorInterestadual12;
                            taxAnexo.IcmsDevoFornecedor4 = icmsDevoFornecedorInterestadual4;
                            taxAnexo.IcmsDevoFornecedor7 = icmsDevoFornecedorInterestadual7;
                            taxAnexo.IcmsDevoFornecedor12 = icmsDevoFornecedorInterestadual12;

                            _service.Create(taxAnexo, null);

                            imp = _service.FindByMonth(companyid, month, year);

                            List<Model.VendaAnexo> vendaAnexos = new List<Model.VendaAnexo>();
                            List<Model.DevoFornecedor> devoFornecedors = new List<Model.DevoFornecedor>();

                            for (int i = 0; i < vendaInterna.Count(); i++)
                            {
                                Model.VendaAnexo vendaAnexo = new Model.VendaAnexo();
                                vendaAnexo.TaxAnexoId = imp.Id;
                                vendaAnexo.Base = Convert.ToDecimal(vendaInterna[i][0]);
                                vendaAnexo.Aliquota = Convert.ToDecimal(vendaInterna[i][1]);
                                vendaAnexo.Icms = Convert.ToDecimal(vendaInterna[i][2]);
                                vendaAnexo.Created = DateTime.Now;
                                vendaAnexo.Updated = vendaAnexo.Created;
                                vendaAnexos.Add(vendaAnexo);
                            }

                            for (int i = 0; i < devoFornecedorInterna.Count(); i++)
                            {
                                Model.DevoFornecedor devoFornecedor = new Model.DevoFornecedor();
                                devoFornecedor.TaxAnexoId = imp.Id;
                                devoFornecedor.Base = Convert.ToDecimal(devoFornecedorInterna[i][0]);
                                devoFornecedor.Aliquota = Convert.ToDecimal(devoFornecedorInterna[i][1]);
                                devoFornecedor.Icms = Convert.ToDecimal(devoFornecedorInterna[i][2]);
                                devoFornecedor.Created = DateTime.Now;
                                devoFornecedor.Updated = devoFornecedor.Created;
                                devoFornecedors.Add(devoFornecedor);
                            }

                            _vendaAnexoService.Create(vendaAnexos);
                            _devoFornecedorService.Create(devoFornecedors);
                        }
                    }
                    
                }

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
