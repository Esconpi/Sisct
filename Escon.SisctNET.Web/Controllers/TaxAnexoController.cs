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
        private readonly IHostingEnvironment _appEnvironment;

        public TaxAnexoController(
            ITaxAnexoService service,
            ICompanyService companyService,
            IConfigurationService configurationService,
            INcmConvenioService ncmConvenioService,
            ICompanyCfopService companyCfopService,
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
                ViewBag.Id = company.Id;
                ViewBag.Company = company.SocialName;
                ViewBag.Document = company.Document;
                ViewBag.Month = month;
                ViewBag.Year = year;

                var result = _service.FindByMonth(id,month,year);
                SessionManager.SetCompanyIdInSession(id);
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
                    exitNotes = importXml.Nfe(directoryNfeExit);
                    entryNotes = importXml.Nfe(directoryNfeEntry);

                    decimal baseCalcCompraInterestadual4 = 0, baseCalcCompraInterestadual7 = 0, baseCalcCompraInterestadual12 = 0,
                       icmsCompraInterestadual4 = 0, icmsCompraInterestadual7 = 0, icmsCompraInterestadual12 = 0,
                       baseCalcVendaInterestadual4 = 0, baseCalcVendaInterestadual7 = 0, baseCalcVendaInterestadual12 = 0,
                       icmsVendaInterestadual4 = 0, icmsVendaInterestadual7 = 0, icmsVendaInterestadual12 = 0,
                       baseCalcDevoFornecedorInterestadual4 = 0, baseCalcDevoFornecedorInterestadual7 = 0, baseCalcDevoFornecedorInterestadual12 = 0,
                       icmsDevoFornecedorInterestadual4 = 0, icmsDevoFornecedorInterestadual7 = 0, icmsDevoFornecedorInterestadual12 = 0,
                       baseCalcDevoClienteInterestadual4 = 0, baseCalcDevoClienteInterestadual12 = 0,
                       icmsDevoClienteInterestadual4 = 0, icmsDevoClienteInterestadual12 = 0,
                       baseCalcTotalA = 0, baseCalcTotalB = 0, icmsTotalA = 0, icmsTotalB = 0;

                    List<List<string>> compraInterna = new List<List<string>>();
                    List<List<string>> devoFornecedorInterna = new List<List<string>>();
                    List<List<string>> vendaInterna = new List<List<string>>();
                    List<List<string>> devoClienteInterna = new List<List<string>>();

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

                            if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && exitNotes[i][1]["finNFe"] != "4" && ncm == false)
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
                                        cc.Add(((Convert.ToDecimal(exitNotes[i][k]["pICMS"]) * Convert.ToDecimal(exitNotes[i][k]["vBC"])) / 100).ToString());
                                        vendaInterna.Add(cc);
                                    }
                                    else
                                    {
                                        vendaInterna[pos][0] = (Convert.ToDecimal(vendaInterna[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                        vendaInterna[pos][2] = (Convert.ToDecimal(vendaInterna[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["pICMS"]) * Convert.ToDecimal(exitNotes[i][k]["vBC"])) / 100)).ToString();
                                    }

                                }
                                else
                                {
                                    if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(4))
                                    {
                                        baseCalcVendaInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                        icmsVendaInterestadual4 += ((Convert.ToDecimal(exitNotes[i][k]["pICMS"]) * Convert.ToDecimal(exitNotes[i][k]["vBC"])) / 100);
                                    }
                                    else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(7))
                                    {
                                        baseCalcVendaInterestadual7 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                        icmsVendaInterestadual7 += ((Convert.ToDecimal(exitNotes[i][k]["pICMS"]) * Convert.ToDecimal(exitNotes[i][k]["vBC"])) / 100);
                                    }
                                    else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(12))
                                    {
                                        baseCalcVendaInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                        icmsVendaInterestadual12 += ((Convert.ToDecimal(exitNotes[i][k]["pICMS"]) * Convert.ToDecimal(exitNotes[i][k]["vBC"])) / 100);
                                    }
                                }
                            }

                        }
                    }

                    // Devolução de Cliente
                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                    {
                        if (exitNotes[i][1]["finNFe"] != "4")
                        {
                            exitNotes.RemoveAt(i);
                            continue;
                        }

                        string CNPJ = exitNotes[i][3].ContainsKey("CNPJ") ? exitNotes[i][3]["CNPJ"] : "";

                        for (int k = 0; k < exitNotes[i].Count(); k++)
                        {
                            if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && CNPJ.Equals(comp.Document))
                            {
                                if (exitNotes[i][1]["idDest"].Equals("1"))
                                {
                                    int pos = -1;
                                    for (int j = 0; j < devoClienteInterna.Count(); j++)
                                    {
                                        if (devoClienteInterna[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                        {
                                            pos = j;
                                        }
                                    }

                                    if (pos < 0)
                                    {
                                        List<string> cc = new List<string>();
                                        cc.Add(exitNotes[i][k]["vBC"]);
                                        cc.Add(exitNotes[i][k]["pICMS"]);
                                        cc.Add(((Convert.ToDecimal(exitNotes[i][k]["pICMS"]) * Convert.ToDecimal(exitNotes[i][k]["vBC"])) / 100).ToString());
                                        devoClienteInterna.Add(cc);
                                    }
                                    else
                                    {
                                        devoClienteInterna[pos][0] = (Convert.ToDecimal(devoClienteInterna[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                        devoClienteInterna[pos][2] = (Convert.ToDecimal(devoClienteInterna[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["pICMS"]) * Convert.ToDecimal(exitNotes[i][k]["vBC"])) / 100)).ToString();
                                    }

                                }
                                else
                                {
                                    if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(4))
                                    {
                                        baseCalcDevoClienteInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                        icmsDevoClienteInterestadual4 += ((Convert.ToDecimal(exitNotes[i][k]["pICMS"]) * Convert.ToDecimal(exitNotes[i][k]["vBC"])) / 100);
                                    }
                                    else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(12))
                                    {
                                        baseCalcDevoClienteInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                        icmsDevoClienteInterestadual12 += ((Convert.ToDecimal(exitNotes[i][k]["pICMS"]) * Convert.ToDecimal(exitNotes[i][k]["vBC"])) / 100);
                                    }
                                }
                            }

                        }
                    }

                    // Devolução a Fornecedor
                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                    {
                        if (exitNotes[i][1]["finNFe"] != "4")
                        {
                            exitNotes.RemoveAt(i);
                            continue;
                        }

                        string CNPJ = exitNotes[i][3].ContainsKey("CNPJ") ? exitNotes[i][3]["CNPJ"] : "";

                        for (int k = 0; k < exitNotes[i].Count(); k++)
                        {
                            if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && !CNPJ.Equals(comp.Document))
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
                                        cc.Add(((Convert.ToDecimal(exitNotes[i][k]["pICMS"]) * Convert.ToDecimal(exitNotes[i][k]["vBC"])) / 100).ToString());
                                        devoFornecedorInterna.Add(cc);
                                    }
                                    else
                                    {
                                        devoFornecedorInterna[pos][0] = (Convert.ToDecimal(devoFornecedorInterna[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                        devoFornecedorInterna[pos][2] = (Convert.ToDecimal(devoFornecedorInterna[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["pICMS"]) * Convert.ToDecimal(exitNotes[i][k]["vBC"])) / 100)).ToString();
                                    }

                                }
                                else
                                {
                                    if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(4))
                                    {
                                        baseCalcDevoFornecedorInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                        icmsDevoFornecedorInterestadual4 += ((Convert.ToDecimal(exitNotes[i][k]["pICMS"]) * Convert.ToDecimal(exitNotes[i][k]["vBC"])) / 100);
                                    }
                                    else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(7))
                                    {
                                        baseCalcDevoFornecedorInterestadual7 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                        icmsDevoFornecedorInterestadual7 += ((Convert.ToDecimal(exitNotes[i][k]["pICMS"]) * Convert.ToDecimal(exitNotes[i][k]["vBC"])) / 100);
                                    }
                                    else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(12))
                                    {
                                        baseCalcDevoFornecedorInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                        icmsDevoFornecedorInterestadual12 += ((Convert.ToDecimal(exitNotes[i][k]["pICMS"]) * Convert.ToDecimal(exitNotes[i][k]["vBC"])) / 100);
                                    }
                                }
                            }

                        }
                    }

                    // Compras
                    for (int i = entryNotes.Count - 1; i >= 0; i--)
                    {
                        if (!entryNotes[i][3]["CNPJ"].Equals(comp.Document))
                        {
                            entryNotes.RemoveAt(i);
                            continue;
                        }

                        bool ncm = false;

                        for (int k = 0; k < entryNotes[i].Count(); k++)
                        {
                            if (entryNotes[i][k].ContainsKey("NCM"))
                            {
                                ncm = false;

                                for (int j = 0; j < ncms.Count(); j++)
                                {
                                    int tamanho = ncms[j].Length;

                                    if (ncms[j].Equals(entryNotes[i][k]["NCM"].Substring(0, tamanho)))
                                    {
                                        ncm = true;
                                        break;
                                    }
                                }
                            }

                            if (entryNotes[i][k].ContainsKey("pICMS") && entryNotes[i][k].ContainsKey("CST") && entryNotes[i][k].ContainsKey("orig") && entryNotes[i][1]["finNFe"] != "4" && ncm == false)
                            {
                                if (entryNotes[i][1]["idDest"].Equals("1"))
                                {
                                    int pos = -1;
                                    for (int j = 0; j < compraInterna.Count(); j++)
                                    {
                                        if (compraInterna[j][1].Equals(entryNotes[i][k]["pICMS"]))
                                        {
                                            pos = j;
                                        }
                                    }

                                    if (pos < 0)
                                    {
                                        List<string> cc = new List<string>();
                                        cc.Add(entryNotes[i][k]["vBC"]);
                                        cc.Add(entryNotes[i][k]["pICMS"]);
                                        cc.Add(((Convert.ToDecimal(entryNotes[i][k]["pICMS"]) * Convert.ToDecimal(entryNotes[i][k]["vBC"])) / 100).ToString());
                                        compraInterna.Add(cc);
                                    }
                                    else
                                    {
                                        compraInterna[pos][0] = (Convert.ToDecimal(compraInterna[pos][0]) + Convert.ToDecimal(entryNotes[i][k]["vBC"])).ToString();
                                        compraInterna[pos][2] = (Convert.ToDecimal(compraInterna[pos][2]) + ((Convert.ToDecimal(entryNotes[i][k]["pICMS"]) * Convert.ToDecimal(entryNotes[i][k]["vBC"])) / 100)).ToString();
                                    }

                                }
                                else
                                {
                                    if (Convert.ToDecimal(entryNotes[i][k]["pICMS"]).Equals(4))
                                    {
                                        baseCalcCompraInterestadual4 += Convert.ToDecimal(entryNotes[i][k]["vBC"]);
                                        icmsCompraInterestadual4 += ((Convert.ToDecimal(entryNotes[i][k]["pICMS"]) * Convert.ToDecimal(entryNotes[i][k]["vBC"])) / 100);
                                    }
                                    else if (Convert.ToDecimal(entryNotes[i][k]["pICMS"]).Equals(7))
                                    {
                                        baseCalcCompraInterestadual7 += Convert.ToDecimal(entryNotes[i][k]["vBC"]);
                                        icmsCompraInterestadual7 += ((Convert.ToDecimal(entryNotes[i][k]["pICMS"]) * Convert.ToDecimal(entryNotes[i][k]["vBC"])) / 100);
                                    }
                                    else if (Convert.ToDecimal(entryNotes[i][k]["pICMS"]).Equals(12))
                                    {
                                        baseCalcCompraInterestadual12 += Convert.ToDecimal(entryNotes[i][k]["vBC"]);
                                        icmsCompraInterestadual12 += ((Convert.ToDecimal(entryNotes[i][k]["pICMS"]) * Convert.ToDecimal(entryNotes[i][k]["vBC"])) / 100);
                                    }
                                }
                            }
                        }

                    }

                    // Devolução de Cliente
                    for (int i = entryNotes.Count - 1; i >= 0; i--)
                    {
                        if (entryNotes[i][1]["finNFe"] != "4")
                        {
                            entryNotes.RemoveAt(i);
                            continue;
                        }

                        for (int k = 0; k < entryNotes[i].Count(); k++)
                        {
                            if (entryNotes[i][k].ContainsKey("pICMS") && entryNotes[i][k].ContainsKey("CST") && entryNotes[i][k].ContainsKey("orig"))
                            {
                                if (entryNotes[i][1]["idDest"].Equals("1"))
                                {
                                    int pos = -1;
                                    for (int j = 0; j < devoClienteInterna.Count(); j++)
                                    {
                                        if (devoClienteInterna[j][1].Equals(entryNotes[i][k]["pICMS"]))
                                        {
                                            pos = j;
                                        }
                                    }

                                    if (pos < 0)
                                    {
                                        List<string> cc = new List<string>();
                                        cc.Add(entryNotes[i][k]["vBC"]);
                                        cc.Add(entryNotes[i][k]["pICMS"]);
                                        cc.Add(((Convert.ToDecimal(entryNotes[i][k]["pICMS"]) * Convert.ToDecimal(entryNotes[i][k]["vBC"])) / 100).ToString());
                                        devoClienteInterna.Add(cc);
                                    }
                                    else
                                    {
                                        devoClienteInterna[pos][0] = (Convert.ToDecimal(devoClienteInterna[pos][0]) + Convert.ToDecimal(entryNotes[i][k]["vBC"])).ToString();
                                        devoClienteInterna[pos][2] = (Convert.ToDecimal(devoClienteInterna[pos][2]) + ((Convert.ToDecimal(entryNotes[i][k]["pICMS"]) * Convert.ToDecimal(entryNotes[i][k]["vBC"])) / 100)).ToString();
                                    }

                                }
                                else
                                {
                                    if (Convert.ToDecimal(entryNotes[i][k]["pICMS"]).Equals(4))
                                    {
                                        baseCalcDevoClienteInterestadual4 += Convert.ToDecimal(entryNotes[i][k]["vBC"]);
                                        icmsDevoClienteInterestadual4 += ((Convert.ToDecimal(entryNotes[i][k]["pICMS"]) * Convert.ToDecimal(entryNotes[i][k]["vBC"])) / 100);
                                    }
                                    else if (Convert.ToDecimal(entryNotes[i][k]["pICMS"]).Equals(12))
                                    {
                                        baseCalcDevoClienteInterestadual12 += Convert.ToDecimal(entryNotes[i][k]["vBC"]);
                                        icmsDevoClienteInterestadual12 += ((Convert.ToDecimal(entryNotes[i][k]["pICMS"]) * Convert.ToDecimal(entryNotes[i][k]["vBC"])) / 100);
                                    }
                                }
                            }
                        }
                    }

                }



                return View(comp);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

    }
}
