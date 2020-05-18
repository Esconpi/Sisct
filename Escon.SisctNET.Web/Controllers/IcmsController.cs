using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Taxation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{
    public class IcmsController : ControllerBaseSisctNET
    {

        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly ICompanyCfopService _companyCfopService;
        private readonly IClientService _clientService;
        private readonly INcmConvenioService _ncmConvenioService;
        private readonly IDarService _darService;
        private readonly IHostingEnvironment _appEnvironment;
        private readonly IProductIncentivoService _productIncentivoService;
        private readonly ICfopService _cfopService;

        public IcmsController(
            ICompanyService companyService,
            IConfigurationService configurationService,
            ICompanyCfopService companyCfopService,
            IClientService clientService,
            INcmConvenioService ncmConvenioService,
            IDarService darService,
            IFunctionalityService functionalityService,
            IProductIncentivoService productIncentivoService,
            ICfopService cfopService,
            IHostingEnvironment env,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "NoteExit")
        {
            _companyService = companyService;
            _configurationService = configurationService;
            _companyCfopService = companyCfopService;
            _clientService = clientService;
            _ncmConvenioService = ncmConvenioService;
            _darService = darService;
            _productIncentivoService = productIncentivoService;
            _cfopService = cfopService;
            _appEnvironment = env;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public async Task<IActionResult> RelatoryExit(int id, string year, string month, string type, IFormFile arquivo)
        {
            try
            {

                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));

                ViewBag.Year = year;
                ViewBag.Month = month;
                ViewBag.SocialName = comp.SocialName;
                ViewBag.Document = comp.Document;
                ViewBag.TypeCompany = comp.TypeCompany;

                var NfeExit = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));
                var NfeEntrada = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                var import = new Import(_companyCfopService);

                string directoryNfeExit = NfeExit.Value + "\\" + comp.Document + "\\" + year + "\\" + month;
                string directoryNfeEntrada = NfeEntrada.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                ViewBag.Type = type;

                if (type.Equals("resumocfop"))
                {
                    
                    List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
                    List<List<string>> cfopsDesordenados = new List<List<string>>();
                    List<int> cfops = new List<int>();

                    notes = import.Nfe(directoryNfeExit);

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        int pos = -1;
                        string cpf = "escon";
                        string cnpj = "escon";
                        string indIEDest = "escon";

                        if (notes[i][3].ContainsKey("CPF"))
                        {
                            cpf = notes[i][3]["CPF"];
                        }

                        if (notes[i][3].ContainsKey("CNPJ"))
                        {
                            cnpj = notes[i][3]["CNPJ"];
                        }


                        if (notes[i][3].ContainsKey("indIEDest"))
                        {
                            indIEDest = notes[i][3]["indIEDest"];
                        }

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("CFOP"))
                            {
                                for (int k = 0; k < cfopsDesordenados.Count(); k++)
                                {
                                    if (cfopsDesordenados[k][0].Equals(notes[i][j]["CFOP"]))
                                    {
                                        pos = k;
                                    }
                                }

                                if(pos < 0)
                                {
                                    var cfp = _cfopService.FindByCode(notes[i][j]["CFOP"]);
                                    List<string> cfop = new List<string>();
                                    cfop.Add(notes[i][j]["CFOP"]);
                                    cfop.Add(cfp.Description);

                                    // Geral
                                    cfop.Add("0");
                                    cfop.Add("0");
                                    cfop.Add("0");
                                    cfop.Add("0");

                                    // Venda com CPF
                                    cfop.Add("0");
                                    cfop.Add("0");
                                    cfop.Add("0");
                                    cfop.Add("0");

                                    // Venda sem CPF
                                    cfop.Add("0");
                                    cfop.Add("0");
                                    cfop.Add("0");
                                    cfop.Add("0");

                                    // Venda com CNPJ com Inscrição Estadual
                                    cfop.Add("0");
                                    cfop.Add("0");
                                    cfop.Add("0");
                                    cfop.Add("0");

                                    // Venda com CNPJ sem Inscrição Estadual
                                    cfop.Add("0");
                                    cfop.Add("0");
                                    cfop.Add("0");
                                    cfop.Add("0");

                                    cfopsDesordenados.Add(cfop);
                                    cfops.Add(Convert.ToInt32(notes[i][j]["CFOP"]));
                                    pos = cfopsDesordenados.Count() - 1;
                                }

                            }

                            // Geral

                            if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                            {
                                cfopsDesordenados[pos][2] = (Convert.ToDecimal(cfopsDesordenados[pos][2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                            }

                            if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                            {
                                cfopsDesordenados[pos][2] = (Convert.ToDecimal(cfopsDesordenados[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                            }

                            if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                            {
                                cfopsDesordenados[pos][2] = (Convert.ToDecimal(cfopsDesordenados[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                            }

                            if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                            {
                                cfopsDesordenados[pos][2] = (Convert.ToDecimal(cfopsDesordenados[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                            }

                            if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                            {
                                cfopsDesordenados[pos][2] = (Convert.ToDecimal(cfopsDesordenados[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                            }

                            if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                            {
                                cfopsDesordenados[pos][3] = (Convert.ToDecimal(cfopsDesordenados[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                            }

                            if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                            {
                                cfopsDesordenados[pos][4] = (Convert.ToDecimal(cfopsDesordenados[pos][4]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                            }

                            if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                            {
                                cfopsDesordenados[pos][5] = (Convert.ToDecimal(cfopsDesordenados[pos][5]) + ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                            }

                            if (cpf != "escon" && cpf != "")
                            {
                                // Com CPF
                                        
                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][6] = (Convert.ToDecimal(cfopsDesordenados[pos][6]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][6] = (Convert.ToDecimal(cfopsDesordenados[pos][6]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][6] = (Convert.ToDecimal(cfopsDesordenados[pos][6]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][6] = (Convert.ToDecimal(cfopsDesordenados[pos][6]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][6] = (Convert.ToDecimal(cfopsDesordenados[pos][6]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsDesordenados[pos][7] = (Convert.ToDecimal(cfopsDesordenados[pos][7]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsDesordenados[pos][8] = (Convert.ToDecimal(cfopsDesordenados[pos][8]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsDesordenados[pos][9] = (Convert.ToDecimal(cfopsDesordenados[pos][9]) + ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                }

                            }
                            else if ((cpf == "escon" || cpf == "") && cnpj == "escon")
                            {
                                // Sem CPF

                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][10] = (Convert.ToDecimal(cfopsDesordenados[pos][10]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][10] = (Convert.ToDecimal(cfopsDesordenados[pos][10]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][10] = (Convert.ToDecimal(cfopsDesordenados[pos][10]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][10] = (Convert.ToDecimal(cfopsDesordenados[pos][10]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][10] = (Convert.ToDecimal(cfopsDesordenados[pos][10]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsDesordenados[pos][11] = (Convert.ToDecimal(cfopsDesordenados[pos][11]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsDesordenados[pos][12] = (Convert.ToDecimal(cfopsDesordenados[pos][12]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsDesordenados[pos][13] = (Convert.ToDecimal(cfopsDesordenados[pos][13]) + ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                }
                            }
                            else if (cnpj != "escon" && cnpj != "" && indIEDest == "1")
                            {
                                // Com CNPJ e com IE

                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][14] = (Convert.ToDecimal(cfopsDesordenados[pos][14]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][14] = (Convert.ToDecimal(cfopsDesordenados[pos][14]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][14] = (Convert.ToDecimal(cfopsDesordenados[pos][14]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][14] = (Convert.ToDecimal(cfopsDesordenados[pos][14]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][14] = (Convert.ToDecimal(cfopsDesordenados[pos][14]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsDesordenados[pos][15] = (Convert.ToDecimal(cfopsDesordenados[pos][15]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsDesordenados[pos][16] = (Convert.ToDecimal(cfopsDesordenados[pos][16]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsDesordenados[pos][17] = (Convert.ToDecimal(cfopsDesordenados[pos][17]) + ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                }
                            }
                            else if (cnpj != "escon" && cnpj != "" && indIEDest != "1")
                            {
                                // Com CNPJ e sem IE
                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][18] = (Convert.ToDecimal(cfopsDesordenados[pos][18]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][18] = (Convert.ToDecimal(cfopsDesordenados[pos][18]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][18] = (Convert.ToDecimal(cfopsDesordenados[pos][18]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][18] = (Convert.ToDecimal(cfopsDesordenados[pos][18]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsDesordenados[pos][18] = (Convert.ToDecimal(cfopsDesordenados[pos][18]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsDesordenados[pos][19] = (Convert.ToDecimal(cfopsDesordenados[pos][19]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsDesordenados[pos][20] = (Convert.ToDecimal(cfopsDesordenados[pos][20]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsDesordenados[pos][21] = (Convert.ToDecimal(cfopsDesordenados[pos][21]) + ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                }
                            }
                                   
                        }

                    }

                    cfops.Sort();

                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                    List<List<string>> cfopsOrdenados = new List<List<string>>();

                    
                    for (int i = 0; i < cfopsDesordenados.Count(); i++)
                    {
                        for(int k = 2;k < 22; k++)
                        {
                            cfopsDesordenados[i][k] = Convert.ToDouble(cfopsDesordenados[i][k].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        }
                    }

                    for(int i = 0; i < cfops.Count(); i++)
                    {
                        int pos = 0;
                        for (int j = 0; i < cfopsDesordenados.Count(); j++)
                        {
                            if (cfops[i] == Convert.ToInt32(cfopsDesordenados[j][0]))
                            {
                                pos = j;
                                break;
                            }
                        }

                        List<string> cc = new List<string>();
                        for(int k = 0; k < 22; k++) { 
                            cc.Add(cfopsDesordenados[pos][k]);
                        }

                        cfopsOrdenados.Add(cc);
                    }

                    ViewBag.Cfop = cfopsOrdenados;
                   
                }
                else if (type.Equals("venda"))
                {
                    List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesTranferencia = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesEntradaDevolucao = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesSaidaDevolucao = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesTransferenciaEntrada = new List<List<Dictionary<string, string>>>();

                    notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                    notesTranferencia = import.NfeExit(directoryNfeExit, id, type, "transferencia");
                    //notesSaidaDevolucao = import.NfeExit(directoryNfeExit, id, type, "devolucao de compra");
                    //notesEntradaDevolucao = import.NfeExit(directoryNfeEntrada, id, type, "devolucao de compra");
                    notesTransferenciaEntrada = import.NotesTransfer(directoryNfeEntrada, id);
                    var cfopstransf = _companyCfopService.FindByCfopActive(id, type, "transferencia").Select(_ => _.Cfop.Code).ToList();

                    decimal totalEntradas = 0, totalTranferenciaInter = 0;
                    for (int i = notesTransferenciaEntrada.Count - 1; i >= 0; i--)
                    {
                        if (!notesTransferenciaEntrada[i][3]["CNPJ"].Equals(comp.Document) && !notesTransferenciaEntrada[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notesTransferenciaEntrada.RemoveAt(i);
                        }
                        else
                        {
                            if (cfopstransf.Contains(notesTransferenciaEntrada[i][4]["CFOP"]) && notesTransferenciaEntrada[i][1]["idDest"].Equals("2"))
                            {
                                totalTranferenciaInter += Convert.ToDecimal(notesTransferenciaEntrada[i][notesTransferenciaEntrada[i].Count - 1]["vNF"]);
                            }
                            totalEntradas += Convert.ToDecimal(notesTransferenciaEntrada[i][notesTransferenciaEntrada[i].Count - 1]["vNF"]);
                        }
                    }

                    for (int i = notesVenda.Count - 1; i >= 0; i--)
                    {
                        if (!notesVenda[i][2]["CNPJ"].Equals(comp.Document) || notesVenda[i].Count <= 5)
                        {
                            notesVenda.RemoveAt(i);
                        }

                    }

                    for (int i = notesTranferencia.Count - 1; i >= 0; i--)
                    {
                        if (!notesTranferencia[i][2]["CNPJ"].Equals(comp.Document) || notesTranferencia[i].Count <= 5)
                        {
                            notesTranferencia.RemoveAt(i);
                        }

                    }

                    var contribuintes = _clientService.FindByContribuinte(id, "all");
                    var contribuintesRaiz = _clientService.FindByContribuinte(id, "raiz");
                    var ncms = _ncmConvenioService.FindByAnnex(Convert.ToInt32(comp.AnnexId));
                    decimal totalVendas = 0, totalNcm = 0, totalTranferencias = 0, totalSaida = 0;
                    int contContribuintes = contribuintes.Count();
                    int contContribuintesRaiz = contribuintesRaiz.Count() + 1;

                    string[,] resumoCnpjs = new string[contContribuintes, 2];
                    string[,] resumoCnpjRaiz = new string[contContribuintesRaiz, 2];
                    string[,] resumoAllCnpjRaiz = new string[contContribuintesRaiz - 1, 2];

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
                        }
                        else
                        {
                            resumoCnpjRaiz[i, 0] = "Não contribuinte";
                            resumoCnpjRaiz[i, 1] = "0";
                        }
                    }

                    for (int i = 0; i < notesVenda.Count(); i++)
                    {
                        int posClienteRaiz = contContribuintesRaiz - 1, posCliente = -1;

                        bool status = false;

                        if (notesVenda[i][3].ContainsKey("CNPJ"))
                        {
                            if (contribuintesRaiz.Contains(notesVenda[i][3]["CNPJ"].Substring(0, 8)))
                            {
                                posClienteRaiz = contribuintesRaiz.IndexOf(notesVenda[i][3]["CNPJ"].Substring(0, 8));
                            }

                            if (contribuintes.Contains(notesVenda[i][3]["CNPJ"]))
                            {
                                posCliente = contribuintes.IndexOf(notesVenda[i][3]["CNPJ"]);
                            }

                        }

                        for (int k = 0; k < notesVenda[i].Count(); k++)
                        {

                            if (notesVenda[i][k].ContainsKey("NCM"))
                            {
                                status = false;

                                for (int j = 0; j < ncms.Count(); j++)
                                {
                                    int tamanho = ncms[j].Length;

                                    if (ncms[j].Equals(notesVenda[i][k]["NCM"].Substring(0, tamanho)))
                                    {
                                        status = true;
                                        break;
                                    }
                                }
                            }

                            if (status == true)
                            {

                                if (notesVenda[i][k].ContainsKey("vProd") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                    totalNcm += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vFrete") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                    totalNcm += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vDesc") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                    totalNcm -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vOutro") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                    totalNcm += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vSeg") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                    totalNcm += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();

                                    }
                                }
                            }
                            else
                            {

                                if (notesVenda[i][k].ContainsKey("vProd") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vFrete") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vDesc") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vOutro") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vSeg") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();

                                    }
                                }
                            }

                        }
                    }


                    for (int i = 0; i < notesTranferencia.Count(); i++)
                    {
                        int posClienteRaiz = contContribuintesRaiz - 1;

                        if (notesTranferencia[i][3].ContainsKey("CNPJ"))
                        {
                            if (contribuintesRaiz.Contains(notesTranferencia[i][3]["CNPJ"].Substring(0, 8)))
                            {
                                posClienteRaiz = contribuintesRaiz.IndexOf(notesTranferencia[i][3]["CNPJ"].Substring(0, 8));
                            }
                        }

                        for (int j = 0; j < notesTranferencia[i].Count; j++)
                        {
                            if (notesTranferencia[i][j].ContainsKey("vProd") && notesTranferencia[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias += Convert.ToDecimal(notesTranferencia[i][j]["vProd"]);
                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                {
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesTranferencia[i][j]["vProd"])).ToString();
                                }
                            }

                            if (notesTranferencia[i][j].ContainsKey("vFrete") && notesTranferencia[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias += Convert.ToDecimal(notesTranferencia[i][j]["vFrete"]);
                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                {
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesTranferencia[i][j]["vFrete"])).ToString();

                                }
                            }
                            if (notesTranferencia[i][j].ContainsKey("vDesc") && notesTranferencia[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias -= Convert.ToDecimal(notesTranferencia[i][j]["vDesc"]);
                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                {
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesTranferencia[i][j]["vDesc"])).ToString();

                                }
                            }

                            if (notesTranferencia[i][j].ContainsKey("vOutro") && notesTranferencia[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias += Convert.ToDecimal(notesTranferencia[i][j]["vOutro"]);
                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                {
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesTranferencia[i][j]["vOutro"])).ToString();

                                }
                            }

                            if (notesTranferencia[i][j].ContainsKey("vSeg") && notesTranferencia[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias += Convert.ToDecimal(notesTranferencia[i][j]["vSeg"]);
                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                {
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesTranferencia[i][j]["vSeg"])).ToString();

                                }
                            }
                        }
                    }

                    decimal totalNcontribuinte = Convert.ToDecimal(resumoCnpjRaiz[contContribuintesRaiz - 1, 1]);
                    decimal totalContribuinte = totalVendas - totalNcontribuinte;
                    totalSaida = totalVendas + totalTranferencias;
                    decimal limiteContribuinte = (totalVendas * Convert.ToDecimal(comp.VendaContribuinte)) / 100,
                        limiteNcm = (totalVendas * Convert.ToDecimal(comp.VendaAnexo)) / 100,
                        limiteGrupo = (totalSaida * Convert.ToDecimal(comp.VendaMGrupo)) / 100,
                        limiteTransferencia = (totalEntradas * Convert.ToDecimal(comp.TransferenciaInterExcedente)) / 100;

                    decimal impostoContribuinte = 0, excedenteContribuinte = 0, excedenteNcm = 0, impostoNcm = 0, excedenteTranfInter = 0, impostoTransfInter = 0;


                    //CNPJ
                    List<List<string>> gruposExecentes = new List<List<string>>();
                    decimal totalExcedente = 0, totalImpostoGrupo = 0;
                    for (int i = 0; i < contContribuintesRaiz - 1; i++)
                    {
                        var totalVendaGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 1]);
                        if (totalVendaGrupo > limiteGrupo)
                        {
                            List<string> grupoExcedente = new List<string>();
                            var cnpjGrupo = resumoAllCnpjRaiz[i, 0];
                            var clientGrupo = _clientService.FindByRaiz(cnpjGrupo);
                            var nomeGrupo = clientGrupo.Name;
                            var percentualGrupo = Math.Round((totalVendaGrupo / totalSaida) * 100, 2);
                            var excedenteGrupo = totalVendaGrupo - limiteGrupo;
                            var impostoGrupo = (excedenteGrupo * Convert.ToDecimal(comp.VendaMGrupoExcedente)) / 100;
                            totalExcedente += totalVendaGrupo;
                            totalImpostoGrupo += impostoGrupo;
                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");
                            grupoExcedente.Add(cnpjGrupo);
                            grupoExcedente.Add(nomeGrupo);
                            grupoExcedente.Add(percentualGrupo.ToString());
                            grupoExcedente.Add(Math.Round(Convert.ToDouble(totalVendaGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                            grupoExcedente.Add(Math.Round(Convert.ToDouble(limiteGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                            grupoExcedente.Add(Math.Round(Convert.ToDouble(excedenteGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                            grupoExcedente.Add(comp.VendaMGrupoExcedente.ToString());
                            grupoExcedente.Add(Math.Round(Convert.ToDouble(impostoGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                            gruposExecentes.Add(grupoExcedente);
                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                        }
                    }


                    //Anexo II
                    if (totalNcm < limiteNcm)
                    {
                        excedenteNcm = limiteNcm - totalNcm;
                        impostoNcm = (excedenteNcm * Convert.ToDecimal(comp.VendaAnexoExcedente)) / 100;
                    }

                    //Contribuinte
                    if (totalContribuinte < limiteContribuinte)
                    {
                        excedenteContribuinte = limiteContribuinte - totalContribuinte;
                        impostoContribuinte = (excedenteContribuinte * Convert.ToDecimal(comp.VendaContribuinteExcedente)) / 100;
                    }

                    // Transferência inter
                    if (limiteTransferencia < totalTranferenciaInter)
                    {
                        excedenteTranfInter = totalTranferenciaInter - limiteTransferencia;
                        impostoTransfInter = (excedenteTranfInter * Convert.ToDecimal(comp.TransferenciaInterExcedente)) / 100;
                    }

                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                    //Geral
                    ViewBag.Contribuinte = Math.Round(Convert.ToDouble(totalContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.NContribuinte = Math.Round(Convert.ToDouble(totalNcontribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalSaida = Math.Round(Convert.ToDouble(totalSaida.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.VendaAnexo = Math.Round(Convert.ToDouble(totalNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalTransferencia = Math.Round(Convert.ToDouble(totalTranferenciaInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //CNPJ
                    ViewBag.PercentualCNPJ = comp.VendaMGrupo;
                    ViewBag.TotalExecedente = Math.Round(Convert.ToDouble(totalExcedente.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.Grupo = gruposExecentes;

                    //Anexo II
                    ViewBag.LimiteAnexo = Math.Round(Convert.ToDouble(limiteNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ExcedenteAnexo = Math.Round(Convert.ToDouble(excedenteNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualExcedenteAnexo = comp.VendaAnexoExcedente;
                    ViewBag.TotalExcedenteAnexo = Math.Round(Convert.ToDouble(impostoNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //Contribuinte
                    ViewBag.LimiteContribuinte = Math.Round(Convert.ToDouble(limiteContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ExcedenteContribuinte = Math.Round(Convert.ToDouble(excedenteContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualExcedenteContribuinte = comp.VendaContribuinteExcedente;
                    ViewBag.TotalExcedenteContribuinte = Math.Round(Convert.ToDouble(impostoContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //Tranferência
                    ViewBag.LimiteTransferencia = Math.Round(Convert.ToDouble(limiteTransferencia.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ExcedenteTransferencia = Math.Round(Convert.ToDouble(excedenteTranfInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentaulTransferencia = comp.TransferenciaInterExcedente;
                    ViewBag.TotalExcedenteTransferencia = Math.Round(Convert.ToDouble(impostoTransfInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //Total Icms
                    ViewBag.TotalIcms = Math.Round(Convert.ToDouble((impostoNcm + impostoContribuinte + impostoTransfInter + totalImpostoGrupo).ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    //Dar
                    var dar = _darService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Type.Equals("Icms")).FirstOrDefault();
                    ViewBag.Dar = dar.Code;
                }
                else if (type.Equals("anexo"))
                {
                    List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                    notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                    var ncms = _ncmConvenioService.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId));

                    var contNcm = ncms.Count();
                    string[,] resumoAnexo = new string[contNcm, 4];

                    for (int i = 0; i < contNcm; i++)
                    {
                        resumoAnexo[i, 0] = ncms[i].Ncm;
                        resumoAnexo[i, 1] = ncms[i].Description;
                        resumoAnexo[i, 2] = "0";
                        resumoAnexo[i, 3] = "0";
                    }

                    for (int i = notesVenda.Count - 1; i >= 0; i--)
                    {
                        if (!notesVenda[i][2]["CNPJ"].Equals(comp.Document) || notesVenda[i].Count <= 5)
                        {
                            notesVenda.RemoveAt(i);
                        }

                    }

                    decimal totalVendas = 0;
                    bool status = false;

                    for (int i = 0; i < notesVenda.Count(); i++)
                    {
                        int pos = 0;
                        for (int j = 0; j < notesVenda[i].Count(); j++)
                        {
                            if (notesVenda[i][j].ContainsKey("NCM"))
                            {
                                status = false;

                                for (int k = 0; k < ncms.Count; k++)
                                {
                                    int tamanho = ncms[k].Ncm.Length;
                                    if (ncms[k].Ncm.Equals(notesVenda[i][j]["NCM"].Substring(0, tamanho)))
                                    {
                                        status = true;
                                        pos = k;
                                        break;
                                    }
                                }
                            }

                            if (status == true)
                            {
                                if (notesVenda[i][j].ContainsKey("vProd") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vProd"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) + Convert.ToDecimal(notesVenda[i][j]["vProd"])).ToString();

                                }

                                if (notesVenda[i][j].ContainsKey("vFrete") && notesVenda[i][j].ContainsKey("cProd"))
                                {

                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vFrete"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) + Convert.ToDecimal(notesVenda[i][j]["vFrete"])).ToString();

                                }

                                if (notesVenda[i][j].ContainsKey("vDesc") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas -= Convert.ToDecimal(notesVenda[i][j]["vDesc"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) - Convert.ToDecimal(notesVenda[i][j]["vDesc"])).ToString();

                                }

                                if (notesVenda[i][j].ContainsKey("vOutro") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vOutro"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) + Convert.ToDecimal(notesVenda[i][j]["vOutro"])).ToString();

                                }

                                if (notesVenda[i][j].ContainsKey("vSeg") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vSeg"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) + Convert.ToDecimal(notesVenda[i][j]["vSeg"])).ToString();
                                }
                            }
                            else
                            {
                                if (notesVenda[i][j].ContainsKey("vProd") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vProd"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vFrete") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vFrete"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vDesc") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas -= Convert.ToDecimal(notesVenda[i][j]["vDesc"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vOutro") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vOutro"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vSeg") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vSeg"]);
                                }
                            }
                        }

                    }

                    decimal percentualTotal = 0;
                    decimal valorTotalNcm = 0;
                    for (int i = 0; i < contNcm; i++)
                    {
                        if (resumoAnexo[i, 2] != "0")
                        {
                            resumoAnexo[i, 3] = ((Convert.ToDecimal(resumoAnexo[i, 2]) * 100) / totalVendas).ToString();
                            valorTotalNcm += Convert.ToDecimal(resumoAnexo[i, 2]);
                            percentualTotal += Convert.ToDecimal(resumoAnexo[i, 3]);
                        }
                    }
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                    List<List<string>> ncm_list = new List<List<string>>();

                    for (int i = 0; i < contNcm; i++)
                    {
                        if (resumoAnexo[i, 2] != "0")
                        {
                            List<string> ncm = new List<string>();
                            ncm.Add(resumoAnexo[i, 0]);
                            ncm.Add(resumoAnexo[i, 1]);

                            ncm.Add(Convert.ToDouble(resumoAnexo[i, 2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            ncm.Add(Convert.ToDouble(resumoAnexo[i, 3].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            ncm_list.Add(ncm);
                        }
                    }

                    ViewBag.TotalNcm = valorTotalNcm.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualTotal = percentualTotal.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalVendas = totalVendas.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""); ;
                    ViewBag.ResumoNcm = ncm_list;

                }
                else if (type.Equals("foraAnexo"))
                {
                    List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                    notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                    var ncms = _ncmConvenioService.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId));

                    for (int i = notesVenda.Count - 1; i >= 0; i--)
                    {
                        if (!notesVenda[i][2]["CNPJ"].Equals(comp.Document) || notesVenda[i].Count <= 5)
                        {
                            notesVenda.RemoveAt(i);
                        }

                    }

                    List<List<string>> ncmsForaAnexo = new List<List<string>>();
                    bool status = false;
                    decimal totalVendas = 0;
                    for (int i = 0; i < notesVenda.Count(); i++)
                    {
                        string ncm = "";
                        for (int j = 0; j < notesVenda[i].Count(); j++)
                        {
                            if (notesVenda[i][j].ContainsKey("NCM"))
                            {
                                status = false;
                                ncm = "";
                                for (int k = 0; k < ncms.Count; k++)
                                {
                                    int tamanho = ncms[k].Ncm.Length;
                                    if (ncms[k].Ncm.Equals(notesVenda[i][j]["NCM"].Substring(0, tamanho)))
                                    {
                                        status = true;
                                        break;
                                    }
                                }

                                if (status == false)
                                {
                                    ncm = notesVenda[i][j]["NCM"];
                                }
                            }

                            if (status == false)
                            {
                                List<string> ncmForaAnexo = new List<string>();
                                int pos = -1;
                                for (int k = 0; k < ncmsForaAnexo.Count(); k++)
                                {
                                    if (ncmsForaAnexo[k][0].Equals(ncm))
                                    {
                                        pos = k;
                                    }
                                }

                                if (pos < 0)
                                {
                                    ncmForaAnexo.Add(ncm);
                                    ncmForaAnexo.Add("0");
                                    ncmForaAnexo.Add("0");
                                    ncmsForaAnexo.Add(ncmForaAnexo);
                                    var x = ncmsForaAnexo.IndexOf(ncmForaAnexo);
                                    pos = ncmsForaAnexo.Count() - 1;
                                }
                                if (notesVenda[i][j].ContainsKey("vProd") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) + Convert.ToDecimal(notesVenda[i][j]["vProd"])).ToString();
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vProd"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vFrete") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) + Convert.ToDecimal(notesVenda[i][j]["vFrete"])).ToString();
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vFrete"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vDesc") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) - Convert.ToDecimal(notesVenda[i][j]["vDesc"])).ToString();
                                    totalVendas -= Convert.ToDecimal(notesVenda[i][j]["vDesc"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vOutro") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) + Convert.ToDecimal(notesVenda[i][j]["vOutro"])).ToString();
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vOutro"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vSeg") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) + Convert.ToDecimal(notesVenda[i][j]["vSeg"])).ToString();
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vSeg"]);
                                }
                            }
                            else
                            {
                                if (notesVenda[i][j].ContainsKey("vProd") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vProd"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vFrete") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vFrete"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vDesc") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas -= Convert.ToDecimal(notesVenda[i][j]["vDesc"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vOutro") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vOutro"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vSeg") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vSeg"]);
                                }
                            }

                        }
                    }

                    decimal valorTotalNcm = 0, percentualTotal = 0;

                    foreach (var ncm in ncmsForaAnexo)
                    {
                        if (!ncm[1].Equals("0"))
                        {
                            ncm[2] = ((Convert.ToDecimal(ncm[1]) * 100) / totalVendas).ToString();
                            valorTotalNcm += Convert.ToDecimal(ncm[1]);
                            percentualTotal += Convert.ToDecimal(ncm[2]);

                        }
                    }

                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");
                    List<List<string>> ncmsForaAnexoList = new List<List<string>>();

                    foreach (var ncm in ncmsForaAnexo)
                    {
                        if (!ncm[1].Equals("0"))
                        {
                            List<string> ncmFora = new List<string>();
                            ncmFora.Add(ncm[0]);
                            ncmFora.Add(Convert.ToDouble(ncm[1].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            ncmFora.Add(Convert.ToDouble(ncm[2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            ncmsForaAnexoList.Add(ncmFora);

                        }
                    }

                    ViewBag.Ncm = ncmsForaAnexoList;
                    ViewBag.PercentualTotal = percentualTotal.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalVendas = totalVendas.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalNcm = valorTotalNcm.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                }
                else if (type.Equals("incentivo"))
                {

                    var darIcms = _darService.FindAll(null).Where(_ => _.Type.Equals("Icms")).FirstOrDefault();
                    var darFunef = _darService.FindAll(null).Where(_ => _.Type.Equals("Funef")).FirstOrDefault();
                    var darCotac = _darService.FindAll(null).Where(_ => _.Type.Equals("Cotac")).FirstOrDefault();

                    if (comp.TypeCompany.Equals(true))
                    {
                        var productincentivo = _productIncentivoService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.CompanyId.Equals(id)).ToList();
                        var codeProdIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Code).ToList();
                        var codeProdST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Code).ToList();

                        if (arquivo == null || arquivo.Length == 0)
                        {
                            ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                            return View(ViewData);
                        }

                        string nomeArquivo = comp.Document + year + month;

                        if (arquivo.FileName.Contains(".txt"))
                            nomeArquivo += ".txt";
                        else
                            nomeArquivo += ".tmp";

                        string caminho_WebRoot = _appEnvironment.WebRootPath;
                        string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Speds\\";
                        string caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                        string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivo);
                        if (System.IO.File.Exists(caminhoDestinoArquivoOriginal))
                        {
                            System.IO.File.Delete(caminhoDestinoArquivoOriginal);
                        }
                        var stream = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);
                        await arquivo.CopyToAsync(stream);
                        stream.Close();
                        decimal creditosIcms = import.SpedCredito(caminhoDestinoArquivoOriginal, comp.Id),
                             debitosIcms = 0;

                        List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                        List<List<Dictionary<string, string>>> notesVendaSt = new List<List<Dictionary<string, string>>>();
                       // List<List<Dictionary<string, string>>> notesSaidaDevoVenda = new List<List<Dictionary<string, string>>>();
                        List<List<Dictionary<string, string>>> notesSaidaDevoCompra = new List<List<Dictionary<string, string>>>();
                        List<List<string>> icmsForaDoEstado = new List<List<string>>();

                        var contribuintes = _clientService.FindByContribuinte(id, "all");

                        notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                        notesVendaSt = import.NfeExit(directoryNfeExit, id, type, "vendaSt");
                        //notesSaidaDevoVenda = import.NfeExit(directoryNfeExit, id, type, "devolução de venda");
                        notesSaidaDevoCompra = import.NfeExit(directoryNfeExit, id, type, "devolucao de compra");

                        decimal totalVendas = 0, naoContriForaDoEstadoIncentivo = 0, ContribuintesIncentivo = 0,
                            naoContribuinteIncentivo = 0, naoContriForaDoEstadoNIncentivo = 0,
                            ContribuintesNIncentivo = 0, naoContribuinteNIncetivo = 0;

                        for (int i = notesVenda.Count - 1; i >= 0; i--)
                        {
                            if (notesVenda[i].Count <= 5)
                            {
                                notesVenda.RemoveAt(i);
                                continue;
                            }

                            int posCliente = -1;

                            if (notesVenda[i][3].ContainsKey("CNPJ"))
                            {
                                if (contribuintes.Contains(notesVenda[i][3]["CNPJ"]))
                                {
                                    posCliente = contribuintes.IndexOf(notesVenda[i][3]["CNPJ"]);
                                }
                            }

                            int posUf = -1;
                            if (notesVenda[i][3].ContainsKey("UF") && notesVenda[i][1]["idDest"].Equals("2"))
                            {

                                for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                {
                                    if (icmsForaDoEstado[j][0].Equals(notesVenda[i][3]["UF"]))
                                    {
                                        posUf = j;
                                    }
                                }

                                if (posUf < 0)
                                {
                                    List<string> uf = new List<string>();
                                    uf.Add(notesVenda[i][3]["UF"]);
                                    uf.Add("0,00");
                                    uf.Add("0,00");
                                    icmsForaDoEstado.Add(uf);
                                }

                            }

                            for (int k = 0; k < notesVenda[i].Count(); k++)
                            {
                                if (notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    if (codeProdIncentivado.Contains(notesVenda[i][k]["cProd"]))
                                    {
                                        if (notesVenda[i][k].ContainsKey("vProd"))
                                        {
                                            if (posCliente < 0)
                                            {
                                                naoContribuinteIncentivo += Convert.ToDecimal(notesVenda[i][k]["vProd"]);

                                                if (notesVenda[i][1]["idDest"].Equals("2"))
                                                {
                                                    naoContriForaDoEstadoIncentivo += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                                    icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                                }
                                            }
                                            else
                                            {
                                                ContribuintesIncentivo += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                            }
                                            totalVendas += Convert.ToDecimal(notesVenda[i][k]["vProd"]);

                                        }

                                        if (notesVenda[i][k].ContainsKey("vFrete"))
                                        {
                                            if (posCliente < 0)
                                            {
                                                naoContribuinteIncentivo += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);

                                                if (notesVenda[i][1]["idDest"].Equals("2"))
                                                {
                                                    naoContriForaDoEstadoIncentivo += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                                    icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                                }

                                            }
                                            else
                                            {
                                                ContribuintesIncentivo += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                            }
                                            totalVendas += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);

                                        }

                                        if (notesVenda[i][k].ContainsKey("vDesc"))
                                        {
                                            if (posCliente < 0)
                                            {
                                                naoContribuinteIncentivo -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);

                                                if (notesVenda[i][1]["idDest"].Equals("2"))
                                                {
                                                    naoContriForaDoEstadoIncentivo -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                                    icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                                }

                                            }
                                            else
                                            {
                                                ContribuintesIncentivo -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                            }
                                            totalVendas -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);

                                        }

                                        if (notesVenda[i][k].ContainsKey("vOutro"))
                                        {
                                            if (posCliente < 0)
                                            {
                                                naoContribuinteIncentivo += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);

                                                if (notesVenda[i][1]["idDest"].Equals("2"))
                                                {
                                                    naoContriForaDoEstadoIncentivo += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                                    icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                                }
                                            }
                                            else
                                            {
                                                ContribuintesIncentivo += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                            }
                                            totalVendas += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);

                                        }

                                        if (notesVenda[i][k].ContainsKey("vSeg"))
                                        {
                                            if (posCliente < 0)
                                            {
                                                naoContribuinteIncentivo += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);

                                                if (notesVenda[i][1]["idDest"].Equals("2"))
                                                {
                                                    naoContriForaDoEstadoIncentivo += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                                    icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                                }

                                            }
                                            else
                                            {
                                                ContribuintesIncentivo += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                            }
                                            totalVendas += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);

                                        }
                                    }
                                    else if (codeProdST.Contains(notesVenda[i][k]["cProd"]))
                                    {
                                        if (notesVenda[i][k].ContainsKey("vProd"))
                                        {
                                            if (posCliente < 0)
                                            {
                                                naoContribuinteNIncetivo += Convert.ToDecimal(notesVenda[i][k]["vProd"]);

                                                if (notesVenda[i][1]["idDest"].Equals("2"))
                                                {
                                                    naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                                    icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                                }

                                            }
                                            else
                                            {
                                                ContribuintesNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                            }
                                            totalVendas += Convert.ToDecimal(notesVenda[i][k]["vProd"]);

                                        }

                                        if (notesVenda[i][k].ContainsKey("vFrete"))
                                        {
                                            if (posCliente < 0)
                                            {
                                                naoContribuinteNIncetivo += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);

                                                if (notesVenda[i][1]["idDest"].Equals("2"))
                                                {
                                                    naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                                    icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                                }
                                            }
                                            else
                                            {
                                                ContribuintesNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                            }
                                            totalVendas += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);

                                        }

                                        if (notesVenda[i][k].ContainsKey("vDesc"))
                                        {
                                            if (posCliente < 0)
                                            {
                                                naoContribuinteNIncetivo += Convert.ToDecimal(notesVenda[i][k]["vDesc"]);

                                                if (notesVenda[i][1]["idDest"].Equals("2"))
                                                {
                                                    naoContriForaDoEstadoNIncentivo -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                                    icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                                }
                                            }
                                            else
                                            {
                                                ContribuintesNIncentivo -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                            }
                                            totalVendas -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);

                                        }

                                        if (notesVenda[i][k].ContainsKey("vOutro"))
                                        {
                                            if (posCliente < 0)
                                            {
                                                naoContribuinteNIncetivo += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);

                                                if (notesVenda[i][1]["idDest"].Equals("2"))
                                                {
                                                    naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                                    icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                                }
                                            }
                                            else
                                            {
                                                ContribuintesNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                            }
                                            totalVendas += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);

                                        }

                                        if (notesVenda[i][k].ContainsKey("vSeg"))
                                        {
                                            if (posCliente < 0)
                                            {
                                                naoContribuinteNIncetivo += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);

                                                if (notesVenda[i][1]["idDest"].Equals("2"))
                                                {
                                                    naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                                    icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                                }
                                            }
                                            else
                                            {
                                                ContribuintesNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                            }
                                            totalVendas += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);

                                        }
                                    }

                                }
                                if (notesVenda[i][k].ContainsKey("pICMS") && notesVenda[i][k].ContainsKey("CST") && notesVenda[i][k].ContainsKey("orig"))
                                {
                                    debitosIcms += (Convert.ToDecimal(notesVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100;
                                    //debitosIcms += Convert.ToDecimal(notesVenda[i][k]["vICMS"]);
                                }

                                if (notesVenda[i][k].ContainsKey("pFCP") && notesVenda[i][k].ContainsKey("CST") && notesVenda[i][k].ContainsKey("orig"))
                                {
                                    debitosIcms += (Convert.ToDecimal(notesVenda[i][k]["pFCP"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100;
                                }
                            }

                        }

                        for (int i = notesVendaSt.Count - 1; i >= 0; i--)
                        {
                            if ((notesVendaSt[i].Count <= 5))
                            {
                                notesVendaSt.RemoveAt(i);
                                continue;
                            }
                            else if (notesVendaSt[i][3].ContainsKey("CNPJ"))
                            {
                                if (contribuintes.Contains(notesVendaSt[i][3]["CNPJ"]))
                                {
                                    notesVendaSt.RemoveAt(i);
                                }
                                continue;
                            }

                            int posUf = -1;
                            if (notesVendaSt[i][3].ContainsKey("UF") && notesVendaSt[i][1]["idDest"].Equals("2"))
                            {

                                for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                {
                                    if (icmsForaDoEstado[j][0].Equals(notesVendaSt[i][3]["UF"]))
                                    {
                                        posUf = j;
                                    }
                                }

                                if (posUf < 0)
                                {
                                    List<string> uf = new List<string>();
                                    uf.Add(notesVendaSt[i][3]["UF"]);
                                    uf.Add("0");
                                    uf.Add("0");
                                    icmsForaDoEstado.Add(uf);
                                }

                            }

                            for (int k = 0; k < notesVendaSt[i].Count(); k++)
                            {
                                if (notesVendaSt[i][k].ContainsKey("vProd") && notesVendaSt[i][k].ContainsKey("cProd"))
                                {
                                    naoContribuinteNIncetivo += Convert.ToDecimal(notesVendaSt[i][k]["vProd"]);
                                    if (notesVendaSt[i][1]["idDest"].Equals("2"))
                                    {
                                        naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(notesVendaSt[i][k]["vProd"]);
                                        icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(notesVendaSt[i][k]["vProd"])).ToString();

                                    }

                                    totalVendas += Convert.ToDecimal(notesVendaSt[i][k]["vProd"]);

                                }

                                if (notesVendaSt[i][k].ContainsKey("vFrete") && notesVendaSt[i][k].ContainsKey("cProd"))
                                {
                                    naoContribuinteNIncetivo += Convert.ToDecimal(notesVendaSt[i][k]["vFrete"]);
                                    if (notesVendaSt[i][1]["idDest"].Equals("2"))
                                    {
                                        naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(notesVendaSt[i][k]["vFrete"]);
                                        icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(notesVendaSt[i][k]["vFrete"])).ToString();

                                    }

                                    totalVendas += Convert.ToDecimal(notesVendaSt[i][k]["vFrete"]);

                                }

                                if (notesVendaSt[i][k].ContainsKey("vDesc") && notesVendaSt[i][k].ContainsKey("cProd"))
                                {
                                    naoContribuinteNIncetivo -= Convert.ToDecimal(notesVendaSt[i][k]["vDesc"]);
                                    if (notesVendaSt[i][1]["idDest"].Equals("2"))
                                    {
                                        naoContriForaDoEstadoNIncentivo -= Convert.ToDecimal(notesVendaSt[i][k]["vDesc"]);
                                        icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) - Convert.ToDecimal(notesVendaSt[i][k]["vDesc"])).ToString();

                                    }


                                    totalVendas -= Convert.ToDecimal(notesVendaSt[i][k]["vDesc"]);

                                }

                                if (notesVendaSt[i][k].ContainsKey("vOutro") && notesVendaSt[i][k].ContainsKey("cProd"))
                                {
                                    naoContribuinteNIncetivo += Convert.ToDecimal(notesVendaSt[i][k]["vOutro"]);
                                    if (notesVendaSt[i][1]["idDest"].Equals("2"))
                                    {
                                        naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(notesVendaSt[i][k]["Outro"]);
                                        icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(notesVendaSt[i][k]["vOutro"])).ToString();

                                    }


                                    totalVendas += Convert.ToDecimal(notesVendaSt[i][k]["vOutro"]);

                                }

                                if (notesVendaSt[i][k].ContainsKey("vSeg") && notesVendaSt[i][k].ContainsKey("cProd"))
                                {
                                    naoContribuinteNIncetivo += Convert.ToDecimal(notesVendaSt[i][k]["vSeg"]);
                                    if (notesVendaSt[i][1]["idDest"].Equals("2"))
                                    {
                                        naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(notesVendaSt[i][k]["vSeg"]);
                                        icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(notesVendaSt[i][k]["vSeg"])).ToString();

                                    }


                                    totalVendas += Convert.ToDecimal(notesVendaSt[i][k]["vSeg"]);
                                }

                            }


                        }

                        /*for (int i = notesSaidaDevoVenda.Count - 1; i >= 0; i--)
                        {
                            if (notesSaidaDevoVenda[i][2].ContainsKey("CNPJ"))
                            {
                                if (notesSaidaDevoVenda[i].Count <= 5)
                                {
                                    notesSaidaDevoVenda.RemoveAt(i);
                                    continue;
                                }
                            }
                            for (int k = 0; k < notesSaidaDevoVenda[i].Count; k++)
                            {
                                if (notesSaidaDevoVenda[i][k].ContainsKey("pICMS") && notesSaidaDevoVenda[i][k].ContainsKey("CST") && notesSaidaDevoVenda[i][k].ContainsKey("orig"))
                                {
                                    creditosIcms += (Convert.ToDecimal(notesSaidaDevoVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesSaidaDevoVenda[i][k]["vBC"])) / 100;
                                }
                            }
                        }*/

                        for (int i = notesSaidaDevoCompra.Count - 1; i >= 0; i--)
                        {
                            if (notesSaidaDevoCompra[i][2].ContainsKey("CNPJ"))
                            {
                                if (notesSaidaDevoCompra[i].Count <= 5)
                                {
                                    notesSaidaDevoCompra.RemoveAt(i);
                                    continue;
                                }
                            }
                            for (int k = 0; k < notesSaidaDevoCompra[i].Count; k++)
                            {
                                if (notesSaidaDevoCompra[i][k].ContainsKey("pICMS") && notesSaidaDevoCompra[i][k].ContainsKey("CST") && notesSaidaDevoCompra[i][k].ContainsKey("orig"))
                                {
                                    debitosIcms += (Convert.ToDecimal(notesSaidaDevoCompra[i][k]["pICMS"]) * Convert.ToDecimal(notesSaidaDevoCompra[i][k]["vBC"])) / 100;
                                }
                            }
                        }

                        //// Cálculos dos Produtos  Incentivados

                        //Contribuinte
                        var icmsContribuinteIncentivo = Math.Round(Convert.ToDecimal(comp.Icms) * ContribuintesIncentivo / 100, 2);

                        //Não Contribuinte
                        var totalVendasNContribuinte = Math.Round(naoContribuinteIncentivo + naoContribuinteNIncetivo);
                        var icmsNContribuinteIncentivo = Math.Round(Convert.ToDecimal(comp.IcmsNContribuinte) * totalVendasNContribuinte / 100, 2);

                        //Não Contribuinte Fora do Estado
                        var totalVendasNContribuinteForaDoEstado = Math.Round(naoContriForaDoEstadoIncentivo + naoContriForaDoEstadoNIncentivo, 2);
                        var icmsNContribuinteForaDoEstado = Math.Round(Convert.ToDecimal(comp.IcmsNContribuinteFora) * totalVendasNContribuinteForaDoEstado / 100, 2);

                        for(int j = 0; j < icmsForaDoEstado.Count(); j++)
                        {
                            icmsForaDoEstado[j][2] = ((Convert.ToDecimal(comp.IcmsNContribuinteFora) * Convert.ToDecimal(icmsForaDoEstado[j][1])) / 100).ToString();
                        }

                        //// Direfença de débito e crédito
                        var diferenca = debitosIcms - creditosIcms;

                        //Total Icms
                        var totalIcms = icmsContribuinteIncentivo + icmsNContribuinteIncentivo;

                        //// FUNEF e COTAC
                        var baseCalculo = diferenca - totalIcms;

                        //FUNEF
                        decimal percentualFunef = Convert.ToDecimal(comp.Funef == null ? 0 : comp.Funef);
                        var totalFunef = Math.Round((baseCalculo * percentualFunef) / 100, 2);

                        //COTAC
                        decimal percentualCotac = Convert.ToDecimal(comp.Cotac == null ? 0 : comp.Cotac);
                        var totalCotac = Math.Round((baseCalculo * percentualCotac) / 100, 2);

                        //Total Funef e Cotac
                        var totalFunefCotac = totalFunef + totalCotac;

                        ////Total Imposto
                        var totalImposto = icmsContribuinteIncentivo + icmsNContribuinteIncentivo + totalFunef + totalCotac;


                        ////Total Imposto Geral
                        var totalImpostoGeral = totalImposto + icmsNContribuinteForaDoEstado;

                        //// Cálculos dos Totais
                        var totalVendaContribuinte = Math.Round(ContribuintesIncentivo + ContribuintesNIncentivo, 2);
                        var totalIcmsGeralIncentivo = Math.Round(icmsContribuinteIncentivo + icmsNContribuinteIncentivo + icmsNContribuinteForaDoEstado, 2);
                        var totalGeralVendasIncentivo = Math.Round(totalVendaContribuinte + totalVendasNContribuinte, 2);



                        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                        //// Produtos Incentivados

                        //Contribuinte
                        ViewBag.VendaContribuinteIncentivo = Convert.ToDouble(ContribuintesIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PercentualIcmsContrib = Convert.ToDouble(comp.Icms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ValorVendaContribIncentivo = Convert.ToDouble(icmsContribuinteIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //Não Contribuinte
                        ViewBag.VendaNContribIncentivo = Convert.ToDouble(naoContribuinteIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalVendaNContribuinte = Convert.ToDouble(totalVendasNContribuinte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PercentualIcmsNContribuinte = Convert.ToDouble(comp.IcmsNContribuinte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ValorVendaNContribIncentivo = Convert.ToDouble(icmsNContribuinteIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                        //Não Contribuinte Fora do Estado
                        ViewBag.VendaNForaEstadoContribuinteIncetivo = Convert.ToDouble(naoContriForaDoEstadoIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalVendaNContribuinteForaDoEstado = Convert.ToDouble(totalVendasNContribuinteForaDoEstado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PercentualIcmsNaoContribForaDoEstado = Convert.ToDouble(comp.IcmsNContribuinteFora.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ValorVendaNContribForaDoEstado = Convert.ToDouble(icmsNContribuinteForaDoEstado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                        {
                            icmsForaDoEstado[j][2] = (Convert.ToDouble(icmsForaDoEstado[j][2].ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "")).ToString();
                        }

                        ViewBag.IcmsForaDoEstado = icmsForaDoEstado;

                        //// Produtos não incentivados

                        //Contribuinte
                        ViewBag.VendaContribuinteNIncentivo = Convert.ToDouble(ContribuintesNIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //Não Contribuinte
                        ViewBag.VendaNContribuinteNIncentivo = Convert.ToDouble(naoContribuinteNIncetivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //Não Contribuinte Fora do Estado
                        ViewBag.VendaNContribuinteNIncentivoForaDoEstado = Convert.ToDouble(naoContriForaDoEstadoNIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                        //// Crédito e Débito
                        //Crédito
                        ViewBag.Credito = Convert.ToDouble(creditosIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //Débito
                        ViewBag.Debito = Convert.ToDouble(debitosIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //Diferença
                        ViewBag.Diferenca = Convert.ToDouble(diferenca.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                        ////Total Icms
                        ViewBag.TotalIcms = Convert.ToDouble(totalIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                        //// FUNEF e COTAC
                        ViewBag.BaseCalculo = Convert.ToDouble(baseCalculo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //FUNEF
                        ViewBag.PercentualFunef = Convert.ToDouble(percentualFunef.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFunef = Convert.ToDouble(totalFunef.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //COTAC
                        ViewBag.PercentualCotac = Convert.ToDouble(percentualCotac.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalCotac = Convert.ToDouble(totalCotac.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //Total Funef e Cotac
                        ViewBag.TotalFunefCotac = Convert.ToDouble(totalFunefCotac.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ////Total Imposto
                        ViewBag.TotalImposto = Convert.ToDouble(totalImposto.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ////Total Imposto Geral
                        ViewBag.TotalImpostoGeral = Convert.ToDouble(totalImpostoGeral.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //// Total
                        ViewBag.TotalVendaContibuinte = Convert.ToDouble(totalVendaContribuinte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGeralVendaNContibuinte = Convert.ToDouble(totalVendasNContribuinte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGeralVendaNContibuinteForaDoEstado = Convert.ToDouble(totalVendasNContribuinteForaDoEstado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGeralIcmsIncentivo = Convert.ToDouble(totalIcmsGeralIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGeralVendasIncentivo = Convert.ToDouble(totalGeralVendasIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    }
                    else
                    {
                        List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                        List<ProductIncentivo> productincentivo = new List<ProductIncentivo>();
                        List<string> codeProdIncentivado = new List<string>();
                        List<string> codeProdST = new List<string>();
                        List<List<string>> percentuaisIncentivado = new List<List<string>>();
                        List<List<string>> percentuaisNIncentivado = new List<List<string>>();
                        
                        if (arquivo == null || arquivo.Length == 0)
                        {
                            ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                            return View(ViewData);
                        }

                        string nomeArquivo = comp.Document + year + month;

                        if (arquivo.FileName.Contains(".txt"))
                            nomeArquivo += ".txt";
                        else
                            nomeArquivo += ".tmp";

                        string caminho_WebRoot = _appEnvironment.WebRootPath;
                        string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Speds\\";
                        string caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                        string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivo);
                        if (System.IO.File.Exists(caminhoDestinoArquivoOriginal))
                        {
                            System.IO.File.Delete(caminhoDestinoArquivoOriginal);
                        }
                        var stream = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);
                        await arquivo.CopyToAsync(stream);
                        stream.Close();
                        decimal creditosIcms = import.SpedCredito(caminhoDestinoArquivoOriginal, comp.Id);

                        notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");


                        decimal vendasIncentivada = 0, vendasNIncentivada = 0, debitoIncetivo = 0, debitoNIncentivo = 0;


                        for (int i = notesVenda.Count - 1; i >= 0; i--)
                        {
                            if (notesVenda[i].Count <= 5)
                            {
                                notesVenda.RemoveAt(i);
                                continue;
                            }
                            if (notesVenda[i][1].ContainsKey("dhEmi"))
                            {
                                productincentivo = _productIncentivoService.FindByDate(comp.Id, Convert.ToDateTime(notesVenda[i][1]["dhEmi"]));
                                codeProdIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Code).ToList();
                                codeProdST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Code).ToList();
                            }

                            bool status = false;
                            decimal percent = 0;

                            for (int k = 0; k < notesVenda[i].Count(); k++)
                            {
                                if (notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    status = false;
                                    percent = 0;

                                    if (codeProdIncentivado.Contains(notesVenda[i][k]["cProd"]))
                                    {
                                        status = true;
                                        var percentualIncentivado = Convert.ToDecimal(productincentivo.Where(_ => _.Code.Equals(notesVenda[i][k]["cProd"])).ToList().Select(_ => _.Percentual).FirstOrDefault());
                                        percent = percentualIncentivado;
                                        if (percentualIncentivado < 100)
                                        {
                                            var percentualNIncentivado = 100 - percentualIncentivado;

                                            if (notesVenda[i][k].ContainsKey("vProd"))
                                            {
                                                vendasIncentivada += ((Convert.ToDecimal(notesVenda[i][k]["vProd"]) * percentualIncentivado) / 100);
                                                vendasNIncentivada += ((Convert.ToDecimal(notesVenda[i][k]["vProd"]) * percentualNIncentivado) / 100);
                                            }

                                            if (notesVenda[i][k].ContainsKey("vFrete"))
                                            {
                                                vendasIncentivada += ((Convert.ToDecimal(notesVenda[i][k]["vFrete"]) * percentualIncentivado) / 100);
                                                vendasNIncentivada += ((Convert.ToDecimal(notesVenda[i][k]["vFrete"]) * percentualNIncentivado) / 100);
                                            }

                                            if (notesVenda[i][k].ContainsKey("vDesc"))
                                            {
                                                vendasIncentivada -= ((Convert.ToDecimal(notesVenda[i][k]["vDesc"]) * percentualIncentivado) / 100);
                                                vendasNIncentivada -= ((Convert.ToDecimal(notesVenda[i][k]["vDesc"]) * percentualNIncentivado) / 100);
                                            }

                                            if (notesVenda[i][k].ContainsKey("vOutro"))
                                            {
                                                vendasIncentivada += ((Convert.ToDecimal(notesVenda[i][k]["vOutro"]) * percentualIncentivado) / 100);
                                                vendasNIncentivada += ((Convert.ToDecimal(notesVenda[i][k]["vOutro"]) * percentualNIncentivado) / 100);
                                            }

                                            if (notesVenda[i][k].ContainsKey("vSeg"))
                                            {
                                                vendasIncentivada += ((Convert.ToDecimal(notesVenda[i][k]["vSeg"]) * percentualIncentivado) / 100);
                                                vendasNIncentivada += ((Convert.ToDecimal(notesVenda[i][k]["vSeg"]) * percentualNIncentivado) / 100);
                                            }
                                        }
                                        else
                                        {
                                            if (notesVenda[i][k].ContainsKey("vProd"))
                                            {
                                                vendasIncentivada += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                            }

                                            if (notesVenda[i][k].ContainsKey("vFrete"))
                                            {
                                                vendasIncentivada += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                            }

                                            if (notesVenda[i][k].ContainsKey("vDesc"))
                                            {
                                                vendasIncentivada -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                            }

                                            if (notesVenda[i][k].ContainsKey("vOutro"))
                                            {
                                                vendasIncentivada += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                            }

                                            if (notesVenda[i][k].ContainsKey("vSeg"))
                                            {
                                                vendasIncentivada += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                            }
                                        }
                                    }
                                    else if(codeProdST.Contains(notesVenda[i][k]["cProd"]))
                                    {
                                        if (notesVenda[i][k].ContainsKey("vProd"))
                                        {
                                            vendasNIncentivada += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                        }

                                        if (notesVenda[i][k].ContainsKey("vFrete"))
                                        {
                                            vendasNIncentivada += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                        }

                                        if (notesVenda[i][k].ContainsKey("vDesc"))
                                        {
                                            vendasNIncentivada -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                        }

                                        if (notesVenda[i][k].ContainsKey("vOutro"))
                                        {
                                            vendasNIncentivada += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                        }

                                        if (notesVenda[i][k].ContainsKey("vSeg"))
                                        {
                                            vendasNIncentivada += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                        }
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("pICMS") && notesVenda[i][k].ContainsKey("CST") && notesVenda[i][k].ContainsKey("orig"))
                                {
                                    if (status == true)
                                    {
                                        if (percent < 100)
                                        {
                                            var percentNIncentivado = 100 - percent;

                                            int pos = -1;
                                            for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                            {
                                                if (percentuaisIncentivado[j][1].Equals(notesVenda[i][k]["pICMS"]))
                                                {
                                                    pos = j;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                List<string> percIncentivado = new List<string>();
                                                percIncentivado.Add(((Convert.ToDecimal(notesVenda[i][k]["vBC"]) * percent) / 100).ToString());
                                                percIncentivado.Add(notesVenda[i][k]["pICMS"]);
                                                percIncentivado.Add((((Convert.ToDecimal(notesVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"]) / 100) * percent) / 100).ToString());
                                                percentuaisIncentivado.Add(percIncentivado);
                                            }
                                            else
                                            {
                                                percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + ((Convert.ToDecimal(notesVenda[i][k]["vBC"])  * percent) / 100)).ToString();
                                                percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + ((((Convert.ToDecimal(notesVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100) * percent) / 100)).ToString();
                                            }

                                            debitoIncetivo += (((Convert.ToDecimal(notesVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"]) / 100) * percent) / 100);

                                            int indice = -1;
                                            for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                            {
                                                if (percentuaisNIncentivado[j][1].Equals(notesVenda[i][k]["pICMS"]))
                                                {
                                                    indice = j;
                                                }
                                            }

                                            if (indice < 0)
                                            {
                                                List<string> percNIncentivado = new List<string>();
                                                percNIncentivado.Add(((Convert.ToDecimal(notesVenda[i][k]["vBC"]) * percentNIncentivado) / 100).ToString());
                                                percNIncentivado.Add(notesVenda[i][k]["pICMS"]);
                                                percNIncentivado.Add((((Convert.ToDecimal(notesVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"]) / 100) * percentNIncentivado) / 100).ToString());
                                                percentuaisNIncentivado.Add(percNIncentivado);
                                            }
                                            else
                                            {
                                                percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + ((Convert.ToDecimal(notesVenda[i][k]["vBC"]) * percentNIncentivado) / 100)).ToString();
                                                percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + (((Convert.ToDecimal(notesVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"]) / 100) * percentNIncentivado) / 100)).ToString();
                                            }
                                            debitoNIncentivo += (((Convert.ToDecimal(notesVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"]) / 100) * percentNIncentivado) / 100);

                                        }
                                        else
                                        {
                                            int pos = -1;
                                            for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                            {
                                                if (percentuaisIncentivado[j][1].Equals(notesVenda[i][k]["pICMS"]))
                                                {
                                                    pos = j;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                List<string> percIncentivado = new List<string>();
                                                percIncentivado.Add(notesVenda[i][k]["vBC"]);
                                                percIncentivado.Add(notesVenda[i][k]["pICMS"]);
                                                percIncentivado.Add(((Convert.ToDecimal(notesVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100).ToString());                                               
                                                percentuaisIncentivado.Add(percIncentivado);
                                            }
                                            else
                                            {
                                                percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + Convert.ToDecimal(notesVenda[i][k]["vBC"])).ToString();
                                                percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + ((Convert.ToDecimal(notesVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"]) / 100))).ToString();
                                            }

                                            debitoIncetivo += ((Convert.ToDecimal(notesVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100);

                                        }
                                    }
                                    else
                                    {
                                        int pos = -1;
                                        for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                        {
                                            if (percentuaisNIncentivado[j][1].Equals(notesVenda[i][k]["pICMS"]))
                                            {
                                                pos = j;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> percNIncentivado = new List<string>();
                                            percNIncentivado.Add(notesVenda[i][k]["vBC"]);
                                            percNIncentivado.Add(notesVenda[i][k]["pICMS"]);
                                            percNIncentivado.Add(((Convert.ToDecimal(notesVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100).ToString());
                                            percentuaisNIncentivado.Add(percNIncentivado);
                                        }
                                        else
                                        {
                                            percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + Convert.ToDecimal(notesVenda[i][k]["vBC"])).ToString();
                                            percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + ((Convert.ToDecimal(notesVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100)).ToString();
                                        }
                                        debitoNIncentivo += ((Convert.ToDecimal(notesVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100);

                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("pFCP") && notesVenda[i][k].ContainsKey("CST") && notesVenda[i][k].ContainsKey("orig"))
                                {
                                    if (status == true)
                                    {
                                        if (percent < 100)
                                        {
                                            var percentNIncentivado = 100 - percent;

                                            int pos = -1;
                                            for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                            {
                                                if (percentuaisIncentivado[j][1].Equals(notesVenda[i][k]["pFCP"]))
                                                {
                                                    pos = j;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                List<string> percIncentivado = new List<string>();
                                                percIncentivado.Add((((Convert.ToDecimal(notesVenda[i][k]["vBC"])) * percent) / 100).ToString());
                                                percIncentivado.Add(notesVenda[i][k]["pFCP"]);
                                                percIncentivado.Add(((((Convert.ToDecimal(notesVenda[i][k]["pFCP"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100) * percent) / 100).ToString());
                                                percentuaisIncentivado.Add(percIncentivado);
                                            }
                                            else
                                            {
                                                percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + (((Convert.ToDecimal(notesVenda[i][k]["pFCP"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"]) / 100) * percent) / 100)).ToString();
                                                percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + (((Convert.ToDecimal(notesVenda[i][k]["pFCP"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"]) / 100) * percent) / 100)).ToString();
                                            }

                                            debitoIncetivo += ((((Convert.ToDecimal(notesVenda[i][k]["pFCP"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100) * percent) / 100);

                                            int indice = -1;
                                            for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                            {
                                                if (percentuaisNIncentivado[j][1].Equals(notesVenda[i][k]["pFCP"]))
                                                {
                                                    indice = j;
                                                }
                                            }

                                            if (indice < 0)
                                            {
                                                List<string> percNIncentivado = new List<string>();
                                                percNIncentivado.Add(((( Convert.ToDecimal(notesVenda[i][k]["vBC"])) * percentNIncentivado) / 100).ToString());
                                                percNIncentivado.Add(notesVenda[i][k]["pFCP"]);
                                                percNIncentivado.Add(((((Convert.ToDecimal(notesVenda[i][k]["pFCP"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100) * percentNIncentivado) / 100).ToString());                                                
                                                percentuaisNIncentivado.Add(percNIncentivado);
                                            }
                                            else
                                            {
                                                percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + (((Convert.ToDecimal(notesVenda[i][k]["vBC"])) * percentNIncentivado) / 100)).ToString();
                                                percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + ((((Convert.ToDecimal(notesVenda[i][k]["pFCP"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100) * percentNIncentivado) / 100)).ToString();
                                            }
                                            debitoNIncentivo += (((Convert.ToDecimal(notesVenda[i][k]["vBC"])) * percentNIncentivado) / 100);

                                        }
                                        else
                                        {
                                            int pos = -1;
                                            for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                            {
                                                if (percentuaisIncentivado[j][1].Equals(notesVenda[i][k]["pFCP"]))
                                                {
                                                    pos = j;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                List<string> percIncentivado = new List<string>();
                                                percIncentivado.Add(notesVenda[i][k]["vBC"]);
                                                percIncentivado.Add(notesVenda[i][k]["pFCP"]);
                                                percIncentivado.Add((Convert.ToDecimal(notesVenda[i][k]["pFCP"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"]) / 100).ToString());
                                                percentuaisIncentivado.Add(percIncentivado);
                                            }
                                            else
                                            {
                                                percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + Convert.ToDecimal(notesVenda[i][k]["vBC"])).ToString();
                                                percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + ((Convert.ToDecimal(notesVenda[i][k]["pFCP"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100)).ToString();
                                            }

                                            debitoIncetivo += (((Convert.ToDecimal(notesVenda[i][k]["pFCP"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100));
                                        }
                                    }
                                    else
                                    {
                                        int pos = -1;
                                        for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                        {
                                            if (percentuaisNIncentivado[j][1].Equals(notesVenda[i][k]["pFCP"]))
                                            {
                                                pos = j;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> percNIncentivado = new List<string>();
                                            percNIncentivado.Add(notesVenda[i][k]["vBC"]);
                                            percNIncentivado.Add(notesVenda[i][k]["pFCP"]);
                                            percNIncentivado.Add(((Convert.ToDecimal(notesVenda[i][k]["pFCP"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100).ToString());

                                            percentuaisNIncentivado.Add(percNIncentivado);
                                        }
                                        else
                                        {
                                            percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + Convert.ToDecimal(notesVenda[i][k]["vBC"])).ToString();
                                            percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + ((Convert.ToDecimal(notesVenda[i][k]["pFCP"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100)).ToString();
                                        }

                                        debitoNIncentivo += (((Convert.ToDecimal(notesVenda[i][k]["pFCP"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100));
                                    }
                                }

                            }
                        }

                         
                        var totalVendas = vendasIncentivada + vendasNIncentivada;

                        var difApuNormal = debitoIncetivo - creditosIcms;

                        var percentualCreditoNIncentivado = vendasNIncentivada / totalVendas;
                        var creditoNIncentivado = creditosIcms * percentualCreditoNIncentivado;

                        var difApuNNormal = debitoNIncentivo - creditoNIncentivado;

                        //Funef e Cotac
                        var baseDeCalcFunef = difApuNormal - difApuNNormal;
                        decimal valorFunef = baseDeCalcFunef * Convert.ToDecimal(comp.Funef) / 100;
                        decimal valorCotac = baseDeCalcFunef * Convert.ToDecimal(comp.Cotac) / 100;

                        var totalImposto = difApuNNormal + valorFunef + valorCotac;

                        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                        List<List<string>> valoresIncentivo = new List<List<string>>();

                        for (int i = 0; i < percentuaisIncentivado.Count(); i++)
                        {
                            List<string> percentual = new List<string>();
                            percentual.Add((Convert.ToDouble(percentuaisIncentivado[i][0].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "")).ToString());
                            percentual.Add(percentuaisIncentivado[i][1].Replace(".", ","));
                            percentual.Add((Convert.ToDouble(percentuaisIncentivado[i][2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "")).ToString());
                            valoresIncentivo.Add(percentual);
                        }

                        List<List<string>> valoresNIncentivo = new List<List<string>>();

                        for (int i = 0; i < percentuaisNIncentivado.Count(); i++)
                        {
                            List<string> percentual = new List<string>();
                            percentual.Add((Convert.ToDouble(percentuaisNIncentivado[i][0].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "")).ToString());
                            percentual.Add(percentuaisNIncentivado[i][1].Replace(".", ","));
                            percentual.Add((Convert.ToDouble(percentuaisNIncentivado[i][2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "")).ToString());
                            valoresNIncentivo.Add(percentual);
                        }


                        if(valoresIncentivo.Count() < valoresNIncentivo.Count())
                        {
                            int diferenca = valoresNIncentivo.Count() - valoresIncentivo.Count();
                            for(int i = 0; i < diferenca; i++)
                            {
                                List<string> percentual = new List<string>();
                                percentual.Add("0,00");
                                percentual.Add("0,00");
                                percentual.Add("0,00");
                                valoresIncentivo.Add(percentual);
                            }
                        }
                        else if(valoresIncentivo.Count() > valoresNIncentivo.Count())
                        {
                            int diferenca = valoresIncentivo.Count() - valoresNIncentivo.Count();
                            for (int i = 0; i < diferenca; i++)
                            {
                                List<string> percentual = new List<string>();
                                percentual.Add("0,00");
                                percentual.Add("0,00");
                                percentual.Add("0,00");
                                valoresNIncentivo.Add(percentual);
                            }
                        }

                        //Incentivado
                        ViewBag.ValoresIncentivo = valoresIncentivo;
                        ViewBag.DebitoIncentivo = Convert.ToDouble(debitoIncetivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""); ViewBag.TotalVendas = Convert.ToDouble(totalVendas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalVendasIncentivadas = Convert.ToDouble(vendasIncentivada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                        //Não Incentivado
                        ViewBag.ValoresNIncentivo = valoresNIncentivo;
                        ViewBag.PercentualCreditoNIncentivo = Convert.ToDouble(percentualCreditoNIncentivado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CreditoNIncentivo = Convert.ToDouble(creditoNIncentivado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DebitoNIncentivo = Convert.ToDouble(debitoNIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""); ViewBag.TotalVendas = Convert.ToDouble(totalVendas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalVendasNIncentivadas = Convert.ToDouble(vendasNIncentivada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                        // Total
                        ViewBag.TotalVendas = Convert.ToDouble(totalVendas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.Credito = Convert.ToDouble(creditosIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""); ViewBag.TotalVendas = Convert.ToDouble(totalVendas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                        //Apuração Normal
                        //Debito - ViewBag.DebitoIncentivo
                        ViewBag.CreditoIncentivo = Convert.ToDouble(creditosIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DifApuNormal = Convert.ToDouble(difApuNormal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //Apuração ñ Incentivada
                        //Debito - ViewBag.DebitoNIncetivo
                        //Credito - CreditoNIncentivo
                        ViewBag.DifApuNNormal = Convert.ToDouble(difApuNNormal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        // Funef e COTAC
                        // DifNormal - DifNIncentivada
                        ViewBag.BaseDeCalcFunef = Convert.ToDouble(baseDeCalcFunef.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PercentFunef = comp.Funef;
                        ViewBag.ValorFunef = Convert.ToDouble(valorFunef.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""); 
                        ViewBag.PercentCotac = comp.Cotac;
                        ViewBag.ValorCotac = Convert.ToDouble(valorCotac.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        // Total De Imposto
                        ViewBag.TotalDeImposto = Convert.ToDouble(totalImposto.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    }

                    ////Código do Dar
                    ViewBag.DarIcms = darIcms.Code;
                    ViewBag.DarFunef = darFunef.Code;
                    ViewBag.DarCotac = darCotac.Code;

                    
                }

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

    }
}