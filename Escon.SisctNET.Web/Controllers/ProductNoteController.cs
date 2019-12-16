using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Taxation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                ViewBag.Note = rst.Nnf;
                ViewBag.Fornecedor = rst.Xnome;
                ViewBag.Valor = rst.Vnf;
                ViewBag.View = rst.View;
                ViewBag.NoteId = rst.Id;

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
                foreach (var prod in list_product)
                {
                    prod.Description = prod.Code + " - " + prod.Price + " - " + prod.Description;
                }
                list_product.Insert(0, new Product() { Description = "Nennhum item selecionado", Id = 0 });
                SelectList products = new SelectList(list_product, "Id", "Description", null);
                ViewBag.ProductId = products;
                ViewBag.TaxationTypeId = taxationtypes;
                ViewBag.Uf = note.Uf;
                ViewBag.Dhemi = note.Dhemi.ToString("dd/MM/yyyy");
                ViewBag.Note = note.Nnf;
                ViewBag.NoteId = note.Id;
                if (result.TaxationTypeId == null)
                {
                    result.TaxationTypeId = 0;
                }
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

                /*if (entity.Pautado == true)
                {
                    products = _service.FindByCnpjCprod(notes, note.Cnpj, rst.Cprod, rst.Ncm, rst.Cest);
                    code2 = note.Company.Document + note.Cnpj + rst.Cprod + rst.Ncm + rst.Cest;
                }*/

                var taxedtype = _taxationTypeService.FindById(Convert.ToInt32(taxaType), GetLog(OccorenceLog.Read));
                var product = _productService.FindById(Convert.ToInt32(productid), GetLog(OccorenceLog.Read));

                if (entity.Pautado == true)
                {
                    var p = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                    decimal baseCalc = 0;
                    decimal valor_icms = p.IcmsCTe + p.Vicms;

                    if (taxedtype.Type == "ST")
                    {
                        decimal total_icms_pauta = 0;
                        decimal total_icms = 0;
                        baseCalc = p.Vbasecalc + p.Vdesc;

                        if (entity.Pautado == true)
                        {
                            decimal quantParaCalc = 0;
                            quantParaCalc = Convert.ToDecimal(p.Qcom);
                            if (quantPauta != "")
                            {
                                p.Qpauta = Convert.ToDecimal(quantPauta);
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
                                p.Fecop = Convert.ToDecimal(fecop);
                                valor_fecop = (Convert.ToDecimal(fecop) / 100) * vAgre;
                            }
                            decimal valorAgreAliqInt = calculation.valorAgregadoAliqInt(AliqInt, Convert.ToDecimal(fecop), vAgre);
                            decimal icms_pauta = valorAgreAliqInt - valor_icms;
                            total_icms_pauta = icms_pauta + valor_fecop;
                        }
                        if (mva != null)
                        {
                            valorAgreg = calculation.ValorAgregadoMva(baseCalc, Convert.ToDecimal(mva));
                            p.Valoragregado = valorAgreg;
                            p.Mva = Convert.ToDecimal(mva);
                        }
                        if (bcrForm != null)
                        {
                            valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(bcrForm), valorAgreg);
                            p.ValorBCR = valorAgreg;
                            p.BCR = Convert.ToDecimal(bcrForm);
                        }
                        if (fecop != null)
                        {
                            p.Fecop = Convert.ToDecimal(fecop);
                            valor_fecop = calculation.valorFecop(Convert.ToDecimal(fecop), valorAgreg);
                            p.TotalFecop = valor_fecop;
                        }
                        p.Aliqinterna = AliqInt;
                        decimal valorAgre_AliqInt = calculation.valorAgregadoAliqInt(Convert.ToDecimal(AliqInt), Convert.ToDecimal(p.Fecop), valorAgreg);
                        p.ValorAC = valorAgre_AliqInt;
                        total_icms = valorAgre_AliqInt;
                        if (bcrForm == null)
                        {
                            total_icms -= valor_icms;
                        }
                        decimal total = Convert.ToDecimal(entity.TotalICMS) + valor_fecop;

                        if (total_icms > total_icms_pauta)
                        {
                            p.TotalICMS = total_icms;
                        }
                        else
                        {
                            p.TotalICMS = total_icms_pauta;
                            p.Pautado = true;
                            p.ProductId = product.Id;
                        }

                    }

                    p.TaxationTypeId = Convert.ToInt32(taxaType);
                    p.Updated = DateTime.Now;
                    p.Status = true;
                    p.Vbasecalc = baseCalc;

                    _service.Update(p, GetLog(Model.OccorenceLog.Read));
                }
                else
                {
                    if (products == null)
                    {
                        var prod = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                        decimal baseCalc = 0;
                        decimal valor_icms = prod.IcmsCTe + prod.Vicms;
                        if (taxedtype.Type == "ST")
                        {
                            decimal total_icms = 0;
                            baseCalc = prod.Vbasecalc + prod.Vdesc;

                            if (mva != null)
                            {
                                valorAgreg = calculation.ValorAgregadoMva(baseCalc, Convert.ToDecimal(mva));
                                prod.Valoragregado = valorAgreg;
                                prod.Mva = Convert.ToDecimal(mva);
                            }
                            if (bcrForm != null)
                            {
                                valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(bcrForm), valorAgreg);
                                prod.ValorBCR = valorAgreg;
                                prod.BCR = Convert.ToDecimal(bcrForm);
                            }
                            if (fecop != null)
                            {
                                prod.Fecop = Convert.ToDecimal(fecop);
                                valor_fecop = calculation.valorFecop(Convert.ToDecimal(fecop), valorAgreg);
                                prod.TotalFecop = valor_fecop;
                            }
                            prod.Aliqinterna = AliqInt;
                            decimal valorAgre_AliqInt = calculation.valorAgregadoAliqInt(Convert.ToDecimal(AliqInt), Convert.ToDecimal(prod.Fecop), valorAgreg);
                            prod.ValorAC = valorAgre_AliqInt;
                            total_icms = valorAgre_AliqInt;
                            if (bcrForm == null)
                            {
                                total_icms -= valor_icms;
                            }
                            decimal total = Convert.ToDecimal(entity.TotalICMS) + valor_fecop;

                            prod.TotalICMS = total_icms;

                        }
                        else if (taxedtype.Type == "Normal")
                        {
                            dif = AliqInt - prod.Picms;
                            prod.Aliqinterna = AliqInt;
                            baseCalc = prod.Vbasecalc;
                            if (prod.Picms != 4)
                            {
                                var aliq_simples = _stateService.FindByUf(note.Uf);
                                dif = calculation.diferencialAliq(AliqInt, aliq_simples.Aliquota);
                                prod.Picms = Convert.ToDecimal(aliq_simples.Aliquota);
                            }
                            prod.Diferencial = dif;
                            decimal icmsApu = (dif / 100) * baseCalc;
                            prod.IcmsApurado = icmsApu;
                        }

                        prod.TaxationTypeId = Convert.ToInt32(taxaType);
                        prod.Updated = DateTime.Now;
                        prod.Status = true;
                        prod.Vbasecalc = baseCalc;

                        var result = _service.Update(prod, GetLog(OccorenceLog.Update));
                    }
                    else
                    {
                        foreach (var item in products)
                        {
                            decimal baseCalc = 0;
                            decimal valor_icms = item.IcmsCTe + item.Vicms;
                            if (taxedtype.Type == "ST")
                            {
                                decimal total_icms = 0;
                                baseCalc = item.Vbasecalc + item.Vdesc;

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

                                item.TotalICMS = total_icms;

                            }
                            else if (taxedtype.Type == "Normal")
                            {
                                dif = AliqInt - item.Picms;
                                item.Aliqinterna = AliqInt;
                                baseCalc = item.Vbasecalc;
                                if (item.Picms != 4)
                                {
                                    var aliq_simples = _stateService.FindByUf(note.Uf);
                                    dif = calculation.diferencialAliq(AliqInt, aliq_simples.Aliquota);
                                    item.Picms = Convert.ToDecimal(aliq_simples.Aliquota);
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
                    }
                }

              

                if (Request.Form["produto"].ToString() == "1" && entity.Pautado == false)
                {
                    decimal? bcr = null;
                    if (!bcrForm.Equals(null))
                    {
                        bcr = Convert.ToDecimal(bcrForm);
                    }
                    var ncm = _ncmService.FindByCode(rst.Ncm);

                    var taxation = new Model.Taxation
                    {
                        CompanyId = note.CompanyId,
                        Code = note.Company.Document + rst.Ncm + note.Uf + rst.Picms,
                        Code2 = code2,
                        Cest = rst.Cest,
                        AliqInterna = Convert.ToDecimal(AliqInt),
                        Diferencial = dif,
                        MVA = entity.Mva,
                        BCR = bcr,
                        Fecop = entity.Fecop,
                        DateStart = Convert.ToDateTime(dateStart),
                        DateEnd = null,
                        TaxationTypeId = Convert.ToInt32(taxaType),
                        Created = DateTime.Now,
                        Updated = DateTime.Now,
                        NcmId = ncm.Id,
                        Picms = rst.Picms,
                        Uf = note.Uf

                    };
                    _taxationService.Create(entity: taxation, GetLog(OccorenceLog.Create));
                    
                }

                foreach (var prod in products)
                {
                    bool status = false;
                    var nota = _noteService.FindById(Convert.ToInt32(prod.NoteId), GetLog(OccorenceLog.Read));

                    var productTaxation = _service.FindByTaxation(Convert.ToInt32(nota.Id));

                    if (productTaxation.Count == 0)
                    {
                        status = true;
                    }

                    nota.Status = status;

                    _noteService.Update(nota, GetLog(OccorenceLog.Update));
                }

                return RedirectToAction("Index", new { noteId = note.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Relatory(int id, int typeTaxation, int type, string year, string month, string nota)
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

                ViewBag.IcmsStNoteS = Convert.ToDouble(Math.Round(icmsStnoteS, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                ViewBag.IcmsStNoteI = Convert.ToDouble(Math.Round(icmsStnoteI, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                ViewBag.SocialName = company.SocialName;
                ViewBag.Document = company.Document;
                ViewBag.Year = year;
                ViewBag.Month = month;
                ViewBag.TaxationType = typeTaxation;

                    
                if (type == 1 || type == 2)
                {    
                    if (type == 2)
                    {
                        notes = notes.Where(_ => _.Nnf.Equals(nota)).ToList();
                        result = _service.FindByProductsType(notes, typeTaxation);
                    }
                    
                    ViewBag.Notes = notes;


                    ViewBag.Registro = result.Count();
                    ViewBag.ValorProd = Convert.ToDouble(result.Select(_ => _.Vprod).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalBC = Convert.ToDouble(Math.Round(result.Select(_ => _.Vbasecalc).Sum(), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalNotas = Convert.ToDouble(total).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    ViewBag.TotalBcICMS = Convert.ToDouble(Math.Round(Convert.ToDecimal(result.Select(_ => _.Valoragregado).Sum()), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalBCR = Convert.ToDouble(result.Select(_ => _.ValorBCR).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAC = Convert.ToDouble(result.Select(_ => _.ValorAC).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalICMSNfe = Convert.ToDouble(result.Select(_ => _.Vicms).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalICMSCte = Convert.ToDouble(Math.Round(result.Select(_ => _.IcmsCTe).Sum(), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal icmsSt = Math.Round(Convert.ToDecimal(result.Select(_ => _.IcmsST).Sum()), 2);
                    decimal? totalIcms = 0, valorDief = 0; 
                    ViewBag.TotalICMSST = Convert.ToDouble(icmsSt).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    if (typeTaxation == 1 || typeTaxation == 7)
                    {
                        decimal totalIcmsPauta = Math.Round(Convert.ToDecimal(result.Where(_ => _.Pautado.Equals(true)).Select(_ => _.TotalICMS).Sum()), 2);
                        decimal totalIcmsMva = Math.Round(Convert.ToDecimal(result.Where(_ => _.Pautado.Equals(false)).Select(_ => _.TotalICMS).Sum()), 2);
                        decimal gnreNPaga = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreNPaga).Distinct().Sum()), 2);
                        decimal gnrePaga = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreSt).Distinct().Sum()), 2);
                        ViewBag.TotalGNREnPaga = Convert.ToDouble(gnreNPaga).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGNREPaga = Convert.ToDouble(gnrePaga).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal base1 = Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2);
                        base1 += Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);
                        decimal base1fecop = Math.Round(Convert.ToDecimal(notes.Select(_ => _.Fecop1).Sum()), 2);
                        decimal valorbase1 = Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                        ViewBag.base1 = Convert.ToDouble(Math.Round(base1, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.base1fecop = Convert.ToDouble(base1fecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.valorbase1 = Convert.ToDouble(Math.Round(valorbase1, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal base2 = Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2);
                        base2 += Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);
                        decimal base2fecop = Math.Round(Convert.ToDecimal(notes.Select(_ => _.Fecop2).Sum()), 2);
                        decimal valorbase2 = Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                        ViewBag.base2 = Convert.ToDouble(Math.Round(base2, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.base2fecop = Convert.ToDouble(base2fecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.valorbase2 = Convert.ToDouble(Math.Round(valorbase2, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.TotalBaseFecop = Convert.ToDouble(base1fecop + base2fecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal baseNfe1Normal = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                        decimal baseNfe1Ret = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);
                        decimal valorNfe1Normal = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe1Ret = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                        ViewBag.fecopNfe1 = Convert.ToDouble(Math.Round(baseNfe1Normal + baseNfe1Ret, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.valorNfe1 = Convert.ToDouble(Math.Round(valorNfe1Normal + valorNfe1Ret, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        decimal baseNfe2Normal = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2);
                        decimal baseNfe2Ret = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2);
                        decimal valorNfe2Normal = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe2Ret = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                        ViewBag.fecopNfe2 = Convert.ToDouble(Math.Round(baseNfe2Normal + baseNfe2Ret, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.valorNfe2 = Convert.ToDouble(Math.Round(valorNfe2Normal + valorNfe2Ret, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal gnreNPagaFecop = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreFecop).Sum()), 2);
                        ViewBag.GNREnPagaFecop = Convert.ToDouble(gnreNPagaFecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal gnrePagaFecop1 = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.FecopGnre1).Sum()), 2);
                        ViewBag.GNREPagaFecop1 = Convert.ToDouble(gnrePagaFecop1).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal gnrePagaFecop2 = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.FecopGnre2).Sum()), 2);
                        ViewBag.GNREPagaFecop2 = Convert.ToDouble(gnrePagaFecop2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal totalGnreFecop = gnrePagaFecop1 + gnrePagaFecop2;
                        ViewBag.TotalGnreFecop = Convert.ToDouble(totalGnreFecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal TotalFecopCalc = valorbase1 + valorbase2;
                        ViewBag.TotalFecopCalculada = Convert.ToDouble(Math.Round(TotalFecopCalc, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        decimal TotalFecopNfe = valorNfe1Normal + valorNfe1Ret + valorNfe2Normal + valorNfe2Ret;
                        ViewBag.TotalFecopNfe = Convert.ToDouble(Math.Round(TotalFecopNfe, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        totalIcms = result.Select(_ => _.TotalICMS).Sum();

                        ViewBag.TotalICMS = Convert.ToDouble(totalIcms).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.TotalICMSSTNota = Convert.ToDouble(totalIcms - totalIcmsPauta).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");                        
                        ViewBag.TotalICMSPauta = Convert.ToDouble(totalIcmsPauta).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalICMSMva = Convert.ToDouble(totalIcmsMva).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        // Valores da dief resumo
                        decimal diefSt = Convert.ToDecimal(totalIcms - icmsSt + gnreNPaga - gnrePaga);
                        ViewBag.ValorDief = Convert.ToDouble(diefSt).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        decimal icmsStnota = Math.Round(Convert.ToDecimal(notes.Select(_ => _.IcmsSt).Sum()), 2);
                        ViewBag.IcmsSt = Convert.ToDouble(icmsStnota).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsPagar = Convert.ToDouble(diefSt - icmsStnota).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        // Valores da dief fecop
                        ViewBag.DifBase1 = Convert.ToDouble(Math.Round(base1 - baseNfe1Normal - baseNfe1Ret, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        decimal difvalor1 = valorbase1 - valorNfe1Normal - valorNfe1Ret - gnrePagaFecop1;
                        ViewBag.DifValor1 = Convert.ToDouble(Math.Round(difvalor1, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DifBase2 = Convert.ToDouble(Math.Round(base2 - baseNfe2Normal - baseNfe2Ret, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        decimal difvalor2 = valorbase2 - valorNfe2Normal - valorNfe2Ret - gnrePagaFecop2;
                        ViewBag.DifValor2 = Convert.ToDouble(Math.Round(difvalor2, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        decimal diftotal = difvalor1 + difvalor2;
                        ViewBag.DifTotal = Convert.ToDouble(Math.Round(diftotal, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                        decimal totalfecop1 = difvalor1 - base1fecop;
                        ViewBag.TotalFecop1 = Convert.ToDouble(Math.Round(totalfecop1, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        decimal totalfecop2 = difvalor2 - base2fecop;
                        ViewBag.TotalFecop2 = Convert.ToDouble(Math.Round(totalfecop2, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFinalFecopCalculada = Convert.ToDouble(Math.Round(totalfecop1 + totalfecop2, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.Incetive = company.Incentive;

                        //Relatorio das empresas incentivadas
                        if (company.Incentive == true)
                        {
                            //Produtos não incentivados
                            var productsNormal = _service.FindByNormal(notes);
                            decimal? totalIcmsNormal = productsNormal.Where(_ => _.TaxationType.Type.Equals("ST")).Select(_ => _.TotalICMS).Sum();

                            icmsSt = Math.Round(Convert.ToDecimal(productsNormal.Select(_ => _.IcmsST).Sum()), 2);
                            ViewBag.TotalICMSST = Convert.ToDouble(icmsSt).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            gnreNPaga = Math.Round(Convert.ToDecimal(productsNormal.Select(_ => _.Note.GnreNPaga).Distinct().Sum()), 2);
                            gnrePaga = Math.Round(Convert.ToDecimal(productsNormal.Select(_ => _.Note.GnreSt).Distinct().Sum()), 2);
                            ViewBag.TotalGNREnPaga = Convert.ToDouble(gnreNPaga).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGNREPaga = Convert.ToDouble(gnrePaga).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            diefSt = Convert.ToDecimal(totalIcmsNormal - icmsSt + gnreNPaga);
                            ViewBag.ValorDief = Convert.ToDouble(diefSt).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            decimal? IcmsMva = productsNormal.Where(_ => _.Pautado.Equals(false) && _.TaxationType.Type.Equals("ST")).Select(_ => _.TotalICMS).Sum();
                            decimal? IcmsPauta = productsNormal.Where(_ => _.Pautado.Equals(true) && _.TaxationType.Type.Equals("ST")).Select(_ => _.TotalICMS).Sum();
                            ViewBag.TotalICMSMva = Convert.ToDouble(IcmsMva).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalICMSPauta = Convert.ToDouble(IcmsPauta).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            icmsStnota = Math.Round(Convert.ToDecimal(productsNormal.Select(_ => _.Note.IcmsSt).Distinct().Sum()), 2);

                            ViewBag.IcmsPagar = Convert.ToDouble(diefSt - icmsStnota).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            base1 = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2);
                            base1 += Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);
                            ViewBag.base1 = Convert.ToDouble(Math.Round(base1, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            base2 = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2);
                            base2 += Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);
                            ViewBag.base2 = Convert.ToDouble(Math.Round(base2, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            valorbase1 = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                            ViewBag.valorbase1 = Convert.ToDouble(Math.Round(valorbase1, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            valorbase2 = Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                            ViewBag.valorbase2 = Convert.ToDouble(Math.Round(valorbase2, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            ViewBag.TotalBaseFecop = Convert.ToDouble(base1fecop + base2fecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            baseNfe1Normal = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                            baseNfe1Ret = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);
                            valorNfe1Normal = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                            valorNfe1Ret = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                            ViewBag.fecopNfe1 = Convert.ToDouble(Math.Round(baseNfe1Normal + baseNfe1Ret, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.valorNfe1 = Convert.ToDouble(Math.Round(valorNfe1Normal + valorNfe1Ret, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            baseNfe2Normal = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2);
                            baseNfe2Ret = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2);
                            valorNfe2Normal = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                            valorNfe2Ret = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                            ViewBag.fecopNfe2 = Convert.ToDouble(Math.Round(baseNfe2Normal + baseNfe2Ret, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.valorNfe2 = Convert.ToDouble(Math.Round(valorNfe2Normal + valorNfe2Ret, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            gnreNPagaFecop = Math.Round(Convert.ToDecimal(productsNormal.Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2);
                            ViewBag.GNREnPagaFecop = Convert.ToDouble(gnreNPagaFecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            gnrePagaFecop1 = Math.Round(Convert.ToDecimal(productsNormal.Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2);
                            ViewBag.GNREPagaFecop1 = Convert.ToDouble(gnrePagaFecop1).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            gnrePagaFecop2 = Math.Round(Convert.ToDecimal(productsNormal.Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2);
                            ViewBag.GNREPagaFecop2 = Convert.ToDouble(gnrePagaFecop2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            totalGnreFecop = gnrePagaFecop1 + gnrePagaFecop2;
                            ViewBag.TotalGnreFecop = Convert.ToDouble(totalGnreFecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            TotalFecopCalc = valorbase1 + valorbase2;
                            ViewBag.TotalFecopCalculada = Convert.ToDouble(Math.Round(TotalFecopCalc, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            TotalFecopNfe = valorNfe1Normal + valorNfe1Ret + valorNfe2Normal + valorNfe2Ret;
                            ViewBag.TotalFecopNfe = Convert.ToDouble(Math.Round(TotalFecopNfe, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            totalIcms = productsNormal.Select(_ => _.TotalICMS).Sum();

                            ViewBag.TotalICMS = Convert.ToDouble(totalIcms).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            //Produto incentivados
                            var productsIncentive = _service.FindByIncentive(notes);

                            ViewBag.Icms = company.Icms;
                            ViewBag.Fecop = company.Fecop;

                            decimal baseIcms = productsIncentive.Select(_ => _.Vbasecalc).Sum();
                            ViewBag.Base = Convert.ToDouble(Math.Round(baseIcms,2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            decimal impostoIcms = Convert.ToDecimal(baseIcms * (company.Icms / 100));
                            ViewBag.ImpostoIcms = Convert.ToDouble(Math.Round(impostoIcms, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            decimal impostoFecop = Convert.ToDecimal(baseIcms * (company.Fecop / 100));
                            ViewBag.ImpostoFecop = Convert.ToDouble(Math.Round(impostoFecop, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");                         
                           

                            decimal? basefunef = totalIcms - impostoIcms;
                            ViewBag.BaseFunef = Convert.ToDouble(Math.Round(Convert.ToDecimal(basefunef), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.Funef = company.Funef;
                            decimal taxaFunef = Convert.ToDecimal(basefunef * (company.Funef / 100));
                            ViewBag.TaxaFunef = Convert.ToDouble(Math.Round(taxaFunef, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            decimal totalImpostoIncentivo = impostoIcms + impostoFecop + taxaFunef;
                            ViewBag.TotalImpostoIncentivo = Convert.ToDouble(Math.Round(totalImpostoIncentivo,2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            /*var notesDentro = notes.Where(_ => _.IdDest.Equals(1)).ToList();
                            var prodructsCfopIn = _service.FindByCfopNotesIn(company.Id, notesDentro);
                            decimal basedeCalcIncentivo = prodructsCfopIn.Select(_ => _.Vbasecalc).Sum();
                            var notesFora = notes.Where(_ => _.IdDest.Equals(1)).ToList();
                            var profucsCfopOut = _service.FindByCfopNotesOut(company.Id, notesFora);*/
                        }

                    }
                    else if (typeTaxation >= 2 && typeTaxation <= 5)
                    {
                        totalIcms = result.Select(_ => _.IcmsApurado).Sum();

                        decimal gnreNPaga = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreNPaga).Distinct().Sum()), 2);
                        decimal gnrePaga = 0;

                        if(typeTaxation == 2)
                        {
                            gnrePaga = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreAp).Distinct().Sum()), 2);
                        }else if (typeTaxation == 3)
                        {
                            gnrePaga = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreCo).Distinct().Sum()), 2);
                        }else if (typeTaxation == 5)
                        {
                            gnrePaga = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreIm).Distinct().Sum()), 2);
                        }

                        ViewBag.TotalGNREnPaga = Convert.ToDouble(gnreNPaga).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGNREPaga = Convert.ToDouble(gnrePaga).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    
                        ViewBag.TotalICMS = Convert.ToDouble(totalIcms).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        valorDief = Convert.ToDecimal(totalIcms - icmsSt + gnreNPaga - gnrePaga);
                        decimal? icmsAp = result.Select(_ => _.Note.IcmsAp).Distinct().Sum();
                        ViewBag.ValorDief = Convert.ToDouble(valorDief).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsAp = Convert.ToDouble(icmsAp).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsPagar = Convert.ToDouble(valorDief - icmsAp).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    }
                    ViewBag.TotalFecop = Convert.ToDouble(result.Select(_ => _.TotalFecop).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFrete = Convert.ToDouble(result.Select(_ => _.Freterateado).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIpi = Convert.ToDouble(result.Select(_ => _.Vipi).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                }
                else if (type == 3)
                {
                    var prod = result.Where(_ => _.Pautado.Equals(false));
                    ViewBag.product = prod;
                    ViewBag.type = type;
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