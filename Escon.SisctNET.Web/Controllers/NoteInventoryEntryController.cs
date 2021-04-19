﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class NoteInventoryEntryController : ControllerBaseSisctNET
    {
        private readonly INoteInventoryEntryService _service;
        private readonly IProductNoteInventoryEntryService _itemService;
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;

        public NoteInventoryEntryController(
            INoteInventoryEntryService service,
            IProductNoteInventoryEntryService itemService,
            ICompanyService companyService,
            IConfigurationService configurationService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Note")
        {
            _service = service;
            _itemService = itemService;
            _companyService = companyService;
            _configurationService = configurationService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(int id, string year, string month)
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

                return View(null);
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
                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
        
        public IActionResult GetAll(int draw, int start)
        {

            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var notasAll = _service.FindByNotes(SessionManager.GetCompanyIdInSession(), SessionManager.GetYearInSession(), SessionManager.GetMonthInSession())
                        .OrderBy(_ => Convert.ToInt64(_.Nnf)).ToList();


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<NoteInventoryEntry> notes = new List<NoteInventoryEntry>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<NoteInventoryEntry> notesTemp = new List<NoteInventoryEntry>();
                notasAll.ToList().ForEach(s =>
                {
                    s.Xnome = Helpers.CharacterEspecials.RemoveDiacritics(s.Xnome);
                    notesTemp.Add(s);
                });

                var ids = notesTemp.Where(c =>
                    c.Xnome.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Nnf.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                notes = notasAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var note = from r in notes
                              where ids.ToArray().Contains(r.Id)
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  nota = r.Nnf,
                                  fornecedor = r.Xnome,
                                  valor = r.Vnf,

                              };

                return Ok(new { draw = draw, recordsTotal = notes.Count(), recordsFiltered = notes.Count(), data = note.Skip(start).Take(lenght) });

            }
            else
            {

                var notes = from r in notasAll
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  nota = r.Nnf,
                                  fornecedor = r.Xnome,
                                  valor = r.Vnf,

                              };
                return Ok(new { draw = draw, recordsTotal = notes.Count(), recordsFiltered = notes.Count(), data = notes.Skip(start).Take(lenght) });
            }

        }

        public JsonResult Import()
        {
            int id = SessionManager.GetCompanyIdInSession();
            string year = SessionManager.GetYearInSession();
            string month = SessionManager.GetMonthInSession();

            var comp = _companyService.FindById(id, null);
            var confDBSisctNfe = _configurationService.FindByName("NFe", null);
            var confDBSisctCte = _configurationService.FindByName("CTe", null);

            var importXml = new Xml.Import();
            var importDir = new Diretorio.Import();
            var calculation = new Taxation.Calculation();

            string directoryNfe = importDir.Entrada(comp, confDBSisctNfe.Value, year, month);
            string directotyCte = importDir.Entrada(comp, confDBSisctCte.Value, year, month);

            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

            notes = importXml.NFeAll(directoryNfe, directotyCte);

            Dictionary<string, string> det = new Dictionary<string, string>();

            int erro = 0;
            string url = "Index", chave = "";


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

                var notaImport = _service.FindByNote(notes[i][0]["chave"]);

                if (notaImport == null)
                {
                    try
                    {
                        string nCT = notes[i][notes[i].Count() - 1].ContainsKey("nCT") ? notes[i][notes[i].Count() - 1]["nCT"] : "";

                        string IEST = notes[i][2].ContainsKey("IEST") ? notes[i][2]["IEST"] : "";

                        string cnpj = notes[i][2].ContainsKey("CNPJ") ? notes[i][2]["CNPJ"] : notes[i][2]["CPF"];

                        Model.NoteInventoryEntry note = new Model.NoteInventoryEntry();

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


                var nota = _service.FindByNote(notes[i][0]["chave"]);

                var produtos = _itemService.FindByNote(nota.Id);

                List<Model.ProductNoteInventoryEntry> addProduct = new List<Model.ProductNoteInventoryEntry>();

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

                    if (notes[i][j].ContainsKey("vICMS") && notes[i][j].ContainsKey("CST"))
                        det.Add("vICMS", notes[i][j]["vICMS"]);

                    if (notes[i][j].ContainsKey("orig"))
                        det.Add("orig", notes[i][j]["orig"]);

                    if (notes[i][j].ContainsKey("pICMS"))
                        det.Add("pICMS", notes[i][j]["pICMS"]);

                    if (notes[i][j].ContainsKey("vIPI") && notes[i][j].ContainsKey("CST"))
                        det.Add("vIPI", notes[i][j]["vIPI"]);

                    if (notes[i][j].ContainsKey("vICMSST") && notes[i][j].ContainsKey("CST"))
                        det.Add("vICMSST", notes[i][j]["vICMSST"]);

                    if (notes[i][j].ContainsKey("vBCST") && notes[i][j].ContainsKey("CST"))
                        det.Add("vBCST", notes[i][j]["vBCST"]);

                    if (notes[i][j].ContainsKey("vBCFCPST") && notes[i][j].ContainsKey("CST"))
                        det.Add("vBCFCPST", notes[i][j]["vBCFCPST"]);

                    if (notes[i][j].ContainsKey("pFCPST") && notes[i][j].ContainsKey("CST"))
                        det.Add("pFCPST", notes[i][j]["pFCPST"]);

                    if (notes[i][j].ContainsKey("vFCPST") && notes[i][j].ContainsKey("CST"))
                        det.Add("vFCPST", notes[i][j]["vFCPST"]);

                    if (notes[i][j].ContainsKey("vBCFCPSTRet") && notes[i][j].ContainsKey("CST"))
                        det.Add("vBCFCPSTRet", notes[i][j]["vBCFCPSTRet"]);

                    if (notes[i][j].ContainsKey("pFCPSTRet") && notes[i][j].ContainsKey("CST"))
                        det.Add("pFCPSTRet", notes[i][j]["pFCPSTRet"]);

                    if (notes[i][j].ContainsKey("vFCPSTRet") && notes[i][j].ContainsKey("CST"))
                        det.Add("vFCPSTRet", notes[i][j]["vFCPSTRet"]);

                    if (notes[i][j].ContainsKey("vPIS") && notes[i][j].ContainsKey("CST"))
                        det.Add("vPIS", notes[i][j]["vPIS"]);

                    if (notes[i][j].ContainsKey("vCOFINS") && notes[i][j].ContainsKey("CST"))
                        det.Add("vCOFINS", notes[i][j]["vCOFINS"]);

                    if (notes[i][j].ContainsKey("frete_prod"))
                        det.Add("frete_prod", notes[i][j]["frete_prod"]);

                    if (notes[i][j].ContainsKey("frete_icms"))
                        det.Add("frete_icms", notes[i][j]["frete_icms"]);

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

                            Model.ProductNoteInventoryEntry prod = new Model.ProductNoteInventoryEntry();

                            decimal baseDeCalc = calculation.BaseCalc(Convert.ToDecimal(det["vProd"]), vFrete, vSeg, vOutro, vDesc, vIPI, frete_prod);
        
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
                                prod.Vipi = vIPI;
                                prod.Vfrete = vFrete;
                                prod.Vseg = vSeg;
                                prod.Voutro = vOutro;
                                prod.Vdesc = vDesc;
                                prod.Freterateado = frete_prod;
                                prod.NoteInventoryEntryId = nota.Id;
                                prod.Nitem = det["nItem"];
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

    }
}
