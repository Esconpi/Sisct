using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class NoteController : ControllerBaseSisctNET
    {
        private readonly INoteService _service;
        private readonly ITaxationService _taxationService;
        private readonly IProductService _productService;
        private readonly IProductNoteService _itemService;
        private readonly ICompanyService _companyService;
        private readonly ITaxationTypeService _taxationTypeService;
        private readonly IConfigurationService _configurationService;
        private readonly IAliquotService _aliquotService;
        private readonly INcmConvenioService _ncmConvenioService;
        private readonly IInternalAliquotService _internalAliquotService;

        public NoteController(
            INoteService service,
            ICompanyService companyService,
            ITaxationService taxationService,
            IProductService productService,
            IProductNoteService itemService,
            ITaxationTypeService taxationTypeService,
            IFunctionalityService functionalityService,
            IConfigurationService configurationService,
            IAliquotService aliquotService,
            INcmConvenioService ncmConvenioService,
            IInternalAliquotService internalAliquotService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Note")
        {
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _itemService = itemService;
            _productService = productService;
            _taxationService = taxationService;
            _aliquotService = aliquotService;
            _taxationTypeService = taxationTypeService;
            _ncmConvenioService = ncmConvenioService;
            _internalAliquotService = internalAliquotService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(long id, string year, string month)

        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);

                ViewBag.Company = comp;

                SessionManager.SetCompanyIdInSession(id);
                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);

                var result = _service.FindByNotes(id, year, month)
                    .OrderBy(_ => _.Status)
                    .ThenBy(_ => _.View)
                    .ToList();
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Error(int error, string chave)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(SessionManager.GetCompanyIdInSession(), null);
                ViewBag.Company = comp;
                ViewBag.Erro = error;
                ViewBag.Chave = chave;
                var notas = SessionManager.GetNotesInSession();
                return View(notas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
            
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.Note entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var note = _service.FindById(id, null);
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
                note.Updated = DateTime.Now;

                _service.Update(note, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index", new { id = note.CompanyId, year = note.AnoRef, month = note.MesRef });
            }

            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Audita()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {

                long id = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var notes = _service.FindByNotes(id, year, month);
                var products = _itemService.FindByProducts(notes);

                products = products.Where(_ => _.Status.Equals(false)).ToList();

                var comp = _companyService.FindById(id, null);

                ViewBag.Registro = products.Count();
                ViewBag.Company = comp;
                return View(products);

            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        public IActionResult DeleteNote(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                long company = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var products = _itemService.FindByNote(id);
                List<Model.ProductNote> deleteProduct = new List<Model.ProductNote>();
                foreach (var product in products)
                {
                    deleteProduct.Add(product);
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
        public IActionResult UpdateView(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var note = _service.FindById(id, null);

                note.View = true;

                _service.Update(note, GetLog(Model.OccorenceLog.Update));

                return RedirectToAction("Index", new { id = note.CompanyId, year = note.AnoRef, month = note.MesRef });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public JsonResult Import()
        {
            long id = SessionManager.GetCompanyIdInSession();
            string year = SessionManager.GetYearInSession();
            string month = SessionManager.GetMonthInSession();

            var comp = _companyService.FindById(id, null);
            var confDBSisctNfe = _configurationService.FindByName("NFe", null);
            var confDBSisctCte = _configurationService.FindByName("CTe", null);

            var importXml = new Xml.Import();
            var importDir = new Diretorio.Import();
            var calculation = new Tax.Calculation();

            string directoryNfe = importDir.Entrada(comp, confDBSisctNfe.Value, year, month),
                   directotyCte = importDir.Entrada(comp, confDBSisctCte.Value, year, month),
                   ufCompany = comp.County.State.UF;

            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

            List<Note> notas = new List<Note>();

            notes = importXml.NFeAll(directoryNfe, directotyCte, comp);

            var taxationCompany = _taxationService.FindByCompanyActive(id);
            var ncmConvenio = _ncmConvenioService.FindByNcmAnnex(Convert.ToInt64(comp.AnnexId));
            var aliquotas = _aliquotService.FindByAllState(null);

            Dictionary<string, string> det = new Dictionary<string, string>();

            int erro = 0;
            string url = "Index", chave = "";

            var taxedtypes = _taxationTypeService.FindAll(null);

            List<Model.Note> updateNote = new List<Model.Note>();
            List<Model.ProductNote> addProduct = new List<Model.ProductNote>();

            for (int i = notes.Count - 1; i >= 0; i--)
            {
                if (notes[i].Count() < 3)
                {
                    notes.RemoveAt(i);
                    continue;
                }

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
                else if (notes[i][1]["idDest"] == "1" && comp.Status)
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
                        string nCT = notes[i][notes[i].Count() - 1].ContainsKey("nCT") ? notes[i][notes[i].Count() - 1]["nCT"] : "",
                               IEST = notes[i][2].ContainsKey("IEST") ? notes[i][2]["IEST"] : "",
                               cnpj = notes[i][2].ContainsKey("CNPJ") ? notes[i][2]["CNPJ"] : notes[i][2]["CPF"];

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
                        url = "Error";
                        erro = 1;
                        chave = notes[i][0]["chave"];
                        break;
                    }
                }
                else
                {
                    Model.Note nTemp = new Model.Note();

                    nTemp.Nnf = notaImport.Nnf;
                    nTemp.Dhemi = notaImport.Dhemi;
                    nTemp.Uf = notaImport.Uf;
                    nTemp.Cnpj = notaImport.Cnpj;
                    nTemp.Xnome = notaImport.Xnome;
                    nTemp.Vnf = notaImport.Vnf;
                    nTemp.MesRef = notaImport.MesRef;
                    nTemp.AnoRef = notaImport.AnoRef;

                    if (!notaImport.MesRef.Equals(month) || !notaImport.AnoRef.Equals(year))
                        notas.Add(nTemp);
                }

                var nota = _service.FindByNote(notes[i][0]["chave"]);

                //var produtos = _itemService.FindByNote(nota.Id);

                bool tributada = true;
                int qtd = 0;

                for (int j = 0; j < notes[i].Count; j++)
                {
                    if (notes[i][j].ContainsKey("nItem"))
                        det.Add("nItem", notes[i][j]["nItem"]);

                    if (notes[i][j].ContainsKey("cProd"))
                        det.Add("cProd", notes[i][j]["cProd"]);

                    if (notes[i][j].ContainsKey("xProd"))
                        det.Add("xProd", notes[i][j]["xProd"]);

                    if (notes[i][j].ContainsKey("NCM"))
                        det.Add("NCM", notes[i][j]["NCM"]);

                    if (notes[i][j].ContainsKey("CEST"))
                        det.Add("CEST", notes[i][j]["CEST"]);

                    if (notes[i][j].ContainsKey("CFOP"))
                        det.Add("CFOP", notes[i][j]["CFOP"]);

                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                        det.Add("vProd", notes[i][j]["vProd"]);

                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                        det.Add("vFrete", notes[i][j]["vFrete"]);

                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                        det.Add("vDesc", notes[i][j]["vDesc"]);

                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                        det.Add("vOutro", notes[i][j]["vOutro"]);

                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                        det.Add("vSeg", notes[i][j]["vSeg"]);

                    if (notes[i][j].ContainsKey("vUnCom"))
                        det.Add("vUnCom", notes[i][j]["vUnCom"]);

                    if (notes[i][j].ContainsKey("uCom"))
                        det.Add("uCom", notes[i][j]["uCom"]);

                    if (notes[i][j].ContainsKey("qCom"))
                        det.Add("qCom", notes[i][j]["qCom"]);

                    if (notes[i][j].ContainsKey("vICMS") && (notes[i][j].ContainsKey("CST") || notes[i][j].ContainsKey("CSOSN")))
                        det.Add("vICMS", notes[i][j]["vICMS"]);

                    if (notes[i][j].ContainsKey("orig"))
                        det.Add("orig", notes[i][j]["orig"]);

                    if (notes[i][j].ContainsKey("pICMS"))
                        det.Add("pICMS", notes[i][j]["pICMS"]);

                    if (notes[i][j].ContainsKey("vIPI") && (notes[i][j].ContainsKey("CST") || notes[i][j].ContainsKey("CSOSN")))
                        det.Add("vIPI", notes[i][j]["vIPI"]);

                    if (notes[i][j].ContainsKey("vICMSST") && (notes[i][j].ContainsKey("CST") || notes[i][j].ContainsKey("CSOSN")))
                        det.Add("vICMSST", notes[i][j]["vICMSST"]);

                    if (notes[i][j].ContainsKey("vBCST") && (notes[i][j].ContainsKey("CST") || notes[i][j].ContainsKey("CSOSN")))
                        det.Add("vBCST", notes[i][j]["vBCST"]);

                    if (notes[i][j].ContainsKey("vBCFCPST") && (notes[i][j].ContainsKey("CST") || notes[i][j].ContainsKey("CSOSN")))
                        det.Add("vBCFCPST", notes[i][j]["vBCFCPST"]);

                    if (notes[i][j].ContainsKey("pFCPST") && (notes[i][j].ContainsKey("CST") || notes[i][j].ContainsKey("CSOSN")))
                        det.Add("pFCPST", notes[i][j]["pFCPST"]);

                    if (notes[i][j].ContainsKey("vFCPST") && (notes[i][j].ContainsKey("CST") || notes[i][j].ContainsKey("CSOSN")))
                        det.Add("vFCPST", notes[i][j]["vFCPST"]);

                    if (notes[i][j].ContainsKey("vBCFCPSTRet") && (notes[i][j].ContainsKey("CST") || notes[i][j].ContainsKey("CSOSN")))
                        det.Add("vBCFCPSTRet", notes[i][j]["vBCFCPSTRet"]);

                    if (notes[i][j].ContainsKey("pFCPSTRet") && (notes[i][j].ContainsKey("CST") || notes[i][j].ContainsKey("CSOSN")))
                        det.Add("pFCPSTRet", notes[i][j]["pFCPSTRet"]);

                    if (notes[i][j].ContainsKey("vFCPSTRet") && (notes[i][j].ContainsKey("CST") || notes[i][j].ContainsKey("CSOSN")))
                        det.Add("vFCPSTRet", notes[i][j]["vFCPSTRet"]);

                    if (notes[i][j].ContainsKey("vPIS") && notes[i][j].ContainsKey("CST"))
                        det.Add("vPIS", notes[i][j]["vPIS"]);

                    if (notes[i][j].ContainsKey("vCOFINS") && notes[i][j].ContainsKey("CST"))
                        det.Add("vCOFINS", notes[i][j]["vCOFINS"]);

                    if (notes[i][j].ContainsKey("frete_prod"))
                        det.Add("frete_prod", notes[i][j]["frete_prod"]);

                    if (notes[i][j].ContainsKey("frete_icms"))
                        det.Add("frete_icms", notes[i][j]["frete_icms"]);

                    if (notes[i][j].ContainsKey("CSOSN"))
                        det.Add("CSOSN", notes[i][j]["CSOSN"]);

                    if (notes[i][j].ContainsKey("baseCalc"))
                    {
                        det.Add("baseCalc", notes[i][j]["baseCalc"]);

                        string nItem = det.ContainsKey("nItem") ? det["nItem"] : "",  NCM = det.ContainsKey("NCM") ? det["NCM"] : "",  
                               CFOP = det.ContainsKey("CFOP") ? det["CFOP"] : "", CEST = det.ContainsKey("CEST") ? det["CEST"] : "";
                       
                        decimal vUnCom = det.ContainsKey("vUnCom") ? Convert.ToDecimal(det["vUnCom"]) : 0, vICMS = det.ContainsKey("vICMS") ? Convert.ToDecimal(det["vICMS"]) : 0,
                                pICMS = det.ContainsKey("pICMS") ? Convert.ToDecimal(det["pICMS"]) : 0, vIPI = det.ContainsKey("vIPI") ? Convert.ToDecimal(det["vIPI"]) : 0,
                                vPIS = det.ContainsKey("vPIS") ? Convert.ToDecimal(det["vPIS"]) : 0, vCOFINS = det.ContainsKey("vCOFINS") ? Convert.ToDecimal(det["vCOFINS"]) : 0,
                                vFrete = det.ContainsKey("vFrete") ? Convert.ToDecimal(det["vFrete"]) : 0, vSeg = det.ContainsKey("vSeg") ? Convert.ToDecimal(det["vSeg"]) : 0,
                                vOutro = det.ContainsKey("vOutro") ? Convert.ToDecimal(det["vOutro"]) : 0, vDesc = det.ContainsKey("vDesc") ? Convert.ToDecimal(det["vDesc"]) : 0,
                                vICMSST = det.ContainsKey("vICMSST") ? Convert.ToDecimal(det["vICMSST"]) : 0, vBCST = det.ContainsKey("vBCST") ? Convert.ToDecimal(det["vBCST"]) : 0,
                                vBCFCPST = det.ContainsKey("vBCFCPST") ? Convert.ToDecimal(det["vBCFCPST"]) : 0, vBCFCPSTRet = det.ContainsKey("vBCFCPSTRet") ? Convert.ToDecimal(det["vBCFCPSTRet"]) : 0,
                                pFCPST = det.ContainsKey("pFCPST") ? Convert.ToDecimal(det["pFCPST"]) : 0, pFCPSTRet = det.ContainsKey("pFCPSTRet") ? Convert.ToDecimal(det["pFCPSTRet"]) : 0,
                                vFCPST = det.ContainsKey("vFCPST") ? Convert.ToDecimal(det["vFCPST"]) : 0, vFCPSTRet = det.ContainsKey("vFCPSTRet") ? Convert.ToDecimal(det["vFCPSTRet"]) : 0,
                                freteIcms = det.ContainsKey("frete_icms") ? Convert.ToDecimal(det["frete_icms"]) : 0, frete_prod = det.ContainsKey("frete_prod") ? Convert.ToDecimal(det["frete_prod"]) : 0,
                                vProd = det.ContainsKey("vProd") ? Convert.ToDecimal(det["vProd"]) : 0;

                        var productImport = nota.Products.Where(_ => _.Nitem.Equals(nItem)).FirstOrDefault();

                        if (productImport == null)
                        {

                            decimal pICMSFormat = Math.Round(pICMS, 2);
                            string pICMSValid = pICMSFormat.ToString(), pICMSValidOrig = pICMSFormat.ToString();

                            if (!pICMSValid.Contains("."))
                                pICMSValid += ".00";

                            if (!pICMSValidOrig.Contains("."))
                                pICMSValidOrig += ".00";

                            var orig = det.ContainsKey("orig") ? Convert.ToInt32(det["orig"]) : 0;

                            var aliquotOrig = _aliquotService.FindByUf(aliquotas, Convert.ToDateTime(notes[i][1]["dhEmi"]), notes[i][2]["UF"], comp.County.State.UF);
                            pICMSValidOrig = aliquotOrig.Aliquota.ToString();
                            pICMSValid = aliquotOrig.Aliquota.ToString();

                            if (Convert.ToDecimal(pICMSValid) != 4)
                            {
                                var state = _aliquotService.FindByUf(aliquotas, Convert.ToDateTime(notes[i][1]["dhEmi"]), notes[i][2]["UF"], comp.County.State.UF);
                                pICMSValid = state.Aliquota.ToString();
                            }

                            if (orig == 1 || orig == 2 || orig == 3 || orig == 8)
                            {
                                var aliquot = _aliquotService.FindByUf(aliquotas, Convert.ToDateTime(notes[i][1]["dhEmi"]), "EXT", comp.County.State.UF);
                                pICMSValid = aliquot.Aliquota.ToString();
                            }

                            var code = calculation.Code(comp.Document, NCM, notes[i][2]["UF"], pICMSValid.Replace(".", ","));
                            var taxed = _taxationService.FindByCode(taxationCompany, code, CEST, Convert.ToDateTime(notes[i][1]["dhEmi"]));

                            bool incentivo = false;

                            if (comp.Incentive && comp.AnnexId.Equals((long)1) && comp.ChapterId.Equals((long)7))
                                incentivo = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, NCM, CEST, comp);

                            if (comp.Incentive && comp.AnnexId.Equals((long)3) && comp.ChapterId.Equals((long)1))
                                incentivo = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, NCM, CEST, comp);

                            if (comp.Incentive && comp.AnnexId.Equals((long)4) && comp.ChapterId.Equals((long)15))
                                incentivo = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, NCM, CEST, comp);

                            if (comp.Incentive && comp.ChapterId.Equals((long)4))
                                incentivo = true;


                            Model.ProductNote prod = new Model.ProductNote();

                            decimal baseDeCalc = calculation.BaseCalc(vProd, vFrete, vSeg, vOutro, vDesc, vIPI, frete_prod);

                            if (taxed == null)
                            {
                                tributada = false;

                                try
                                {
                                    prod.Cprod = det["cProd"];
                                    prod.Ncm = NCM;
                                    prod.Cest = CEST;
                                    prod.Cfop = CFOP;
                                    prod.Xprod = det["xProd"];
                                    prod.Vprod = vProd;
                                    prod.Qcom = Convert.ToDecimal(det["qCom"]);
                                    prod.Ucom = det["uCom"];
                                    prod.Vuncom = vUnCom;
                                    prod.Vicms = vICMS;
                                    prod.Picms = Convert.ToDecimal(pICMSValid);
                                    prod.PicmsOrig = Convert.ToDecimal(pICMSValidOrig);
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
                                    prod.Orig = orig;
                                    prod.Incentivo = incentivo;
                                    prod.Status = false;
                                    prod.Pautado = false;
                                    prod.PercentualInciso = null;
                                    prod.Created = DateTime.Now;
                                    prod.Updated = DateTime.Now;

                                }
                                catch
                                {
                                    url = "Error";
                                    erro = 1;
                                    chave = notes[i][0]["chave"];
                                    break;
                                }

                                det.Clear();
                            }
                            else
                            {

                                if (comp.Incentive.Equals(true) && comp.ChapterId.Equals((long)4) && taxed.PercentualInciso == null)
                                    incentivo = false;
                                else if (comp.Incentive.Equals(true) && comp.ChapterId.Equals((long)4) && taxed.PercentualInciso != null)
                                    incentivo = true;

                                var taxedtype = taxedtypes.Where(_ => _.Id.Equals(taxed.TaxationTypeId)).FirstOrDefault();
                                decimal valorAgreg = 0, valorFecop = 0, valorbcr = 0, valorIcms = vICMS + freteIcms,
                                        valorAgreAliqInt = 0, totalIcms = 0, dif = 0, icmsApu = 0, icmsApuCTe = 0, 
                                        baseCalc = 0, dif_frete = 0, aliqInterna = Convert.ToDecimal(taxed.AliqInterna);

                                if (taxedtype.Type == "ST")
                                {
                                    //baseCalc = calculation.BaseCalc(baseDeCalc, vDesc);
                                    baseCalc = baseDeCalc;

                                    if (taxed.MVA != null)
                                        valorAgreg = calculation.ValorAgregadoMva(baseCalc, Convert.ToDecimal(taxed.MVA));

                                    if (taxed.EBcr && taxed.BCR != null)
                                    {
                                        valorbcr = calculation.ValorAgregadoBcr(Convert.ToDecimal(taxed.BCR), valorAgreg);
                                        valorIcms = 0;
                                    }

                                    decimal percentFecop = 0;

                                    if (taxed.Fecop != null)
                                    {
                                        percentFecop = Convert.ToDecimal(taxed.Fecop);
                                        valorFecop = calculation.ValorFecop(Convert.ToDecimal(taxed.Fecop), valorAgreg);
                                    }
                                    else
                                    {
                                        valorFecop = calculation.ValorFecop(0, valorAgreg);
                                    }

                                    valorAgreAliqInt = calculation.ValorAgregadoAliqInt(aliqInterna, percentFecop, valorAgreg);

                                    if (valorbcr > 0)
                                        valorAgreAliqInt = calculation.ValorAgregadoAliqInt(aliqInterna, percentFecop, valorbcr);

                                    totalIcms = calculation.TotalIcms(valorAgreAliqInt, valorIcms);

                                }
                                else if (taxedtype.Type == "Normal" && taxedtype.Id.Equals((long)1))
                                {

                                    //var aliq_simples = _aliquotService.FindByUf(notes[i][2]["UF"]);
                                    baseCalc = baseDeCalc;

                                   /* if (taxed.AliqInternaCTe != null)
                                        aliqCte = Convert.ToDecimal(taxed.AliqInternaCTe);
                                   */

                                    dif = calculation.DiferencialAliq(aliqInterna, Convert.ToDecimal(pICMSValid));
                                    dif_frete = calculation.DiferencialAliq(aliqInterna, Convert.ToDecimal(pICMSValid));
                                    icmsApu = calculation.IcmsApurado(dif, baseCalc - frete_prod);
                                    icmsApuCTe = calculation.IcmsApurado(dif_frete, frete_prod);
                                }
                                else if (taxedtype.Type == "Normal" && !taxedtype.Id.Equals((long)1))
                                {
                                    //var aliq_simples = _aliquotService.FindByUf(notes[i][2]["UF"]);
                                    baseCalc = baseDeCalc;

                                    /*if (taxed.AliqInternaCTe != null)
                                        aliqCte = Convert.ToDecimal(taxed.AliqInternaCTe);
                                    */

                                    dif = calculation.DiferencialAliq(aliqInterna, Convert.ToDecimal(pICMSValid));
                                    dif_frete = calculation.DiferencialAliq(aliqInterna, Convert.ToDecimal(pICMSValidOrig));
                                    icmsApu = calculation.IcmsApurado(dif, baseCalc - frete_prod);
                                    icmsApuCTe = calculation.IcmsApurado(dif_frete, frete_prod);
                                }
                                else if (taxedtype.Type == "Isento")
                                {
                                    baseCalc = baseDeCalc;
                                }
                                else if (taxedtype.Type == "NT")
                                {
                                    baseCalc = baseDeCalc;
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
                                    prod.Picms = Convert.ToDecimal(pICMSValid);
                                    prod.PicmsOrig = Convert.ToDecimal(pICMSValidOrig);
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
                                    prod.Valoragregado = valorAgreg;
                                    prod.ValorBCR = valorbcr;
                                    prod.ValorAC = valorAgreAliqInt;
                                    prod.TotalICMS = totalIcms;
                                    prod.TotalFecop = valorFecop;
                                    prod.Diferencial = dif;
                                    prod.IcmsApurado = icmsApu;
                                    prod.IcmsApuradoCTe = icmsApuCTe;
                                    prod.NoteId = nota.Id;
                                    prod.Nitem = det["nItem"];
                                    prod.Orig = orig;
                                    prod.Incentivo = incentivo;
                                    prod.Produto = "Normal";
                                    prod.Status = true;
                                    prod.Pautado = false;
                                    prod.TaxationTypeId = taxed.TaxationTypeId;
                                    prod.AliqInterna = aliqInterna;
                                    prod.Mva = taxed.MVA;
                                    prod.BCR = taxed.BCR;
                                    prod.Fecop = taxed.Fecop;
                                    prod.DateStart = Convert.ToDateTime(taxed.DateStart);
                                    prod.PercentualInciso = taxed.PercentualInciso;
                                    prod.AliqInternaCTe = aliqInterna;
                                    prod.DiferencialCTe = dif_frete;
                                    prod.EBcr = taxed.EBcr;
                                    prod.Created = DateTime.Now;
                                    prod.Updated = DateTime.Now;

                                }
                                catch
                                {
                                    url = "Error";
                                    erro = 1;
                                    chave = notes[i][0]["chave"];
                                    break;
                                }

                                det.Clear();
                            }

                            addProduct.Add(prod);
                            qtd += 1;
                        }
                        else
                        {
                            det.Clear();
                        }
                    }
                }

                if (tributada == true && qtd > 0)
                {
                    nota.Status = true;
                    updateNote.Add(nota);
                }
            }

            _service.Update(updateNote, GetLog(Model.OccorenceLog.Update));
            _itemService.Create(addProduct, GetLog(OccorenceLog.Create));

            if (notas.Count() > 0 && erro == 0)
            {
                url = "Error";
                SessionManager.SetNotesInSession(notas);
            }

            var result = new
            {
                Controller = "Note",
                URL = url,
                Erro = erro,
                Chave = chave,
            };

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

            return Json(result);
        }

        public JsonResult Delete()
        {

            long id = SessionManager.GetCompanyIdInSession();
            string year = SessionManager.GetYearInSession();
            string month = SessionManager.GetMonthInSession();

            var notes = _service.FindByNotes(id, year, month);

            var products = _itemService.FindByProducts(notes);

            List<Model.ProductNote> deleteProduct = new List<Model.ProductNote>();
            List<Model.Note> deleteNote = new List<Model.Note>();

            foreach (var product in products)
            {
                deleteProduct.Add(product);
            }

            foreach (var note in notes)
            {
                deleteNote.Add(note);
            }

            _itemService.Delete(deleteProduct, GetLog(OccorenceLog.Delete));
            _service.Delete(deleteNote, GetLog(OccorenceLog.Delete));

            int erro = 0;
            string url = "Index", chave = "Nenhuma";

            var result = new
            {
                Controller = "Note",
                URL = url,
                Erro = erro,
                Chave = chave,
            };

            return Json(result);

        }

    }
}