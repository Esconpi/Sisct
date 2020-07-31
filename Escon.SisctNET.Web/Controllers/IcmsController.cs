using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Period;
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
        private readonly ISuspensionService _suspensionService;
        private readonly IProductNoteService _itemService;
        private readonly INotificationService _notificationService;
        private readonly INoteService _noteService;
        private readonly ICreditBalanceService _creditBalanceService;

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
            ISuspensionService suspensionService,
            IProductNoteService itemService,
            INotificationService notificationService,
            IHostingEnvironment env,
            INoteService noteService,
            ICreditBalanceService creditBalanceService,
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
            _suspensionService = suspensionService;
            _itemService = itemService;
            _notificationService = notificationService;
            _appEnvironment = env;
            _noteService = noteService;
            _creditBalanceService = creditBalanceService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public async Task<IActionResult> RelatoryExit(int id, string year, string month, string type, int cfopid, IFormFile arquivo)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }

            try
            {

                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));

                ViewBag.Company = comp;

                ViewBag.Year = year;
                ViewBag.Month = month;
                ViewBag.SocialName = comp.SocialName;
                ViewBag.Document = comp.Document;
                ViewBag.TypeCompany = comp.TypeCompany;

                var NfeExit = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));
                var NfeEntrada = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                var import = new Import(_companyCfopService);
                var mes = new Month();

                string directoryNfeExit = NfeExit.Value + "\\" + comp.Document + "\\" + year + "\\" + month;
                string directoryNfeEntrada = NfeEntrada.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                ViewBag.Type = type;
                ViewBag.AnnexId = comp.AnnexId;
                ViewBag.SectionId = comp.SectionId;

                if (type.Equals("resumocfop"))
                {

                    List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
                    List<List<string>> cfopsDesordenados = new List<List<string>>();
                    List<int> cfops = new List<int>();

                    decimal valorContabilSaida = 0, baseCalcIcmsSaida = 0, valorIcmsSaida = 0, valorFecopSaida = 0,
                        valorContabilEntrada = 0, baseCalcIcmsEntrada = 0, valorIcmsEntrada = 0, valorFecopEntrada = 0;
                    int codeCfop = 0;

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
                                pos = -1;
                                codeCfop = Convert.ToInt32(notes[i][j]["CFOP"]);
                                for (int k = 0; k < cfopsDesordenados.Count(); k++)
                                {
                                    if (cfopsDesordenados[k][0].Equals(notes[i][j]["CFOP"]))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                                if (pos < 0)
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

                                if (codeCfop >= 5000)
                                {
                                    valorContabilSaida += Convert.ToDecimal(notes[i][j]["vProd"]);
                                }
                                else
                                {
                                    valorContabilEntrada += Convert.ToDecimal(notes[i][j]["vProd"]);
                                }
                            }

                            if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                            {
                                cfopsDesordenados[pos][2] = (Convert.ToDecimal(cfopsDesordenados[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();

                                if (codeCfop >= 5000)
                                {
                                    valorContabilSaida += Convert.ToDecimal(notes[i][j]["Frete"]);
                                }
                                else
                                {
                                    valorContabilEntrada += Convert.ToDecimal(notes[i][j]["Frete"]);
                                }
                            }

                            if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                            {
                                cfopsDesordenados[pos][2] = (Convert.ToDecimal(cfopsDesordenados[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                if (codeCfop >= 5000)
                                {
                                    valorContabilSaida -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }
                                else
                                {
                                    valorContabilEntrada -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }
                            }

                            if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                            {
                                cfopsDesordenados[pos][2] = (Convert.ToDecimal(cfopsDesordenados[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                if (codeCfop >= 5000)
                                {
                                    valorContabilSaida += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }
                                else
                                {
                                    valorContabilEntrada += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }
                            }

                            if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                            {
                                cfopsDesordenados[pos][2] = (Convert.ToDecimal(cfopsDesordenados[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                if (codeCfop >= 5000)
                                {
                                    valorContabilSaida += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }
                                else
                                {
                                    valorContabilEntrada += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }
                            }

                            if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                            {
                                cfopsDesordenados[pos][3] = (Convert.ToDecimal(cfopsDesordenados[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                if (codeCfop >= 5000)
                                {
                                    baseCalcIcmsSaida += Convert.ToDecimal(notes[i][j]["vBC"]);
                                }
                                else
                                {
                                    baseCalcIcmsEntrada += Convert.ToDecimal(notes[i][j]["vBC"]);
                                }
                            }

                            if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                            {
                                cfopsDesordenados[pos][4] = (Convert.ToDecimal(cfopsDesordenados[pos][4]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                if (codeCfop >= 5000)
                                {
                                    //valorIcmsSaida += Convert.ToDecimal(notes[i][j]["vICMS"]);
                                    valorIcmsSaida += ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100);
                                }
                                else
                                {
                                    //valorIcmsEntrada += Convert.ToDecimal(notes[i][j]["vICMS"]);
                                    valorIcmsEntrada += ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100);
                                }
                            }

                            if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                            {
                                cfopsDesordenados[pos][5] = (Convert.ToDecimal(cfopsDesordenados[pos][5]) + ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                if (codeCfop >= 5000)
                                {
                                    //valorFecopSaida += Convert.ToDecimal(notes[i][j]["vFCP"]);
                                    valorFecopSaida += ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100);
                                }
                                else
                                {
                                    //valorFecopEntrada += Convert.ToDecimal(notes[i][j]["vFCP"]);
                                    valorFecopEntrada += ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100);
                                }
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
                        for (int k = 2; k < 22; k++)
                        {
                            cfopsDesordenados[i][k] = Convert.ToDouble(cfopsDesordenados[i][k].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        }
                    }

                    for (int i = 0; i < cfops.Count(); i++)
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
                        for (int k = 0; k < 22; k++)
                        {
                            cc.Add(cfopsDesordenados[pos][k]);
                        }

                        cfopsOrdenados.Add(cc);
                    }

                    ViewBag.Cfop = cfopsOrdenados;

                    //Entrada
                    ViewBag.ValorContabilEntrada = Convert.ToDouble(valorContabilEntrada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseIcmsEntrada = Convert.ToDouble(baseCalcIcmsEntrada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorIcmsEntrada = Convert.ToDouble(valorIcmsEntrada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorFecopEntrada = Convert.ToDouble(valorFecopEntrada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //Saida
                    ViewBag.ValorContabilSaida = Convert.ToDouble(valorContabilSaida.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseIcmsSaida = Convert.ToDouble(baseCalcIcmsSaida.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorIcmsSaida = Convert.ToDouble(valorIcmsSaida.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorFecopSaida = Convert.ToDouble(valorFecopSaida.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //Geral
                    ViewBag.ValorContabil = Convert.ToDouble((valorContabilEntrada + valorContabilSaida).ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseIcms = Convert.ToDouble((baseCalcIcmsEntrada + baseCalcIcmsSaida).ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorIcms = Convert.ToDouble((valorIcmsEntrada + valorIcmsSaida).ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorFecop = Convert.ToDouble((valorFecopEntrada + valorFecopSaida).ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                }
                else if (type.Equals("venda"))
                {

                    List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();

                    /*List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesTranferencia = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesEntradaDevolucao = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesSaidaDevolucao = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesTransferenciaEntrada = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesDevoSaida = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesDevoEntrada = new List<List<Dictionary<string, string>>>();*/

                    exitNotes = import.Nfe(directoryNfeExit);
                    entryNotes = import.Nfe(directoryNfeEntrada);

                    var ncms = _ncmConvenioService.FindByAnnex(Convert.ToInt32(comp.AnnexId));
                    var clientesAll = _clientService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).Select(_ => _.Document).ToList();
                    var suspensions = _suspensionService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).ToList();

                    /*notesDevoEntrada = import.NfeExit(directoryNfeEntrada, id, type, "devo");
                    notesTransferenciaEntrada = import.NotesTransfer(directoryNfeEntrada, id);
                    notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                    notesTranferencia = import.NfeExit(directoryNfeExit, id, type, "transferencia");
                    notesTransferenciaEntrada = import.NotesTransfer(directoryNfeEntrada, id);
                    notesDevoSaida = import.NfeExit(directoryNfeExit, id, type, "devo");
                    notesDevoEntrada = import.NfeExit(directoryNfeEntrada, id, type, "devo");*/


                    var contribuintes = _clientService.FindByContribuinte(id, "all");
                    var contribuintesRaiz = _clientService.FindByContribuinte(id, "raiz");

                    var cfopsVenda = _companyCfopService.FindByCfopActive(id, "venda", "venda").Select(_ => _.Cfop.Code);
                    var cfopsTransf = _companyCfopService.FindByCfopActive(id, "venda", "transferencia").Select(_ => _.Cfop.Code);
                    var cfopsDevo = _companyCfopService.FindByCfopActive(id, "venda", "devo").Select(_ => _.Cfop.Code);

                    decimal totalVendas = 0, totalNcm = 0, totalTranferencias = 0, totalSaida = 0, totalDevo = 0,
                        totalDevoAnexo = 0, totalDevoContribuinte = 0, totalVendasSuspensao = 0;
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

                    decimal totalEntradas = 0, totalTranferenciaInter = 0;

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
                                    totalEntradas += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                }
                                if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vFrete"))
                                {
                                    if (entryNotes[i][1]["idDest"].Equals("2"))
                                    {
                                        totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                    }
                                    totalEntradas += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                }
                                if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vDesc"))
                                {
                                    if (entryNotes[i][1]["idDest"].Equals("2"))
                                    {
                                        totalTranferenciaInter -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                    }
                                    totalEntradas -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                }
                                if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vOutro"))
                                {
                                    if (entryNotes[i][1]["idDest"].Equals("2"))
                                    {
                                        totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                    }
                                    totalEntradas += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                }
                                if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vSeg"))
                                {
                                    if (entryNotes[i][1]["idDest"].Equals("2"))
                                    {
                                        totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                    }
                                    totalEntradas += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
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
                    decimal limiteNContribuinte = (baseCalc * (Convert.ToDecimal(comp.VendaCpf))) / 100,
                        limiteNcm = (baseCalc * Convert.ToDecimal(comp.VendaAnexo)) / 100,
                        limiteGrupo = (totalSaida * Convert.ToDecimal(comp.VendaMGrupo)) / 100,
                        limiteTransferencia = (totalEntradas * Convert.ToDecimal(comp.TransferenciaInterExcedente)) / 100;

                    decimal impostoNContribuinte = 0, excedenteNContribuinte = 0, excedenteNcm = 0, impostoNcm = 0, excedenteTranfInter = 0, impostoTransfInter = 0;


                    //CNPJ
                    List<List<string>> gruposExecentes = new List<List<string>>();
                    decimal totalVendaGrupo = 0, totalExcedente = 0, totalDevoGrupo = 0, totalImpostoGrupo = 0;
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
                            var excedenteGrupo = baseCalcGrupo - limiteGrupo;
                            var impostoGrupo = (excedenteGrupo * Convert.ToDecimal(comp.VendaMGrupoExcedente)) / 100;
                            totalVendaGrupo += vendaGrupo;
                            totalDevoGrupo += devoGrupo;
                            totalExcedente += baseCalcGrupo;
                            totalImpostoGrupo += impostoGrupo;
                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");
                            grupoExcedente.Add(cnpjGrupo);
                            grupoExcedente.Add(nomeGrupo);
                            grupoExcedente.Add(percentGrupo.ToString());
                            grupoExcedente.Add(Math.Round(Convert.ToDouble(vendaGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                            grupoExcedente.Add(Math.Round(Convert.ToDouble(limiteGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                            grupoExcedente.Add(Math.Round(Convert.ToDouble(excedenteGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                            grupoExcedente.Add(comp.VendaMGrupoExcedente.ToString());
                            grupoExcedente.Add(Math.Round(Convert.ToDouble(impostoGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                            grupoExcedente.Add(Math.Round(Convert.ToDouble(baseCalcGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                            grupoExcedente.Add(Math.Round(Convert.ToDouble(devoGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());

                            gruposExecentes.Add(grupoExcedente);
                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                        }
                    }

                    decimal baseCalcNcm = totalNcm - totalDevoAnexo;

                    //Anexo II
                    if (baseCalcNcm < limiteNcm)
                    {
                        excedenteNcm = limiteNcm - baseCalcNcm;
                        impostoNcm = (excedenteNcm * Convert.ToDecimal(comp.VendaAnexoExcedente)) / 100;
                    }

                    //Não Contribuinte
                    if (baseCalcNContribuinte > limiteNContribuinte && limiteNContribuinte > 0)
                    {
                        excedenteNContribuinte = baseCalcNContribuinte - limiteNContribuinte;
                        impostoNContribuinte = (excedenteNContribuinte * Convert.ToDecimal(comp.VendaCpfExcedente)) / 100;
                    }

                    // Transferência inter
                    if (limiteTransferencia < totalTranferenciaInter)
                    {
                        excedenteTranfInter = totalTranferenciaInter - limiteTransferencia;
                        impostoTransfInter = (excedenteTranfInter * Convert.ToDecimal(comp.TransferenciaInterExcedente)) / 100;
                    }

                    // Suspensão
                    decimal valorSuspensao = (totalVendasSuspensao * Convert.ToDecimal(comp.Suspension)) / 100;

                    // Percentuais
                    decimal percentualVendaContribuinte = (baseCalcContribuinte * 100) / baseCalc;
                    decimal percentualVendaNContribuinte = (baseCalcNContribuinte * 100) / baseCalc;
                    decimal percentualVendaAnexo = (baseCalcNcm * 100) / baseCalc;
                    decimal percentualGrupo = (totalExcedente * 100) / baseCalc;

                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                    //Geral
                    ViewBag.Contribuinte = Math.Round(Convert.ToDouble(totalContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.NContribuinte = Math.Round(Convert.ToDouble(totalNcontribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalVenda = Math.Round(Convert.ToDouble(totalVendas.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.VendaAnexo = Math.Round(Convert.ToDouble(baseCalcNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalTransferencia = Math.Round(Convert.ToDouble(totalTranferenciaInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDevo = Math.Round(Convert.ToDouble(totalDevo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseCalc = Math.Round(Convert.ToDouble(baseCalc.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDevoAnexo = Math.Round(Convert.ToDouble(totalDevoAnexo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseCalcAnexo = Math.Round(Convert.ToDouble(baseCalcNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDevoContrib = Math.Round(Convert.ToDouble(totalDevoContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseCalcContrib = Math.Round(Convert.ToDouble(baseCalcContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDevoNContrib = Math.Round(Convert.ToDouble(totalDevoNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseCalcNContrib = Math.Round(Convert.ToDouble(baseCalcNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Percentuais
                    ViewBag.PercentualVendaContribuinte = Math.Round(Convert.ToDouble(percentualVendaContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualVendaNContribuinte = Math.Round(Convert.ToDouble(percentualVendaNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualVendaAnexo = Math.Round(Convert.ToDouble(percentualVendaAnexo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualVendaGrupo = Math.Round(Convert.ToDouble(percentualGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //CNPJ
                    ViewBag.PercentualCNPJ = comp.VendaMGrupo;
                    ViewBag.TotalVendaGrupo = Math.Round(Convert.ToDouble(totalVendaGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDevoGrupo = Math.Round(Convert.ToDouble(totalDevoGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalExecedente = Math.Round(Convert.ToDouble(totalExcedente.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.Grupo = gruposExecentes;

                    //Anexo II
                    ViewBag.LimiteAnexo = Math.Round(Convert.ToDouble(limiteNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ExcedenteAnexo = Math.Round(Convert.ToDouble(excedenteNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualExcedenteAnexo = comp.VendaAnexoExcedente;
                    ViewBag.TotalExcedenteAnexo = Math.Round(Convert.ToDouble(impostoNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //Não Contribuinte
                    ViewBag.LimiteNContribuinte = Math.Round(Convert.ToDouble(limiteNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ExcedenteNContribuinte = Math.Round(Convert.ToDouble(excedenteNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualExcedenteNContribuinte = comp.VendaCpfExcedente;
                    ViewBag.TotalExcedenteNContribuinte = Math.Round(Convert.ToDouble(impostoNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //Tranferência
                    ViewBag.LimiteTransferencia = Math.Round(Convert.ToDouble(limiteTransferencia.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ExcedenteTransferencia = Math.Round(Convert.ToDouble(excedenteTranfInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentaulTransferencia = comp.TransferenciaInterExcedente;
                    ViewBag.TotalExcedenteTransferencia = Math.Round(Convert.ToDouble(impostoTransfInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Suspensão
                    ViewBag.BaseCalcSuspensao = Math.Round(Convert.ToDouble(totalVendasSuspensao.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentaulSuspensao = comp.Suspension;
                    ViewBag.TotalSuspensao = Math.Round(Convert.ToDouble(valorSuspensao.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    //Total Icms
                    ViewBag.TotalIcms = Math.Round(Convert.ToDouble((impostoNcm + impostoNContribuinte + impostoTransfInter + totalImpostoGrupo + valorSuspensao).ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    //Dar
                    var dars = _darService.FindAll(null);
                    ViewBag.DarIcms = dars.Where(_ => _.Type.Equals("Icms")).Select(_ => _.Code).FirstOrDefault();
                    ViewBag.DarFunef = dars.Where(_ => _.Type.Equals("Funef")).Select(_ => _.Code).FirstOrDefault();
                    ViewBag.DarCotac = dars.Where(_ => _.Type.Equals("Cotac")).Select(_ => _.Code).FirstOrDefault();
                    ViewBag.DarFecop = dars.Where(_ => _.Type.Equals("Fecop")).Select(_ => _.Code).FirstOrDefault();
                }
                else if (type.Equals("anexo"))
                {
                    ViewBag.Anexo = comp.Annex.Description + " - " + comp.Annex.Convenio;

                    List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                    if (comp.AnnexId.Equals(3))
                    {
                        notesVenda = import.Nfe(directoryNfeExit);
                    }
                    else
                    {
                        notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                    }

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
                                    string ncm = "";

                                    if (tamanho < 8)
                                    {
                                        ncm = notesVenda[i][j]["NCM"].Substring(0, tamanho);
                                    }
                                    else
                                    {
                                        ncm = notesVenda[i][j]["NCM"];
                                    }
                                    if (ncms[k].Ncm.Equals(ncm))
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
                    if (!comp.AnnexId.Equals(3))
                    {
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

                        List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                        List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();

                        if (comp.TypeCompany.Equals(true))
                        {
                            var productincentivo = _productIncentivoService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.CompanyId.Equals(id)).ToList();

                            var codeProdIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Code).ToList();
                            var codeProdST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Code).ToList();
                            var codeProdIsento = productincentivo.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Code).ToList();

                            var cestIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Cest).ToList();
                            var cestST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Cest).ToList();
                            var cestIsento = productincentivo.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Cest).ToList();

                            decimal creditosIcms = import.SpedCredito(caminhoDestinoArquivoOriginal, comp.Id),
                                    debitosIcms = 0;

                            /*List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                            List<List<Dictionary<string, string>>> notesVendaSt = new List<List<Dictionary<string, string>>>();
                            List<List<Dictionary<string, string>>> notesSaidaDevoCompra = new List<List<Dictionary<string, string>>>();*/

                            List<List<string>> icmsForaDoEstado = new List<List<string>>();

                            var contribuintes = _clientService.FindByContribuinte(id, "all");
                            var clientesAll = _clientService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).Select(_ => _.Document).ToList();

                            exitNotes = import.Nfe(directoryNfeExit);

                            /*notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                            notesVendaSt = import.NfeExit(directoryNfeExit, id, type, "vendaSt");
                            notesSaidaDevoCompra = import.NfeExit(directoryNfeExit, id, type, "devolucao de compra");*/

                            var cfopsDevoCompra = _companyCfopService.FindByCfopActive(id, "incentivo", "devolucao de compra").Select(_ => _.Cfop.Code);
                            var cfopsVendaST = _companyCfopService.FindByCfopActive(id, "incentivo", "vendaSt").Select(_ => _.Cfop.Code);
                            var cfopsVenda = _companyCfopService.FindByCfopActive(id, "incentivo", "venda").Select(_ => _.Cfop.Code);
                            var cfospDevoVenda = _companyCfopService.FindByCfopActive(id, "entrada", "devolução de venda").Select(_ => _.Cfop.Code);

                            decimal totalVendas = 0, naoContribuinteIncentivo = 0, naoContribuinteNIncetivo = 0, vendaCfopSTNaoContribuinteNIncetivo = 0, NaoContribuiteIsento = 0,
                                naoContriForaDoEstadoIncentivo = 0, naoContriForaDoEstadoNIncentivo = 0, vendaCfopSTNaoContriForaDoEstadoNIncentivo = 0, NaoContribuinteForaDoEstadoIsento = 0,
                                ContribuintesNIncentivo = 0, ContribuintesIncentivoAliqM25 = 0, ContribuintesIncentivo = 0, vendaCfopSTContribuintesNIncentivo = 0,
                                ContribuinteIsento = 0;

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
                                        cest = null;
                                        if (exitNotes[i][k].ContainsKey("CEST"))
                                        {
                                            cest = exitNotes[i][k]["CEST"];
                                            if (cest.Equals(""))
                                            {
                                                cest = null;
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
                                    string cest = null;
                                    if (exitNotes[i][k].ContainsKey("CEST"))
                                    {
                                        cest = exitNotes[i][k]["CEST"];
                                        if (cest.Equals(""))
                                        {
                                            cest = null;
                                        }
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
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
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
                                        creditosIcms += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                    }
                                }
                            }

                            //// Cálculos dos Produtos  Incentivados

                            //Contribuinte
                            var icmsContribuinteIncentivo = Math.Round(Convert.ToDecimal(comp.Icms) * ContribuintesIncentivo / 100, 2);
                            var icmsContribuinteIncentivoAliqM25 = Math.Round(Convert.ToDecimal(comp.IcmsAliqM25) * ContribuintesIncentivoAliqM25 / 100, 2);

                            //Não Contribuinte
                            var totalVendasNContribuinte = Math.Round(naoContribuinteIncentivo + naoContribuinteNIncetivo + vendaCfopSTNaoContribuinteNIncetivo, 2);
                            var icmsNContribuinteIncentivo = Math.Round(Convert.ToDecimal(comp.IcmsNContribuinte) * naoContribuinteIncentivo / 100, 2);

                            //Não Contribuinte Fora do Estado
                            var totalVendasNContribuinteForaDoEstado = Math.Round(naoContriForaDoEstadoIncentivo + naoContriForaDoEstadoNIncentivo + vendaCfopSTNaoContriForaDoEstadoNIncentivo, 2);
                            var icmsNContribuinteForaDoEstado = Math.Round(Convert.ToDecimal(comp.IcmsNContribuinteFora) * totalVendasNContribuinteForaDoEstado / 100, 2);

                            for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                            {
                                icmsForaDoEstado[j][2] = ((Convert.ToDecimal(comp.IcmsNContribuinteFora) * Convert.ToDecimal(icmsForaDoEstado[j][1])) / 100).ToString();
                            }

                            //// Direfença de débito e crédito
                            var diferenca = debitosIcms - creditosIcms;

                            //Total Icms
                            var totalIcms = icmsContribuinteIncentivo + icmsNContribuinteIncentivo + icmsContribuinteIncentivoAliqM25;

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
                            var totalVendaContribuinte = Math.Round(ContribuintesIncentivo + ContribuintesNIncentivo + vendaCfopSTContribuintesNIncentivo, 2);
                            var totalIcmsGeralIncentivo = Math.Round(icmsContribuinteIncentivo + icmsNContribuinteIncentivo + icmsNContribuinteForaDoEstado, 2);
                            var totalGeralVendasIncentivo = Math.Round(totalVendaContribuinte + totalVendasNContribuinte + ContribuinteIsento + NaoContribuiteIsento + ContribuintesIncentivoAliqM25, 2);



                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                            //// Produtos Incentivados

                            //Contribuinte
                            ViewBag.VendaContribuinteIncentivo = Convert.ToDouble(ContribuintesIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.PercentualIcmsContrib = Convert.ToDouble(comp.Icms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.ValorVendaContribIncentivo = Convert.ToDouble(icmsContribuinteIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.ContribuinteIsento = Convert.ToDouble(ContribuinteIsento.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.ContribuinteIncentivoAliM25 = Convert.ToDouble(ContribuintesIncentivoAliqM25.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.ValorVendaContribuinteAliM25 = Convert.ToDouble(icmsContribuinteIncentivoAliqM25.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.PercentualIcmsAiqM25Contrib = Convert.ToDouble(comp.IcmsAliqM25.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.VendaSTContribuinte = Convert.ToDouble(vendaCfopSTContribuintesNIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            //Não Contribuinte
                            ViewBag.VendaNContribIncentivo = Convert.ToDouble(naoContribuinteIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalVendaNContribuinte = Convert.ToDouble(totalVendasNContribuinte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.PercentualIcmsNContribuinte = Convert.ToDouble(comp.IcmsNContribuinte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.ValorVendaNContribIncentivo = Convert.ToDouble(icmsNContribuinteIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.NaoContribuinteIsento = Convert.ToDouble(NaoContribuiteIsento.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.VendaSTNContribuinte = Convert.ToDouble(vendaCfopSTNaoContribuinteNIncetivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            //Não Contribuinte Fora do Estado
                            ViewBag.VendaNForaEstadoContribuinteIncetivo = Convert.ToDouble(naoContriForaDoEstadoIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalVendaNContribuinteForaDoEstado = Convert.ToDouble(totalVendasNContribuinteForaDoEstado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.PercentualIcmsNaoContribForaDoEstado = Convert.ToDouble(comp.IcmsNContribuinteFora.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.ValorVendaNContribForaDoEstado = Convert.ToDouble(icmsNContribuinteForaDoEstado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.NaoContribuinteForaDoEstadoIsento = Convert.ToDouble(NaoContribuinteForaDoEstadoIsento.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.VendaSTForaEstadoNContribuinte = Convert.ToDouble(vendaCfopSTNaoContriForaDoEstadoNIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


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
                            ViewBag.TotalVendaContibuinte = Convert.ToDouble((totalVendaContribuinte + ContribuinteIsento + ContribuintesIncentivoAliqM25).ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGeralVendaNContibuinte = Convert.ToDouble((totalVendasNContribuinte + NaoContribuiteIsento).ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGeralVendaNContibuinteForaDoEstado = Convert.ToDouble((totalVendasNContribuinteForaDoEstado + NaoContribuinteForaDoEstadoIsento).ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGeralIcmsIncentivo = Convert.ToDouble(totalIcmsGeralIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGeralVendasIncentivo = Convert.ToDouble(totalGeralVendasIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.Uf = comp.Uf;

                        }
                        else
                        {
                            /*List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                            List<List<Dictionary<string, string>>> notesDevo = new List<List<Dictionary<string, string>>>();*/

                            List<ProductIncentivo> productincentivo = new List<ProductIncentivo>();
                            List<string> codeProdIncentivado = new List<string>();
                            List<string> codeProdST = new List<string>();
                            List<string> codeProdIsento = new List<string>();
                            List<string> cestIncentivado = new List<string>();
                            List<string> cestST = new List<string>();
                            List<string> cestIsento = new List<string>();
                            List<List<string>> percentuaisIncentivado = new List<List<string>>();
                            List<List<string>> percentuaisNIncentivado = new List<List<string>>();


                            decimal creditosIcms = import.SpedCredito(caminhoDestinoArquivoOriginal, comp.Id);

                            /*notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                            notesDevo = import.NfeExit(directoryNfeExit, id, type, "devolucao de compra");*/

                            exitNotes = import.Nfe(directoryNfeExit);

                            var cfopsDevoCompra = _companyCfopService.FindByCfopActive(id, "incentivo", "devolucao de compra").Select(_ => _.Cfop.Code);
                            var cfopsVenda = _companyCfopService.FindByCfopActive(id, "incentivo", "venda").Select(_ => _.Cfop.Code);
                            var cfospDevoVenda = _companyCfopService.FindByCfopActive(id, "entrada", "devolução de venda").Select(_ => _.Cfop.Code);

                            var prodsIncentivo = _productIncentivoService.FindAll(null);

                            decimal vendasIncentivada = 0, vendasNIncentivada = 0, debitoIncetivo = 0, debitoNIncentivo = 0;

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

                                        string cest = null;
                                        if (exitNotes[i][k].ContainsKey("CEST"))
                                        {
                                            cest = exitNotes[i][k]["CEST"];
                                            if (cest.Equals(""))
                                            {
                                                cest = null;
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

                                        if (cfop == true)
                                        {
                                            if (codeProdIncentivado.Contains(exitNotes[i][k]["cProd"]))
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
                                            else if (codeProdST.Contains(exitNotes[i][k]["cProd"]))
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
                                        string cest = null;
                                        if (exitNotes[i][k].ContainsKey("CEST"))
                                        {
                                            cest = exitNotes[i][k]["CEST"];
                                            if (cest.Equals(""))
                                            {
                                                cest = null;
                                            }
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

                            // Devolução de Venda
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
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
                                        creditosIcms += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                    }
                                }
                            }

                            var totalVendas = vendasIncentivada + vendasNIncentivada;

                            var difApuNormal = debitoIncetivo - creditosIcms;

                            var percentualCreditoNIncentivado = vendasNIncentivada / totalVendas * 100;
                            var creditoNIncentivado = creditosIcms * percentualCreditoNIncentivado / 100;

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


                            if (valoresIncentivo.Count() < valoresNIncentivo.Count())
                            {
                                int diferenca = valoresNIncentivo.Count() - valoresIncentivo.Count();
                                for (int i = 0; i < diferenca; i++)
                                {
                                    List<string> percentual = new List<string>();
                                    percentual.Add("0,00");
                                    percentual.Add("0,00");
                                    percentual.Add("0,00");
                                    valoresIncentivo.Add(percentual);
                                }
                            }
                            else if (valoresIncentivo.Count() > valoresNIncentivo.Count())
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
                    }
                    else if (comp.AnnexId.Equals(3))
                    {
                        List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                        var clientesAll = _clientService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).ToList();
                        var nContribuintes = clientesAll.Where(_ => _.TypeClientId.Equals(2)).Select(_ => _.Document).ToList();
                        var suspensions = _suspensionService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).ToList();

                        var cfopVenda = _companyCfopService.FindAll(null).Where(_ => _.CompanyId.Equals(id) && _.Active.Equals(true) && (_.CfopTypeId.Equals(1) || _.CfopTypeId.Equals(4) || _.CfopTypeId.Equals(5))).Select(_ => _.Cfop.Code).ToList();

                        decimal totalDarFecop = 0, totalDarIcms = 0;

                        if (comp.SectionId.Equals(1))
                        {
                            exitNotes = import.Nfe(directoryNfeExit);

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

                            //  Elencadas
                            // Internas
                            decimal icmsInternaElencada = (InternasElencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                            decimal fecopInternaElencada = (InternasElencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100;
                            decimal totalInternasElencada = icmsInternaElencada;
                            decimal icmsPresumidoInternaElencada = (InternasElencadasPortaria * Convert.ToDecimal(comp.IncIInterna)) / 100;
                            decimal totalIcmsInternaElencada = totalInternasElencada - icmsPresumidoInternaElencada;

                            totalDarFecop += fecopInternaElencada;
                            totalDarIcms += totalIcmsInternaElencada;

                            // Interestadual
                            decimal icmsInterestadualElencada = (InterestadualElencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                            decimal fecopInterestadualElencada = (InterestadualElencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100;
                            decimal totalInterestadualElencada = icmsInterestadualElencada;
                            decimal icmsPresumidoInterestadualElencada = (InterestadualElencadasPortaria * Convert.ToDecimal(comp.IncIInterestadual)) / 100;
                            decimal totalIcmsInterestadualElencada = totalInterestadualElencada - icmsPresumidoInterestadualElencada;

                            totalDarFecop += fecopInterestadualElencada;
                            totalDarIcms += totalIcmsInterestadualElencada;

                            //  Deselencadas
                            //  Internas
                            decimal icmsInternaDeselencada = (InternasDeselencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                            decimal fecopInternaDeselencada = (InternasDeselencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100;
                            decimal totalInternasDeselencada = icmsInternaDeselencada;
                            decimal icmsPresumidoInternaDeselencada = (InternasDeselencadasPortaria * Convert.ToDecimal(comp.IncIIInterna)) / 100;
                            decimal totalIcmsInternaDeselencada = totalInternasDeselencada - icmsPresumidoInternaDeselencada;

                            totalDarFecop += fecopInternaDeselencada;
                            totalDarIcms += totalIcmsInternaDeselencada;

                            // Interestadual
                            decimal icmsInterestadualDeselencada = (InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                            decimal fecopInterestadualDeselencada = (InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100;
                            decimal totalInterestadualDeselencada = icmsInterestadualDeselencada;
                            decimal icmsPresumidoInterestadualDeselencada = (InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.IncIIInterestadual)) / 100;
                            decimal totalIcmsInterestadualDeselencada = totalInterestadualDeselencada - icmsPresumidoInterestadualDeselencada;

                            totalDarFecop += fecopInterestadualDeselencada;
                            totalDarIcms += totalIcmsInterestadualDeselencada;

                            //  Percentual
                            decimal percentualVendas = (vendasClienteCredenciado * 100) / vendas;

                            var notifi = _notificationService.FindByCurrentMonth(id, month, year);

                            if (percentualVendas < Convert.ToDecimal(comp.VendaArt781))
                            {
                                if (notifi != null)
                                {
                                    Model.Notification nn = new Notification();
                                    nn.Description = "Venda p/ Cliente Credenciado no Art. 781 menor que " + comp.VendaArt781.ToString() + " %";
                                    nn.Percentual = percentualVendas;
                                    nn.MesRef = month;
                                    nn.AnoRef = year;
                                    nn.CompanyId = id;
                                    nn.Created = DateTime.Now;
                                    nn.Updated = nn.Created;
                                    _notificationService.Create(nn, GetLog(Model.OccorenceLog.Create));
                                }
                                else
                                {
                                    notifi.Percentual = percentualVendas;
                                    notifi.Updated = DateTime.Now;
                                    _notificationService.Update(notifi, GetLog(Model.OccorenceLog.Update));
                                }
                            }
                            else
                            {
                                if(notifi != null)
                                {
                                    _notificationService.Delete(notifi.Id, GetLog(Model.OccorenceLog.Delete));
                                }
                            }

                            //  Suspensão
                            decimal totalSuspensao = (suspensao * Convert.ToDecimal(comp.Suspension)) / 100;
                            totalDarIcms += totalSuspensao;

                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                            //  Elencadas
                            // Internas
                            ViewBag.VendasInternasElencadas = Convert.ToDouble(vendasInternasElencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.InternasElencadas = Convert.ToDouble(InternasElencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.InternasElencadasPortaria = Convert.ToDouble(InternasElencadasPortaria.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.IcmsInternasElencadas = Convert.ToDouble(icmsInternaElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.FecopInternasElencadas = Convert.ToDouble(fecopInternaElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalInternasElencadas = Convert.ToDouble(totalInternasElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.IcmsPresumidoInternasElencadas = Convert.ToDouble(icmsPresumidoInternaElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalIcmsInternasElencadas = Convert.ToDouble(totalIcmsInternaElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            // Interestadual
                            ViewBag.VendasInterestadualElencadas = Convert.ToDouble(vendasInterestadualElencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.InterestadualElencadas = Convert.ToDouble(InterestadualElencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.InterestadualElencadasPortaria = Convert.ToDouble(InterestadualElencadasPortaria.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.IcmsInterestadualElencadas = Convert.ToDouble(icmsInterestadualElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.FecopInterestadualElencadas = Convert.ToDouble(fecopInterestadualElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalInterestadualElencadas = Convert.ToDouble(totalInterestadualElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.IcmsPresumidoInterestadualElencadas = Convert.ToDouble(icmsPresumidoInterestadualElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalIcmsInterestadualElencadas = Convert.ToDouble(totalIcmsInterestadualElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            //  Deselencadas
                            //  Internas
                            ViewBag.VendasInternasDeselencadas = Convert.ToDouble(vendasInternasDeselencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.InternasDeselencadas = Convert.ToDouble(InternasDeselencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.InternasDeselencadasPortaria = Convert.ToDouble(InternasDeselencadasPortaria.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""); ViewBag.IcmsInternasElencadas = Convert.ToDouble(icmsInternaElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.IcmsInternasDeselencadas = Convert.ToDouble(icmsInternaDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.FecopInternasDeselencadas = Convert.ToDouble(fecopInternaDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalInternasDeselencadas = Convert.ToDouble(totalInternasDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.IcmsPresumidoInternasDeselencadas = Convert.ToDouble(icmsPresumidoInternaDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalIcmsInternasDeselencadas = Convert.ToDouble(totalIcmsInternaDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            // Interestadual
                            ViewBag.VendasInterestadualDeselencadas = Convert.ToDouble(vendasInterestadualDeselencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.InterestadualDeselencadas = Convert.ToDouble(InterestadualDeselencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.InterestadualDeselencadasPortaria = Convert.ToDouble(InterestadualDeselencadasPortaria.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.IcmsInterestadualDeselencadas = Convert.ToDouble(icmsInterestadualDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.FecopInterestadualDeselencadas = Convert.ToDouble(fecopInterestadualDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalInterestadualDeselencadas = Convert.ToDouble(totalInterestadualDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.IcmsPresumidoInterestadualDeselencadas = Convert.ToDouble(icmsPresumidoInterestadualDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalIcmsInterestadualDeselencadas = Convert.ToDouble(totalIcmsInterestadualDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            //  Percentual
                            ViewBag.PercentualVendas = percentualVendas;

                            //  Suspensão
                            ViewBag.PercentualSuspensao = comp.Suspension;
                            ViewBag.Suspensao = Convert.ToDouble(suspensao.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalSuspensao = Convert.ToDouble(totalSuspensao.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        }

                        ViewBag.AliqInterna = comp.AliqInterna;
                        ViewBag.Fecop = comp.Fecop;

                        //  Elencadas
                        ViewBag.IncIInterna = comp.IncIInterna;
                        ViewBag.IncIInterestadual = comp.IncIInterestadual;

                        //  Deselencadas
                        ViewBag.IncIIInterna = comp.IncIIInterna;
                        ViewBag.IncIIInterestadual = comp.IncIIInterestadual;

                        // Geral
                        ViewBag.TotalDarFecop = Convert.ToDouble(totalDarFecop.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalDarIcms = Convert.ToDouble(totalDarIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    }

                    //Dar
                    var dars = _darService.FindAll(null);
                    ViewBag.DarIcms = dars.Where(_ => _.Type.Equals("Icms")).Select(_ => _.Code).FirstOrDefault();
                    ViewBag.DarFunef = dars.Where(_ => _.Type.Equals("Funef")).Select(_ => _.Code).FirstOrDefault();
                    ViewBag.DarCotac = dars.Where(_ => _.Type.Equals("Cotac")).Select(_ => _.Code).FirstOrDefault();
                    ViewBag.DarFecop = dars.Where(_ => _.Type.Equals("Fecop")).Select(_ => _.Code).FirstOrDefault();
                }
                else if (type.Equals("resumoporcfop"))
                {
                    var cfop = _cfopService.FindById(cfopid, null);
                    decimal totalNota = 0,valorContabil = 0, baseIcms = 0, valorIcms = 0, valorFecop = 0;
                    ViewBag.Code = cfop.Code;
                    List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

                    notes = import.NfeExit(directoryNfeExit, cfop.Code);

                    List<List<string>> resumoNote = new List<List<string>>();


                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i].Count() <= 5 )
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        int pos = -1;
                        for (int e = 0; e < resumoNote.Count(); e++)
                        {
                            if (resumoNote[e][0].Equals(notes[i][0]["chave"]))
                            {
                                pos = e;
                            }
                        }

                        if (pos < 0)
                        {
                            List<string> note = new List<string>();
                            note.Add(notes[i][0]["chave"]);
                            note.Add(notes[i][1]["nNF"]);
                            note.Add("0");
                            note.Add("0");
                            note.Add("0");
                            note.Add("0");
                            note.Add("0");
                            resumoNote.Add(note);
                            pos = resumoNote.Count() - 1;
                        }

                        for (int j = 0; j < notes[i].Count(); j++)
                        {

                            if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                            {
                                resumoNote[pos][2] = (Convert.ToDecimal(resumoNote[pos][2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                valorContabil += Convert.ToDecimal(notes[i][j]["vProd"]);
                            }

                            if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                            {
                                resumoNote[pos][2] = (Convert.ToDecimal(resumoNote[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                valorContabil += Convert.ToDecimal(notes[i][j]["vFrete"]);
                            }

                            if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                            {
                                resumoNote[pos][2] = (Convert.ToDecimal(resumoNote[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                valorContabil -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                            }

                            if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                            {
                                resumoNote[pos][2] = (Convert.ToDecimal(resumoNote[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                valorContabil += Convert.ToDecimal(notes[i][j]["vOutro"]);
                            }

                            if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                            {
                                resumoNote[pos][2] = (Convert.ToDecimal(resumoNote[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                valorContabil += Convert.ToDecimal(notes[i][j]["vSeg"]);
                            }

                            if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                            {
                                resumoNote[pos][3] = (Convert.ToDecimal(resumoNote[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                baseIcms += Convert.ToDecimal(notes[i][j]["vBC"]);
                            }

                            if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                            {
                                resumoNote[pos][4] = (Convert.ToDecimal(resumoNote[pos][4]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                valorIcms += ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100);
                            }

                            if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                            {
                                resumoNote[pos][5] = (Convert.ToDecimal(resumoNote[pos][5]) + ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                valorFecop += ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100);
                            }

                            if (notes[i][j].ContainsKey("vNF"))
                            {
                                resumoNote[pos][6] = notes[i][j]["vNF"];
                                totalNota += Convert.ToDecimal(notes[i][j]["vNF"]);
                            }
                        }

                    }

                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");
                    for (int i = 0; i < resumoNote.Count(); i++)
                    {
                        resumoNote[i][2] = Convert.ToDouble(resumoNote[i][2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        resumoNote[i][3] = Convert.ToDouble(resumoNote[i][3].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        resumoNote[i][4] = Convert.ToDouble(resumoNote[i][4].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        resumoNote[i][5] = Convert.ToDouble(resumoNote[i][5].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        resumoNote[i][6] = Convert.ToDouble(resumoNote[i][6].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    }

                    ViewBag.Cfop = resumoNote;
                    ViewBag.TotalNota = Convert.ToDouble(totalNota.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorContabil = Convert.ToDouble(valorContabil.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseIcms = Convert.ToDouble(baseIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorIcms = Convert.ToDouble(valorIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorFecop = Convert.ToDouble(valorFecop.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                }
                else if (type.Equals("suspensao"))
                {
                    List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesTranferencia = new List<List<Dictionary<string, string>>>();
                    List<List<string>> notes = new List<List<string>>();

                    var suspensions = _suspensionService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).ToList();

                    entryNotes = import.Nfe(directoryNfeExit);

                    decimal valorTotal = 0;

                    var cfops = _companyCfopService.FindAll(null).Where(_ => _.CompanyId.Equals(id) && _.Active.Equals(true) && (_.CfopTypeId.Equals(1) || _.CfopTypeId.Equals(2) || _.CfopTypeId.Equals(5) || _.CfopTypeId.Equals(4))).Select(_ => _.Cfop.Code).ToList();

                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                    List<List<string>> periodos = new List<List<string>>();

                    for (int i = entryNotes.Count - 1; i >= 0; i--)
                    {

                        if (!entryNotes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            entryNotes.RemoveAt(i);
                            continue;
                        }
                        List<string> note = new List<string>();
                        bool suspenso = false, cfop = false;

                        if (entryNotes[i][1].ContainsKey("dhEmi"))
                        {
                            foreach (var suspension in suspensions)
                            {
                                if (Convert.ToDateTime(entryNotes[i][1]["dhEmi"]) >= Convert.ToDateTime(suspension.DateStart) && Convert.ToDateTime(entryNotes[i][1]["dhEmi"]) < Convert.ToDateTime(suspension.DateEnd))
                                {

                                    bool existe = false;

                                    foreach (var p in periodos)
                                    {
                                        if (p[0].Equals(suspension.Id.ToString()))
                                        {
                                            existe = true;
                                            break;
                                        }
                                    }

                                    if (existe == false)
                                    {
                                        List<string> periodo = new List<string>();
                                        periodo.Add(suspension.Id.ToString());
                                        periodo.Add(Convert.ToDateTime(suspension.DateStart).ToString());
                                        periodo.Add(Convert.ToDateTime(suspension.DateEnd).ToString());
                                        periodos.Add(periodo);
                                    }
                                    suspenso = true;
                                    break;
                                }
                            }
                        }


                        if (suspenso == true)
                        {
                            note.Add(entryNotes[i][1]["natOp"]);
                            note.Add(entryNotes[i][1]["mod"]);
                            note.Add(entryNotes[i][1]["nNF"]);
                            note.Add(Convert.ToDateTime(entryNotes[i][1]["dhEmi"]).ToString());
                        }


                        for (int k = 0; k < entryNotes[i].Count(); k++)
                        {
                            if (entryNotes[i][k].ContainsKey("CFOP"))
                            {
                                if (cfops.Contains(entryNotes[i][k]["CFOP"]))
                                {
                                    cfop = true;
                                }
                            }

                            if (entryNotes[i][k].ContainsKey("vNF"))
                            {
                                if (suspenso == true && cfop == true)
                                {
                                    note.Add(Convert.ToDecimal(entryNotes[i][k]["vNF"].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                                    valorTotal += Convert.ToDecimal(entryNotes[i][k]["vNF"].Replace(".", ","));
                                    notes.Add(note);
                                }
                            }

                        }

                    }

                    ViewBag.Notes = notes;
                    ViewBag.Periodos = periodos;
                    ViewBag.Total = Math.Round(Convert.ToDouble(valorTotal), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                }
                else if (type.Equals("anexoAutoPecas"))
                {
                    List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();
                    
                    var notes = _noteService.FindByNotes(id, year, month);
                    var products = _itemService.FindByProductsType(notes, 2);

                    decimal totalIcmsFreteIE = 0;

                    foreach (var prod in products)
                    {
                        if (!prod.Note.Iest.Equals(""))
                        {
                            if (Convert.ToDecimal(prod.Diferencial) > 0)
                            {
                                totalIcmsFreteIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                            }
                        }
                    }

                    decimal totalIcmsSIE = Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.IcmsApurado).Sum());
                    decimal icmsStnoteSIE = Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum());

                    decimal valorNfe1NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    icmsStnoteSIE += valorNfe1NormalSIE + valorNfe1RetSIE + valorNfe2NormalSIE + valorNfe2RetSIE;

                    decimal gnrePagaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreAp).Distinct().Sum()), 2);
                    decimal gnreNPagaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreNAp).Distinct().Sum()), 2);
                    decimal icmsApSIE = Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.IcmsAp).Distinct().Sum());

                    decimal valorDiefSIE = Convert.ToDecimal((totalIcmsSIE + totalIcmsFreteIE) - icmsStnoteSIE - gnrePagaSIE + gnreNPagaSIE);
                    decimal icmsAPAPagar = valorDiefSIE - icmsApSIE;

                    exitNotes = import.Nfe(directoryNfeExit);
                    entryNotes = import.Nfe(directoryNfeEntrada);

                    var ncms = _ncmConvenioService.FindByAnnex(Convert.ToInt32(comp.AnnexId));

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

                    var cfopsVenda = _companyCfopService.FindAll(null).Where(_ => _.CompanyId.Equals(id) && _.Active.Equals(true) && (_.CfopTypeId.Equals(1) || _.CfopTypeId.Equals(2) || _.CfopTypeId.Equals(4) || _.CfopTypeId.Equals(5))).Select(_ => _.Cfop.Code);
                    var cfopsDevo = _companyCfopService.FindAll(null).Where(_ => _.CompanyId.Equals(id) && _.Active.Equals(true) && (_.CfopTypeId.Equals(3) || _.CfopTypeId.Equals(7))).Select(_ => _.Cfop.Code); ;

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

                    var mesAtual = mes.NumberMonth(month);
                    var mesAnterior = mes.NameMonth(mesAtual);
                    decimal saldoCredorAnterior = 0;

                    string ano = year;

                    if (mesAtual.Equals(1))
                    {
                        ano = (Convert.ToInt32(year) - 1).ToString();
                    }

                    var creditLast = _creditBalanceService.FindByLastMonth(id, mesAnterior, ano);

                    if (creditLast != null)
                    {
                        saldoCredorAnterior = Convert.ToDecimal(creditLast.Saldo);
                    }

                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                    //  VENDAS
                    // Internas
                    for (int i = 0; i < vendaInterna.Count(); i++)
                    {
                        vendaInterna[i][0] = Convert.ToDouble(vendaInterna[i][0].ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        vendaInterna[i][1] = Convert.ToInt32(Convert.ToDecimal(vendaInterna[i][1].Replace(".", ","))).ToString();
                        vendaInterna[i][2] = Convert.ToDouble(vendaInterna[i][2].ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        baseCalcTotalB += Convert.ToDecimal(vendaInterna[i][0]);
                        icmsTotalB += Convert.ToDecimal(vendaInterna[i][2]);
                    }

                    ViewBag.VendasInternas = vendaInterna;

                    // Interestadual
                    ViewBag.BaseCalcVendaInterestadual4 = Convert.ToDouble(baseCalcVendaInterestadual4.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseCalcVendaInterestadual7 = Convert.ToDouble(baseCalcVendaInterestadual7.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseCalcVendaInterestadual12 = Convert.ToDouble(baseCalcVendaInterestadual12.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsVendaInterestadual4 = Convert.ToDouble(icmsVendaInterestadual4.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsVendaInterestadual7 = Convert.ToDouble(icmsVendaInterestadual7.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsVendaInterestadual12 = Convert.ToDouble(icmsVendaInterestadual12.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //  COMPRAS
                    // Internas
                    for (int i = 0; i < compraInterna.Count(); i++)
                    {
                        compraInterna[i][0] = Convert.ToDouble(compraInterna[i][0].ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        compraInterna[i][1] = Convert.ToInt32(Convert.ToDecimal(compraInterna[i][1].Replace(".", ","))).ToString();
                        compraInterna[i][2] = Convert.ToDouble(compraInterna[i][2].ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        baseCalcTotalA += Convert.ToDecimal(compraInterna[i][0]);
                        icmsTotalA += Convert.ToDecimal(compraInterna[i][2]);
                    }

                    ViewBag.ComprasInternas = compraInterna;

                    // Interestadual
                    ViewBag.BaseCalcCompraInterestadual4 = Convert.ToDouble(baseCalcCompraInterestadual4.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseCalcCompraInterestadual7 = Convert.ToDouble(baseCalcCompraInterestadual7.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseCalcCompraInterestadual12 = Convert.ToDouble(baseCalcCompraInterestadual12.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsCompraInterestadual4 = Convert.ToDouble(icmsCompraInterestadual4.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsCompraInterestadual7 = Convert.ToDouble(icmsCompraInterestadual7.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsCompraInterestadual12 = Convert.ToDouble(icmsCompraInterestadual12.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //  Devoluções a Fornecedor
                    // Internas
                    for (int i = 0; i < devoFornecedorInterna.Count(); i++)
                    {
                        devoFornecedorInterna[i][0] = Convert.ToDouble(devoFornecedorInterna[i][0].ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        devoFornecedorInterna[i][1] = Convert.ToInt32(Convert.ToDecimal(devoFornecedorInterna[i][1].Replace(".", ","))).ToString();
                        devoFornecedorInterna[i][2] = Convert.ToDouble(devoFornecedorInterna[i][2].ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        baseCalcTotalA -= Convert.ToDecimal(devoFornecedorInterna[i][0]);
                        icmsTotalA -= Convert.ToDecimal(devoFornecedorInterna[i][2]);
                    }

                    ViewBag.DevoFornecedorInternas = devoFornecedorInterna;

                    // Interestadual
                    ViewBag.BaseCalcDevoFornInterestadual4 = Convert.ToDouble(baseCalcDevoFornecedorInterestadual4.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseCalcDevoFornInterestadual7 = Convert.ToDouble(baseCalcDevoFornecedorInterestadual7.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseCalcDevoFornInterestadual12 = Convert.ToDouble(baseCalcDevoFornecedorInterestadual12.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsDevoFornInterestadual4 = Convert.ToDouble(icmsDevoFornecedorInterestadual4.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsDevoFornInterestadual7 = Convert.ToDouble(icmsDevoFornecedorInterestadual7.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsDevoFornInterestadual12 = Convert.ToDouble(icmsDevoFornecedorInterestadual12.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //  Devoluções de Cliente
                    // Internas
                    for (int i = 0; i < devoClienteInterna.Count(); i++)
                    {
                        devoClienteInterna[i][0] = Convert.ToDouble(devoClienteInterna[i][0].ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        devoClienteInterna[i][1] = Convert.ToInt32(Convert.ToDecimal(devoClienteInterna[i][1].Replace(".", ","))).ToString();
                        devoClienteInterna[i][2] = Convert.ToDouble(devoClienteInterna[i][2].ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        baseCalcTotalB -= Convert.ToDecimal(devoClienteInterna[i][0]);
                        icmsTotalB -= Convert.ToDecimal(devoClienteInterna[i][2]);
                    }

                    ViewBag.DevoClienteInternas = devoClienteInterna;

                    // Interestadual
                    ViewBag.BaseCalcDevoCliInterestadual4 = Convert.ToDouble(baseCalcDevoClienteInterestadual4.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.BaseCalcDevoCliInterestadual12 = Convert.ToDouble(baseCalcDevoClienteInterestadual12.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsDevoCliInterestadual4 = Convert.ToDouble(icmsDevoClienteInterestadual4.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsDevoCliInterestadual12 = Convert.ToDouble(icmsDevoClienteInterestadual12.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    //  Total
                    // A
                    baseCalcTotalA += Convert.ToDecimal((baseCalcCompraInterestadual4 + baseCalcCompraInterestadual7 + baseCalcCompraInterestadual12).ToString().Replace(".", ","));
                    baseCalcTotalA -= Convert.ToDecimal((baseCalcDevoFornecedorInterestadual4 + baseCalcDevoFornecedorInterestadual7 + baseCalcDevoFornecedorInterestadual12).ToString().Replace(".", ","));
                    icmsTotalA += Convert.ToDecimal((icmsCompraInterestadual4 + icmsCompraInterestadual7 + icmsCompraInterestadual12).ToString().Replace(".", ","));
                    icmsTotalA -= Convert.ToDecimal((icmsDevoFornecedorInterestadual4 + icmsDevoFornecedorInterestadual7 + icmsDevoFornecedorInterestadual12).ToString().Replace(".", ","));
                    ViewBag.BaseCalcTotalA = Convert.ToDouble(baseCalcTotalA.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsTotalA = Convert.ToDouble(icmsTotalA.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // B
                    baseCalcTotalB += Convert.ToDecimal((baseCalcVendaInterestadual4 + baseCalcVendaInterestadual7 + baseCalcVendaInterestadual12).ToString().Replace(".", ","));
                    baseCalcTotalB -= Convert.ToDecimal((baseCalcDevoClienteInterestadual4 + baseCalcDevoClienteInterestadual12).ToString().Replace(".", ","));
                    icmsTotalB += Convert.ToDecimal((icmsVendaInterestadual4 + icmsVendaInterestadual7 + icmsVendaInterestadual12).ToString().Replace(".", ","));
                    icmsTotalB -= Convert.ToDecimal((icmsDevoClienteInterestadual4 + icmsDevoClienteInterestadual12).ToString().Replace(".", ","));
                    ViewBag.BaseCalcTotalB = Convert.ToDouble(baseCalcTotalB.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsTotalB = Convert.ToDouble(icmsTotalB.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Saldo Credor Mes Anterior
                    ViewBag.SaldoCredorAnterior = Convert.ToDouble(saldoCredorAnterior.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // CRÉDITO DA ANTECIPAÇÃO PARCIAL PAGA
                    ViewBag.APPagar = Convert.ToDouble(icmsAPAPagar.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Saldo Devedor
                    decimal saldoDevedor = icmsTotalB - icmsTotalA - icmsAPAPagar - saldoCredorAnterior;
                    ViewBag.SaldoDevedor = Convert.ToDouble(saldoDevedor.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Saldo Credor
                    decimal saldoCredor = icmsTotalA + icmsAPAPagar + saldoCredorAnterior - icmsTotalB;
                    ViewBag.SaldoCredor = Convert.ToDouble(saldoCredor.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    var creditCurrent = _creditBalanceService.FindByCurrentMonth(id, month, year);

                    if (creditCurrent == null)
                    {
                        Model.CreditBalance credit = new Model.CreditBalance();
                        credit.CompanyId = id;
                        credit.MesRef = month;
                        credit.AnoRef = year;
                        credit.Saldo = saldoCredor;
                        credit.Created = DateTime.Now;
                        credit.Updated = credit.Created;
                        _creditBalanceService.Create(credit, GetLog(Model.OccorenceLog.Create));
                    }
                    else
                    {
                        creditCurrent.Updated = DateTime.Now;
                        creditCurrent.Saldo = saldoCredor;
                        _creditBalanceService.Update(creditCurrent,GetLog(Model.OccorenceLog.Update));
                    }
                    
                }
                else if (type.Equals("anexoMedicamento"))
                {
                    List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                    var clientesAll = _clientService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).ToList();
                    var nContribuintes = clientesAll.Where(_ => _.TypeClientId.Equals(2)).Select(_ => _.Document).ToList();

                    List<List<string>> elencadaInterna = new List<List<string>>();
                    List<List<string>> elencadaInterestadual = new List<List<string>>();
                    List<List<string>> daselencadaInterna = new List<List<string>>();
                    List<List<string>> daselencadaInterestadual = new List<List<string>>();

                    exitNotes = import.Nfe(directoryNfeExit);

                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                    {
                        if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            exitNotes.RemoveAt(i);
                            continue;
                        }

                        bool nContribuinte = false, ncm = false;

                        if (exitNotes[i][3].ContainsKey("CNPJ"))
                        {
                            if (nContribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                            {
                                nContribuinte = true;
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

                        decimal valorNF = 0;

                        for (int j = 0; j < exitNotes[i].Count; j++)
                        {
                            if (exitNotes[i][j].ContainsKey("NCM"))
                            {
                                ncm = _itemService.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId), exitNotes[i][j]["NCM"].ToString());
                            }

                            if(ncm == true)
                            {
                                
                                if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                {
                                    valorNF += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                }

                                if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                {
                                    valorNF += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                }

                                if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                {
                                    valorNF -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                }

                                if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                {
                                    valorNF += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                }

                                if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                {
                                    valorNF += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                }
                              
                            }

                        }

                        string CNPJ = exitNotes[i][3].ContainsKey("CNPJ") ? exitNotes[i][3]["CNPJ"] : "";
                        string IE = exitNotes[i][3].ContainsKey("IE") ? exitNotes[i][3]["IE"] : "";

                        List<string> note = new List<string>();

                        note.Add(exitNotes[i][1]["nNF"]);
                        note.Add(exitNotes[i][3]["xNome"]);
                        note.Add(CNPJ);
                        note.Add(exitNotes[i][3]["UF"]);
                        note.Add(IE);
                        note.Add(valorNF.ToString());

                        if (nContribuinte == true)
                        {
                            if(exitNotes[i][1]["idDest"] == "1")
                            {
                                // Internas


                            }
                            else
                            {
                                // Interestadual

                            }
                        }
                        else
                        {
                            if (exitNotes[i][1]["idDest"] == "1")
                            {
                                // Internas


                            }
                            else
                            {
                                // Interestadual

                            }
                        }

                        valorNF = 0;
                        note.Clear(); 
                    }
                
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