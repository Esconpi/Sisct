﻿using Escon.SisctNET.Model;
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
            : base(functionalityService, "Company")
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

        public IActionResult Relatory(int companyId, string year, string month, string type, int cfopid, string opcao, string arquivo)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                opcao = "saida";

                var comp = _companyService.FindById(companyId, GetLog(Model.OccorenceLog.Read));

                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);


                var NfeEntry = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                var NfeExit = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));

                var importXml = new Xml.Import(_companyCfopService);
                var importSped = new Sped.Import(_companyCfopService);
                var importMes = new Period.Month();
                var importDir = new Diretorio.Import();

                string directoryNfeExit = "", directoryNfeEntry = importDir.Entrada(comp, NfeEntry.Value, year, month);

                if (arquivo != null)
                {
                    if (arquivo.Equals("xmlE"))
                        directoryNfeExit = importDir.SaidaEmpresa(comp, NfeExit.Value, year, month);
                    else
                        directoryNfeExit = importDir.SaidaSefaz(comp, NfeExit.Value, year, month);
                }

                ViewBag.Company = comp;
                ViewBag.Type = type;
                ViewBag.Opcao = opcao;
                ViewBag.Arquivo = arquivo;

                var imp = _taxService.FindByMonth(companyId, month, year, "Icms");
                var impAnexo = _taxAnexoService.FindByMonth(companyId, month, year);

                //  Saida
                var cfopsVenda = _companyCfopService.FindByCfopVenda(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsVendaST = _companyCfopService.FindByCfopVendaST(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsBoniVenda = _companyCfopService.FindByCfopBonificacaoVenda(comp.Document).Select(_ => _.Cfop.Code).ToList();

                //  Transferencia
                var cfopsTransf = _companyCfopService.FindByCfopTransferencia(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsTransfST = _companyCfopService.FindByCfopTransferenciaST(comp.Document).Select(_ => _.Cfop.Code).ToList();

                List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

                var ncmConvenio = _ncmConvenioService.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId));
                var cfopsCompany = _companyCfopService.FindByCompany(companyId);

                if (type.Equals("resumoCfop"))
                {
                    //  Resumo CFOP

                    List<List<string>> cfops = new List<List<string>>();

                    if (opcao.Equals("saida"))
                    {
                        notes = importXml.NFeAll(directoryNfeExit);

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
                                cpf = notes[i][3]["CPF"];

                            if (notes[i][3].ContainsKey("CNPJ"))
                                cnpj = notes[i][3]["CNPJ"];

                            if (notes[i][3].ContainsKey("indIEDest"))
                                indIEDest = notes[i][3]["indIEDest"];

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
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                    cfops[pos][3] = (Convert.ToDecimal(cfops[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    cfops[pos][4] = (Convert.ToDecimal(cfops[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();

                                if (cpf != "escon" && cpf != "")
                                {
                                    // Com CPF

                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();

                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                        cfops[pos][7] = (Convert.ToDecimal(cfops[pos][7]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();

                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        cfops[pos][8] = (Convert.ToDecimal(cfops[pos][8]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();

                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        cfops[pos][9] = (Convert.ToDecimal(cfops[pos][9]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();

                                }
                                else if ((cpf == "escon" || cpf == "") && cnpj == "escon")
                                {
                                    // Sem CPF

                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][10] = (Convert.ToDecimal(cfops[pos][10]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][10] = (Convert.ToDecimal(cfops[pos][10]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][10] = (Convert.ToDecimal(cfops[pos][10]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][10] = (Convert.ToDecimal(cfops[pos][10]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][10] = (Convert.ToDecimal(cfops[pos][10]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();

                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                        cfops[pos][11] = (Convert.ToDecimal(cfops[pos][11]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();

                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        cfops[pos][12] = (Convert.ToDecimal(cfops[pos][12]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();

                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        cfops[pos][13] = (Convert.ToDecimal(cfops[pos][13]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                }
                                else if (cnpj != "escon" && cnpj != "" && indIEDest == "1")
                                {
                                    // Com CNPJ e com IE

                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][14] = (Convert.ToDecimal(cfops[pos][14]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][14] = (Convert.ToDecimal(cfops[pos][14]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][14] = (Convert.ToDecimal(cfops[pos][14]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][14] = (Convert.ToDecimal(cfops[pos][14]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][14] = (Convert.ToDecimal(cfops[pos][14]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();

                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                        cfops[pos][15] = (Convert.ToDecimal(cfops[pos][15]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();

                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        cfops[pos][16] = (Convert.ToDecimal(cfops[pos][16]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();

                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        cfops[pos][17] = (Convert.ToDecimal(cfops[pos][17]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();

                                }
                                else if (cnpj != "escon" && cnpj != "" && indIEDest != "1")
                                {
                                    // Com CNPJ e sem IE
                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][18] = (Convert.ToDecimal(cfops[pos][18]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][18] = (Convert.ToDecimal(cfops[pos][18]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][18] = (Convert.ToDecimal(cfops[pos][18]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][18] = (Convert.ToDecimal(cfops[pos][18]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                        cfops[pos][18] = (Convert.ToDecimal(cfops[pos][18]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();

                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                        cfops[pos][19] = (Convert.ToDecimal(cfops[pos][19]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();

                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        cfops[pos][20] = (Convert.ToDecimal(cfops[pos][20]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();

                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        cfops[pos][21] = (Convert.ToDecimal(cfops[pos][21]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                }

                            }

                        }

                        ViewBag.Cfop = cfops.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                    }
                    else
                    {
                        notes = importXml.NFeAll(directoryNfeEntry);

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

                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                    cfops[pos][3] = (Convert.ToDecimal(cfops[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    cfops[pos][4] = (Convert.ToDecimal(cfops[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();

                            }

                        }

                        ViewBag.Cfop = cfops.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                    }

                }
                else if (type.Equals("resumoCfopCst"))
                {
                    //  Resumo CFOP/CST

                    List<List<string>> cfops = new List<List<string>>();

                    notes = importXml.NFeAll(directoryNfeExit);

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
                                    vProd += Convert.ToDecimal(notes[i][j]["vFrete"]);

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    vProd -= Convert.ToDecimal(notes[i][j]["vDesc"]);

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    vProd += Convert.ToDecimal(notes[i][j]["vOutro"]);

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    vProd += Convert.ToDecimal(notes[i][j]["vSeg"]);
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
                                cfops[pos][7] = (Convert.ToDecimal(cfops[pos][7]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();

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
                                cfops[pos][7] = (Convert.ToDecimal(cfops[pos][7]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                        }

                    }

                    ViewBag.Cfop = cfops.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                }
                else if (type.Equals("resumoCfopAliq"))
                {
                    //  Resumo CFOP/Aliquota

                    List<List<string>> cfops = new List<List<string>>();

                    notes = importXml.NFeAll(directoryNfeExit);

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        string CFOP = "", pICMS = "0", pFCP = "0";
                        decimal vProd = 0, vBC = 0, vICMS = 0, vFCP = 0;
                        int pos = -1;
                        bool status = false;

                        for (int j = 0; j < notes[i].Count(); j++)
                        {

                            if ((notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM")))
                            {
                                if (status)
                                {
                                    for (int e = 0; e < cfops.Count(); e++)
                                    {
                                        if (cfops[e][0].Equals(CFOP) && Convert.ToDecimal(cfops[e][4]).Equals(Convert.ToDecimal(pICMS)) && Convert.ToDecimal(cfops[e][6]).Equals(Convert.ToDecimal(pFCP)))
                                        {
                                            pos = e;
                                            break;
                                        }
                                    }

                                    if (pos < 0)
                                    {
                                        var cfp = _cfopService.FindByCode(CFOP);
                                        List<string> cc = new List<string>();
                                        cc.Add(CFOP);
                                        cc.Add(cfp.Description);
                                        cc.Add(vProd.ToString());
                                        cc.Add(vBC.ToString());
                                        cc.Add(pICMS);
                                        cc.Add(vICMS.ToString());
                                        cc.Add(pFCP);
                                        cc.Add(vFCP.ToString());
                                        cfops.Add(cc);
                                    }
                                    else
                                    {
                                        cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + vProd).ToString();
                                        cfops[pos][3] = (Convert.ToDecimal(cfops[pos][3]) + vBC).ToString();
                                        cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + vICMS).ToString();
                                        cfops[pos][7] = (Convert.ToDecimal(cfops[pos][7]) + vFCP).ToString();
                                    }
                                }

                                pos = -1;
                                status = false;
                                CFOP = notes[i][j]["CFOP"];
                                vBC = 0;
                                vICMS = 0;
                                vFCP = 0;
                                pICMS = "0";
                                pFCP = "0";

                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    vProd = 0;
                                    vProd += Convert.ToDecimal(notes[i][j]["vProd"]);
                                    status = true;
                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    vProd += Convert.ToDecimal(notes[i][j]["vFrete"]);

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    vProd -= Convert.ToDecimal(notes[i][j]["vDesc"]);

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    vProd += Convert.ToDecimal(notes[i][j]["vOutro"]);

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    vProd += Convert.ToDecimal(notes[i][j]["vSeg"]);
                            }

                            if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                            {
                                vBC = Convert.ToDecimal(notes[i][j]["vBC"]);
                                status = true;
                            }
                               
                            if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                            {
                                pICMS = notes[i][j]["pICMS"];
                                vICMS = Convert.ToDecimal(notes[i][j]["vICMS"]);
                                status = true;
                            }
                                
                            if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                            {
                                pFCP = notes[i][j]["pFCP"];
                                vFCP = Convert.ToDecimal(notes[i][j]["vFCP"]);
                                status = true;
                            }

                            if (notes[i][j].ContainsKey("vNF"))
                            {
                                for (int e = 0; e < cfops.Count(); e++)
                                {
                                    if (cfops[e][0].Equals(CFOP) && Convert.ToDecimal(cfops[e][4]).Equals(Convert.ToDecimal(pICMS)) && Convert.ToDecimal(cfops[e][6]).Equals(Convert.ToDecimal(pFCP)))
                                    {
                                        pos = e;
                                        break;
                                    }
                                }

                                if (pos < 0)
                                {
                                    var cfp = _cfopService.FindByCode(CFOP);
                                    List<string> cc = new List<string>();
                                    cc.Add(CFOP);
                                    cc.Add(cfp.Description);
                                    cc.Add(vProd.ToString());
                                    cc.Add(vBC.ToString());
                                    cc.Add(pICMS);
                                    cc.Add(vICMS.ToString());
                                    cc.Add(pFCP);
                                    cc.Add(vFCP.ToString());
                                    cfops.Add(cc);
                                }
                                else
                                {
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + vProd).ToString();
                                    cfops[pos][3] = (Convert.ToDecimal(cfops[pos][3]) + vBC).ToString();
                                    cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + vICMS).ToString();
                                    cfops[pos][7] = (Convert.ToDecimal(cfops[pos][7]) + vFCP).ToString();
                                }
                            }
                        }

                    }

                    ViewBag.Cfop = cfops.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                }
                else if (type.Equals("resumoPorCfop"))
                {
                    //  Resumo por CFOP

                    var cfop = _cfopService.FindById(cfopid, null);
                    decimal totalNota = 0, valorContabil = 0, baseIcms = 0, valorIcms = 0, valorFecop = 0;
                    ViewBag.Code = cfop.Code;

                    opcao = "saida";

                    if (opcao.Equals("saida"))
                        notes = importXml.NFeAllCFOP(directoryNfeExit, cfop.Code);
                    else
                        notes = importXml.NFeAllCFOP(directoryNfeEntry, cfop.Code);


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
                                break;
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
                else if (type.Equals("resumoCfopAnexo"))
                {
                    //  Resumo CFOP Anexo

                    notes = importXml.NFeAll(directoryNfeExit);

                    List<List<string>> cfopsAnexo = new List<List<string>>();

                    decimal valorContabil = 0, valorBC = 0, valorIcms = 0, valorFecop = 0;

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        int pos = -1;

                        bool status = false;

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("NCM"))
                                status = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, notes[i][j]["NCM"]);

                            if (status)
                            {
                                if (notes[i][j].ContainsKey("CFOP"))
                                {
                                    pos = -1;
                                    for (int k = 0; k < cfopsAnexo.Count(); k++) {
                                        if (cfopsAnexo[k][0].Equals(notes[i][j]["CFOP"]))
                                        {
                                            pos = k;
                                            break;
                                        }
                                    }
                                        

                                    if (pos < 0)
                                    {
                                        List<string> cfopAnexo = new List<string>();

                                        var cc = cfopsCompany.Where(_ => _.Cfop.Code.Equals(notes[i][j]["CFOP"])).FirstOrDefault();
                                        cfopAnexo.Add(notes[i][j]["CFOP"]);
                                        cfopAnexo.Add(cc.Cfop.Description);
                                        cfopAnexo.Add("0");
                                        cfopAnexo.Add("0");
                                        cfopAnexo.Add("0");
                                        cfopAnexo.Add("0");
                                        cfopsAnexo.Add(cfopAnexo);
                                        pos = cfopsAnexo.Count() - 1;
                                    }
                                }

                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexo[pos][2] = (Convert.ToDecimal(cfopsAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vProd"]);

                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexo[pos][2] = (Convert.ToDecimal(cfopsAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexo[pos][2] = (Convert.ToDecimal(cfopsAnexo[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    valorContabil -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexo[pos][2] = (Convert.ToDecimal(cfopsAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexo[pos][2] = (Convert.ToDecimal(cfopsAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsAnexo[pos][3] = (Convert.ToDecimal(cfopsAnexo[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    valorBC += Convert.ToDecimal(notes[i][j]["vBC"]);
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsAnexo[pos][4] = (Convert.ToDecimal(cfopsAnexo[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    valorIcms += Convert.ToDecimal(notes[i][j]["vICMS"]);
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsAnexo[pos][5] = (Convert.ToDecimal(cfopsAnexo[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    valorFecop += Convert.ToDecimal(notes[i][j]["vFCP"]);
                                }
                            }
                        }
                    }

                    ViewBag.CfopAnexo = cfopsAnexo.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                }
                else if (type.Equals("resumoCfopAnexoAnalitico"))
                {
                    //  Resumo CFOP Anexo Analítico

                    notes = importXml.NFeAll(directoryNfeExit);

                    List<List<string>> cfopsAnexo = new List<List<string>>();
                    List<List<string>> produtos = new List<List<string>>();

                    decimal valorContabil = 0, valorBC = 0, valorIcms = 0, valorFecop = 0;

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        int pos = -1;

                        bool ncm = false;

                        List<string> produto = new List<string>();
                        produto.Add(notes[i][0]["chave"]);
                        produto.Add(notes[i][1]["nNF"]);
                        produto.Add("xProd");
                        produto.Add("cProd");
                        produto.Add("NCM");
                        produto.Add("CFOP");
                        produto.Add("0");
                        produto.Add("0");
                        produto.Add("0");
                        produto.Add("0");

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("NCM"))
                            {

                                if (ncm)
                                {
                                    produtos.Add(produto);

                                    produto = new List<string>();
                                    produto.Add(notes[i][0]["chave"]);
                                    produto.Add(notes[i][1]["nNF"]);
                                    produto.Add("xProd");
                                    produto.Add("cProd");
                                    produto.Add("NCM");
                                    produto.Add("CFOP");
                                    produto.Add("0");
                                    produto.Add("0");
                                    produto.Add("0");
                                    produto.Add("0");
                                }

                                ncm = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, notes[i][j]["NCM"]);                                

                                if (ncm)
                                {
                                    produto[2] = notes[i][j]["xProd"];
                                    produto[3] = notes[i][j]["cProd"];
                                    produto[4] = notes[i][j]["NCM"];
                                    produto[5] = "CFOP";
                                    produto[6] = "0";
                                    produto[7] = "0";
                                    produto[8] = "0";
                                    produto[9] = "0";
                                }
                            }

                            if (ncm)
                            {
                                if (notes[i][j].ContainsKey("CFOP"))
                                {
                                    pos = -1;
                                    for (int k = 0; k < cfopsAnexo.Count(); k++)
                                    {
                                        if (cfopsAnexo[k][0].Equals(notes[i][j]["CFOP"]))
                                        {
                                            pos = k;
                                            break;
                                        }
                                    }

                                    if (pos < 0)
                                    {
                                        List<string> cfopAnexo = new List<string>();

                                        var cc = cfopsCompany.Where(_ => _.Cfop.Code.Equals(notes[i][j]["CFOP"])).FirstOrDefault();
                                        cfopAnexo.Add(notes[i][j]["CFOP"]);
                                        cfopAnexo.Add(cc.Cfop.Description);
                                        cfopAnexo.Add("0");
                                        cfopAnexo.Add("0");
                                        cfopAnexo.Add("0");
                                        cfopAnexo.Add("0");
                                        cfopsAnexo.Add(cfopAnexo);
                                        pos = cfopsAnexo.Count() - 1;
                                    }
                                    produto[5] = notes[i][j]["CFOP"];
                                }

                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexo[pos][2] = (Convert.ToDecimal(cfopsAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    produto[6] = (Convert.ToDecimal(produto[6]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vProd"]);
                                    
                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexo[pos][2] = (Convert.ToDecimal(cfopsAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    produto[6] = (Convert.ToDecimal(produto[6]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexo[pos][2] = (Convert.ToDecimal(cfopsAnexo[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    produto[6] = (Convert.ToDecimal(produto[6]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    valorContabil -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexo[pos][2] = (Convert.ToDecimal(cfopsAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    produto[6] = (Convert.ToDecimal(produto[6]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexo[pos][2] = (Convert.ToDecimal(cfopsAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    produto[6] = (Convert.ToDecimal(produto[6]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsAnexo[pos][3] = (Convert.ToDecimal(cfopsAnexo[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    produto[7] = (Convert.ToDecimal(produto[7]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    valorBC += Convert.ToDecimal(notes[i][j]["vBC"]);
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsAnexo[pos][4] = (Convert.ToDecimal(cfopsAnexo[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    produto[8] = (Convert.ToDecimal(produto[8]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    valorIcms += Convert.ToDecimal(notes[i][j]["vICMS"]);
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsAnexo[pos][5] = (Convert.ToDecimal(cfopsAnexo[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    produto[9] = (Convert.ToDecimal(produto[9]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    valorFecop += Convert.ToDecimal(notes[i][j]["vFCP"]);
                                }                                
                            }

                            if (notes[i][j].ContainsKey("vNF"))
                            {
                                if (ncm == true)
                                {
                                    produtos.Add(produto);
                                }
                            }
                        }
                    }

                    ViewBag.CfopAnexo = cfopsAnexo.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                    ViewBag.Produtos = produtos.OrderBy(_ => Convert.ToInt32(_[1])).ToList();
                }
                else if (type.Equals("resumoCfopAnexoDC"))
                {
                    //  Resumo CFOP Anexo Débito e Crédito
                    List<List<string>> cfopsAnexoEntry = new List<List<string>>();
                    List<List<string>> cfopsAnexoExit = new List<List<string>>();

                    decimal valorContabil = 0, valorBC = 0, valorIcms = 0, valorFecop = 0;


                    // Entrada
                    notes = importXml.NFeAll(directoryNfeEntry);

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][3]["CNPJ"].Equals(comp.Document))
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        string ncmTemp = "";

                        int pos = -1;

                        bool status = false;

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("NCM"))
                            {

                                status = false;
                                ncmTemp = "";


                                for (int k = 0; k < ncmConvenio.Count; k++)
                                {
                                    int tamanho = ncmConvenio[k].Ncm.Length;


                                    if (tamanho < 8 && notes[i][j]["NCM"].Length > tamanho)
                                    {
                                        ncmTemp = notes[i][j]["NCM"].Substring(0, tamanho);
                                    }
                                    else
                                    {
                                        ncmTemp = notes[i][j]["NCM"];
                                    }

                                    if (ncmConvenio[k].Ncm.Equals(ncmTemp))
                                    {
                                        status = true;
                                        break;
                                    }
                                }
                            }

                            if (status == true)
                            {
                                if (notes[i][j].ContainsKey("CFOP"))
                                {
                                    pos = -1;
                                    for (int k = 0; k < cfopsAnexoEntry.Count(); k++)
                                    {
                                        if (cfopsAnexoEntry[k][0].Equals(notes[i][j]["CFOP"]))
                                        {
                                            pos = k;
                                        }
                                    }

                                    if (pos < 0)
                                    {
                                        List<string> cfopAnexo = new List<string>();

                                        var cc = cfopsCompany.Where(_ => _.Cfop.Code.Equals(notes[i][j]["CFOP"])).FirstOrDefault();
                                        cfopAnexo.Add(notes[i][j]["CFOP"]);
                                        cfopAnexo.Add(cc.Cfop.Description);
                                        cfopAnexo.Add("0");
                                        cfopAnexo.Add("0");
                                        cfopAnexo.Add("0");
                                        cfopAnexo.Add("0");
                                        cfopsAnexoEntry.Add(cfopAnexo);
                                        pos = cfopsAnexoEntry.Count() - 1;
                                    }
                                }

                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexoEntry[pos][2] = (Convert.ToDecimal(cfopsAnexoEntry[pos][2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vProd"]);

                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexoEntry[pos][2] = (Convert.ToDecimal(cfopsAnexoEntry[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexoEntry[pos][2] = (Convert.ToDecimal(cfopsAnexoEntry[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    valorContabil -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexoEntry[pos][2] = (Convert.ToDecimal(cfopsAnexoEntry[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexoEntry[pos][2] = (Convert.ToDecimal(cfopsAnexoEntry[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsAnexoEntry[pos][3] = (Convert.ToDecimal(cfopsAnexoEntry[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    valorBC += Convert.ToDecimal(notes[i][j]["vBC"]);
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsAnexoEntry[pos][4] = (Convert.ToDecimal(cfopsAnexoEntry[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    valorIcms += Convert.ToDecimal(notes[i][j]["vICMS"]);
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsAnexoEntry[pos][5] = (Convert.ToDecimal(cfopsAnexoEntry[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    valorFecop += Convert.ToDecimal(notes[i][j]["vFCP"]);
                                }
                            }
                        }
                    }

                    ViewBag.CfopAnexoEntry = cfopsAnexoEntry.OrderBy(_ => Convert.ToInt32(_[0])).ToList();

                    // Saida 
                    notes = importXml.NFeAll(directoryNfeExit);

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        int pos = -1;

                        bool ncm = false;

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("NCM"))
                                ncm = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, notes[i][j]["NCM"]);

                            if (ncm)
                            {
                                if (notes[i][j].ContainsKey("CFOP"))
                                {
                                    pos = -1;
                                    for (int k = 0; k < cfopsAnexoExit.Count(); k++)
                                    {
                                        if (cfopsAnexoExit[k][0].Equals(notes[i][j]["CFOP"]))
                                        {
                                            pos = k;
                                            break;
                                        }
                                    }

                                    if (pos < 0)
                                    {
                                        List<string> cfopAnexo = new List<string>();

                                        var cc = cfopsCompany.Where(_ => _.Cfop.Code.Equals(notes[i][j]["CFOP"])).FirstOrDefault();
                                        cfopAnexo.Add(notes[i][j]["CFOP"]);
                                        cfopAnexo.Add(cc.Cfop.Description);
                                        cfopAnexo.Add("0");
                                        cfopAnexo.Add("0");
                                        cfopAnexo.Add("0");
                                        cfopAnexo.Add("0");
                                        cfopsAnexoExit.Add(cfopAnexo);
                                        pos = cfopsAnexoExit.Count() - 1;
                                    }
                                }

                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexoExit[pos][2] = (Convert.ToDecimal(cfopsAnexoExit[pos][2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vProd"]);

                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexoExit[pos][2] = (Convert.ToDecimal(cfopsAnexoExit[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexoExit[pos][2] = (Convert.ToDecimal(cfopsAnexoExit[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    valorContabil -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexoExit[pos][2] = (Convert.ToDecimal(cfopsAnexoExit[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsAnexoExit[pos][2] = (Convert.ToDecimal(cfopsAnexoExit[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsAnexoExit[pos][3] = (Convert.ToDecimal(cfopsAnexoExit[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    valorBC += Convert.ToDecimal(notes[i][j]["vBC"]);
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsAnexoExit[pos][4] = (Convert.ToDecimal(cfopsAnexoExit[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    valorIcms += Convert.ToDecimal(notes[i][j]["vICMS"]);
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsAnexoExit[pos][5] = (Convert.ToDecimal(cfopsAnexoExit[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    valorFecop += Convert.ToDecimal(notes[i][j]["vFCP"]);
                                }
                            }
                        }
                    }

                    ViewBag.CfopAnexoExit = cfopsAnexoExit.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                }
                else if (type.Equals("resumoCfopForaAnexo"))
                {
                    //  Resumo CFOP Fora Anexo

                    notes = importXml.NFeAll(directoryNfeExit);

                    List<List<string>> cfopsForaAnexo = new List<List<string>>();

                    decimal valorContabil = 0, valorBC = 0, valorIcms = 0, valorFecop = 0;

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        int pos = -1;

                        bool ncm = true;

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("NCM"))
                                ncm = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, notes[i][j]["NCM"]);

                            if (!ncm)
                            {
                                if (notes[i][j].ContainsKey("CFOP"))
                                {
                                    pos = -1;
                                    for (int k = 0; k < cfopsForaAnexo.Count(); k++)
                                    {
                                        if (cfopsForaAnexo[k][0].Equals(notes[i][j]["CFOP"]))
                                        {
                                            pos = k;
                                            break;
                                        }
                                    }

                                    if (pos < 0)
                                    {
                                        List<string> cfopForaAnexo = new List<string>();

                                        var cc = cfopsCompany.Where(_ => _.Cfop.Code.Equals(notes[i][j]["CFOP"])).FirstOrDefault();
                                        cfopForaAnexo.Add(notes[i][j]["CFOP"]);
                                        cfopForaAnexo.Add(cc.Cfop.Description);
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopsForaAnexo.Add(cfopForaAnexo);
                                        pos = cfopsForaAnexo.Count() - 1;
                                    }
                                }

                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vProd"]);
                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    valorContabil -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexo[pos][3] = (Convert.ToDecimal(cfopsForaAnexo[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    valorBC += Convert.ToDecimal(notes[i][j]["vBC"]);
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexo[pos][4] = (Convert.ToDecimal(cfopsForaAnexo[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    valorIcms += Convert.ToDecimal(notes[i][j]["vICMS"]);
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexo[pos][5] = (Convert.ToDecimal(cfopsForaAnexo[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    valorFecop += Convert.ToDecimal(notes[i][j]["vFCP"]);
                                }
                            }

                        }
                    }

                    ViewBag.CfopForaAnexo = cfopsForaAnexo.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                }
                else if (type.Equals("resumoCfopForaAnexoAnalitico"))
                {
                    //  Resumo CFOP Fora Anexo Analítico

                    notes = importXml.NFeAll(directoryNfeExit);

                    List<List<string>> cfopsForaAnexo = new List<List<string>>();
                    List<List<string>> produtos = new List<List<string>>();

                    decimal valorContabil = 0, valorBC = 0, valorIcms = 0, valorFecop = 0;

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        int pos = -1;

                        bool ncm = true;

                        List<string> produto = new List<string>();
                        produto.Add(notes[i][0]["chave"]);
                        produto.Add(notes[i][1]["nNF"]);
                        produto.Add("xProd");
                        produto.Add("cProd");
                        produto.Add("NCM");
                        produto.Add("CFOP");
                        produto.Add("0");
                        produto.Add("0");
                        produto.Add("0");
                        produto.Add("0");

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("NCM"))
                            {

                                if (ncm == false)
                                {
                                    produtos.Add(produto);

                                    produto = new List<string>();
                                    produto.Add(notes[i][0]["chave"]);
                                    produto.Add(notes[i][1]["nNF"]);
                                    produto.Add("xProd");
                                    produto.Add("cProd");
                                    produto.Add("NCM");
                                    produto.Add("CFOP");
                                    produto.Add("0");
                                    produto.Add("0");
                                    produto.Add("0");
                                    produto.Add("0");
                                }

                                ncm = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, notes[i][j]["NCM"]);

                                if (ncm == false)
                                {
                                    produto[2] = notes[i][j]["xProd"];
                                    produto[3] = notes[i][j]["cProd"];
                                    produto[4] = notes[i][j]["NCM"];
                                    produto[5] = "CFOP";
                                    produto[6] = "0";
                                    produto[7] = "0";
                                    produto[8] = "0";
                                    produto[9] = "0";
                                }
                            }

                            if (!ncm)
                            {
                                if (notes[i][j].ContainsKey("CFOP"))
                                {
                                    pos = -1;
                                    for (int k = 0; k < cfopsForaAnexo.Count(); k++)
                                    {
                                        if (cfopsForaAnexo[k][0].Equals(notes[i][j]["CFOP"]))
                                        {
                                            pos = k;
                                            break;
                                        }
                                    }

                                    if (pos < 0)
                                    {
                                        List<string> cfopForaAnexo = new List<string>();

                                        var cc = cfopsCompany.Where(_ => _.Cfop.Code.Equals(notes[i][j]["CFOP"])).FirstOrDefault();
                                        cfopForaAnexo.Add(notes[i][j]["CFOP"]);
                                        cfopForaAnexo.Add(cc.Cfop.Description);
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopsForaAnexo.Add(cfopForaAnexo);
                                        pos = cfopsForaAnexo.Count() - 1;
                                    }
                                    produto[5] = notes[i][j]["CFOP"];
                                }

                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    produto[6] = (Convert.ToDecimal(produto[6]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vProd"]);

                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    produto[6] = (Convert.ToDecimal(produto[6]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    produto[6] = (Convert.ToDecimal(produto[6]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    valorContabil -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    produto[6] = (Convert.ToDecimal(produto[6]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexo[pos][2] = (Convert.ToDecimal(cfopsForaAnexo[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    produto[6] = (Convert.ToDecimal(produto[6]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexo[pos][3] = (Convert.ToDecimal(cfopsForaAnexo[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    produto[7] = (Convert.ToDecimal(produto[7]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    valorBC += Convert.ToDecimal(notes[i][j]["vBC"]);
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexo[pos][4] = (Convert.ToDecimal(cfopsForaAnexo[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    produto[8] = (Convert.ToDecimal(produto[8]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    valorIcms += Convert.ToDecimal(notes[i][j]["vICMS"]);
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexo[pos][5] = (Convert.ToDecimal(cfopsForaAnexo[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    produto[9] = (Convert.ToDecimal(produto[9]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    valorFecop += Convert.ToDecimal(notes[i][j]["vFCP"]);
                                }
                            }

                            if (notes[i][j].ContainsKey("vNF"))
                            {
                                if (ncm == false)
                                {
                                    produtos.Add(produto);
                                }
                            }
                        }
                    }

                    ViewBag.CfopForaAnexo = cfopsForaAnexo.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                    ViewBag.Produtos = produtos.OrderBy(_ => Convert.ToInt32(_[1])).ToList();
                }
                else if (type.Equals("resumoCfopForaAnexoDC"))
                {
                    //  Resumo CFOP Fora Anexo Débito e Crédito

                    List<List<string>> cfopsForaAnexoEntry = new List<List<string>>();
                    List<List<string>> cfopsForaAnexoExit = new List<List<string>>();

                    decimal valorContabil = 0, valorBC = 0, valorIcms = 0, valorFecop = 0;


                    //  Entrada
                    notes = importXml.NFeAll(directoryNfeEntry);

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][3]["CNPJ"].Equals(comp.Document))
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        int pos = -1;

                        bool ncm = true;

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("NCM"))
                                ncm = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, notes[i][j]["NCM"]);

                            if (ncm)
                            {
                                if (notes[i][j].ContainsKey("CFOP"))
                                {
                                    pos = -1;
                                    for (int k = 0; k < cfopsForaAnexoEntry.Count(); k++)
                                    {
                                        if (cfopsForaAnexoEntry[k][0].Equals(notes[i][j]["CFOP"]))
                                        {
                                            pos = k;
                                            break;
                                        }
                                    }

                                    if (pos < 0)
                                    {
                                        List<string> cfopForaAnexo = new List<string>();

                                        var cc = cfopsCompany.Where(_ => _.Cfop.Code.Equals(notes[i][j]["CFOP"])).FirstOrDefault();
                                        cfopForaAnexo.Add(notes[i][j]["CFOP"]);
                                        cfopForaAnexo.Add(cc.Cfop.Description);
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopsForaAnexoEntry.Add(cfopForaAnexo);
                                        pos = cfopsForaAnexoEntry.Count() - 1;
                                    }
                                }

                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexoEntry[pos][2] = (Convert.ToDecimal(cfopsForaAnexoEntry[pos][2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vProd"]);

                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexoEntry[pos][2] = (Convert.ToDecimal(cfopsForaAnexoEntry[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexoEntry[pos][2] = (Convert.ToDecimal(cfopsForaAnexoEntry[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    valorContabil -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexoEntry[pos][2] = (Convert.ToDecimal(cfopsForaAnexoEntry[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexoEntry[pos][2] = (Convert.ToDecimal(cfopsForaAnexoEntry[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexoEntry[pos][3] = (Convert.ToDecimal(cfopsForaAnexoEntry[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    valorBC += Convert.ToDecimal(notes[i][j]["vBC"]);
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexoEntry[pos][4] = (Convert.ToDecimal(cfopsForaAnexoEntry[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    valorIcms += Convert.ToDecimal(notes[i][j]["vICMS"]);
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexoEntry[pos][5] = (Convert.ToDecimal(cfopsForaAnexoEntry[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    valorFecop += Convert.ToDecimal(notes[i][j]["vFCP"]);
                                }
                            }

                        }
                    }

                    ViewBag.CfopForaAnexoEntry = cfopsForaAnexoEntry.OrderBy(_ => Convert.ToInt32(_[0])).ToList();

                    //  Saida
                    notes = importXml.NFeAll(directoryNfeExit);

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        string ncmTemp = "";

                        int pos = -1;

                        bool status = true;

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("NCM"))
                            {

                                status = false;
                                ncmTemp = "";


                                for (int k = 0; k < ncmConvenio.Count; k++)
                                {
                                    int tamanho = ncmConvenio[k].Ncm.Length;


                                    if (tamanho < 8 && notes[i][j]["NCM"].Length > tamanho)
                                    {
                                        ncmTemp = notes[i][j]["NCM"].Substring(0, tamanho);
                                    }
                                    else
                                    {
                                        ncmTemp = notes[i][j]["NCM"];
                                    }

                                    if (ncmConvenio[k].Ncm.Equals(ncmTemp))
                                    {
                                        status = true;
                                        break;
                                    }
                                }
                            }

                            if (status == false)
                            {
                                if (notes[i][j].ContainsKey("CFOP"))
                                {
                                    pos = -1;
                                    for (int k = 0; k < cfopsForaAnexoExit.Count(); k++)
                                    {
                                        if (cfopsForaAnexoExit[k][0].Equals(notes[i][j]["CFOP"]))
                                        {
                                            pos = k;
                                        }
                                    }

                                    if (pos < 0)
                                    {
                                        List<string> cfopForaAnexo = new List<string>();

                                        var cc = cfopsCompany.Where(_ => _.Cfop.Code.Equals(notes[i][j]["CFOP"])).FirstOrDefault();
                                        cfopForaAnexo.Add(notes[i][j]["CFOP"]);
                                        cfopForaAnexo.Add(cc.Cfop.Description);
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopForaAnexo.Add("0");
                                        cfopsForaAnexoExit.Add(cfopForaAnexo);
                                        pos = cfopsForaAnexoExit.Count() - 1;
                                    }
                                }

                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexoExit[pos][2] = (Convert.ToDecimal(cfopsForaAnexoExit[pos][2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vProd"]);

                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexoExit[pos][2] = (Convert.ToDecimal(cfopsForaAnexoExit[pos][2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexoExit[pos][2] = (Convert.ToDecimal(cfopsForaAnexoExit[pos][2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    valorContabil -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexoExit[pos][2] = (Convert.ToDecimal(cfopsForaAnexoExit[pos][2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopsForaAnexoExit[pos][2] = (Convert.ToDecimal(cfopsForaAnexoExit[pos][2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    valorContabil += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexoExit[pos][3] = (Convert.ToDecimal(cfopsForaAnexoExit[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    valorBC += Convert.ToDecimal(notes[i][j]["vBC"]);
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexoExit[pos][4] = (Convert.ToDecimal(cfopsForaAnexoExit[pos][4]) + Convert.ToDecimal(notes[i][j]["vICMS"])).ToString();
                                    valorIcms += Convert.ToDecimal(notes[i][j]["vICMS"]);
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopsForaAnexoExit[pos][5] = (Convert.ToDecimal(cfopsForaAnexoExit[pos][5]) + Convert.ToDecimal(notes[i][j]["vFCP"])).ToString();
                                    valorFecop += Convert.ToDecimal(notes[i][j]["vFCP"]);
                                }
                            }

                        }
                    }

                    ViewBag.CfopForaAnexoExit = cfopsForaAnexoExit.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                }
                else if (type.Equals("anexo"))
                {
                    //  Resumo Vendas Anexo

                    ViewBag.Anexo = comp.Annex.Description + " - " + comp.Annex.Convenio;

                    cfopsVenda.AddRange(cfopsVendaST);
                    cfopsVenda.AddRange(cfopsBoniVenda);
                    cfopsVenda.AddRange(cfopsTransf);
                    cfopsVenda.AddRange(cfopsTransfST);

                    notes = importXml.NFeAll(directoryNfeExit, cfopsVenda);

                    var contNcm = ncmConvenio.Count();
                    string[,] resumoAnexo = new string[contNcm, 4];

                    for (int i = 0; i < contNcm; i++)
                    {
                        resumoAnexo[i, 0] = ncmConvenio[i].Ncm;
                        resumoAnexo[i, 1] = ncmConvenio[i].Description;
                        resumoAnexo[i, 2] = "0";
                        resumoAnexo[i, 3] = "0";
                    }


                    decimal totalVendas = 0;
                    

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i].Count <= 5)
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        int pos = 0;
                        bool ncm = false;

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("NCM"))
                            {
                                ncm = false;

                                for (int k = 0; k < ncmConvenio.Count; k++)
                                {
                                    int tamanho = ncmConvenio[k].Ncm.Length;
                                    string ncmTemp = "";

                                    if (tamanho < 8)
                                        ncmTemp = notes[i][j]["NCM"].Substring(0, tamanho);
                                    else
                                        ncmTemp = notes[i][j]["NCM"];

                                    if (ncmConvenio[k].Ncm.Equals(ncmTemp))
                                    {
                                        ncm = true;
                                        pos = k;
                                        break;
                                    }
                                }
                            }

                            if (ncm)
                            {
                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notes[i][j]["vProd"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();

                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {

                                    totalVendas += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();

                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();

                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();

                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vICMSST"))
                                {
                                    totalVendas += Convert.ToDecimal(notes[i][j]["vICMSST"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) + Convert.ToDecimal(notes[i][j]["vICMSST"])).ToString();
                                }
                            }
                            else
                            {
                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    totalVendas += Convert.ToDecimal(notes[i][j]["vProd"]);

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    totalVendas += Convert.ToDecimal(notes[i][j]["vFrete"]);

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    totalVendas -= Convert.ToDecimal(notes[i][j]["vDesc"]);

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    totalVendas += Convert.ToDecimal(notes[i][j]["vOutro"]);

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    totalVendas += Convert.ToDecimal(notes[i][j]["vSeg"]);

                                if (notes[i][j].ContainsKey("vICMSST"))
                                    totalVendas += Convert.ToDecimal(notes[i][j]["vICMSST"]);
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
                    //  Resumo Vendas Fora do Anexo

                    cfopsVenda.AddRange(cfopsVendaST);
                    cfopsVenda.AddRange(cfopsTransf);
                    cfopsVenda.AddRange(cfopsTransfST);
                    cfopsVenda.AddRange(cfopsBoniVenda);

                    notes = importXml.NFeAll(directoryNfeExit, cfopsVenda);

                    var ncms = _ncmConvenioService.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId));

                    List<List<string>> ncmsForaAnexo = new List<List<string>>();
                    decimal totalVendas = 0;

                    var ncmsAll = _ncmService.FindAll(null);

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i].Count <= 5)
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        string ncmTemp = "";
                        bool ncm = false;

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("NCM"))
                            {
                                ncm = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, notes[i][j]["NCM"]);
                                ncmTemp = notes[i][j]["NCM"];
                            }


                            if (!ncm)
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
                                        ncmForaAnexo.Add(ncmTemp);
                                        ncmForaAnexo.Add("0");
                                        ncmForaAnexo.Add("0");
                                        ncmForaAnexo.Add(nn.Description);
                                        ncmsForaAnexo.Add(ncmForaAnexo);
                                        var x = ncmsForaAnexo.IndexOf(ncmForaAnexo);
                                        pos = ncmsForaAnexo.Count() - 1;
                                    }
                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                        totalVendas += Convert.ToDecimal(notes[i][j]["vProd"]);
                                    }

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                        totalVendas += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                    }

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                        totalVendas -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                    }

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                        totalVendas += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                    }

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                        totalVendas += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                    }

                                    if (notes[i][j].ContainsKey("vICMSST"))
                                    {
                                        ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) + Convert.ToDecimal(notes[i][j]["vICMSST"])).ToString();
                                        totalVendas += Convert.ToDecimal(notes[i][j]["vICMSST"]);
                                    }

                                }

                            }
                            else
                            {
                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    totalVendas += Convert.ToDecimal(notes[i][j]["vProd"]);

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    totalVendas += Convert.ToDecimal(notes[i][j]["vFrete"]);

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    totalVendas -= Convert.ToDecimal(notes[i][j]["vDesc"]);

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    totalVendas += Convert.ToDecimal(notes[i][j]["vOutro"]);

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    totalVendas += Convert.ToDecimal(notes[i][j]["vSeg"]);

                                if (notes[i][j].ContainsKey("vICMSST"))
                                    totalVendas += Convert.ToDecimal(notes[i][j]["vICMSST"]);
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
                    //  Resumo Notas no Periodo de Suspensão da Empresa

                    List<List<string>> notas = new List<List<string>>();

                    var suspensions = _suspensionService.FindAll(null).Where(_ => _.CompanyId.Equals(companyId)).ToList();

                    cfopsVenda.AddRange(cfopsVendaST);
                    cfopsVenda.AddRange(cfopsTransf);
                    cfopsVenda.AddRange(cfopsTransfST);
                    cfopsVenda.AddRange(cfopsBoniVenda);

                    notes = importXml.NFeAll(directoryNfeExit, cfopsVenda);

                    decimal valorTotal = 0;                    

                    List<List<string>> periodos = new List<List<string>>();

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {

                        if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i][1]["finNFe"] == "4")
                        {
                            notes.RemoveAt(i);
                            continue;
                        }
                        List<string> note = new List<string>();
                        bool suspenso = false;
                        decimal vProd = 0;

                        if (notes[i][1].ContainsKey("dhEmi"))
                        {
                            foreach (var suspension in suspensions)
                            {
                                if (Convert.ToDateTime(notes[i][1]["dhEmi"]) >= Convert.ToDateTime(suspension.DateStart) && Convert.ToDateTime(notes[i][1]["dhEmi"]) < Convert.ToDateTime(suspension.DateEnd))
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

                                    if (!existe)
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


                        if (suspenso)
                        {
                            note.Add(notes[i][1]["natOp"]);
                            note.Add(notes[i][1]["mod"]);
                            note.Add(notes[i][1]["nNF"]);
                            note.Add(notes[i][1]["dhEmi"]);
                        }


                        for (int k = 0; k < notes[i].Count(); k++)
                        {

                            if (notes[i][k].ContainsKey("vProd") && notes[i][k].ContainsKey("cProd"))
                                vProd += Convert.ToDecimal(notes[i][k]["vProd"]);

                            if (notes[i][k].ContainsKey("vFrete") && notes[i][k].ContainsKey("cProd"))
                                vProd += Convert.ToDecimal(notes[i][k]["vFrete"]);

                            if (notes[i][k].ContainsKey("vDesc") && notes[i][k].ContainsKey("cProd"))
                                vProd -= Convert.ToDecimal(notes[i][k]["vDesc"]);

                            if (notes[i][k].ContainsKey("vOutro") && notes[i][k].ContainsKey("cProd"))
                                vProd += Convert.ToDecimal(notes[i][k]["vOutro"]);

                            if (notes[i][k].ContainsKey("vSeg") && notes[i][k].ContainsKey("cProd"))
                                vProd += Convert.ToDecimal(notes[i][k]["vSeg"]);

                            if (notes[i][k].ContainsKey("vNF") && suspenso)
                            {
                                note.Add(vProd.ToString());
                                valorTotal += vProd;
                                notas.Add(note);
                            }

                        }

                    }

                    ViewBag.Notes = notas.OrderBy(_ => Convert.ToInt32(_[2])).ToList();
                    ViewBag.Periodos = periodos.OrderBy(_ => Convert.ToDateTime(_[1])).ToList();
                    ViewBag.Total = valorTotal;

                }
                else if (type.Equals("produtoST"))
                {
                    //  Resumo Produtos ST

                    notes = importXml.NFeAll(directoryNfeExit);

                    var codeProd = _productIncentivoService.FindByAllProducts(companyId).Select(_ => _.Code).ToList();
                    var codeProdST = _productIncentivoService.FindByAllProducts(companyId).Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Code).ToList();

                    var cestProd = _productIncentivoService.FindByAllProducts(companyId).Select(_ => _.Cest).ToList();
                    var cestST = _productIncentivoService.FindByAllProducts(companyId).Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Cest).ToList();

                    List<List<string>> produtos = new List<List<string>>();

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i][1]["tpNF"].Equals("0") || notes[i][1]["finNFe"] == "4")
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        decimal valorProduto = 0;
                        string cProd = "", cest = "", xProd = "";

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("cProd"))
                            {
                                cProd = notes[i][j]["cProd"];
                                xProd = notes[i][j]["xProd"];
                                valorProduto = 0;
                                cest = "";

                                if (notes[i][j].ContainsKey("CEST"))
                                    cest = notes[i][j]["CEST"];

                                if (notes[i][j].ContainsKey("vProd"))
                                    valorProduto += Convert.ToDecimal(notes[i][j]["vProd"]);

                                if (notes[i][j].ContainsKey("vFrete"))
                                    valorProduto += Convert.ToDecimal(notes[i][j]["vFrete"]);

                                if (notes[i][j].ContainsKey("vDesc"))
                                    valorProduto -= Convert.ToDecimal(notes[i][j]["vDesc"]);

                                if (notes[i][j].ContainsKey("vOutro"))
                                    valorProduto += Convert.ToDecimal(notes[i][j]["vOutro"]);

                                if (notes[i][j].ContainsKey("vSeg"))
                                    valorProduto += Convert.ToDecimal(notes[i][j]["vSeg"]);
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
                                            produto.Add("CNPJ");
                                        else
                                            produto.Add("CPF");

                                        produto.Add(cProd);
                                        produto.Add(xProd);
                                        produto.Add(cest);
                                        produto.Add(notes[i][j]["vBC"]);
                                        produto.Add(notes[i][j]["vICMS"]);

                                        if (notes[i][j].ContainsKey("pFCP"))
                                            produto.Add(notes[i][j]["vFCP"]);
                                        else
                                            produto.Add("0");

                                        if (notes[i][j].ContainsKey("vBCST"))
                                            produto.Add(notes[i][j]["vBCST"]);
                                        else
                                            produto.Add("0");

                                        if (notes[i][j].ContainsKey("vICMSST"))
                                            produto.Add(notes[i][j]["vICMSST"]);
                                        else
                                            produto.Add("0");

                                        if (notes[i][j].ContainsKey("vFCPST"))
                                            produto.Add(notes[i][j]["vFCPST"]);
                                        else
                                            produto.Add("0");

                                        if (produto[3].Equals("CNPJ"))
                                        {
                                            if (!notes[i][2]["UF"].Equals(notes[i][3]["UF"]))
                                            {
                                                if (Convert.ToDecimal(notes[i][j]["vBC"]) > 0 && Convert.ToDecimal(produto[10]) <= 0)
                                                    produto.Add("C");
                                                else
                                                    produto.Add("E");
                                            }
                                            else
                                            {
                                                if (Convert.ToDecimal(notes[i][j]["vBC"]) > 0 && Convert.ToDecimal(produto[10]) > 0)
                                                    produto.Add("E");
                                                else
                                                    produto.Add("C");
                                            }
                                        }

                                        if (produto[3].Equals("CPF"))
                                        {
                                            if (!notes[i][2]["UF"].Equals(notes[i][3]["UF"]))
                                            {
                                                if (Convert.ToDecimal(notes[i][j]["vBC"]) > 0 && Convert.ToDecimal(produto[10]) <= 0)
                                                    produto.Add("C");
                                                else
                                                    produto.Add("E");
                                            }
                                            else
                                            {
                                                if (notes[i][j].ContainsKey("vBCST"))
                                                {
                                                    if (Convert.ToDecimal(notes[i][j]["vBC"]) > 0 && Convert.ToDecimal(notes[i][j]["vBC"]) <= 0)
                                                        produto.Add("E");
                                                    else
                                                        produto.Add("C");
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
                    //  Tributação Divergente ICMS

                    List<List<string>> prodsXml = new List<List<string>>();

                    notes = importXml.NFeAll(directoryNfeExit);

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
                                cest = notes[i][j]["CEST"];

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

                        if (!achou)
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

                            if (contem)
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
                                    divergente.Add("");
                                else
                                    divergente.Add(temp.Cst.Code);

                                divergentes.Add(divergente);
                            }
                        }

                    }

                    ViewBag.Divergentes = divergentes.OrderBy(_ => _[0]).ThenBy(_ => _[5]).ToList();
                }
                else if (type.Equals("icmsExcedente"))
                {
                    //  Imposto ICMS Excedente

                    if (imp == null)
                    {
                        ViewBag.Erro = 1;
                        return View();
                    }

                    var grupos = _grupoService.FindByGrupos(imp.Id);


                    decimal totalTranferenciaSaida = Convert.ToDecimal(imp.TransferenciaSaida), totalTranferenciaEntrada = Convert.ToDecimal(imp.TransferenciaEntrada),
                            totalTranferenciaInter = Convert.ToDecimal(imp.TransferenciaInter), totalVendasSuspensao = Convert.ToDecimal(imp.Suspensao),
                            totalCompra = Convert.ToDecimal(imp.Compras) + totalTranferenciaEntrada, totalDevoCompra = Convert.ToDecimal(imp.DevolucaoCompras),
                            totalVenda = Convert.ToDecimal(imp.Vendas) + totalTranferenciaSaida, totalDevoVenda = Convert.ToDecimal(imp.DevolucaoVendas),
                            totalVendaGrupo = Convert.ToDecimal(grupos.Sum(_ => _.Vendas)), totalDevoGrupo = Convert.ToDecimal(grupos.Sum(_ => _.Devolucao)),
                            totalNcm = Convert.ToDecimal(imp.VendasNcm), totalDevoNcm = Convert.ToDecimal(imp.DevolucaoNcm),
                            totalIncisoI = Convert.ToDecimal(imp.VendasIncisoI), totalDevoIncisoI = Convert.ToDecimal(imp.DevolucaoVendasIncisoI),
                            totalIncisoII = Convert.ToDecimal(imp.VendasIncisoII), totalDevoIncisoII = Convert.ToDecimal(imp.DevolucaoVendasIncisoII),
                            totalNcontribuinte = Convert.ToDecimal(imp.VendasNContribuinte), totalDevoNContribuinte = Convert.ToDecimal(imp.DevolucaoNContribuinte),
                            totalContribuinte = totalVenda - totalNcontribuinte, totalDevoContribuinte = totalDevoVenda - totalDevoNContribuinte,
                            baseCalcCompra = totalCompra - totalDevoCompra,
                            baseCalcVenda = totalVenda - totalDevoVenda,
                            baseCalcGrupo = totalVendaGrupo - totalDevoGrupo,
                            baseCalcNcm = totalNcm - totalDevoNcm,
                            baseCalcIncisoI = totalIncisoI - totalDevoIncisoI,
                            baseCalcIncisoII = totalIncisoII - totalDevoIncisoII,
                            baseCalcNContribuinte = totalNcontribuinte - totalDevoNContribuinte,
                            baseCalcContribuinte = totalContribuinte - totalDevoContribuinte,
                            limiteGrupo = (baseCalcVenda * Convert.ToDecimal(comp.VendaMGrupo)) / 100,
                            limiteNcm = (baseCalcVenda * Convert.ToDecimal(comp.VendaAnexo)) / 100,
                            limiteNContribuinte = (baseCalcVenda * (Convert.ToDecimal(comp.VendaCpf))) / 100,
                            limiteContribuinte = (baseCalcVenda * (Convert.ToDecimal(comp.VendaContribuinte))) / 100,
                            limiteTransferencia = (baseCalcVenda * Convert.ToDecimal(comp.Transferencia)) / 100,
                            limiteTransferenciaInter = (baseCalcCompra * Convert.ToDecimal(comp.TransferenciaInter)) / 100;

                    if (comp.ChapterId == 4)
                        limiteNcm = (baseCalcVenda * Convert.ToDecimal(comp.Faturamento)) / 100;

                    decimal excedenteGrupo = 0, impostoGrupo = 0, excedenteNcm = 0, impostoNcm = 0, impostoNContribuinte = 0, excedenteNContribuinte = 0,
                            impostoContribuinte = 0, excedenteContribuinte = 0, excedenteTranf = 0, impostoTransf = 0, excedenteTranfInter = 0, impostoTransfInter = 0;


                    //  CNPJ
                    List<List<string>> gruposExecentes = new List<List<string>>();

                    if (baseCalcGrupo > limiteGrupo)
                    {
                        excedenteGrupo = baseCalcGrupo - limiteGrupo;
                        impostoGrupo = Math.Round((excedenteGrupo * Convert.ToDecimal(comp.VendaMGrupoExcedente)) / 100,2);
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

                    //  Anexo II ou Inciso I e II
                    if (baseCalcNcm < limiteNcm && (comp.AnnexId == 1 || comp.ChapterId == 4))
                    {
                        excedenteNcm = limiteNcm - baseCalcNcm;

                        if(comp.AnnexId == 1)
                            impostoNcm = Math.Round((excedenteNcm * Convert.ToDecimal(comp.VendaAnexoExcedente)) / 100,2);
                        else
                            impostoNcm = Math.Round((excedenteNcm * Convert.ToDecimal(comp.FaturamentoExcedente)) / 100, 2);
                    }

                    //  Contribuinte
                    if (baseCalcContribuinte < limiteContribuinte && comp.ChapterId == 4)
                    {
                        excedenteContribuinte = baseCalcContribuinte - limiteContribuinte;
                        impostoContribuinte = Math.Round((excedenteContribuinte * Convert.ToDecimal(comp.VendaContribuinteExcedente)) / 100, 2);
                    }

                    //  Não Contribuinte
                    if (baseCalcNContribuinte > limiteNContribuinte && comp.ChapterId != 4)
                    {
                        excedenteNContribuinte = baseCalcNContribuinte - limiteNContribuinte;
                        impostoNContribuinte = Math.Round((excedenteNContribuinte * Convert.ToDecimal(comp.VendaCpfExcedente)) / 100,2);
                    }

                    //  Transferência
                    if(totalTranferenciaSaida > limiteTransferencia && comp.ChapterId == 4)
                    {
                        excedenteTranf = totalTranferenciaSaida - limiteTransferencia;
                        impostoTransf = Math.Round((excedenteTranf * Convert.ToDecimal(comp.TransferenciaExcedente)) / 100, 2);
                    }

                    //  Transferência inter
                    if (totalTranferenciaInter > limiteTransferenciaInter)
                    {
                        excedenteTranfInter = totalTranferenciaInter - limiteTransferenciaInter;
                        impostoTransfInter = Math.Round((excedenteTranfInter * Convert.ToDecimal(comp.TransferenciaInterExcedente)) / 100,2);
                    }

                    //  Suspensão
                    decimal valorSuspensao = Math.Round((totalVendasSuspensao * Convert.ToDecimal(comp.Suspension)) / 100,2);

                    //  Percentuais
                    decimal percentualVendaContribuinte = (baseCalcContribuinte * 100) / baseCalcVenda,
                            percentualVendaNContribuinte = (baseCalcNContribuinte * 100) / baseCalcVenda,
                            percentualVendaNcm = (baseCalcNcm * 100) / baseCalcVenda,
                            percentualVendaIncisoI = (baseCalcIncisoI * 100) / baseCalcVenda,
                            percentualVendaIncisoII = (baseCalcIncisoII * 100) / baseCalcVenda,
                            percentualGrupo = (baseCalcGrupo * 100) / baseCalcVenda;

                    //  Geral
                    ViewBag.TotalVenda = totalVenda;
                    ViewBag.TotalDevo = totalDevoVenda;
                    ViewBag.BaseCalc = baseCalcVenda;

                    //  CNPJ
                    ViewBag.TotalVendaGrupo = totalVendaGrupo;
                    ViewBag.TotalDevoGrupo = totalDevoGrupo;
                    ViewBag.TotalBaseCalcuGrupo = baseCalcGrupo;
                    ViewBag.PercentualVendaGrupo = percentualGrupo;
                    ViewBag.ExcedenteGrupo = excedenteGrupo;
                    ViewBag.TotalExcedenteGrupo = impostoGrupo;
                    ViewBag.LimiteGrupo = limiteGrupo;
                    ViewBag.Grupo = gruposExecentes;

                    //  Anexo II ou Inciso I e II
                    ViewBag.VendaNcm = totalNcm;
                    ViewBag.TotalDevoNcm = totalDevoNcm;
                    ViewBag.BaseCalcNcm = baseCalcNcm;
                    ViewBag.PercentualVendaNcm = percentualVendaNcm;
                    ViewBag.LimiteNcm = limiteNcm;
                    ViewBag.ExcedenteNcm = excedenteNcm;
                    ViewBag.TotalExcedenteNcm = impostoNcm;

                    // Inciso I e II
                    ViewBag.VendasIncisoI = totalIncisoI;
                    ViewBag.VendasIncisoII = totalIncisoII;
                    ViewBag.TotalDevoIncisoI = totalDevoIncisoI;
                    ViewBag.TotalDevoIncisoII = totalDevoIncisoII;
                    ViewBag.BaseCalcIncisoI = baseCalcIncisoI;
                    ViewBag.BaseCalcIncisoII = baseCalcIncisoII;
                    ViewBag.PercentualVendaIncisoI = percentualVendaIncisoI;
                    ViewBag.PercentualVendaIncisoII = percentualVendaIncisoII;

                    //  Contribuinte
                    ViewBag.Contribuinte = totalContribuinte;
                    ViewBag.TotalDevoContribuite = totalDevoContribuinte;
                    ViewBag.BaseCalcContribuinte = baseCalcContribuinte;
                    ViewBag.PercentualVendaContribuinte = percentualVendaContribuinte;
                    ViewBag.LimiteContribuinte = limiteContribuinte;
                    ViewBag.ExcedenteContribuinte = excedenteContribuinte;
                    ViewBag.TotalExcedenteContribuinte = impostoContribuinte;

                    //  Não Contribuinte
                    ViewBag.NContribuinte = totalNcontribuinte;
                    ViewBag.TotalDevoNContribuinte = totalDevoNContribuinte;
                    ViewBag.BaseCalcNContribuinte = baseCalcNContribuinte;
                    ViewBag.PercentualVendaNContribuinte = percentualVendaNContribuinte;
                    ViewBag.LimiteNContribuinte = limiteNContribuinte;
                    ViewBag.ExcedenteNContribuinte = excedenteNContribuinte;
                    ViewBag.TotalExcedenteNContribuinte = impostoNContribuinte;

                    //  Tranferência
                    ViewBag.TotalTransferencia = totalTranferenciaSaida;
                    ViewBag.LimiteTransferencia = limiteTransferencia;
                    ViewBag.ExcedenteTransferencia = excedenteTranf;
                    ViewBag.TotalExcedenteTransferencia = impostoTransf;

                    //  Tranferência Interestadual
                    ViewBag.TotalTransferenciaInter = totalTranferenciaInter;
                    ViewBag.LimiteTransferenciaInter = limiteTransferenciaInter;
                    ViewBag.ExcedenteTransferenciaInter = excedenteTranfInter;
                    ViewBag.TotalExcedenteTransferenciaInter = impostoTransfInter;

                    //  Suspensão
                    ViewBag.BaseCalcSuspensao = totalVendasSuspensao;
                    ViewBag.TotalSuspensao = valorSuspensao;


                    //  Total Icms
                    ViewBag.TotalIcms = impostoNcm + impostoContribuinte + impostoNContribuinte + impostoTransf + impostoTransfInter + impostoGrupo + valorSuspensao;


                    //Dar
                    var dars = _darService.FindAll(null);
                    ViewBag.DarIcms = dars.Where(_ => _.Type.Equals("Icms")).Select(_ => _.Code).FirstOrDefault();
                    ViewBag.DarFunef = dars.Where(_ => _.Type.Equals("Funef")).Select(_ => _.Code).FirstOrDefault();
                    ViewBag.DarCotac = dars.Where(_ => _.Type.Equals("Cotac")).Select(_ => _.Code).FirstOrDefault();
                    ViewBag.DarFecop = dars.Where(_ => _.Type.Equals("Fecop")).Select(_ => _.Code).FirstOrDefault();
                }
                else if (type.Equals("incentivo"))
                {
                    //  Imposto Sobre Saida 

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
                            ViewBag.Uf = comp.County.State.UF;

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

                            var notifi = _notificationService.FindByCurrentMonth(companyId, month, year);

                            if (percentualVendas < Convert.ToDecimal(comp.VendaArt781))
                            {
                                if (notifi != null)
                                {
                                    Model.Notification nn = new Notification();
                                    nn.Description = "Venda p/ Cliente Credenciado no Art. 781 menor que " + comp.VendaArt781.ToString() + " %";
                                    nn.Percentual = percentualVendas;
                                    nn.MesRef = month;
                                    nn.AnoRef = year;
                                    nn.CompanyId = companyId;
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
                    //  Relatório Anexo Auto Peças

                    if (impAnexo == null)
                    {
                        ViewBag.Erro = 1;
                        return View();
                    }

                    var notas = _noteService.FindByNotes(companyId, year, month);
                    var products = _itemService.FindByProductsType(notas, Model.TypeTaxation.AP);

                    decimal totalFreteAPIE = 0;

                    foreach (var prod in products)
                    {
                        if (!prod.Note.Iest.Equals("") && prod.TaxationTypeId.Equals(1))
                            if (Convert.ToDecimal(prod.Diferencial) > 0)
                                totalFreteAPIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                    }

                    ViewBag.TotalFreteAPIE = totalFreteAPIE;

                    decimal valorNfe1NormalAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe1RetAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe2NormalAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe2RetAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe1NormalAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe1RetAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe2NormalAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe2RetAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal? gnrePagaAPIE = Math.Round(Convert.ToDecimal(notas.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreAp).Sum()), 2),
                             gnreNPagaAPIE = Math.Round(Convert.ToDecimal(notas.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNAp).Sum()), 2),
                             gnrePagaAPSIE = Math.Round(Convert.ToDecimal(notas.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreAp).Sum()), 2),
                             gnreNPagaAPSIE = Math.Round(Convert.ToDecimal(notas.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNAp).Sum()), 2),
                             icmsStAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalAPIE + valorNfe1RetAPIE + valorNfe2NormalAPIE + valorNfe2RetAPIE,
                             icmsStAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalAPSIE + valorNfe1RetAPSIE + valorNfe2NormalAPSIE + valorNfe2RetAPSIE,
                             totalApuradoAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsApurado).Sum()), 2),
                             totalApuradoAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsApurado).Sum()), 2),
                             totalDiefAPSIE = Convert.ToDecimal((totalApuradoAPSIE + totalFreteAPIE) - icmsStAPSIE + gnreNPagaAPSIE - gnrePagaAPSIE),
                             totalDiefAPIE = Convert.ToDecimal(totalApuradoAPIE + gnreNPagaAPIE - icmsStAPIE - gnrePagaAPIE - totalFreteAPIE);

                    int? qtdAPSIE = products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Count(),
                         qtdAPIE = products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Count();

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

                    decimal icmsAPnotaIE = Convert.ToDecimal(notas.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsAp).Sum()),
                            icmsAPnotaSIE = Convert.ToDecimal(notas.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsAp).Sum());
                    ViewBag.IcmsAPIE = icmsAPnotaIE;
                    ViewBag.IcmsAPSIE = icmsAPnotaSIE;

                    decimal icmsAPagarAPSIE = Convert.ToDecimal(totalDiefAPSIE - icmsAPnotaSIE),
                            icmsAPagarAPIE = Convert.ToDecimal(totalDiefAPIE - icmsAPnotaIE);
                    ViewBag.IcmsAPagarAPSIE = icmsAPagarAPSIE;
                    ViewBag.IcmsAPagarAPIE = icmsAPagarAPIE;

                    decimal icmsAPAPagar = 0;

                    if (icmsAPagarAPSIE > 0)
                        icmsAPAPagar += icmsAPagarAPSIE;

                    if (icmsAPagarAPIE > 0)
                        icmsAPAPagar += icmsAPagarAPIE;

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
                    var mesAnterior = importMes.NameMonthPrevious(mesAtual);
                    decimal saldoCredorAnterior = 0;

                    string ano = year;

                    if (mesAtual.Equals(1))
                        ano = (Convert.ToInt32(year) - 1).ToString();

                    var creditLast = _creditBalanceService.FindByLastMonth(companyId, mesAnterior, ano);

                    if (creditLast != null)
                        saldoCredorAnterior = Convert.ToDecimal(creditLast.Saldo);

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
                        saldoCredor = 0;

                    ViewBag.SaldoCredor = saldoCredor;

                    var creditCurrent = _creditBalanceService.FindByCurrentMonth(companyId, month, year);

                    if (creditCurrent == null)
                    {
                        Model.CreditBalance credit = new Model.CreditBalance();
                        credit.CompanyId = companyId;
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
                    //  Relatório Anexo Medicamentos

                    var clientesAll = _clientService.FindByCompany(companyId);
                    var nContribuintes = clientesAll.Where(_ => _.TypeClientId.Equals(2)).Select(_ => _.Document).ToList();

                    List<List<string>> elencadaInterna = new List<List<string>>();
                    List<List<string>> elencadaInterestadual = new List<List<string>>();
                    List<List<string>> daselencadaInterna = new List<List<string>>();
                    List<List<string>> daselencadaInterestadual = new List<List<string>>();

                    notes = importXml.NFeAll(directoryNfeExit);

                    decimal valorElencadaInterna = 0, valorElencadaInterestadual = 0, valorDaselencadaInterna = 0, valorDaselencadaInterestadual = 0;

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i][1]["finNFe"] == "4")
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        bool clenteCredenciado = false, ncm = false;

                        if (notes[i][3].ContainsKey("CNPJ"))
                        {
                            if (nContribuintes.Contains(notes[i][3]["CNPJ"]))
                                clenteCredenciado = true;

                            bool existe = false;

                            if (clientesAll.Select(_ => _.Document).Contains(notes[i][3]["CNPJ"]))
                                existe = true;

                            if (!existe)
                            {
                                ViewBag.Erro = 1;
                                return View();
                            }
                        }

                        decimal valorNF = 0;

                        for (int j = 0; j < notes[i].Count; j++)
                        {
                            if (notes[i][j].ContainsKey("NCM"))
                                ncm = _itemService.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId), notes[i][j]["NCM"].ToString());

                            if (ncm)
                            {
                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    valorNF += Convert.ToDecimal(notes[i][j]["vProd"]);

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    valorNF += Convert.ToDecimal(notes[i][j]["vFrete"]);

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    valorNF -= Convert.ToDecimal(notes[i][j]["vDesc"]);

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    valorNF += Convert.ToDecimal(notes[i][j]["vOutro"]);

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    valorNF += Convert.ToDecimal(notes[i][j]["vSeg"]);

                            }

                        }

                        string CNPJ = notes[i][3].ContainsKey("CNPJ") ? notes[i][3]["CNPJ"] : "";
                        string IE = notes[i][3].ContainsKey("IE") ? notes[i][3]["IE"] : "";

                        if (valorNF > 0)
                        {
                            List<string> note = new List<string>();

                            note.Add(notes[i][1]["nNF"]);
                            note.Add(notes[i][3]["xNome"]);
                            note.Add(notes[i][3]["UF"]);
                            note.Add(CNPJ);
                            note.Add(IE);
                            note.Add(valorNF.ToString());
                            note.Add("0");

                            if (clenteCredenciado == true)
                            {
                                if (notes[i][1]["idDest"] == "1")
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
                                if (notes[i][1]["idDest"] == "1")
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