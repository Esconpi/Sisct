using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Escon.SisctNET.Web.Taxation;
using System.Linq;

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
        private readonly IStateService _stateService;
        private readonly INcmConvenioService _ncmConvenioService;

        public NoteController(
            INoteService service,
            ICompanyService companyService,
            ITaxationService taxationService,
            IProductService productService,
            IProductNoteService itemService,
            ITaxationTypeService taxationTypeService,
            IFunctionalityService functionalityService,
            IConfigurationService configurationService,
            IStateService stateService,
            INcmConvenioService ncmConvenioService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Note")
        {
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _itemService = itemService;
            _product = productService;
            _taxationService = taxationService;
            _stateService = stateService;
            _taxationTypeService = taxationTypeService;
            _ncmConvenioService = ncmConvenioService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        [HttpGet]
        public IActionResult Index(int id, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));

                ViewBag.Comp = comp;

                SessionManager.SetCompanyIdInSession(id);
                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);

                var result = _service.FindByNotes(id, year, month).OrderBy(_ => _.Status).ThenBy(_ => _.View).ToList();
                ViewBag.Count = result.Count();
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Import()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                int id = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                var confDBSisctNfe = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));
                var confDBSisctCte = _configurationService.FindByName("CTe", GetLog(Model.OccorenceLog.Read));

                var importXml = new Xml.Import();
                var importDir = new Diretorio.Import();

                ViewBag.Comp = comp; 

                string directoryNfe = importDir.Entrada(comp, confDBSisctNfe.Value, year, month);
                string directotyCte = importDir.Entrada(comp, confDBSisctCte.Value, year, month);

                List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

                List<Note> notas = new  List<Note>();

                notes = importXml.Nfe(directoryNfe, directotyCte);

                var taxationCompany = _taxationService.FindByCompany(id);
                var ncmConvenio = _ncmConvenioService.FindAll(null);
                var states = _stateService.FindAll(null);
                
                Dictionary<string, string> det = new Dictionary<string, string>();
                
                for (int i = notes.Count - 1; i >= 0; i--)
                {
                    if (notes[i][1]["finNFe"] == "4")
                    {
                        notes.RemoveAt(i);
                        continue;
                    }
                    else if (!notes[i][3]["CNPJ"].Equals(comp.Document))
                    {
                        notes.RemoveAt(i);
                        continue;
                    }
                    else if (notes[i][1]["idDest"] == "1" && comp.Status == true)
                    {
                        if (notes[i][2]["UF"] == notes[i][3]["UF"])
                        {
                            notes.RemoveAt(i);
                            continue;
                        }
                    }

                    var notaImport = _service.FindByNote(notes[i][0]["chave"]);

                    if (notaImport == null)
                    {
                        try
                        {
                            string nCT = notes[i][notes[i].Count() - 1].ContainsKey("nCT") ? notes[i][notes[i].Count() - 1]["nCT"] : "";

                            string IEST = notes[i][2].ContainsKey("IEST") ? notes[i][2]["IEST"] : "";

                            string cnpj = notes[i][2].ContainsKey("CNPJ") ? notes[i][2]["CNPJ"] : notes[i][2]["CPF"];

                            Model.Note note = new Model.Note();

                            note.CompanyId = id;
                            note.Chave = notes[i][0]["chave"];
                            note.Nnf = notes[i][1]["nNF"];
                            note.Dhemi = Convert.ToDateTime(notes[i][1]["dhEmi"]);
                            note.Cnpj = cnpj;
                            note.Crt = notes[i][2]["CRT"];
                            note.Uf = notes[i][2]["UF"];
                            note.Ie = notes[i][2]["IE"];
                            note.Iest = IEST;
                            note.Nct = nCT;
                            note.Xnome = notes[i][2]["xNome"];
                            note.Vnf = Convert.ToDecimal(notes[i][4]["vNF"]);
                            note.IdDest = Convert.ToInt32(notes[i][1]["idDest"]);
                            note.Status = false;
                            note.AnoRef = year;
                            note.MesRef = month;
                            note.Created = DateTime.Now;
                            note.Updated = DateTime.Now;


                            _service.Create(note, GetLog(Model.OccorenceLog.Create));
                        }
                        catch
                        {
                            ViewBag.Erro = 1;
                            ViewBag.Chave = notes[i][0]["chave"];
                            return View(null);
                        }

                       
                    }
                    else
                    {
                        if (!notaImport.MesRef.Equals(month) || !notaImport.AnoRef.Equals(year))
                        {
                            notas.Add(notaImport);
                        }
                    }

                    var nota = _service.FindByNote(notes[i][0]["chave"]);

                    var produtos = _itemService.FindByNote(nota.Id);

                    bool tributada = true;

                    List<Model.ProductNote> addProduct = new List<Model.ProductNote>();

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

                            string nItem = det.ContainsKey("nItem") ? det["nItem"] : "";
                            string NCM = det.ContainsKey("NCM") ? det["NCM"] : "";
                            string CFOP = det.ContainsKey("CFOP") ? det["CFOP"] : "";
                            string CEST = det.ContainsKey("CEST") ? det["CEST"] : "";
                            decimal vUnCom = det.ContainsKey("vUnCom") ? Convert.ToDecimal(det["vUnCom"]) : 0;
                            decimal vICMS = det.ContainsKey("vICMS") ? Convert.ToDecimal(det["vICMS"]) : 0;
                            decimal pICMS = det.ContainsKey("pICMS") ? Convert.ToDecimal(det["pICMS"]) : 0;
                            decimal vIPI = det.ContainsKey("vIPI") ? Convert.ToDecimal(det["vIPI"]) : 0;
                            decimal vPIS = det.ContainsKey("vPIS") ? Convert.ToDecimal(det["vPIS"]) : 0;
                            decimal vCOFINS = det.ContainsKey("vCOFINS") ? Convert.ToDecimal(det["vCOFINS"]) : 0;
                            decimal vFrete = det.ContainsKey("vFrete") ? Convert.ToDecimal(det["vFrete"]) : 0;
                            decimal vSeg = det.ContainsKey("vSeg") ? Convert.ToDecimal(det["vSeg"]) : 0;
                            decimal vOutro = det.ContainsKey("vOutro") ? Convert.ToDecimal(det["vOutro"]) : 0;
                            decimal vDesc = det.ContainsKey("vDesc") ? Convert.ToDecimal(det["vDesc"]) : 0;
                            decimal vICMSST = det.ContainsKey("vICMSST") ? Convert.ToDecimal(det["vICMSST"]) : 0;
                            decimal vBCST = det.ContainsKey("vBCST") ? Convert.ToDecimal(det["vBCST"]) : 0;
                            decimal vBCFCPST = det.ContainsKey("vBCFCPST") ? Convert.ToDecimal(det["vBCFCPST"]) : 0;
                            decimal vBCFCPSTRet = det.ContainsKey("vBCFCPSTRet") ? Convert.ToDecimal(det["vBCFCPSTRet"]) : 0;
                            decimal pFCPST = det.ContainsKey("pFCPST") ? Convert.ToDecimal(det["pFCPST"]) : 0;
                            decimal pFCPSTRet = det.ContainsKey("pFCPSTRet") ? Convert.ToDecimal(det["pFCPSTRet"]) : 0;
                            decimal vFCPST = det.ContainsKey("vFCPST") ? Convert.ToDecimal(det["vFCPST"]) : 0;
                            decimal vFCPSTRet = det.ContainsKey("vFCPSTRet") ? Convert.ToDecimal(det["vFCPSTRet"]) : 0;
                            decimal freteIcms = det.ContainsKey("frete_icms") ? Convert.ToDecimal(det["frete_icms"]) : 0;
                            decimal frete_prod = det.ContainsKey("frete_prod") ? Convert.ToDecimal(det["frete_prod"]) : 0;

                            var productImport = produtos.Where(_ => _.Nitem.Equals(nItem)).FirstOrDefault();

                            if (productImport == null)
                            {

                                decimal pICMSFormat = Math.Round(pICMS, 2);                          
                                string number = pICMSFormat.ToString();

                                if (!number.Contains("."))
                                {
                                    number += ".00";
                                }

                                if (!number.Equals("4.00"))
                                {
                                    var state = _stateService.FindByUf(states, Convert.ToDateTime(notes[i][1]["dhEmi"]), notes[i][2]["UF"], notes[i][3]["UF"]);
                                    number = state.Aliquota.ToString();
                                }

                                var code = comp.Document + NCM + notes[i][2]["UF"] + number.Replace(".", ",");
                                var taxed = _taxationService.FindByCode(taxationCompany, code, CEST, Convert.ToDateTime(notes[i][1]["dhEmi"]));

                                bool incentivo = false;

                                if (nota.Company.Incentive && (!nota.Company.AnnexId.Equals(2) && nota.Company.AnnexId != null))
                                {
                                    incentivo = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, Convert.ToInt32(nota.Company.AnnexId), NCM);
                                }

                                Model.ProductNote prod = new Model.ProductNote();

                                decimal baseDeCalc = Convert.ToDecimal(det["vProd"]) + vFrete + vSeg + vOutro - vDesc + vIPI + frete_prod;

                                if (taxed == null)
                                {
                                    try
                                    {
                                        tributada = false;

                                        prod.Cprod = det["cProd"];
                                        prod.Ncm = NCM;
                                        prod.Cest = CEST;
                                        prod.Cfop = CFOP;
                                        prod.Xprod = det["xProd"];
                                        prod.Vprod = Convert.ToDecimal(det["vProd"]);
                                        prod.Qcom = Convert.ToDecimal(det["qCom"]);
                                        prod.Ucom = det["uCom"];
                                        prod.Vuncom = vUnCom;
                                        prod.Vicms = vICMS;
                                        prod.Picms = pICMS;
                                        prod.Vipi = vIPI;
                                        prod.Vpis = vPIS;
                                        prod.Vcofins = vCOFINS;
                                        prod.Vbasecalc = baseDeCalc;
                                        prod.Vfrete = vFrete;
                                        prod.Vseg = vSeg;
                                        prod.Voutro = vOutro;
                                        prod.Vdesc = vDesc;
                                        prod.IcmsST = vICMSST;
                                        prod.VbcFcpSt = vBCFCPST;
                                        prod.VbcFcpStRet = vBCFCPSTRet;
                                        prod.pFCPST = pFCPST;
                                        prod.pFCPSTRET = pFCPSTRet;
                                        prod.VfcpST = vFCPST;
                                        prod.VfcpSTRet = vFCPSTRet;
                                        prod.IcmsCTe = freteIcms;
                                        prod.Freterateado = frete_prod;
                                        prod.NoteId = nota.Id;
                                        prod.Nitem = det["nItem"];
                                        prod.Orig = Convert.ToInt32(det["orig"]);
                                        prod.Incentivo = incentivo;
                                        prod.Status = false;
                                        prod.Created = DateTime.Now;
                                        prod.Updated = DateTime.Now;

                                    }
                                    catch
                                    {
                                        ViewBag.Erro = 1;
                                        ViewBag.Chave = notes[i][0]["chave"];
                                        return View(null);
                                    }

                                    det.Clear();
                                }
                                else
                                {

                                    var taxedtype = _taxationTypeService.FindById(taxed.TaxationTypeId, GetLog(Model.OccorenceLog.Read));
                                    var calculation = new Calculation();
                                    decimal valorAgreg = 0, valorFecop = 0, valorbcr = 0, valorIcms = vICMS + freteIcms,
                                            valorAgreAliqInt = 0, totalIcms = 0, dif = 0, icmsApu = 0, baseCalc = 0;
                                    
                                    if (taxedtype.Type == "ST")
                                    {
                                        baseCalc = calculation.baseCalc(baseDeCalc, vDesc);

                                        if (taxed.MVA != null)
                                        {
                                            valorAgreg = calculation.ValorAgregadoMva(baseCalc, Convert.ToDecimal(taxed.MVA));
                                        }
                                        if (taxed.BCR != null)
                                        {
                                            valorbcr = calculation.ValorAgregadoBcr(Convert.ToDecimal(taxed.BCR), valorAgreg);
                                            valorIcms = 0;
                                        }

                                        decimal percentFecop = 0;

                                        if (taxed.Fecop != null)
                                        {
                                            percentFecop = Convert.ToDecimal(taxed.Fecop);
                                            valorFecop = calculation.valorFecop(Convert.ToDecimal(taxed.Fecop), valorAgreg);
                                        }
                                        else
                                        {
                                            valorFecop = calculation.valorFecop(0, valorAgreg);
                                        }

                                        valorAgreAliqInt = calculation.valorAgregadoAliqInt(Convert.ToDecimal(taxed.AliqInterna), percentFecop, valorAgreg);

                                        if (valorbcr > 0)
                                        {
                                            valorAgreAliqInt = calculation.valorAgregadoAliqInt(Convert.ToDecimal(taxed.AliqInterna), percentFecop, valorbcr);
                                        }

                                        totalIcms = calculation.totalIcms(valorAgreAliqInt, valorIcms);

                                    }
                                    else if (taxedtype.Type == "Normal")
                                    {

                                        var aliq_simples = _stateService.FindByUf(notes[i][2]["UF"]);
                                        baseCalc = baseDeCalc;
                                        if (number != "4.00")
                                        {
                                            pICMS = aliq_simples.Aliquota;
                                        }
                                        dif = calculation.diferencialAliq(Convert.ToDecimal(taxed.AliqInterna), pICMS);
                                        icmsApu = calculation.icmsApurado(dif, baseCalc);
                                    }
                                    
                                    try
                                    {
                                        prod.Cprod = det["cProd"];
                                        prod.Ncm = NCM;
                                        prod.Cest = CEST;
                                        prod.Cfop = CFOP;
                                        prod.Xprod = det["xProd"];
                                        prod.Vprod = Convert.ToDecimal(det["vProd"]);
                                        prod.Qcom = Convert.ToDecimal(det["qCom"]);
                                        prod.Ucom = det["uCom"];
                                        prod.Vuncom = vUnCom;
                                        prod.Vicms = vICMS;
                                        prod.Picms = pICMS;
                                        prod.Vipi = vIPI;
                                        prod.Vpis = vPIS;
                                        prod.Vcofins = vCOFINS;
                                        prod.Vbasecalc = baseCalc;
                                        prod.Vfrete = vFrete;
                                        prod.Vseg = vSeg;
                                        prod.Voutro = vOutro;
                                        prod.Vdesc = vDesc;
                                        prod.IcmsST = vICMSST;
                                        prod.VbcFcpSt = vBCFCPST;
                                        prod.VbcFcpStRet = vBCFCPSTRet;
                                        prod.pFCPST = pFCPST;
                                        prod.pFCPSTRET = pFCPSTRet;
                                        prod.VfcpST = vFCPST;
                                        prod.VfcpSTRet = vFCPSTRet;
                                        prod.IcmsCTe = freteIcms;
                                        prod.Freterateado = frete_prod;
                                        prod.Aliqinterna = taxed.AliqInterna;
                                        prod.Mva = taxed.MVA;
                                        prod.BCR = taxed.BCR;
                                        prod.Fecop = taxed.Fecop;
                                        prod.Valoragregado = valorAgreg;
                                        prod.ValorBCR = valorbcr;
                                        prod.ValorAC = valorAgreAliqInt;
                                        prod.TotalICMS = totalIcms;
                                        prod.TotalFecop = valorFecop;
                                        prod.Diferencial = dif;
                                        prod.IcmsApurado = icmsApu;
                                        prod.TaxationTypeId = taxed.TaxationTypeId;
                                        prod.NoteId = nota.Id;
                                        prod.Nitem = det["nItem"];
                                        prod.Orig = Convert.ToInt32(det["orig"]);
                                        prod.Incentivo = incentivo;
                                        prod.Produto = "Normal";
                                        prod.Status = true;
                                        prod.Pautado = false;
                                        prod.DateStart = Convert.ToDateTime(taxed.DateStart);
                                        prod.Created = DateTime.Now;
                                        prod.Updated = DateTime.Now;

                                    }
                                    catch
                                    {
                                        ViewBag.Erro = 1;
                                        ViewBag.Chave = notes[i][0]["chave"];
                                        return View(null);
                                    }


                                    det.Clear();
                                }

                                addProduct.Add(prod);
                            }
                            else
                            {
                                det.Clear();
                            }
                        }
                    }

                    _itemService.Create(addProduct, GetLog(OccorenceLog.Create));
                    addProduct.Clear();
                    //var productsTaxation = _itemService.FindByTaxation(nota.Id);

                    if (tributada == true)
                    {
                        nota.Status = true;
                        _service.Update(nota, GetLog(Model.OccorenceLog.Update));
                    }
                }

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                if (notas.Count > 0)
                {
                    return View(notas);
                }
                else
                {
                    return RedirectToAction("Index", new { id = id, year = year, month = month });
                }
              
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message});
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                var note = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                note.Updated = DateTime.Now;
                note.IcmsAp = entity.IcmsAp;
                note.IcmsSt = entity.IcmsSt;
                note.IcmsCo = entity.IcmsCo;
                note.IcmsIm = entity.IcmsIm;
                note.GnreAp = entity.GnreAp;
                note.GnreCo = entity.GnreCo;
                note.GnreIm = entity.GnreIm;
                note.GnreSt = entity.GnreSt;
                note.GnreNAp = entity.GnreNAp;
                note.GnreNCo = entity.GnreNCo;
                note.GnreNIm = entity.GnreNIm;
                note.GnreNSt = entity.GnreNSt;
                note.Fecop1 = entity.Fecop1;
                note.Fecop2 = entity.Fecop2;
                note.FecopGnre1 = entity.FecopGnre1;
                note.FecopGnre2 = entity.FecopGnre2;
                note.Desconto = entity.Desconto;
                note.Frete = entity.Frete;
                note.GnreFecop = entity.GnreFecop;

                var result = _service.Update(note, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index", new { id = note.CompanyId, year = note.AnoRef, month = note.MesRef });
            }

            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Audita()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {

                int id = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var notes = _service.FindByNotes(id, year, month);
                var products = _itemService.FindByProducts(notes);

                products = products.Where(_ => _.Status.Equals(false)).ToList();

                var comp = _companyService.FindById(id, null);

                ViewBag.Registro = products.Count();
                ViewBag.Comp = comp;
                return View(products);

            }
            catch(Exception e)
            {
                return BadRequest(new { erro = 500, message = e.Message });
            }

        }

        public IActionResult Delete()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                int id = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var notes = _service.FindByNotes(id, year, month);

                var products = _itemService.FindByProducts(notes);

                List<Model.ProductNote> deleteProduct = new List<Model.ProductNote>();
                List<Model.Note> deleteNote = new List<Model.Note>();

                foreach (var product in products)
                {
                    deleteProduct.Add(product);
                    //_itemService.Delete(product.Id, GetLog(Model.OccorenceLog.Delete));
                }

                foreach (var note in notes)
                {
                    deleteNote.Add(note);
                    //_service.Delete(note.Id, GetLog(Model.OccorenceLog.Delete));
                }

                _itemService.Delete(deleteProduct, GetLog(OccorenceLog.Delete));
                _service.Delete(deleteNote, GetLog(OccorenceLog.Delete));
                return RedirectToAction("Index", new { id = id, year = year, month = month });

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult DeleteNote(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                int company = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var products = _itemService.FindByNote(id);
                List<Model.ProductNote> deleteProduct = new List<Model.ProductNote>();
                foreach (var product in products)
                {
                    deleteProduct.Add(product);
                   // _itemService.Delete(product.Id, GetLog(Model.OccorenceLog.Delete));
                }
                _itemService.Delete(deleteProduct, GetLog(OccorenceLog.Delete));
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
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