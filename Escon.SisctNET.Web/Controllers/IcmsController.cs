using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
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
        private readonly ITaxAnexoService _taxAnexoService;
        private readonly ICompraAnexoService _compraAnexoService;
        private readonly IDevoClienteService _devoClienteService;
        private readonly IDevoFornecedorService _devoFornecedorService;
        private readonly IVendaAnexoService _vendaAnexoService;

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
            ITaxAnexoService taxAnexoService,
            ICompraAnexoService compraAnexoService,
            IDevoClienteService devoClienteService,
            IDevoFornecedorService devoFornecedorService,
            IVendaAnexoService vendaAnexoService,
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
            _taxAnexoService = taxAnexoService;
            _compraAnexoService = compraAnexoService;
            _devoClienteService = devoClienteService;
            _devoFornecedorService = devoFornecedorService;
            _vendaAnexoService = vendaAnexoService;
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

                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);
                ViewBag.Company = comp;

                var NfeExit = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));
                var NfeEntrada = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                var importXml = new Xml.Import(_companyCfopService);
                var importSped = new Sped.Import(_companyCfopService);
                var importMes = new Period.Month();

                string directoryNfeExit = NfeExit.Value + "\\" + comp.Document + "\\" + year + "\\" + month;
                string directoryNfeEntrada = NfeEntrada.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                ViewBag.Type = type;
                ViewBag.Opcao = opcao;

                var imp = _taxService.FindByMonth(id, month, year);
                var impAnexo = _taxAnexoService.FindByMonth(id, month, year);

                var cfopsDevoCompra = _companyCfopService.FindByCfopDevoCompra(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();
                var cfopsDevoVenda = _companyCfopService.FindByCfopDevoVenda(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();
                var cfopsCompra = _companyCfopService.FindByCfopCompra(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();
                var cfopsCompraST = _companyCfopService.FindByCfopCompraST(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();
                var cfopsVendaST = _companyCfopService.FindByCfopVendaST(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsVenda = _companyCfopService.FindByCfopVenda(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsTransf = _companyCfopService.FindByCfopTransferencia(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsTransfST = _companyCfopService.FindByCfopTransferenciaST(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsBoniVenda = _companyCfopService.FindByCfopBonificacaoVenda(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsBoniCompra = _companyCfopService.FindByCfopBonificacaoCompra(comp.Document).Select(_ => _.Cfop.Code).ToList();

                if (type.Equals("resumoCfop"))
                {
                    List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
                    List<List<string>> cfops = new List<List<string>>();

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
                                  
                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                   
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                   
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                   
                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfops[pos][3] = (Convert.ToDecimal(cfops[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfops[pos][4] = (Convert.ToDecimal(cfops[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                  
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

                                    }

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    }

                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfops[pos][3] = (Convert.ToDecimal(cfops[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfops[pos][4] = (Convert.ToDecimal(cfops[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                }

                            }

                        }

                        ViewBag.Cfop = cfops.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                    }

                }
                else if (type.Equals("resumoCfopCst"))
                {
                    List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
                    List<List<string>> cfops = new List<List<string>>();

                    notes = importXml.Nfe(directoryNfeExit);

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        string cfop = "";
                        decimal vProd = 0;
                        int pos = -1;

                        for (int j = 0; j < notes[i].Count(); j++)
                        {

                            if ((notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM")))
                            {
                                pos = -1;
                                cfop = notes[i][j]["CFOP"];

                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    vProd = 0;
                                    vProd += Convert.ToDecimal(notes[i][j]["vProd"]);

                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    vProd += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    vProd -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    vProd += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    vProd += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }
                            }


                            if (notes[i][j].ContainsKey("CST"))
                            {

                                for (int e = 0; e < cfops.Count(); e++)
                                {
                                    if (cfops[e][0].Equals(cfop) && cfops[e][2].Equals(notes[i][j]["CST"]))
                                    {
                                        pos = e;
                                        break;
                                    }
                                }

                                if (pos < 0)
                                {
                                    var cfp = _cfopService.FindByCode(cfop);
                                    List<string> cc = new List<string>();
                                    cc.Add(cfop);
                                    cc.Add(cfp.Description);
                                    cc.Add(notes[i][j]["CST"]);
                                    cc.Add("0");
                                    cc.Add("0");
                                    cc.Add("0");
                                    cc.Add("0");
                                    cc.Add("0");
                                    cfops.Add(cc);
                                    pos = cfops.Count() - 1;
                                }
                                cfops[pos][4] = (Convert.ToDecimal(cfops[pos][4]) + vProd).ToString();
                            }

                            if (notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("pICMS"))
                            {
                                cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();

                            }

                            if (notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("pFCP"))
                            {
                                cfops[pos][7] = (Convert.ToDecimal(cfops[pos][7]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                            }

                            if (notes[i][j].ContainsKey("CSOSN"))
                            {

                                for (int e = 0; e < cfops.Count(); e++)
                                {
                                    if (cfops[e][0].Equals(cfop) && cfops[e][2].Equals(notes[i][j]["CSOSN"]))
                                    {
                                        pos = e;
                                        break;
                                    }
                                }

                                if (pos < 0)
                                {
                                    var cfp = _cfopService.FindByCode(cfop);
                                    List<string> cc = new List<string>();
                                    cc.Add(cfop);
                                    cc.Add(cfp.Description);
                                    cc.Add(notes[i][j]["CSOSN"]);
                                    cc.Add("0");
                                    cc.Add("0");
                                    cc.Add("0");
                                    cc.Add("0");
                                    cc.Add("0");
                                    cfops.Add(cc);
                                    pos = cfops.Count() - 1;
                                }
                                cfops[pos][4] = (Convert.ToDecimal(cfops[pos][4]) + vProd).ToString();
                            }

                            if (notes[i][j].ContainsKey("CSOSN") && notes[i][j].ContainsKey("pICMS"))
                            {
                                cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();

                            }

                            if (notes[i][j].ContainsKey("CSOSN") && notes[i][j].ContainsKey("pFCP"))
                            {
                                cfops[pos][7] = (Convert.ToDecimal(cfops[pos][7]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                            }
                        }

                    }

                    ViewBag.Cfop = cfops.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                }
                else if (type.Equals("resumoPorCfop"))
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
                                resumoNote[pos][4] = (Convert.ToDecimal(resumoNote[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                valorIcms += Convert.ToDecimal(notes[i][j]["vICMS"]);
                            }

                            if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                            {
                                resumoNote[pos][5] = (Convert.ToDecimal(resumoNote[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                valorFecop += Convert.ToDecimal(notes[i][j]["vFCP"]);
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
                else if (type.Equals("anexo"))
                {
                    ViewBag.Anexo = comp.Annex.Description + " - " + comp.Annex.Convenio;

                    List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();

                    cfopsVenda.AddRange(cfopsVendaST);
                    cfopsVenda.AddRange(cfopsTransf);
                    cfopsVenda.AddRange(cfopsTransfST);
                    cfopsVenda.AddRange(cfopsBoniVenda);

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

                    cfopsVenda.AddRange(cfopsVendaST);
                    cfopsVenda.AddRange(cfopsTransf);
                    cfopsVenda.AddRange(cfopsTransfST);
                    cfopsVenda.AddRange(cfopsBoniVenda);

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
                else if (type.Equals("suspensao"))
                {
                    List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                    List<List<string>> notes = new List<List<string>>();

                    var suspensions = _suspensionService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).ToList();

                    cfopsVenda.AddRange(cfopsVendaST);
                    cfopsVenda.AddRange(cfopsTransf);
                    cfopsVenda.AddRange(cfopsTransfST);
                    cfopsVenda.AddRange(cfopsBoniVenda);

                    exitNotes = importXml.NfeExit(directoryNfeExit, cfopsVenda);

                    decimal valorTotal = 0;                    

                    List<List<string>> periodos = new List<List<string>>();

                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                    {

                        if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document) || exitNotes[i][1]["finNFe"] == "4")
                        {
                            exitNotes.RemoveAt(i);
                            continue;
                        }
                        List<string> note = new List<string>();
                        bool suspenso = false;
                        decimal vProd = 0;

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

                            if (exitNotes[i][k].ContainsKey("vProd") && exitNotes[i][k].ContainsKey("cProd"))
                            {
                                vProd += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                            }

                            if (exitNotes[i][k].ContainsKey("vFrete") && exitNotes[i][k].ContainsKey("cProd"))
                            {

                                 vProd += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);

                            }

                            if (exitNotes[i][k].ContainsKey("vDesc") && exitNotes[i][k].ContainsKey("cProd"))
                            {
                                vProd -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);

                            }

                            if (exitNotes[i][k].ContainsKey("vOutro") && exitNotes[i][k].ContainsKey("cProd"))
                            {
                                vProd += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);

                            }

                            if (exitNotes[i][k].ContainsKey("vSeg") && exitNotes[i][k].ContainsKey("cProd"))
                            {
                                vProd += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                            }

                            if (exitNotes[i][k].ContainsKey("vNF") && suspenso == true)
                            {
                                note.Add(vProd.ToString());
                                valorTotal += vProd;
                                notes.Add(note);
                            }

                        }

                    }

                    ViewBag.Notes = notes.OrderBy(_ => Convert.ToInt32(_[2])).ToList();
                    ViewBag.Periodos = periodos.OrderBy(_ => Convert.ToDateTime(_[1])).ToList();
                    ViewBag.Total = valorTotal;

                }
                else if (type.Equals("foraIncentivo"))
                {
                    List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();

                    cfopsVenda.AddRange(cfopsVendaST);
                    cfopsVenda.AddRange(cfopsTransf);
                    cfopsVenda.AddRange(cfopsTransfST);
                    cfopsVenda.AddRange(cfopsBoniVenda);

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
                else if (type.Equals("tributacaoDiveregente"))
                {
                    List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
                    List<List<string>> prodsXml = new List<List<string>>();

                    notes = importXml.Nfe(directoryNfeExit);

                    var prodsAllCompany = _productIncentivoService.FindByAllProducts(comp.Document);


                    var mes = importMes.NumberMonth(month);

                    DateTime data = Convert.ToDateTime("01" + "/" + mes + "/" + year);
                    var prodsAll = _productIncentivoService.FindByDate(prodsAllCompany, data);

                    var ncmsAll = _ncmService.FindAll(null);

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        string cProd = "", xProd = "", cfop = "", ncm = "", cest = "";

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("cProd"))
                            {
                                cProd = notes[i][j]["cProd"];
                                xProd = notes[i][j]["xProd"];
                                ncm = notes[i][j]["NCM"];
                                cfop = notes[i][j]["CFOP"];
                            }

                            if (notes[i][j].ContainsKey("CEST"))
                            {
                                cest = notes[i][j]["CEST"];
                            }

                            if (notes[i][j].ContainsKey("CST"))
                            {
                                var temp = prodsAllCompany.Where(_ => _.Code.Equals(cProd) && _.Ncm.Equals(ncm) && _.Cest.Equals(cest)).FirstOrDefault();

                                if (temp == null)
                                {
                                    ViewBag.Erro = 1;
                                    return View();
                                }

                                int pos = -1;

                                for (int k = 0; k < prodsXml.Count(); k++)
                                {
                                    if (prodsXml[k][0].Equals(cProd) && prodsXml[k][2].Equals(ncm) && prodsXml[k][3].Equals(cest) &&
                                        prodsXml[k][4].Equals(notes[i][j]["CST"]) && prodsXml[k][5].Equals(cfop))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                                if (pos < 0)
                                {
                                    List<string> prods = new List<string>();
                                    prods.Add(cProd);
                                    prods.Add(xProd);
                                    prods.Add(ncm);
                                    prods.Add(cest);
                                    prods.Add(notes[i][j]["CST"]);
                                    prods.Add(cfop);

                                    prodsXml.Add(prods);

                                }
                                cProd = "";
                                xProd = "";
                                ncm = "";
                                cest = "";
                            }

                            if (notes[i][j].ContainsKey("CSOSN"))
                            {
                                var temp = prodsAllCompany.Where(_ => _.Code.Equals(cProd) && _.Ncm.Equals(ncm) && _.Cest.Equals(cest)).FirstOrDefault();

                                if (temp == null)
                                {
                                    ViewBag.Erro = 1;
                                    return View();
                                }

                                int pos = -1;

                                for (int k = 0; k < prodsXml.Count(); k++)
                                {
                                    if (prodsXml[k][0].Equals(cProd) && prodsXml[k][2].Equals(ncm) && prodsXml[k][3].Equals(cest) &&
                                        prodsXml[k][4].Equals(notes[i][j]["CSOSN"]) && prodsXml[k][5].Equals(cfop))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                                if (pos < 0)
                                {
                                    List<string> prods = new List<string>();
                                    prods.Add(cProd);
                                    prods.Add(xProd);
                                    prods.Add(ncm);
                                    prods.Add(cest);
                                    prods.Add(notes[i][j]["CSOSN"]);
                                    prods.Add(cfop);

                                    prodsXml.Add(prods);

                                }
                                cProd = "";
                                xProd = "";
                                ncm = "";
                                cest = "";
                            }
                        }

                    }

                    List<List<string>> divergentes = new List<List<string>>();

                    foreach (var prodX in prodsXml)
                    {
                        bool achou = false;
                        foreach (var prodS in prodsAll)
                        {
                            string cst = "";
                            if (prodS.CstId != null)
                            {
                                cst = prodS.Cst.Code;
                            }

                            if (prodS.Code.Equals(prodX[0]) && prodS.Ncm.Equals(prodX[2]) &&
                               prodS.Cest.Equals(prodX[3]) && cst.Equals(prodX[4]))
                            {
                                achou = true;
                                break;
                            }
                        }

                        if (achou == false)
                        {
                            Model.ProductIncentivo temp = new Model.ProductIncentivo();
                            bool contem = false;

                            foreach (var prodS in prodsAll)
                            {
                                if (prodS.Code.Equals(prodX[0]) && prodS.Ncm.Equals(prodX[2]) &&
                                   prodS.Cest.Equals(prodX[3]))
                                {
                                    contem = true;
                                    temp = prodS;
                                    break;
                                }
                            }

                            if (contem == true)
                            {
                                List<string> divergente = new List<string>();

                                var ncmXTemp = ncmsAll.Where(_ => _.Code.Equals(prodX[2])).FirstOrDefault();

                                if (ncmXTemp == null)
                                {
                                    ViewBag.Ncm = prodX[2];
                                    ViewBag.Erro = 2;
                                    return View();
                                }

                                divergente.Add(prodX[0]);
                                divergente.Add(prodX[1]);
                                divergente.Add(prodX[2]);
                                divergente.Add(ncmXTemp.Description);
                                divergente.Add(prodX[3]);
                                divergente.Add(prodX[5]);
                                divergente.Add(prodX[4]);
                                

                                if (temp.CstId == null)
                                {
                                    divergente.Add("");
                                }
                                else
                                {
                                    divergente.Add(temp.Cst.Code);
                                }

                                divergentes.Add(divergente);
                            }
                        }

                    }

                    ViewBag.Divergentes = divergentes.OrderBy(_ => _[0]).ThenBy(_ => _[5]).ToList();
                }
                else if (type.Equals("venda"))
                {

                    if (imp == null)
                    {
                        ViewBag.Erro = 1;
                        return View();
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
                        totalImpostoGrupo = Math.Round((totalExcedente * Convert.ToDecimal(comp.VendaMGrupoExcedente)) / 100,2);
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
                        impostoNcm = Math.Round((excedenteNcm * Convert.ToDecimal(comp.VendaAnexoExcedente)) / 100,2);
                    }

                    //Não Contribuinte
                    if (baseCalcNContribuinte > limiteNContribuinte && limiteNContribuinte > 0)
                    {
                        excedenteNContribuinte = baseCalcNContribuinte - limiteNContribuinte;
                        impostoNContribuinte = Math.Round((excedenteNContribuinte * Convert.ToDecimal(comp.VendaCpfExcedente)) / 100,2);
                    }

                    // Transferência inter
                    if (limiteTransferencia < totalTranferenciaInter)
                    {
                        excedenteTranfInter = totalTranferenciaInter - limiteTransferencia;
                        impostoTransfInter = Math.Round((excedenteTranfInter * Convert.ToDecimal(comp.TransferenciaInterExcedente)) / 100,2);
                    }

                    // Suspensão
                    decimal valorSuspensao = Math.Round((totalVendasSuspensao * Convert.ToDecimal(comp.Suspension)) / 100,2);

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
                else if (type.Equals("incentivo"))
                {
                    if (imp == null)
                    {
                        ViewBag.Erro = 1;
                        return View();
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
                            decimal fecopInternaElencada = Math.Round((InternasElencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100,2);
                            decimal totalInternasElencada = icmsInternaElencada;
                            decimal icmsPresumidoInternaElencada = (InternasElencadasPortaria * Convert.ToDecimal(comp.IncIInterna)) / 100;
                            decimal totalIcmsInternaElencada = Math.Round(totalInternasElencada - icmsPresumidoInternaElencada,2);

                            totalDarFecop += fecopInternaElencada;
                            totalDarIcms += totalIcmsInternaElencada;

                            // Interestadual
                            decimal icmsInterestadualElencada = (InterestadualElencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                            decimal fecopInterestadualElencada =Math.Round((InterestadualElencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100,2);
                            decimal totalInterestadualElencada = icmsInterestadualElencada;
                            decimal icmsPresumidoInterestadualElencada = (InterestadualElencadasPortaria * Convert.ToDecimal(comp.IncIInterestadual)) / 100;
                            decimal totalIcmsInterestadualElencada = Math.Round(totalInterestadualElencada - icmsPresumidoInterestadualElencada,2);

                            totalDarFecop += fecopInterestadualElencada;
                            totalDarIcms += totalIcmsInterestadualElencada;

                            //  Deselencadas
                            //  Internas
                            decimal icmsInternaDeselencada = (InternasDeselencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                            decimal fecopInternaDeselencada = Math.Round((InternasDeselencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100,2);
                            decimal totalInternasDeselencada = icmsInternaDeselencada;
                            decimal icmsPresumidoInternaDeselencada = (InternasDeselencadasPortaria * Convert.ToDecimal(comp.IncIIInterna)) / 100;
                            decimal totalIcmsInternaDeselencada = Math.Round(totalInternasDeselencada - icmsPresumidoInternaDeselencada,2);

                            totalDarFecop += fecopInternaDeselencada;
                            totalDarIcms += totalIcmsInternaDeselencada;

                            // Interestadual
                            decimal icmsInterestadualDeselencada = (InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                            decimal fecopInterestadualDeselencada = Math.Round((InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100,2);
                            decimal totalInterestadualDeselencada = icmsInterestadualDeselencada;
                            decimal icmsPresumidoInterestadualDeselencada = (InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.IncIIInterestadual)) / 100;
                            decimal totalIcmsInterestadualDeselencada = Math.Round(totalInterestadualDeselencada - icmsPresumidoInterestadualDeselencada,2);

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
                            decimal totalSuspensao = Math.Round((suspensao * Convert.ToDecimal(comp.Suspension)) / 100,2);
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
                else if (type.Equals("anexoAutoPecas"))
                {
                    if (impAnexo == null)
                    {
                        ViewBag.Erro = 1;
                        return View();
                    }

                    var notes = _noteService.FindByNotes(id, year, month);
                    var products = _itemService.FindByProductsType(notes, Model.TypeTaxation.AP);

                    decimal totalFreteAPIE = 0;

                    foreach (var prod in products)
                    {
                        if (!prod.Note.Iest.Equals("") && prod.TaxationTypeId.Equals(1))
                        {
                            if (Convert.ToDecimal(prod.Diferencial) > 0)
                            {
                                totalFreteAPIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                            }
                        }
                    }

                    ViewBag.TotalFreteAPIE = totalFreteAPIE;

                    decimal valorNfe1NormalAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe1NormalAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal? gnrePagaAPIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreAp).Sum()), 2);
                    decimal? gnreNPagaAPIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNAp).Sum()), 2);
                    decimal? gnrePagaAPSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreAp).Sum()), 2);
                    decimal? gnreNPagaAPSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNAp).Sum()), 2);

                    decimal? icmsStAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalAPIE + valorNfe1RetAPIE + valorNfe2NormalAPIE + valorNfe2RetAPIE;
                    decimal? icmsStAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalAPSIE + valorNfe1RetAPSIE + valorNfe2NormalAPSIE + valorNfe2RetAPSIE;
                    decimal? totalApuradoAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalApuradoAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalDiefAPSIE = Convert.ToDecimal((totalApuradoAPSIE + totalFreteAPIE) - icmsStAPSIE + gnreNPagaAPSIE - gnrePagaAPSIE);
                    decimal? totalDiefAPIE = Convert.ToDecimal(totalApuradoAPIE + gnreNPagaAPIE - icmsStAPIE - gnrePagaAPIE - totalFreteAPIE);
                    int? qtdAPSIE = products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Count();
                    int? qtdAPIE = products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Count();

                    ViewBag.QtdAPSIE = qtdAPSIE;
                    ViewBag.QtdAPIE = qtdAPIE;
                    ViewBag.TotatlApuradoAPIE = totalApuradoAPIE;
                    ViewBag.TotatlApuradoAPSIE = totalApuradoAPSIE;
                    ViewBag.TotalIcmsPagoAPIE = icmsStAPIE;
                    ViewBag.TotalIcmsPagoAPSIE = icmsStAPSIE;

                    ViewBag.GnrePagaAPSIE = gnrePagaAPSIE;
                    ViewBag.GnrePagaAPIE = gnrePagaAPIE;
                    ViewBag.GnreNPagaAPSIE = gnreNPagaAPSIE;
                    ViewBag.GnreNPagaAPIE = gnreNPagaAPIE;

                    ViewBag.TotalDiefAPSIE = totalDiefAPSIE;
                    ViewBag.TotalDiefAPIE = totalDiefAPIE;

                    decimal icmsAPnotaIE = Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsAp).Sum());
                    decimal icmsAPnotaSIE = Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsAp).Sum());
                    ViewBag.IcmsAPIE = icmsAPnotaIE;
                    ViewBag.IcmsAPSIE = icmsAPnotaSIE;

                    decimal IcmsAPagarAPSIE = Convert.ToDecimal(totalDiefAPSIE - icmsAPnotaSIE);
                    decimal IcmsAPagarAPIE = Convert.ToDecimal(totalDiefAPIE - icmsAPnotaIE);
                    ViewBag.IcmsAPagarAPSIE = IcmsAPagarAPSIE;
                    ViewBag.IcmsAPagarAPIE = IcmsAPagarAPIE;

                    decimal icmsAPAPagar = 0;


                    if (IcmsAPagarAPSIE > 0)
                    {
                        icmsAPAPagar += IcmsAPagarAPSIE;

                    }

                    if (IcmsAPagarAPIE > 0)
                    {
                        icmsAPAPagar += IcmsAPagarAPIE;
                    }

                    var vendas = _vendaAnexoService.FindByVendasTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();
                    var devoFornecedors = _devoFornecedorService.FindByDevoTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();
                    var compras = _compraAnexoService.FindByComprasTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();
                    var devoClientes = _devoClienteService.FindByDevoTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();

                    ViewBag.VendasInternas = vendas;
                    ViewBag.DevoFornecedorInternas = devoFornecedors;
                    ViewBag.ComprasInternas = compras;
                    ViewBag.DevoClienteInternas = devoClientes;
                    ViewBag.TaxAnexo = impAnexo;

                    var mesAtual = importMes.NumberMonth(month);
                    var mesAnterior = importMes.NameMonthLast(mesAtual);
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

                    //  Total
                    // A
                    decimal icmsTotalA = Convert.ToDecimal(compras.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsCompra4) +
                                        Convert.ToDecimal(impAnexo.IcmsCompra7) + Convert.ToDecimal(impAnexo.IcmsCompra12);
                    icmsTotalA -= (Convert.ToDecimal(devoFornecedors.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsDevoFornecedor4) +
                                    Convert.ToDecimal(impAnexo.IcmsDevoFornecedor7) + Convert.ToDecimal(impAnexo.IcmsDevoFornecedor12));
                    ViewBag.IcmsTotalA = icmsTotalA;

                    // D
                    decimal icmsTotalD = Convert.ToDecimal(vendas.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsVenda4) +
                                         Convert.ToDecimal(impAnexo.IcmsVenda7) + Convert.ToDecimal(impAnexo.IcmsVenda12);
                    icmsTotalD -= (Convert.ToDecimal(devoClientes.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsDevoCliente4) +
                                    Convert.ToDecimal(impAnexo.IcmsDevoCliente12));
                    ViewBag.IcmsTotalD = icmsTotalD;

                    // Saldo Credor Mes Anterior
                    ViewBag.SaldoCredorAnterior = saldoCredorAnterior;

                    // CRÉDITO DA ANTECIPAÇÃO PARCIAL PAGA
                    ViewBag.APPagar = icmsAPAPagar;

                    // Saldo Devedor
                    decimal saldoDevedor = icmsTotalD - icmsTotalA - icmsAPAPagar - saldoCredorAnterior;
                    ViewBag.SaldoDevedor = saldoDevedor;

                    // Saldo Credor
                    decimal saldoCredor = icmsTotalA + icmsAPAPagar + saldoCredorAnterior - icmsTotalD;

                    if (saldoCredor < 0)
                    {
                        saldoCredor = 0;
                    }

                    ViewBag.SaldoCredor = saldoCredor;

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
                                ViewBag.Erro = 1;
                                return View();
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