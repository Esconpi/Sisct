using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Period;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
        private readonly IProductIncentivoService _productIncentivoService;
        private readonly ICfopService _cfopService;
        private readonly ISuspensionService _suspensionService;
        private readonly IProductNoteService _itemService;
        private readonly INotificationService _notificationService;
        private readonly INoteService _noteService;
        private readonly ICreditBalanceService _creditBalanceService;
        private readonly INcmService _ncmService;
        private readonly ITaxService _taxService;
        private readonly IGrupoService _grupoService;

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
            INoteService noteService,
            ICreditBalanceService creditBalanceService,
            INcmService ncmService,
            ITaxService taxService,
            IGrupoService grupoService,
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
            _noteService = noteService;
            _creditBalanceService = creditBalanceService;
            _ncmService = ncmService;
            _taxService = taxService;
            _grupoService = grupoService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Relatory(int id, string year, string month, string type, int cfopid, string opcao)
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

                var importXml = new Xml.Import(_companyCfopService);
                var importSped = new Sped.Import(_companyCfopService);
                var mes = new Month();

                string directoryNfeExit = NfeExit.Value + "\\" + comp.Document + "\\" + year + "\\" + month;
                string directoryNfeEntrada = NfeEntrada.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                ViewBag.Type = type;
                ViewBag.Opcao = opcao;
                ViewBag.AnnexId = comp.AnnexId;
                ViewBag.SectionId = comp.SectionId;

                var imp = _taxService.FindByMonth(id, month, year);

                var cfopsVenda = _companyCfopService.FindByCfopVenda(comp.Document).Select(_ => _.Cfop.Code).ToList();

                if (type.Equals("resumocfop"))
                {

                    List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
                    List<List<string>> cfops = new List<List<string>>();

                    decimal valorContabilSaida = 0, baseCalcIcmsSaida = 0, valorIcmsSaida = 0, valorFecopSaida = 0,
                        valorContabilEntrada = 0, baseCalcIcmsEntrada = 0, valorIcmsEntrada = 0, valorFecopEntrada = 0;
                    int codeCfop = 0;

                    if (opcao.Equals("saida"))
                    {
                        notes = importXml.Nfe(directoryNfeExit);

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
                                    for (int k = 0; k < cfops.Count(); k++)
                                    {
                                        if (cfops[k][0].Equals(notes[i][j]["CFOP"]))
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

                                        cfops.Add(cfop);
                                        //cfops.Add(Convert.ToInt32(notes[i][j]["CFOP"]));
                                        pos = cfops.Count() - 1;
                                    }

                                }

                                // Geral

                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();

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
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();

                                    if (codeCfop >= 5000)
                                    {
                                        valorContabilSaida += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                    }
                                    else
                                    {
                                        valorContabilEntrada += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                    }
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
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
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
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
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
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
                                    cfops[pos][3] = (Convert.ToDecimal(cfops[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
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
                                    cfops[pos][4] = (Convert.ToDecimal(cfops[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    if (codeCfop >= 5000)
                                    {
                                        valorIcmsSaida += Convert.ToDecimal(notes[i][j]["vICMS"]);
                                        //valorIcmsSaida += ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100);
                                    }
                                    else
                                    {
                                        valorIcmsEntrada += Convert.ToDecimal(notes[i][j]["vICMS"]);
                                        //valorIcmsEntrada += ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100);
                                    }
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    if (codeCfop >= 5000)
                                    {
                                        valorFecopSaida += Convert.ToDecimal(notes[i][j]["vFCP"]);
                                        //valorFecopSaida += ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100);
                                    }
                                    else
                                    {
                                        valorFecopEntrada += Convert.ToDecimal(notes[i][j]["vFCP"]);
                                        //valorFecopEntrada += ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100);
                                    }
                                }

                                if (cpf != "escon" && cpf != "")
                                {
                                    // Com CPF

                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfops[pos][7] = (Convert.ToDecimal(cfops[pos][7]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfops[pos][8] = (Convert.ToDecimal(cfops[pos][8]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfops[pos][9] = (Convert.ToDecimal(cfops[pos][9]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    }

                                }
                                else if ((cpf == "escon" || cpf == "") && cnpj == "escon")
                                {
                                    // Sem CPF

                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][10] = (Convert.ToDecimal(cfops[pos][10]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][10] = (Convert.ToDecimal(cfops[pos][10]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][10] = (Convert.ToDecimal(cfops[pos][10]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][10] = (Convert.ToDecimal(cfops[pos][10]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][10] = (Convert.ToDecimal(cfops[pos][10]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfops[pos][11] = (Convert.ToDecimal(cfops[pos][11]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfops[pos][12] = (Convert.ToDecimal(cfops[pos][12]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfops[pos][13] = (Convert.ToDecimal(cfops[pos][13]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    }
                                }
                                else if (cnpj != "escon" && cnpj != "" && indIEDest == "1")
                                {
                                    // Com CNPJ e com IE

                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][14] = (Convert.ToDecimal(cfops[pos][14]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][14] = (Convert.ToDecimal(cfops[pos][14]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][14] = (Convert.ToDecimal(cfops[pos][14]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][14] = (Convert.ToDecimal(cfops[pos][14]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][14] = (Convert.ToDecimal(cfops[pos][14]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfops[pos][15] = (Convert.ToDecimal(cfops[pos][15]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfops[pos][16] = (Convert.ToDecimal(cfops[pos][16]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfops[pos][17] = (Convert.ToDecimal(cfops[pos][17]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    }
                                }
                                else if (cnpj != "escon" && cnpj != "" && indIEDest != "1")
                                {
                                    // Com CNPJ e sem IE
                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][18] = (Convert.ToDecimal(cfops[pos][18]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][18] = (Convert.ToDecimal(cfops[pos][18]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][18] = (Convert.ToDecimal(cfops[pos][18]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][18] = (Convert.ToDecimal(cfops[pos][18]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][18] = (Convert.ToDecimal(cfops[pos][18]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfops[pos][19] = (Convert.ToDecimal(cfops[pos][19]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfops[pos][20] = (Convert.ToDecimal(cfops[pos][20]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfops[pos][21] = (Convert.ToDecimal(cfops[pos][21]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    }
                                }

                            }

                        }

                        ViewBag.Cfop = cfops.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                    }
                    else
                    {
                        notes = importXml.Nfe(directoryNfeEntrada);

                        for (int i = notes.Count - 1; i >= 0; i--)
                        {
                            if (!notes[i][3]["CNPJ"].Equals(comp.Document))
                            {
                                notes.RemoveAt(i);
                                continue;
                            }

                            int pos = -1;

                            for (int j = 0; j < notes[i].Count(); j++)
                            {
                                if (notes[i][j].ContainsKey("CFOP"))
                                {
                                    pos = -1;
                                    codeCfop = Convert.ToInt32(notes[i][j]["CFOP"]);
                                    for (int k = 0; k < cfops.Count(); k++)
                                    {
                                        if (cfops[k][0].Equals(notes[i][j]["CFOP"]))
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

                                        cfops.Add(cfop);
                                        //cfops.Add(Convert.ToInt32(notes[i][j]["CFOP"]));
                                        pos = cfops.Count() - 1;
                                    }

                                }

                                // Geral
                                if (notes[i][j].ContainsKey("cProd"))
                                {
                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();

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
                                        cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();

                                        if (codeCfop >= 5000)
                                        {
                                            valorContabilSaida += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                        }
                                        else
                                        {
                                            valorContabilEntrada += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                        }
                                    }

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
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
                                        cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
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
                                        cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                        if (codeCfop >= 5000)
                                        {
                                            valorContabilSaida += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                        }
                                        else
                                        {
                                            valorContabilEntrada += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                        }
                                    }

                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfops[pos][3] = (Convert.ToDecimal(cfops[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
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
                                    cfops[pos][4] = (Convert.ToDecimal(cfops[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    if (codeCfop >= 5000)
                                    {
                                        valorIcmsSaida += Convert.ToDecimal(notes[i][j]["vICMS"]);
                                        //valorIcmsSaida += ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100);
                                    }
                                    else
                                    {
                                        valorIcmsEntrada += Convert.ToDecimal(notes[i][j]["vICMS"]);
                                        //valorIcmsEntrada += ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100);
                                    }
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    if (codeCfop >= 5000)
                                    {
                                        valorFecopSaida += Convert.ToDecimal(notes[i][j]["vFCP"]);
                                        //valorFecopSaida += ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100);
                                    }
                                    else
                                    {
                                        valorFecopEntrada += Convert.ToDecimal(notes[i][j]["vFCP"]);
                                        //valorFecopEntrada += ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100);
                                    }
                                }

                            }

                        }

                        ViewBag.Cfop = cfops.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                    }

                }
                else if (type.Equals("venda"))
                {

                    if (imp == null)
                    {
                        throw new Exception("Os dados para calcular ICMS não foram importados");
                    }

                    var grupos = _grupoService.FindByGrupos(imp.Id);


                    decimal totalVendas = Convert.ToDecimal(imp.Vendas), totalNcm = Convert.ToDecimal(imp.VendasNcm), totalTranferencias = Convert.ToDecimal(imp.Transferencia),
                         totalDevo = Convert.ToDecimal(imp.Devolucao), totalDevoAnexo = Convert.ToDecimal(imp.DevolucaoNcm), totalDevoContribuinte = 0,
                        totalVendasSuspensao = Convert.ToDecimal(imp.Suspensao), totalTranferenciaInter = Convert.ToDecimal(imp.TransferenciaInter);


                    decimal totalNcontribuinte = Convert.ToDecimal(imp.VendasNContribuinte), baseCalc = totalVendas - totalDevo, totalContribuinte = totalVendas - totalNcontribuinte,
                    baseCalcContribuinte = totalContribuinte - totalDevoContribuinte, totalDevoNContribuinte = totalDevo - totalDevoContribuinte,
                    baseCalcNContribuinte = totalNcontribuinte - totalDevoNContribuinte, totalSaida = baseCalc + totalTranferencias;

                    decimal limiteNContribuinte = (baseCalc * (Convert.ToDecimal(comp.VendaCpf))) / 100,
                    limiteNcm = (baseCalc * Convert.ToDecimal(comp.VendaAnexo)) / 100,
                    limiteGrupo = (totalSaida * Convert.ToDecimal(comp.VendaMGrupo)) / 100,
                    limiteTransferencia = (totalTranferencias * Convert.ToDecimal(comp.Transferencia)) / 100;

                    decimal impostoNContribuinte = 0, excedenteNContribuinte = 0, excedenteNcm = 0, impostoNcm = 0, excedenteTranfInter = 0, impostoTransfInter = 0;

                    totalVendas = totalVendas + totalTranferencias;

                    //CNPJ
                    List<List<string>> gruposExecentes = new List<List<string>>();
                    decimal totalVendaGrupo = Convert.ToDecimal(grupos.Sum(_ => _.Vendas)),
                        totalDevoGrupo = Convert.ToDecimal(grupos.Sum(_ => _.Devolucao)),
                        baseCalcGrupo = totalVendaGrupo - totalDevoGrupo,
                        totalExcedente = 0, totalImpostoGrupo = 0;

                    if (baseCalcGrupo > limiteGrupo)
                    {
                        totalExcedente = baseCalcGrupo - limiteGrupo;
                        totalImpostoGrupo = (totalExcedente * Convert.ToDecimal(comp.VendaMGrupoExcedente)) / 100;
                    }

                    foreach (var g in grupos)
                    {
                        List<string> grupoExcedente = new List<string>();
                        grupoExcedente.Add(g.Cnpj);
                        grupoExcedente.Add(g.Nome);
                        grupoExcedente.Add(Math.Round(Convert.ToDecimal(g.BaseCalculo), 2).ToString());
                        grupoExcedente.Add(g.Percentual.ToString());

                        gruposExecentes.Add(grupoExcedente);
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
                    decimal percentualGrupo = (baseCalcGrupo * 100) / baseCalc;

                    //Geral
                    ViewBag.Contribuinte = totalContribuinte;
                    ViewBag.NContribuinte = totalNcontribuinte;
                    ViewBag.TotalVenda = totalVendas;
                    ViewBag.VendaAnexo = totalNcm;
                    ViewBag.TotalTransferencia = totalTranferenciaInter;
                    ViewBag.TotalDevo = totalDevo;
                    ViewBag.BaseCalc = baseCalc;
                    ViewBag.TotalDevoAnexo = totalDevoAnexo;
                    ViewBag.BaseCalcAnexo = baseCalcNcm;
                    ViewBag.TotalDevoContrib = totalDevoContribuinte;
                    ViewBag.BaseCalcContrib = baseCalcContribuinte;
                    ViewBag.TotalDevoNContrib = totalDevoNContribuinte;
                    ViewBag.BaseCalcNContrib = baseCalcNContribuinte;

                    // Percentuais
                    ViewBag.PercentualVendaContribuinte = percentualVendaContribuinte;
                    ViewBag.PercentualVendaNContribuinte = percentualVendaNContribuinte;
                    ViewBag.PercentualVendaAnexo = percentualVendaAnexo;
                    ViewBag.PercentualVendaGrupo = percentualGrupo;

                    //CNPJ
                    ViewBag.PercentualCNPJ = Convert.ToDecimal(comp.VendaMGrupo);
                    ViewBag.TotalVendaGrupo = totalVendaGrupo;
                    ViewBag.TotalDevoGrupo = totalDevoGrupo;
                    ViewBag.TotalBaseCalcuGrupo = baseCalcGrupo;
                    ViewBag.ExcedenteGrupo = totalExcedente;
                    ViewBag.TotalExcedenteGrupo = totalImpostoGrupo;
                    ViewBag.LimiteGrupo = limiteGrupo;
                    ViewBag.PercentualExcedenteGrupo = Convert.ToDecimal(comp.VendaMGrupoExcedente);
                    ViewBag.Grupo = gruposExecentes;

                    //Anexo II
                    ViewBag.LimiteAnexo = limiteNcm;
                    ViewBag.ExcedenteAnexo = excedenteNcm;
                    ViewBag.PercentualExcedenteAnexo = Convert.ToDecimal(comp.VendaAnexoExcedente);
                    ViewBag.TotalExcedenteAnexo = impostoNcm;

                    //Não Contribuinte
                    ViewBag.LimiteNContribuinte = limiteNContribuinte;
                    ViewBag.ExcedenteNContribuinte = excedenteNContribuinte;
                    ViewBag.PercentualExcedenteNContribuinte = Convert.ToDecimal(comp.VendaCpfExcedente);
                    ViewBag.TotalExcedenteNContribuinte = impostoNContribuinte;

                    //Tranferência
                    ViewBag.LimiteTransferencia = limiteTransferencia;
                    ViewBag.ExcedenteTransferencia = excedenteTranfInter;
                    ViewBag.PercentaulTransferencia = Convert.ToDecimal(comp.TransferenciaInterExcedente);
                    ViewBag.TotalExcedenteTransferencia = impostoTransfInter;

                    // Suspensão
                    ViewBag.BaseCalcSuspensao = totalVendasSuspensao;
                    ViewBag.PercentaulSuspensao = Convert.ToDecimal(comp.Suspension);
                    ViewBag.TotalSuspensao = valorSuspensao;


                    //Total Icms
                    ViewBag.TotalIcms = impostoNcm + impostoNContribuinte + impostoTransfInter + totalImpostoGrupo + valorSuspensao;


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

                    notesVenda = importXml.NfeExit(directoryNfeExit, cfopsVenda);

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


                    decimal totalVendas = 0;
                    bool status = false;

                    for (int i = notesVenda.Count - 1; i >= 0; i--)
                    {
                        if (!notesVenda[i][2]["CNPJ"].Equals(comp.Document) || notesVenda[i].Count <= 5)
                        {
                            notesVenda.RemoveAt(i);
                            continue;
                        }

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

                                if (notesVenda[i][j].ContainsKey("vICMSST"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vICMSST"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) + Convert.ToDecimal(notesVenda[i][j]["vICMSST"])).ToString();
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

                                if (notesVenda[i][j].ContainsKey("vICMSST"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vICMSST"]);
                                }
                            }
                        
                        }
                    }


                    decimal percentualTotal = 0;
                    decimal valorTotalNcm = 0;

                    List<List<string>> ncm_list = new List<List<string>>();

                    for (int i = 0; i < contNcm; i++)
                    {
                        if (resumoAnexo[i, 2] != "0")
                        {
                            resumoAnexo[i, 3] = ((Convert.ToDecimal(resumoAnexo[i, 2]) * 100) / totalVendas).ToString();
                            valorTotalNcm += Convert.ToDecimal(resumoAnexo[i, 2]);
                            percentualTotal += Convert.ToDecimal(resumoAnexo[i, 3]);

                            List<string> ncm = new List<string>();
                            ncm.Add(resumoAnexo[i, 0]);
                            ncm.Add(resumoAnexo[i, 1]);
                            ncm.Add(resumoAnexo[i, 2]);
                            ncm.Add(resumoAnexo[i, 3]);
                            ncm_list.Add(ncm);
                        }
                    }

                    ViewBag.TotalNcm = valorTotalNcm;
                    ViewBag.PercentualTotal = percentualTotal;
                    ViewBag.TotalVendas = totalVendas;
                    ViewBag.ResumoNcm = ncm_list.OrderBy(_ => Convert.ToInt32(_[0])).ToList();

                }
                else if (type.Equals("foraAnexo"))
                {
                    List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                    notesVenda = importXml.NfeExit(directoryNfeExit, cfopsVenda);
                    var ncms = _ncmConvenioService.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId));

                    List<List<string>> ncmsForaAnexo = new List<List<string>>();
                    bool status = false;
                    decimal totalVendas = 0;

                    var ncmsAll = _ncmService.FindAll(null);

                    for (int i = notesVenda.Count - 1; i >= 0; i--)
                    {
                        if (!notesVenda[i][2]["CNPJ"].Equals(comp.Document) || notesVenda[i].Count <= 5)
                        {
                            notesVenda.RemoveAt(i);
                            continue;
                        }

                        string ncm = "", ncmTemp = "";
                        for (int j = 0; j < notesVenda[i].Count(); j++)
                        {
                            if (notesVenda[i][j].ContainsKey("NCM"))
                            {
                                status = false;
                                ncm = "";
                                ncmTemp = "";

                                for (int k = 0; k < ncms.Count; k++)
                                {
                                    int tamanho = ncms[k].Ncm.Length;


                                    if (tamanho < 8)
                                    {
                                        ncmTemp = notesVenda[i][j]["NCM"].Substring(0, tamanho);
                                    }
                                    else
                                    {
                                        ncmTemp = notesVenda[i][j]["NCM"];
                                    }

                                    if (ncms[k].Ncm.Equals(ncmTemp))
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
                                    if (ncmsForaAnexo[k][0].Equals(ncmTemp))
                                    {
                                        pos = k;
                                    }
                                }

                                if (!ncm.Equals(""))
                                {
                                    if (pos < 0)
                                    {
                                        var nn = ncmsAll.Where(_ => _.Code.Equals(ncm)).FirstOrDefault();
                                        ncmForaAnexo.Add(ncm);
                                        ncmForaAnexo.Add("0");
                                        ncmForaAnexo.Add("0");
                                        ncmForaAnexo.Add(nn.Description);
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

                                    if (notesVenda[i][j].ContainsKey("vICMSST"))
                                    {
                                        ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) + Convert.ToDecimal(notesVenda[i][j]["vICMSST"])).ToString();
                                        totalVendas += Convert.ToDecimal(notesVenda[i][j]["vICMSST"]);
                                    }

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

                                if (notesVenda[i][j].ContainsKey("vICMSST"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vICMSST"]);
                                }
                            }

                        }
                    }

                    decimal valorTotalNcm = 0, percentualTotal = 0;

                    List<List<string>> ncmsForaAnexoList = new List<List<string>>();

                    foreach (var ncm in ncmsForaAnexo)
                    {
                        if (!ncm[1].Equals("0"))
                        {
                            ncm[2] = ((Convert.ToDecimal(ncm[1]) * 100) / totalVendas).ToString();
                            valorTotalNcm += Convert.ToDecimal(ncm[1]);
                            percentualTotal += Convert.ToDecimal(ncm[2]);

                            List<string> ncmFora = new List<string>();
                            ncmFora.Add(ncm[0]);
                            ncmFora.Add(ncm[1]);
                            ncmFora.Add(ncm[2]);
                            ncmFora.Add(ncm[3]);
                            ncmsForaAnexoList.Add(ncmFora);

                        }
                    }

                    ViewBag.Ncm = ncmsForaAnexoList.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                    ViewBag.PercentualTotal = percentualTotal;
                    ViewBag.TotalVendas = totalVendas;
                    ViewBag.TotalNcm = valorTotalNcm;
                }
                else if (type.Equals("incentivo"))
                {
                    if (imp == null)
                    {
                        throw new Exception("Os dados para calcular ICMS não foram importados");
                    }

                    if (!comp.AnnexId.Equals(3))
                    {
                        var grupos = _grupoService.FindByGrupos(imp.Id);

                        if (comp.TypeCompany.Equals(true))
                        {

                            decimal creditosIcms = Convert.ToDecimal(imp.Credito), debitosIcms = Convert.ToDecimal(imp.Debito);

                            decimal naoContribuinteIncentivo = Convert.ToDecimal(imp.VendasNContribuinte), naoContriForaDoEstadoIncentivo = Convert.ToDecimal(imp.VendasNContribuinteFora),
                                vendaCfopSTContribuintesNIncentivo = Convert.ToDecimal(imp.ReceitaST1), ContribuinteIsento = Convert.ToDecimal(imp.ReceitaIsento1),
                                ContribuintesIncentivo = Convert.ToDecimal(imp.VendasContribuinte1), ContribuintesNIncentivo = Convert.ToDecimal(imp.ReceitaNormal1),
                                ContribuintesIncentivoAliqM25 = Convert.ToDecimal(imp.VendasContribuinte2), naoContribuinteNIncetivo = Convert.ToDecimal(imp.ReceitaNormal2),
                                vendaCfopSTNaoContribuinteNIncetivo = Convert.ToDecimal(imp.ReceitaST2), NaoContribuiteIsento = Convert.ToDecimal(imp.ReceitaIsento2),
                                naoContriForaDoEstadoNIncentivo = Convert.ToDecimal(imp.ReceitaNormal3), vendaCfopSTNaoContriForaDoEstadoNIncentivo = Convert.ToDecimal(imp.ReceitaST3),
                                NaoContribuinteForaDoEstadoIsento = Convert.ToDecimal(imp.ReceitaIsento3);


                            //Contribuinte
                            var icmsContribuinteIncentivo = Math.Round(Convert.ToDecimal(comp.Icms) * ContribuintesIncentivo / 100, 2);
                            var icmsContribuinteIncentivoAliqM25 = Math.Round(Convert.ToDecimal(comp.IcmsAliqM25) * ContribuintesIncentivoAliqM25 / 100, 2);

                            //Não Contribuinte
                            var totalVendasNContribuinte = Math.Round(naoContribuinteIncentivo + naoContribuinteNIncetivo + vendaCfopSTNaoContribuinteNIncetivo, 2);
                            var icmsNContribuinteIncentivo = Math.Round(Convert.ToDecimal(comp.IcmsNContribuinte) * naoContribuinteIncentivo / 100, 2);

                            //Não Contribuinte Fora do Estado
                            var totalVendasNContribuinteForaDoEstado = Math.Round(naoContriForaDoEstadoIncentivo + naoContriForaDoEstadoNIncentivo + vendaCfopSTNaoContriForaDoEstadoNIncentivo, 2);
                            var icmsNContribuinteForaDoEstado = Math.Round(Convert.ToDecimal(comp.IcmsNContribuinteFora) * totalVendasNContribuinteForaDoEstado / 100, 2);

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


                            //// Produtos Incentivados

                            //Contribuinte
                            ViewBag.VendaContribuinteIncentivo = ContribuintesIncentivo;
                            ViewBag.PercentualIcmsContrib = Convert.ToDecimal(comp.Icms);
                            ViewBag.ValorVendaContribIncentivo = icmsContribuinteIncentivo;
                            ViewBag.ContribuinteIsento = ContribuinteIsento;
                            ViewBag.ContribuinteIncentivoAliM25 = ContribuintesIncentivoAliqM25;
                            ViewBag.ValorVendaContribuinteAliM25 = icmsContribuinteIncentivoAliqM25;
                            ViewBag.PercentualIcmsAiqM25Contrib = Convert.ToDecimal(comp.IcmsAliqM25);
                            ViewBag.VendaSTContribuinte = vendaCfopSTContribuintesNIncentivo;


                            //Não Contribuinte
                            ViewBag.VendaNContribIncentivo = naoContribuinteIncentivo;
                            ViewBag.TotalVendaNContribuinte = totalVendasNContribuinte;
                            ViewBag.PercentualIcmsNContribuinte = Convert.ToDecimal(comp.IcmsNContribuinte);
                            ViewBag.ValorVendaNContribIncentivo = icmsNContribuinteIncentivo;
                            ViewBag.NaoContribuinteIsento = NaoContribuiteIsento;
                            ViewBag.VendaSTNContribuinte = vendaCfopSTNaoContribuinteNIncetivo;

                            //Não Contribuinte Fora do Estado
                            ViewBag.VendaNForaEstadoContribuinteIncetivo = naoContriForaDoEstadoIncentivo;
                            ViewBag.TotalVendaNContribuinteForaDoEstado = totalVendasNContribuinteForaDoEstado;
                            ViewBag.PercentualIcmsNaoContribForaDoEstado = Convert.ToDecimal(comp.IcmsNContribuinteFora);
                            ViewBag.ValorVendaNContribForaDoEstado = icmsNContribuinteForaDoEstado;
                            ViewBag.NaoContribuinteForaDoEstadoIsento = NaoContribuinteForaDoEstadoIsento;
                            ViewBag.VendaSTForaEstadoNContribuinte = vendaCfopSTNaoContriForaDoEstadoNIncentivo;

                            List<List<string>> icmsForaDoEstado = new List<List<string>>();

                            foreach (var g in grupos)
                            {
                                List<string> icmsFora = new List<string>();
                                icmsFora.Add(g.Uf);
                                icmsFora.Add(g.Icms.ToString());
                                icmsForaDoEstado.Add(icmsFora);

                            }

                            ViewBag.IcmsForaDoEstado = icmsForaDoEstado;

                            //// Produtos não incentivados

                            //Contribuinte
                            ViewBag.VendaContribuinteNIncentivo = ContribuintesNIncentivo;

                            //Não Contribuinte
                            ViewBag.VendaNContribuinteNIncentivo = naoContribuinteNIncetivo;

                            //Não Contribuinte Fora do Estado
                            ViewBag.VendaNContribuinteNIncentivoForaDoEstado = naoContriForaDoEstadoNIncentivo;

                            //// Crédito e Débito
                            //Crédito
                            ViewBag.Credito = creditosIcms;

                            //Débito
                            ViewBag.Debito = debitosIcms;

                            //Diferença
                            ViewBag.Diferenca = diferenca;


                            ////Total Icms
                            ViewBag.TotalIcms = totalIcms;


                            //// FUNEF e COTAC
                            ViewBag.BaseCalculo = baseCalculo;

                            //FUNEF
                            ViewBag.PercentualFunef = percentualFunef;
                            ViewBag.TotalFunef = totalFunef;

                            //COTAC
                            ViewBag.PercentualCotac = percentualCotac;
                            ViewBag.TotalCotac = totalCotac;

                            //Total Funef e Cotac
                            ViewBag.TotalFunefCotac = totalFunefCotac;

                            ////Total Imposto
                            ViewBag.TotalImposto = totalImposto;

                            ////Total Imposto Geral
                            ViewBag.TotalImpostoGeral = totalImpostoGeral;

                            //// Total
                            ViewBag.TotalVendaContibuinte = totalVendaContribuinte + ContribuinteIsento + ContribuintesIncentivoAliqM25;
                            ViewBag.TotalGeralVendaNContibuinte = totalVendasNContribuinte + NaoContribuiteIsento;
                            ViewBag.TotalGeralVendaNContibuinteForaDoEstado = totalVendasNContribuinteForaDoEstado + NaoContribuinteForaDoEstadoIsento;
                            ViewBag.TotalGeralIcmsIncentivo = totalIcmsGeralIncentivo;
                            ViewBag.TotalGeralVendasIncentivo = totalGeralVendasIncentivo;
                            ViewBag.Uf = comp.Uf;

                        }
                        else
                        {

                            decimal creditosIcms = creditosIcms = Convert.ToDecimal(imp.Credito);

                            decimal vendasIncentivada = Convert.ToDecimal(imp.VendasIncentivada), vendasNIncentivada = Convert.ToDecimal(imp.VendasNIncentivada),
                                debitoIncetivo = Convert.ToDecimal(grupos.Sum(_ => _.Icms)), debitoNIncentivo = Convert.ToDecimal(grupos.Sum(_ => _.IcmsNIncentivo));

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


                            List<List<string>> valoresIncentivo = new List<List<string>>();
                            List<List<string>> valoresNIncentivo = new List<List<string>>();

                            foreach (var g in grupos)
                            {
                                List<string> percentualIncentivo = new List<string>();
                                percentualIncentivo.Add(g.BaseCalculo.ToString());
                                percentualIncentivo.Add(g.Percentual.ToString());
                                percentualIncentivo.Add(g.Icms.ToString());
                                valoresIncentivo.Add(percentualIncentivo);

                                List<string> percentualNIncentivo = new List<string>();
                                percentualNIncentivo.Add(g.BaseCalculoNIncentivo.ToString());
                                percentualNIncentivo.Add(g.PercentualNIncentivo.ToString());
                                percentualNIncentivo.Add(g.IcmsNIncentivo.ToString());
                                valoresNIncentivo.Add(percentualNIncentivo);
                            }


                            //Incentivado
                            ViewBag.ValoresIncentivo = valoresIncentivo;
                            ViewBag.DebitoIncentivo = debitoIncetivo;
                            ViewBag.TotalVendas = totalVendas;
                            ViewBag.TotalVendasIncentivadas = vendasIncentivada;


                            //Não Incentivado
                            ViewBag.ValoresNIncentivo = valoresNIncentivo;
                            ViewBag.PercentualCreditoNIncentivo = percentualCreditoNIncentivado;
                            ViewBag.CreditoNIncentivo = creditoNIncentivado;
                            ViewBag.DebitoNIncentivo = debitoNIncentivo;
                            ViewBag.TotalVendas = totalVendas;
                            ViewBag.TotalVendasNIncentivadas = vendasNIncentivada;


                            // Total
                            ViewBag.TotalVendas = totalVendas;
                            ViewBag.Credito = creditosIcms;
                            ViewBag.TotalVendas = totalVendas;


                            //Apuração Normal
                            //Debito - ViewBag.DebitoIncentivo
                            ViewBag.CreditoIncentivo = creditosIcms;
                            ViewBag.DifApuNormal = difApuNormal;

                            //Apuração ñ Incentivada
                            //Debito - ViewBag.DebitoNIncetivo
                            //Credito - CreditoNIncentivo
                            ViewBag.DifApuNNormal = difApuNNormal;

                            // Funef e COTAC
                            // DifNormal - DifNIncentivada
                            ViewBag.BaseDeCalcFunef = baseDeCalcFunef;
                            ViewBag.PercentFunef = Convert.ToDecimal(comp.Funef);
                            ViewBag.ValorFunef = valorFunef;
                            ViewBag.PercentCotac = Convert.ToDecimal(comp.Cotac);
                            ViewBag.ValorCotac = valorCotac;

                            // Total De Imposto
                            ViewBag.TotalDeImposto = totalImposto;
                        }
                    }
                    else if (comp.AnnexId.Equals(3))
                    {

                        decimal totalDarFecop = 0, totalDarIcms = 0;

                        if (comp.SectionId.Equals(2))
                        {

                            decimal vendasInternasElencadas = Convert.ToDecimal(imp.VendasInternas1), vendasInterestadualElencadas = Convert.ToDecimal(imp.VendasInterestadual1),
                                vendasInternasDeselencadas = Convert.ToDecimal(imp.VendasInternas2), vendasInterestadualDeselencadas = Convert.ToDecimal(imp.VendasInterestadual2),
                                InternasElencadas = Convert.ToDecimal(imp.SaidaInterna1), InterestadualElencadas = Convert.ToDecimal(imp.SaidaInterestadual1),
                                InternasElencadasPortaria = Convert.ToDecimal(imp.SaidaPortInterna1), InterestadualElencadasPortaria = Convert.ToDecimal(imp.SaidaPortInterestadual1),
                                InternasDeselencadas = Convert.ToDecimal(imp.SaidaInterna2), InterestadualDeselencadas = Convert.ToDecimal(imp.SaidaInterestadual2),
                                InternasDeselencadasPortaria = Convert.ToDecimal(imp.SaidaPortInterna2),
                                InterestadualDeselencadasPortaria = Convert.ToDecimal(imp.SaidaPortInterestadual2),
                                suspensao = Convert.ToDecimal(imp.Suspensao), vendasClienteCredenciado = Convert.ToDecimal(imp.VendasClientes),
                                vendas = vendasInternasElencadas + vendasInterestadualElencadas + vendasInternasDeselencadas + vendasInterestadualDeselencadas;


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
                                if (notifi != null)
                                {
                                    _notificationService.Delete(notifi.Id, GetLog(Model.OccorenceLog.Delete));
                                }
                            }

                            //  Suspensão
                            decimal totalSuspensao = (suspensao * Convert.ToDecimal(comp.Suspension)) / 100;
                            totalDarIcms += totalSuspensao;


                            //  Elencadas
                            // Internas
                            ViewBag.VendasInternasElencadas = vendasInternasElencadas;
                            ViewBag.InternasElencadas = InternasElencadas;
                            ViewBag.InternasElencadasPortaria = InternasElencadasPortaria;
                            ViewBag.IcmsInternasElencadas = icmsInternaElencada;
                            ViewBag.FecopInternasElencadas = fecopInternaElencada;
                            ViewBag.TotalInternasElencadas = totalInternasElencada;
                            ViewBag.IcmsPresumidoInternasElencadas = icmsPresumidoInternaElencada;
                            ViewBag.TotalIcmsInternasElencadas = totalIcmsInternaElencada;

                            // Interestadual
                            ViewBag.VendasInterestadualElencadas = vendasInterestadualElencadas;
                            ViewBag.InterestadualElencadas = InterestadualElencadas;
                            ViewBag.InterestadualElencadasPortaria = InterestadualElencadasPortaria;
                            ViewBag.IcmsInterestadualElencadas = icmsInterestadualElencada;
                            ViewBag.FecopInterestadualElencadas = fecopInterestadualElencada;
                            ViewBag.TotalInterestadualElencadas = totalInterestadualElencada;
                            ViewBag.IcmsPresumidoInterestadualElencadas = icmsPresumidoInterestadualElencada;
                            ViewBag.TotalIcmsInterestadualElencadas = totalIcmsInterestadualElencada;

                            //  Deselencadas
                            //  Internas
                            ViewBag.VendasInternasDeselencadas = vendasInternasDeselencadas;
                            ViewBag.InternasDeselencadas = InternasDeselencadas;
                            ViewBag.InternasDeselencadasPortaria = InternasDeselencadasPortaria;
                            ViewBag.IcmsInternasElencadas = icmsInternaElencada;
                            ViewBag.IcmsInternasDeselencadas = icmsInternaDeselencada;
                            ViewBag.FecopInternasDeselencadas = fecopInternaDeselencada;
                            ViewBag.TotalInternasDeselencadas = totalInternasDeselencada;
                            ViewBag.IcmsPresumidoInternasDeselencadas = icmsPresumidoInternaDeselencada;
                            ViewBag.TotalIcmsInternasDeselencadas = totalIcmsInternaDeselencada;


                            // Interestadual
                            ViewBag.VendasInterestadualDeselencadas = vendasInterestadualDeselencadas;
                            ViewBag.InterestadualDeselencadas = InterestadualDeselencadas;
                            ViewBag.InterestadualDeselencadasPortaria = InterestadualDeselencadasPortaria;
                            ViewBag.IcmsInterestadualDeselencadas = icmsInterestadualDeselencada;
                            ViewBag.FecopInterestadualDeselencadas = fecopInterestadualDeselencada;
                            ViewBag.TotalInterestadualDeselencadas = totalInterestadualDeselencada;
                            ViewBag.IcmsPresumidoInterestadualDeselencadas = icmsPresumidoInterestadualDeselencada;
                            ViewBag.TotalIcmsInterestadualDeselencadas = totalIcmsInterestadualDeselencada;


                            //  Percentual
                            ViewBag.PercentualVendas = percentualVendas;

                            //  Suspensão
                            ViewBag.PercentualSuspensao = Convert.ToDecimal(comp.Suspension);
                            ViewBag.Suspensao = suspensao;
                            ViewBag.TotalSuspensao = totalSuspensao;
                        }

                        ViewBag.AliqInterna = Convert.ToDecimal(comp.AliqInterna);
                        ViewBag.Fecop = Convert.ToDecimal(comp.Fecop);

                        //  Elencadas
                        ViewBag.IncIInterna = Convert.ToDecimal(comp.IncIInterna);
                        ViewBag.IncIInterestadual = Convert.ToDecimal(comp.IncIInterestadual);

                        //  Deselencadas
                        ViewBag.IncIIInterna = Convert.ToDecimal(comp.IncIIInterna);
                        ViewBag.IncIIInterestadual = Convert.ToDecimal(comp.IncIIInterestadual);

                        // Geral
                        ViewBag.TotalDarFecop = totalDarFecop;
                        ViewBag.TotalDarIcms = totalDarIcms;

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
                    decimal totalNota = 0, valorContabil = 0, baseIcms = 0, valorIcms = 0, valorFecop = 0;
                    ViewBag.Code = cfop.Code;
                    List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

                    if (opcao.Equals("saida"))
                    {
                        notes = importXml.NfeExit(directoryNfeExit, cfop.Code);
                    }
                    else
                    {
                        notes = importXml.NfeExit(directoryNfeEntrada, cfop.Code);
                    }


                    List<List<string>> resumoNote = new List<List<string>>();

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (opcao.Equals("saida"))
                        {
                            if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i].Count() <= 5)
                            {
                                notes.RemoveAt(i);
                                continue;
                            }
                        }
                        else
                        {
                            if (!notes[i][3]["CNPJ"].Equals(comp.Document) || notes[i].Count() <= 5)
                            {
                                notes.RemoveAt(i);
                                continue;
                            }
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
                            note.Add(notes[i][1]["dhEmi"]);
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


                    ViewBag.Cfop = resumoNote.OrderBy(_ => Convert.ToInt32(_[1])).ToList();
                    ViewBag.TotalNota = totalNota;
                    ViewBag.ValorContabil = valorContabil;
                    ViewBag.BaseIcms = baseIcms;
                    ViewBag.ValorIcms = valorIcms;
                    ViewBag.ValorFecop = valorFecop;
                }
                else if (type.Equals("suspensao"))
                {
                    List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                    List<List<string>> notes = new List<List<string>>();

                    var suspensions = _suspensionService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).ToList();

                    exitNotes = importXml.Nfe(directoryNfeExit);

                    decimal valorTotal = 0;

                    var cfops = _companyCfopService.FindAll(null).Where(_ => _.CompanyId.Equals(id) && _.Active.Equals(true) && (_.CfopTypeId.Equals(1) || _.CfopTypeId.Equals(2) || _.CfopTypeId.Equals(5) || _.CfopTypeId.Equals(4))).Select(_ => _.Cfop.Code).ToList();

                    List<List<string>> periodos = new List<List<string>>();

                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                    {

                        if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            exitNotes.RemoveAt(i);
                            continue;
                        }
                        List<string> note = new List<string>();
                        bool suspenso = false, cfop = false;

                        if (exitNotes[i][1].ContainsKey("dhEmi"))
                        {
                            foreach (var suspension in suspensions)
                            {
                                if (Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) >= Convert.ToDateTime(suspension.DateStart) && Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) < Convert.ToDateTime(suspension.DateEnd))
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
                                        periodo.Add(Convert.ToDateTime(suspension.DateStart).ToString("dd/MM/yyyy hh:mm:ss"));
                                        periodo.Add(Convert.ToDateTime(suspension.DateEnd).ToString("dd/MM/yyyy hh:mm:ss"));
                                        periodos.Add(periodo);
                                    }
                                    suspenso = true;
                                    break;
                                }
                            }
                        }


                        if (suspenso == true)
                        {
                            note.Add(exitNotes[i][1]["natOp"]);
                            note.Add(exitNotes[i][1]["mod"]);
                            note.Add(exitNotes[i][1]["nNF"]);
                            note.Add(exitNotes[i][1]["dhEmi"]);
                        }


                        for (int k = 0; k < exitNotes[i].Count(); k++)
                        {
                            if (exitNotes[i][k].ContainsKey("CFOP"))
                            {
                                if (cfops.Contains(exitNotes[i][k]["CFOP"]))
                                {
                                    cfop = true;
                                }
                            }

                            if (exitNotes[i][k].ContainsKey("vNF"))
                            {
                                if (suspenso == true && cfop == true)
                                {
                                    note.Add(exitNotes[i][k]["vNF"]);
                                    valorTotal += Convert.ToDecimal(exitNotes[i][k]["vNF"]);
                                    notes.Add(note);
                                }
                            }

                        }

                    }

                    ViewBag.Notes = notes.OrderBy(_ => Convert.ToInt32(_[2])).ToList();
                    ViewBag.Periodos = periodos.OrderBy(_ => Convert.ToDateTime(_[1])).ToList();
                    ViewBag.Total = valorTotal;

                }
                else if (type.Equals("anexoAutoPecas"))
                {
                    List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();

                    var notes = _noteService.FindByNotes(id, year, month);
                    var products = _itemService.FindByProductsType(notes, Model.TypeTaxation.AP);

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

                    exitNotes = importXml.Nfe(directoryNfeExit);
                    entryNotes = importXml.Nfe(directoryNfeEntrada);

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

                    cfopsVenda = _companyCfopService.FindAll(null).Where(_ => _.CompanyId.Equals(id) && _.Active.Equals(true) && (_.CfopTypeId.Equals(1) || _.CfopTypeId.Equals(2) || _.CfopTypeId.Equals(4) || _.CfopTypeId.Equals(5))).Select(_ => _.Cfop.Code).ToList();
                    var cfopsDevo = _companyCfopService.FindAll(null).Where(_ => _.CompanyId.Equals(id) && _.Active.Equals(true) && (_.CfopTypeId.Equals(3) || _.CfopTypeId.Equals(7))).Select(_ => _.Cfop.Code).ToList();

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
                        _creditBalanceService.Update(creditCurrent, GetLog(Model.OccorenceLog.Update));
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

                    exitNotes = importXml.Nfe(directoryNfeExit);

                    decimal valorElencadaInterna = 0, valorElencadaInterestadual = 0, valorDaselencadaInterna = 0, valorDaselencadaInterestadual = 0;

                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                    {
                        if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document) || exitNotes[i][1]["finNFe"] == "4")
                        {
                            exitNotes.RemoveAt(i);
                            continue;
                        }

                        bool clenteCredenciado = false, ncm = false;

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

                        decimal valorNF = 0;

                        for (int j = 0; j < exitNotes[i].Count; j++)
                        {
                            if (exitNotes[i][j].ContainsKey("NCM"))
                            {
                                ncm = _itemService.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId), exitNotes[i][j]["NCM"].ToString());
                            }

                            if (ncm == true)
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

                        if (valorNF > 0)
                        {
                            List<string> note = new List<string>();

                            note.Add(exitNotes[i][1]["nNF"]);
                            note.Add(exitNotes[i][3]["xNome"]);
                            note.Add(exitNotes[i][3]["UF"]);
                            note.Add(CNPJ);
                            note.Add(IE);
                            note.Add(valorNF.ToString());
                            note.Add("0");

                            if (clenteCredenciado == true)
                            {
                                if (exitNotes[i][1]["idDest"] == "1")
                                {
                                    // Internas
                                    note.Add(comp.IncIInterna.ToString());
                                    elencadaInterna.Add(note);
                                    valorElencadaInterna += valorNF;
                                }
                                else
                                {
                                    // Interestadual
                                    note.Add(comp.IncIInterestadual.ToString());
                                    elencadaInterestadual.Add(note);
                                    valorElencadaInterestadual += valorNF;
                                }
                            }
                            else
                            {
                                if (exitNotes[i][1]["idDest"] == "1")
                                {
                                    // Internas
                                    note.Add(comp.IncIIInterna.ToString());
                                    daselencadaInterna.Add(note);
                                    valorDaselencadaInterna += valorNF;
                                }
                                else
                                {
                                    // Interestadual
                                    note.Add(comp.IncIIInterestadual.ToString());
                                    daselencadaInterestadual.Add(note);
                                    valorDaselencadaInterestadual += valorNF;
                                }
                            }
                        }


                        valorNF = 0;
                    }

                    //  Elencadas
                    decimal valorCreditoElencadaInterna = (valorElencadaInterna * Convert.ToDecimal(comp.IncIInterna)) / 100;
                    decimal valorCreditoElencadaInterestadual = (valorElencadaInterestadual * Convert.ToDecimal(comp.IncIInterestadual)) / 100;

                    //  Daselencada
                    decimal valorCreditoDaselencadaInterna = (valorDaselencadaInterna * Convert.ToDecimal(comp.IncIIInterna)) / 100;
                    decimal valorCreditoDaselencadaInterestadual = (valorDaselencadaInterestadual * Convert.ToDecimal(comp.IncIIInterestadual)) / 100;

                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                    //  Elencadas
                    // Interna
                    for (int i = 0; i < elencadaInterna.Count(); i++)
                    {
                        elencadaInterna[i][6] = ((Convert.ToDecimal(elencadaInterna[i][5].Replace(".", ",")) * Convert.ToDecimal(elencadaInterna[i][7].Replace(".", ","))) / 100).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        elencadaInterna[i][5] = Convert.ToDecimal(elencadaInterna[i][5].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        elencadaInterna[i][7] = elencadaInterna[i][7].Replace(".", ",");
                    }

                    //Interestadual
                    for (int i = 0; i < elencadaInterestadual.Count(); i++)
                    {
                        elencadaInterestadual[i][6] = ((Convert.ToDecimal(elencadaInterestadual[i][5].Replace(".", ",")) * Convert.ToDecimal(elencadaInterestadual[i][7].Replace(".", ","))) / 100).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        elencadaInterestadual[i][5] = Convert.ToDecimal(elencadaInterestadual[i][5].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        elencadaInterestadual[i][7] = elencadaInterestadual[i][7].Replace(".", ",");
                    }

                    //  Daselencada
                    // Interna
                    for (int i = 0; i < daselencadaInterna.Count(); i++)
                    {
                        daselencadaInterna[i][6] = ((Convert.ToDecimal(daselencadaInterna[i][5].Replace(".", ",")) * Convert.ToDecimal(daselencadaInterna[i][7].Replace(".", ","))) / 100).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        daselencadaInterna[i][5] = Convert.ToDecimal(daselencadaInterna[i][5].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        daselencadaInterna[i][7] = daselencadaInterna[i][7].Replace(".", ",");
                    }

                    //  Elencadas
                    // Interna
                    ViewBag.ElencadaInterna = elencadaInterna;
                    ViewBag.ValorElencadaInterna = valorElencadaInterna.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorCreditoElencadaInterna = valorCreditoElencadaInterna.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualElencadaInterna = comp.IncIInterna.ToString().Replace(".", ",");

                    //Interestadual
                    ViewBag.ElencadaInterestadual = elencadaInterestadual;
                    ViewBag.ValorElencadaInterestadual = valorElencadaInterestadual.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorCreditoElencadaInterestadual = valorCreditoElencadaInterestadual.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualElencadaInterestadual = comp.IncIInterestadual.ToString().Replace(".", ",");


                    //  Daselencada
                    // Interna
                    ViewBag.DaselencadaInterna = daselencadaInterna;
                    ViewBag.ValorDaselencadaInterna = valorDaselencadaInterna.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorCreditoDaselencadaInterna = valorCreditoDaselencadaInterna.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualDaselencadaInterna = comp.IncIIInterna.ToString().Replace(".", ",");

                    //Interestadual
                    ViewBag.DaselencadaInterestadual = daselencadaInterna;
                    ViewBag.ValorDaselencadaInterestadual = valorDaselencadaInterestadual.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorCreditoDaselencadaInterestadual = valorCreditoDaselencadaInterestadual.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualDaselencadaInterestadual = comp.IncIIInterestadual.ToString().Replace(".", ",");

                }
                else if (type.Equals("vendaforaincentivo"))
                {
                    List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                    notesVenda = importXml.Nfe(directoryNfeExit);

                    var ncms = _ncmConvenioService.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId));
                    var cfopsCompany = _companyCfopService.FindByCompany(id);

                    List<List<string>> cfopsForaAnexo = new List<List<string>>();
                    bool status = false;
                    decimal valorContabil = 0, valorBC = 0, valorIcms = 0, valorFecop = 0;

                    for (int i = notesVenda.Count - 1; i >= 0; i--)
                    {
                        if (!notesVenda[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notesVenda.RemoveAt(i);
                        }

                        string ncm = "", ncmTemp = "";

                        int pos = -1;

                        for (int j = 0; j < notesVenda[i].Count(); j++)
                        {
                            if (notesVenda[i][j].ContainsKey("NCM"))
                            {
                                status = false;
                                ncm = "";
                                ncmTemp = "";

                                for (int k = 0; k < ncms.Count; k++)
                                {
                                    int tamanho = ncms[k].Ncm.Length;


                                    if (tamanho < 8)
                                    {
                                        ncmTemp = notesVenda[i][j]["NCM"].Substring(0, tamanho);
                                    }
                                    else
                                    {
                                        ncmTemp = notesVenda[i][j]["NCM"];
                                    }

                                    if (ncms[k].Ncm.Equals(ncmTemp))
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
                                if (notesVenda[i][j].ContainsKey("CFOP"))
                                {

                                    pos = -1;
                                    for (int k = 0; k < cfopsForaAnexo.Count(); k++)
                                    {
                                        if (cfopsForaAnexo[k][0].Equals(notesVenda[i][j]["CFOP"]))
                                        {
                                            pos = k;
                                        }
                                    }

                                    if (pos < 0)
                                    {
                                        List<string> cfopForaAnexo = new List<string>();

                                        var cc = cfopsCompany.Where(_ => _.Cfop.Code.Equals(notesVenda[i][j]["CFOP"])).FirstOrDefault();
                                        cfopForaAnexo.Add(notesVenda[i][j]["CFOP"]);
                                        cfopForaAnexo.Add(cc.Cfop.Description);
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopsForaAnexo.Add(cfopForaAnexo);
                                        pos = cfopsForaAnexo.Count() - 1;
                                    }

                                }

                                if (notesVenda[i][j].ContainsKey("vProd") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) + Convert.ToDecimal(notesVenda[i][j]["vProd"])).ToString();
                                    valorContabil += Convert.ToDecimal(notesVenda[i][j]["vProd"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vFrete") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) + Convert.ToDecimal(notesVenda[i][j]["vFrete"])).ToString();
                                    valorContabil += Convert.ToDecimal(notesVenda[i][j]["vFrete"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vDesc") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) - Convert.ToDecimal(notesVenda[i][j]["vDesc"])).ToString();
                                    valorContabil -= Convert.ToDecimal(notesVenda[i][j]["vDesc"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vOutro") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) + Convert.ToDecimal(notesVenda[i][j]["vOutro"])).ToString();
                                    valorContabil += Convert.ToDecimal(notesVenda[i][j]["vOutro"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vSeg") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) + Convert.ToDecimal(notesVenda[i][j]["vSeg"])).ToString();
                                    valorContabil += Convert.ToDecimal(notesVenda[i][j]["vSeg"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vBC") && notesVenda[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexo[pos][3] = (Convert.ToDecimal(cfopsForaAnexo[pos][3]) + Convert.ToDecimal(notesVenda[i][j]["vBC"])).ToString();
                                    valorBC += Convert.ToDecimal(notesVenda[i][j]["vBC"]);
                                }

                                if (notesVenda[i][j].ContainsKey("pICMS") && notesVenda[i][j].ContainsKey("CST") && notesVenda[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexo[pos][4] = (Convert.ToDecimal(cfopsForaAnexo[pos][4]) + ((Convert.ToDecimal(notesVenda[i][j]["pICMS"]) * Convert.ToDecimal(notesVenda[i][j]["vBC"])) / 100)).ToString();
                                    valorIcms += (Convert.ToDecimal(notesVenda[i][j]["pICMS"]) * Convert.ToDecimal(notesVenda[i][j]["vBC"])) / 100;
                                }

                                if (notesVenda[i][j].ContainsKey("pFCP") && notesVenda[i][j].ContainsKey("CST") && notesVenda[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexo[pos][5] = (Convert.ToDecimal(cfopsForaAnexo[pos][5]) + ((Convert.ToDecimal(notesVenda[i][j]["pFCP"]) * Convert.ToDecimal(notesVenda[i][j]["vBC"])) / 100)).ToString();
                                    valorFecop += (Convert.ToDecimal(notesVenda[i][j]["pFCP"]) * Convert.ToDecimal(notesVenda[i][j]["vBC"])) / 100;
                                }

                            }

                        }
                    }

                    ViewBag.CfopNIncentivo = cfopsForaAnexo.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                }
                else if (type.Equals("produtoST"))
                {
                    List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

                    notes = importXml.Nfe(directoryNfeExit);

                    var codeProd = _productIncentivoService.FindByAllProducts(id).Select(_ => _.Code).ToList();
                    var codeProdST = _productIncentivoService.FindByAllProducts(id).Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Code).ToList();

                    var cestProd = _productIncentivoService.FindByAllProducts(id).Select(_ => _.Cest).ToList();
                    var cestST = _productIncentivoService.FindByAllProducts(id).Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Cest).ToList();

                    List<List<string>> produtos = new List<List<string>>();

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i][1]["tpNF"].Equals("0") || notes[i][1]["finNFe"] == "4")
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        decimal valorProduto = 0;
                        string cProd = null, cest = null, xProd = "";

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("cProd"))
                            {
                                cProd = notes[i][j]["cProd"];
                                xProd = notes[i][j]["xProd"];
                                valorProduto = 0;
                                cest = "";

                                if (notes[i][j].ContainsKey("CEST"))
                                {
                                    cest = notes[i][j]["CEST"];
                                }

                                if (notes[i][j].ContainsKey("vProd"))
                                {
                                    valorProduto += Convert.ToDecimal(notes[i][j]["vProd"]);
                                }

                                if (notes[i][j].ContainsKey("vFrete"))
                                {
                                    valorProduto += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                }

                                if (notes[i][j].ContainsKey("vDesc"))
                                {
                                    valorProduto -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }

                                if (notes[i][j].ContainsKey("vOutro"))
                                {
                                    valorProduto += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }

                                if (notes[i][j].ContainsKey("vSeg"))
                                {
                                    valorProduto += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }
                            }

                            if ((notes[i][j].ContainsKey("pICMS") || notes[i][j].ContainsKey("pICMSST")) && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                            {
                                if (codeProd.Contains(cProd) && cestProd.Contains(cest))
                                {
                                    if (codeProdST.Contains(cProd) && cestST.Contains(cest))
                                    {
                                        List<string> produto = new List<string>();
                                        produto.Add(notes[i][1]["nNF"]);
                                        produto.Add(notes[i][1]["dhEmi"]);
                                        produto.Add(notes[i][3]["UF"]);

                                        if (notes[i][3].ContainsKey("CNPJ"))
                                        {
                                            produto.Add("CNPJ");
                                        }
                                        else
                                        {
                                            produto.Add("CPF");
                                        }

                                        produto.Add(cProd);
                                        produto.Add(xProd);
                                        produto.Add(cest);
                                        produto.Add(notes[i][j]["vBC"]);
                                        produto.Add(notes[i][j]["vICMS"]);

                                        if (notes[i][j].ContainsKey("pFCP"))
                                        {
                                            produto.Add(notes[i][j]["vFCP"]);
                                        }
                                        else
                                        {
                                            produto.Add("0");
                                        }

                                        if (notes[i][j].ContainsKey("vBCST"))
                                        {
                                            produto.Add(notes[i][j]["vBCST"]);
                                        }
                                        else
                                        {
                                            produto.Add("0");
                                        }

                                        if (notes[i][j].ContainsKey("vICMSST"))
                                        {
                                            produto.Add(notes[i][j]["vICMSST"]);
                                        }
                                        else
                                        {
                                            produto.Add("0");
                                        }

                                        if (notes[i][j].ContainsKey("vFCPST"))
                                        {
                                            produto.Add(notes[i][j]["vFCPST"]);
                                        }
                                        else
                                        {
                                            produto.Add("0");
                                        }

                                        if (produto[3].Equals("CNPJ"))
                                        {
                                            if (!notes[i][2]["UF"].Equals(notes[i][3]["UF"]))
                                            {
                                                if (Convert.ToDecimal(notes[i][j]["vBC"]) > 0 && Convert.ToDecimal(produto[10]) <= 0)
                                                {
                                                    produto.Add("C");
                                                }
                                                else
                                                {
                                                    produto.Add("E");
                                                }
                                            }
                                            else
                                            {
                                                if (Convert.ToDecimal(notes[i][j]["vBC"]) > 0 && Convert.ToDecimal(produto[10]) > 0)
                                                {
                                                    produto.Add("E");
                                                }
                                                else
                                                {
                                                    produto.Add("C");
                                                }
                                            }
                                        }

                                        if (produto[3].Equals("CPF"))
                                        {

                                            if (!notes[i][2]["UF"].Equals(notes[i][3]["UF"]))
                                            {
                                                if (Convert.ToDecimal(notes[i][j]["vBC"]) > 0 && Convert.ToDecimal(produto[10]) <= 0)
                                                {
                                                    produto.Add("C");
                                                }
                                                else
                                                {
                                                    produto.Add("E");
                                                }
                                            }
                                            else
                                            {
                                                if (notes[i][j].ContainsKey("vBCST"))
                                                {
                                                    if (Convert.ToDecimal(notes[i][j]["vBC"]) > 0 && Convert.ToDecimal(notes[i][j]["vBC"]) <= 0)
                                                    {
                                                        produto.Add("E");
                                                    }
                                                    else
                                                    {
                                                        produto.Add("C");
                                                    }
                                                }
                                                else
                                                {
                                                    produto.Add("E");
                                                }

                                                
                                            }
                                        }

                                        produtos.Add(produto);
                                    }
                                }
                                else
                                {
                                    throw new Exception("Há Produtos não Tributado");
                                }
                            }
                        }

                    }

                    ViewBag.Produtos = produtos.OrderBy(_ => _[2]).ThenBy(_ => _[3]).ToList();
                }

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

    }
}