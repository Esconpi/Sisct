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
        private readonly IDarService _darService;

        public ProductNote(
            IProductNoteService service,
            INoteService noteService,
            INcmService ncmService,
            IProductService productService,
            ITaxationTypeService taxationTypeService,
            IStateService stateService,
            ITaxationService taxationService,
            ICompanyService companyService,
            IDarService darService,
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
            _darService = darService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(int noteId)
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
                    var result = _service.FindByNotes(noteId, GetLog(OccorenceLog.Read));
                    var rst = _noteService.FindById(noteId, GetLog(OccorenceLog.Read));
                    ViewBag.Id = rst.CompanyId;
                    ViewBag.Year = rst.AnoRef;
                    ViewBag.Month = rst.MesRef;
                    ViewBag.Note = rst.Nnf;
                    ViewBag.Fornecedor = rst.Xnome;
                    ViewBag.Valor = rst.Vnf;
                    ViewBag.Data = rst.Dhemi.ToString("dd/MM/yyyy");
                    ViewBag.Uf = rst.Uf;
                    ViewBag.View = rst.View;
                    ViewBag.NoteId = rst.Id;

                    return View(result);
                }
              
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

                if(ncm == null)
                {
                    string message = "O Ncm " + result.Ncm + " não estar cadastrado";
                    throw new Exception(message);
                }

                ViewBag.DescriptionNCM = ncm.Description;

                List<TaxationType> list_taxation = _taxationTypeService.FindAll(GetLog(OccorenceLog.Read));
                                             
                list_taxation.Insert(0, new TaxationType() { Description = "Nennhum item selecionado", Id = 0 });
                
                
                SelectList taxationtypes = new SelectList(list_taxation, "Id", "Description", null);
                //List<Product> list_product = _productService.FindAll(GetLog(OccorenceLog.Read));
                List<Product> list_product = _service.FindAllInDate(result.Note.Dhemi);
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
                if (result.ProductId == null)
                {
                    result.ProductId = 0;
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

                var taxedtype = _taxationTypeService.FindById(Convert.ToInt32(taxaType), GetLog(OccorenceLog.Read));
                var product = _productService.FindById(Convert.ToInt32(productid), GetLog(OccorenceLog.Read));

                if (entity.Pautado == true)
                {
                    var prod = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                    
                    decimal baseCalc = 0;
                    decimal valor_icms = prod.IcmsCTe + prod.Vicms;

                    if (taxedtype.Type == "ST")
                    {
                        decimal total_icms_pauta = 0;
                        decimal total_icms = 0;
                        baseCalc = prod.Vbasecalc + prod.Vdesc;


                        decimal quantParaCalc = 0;
                        quantParaCalc = Convert.ToDecimal(prod.Qcom);
                        if (quantPauta != "")
                        {
                            prod.Qpauta = Convert.ToDecimal(quantPauta);
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
                            prod.Fecop = Convert.ToDecimal(fecop);
                            valor_fecop = (Convert.ToDecimal(fecop) / 100) * vAgre;
                        }
                        decimal valorAgreAliqInt = calculation.valorAgregadoAliqInt(AliqInt, Convert.ToDecimal(fecop), vAgre);
                        decimal icms_pauta = valorAgreAliqInt - valor_icms;
                        total_icms_pauta = icms_pauta + valor_fecop;
                        
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
                            valor_icms = valor_icms * Convert.ToDecimal(bcrForm) / 100;
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
                        total_icms = valorAgre_AliqInt - valor_icms;
                        
                        decimal total = Convert.ToDecimal(entity.TotalICMS) + valor_fecop;

                        if (total_icms > total_icms_pauta)
                        {
                            prod.TotalICMS = total_icms;
                        }
                        else
                        {
                            prod.TotalICMS = total_icms_pauta;
                            prod.Pautado = true;
                            prod.ProductId = product.Id;
                        }

                    }
                    if (product.Group.Active.Equals(true))
                    {
                        prod.Incentivo = true;
                    }
                    prod.TaxationTypeId = Convert.ToInt32(taxaType);
                    prod.Updated = DateTime.Now;
                    prod.Status = true;
                    prod.Vbasecalc = baseCalc;

                    _service.Update(prod, GetLog(Model.OccorenceLog.Read));
                }
                else
                {
                    if (Request.Form["produto"].ToString() == "2")
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
                                valor_icms = valor_icms * Convert.ToDecimal(bcrForm) / 100;
                            }

                            if (fecop != null)
                            {
                                prod.Fecop = Convert.ToDecimal(fecop);
                                valor_fecop = calculation.valorFecop(Convert.ToDecimal(fecop), valorAgreg);
                                prod.TotalFecop = valor_fecop;
                            }
                            else
                            {
                                prod.Fecop = Convert.ToDecimal(0);
                                valor_fecop = calculation.valorFecop(Convert.ToDecimal(0), valorAgreg);
                                prod.TotalFecop = valor_fecop;
                            }
                            prod.Aliqinterna = AliqInt;
                            decimal valorAgre_AliqInt = calculation.valorAgregadoAliqInt(Convert.ToDecimal(AliqInt), Convert.ToDecimal(prod.Fecop), valorAgreg);
                            prod.ValorAC = valorAgre_AliqInt;
                            total_icms = valorAgre_AliqInt - valor_icms;
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

                        if (note.Company.Incentive.Equals(true) && note.Company.AnnexId.Equals(2))
                        {
                            prod.Incentivo = false;
                        }


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
                                    valor_icms = valor_icms * Convert.ToDecimal(bcrForm) / 100;
                                }

                                if (fecop != null)
                                {
                                    item.Fecop = Convert.ToDecimal(fecop);
                                    valor_fecop = calculation.valorFecop(Convert.ToDecimal(fecop), valorAgreg);
                                    item.TotalFecop = valor_fecop;
                                }
                                else
                                {
                                    item.Fecop = Convert.ToDecimal(0);
                                    valor_fecop = calculation.valorFecop(Convert.ToDecimal(0), valorAgreg);
                                    item.TotalFecop = valor_fecop;
                                }
                                item.Aliqinterna = AliqInt;
                                decimal valorAgre_AliqInt = calculation.valorAgregadoAliqInt(Convert.ToDecimal(AliqInt), Convert.ToDecimal(item.Fecop), valorAgreg);
                                item.ValorAC = valorAgre_AliqInt;
                                total_icms = valorAgre_AliqInt - valor_icms;

                                //decimal total = Convert.ToDecimal(item.TotalICMS) + valor_fecop;

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

                            if (note.Company.Incentive.Equals(true) && note.Company.AnnexId.Equals(2))
                            {
                                item.Incentivo = false;
                            }

                            var result = _service.Update(item, GetLog(OccorenceLog.Update));
                        }
                    }
                }

              

                if (Request.Form["produto"].ToString() == "1" && entity.Pautado == false)
                {
                    decimal? bcr = null;
                    if (bcrForm != null)
                    {
                        bcr = Convert.ToDecimal(bcrForm);
                    }

                    var ncm = _ncmService.FindByCode(rst.Ncm);

                    if (rst.Picms != 4)
                    {
                        var state = _stateService.FindByUf(note.Uf);
                        rst.Picms = state.Aliquota;
                    }
                    string code = note.Company.Document + rst.Ncm + note.Uf + rst.Picms;

                    var taxationcm = _taxationService.FindByNcm(code);

                    if (taxationcm != null)
                    {
                        taxationcm.DateEnd = Convert.ToDateTime(dateStart).AddDays(-1);
                        _taxationService.Update(taxationcm, GetLog(OccorenceLog.Update));
                    }
                    var taxation = new Model.Taxation
                    {
                        CompanyId = note.CompanyId,
                        Code = code,
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
                var notasTaxation = result.Select(_ => _.Note).Distinct().ToList();
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
                ViewBag.type = type;

              
                if (type == 1 || type == 2 || type == 4 || type == 5 || type == 7 || type == 8)
                {    
                    if (type == 2)
                    {
                        notes = notes.Where(_ => _.Nnf.Equals(nota)).ToList();
                        total = notes.Select(_ => _.Vnf).Sum();
                        result = _service.FindByProductsType(notes, typeTaxation);
                    }


                    if (type == 4)
                    {
                        ViewBag.NotasTaxation = notasTaxation;
                        ViewBag.Products = result;
                       
                    }

                    if (type == 7)
                    {
                        result = result.Where(_ => _.Incentivo.Equals(true)).ToList();
                    }
                    else if (type == 8)
                    {
                        result = result.Where(_ => _.Incentivo.Equals(false)).ToList();
                    }

                    ViewBag.Registro = result.Count();
                    var totalFecop = result.Select(_ => _.TotalFecop).Sum();

                    if (type == 5)
                    {
                        List<List<string>> notasAgrup = new List<List<string>>();
                        for (int i = 0; i < notasTaxation.Count; i++)
                        {
                            List<string> notesAgrup = new List<string>();
                            notesAgrup.Add(notasTaxation[i].Nnf.ToString());
                            notesAgrup.Add(notasTaxation[i].Xnome.ToString());
                            notesAgrup.Add(notasTaxation[i].Dhemi.ToString("dd/MM"));
                            notesAgrup.Add(Convert.ToDouble(notasTaxation[i].Vnf).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            var Vprod = Convert.ToDouble(result.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vprod).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""); 
                            var Vipi = Convert.ToDouble(result.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vipi).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var frete = Convert.ToDouble(result.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Freterateado).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var bcTotal = Convert.ToDouble(result.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vbasecalc).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var bcIcms = Convert.ToDouble(result.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Valoragregado).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var bcr = Convert.ToDouble(result.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.ValorBCR).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var vAC = Convert.ToDouble(result.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.ValorAC).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var nfe = Convert.ToDouble(result.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vicms).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var cte = Convert.ToDouble(result.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.IcmsCTe).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var icms = Convert.ToDouble(result.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.IcmsST).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var icmsTotal = Convert.ToDouble(result.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.TotalICMS).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var fecopTotal = Convert.ToDouble(result.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.TotalFecop).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var vFrete = Convert.ToDouble(result.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vfrete).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var icmsApurado = Convert.ToDouble(result.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.IcmsApurado).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            notesAgrup.Add(Vprod);
                            notesAgrup.Add(Vipi);
                            notesAgrup.Add(notasTaxation[i].Nct.ToString());
                            notesAgrup.Add(frete);
                            notesAgrup.Add(bcTotal);
                            notesAgrup.Add(bcIcms);
                            notesAgrup.Add(bcr);
                            notesAgrup.Add(vAC);
                            notesAgrup.Add(nfe);
                            notesAgrup.Add(cte);
                            notesAgrup.Add(icms);
                            notesAgrup.Add(icmsTotal);
                            notesAgrup.Add(fecopTotal);
                            notesAgrup.Add(vFrete);
                            notesAgrup.Add(icmsApurado);
                            notasAgrup.Add(notesAgrup);
                        }

                        ViewBag.NotasTaxation = notasAgrup;
                        ViewBag.Registro = notasAgrup.Count();
                    }


                    ViewBag.Notes = notes;


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
                        decimal gnreNPaga = Math.Round(Convert.ToDecimal(notasTaxation.Select(_ => _.GnreNSt).Sum()), 2);
                        decimal gnrePaga = Math.Round(Convert.ToDecimal(notasTaxation.Select(_ => _.GnreSt).Sum()), 2);
                        ViewBag.TotalGNREnPaga = Convert.ToDouble(gnreNPaga).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGNREPaga = Convert.ToDouble(gnrePaga).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal base1 = Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2);
                        base1 += Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);
                        decimal base1fecop = Math.Round(Convert.ToDecimal(notasTaxation.Select(_ => _.Fecop1).Sum()), 2);
                        decimal valorbase1 = Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                        ViewBag.base1 = Convert.ToDouble(Math.Round(base1, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.base1fecop = Convert.ToDouble(base1fecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.valorbase1 = Convert.ToDouble(Math.Round(valorbase1, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal base2 = Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2);
                        base2 += Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);
                        decimal base2fecop = Math.Round(Convert.ToDecimal(notasTaxation.Select(_ => _.Fecop2).Sum()), 2);
                        decimal valorbase2 = Math.Round(Convert.ToDecimal(result.Where(_ => _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                        ViewBag.base2 = Convert.ToDouble(Math.Round(base2, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.base2fecop = Convert.ToDouble(base2fecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.valorbase2 = Convert.ToDouble(Math.Round(valorbase2, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        var totalBaseFecop = base1fecop + base2fecop;
                        ViewBag.TotalBaseFecop = Convert.ToDouble(totalBaseFecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

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

                        decimal gnreNPagaFecop = Math.Round(Convert.ToDecimal(notasTaxation.Select(_ => _.GnreFecop).Sum()), 2);
                        ViewBag.GNREnPagaFecop = Convert.ToDouble(gnreNPagaFecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal gnrePagaFecop1 = Math.Round(Convert.ToDecimal(notasTaxation.Select(_ => _.FecopGnre1).Sum()), 2);
                        ViewBag.GNREPagaFecop1 = Convert.ToDouble(gnrePagaFecop1).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal gnrePagaFecop2 = Math.Round(Convert.ToDecimal(notasTaxation.Select(_ => _.FecopGnre2).Sum()), 2);
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
                        decimal icmsStnota = Math.Round(Convert.ToDecimal(notasTaxation.Select(_ => _.IcmsSt).Sum()), 2);
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

                            var notasTaxationNormal = productsNormal.Select(_ => _.Note).Distinct();

                            icmsSt = Math.Round(Convert.ToDecimal(productsNormal.Select(_ => _.IcmsST).Sum()), 2);
                            ViewBag.TotalICMSST = Convert.ToDouble(icmsSt).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            gnreNPaga = Math.Round(Convert.ToDecimal(notasTaxationNormal.Select(_ => _.GnreNSt).Distinct().Sum()), 2);
                            gnrePaga = Math.Round(Convert.ToDecimal(notasTaxationNormal.Select(_ => _.GnreSt).Distinct().Sum()), 2);
                            ViewBag.TotalGNREnPaga = Convert.ToDouble(gnreNPaga).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGNREPaga = Convert.ToDouble(gnrePaga).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            diefSt = Convert.ToDecimal(totalIcmsNormal - icmsSt + gnreNPaga);
                            ViewBag.ValorDief = Convert.ToDouble(diefSt).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            decimal? IcmsMva = productsNormal.Where(_ => _.Pautado.Equals(false) && _.TaxationType.Type.Equals("ST")).Select(_ => _.TotalICMS).Sum();
                            decimal? IcmsPauta = productsNormal.Where(_ => _.Pautado.Equals(true) && _.TaxationType.Type.Equals("ST")).Select(_ => _.TotalICMS).Sum();
                            ViewBag.TotalICMSMva = Convert.ToDouble(IcmsMva).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalICMSPauta = Convert.ToDouble(IcmsPauta).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            icmsStnota = Math.Round(Convert.ToDecimal(notasTaxationNormal.Select(_ => _.IcmsSt).Distinct().Sum()), 2);

                            ViewBag.IcmsPagar = Convert.ToDouble(diefSt - icmsStnota).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            base1 = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2);
                            base1 += Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);
                            ViewBag.base1 = Convert.ToDouble(Math.Round(base1, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            base2 = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2);
                            base2 += Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);
                            ViewBag.base2 = Convert.ToDouble(Math.Round(base2, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            valorbase1 = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                            ViewBag.valorbase1 = Convert.ToDouble(Math.Round(valorbase1, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            valorbase2 = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                            ViewBag.valorbase2 = Convert.ToDouble(Math.Round(valorbase2, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            totalBaseFecop = base1fecop + base2fecop;
                            ViewBag.TotalBaseFecop = Convert.ToDouble(totalBaseFecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


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


                            gnreNPagaFecop = Math.Round(Convert.ToDecimal(notasTaxationNormal.Select(_ => _.GnreFecop).Distinct().Sum()), 2);
                            ViewBag.GNREnPagaFecop = Convert.ToDouble(gnreNPagaFecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            gnrePagaFecop1 = Math.Round(Convert.ToDecimal(notasTaxationNormal.Select(_ => _.FecopGnre1).Distinct().Sum()), 2);
                            ViewBag.GNREPagaFecop1 = Convert.ToDouble(gnrePagaFecop1).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            gnrePagaFecop2 = Math.Round(Convert.ToDecimal(notasTaxationNormal.Select(_ => _.FecopGnre2).Distinct().Sum()), 2);
                            ViewBag.GNREPagaFecop2 = Convert.ToDouble(gnrePagaFecop2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            totalGnreFecop = gnrePagaFecop1 + gnrePagaFecop2;
                            ViewBag.TotalGnreFecop = Convert.ToDouble(totalGnreFecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            TotalFecopCalc = valorbase1 + valorbase2;
                            ViewBag.TotalFecopCalculada = Convert.ToDouble(Math.Round(TotalFecopCalc, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            TotalFecopNfe = valorNfe1Normal + valorNfe1Ret + valorNfe2Normal + valorNfe2Ret;
                            ViewBag.TotalFecopNfe = Convert.ToDouble(Math.Round(TotalFecopNfe, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            // Valores da dief fecop
                            ViewBag.DifBase1 = Convert.ToDouble(Math.Round(base1 - baseNfe1Normal - baseNfe1Ret, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            difvalor1 = valorbase1 - valorNfe1Normal - valorNfe1Ret - gnrePagaFecop1;
                            ViewBag.DifValor1 = Convert.ToDouble(Math.Round(difvalor1, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.DifBase2 = Convert.ToDouble(Math.Round(base2 - baseNfe2Normal - baseNfe2Ret, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            difvalor2 = valorbase2 - valorNfe2Normal - valorNfe2Ret - gnrePagaFecop2;
                            ViewBag.DifValor2 = Convert.ToDouble(Math.Round(difvalor2, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            diftotal = difvalor1 + difvalor2;
                            ViewBag.DifTotal = Convert.ToDouble(Math.Round(diftotal, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            totalfecop1 = difvalor1 - base1fecop;
                            ViewBag.TotalFecop1 = Convert.ToDouble(Math.Round(totalfecop1, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            totalfecop2 = difvalor2 - base2fecop;
                            ViewBag.TotalFecop2 = Convert.ToDouble(Math.Round(totalfecop2, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalFinalFecopCalculada = Convert.ToDouble(Math.Round(totalfecop1 + totalfecop2, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


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

                            decimal? impostoGeral = 0;

                            if (type == 7)
                            {
                                var productsP = _service.FindByProductsType(notes, typeTaxation);

                                totalIcms = productsP.Select(_ => _.TotalICMS).Sum();
                                var totalFecop1 = productsP.Select(_ => _.TotalFecop).Sum();

                                impostoGeral = totalIcms + totalFecop1;
                            }
                            else
                            {
                                impostoGeral = totalIcms + totalFecop;
                            }

                            decimal? basefunef = impostoGeral - impostoIcms;
                            ViewBag.BaseFunef = Convert.ToDouble(Math.Round(Convert.ToDecimal(basefunef), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.Funef = company.Funef;
                            decimal taxaFunef = Convert.ToDecimal(basefunef * (company.Funef / 100));
                            ViewBag.TaxaFunef = Convert.ToDouble(Math.Round(taxaFunef, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            decimal totalImpostoIncentivo = impostoIcms + impostoFecop + taxaFunef;
                            ViewBag.TotalImpostoIncentivo = Convert.ToDouble(Math.Round(totalImpostoIncentivo,2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            ViewBag.ImpostoGeral = impostoGeral;
                        }

                    }
                    else if (typeTaxation >= 2 && typeTaxation <= 5)
                    {
                        totalIcms = result.Select(_ => _.IcmsApurado).Sum();

                        decimal gnrePaga = 0, gnreNPaga = 0;
                        decimal? icmsAp = 0;
                        if (typeTaxation == 2)
                        {
                            gnrePaga = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreAp).Distinct().Sum()), 2);
                            gnreNPaga = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreNAp).Distinct().Sum()), 2);
                            icmsAp = result.Select(_ => _.Note.IcmsAp).Distinct().Sum();
                        }
                        else if (typeTaxation == 3)
                        {
                            gnrePaga = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreCo).Distinct().Sum()), 2);
                            gnreNPaga = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreNCo).Distinct().Sum()), 2);
                            icmsAp = result.Select(_ => _.Note.IcmsCo).Distinct().Sum();
                        }
                        else if (typeTaxation == 5)
                        {
                            gnrePaga = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreIm).Distinct().Sum()), 2);
                            gnreNPaga = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreNIm).Distinct().Sum()), 2);
                            icmsAp = result.Select(_ => _.Note.IcmsIm).Distinct().Sum();
                        }

                        ViewBag.TotalGNREnPaga = Convert.ToDouble(gnreNPaga).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGNREPaga = Convert.ToDouble(gnrePaga).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    
                        ViewBag.TotalICMS = Convert.ToDouble(totalIcms).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        valorDief = Convert.ToDecimal(totalIcms - icmsSt + gnreNPaga - gnrePaga);
                        
                        ViewBag.ValorDief = Convert.ToDouble(valorDief).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsAp = Convert.ToDouble(icmsAp).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsPagar = Convert.ToDouble(valorDief - icmsAp).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    }
                    
                    ViewBag.TotalFecop = Convert.ToDouble(totalFecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFrete = Convert.ToDouble(result.Select(_ => _.Freterateado).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIpi = Convert.ToDouble(result.Select(_ => _.Vipi).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                }
                else if (type == 3)
                {
                    var prod = result.Where(_ => _.Pautado.Equals(false));
                    ViewBag.product = prod;
                }else if (type == 6)
                {
                    result = _service.FindByProducts(notes);

                    // Icms Substituição Tributária
                    decimal? totalApuradoST = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)).Select(_ => _.TotalICMS).Sum()), 2);
                    decimal? totalIcmsPagoST = Math.Round(Convert.ToDecimal(notes.Select(_ => _.IcmsSt).Sum()), 2);
                    decimal? totalAPagaST = totalApuradoST - totalIcmsPagoST;
                    int? qtdST = result.Where(_ => _.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)).Count();
                    ViewBag.QtdST = qtdST;
                    ViewBag.TotatlApuradoST = Convert.ToDouble(totalApuradoST).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoST = Convert.ToDouble(totalIcmsPagoST).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAPagaST = Convert.ToDouble(totalAPagaST).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Antecipação Parcial
                    decimal? totalApuradoAP = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsApurado).Sum()), 2);
                    var n = notes.Select(_ => _.IcmsAp);
                    decimal? totalIcmsPagoAP = Math.Round(Convert.ToDecimal(notes.Select(_ => _.IcmsAp).Sum()), 2);
                    decimal? totalAPagaAP = totalApuradoAP - totalIcmsPagoAP;
                    int? qtdAP = result.Where(_ => _.TaxationTypeId.Equals(1)).Count();
                    ViewBag.QtdAP = qtdAP;
                    ViewBag.TotalApuradoAP = Convert.ToDouble(totalApuradoAP).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoAP = Convert.ToDouble(totalIcmsPagoAP).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAPagaAP = Convert.ToDouble(totalAPagaAP).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Consumo
                    decimal? totalApuradoCO = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalIcmsPagoCO = Math.Round(Convert.ToDecimal(notes.Select(_ => _.IcmsCo).Sum()), 2);
                    decimal? totalAPagaCO = totalApuradoCO - totalIcmsPagoCO;
                    int? qtdCO = result.Where(_ => _.TaxationTypeId.Equals(2)).Count();
                    ViewBag.QtdCO = qtdCO;
                    ViewBag.TotalApuradoCO = Convert.ToDouble(totalApuradoCO).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotatIcmsPagoCO = Convert.ToDouble(totalIcmsPagoCO).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAPagaCO = Convert.ToDouble(totalAPagaCO).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Consumo para Revenda
                    decimal? totalApuradoCOR = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsApurado).Sum()), 2);
                    int? qtdCOR = result.Where(_ => _.TaxationTypeId.Equals(4)).Count();
                    ViewBag.QtdCOR = qtdCOR;
                    ViewBag.TotalApuradoCOR = Convert.ToDouble(totalApuradoCOR).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAPagaCOR = Convert.ToDouble(totalApuradoCOR).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Imobilizado
                    decimal? totalApuradoIM = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalIcmsPagoIM = Math.Round(Convert.ToDecimal(notes.Select(_ => _.IcmsIm).Sum()), 2);
                    decimal? totalAPagaIM = totalApuradoIM - totalIcmsPagoIM;
                    int? qtdIM = result.Where(_ => _.TaxationTypeId.Equals(3)).Count();
                    ViewBag.QtdIM = qtdIM;
                    ViewBag.TotalApuradoIM = Convert.ToDouble(totalApuradoIM).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoIM = Convert.ToDouble(totalIcmsPagoIM).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAPagaIM = Convert.ToDouble(totalAPagaIM).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Antecipação Total
                    decimal? totalApuradoAT = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(8)).Select(_ => _.TotalICMS).Sum()), 2);
                    int? qtdAT = result.Where(_ => _.TaxationTypeId.Equals(8)).Count();
                    ViewBag.QtdAT = qtdAT;
                    ViewBag.TotalApuradoAT = Convert.ToDouble(totalApuradoAT).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAPagaAT = Convert.ToDouble(totalApuradoAT).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Isento
                    int? qtdIsento = result.Where(_ => _.TaxationTypeId.Equals(7)).Count();
                    ViewBag.QtdIsento = qtdIsento;


                    // Somatório Geral
                    int? qtdGeral = qtdST + qtdIsento + qtdIM + qtdCOR + qtdAP + qtdAT + qtdCO;
                    decimal? totalApuradoGeral = totalApuradoST + totalApuradoAP + totalAPagaCO + totalApuradoAT + totalApuradoCOR + totalApuradoIM;
                    decimal? totalIcmsPagoGeral = totalIcmsPagoAP + totalIcmsPagoCO + totalIcmsPagoIM + totalIcmsPagoST;
                    decimal? totalAPagaGeral = totalAPagaST + totalAPagaAP + totalAPagaIM + totalAPagaCO + totalApuradoCOR + totalApuradoAT;
                    ViewBag.QtdGeral = qtdGeral;
                    ViewBag.TotalApuradoGeral = Convert.ToDouble(totalApuradoGeral).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoGeral = Convert.ToDouble(totalIcmsPagoGeral).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAPagaGeral = Convert.ToDouble(totalAPagaGeral).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    
                }

                var dar = _darService.FindAll(GetLog(OccorenceLog.Read));

                var darFecop = dar.Where(_ => _.Type.Equals("Fecop")).Select(_ => _.Code).FirstOrDefault();
                var darStCo = dar.Where(_ => _.Type.Equals("ST-CO")).Select(_ => _.Code).FirstOrDefault();
                var darIcms = dar.Where(_ => _.Type.Equals("Icms")).Select(_ => _.Code).FirstOrDefault();
                var darAp = dar.Where(_ => _.Type.Equals("AP")).Select(_ => _.Code).FirstOrDefault();
                var darIm = dar.Where(_ => _.Type.Equals("IM")).Select(_ => _.Code).FirstOrDefault();
                var darFunef = dar.Where(_ => _.Type.Equals("Funef")).Select(_ => _.Code).FirstOrDefault();
                ViewBag.DarFecop = darFecop;
                ViewBag.DarSTCO = darStCo;
                ViewBag.DarIcms = darIcms;
                ViewBag.DarAp = darAp;
                ViewBag.DarIm = darIm;
                ViewBag.DarFunef = darFunef;

                return View(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
           
    }
}