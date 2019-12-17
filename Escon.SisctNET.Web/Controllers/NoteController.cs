using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Escon.SisctNET.Web.Taxation;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Escon.SisctNET.Web.Controllers
{
    public class NoteController : ControllerBaseSisctNET
    {
        private readonly INoteService _service;
        private readonly ITaxationService _taxationService;
        private readonly IProductService _product;
        private readonly IProductNoteService _itemService;
        private readonly ICompanyService _companyService;
        private readonly ITaxationTypeService _taxationTypeService;
        private readonly IConfigurationService _configurationService;

        public NoteController(
            INoteService service,
            ICompanyService companyService,
            ITaxationService taxationService,
            IProductService productService,
            IProductNoteService itemService,
            ITaxationTypeService taxationTypeService,
            IFunctionalityService functionalityService,
            IConfigurationService configurationService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Note")
        {
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _itemService = itemService;
            _product = productService;
            _taxationService = taxationService;
            _taxationTypeService = taxationTypeService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        [HttpGet]
        public IActionResult Index(int id, string year, string month)
        {
            try
            {
                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                ViewBag.Id = comp.Id;
                ViewBag.Year = year;
                ViewBag.Month = month;
                ViewBag.SocialName = comp.SocialName;
                ViewBag.Document = comp.Document;
                ViewBag.Status = comp.Status;

                var result = _service.FindByNotes(id, year, month);

                return View(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Import(int id, string year, string month)
        {
            try
            {
                List<string> LinhasNota = new List<string>();

                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                var confDBSisctNfe = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));
                var confDBSisctCte = _configurationService.FindByName("CTe", GetLog(Model.OccorenceLog.Read));
                var import = new Import();

                string directoryNfe = confDBSisctNfe.Value + "\\" + comp.Document + "\\" + year + "\\" + month;
                string directotyCte = confDBSisctCte.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

                notes = import.Nfe(directoryNfe, directotyCte);          

                var rst = _companyService.FindByDocument(comp.Document, GetLog(Model.OccorenceLog.Read));
                for (int i = notes.Count - 1; i >= 0; i--)
                {
                    if (notes[i][1]["finNFe"] == "4")
                    {
                        notes.RemoveAt(i);
                    }
                    else if (notes[i][1]["idDest"] == "1" && rst.Status == true)
                    {
                        if (notes[i][2]["UF"] == notes[i][3]["UF"])
                        {
                            notes.RemoveAt(i);
                        }
                    }
                }
                 
                Dictionary<string, string> det = new Dictionary<string, string>();

                for (int i = 0; i < notes.Count; i++)
                {
                    var notaImport = _service.FindByNote(notes[i][0]["chave"]);
                    if (notaImport == null)
                    {
                        int lastposicao = notes[i].Count;
                        string nCT = notes[i][lastposicao - 1]["nCT"];

                        string IEST = "";
                        if (notes[i][2].ContainsKey("IEST"))
                        {
                            IEST = notes[i][2]["IEST"];
                        }

                        try
                        {
                            LinhasNota.Add(string.Format("('{}')", MySqlHelper.EscapeString("")));
                            var note = new Model.Note
                            {
                                CompanyId = id,
                                Chave = notes[i][0]["chave"],
                                Nnf = notes[i][1]["nNF"],
                                Dhemi = Convert.ToDateTime(notes[i][1]["dhEmi"]),
                                Cnpj = notes[i][2]["CNPJ"],
                                Crt = notes[i][2]["CRT"],
                                Uf = notes[i][2]["UF"],
                                Ie = notes[i][2]["IE"],
                                Iest = IEST,
                                AnoRef = year,
                                MesRef = month,
                                Created = DateTime.Now,
                                Updated = DateTime.Now,
                                Nct = nCT,
                                Xnome = notes[i][2]["xNome"],
                                Vnf = Convert.ToDecimal(notes[i][4]["vNF"]),
                                Status = false,
                                IdDest = Convert.ToInt32(notes[i][1]["idDest"])
                            };

                            _service.Create(entity: note, GetLog(Model.OccorenceLog.Create));
                        }
                        catch
                        {

                            string message = "A nota " + notes[i][0]["chave"] + " estar com erro de codificação no xml";
                            throw new Exception(message);
                            
                        }

                       
                    }

                    var nota = _service.FindByNote(notes[i][0]["chave"]);

                    int noteId = nota.Id;

                    for (int j = 0; j < notes[i].Count; j++)
                    {
                        if (notes[i][j].ContainsKey("nItem"))
                        {
                            det.Add("nItem", notes[i][j]["nItem"]);
                        }
                        if (notes[i][j].ContainsKey("cProd"))
                        {
                            det.Add("cProd", notes[i][j]["cProd"]);
                        }
                        if (notes[i][j].ContainsKey("xProd"))
                        {
                            det.Add("xProd", notes[i][j]["xProd"]);
                        }
                        if (notes[i][j].ContainsKey("NCM"))
                        {
                            det.Add("NCM", notes[i][j]["NCM"]);
                        }
                        if (notes[i][j].ContainsKey("CEST"))
                        {
                            det.Add("CEST", notes[i][j]["CEST"]);
                        }
                        if (notes[i][j].ContainsKey("CFOP"))
                        {
                            det.Add("CFOP", notes[i][j]["CFOP"]);
                        }
                        if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                        {
                            det.Add("vProd", notes[i][j]["vProd"]);
                        }
                        if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                        {
                            det.Add("vFrete", notes[i][j]["vFrete"]);
                        }
                        if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                        {
                            det.Add("vDesc", notes[i][j]["vDesc"]);
                        }
                        if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                        {
                            det.Add("vOutro", notes[i][j]["vOutro"]);
                        }
                        if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                        {
                            det.Add("vSeg", notes[i][j]["vSeg"]);
                        }
                        if (notes[i][j].ContainsKey("vUnCom"))
                        {
                            det.Add("vUnCom", notes[i][j]["vUnCom"]);
                        }
                        if (notes[i][j].ContainsKey("uCom"))
                        {
                            det.Add("uCom", notes[i][j]["uCom"]);
                        }
                        if (notes[i][j].ContainsKey("qCom"))
                        {
                            det.Add("qCom", notes[i][j]["qCom"]);
                        }
                        if (notes[i][j].ContainsKey("vICMS") && notes[i][j].ContainsKey("CST"))
                        {
                            det.Add("vICMS", notes[i][j]["vICMS"]);
                        }
                        if (notes[i][j].ContainsKey("orig"))
                        {
                            det.Add("orig", notes[i][j]["orig"]);
                        }
                        if (notes[i][j].ContainsKey("pICMS"))
                        {
                            det.Add("pICMS", notes[i][j]["pICMS"]);
                        }
                        if (notes[i][j].ContainsKey("vIPI") && notes[i][j].ContainsKey("CST"))
                        {
                            det.Add("vIPI", notes[i][j]["vIPI"]);
                        }
                        if (notes[i][j].ContainsKey("vICMSST") && notes[i][j].ContainsKey("CST"))
                        {
                            det.Add("vICMSST", notes[i][j]["vICMSST"]);
                        }
                        if (notes[i][j].ContainsKey("vBCST") && notes[i][j].ContainsKey("CST"))
                        {
                            det.Add("vBCST", notes[i][j]["vBCST"]);
                        }
                        if (notes[i][j].ContainsKey("vBCFCPST") && notes[i][j].ContainsKey("CST"))
                        {
                            det.Add("vBCFCPST", notes[i][j]["vBCFCPST"]);
                        }
                        if (notes[i][j].ContainsKey("pFCPST") && notes[i][j].ContainsKey("CST"))
                        {
                            det.Add("pFCPST", notes[i][j]["pFCPST"]);
                        }
                        if (notes[i][j].ContainsKey("vFCPST") && notes[i][j].ContainsKey("CST"))
                        {
                            det.Add("vFCPST", notes[i][j]["vFCPST"]);
                        }
                        if (notes[i][j].ContainsKey("vBCFCPSTRet") && notes[i][j].ContainsKey("CST"))
                        {
                            det.Add("vBCFCPSTRet", notes[i][j]["vBCFCPSTRet"]);
                        }
                        if (notes[i][j].ContainsKey("pFCPSTRet") && notes[i][j].ContainsKey("CST"))
                        {
                            det.Add("pFCPSTRet", notes[i][j]["pFCPSTRet"]);
                        }
                        if (notes[i][j].ContainsKey("vFCPSTRet") && notes[i][j].ContainsKey("CST"))
                        {
                            det.Add("vFCPSTRet", notes[i][j]["vFCPSTRet"]);
                        }
                        if (notes[i][j].ContainsKey("vPIS") && notes[i][j].ContainsKey("CST"))
                        {
                            det.Add("vPIS", notes[i][j]["vPIS"]);
                        }
                        if (notes[i][j].ContainsKey("vCOFINS") && notes[i][j].ContainsKey("CST"))
                        {
                            det.Add("vCOFINS", notes[i][j]["vCOFINS"]);
                        }
                        if (notes[i][j].ContainsKey("frete_prod"))
                        {
                            det.Add("frete_prod", notes[i][j]["frete_prod"]);
                        }
                        if (notes[i][j].ContainsKey("frete_icms"))
                        {
                            det.Add("frete_icms", notes[i][j]["frete_icms"]);
                        }
                        if (notes[i][j].ContainsKey("baseCalc"))
                        {
                            det.Add("baseCalc", notes[i][j]["baseCalc"]);

                            decimal vUnCom = 0, vICMS = 0, pICMS = 0, vIPI = 0, vPIS = 0, vCOFINS = 0, vFrete = 0,
                                    vSeg = 0, vOutro = 0, vDesc = 0, vICMSST = 0, frete_icms = 0, frete_prod = 0, baseDeCalc = 0,
                                    vBCST = 0, vBCFCPST = 0, pFCPST = 0, vFCPST = 0, vBCFCPSTRet = 0, pFCPSTRet = 0, vFCPSTRet = 0;
                            string NCM = "", CEST = "", CFOP = "";
                            string nItem = "";

                            if (det.ContainsKey("NCM"))
                            {
                                NCM = det["NCM"];
                            }

                            if (det.ContainsKey("CFOP"))
                            {
                                CFOP = det["CFOP"];
                            }

                            if (det.ContainsKey("CEST"))
                            {
                                CEST = det["CEST"];
                            }

                            if (det.ContainsKey("vUnCom"))
                            {
                                vUnCom = Convert.ToDecimal(det["vUnCom"]);
                            }

                            if (det.ContainsKey("vICMS"))
                            {
                                vICMS = Convert.ToDecimal(det["vICMS"]);
                            }

                            if (det.ContainsKey("pICMS"))
                            {
                                pICMS = Convert.ToDecimal(det["pICMS"]);
                            }

                            if (det.ContainsKey("vIPI"))
                            {
                                vIPI = Convert.ToDecimal(det["vIPI"]);
                            }

                            if (det.ContainsKey("vPIS"))
                            {
                                vPIS = Convert.ToDecimal(det["vPIS"]);
                            }

                            if (det.ContainsKey("vCOFINS"))
                            {
                                vCOFINS = Convert.ToDecimal(det["vCOFINS"]);
                            }

                            if (det.ContainsKey("vFrete"))
                            {
                                vFrete = Convert.ToDecimal(det["vFrete"]);
                            }

                            if (det.ContainsKey("vSeg"))
                            {
                                vSeg = Convert.ToDecimal(det["vSeg"]);
                            }

                            if (det.ContainsKey("vOutro"))
                            {
                                vOutro = Convert.ToDecimal(det["vOutro"]);
                            }

                            if (det.ContainsKey("vDesc"))
                            {
                                vDesc = Convert.ToDecimal(det["vDesc"]);
                            }

                            if (det.ContainsKey("vICMSST"))
                            {
                                vICMSST = Convert.ToDecimal(det["vICMSST"]);
                            }

                            if (det.ContainsKey("vBCST"))
                            {
                                vBCST = Convert.ToDecimal(det["vBCST"]);
                            }

                            if (det.ContainsKey("vBCFCPST"))
                            {
                                vBCFCPST = Convert.ToDecimal(det["vBCFCPST"]);
                            }

                            if (det.ContainsKey("vBCFCPSTRet"))
                            {
                                vBCFCPSTRet = Convert.ToDecimal(det["vBCFCPSTRet"]);
                            }

                            if (det.ContainsKey("pFCPST"))
                            {
                                pFCPST = Convert.ToDecimal(det["pFCPST"]);
                            }

                            if (det.ContainsKey("pFCPSTRet"))
                            {
                                pFCPSTRet = Convert.ToDecimal(det["pFCPSTRet"]);
                            }

                            if (det.ContainsKey("vFCPST"))
                            {
                                vFCPST = Convert.ToDecimal(det["vFCPST"]);
                            }

                            if (det.ContainsKey("vFCPSTRet"))
                            {
                                vFCPSTRet = Convert.ToDecimal(det["vFCPSTRet"]);
                            }

                            if (det.ContainsKey("frete_icms"))
                            {
                                frete_icms = Convert.ToDecimal(det["frete_icms"]);
                            }

                            if (det.ContainsKey("frete_prod"))
                            {
                                frete_prod = Convert.ToDecimal(det["frete_prod"]);
                            }
                            if (det.ContainsKey("baseCalc"))
                            {
                                baseDeCalc = Convert.ToDecimal(det["baseCalc"]);
                            }
                            if (det.ContainsKey("nItem"))
                            {
                                nItem = det["nItem"];
                            }

                            var productImport = _itemService.FindByProduct(noteId, nItem);

                            if (productImport == null)
                            {

                                decimal pICMSFormat = Math.Round(pICMS, 2);                          
                                string number = pICMSFormat.ToString();

                                if (pICMSFormat == 0)
                                {
                                    number = "0.00";
                                }

                                var code = comp.Document + NCM + notes[i][2]["UF"] + number.Replace(".", ",");
                                var taxed = _taxationService.FindByCode(code, Convert.ToDateTime(notes[i][1]["dhEmi"]));


                                if (taxed == null)
                                {
                                    bool incentivo = false;

                                    if (nota.Company.Incentive && nota.Company.AnnexId != null)
                                    {
                                        incentivo = _itemService.FindByNcmAnnex(Convert.ToInt32(nota.Company.AnnexId), NCM);
                                    }
                                    try
                                    {
                                        var item = new Model.ProductNote
                                        {
                                            Nnf = notes[i][1]["nNF"],
                                            Cprod = det["cProd"],
                                            Ncm = NCM,
                                            Cest = CEST,
                                            Cfop = CFOP,
                                            Xprod = det["xProd"],
                                            Vprod = Convert.ToDecimal(det["vProd"]),
                                            Qcom = Convert.ToDecimal(det["qCom"]),
                                            Ucom = det["uCom"],
                                            Vuncom = vUnCom,
                                            Vicms = vICMS,
                                            Picms = pICMS,
                                            Vipi = vIPI,
                                            Vpis = vPIS,
                                            Vcofins = vCOFINS,
                                            Vbasecalc = baseDeCalc,
                                            Vfrete = vFrete,
                                            Vseg = vSeg,
                                            Voutro = vOutro,
                                            Vdesc = vDesc,
                                            Created = DateTime.Now,
                                            Updated = DateTime.Now,
                                            IcmsST = vICMSST,
                                            VbcFcpSt = vBCFCPST,
                                            VbcFcpStRet = vBCFCPSTRet,
                                            pFCPST = pFCPST,
                                            pFCPSTRET = pFCPSTRet,
                                            VfcpST = vFCPST,
                                            VfcpSTRet = vFCPSTRet,
                                            IcmsCTe = frete_icms,
                                            Freterateado = frete_prod,
                                            NoteId = noteId,
                                            Nitem = det["nItem"],
                                            Status = false,
                                            Orig = Convert.ToInt32(det["orig"]),
                                            Incentivo = incentivo
                                        };
                                        _itemService.Create(entity: item, GetLog(Model.OccorenceLog.Create));
                                    }
                                    catch
                                    {
                                        string message = "A nota " + notes[i][0]["chave"] + " estar com erro de codificação no xml";
                                        throw new Exception(message);
                                    }

                                    det.Clear();
                                }
                                else
                                {

                                    var taxedtype = _taxationTypeService.FindById(taxed.TaxationTypeId, GetLog(Model.OccorenceLog.Read));
                                    var calculation = new Calculation();
                                    decimal valorAgreg = 0, valor_fecop = 0, valorbcr = 0, valor_icms = vICMS + frete_icms,
                                            valorAgre_AliqInt = 0, cms = 0, dif = 0, icmsApu = 0, baseCalc = 0;

                                    
                                    if (taxedtype.Type == "ST")
                                    {
                                        baseCalc = Convert.ToDecimal(det["baseCalc"]) + vDesc;

                                        if (taxed.MVA != null)
                                        {
                                            valorAgreg = calculation.ValorAgregadoMva(baseCalc, Convert.ToDecimal(taxed.MVA));
                                        }
                                        if (taxed.BCR != null)
                                        {
                                            valorbcr = calculation.ValorAgregadoBcr(Convert.ToDecimal(taxed.BCR), valorAgreg);
                                        }

                                        if (taxed.Fecop != null)
                                        {
                                            valor_fecop = calculation.valorFecop(Convert.ToDecimal(taxed.Fecop), valorAgreg);
                                        }
                                        valorAgre_AliqInt = calculation.valorAgregadoAliqInt(Convert.ToDecimal(taxed.AliqInterna), valor_fecop, valorAgreg);
                                        if (valorbcr > 0)
                                        {
                                            valorAgre_AliqInt = calculation.valorAgregadoAliqInt(Convert.ToDecimal(taxed.AliqInterna), valor_fecop, valorbcr);
                                        }
                                        cms = valorAgre_AliqInt - valor_icms;

                                    }
                                    else if (taxedtype.Type == "Normal")
                                    {
                                        baseCalc = Convert.ToDecimal(det["baseCalc"]);
                                        dif = calculation.diferencialAliq(Convert.ToDecimal(taxed.AliqInterna), pICMS);
                                        icmsApu = calculation.icmsApurado(dif, baseCalc);
                                    }

                                    bool incentivo = false;

                                    if (nota.Company.Incentive && nota.Company.AnnexId != null)
                                    {
                                        incentivo = _itemService.FindByNcmAnnex(Convert.ToInt32(nota.Company.AnnexId), NCM);
                                    }

                                    try
                                    {
                                        var item = new Model.ProductNote
                                        {
                                            Nnf = notes[i][1]["nNF"],
                                            Cprod = det["cProd"],
                                            Ncm = NCM,
                                            Cest = CEST,
                                            Cfop = CFOP,
                                            Xprod = det["xProd"],
                                            Vprod = Convert.ToDecimal(det["vProd"]),
                                            Qcom = Convert.ToDecimal(det["qCom"]),
                                            Ucom = det["uCom"],
                                            Vuncom = vUnCom,
                                            Vicms = vICMS,
                                            Picms = pICMS,
                                            Vipi = vIPI,
                                            Vpis = vPIS,
                                            Vcofins = vCOFINS,
                                            Vbasecalc = baseCalc,
                                            Vfrete = vFrete,
                                            Vseg = vSeg,
                                            Voutro = vOutro,
                                            Vdesc = vDesc,
                                            Created = DateTime.Now,
                                            Updated = DateTime.Now,
                                            IcmsST = vICMSST,
                                            VbcFcpSt = vBCFCPST,
                                            VbcFcpStRet = vBCFCPSTRet,
                                            pFCPST = pFCPST,
                                            pFCPSTRET = pFCPSTRet,
                                            VfcpST = vFCPST,
                                            VfcpSTRet = vFCPSTRet,
                                            IcmsCTe = frete_icms,
                                            Freterateado = frete_prod,
                                            Aliqinterna = taxed.AliqInterna,
                                            Mva = taxed.MVA,
                                            BCR = taxed.BCR,
                                            Fecop = taxed.Fecop,
                                            Valoragregado = valorAgreg,
                                            ValorBCR = valorbcr,
                                            ValorAC = valorAgre_AliqInt,
                                            TotalICMS = cms,
                                            TotalFecop = valor_fecop,
                                            Diferencial = dif,
                                            IcmsApurado = icmsApu,
                                            Status = true,
                                            Pautado = false,
                                            TaxationTypeId = taxed.TaxationTypeId,
                                            NoteId = noteId,
                                            Nitem = det["nItem"],
                                            Orig = Convert.ToInt32(det["orig"]),
                                            Incentivo = incentivo
                                        };

                                        _itemService.Create(entity: item, GetLog(Model.OccorenceLog.Create));
                                    }
                                    catch
                                    {
                                        string message = "A nota " + notes[i][0]["chave"] + " estar com erro de codificação no xml";
                                        throw new Exception(message);
                                    }


                                    det.Clear();
                                }   
                            }
                            else
                            {
                                det.Clear();
                            }
                        }
                    }


                    var productsTaxation = _itemService.FindByTaxation(noteId);

                    if (productsTaxation.Count == 0)
                    {
                        nota.Status = true;
                        _service.Update(nota, GetLog(Model.OccorenceLog.Update));
                    }
                }

                return RedirectToAction("Index", new { id = id, year = year , month = month});

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message});
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
            
        }

        [HttpPost]
        public IActionResult Edit(int id, Model.Note entity)
        {
            try
            {
                var note = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                note.Updated = DateTime.Now;

                if (Request.Form["GnreNPaga"] != "")
                {
                    note.GnreNPaga = Convert.ToDecimal(Request.Form["GnreNPaga"]);
                }

                if (Request.Form["IcmsAp"] != "")
                {
                    note.IcmsAp = Convert.ToDecimal(Request.Form["IcmsAp"]);
                }

                if (Request.Form["IcmsSt"] != "")
                {
                    note.IcmsSt = Convert.ToDecimal(Request.Form["IcmsSt"]);
                }

                if (Request.Form["IcmsCo"] != "")
                {
                    note.IcmsCo = Convert.ToDecimal(Request.Form["IcmsCo"]);
                }

                if (Request.Form["IcmsIm"] != "")
                {
                    note.IcmsIm = Convert.ToDecimal(Request.Form["IcmsIm"]);
                }

                if (Request.Form["GnreAp"] != "")
                {
                    note.GnreAp = Convert.ToDecimal(Request.Form["GnreAp"]);
                }

                if (Request.Form["GnreCo"] != "")
                {
                    note.GnreCo = Convert.ToDecimal(Request.Form["GnreCo"]);
                }

                if (Request.Form["GnreIm"] != "")
                {
                    note.GnreIm = Convert.ToDecimal(Request.Form["GnreIm"]);
                }

                if (Request.Form["GnreSt"] != "")
                {
                    note.GnreSt = Convert.ToDecimal(Request.Form["GnreSt"]);
                }

                if (Request.Form["Fecop1"] != "")
                {
                    note.Fecop1 = Convert.ToDecimal(Request.Form["Fecop1"]);
                }

                if (Request.Form["Fecop2"] != "")
                {
                    note.Fecop2 = Convert.ToDecimal(Request.Form["Fecop2"]);
                }

                if (Request.Form["FecopGnre1"] != "")
                {
                    note.FecopGnre1 = Convert.ToDecimal(Request.Form["FecopGnre1"]);
                }

                if (Request.Form["FecopGnre2"] != "")
                {
                    note.FecopGnre2 = Convert.ToDecimal(Request.Form["FecopGnre2"]);
                }

                if (Request.Form["Desconto"] != "")
                {
                    note.Desconto = Convert.ToDecimal(Request.Form["Desconto"]);
                }

                if (Request.Form["Frete"] != "")
                {
                    note.Frete = Convert.ToDecimal(Request.Form["Frete"]);
                }


                var result = _service.Update(note, GetLog(Model.OccorenceLog.Update));

                
                return RedirectToAction("Index", new { id = note.CompanyId, year = note.AnoRef, month = note.MesRef });
            }

            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Delete(int id, string year, string month)
        {
            try
            {
                var notes = _service.FindByNotes(id, year, month);

                var products = _itemService.FindByProducts(notes);

                foreach (var product in products)
                {
                    _itemService.Delete(product.Id, GetLog(Model.OccorenceLog.Delete));
                }

                foreach (var note in notes)
                {
                    _service.Delete(note.Id, GetLog(Model.OccorenceLog.Delete));
                }


                return RedirectToAction("Index", new { id = id, year = year, month = month });

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult DeleteNote(int id,int company, string year, string month)
        {
            try
            {
                var products = _itemService.FindByNotes(id);
                foreach (var product in products)
                {
                    _itemService.Delete(product.Id, GetLog(Model.OccorenceLog.Delete));
                }
                _service.Delete(id, GetLog(Model.OccorenceLog.Delete));

                return RedirectToAction("Index", new { id = company, year = year, month = month });
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}