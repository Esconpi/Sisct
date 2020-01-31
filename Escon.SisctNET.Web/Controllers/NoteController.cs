using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Escon.SisctNET.Web.Taxation;
using System.Threading;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using System.Data;
using Microsoft.Extensions.Configuration;

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
        public IConfiguration _configuration { get; }

        public NoteController(
            INoteService service,
            ICompanyService companyService,
            ITaxationService taxationService,
            IProductService productService,
            IProductNoteService itemService,
            ITaxationTypeService taxationTypeService,
            IFunctionalityService functionalityService,
            IConfigurationService configurationService,
            IConfiguration configuration,
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
            _configuration = configuration;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        [HttpGet]
        public IActionResult Index(int id, string year, string month)
        {
            try
            {
                var login = SessionManager.GetLoginInSession();

                if (login == null)
                {
                    return RedirectToAction("Index", "Authentication");
                }
                else
                {
                    var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                    ViewBag.Id = comp.Id;
                    ViewBag.Year = year;
                    ViewBag.Month = month;
                    ViewBag.SocialName = comp.SocialName;
                    ViewBag.Document = comp.Document;
                    ViewBag.Status = comp.Status;

                    var result = _service.FindByNotes(id, year, month);
                    ViewBag.Count = result.Count();
                    return View(result);
                }

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
                var connectionString = _configuration["MySqlConnection:MySqlConnectionString"];

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
                List<string> notasDoMes = new List<string>();


                using (MySqlConnection mConnection = new MySqlConnection(connectionString))
                {
                    StringBuilder sCommand = new StringBuilder("INSERT INTO Note (CompanyId, Chave, Nnf, Dhemi, Cnpj, Crt, Uf, Ie, Iest, AnoRef, MesRef, Created, Updated, Nct, Xnome, Vnf, Status, IdDest) VALUES ");

                    List<string> LinhasNota = new List<string>();
                    for (int i = 0; i < notes.Count; i++)
                    {
                        var notaImport = _service.FindByNote(notes[i][0]["chave"]);
                        if (notasDoMes.Contains(notes[i][0]["chave"]) || notaImport != null)
                        {
                            continue;
                        }
                        notasDoMes.Add(notes[i][0]["chave"]);
                        int lastposicao = notes[i].Count;
                        string nCT = notes[i][lastposicao - 1]["nCT"];

                        string IEST = "";
                        if (notes[i][2].ContainsKey("IEST"))
                        {
                            IEST = notes[i][2]["IEST"];
                        }
                        try
                        {
                            var dateEmit = notes[i][1]["dhEmi"].Split("T");
                            LinhasNota.Add(string.Format("({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}',{15},{16},{17})",
                            id, notes[i][0]["chave"], notes[i][1]["nNF"], dateEmit[0], notes[i][2]["CNPJ"], notes[i][2]["CRT"], notes[i][2]["UF"],
                            notes[i][2]["IE"], IEST, year, month, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), nCT, 
                            notes[i][2]["xNome"], Convert.ToDecimal(notes[i][4]["vNF"]), false, Convert.ToInt32(notes[i][1]["idDest"])));
                        }
                        catch
                        {
                            string message = "A nota " + notes[i][0]["chave"] + " estar com erro de codificação no xml";
                            throw new Exception(message);
                        }
                        
                        
                    }
                    sCommand.Append(string.Join(",", LinhasNota));
                    sCommand.Append(";");
                    mConnection.Open();
                    using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), mConnection))
                    {
                        myCmd.CommandType = CommandType.Text;
                        myCmd.ExecuteNonQuery();
                    }
                    mConnection.Close();
                        
                    StringBuilder sCommandProd = new StringBuilder("INSERT INTO ProductNote (Nnf, Cprod, Ncm, Cest, Cfop, Xprod, Vprod, Qcom, Ucom, Vuncom, Vicms, Picms, Vipi, " + 
                        "Vpis, Vcofins, Vbasecalc, Vfrete, Vseg, Voutro, Vdesc, Created, Updated, IcmsST, VbcFcpSt, VbcFcpStRet, pFCPST, pFCPSTRET, VfcpST, VfcpSTRet," + 
                        "icmsCTE, Freterateado, Aliqinterna, Mva, BCR, Fecop, Valoragregado, ValorBCR, ValorAC, TotalICMS, TotalFecop, Diferencial, IcmsApurado, Status," + 
                        "Pautado, TaxationTypeId, NoteId, Nitem, Orig, Incentivo) VALUES ");

                    List<string> LinhasProduto = new List<string>();
                    foreach (var note in notes)
                    {

                        var nota = _service.FindByNote(note[0]["chave"]);
                        int noteId = nota.Id;

                        for (int j = 0; j < note.Count; j++)
                        {
                            if (note[j].ContainsKey("nItem"))
                            {
                                det.Add("nItem", note[j]["nItem"]);
                            }
                            if (note[j].ContainsKey("cProd"))
                            {
                                det.Add("cProd", note[j]["cProd"]);
                            }
                            if (note[j].ContainsKey("xProd"))
                            {
                                det.Add("xProd", note[j]["xProd"]);
                            }
                            if (note[j].ContainsKey("NCM"))
                            {
                                det.Add("NCM", note[j]["NCM"]);
                            }
                            if (note[j].ContainsKey("CEST"))
                            {
                                det.Add("CEST", note[j]["CEST"]);
                            }
                            if (note[j].ContainsKey("CFOP"))
                            {
                                det.Add("CFOP", note[j]["CFOP"]);
                            }
                            if (note[j].ContainsKey("vProd") && note[j].ContainsKey("cProd"))
                            {
                                det.Add("vProd", note[j]["vProd"]);
                            }
                            if (note[j].ContainsKey("vFrete") && note[j].ContainsKey("cProd"))
                            {
                                det.Add("vFrete", note[j]["vFrete"]);
                            }
                            if (note[j].ContainsKey("vDesc") && note[j].ContainsKey("cProd"))
                            {
                                det.Add("vDesc", note[j]["vDesc"]);
                            }
                            if (note[j].ContainsKey("vOutro") && note[j].ContainsKey("cProd"))
                            {
                                det.Add("vOutro", note[j]["vOutro"]);
                            }
                            if (note[j].ContainsKey("vSeg") && note[j].ContainsKey("cProd"))
                            {
                                det.Add("vSeg", note[j]["vSeg"]);
                            }
                            if (note[j].ContainsKey("vUnCom"))
                            {
                                det.Add("vUnCom", note[j]["vUnCom"]);
                            }
                            if (note[j].ContainsKey("uCom"))
                            {
                                det.Add("uCom", note[j]["uCom"]);
                            }
                            if (note[j].ContainsKey("qCom"))
                            {
                                det.Add("qCom", note[j]["qCom"]);
                            }
                            if (note[j].ContainsKey("vICMS") && note[j].ContainsKey("CST"))
                            {
                                det.Add("vICMS", note[j]["vICMS"]);
                            }
                            if (note[j].ContainsKey("orig"))
                            {
                                det.Add("orig", note[j]["orig"]);
                            }
                            if (note[j].ContainsKey("pICMS"))
                            {
                                det.Add("pICMS", note[j]["pICMS"]);
                            }
                            if (note[j].ContainsKey("vIPI") && note[j].ContainsKey("CST"))
                            {
                                det.Add("vIPI", note[j]["vIPI"]);
                            }
                            if (note[j].ContainsKey("vICMSST") && note[j].ContainsKey("CST"))
                            {
                                det.Add("vICMSST", note[j]["vICMSST"]);
                            }
                            if (note[j].ContainsKey("vBCST") && note[j].ContainsKey("CST"))
                            {
                                det.Add("vBCST", note[j]["vBCST"]);
                            }
                            if (note[j].ContainsKey("vBCFCPST") && note[j].ContainsKey("CST"))
                            {
                                det.Add("vBCFCPST", note[j]["vBCFCPST"]);
                            }
                            if (note[j].ContainsKey("pFCPST") && note[j].ContainsKey("CST"))
                            {
                                det.Add("pFCPST", note[j]["pFCPST"]);
                            }
                            if (note[j].ContainsKey("vFCPST") && note[j].ContainsKey("CST"))
                            {
                                det.Add("vFCPST", note[j]["vFCPST"]);
                            }
                            if (note[j].ContainsKey("vBCFCPSTRet") && note[j].ContainsKey("CST"))
                            {
                                det.Add("vBCFCPSTRet", note[j]["vBCFCPSTRet"]);
                            }
                            if (note[j].ContainsKey("pFCPSTRet") && note[j].ContainsKey("CST"))
                            {
                                det.Add("pFCPSTRet", note[j]["pFCPSTRet"]);
                            }
                            if (note[j].ContainsKey("vFCPSTRet") && note[j].ContainsKey("CST"))
                            {
                                det.Add("vFCPSTRet", note[j]["vFCPSTRet"]);
                            }
                            if (note[j].ContainsKey("vPIS") && note[j].ContainsKey("CST"))
                            {
                                det.Add("vPIS", note[j]["vPIS"]);
                            }
                            if (note[j].ContainsKey("vCOFINS") && note[j].ContainsKey("CST"))
                            {
                                det.Add("vCOFINS", note[j]["vCOFINS"]);
                            }
                            if (note[j].ContainsKey("frete_prod"))
                            {
                                det.Add("frete_prod", note[j]["frete_prod"]);
                            }
                            if (note[j].ContainsKey("frete_icms"))
                            {
                                det.Add("frete_icms", note[j]["frete_icms"]);
                            }
                            if (note[j].ContainsKey("baseCalc"))
                            {
                                det.Add("baseCalc", note[j]["baseCalc"]);

                                decimal vUnCom = 0, vICMS = 0, pICMS = 0, vIPI = 0, vPIS = 0, vCOFINS = 0, vFrete = 0,
                                        vSeg = 0, vOutro = 0, vDesc = 0, vICMSST = 0, frete_icms = 0, frete_prod = 0, baseDeCalc = 0,
                                        vBCST = 0, vBCFCPST = 0, pFCPST = 0, vFCPST = 0, vBCFCPSTRet = 0, pFCPSTRet = 0, vFCPSTRet = 0;
                                string NCM = "", CEST = "", CFOP = "";
                                string nItem = "";

                                NCM = det.ContainsKey("NCM") ? det["NCM"] : NCM;
                                CFOP = det.ContainsKey("CFOP") ? det["CFOP"] : CFOP;
                                CEST = det.ContainsKey("CEST") ? det["CEST"] : CEST;
                                vUnCom = det.ContainsKey("vUnCom") ? Convert.ToDecimal(det["vUnCom"]) : vUnCom;
                                vICMS = det.ContainsKey("vICMS") ? Convert.ToDecimal(det["vICMS"]) : vICMS;
                                pICMS = det.ContainsKey("pICMS") ? Convert.ToDecimal(det["pICMS"]) : pICMS;
                                vIPI = det.ContainsKey("vIPI") ? Convert.ToDecimal(det["vIPI"]) : vIPI;
                                vPIS = det.ContainsKey("vPIS") ? Convert.ToDecimal(det["vPIS"]) : vPIS;
                                vCOFINS = det.ContainsKey("vCOFINS") ? Convert.ToDecimal(det["vCOFINS"]) : vCOFINS;
                                vFrete = det.ContainsKey("vFrete") ? Convert.ToDecimal(det["vFrete"]) : vFrete;
                                vSeg = det.ContainsKey("vSeg") ? Convert.ToDecimal(det["vSeg"]) : vSeg;
                                vOutro = det.ContainsKey("vOutro") ? Convert.ToDecimal(det["vOutro"]) : vOutro;
                                vDesc = det.ContainsKey("vDesc") ? Convert.ToDecimal(det["vDesc"]) : vDesc;
                                vICMSST = det.ContainsKey("vICMSST") ? Convert.ToDecimal(det["vICMSST"]) : vICMSST;
                                vBCST = det.ContainsKey("vBCST") ? Convert.ToDecimal(det["vBCST"]) : vBCST;
                                vBCFCPST = det.ContainsKey("vBCFCPST") ? Convert.ToDecimal(det["vBCFCPST"]) : vBCFCPST;
                                vBCFCPSTRet = det.ContainsKey("vBCFCPSTRet") ? Convert.ToDecimal(det["vBCFCPSTRet"]) : vBCFCPSTRet;
                                pFCPST = det.ContainsKey("pFCPST") ? Convert.ToDecimal(det["pFCPST"]) : pFCPST;
                                pFCPSTRet = det.ContainsKey("pFCPSTRet") ? Convert.ToDecimal(det["pFCPSTRet"]) : pFCPSTRet;
                                vFCPST = det.ContainsKey("vFCPST") ? Convert.ToDecimal(det["vFCPST"]) : vFCPST;
                                vFCPSTRet = det.ContainsKey("vFCPSTRet") ? Convert.ToDecimal(det["vFCPSTRet"]) : vFCPSTRet;
                                frete_icms = det.ContainsKey("frete_icms") ? Convert.ToDecimal(det["frete_icms"]) : frete_icms;
                                frete_prod = det.ContainsKey("frete_prod") ? Convert.ToDecimal(det["frete_prod"]) : frete_prod;
                                baseDeCalc = det.ContainsKey("baseCalc") ? Convert.ToDecimal(det["baseCalc"]) : baseDeCalc;
                                nItem = det.ContainsKey("nItem") ? det["nItem"] : nItem;

                                var productImport = _itemService.FindByProduct(noteId, nItem);

                                if (productImport == null)
                                {                                    
                                    string number = Math.Round(pICMS, 2).ToString();

                                    if (!number.Contains("."))
                                    {
                                        number += ".00";
                                    }

                                    var code = comp.Document + NCM + note[2]["UF"] + number.Replace(".", ",");
                                    var taxed = _taxationService.FindByCode(code, Convert.ToDateTime(note[1]["dhEmi"]));


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
                                                Nnf = note[1]["nNF"],
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
                                            //_itemService.Create(entity: item, GetLog(Model.OccorenceLog.Create));


                                            LinhasProduto.Add(string.Format("('{0}','{1}','{2}','{3}','{4}','{5}',{6},{7},'{8}',{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}," +
                                                "'{20}','{21}',{22},{23},{24},{25},{26},{27},{28},{29},{30},null,null,null,null,null,null,null,null,null,null,null,{31},{32},null," +
                                                "{33},{34},{35},{36})", note[1]["nNF"],  det["cProd"], NCM, CEST, CFOP, det["xProd"], det["vProd"], det["qCom"], det["uCom"], vUnCom, 
                                                vICMS, pICMS,vIPI, vPIS, vCOFINS, baseDeCalc, vFrete, vSeg, vOutro, vDesc, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 
                                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), vICMSST, vBCFCPST, vBCFCPSTRet, pFCPST, pFCPSTRet, vFCPST, vFCPSTRet, frete_icms, 
                                                frete_prod, false, false, noteId, det["nItem"], det["orig"], incentivo));
                                        }
                                        catch
                                        {
                                            string message = "A nota " + note[0]["chave"] + " estar com erro de codificação no xml";
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
                                            decimal percentFecop = 0;
                                            if (taxed.Fecop != null)
                                            {
                                                percentFecop = Convert.ToDecimal(taxed.Fecop);
                                                valor_fecop = calculation.valorFecop(Convert.ToDecimal(taxed.Fecop), valorAgreg);
                                            }

                                            valorAgre_AliqInt = calculation.valorAgregadoAliqInt(Convert.ToDecimal(taxed.AliqInterna), percentFecop, valorAgreg);
                                            if (valorbcr > 0)
                                            {
                                                valorAgre_AliqInt = calculation.valorAgregadoAliqInt(Convert.ToDecimal(taxed.AliqInterna), percentFecop, valorbcr);
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
                                                Nnf = note[1]["nNF"],
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
                                            if (taxed.MVA == null)
                                            {
                                                LinhasProduto.Add(string.Format("('{0}','{1}','{2}','{3}','{4}','{5}',{6},{7},'{8}',{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}," +
                                                    "'{20}','{21}',{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},null,null,null,{32},{33},{34},{35},{36},{37},{38},{39},{40},{41}," +
                                                    "{42},{43},{44},{45})", note[1]["nNF"], det["cProd"], NCM, CEST, CFOP, det["xProd"], det["vProd"], det["qCom"], det["uCom"], vUnCom,
                                                    vICMS, pICMS, vIPI, vPIS, vCOFINS, baseDeCalc, vFrete, vSeg, vOutro, vDesc, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), vICMSST, vBCFCPST, vBCFCPSTRet, pFCPST, pFCPSTRet, vFCPST, vFCPSTRet, frete_icms,
                                                    frete_prod, taxed.AliqInterna, valorAgreg, valorbcr, valorAgre_AliqInt, cms, valor_fecop, dif,
                                                    icmsApu, true, false, taxed.TaxationTypeId, noteId, det["nItem"], det["orig"], incentivo));
                                            }
                                            else if (taxed.MVA != null && taxed.BCR == null && taxed.Fecop != null)
                                            {
                                                LinhasProduto.Add(string.Format("('{0}','{1}','{2}','{3}','{4}','{5}',{6},{7},'{8}',{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}," +
                                                        "'{20}','{21}',{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},null,{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43}," +
                                                        "{44},{45},{46},{47})", note[1]["nNF"], det["cProd"], NCM, CEST, CFOP, det["xProd"], det["vProd"], det["qCom"], det["uCom"], vUnCom,
                                                        vICMS, pICMS, vIPI, vPIS, vCOFINS, baseDeCalc, vFrete, vSeg, vOutro, vDesc, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), vICMSST, vBCFCPST, vBCFCPSTRet, pFCPST, pFCPSTRet, vFCPST, vFCPSTRet, frete_icms,
                                                        frete_prod, taxed.AliqInterna, taxed.MVA, taxed.Fecop, valorAgreg, valorbcr, valorAgre_AliqInt, cms, valor_fecop, dif,
                                                        icmsApu, true, false, taxed.TaxationTypeId, noteId, det["nItem"], det["orig"], incentivo));
                                            }
                                            else if (taxed.MVA != null && taxed.BCR != null && taxed.Fecop == null)
                                            {
                                                LinhasProduto.Add(string.Format("('{0}','{1}','{2}','{3}','{4}','{5}',{6},{7},'{8}',{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}," +
                                                        "'{20}','{21}',{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},null,{34},{35},{36},{37},{38},{39},{40},{41},{42},{43}," +
                                                        "{44},{45},{46},{47})", note[1]["nNF"], det["cProd"], NCM, CEST, CFOP, det["xProd"], det["vProd"], det["qCom"], det["uCom"], vUnCom,
                                                        vICMS, pICMS, vIPI, vPIS, vCOFINS, baseDeCalc, vFrete, vSeg, vOutro, vDesc, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), vICMSST, vBCFCPST, vBCFCPSTRet, pFCPST, pFCPSTRet, vFCPST, vFCPSTRet, frete_icms,
                                                        frete_prod, taxed.AliqInterna, taxed.MVA, taxed.BCR, valorAgreg, valorbcr, valorAgre_AliqInt, cms, valor_fecop, dif,
                                                        icmsApu, true, false, taxed.TaxationTypeId, noteId, det["nItem"], det["orig"], incentivo));
                                            }
                                            else if (taxed.MVA != null && taxed.BCR == null && taxed.Fecop == null)
                                            {
                                                LinhasProduto.Add(string.Format("('{0}','{1}','{2}','{3}','{4}','{5}',{6},{7},'{8}',{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}," +
                                                        "'{20}','{21}',{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},null,null,{34},{35},{36},{37},{38},{39},{40},{41},{42}," +
                                                        "{43},{44},{45},{46})", note[1]["nNF"], det["cProd"], NCM, CEST, CFOP, det["xProd"], det["vProd"], det["qCom"], det["uCom"], vUnCom,
                                                        vICMS, pICMS, vIPI, vPIS, vCOFINS, baseDeCalc, vFrete, vSeg, vOutro, vDesc, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), vICMSST, vBCFCPST, vBCFCPSTRet, pFCPST, pFCPSTRet, vFCPST, vFCPSTRet, frete_icms,
                                                        frete_prod, taxed.AliqInterna, taxed.MVA, valorAgreg, valorbcr, valorAgre_AliqInt, cms, valor_fecop, dif,
                                                        icmsApu, true, false, taxed.TaxationTypeId, noteId, det["nItem"], det["orig"], incentivo));
                                            }
                                            else
                                            {
                                                LinhasProduto.Add(string.Format("('{0}','{1}','{2}','{3}','{4}','{5}',{6},{7},'{8}',{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}," +
                                                    "'{20}','{21}',{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44}," +
                                                    "{45},{46},{47},{48})", note[1]["nNF"], det["cProd"], NCM, CEST, CFOP, det["xProd"], det["vProd"], det["qCom"], det["uCom"], vUnCom,
                                                    vICMS, pICMS, vIPI, vPIS, vCOFINS, baseDeCalc, vFrete, vSeg, vOutro, vDesc, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), vICMSST, vBCFCPST, vBCFCPSTRet, pFCPST, pFCPSTRet, vFCPST, vFCPSTRet, frete_icms,
                                                    frete_prod, taxed.AliqInterna, taxed.MVA, taxed.BCR, taxed.Fecop, valorAgreg, valorbcr, valorAgre_AliqInt, cms, valor_fecop, dif,
                                                    icmsApu, true, false, taxed.TaxationTypeId, noteId, det["nItem"], det["orig"], incentivo));
                                            }

                                        }
                                        catch
                                        {
                                            string message = "A nota " + note[0]["chave"] + " estar com erro de codificação no xml";
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
                    }
                    sCommandProd.Append(string.Join(",", LinhasProduto));
                    sCommandProd.Append(";");
                    mConnection.Open();
                    using (MySqlCommand myCmd = new MySqlCommand(sCommandProd.ToString(), mConnection))
                    {
                        myCmd.CommandType = CommandType.Text;
                        myCmd.ExecuteNonQuery();
                    }
                    mConnection.Close();

                    var notasMesComp = _service.FindByNotes(id, year, month); 
                    foreach (var notaMes in notasMesComp)
                    {
                        var productsTaxation = _itemService.FindByTaxation(notaMes.Id);

                        if (productsTaxation.Count == 0)
                        {
                            notaMes.Status = true;
                            _service.Update(notaMes, GetLog(Model.OccorenceLog.Update));
                        }
                    }
                }
                return RedirectToAction("Index", new { id = id, year = year, month = month });
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
                else
                {
                    note.GnreNPaga = null;
                }

                if (Request.Form["IcmsAp"] != "")
                {
                    note.IcmsAp = Convert.ToDecimal(Request.Form["IcmsAp"]);
                }
                else
                {
                    note.IcmsAp = null;
                }

                if (Request.Form["IcmsSt"] != "")
                {
                    note.IcmsSt = Convert.ToDecimal(Request.Form["IcmsSt"]);
                }
                else
                {
                    note.IcmsSt = null;
                }

                if (Request.Form["IcmsCo"] != "")
                {
                    note.IcmsCo = Convert.ToDecimal(Request.Form["IcmsCo"]);
                }
                else
                {
                    note.IcmsCo = null;
                }

                if (Request.Form["IcmsIm"] != "")
                {
                    note.IcmsIm = Convert.ToDecimal(Request.Form["IcmsIm"]);
                }
                else
                {
                    note.IcmsIm = null;
                }

                if (Request.Form["GnreAp"] != "")
                {
                    note.GnreAp = Convert.ToDecimal(Request.Form["GnreAp"]);
                }
                else
                {
                    note.GnreAp = null;
                }

                if (Request.Form["GnreCo"] != "")
                {
                    note.GnreCo = Convert.ToDecimal(Request.Form["GnreCo"]);
                }
                else
                {
                    note.GnreCo = null;
                }

                if (Request.Form["GnreIm"] != "")
                {
                    note.GnreIm = Convert.ToDecimal(Request.Form["GnreIm"]);
                }
                else
                {
                    note.GnreIm = null;
                }

                if (Request.Form["GnreSt"] != "")
                {
                    note.GnreSt = Convert.ToDecimal(Request.Form["GnreSt"]);
                }
                else
                {
                    note.GnreSt = null;
                }

                if (Request.Form["Fecop1"] != "")
                {
                    note.Fecop1 = Convert.ToDecimal(Request.Form["Fecop1"]);
                }
                else
                {
                    note.Fecop1 = null;
                }

                if (Request.Form["Fecop2"] != "")
                {
                    note.Fecop2 = Convert.ToDecimal(Request.Form["Fecop2"]);
                }
                else
                {
                    note.Fecop2 = null;
                }

                if (Request.Form["FecopGnre1"] != "")
                {
                    note.FecopGnre1 = Convert.ToDecimal(Request.Form["FecopGnre1"]);
                }
                else
                {
                    note.FecopGnre1 = null;
                }

                if (Request.Form["FecopGnre2"] != "")
                {
                    note.FecopGnre2 = Convert.ToDecimal(Request.Form["FecopGnre2"]);
                }
                else
                {
                    note.FecopGnre2 = null;
                }

                if (Request.Form["Desconto"] != "")
                {
                    note.Desconto = Convert.ToDecimal(Request.Form["Desconto"]);
                }
                else
                {
                    note.Desconto = null;
                }

                if (Request.Form["Frete"] != "")
                {
                    note.Frete = Convert.ToDecimal(Request.Form["Frete"]);
                }
                else
                {
                    note.Frete = null;
                }


                var result = _service.Update(note, GetLog(Model.OccorenceLog.Update));

                
                return RedirectToAction("Index", new { id = note.CompanyId, year = note.AnoRef, month = note.MesRef });
            }

            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Audita(int id, string year, string month)
        {
            try
            {
                var notes = _service.FindByNotes(id, year, month);
                var products = _itemService.FindByProducts(notes);

                products = products.Where(_ => _.Status.Equals(false)).ToList();
                ViewBag.Registro = products.Count();
                return View(products);

            }
            catch(Exception e)
            {
                return BadRequest(new { erro = 500, message = e.Message });
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

        [HttpGet]
        public IActionResult UpdateView(int id)
        {
            try
            {
                var note = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                note.View = true;

                _service.Update(note, GetLog(Model.OccorenceLog.Update));

                return RedirectToAction("Index", new { id = note.CompanyId, year = note.AnoRef, month = note.MesRef });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}