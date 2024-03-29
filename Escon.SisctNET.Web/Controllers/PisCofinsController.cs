﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Escon.SisctNET.Web.Controllers
{
    public class PisCofinsController : ControllerBaseSisctNET
    {
        private readonly ITaxationNcmService _service;
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly INcmService _ncmService;
        private readonly ITaxService _taxService;
        private readonly IBaseService _baseService;
        private readonly ICfopService _cfopService;
        private readonly ICstService _cstService;
        private readonly IHostingEnvironment _appEnvironment;

        public PisCofinsController(
            ITaxationNcmService service,
            ICompanyService companyService,
            IConfigurationService configurationService,
            INcmService ncmService,
            ITaxService taxService,
            IBaseService baseService,
            ICfopService cfopService,
            ICstService cstService,
            IHostingEnvironment appEnvironment,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Company")
        {
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _ncmService = ncmService;
            _taxService = taxService;
            _baseService = baseService;
            _cfopService = cfopService;
            _cstService = cstService;
            _appEnvironment = appEnvironment;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public async Task<IActionResult> Relatory(long companyId, string year, string month, string trimestre, string type, 
            long cfopid, string archive, IFormFile arquivo)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _companyService.FindById(companyId, null);

                ViewBag.Year = year;
                ViewBag.Month = month;
                ViewBag.Type = type;
                ViewBag.Trimestre = trimestre;
                ViewBag.Archive = archive;
                ViewBag.Company = comp;

                if (comp.CountingType.Name.Equals("Nenhum"))
                {
                    ViewBag.Erro = 1;
                    return View();
                }

                var NfeExit = _configurationService.FindByName("NFe Saida", null);
                var NfeEntry = _configurationService.FindByName("NFe", null);

                var importXml = new Xml.Import(_cfopService, _service);
                var importSped = new Sped.Import(_cfopService, _service);
                var importTrimestre = new Period.Trimestre();
                var importMes = new Period.Month();
                var importDir = new Diretorio.Import();

                string directoryNfeExit = "";

                if (archive != null)
                {
                    if (archive.Equals("xmlE"))
                        directoryNfeExit = importDir.SaidaEmpresa(comp, NfeExit.Value, year, month);
                    else
                        directoryNfeExit = importDir.SaidaSefaz(comp, NfeExit.Value, year, month);
                }

                string directoryNfeEntry = importDir.Entrada(comp, NfeEntry.Value, year, month);

                var cfopAll = _cfopService.FindByType(null);

                //  Saida
                var cfopsVenda = _cfopService.FindByCfopVenda(cfopAll)
                    .Select(_ => _.Code)
                    .Distinct()
                    .ToList();
                var cfopsVendaIM = _cfopService.FindByCfopVendaIM(cfopAll)
                    .Select(_ => _.Code)
                    .Distinct()
                    .ToList();
                var cfopsVendaST = _cfopService.FindByCfopVendaST(cfopAll)
                    .Select(_ => _.Code)
                    .Distinct()
                    .ToList();
                var cfopsBoniVenda = _cfopService.FindByCfopBonificacaoVenda(cfopAll)
                    .Select(_ => _.Code)
                    .Distinct()
                    .ToList();

                //  Transferencia
                var cfopsTransf = _cfopService.FindByCfopTransferencia(cfopAll)
                    .Select(_ => _.Code)
                    .Distinct()
                    .ToList();
                var cfopsTransfST = _cfopService.FindByCfopTransferenciaST(cfopAll)
                    .Select(_ => _.Code)
                    .Distinct()
                    .ToList();

                List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

                var ncms = _service.FindByCompany(comp.Document);

                if (type.Equals("resumoCfop"))
                {
                    //  Resumo CFOP

                    List<List<string>> cfops = new List<List<string>>();

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

                                    //  Bases
                                    cfop.Add("0");
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

                            if (notes[i][j].ContainsKey("pPIS") && notes[i][j].ContainsKey("CSTP"))
                            {
                                cfops[pos][3] = (Convert.ToDecimal(cfops[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                cfops[pos][4] = (Convert.ToDecimal(cfops[pos][4]) + Convert.ToDecimal(notes[i][j]["vPIS"])).ToString();
                            }

                            if (notes[i][j].ContainsKey("pCOFINS") && notes[i][j].ContainsKey("CSTC"))
                            {
                                cfops[pos][22] = (Convert.ToDecimal(cfops[pos][22]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + Convert.ToDecimal(notes[i][j]["vCOFINS"])).ToString();
                            }

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

                                if (notes[i][j].ContainsKey("pPIS") && notes[i][j].ContainsKey("CSTP"))
                                {
                                    cfops[pos][7] = (Convert.ToDecimal(cfops[pos][7]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    cfops[pos][8] = (Convert.ToDecimal(cfops[pos][8]) + Convert.ToDecimal(notes[i][j]["vPIS"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("pCOFINS") && notes[i][j].ContainsKey("CSTC"))
                                {
                                    cfops[pos][23] = (Convert.ToDecimal(cfops[pos][23]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    cfops[pos][9] = (Convert.ToDecimal(cfops[pos][9]) + Convert.ToDecimal(notes[i][j]["vCOFINS"])).ToString();
                                }


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

                                if (notes[i][j].ContainsKey("pPIS") && notes[i][j].ContainsKey("CSTP"))
                                {
                                    cfops[pos][11] = (Convert.ToDecimal(cfops[pos][11]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    cfops[pos][12] = (Convert.ToDecimal(cfops[pos][12]) + Convert.ToDecimal(notes[i][j]["vPIS"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("pCOFINS") && notes[i][j].ContainsKey("CSTC"))
                                {
                                    cfops[pos][24] = (Convert.ToDecimal(cfops[pos][24]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    cfops[pos][13] = (Convert.ToDecimal(cfops[pos][13]) + Convert.ToDecimal(notes[i][j]["vCOFINS"])).ToString();
                                }
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

                                if (notes[i][j].ContainsKey("pPIS") && notes[i][j].ContainsKey("CSTP"))
                                {
                                    cfops[pos][15] = (Convert.ToDecimal(cfops[pos][15]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    cfops[pos][16] = (Convert.ToDecimal(cfops[pos][16]) + Convert.ToDecimal(notes[i][j]["vPIS"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("pCOFINS") && notes[i][j].ContainsKey("CSTC"))
                                {
                                    cfops[pos][25] = (Convert.ToDecimal(cfops[pos][25]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    cfops[pos][17] = (Convert.ToDecimal(cfops[pos][17]) + Convert.ToDecimal(notes[i][j]["vCOFINS"])).ToString();
                                }
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

                                if (notes[i][j].ContainsKey("pPIS") && notes[i][j].ContainsKey("CSTP"))
                                {
                                    cfops[pos][19] = (Convert.ToDecimal(cfops[pos][19]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    cfops[pos][20] = (Convert.ToDecimal(cfops[pos][20]) + Convert.ToDecimal(notes[i][j]["vPIS"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("pCOFINS") && notes[i][j].ContainsKey("CSTC"))
                                {
                                    cfops[pos][26] = (Convert.ToDecimal(cfops[pos][26]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    cfops[pos][21] = (Convert.ToDecimal(cfops[pos][21]) + Convert.ToDecimal(notes[i][j]["vCOFINS"])).ToString();
                                }
                            }

                        }

                    }

                    ViewBag.Cfop = cfops.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
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

                        string CFOP = "", cstP = "";
                        decimal vProd = 0, basePis = 0, vPis = 0;
                        int pos = -1;

                        for (int j = 0; j < notes[i].Count(); j++)
                        {

                            if ((notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM")))
                            {
                                pos = -1;
                                CFOP = notes[i][j]["CFOP"];

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

                            if (notes[i][j].ContainsKey("CSTP"))
                            {
                                cstP = notes[i][j]["CSTP"];
                                basePis = 0;
                                vPis = 0;
                            }

                            if (notes[i][j].ContainsKey("CSTP") && notes[i][j].ContainsKey("pPIS"))
                            {
                                basePis += Convert.ToDecimal(notes[i][j]["vBC"]);
                                vPis += Convert.ToDecimal(notes[i][j]["vPIS"]);
                            }

                            if (notes[i][j].ContainsKey("CSTC"))
                            {

                                for (int e = 0; e < cfops.Count(); e++)
                                {
                                    if (cfops[e][0].Equals(CFOP) && cfops[e][2].Equals(cstP) && cfops[e][3].Equals(notes[i][j]["CSTC"]))
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
                                    cc.Add(cstP);
                                    cc.Add(notes[i][j]["CSTC"]);
                                    cc.Add("0");
                                    cc.Add("0");
                                    cc.Add("0");
                                    cc.Add("0");
                                    cc.Add("0");
                                    cfops.Add(cc);
                                    pos = cfops.Count() - 1;
                                }
                                cfops[pos][4] = (Convert.ToDecimal(cfops[pos][4]) + vProd).ToString();
                                cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + basePis).ToString();
                                cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) + vPis).ToString();
                            }


                            if (notes[i][j].ContainsKey("CSTC") && notes[i][j].ContainsKey("pCOFINS"))
                            {
                                cfops[pos][7] = (Convert.ToDecimal(cfops[pos][7]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                cfops[pos][8] = (Convert.ToDecimal(cfops[pos][8]) + Convert.ToDecimal(notes[i][j]["vCOFINS"])).ToString();

                            }
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

                        string cfop = "", pPIS = "0", pCOFINS = "0";
                        decimal vProd = 0, vBCP = 0, vPIS = 0, vBCC = 0, vCOFINS = 0;
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
                                        if (cfops[e][0].Equals(cfop) && Convert.ToDecimal(cfops[e][4]).Equals(Convert.ToDecimal(pPIS)) && Convert.ToDecimal(cfops[e][7]).Equals(Convert.ToDecimal(pCOFINS)))
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
                                        cc.Add(vProd.ToString());
                                        cc.Add(vBCP.ToString());
                                        cc.Add(pPIS);
                                        cc.Add(vPIS.ToString());
                                        cc.Add(vBCC.ToString());
                                        cc.Add(pCOFINS);
                                        cc.Add(vCOFINS.ToString());
                                        cfops.Add(cc);
                                    }
                                    else
                                    {
                                        cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + vProd).ToString();
                                        cfops[pos][3] = (Convert.ToDecimal(cfops[pos][3]) + vBCP).ToString();
                                        cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + vPIS).ToString();
                                        cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) + vBCC).ToString();
                                        cfops[pos][8] = (Convert.ToDecimal(cfops[pos][8]) + vCOFINS).ToString();
                                    }
                                }

                                pos = -1;
                                status = false;
                                cfop = notes[i][j]["CFOP"];
                                vBCP = 0;
                                vPIS = 0;
                                vBCC = 0;
                                vCOFINS = 0;
                                pPIS = "0";
                                pCOFINS = "0";

                                if (notes[i][j].ContainsKey("vProd"))
                                {
                                    vProd = 0;
                                    vProd += Convert.ToDecimal(notes[i][j]["vProd"]);
                                    status = true;
                                }

                                if (notes[i][j].ContainsKey("vFrete"))
                                    vProd += Convert.ToDecimal(notes[i][j]["vFrete"]);

                                if (notes[i][j].ContainsKey("vDesc"))
                                    vProd -= Convert.ToDecimal(notes[i][j]["vDesc"]);

                                if (notes[i][j].ContainsKey("vOutro"))
                                    vProd += Convert.ToDecimal(notes[i][j]["vOutro"]);

                                if (notes[i][j].ContainsKey("vSeg"))
                                    vProd += Convert.ToDecimal(notes[i][j]["vSeg"]);
                            }

                            if (notes[i][j].ContainsKey("pPIS") && notes[i][j].ContainsKey("CSTP"))
                            {
                                vBCP = Convert.ToDecimal(notes[i][j]["vBC"]);
                                pPIS = notes[i][j]["pPIS"];
                                vPIS = Convert.ToDecimal(notes[i][j]["vPIS"]);
                                status = true;
                            }

                            if (notes[i][j].ContainsKey("pCOFINS") && notes[i][j].ContainsKey("CSTC"))
                            {
                                vBCC = Convert.ToDecimal(notes[i][j]["vBC"]);
                                pCOFINS = notes[i][j]["pCOFINS"];
                                vCOFINS = Convert.ToDecimal(notes[i][j]["vCOFINS"]);
                                status = true;
                            }

                            if (notes[i][j].ContainsKey("vNF"))
                            {
                                for (int e = 0; e < cfops.Count(); e++)
                                {
                                    if (cfops[e][0].Equals(cfop) && Convert.ToDecimal(cfops[e][4]).Equals(Convert.ToDecimal(pPIS)) && Convert.ToDecimal(cfops[e][7]).Equals(Convert.ToDecimal(pCOFINS)))
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
                                    cc.Add(vProd.ToString());
                                    cc.Add(vBCP.ToString());
                                    cc.Add(pPIS);
                                    cc.Add(vPIS.ToString());
                                    cc.Add(vBCC.ToString());
                                    cc.Add(pCOFINS);
                                    cc.Add(vCOFINS.ToString());
                                    cfops.Add(cc);
                                }
                                else
                                {
                                    cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + vProd).ToString();
                                    cfops[pos][3] = (Convert.ToDecimal(cfops[pos][3]) + vBCP).ToString();
                                    cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + vPIS).ToString();
                                    cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) + vBCC).ToString();
                                    cfops[pos][8] = (Convert.ToDecimal(cfops[pos][8]) + vCOFINS).ToString();
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
                    decimal totalNota = 0, valorContabil = 0, basePis = 0, valorPis = 0, baseCofins = 0, valorCofins = 0;
                    ViewBag.Code = cfop.Code;

                    notes = importXml.NFeCFOP(directoryNfeExit, cfop.Code);

                    List<List<string>> resumoNote = new List<List<string>>();

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {

                        if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i].Count() <= 5)
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
                            note.Add(notes[i][1]["dhEmi"]);
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

                            if (notes[i][j].ContainsKey("pPIS") && notes[i][j].ContainsKey("CSTP"))
                            {
                                resumoNote[pos][3] = (Convert.ToDecimal(resumoNote[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                basePis += Convert.ToDecimal(notes[i][j]["vBC"]);
                                resumoNote[pos][4] = (Convert.ToDecimal(resumoNote[pos][4]) + Convert.ToDecimal(notes[i][j]["vPIS"])).ToString();
                                valorPis += Convert.ToDecimal(notes[i][j]["vPIS"]);
                            }

                            if (notes[i][j].ContainsKey("pCOFINS") && notes[i][j].ContainsKey("CSTC"))
                            {
                                resumoNote[pos][5] = (Convert.ToDecimal(resumoNote[pos][5]) + Convert.ToDecimal(notes[i][j]["vCOFINS"])).ToString();
                                valorCofins += Convert.ToDecimal(notes[i][j]["vCOFINS"]);
                                resumoNote[pos][8] = (Convert.ToDecimal(resumoNote[pos][8]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                baseCofins += Convert.ToDecimal(notes[i][j]["vBC"]);
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
                    ViewBag.BasePIS = basePis;
                    ViewBag.ValorPIS = valorPis;
                    ViewBag.BaseCOFINS = baseCofins;
                    ViewBag.ValorCOFINS = valorCofins;
                }
                else if (type.Equals("resumoProdNcm"))
                {
                    //  Resumo Venda Produto/NCM 

                    List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();
                    List<string> codeProdMono = new List<string>();
                    List<string> codeProdNormal = new List<string>();
                    List<string> ncmMono = new List<string>();
                    List<string> ncmNormal = new List<string>();
                    List<List<string>> resumoNcm = new List<List<string>>();

                    var ncmsAll = _ncmService.FindAll(null);

                    cfopsVenda.AddRange(cfopsVendaST);
                    cfopsVenda.AddRange(cfopsTransf);
                    cfopsVenda.AddRange(cfopsTransfST);

                    notes = importXml.NFeAll(directoryNfeExit, cfopsVenda);

                    decimal valorProduto = 0, valorPis = 0, valorCofins = 0, quantidadeTotal = 0;

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i][1]["finNFe"] == "4")
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        if (notes[i][1].ContainsKey("dhEmi"))
                        {
                            ncmsTaxation = _service.FindAllInDate(ncms, Convert.ToDateTime(notes[i][1]["dhEmi"]));

                            codeProdMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(2)).Select(_ => _.CodeProduct).ToList();
                            codeProdNormal = ncmsTaxation.Where(_ => !_.TaxationTypeNcmId.Equals(2)).Select(_ => _.CodeProduct).ToList();
                            ncmMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(2)).Select(_ => _.Ncm.Code).ToList();
                            ncmNormal = ncmsTaxation.Where(_ => !_.TaxationTypeNcmId.Equals(2)).Select(_ => _.Ncm.Code).ToList();
                        }

                        string cProd = "", NCM = "", xProd = "";
                        decimal vProd = 0, quantidade = 0;
                        int pos = -1;


                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM"))
                            {
                                cProd = notes[i][j]["cProd"];
                                NCM = notes[i][j]["NCM"];
                                xProd = notes[i][j]["xProd"];
                                quantidade = Convert.ToDecimal(notes[i][j]["qCom"]);

                                if (notes[i][j].ContainsKey("vProd"))
                                {
                                    vProd = 0;
                                    vProd += Convert.ToDecimal(notes[i][j]["vProd"]);
                                }

                                if (notes[i][j].ContainsKey("vFrete"))
                                {
                                    vProd += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                }

                                if (notes[i][j].ContainsKey("vDesc"))
                                {
                                    vProd -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }

                                if (notes[i][j].ContainsKey("vOutro"))
                                {
                                    vProd += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }

                                if (notes[i][j].ContainsKey("vSeg"))
                                {
                                    vProd += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }

                            }

                            if (notes[i][j].ContainsKey("CSOSN"))
                            {
                                pos = -1;

                                if (comp.Taxation == "Produto")
                                {
                                    if (codeProdMono.Contains(cProd) && ncmMono.Contains(NCM))
                                    {
                                        for (int k = 0; k < resumoNcm.Count(); k++)
                                        {
                                            if (resumoNcm[k][0].Equals(NCM) && resumoNcm[k][1].Equals(cProd))
                                            {
                                                pos = k;
                                                break;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> ncmTemp = new List<string>();
                                            ncmTemp.Add(NCM);
                                            ncmTemp.Add(cProd);
                                            ncmTemp.Add("0");
                                            ncmTemp.Add("0");
                                            ncmTemp.Add(vProd.ToString());
                                            ncmTemp.Add(xProd);
                                            ncmTemp.Add("0");
                                            resumoNcm.Add(ncmTemp);
                                            pos = resumoNcm.Count() - 1;
                                        }

                                        if (pos >= 0)
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + vProd).ToString();
                                            resumoNcm[pos][6] = (Convert.ToDecimal(resumoNcm[pos][6]) + quantidade).ToString();

                                        }

                                        valorProduto += vProd;
                                        quantidadeTotal += quantidade;
                                    }
                                    else if (codeProdNormal.Contains(cProd) && ncmNormal.Contains(NCM))
                                    {
                                        for (int k = 0; k < resumoNcm.Count(); k++)
                                        {
                                            if (resumoNcm[k][0].Equals(NCM) && resumoNcm[k][1].Equals(cProd))
                                            {
                                                pos = k;
                                                break;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> ncmTemp = new List<string>();
                                            ncmTemp.Add(NCM);
                                            ncmTemp.Add(cProd);
                                            ncmTemp.Add("0");
                                            ncmTemp.Add("0");
                                            ncmTemp.Add(vProd.ToString());
                                            ncmTemp.Add(xProd);
                                            ncmTemp.Add("0");
                                            resumoNcm.Add(ncmTemp);
                                            pos = resumoNcm.Count() - 1;
                                        }

                                        if (pos >= 0)
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + vProd).ToString();
                                            resumoNcm[pos][6] = (Convert.ToDecimal(resumoNcm[pos][6]) + quantidade).ToString();

                                        }

                                        valorProduto += vProd;
                                        quantidadeTotal += quantidade;
                                    }
                                    else
                                    {
                                        ViewBag.NCM = NCM;
                                        ViewBag.Erro = 2;
                                        return View();
                                    }
                                }
                                else
                                {
                                    if (ncmMono.Contains(NCM))
                                    {
                                        for (int k = 0; k < resumoNcm.Count(); k++)
                                        {
                                            if (resumoNcm[k][0].Equals(NCM) && resumoNcm[k][1].Equals(cProd))
                                            {
                                                pos = k;
                                                break;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> ncmTemp = new List<string>();
                                            ncmTemp.Add(NCM);
                                            ncmTemp.Add(cProd);
                                            ncmTemp.Add("0");
                                            ncmTemp.Add("0");
                                            ncmTemp.Add(vProd.ToString());
                                            ncmTemp.Add(xProd);
                                            ncmTemp.Add("0");
                                            resumoNcm.Add(ncmTemp);
                                            pos = resumoNcm.Count() - 1;
                                        }

                                        if (pos >= 0)
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + vProd).ToString();
                                            resumoNcm[pos][6] = (Convert.ToDecimal(resumoNcm[pos][6]) + quantidade).ToString();

                                        }

                                        valorProduto += vProd;
                                        quantidadeTotal += quantidade;
                                    }
                                    else if (ncmNormal.Contains(NCM))
                                    {
                                        for (int k = 0; k < resumoNcm.Count(); k++)
                                        {
                                            if (resumoNcm[k][0].Equals(NCM) && resumoNcm[k][1].Equals(cProd))
                                            {
                                                pos = k;
                                                break;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> ncmTemp = new List<string>();
                                            ncmTemp.Add(NCM);
                                            ncmTemp.Add(cProd);
                                            ncmTemp.Add("0");
                                            ncmTemp.Add("0");
                                            ncmTemp.Add(vProd.ToString());
                                            ncmTemp.Add(xProd);
                                            ncmTemp.Add("0");
                                            resumoNcm.Add(ncmTemp);
                                            pos = resumoNcm.Count() - 1;
                                        }

                                        if (pos >= 0)
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + vProd).ToString();
                                            resumoNcm[pos][6] = (Convert.ToDecimal(resumoNcm[pos][6]) + quantidade).ToString();

                                        }

                                        valorProduto += vProd;
                                        quantidadeTotal += quantidade;
                                    }
                                    else
                                    {
                                        ViewBag.NCM = notes[i][j]["NCM"];
                                        ViewBag.Erro = 2;
                                        return View();
                                    }
                                }

                            }

                            if (notes[i][j].ContainsKey("CST"))
                            {

                                pos = -1;

                                if (comp.Taxation == "Produto")
                                {
                                    if (codeProdMono.Contains(cProd) && ncmMono.Contains(NCM))
                                    {
                                        for (int k = 0; k < resumoNcm.Count(); k++)
                                        {
                                            if (resumoNcm[k][0].Equals(NCM) && resumoNcm[k][1].Equals(cProd))
                                            {
                                                pos = k;
                                                break;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> ncmTemp = new List<string>();
                                            ncmTemp.Add(NCM);
                                            ncmTemp.Add(cProd);
                                            ncmTemp.Add("0");
                                            ncmTemp.Add("0");
                                            ncmTemp.Add(vProd.ToString());
                                            ncmTemp.Add(xProd);
                                            resumoNcm.Add(ncmTemp);
                                            pos = resumoNcm.Count() - 1;
                                        }

                                        if (pos >= 0)
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + vProd).ToString();

                                        }

                                        valorProduto += vProd;
                                    }
                                    else if (codeProdNormal.Contains(cProd) && ncmNormal.Contains(NCM))
                                    {
                                        for (int k = 0; k < resumoNcm.Count(); k++)
                                        {
                                            if (resumoNcm[k][0].Equals(NCM) && resumoNcm[k][1].Equals(cProd))
                                            {
                                                pos = k;
                                                break;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> ncmTemp = new List<string>();
                                            ncmTemp.Add(NCM);
                                            ncmTemp.Add(cProd);
                                            ncmTemp.Add("0");
                                            ncmTemp.Add("0");
                                            ncmTemp.Add(vProd.ToString());
                                            ncmTemp.Add(xProd);
                                            resumoNcm.Add(ncmTemp);
                                            pos = resumoNcm.Count() - 1;
                                        }

                                        if (pos >= 0)
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + vProd).ToString();

                                        }

                                        valorProduto += vProd;
                                    }
                                    else
                                    {
                                        ViewBag.NCM = NCM;
                                        ViewBag.Erro = 2;
                                        return View();
                                    }
                                }
                                else
                                {
                                    if (ncmMono.Contains(NCM))
                                    {
                                        for (int k = 0; k < resumoNcm.Count(); k++)
                                        {
                                            if (resumoNcm[k][0].Equals(NCM) && resumoNcm[k][1].Equals(cProd))
                                            {
                                                pos = k;
                                                break;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> ncmTemp = new List<string>();
                                            ncmTemp.Add(NCM);
                                            ncmTemp.Add(cProd);
                                            ncmTemp.Add("0");
                                            ncmTemp.Add("0");
                                            ncmTemp.Add(vProd.ToString());
                                            ncmTemp.Add(xProd);
                                            resumoNcm.Add(ncmTemp);
                                            pos = resumoNcm.Count() - 1;
                                        }

                                        if (pos >= 0)
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + vProd).ToString();

                                        }

                                        valorProduto += vProd;
                                    }
                                    else if (ncmNormal.Contains(NCM))
                                    {
                                        for (int k = 0; k < resumoNcm.Count(); k++)
                                        {
                                            if (resumoNcm[k][0].Equals(NCM) && resumoNcm[k][1].Equals(cProd))
                                            {
                                                pos = k;
                                                break;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> ncmTemp = new List<string>();
                                            ncmTemp.Add(NCM);
                                            ncmTemp.Add(cProd);
                                            ncmTemp.Add("0");
                                            ncmTemp.Add("0");
                                            ncmTemp.Add(vProd.ToString());
                                            ncmTemp.Add(xProd);
                                            resumoNcm.Add(ncmTemp);
                                            pos = resumoNcm.Count() - 1;
                                        }

                                        if (pos >= 0)
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + vProd).ToString();

                                        }

                                        valorProduto += vProd;
                                    }
                                    else
                                    {
                                        ViewBag.NCM = notes[i][j]["NCM"];
                                        ViewBag.Erro = 2;
                                        return View();
                                    }
                                }

                            }

                            if (notes[i][j].ContainsKey("pPIS") && notes[i][j].ContainsKey("CSTP"))
                            {
                                resumoNcm[pos][2] = (Convert.ToDecimal(resumoNcm[pos][2]) + Convert.ToDecimal(notes[i][j]["vPIS"])).ToString();
                                valorPis += Convert.ToDecimal(notes[i][j]["vPIS"]);
                            }

                            if (notes[i][j].ContainsKey("pCOFINS") && notes[i][j].ContainsKey("CSTC"))
                            {
                                resumoNcm[pos][3] = (Convert.ToDecimal(resumoNcm[pos][3]) + Convert.ToDecimal(notes[i][j]["vCOFINS"])).ToString();
                                valorCofins += Convert.ToDecimal(notes[i][j]["vCOFINS"]);
                            }
                        }

                    }

                    ViewBag.Ncm = resumoNcm.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                    ViewBag.Quantidade = quantidadeTotal;
                    ViewBag.ValorProduto = valorProduto;
                    ViewBag.ValorPis = valorPis;
                    ViewBag.ValorCofins = valorCofins;
                }
                else if (type.Equals("resumoCfopMono"))
                {
                    //  Resumo CFOP Monofásico

                    List<List<string>> cfops = new List<List<string>>();

                    List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();
                    List<string> codeProdMono = new List<string>();
                    List<string> codeProdNormal = new List<string>();
                    List<string> ncmMono = new List<string>();
                    List<string> ncmNormal = new List<string>();

                    var prodAll = ncms.Select(_ => _.CodeProduct).ToList();
                    var ncmsAll = ncms.Select(_ => _.Ncm.Code).ToList();

                    notes = importXml.NFeAll(directoryNfeExit);

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        if (notes[i][1].ContainsKey("dhEmi"))
                        {
                            ncmsTaxation = _service.FindAllInDate(ncms, Convert.ToDateTime(notes[i][1]["dhEmi"]));
                            codeProdMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(2) || _.TaxationTypeNcmId.Equals(3) || _.TaxationTypeNcmId.Equals(4) || _.TaxationTypeNcmId.Equals(5)).Select(_ => _.CodeProduct).ToList();
                            codeProdNormal = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(1) || _.TaxationTypeNcmId.Equals(6)).Select(_ => _.CodeProduct).ToList();
                            ncmMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(2) || _.TaxationTypeNcmId.Equals(3) || _.TaxationTypeNcmId.Equals(4) || _.TaxationTypeNcmId.Equals(5)).Select(_ => _.Ncm.Code).ToList();
                            ncmNormal = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(1) || _.TaxationTypeNcmId.Equals(6)).Select(_ => _.Ncm.Code).ToList();
                        }


                        bool status = false;
                        decimal vProd = 0;
                        string cProd = "", NCM = "", CFOP = "", CSOSN = "";
                        int pos = -1;

                        for (int j = 0; j < notes[i].Count(); j++)
                        {

                            if (notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM"))
                            {
                                cProd = notes[i][j]["cProd"];
                                NCM = notes[i][j]["NCM"];
                                CFOP = notes[i][j]["CFOP"];

                                if (notes[i][j].ContainsKey("vProd"))
                                {
                                    vProd = 0;
                                    vProd += Convert.ToDecimal(notes[i][j]["vProd"]);
                                }

                                if (notes[i][j].ContainsKey("vFrete"))
                                {
                                    vProd += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                }

                                if (notes[i][j].ContainsKey("vDesc"))
                                {
                                    vProd -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }

                                if (notes[i][j].ContainsKey("vOutro"))
                                {
                                    vProd += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }

                                if (notes[i][j].ContainsKey("vSeg"))
                                {
                                    vProd += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }

                            }

                            if (notes[i][j].ContainsKey("CSOSN"))
                            {
                                CSOSN = notes[i][j]["CSOSN"];
                                status = false;
                                pos = -1;

                                if (notes[i][j]["CSOSN"] == "500")
                                {
                                    if (comp.Taxation == "Produto")
                                    {
                                        if (codeProdMono.Contains(cProd) && ncmMono.Contains(NCM))
                                        {
                                            status = true;

                                            for (int k = 0; k < cfops.Count(); k++)
                                            {
                                                if (cfops[k][0].Equals(CFOP))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var cfp = _cfopService.FindByCode(CFOP);
                                                List<string> cfop = new List<string>();
                                                cfop.Add(CFOP);
                                                cfop.Add(cfp.Description);

                                                // Geral
                                                cfop.Add("0");
                                                cfop.Add("0");
                                                cfop.Add("0");
                                                cfop.Add("0");
                                                cfop.Add("0");

                                                cfops.Add(cfop);
                                                pos = cfops.Count() - 1;
                                            }

                                            cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + vProd).ToString();
                                        }
                                        else if (codeProdNormal.Contains(cProd) && ncmNormal.Contains(NCM))
                                        {
                                            pos = -1;
                                            status = false;
                                            continue;
                                        }
                                        else
                                        {
                                            ViewBag.NCM = NCM;
                                            ViewBag.Erro = 2;
                                            return View();
                                        }
                                    }
                                    else
                                    {
                                        if (ncmMono.Contains(NCM))
                                        {
                                            status = true;


                                            for (int k = 0; k < cfops.Count(); k++)
                                            {
                                                if (cfops[k][0].Equals(CFOP))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var cfp = _cfopService.FindByCode(CFOP);
                                                List<string> cfop = new List<string>();
                                                cfop.Add(CFOP);
                                                cfop.Add(cfp.Description);

                                                // Geral
                                                cfop.Add("0");
                                                cfop.Add("0");
                                                cfop.Add("0");
                                                cfop.Add("0");
                                                cfop.Add("0");

                                                cfops.Add(cfop);
                                                pos = cfops.Count() - 1;
                                            }

                                            cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + vProd).ToString();
                                        }
                                        else if (ncmNormal.Contains(NCM))
                                        {
                                            pos = -1;
                                            status = false;
                                            continue;
                                        }
                                        else
                                        {
                                            ViewBag.NCM = notes[i][j]["NCM"];
                                            ViewBag.Erro = 2;
                                            return View();
                                        }
                                    }

                                }

                            }

                            if (notes[i][j].ContainsKey("CST"))
                            {
                                CSOSN = "500";
                                status = false;
                                pos = -1;

                                if (CSOSN == "500")
                                {
                                    if (comp.Taxation == "Produto")
                                    {
                                        if (codeProdMono.Contains(cProd) && ncmMono.Contains(NCM))
                                        {
                                            status = true;

                                            for (int k = 0; k < cfops.Count(); k++)
                                            {
                                                if (cfops[k][0].Equals(CFOP))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var cfp = _cfopService.FindByCode(CFOP);
                                                List<string> cfop = new List<string>();
                                                cfop.Add(CFOP);
                                                cfop.Add(cfp.Description);

                                                // Geral
                                                cfop.Add("0");
                                                cfop.Add("0");
                                                cfop.Add("0");
                                                cfop.Add("0");
                                                cfop.Add("0");

                                                cfops.Add(cfop);
                                                pos = cfops.Count() - 1;
                                            }

                                            cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + vProd).ToString();
                                        }
                                        else if (codeProdNormal.Contains(cProd) && ncmNormal.Contains(NCM))
                                        {
                                            pos = -1;
                                            status = false;
                                            continue;
                                        }
                                        else
                                        {
                                            ViewBag.NCM = NCM;
                                            ViewBag.Erro = 2;
                                            return View();
                                        }
                                    }
                                    else
                                    {
                                        if (ncmMono.Contains(NCM))
                                        {
                                            status = true;


                                            for (int k = 0; k < cfops.Count(); k++)
                                            {
                                                if (cfops[k][0].Equals(CFOP))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var cfp = _cfopService.FindByCode(CFOP);
                                                List<string> cfop = new List<string>();
                                                cfop.Add(CFOP);
                                                cfop.Add(cfp.Description);

                                                // Geral
                                                cfop.Add("0");
                                                cfop.Add("0");
                                                cfop.Add("0");
                                                cfop.Add("0");
                                                cfop.Add("0");

                                                cfops.Add(cfop);
                                                pos = cfops.Count() - 1;
                                            }

                                            cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + vProd).ToString();
                                        }
                                        else if (ncmNormal.Contains(NCM))
                                        {
                                            pos = -1;
                                            status = false;
                                            continue;
                                        }
                                        else
                                        {
                                            ViewBag.NCM = notes[i][j]["NCM"];
                                            ViewBag.Erro = 2;
                                            return View();
                                        }
                                    }

                                }

                            }

                            if (notes[i][j].ContainsKey("pPIS") && notes[i][j].ContainsKey("CSTP") && status && CSOSN == "500")
                            {
                                cfops[pos][3] = (Convert.ToDecimal(cfops[pos][3]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                cfops[pos][4] = (Convert.ToDecimal(cfops[pos][4]) + Convert.ToDecimal(notes[i][j]["vPIS"])).ToString();

                            }

                            if (notes[i][j].ContainsKey("pCOFINS") && notes[i][j].ContainsKey("CSTC") && status && CSOSN == "500")
                            {
                                cfops[pos][5] = (Convert.ToDecimal(cfops[pos][5]) + Convert.ToDecimal(notes[i][j]["vCOFINS"])).ToString();
                                cfops[pos][6] = (Convert.ToDecimal(cfops[pos][6]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();

                            }

                        }

                    }

                    ViewBag.Cfop = cfops.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                }
                else if (type.Equals("resumoProdCfopNcmCstMono"))
                {
                    //  Resumo Produto/CFOP/NCM/CST Monofásico

                    List<List<string>> produtos = new List<List<string>>();

                    List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();
                    List<string> codeProdMono = new List<string>();
                    List<string> codeProdNormal = new List<string>();
                    List<string> ncmMono = new List<string>();
                    List<string> ncmNormal = new List<string>();

                    notes = importXml.NFeAll(directoryNfeExit);

                    var prodAll = ncms.Select(_ => _.CodeProduct).ToList();
                    var ncmsAll = ncms.Select(_ => _.Ncm.Code).ToList();

                    decimal vTotal = 0;

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        if (notes[i][1].ContainsKey("dhEmi"))
                        {
                            ncmsTaxation = _service.FindAllInDate(ncms, Convert.ToDateTime(notes[i][1]["dhEmi"]));
                            codeProdMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(2) || _.TaxationTypeNcmId.Equals(3) || _.TaxationTypeNcmId.Equals(4) || _.TaxationTypeNcmId.Equals(5)).Select(_ => _.CodeProduct).ToList();
                            codeProdNormal = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(1) || _.TaxationTypeNcmId.Equals(6)).Select(_ => _.CodeProduct).ToList();
                            ncmMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(2) || _.TaxationTypeNcmId.Equals(3) || _.TaxationTypeNcmId.Equals(4) || _.TaxationTypeNcmId.Equals(5)).Select(_ => _.Ncm.Code).ToList();
                            ncmNormal = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(1) || _.TaxationTypeNcmId.Equals(6)).Select(_ => _.Ncm.Code).ToList();
                        }


                        bool status = false;
                        string cProd = "", xProd = "", cfop = "", NCM = "", cstP = "", CSOSN = "";
                        decimal vProd = 0;
                        int pos = -1;

                        for (int j = 0; j < notes[i].Count(); j++)
                        {

                            if ((notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM")))
                            {
                                pos = -1;
                                NCM = notes[i][j]["NCM"];
                                cfop = notes[i][j]["CFOP"];
                                cProd = notes[i][j]["cProd"];
                                xProd = notes[i][j]["xProd"];

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

                            if (notes[i][j].ContainsKey("CSOSN"))
                            {
                                CSOSN = notes[i][j]["CSOSN"];
                                status = false;

                                if (notes[i][j]["CSOSN"] == "500")
                                {
                                    if (comp.Taxation == "Produto")
                                    {
                                        if (codeProdMono.Contains(cProd) && ncmMono.Contains(NCM))
                                        {
                                            status = true;
                                        }
                                        else if (codeProdNormal.Contains(cProd) && ncmNormal.Contains(NCM))
                                        {
                                            status = false;
                                            continue;
                                        }
                                        else
                                        {
                                            ViewBag.NCM = NCM;
                                            ViewBag.Erro = 2;
                                            return View();
                                        }
                                    }
                                    else
                                    {
                                        if (ncmMono.Contains(NCM))
                                        {
                                            status = true;

                                        }
                                        else if (ncmNormal.Contains(NCM))
                                        {
                                            status = false;
                                            continue;
                                        }
                                        else
                                        {
                                            ViewBag.NCM = notes[i][j]["NCM"];
                                            ViewBag.Erro = 2;
                                            return View();
                                        }
                                    }
                                }
                            }

                            if (notes[i][j].ContainsKey("CST"))
                            {
                                CSOSN = "500";
                                status = false;

                                if (CSOSN == "500")
                                {
                                    if (comp.Taxation == "Produto")
                                    {
                                        if (codeProdMono.Contains(cProd) && ncmMono.Contains(NCM))
                                        {
                                            status = true;
                                        }
                                        else if (codeProdNormal.Contains(cProd) && ncmNormal.Contains(NCM))
                                        {
                                            status = false;
                                            continue;
                                        }
                                        else
                                        {
                                            ViewBag.NCM = NCM;
                                            ViewBag.Erro = 2;
                                            return View();
                                        }
                                    }
                                    else
                                    {
                                        if (ncmMono.Contains(NCM))
                                        {
                                            status = true;

                                        }
                                        else if (ncmNormal.Contains(NCM))
                                        {
                                            status = false;
                                            continue;
                                        }
                                        else
                                        {
                                            ViewBag.NCM = notes[i][j]["NCM"];
                                            ViewBag.Erro = 2;
                                            return View();
                                        }
                                    }
                                }
                            }


                            if (notes[i][j].ContainsKey("CSTP") && CSOSN == "500" && status)
                                cstP = notes[i][j]["CSTP"];

                            if (notes[i][j].ContainsKey("CSTC") && CSOSN == "500" && status)
                            {

                                for (int e = 0; e < produtos.Count(); e++)
                                {
                                    if (produtos[e][0].Equals(cProd) && produtos[e][2].Equals(NCM) && produtos[e][3].Equals(cfop) &&
                                        produtos[e][4].Equals(cstP) && produtos[e][5].Equals(notes[i][j]["CSTC"]))
                                    {
                                        pos = e;
                                        break;
                                    }
                                }

                                if (pos < 0)
                                {

                                    List<string> prod = new List<string>();
                                    prod.Add(cProd);
                                    prod.Add(xProd);
                                    prod.Add(NCM);
                                    prod.Add(cfop);
                                    prod.Add(cstP);
                                    prod.Add(notes[i][j]["CSTC"]);
                                    prod.Add("0");
                                    produtos.Add(prod);
                                    pos = produtos.Count() - 1;
                                }
                                produtos[pos][6] = (Convert.ToDecimal(produtos[pos][6]) + vProd).ToString();
                                vTotal += vProd;

                            }
                        }

                    }

                    ViewBag.Produtos = produtos.OrderBy(_ => Convert.ToInt32(_[2])).ToList();
                    ViewBag.Total = vTotal;
                }
                else if (type.Equals("resumoVendaNcmMono"))
                {
                    //  Resumo Venda NCM Monofásico

                    List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();
                    List<string> codeProdMono = new List<string>();
                    List<string> codeProdNormal = new List<string>();
                    List<string> ncmMono = new List<string>();
                    List<string> ncmNormal = new List<string>();
                    List<List<string>> resumoNcm = new List<List<string>>();

                    var ncmsAll = _ncmService.FindAll(null);

                    cfopsVenda.AddRange(cfopsVendaIM);
                    cfopsVenda.AddRange(cfopsVendaST);
                    cfopsVenda.AddRange(cfopsTransf);
                    cfopsVenda.AddRange(cfopsTransfST);

                    notes = importXml.NFeAll(directoryNfeExit, cfopsVenda);

                    decimal valorProduto = 0, valorPis = 0, valorCofins = 0;

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i][1]["finNFe"] == "4")
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        if (notes[i][1].ContainsKey("dhEmi"))
                        {
                            ncmsTaxation = _service.FindAllInDate(ncms, Convert.ToDateTime(notes[i][1]["dhEmi"]));

                            codeProdMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(2) || _.TaxationTypeNcmId.Equals(3) || _.TaxationTypeNcmId.Equals(4) || _.TaxationTypeNcmId.Equals(5)).Select(_ => _.CodeProduct).ToList();
                            codeProdNormal = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(1) || _.TaxationTypeNcmId.Equals(6)).Select(_ => _.CodeProduct).ToList();
                            ncmMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(2) || _.TaxationTypeNcmId.Equals(3) || _.TaxationTypeNcmId.Equals(4) || _.TaxationTypeNcmId.Equals(5)).Select(_ => _.Ncm.Code).ToList();
                            ncmNormal = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(1) || _.TaxationTypeNcmId.Equals(6)).Select(_ => _.Ncm.Code).ToList();
                        }

                        string cProd = "", NCM = "", CSOSN = "";
                        decimal vProd = 0;
                        bool status = false;
                        int pos = -1;


                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM"))
                            {
                                cProd = notes[i][j]["cProd"];
                                NCM = notes[i][j]["NCM"];

                                if (notes[i][j].ContainsKey("vProd"))
                                {
                                    vProd = 0;
                                    vProd += Convert.ToDecimal(notes[i][j]["vProd"]);
                                }

                                if (notes[i][j].ContainsKey("vFrete"))
                                {
                                    vProd += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                }

                                if (notes[i][j].ContainsKey("vDesc"))
                                {
                                    vProd -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                }

                                if (notes[i][j].ContainsKey("vOutro"))
                                {
                                    vProd += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                }

                                if (notes[i][j].ContainsKey("vSeg"))
                                {
                                    vProd += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                }

                            }

                            if (notes[i][j].ContainsKey("CSOSN"))
                            {
                                CSOSN = notes[i][j]["CSOSN"];

                                pos = -1;
                                status = false;

                                if (notes[i][j]["CSOSN"] == "500")
                                {
                                    if (comp.Taxation == "Produto")
                                    {
                                        if (codeProdMono.Contains(cProd) && ncmMono.Contains(NCM))
                                        {
                                            status = true;

                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add("0");
                                                ncmTemp.Add("0");
                                                ncmTemp.Add(vProd.ToString());
                                                resumoNcm.Add(ncmTemp);
                                                pos = resumoNcm.Count() - 1;
                                            }

                                            if (pos >= 0)
                                            {
                                                resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + vProd).ToString();
                                             
                                            }

                                            valorProduto += vProd;
                                        }
                                        else if (codeProdNormal.Contains(cProd) && ncmNormal.Contains(NCM))
                                        {
                                            pos = -1;
                                            status = false;
                                            continue;
                                        }
                                        else
                                        {
                                            ViewBag.NCM = NCM;
                                            ViewBag.Erro = 2;
                                            return View();
                                        }
                                    }
                                    else
                                    {
                                        if (ncmMono.Contains(NCM))
                                        {
                                            status = true;

                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add("0");
                                                ncmTemp.Add("0");
                                                ncmTemp.Add(vProd.ToString());
                                                resumoNcm.Add(ncmTemp);
                                                pos = resumoNcm.Count() - 1;
                                            }

                                            if (pos >= 0)
                                            {
                                                resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + vProd).ToString();
                                            }

                                            valorProduto += vProd;
                                        }
                                        else if (ncmNormal.Contains(NCM))
                                        {
                                            pos = -1;
                                            status = false;
                                            continue;
                                        }
                                        else
                                        {
                                            ViewBag.NCM = NCM;
                                            ViewBag.Erro = 2;
                                            return View();
                                        }
                                    }

                                }

                            }

                            if (notes[i][j].ContainsKey("CST"))
                            {
                                CSOSN = "500";

                                pos = -1;
                                status = false;

                                if (CSOSN == "500")
                                {
                                    if (comp.Taxation == "Produto")
                                    {
                                        if (codeProdMono.Contains(cProd) && ncmMono.Contains(NCM))
                                        {
                                            status = true;

                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add("0");
                                                ncmTemp.Add("0");
                                                ncmTemp.Add(vProd.ToString());
                                                resumoNcm.Add(ncmTemp);
                                                pos = resumoNcm.Count() - 1;
                                            }

                                            if (pos >= 0)
                                            {
                                                resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + vProd).ToString();

                                            }

                                            valorProduto += vProd;
                                        }
                                        else if (codeProdNormal.Contains(cProd) && ncmNormal.Contains(NCM))
                                        {
                                            pos = -1;
                                            status = false;
                                            continue;
                                        }
                                        else
                                        {
                                            ViewBag.NCM = NCM;
                                            ViewBag.Erro = 2;
                                            return View();
                                        }
                                    }
                                    else
                                    {
                                        if (ncmMono.Contains(NCM))
                                        {
                                            status = true;

                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add("0");
                                                ncmTemp.Add("0");
                                                ncmTemp.Add(vProd.ToString());
                                                resumoNcm.Add(ncmTemp);
                                                pos = resumoNcm.Count() - 1;
                                            }

                                            if (pos >= 0)
                                            {
                                                resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + vProd).ToString();
                                            }

                                            valorProduto += vProd;
                                        }
                                        else if (ncmNormal.Contains(NCM))
                                        {
                                            pos = -1;
                                            status = false;
                                            continue;
                                        }
                                        else
                                        {
                                            ViewBag.NCM = NCM;
                                            ViewBag.Erro = 2;
                                            return View();
                                        }
                                    }

                                }

                            }
                           
                            if (notes[i][j].ContainsKey("pPIS") && notes[i][j].ContainsKey("CSTP") && CSOSN == "500" && status)
                            {
                                resumoNcm[pos][2] = (Convert.ToDecimal(resumoNcm[pos][2]) + Convert.ToDecimal(notes[i][j]["vPIS"])).ToString();
                                valorPis += Convert.ToDecimal(notes[i][j]["vPIS"]);
                            }

                            if (notes[i][j].ContainsKey("pCOFINS") && notes[i][j].ContainsKey("CSTC") && CSOSN == "500" && status)
                            {
                                resumoNcm[pos][3] = (Convert.ToDecimal(resumoNcm[pos][3]) + Convert.ToDecimal(notes[i][j]["vCOFINS"])).ToString();
                                valorCofins += Convert.ToDecimal(notes[i][j]["vCOFINS"]);
                            }
                        }

                    }

                    ViewBag.Ncm = resumoNcm.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                    ViewBag.ValorProduto = valorProduto;
                    ViewBag.ValorPis = valorPis;
                    ViewBag.ValorCofins = valorCofins;
                }
                else if (type.Equals("resumoDevoNcmMono"))
                {
                    //  Resumo Devolução NCM Monofásico

                    if (arquivo == null || arquivo.Length == 0)
                    {
                        ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                        return View(ViewData);
                    }

                    string filedirSped = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Speds");

                    if (!Directory.Exists(filedirSped))
                    {
                        Directory.CreateDirectory(filedirSped);
                    }

                    string nomeArquivoSped = comp.Document + year + month;

                    if (arquivo.FileName.Contains(".txt"))
                        nomeArquivoSped += ".txt";
                    else
                        nomeArquivoSped += ".tmp";

                    string caminho_WebRoot = _appEnvironment.WebRootPath;

                    string caminhoDestinoArquivoSped = caminho_WebRoot + "\\Uploads\\Speds\\";
                    string caminhoDestinoArquivoOriginalSped = caminhoDestinoArquivoSped + nomeArquivoSped;

                    string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivoSped);
                    if (System.IO.File.Exists(caminhoDestinoArquivoOriginalSped))
                    {
                        System.IO.File.Delete(caminhoDestinoArquivoOriginalSped);
                    }

                    var streamSped = new FileStream(caminhoDestinoArquivoOriginalSped, FileMode.Create);
                    await arquivo.CopyToAsync(streamSped);
                    streamSped.Close();
                }
                else if (type.Equals("tributacaoDivergente"))
                {
                    // Tributação de NCM Monofásico Divergente
                    List<List<string>> resumoNcm = new List<List<string>>();
                    List<List<string>> resumoProduto = new List<List<string>>();

                    List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();
                    List<string> codeProdMono = new List<string>();
                    List<string> codeProdNormal = new List<string>();
                    List<string> codeProdST = new List<string>();
                    List<string> codeProdAliqZero = new List<string>();
                    List<string> codeProdIsento = new List<string>();
                    List<string> codeProdOutras = new List<string>();
                    List<string> ncmMono = new List<string>();
                    List<string> ncmNormal = new List<string>();
                    List<string> ncmST = new List<string>();
                    List<string> ncmAliqZero = new List<string>();
                    List<string> ncmIsento = new List<string>();
                    List<string> ncmOutras = new List<string>();


                    var ncmsAll = _ncmService.FindAll(null);
                    var cstAll = _cstService.FindAll(null).Where(_ => _.Type.Equals(true)).ToList();

                    var cstNormal = cstAll.Where(_ => _.TaxationTypeNcmId.Equals(1)).Select(_ => _.Code).ToList();
                    var cstMono = cstAll.Where(_ => _.TaxationTypeNcmId.Equals(2)).Select(_ => _.Code).ToList();
                    var cstAliqZero = cstAll.Where(_ =>  _.TaxationTypeNcmId.Equals(3)).Select(_ => _.Code).ToList();
                    var cstST = cstAll.Where(_ => _.TaxationTypeNcmId.Equals(4)).Select(_ => _.Code).ToList();
                    var cstIsento = cstAll.Where(_ => _.TaxationTypeNcmId.Equals(5)).Select(_ => _.Code).ToList();
                    var cstOutras = cstAll.Where(_ => _.TaxationTypeNcmId.Equals(6)).Select(_ => _.Code).ToList();

                    notes = importXml.NFeAll(directoryNfeExit);

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i][1]["finNFe"] == "4")
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        if (notes[i][1].ContainsKey("dhEmi"))
                        {
                            ncmsTaxation = _service.FindAllInDate(ncms, Convert.ToDateTime(notes[i][1]["dhEmi"]));

                            codeProdMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(2)).Select(_ => _.CodeProduct).ToList();
                            codeProdNormal = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(1)).Select(_ => _.CodeProduct).ToList();
                            codeProdST = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(4)).Select(_ => _.CodeProduct).ToList();
                            codeProdAliqZero = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(3)).Select(_ => _.CodeProduct).ToList();
                            codeProdIsento = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(5)).Select(_ => _.CodeProduct).ToList();
                            codeProdOutras = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(6)).Select(_ => _.CodeProduct).ToList();


                            ncmMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(2)).Select(_ => _.Ncm.Code).ToList();
                            ncmNormal = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(1)).Select(_ => _.Ncm.Code).ToList();
                            ncmST = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(4)).Select(_ => _.Ncm.Code).ToList();
                            ncmAliqZero = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(3)).Select(_ => _.Ncm.Code).ToList();
                            ncmIsento = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(5)).Select(_ => _.Ncm.Code).ToList();
                            ncmOutras = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(6)).Select(_ => _.Ncm.Code).ToList();
                        }

                        string cProd = "", xProd =  "", NCM = "";

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM"))
                            {
                                cProd = notes[i][j]["cProd"];
                                NCM = notes[i][j]["NCM"];
                                xProd = notes[i][j]["xProd"];

                            }

                            if (notes[i][j].ContainsKey("CSTP"))
                            {
                                if (comp.Taxation == "Produto")
                                {
                                    if (codeProdMono.Contains(cProd) && ncmMono.Contains(NCM))
                                    {
                                        if (!cstMono.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("MONOFÁSICO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdNormal.Contains(cProd) && ncmNormal.Contains(NCM))
                                    {
                                        if (!cstNormal.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("NORMAL");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdAliqZero.Contains(cProd) && ncmAliqZero.Contains(NCM))
                                    {
                                        if (!cstAliqZero.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ALIQ. ZERO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdST.Contains(cProd) && ncmST.Contains(NCM))
                                    {
                                        if (!cstST.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("S. TRIBUTÁRIA");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdIsento.Contains(cProd) && ncmIsento.Contains(NCM))
                                    {
                                        if (!cstIsento.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ISENTO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdOutras.Contains(cProd) && ncmOutras.Contains(NCM))
                                    {
                                        if (!cstOutras.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("OUTRAS");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.NCM = "Producto: " + cProd + " NCM: " + NCM;
                                        ViewBag.Erro = 2;
                                        return View();
                                    }
                                }
                                else
                                {
                                    if (ncmMono.Contains(NCM))
                                    {
                                        if (!cstMono.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("MONOFÁSICO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmNormal.Contains(NCM))
                                    {
                                        if (!cstNormal.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var y = notes[i][j]["CSTP"];
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("NORMAL");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmAliqZero.Contains(NCM))
                                    {
                                        if (!cstAliqZero.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ALIQ. ZERO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmST.Contains(NCM))
                                    {
                                        if (!cstST.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("S. TRIBUTÁRIA");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmIsento.Contains(NCM))
                                    {
                                        if (!cstIsento.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ISENTO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmOutras.Contains(NCM))
                                    {
                                        if (!cstOutras.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("OUTRAS");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.NCM = "NCM: " + NCM;
                                        ViewBag.Erro = 2;
                                        return View();
                                    }
                                }
                               
                            }

                            if (notes[i][j].ContainsKey("CSTC"))
                            {
                                if (comp.Taxation == "Produto")
                                {
                                    if (codeProdMono.Contains(cProd) && ncmMono.Contains(NCM))
                                    {
                                        if (!cstMono.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("MONOFÁSICO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdNormal.Contains(cProd) && ncmNormal.Contains(NCM))
                                    {
                                        if (!cstNormal.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("NORMAL");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdAliqZero.Contains(cProd) && ncmAliqZero.Contains(NCM))
                                    {
                                        if (!cstAliqZero.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ALIQ. ZERO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdST.Contains(cProd) && ncmST.Contains(NCM))
                                    {
                                        if (!cstST.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("S. TRIBUTÁRIA");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdIsento.Contains(cProd) && ncmIsento.Contains(NCM))
                                    {
                                        if (!cstIsento.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ISENTO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdOutras.Contains(cProd) && ncmOutras.Contains(NCM))
                                    {
                                        if (!cstOutras.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("OUTRAS");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.NCM = "Producto: " + cProd + " NCM: " + NCM;
                                        ViewBag.Erro = 2;
                                        return View();
                                    }
                                }
                                else
                                {
                                    if (ncmMono.Contains(NCM))
                                    {
                                        if (!cstMono.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("MONOFÁSICO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmNormal.Contains(NCM))
                                    {
                                        if (!cstNormal.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var y = notes[i][j]["CSTP"];
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("NORMAL");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmAliqZero.Contains(NCM))
                                    {
                                        if (!cstAliqZero.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ALIQ. ZERO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmST.Contains(NCM))
                                    {
                                        if (!cstST.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("S. TRIBUTÁRIA");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmIsento.Contains(NCM))
                                    {
                                        if (!cstIsento.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ISENTO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmOutras.Contains(NCM))
                                    {
                                        if (!cstOutras.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("OUTRAS");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.NCM = "NCM: " + NCM;
                                        ViewBag.Erro = 2;
                                        return View();
                                    }
                                }
                            }
                        }
                    }

                    ViewBag.Ncms = resumoNcm.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                    ViewBag.Produtos = resumoProduto.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                }
                else if (type.Equals("tributacaoDivergenteNota"))
                {
                    // Tributação de NCM Monofásico Divergente/Nota
                    List<List<string>> resumoNcm = new List<List<string>>();
                    List<List<string>> resumoProduto = new List<List<string>>();

                    List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();
                    List<string> codeProdMono = new List<string>();
                    List<string> codeProdNormal = new List<string>();
                    List<string> codeProdST = new List<string>();
                    List<string> codeProdAliqZero = new List<string>();
                    List<string> codeProdIsento = new List<string>();
                    List<string> codeProdOutras = new List<string>();
                    List<string> ncmMono = new List<string>();
                    List<string> ncmNormal = new List<string>();
                    List<string> ncmST = new List<string>();
                    List<string> ncmAliqZero = new List<string>();
                    List<string> ncmIsento = new List<string>();
                    List<string> ncmOutras = new List<string>();


                    var ncmsAll = _ncmService.FindAll(null);
                    var cstAll = _cstService.FindAll(null).Where(_ => _.Type.Equals(true)).ToList();

                    var cstNormal = cstAll.Where(_ => _.TaxationTypeNcmId.Equals(1)).Select(_ => _.Code).ToList();
                    var cstMono = cstAll.Where(_ => _.TaxationTypeNcmId.Equals(2)).Select(_ => _.Code).ToList();
                    var cstAliqZero = cstAll.Where(_ => _.TaxationTypeNcmId.Equals(3)).Select(_ => _.Code).ToList();
                    var cstST = cstAll.Where(_ => _.TaxationTypeNcmId.Equals(4)).Select(_ => _.Code).ToList();
                    var cstIsento = cstAll.Where(_ => _.TaxationTypeNcmId.Equals(5)).Select(_ => _.Code).ToList();
                    var cstOutras = cstAll.Where(_ => _.TaxationTypeNcmId.Equals(6)).Select(_ => _.Code).ToList();

                    notes = importXml.NFeAll(directoryNfeExit);

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i][1]["finNFe"] == "4")
                        {
                            notes.RemoveAt(i);
                            continue;
                        }

                        if (notes[i][1].ContainsKey("dhEmi"))
                        {
                            ncmsTaxation = _service.FindAllInDate(ncms, Convert.ToDateTime(notes[i][1]["dhEmi"]));

                            codeProdMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(2)).Select(_ => _.CodeProduct).ToList();
                            codeProdNormal = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(1)).Select(_ => _.CodeProduct).ToList();
                            codeProdST = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(4)).Select(_ => _.CodeProduct).ToList();
                            codeProdAliqZero = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(3)).Select(_ => _.CodeProduct).ToList();
                            codeProdIsento = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(5)).Select(_ => _.CodeProduct).ToList();
                            codeProdOutras = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(6)).Select(_ => _.CodeProduct).ToList();


                            ncmMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(2)).Select(_ => _.Ncm.Code).ToList();
                            ncmNormal = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(1)).Select(_ => _.Ncm.Code).ToList();
                            ncmST = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(4)).Select(_ => _.Ncm.Code).ToList();
                            ncmAliqZero = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(3)).Select(_ => _.Ncm.Code).ToList();
                            ncmIsento = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(5)).Select(_ => _.Ncm.Code).ToList();
                            ncmOutras = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals(6)).Select(_ => _.Ncm.Code).ToList();
                        }

                        string cProd = "", xProd = "", NCM = "";

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM"))
                            {
                                cProd = notes[i][j]["cProd"];
                                NCM = notes[i][j]["NCM"];
                                xProd = notes[i][j]["xProd"];

                            }

                            if (notes[i][j].ContainsKey("CSTP"))
                            {
                                if (comp.Taxation == "Produto")
                                {
                                    if (codeProdMono.Contains(cProd) && ncmMono.Contains(NCM))
                                    {
                                        if (!cstMono.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("MONOFÁSICO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdNormal.Contains(cProd) && ncmNormal.Contains(NCM))
                                    {
                                        if (!cstNormal.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("NORMAL");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdAliqZero.Contains(cProd) && ncmAliqZero.Contains(NCM))
                                    {
                                        if (!cstAliqZero.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ALIQ. ZERO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdST.Contains(cProd) && ncmST.Contains(NCM))
                                    {
                                        if (!cstST.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("S. TRIBUTÁRIA");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdIsento.Contains(cProd) && ncmIsento.Contains(NCM))
                                    {
                                        if (!cstIsento.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ISENTO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdOutras.Contains(cProd) && ncmOutras.Contains(NCM))
                                    {
                                        if (!cstOutras.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("OUTRAS");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.NCM = NCM;
                                        ViewBag.Erro = 2;
                                        return View();
                                    }
                                }
                                else
                                {
                                    if (ncmMono.Contains(NCM))
                                    {
                                        if (!cstMono.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("MONOFÁSICO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmNormal.Contains(NCM))
                                    {
                                        if (!cstNormal.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var y = notes[i][j]["CSTP"];
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("NORMAL");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmAliqZero.Contains(NCM))
                                    {
                                        if (!cstAliqZero.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ALIQ. ZERO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmST.Contains(NCM))
                                    {
                                        if (!cstST.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("S. TRIBUTÁRIA");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmIsento.Contains(NCM))
                                    {
                                        if (!cstIsento.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ISENTO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmOutras.Contains(NCM))
                                    {
                                        if (!cstOutras.Contains(notes[i][j]["CSTP"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTP"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("OUTRAS");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.NCM = NCM;
                                        ViewBag.Erro = 2;
                                        return View();
                                    }
                                }
                            }

                            if (notes[i][j].ContainsKey("CSTC"))
                            {
                                if (comp.Taxation == "Produto")
                                {
                                    if (codeProdMono.Contains(cProd) && ncmMono.Contains(NCM))
                                    {
                                        if (!cstMono.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("MONOFÁSICO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdNormal.Contains(cProd) && ncmNormal.Contains(NCM))
                                    {
                                        if (!cstNormal.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("NORMAL");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdAliqZero.Contains(cProd) && ncmAliqZero.Contains(NCM))
                                    {
                                        if (!cstAliqZero.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ALIQ. ZERO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdST.Contains(cProd) && ncmST.Contains(NCM))
                                    {
                                        if (!cstST.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("S. TRIBUTÁRIA");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdIsento.Contains(cProd) && ncmIsento.Contains(NCM))
                                    {
                                        if (!cstIsento.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ISENTO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (codeProdOutras.Contains(cProd) && ncmOutras.Contains(NCM))
                                    {
                                        if (!cstOutras.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("OUTRAS");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.NCM = NCM;
                                        ViewBag.Erro = 2;
                                        return View();
                                    }
                                }
                                else
                                {
                                    if (ncmMono.Contains(NCM))
                                    {
                                        if (!cstMono.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("MONOFÁSICO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmNormal.Contains(NCM))
                                    {
                                        if (!cstNormal.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var y = notes[i][j]["CSTP"];
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("NORMAL");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmAliqZero.Contains(NCM))
                                    {
                                        if (!cstAliqZero.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ALIQ. ZERO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmST.Contains(NCM))
                                    {
                                        if (!cstST.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("S. TRIBUTÁRIA");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmIsento.Contains(NCM))
                                    {
                                        if (!cstIsento.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("ISENTO");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else if (ncmOutras.Contains(NCM))
                                    {
                                        if (!cstOutras.Contains(notes[i][j]["CSTC"]))
                                        {
                                            int pos = -1;
                                            for (int k = 0; k < resumoNcm.Count(); k++)
                                            {
                                                if (resumoNcm[k][0].Equals(NCM))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                var cstTemp = cstAll.Where(_ => _.Code.Equals(notes[i][j]["CSTC"])).FirstOrDefault();
                                                List<string> ncmTemp = new List<string>();
                                                ncmTemp.Add(nn.Code);
                                                ncmTemp.Add(nn.Description);
                                                ncmTemp.Add(cstTemp.Code);
                                                ncmTemp.Add(cstTemp.TaxationTypeNcm.Description);
                                                ncmTemp.Add("OUTRAS");
                                                resumoNcm.Add(ncmTemp);
                                            }

                                            int posProd = -1;

                                            for (int k = 0; k < resumoProduto.Count(); k++)
                                            {
                                                if (resumoProduto[k][0].Equals(NCM) && resumoProduto[k][1].Equals(cProd) && resumoProduto[k][3].Equals(notes[i][1]["nNF"]))
                                                {
                                                    posProd = k;
                                                    break;
                                                }
                                            }

                                            if (posProd < 0)
                                            {
                                                var nn = ncmsAll.Where(_ => _.Code.Equals(NCM)).FirstOrDefault();
                                                List<string> prodTemp = new List<string>();
                                                prodTemp.Add(nn.Code);
                                                prodTemp.Add(cProd);
                                                prodTemp.Add(xProd);
                                                prodTemp.Add(notes[i][1]["nNF"]);
                                                resumoProduto.Add(prodTemp);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.NCM = NCM;
                                        ViewBag.Erro = 2;
                                        return View();
                                    }
                                }
                            }
                        }
                    }

                    ViewBag.Ncms = resumoNcm.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                    ViewBag.Produtos = resumoProduto.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                }
                else if (type.Equals("relatorioSimples"))
                {
                    var imp = _taxService.FindByMonth(companyId, month, year, "PisCofins");

                    if (imp == null)
                    {
                        ViewBag.Erro = 3;
                        return View();
                    }

                    ViewBag.Imposto = imp;

                    decimal vendasNomalNormal = Convert.ToDecimal(imp.VendaNormalNormal), vendasNormalMono = Convert.ToDecimal(imp.VendaNormalMonofasico), vendasNormalST = Convert.ToDecimal(imp.VendaNormalST),
                            vendasNormalAliqZero = Convert.ToDecimal(imp.VendaNormalAliqZero), vendasNormalIsento = Convert.ToDecimal(imp.VendaNormalIsento), vendasSTNormal = Convert.ToDecimal(imp.VendaSTNormal),
                            vendasSTMono = Convert.ToDecimal(imp.VendaSTMonofasico), vendasSTST = Convert.ToDecimal(imp.VendaSTST), vendasSTAliqZero = Convert.ToDecimal(imp.VendaSTAliqZero),
                            vendasSTIsento = Convert.ToDecimal(imp.VendaSTIsento), vendasNormalOutras = Convert.ToDecimal(imp.VendaNormalOutras), vendasSTOutras = Convert.ToDecimal(imp.VendaSTOutras),
                            receitaServico = Convert.ToDecimal(imp.Receita4);
                           
                    decimal devoNormalNormal = Convert.ToDecimal(imp.DevoNormalNormal) + Convert.ToDecimal(imp.DevoNormalNormalP), devoNormalMono = Convert.ToDecimal(imp.DevoNormalMonofasico) + Convert.ToDecimal(imp.DevoNormalMonofasicoP),
                            devoNormalST = Convert.ToDecimal(imp.DevoNormalST) + Convert.ToDecimal(imp.DevoNormalSTP), devoNormalAliqZero = Convert.ToDecimal(imp.DevoNormalAliqZero) + Convert.ToDecimal(imp.DevoNormalAliqZeroP),
                            devoNormalIsento = Convert.ToDecimal(imp.DevoNormalIsento) + Convert.ToDecimal(imp.DevoNormalIsentoP),  devoSTMono = Convert.ToDecimal(imp.DevoSTMonofasico) + Convert.ToDecimal(imp.DevoSTMonofasicoP),
                            devoSTNormal = Convert.ToDecimal(imp.DevoSTNormal) + Convert.ToDecimal(imp.DevoSTNormalP), devoSTST = Convert.ToDecimal(imp.DevoSTST) + Convert.ToDecimal(imp.DevoSTSTP), 
                            devoSTAliqZero = Convert.ToDecimal(imp.DevoSTAliqZero) + Convert.ToDecimal(imp.DevoSTAliqZeroP), devoSTIsento = Convert.ToDecimal(imp.DevoSTIsento) + Convert.ToDecimal(imp.DevoSTIsentoP), 
                            devoNormalOutras = Convert.ToDecimal(imp.DevoNormalOutras) + Convert.ToDecimal(imp.DevoNormalOutrasP), devoSTOutras = Convert.ToDecimal(imp.DevoSTOutras) + Convert.ToDecimal(imp.DevoSTOutrasP),
                            devoServico = Convert.ToDecimal(imp.Devolucao4);


                    //  Base
                    decimal baseNormalNormal = vendasNomalNormal - devoNormalNormal,
                            baseNormalMono = vendasNormalMono - devoNormalMono,
                            baseNormalST = vendasNormalST - devoNormalST,
                            baseNormalAliqZero = vendasNormalAliqZero - devoNormalAliqZero,
                            baseNormalIsento = vendasNormalIsento - devoNormalIsento,
                            baseNormalOutras = vendasNormalOutras - devoNormalOutras,
                            baseSTNormal = vendasSTNormal - devoSTNormal,
                            baseSTMono = vendasSTMono - devoSTMono,
                            baseSTST = vendasSTST - devoSTST,
                            baseSTAliqZero = vendasSTAliqZero - devoSTAliqZero,
                            baseSTIsento = vendasSTIsento - devoSTIsento,
                            baseSTOutras = vendasSTOutras - devoSTOutras,
                            baseServico = receitaServico - devoServico;


                    //  Totais
                    decimal totalVendas = vendasNomalNormal + vendasNormalMono + vendasNormalST + vendasNormalAliqZero + vendasNormalIsento + vendasSTNormal + vendasSTMono + vendasSTST +
                        vendasSTAliqZero + vendasSTIsento + vendasNormalOutras + vendasSTOutras + receitaServico;

                    decimal totalDevo = devoNormalNormal + devoNormalMono + devoNormalST + devoNormalAliqZero + devoNormalIsento + devoSTNormal + devoSTMono + devoSTST + devoSTAliqZero + 
                        devoSTIsento + devoNormalOutras + devoSTOutras + devoServico;

                    decimal totalBase = totalVendas - totalDevo;

                    //  Vendas
                    ViewBag.VendasNomalNormal = vendasNomalNormal;
                    ViewBag.VendasNormalMono = vendasNormalMono;
                    ViewBag.VendasNormalST = vendasNormalST;
                    ViewBag.VendasNormalAliqZero = vendasNormalAliqZero;
                    ViewBag.VendasNormalIsento = vendasNormalIsento;
                    ViewBag.VendasNormalOutras = vendasNormalOutras;
                    ViewBag.VendasSTNormal = vendasSTNormal;
                    ViewBag.VendasSTMono = vendasSTMono;
                    ViewBag.VendasSTST = vendasSTST;
                    ViewBag.VendasSTAliqZero = vendasSTAliqZero;
                    ViewBag.VendasSTIsento = vendasSTIsento;
                    ViewBag.VendasSTOutras = vendasSTOutras;
                    ViewBag.ReceitaServico = receitaServico;

                    //  Devoluções
                    ViewBag.DevoNormalNormal = devoNormalNormal;
                    ViewBag.DevoNormalMono = devoNormalMono;
                    ViewBag.DevoNormalST = devoNormalST;
                    ViewBag.DevoNormalAliqZero = devoNormalAliqZero;
                    ViewBag.DevoNormalIsento = devoNormalIsento;
                    ViewBag.DevoNormalOutras = devoNormalOutras;
                    ViewBag.DevoSTNormal = devoSTNormal;
                    ViewBag.DevoSTMono = devoSTMono;
                    ViewBag.DevoSTST = devoSTST;
                    ViewBag.DevoSTAliqZero = devoSTAliqZero;
                    ViewBag.DevoSTIsento = devoSTIsento;
                    ViewBag.DevoSTOutras = devoSTOutras;
                    ViewBag.DevoServico = devoServico;

                    //  Bases
                    ViewBag.BaseNormalNormal = baseNormalNormal;
                    ViewBag.BaseNormalMono = baseNormalMono;
                    ViewBag.BaseNormalST = baseNormalST;
                    ViewBag.BaseNormalAliqZero = baseNormalAliqZero;
                    ViewBag.BaseNormalIsento = baseNormalIsento;
                    ViewBag.BaseNormalOutras = baseNormalOutras;
                    ViewBag.BaseSTNormal = baseSTNormal;
                    ViewBag.BaseSTMono = baseSTMono;
                    ViewBag.BaseSTST = baseSTST;
                    ViewBag.BaseSTAliqZero = baseSTAliqZero;
                    ViewBag.BaseSTIsento = baseSTIsento;
                    ViewBag.BaseSTOutras = baseSTOutras;
                    ViewBag.BaseServico = baseServico;

                    //  Totais
                    ViewBag.TotalVendas = totalVendas;
                    ViewBag.TotalDevo = totalDevo;
                    ViewBag.TotalBase = totalBase;
                }
                else if (type.Equals("imposto"))
                {
                    //  Apuração Imposto Federal

                    if (comp.CountingType.Name.Equals("Lucro Real"))
                    {
                        //  Empresa Lucro Real

                        var imp = _taxService.FindByMonth(companyId, month, year, "PisCofins");

                        if (imp == null)
                        {
                            ViewBag.Erro = 3;
                            return View();
                        }

                        ViewBag.Imposto = imp;


                        var mesAtual = importMes.NumberMonth(month);
                        var mesAnterior = importMes.NameMonthPrevious(mesAtual);
                        decimal saldoCredorCofins = 0, saldoCredorPis = 0;

                        string ano = year;

                        if (mesAtual.Equals(1))
                            ano = (Convert.ToInt32(year) - 1).ToString();

                        var creditLast = _taxService.FindByMonth(companyId, mesAnterior, ano, "PisCofins");

                        if (creditLast != null)
                        {
                            saldoCredorCofins = Convert.ToDecimal(creditLast.SaldoCredorCofins);
                            saldoCredorPis = Convert.ToDecimal(creditLast.SaldoCredorPis);
                        }

                        //  Entrada
                        decimal totalCompra = Convert.ToDecimal(imp.Compra) - Convert.ToDecimal(imp.DevolucaoCompra) - Convert.ToDecimal(imp.CompraTI) +
                                              Convert.ToDecimal(imp.Energia) + Convert.ToDecimal(imp.AluguelPredio) + Convert.ToDecimal(imp.AluguelME) +
                                              Convert.ToDecimal(imp.DespesasF) + Convert.ToDecimal(imp.DespesasME) + Convert.ToDecimal(imp.DespesasA),
                                cofinsEntrada = Math.Round(totalCompra * Convert.ToDecimal(comp.PercentualCofins) / 100, 2),
                                cofinsCredito = cofinsEntrada + Convert.ToDecimal(imp.CofinsRetido) + saldoCredorCofins,
                                pisEntrada = Math.Round(totalCompra * Convert.ToDecimal(comp.PercentualPis) / 100, 2),
                                pisCredito = pisEntrada + Convert.ToDecimal(imp.PisRetido) + saldoCredorPis;

                        ViewBag.TotalCompra = totalCompra;
                        ViewBag.CofinsEntrada = cofinsEntrada;
                        ViewBag.CofinsCredito = cofinsCredito;
                        ViewBag.PisEntrada = pisEntrada;
                        ViewBag.PisCredito = pisCredito;

                        //  Saida
                        decimal totalVendaLiquida = Convert.ToDecimal(imp.Venda) - Convert.ToDecimal(imp.DevolucaoVenda) - Convert.ToDecimal(imp.DevolucaoVendaP) - Convert.ToDecimal(imp.VendaTI),
                               totalVenda = totalVendaLiquida + Convert.ToDecimal(imp.PrestacaoServico) + Convert.ToDecimal(imp.ReceitaFinanceira) +
                                            Convert.ToDecimal(imp.Capital) + Convert.ToDecimal(imp.ReceitaAluguel) + Convert.ToDecimal(imp.Juros),
                               cofinsSaidaRF = Math.Round(Convert.ToDecimal(imp.ReceitaFinanceira) * Convert.ToDecimal(comp.PercentualCofinsRF) / 100, 2),
                               cofinsDebito = Math.Round((totalVenda * Convert.ToDecimal(comp.PercentualCofins) / 100), 2),
                               pisSaidaRF = Math.Round(Convert.ToDecimal(imp.ReceitaFinanceira) * Convert.ToDecimal(comp.PercentualPisRF) / 100, 2),
                               pisDebito = Math.Round((totalVenda * Convert.ToDecimal(comp.PercentualPis) / 100), 2),
                               perda = Convert.ToDecimal(imp.Perda), cofinsPerda = perda * Convert.ToDecimal(comp.PercentualCofins) / 100,
                               pisPerda = perda * Convert.ToDecimal(comp.PercentualPis) / 100;


                        ViewBag.TotalVendaLiquida = totalVendaLiquida;
                        ViewBag.TotalVenda = totalVenda;
                        ViewBag.CofinsSaidaRF = cofinsSaidaRF;
                        ViewBag.CofinsDebito = cofinsDebito;
                        ViewBag.CofinsPerda = cofinsPerda;
                        ViewBag.PisSaidaRF = pisSaidaRF;
                        ViewBag.PisDebito = pisDebito;
                        ViewBag.PisPerda = pisPerda;
                        ViewBag.BasePerda = perda;


                        //  Apuração PIS/COFINS
                        decimal cofinsTotalCredito = cofinsCredito - cofinsPerda, cofinsTotalDebito = cofinsDebito + cofinsSaidaRF,
                                pisTotalCredito = pisCredito - pisPerda, pisTotaDebito = pisDebito + pisSaidaRF,
                                cofinsRecolher = 0, pisRecolher = 0;

                        if (cofinsTotalDebito - cofinsTotalCredito < 0)
                            imp.SaldoCredorCofins = cofinsTotalCredito - cofinsTotalDebito;
                        else
                            cofinsRecolher = cofinsTotalDebito - cofinsTotalCredito;

                        if (pisTotaDebito - pisTotalCredito < 0)
                            imp.SaldoCredorPis = pisTotalCredito - pisTotaDebito;
                        else
                            pisRecolher = pisTotaDebito - pisTotalCredito;


                        ViewBag.CofinsTotalCredito = cofinsTotalCredito;
                        ViewBag.CofinsTotalDebito = cofinsTotalDebito;
                        ViewBag.CofinsRecolher = cofinsRecolher;
                        ViewBag.PisTotalCredito = pisTotalCredito;
                        ViewBag.PisTotalDebito = pisTotaDebito;
                        ViewBag.PisRecolher = pisRecolher;

                        imp.Updated = DateTime.Now;

                    }
                    else if (comp.CountingType.Name.Equals("Lucro Presumido"))
                    {
                        //  Empresa Lucro Presumido
                        var basePisCofins = _baseService.FindByName("Irpj");

                        if (trimestre == "Nenhum")
                        {
                            var imp = _taxService.FindByMonth(companyId, month, year, "PisCofins");

                            if (imp == null)
                            {
                                ViewBag.Erro = 3;
                                return View();
                            }

                            decimal receitaPetroleo = Convert.ToDecimal(imp.Receita1), receitaComercio = Convert.ToDecimal(imp.Receita2), receitaTransporte = Convert.ToDecimal(imp.Receita3),
                                    receitaServico = Convert.ToDecimal(imp.Receita4), receitaMono = Convert.ToDecimal(imp.ReceitaMono),
                                    devolucaoPetroleo = Convert.ToDecimal(imp.Devolucao1), devolucaoComercio = Convert.ToDecimal(imp.Devolucao2),
                                    devolucaoTransporte = Convert.ToDecimal(imp.Devolucao3), devolucaoServico = Convert.ToDecimal(imp.Devolucao4),
                                    devolucaoNormal = Convert.ToDecimal(imp.DevolucaoNormal), reducaoIcms = Convert.ToDecimal(imp.ReducaoIcms);

                            devolucaoPetroleo += Convert.ToDecimal(imp.Devolucao1P);
                            devolucaoComercio += Convert.ToDecimal(imp.Devolucao2P);
                            devolucaoTransporte += Convert.ToDecimal(imp.Devolucao3P);
                            devolucaoServico += Convert.ToDecimal(imp.Devolucao4P);
                            devolucaoNormal += Convert.ToDecimal(imp.DevolucaoNormalP);


                            decimal baseCalcAntesMono = receitaComercio + receitaServico + receitaPetroleo + receitaTransporte,
                                    baseCalcPisCofins = baseCalcAntesMono - receitaMono - devolucaoNormal - reducaoIcms;

                            //PIS
                            decimal pisApurado = (baseCalcPisCofins * Convert.ToDecimal(comp.PercentualPis)) / 100,
                                    pisRetido = Convert.ToDecimal(imp.PisRetido),
                                    pisAPagar = pisApurado - pisRetido;

                            //COFINS
                            decimal cofinsApurado = (baseCalcPisCofins * Convert.ToDecimal(comp.PercentualCofins)) / 100,
                                    cofinsRetido = Convert.ToDecimal(imp.CofinsRetido),
                                    cofinsAPagar = cofinsApurado - cofinsRetido;

                            // CSLL E IRPJ
                            decimal baseCalcCsllIrpjPetroleo = receitaPetroleo - devolucaoPetroleo,
                                    baseCalcCsllIrpjComercio = receitaComercio - devolucaoComercio,
                                    baseCalcCsllIrpjTransporte = receitaTransporte - devolucaoTransporte,
                                    baseCalcCsllIrpjServico = receitaServico - devolucaoServico;

                            //CSLL
                            decimal percentualCsll1 = Convert.ToDecimal(comp.CSLL1),
                                    percentualCsll2 = Convert.ToDecimal(comp.CSLL2),
                                    csll1 = (baseCalcCsllIrpjPetroleo * percentualCsll1 / 100),
                                    csll2 = (baseCalcCsllIrpjComercio * percentualCsll1 / 100),
                                    csll3 = (baseCalcCsllIrpjTransporte * percentualCsll1 / 100),
                                    csll4 = (baseCalcCsllIrpjServico * percentualCsll2 / 100),
                                    csllApurado = (csll1 + csll2 + csll3 + csll4) * Convert.ToDecimal(comp.PercentualCSLL) / 100,
                                    csllRetido = Convert.ToDecimal(imp.CsllRetido),
                                    csllAPagar = csllApurado - csllRetido;

                            //IRPJ
                            decimal percentualIrpj1 = Convert.ToDecimal(comp.IRPJ1),
                                    percentualIrpj2 = Convert.ToDecimal(comp.IRPJ2),
                                    percentualIrpj3 = Convert.ToDecimal(comp.IRPJ3),
                                    percentualIrpj4 = Convert.ToDecimal(comp.IRPJ4),
                                    irp1 = (baseCalcCsllIrpjPetroleo * percentualIrpj1 / 100),
                                    irp2 = (baseCalcCsllIrpjComercio * percentualIrpj2 / 100),
                                    irp3 = (baseCalcCsllIrpjTransporte * percentualIrpj3 / 100),
                                    irp4 = (baseCalcCsllIrpjServico * percentualIrpj4 / 100),
                                    irpjApurado = ((irp1 + irp2 + irp3 + irp4) * Convert.ToDecimal(comp.PercentualIRPJ)) / 100,
                                    irpjRetido = Convert.ToDecimal(imp.IrpjRetido),
                                    irpjAPagar = irpjApurado - irpjRetido;

                            //CPRB
                            decimal cprbAPagar = (baseCalcAntesMono * Convert.ToDecimal(comp.CPRB)) / 100;

                            //Comércio
                            ViewBag.FaturamentoComercio = receitaComercio;
                            ViewBag.DevolucaoComercio = devolucaoComercio;

                            //Serviço
                            ViewBag.FaturamentoServico = receitaServico;
                            ViewBag.DevolucaoServico = devolucaoServico;

                            //Petróleo
                            ViewBag.FaturamentoPetroleo = receitaPetroleo;
                            ViewBag.DevolucaoPetroleo = devolucaoPetroleo;

                            //Transporte
                            ViewBag.FaturamentoTransporte = receitaTransporte;
                            ViewBag.DevolucaoTransporte = devolucaoTransporte;

                            //Dados PIS e COFINS
                            ViewBag.DevolucaoNormal = devolucaoNormal;
                            ViewBag.BaseCalcAntesMono = baseCalcAntesMono;
                            ViewBag.BaseCalcMono = receitaMono;
                            ViewBag.DevolucaoMono = devolucaoNormal;
                            ViewBag.BaseCalcPisCofins = baseCalcPisCofins;
                            ViewBag.ReducaoIcms = reducaoIcms;

                            //PIS
                            ViewBag.PisApurado = pisApurado;
                            ViewBag.PisRetido = pisRetido;
                            ViewBag.PisAPagar = pisAPagar;

                            //COFINS
                            ViewBag.CofinsApurado = cofinsApurado;
                            ViewBag.CofinsRetido = cofinsRetido;
                            ViewBag.CofinsAPagar = cofinsAPagar;

                            // CSLL E IRPJ
                            ViewBag.BaseCalcCsllIrpjPetroleo = baseCalcCsllIrpjPetroleo;
                            ViewBag.BaseCalcCsllIrpjComercio = baseCalcCsllIrpjComercio;
                            ViewBag.BaseCalcCsllIrpjTransporte = baseCalcCsllIrpjTransporte;
                            ViewBag.BaseCalcCsllIrpjServico = baseCalcCsllIrpjServico;

                            //CSLL
                            ViewBag.CsllApurado = csllApurado;
                            ViewBag.CsllRetido = csllRetido;
                            ViewBag.CsllAPagar = csllAPagar;

                            //IRPJ
                            ViewBag.IrpjApurado = irpjApurado;
                            ViewBag.IrpjRetido = irpjRetido;
                            ViewBag.IrpjAPagar = irpjAPagar;

                            //CPRB
                            ViewBag.CprbAPagar = cprbAPagar;

                        }
                        else
                        {
                            var mesesTrimestre = importTrimestre.Months(trimestre);

                            List<List<string>> impostosTrimestre = new List<List<string>>();

                            decimal irpj1Total = 0, irpj2Total = 0, irpj3Total = 0, irpj4Total = 0, irpjFonteServico = 0, irpjFonteAF = 0, irpjPagoTotal = 0,
                              csll1Total = 0, csll2Total = 0, csll3Total = 0, csll4Total = 0, csllFonte = 0, csllPagoTotal = 0,
                              capitalIM = 0, bonificacao = 0, receitaAF = 0;

                            decimal receitaPetroleo = 0, receitaComercio = 0, receitaTransporte = 0, receitaServico = 0,
                                devolucaoPetroleo = 0, devolucaoComercio = 0, devolucaoTransporte = 0, devolucaoServico = 0,
                                receitas = 0, receitasMono = 0, devolucoesNormal = 0, baseCalcPisCofinsTotal = 0,
                                baseCalcCsllIrpjTotal1 = 0, baseCalcCsllIrpjTotal2 = 0, baseCalcCsllIrpjTotal3 = 0, baseCalcCsllIrpjTotal4 = 0,
                                pisApuradoTotal = 0, pisRetidoTotal = 0, pisAPagarTotal = 0, cofinsApuradoTotal = 0, cofinsRetidoTotal = 0, cofinsAPagarTotal = 0,
                                csllApuradoTotal = 0, csllRetidoTotal = 0, irpjApuradoTotal = 0, irpjRetidoTotal = 0,
                                cprbAPagarTotal = 0, reducaoIcmsTotal = 0;

                            foreach (var m in mesesTrimestre)
                            {
                                var imp = _taxService.FindByMonth(companyId, m, year);

                                if (imp == null) continue;

                                decimal receita1 = Convert.ToDecimal(imp.Receita1), receita2 = Convert.ToDecimal(imp.Receita2),
                                        receita3 = Convert.ToDecimal(imp.Receita3), receita4 = Convert.ToDecimal(imp.Receita4),
                                        receita = receita1 + receita2 + receita3 + receita4,
                                        devolucao1 = Convert.ToDecimal(imp.Devolucao1),
                                        devolucao2 = Convert.ToDecimal(imp.Devolucao2),
                                        devolucao3 = Convert.ToDecimal(imp.Devolucao3),
                                        devolucao4 = Convert.ToDecimal(imp.Devolucao4),
                                        devolucoes = devolucao1 + devolucao2 + devolucao3 + devolucao4,
                                        devolucaoNormal = Convert.ToDecimal(imp.DevolucaoNormal),
                                        reducaoIcms = Convert.ToDecimal(imp.ReducaoIcms);

                                devolucao1 += Convert.ToDecimal(imp.Devolucao1P);
                                devolucao2 += Convert.ToDecimal(imp.Devolucao2P);
                                devolucao3 += Convert.ToDecimal(imp.Devolucao3P);
                                devolucao4 += Convert.ToDecimal(imp.Devolucao4P);
                                devolucaoNormal += Convert.ToDecimal(imp.DevolucaoNormalP);

                                devolucoes = devolucao1 + devolucao2 + devolucao3 + devolucao4;

                                // PIS E COFINS
                                decimal baseCalcPisCofins = receita - Convert.ToDecimal(imp.ReceitaMono) - devolucaoNormal - reducaoIcms,
                                        pisApurado = (baseCalcPisCofins * Convert.ToDecimal(comp.PercentualPis)) / 100,
                                        pisRetido = Convert.ToDecimal(imp.PisRetido), pisAPagar = pisApurado - pisRetido,
                                        cofinsApurado = (baseCalcPisCofins * Convert.ToDecimal(comp.PercentualCofins)) / 100,
                                        cofinsRetido = Convert.ToDecimal(imp.CofinsRetido), cofinsAPagar = cofinsApurado - cofinsRetido;

                                // CSLL E IRPJ
                                decimal baseCalcCsllIrpj1 = receita1 - devolucao1, baseCalcCsllIrpj2 = receita2 - devolucao2,
                                        baseCalcCsllIrpj3 = receita3 - devolucao3, baseCalcCsllIrpj4 = receita4 - devolucao4;

                                //CSLL
                                decimal percentualCsll1 = Convert.ToDecimal(comp.CSLL1), percentualCsll2 = Convert.ToDecimal(comp.CSLL2),
                                        csll1 = (baseCalcCsllIrpj1 * percentualCsll1 / 100), csll2 = (baseCalcCsllIrpj2 * percentualCsll1 / 100),
                                        csll3 = (baseCalcCsllIrpj3 * percentualCsll1 / 100), csll4 = (baseCalcCsllIrpj4 * percentualCsll2 / 100),
                                        csllApurado = ((csll1 + csll2 + csll3 + csll4) * Convert.ToDecimal(comp.PercentualCSLL)) / 100,
                                        csllRetido = Convert.ToDecimal(imp.CsllRetido), csllPago = Convert.ToDecimal(imp.CsllPago);

                                //IRPJ
                                decimal percentualIrpj1 = Convert.ToDecimal(comp.IRPJ1), percentualIrpj2 = Convert.ToDecimal(comp.IRPJ2),
                                        percentualIrpj3 = Convert.ToDecimal(comp.IRPJ3), percentualIrpj4 = Convert.ToDecimal(comp.IRPJ4),
                                        irp1 = (baseCalcCsllIrpj1 * percentualIrpj1 / 100), irp2 = (baseCalcCsllIrpj2 * percentualIrpj2 / 100),
                                        irp3 = (baseCalcCsllIrpj3 * percentualIrpj3 / 100), irp4 = (baseCalcCsllIrpj4 * percentualIrpj4 / 100),
                                        irpjApurado = ((irp1 + irp2 + irp3 + irp4) * Convert.ToDecimal(comp.PercentualIRPJ)) / 100,
                                        irpjRetido = Convert.ToDecimal(imp.IrpjRetido), irpjPago = Convert.ToDecimal(imp.IrpjPago);

                                //CPRB
                                decimal cprbAPagar = (baseCalcPisCofins * Convert.ToDecimal(comp.CPRB)) / 100;

                                receitaPetroleo += Convert.ToDecimal(imp.Receita1);
                                receitaComercio += Convert.ToDecimal(imp.Receita2);
                                receitaTransporte += Convert.ToDecimal(imp.Receita3);
                                receitaServico += Convert.ToDecimal(imp.Receita4);
                                devolucaoPetroleo += (Convert.ToDecimal(imp.Devolucao1) + Convert.ToDecimal(imp.Devolucao1P));
                                devolucaoComercio += (Convert.ToDecimal(imp.Devolucao2) + Convert.ToDecimal(imp.Devolucao2P));
                                devolucaoTransporte += (Convert.ToDecimal(imp.Devolucao3) + Convert.ToDecimal(imp.Devolucao3P));
                                devolucaoServico += (Convert.ToDecimal(imp.Devolucao4) + Convert.ToDecimal(imp.Devolucao4P));
                                receitas += receita;
                                receitasMono += Convert.ToDecimal(imp.ReceitaMono);
                                devolucoesNormal += devolucaoNormal;
                                reducaoIcmsTotal += reducaoIcms;

                                // PIS E COFINS
                                baseCalcPisCofinsTotal += baseCalcPisCofins;
                                pisApuradoTotal += pisApurado;
                                pisRetidoTotal += pisRetido;
                                pisAPagarTotal += pisAPagar;
                                cofinsApuradoTotal += cofinsApurado;
                                cofinsRetidoTotal += cofinsRetido;
                                cofinsAPagarTotal += cofinsAPagar;

                                // CSLL E IRPJ
                                baseCalcCsllIrpjTotal1 += baseCalcCsllIrpj1;
                                baseCalcCsllIrpjTotal2 += baseCalcCsllIrpj2;
                                baseCalcCsllIrpjTotal3 += baseCalcCsllIrpj3;
                                baseCalcCsllIrpjTotal4 += baseCalcCsllIrpj4;
                                capitalIM += Convert.ToDecimal(imp.CapitalIM);
                                bonificacao += Convert.ToDecimal(imp.Bonificacao);
                                receitaAF += Convert.ToDecimal(imp.ReceitaAF);

                                // CSLL
                                csllApuradoTotal += csllApurado;
                                csllRetidoTotal += csllRetido;
                                csll1Total += csll1;
                                csll2Total += csll2;
                                csll3Total += csll3;
                                csll4Total += csll4;
                                csllFonte += Convert.ToDecimal(imp.CsllFonte);
                                csllPagoTotal += csllPago;

                                //IRPJ
                                irpjApuradoTotal += irpjApurado;
                                irpjRetidoTotal += irpjRetido;
                                irpj1Total += irp1;
                                irpj2Total += irp2;
                                irpj3Total += irp3;
                                irpj4Total += irp4;
                                irpjFonteServico += Convert.ToDecimal(imp.IrpjFonteServico);
                                irpjFonteAF += Convert.ToDecimal(imp.IrpjFonteFinanceira);
                                irpjPagoTotal += irpjPago;

                                //CPRB
                                cprbAPagarTotal += cprbAPagar;

                                List<string> imposto = new List<string>();

                                imposto.Add(imp.MesRef);
                                imposto.Add(Convert.ToDecimal(imp.Receita1).ToString());
                                imposto.Add(Convert.ToDecimal(imp.Receita2).ToString());
                                imposto.Add(Convert.ToDecimal(imp.Receita3).ToString());
                                imposto.Add(Convert.ToDecimal(imp.Receita4).ToString());
                                imposto.Add((Convert.ToDecimal(imp.Devolucao1) + Convert.ToDecimal(imp.Devolucao1P)).ToString());
                                imposto.Add((Convert.ToDecimal(imp.Devolucao2) + Convert.ToDecimal(imp.Devolucao2P)).ToString());
                                imposto.Add((Convert.ToDecimal(imp.Devolucao3) + Convert.ToDecimal(imp.Devolucao3P)).ToString());
                                imposto.Add((Convert.ToDecimal(imp.Devolucao4) + Convert.ToDecimal(imp.Devolucao4P)).ToString());
                                imposto.Add(receita.ToString());
                                imposto.Add(Convert.ToDecimal(imp.ReceitaMono).ToString());
                                imposto.Add(devolucaoNormal.ToString());
                                imposto.Add(reducaoIcms.ToString());
                                imposto.Add(baseCalcPisCofins.ToString());
                                imposto.Add(baseCalcCsllIrpj1.ToString());
                                imposto.Add(baseCalcCsllIrpj2.ToString());
                                imposto.Add(baseCalcCsllIrpj3.ToString());
                                imposto.Add(baseCalcCsllIrpj4.ToString());
                                imposto.Add(pisApurado.ToString());
                                imposto.Add(pisRetido.ToString());
                                imposto.Add(pisAPagar.ToString());
                                imposto.Add(cofinsApurado.ToString());
                                imposto.Add(cofinsRetido.ToString());
                                imposto.Add(cofinsAPagar.ToString());
                                imposto.Add(csllApurado.ToString());
                                imposto.Add(csllRetido.ToString());
                                imposto.Add(csllPago.ToString());
                                imposto.Add(irpjApurado.ToString());
                                imposto.Add(irpjRetido.ToString());
                                imposto.Add(irpjPago.ToString());
                                imposto.Add(cprbAPagar.ToString());
                                impostosTrimestre.Add(imposto);
                            }


                            // IRPJ
                            decimal irpjSubTotal = irpj1Total + irpj2Total + irpj3Total + irpj4Total,
                                    baseCalcIrpjNormal = irpjSubTotal + capitalIM + bonificacao + receitaAF,
                                    irpjNormal = baseCalcIrpjNormal * Convert.ToDecimal(comp.PercentualIRPJ) / 100,
                                    baseCalcAdcionalIrpj = 0, limite = Convert.ToDecimal(basePisCofins.Value),
                                    difImposto = baseCalcIrpjNormal - limite;

                            if (difImposto > 0)
                                baseCalcAdcionalIrpj = difImposto;

                            decimal adicionalIrpj = (baseCalcAdcionalIrpj * Convert.ToDecimal(comp.AdicionalIRPJ)) / 100,
                                    totalIrpj = irpjNormal + adicionalIrpj, irpjAPagar = totalIrpj - irpjFonteAF - irpjFonteServico - irpjRetidoTotal - irpjPagoTotal;

                            // CSLL
                            decimal csllTotal = csll1Total + csll2Total + csll3Total + csll4Total, baseCalcCsll = csllTotal + capitalIM + bonificacao + receitaAF,
                                    csllNormal = baseCalcCsll * Convert.ToDecimal(comp.PercentualCSLL) / 100, csllAPagar = csllNormal - csllFonte - csllRetidoTotal - csllPagoTotal;

                            ViewBag.Impostos = impostosTrimestre;
                            ViewBag.ReceitaPetroleo = receitaPetroleo;
                            ViewBag.ReceitaComercio = receitaComercio;
                            ViewBag.ReceitaTransporte = receitaTransporte;
                            ViewBag.ReceitaServico = receitaServico;
                            ViewBag.DeolucaoPetroleo = devolucaoPetroleo;
                            ViewBag.DeolucaoComercio = devolucaoComercio;
                            ViewBag.DeolucaoTransporte = devolucaoTransporte;
                            ViewBag.DeolucaoServico = devolucaoServico;
                            ViewBag.Receitas = receitas;
                            ViewBag.ReceitasMono = receitasMono;
                            ViewBag.DevolucaoNormal = devolucoesNormal;
                            ViewBag.BaseCalcPisCofins = baseCalcPisCofinsTotal;
                            ViewBag.BaseCalcCsllIrpj1 = baseCalcCsllIrpjTotal1;
                            ViewBag.BaseCalcCsllIrpj2 = baseCalcCsllIrpjTotal2;
                            ViewBag.BaseCalcCsllIrpj3 = baseCalcCsllIrpjTotal3;
                            ViewBag.BaseCalcCsllIrpj4 = baseCalcCsllIrpjTotal4;
                            ViewBag.PisApurado = pisApuradoTotal;
                            ViewBag.PisRetido = pisRetidoTotal;
                            ViewBag.PisAPagar = pisAPagarTotal;
                            ViewBag.CofinsApurado = cofinsApuradoTotal;
                            ViewBag.CofinsRetido = cofinsRetidoTotal;
                            ViewBag.CofinsAPagar = cofinsAPagarTotal;
                            ViewBag.CsllApurado = csllApuradoTotal;
                            ViewBag.CsllRetido = csllRetidoTotal;
                            ViewBag.IrpjApurado = irpjApuradoTotal;
                            ViewBag.IrpjRetido = irpjRetidoTotal;
                            ViewBag.CprbAPagar = cprbAPagarTotal;
                            ViewBag.ReducaoIcms = reducaoIcmsTotal;


                            // IRPJ e CSLL
                            ViewBag.CapitalIM = capitalIM;
                            ViewBag.Bonificacao = bonificacao;
                            ViewBag.ReceitaAF = receitaAF;

                            // IRPJ
                            ViewBag.Irpj1 = irpj1Total;
                            ViewBag.Irpj2 = irpj2Total;
                            ViewBag.Irpj3 = irpj3Total;
                            ViewBag.Irpj4 = irpj4Total;
                            ViewBag.IrpjTotal = irpjSubTotal;
                            ViewBag.BaseCalcIrpjNormal = baseCalcIrpjNormal;
                            ViewBag.BaseCalcAdicionalIrpj = baseCalcAdcionalIrpj;
                            ViewBag.IrpjNormal = irpjNormal;
                            ViewBag.AdicionalIrpj = adicionalIrpj;
                            ViewBag.TotalIrpj = totalIrpj;
                            ViewBag.IrpjFonteServico = irpjFonteServico;
                            ViewBag.IrpjFonteAF = irpjFonteAF;
                            ViewBag.IrpjAPagar = irpjAPagar;
                            ViewBag.IrpjPago = irpjPagoTotal;

                            // CSLL
                            ViewBag.Csll1 = csll1Total;
                            ViewBag.Csll2 = csll2Total;
                            ViewBag.Csll3 = csll3Total;
                            ViewBag.Csll4 = csll4Total;
                            ViewBag.CsllTotal = csllTotal;
                            ViewBag.BaseCalcCsll = baseCalcCsll;
                            ViewBag.CsllNormal = csllNormal;
                            ViewBag.CsllFonte = csllFonte;
                            ViewBag.CsllAPagar = csllAPagar;
                            ViewBag.CsllPago = csllPagoTotal;

                        }
                    }
                    else if (comp.CountingType.Name.Equals("Simples Nacional"))
                    {
                        //  Empresa do Simples
                    }

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
