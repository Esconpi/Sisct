﻿using Escon.SisctNET.Model;
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
        private readonly IAliquotConfazService _aliquotConfazService;
        private readonly IInternalAliquotConfazService _internalAliquotConfazService;
        private readonly ITaxationPService _taxationPService;

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
            IAliquotConfazService aliquotConfazService,
            IInternalAliquotConfazService internalAliquotConfazService,
            ITaxationPService taxationPService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Note")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
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
            _aliquotConfazService = aliquotConfazService;
            _internalAliquotConfazService = internalAliquotConfazService;
            _taxationPService = taxationPService;
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

            var taxationsCompany = _taxationService.FindByCompanyActive(id);
            var ncmConvenio = _ncmConvenioService.FindByAnnex(null);
            var ncmConvenioAnnex = ncmConvenio.Where(_ => _.AnnexId.Equals(comp.AnnexId)).ToList();
            var ncmConvenioBCR = ncmConvenio.Where(_ => _.Annex.Description.Equals("ANEXO I - MÁQUINAS, APARELHOS E EQUIPAMENTOS INDUSTRIAIS") ||
                                                        _.Annex.Description.Equals("ANEXO II - MÁQUINAS E IMPLEMENTOS AGRÍCOLA"))
                                            .ToList();
            var aliquotas = _aliquotService.FindByAllState(null);
            var aliquotasConfaz = _aliquotConfazService.FindByAllState(null);
            var aliquotasinternaConfaz = _internalAliquotConfazService.FindByAllState(null);
            //var taxedtypes = _taxationTypeService.FindAll(null);
            var products = _productService.FindAll(null);
            var taxationsPCompany = _taxationPService.FindByCompanyActive(id);

            Dictionary<string, string> det = new Dictionary<string, string>();

            int erro = 0;
            string url = "Index", chave = "";

            List<NcmConvenio> ncmConvenioAnnexTemp = new List<NcmConvenio>();
            List<NcmConvenio> ncmConvenioBCRTemp = new List<NcmConvenio>();
            List<Note> updateNote = new List<Note>();
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

                Model.Note notaImport = _service.FindByNote(notes[i][0]["chave"]),
                           nota = notaImport;

                if (notaImport == null)
                {
                    try
                    {
                        string nCT = notes[i][notes[i].Count() - 1].ContainsKey("nCT") ? notes[i][notes[i].Count() - 1]["nCT"] : "",
                               IEST = notes[i][2].ContainsKey("IEST") ? notes[i][2]["IEST"] : "",
                               cnpj = notes[i][2].ContainsKey("CNPJ") ? notes[i][2]["CNPJ"] : notes[i][2]["CPF"];

                        Model.Note note = new Model.Note()
                        {
                            CompanyId = id,
                            Chave = notes[i][0]["chave"],
                            Nnf = notes[i][1]["nNF"],
                            Dhemi = Convert.ToDateTime(notes[i][1]["dhEmi"]),
                            Cnpj = cnpj,
                            Crt = notes[i][2]["CRT"],
                            Uf = notes[i][2]["UF"],
                            Ie = notes[i][2]["IE"],
                            Iest = IEST,
                            Nct = nCT,
                            Xnome = notes[i][2]["xNome"],
                            Vnf = Convert.ToDecimal(notes[i][4]["vNF"]),
                            IdDest = Convert.ToInt32(notes[i][1]["idDest"]),
                            AnoRef = year,
                            MesRef = month
                        };

                        nota = _service.Create(note, GetLog(Model.OccorenceLog.Create));

                        nota.Products = new List<Model.ProductNote>();

                        ncmConvenioAnnexTemp = _ncmConvenioService.FindAllInDate(ncmConvenioAnnex, note.Dhemi);
                        ncmConvenioBCRTemp = _ncmConvenioService.FindAllInDate(ncmConvenioBCR, note.Dhemi);
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
                    Model.Note nTemp = new Model.Note()
                    {
                        CompanyId = id,
                        Nnf = notaImport.Nnf,
                        Dhemi = notaImport.Dhemi,
                        Uf = notaImport.Uf,
                        Cnpj = notaImport.Cnpj,
                        Xnome = notaImport.Xnome,
                        Vnf = notaImport.Vnf,
                        MesRef = notaImport.MesRef,
                        AnoRef = notaImport.AnoRef
                    };

                    if (!notaImport.MesRef.Equals(month) || !notaImport.AnoRef.Equals(year))
                        notas.Add(nTemp);
                }

                int qtd = 0, qtdTaxation = 0;

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

                    if (notes[i][j].ContainsKey("orig"))
                        det.Add("orig", notes[i][j]["orig"]);

                    if (notes[i][j].ContainsKey("pICMS"))
                        det.Add("pICMS", notes[i][j]["pICMS"]);

                    if (notes[i][j].ContainsKey("orig") && notes[i][j].ContainsKey("CST"))
                        det.Add("CST", notes[i][j]["CST"]);

                    if (notes[i][j].ContainsKey("orig") && notes[i][j].ContainsKey("CSOSN"))
                        det.Add("CSOSN", notes[i][j]["CSOSN"]);

                    if (notes[i][j].ContainsKey("vICMS") && (notes[i][j].ContainsKey("CST") || notes[i][j].ContainsKey("CSOSN")))
                        det.Add("vICMS", notes[i][j]["vICMS"]);

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

                    if (notes[i][j].ContainsKey("vPIS") && (notes[i][j].ContainsKey("CST") || notes[i][j].ContainsKey("CSOSN")))
                        det.Add("vPIS", notes[i][j]["vPIS"]);

                    if (notes[i][j].ContainsKey("vCOFINS") && (notes[i][j].ContainsKey("CST") || notes[i][j].ContainsKey("CSOSN")))
                        det.Add("vCOFINS", notes[i][j]["vCOFINS"]);

                    if (notes[i][j].ContainsKey("frete_prod"))
                        det.Add("frete_prod", notes[i][j]["frete_prod"]);

                    if (notes[i][j].ContainsKey("frete_icms"))
                        det.Add("frete_icms", notes[i][j]["frete_icms"]);

                    if (notes[i][j].ContainsKey("baseCalc"))
                    {
                        det.Add("baseCalc", notes[i][j]["baseCalc"]);

                        string nItem = det.ContainsKey("nItem") ? det["nItem"] : "",  NCM = det.ContainsKey("NCM") ? det["NCM"] : "",  
                               CFOP = det.ContainsKey("CFOP") ? det["CFOP"] : "", CEST = det.ContainsKey("CEST") ? det["CEST"] : "",
                               CST = det.ContainsKey("CST") ? det["CST"] : "", CSOSN = det.ContainsKey("CSOSN") ? det["CSOSN"] : "";
                       
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

                            var aliquotOrig = _aliquotService.FindByUf(aliquotas, nota.Dhemi, nota.Uf, comp.County.State.UF);
                            pICMSValidOrig = aliquotOrig.Aliquota.ToString();
                            pICMSValid = aliquotOrig.Aliquota.ToString();

                            if (Convert.ToDecimal(pICMSValid) != 4)
                            {
                                var state = _aliquotService.FindByUf(aliquotas, nota.Dhemi, nota.Uf, comp.County.State.UF);
                                pICMSValid = state.Aliquota.ToString();
                            }

                            if (orig == 1 || orig == 2 || orig == 3 || orig == 8)
                            {
                                var aliquot = _aliquotService.FindByUf(aliquotas, nota.Dhemi, "EXT", comp.County.State.UF);
                                pICMSValid = aliquot.Aliquota.ToString();
                            }

                            var code = calculation.Code(comp.Document, NCM, nota.Uf, pICMSValid.Replace(".", ","));
                            var taxed = _taxationService.FindByCode(taxationsCompany, code, CEST, nota.Dhemi);

                            bool incentivo = false, eBcr = false, divergent = false;

                            var ncmBcr = _ncmConvenioService.FindByNcmAnnex(ncmConvenioBCRTemp, NCM, CEST, comp);
                            decimal? aliquotConfaz = null, internalAliquotConfaz = null;

                            if (ncmBcr != null)
                            {
                                if (CST == "20")
                                    eBcr = true;
                                else
                                    divergent = true;

                                aliquotConfaz = _aliquotConfazService.FindByUf(aliquotasConfaz, nota.Dhemi, nota.Uf, comp.County.State.UF, ncmBcr.AnnexId).Aliquota;
                                internalAliquotConfaz = _internalAliquotConfazService.FindByUf(aliquotasinternaConfaz, nota.Dhemi, comp.County.State.UF, ncmBcr.AnnexId).Aliquota;
                            }
                            else
                            {
                                if (taxed != null)
                                    eBcr = taxed.EBcr;
                            }

                            if (comp.Incentive)
                            {
                                if (comp.Annex.Description.Equals("ANEXO II - AUTOPEÇAS") && comp.Chapter.Name.Equals("CAPÍTULO IV-B"))
                                    incentivo = _ncmConvenioService.FindByNcmExists(ncmConvenioAnnexTemp, NCM, CEST, comp);

                                if (comp.Annex.Description.Equals("ANEXO ÚNICO") && comp.Chapter.Name.Equals("CAPÍTULO II"))
                                    incentivo = _ncmConvenioService.FindByNcmExists(ncmConvenioAnnexTemp, NCM, CEST, comp);

                                if ((comp.Annex.Description.Equals("ANEXO CCCXXVI (Art. 791 - A)") && comp.Chapter.Name.Equals("CAPÍTULO II – A")) || 
                                    comp.Annex.Description.Equals("ANEXO VII (Art. 59 Parte 1) - REGIMES ESPECIAIS DE TRIBUTAÇÃO"))
                                    incentivo = _ncmConvenioService.FindByNcmExists(ncmConvenioAnnexTemp, NCM, CEST, comp);

                                if (comp.Chapter.Name.Equals("CAPÍTULO IV-C"))
                                    incentivo = true;
                            }

                            decimal baseDeCalc = calculation.BaseCalc(vProd, vFrete, vSeg, vOutro, vDesc, vIPI, frete_prod);

                            Model.ProductNote prod = new Model.ProductNote()
                            {
                                Cprod = det["cProd"],
                                Ncm = NCM,
                                Cest = CEST,
                                Cfop = CFOP,
                                Cst = CST,
                                Csosn = CSOSN,
                                Xprod = det["xProd"],
                                Vprod = vProd,
                                Qcom = Convert.ToDecimal(det["qCom"]),
                                Ucom = det["uCom"],
                                Vuncom = vUnCom,
                                Vicms = vICMS,
                                Picms = Convert.ToDecimal(pICMSValid),
                                PicmsOrig = Convert.ToDecimal(pICMSValidOrig),
                                PicmsBCR = aliquotConfaz,
                                Vipi = vIPI,
                                Vpis = vPIS,
                                Vcofins = vCOFINS,
                                Vbasecalc = baseDeCalc,
                                Vfrete = vFrete,
                                Vseg = vSeg,
                                Voutro = vOutro,
                                Vdesc = vDesc,
                                IcmsST = vICMSST,
                                VbcFcpSt = vBCFCPST,
                                VbcFcpStRet = vBCFCPSTRet,
                                pFCPST = pFCPST,
                                pFCPSTRET = pFCPSTRet,
                                VfcpST = vFCPST,
                                VfcpSTRet = vFCPSTRet,
                                IcmsCTe = freteIcms,
                                Freterateado = frete_prod,
                                Nitem = det["nItem"],
                                Orig = orig,
                                Incentivo = incentivo,
                                EBcr = eBcr,
                                Divergent = divergent,
                                AliqInternaBCR = internalAliquotConfaz,
                                DateStart = new DateTime(nota.Dhemi.Year, nota.Dhemi.Month, 1),
                                TaxationTypeId = 10,
                                NoteId = nota.Id,
                                Created = DateTime.Now,
                                Updated = DateTime.Now
                            };

                            if (taxed == null)
                            {
                                if(prod.Ucom.ToUpper().Equals("UN") || prod.Ucom.ToUpper().Equals("UND") || 
                                   prod.Ucom.ToUpper().Equals("GF") || prod.Ucom.ToUpper().Equals("GR") || 
                                   prod.Ucom.ToUpper().Equals("QT"))
                                {
                                    code = calculation.CodeP(comp.Document, nota.Cnpj, prod.Cprod, NCM, nota.Uf, pICMSValid.Replace(".", ","));
                                    var taxedP = _taxationPService.FindByCode(taxationsPCompany, code, CEST, nota.Dhemi);

                                    if (taxedP != null)
                                    {
                                        if (comp.Incentive)
                                        {
                                            if (comp.Chapter.Name.Equals("CAPÍTULO IV-C") && taxedP.PercentualInciso == null)
                                                prod.Incentivo = false;
                                            else if (comp.Chapter.Name.Equals("CAPÍTULO IV-C") && taxedP.PercentualInciso != null)
                                                prod.Incentivo = true;
                                        }

                                        var product = _productService.FindByProduct(products, taxedP.Product, taxedP.GroupId, nota.Dhemi);

                                        decimal baseCalc = baseDeCalc, valorIcms = vICMS + freteIcms;

                                        if (taxedP.TaxationType.Type == "ST")
                                        {

                                            decimal precoPauta = Convert.ToDecimal(product.Price), totalIcmsPauta = 0,
                                                    quantParaCalc = 0;

                                            quantParaCalc = Convert.ToDecimal(prod.Qcom);

                                            decimal? valorAgreg = null;

                                            // PP feito com os dados da tabela do Ato Normativo
                                            decimal baseCalcPauta = precoPauta * quantParaCalc;

                                            if (taxedP.MVA != null)
                                            {
                                                decimal valorAgregado = calculation.ValorAgregadoMva(baseCalc, Convert.ToDecimal(taxedP.MVA));
                                                valorAgreg = valorAgregado;
                                                prod.Valoragregado = valorAgregado;
                                                prod.Mva = taxedP.MVA;
                                            }

                                            if (taxedP.EBcr && taxedP.BCR != null)
                                            {
                                                decimal valorBcr = calculation.ValorAgregadoBcr(Convert.ToDecimal(taxedP.BCR), Convert.ToDecimal(valorAgreg));
                                                valorAgreg = valorBcr;
                                                prod.ValorBCR = valorBcr;
                                                prod.BCR = taxedP.BCR;
                                                if (prod.Picms.Equals(12))
                                                    valorIcms = baseCalc * 7 / 100;
                                            }

                                            if (taxedP.Fecop != null)
                                            {
                                                prod.Fecop = Convert.ToDecimal(taxedP.Fecop);
                                                decimal valorFecop = calculation.ValorFecop(Convert.ToDecimal(taxedP.Fecop), Convert.ToDecimal(valorAgreg));
                                                decimal valorFecop2 = calculation.ValorFecop(Convert.ToDecimal(taxedP.Fecop), Convert.ToDecimal(baseCalcPauta));
                                                prod.TotalFecop = valorFecop;
                                                prod.TotalFecop2 = valorFecop2;
                                            }

                                            if (baseCalcPauta >= valorAgreg)
                                                prod.TaxationPauta = false;

                                            prod.AliqInterna = taxedP.AliqInterna;
                                            decimal valorAgreAliqInt = calculation.ValorAgregadoAliqInt(Convert.ToDecimal(taxedP.AliqInterna), Convert.ToDecimal(taxedP.Fecop), Convert.ToDecimal(valorAgreg));
                                            decimal valorAgreAliqInt2 = calculation.ValorAgregadoAliqInt(Convert.ToDecimal(taxedP.AliqInterna), Convert.ToDecimal(taxedP.Fecop), Convert.ToDecimal(baseCalcPauta));
                                            prod.ValorAC = valorAgreAliqInt;
                                            prod.ValorAC2 = valorAgreAliqInt2;
                                           
                                            decimal totalIcms = calculation.TotalIcms(valorAgreAliqInt, valorIcms);
                                            decimal totalIcms2 = calculation.TotalIcms(valorAgreAliqInt2, valorIcms);
                                           
                                            if (totalIcms < 0)
                                                totalIcms = 0;

                                            if (totalIcms2 < 0)
                                                totalIcms2 = 0;

                                            prod.TotalICMS = totalIcms;
                                            prod.TotalICMS = totalIcms2;
                                        }

                                        prod.ProductId = product.Id;

                                        if (comp.Incentive && product.Group.Active.Equals(true))
                                            prod.Incentivo = true;

                                        prod.Pautado = true;
                                        prod.TaxationTypeId = taxedP.TaxationTypeId;
                                        prod.EBcr = taxedP.EBcr;
                                        prod.Status = true;
                                        prod.Vbasecalc = baseCalc;
                                        prod.DateStart = Convert.ToDateTime(taxedP.DateStart);
                                        prod.Produto = "Normal";
                                        prod.PercentualInciso = taxedP.PercentualInciso;

                                        qtdTaxation += 1;
                                    }
                                }
                                   
                                det.Clear();
                            }
                            else if (taxed != null && eBcr != taxed.EBcr)
                            {
                                det.Clear();
                            }
                            else
                            {
                                if (comp.Incentive)
                                {
                                    if (comp.Chapter.Name.Equals("CAPÍTULO IV-C") && taxed.PercentualInciso == null)
                                        incentivo = false;
                                    else if (comp.Chapter.Name.Equals("CAPÍTULO IV-C") && taxed.PercentualInciso != null)
                                        incentivo = true;
                                }

                                decimal? valorAgregado = null, valorFecop = null, valorFecop2 = null, valorbcr = null, valorAgreAliqInt = null, valorAgreAliqInt2 = null, 
                                        dif = null, dif_frete = null, icmsApu = null, icmsApuCTe = null, percentualInciso = taxed.PercentualInciso,
                                        mva = null, bcr = null, fecop = null;

                                decimal valorIcms = vICMS + freteIcms, totalIcms = 0, totalIcms2 = 0, baseCalc = 0, baseCalc2 = 0,
                                    aliqInterna = Convert.ToDecimal(taxed.AliqInterna);

                                bool ehBcr = taxed.EBcr, tributoPauta = false, taxationPauta = false;
                                
                                long taxationTypeId = taxed.TaxationTypeId;

                                DateTime dateStart = Convert.ToDateTime(taxed.DateStart), dataRef = new DateTime(2023, 3, 30),
                                         dataTemp = new DateTime(Convert.ToInt32(nota.AnoRef), GetIntMonth(nota.MesRef), 1);

                                if (taxed.TaxationType.Type == "ST")
                                {
                                    baseCalc = baseDeCalc;

                                    code = calculation.CodeP(comp.Document, nota.Cnpj, prod.Cprod, NCM, nota.Uf, pICMSValid.Replace(".", ","));
                                    var taxedP = _taxationPService.FindByCode(taxationsPCompany, code, CEST, nota.Dhemi);

                                    decimal? valorAgreg = null;

                                    if (taxedP == null)
                                    {
                                        if (taxed.MVA != null)
                                        {
                                            mva = taxed.MVA;
                                            valorAgregado = calculation.ValorAgregadoMva(baseCalc, Convert.ToDecimal(mva));
                                            valorAgreg = valorAgregado;
                                        }

                                        if (taxed.EBcr && taxed.BCR != null)
                                        {
                                            bcr = taxed.BCR;
                                            valorbcr = calculation.ValorAgregadoBcr(Convert.ToDecimal(bcr), Convert.ToDecimal(valorAgregado));
                                            valorAgreg = valorbcr;
                                            if (prod.Picms.Equals(12))
                                                valorIcms = (baseCalc * 7 / 100) + prod.IcmsCTe;
                                        }

                                        if (taxed.Fecop != null)
                                        {
                                            fecop = taxed.Fecop;
                                            valorFecop = calculation.ValorFecop(Convert.ToDecimal(fecop), Convert.ToDecimal(valorAgreg));
                                        }

                                        valorAgreAliqInt = calculation.ValorAgregadoAliqInt(aliqInterna, Convert.ToDecimal(fecop), Convert.ToDecimal(valorAgreg));
                                        totalIcms = calculation.TotalIcms(Convert.ToDecimal(valorAgreAliqInt), valorIcms);

                                        if (totalIcms < 0)
                                            totalIcms = 0;

                                        prod.Incentivo = incentivo;
                                        prod.Vbasecalc = baseCalc;
                                        prod.Vbasecalc2 = baseCalc2;
                                        prod.Valoragregado = valorAgregado;
                                        prod.ValorBCR = valorbcr;
                                        prod.ValorAC = valorAgreAliqInt;
                                        prod.TotalICMS = totalIcms;
                                        prod.TotalFecop = valorFecop;
                                        prod.TaxationTypeId = taxationTypeId;
                                        prod.AliqInterna = aliqInterna;
                                        prod.Mva = mva;
                                        prod.Produto = "Normal";
                                        prod.Status = true;
                                        prod.BCR = bcr;
                                        prod.Fecop = fecop;
                                        prod.DateStart = dateStart;
                                        prod.PercentualInciso = percentualInciso;
                                        prod.EBcr = ehBcr;

                                        qtdTaxation += 1;

                                    }
                                    else
                                    {
                                        if (prod.Ucom.ToUpper().Equals("UN") || prod.Ucom.ToUpper().Equals("UND") ||
                                            prod.Ucom.ToUpper().Equals("GF") || prod.Ucom.ToUpper().Equals("GR") || 
                                            prod.Ucom.ToUpper().Equals("QT"))
                                        {
                                            var product = _productService.FindByProduct(products, taxedP.Product, taxedP.GroupId, nota.Dhemi);

                                            if (taxedP.TaxationType.Type == "ST")
                                            {
                                                decimal precoPauta = Convert.ToDecimal(product.Price), totalIcmsPauta = 0,
                                                        quantParaCalc = 0, valorFecopPauta = 0;

                                                quantParaCalc = Convert.ToDecimal(prod.Qcom);

                                                // PP feito com os dados da tabela do Ato Normativo
                                                baseCalc2 = precoPauta * quantParaCalc;

                                                if (taxedP.MVA != null)
                                                {
                                                    mva = taxedP.MVA;
                                                    valorAgregado = calculation.ValorAgregadoMva(baseCalc, Convert.ToDecimal(mva));
                                                    valorAgreg = valorAgregado;
                                                }

                                                if (taxedP.EBcr && taxedP.BCR != null)
                                                {
                                                    bcr = taxedP.BCR;
                                                    valorbcr = calculation.ValorAgregadoBcr(Convert.ToDecimal(bcr), Convert.ToDecimal(valorAgreg));
                                                    valorAgreg = valorbcr;
                                                    if (prod.Picms.Equals(12))
                                                        valorIcms = (baseCalc * 7 / 100) + prod.IcmsCTe;
                                                }

                                                if (taxedP.Fecop != null)
                                                {
                                                    fecop = taxedP.Fecop;
                                                    valorFecop = calculation.ValorFecop(Convert.ToDecimal(fecop), Convert.ToDecimal(valorAgreg));
                                                    valorFecop2 = calculation.ValorFecop(Convert.ToDecimal(fecop), Convert.ToDecimal(baseCalc2));
                                                }

                                                if (baseCalc2 >= valorAgreg)
                                                    taxationPauta = true;

                                                aliqInterna = Convert.ToDecimal(taxedP.AliqInterna);
                                                valorAgreAliqInt = calculation.ValorAgregadoAliqInt(aliqInterna, Convert.ToDecimal(fecop), Convert.ToDecimal(valorAgreg));
                                                valorAgreAliqInt2 = calculation.ValorAgregadoAliqInt(aliqInterna, Convert.ToDecimal(fecop), baseCalc2);

                                                totalIcms = calculation.TotalIcms(Convert.ToDecimal(valorAgreAliqInt), valorIcms);
                                                totalIcms2 = calculation.TotalIcms(Convert.ToDecimal(valorAgreAliqInt2), valorIcms);

                                                if (totalIcms < 0)
                                                    totalIcms = 0;

                                                if (totalIcms2 < 0)
                                                    totalIcms2 = 0;
                                            }


                                            if (comp.Incentive && product.Group.Active.Equals(true))
                                                incentivo = true;

                                            taxationTypeId = taxedP.TaxationTypeId;
                                            ehBcr = taxedP.EBcr;
                                            dateStart = Convert.ToDateTime(taxedP.DateStart);
                                            percentualInciso = taxedP.PercentualInciso;

                                            prod.Pautado = true;
                                            prod.ProductId = product.Id;
                                            prod.Incentivo = incentivo;
                                            prod.Vbasecalc = baseCalc;
                                            prod.Vbasecalc2 = baseCalc2;
                                            prod.Valoragregado = valorAgregado;
                                            prod.ValorBCR = valorbcr;
                                            prod.ValorAC = valorAgreAliqInt;
                                            prod.ValorAC2 = valorAgreAliqInt2;
                                            prod.TotalICMS = totalIcms;
                                            prod.TotalICMS2 = totalIcms2;
                                            prod.TotalFecop = valorFecop;
                                            prod.TotalFecop2 = valorFecop2;
                                            prod.TaxationTypeId = taxationTypeId;
                                            prod.AliqInterna = aliqInterna;
                                            prod.Mva = mva;
                                            prod.Produto = "Normal";
                                            prod.Status = true;
                                            prod.BCR = bcr;
                                            prod.Fecop = fecop;
                                            prod.DateStart = dateStart;
                                            prod.PercentualInciso = percentualInciso;
                                            prod.EBcr = ehBcr;
                                            prod.TaxationPauta = taxationPauta;

                                            qtdTaxation += 1;
                                        }
                                    }
                                }
                                else if (taxed.TaxationType.Type == "Normal" && taxed.TaxationType.Description.Equals("1  AP - Antecipação parcial"))
                                {
                                    baseCalc = baseDeCalc - frete_prod;

                                    dif = calculation.DiferencialAliq(aliqInterna, Convert.ToDecimal(pICMSValid));
                                    dif_frete = calculation.DiferencialAliq(aliqInterna, Convert.ToDecimal(pICMSValid));

                                    if (taxed.EBcr && aliquotConfaz != null && internalAliquotConfaz != null)
                                    {
                                        if(orig == 1 || orig == 2 || orig == 3 || orig == 8)
                                            dif = calculation.DiferencialAliq(Convert.ToDecimal(internalAliquotConfaz), Convert.ToDecimal(pICMSValid));
                                        else
                                            dif = calculation.DiferencialAliq(Convert.ToDecimal(internalAliquotConfaz), Convert.ToDecimal(aliquotConfaz));
                                    }

                                    if (dif < 0)
                                        dif = 0;

                                    if (dif_frete < 0)
                                        dif_frete = 0;

                                    icmsApu = calculation.IcmsApurado(Convert.ToDecimal(dif), baseCalc);
                                    icmsApuCTe = calculation.IcmsApurado(Convert.ToDecimal(dif_frete), frete_prod);

                                    prod.Incentivo = incentivo;
                                    prod.Vbasecalc = baseCalc;
                                    prod.Diferencial = dif;
                                    prod.DiferencialCTe = dif_frete;
                                    prod.IcmsApurado = icmsApu;
                                    prod.IcmsApuradoCTe = icmsApuCTe;
                                    prod.TaxationTypeId = taxationTypeId;
                                    prod.AliqInterna = aliqInterna;
                                    prod.Produto = "Normal";
                                    prod.Status = true;
                                    prod.BCR = bcr;
                                    prod.DateStart = dateStart;
                                    prod.PercentualInciso = percentualInciso;
                                    prod.EBcr = ehBcr;

                                    qtdTaxation += 1;
                                }
                                else if (taxed.TaxationType.Type == "Normal" && !taxed.TaxationType.Description.Equals("1  AP - Antecipação parcial"))
                                {
                                    baseCalc = baseDeCalc - frete_prod;

                                    dif = calculation.DiferencialAliq(aliqInterna, Convert.ToDecimal(pICMSValid));
                                    dif_frete = calculation.DiferencialAliq(aliqInterna, Convert.ToDecimal(pICMSValidOrig));

                                    bcr = taxed.BCR;

                                    if (taxed.EBcr && aliquotConfaz != null && internalAliquotConfaz != null)
                                    {
                                        if (orig == 1 || orig == 2 || orig == 3 || orig == 8)
                                            dif = calculation.DiferencialAliq(Convert.ToDecimal(internalAliquotConfaz), Convert.ToDecimal(pICMSValid));
                                        else
                                            dif = calculation.DiferencialAliq(Convert.ToDecimal(internalAliquotConfaz), Convert.ToDecimal(aliquotConfaz));
                                    }

                                    if (dif < 0)
                                        dif = 0;

                                    if (dif_frete < 0)
                                        dif_frete = 0;

                                    if (dataTemp < dataRef)
                                    {
                                        icmsApu = calculation.IcmsApurado(Convert.ToDecimal(dif), baseCalc);
                                        icmsApuCTe = calculation.IcmsApurado(Convert.ToDecimal(dif_frete), frete_prod);
                                    }
                                    else
                                    {

                                        if (taxed.EBcr && aliquotConfaz == null && internalAliquotConfaz == null)
                                        {
                                            decimal base1 = calculation.Base1(baseCalc, Convert.ToDecimal(pICMSValid)),
                                                    bcrIntra = 0, bcrInter = 100, icmsInter = 0, baseDifal = 0, icmsIntra = 0;

                                            if (taxed.BCR != null)
                                                bcrIntra = Convert.ToDecimal(taxed.BCR);

                                            icmsInter = calculation.IcmsBCR(base1, bcrInter);

                                            if (prod.Csosn != null && prod.Csosn != "")
                                                baseDifal = calculation.Base3(baseCalc - icmsInter, aliqInterna);
                                            else if (vICMS > 0)
                                                baseDifal = calculation.Base3(baseCalc - prod.Vicms, aliqInterna);
                                            else
                                                baseDifal = calculation.Base3(baseCalc, aliqInterna);

                                            icmsIntra = calculation.IcmsBCRIntra(baseDifal, bcrIntra, aliqInterna);
                                            icmsApu = calculation.Icms(icmsIntra, icmsInter);

                                            decimal base1CTe = calculation.Base1(frete_prod, Convert.ToDecimal(pICMSValidOrig)),
                                                    bcrIntraCTe = 100, bcrInterCTe = 100,
                                                    icmsInterCTe = calculation.IcmsBCR(base1CTe, bcrInterCTe),
                                                    baseDifalCTe = 0, icmsIntraCTe = 0;

                                            if (freteIcms > 0)
                                                baseDifalCTe = calculation.Base3(frete_prod - freteIcms, aliqInterna);
                                            else
                                                baseDifalCTe = calculation.Base3(frete_prod, aliqInterna);

                                            icmsIntraCTe = calculation.IcmsBCRIntra(baseDifalCTe, bcrIntraCTe, aliqInterna);
                                            icmsApuCTe = calculation.Icms(icmsIntraCTe, icmsInterCTe);
                                        }
                                        else if (taxed.EBcr && aliquotConfaz != null && internalAliquotConfaz != null)
                                        {
                                            decimal base1 = calculation.Base1(baseCalc, Convert.ToDecimal(pICMSValid)),
                                                    bcrIntra = 0, bcrInter = 100, icmsInter = 0, baseDifal = 0, icmsIntra = 0;

                                            bcrIntra = calculation.BCR(Convert.ToDecimal(internalAliquotConfaz), aliqInterna);
                                            bcrInter = calculation.BCR(Convert.ToDecimal(aliquotConfaz), Convert.ToDecimal(pICMSValid));
                                            icmsInter = calculation.IcmsBCR(base1, bcrInter);

                                            if (prod.Csosn != null && prod.Csosn != "")
                                                baseDifal = calculation.Base3(baseCalc - icmsInter, aliqInterna);
                                            else if (prod.Vicms > 0)
                                                baseDifal = calculation.Base3(baseCalc - prod.Vicms, aliqInterna);
                                            else
                                                baseDifal = calculation.Base3(baseCalc, aliqInterna);

                                            icmsIntra = calculation.IcmsBCRIntra(baseDifal, bcrIntra, aliqInterna);
                                            icmsApu = calculation.Icms(icmsIntra, icmsInter);

                                            decimal base1CTe = calculation.Base1(frete_prod, Convert.ToDecimal(pICMSValid)),
                                                    bcrIntraCTe = 100, bcrInterCTe = 100,
                                                    icmsInterCTe = calculation.IcmsBCR(base1CTe, bcrInterCTe),
                                                    baseDifalCTe = 0, icmsIntraCTe = 0;

                                            if (freteIcms > 0)
                                                baseDifalCTe = calculation.Base3(frete_prod - freteIcms, aliqInterna);
                                            else
                                                baseDifalCTe = calculation.Base3(frete_prod, aliqInterna);

                                            icmsIntraCTe = calculation.IcmsBCRIntra(baseDifalCTe, bcrIntraCTe, aliqInterna);
                                            icmsApuCTe = calculation.Icms(icmsIntraCTe, icmsInterCTe);
                                        }
                                        else
                                        {
                                            decimal base1 = calculation.Base1(baseCalc, Convert.ToDecimal(pICMSValid)),
                                                    base2 = 0, base3 = 0, baseDifal = 0;

                                            if (prod.Csosn != null && prod.Csosn != "")
                                                base2 = calculation.Base2(baseCalc, base1);
                                            else if (prod.Vicms > 0)
                                                base2 = calculation.Base2(baseCalc, prod.Vicms);
                                            else
                                                base2 = baseCalc;

                                            base3 = calculation.Base3(base2, aliqInterna);
                                            baseDifal = calculation.BaseDifal(base3, aliqInterna);

                                            if (comp.County.State.Difal.Equals("Base Única"))
                                                icmsApu = calculation.BaseDifal(base3, Convert.ToDecimal(dif));
                                            else
                                                icmsApu = calculation.Icms(baseDifal, base1);

                                            decimal base1CTe = calculation.Base1(frete_prod, Convert.ToDecimal(pICMSValidOrig)),
                                                base2CTe = 0, base3CTe = 0, baseDifalCTe = 0;

                                            if (freteIcms > 0)
                                                base2CTe = calculation.Base2(frete_prod, freteIcms);
                                            else
                                                base2CTe = frete_prod;

                                            base3CTe = calculation.Base3(base2CTe, aliqInterna);
                                            baseDifalCTe = calculation.BaseDifal(base3CTe, aliqInterna);

                                            if (comp.County.State.Difal.Equals("Base Única"))
                                                icmsApuCTe = calculation.BaseDifal(base3CTe, Convert.ToDecimal(dif_frete));
                                            else
                                                icmsApuCTe = calculation.Icms(baseDifalCTe, base1CTe);
                                        }
                                    }


                                    prod.Incentivo = incentivo;
                                    prod.Vbasecalc = baseCalc;
                                    prod.Diferencial = dif;
                                    prod.DiferencialCTe = dif_frete;
                                    prod.IcmsApurado = icmsApu;
                                    prod.IcmsApuradoCTe = icmsApuCTe;
                                    prod.TaxationTypeId = taxationTypeId;
                                    prod.AliqInterna = aliqInterna;
                                    prod.Produto = "Normal";
                                    prod.Status = true;
                                    prod.BCR = bcr;
                                    prod.DateStart = dateStart;
                                    prod.PercentualInciso = percentualInciso;
                                    prod.EBcr = ehBcr;

                                    qtdTaxation += 1;
                                }
                                else if (taxed.TaxationType.Type == "Isento")
                                {
                                    baseCalc = baseDeCalc;

                                    prod.Incentivo = incentivo;
                                    prod.Vbasecalc = baseCalc;
                                    prod.TaxationTypeId = taxationTypeId;
                                    prod.AliqInterna = aliqInterna;
                                    prod.Mva = mva;
                                    prod.Produto = "Normal";
                                    prod.Status = true;
                                    prod.BCR = bcr;
                                    prod.Fecop = fecop;
                                    prod.DateStart = dateStart;
                                    prod.PercentualInciso = percentualInciso;
                                    prod.EBcr = ehBcr;

                                    qtdTaxation += 1;
                                }
                                else if (taxed.TaxationType.Type == "NT")
                                {
                                    baseCalc = baseDeCalc;

                                    prod.Incentivo = incentivo;
                                    prod.Vbasecalc = baseCalc;
                                    prod.TaxationTypeId = taxationTypeId;
                                    prod.AliqInterna = aliqInterna;
                                    prod.Mva = mva;
                                    prod.Produto = "Normal";
                                    prod.Status = true;
                                    prod.BCR = bcr;
                                    prod.Fecop = fecop;
                                    prod.DateStart = dateStart;
                                    prod.PercentualInciso = percentualInciso;
                                    prod.EBcr = ehBcr;

                                    qtdTaxation += 1;
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

                if (qtd > 0 && qtd.Equals(qtdTaxation))
                {
                    nota.Status = true;
                    nota.Updated = DateTime.Now;
                    updateNote.Add(nota);
                }
            }

            _service.Update(updateNote, GetLog(Model.OccorenceLog.Update));
            _itemService.Create(addProduct, GetLog(Model.OccorenceLog.Create));

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

        private int GetIntMonth(string month)
        {
            switch (month.ToLowerInvariant())
            {
                case "janeiro":
                    return 1;
                case "fevereiro":
                    return 2;
                case "março":
                    return 3;
                case "abril":
                    return 4;
                case "maio":
                    return 5;
                case "junho":
                    return 6;
                case "julho":
                    return 7;
                case "agosto":
                    return 8;
                case "setembro":
                    return 9;
                case "outubro":
                    return 10;
                case "novembro":
                    return 11;
                case "dezembro":
                    return 12;
                default:
                    return 0;
            }
        }
    }
}