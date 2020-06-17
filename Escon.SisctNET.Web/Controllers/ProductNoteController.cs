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
        private readonly IProduct1Service _product1Service;

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
            IProduct1Service product1Service,
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
            _product1Service = product1Service;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(int noteId)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }

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
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }
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
                ViewBag.DataNote = note.Dhemi;

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
            
                List<Product1> list_product1 = _service.FindAllInDate1(result.Note.Dhemi);
                foreach (var prod in list_product1)
                {
                    prod.Description = prod.Code + " - " + prod.Price + " - " + prod.Description;
                }
                list_product1.Insert(0, new Product1() { Description = "Nennhum item selecionado", Id = 0 });
                SelectList products1 = new SelectList(list_product1, "Id", "Description", null);
                ViewBag.Product1Id = products1;
               

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
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }
            try
            { 
                var rst = _service.FindById(id, GetLog(OccorenceLog.Read));
                var note = _noteService.FindByNote(rst.Note.Chave);
                var calculation = new Calculation();

                var mva = entity.Mva;
                var fecop = entity.Fecop;
                var bcrForm = entity.BCR;
                decimal AliqInt = Convert.ToDecimal(Request.Form["AliqInt"]);
                var taxaType = Request.Form["taxaType"];
                var productid = Request.Form["productid"];
                var product1id = Request.Form["product1id"];
                var quantPauta = Request.Form["quantAlterada"];
                var dateStart = Request.Form["dateStart"];
                var dateNote = Request.Form["dataNote"];

                decimal valorAgreg = 0, dif = 0;
                decimal valor_fecop = 0;
                string code2 = "";
                var notes = _noteService.FindByUf(note.Company.Id,note.AnoRef,note.MesRef,note.Uf);
                var products = _service.FindByNcmUfAliq(notes,entity.Ncm,entity.Picms, rst.Cest);

                var taxedtype = _taxationTypeService.FindById(Convert.ToInt32(taxaType), GetLog(OccorenceLog.Read));

                var product = _productService.FindById(Convert.ToInt32(productid), GetLog(OccorenceLog.Read));

                var product1 = _product1Service.FindById(Convert.ToInt32(product1id), GetLog(OccorenceLog.Read));

                

                if (entity.Pautado == true)
                {
                    var prod = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                    decimal precoPauta = 0;

                    if (Convert.ToDateTime(dateNote) < Convert.ToDateTime("10/02/2020"))
                    {
                        precoPauta = Convert.ToDecimal(product.Price);
                    }
                    else if (Convert.ToDateTime(dateNote) >= Convert.ToDateTime("10/02/2020"))
                    {
                        precoPauta = Convert.ToDecimal(product1.Price);
                    }

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
                        decimal vAgre = calculation.valorAgregadoPauta(Convert.ToDecimal(quantParaCalc), precoPauta);

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
                            //valor_icms = valor_icms * Convert.ToDecimal(bcrForm) / 100;
                            valor_icms = 0;
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
                           
                        }

                        prod.Pautado = true;

                        if (product != null)
                        {
                            prod.ProductId = product.Id;
                        }

                        if (product1 != null)
                        {
                            prod.Product1Id = product1.Id;
                        }

                    }

                    if (product != null)
                    {
                        if (product.Group.Active.Equals(true))
                        {
                            prod.Incentivo = true;
                        }
                    }

                    if (product1 != null)
                    {
                        if (product1.Group.Active.Equals(true))
                        {
                            prod.Incentivo = true;
                        }
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
                                //valor_icms = valor_icms * Convert.ToDecimal(bcrForm) / 100;
                                valor_icms = 0;
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
                                    //valor_icms = valor_icms * Convert.ToDecimal(bcrForm) / 100;
                                    valor_icms = 0;
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

                    var taxationcm = _taxationService.FindByNcm(code, rst.Cest);

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
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }
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
                        notesS = notes.Where(_ => _.Iest == "");
                        notesI = notes.Where(_ => _.Iest != "");
                        icmsStnoteS = _service.FindBySubscription(notesS.ToList(), typeTaxation);
                        icmsStnoteI = _service.FindBySubscription(notesI.ToList(), typeTaxation);
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

                    decimal icmsStIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum()), 2);
                    decimal icmsStSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum()), 2);
                    decimal? totalIcmsIE = 0, totalIcmsSIE = 0, valorDief = 0; 
                    ViewBag.TotalICMSST = Convert.ToDouble(icmsStIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    if (typeTaxation == 1 || typeTaxation == 7)
                    {
                        decimal totalIcmsPautaSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Pautado.Equals(true) && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2);
                        decimal totalIcmsMvaSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Pautado.Equals(false) && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2);
                        decimal totalIcmsPautaIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Pautado.Equals(true) && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2);
                        decimal totalIcmsMvaIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Pautado.Equals(false) && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2);
                        
                        decimal gnreNPagaSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNSt).Sum()), 2);
                        decimal gnrePagaSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreSt).Sum()), 2);
                        decimal gnreNPagaIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNSt).Sum()), 2);
                        decimal gnrePagaIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreSt).Sum()), 2);
                        ViewBag.TotalGNREnPagaSIE = Convert.ToDouble(gnreNPagaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGNREPagaSIE = Convert.ToDouble(gnrePagaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGNREnPagaIE = Convert.ToDouble(gnreNPagaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGNREPagaIE = Convert.ToDouble(gnrePagaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal base1SIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2);
                        base1SIE += Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);
                        decimal base1IE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2);
                        base1IE += Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);
                        
                        decimal base1fecopIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.Fecop1).Sum()), 2);
                        decimal base1fecopSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.Fecop1).Sum()), 2);

                        decimal valorbase1IE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                        decimal valorbase1SIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);

                        ViewBag.base1SIE = Convert.ToDouble(Math.Round(base1SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.base1IE = Convert.ToDouble(Math.Round(base1IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.base1fecopIE = Convert.ToDouble(base1fecopIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.base1fecopSIE = Convert.ToDouble(base1fecopSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.valorbase1IE = Convert.ToDouble(Math.Round(valorbase1IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.valorbase1SIE = Convert.ToDouble(Math.Round(valorbase1SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal base2IE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2);
                        base2IE += Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);
                        decimal base2SIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2);
                        base2SIE += Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);
                       
                        decimal base2fecopIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.Fecop2).Sum()), 2);
                        decimal base2fecopSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.Fecop2).Sum()), 2);

                        decimal valorbase2IE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                        decimal valorbase2SIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);

                        ViewBag.base2IE = Convert.ToDouble(Math.Round(base2IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.base2SIE = Convert.ToDouble(Math.Round(base2SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.base2fecopIE = Convert.ToDouble(base2fecopIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.base2fecopSIE = Convert.ToDouble(base2fecopSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.valorbase2IE = Convert.ToDouble(Math.Round(valorbase2IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.valorbase2SIE = Convert.ToDouble(Math.Round(valorbase2SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        var totalBaseFecopIE = base1fecopIE + base2fecopIE;
                        var totalBaseFecopSIE = base1fecopSIE + base2fecopSIE;
                        ViewBag.TotalBaseFecopIE = Convert.ToDouble(totalBaseFecopIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalBaseFecopSIE = Convert.ToDouble(totalBaseFecopSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

    
                        decimal baseNfe1NormalIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                        decimal baseNfe1NormalSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                        decimal baseNfe1RetIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);
                        decimal baseNfe1RetSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);
                        ViewBag.fecopNfe1IE = Convert.ToDouble(Math.Round(baseNfe1NormalIE + baseNfe1RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.fecopNfe1SIE = Convert.ToDouble(Math.Round(baseNfe1NormalSIE + baseNfe1RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal valorNfe1NormalIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe1RetIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                        decimal valorNfe1NormalSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe1RetSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                        ViewBag.valorNfe1IE = Convert.ToDouble(Math.Round(valorNfe1NormalIE + valorNfe1RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.valorNfe1SIE = Convert.ToDouble(Math.Round(valorNfe1NormalSIE + valorNfe1RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal baseNfe2NormalIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2);
                        decimal baseNfe2RetIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2);
                        decimal baseNfe2NormalSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2);
                        decimal baseNfe2RetSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2);
                        ViewBag.fecopNfe2IE = Convert.ToDouble(Math.Round(baseNfe2NormalIE + baseNfe2RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.fecopNfe2SIE = Convert.ToDouble(Math.Round(baseNfe2NormalSIE + baseNfe2RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal valorNfe2NormalIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe2RetIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                        decimal valorNfe2NormalSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe2RetSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                        ViewBag.valorNfe2IE = Convert.ToDouble(Math.Round(valorNfe2NormalIE + valorNfe2RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.valorNfe2SIE = Convert.ToDouble(Math.Round(valorNfe2NormalSIE + valorNfe2RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal gnreNPagaFecopIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreFecop).Sum()), 2);
                        decimal gnreNPagaFecopSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreFecop).Sum()), 2);
                        ViewBag.GNREnPagaFecopIE = Convert.ToDouble(gnreNPagaFecopIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.GNREnPagaFecopSIE = Convert.ToDouble(gnreNPagaFecopSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal gnrePagaFecop1IE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.FecopGnre1).Sum()), 2);
                        decimal gnrePagaFecop1SIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.FecopGnre1).Sum()), 2);
                        ViewBag.GNREPagaFecop1IE = Convert.ToDouble(gnrePagaFecop1IE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.GNREPagaFecop1SIE = Convert.ToDouble(gnrePagaFecop1SIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal gnrePagaFecop2IE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.FecopGnre2).Sum()), 2);
                        decimal gnrePagaFecop2SIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.FecopGnre2).Sum()), 2);

                        ViewBag.GNREPagaFecop2IE = Convert.ToDouble(gnrePagaFecop2IE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.GNREPagaFecop2SIE = Convert.ToDouble(gnrePagaFecop2SIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal totalGnreFecopIE = gnrePagaFecop1IE + gnrePagaFecop2IE;
                        ViewBag.TotalGnreFecopIE = Convert.ToDouble(totalGnreFecopIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        decimal totalGnreFecopSIE = gnrePagaFecop1SIE + gnrePagaFecop2SIE;
                        ViewBag.TotalGnreFecopSIE = Convert.ToDouble(totalGnreFecopSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal TotalFecopCalcIE = valorbase1IE + valorbase2IE;
                        decimal TotalFecopCalcSIE = valorbase1SIE + valorbase2SIE;
                        ViewBag.TotalFecopCalculadaIE = Convert.ToDouble(Math.Round(TotalFecopCalcIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFecopCalculadaSIE = Convert.ToDouble(Math.Round(TotalFecopCalcSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal TotalFecopNfeIE = valorNfe1NormalIE + valorNfe1RetIE + valorNfe2NormalIE + valorNfe2RetIE;
                        decimal TotalFecopNfeSIE = valorNfe1NormalSIE + valorNfe1RetSIE + valorNfe2NormalSIE + valorNfe2RetSIE;
                        ViewBag.TotalFecopNfeIE = Convert.ToDouble(Math.Round(TotalFecopNfeIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFecopNfeSIE = Convert.ToDouble(Math.Round(TotalFecopNfeSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        totalIcmsIE = result.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum();
                        totalIcmsSIE = result.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum();
                        ViewBag.TotalICMS = Convert.ToDouble(totalIcmsIE + totalIcmsSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.TotalICMSSTNota = Convert.ToDouble(totalIcmsIE - (totalIcmsPautaSIE + totalIcmsPautaIE)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");                        
                        ViewBag.TotalICMSPautaSIE = Convert.ToDouble(totalIcmsPautaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalICMSMvaSIE = Convert.ToDouble(totalIcmsMvaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalICMSPautaIE = Convert.ToDouble(totalIcmsPautaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalICMSMvaIE = Convert.ToDouble(totalIcmsMvaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        // Valores da dief resumo
                        //decimal icmsTemp = 0;
                        /*if(gnrePaga < icmsSt)
                        {
                            icmsTemp = icmsSt - gnrePaga;
                        }
                        else
                        {
                            icmsTemp = icmsSt;
                        }*/
                        decimal diefStIE = Convert.ToDecimal(totalIcmsIE - icmsStIE - gnrePagaIE + gnreNPagaIE);
                        decimal diefStSIE = Convert.ToDecimal(totalIcmsSIE - icmsStSIE - gnrePagaSIE + gnreNPagaSIE);
                        ViewBag.ValorDiefIE = Convert.ToDouble(diefStIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ValorDiefSIE = Convert.ToDouble(diefStSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                       
                        decimal icmsStnotaIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsSt).Sum()), 2);
                        decimal icmsStnotaSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsSt).Sum()), 2);
                        ViewBag.IcmsStIE = Convert.ToDouble(icmsStnotaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsStSIE = Convert.ToDouble(icmsStnotaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        
                        ViewBag.IcmsPagarIE = Convert.ToDouble(diefStIE - icmsStnotaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsPagarSIE = Convert.ToDouble(diefStSIE - icmsStnotaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        // Valores da dief fecop
                        ViewBag.DifBase1IE = Convert.ToDouble(Math.Round(base1IE - baseNfe1NormalIE - baseNfe1RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DifBase1SIE = Convert.ToDouble(Math.Round(base1SIE - baseNfe1NormalSIE - baseNfe1RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal difvalor1IE = valorbase1IE - valorNfe1NormalIE - valorNfe1RetIE - gnrePagaFecop1IE;
                        decimal difvalor1SIE = valorbase1SIE - valorNfe1NormalSIE - valorNfe1RetSIE - gnrePagaFecop1SIE;
                        ViewBag.DifValor1IE = Convert.ToDouble(Math.Round(difvalor1IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DifValor1SIE = Convert.ToDouble(Math.Round(difvalor1SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                       
                        ViewBag.DifBase2IE = Convert.ToDouble(Math.Round(base2IE - baseNfe2NormalIE - baseNfe2RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DifBase2SIE = Convert.ToDouble(Math.Round(base2SIE - baseNfe2NormalSIE - baseNfe2RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal difvalor2IE = valorbase2IE - valorNfe2NormalIE - valorNfe2RetIE - gnrePagaFecop2IE;
                        decimal difvalor2SIE = valorbase2SIE - valorNfe2NormalSIE - valorNfe2RetSIE - gnrePagaFecop2SIE;
                        ViewBag.DifValor2IE = Convert.ToDouble(Math.Round(difvalor2IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DifValor2SIE = Convert.ToDouble(Math.Round(difvalor2SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal diftotalIE = difvalor1IE + difvalor2IE;
                        decimal diftotalSIE = difvalor1SIE + difvalor2SIE;
                        ViewBag.DifTotalIE = Convert.ToDouble(Math.Round(diftotalIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DifTotalSIE = Convert.ToDouble(Math.Round(diftotalSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal totalfecop1IE = difvalor1IE - base1fecopIE;
                        decimal totalfecop1SIE = difvalor1SIE - base1fecopSIE;
                        ViewBag.TotalFecop1IE = Convert.ToDouble(Math.Round(totalfecop1IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFecop1SIE = Convert.ToDouble(Math.Round(totalfecop1SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal totalfecop2IE = difvalor2IE - base2fecopIE;
                        decimal totalfecop2SIE = difvalor2SIE - base2fecopSIE;
                        ViewBag.TotalFecop2IE = Convert.ToDouble(Math.Round(totalfecop2IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFecop2SIE = Convert.ToDouble(Math.Round(totalfecop2IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.TotalFinalFecopCalculadaIE = Convert.ToDouble(Math.Round(totalfecop1IE + totalfecop2IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFinalFecopCalculadaSIE = Convert.ToDouble(Math.Round(totalfecop1SIE + totalfecop2SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.Incetive = company.Incentive;
                        ViewBag.TypeIncetive = company.TipoApuracao;

                        //Relatorio das empresas incentivadas
                        if (company.Incentive == true)
                        {
                            //Produtos não incentivados
                            var productsNormal = _service.FindByNormal(notes);
                            decimal? totalIcmsNormalIE = productsNormal.Where(_ => _.TaxationType.Type.Equals("ST") && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum();
                            decimal? totalIcmsNormalSIE = productsNormal.Where(_ => _.TaxationType.Type.Equals("ST") && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum();

                            var notasTaxationNormal = productsNormal.Select(_ => _.Note).Distinct();

                            icmsStIE = Math.Round(Convert.ToDecimal(productsNormal.Select(_ => _.IcmsST).Sum()), 2);
                            ViewBag.TotalICMSST = Convert.ToDouble(icmsStIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            gnreNPagaSIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNSt).Distinct().Sum()), 2);
                            gnrePagaSIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreSt).Distinct().Sum()), 2);
                            gnreNPagaIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNSt).Distinct().Sum()), 2);
                            gnrePagaIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreSt).Distinct().Sum()), 2);
                            ViewBag.TotalGNREnPagaSIE = Convert.ToDouble(gnreNPagaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGNREPagaSIE = Convert.ToDouble(gnrePagaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGNREnPagaIE = Convert.ToDouble(gnreNPagaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGNREPagaIE = Convert.ToDouble(gnrePagaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            diefStIE = Convert.ToDecimal(totalIcmsNormalIE - icmsStIE - gnrePagaIE + gnreNPagaIE);
                            diefStSIE = Convert.ToDecimal(totalIcmsNormalSIE - icmsStSIE - gnrePagaSIE + gnreNPagaSIE);
                            ViewBag.ValorDiefIE = Convert.ToDouble(diefStIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.ValorDiefSIE = Convert.ToDouble(diefStSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            decimal? IcmsMvaIE = productsNormal.Where(_ => _.Pautado.Equals(false) && _.TaxationType.Type.Equals("ST") && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum();
                            decimal? IcmsPautaIE = productsNormal.Where(_ => _.Pautado.Equals(true) && _.TaxationType.Type.Equals("ST") && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum();
                            decimal? IcmsMvaSIE = productsNormal.Where(_ => _.Pautado.Equals(false) && _.TaxationType.Type.Equals("ST") && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum();
                            decimal? IcmsPautaSIE = productsNormal.Where(_ => _.Pautado.Equals(true) && _.TaxationType.Type.Equals("ST") && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum();
                            ViewBag.TotalICMSMvaIE = Convert.ToDouble(IcmsMvaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalICMSPautaIE = Convert.ToDouble(IcmsPautaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalICMSPautaSIE = Convert.ToDouble(IcmsPautaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalICMSMvaSIE = Convert.ToDouble(IcmsMvaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                           

                            icmsStnotaIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsSt).Sum()), 2);
                            icmsStnotaSIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsSt).Sum()), 2);
                            ViewBag.IcmsStIE = Convert.ToDouble(icmsStnotaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.IcmsStSIE = Convert.ToDouble(icmsStnotaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            ViewBag.IcmsPagarIE = Convert.ToDouble(diefStIE - icmsStnotaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.IcmsPagarSIE = Convert.ToDouble(diefStSIE - icmsStnotaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            base1SIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2);
                            base1SIE += Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);
                            base1IE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2);
                            base1IE += Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);

                            base1fecopIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.Fecop1).Sum()), 2);
                            base1fecopSIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.Fecop1).Sum()), 2);

                            valorbase1IE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                            valorbase1SIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);

                            ViewBag.base1SIE = Convert.ToDouble(Math.Round(base1SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.base1IE = Convert.ToDouble(Math.Round(base1IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            ViewBag.base1fecopIE = Convert.ToDouble(base1fecopIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.base1fecopSIE = Convert.ToDouble(base1fecopSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            ViewBag.valorbase1IE = Convert.ToDouble(Math.Round(valorbase1IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.valorbase1SIE = Convert.ToDouble(Math.Round(valorbase1SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            base2IE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2);
                            base2IE += Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);
                            base2SIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2);
                            base2SIE += Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);

                            base2fecopIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.Fecop2).Sum()), 2);
                            base2fecopSIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.Fecop2).Sum()), 2);

                            valorbase2IE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                            valorbase2SIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);

                            ViewBag.base2IE = Convert.ToDouble(Math.Round(base2IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.base2SIE = Convert.ToDouble(Math.Round(base2SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.base2fecopIE = Convert.ToDouble(base2fecopIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.base2fecopSIE = Convert.ToDouble(base2fecopSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.valorbase2IE = Convert.ToDouble(Math.Round(valorbase2IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.valorbase2SIE = Convert.ToDouble(Math.Round(valorbase2SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            totalBaseFecopIE = base1fecopIE + base2fecopIE;
                            totalBaseFecopSIE = base1fecopSIE + base2fecopSIE;
                            ViewBag.TotalBaseFecopIE = Convert.ToDouble(totalBaseFecopIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalBaseFecopSIE = Convert.ToDouble(totalBaseFecopSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            baseNfe1NormalIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                            baseNfe1NormalSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                            baseNfe1RetIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);
                            baseNfe1RetSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);
                            ViewBag.fecopNfe1IE = Convert.ToDouble(Math.Round(baseNfe1NormalIE + baseNfe1RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.fecopNfe1SIE = Convert.ToDouble(Math.Round(baseNfe1NormalSIE + baseNfe1RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            valorNfe1NormalIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                            valorNfe1RetIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                            valorNfe1NormalSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                            valorNfe1RetSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                            ViewBag.valorNfe1IE = Convert.ToDouble(Math.Round(valorNfe1NormalIE + valorNfe1RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.valorNfe1SIE = Convert.ToDouble(Math.Round(valorNfe1NormalSIE + valorNfe1RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            baseNfe2NormalIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2);
                            baseNfe2RetIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2);
                            baseNfe2NormalSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2);
                            baseNfe2RetSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2);
                            ViewBag.fecopNfe2IE = Convert.ToDouble(Math.Round(baseNfe2NormalIE + baseNfe2RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.fecopNfe2SIE = Convert.ToDouble(Math.Round(baseNfe2NormalSIE + baseNfe2RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            valorNfe2NormalIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                            valorNfe2RetIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                            valorNfe2NormalSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                            valorNfe2RetSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                            ViewBag.valorNfe2IE = Convert.ToDouble(Math.Round(valorNfe2NormalIE + valorNfe2RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.valorNfe2SIE = Convert.ToDouble(Math.Round(valorNfe2NormalSIE + valorNfe2RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            gnreNPagaFecopIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreFecop).Sum()), 2);
                            gnreNPagaFecopSIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreFecop).Sum()), 2);
                            ViewBag.GNREnPagaFecopIE = Convert.ToDouble(gnreNPagaFecopIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.GNREnPagaFecopSIE = Convert.ToDouble(gnreNPagaFecopSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            gnrePagaFecop1IE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.FecopGnre1).Sum()), 2);
                            gnrePagaFecop1SIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.FecopGnre1).Sum()), 2);
                            ViewBag.GNREPagaFecop1IE = Convert.ToDouble(gnrePagaFecop1IE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.GNREPagaFecop1SIE = Convert.ToDouble(gnrePagaFecop1SIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            gnrePagaFecop2IE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.FecopGnre2).Sum()), 2);
                            gnrePagaFecop2SIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.FecopGnre2).Sum()), 2);

                            ViewBag.GNREPagaFecop2IE = Convert.ToDouble(gnrePagaFecop2IE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.GNREPagaFecop2SIE = Convert.ToDouble(gnrePagaFecop2SIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            totalGnreFecopIE = gnrePagaFecop1IE + gnrePagaFecop2IE;
                            ViewBag.TotalGnreFecopIE = Convert.ToDouble(totalGnreFecopIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            totalGnreFecopSIE = gnrePagaFecop1SIE + gnrePagaFecop2SIE;
                            ViewBag.TotalGnreFecopSIE = Convert.ToDouble(totalGnreFecopSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            TotalFecopCalcIE = valorbase1IE + valorbase2IE;
                            TotalFecopCalcSIE = valorbase1SIE + valorbase2SIE;
                            ViewBag.TotalFecopCalculadaIE = Convert.ToDouble(Math.Round(TotalFecopCalcIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalFecopCalculadaSIE = Convert.ToDouble(Math.Round(TotalFecopCalcSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            TotalFecopNfeIE = valorNfe1NormalIE + valorNfe1RetIE + valorNfe2NormalIE + valorNfe2RetIE;
                            TotalFecopNfeSIE = valorNfe1NormalSIE + valorNfe1RetSIE + valorNfe2NormalSIE + valorNfe2RetSIE;
                            ViewBag.TotalFecopNfeIE = Convert.ToDouble(Math.Round(TotalFecopNfeIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalFecopNfeSIE = Convert.ToDouble(Math.Round(TotalFecopNfeSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            // Valores da dief fecop
                            ViewBag.DifBase1IE = Convert.ToDouble(Math.Round(base1IE - baseNfe1NormalIE - baseNfe1RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.DifBase1SIE = Convert.ToDouble(Math.Round(base1SIE - baseNfe1NormalSIE - baseNfe1RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            difvalor1IE = valorbase1IE - valorNfe1NormalIE - valorNfe1RetIE - gnrePagaFecop1IE;
                            difvalor1SIE = valorbase1SIE - valorNfe1NormalSIE - valorNfe1RetSIE - gnrePagaFecop1SIE;
                            ViewBag.DifValor1IE = Convert.ToDouble(Math.Round(difvalor1IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.DifValor1SIE = Convert.ToDouble(Math.Round(difvalor1SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            ViewBag.DifBase2IE = Convert.ToDouble(Math.Round(base2IE - baseNfe2NormalIE - baseNfe2RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.DifBase2SIE = Convert.ToDouble(Math.Round(base2SIE - baseNfe2NormalSIE - baseNfe2RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            difvalor2IE = valorbase2IE - valorNfe2NormalIE - valorNfe2RetIE - gnrePagaFecop2IE;
                            difvalor2SIE = valorbase2SIE - valorNfe2NormalSIE - valorNfe2RetSIE - gnrePagaFecop2SIE;
                            ViewBag.DifValor2IE = Convert.ToDouble(Math.Round(difvalor2IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.DifValor2SIE = Convert.ToDouble(Math.Round(difvalor2SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            diftotalIE = difvalor1IE + difvalor2IE;
                            diftotalSIE = difvalor1SIE + difvalor2SIE;
                            ViewBag.DifTotalIE = Convert.ToDouble(Math.Round(diftotalIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.DifTotalSIE = Convert.ToDouble(Math.Round(diftotalSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            totalfecop1IE = difvalor1IE - base1fecopIE;
                            totalfecop1SIE = difvalor1SIE - base1fecopSIE;
                            ViewBag.TotalFecop1IE = Convert.ToDouble(Math.Round(totalfecop1IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalFecop1SIE = Convert.ToDouble(Math.Round(totalfecop1SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            totalfecop2IE = difvalor2IE - base2fecopIE;
                            totalfecop2SIE = difvalor2SIE - base2fecopSIE;
                            ViewBag.TotalFecop2IE = Convert.ToDouble(Math.Round(totalfecop2IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalFecop2SIE = Convert.ToDouble(Math.Round(totalfecop2IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            ViewBag.TotalFinalFecopCalculadaIE = Convert.ToDouble(Math.Round(totalfecop1IE + totalfecop2IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalFinalFecopCalculadaSIE = Convert.ToDouble(Math.Round(totalfecop1SIE + totalfecop2SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
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

                                totalIcmsIE = productsP.Select(_ => _.TotalICMS).Sum();
                                var totalFecop1 = productsP.Select(_ => _.TotalFecop).Sum();

                                impostoGeral = totalIcmsIE + totalFecop1;
                            }
                            else
                            {
                                impostoGeral = totalIcmsIE + totalIcmsSIE + totalFecop;
                            }

                            decimal? basefunef = impostoGeral - impostoIcms;
                            ViewBag.BaseFunef = Convert.ToDouble(Math.Round(Convert.ToDecimal(basefunef), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.Funef = company.Funef;
                            decimal taxaFunef = Convert.ToDecimal(basefunef * (company.Funef / 100));
                            ViewBag.TaxaFunef = Convert.ToDouble(Math.Round(taxaFunef, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            decimal totalImpostoIncentivo = impostoIcms + impostoFecop + taxaFunef;
                            if(typeTaxation == 1 && type != 8 && type != 7) {
                                ViewBag.TotalImpostoIncentivo = Convert.ToDouble(Math.Round(totalImpostoIncentivo + (diefStIE - icmsStnotaIE) + (diefStSIE - icmsStnotaSIE) + (totalfecop1IE + totalfecop2IE) + (totalfecop1SIE + totalfecop2SIE), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            }
                            else if(typeTaxation == 1 && type == 7)
                            {
                                ViewBag.TotalImpostoIncentivo = Convert.ToDouble(Math.Round(totalImpostoIncentivo, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            }

                            ViewBag.ImpostoGeral = impostoGeral;
                        }

                    }
                    else if (typeTaxation >= 2 && typeTaxation <= 5)
                    {
                        totalIcmsIE = result.Select(_ => _.IcmsApurado).Sum();

                        decimal valorNfe1Normal = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe1Ret = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                        decimal valorNfe2Normal = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe2Ret = Math.Round(Convert.ToDecimal(result.Where(_ => _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                        icmsStnoteS += valorNfe1Normal + valorNfe1Ret + valorNfe2Normal + valorNfe2Ret;
                        icmsStIE += valorNfe1Normal + valorNfe1Ret + valorNfe2Normal + valorNfe2Ret;

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

                    
                        ViewBag.TotalICMS = Convert.ToDouble(totalIcmsIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        /*decimal icmsTemp = 0;
                        if (gnrePaga < icmsSt)
                        {
                            icmsTemp = icmsSt - gnrePaga;
                        }
                        else
                        {
                            icmsTemp = icmsSt;
                        }*/
                        

                        valorDief = Convert.ToDecimal(totalIcmsIE - icmsStIE - gnrePaga + gnreNPaga);
                        
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
                }
                else if (type == 6)
                {
                    result = _service.FindByProducts(notes);


                    // Icms Substituição Tributária
                    decimal? gnrePagaST = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreSt).Distinct().Sum()), 2);
                    decimal? gnreNPagaST = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreNSt).Distinct().Sum()), 2);
                    decimal? icmsStST = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)).Select(_ => _.IcmsST).Sum()), 2);
                    decimal? totalApuradoST = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)).Select(_ => _.TotalICMS).Sum()), 2) - icmsStST + gnreNPagaST - gnrePagaST;
                    decimal? totalIcmsPagoST = Math.Round(Convert.ToDecimal(notes.Select(_ => _.IcmsSt).Sum()), 2);
                    decimal? totalAPagaST = totalApuradoST - totalIcmsPagoST;
                    int? qtdST = result.Where(_ => _.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)).Count();
                    ViewBag.QtdST = qtdST;
                    ViewBag.TotatlApuradoST = Convert.ToDouble(totalApuradoST).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoST = Convert.ToDouble(totalIcmsPagoST).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAPagaST = Convert.ToDouble(totalAPagaST).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Antecipação Parcial
                    decimal? gnrePagaAP = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreAp).Distinct().Sum()), 2);
                    decimal? gnreNPagaAP = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreNAp).Distinct().Sum()), 2);
                    decimal? icmsStAP = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsST).Sum()), 2);              
                    decimal? totalApuradoAP = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsApurado).Sum()), 2) - icmsStAP + gnreNPagaAP + gnrePagaAP;
                    var n = notes.Select(_ => _.IcmsAp);
                    decimal? totalIcmsPagoAP = Math.Round(Convert.ToDecimal(notes.Select(_ => _.IcmsAp).Sum()), 2);
                    decimal? totalAPagaAP = totalApuradoAP - totalIcmsPagoAP;
                    int? qtdAP = result.Where(_ => _.TaxationTypeId.Equals(1)).Count();
                    ViewBag.QtdAP = qtdAP;
                    ViewBag.TotalApuradoAP = Convert.ToDouble(totalApuradoAP).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoAP = Convert.ToDouble(totalIcmsPagoAP).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAPagaAP = Convert.ToDouble(totalAPagaAP).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Consumo
                    decimal? gnrePagaCO = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreCo).Distinct().Sum()), 2);
                    decimal? gnreNPagaCO = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreNCo).Distinct().Sum()), 2);
                    decimal? icmsStCO = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsST).Sum()), 2);
                    decimal? totalApuradoCO = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsApurado).Sum()), 2) - icmsStCO + gnreNPagaCO - gnrePagaCO;
                    decimal? totalIcmsPagoCO = Math.Round(Convert.ToDecimal(notes.Select(_ => _.IcmsCo).Sum()), 2);
                    decimal? totalAPagaCO = totalApuradoCO - totalIcmsPagoCO;
                    int? qtdCO = result.Where(_ => _.TaxationTypeId.Equals(2)).Count();
                    ViewBag.QtdCO = qtdCO;
                    ViewBag.TotalApuradoCO = Convert.ToDouble(totalApuradoCO).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotatIcmsPagoCO = Convert.ToDouble(totalIcmsPagoCO).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAPagaCO = Convert.ToDouble(totalAPagaCO).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Consumo para Revenda
                    decimal? icmsStCOR = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsST).Sum()), 2);
                    decimal? totalApuradoCOR = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsApurado).Sum()), 2) - icmsStCOR;
                    int? qtdCOR = result.Where(_ => _.TaxationTypeId.Equals(4)).Count();
                    ViewBag.QtdCOR = qtdCOR;
                    ViewBag.TotalApuradoCOR = Convert.ToDouble(totalApuradoCOR).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAPagaCOR = Convert.ToDouble(totalApuradoCOR).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Imobilizado
                    decimal? gnrePagaIM = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreIm).Distinct().Sum()), 2);
                    decimal? gnreNPagaIM = Math.Round(Convert.ToDecimal(result.Select(_ => _.Note.GnreNIm).Distinct().Sum()), 2);
                    decimal? icmsStIM = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsST).Sum()), 2);
                    decimal? totalApuradoIM = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsApurado).Sum()), 2) - icmsStIM + gnreNPagaIM - gnrePagaIM;
                    decimal? totalIcmsPagoIM = Math.Round(Convert.ToDecimal(notes.Select(_ => _.IcmsIm).Sum()), 2);
                    decimal? totalAPagaIM = totalApuradoIM - totalIcmsPagoIM;
                    int? qtdIM = result.Where(_ => _.TaxationTypeId.Equals(3)).Count();
                    ViewBag.QtdIM = qtdIM;
                    ViewBag.TotalApuradoIM = Convert.ToDouble(totalApuradoIM).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoIM = Convert.ToDouble(totalIcmsPagoIM).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAPagaIM = Convert.ToDouble(totalAPagaIM).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    // Antecipação Total
                    decimal? icmsStAT = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(8)).Select(_ => _.IcmsST).Sum()), 2);
                    decimal? totalApuradoAT = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(8)).Select(_ => _.TotalICMS).Sum()), 2) - icmsStAT;
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
                    decimal? totalAPagaGeral = totalApuradoGeral - totalIcmsPagoGeral;
                    ViewBag.QtdGeral = qtdGeral;
                    ViewBag.TotalApuradoGeral = Convert.ToDouble(totalApuradoGeral).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoGeral = Convert.ToDouble(totalIcmsPagoGeral).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAPagaGeral = Convert.ToDouble(totalAPagaGeral).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    
                }
                else if (type == 9)
                {
                    result = _service.FindByProductsType(notesS.ToList(), typeTaxation);
                    List<List<string>> fornecedores = new List<List<string>>();
                    decimal icmsStTotal = 0, fecopStTotal = 0, gnrePagaTotal = 0, gnreNPagaTotal = 0, gnrefecopPagaTotal = 0;
                    
                    foreach (var prod in result)
                    {
                        int pos = -1;
                        for(int i = 0; i < fornecedores.Count(); i++)
                        {
                            if (prod.NoteId.Equals(Convert.ToInt32(fornecedores[i][0])))
                            {
                                pos = i;
                                break;
                            }
                        }

                        if (pos < 0)
                        {
                            List<string> forenecedor = new List<string>();
                            forenecedor.Add(prod.NoteId.ToString());
                            forenecedor.Add(prod.Note.Dhemi.ToString("dd-MM-yyyy"));
                            forenecedor.Add(prod.Note.Nnf);
                            forenecedor.Add(prod.Note.Xnome);
                            forenecedor.Add("0,00");
                            if (typeTaxation.Equals(1))
                            {
                                forenecedor.Add(prod.Note.GnreSt.ToString());
                                forenecedor.Add(prod.Note.GnreNSt.ToString());
                                gnrePagaTotal += Convert.ToDecimal(prod.Note.GnreSt);
                                gnreNPagaTotal += Convert.ToDecimal(prod.Note.GnreNSt);
                            }
                            else if (typeTaxation.Equals(2))
                            {
                                forenecedor.Add(prod.Note.GnreAp.ToString());
                                forenecedor.Add(prod.Note.GnreNAp.ToString());
                                gnrePagaTotal += Convert.ToDecimal(prod.Note.GnreAp);
                                gnreNPagaTotal += Convert.ToDecimal(prod.Note.GnreNAp);
                            }
                            else if (typeTaxation.Equals(3))
                            {
                                forenecedor.Add(prod.Note.GnreCo.ToString());
                                forenecedor.Add(prod.Note.GnreNCo.ToString());
                                gnrePagaTotal += Convert.ToDecimal(prod.Note.GnreCo);
                                gnreNPagaTotal += Convert.ToDecimal(prod.Note.GnreNCo);
                            }
                            else if (typeTaxation.Equals(5))
                            {
                                forenecedor.Add(prod.Note.GnreIm.ToString());
                                forenecedor.Add(prod.Note.GnreNIm.ToString());
                                gnrePagaTotal += Convert.ToDecimal(prod.Note.GnreIm);
                                gnreNPagaTotal += Convert.ToDecimal(prod.Note.GnreNIm);
                            }
                            else
                            {
                                forenecedor.Add("0,00");
                                forenecedor.Add("0,00");
                                gnrePagaTotal += 0;
                                gnreNPagaTotal += 0;
                            }
                            forenecedor.Add("0,00");
                            forenecedor.Add((prod.Note.FecopGnre1 + prod.Note.FecopGnre2).ToString());
                            gnrefecopPagaTotal += Convert.ToDecimal(prod.Note.FecopGnre1 + prod.Note.FecopGnre2);
                            fornecedores.Add(forenecedor);
                            pos = fornecedores.Count() - 1;
                        }

                        fornecedores[pos][4] = (Convert.ToDecimal(fornecedores[pos][4]) + prod.IcmsST).ToString();
                        fornecedores[pos][7] = (Convert.ToDecimal(fornecedores[pos][7]) + prod.VfcpST + prod.VfcpSTRet).ToString();
                        icmsStTotal += Convert.ToDecimal(prod.IcmsST);
                        fecopStTotal += (Convert.ToDecimal(prod.VfcpST + prod.VfcpSTRet));
                    }
                    List<List<string>> fornecedoresFinal = new List<List<string>>();
                    for (int i = 0; i < fornecedores.Count(); i++)
                    {
                        if(Convert.ToDecimal(fornecedores[i][4]) > 0 || Convert.ToDecimal(fornecedores[i][7]) > 0)
                        {
                            fornecedoresFinal.Add(fornecedores[i]);
                        }
                    }
                    ViewBag.Fornecedores = fornecedoresFinal;
                    ViewBag.IcmsStTotal = Convert.ToDouble(Math.Round(icmsStTotal, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.FecopStTotal = Convert.ToDouble(Math.Round(fecopStTotal, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnrePagaTotal = Convert.ToDouble(Math.Round(gnrePagaTotal, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnreNPagaTotal = Convert.ToDouble(Math.Round(gnreNPagaTotal, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnreFecopPagaTotal = Convert.ToDouble(Math.Round(gnrefecopPagaTotal, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

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
                ViewBag.IcmsStNoteS = Convert.ToDouble(Math.Round(icmsStnoteS, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                ViewBag.IcmsStNoteI = Convert.ToDouble(Math.Round(icmsStnoteI, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                return View(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
           
    }
}