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

                var icmsStnoteSIE = _service.FindBySubscription(notesS.ToList(), typeTaxation);
                var icmsStnoteIE = _service.FindBySubscription(notesI.ToList(), typeTaxation);

                ViewBag.SocialName = company.SocialName;
                ViewBag.Document = company.Document;
                ViewBag.Year = year;
                ViewBag.Month = month;
                ViewBag.TaxationType = typeTaxation;
                ViewBag.type = type;

                ViewBag.Incetive = company.Incentive;
                ViewBag.TypeIncetive = company.TipoApuracao;


                if (type == 1 || type == 2 || type == 4 || type == 5 || type == 7 || type == 8)
                {    
                    if (type == 2)
                    {
                        notes = notes.Where(_ => _.Nnf.Equals(nota)).ToList();
                        total = notes.Select(_ => _.Vnf).Sum();
                        notesS = notes.Where(_ => _.Iest == "");
                        notesI = notes.Where(_ => _.Iest != "");
                        icmsStnoteSIE = _service.FindBySubscription(notesS.ToList(), typeTaxation);
                        icmsStnoteIE = _service.FindBySubscription(notesI.ToList(), typeTaxation);
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
                    decimal? totalIcmsIE = 0, totalIcmsSIE = 0, valorDiefIE = 0; 
                    ViewBag.TotalICMSST = Convert.ToDouble(icmsStIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    if (typeTaxation == 1 || typeTaxation == 7)
                    {
                        decimal totalFreteIE = 0;

                        foreach (var prod in result)
                        {
                            if (!prod.Note.Iest.Equals(""))
                            {
                                if (Convert.ToDecimal(prod.Aliqinterna) > 0)
                                {
                                    totalFreteIE += Convert.ToDecimal((prod.Freterateado * prod.Aliqinterna) / 100);
                                }
                            }
                        }

                        ViewBag.TotalFreteIE = Convert.ToDouble(totalFreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

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
                        decimal diefStSIE = Convert.ToDecimal((totalIcmsSIE + totalFreteIE) - icmsStSIE - gnrePagaSIE + gnreNPagaSIE);
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

                        //Relatorio das empresas incentivadas
                        if (company.Incentive == true && company.TipoApuracao == true)
                        {
                            //Produtos não incentivados
                            var productsNormal = _service.FindByNormal(notes);
                            decimal? totalIcmsNormalIE = productsNormal.Where(_ => _.TaxationType.Type.Equals("ST") && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum();
                            decimal? totalIcmsNormalSIE = productsNormal.Where(_ => _.TaxationType.Type.Equals("ST") && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum();

                            var notasTaxationNormal = productsNormal.Select(_ => _.Note).Distinct();

                            foreach (var prod in productsNormal)
                            {
                                if (!prod.Note.Iest.Equals(""))
                                {
                                    if (Convert.ToDecimal(prod.Aliqinterna) > 0)
                                    {
                                        totalFreteIE += Convert.ToDecimal((prod.Freterateado * prod.Aliqinterna) / 100);
                                    }
                                }
                            }

                            ViewBag.TotalFreteIE = Convert.ToDouble(totalFreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

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
                            diefStSIE = Convert.ToDecimal((totalIcmsNormalSIE + totalFreteIE) - icmsStSIE - gnrePagaSIE + gnreNPagaSIE);
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
                                ViewBag.TotalImpostoIncentivo = Convert.ToDouble(Math.Round(totalImpostoIncentivo + (diefStSIE - icmsStnotaSIE) + (totalfecop1SIE + totalfecop2SIE), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
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
                        decimal totalFreteIE = 0;
                        foreach(var prod in result)
                        {
                            if (!prod.Note.Iest.Equals(""))
                            {
                                if (Convert.ToDecimal(prod.Diferencial) > 0)
                                {
                                    totalFreteIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                                }
                            }
                        }
                        ViewBag.TotalFreteIE = Convert.ToDouble(totalFreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        
                        totalIcmsIE = result.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.IcmsApurado).Sum();
                        totalIcmsSIE = result.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.IcmsApurado).Sum();

                        decimal valorNfe1NormalSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe1RetSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                        decimal valorNfe2NormalSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe2RetSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                        decimal valorNfe1NormalIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe1RetIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                        decimal valorNfe2NormalIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe2RetIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                        icmsStnoteSIE += valorNfe1NormalSIE + valorNfe1RetSIE + valorNfe2NormalSIE + valorNfe2RetSIE;
                        icmsStnoteIE += valorNfe1NormalIE + valorNfe1RetIE + valorNfe2NormalIE + valorNfe2RetIE;

                        decimal gnrePagaIE = 0, gnreNPagaIE = 0, gnrePagaSIE = 0, gnreNPagaSIE = 0;
                        decimal? icmsApIE = 0, icmsApSIE = 0;
                        if (typeTaxation == 2)
                        {
                            gnrePagaIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreAp).Distinct().Sum()), 2);
                            gnreNPagaIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreNAp).Distinct().Sum()), 2);
                            gnrePagaSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreAp).Distinct().Sum()), 2);
                            gnreNPagaSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreNAp).Distinct().Sum()), 2);
                            icmsApIE = result.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.IcmsAp).Distinct().Sum();
                            icmsApSIE = result.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.IcmsAp).Distinct().Sum();
                        }
                        else if (typeTaxation == 3)
                        {
                            gnrePagaIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreCo).Distinct().Sum()), 2);
                            gnreNPagaIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreNCo).Distinct().Sum()), 2);
                            gnrePagaSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreCo).Distinct().Sum()), 2);
                            gnreNPagaSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreNCo).Distinct().Sum()), 2);
                            icmsApIE = result.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.IcmsCo).Distinct().Sum();
                            icmsApSIE = result.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.IcmsCo).Distinct().Sum();

                        }
                        else if (typeTaxation == 5)
                        {
                            gnrePagaIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreIm).Distinct().Sum()), 2);
                            gnreNPagaIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreNIm).Distinct().Sum()), 2);
                            gnrePagaSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreIm).Distinct().Sum()), 2);
                            gnreNPagaSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreNIm).Distinct().Sum()), 2);
                            icmsApIE = result.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.IcmsIm).Distinct().Sum();
                            icmsApSIE = result.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.IcmsIm).Distinct().Sum();
                        }

                        ViewBag.TotalGNREnPagaIE = Convert.ToDouble(gnreNPagaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGNREPagaIE = Convert.ToDouble(gnrePagaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGNREnPagaSIE = Convert.ToDouble(gnreNPagaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGNREPagaSIE = Convert.ToDouble(gnrePagaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                        ViewBag.TotalICMS = Convert.ToDouble(totalIcmsIE + totalIcmsSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalICMSIE = Convert.ToDouble(totalIcmsIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalICMSSIE = Convert.ToDouble(totalIcmsSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        /*decimal icmsTemp = 0;
                        if (gnrePaga < icmsSt)
                        {
                            icmsTemp = icmsSt - gnrePaga;
                        }
                        else
                        {
                            icmsTemp = icmsSt;
                        }*/


                        valorDiefIE = Convert.ToDecimal(totalIcmsIE - icmsStnoteIE - gnrePagaIE + gnreNPagaIE);
                        var valorDiefSIE = Convert.ToDecimal((totalIcmsSIE + totalFreteIE) - icmsStnoteSIE - gnrePagaSIE + gnreNPagaSIE);

                        ViewBag.ValorDiefIE = Convert.ToDouble(valorDiefIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ValorDiefSIE = Convert.ToDouble(valorDiefSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.IcmsApIE = Convert.ToDouble(icmsApIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsApSIE = Convert.ToDouble(icmsApSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.IcmsPagarIE = Convert.ToDouble(valorDiefIE - icmsApIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsPagarSIE = Convert.ToDouble(valorDiefSIE - icmsApSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

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

                    // Substituição Tributária

                    decimal totalFreteSTIE = 0;

                    foreach (var prod in result)
                    {
                        if (!prod.Note.Iest.Equals("") && (prod.TaxationTypeId.Equals(5) || prod.TaxationTypeId.Equals(6)))
                        {
                            if (Convert.ToDecimal(prod.Aliqinterna) > 0)
                            {
                                totalFreteSTIE += Convert.ToDecimal((prod.Freterateado * prod.Aliqinterna) / 100);
                            }
                        }
                    }


                    ViewBag.TotalFreteSTIE = Convert.ToDouble(totalFreteSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal? gnrePagaSTIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreSt).Sum()), 2);
                    decimal? gnreNPagaSTIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ =>_.GnreNSt).Sum()), 2);
                    decimal? gnrePagaSTSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreSt).Sum()), 2);
                    decimal? gnreNPagaSTSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNSt).Sum()), 2);

                    decimal? icmsStSTIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.IcmsST).Sum()), 2);
                    decimal? icmsStSTSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.IcmsST).Sum()), 2);
                    decimal? totalApuradoSTIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.TotalICMS).Sum()), 2);
                    decimal? totalApuradoSTSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.TotalICMS).Sum()), 2);
                    decimal? totalDiefSTSIE = (totalApuradoSTSIE + totalFreteSTIE) - icmsStSTSIE + gnreNPagaSTSIE - gnrePagaSTSIE;
                    decimal? totalDiefSTIE = totalApuradoSTIE - icmsStSTIE + gnreNPagaSTIE - gnrePagaSTIE;
                    int? qtdSTSIE = result.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Count();
                    int? qtdSTIE = result.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Count();

                    ViewBag.QtdSTSIE = qtdSTSIE;
                    ViewBag.QtdSTIE = qtdSTIE;
                    ViewBag.TotatlApuradoSTIE = Convert.ToDouble(totalApuradoSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotatlApuradoSTSIE = Convert.ToDouble(totalApuradoSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoSTIE = Convert.ToDouble(icmsStSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoSTSIE = Convert.ToDouble(icmsStSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    
                    ViewBag.GnrePagaSTSIE = Convert.ToDouble(gnrePagaSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnrePagaSTIE = Convert.ToDouble(gnrePagaSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnreNPagaSTSIE = Convert.ToDouble(gnreNPagaSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnreNPagaSTIE = Convert.ToDouble(gnreNPagaSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    
                    ViewBag.TotalDiefSTSIE = Convert.ToDouble(totalDiefSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDiefSTIE = Convert.ToDouble(totalDiefSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal icmsStnotaIE = Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsSt).Sum());
                    decimal icmsStnotaSIE = Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsSt).Sum());
                    ViewBag.IcmsSTIE = Convert.ToDouble(icmsStnotaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsSTSIE = Convert.ToDouble(icmsStnotaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal IcmsAPagarSTSIE = Convert.ToDecimal(totalDiefSTSIE - icmsStnotaSIE);
                    decimal IcmsAPagarSTIE = Convert.ToDecimal(totalDiefSTIE - icmsStnotaIE);
                    ViewBag.IcmsAPagarSTSIE = Convert.ToDouble(IcmsAPagarSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsAPagarSTIE = Convert.ToDouble(IcmsAPagarSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal valorbase1STIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                    decimal valorbase1STSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                    decimal valorbase2STIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                    decimal valorbase2STSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);

                    decimal TotalFecopCalcSTIE = valorbase1STIE + valorbase2STIE;
                    decimal TotalFecopCalcSTSIE = valorbase1STSIE + valorbase2STSIE;
                    ViewBag.TotalFecopCalculadaSTIE = Convert.ToDouble(Math.Round(TotalFecopCalcSTIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFecopCalculadaSTSIE = Convert.ToDouble(Math.Round(TotalFecopCalcSTSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

      
                    decimal valorNfe1NormalSTIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetSTIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe1NormalSTSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetSTSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal valorNfe2NormalSTIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetSTIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalSTSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetSTSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    
                    decimal TotalFecopNfeSTIE = valorNfe1NormalSTIE + valorNfe1RetSTIE + valorNfe2NormalSTIE + valorNfe2RetSTIE;
                    decimal TotalFecopNfeSTSIE = valorNfe1NormalSTSIE + valorNfe1RetSTSIE + valorNfe2NormalSTSIE + valorNfe2RetSTSIE;
                    ViewBag.TotalFecopNfeSTIE = Convert.ToDouble(Math.Round(TotalFecopNfeSTIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFecopNfeSTSIE = Convert.ToDouble(Math.Round(TotalFecopNfeSTSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    
                    decimal gnreNPagaFecopSTIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2);
                    decimal gnreNPagaFecopSTSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2);
                    ViewBag.GNREnPagaFecopSTIE = Convert.ToDouble(gnreNPagaFecopSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GNREnPagaFecopSTSIE = Convert.ToDouble(gnreNPagaFecopSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal gnrePagaFecop1STIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2);
                    decimal gnrePagaFecop1STSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2);
                    decimal gnrePagaFecop2STIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2);
                    decimal gnrePagaFecop2STSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2);
                    ViewBag.GNREPagaFecopSTIE = Convert.ToDouble(gnrePagaFecop2STIE + gnrePagaFecop1STIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GNREPagaFecopSTSIE = Convert.ToDouble(gnrePagaFecop2STSIE + gnrePagaFecop1STSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal totalGnreFecopSTIE = gnrePagaFecop1STIE + gnrePagaFecop2STIE;
                    ViewBag.TotalGnreFecopSTIE = Convert.ToDouble(totalGnreFecopSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    decimal totalGnreFecopSTSIE = gnrePagaFecop1STSIE + gnrePagaFecop2STSIE;
                    ViewBag.TotalGnreFecopSTSIE = Convert.ToDouble(totalGnreFecopSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal totalfecopDiefSTIE = TotalFecopCalcSTIE - totalGnreFecopSTIE + (gnrePagaFecop2STIE + gnrePagaFecop1STIE) - TotalFecopNfeSTIE;
                    decimal totalfecopDiefSTSIE = TotalFecopCalcSTSIE - totalGnreFecopSTSIE + (gnrePagaFecop2STSIE + gnrePagaFecop1STSIE) - TotalFecopNfeSTSIE;
                    ViewBag.TotalFecopDiefSTIE = Convert.ToDouble(Math.Round(totalfecopDiefSTIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFecopDiefSTSIE = Convert.ToDouble(Math.Round(totalfecopDiefSTSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal? icmsFecop1STIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2);
                    decimal? icmsFecop1STSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2);
                    decimal? icmsFecop2STIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2);
                    decimal? icmsFecop2STSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2);
                    ViewBag.IcmsFecopSTIE = Convert.ToDouble(icmsFecop1STIE + icmsFecop2STIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsFecopSTSIE = Convert.ToDouble(icmsFecop1STSIE + icmsFecop2STSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    ViewBag.TotalFinalFecopCalculadaSTIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalfecopDiefSTIE - (icmsFecop1STIE + icmsFecop2STIE)), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFinalFecopCalculadaSTSIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalfecopDiefSTSIE - (icmsFecop1STSIE + icmsFecop2STSIE)), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    //Produtos Incentivo
                    if (company.Incentive == true && company.TipoApuracao == true)
                    {
                        //Produtos não incentivados
                        var productsNormal = _service.FindByNormal(notes);
                        productsNormal = productsNormal.Where(_ => _.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)).ToList();


                        totalFreteSTIE = 0;

                        foreach (var prod in productsNormal)
                        {
                            if (!prod.Note.Iest.Equals("") && (prod.TaxationTypeId.Equals(5) || prod.TaxationTypeId.Equals(6)))
                            {
                                if (Convert.ToDecimal(prod.Aliqinterna) > 0)
                                {
                                    totalFreteSTIE += Convert.ToDecimal((prod.Freterateado * prod.Aliqinterna) / 100);
                                }
                            }
                        }

                        ViewBag.TotalFreteSTIE = Convert.ToDouble(totalFreteSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        gnrePagaSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreSt).Distinct().Sum()), 2);
                        gnreNPagaSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreNSt).Distinct().Sum()), 2);
                        gnrePagaSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreSt).Distinct().Sum()), 2);
                        gnreNPagaSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreNSt).Distinct().Sum()), 2);

                        icmsStSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.IcmsST).Sum()), 2);
                        icmsStSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.IcmsST).Sum()), 2);
                        totalApuradoSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.TotalICMS).Sum()), 2);
                        totalApuradoSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.TotalICMS).Sum()), 2);
                        totalDiefSTSIE = (totalApuradoSTSIE + totalFreteSTIE) - icmsStSTSIE + gnreNPagaSTSIE - gnrePagaSTSIE;
                        totalDiefSTIE = totalApuradoSTIE - icmsStSTIE + gnreNPagaSTIE - gnrePagaSTIE;

                        ViewBag.TotatlApuradoSTIE = Convert.ToDouble(totalApuradoSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotatlApuradoSTSIE = Convert.ToDouble(totalApuradoSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalIcmsPagoSTIE = Convert.ToDouble(icmsStSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalIcmsPagoSTSIE = Convert.ToDouble(icmsStSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.GnrePagaSTSIE = Convert.ToDouble(gnrePagaSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.GnrePagaSTIE = Convert.ToDouble(gnrePagaSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.GnreNPagaSTSIE = Convert.ToDouble(gnreNPagaSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.GnreNPagaSTIE = Convert.ToDouble(gnreNPagaSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.TotalDiefSTSIE = Convert.ToDouble(totalDiefSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalDiefSTIE = Convert.ToDouble(totalDiefSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        icmsStnotaIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.IcmsSt).Distinct().Sum()), 2);
                        icmsStnotaSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.IcmsSt).Distinct().Sum()), 2);
                        ViewBag.IcmsSTIE = Convert.ToDouble(icmsStnotaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsSTSIE = Convert.ToDouble(icmsStnotaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        IcmsAPagarSTSIE = Convert.ToDecimal(totalDiefSTSIE - icmsStnotaSIE);
                        IcmsAPagarSTIE = Convert.ToDecimal(totalDiefSTIE - icmsStnotaIE);
                        ViewBag.IcmsAPagarSTSIE = Convert.ToDouble(IcmsAPagarSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsAPagarSTIE = Convert.ToDouble(IcmsAPagarSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        valorbase1STIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                        valorbase1STSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                        valorbase2STIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                        valorbase2STSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);

                        TotalFecopCalcSTIE = valorbase1STIE + valorbase2STIE;
                        TotalFecopCalcSTSIE = valorbase1STSIE + valorbase2STSIE;
                        ViewBag.TotalFecopCalculadaSTIE = Convert.ToDouble(Math.Round(TotalFecopCalcSTIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFecopCalculadaSTSIE = Convert.ToDouble(Math.Round(TotalFecopCalcSTSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                        valorNfe1NormalSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        valorNfe1RetSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                        valorNfe1NormalSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        valorNfe1RetSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);

                        valorNfe2NormalSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        valorNfe2RetSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                        valorNfe2NormalSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        valorNfe2RetSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                        TotalFecopNfeSTIE = valorNfe1NormalSTIE + valorNfe1RetSTIE + valorNfe2NormalSTIE + valorNfe2RetSTIE;
                        TotalFecopNfeSTSIE = valorNfe1NormalSTSIE + valorNfe1RetSTSIE + valorNfe2NormalSTSIE + valorNfe2RetSTSIE;
                        ViewBag.TotalFecopNfeSTIE = Convert.ToDouble(Math.Round(TotalFecopNfeSTIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFecopNfeSTSIE = Convert.ToDouble(Math.Round(TotalFecopNfeSTSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        gnreNPagaFecopSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2);
                        gnreNPagaFecopSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2);
                        ViewBag.GNREnPagaFecopSTIE = Convert.ToDouble(gnreNPagaFecopSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.GNREnPagaFecopSTSIE = Convert.ToDouble(gnreNPagaFecopSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        gnrePagaFecop1STIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2);
                        gnrePagaFecop1STSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2);
                        gnrePagaFecop2STIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2);
                        gnrePagaFecop2STSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2);
                        ViewBag.GNREPagaFecopSTIE = Convert.ToDouble(gnrePagaFecop2STIE + gnrePagaFecop1STIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.GNREPagaFecopSTSIE = Convert.ToDouble(gnrePagaFecop2STSIE + gnrePagaFecop1STSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        totalGnreFecopSTIE = gnrePagaFecop1STIE + gnrePagaFecop2STIE;
                        ViewBag.TotalGnreFecopSTIE = Convert.ToDouble(totalGnreFecopSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        totalGnreFecopSTSIE = gnrePagaFecop1STSIE + gnrePagaFecop2STSIE;
                        ViewBag.TotalGnreFecopSTSIE = Convert.ToDouble(totalGnreFecopSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        totalfecopDiefSTIE = TotalFecopCalcSTIE - totalGnreFecopSTIE + (gnrePagaFecop2STIE + gnrePagaFecop1STIE) - TotalFecopNfeSTIE;
                        totalfecopDiefSTSIE = TotalFecopCalcSTSIE - totalGnreFecopSTSIE + (gnrePagaFecop2STSIE + gnrePagaFecop1STSIE) - TotalFecopNfeSTSIE;
                        ViewBag.TotalFecopDiefSTIE = Convert.ToDouble(Math.Round(totalfecopDiefSTIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFecopDiefSTSIE = Convert.ToDouble(Math.Round(totalfecopDiefSTSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        icmsFecop1STIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2);
                        icmsFecop1STSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2);
                        icmsFecop2STIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2);
                        icmsFecop2STSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2);
                        ViewBag.IcmsFecopSTIE = Convert.ToDouble(icmsFecop1STIE + icmsFecop2STIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsFecopSTSIE = Convert.ToDouble(icmsFecop1STSIE + icmsFecop2STSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.TotalFinalFecopCalculadaSTIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalfecopDiefSTIE - (icmsFecop1STIE + icmsFecop2STIE)), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFinalFecopCalculadaSTSIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalfecopDiefSTSIE - (icmsFecop1STSIE + icmsFecop2STSIE)), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //Produto incentivados
                        var productsIncentive = _service.FindByIncentive(notes);
                        productsIncentive = productsIncentive.Where(_ => _.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)).ToList();

                        ViewBag.Icms = company.Icms;
                        ViewBag.Fecop = company.Fecop;

                        decimal baseIcms = productsIncentive.Select(_ => _.Vbasecalc).Sum();
                        ViewBag.Base = Convert.ToDouble(Math.Round(baseIcms, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        decimal impostoIcms = Convert.ToDecimal(baseIcms * (company.Icms / 100));
                        ViewBag.ImpostoIcms = Convert.ToDouble(Math.Round(impostoIcms, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        decimal impostoFecop = Convert.ToDecimal(baseIcms * (company.Fecop / 100));
                        ViewBag.ImpostoFecop = Convert.ToDouble(Math.Round(impostoFecop, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal? impostoGeral = 0;

                        impostoGeral = result.Where(_ => _.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)).Select(_ => _.TotalICMS).Sum() + result.Where(_ => _.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)).Select(_ => _.TotalFecop).Sum();

                        decimal? basefunef = impostoGeral - impostoIcms;
                        ViewBag.BaseFunef = Convert.ToDouble(Math.Round(Convert.ToDecimal(basefunef), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.Funef = company.Funef;
                        decimal taxaFunef = Convert.ToDecimal(basefunef * (company.Funef / 100));
                        ViewBag.TaxaFunef = Convert.ToDouble(Math.Round(taxaFunef, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal totalImpostoIncentivo = impostoIcms + impostoFecop + taxaFunef;

                        ViewBag.TotalImpostoIncentivo = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalImpostoIncentivo + IcmsAPagarSTSIE + (totalfecopDiefSTSIE - (icmsFecop1STSIE + icmsFecop2STSIE))), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.ImpostoGeral = impostoGeral;

                    }


                    // Antecipação Parcial

                    decimal totalFreteAPIE = 0;

                    foreach (var prod in result)
                    {
                        if (!prod.Note.Iest.Equals("") && prod.TaxationTypeId.Equals(1))
                        {
                            if (Convert.ToDecimal(prod.Diferencial) > 0)
                            {
                                totalFreteAPIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                            }
                        }
                    }

                    ViewBag.TotalFreteAPIE = Convert.ToDouble(totalFreteAPIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal valorNfe1NormalAPSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetAPSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalAPSIE = Math.Round(Convert.ToDecimal(result.Where( _ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetAPSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe1NormalAPIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetAPIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalAPIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetAPIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal? gnrePagaAPIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreAp).Sum()), 2);
                    decimal? gnreNPagaAPIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNAp).Sum()), 2);
                    decimal? gnrePagaAPSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreAp).Sum()), 2);
                    decimal? gnreNPagaAPSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNAp).Sum()), 2);
                    
                    decimal? icmsStAPIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalAPIE + valorNfe1RetAPIE + valorNfe2NormalAPIE + valorNfe2RetAPIE;
                    decimal? icmsStAPSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalAPSIE + valorNfe1RetAPSIE + valorNfe2NormalAPSIE + valorNfe2RetAPSIE;
                    decimal? totalApuradoAPIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalApuradoAPSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalDiefAPSIE = (totalApuradoAPSIE + totalFreteAPIE) - icmsStAPSIE + gnreNPagaAPSIE - gnrePagaAPSIE;
                    decimal? totalDiefAPIE = totalApuradoAPIE - icmsStAPIE + gnreNPagaAPIE - gnrePagaAPIE;
                    int? qtdAPSIE = result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Count();
                    int? qtdAPIE = result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Count();

                    ViewBag.QtdAPSIE = qtdAPSIE;
                    ViewBag.QtdAPIE = qtdAPIE;
                    ViewBag.TotatlApuradoAPIE = Convert.ToDouble(totalApuradoAPIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotatlApuradoAPSIE = Convert.ToDouble(totalApuradoAPSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoAPIE = Convert.ToDouble(icmsStAPIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoAPSIE = Convert.ToDouble(icmsStAPSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    
                    ViewBag.GnrePagaAPSIE = Convert.ToDouble(gnrePagaAPSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnrePagaAPIE = Convert.ToDouble(gnrePagaAPIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnreNPagaAPSIE = Convert.ToDouble(gnreNPagaAPSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnreNPagaAPIE = Convert.ToDouble(gnreNPagaAPIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    
                    ViewBag.TotalDiefAPSIE = Convert.ToDouble(totalDiefAPSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDiefAPIE = Convert.ToDouble(totalDiefAPIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal icmsAPnotaIE = Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsAp).Sum());
                    decimal icmsAPnotaSIE = Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsAp).Sum());
                    ViewBag.IcmsAPIE = Convert.ToDouble(icmsAPnotaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsAPSIE = Convert.ToDouble(icmsAPnotaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal IcmsAPagarAPSIE = Convert.ToDecimal(totalDiefAPSIE - icmsAPnotaSIE);
                    decimal IcmsAPagarAPIE = Convert.ToDecimal(totalDiefAPIE - icmsAPnotaIE);
                    ViewBag.IcmsAPagarAPSIE = Convert.ToDouble(IcmsAPagarAPSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsAPagarAPIE = Convert.ToDouble(IcmsAPagarAPIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    // Consumo

                    decimal totalFreteCOIE = 0;

                    foreach (var prod in result)
                    {
                        if (!prod.Note.Iest.Equals("") && prod.TaxationTypeId.Equals(2))
                        {
                            if (Convert.ToDecimal(prod.Diferencial) > 0)
                            {
                                totalFreteCOIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                            }
                        }
                    }

                    ViewBag.TotalFreteCOIE = Convert.ToDouble(totalFreteCOIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal valorNfe1NormalCOSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(2) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetCOSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(2) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalCOSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(2) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetCOSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(2) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe1NormalCOIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(2) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetCOIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(2) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalCOIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(2) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetCOIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(2) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal? gnrePagaCOIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreCo).Sum()), 2);
                    decimal? gnreNPagaCOIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNCo).Sum()), 2);
                    decimal? gnrePagaCOSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreCo).Sum()), 2);
                    decimal? gnreNPagaCOSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNCo).Sum()), 2);
                    
                    decimal? icmsStCOIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalCOIE + valorNfe1RetCOIE + valorNfe2NormalCOIE + valorNfe2RetCOIE;
                    decimal? icmsStCOSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalCOSIE + valorNfe1RetCOSIE + valorNfe2NormalCOSIE + valorNfe2RetCOSIE;
                    decimal? totalApuradoCOIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalApuradoCOSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalDiefCOSIE = (totalApuradoCOSIE + totalFreteCOIE) - icmsStCOSIE + gnreNPagaCOSIE - gnrePagaCOSIE;
                    decimal? totalDiefCOIE = totalApuradoCOIE - icmsStCOIE + gnreNPagaCOIE - gnrePagaCOIE;
                    int? qtdCOSIE = result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Count();
                    int? qtdCOIE = result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Count();

                    ViewBag.QtdCOSIE = qtdCOSIE;
                    ViewBag.QtdCOIE = qtdCOIE;
                    ViewBag.TotatlApuradoCOIE = Convert.ToDouble(totalApuradoCOIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotatlApuradoCOSIE = Convert.ToDouble(totalApuradoCOSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoCOIE = Convert.ToDouble(icmsStCOIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoCOSIE = Convert.ToDouble(icmsStCOSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    
                    ViewBag.GnrePagaCOSIE = Convert.ToDouble(gnrePagaCOSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnrePagaCOIE = Convert.ToDouble(gnrePagaCOIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnreNPagaCOSIE = Convert.ToDouble(gnreNPagaCOSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnreNPagaCOIE = Convert.ToDouble(gnreNPagaCOIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    
                    ViewBag.TotalDiefCOSIE = Convert.ToDouble(totalDiefCOSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDiefCOIE = Convert.ToDouble(totalDiefCOIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal icmsCOnotaIE = Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsCo).Sum());
                    decimal icmsCOnotaSIE = Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsCo).Sum());
                    ViewBag.IcmsCOIE = Convert.ToDouble(icmsCOnotaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsCOSIE = Convert.ToDouble(icmsCOnotaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal IcmsAPagarCOSIE = Convert.ToDecimal(totalDiefCOSIE - icmsCOnotaSIE);
                    decimal IcmsAPagarCOIE = Convert.ToDecimal(totalDiefCOIE - icmsCOnotaIE);
                    ViewBag.IcmsAPagarCOSIE = Convert.ToDouble(IcmsAPagarCOSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsAPagarCOIE = Convert.ToDouble(IcmsAPagarCOIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    // Consumo para Revenda

                    decimal totalFreteCORIE = 0;

                    foreach (var prod in result)
                    {
                        if (!prod.Note.Iest.Equals("") && prod.TaxationTypeId.Equals(4))
                        {
                            if (Convert.ToDecimal(prod.Diferencial) > 0)
                            {
                                totalFreteCORIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                            }
                        }
                    }

                    ViewBag.TotalFreteCORIE = Convert.ToDouble(totalFreteCORIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal valorNfe1NormalCORSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(4) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetCORSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(4) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalCORSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(4) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetCORSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(4) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe1NormalCORIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(4) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetCORIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(4) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalCORIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(4) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetCORIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(4) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal? icmsStCORIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalCORIE + valorNfe1RetCORIE + valorNfe2NormalCORIE + valorNfe2RetCORIE;
                    decimal? icmsStCORSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalCORSIE + valorNfe1RetCORSIE + valorNfe2NormalCORSIE + valorNfe2RetCORSIE;
                    decimal? totalApuradoCORIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalApuradoCORSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalDiefCORSIE = (totalApuradoCORSIE + totalFreteCORIE) - icmsStCORSIE;
                    decimal? totalDiefCORIE = totalApuradoCORIE - icmsStCORIE;
                    int? qtdCORSIE = result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Count();
                    int? qtdCORIE = result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Count();

                    ViewBag.QtdCORSIE = qtdCORSIE;
                    ViewBag.QtdCORIE = qtdCORIE;
                    ViewBag.TotatlApuradoCORIE = Convert.ToDouble(totalApuradoCORIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotatlApuradoCORSIE = Convert.ToDouble(totalApuradoCORSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoCORIE = Convert.ToDouble(icmsStCORIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoCORSIE = Convert.ToDouble(icmsStCORSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    ViewBag.TotalDiefCORSIE = Convert.ToDouble(totalDiefCORSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDiefCORIE = Convert.ToDouble(totalDiefCORIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal IcmsAPagarCORSIE = Convert.ToDecimal(totalDiefCORSIE);
                    decimal IcmsAPagarCORIE = Convert.ToDecimal(totalDiefCORIE);
                    ViewBag.IcmsAPagarCORSIE = Convert.ToDouble(IcmsAPagarCORSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsAPagarCORIE = Convert.ToDouble(IcmsAPagarCORIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    // Imobilizado

                    decimal totalFreteIMIE = 0;

                    foreach (var prod in result)
                    {
                        if (!prod.Note.Iest.Equals("") && prod.TaxationTypeId.Equals(3))
                        {
                            if (Convert.ToDecimal(prod.Diferencial) > 0)
                            {
                                totalFreteIMIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                            }
                        }
                    }

                    ViewBag.TotalFreteIMIE = Convert.ToDouble(totalFreteIMIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal valorNfe1NormalIMSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(3) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetIMSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(3) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalIMSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(3) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetIMSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(3) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe1NormalIMIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(3) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetIMIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(3) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalIMIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(3) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetIMIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.TaxationTypeId.Equals(3) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal? gnrePagaIMIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreIm).Sum()), 2);
                    decimal? gnreNPagaIMIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNIm).Sum()), 2);
                    decimal? gnrePagaIMSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreIm).Sum()), 2);
                    decimal? gnreNPagaIMSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNIm).Sum()), 2);

                    decimal? icmsStIMIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalIMIE + valorNfe1RetIMIE + valorNfe2NormalIMIE + valorNfe2RetIMIE;
                    decimal? icmsStIMSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalIMSIE + valorNfe1RetIMSIE + valorNfe2NormalIMSIE + valorNfe2RetIMSIE;
                    decimal? totalApuradoIMIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalApuradoIMSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalDiefIMSIE = (totalApuradoIMSIE + totalFreteIMIE) - icmsStIMSIE + gnreNPagaIMSIE - gnrePagaIMSIE;
                    decimal? totaDiefIMIE = totalApuradoIMIE - icmsStIMIE + gnreNPagaIMIE - gnrePagaIMIE;
                    int? qtdIMSIE = result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Count();
                    int? qtdIMIE = result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Count();

                    ViewBag.QtdIMSIE = qtdIMSIE;
                    ViewBag.QtdIMIE = qtdIMIE;
                    ViewBag.TotatlApuradoIMIE = Convert.ToDouble(totalApuradoIMIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotatlApuradoIMSIE = Convert.ToDouble(totalApuradoIMSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoIMIE = Convert.ToDouble(icmsStIMIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoIMSIE = Convert.ToDouble(icmsStIMSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    ViewBag.GnrePagaIMSIE = Convert.ToDouble(gnrePagaIMSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnrePagaIMIE = Convert.ToDouble(gnrePagaIMIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnreNPagaIMSIE = Convert.ToDouble(gnreNPagaIMSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnreNPagaIMIE = Convert.ToDouble(gnreNPagaIMIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    ViewBag.TotalDiefIMSIE = Convert.ToDouble(totalDiefIMSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDiefIMIE = Convert.ToDouble(totaDiefIMIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal icmsIMnotaIE = Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsIm).Sum());
                    decimal icmsIMnotaSIE = Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsIm).Sum());
                    ViewBag.IcmsIMIE = Convert.ToDouble(icmsIMnotaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsIMSIE = Convert.ToDouble(icmsIMnotaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal IcmsAPagarIMSIE = Convert.ToDecimal(totalDiefIMSIE - icmsIMnotaSIE);
                    decimal IcmsAPagarIMIE = Convert.ToDecimal(totaDiefIMIE - icmsIMnotaIE);
                    ViewBag.IcmsAPagarIMSIE = Convert.ToDouble(IcmsAPagarIMSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsAPagarIMIE = Convert.ToDouble(IcmsAPagarIMIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    // Antecipação Total
                    decimal totalFreteATIE = 0;

                    foreach (var prod in result)
                    {
                        if (!prod.Note.Iest.Equals("") && prod.TaxationTypeId.Equals(8))
                        {
                            if (Convert.ToDecimal(prod.Aliqinterna) > 0)
                            {
                                totalFreteATIE += Convert.ToDecimal((prod.Freterateado * prod.Aliqinterna) / 100);
                            }
                        }
                    }


                    ViewBag.TotalFreteATIE = Convert.ToDouble(totalFreteATIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal? icmsStATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.IcmsST).Sum()), 2);
                    decimal? icmsStATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.IcmsST).Sum()), 2);
                    decimal? totalApuradoATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.TotalICMS).Sum()), 2);
                    decimal? totalApuradoATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.TotalICMS).Sum()), 2);
                    decimal? totalDiefATSIE = (totalApuradoATSIE + totalFreteATIE) - icmsStATSIE;
                    decimal? totalDiefATIE = totalApuradoATIE - icmsStATIE;
                    int? qtdATSIE = result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Count();
                    int? qtdATIE = result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Count();

                    ViewBag.QtdATSIE = qtdATSIE;
                    ViewBag.QtdATIE = qtdATIE;
                    ViewBag.TotatlApuradoATIE = Convert.ToDouble(totalApuradoATIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotatlApuradoATSIE = Convert.ToDouble(totalApuradoATSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoATIE = Convert.ToDouble(icmsStATIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIcmsPagoATSIE = Convert.ToDouble(icmsStATSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    ViewBag.TotalDiefATSIE = Convert.ToDouble(totalDiefATSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDiefATIE = Convert.ToDouble(totalDiefATIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal IcmsAPagarATSIE = Convert.ToDecimal(totalDiefATSIE);
                    decimal IcmsAPagarATIE = Convert.ToDecimal(totalDiefATIE);
                    ViewBag.IcmsAPagarATSIE = Convert.ToDouble(IcmsAPagarATSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsAPagarATIE = Convert.ToDouble(IcmsAPagarATIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal valorbase1ATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                    decimal valorbase1ATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                    decimal valorbase2ATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                    decimal valorbase2ATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);

                    decimal TotalFecopCalcATIE = valorbase1ATIE + valorbase2ATIE;
                    decimal TotalFecopCalcATSIE = valorbase1ATSIE + valorbase2ATSIE;
                    ViewBag.TotalFecopCalculadaATIE = Convert.ToDouble(Math.Round(TotalFecopCalcATIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFecopCalculadaATSIE = Convert.ToDouble(Math.Round(TotalFecopCalcATSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal baseNfe1NormalATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                    decimal baseNfe1NormalATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                    decimal baseNfe1RetATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);
                    decimal baseNfe1RetATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);


                    decimal valorNfe1NormalATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe1NormalATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal valorNfe2NormalATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal TotalFecopNfeATIE = valorNfe1NormalATIE + valorNfe1RetATIE + valorNfe2NormalATIE + valorNfe2RetATIE;
                    decimal TotalFecopNfeATSIE = valorNfe1NormalATSIE + valorNfe1RetATSIE + valorNfe2NormalATSIE + valorNfe2RetATSIE;
                    ViewBag.TotalFecopNfeATIE = Convert.ToDouble(Math.Round(TotalFecopNfeATIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFecopNfeATSIE = Convert.ToDouble(Math.Round(TotalFecopNfeATSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal gnreNPagaFecopATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2);
                    decimal gnreNPagaFecopATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2);
                    ViewBag.GNREnPagaFecopATIE = Convert.ToDouble(gnreNPagaFecopATIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GNREnPagaFecopATSIE = Convert.ToDouble(gnreNPagaFecopATSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal gnrePagaFecop1ATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2);
                    decimal gnrePagaFecop1ATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2);
                    decimal gnrePagaFecop2ATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2);
                    decimal gnrePagaFecop2ATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2);

                    ViewBag.GNREPagaFecopATIE = Convert.ToDouble(gnrePagaFecop2ATIE + gnrePagaFecop1ATIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GNREPagaFecopATSIE = Convert.ToDouble(gnrePagaFecop2ATSIE + gnrePagaFecop1ATSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal totalGnreFecopATIE = gnrePagaFecop1ATIE + gnrePagaFecop2ATIE;
                    ViewBag.TotalGnreFecopATIE = Convert.ToDouble(totalGnreFecopATIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    decimal totalGnreFecopATSIE = gnrePagaFecop1ATSIE + gnrePagaFecop2ATSIE;
                    ViewBag.TotalGnreFecopATSIE = Convert.ToDouble(totalGnreFecopATSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal totalfecopDiefATIE = TotalFecopCalcATIE - totalGnreFecopATIE + (gnrePagaFecop2ATIE + gnrePagaFecop1ATIE) - TotalFecopNfeATIE;
                    decimal totalfecopDiefATSIE = TotalFecopCalcATSIE - totalGnreFecopATSIE + (gnrePagaFecop2ATSIE + gnrePagaFecop1ATSIE) - TotalFecopNfeATSIE;
                    ViewBag.TotalFecopDiefATIE = Convert.ToDouble(Math.Round(totalfecopDiefATIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFecopDiefATSIE = Convert.ToDouble(Math.Round(totalfecopDiefATSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal? icmsFecop1ATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2);
                    decimal? icmsFecop1ATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2);
                    decimal? icmsFecop2ATIE = Math.Round(Convert.ToDecimal(result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2);
                    decimal? icmsFecop2ATSIE = Math.Round(Convert.ToDecimal(result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2);
                    ViewBag.IcmsFecopATIE = Convert.ToDouble(icmsFecop1ATIE + icmsFecop2ATIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsFecopATSIE = Convert.ToDouble(icmsFecop1ATSIE + icmsFecop2ATSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    ViewBag.TotalFinalFecopCalculadaATIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalfecopDiefATIE - (icmsFecop1ATIE + icmsFecop2ATIE)), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFinalFecopCalculadaATSIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalfecopDiefATSIE - (icmsFecop1ATSIE + icmsFecop2ATSIE)), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    // Isento
                    int? qtdIsentoIE = result.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(7)).Count();
                    int? qtdIsentoSIE = result.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(7)).Count();
                    ViewBag.QtdIsentoIE = qtdIsentoIE;
                    ViewBag.QtdIsentoSIE = qtdIsentoSIE;


                    // Somatório Geral
                    int qtdGeralIE = Convert.ToInt32(qtdSTIE + qtdCOIE + qtdAPIE + qtdATIE + qtdCORIE + qtdIsentoIE + qtdIMIE);
                    int qtdGeralSIE = Convert.ToInt32(qtdSTSIE + qtdCOSIE + qtdAPSIE + qtdATSIE + qtdCORSIE + qtdIsentoSIE + qtdIMSIE);
                    ViewBag.QtdGeralIE = qtdGeralIE;
                    ViewBag.QtdGeralSIE = qtdGeralSIE;

                    decimal totalApuradoGeralIE = Convert.ToDecimal(totalApuradoSTIE + totalApuradoIMIE + totalApuradoCOIE + totalApuradoAPIE + totalApuradoCORIE + totalApuradoATIE);
                    decimal totalApuradoGeralSIE = Convert.ToDecimal(totalApuradoSTSIE + totalApuradoIMSIE + totalApuradoCOSIE + totalApuradoAPSIE + totalApuradoCORSIE + totalApuradoATSIE);
                    ViewBag.TotalApuradoGeralIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalApuradoGeralIE),2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalApuradoGeralSIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalApuradoGeralSIE),2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal GnrePagaGeralIE = Convert.ToDecimal(gnrePagaIMIE + gnrePagaAPIE + gnrePagaCOIE + gnrePagaSTIE);
                    decimal GnrePagaGeralSIE = Convert.ToDecimal(gnrePagaIMSIE + gnrePagaAPSIE + gnrePagaCOSIE + gnrePagaSTSIE);
                    ViewBag.GnrePagaGeralIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(GnrePagaGeralIE), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnrePagaGeralSIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(GnrePagaGeralSIE), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal GnreNPagaGeralIE = Convert.ToDecimal(gnreNPagaIMIE + gnreNPagaAPIE + gnreNPagaCOIE + gnreNPagaSTIE);
                    decimal GnreNPagaGeralSIE = Convert.ToDecimal(gnreNPagaIMSIE + gnreNPagaAPSIE + gnreNPagaCOSIE + gnreNPagaSTSIE);
                    ViewBag.GnreNPagaGeralIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(GnreNPagaGeralIE), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GnreNPagaGeralSIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(GnreNPagaGeralSIE), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal icmsNotaGeralIE = Convert.ToDecimal(icmsStATIE + icmsStAPIE + icmsStSTIE + icmsStIMIE + icmsStCORIE + icmsStCOIE);
                    decimal icmsNotaGeralSIE = Convert.ToDecimal(icmsStATSIE + icmsStAPSIE + icmsStSTSIE + icmsStIMSIE + icmsStCORSIE + icmsStCOSIE);
                    ViewBag.IcmsNotaGeralIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(icmsNotaGeralIE), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsNotaGeralSIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(icmsNotaGeralSIE), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal totalDiefGeralIE = Convert.ToDecimal(totalDiefCORIE + totalDiefSTIE + totalDiefCOIE + totaDiefIMIE + totalDiefATIE + totalDiefAPIE);
                    decimal totalDiefGeralSIE = Convert.ToDecimal(totalDiefCORSIE + totalDiefSTSIE + totalDiefCORSIE + totalDiefIMSIE + totalDiefATSIE + totalDiefAPSIE);
                    ViewBag.DiefGeralIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalDiefGeralIE), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.DiefGeralSIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalDiefGeralSIE), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                }
                else if (type == 9)
                {
                    result = _service.FindByProductsType(notesS.ToList(), typeTaxation);
                    List<List<string>> fornecedores = new List<List<string>>();
                    decimal icmsStTotal = 0, fecopStTotal = 0, gnrePagaTotal = 0, gnreNPagaTotal = 0, gnrefecopPagaTotal = 0;
                    
                    foreach (var prod in result)
                    {
                        if (prod.Note.Iest.Equals(""))
                        {
                            int pos = -1;
                            for (int i = 0; i < fornecedores.Count(); i++)
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
                ViewBag.IcmsStNoteS = Convert.ToDouble(Math.Round(icmsStnoteSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                ViewBag.IcmsStNoteI = Convert.ToDouble(Math.Round(icmsStnoteIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                return View(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
           
    }
}