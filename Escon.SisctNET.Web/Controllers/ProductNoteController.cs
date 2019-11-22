using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Taxation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class ProductNote : ControllerBaseSisctNET
    {

        private readonly IProductNoteService _service;
        private readonly ITaxationTypeService _taxationTypeService;
        private readonly IProductService _productService;
        private readonly INoteService _noteService;
        private readonly INcmService _ncmService;
        private readonly IStateService _stateService;
        private readonly ITaxationService _taxationService;
        private readonly ICompanyService _companyService;

        public ProductNote(
            IProductNoteService service,
            INoteService noteService,
            INcmService ncmService,
            IProductService productService,
            ITaxationTypeService taxationTypeService,
            IStateService stateService,
            ITaxationService taxationService,
            ICompanyService companyService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "ProductNote")
        {
            _service = service;
            _noteService = noteService;
            _ncmService = ncmService;
            _taxationTypeService = taxationTypeService;
            _productService = productService;
            _stateService = stateService;
            _taxationService = taxationService;
            _companyService = companyService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(int noteId)
        {

            try 
            { 
                var result = _service.FindByNotes(noteId, GetLog(OccorenceLog.Read));
                var rst = _noteService.FindById(noteId, GetLog(OccorenceLog.Read));
                ViewBag.Id = rst.CompanyId;
                ViewBag.Year = rst.AnoRef;
                ViewBag.Month = rst.MesRef;
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Product(int id)
        {
            try
            {
                var result = _service.FindById(id, GetLog(OccorenceLog.Read));
                var note = _noteService.FindByNote(result.Note.Chave);
                var ncm = _ncmService.FindByCode(result.Ncm);
                ViewBag.DescriptionNCM = ncm.Description;
                List<TaxationType> list_taxation = _taxationTypeService.FindAll(GetLog(OccorenceLog.Read));
                
                
                
                list_taxation.Insert(0, new TaxationType() { Description = "Nennhum item selecionado", Id = 0 });
                
                
                SelectList taxationtypes = new SelectList(list_taxation, "Id", "Description", null);
                List<Product> list_product = _productService.FindAll(GetLog(OccorenceLog.Read));
                list_product.Insert(0, new Product() { Description = "Nennhum item selecionado", Id = 0 });
                SelectList products = new SelectList(list_product, "Id", "Description", null);
                ViewBag.ProductId = products;
                ViewBag.TaxationTypeId = taxationtypes;
                ViewBag.Uf = note.Uf;
                ViewBag.Dhemi = note.Dhemi.ToString("dd/MM/yyyy");
                ViewBag.Note = note.Nnf;
                ViewBag.NoteId = note.Id;
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Product(int id, Model.ProductNote entity)
        {
            try
            {
                var rst = _service.FindById(id, GetLog(OccorenceLog.Read));
                var note = _noteService.FindByNote(rst.Note.Chave);
                var calculation = new Calculation();

                string mva = Request.Form["mva"];
                if (mva == "") { mva = null; }
                string fecop = Request.Form["fecop"];
                if (fecop == "") { fecop = null; }
                string bcrForm = Request.Form["bcr"];
                if (bcrForm == "") { bcrForm = null; }
                decimal AliqInt = Convert.ToDecimal(Request.Form["AliqInt"]);
                var taxaType = Request.Form["taxaType"];
                var productid = Request.Form["productid"];
                var quantPauta = Request.Form["quantAlterada"];
                var dateStart = Request.Form["dateStart"];
                decimal valorAgreg = 0, dif = 0;
                decimal valor_fecop = 0;
                string code2 = "";
                var notes = _noteService.FindByUf(note.Company.Id,note.AnoRef,note.MesRef,note.Uf);
                var products = _service.FindByNcmUfAliq(notes,entity.Ncm,entity.Picms);
                if (entity.Pautado == true)
                {
                    products = _service.FindByCnpjCprod(notes, note.Cnpj, rst.Cprod, rst.Ncm, rst.Cest);
                    code2 = note.Cnpj + rst.Cprod + rst.Ncm + rst.Cest;
                }

                var taxedtype = _taxationTypeService.FindById(Convert.ToInt32(taxaType), GetLog(OccorenceLog.Read));
                var product = _productService.FindById(Convert.ToInt32(productid), GetLog(OccorenceLog.Read));
                
                bool pauta = false;

                foreach (var item in products)
                {
                    decimal baseCalc = 0;
                    decimal valor_icms = item.IcmsCTe + item.Vicms;
                    if (taxedtype.Type == "ST")
                    {
                        decimal total_icms_pauta = 0;
                        decimal total_icms = 0;
                        baseCalc = item.Vbasecalc + item.Vdesc;

                        if (entity.Pautado == true)
                        {
                            decimal quantParaCalc = 0;
                            quantParaCalc = Convert.ToDecimal(item.Qcom);
                            if (quantPauta != "")
                            {
                                item.Qpauta = Convert.ToDecimal(quantPauta);
                                quantParaCalc = Convert.ToDecimal(quantPauta);
                            }
                            // Primeiro PP feito pela tabela
                            decimal vAgre = calculation.valorAgregadoPauta(Convert.ToDecimal(quantParaCalc), Convert.ToDecimal(product.Price));

                            // Segundo PP feito com os dados do produto
                            decimal vAgre2 = Convert.ToDecimal(baseCalc / quantParaCalc);
                            if (vAgre2 > vAgre)
                            {
                                vAgre = vAgre2;
                            }

                            if (fecop != null)
                            {
                                item.Fecop = Convert.ToDecimal(fecop);
                                valor_fecop = (Convert.ToDecimal(fecop) / 100) * vAgre;
                            }
                            decimal valorAgreAliqInt = calculation.valorAgregadoAliqInt(AliqInt, Convert.ToDecimal(fecop), vAgre);
                            decimal icms_pauta = valorAgreAliqInt - valor_icms;
                            total_icms_pauta = icms_pauta + valor_fecop;
                        }
                        if (mva != null)
                        {
                            valorAgreg = calculation.ValorAgregadoMva(baseCalc, Convert.ToDecimal(mva));
                            item.Valoragregado = valorAgreg;
                            item.Mva = Convert.ToDecimal(mva);
                        }
                        if (bcrForm != null)
                        {
                            valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(bcrForm), valorAgreg);
                            item.ValorBCR = valorAgreg;
                            item.BCR = Convert.ToDecimal(bcrForm);
                        }
                        if (fecop != null)
                        {
                            item.Fecop = Convert.ToDecimal(fecop);
                            valor_fecop = calculation.valorFecop(Convert.ToDecimal(fecop), valorAgreg);
                            item.TotalFecop = valor_fecop;
                        }
                        item.Aliqinterna = AliqInt;
                        decimal valorAgre_AliqInt = calculation.valorAgregadoAliqInt(Convert.ToDecimal(AliqInt), Convert.ToDecimal(item.Fecop), valorAgreg);
                        item.ValorAC = valorAgre_AliqInt;
                        total_icms = valorAgre_AliqInt;
                        if (bcrForm == null)
                        {
                            total_icms -= valor_icms;
                        }
                        decimal total = Convert.ToDecimal(entity.TotalICMS) + valor_fecop;

                        if (total_icms > total_icms_pauta)
                        {
                            item.TotalICMS = total_icms;
                        }
                        else
                        {
                            item.TotalICMS = total_icms_pauta;
                            pauta = true;
                            item.Pautado = true;
                            item.ProductId = product.Id;
                        }
                    }
                    else if (taxedtype.Type == "Normal")
                    {
                        dif = AliqInt - item.Picms;
                        item.Aliqinterna = AliqInt;
                        baseCalc = item.Vbasecalc;
                        if (note.Crt != "3")
                        {
                            var aliq_simples = _stateService.FindByUf(note.Uf);
                            dif = calculation.diferencialAliq(AliqInt, aliq_simples.Aliquota);
                        }
                        item.Diferencial = dif;
                        decimal icmsApu = (dif / 100) * baseCalc;
                        item.IcmsApurado = icmsApu;
                    }
                    
                    item.TaxationTypeId = Convert.ToInt32(taxaType);
                    item.Updated = DateTime.Now;
                    item.Status = true;
                    item.Vbasecalc = baseCalc;

                    var result = _service.Update(item, GetLog(OccorenceLog.Update));    
                }

                var produtoEspecial = Request.Form["produtoEspecial"];
                if (produtoEspecial != "on" && entity.Pautado == false)
                {
                    var taxation = new Model.Taxation
                    {
                        Code = note.Company + rst.Ncm + note.Uf + rst.Picms,
                        Code2 = code2,
                        Cest = rst.Cest,
                        AliqInterna = Convert.ToDecimal(AliqInt),
                        Diferencial = dif,
                        MVA = entity.Mva,
                        BCR = Convert.ToDecimal(bcrForm),
                        Fecop = entity.Fecop,
                        DateStart = Convert.ToDateTime(dateStart),
                        DateEnd = null,
                        TaxationTypeId = Convert.ToInt32(taxaType),
                        Created = DateTime.Now,
                        Updated = DateTime.Now
                    };
                    _taxationService.Create(entity: taxation, GetLog(OccorenceLog.Create));
                    
                }

                bool status = false;
                var noteTaxation = _service.FindByTaxation(Convert.ToInt32(rst.NoteId));

                if (noteTaxation.Count == 0)
                {
                    status = true;
                }

                note.Status = status;
                _noteService.Update(note, GetLog(OccorenceLog.Update));

                return RedirectToAction("Index", new { noteId = note.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Relatory(int id, int typeTaxation, int type, string year, string month)
        {
            try
            {
                var company = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                var notes = _noteService.FindByNotes(id, year, month);
                var result = _service.FindByProductsType(notes, typeTaxation);
                var notas = result.Select(_ => _.Nnf).Distinct();
                var total = _service.FindByTotal(notas.ToList());
                var notesS = notes.Where(_ => _.Iest == "");
                var notesI = notes.Where(_ => _.Iest != "");

                
                var icmsStnoteS = _service.FindBySubscription(notesS.ToList(), typeTaxation);
                var icmsStnoteI = _service.FindBySubscription(notesI.ToList(), typeTaxation);

                ViewBag.IcmsStNoteS = Math.Round(icmsStnoteS, 2);
                ViewBag.IcmsStNoteI = Math.Round(icmsStnoteI, 2);

                if (type == 1)
                {
                    ViewBag.TaxationType = typeTaxation;
                    ViewBag.Notes = notes;
                    ViewBag.SocialName = company.SocialName;
                    ViewBag.Document = company.Document;
                    ViewBag.Year = year;
                    ViewBag.Month = month;

                    ViewBag.Registro = result.Count();
                    ViewBag.ValorProd = result.Select(_ => _.Vprod).Sum();
                    ViewBag.TotalBC = Math.Round(result.Select(_ => _.Vbasecalc).Sum(), 2);
                    ViewBag.TotalNotas = total;

                    ViewBag.TotalBcICMS = Math.Round(Convert.ToDecimal(result.Select(_ => _.Valoragregado).Sum()), 2);
                    ViewBag.TotalBCR = result.Select(_ => _.ValorBCR).Sum();
                    ViewBag.TotalAC = result.Select(_ => _.ValorAC).Sum();
                    ViewBag.TotalICMSNfe = result.Select(_ => _.Vicms).Sum();
                    ViewBag.TotalICMSCte = Math.Round(result.Select(_ => _.IcmsCTe).Sum(), 2);

                    decimal icmsSt = Math.Round(Convert.ToDecimal(result.Select(_ => _.IcmsST).Sum()), 2);
                    decimal? totalIcms = 0, valorDief = 0; 
                    ViewBag.TotalICMSST = icmsSt;
                    if (typeTaxation == 1)
                    {
                        decimal totalIcmsPauta = Math.Round(Convert.ToDecimal(result.Where(_ => _.Pautado.Equals(true)).Select(_ => _.TotalICMS).Sum()), 2);
                        decimal gnreNPaga = Math.Round(Convert.ToDecimal(notes.Select(_ => _.GnreNPaga).Sum()), 2);
                        ViewBag.TotalGNRE = gnreNPaga;

                        decimal base1 = Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2);
                        base1 += Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);
                        decimal base1fecop = Math.Round(Convert.ToDecimal(notes.Select(_ => _.Fecop1).Sum()), 2);
                        decimal valorbase1 = Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                        ViewBag.base1 = Math.Round(base1, 2);
                        ViewBag.base1fecop = base1fecop;
                        ViewBag.valorbase1 = Math.Round(valorbase1, 2);

                        decimal base2 = Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2);
                        base2 += Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);
                        decimal base2fecop = Math.Round(Convert.ToDecimal(notes.Select(_ => _.Fecop2).Sum()), 2);
                        decimal valorbase2 = Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                        ViewBag.base2 = Math.Round(base2, 2);
                        ViewBag.base2fecop = base2fecop;
                        ViewBag.valorbase2 = Math.Round(valorbase2, 2);

                        ViewBag.TotalBaseFecop = base1fecop + base2fecop;

                        decimal baseNfe1Normal = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                        decimal baseNfe1Ret = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);
                        decimal valorNfe1Normal = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe1Ret = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                        ViewBag.fecopNfe1 = Math.Round(baseNfe1Normal + baseNfe1Ret, 2);
                        ViewBag.valorNfe1 = Math.Round(valorNfe1Normal + valorNfe1Ret, 2);
                        decimal baseNfe2Normal = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2);
                        decimal baseNfe2Ret = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2);
                        decimal valorNfe2Normal = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe2Ret = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                        ViewBag.fecopNfe2 = Math.Round(baseNfe2Normal + baseNfe2Ret, 2);
                        ViewBag.valorNfe2 = Math.Round(valorNfe2Normal + valorNfe2Ret, 2);

                        decimal TotalFecopCalc = valorbase1 + valorbase2;
                        ViewBag.TotalFecopCalculada = Math.Round(TotalFecopCalc, 2);
                        decimal TotalFecopNfe = valorNfe1Normal + valorNfe1Ret + valorNfe2Normal + valorNfe2Ret;
                        ViewBag.TotalFecopNfe = Math.Round(TotalFecopNfe, 2);

                        totalIcms = result.Select(_ => _.TotalICMS).Sum();
                        ViewBag.TotalICMS = totalIcms;
                        ViewBag.TotalICMSSTNota = totalIcms - totalIcmsPauta;                        
                        ViewBag.TotalICMSPauta = totalIcmsPauta;

                        // Valores da dief resumo
                        decimal diefSt = Convert.ToDecimal(totalIcms - icmsSt + gnreNPaga);
                        ViewBag.ValorDief = diefSt;
                        decimal icmsStnota = Math.Round(Convert.ToDecimal(notes.Select(_ => _.IcmsSt).Sum()), 2);
                        ViewBag.IcmsSt = icmsStnota;
                        ViewBag.IcmsPagar = diefSt - icmsStnota;

                        // Valores da dief fecop
                        ViewBag.DifBase1 = Math.Round(base1 - baseNfe1Normal - baseNfe1Ret, 2);
                        ViewBag.DifValor1 = Math.Round(valorbase1 - valorNfe1Normal - valorNfe1Ret, 2);
                        ViewBag.DifBase2 = Math.Round(base2 - baseNfe2Normal - baseNfe2Ret, 2);
                        ViewBag.DifValor2 = Math.Round(valorbase2 - valorNfe2Normal - valorNfe2Ret, 2);
                        ViewBag.DifTotal = Math.Round(TotalFecopCalc - TotalFecopNfe - (base1fecop + base2fecop), 2);                        

                    }
                    else if (typeTaxation >= 2 && typeTaxation <= 5)
                    {
                        totalIcms = result.Select(_ => _.IcmsApurado).Sum();
                        ViewBag.TotalICMS = totalIcms;
                        valorDief = totalIcms - icmsSt;
                        decimal? icmsAp = notes.Select(_ => _.IcmsAp).Sum();
                        ViewBag.ValorDief = valorDief;
                        ViewBag.IcmsAp = icmsAp;
                        ViewBag.IcmsPagar = valorDief - icmsAp;
                    }
                    
                    ViewBag.TotalFecop = result.Select(_ => _.TotalFecop).Sum();
                    ViewBag.TotalFrete = result.Select(_ => _.Freterateado).Sum();
                    ViewBag.TotalIpi = result.Select(_ => _.Vipi).Sum();
                }
                else if (type == 2)
                {
                    var teste = result.GroupBy(_ => _.Nnf);
                }
                return View(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
           
    }
}