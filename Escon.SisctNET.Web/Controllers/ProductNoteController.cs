using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Taxation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IConfigurationService _configurationService;
        private readonly ICompanyCfopService _companyCfopService;
        private readonly ISuspensionService _suspensionService;
        private readonly IClientService _clientService;
        private readonly INcmConvenioService _ncmConvenioService;
        private readonly IProductIncentivoService _productIncentivoService;
        private readonly IHostingEnvironment _appEnvironment;

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
            IConfigurationService configurationService,
            ICompanyCfopService companyCfopService,
            ISuspensionService suspensionService,
            IClientService clientService,
            INcmConvenioService ncmConvenioService,
            IProductIncentivoService productIncentivoService,
            IHostingEnvironment env,
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
            _configurationService = configurationService;
            _companyCfopService = companyCfopService;
            _suspensionService = suspensionService;
            _clientService = clientService;
            _ncmConvenioService = ncmConvenioService;
            _productIncentivoService = productIncentivoService;
            _appEnvironment = env;
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
                    var result = _service.FindByNotes(noteId, GetLog(OccorenceLog.Read)).OrderBy(_ => _.Status).ToList();
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
                    ViewBag.Incentivo = rst.Company.Incentive;
                    ViewBag.Anexo = rst.Company.AnnexId;

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
                        else
                        {
                            prod.Valoragregado = null;
                            prod.Mva = null;
                        }

                        if (bcrForm != null)
                        {
                            valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(bcrForm), valorAgreg);
                            prod.ValorBCR = valorAgreg;
                            prod.BCR = Convert.ToDecimal(bcrForm);
                            valor_icms = 0;
                        }
                        else
                        {
                            prod.ValorBCR = null;
                            prod.BCR = null;
                        }

                        if (fecop != null)
                        {
                            prod.Fecop = Convert.ToDecimal(fecop);
                            valor_fecop = calculation.valorFecop(Convert.ToDecimal(fecop), valorAgreg);
                            prod.TotalFecop = valor_fecop;
                        }
                        else
                        {
                            prod.Fecop = null;
                            prod.TotalFecop = null;
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
                    prod.Incentivo = true;
                    prod.DateStart = Convert.ToDateTime(dateStart);
                    prod.Produto = "Especial";

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
                            else
                            {
                                prod.Valoragregado = null;
                                prod.Mva = null;
                            }

                            if (bcrForm != null)
                            {
                                valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(bcrForm), valorAgreg);
                                prod.ValorBCR = valorAgreg;
                                prod.BCR = Convert.ToDecimal(bcrForm);
                                valor_icms = 0;
                            }
                            else
                            {
                                prod.ValorBCR = null;
                                prod.BCR = null;
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
                            
                            prod.Mva = null;
                            prod.Valoragregado = null;
                            prod.ValorBCR = null;
                            prod.BCR = null;
                            prod.Fecop = null;
                            prod.TotalFecop = null;
                            prod.ValorAC = null;
                            prod.TotalICMS = null;

                            prod.Diferencial = dif;
                            decimal icmsApu = (dif / 100) * baseCalc;
                            prod.IcmsApurado = icmsApu;
                        }
                        else if (taxedtype.Type == "Isento")
                        {
                            prod.Mva = null;
                            prod.Valoragregado = null;
                            prod.ValorBCR = null;
                            prod.BCR = null;
                            prod.Fecop = null;
                            prod.TotalFecop = null;
                            prod.ValorAC = null;
                            prod.TotalICMS = null;
                            prod.Diferencial = null;
                            prod.IcmsApurado = null;
                        }

                        prod.TaxationTypeId = Convert.ToInt32(taxaType);
                        prod.Updated = DateTime.Now;
                        prod.Status = true;
                        prod.Vbasecalc = baseCalc;
                        prod.ProductId = null;
                        prod.Product1Id = null;
                        prod.Pautado = false;
                        prod.DateStart = Convert.ToDateTime(dateStart);

                        if (note.Company.Incentive.Equals(true) && note.Company.AnnexId.Equals(2))
                        {
                            prod.Incentivo = false;
                        }

                        prod.Qpauta = null;
                        prod.Produto = "Especial";

                        var result = _service.Update(prod, GetLog(OccorenceLog.Update));
                    }
                    else
                    {
                        List<Model.ProductNote> updateProducts = new List<Model.ProductNote>();

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
                                else
                                {
                                    item.Valoragregado = valorAgreg;
                                    item.Mva = null;
                                }

                                if (bcrForm != null)
                                {
                                    valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(bcrForm), valorAgreg);
                                    item.ValorBCR = valorAgreg;
                                    item.BCR = Convert.ToDecimal(bcrForm);
                                    valor_icms = 0;
                                }
                                else
                                {
                                    item.ValorBCR = null;
                                    item.BCR = null;
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

                                item.Mva = null;
                                item.Valoragregado = null;
                                item.ValorBCR = null;
                                item.BCR = null;
                                item.Fecop = null;
                                item.TotalFecop = null;
                                item.ValorAC = null;
                                item.TotalICMS = null;
                            }
                            else if (taxedtype.Type == "Isento")
                            {
                                item.Mva = null;
                                item.Valoragregado = null;
                                item.ValorBCR = null;
                                item.BCR = null;
                                item.Fecop = null;
                                item.TotalFecop = null;
                                item.ValorAC = null;
                                item.TotalICMS = null;
                                item.Diferencial = null;
                                item.IcmsApurado = null;
                            }

                            item.TaxationTypeId = Convert.ToInt32(taxaType);
                            item.Updated = DateTime.Now;
                            item.Status = true;
                            item.Vbasecalc = baseCalc;

                            item.ProductId = null;
                            item.Product1Id = null;
                            item.Pautado = false;
                            item.DateStart = Convert.ToDateTime(dateStart);

                            if (note.Company.Incentive.Equals(true) && note.Company.AnnexId.Equals(2))
                            {
                                item.Incentivo = false;
                            }

                            item.Qpauta = null;
                            item.Produto = "Normal";
          
                            updateProducts.Add(item);
                            //var result = _service.Update(item, GetLog(OccorenceLog.Update));
                        }

                       _service.Update(updateProducts);
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

                List<Note> updateNote = new List<Note>();

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

                    updateNote.Add(nota);

                    //_noteService.Update(nota, GetLog(OccorenceLog.Update));
                }

                _noteService.Update(updateNote);

                return RedirectToAction("Index", new { noteId = note.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Relatory(int id, int typeTaxation, int type, string year, string month, string nota, IFormFile arquivo)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }
            try
            {
                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                var notes = _noteService.FindByNotes(id, year, month);
                var products = _service.FindByProductsType(notes, typeTaxation).OrderBy(_ => _.Note.Iest).ToList();
                var notasTaxation = products.Select(_ => _.Note).Distinct().ToList();
                var notas = products.Select(_ => _.Nnf).Distinct();
                var total = _service.FindByTotal(notas.ToList());
                var notesS = notes.Where(_ => _.Iest == "");
                var notesI = notes.Where(_ => _.Iest != "");

                var calculation = new Calculation();

                var icmsStnoteSIE = _service.FindBySubscription(notesS.ToList(), typeTaxation);
                var icmsStnoteIE = _service.FindBySubscription(notesI.ToList(), typeTaxation);

                ViewBag.SocialName = comp.SocialName;
                ViewBag.Document = comp.Document;
                ViewBag.Year = year;
                ViewBag.Month = month;
                ViewBag.TaxationType = typeTaxation;
                ViewBag.type = type;

                ViewBag.Incetive = comp.Incentive;
                ViewBag.TypeIncetive = comp.TipoApuracao;
                ViewBag.TypeCompany = comp.TypeCompany;
                ViewBag.Anexo = comp.AnnexId;

                var NfeExit = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));
                var NfeEntrada = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                var import = new Import(_companyCfopService);

                string directoryNfeExit = NfeExit.Value + "\\" + comp.Document + "\\" + year + "\\" + month;
                string directoryNfeEntrada = NfeEntrada.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

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
                        products = _service.FindByProductsType(notes, typeTaxation);
                    }


                    if (type == 4)
                    {
                        ViewBag.NotasTaxation = notasTaxation;
                        ViewBag.Products = products;
                       
                    }

                    if (type == 7)
                    {
                        products = products.Where(_ => _.Incentivo.Equals(true)).ToList();
                    }
                    else if (type == 8)
                    {
                        products = products.Where(_ => _.Incentivo.Equals(false)).ToList();
                    }

                    ViewBag.Registro = products.Count();
                    decimal totalFecop = Convert.ToDecimal(products.Select(_ => _.TotalFecop).Sum());

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
                            var Vprod = Convert.ToDouble(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vprod).Sum() + 
                                products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vfrete).Sum() + 
                                products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vseg).Sum() +
                                products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Voutro).Sum() -
                                products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vdesc).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""); 
                            var Vipi = Convert.ToDouble(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vipi).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var frete = Convert.ToDouble(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Freterateado).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var bcTotal = Convert.ToDouble(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vbasecalc).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var bcIcms = Convert.ToDouble(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Valoragregado).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var bcr = Convert.ToDouble(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.ValorBCR).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var vAC = Convert.ToDouble(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.ValorAC).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var nfe = Convert.ToDouble(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vicms).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var cte = Convert.ToDouble(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.IcmsCTe).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var icms = Convert.ToDouble(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.IcmsST).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var icmsTotal = Convert.ToDouble(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.TotalICMS).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var fecopTotal = Convert.ToDouble(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.TotalFecop).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var vFrete = Convert.ToDouble(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vfrete).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            var icmsApurado = Convert.ToDouble(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.IcmsApurado).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

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

                    decimal baseCalculo = Convert.ToDecimal(products.Select(_ => _.Vprod).Sum() + products.Select(_ => _.Voutro).Sum() + 
                        products.Select(_ => _.Vseg).Sum() - products.Select(_ => _.Vdesc).Sum() + products.Select(_ => _.Vfrete).Sum() +
                        products.Select(_ => _.Freterateado).Sum() + products.Select(_ => _.Vipi).Sum());
                    ViewBag.ValorProd = Convert.ToDouble(products.Select(_ => _.Vprod).Sum() + products.Select(_ => _.Voutro).Sum() + products.Select(_ => _.Vseg).Sum() - products.Select(_ => _.Vdesc).Sum() + products.Select(_ => _.Vfrete).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    //ViewBag.TotalBC = Convert.ToDouble(Math.Round(products.Select(_ => _.Vbasecalc).Sum(), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalBC = Convert.ToDouble(Math.Round(baseCalculo, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    ViewBag.TotalNotas = Convert.ToDouble(total).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    ViewBag.TotalFecop = Convert.ToDouble(totalFecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFrete = Convert.ToDouble(products.Select(_ => _.Freterateado).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalIpi = Convert.ToDouble(products.Select(_ => _.Vipi).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    ViewBag.TotalBcICMS = Convert.ToDouble(Math.Round(Convert.ToDecimal(products.Select(_ => _.Valoragregado).Sum()), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalBCR = Convert.ToDouble(products.Select(_ => _.ValorBCR).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalAC = Convert.ToDouble(products.Select(_ => _.ValorAC).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalICMSNfe = Convert.ToDouble(products.Select(_ => _.Vicms).Sum()).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalICMSCte = Convert.ToDouble(Math.Round(products.Select(_ => _.IcmsCTe).Sum(), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal icmsGeralStIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum()), 2);
                    decimal icmsGeralStSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum()), 2);
                    ViewBag.IcmsGeralSTIE = Convert.ToDouble(icmsGeralStIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsGeralSTSIE = Convert.ToDouble(icmsGeralStSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal? totalIcmsIE = 0, totalIcmsSIE = 0, valorDiefIE = 0;
                    decimal totalGeralIcmsST = Convert.ToDecimal(products.Select(_ => _.IcmsST).Sum());
                    ViewBag.TotalICMSST = Convert.ToDouble(totalGeralIcmsST).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    decimal totalGeralIcms = Convert.ToDecimal(products.Select(_ => _.TotalICMS).Sum());
                    ViewBag.TotalICMSGeral = Convert.ToDouble(totalGeralIcms).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    if (typeTaxation == 1 || typeTaxation == 7)
                    {
                        decimal totalIcmsFreteIE = 0, totalFecop1FreteIE = 0, totalFecop2FreteIE = 0, base1FecopFreteIE = 0, base2FecopFreteIE = 0,
                            totalDarSTCO = 0, totalDarFecop = 0, totalDarIcms = 0, totalDarCotac = 0, totalDarFunef = 0;

                        foreach (var prod in products)
                        {
                            if (!prod.Note.Iest.Equals(""))
                            {
                                if (Convert.ToDecimal(prod.Aliqinterna) > 0)
                                {
                                    decimal valorAgreg = 0;
                                    if(prod.Mva != null)
                                    {
                                        valorAgreg = calculation.ValorAgregadoMva(prod.Freterateado, Convert.ToDecimal(prod.Mva));
                                    }
                                    if (prod.BCR != null)
                                    {
                                        valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(prod.BCR), valorAgreg);
                                    }
                                    if (prod.Fecop != null)
                                    {
                                        if (Convert.ToDecimal(prod.Fecop).Equals(1))
                                        {
                                            base1FecopFreteIE += valorAgreg;
                                            totalFecop1FreteIE += calculation.valorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                        }
                                        else
                                        {
                                            base2FecopFreteIE += valorAgreg;
                                            totalFecop2FreteIE += calculation.valorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                        }
                                        
                                    }

                                    totalIcmsFreteIE += calculation.valorAgregadoAliqInt(Convert.ToDecimal(prod.Aliqinterna), Convert.ToDecimal(prod.Fecop), valorAgreg) - prod.IcmsCTe;
                                }
                            }
                        }

                        ViewBag.TotalIcmsFreteIE = Convert.ToDouble(totalIcmsFreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFecop1FreteIE = Convert.ToDouble(totalFecop1FreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFecop2FreteIE = Convert.ToDouble(totalFecop2FreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseFecop1FreteIE = Convert.ToDouble(base1FecopFreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseFecop2FreteIE = Convert.ToDouble(base2FecopFreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFecopFreteIE = Convert.ToDouble(totalFecop1FreteIE + totalFecop2FreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal totalIcmsPautaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Pautado.Equals(true) && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2);
                        decimal totalIcmsMvaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Pautado.Equals(false) && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2);
                        decimal totalIcmsPautaIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Pautado.Equals(true) && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2);
                        decimal totalIcmsMvaIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Pautado.Equals(false) && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2);

                        decimal gnreNPagaSIE = 0, gnrePagaSIE = 0, gnreNPagaIE = 0, gnrePagaIE = 0;

                        if (typeTaxation == 1)
                        {
                            gnreNPagaSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNSt).Sum()), 2);
                            gnrePagaSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreSt).Sum()), 2);
                            gnreNPagaIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNSt).Sum()), 2);
                            gnrePagaIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreSt).Sum()), 2);
                        }

                        ViewBag.TotalGNREnPagaSIE = Convert.ToDouble(gnreNPagaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGNREPagaSIE = Convert.ToDouble(gnrePagaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGNREnPagaIE = Convert.ToDouble(gnreNPagaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalGNREPagaIE = Convert.ToDouble(gnrePagaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal base1SIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2);
                        base1SIE += Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);
                        decimal base1IE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2);
                        base1IE += Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);                        
                        decimal base1fecopIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.Fecop1).Sum()), 2);
                        decimal base1fecopSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.Fecop1).Sum()), 2);

                        decimal valorbase1IE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                        decimal valorbase1SIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);

                        ViewBag.base1SIE = Convert.ToDouble(Math.Round(base1SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.base1IE = Convert.ToDouble(Math.Round(base1IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.base1fecopIE = Convert.ToDouble(base1fecopIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.base1fecopSIE = Convert.ToDouble(base1fecopSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.valorbase1IE = Convert.ToDouble(Math.Round(valorbase1IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.valorbase1SIE = Convert.ToDouble(Math.Round(valorbase1SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal base2IE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2);
                        base2IE += Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);
                        decimal base2SIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2);
                        base2SIE += Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);
                       
                        decimal base2fecopIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.Fecop2).Sum()), 2);
                        decimal base2fecopSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.Fecop2).Sum()), 2);

                        decimal valorbase2IE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                        decimal valorbase2SIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);

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

    
                        decimal baseNfe1NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                        decimal baseNfe1NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                        decimal baseNfe1RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);
                        decimal baseNfe1RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);
                        ViewBag.fecopNfe1IE = Convert.ToDouble(Math.Round(baseNfe1NormalIE + baseNfe1RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.fecopNfe1SIE = Convert.ToDouble(Math.Round(baseNfe1NormalSIE + baseNfe1RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal valorNfe1NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe1RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                        decimal valorNfe1NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe1RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                        ViewBag.valorNfe1IE = Convert.ToDouble(Math.Round(valorNfe1NormalIE + valorNfe1RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.valorNfe1SIE = Convert.ToDouble(Math.Round(valorNfe1NormalSIE + valorNfe1RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal baseNfe2NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2);
                        decimal baseNfe2RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2);
                        decimal baseNfe2NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2);
                        decimal baseNfe2RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2);
                        ViewBag.fecopNfe2IE = Convert.ToDouble(Math.Round(baseNfe2NormalIE + baseNfe2RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.fecopNfe2SIE = Convert.ToDouble(Math.Round(baseNfe2NormalSIE + baseNfe2RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal valorNfe2NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe2RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                        decimal valorNfe2NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe2RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
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

                        totalIcmsIE = products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum();
                        totalIcmsSIE = products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum();
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

                        decimal diefStIE = Convert.ToDecimal(totalIcmsIE - icmsGeralStIE - gnrePagaIE + gnreNPagaIE - totalIcmsFreteIE);
                        decimal diefStSIE = Convert.ToDecimal(totalIcmsSIE - icmsGeralStSIE - gnrePagaSIE + gnreNPagaSIE + totalIcmsFreteIE);
                        ViewBag.ValorDiefIE = Convert.ToDouble(diefStIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ValorDiefSIE = Convert.ToDouble(diefStSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                       
                        decimal icmsStnotaIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsSt).Sum()), 2);
                        decimal icmsStnotaSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsSt).Sum()), 2);
                        ViewBag.IcmsStIE = Convert.ToDouble(icmsStnotaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsStSIE = Convert.ToDouble(icmsStnotaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        
                        ViewBag.IcmsPagarIE = Convert.ToDouble(diefStIE - icmsStnotaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsPagarSIE = Convert.ToDouble(diefStSIE - icmsStnotaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        // Valores da dief fecop
                        ViewBag.DifBase1IE = Convert.ToDouble(Math.Round(base1IE - baseNfe1NormalIE - baseNfe1RetIE - base1FecopFreteIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DifBase1SIE = Convert.ToDouble(Math.Round(base1SIE - baseNfe1NormalSIE - baseNfe1RetSIE + base1FecopFreteIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal difvalor1IE = valorbase1IE - valorNfe1NormalIE - valorNfe1RetIE - gnrePagaFecop1IE - totalFecop1FreteIE - totalFecop2FreteIE;
                        decimal difvalor1SIE = valorbase1SIE - valorNfe1NormalSIE - valorNfe1RetSIE - gnrePagaFecop1SIE + totalFecop1FreteIE + totalFecop2FreteIE;
                        ViewBag.DifValor1IE = Convert.ToDouble(Math.Round(difvalor1IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DifValor1SIE = Convert.ToDouble(Math.Round(difvalor1SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                       
                        ViewBag.DifBase2IE = Convert.ToDouble(Math.Round(base2IE - baseNfe2NormalIE - baseNfe2RetIE - base2FecopFreteIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DifBase2SIE = Convert.ToDouble(Math.Round(base2SIE - baseNfe2NormalSIE - baseNfe2RetSIE + base2FecopFreteIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal difvalor2IE = valorbase2IE - valorNfe2NormalIE - valorNfe2RetIE - gnrePagaFecop2IE - totalFecop2FreteIE;
                        decimal difvalor2SIE = valorbase2SIE - valorNfe2NormalSIE - valorNfe2RetSIE - gnrePagaFecop2SIE + totalFecop2FreteIE;
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
                        if (comp.Incentive == true && comp.AnnexId != null && typeTaxation == 1)
                        {
                            var productsAll = _service.FindByProductsType(notes, typeTaxation);

                            //Produtos não incentivados
                            var productsNormal = productsAll.Where(_ => _.Incentivo.Equals(false)).ToList();
                            decimal? totalIcmsNormalIE = productsNormal.Where(_ => _.TaxationType.Type.Equals("ST") && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum();
                            decimal? totalIcmsNormalSIE = productsNormal.Where(_ => _.TaxationType.Type.Equals("ST") && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum();

                            var notasTaxationNormal = productsNormal.Select(_ => _.Note).Distinct();
                            totalIcmsFreteIE = 0;
                            totalFecop1FreteIE = 0;
                            totalFecop2FreteIE = 0;
                            base1FecopFreteIE = 0;
                            base2FecopFreteIE = 0;
                            foreach (var prod in productsNormal)
                            {
                                if (!prod.Note.Iest.Equals("") && prod.Incentivo.Equals(false))
                                {
                                    if (Convert.ToDecimal(prod.Aliqinterna) > 0)
                                    {
                                        decimal valorAgreg = 0;
                                        if (prod.Mva != null)
                                        {
                                            valorAgreg = calculation.ValorAgregadoMva(prod.Freterateado, Convert.ToDecimal(prod.Mva));
                                        }
                                        if (prod.BCR != null)
                                        {
                                            valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(prod.BCR), valorAgreg);
                                        }
                                        if (prod.Fecop != null)
                                        {
                                            if (Convert.ToDecimal(prod.Fecop).Equals(1))
                                            {
                                                base1FecopFreteIE += valorAgreg;
                                                totalFecop1FreteIE += calculation.valorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                            }
                                            else
                                            {
                                                base2FecopFreteIE += valorAgreg;
                                                totalFecop2FreteIE += calculation.valorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                            }
                                        }

                                        totalIcmsFreteIE += calculation.valorAgregadoAliqInt(Convert.ToDecimal(prod.Aliqinterna), Convert.ToDecimal(prod.Fecop), valorAgreg) - prod.IcmsCTe;
                                    }
                                }
                            }

                            ViewBag.TotalIcmsFreteIE = Convert.ToDouble(totalIcmsFreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalFecop1FreteIE = Convert.ToDouble(totalFecop1FreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalFecop2FreteIE = Convert.ToDouble(totalFecop2FreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.BaseFecop1FreteIE = Convert.ToDouble(base1FecopFreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.BaseFecop2FreteIE = Convert.ToDouble(base2FecopFreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalFecopFreteIE = Convert.ToDouble(totalFecop1FreteIE + totalFecop2FreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            icmsGeralStIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum()), 2);
                            icmsGeralStSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum()), 2);

                            ViewBag.IcmsGeralSTIE = Convert.ToDouble(icmsGeralStIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.IcmsGeralSTSIE = Convert.ToDouble(icmsGeralStSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            ViewBag.TotalICMSST = Convert.ToDouble(icmsGeralStIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            gnreNPagaSIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNSt).Distinct().Sum()), 2);
                            gnrePagaSIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreSt).Distinct().Sum()), 2);
                            gnreNPagaIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNSt).Distinct().Sum()), 2);
                            gnrePagaIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreSt).Distinct().Sum()), 2);
                            ViewBag.TotalGNREnPagaSIE = Convert.ToDouble(gnreNPagaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGNREPagaSIE = Convert.ToDouble(gnrePagaSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGNREnPagaIE = Convert.ToDouble(gnreNPagaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGNREPagaIE = Convert.ToDouble(gnrePagaIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            diefStIE = Convert.ToDecimal(totalIcmsNormalIE - icmsGeralStIE - gnrePagaIE + gnreNPagaIE - totalIcmsFreteIE);
                            diefStSIE = Convert.ToDecimal(totalIcmsNormalSIE - icmsGeralStSIE - gnrePagaSIE + gnreNPagaSIE + totalIcmsFreteIE);
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

                            totalDarSTCO += (diefStSIE - icmsStnotaSIE);

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
                            ViewBag.DifBase1IE = Convert.ToDouble(Math.Round(base1IE - baseNfe1NormalIE - baseNfe1RetIE - base1FecopFreteIE - base1FecopFreteIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.DifBase1SIE = Convert.ToDouble(Math.Round(base1SIE - baseNfe1NormalSIE - baseNfe1RetSIE + base1FecopFreteIE + base1FecopFreteIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            difvalor1IE = valorbase1IE - valorNfe1NormalIE - valorNfe1RetIE - gnrePagaFecop1IE - totalFecop1FreteIE - totalFecop2FreteIE;
                            difvalor1SIE = valorbase1SIE - valorNfe1NormalSIE - valorNfe1RetSIE - gnrePagaFecop1SIE + totalFecop1FreteIE + totalFecop2FreteIE;
                            ViewBag.DifValor1IE = Convert.ToDouble(Math.Round(difvalor1IE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.DifValor1SIE = Convert.ToDouble(Math.Round(difvalor1SIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            ViewBag.DifBase2IE = Convert.ToDouble(Math.Round(base2IE - baseNfe2NormalIE - baseNfe2RetIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.DifBase2SIE = Convert.ToDouble(Math.Round(base2SIE - baseNfe2NormalSIE - baseNfe2RetSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            difvalor2IE = valorbase2IE - valorNfe2NormalIE - valorNfe2RetIE - gnrePagaFecop2IE - totalFecop2FreteIE;
                            difvalor2SIE = valorbase2SIE - valorNfe2NormalSIE - valorNfe2RetSIE - gnrePagaFecop2SIE + totalFecop2FreteIE;
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

                            totalDarFecop += (totalfecop1SIE + totalfecop2SIE);

                            //Produto incentivados
                            var productsP = productsAll.Where(_ => _.Incentivo.Equals(true)).ToList();

                           
                            decimal? impostoGeral = 0, vProds = 0, freterateado = 0, totalBc = 0, totalIpi = 0, totalBcIcms = 0, totalBCR = 0, totalAC = 0,
                                totalIcmsNfe = 0, totalIcmsCTe = 0, totalIcmsNormal = 0, totalFecopNormal = 0, totalIcmsIncentivo = 0, totalFecopIncentivo = 0 ;
                           
                            int registros = 0;

                            if (type == 8)
                            {
                                total = productsNormal.Select(_ => _.Note.Vnf).Distinct().Sum();
                                registros = productsNormal.Count();
                                vProds = Convert.ToDecimal(productsNormal.Select(_ => _.Vprod).Sum() + productsNormal.Select(_ => _.Voutro).Sum() + productsNormal.Select(_ => _.Vseg).Sum() - productsNormal.Select(_ => _.Vdesc).Sum() + productsNormal.Select(_ => _.Vfrete).Sum());
                                freterateado = Convert.ToDecimal(productsNormal.Select(_ => _.Freterateado).Sum());

                                totalBc = Convert.ToDecimal(productsNormal.Select(_ => _.Vprod).Sum() + productsNormal.Select(_ => _.Voutro).Sum() +
                                                productsNormal.Select(_ => _.Vseg).Sum() - productsNormal.Select(_ => _.Vdesc).Sum() + productsNormal.Select(_ => _.Vfrete).Sum() +
                                                productsNormal.Select(_ => _.Freterateado).Sum() + productsNormal.Select(_ => _.Vipi).Sum());
                                //totalBc = Convert.ToDecimal(productsNormal.Select(_ => _.Vbasecalc).Sum());
                                totalIpi = Convert.ToDecimal(productsNormal.Select(_ => _.Vipi).Sum());
                                totalBcIcms = Convert.ToDecimal(productsNormal.Select(_ => _.Valoragregado).Sum());
                                totalBCR = Convert.ToDecimal(productsNormal.Select(_ => _.ValorBCR).Sum());
                                totalAC = Convert.ToDecimal(productsNormal.Select(_ => _.ValorAC).Sum());
                                totalIcmsNfe = Convert.ToDecimal(productsNormal.Select(_ => _.Vicms).Sum());
                                totalIcmsCTe = Convert.ToDecimal(productsNormal.Select(_ => _.IcmsCTe).Sum());
                                totalGeralIcmsST = Convert.ToDecimal(productsNormal.Select(_ => _.IcmsST).Sum());
                                totalGeralIcms = Convert.ToDecimal(productsNormal.Select(_ => _.TotalICMS).Sum());
                                totalFecop = Convert.ToDecimal(productsNormal.Select(_ => _.TotalFecop).Sum());
                            }

                            if (type == 7)
                            {                               
                                totalIcmsIE = productsAll.Select(_ => _.TotalICMS).Sum();
                                var totalFecop1 = productsAll.Select(_ => _.TotalFecop).Sum();

                                total = productsP.Select(_ => _.Note.Vnf).Distinct().Sum();
                                registros = productsP.Count();

                                vProds = Convert.ToDecimal(productsP.Select(_ => _.Vprod).Sum() + productsP.Select(_ => _.Voutro).Sum() + productsP.Select(_ => _.Vseg).Sum() - productsP.Select(_ => _.Vdesc).Sum() + productsP.Select(_ => _.Vfrete).Sum());
                                freterateado = Convert.ToDecimal(productsP.Select(_ => _.Freterateado).Sum());
                                totalBc = Convert.ToDecimal(productsP.Select(_ => _.Vprod).Sum() + productsP.Select(_ => _.Voutro).Sum() +
                                                productsP.Select(_ => _.Vseg).Sum() - productsP.Select(_ => _.Vdesc).Sum() + productsP.Select(_ => _.Vfrete).Sum() +
                                                productsP.Select(_ => _.Freterateado).Sum() + productsP.Select(_ => _.Vipi).Sum());

                                //totalBc = Convert.ToDecimal(productsP.Select(_ => _.Vbasecalc).Sum());
                                totalIpi = Convert.ToDecimal(productsP.Select(_ => _.Vipi).Sum());
                                totalBcIcms = Convert.ToDecimal(productsP.Select(_ => _.Valoragregado).Sum());
                                totalBCR = Convert.ToDecimal(productsP.Select(_ => _.ValorBCR).Sum());
                                totalAC = Convert.ToDecimal(productsP.Select(_ => _.ValorAC).Sum());
                                totalIcmsNfe = Convert.ToDecimal(productsP.Select(_ => _.Vicms).Sum());
                                totalIcmsCTe = Convert.ToDecimal(productsP.Select(_ => _.IcmsCTe).Sum());
                                totalGeralIcmsST = Convert.ToDecimal(productsP.Select(_ => _.IcmsST).Sum());
                                totalGeralIcms = Convert.ToDecimal(productsP.Select(_ => _.TotalICMS).Sum());
                                totalFecop = Convert.ToDecimal(productsP.Select(_ => _.TotalFecop).Sum());

                            }


                            if(type == 7 || type == 8)
                            {
                                ViewBag.Registro = registros;
                                ViewBag.TotalNotas = Convert.ToDouble(Math.Round(Convert.ToDouble(total), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.ValorProd = Convert.ToDouble(Math.Round(Convert.ToDouble(vProds), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalFrete = Convert.ToDouble(Math.Round(Convert.ToDouble(freterateado), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalBC = Convert.ToDouble(Math.Round(Convert.ToDouble(totalBc), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalIpi = Convert.ToDouble(Math.Round(Convert.ToDouble(totalIpi), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalBcICMS = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalBcIcms), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalBCR = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalBCR), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalAC = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalAC), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalICMSNfe = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalIcmsNfe), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalICMSCte = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalIcmsCTe), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalICMSST = Convert.ToDouble(totalGeralIcmsST).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalICMSGeral = Convert.ToDouble(totalGeralIcms).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalFecop = Convert.ToDouble(totalFecop).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                
                            }

                            totalIcmsNormal = Convert.ToDecimal(productsNormal.Select(_ => _.TotalICMS).Sum());
                            totalIcmsIncentivo = Convert.ToDecimal(productsP.Select(_ => _.TotalICMS).Sum());
                            totalFecopNormal = Convert.ToDecimal(productsNormal.Select(_ => _.TotalFecop).Sum());
                            totalFecopIncentivo = Convert.ToDecimal(productsP.Select(_ => _.TotalFecop).Sum());

                            impostoGeral = totalIcmsNormal + totalIcmsIncentivo + totalFecopNormal + totalFecopIncentivo;

                            ViewBag.IcmsNormal = Convert.ToDouble(totalIcmsNormal).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.IcmsIncentivo = Convert.ToDouble(totalIcmsIncentivo).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.FecopNormal = Convert.ToDouble(totalFecopNormal).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.FecopIncentivo = Convert.ToDouble(totalFecopIncentivo).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            ViewBag.ImpostoGeral = Convert.ToDouble(Math.Round(Convert.ToDecimal(impostoGeral), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            decimal totalImpostoIncentivo = 0, impostoIcms = 0, impostoFecop = 0;

                            if (comp.AnnexId.Equals(3) && type != 8)
                            {
                                List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                                var clientesAll = _clientService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).ToList();
                                var nContribuintes = clientesAll.Where(_ => _.TypeClientId.Equals(2)).Select(_ => _.Document).ToList();
                                var suspensions = _suspensionService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).ToList();

                                var cfopVenda = _companyCfopService.FindAll(null).Where(_ => _.CompanyId.Equals(id) && _.Active.Equals(true) && (_.CfopTypeId.Equals(1) || _.CfopTypeId.Equals(4) || _.CfopTypeId.Equals(5))).Select(_ => _.Cfop.Code).ToList();

                                if (comp.SectionId.Equals(2))
                                {
                                    exitNotes = import.Nfe(directoryNfeExit);

                                    decimal vendasInternasElencadas = 0, vendasInterestadualElencadas = 0, vendasInternasDeselencadas = 0, vendasInterestadualDeselencadas = 0,
                                        InternasElencadas = 0, InterestadualElencadas = 0, InternasElencadasPortaria = 0, InterestadualElencadasPortaria = 0,
                                        InternasDeselencadas = 0, InterestadualDeselencadas = 0, InternasDeselencadasPortaria = 0, InterestadualDeselencadasPortaria = 0,
                                        suspensao = 0, vendasContribuintes = 0, vendas = 0;

                                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                                    {
                                        if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                                        {
                                            exitNotes.RemoveAt(i);
                                            continue;
                                        }


                                        bool nContribuinte = false, ncm = false, cfop = false, suspenso = false;

                                        if (exitNotes[i][1].ContainsKey("dhEmi"))
                                        {
                                            foreach (var suspension in suspensions)
                                            {
                                                if (Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) >= Convert.ToDateTime(suspension.DateStart) && Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) < Convert.ToDateTime(suspension.DateEnd))
                                                {
                                                    suspenso = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (exitNotes[i][3].ContainsKey("CNPJ"))
                                        {
                                            if (nContribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                            {
                                                nContribuinte = true;
                                            }

                                            bool existe = false;

                                            if (clientesAll.Select(_ => _.Document).Contains(exitNotes[i][3]["CNPJ"]))
                                            {
                                                existe = true;
                                            }

                                            if (existe == false)
                                            {
                                                throw new Exception("Há Clientes não Importados");
                                            }
                                        }

                                        for (int j = 0; j < exitNotes[i].Count; j++)
                                        {
                                            if (exitNotes[i][j].ContainsKey("NCM"))
                                            {
                                                ncm = _service.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId), exitNotes[i][j]["NCM"].ToString());
                                            }

                                            if (exitNotes[i][j].ContainsKey("CFOP"))
                                            {
                                                if (cfopVenda.Contains(exitNotes[i][j]["CFOP"]))
                                                {
                                                    cfop = true;
                                                }
                                            }

                                            if (nContribuinte == true)
                                            {
                                                if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasContribuintes += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }

                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasContribuintes += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasContribuintes -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        vendas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasContribuintes += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasContribuintes += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualDeselencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualDeselencadasPortaria -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            }
                                                        }
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            if (suspenso == true)
                                                            {
                                                                suspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }

                                    //  Elencadas
                                    // Internas
                                    decimal icmsInternaElencada = (InternasElencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                                    decimal fecopInternaElencada = (InternasElencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100;
                                    decimal totalInternasElencada = icmsInternaElencada;
                                    decimal icmsPresumidoInternaElencada = (InternasElencadasPortaria * Convert.ToDecimal(comp.IncIInterna)) / 100;
                                    decimal totalIcmsInternaElencada = totalInternasElencada - icmsPresumidoInternaElencada;

                                    totalDarFecop += fecopInternaElencada;
                                    totalDarIcms += totalIcmsInternaElencada;
                                    impostoIcms += totalIcmsInternaElencada;
                                    impostoFecop += fecopInternaElencada;

                                    // Interestadual
                                    decimal icmsInterestadualElencada = (InterestadualElencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                                    decimal fecopInterestadualElencada = (InterestadualElencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100;
                                    decimal totalInterestadualElencada = icmsInterestadualElencada;
                                    decimal icmsPresumidoInterestadualElencada = (InterestadualElencadasPortaria * Convert.ToDecimal(comp.IncIInterestadual)) / 100;
                                    decimal totalIcmsInterestadualElencada = totalInterestadualElencada - icmsPresumidoInterestadualElencada;

                                    totalDarFecop += fecopInterestadualElencada;
                                    totalDarIcms += totalIcmsInterestadualElencada;
                                    impostoIcms += totalIcmsInterestadualElencada;
                                    impostoFecop += fecopInterestadualElencada;

                                    //  Deselencadas
                                    //  Internas
                                    decimal icmsInternaDeselencada = (InternasDeselencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                                    decimal fecopInternaDeselencada = (InternasDeselencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100;
                                    decimal totalInternasDeselencada = icmsInternaDeselencada;
                                    decimal icmsPresumidoInternaDeselencada = (InternasDeselencadasPortaria * Convert.ToDecimal(comp.IncIIInterna)) / 100;
                                    decimal totalIcmsInternaDeselencada = totalInternasDeselencada - icmsPresumidoInternaDeselencada;

                                    totalDarFecop += fecopInternaDeselencada;
                                    totalDarIcms += totalIcmsInternaDeselencada;
                                    impostoIcms += totalIcmsInternaDeselencada;
                                    impostoFecop += fecopInternaDeselencada;

                                    // Interestadual
                                    decimal icmsInterestadualDeselencada = (InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                                    decimal fecopInterestadualDeselencada = (InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100;
                                    decimal totalInterestadualDeselencada = icmsInterestadualDeselencada;
                                    decimal icmsPresumidoInterestadualDeselencada = (InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.IncIIInterestadual)) / 100;
                                    decimal totalIcmsInterestadualDeselencada = totalInterestadualDeselencada - icmsPresumidoInterestadualDeselencada;

                                    totalDarFecop += fecopInterestadualDeselencada;
                                    totalDarIcms += totalIcmsInterestadualDeselencada;
                                    impostoIcms += totalIcmsInterestadualDeselencada;
                                    impostoFecop += fecopInterestadualDeselencada;

                                    //  Percentual
                                    decimal percentualVendas = (vendasContribuintes * 100) / vendas;

                                    //  Suspensão
                                    decimal totalSuspensao = (suspensao * Convert.ToDecimal(comp.Suspension)) / 100;
                                    totalDarIcms += totalSuspensao;
                                    impostoIcms += totalSuspensao;

                                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                                    //  Elencadas
                                    // Internas
                                    ViewBag.VendasInternasElencadas = Convert.ToDouble(vendasInternasElencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.InternasElencadas = Convert.ToDouble(InternasElencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.InternasElencadasPortaria = Convert.ToDouble(InternasElencadasPortaria.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.IcmsInternasElencadas = Convert.ToDouble(icmsInternaElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.FecopInternasElencadas = Convert.ToDouble(fecopInternaElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.TotalInternasElencadas = Convert.ToDouble(totalInternasElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.IcmsPresumidoInternasElencadas = Convert.ToDouble(icmsPresumidoInternaElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.TotalIcmsInternasElencadas = Convert.ToDouble(totalIcmsInternaElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                                    // Interestadual
                                    ViewBag.VendasInterestadualElencadas = Convert.ToDouble(vendasInterestadualElencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.InterestadualElencadas = Convert.ToDouble(InterestadualElencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.InterestadualElencadasPortaria = Convert.ToDouble(InterestadualElencadasPortaria.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.IcmsInterestadualElencadas = Convert.ToDouble(icmsInterestadualElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.FecopInterestadualElencadas = Convert.ToDouble(fecopInterestadualElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.TotalInterestadualElencadas = Convert.ToDouble(totalInterestadualElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.IcmsPresumidoInterestadualElencadas = Convert.ToDouble(icmsPresumidoInterestadualElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.TotalIcmsInterestadualElencadas = Convert.ToDouble(totalIcmsInterestadualElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                                    //  Deselencadas
                                    //  Internas
                                    ViewBag.VendasInternasDeselencadas = Convert.ToDouble(vendasInternasDeselencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.InternasDeselencadas = Convert.ToDouble(InternasDeselencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.InternasDeselencadasPortaria = Convert.ToDouble(InternasDeselencadasPortaria.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""); ViewBag.IcmsInternasElencadas = Convert.ToDouble(icmsInternaElencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.IcmsInternasDeselencadas = Convert.ToDouble(icmsInternaDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.FecopInternasDeselencadas = Convert.ToDouble(fecopInternaDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.TotalInternasDeselencadas = Convert.ToDouble(totalInternasDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.IcmsPresumidoInternasDeselencadas = Convert.ToDouble(icmsPresumidoInternaDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.TotalIcmsInternasDeselencadas = Convert.ToDouble(totalIcmsInternaDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                                    // Interestadual
                                    ViewBag.VendasInterestadualDeselencadas = Convert.ToDouble(vendasInterestadualDeselencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.InterestadualDeselencadas = Convert.ToDouble(InterestadualDeselencadas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.InterestadualDeselencadasPortaria = Convert.ToDouble(InterestadualDeselencadasPortaria.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.IcmsInterestadualDeselencadas = Convert.ToDouble(icmsInterestadualDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.FecopInterestadualDeselencadas = Convert.ToDouble(fecopInterestadualDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.TotalInterestadualDeselencadas = Convert.ToDouble(totalInterestadualDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.IcmsPresumidoInterestadualDeselencadas = Convert.ToDouble(icmsPresumidoInterestadualDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.TotalIcmsInterestadualDeselencadas = Convert.ToDouble(totalIcmsInterestadualDeselencada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                                    //  Percentual
                                    ViewBag.PercentualVendas = percentualVendas;

                                    //  Suspensão
                                    ViewBag.PercentualSuspensao = comp.Suspension;
                                    ViewBag.Suspensao = Convert.ToDouble(suspensao.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                    ViewBag.TotalSuspensao = Convert.ToDouble(totalSuspensao.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                }

                                ViewBag.AliqInterna = comp.AliqInterna;
                                ViewBag.Fecop = comp.Fecop;

                                //  Elencadas
                                ViewBag.IncIInterna = comp.IncIInterna;
                                ViewBag.IncIInterestadual = comp.IncIInterestadual;

                                //  Deselencadas
                                ViewBag.IncIIInterna = comp.IncIIInterna;
                                ViewBag.IncIIInterestadual = comp.IncIIInterestadual;

                            }

                            if (!comp.AnnexId.Equals(3)) 
                            { 
                                ViewBag.Icms = comp.Icms;
                                ViewBag.Fecop = comp.Fecop;

                                //decimal baseIcms = productsP.Select(_ => _.Vbasecalc).Sum();
                                decimal baseIcms = Convert.ToDecimal(productsP.Select(_ => _.Vprod).Sum() + productsP.Select(_ => _.Voutro).Sum() +
                                                productsP.Select(_ => _.Vseg).Sum() - productsP.Select(_ => _.Vdesc).Sum() + productsP.Select(_ => _.Vfrete).Sum() +
                                                productsP.Select(_ => _.Freterateado).Sum() + productsP.Select(_ => _.Vipi).Sum());
                                ViewBag.Base = Convert.ToDouble(Math.Round(baseIcms, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                impostoIcms = Convert.ToDecimal(baseIcms * (comp.Icms / 100));
                                impostoFecop = Convert.ToDecimal(baseIcms * (comp.Fecop / 100));
                                

                                totalDarFecop += impostoFecop;
                                totalDarIcms += impostoIcms;
                            }

                            ViewBag.ImpostoFecop = Convert.ToDouble(Math.Round(impostoFecop, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.ImpostoIcms = Convert.ToDouble(Math.Round(impostoIcms, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            decimal? basefunef = impostoGeral - impostoIcms;
                            ViewBag.BaseFunef = Convert.ToDouble(Math.Round(Convert.ToDecimal(basefunef), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.Funef = comp.Funef;

                            decimal taxaFunef = 0;

                            if (basefunef > 0)
                            {
                                taxaFunef = Convert.ToDecimal(basefunef * (Convert.ToDecimal(comp.Funef) / 100));
                            }
                            ViewBag.TaxaFunef = Convert.ToDouble(Math.Round(taxaFunef, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            totalDarFunef += taxaFunef;

                            totalImpostoIncentivo = impostoIcms + impostoFecop + taxaFunef;

                            if(typeTaxation == 1 && type != 8 && type != 7) {
                                ViewBag.TotalImpostoIncentivo = Convert.ToDouble(Math.Round(totalImpostoIncentivo + (diefStSIE - icmsStnotaSIE) + (totalfecop1SIE + totalfecop2SIE), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            }
                            else if(typeTaxation == 1 && type == 7)
                            {
                                ViewBag.TotalImpostoIncentivo = Convert.ToDouble(Math.Round(totalImpostoIncentivo, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            }

                            
                            if (type != 8 && type != 7 && comp.AnnexId != 3)
                            {
                                List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                                List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();

                                /*List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                                List<List<Dictionary<string, string>>> notesTranferencia = new List<List<Dictionary<string, string>>>();
                                List<List<Dictionary<string, string>>> notesEntradaDevolucao = new List<List<Dictionary<string, string>>>();
                                List<List<Dictionary<string, string>>> notesSaidaDevolucao = new List<List<Dictionary<string, string>>>();
                                List<List<Dictionary<string, string>>> notesTransferenciaEntrada = new List<List<Dictionary<string, string>>>();
                                List<List<Dictionary<string, string>>> notesDevoSaida = new List<List<Dictionary<string, string>>>();
                                List<List<Dictionary<string, string>>> notesDevoEntrada = new List<List<Dictionary<string, string>>>();*/

                                exitNotes = import.Nfe(directoryNfeExit);
                                entryNotes = import.Nfe(directoryNfeEntrada);

                                var ncms = _ncmConvenioService.FindByAnnex(Convert.ToInt32(comp.AnnexId));
                                var clientesAll = _clientService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).Select(_ => _.Document).ToList();
                                var suspensions = _suspensionService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).ToList();

                                /*notesDevoEntrada = import.NfeExit(directoryNfeEntrada, id, type, "devo");
                                notesTransferenciaEntrada = import.NotesTransfer(directoryNfeEntrada, id);
                                notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                                notesTranferencia = import.NfeExit(directoryNfeExit, id, type, "transferencia");
                                notesTransferenciaEntrada = import.NotesTransfer(directoryNfeEntrada, id);
                                notesDevoSaida = import.NfeExit(directoryNfeExit, id, type, "devo");
                                notesDevoEntrada = import.NfeExit(directoryNfeEntrada, id, type, "devo");*/


                                var contribuintes = _clientService.FindByContribuinte(id, "all");
                                var contribuintesRaiz = _clientService.FindByContribuinte(id, "raiz");

                                var cfopsVenda = _companyCfopService.FindByCfopActive(id, "venda", "venda").Select(_ => _.Cfop.Code);
                                var cfopsTransf = _companyCfopService.FindByCfopActive(id, "venda", "transferencia").Select(_ => _.Cfop.Code);
                                var cfopsDevo = _companyCfopService.FindByCfopActive(id, "venda", "devo").Select(_ => _.Cfop.Code);

                                decimal totalVendas = 0, totalNcm = 0, totalTranferencias = 0, totalSaida = 0, totalDevo = 0,
                                    totalDevoAnexo = 0, totalDevoContribuinte = 0, totalVendasSuspensao = 0;
                                int contContribuintes = contribuintes.Count();
                                int contContribuintesRaiz = contribuintesRaiz.Count() + 1;

                                string[,] resumoCnpjs = new string[contContribuintes, 2];
                                string[,] resumoCnpjRaiz = new string[contContribuintesRaiz, 2];
                                string[,] resumoAllCnpjRaiz = new string[contContribuintesRaiz - 1, 3];

                                for (int i = 0; i < contContribuintes; i++)
                                {
                                    resumoCnpjs[i, 0] = contribuintes[i];
                                    resumoCnpjs[i, 1] = "0";
                                }

                                for (int i = 0; i < contContribuintesRaiz; i++)
                                {
                                    if (i < contContribuintesRaiz - 1)
                                    {
                                        resumoCnpjRaiz[i, 0] = contribuintesRaiz[i];
                                        resumoAllCnpjRaiz[i, 0] = contribuintesRaiz[i];
                                        resumoCnpjRaiz[i, 1] = "0";
                                        resumoAllCnpjRaiz[i, 1] = "0";
                                        resumoAllCnpjRaiz[i, 2] = "0";
                                    }
                                    else
                                    {
                                        resumoCnpjRaiz[i, 0] = "Não contribuinte";
                                        resumoCnpjRaiz[i, 1] = "0";
                                    }
                                }

                                decimal totalEntradas = 0, totalTranferenciaInter = 0;

                                // Transferência Entrada
                                for (int i = entryNotes.Count - 1; i >= 0; i--)
                                {
                                    if (!entryNotes[i][3]["CNPJ"].Equals(comp.Document))
                                    {
                                        entryNotes.RemoveAt(i);
                                        continue;
                                    }

                                    bool cfop = false;

                                    for (int j = 0; j < entryNotes[i].Count; j++)
                                    {
                                        if (entryNotes[i][j].ContainsKey("CFOP"))
                                        {
                                            cfop = false;
                                            if (cfopsTransf.Contains(entryNotes[i][j]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }
                                        if (cfop == true)
                                        {
                                            if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vProd"))
                                            {
                                                if (entryNotes[i][1]["idDest"].Equals("2"))
                                                {
                                                    totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                                }
                                                totalEntradas += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            }
                                            if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vFrete"))
                                            {
                                                if (entryNotes[i][1]["idDest"].Equals("2"))
                                                {
                                                    totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                                }
                                                totalEntradas += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            }
                                            if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vDesc"))
                                            {
                                                if (entryNotes[i][1]["idDest"].Equals("2"))
                                                {
                                                    totalTranferenciaInter -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                                }
                                                totalEntradas -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            }
                                            if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vOutro"))
                                            {
                                                if (entryNotes[i][1]["idDest"].Equals("2"))
                                                {
                                                    totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                                }
                                                totalEntradas += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            }
                                            if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vSeg"))
                                            {
                                                if (entryNotes[i][1]["idDest"].Equals("2"))
                                                {
                                                    totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                                }
                                                totalEntradas += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            }
                                        }
                                    }

                                }

                                // Vendas 
                                for (int i = exitNotes.Count - 1; i >= 0; i--)
                                {
                                    if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }

                                    int posClienteRaiz = contContribuintesRaiz - 1, posCliente = -1;

                                    bool status = false, suspenso = false, cfop = false;

                                    if (exitNotes[i][1].ContainsKey("dhEmi"))
                                    {
                                        foreach (var suspension in suspensions)
                                        {
                                            if (Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) >= Convert.ToDateTime(suspension.DateStart) && Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) < Convert.ToDateTime(suspension.DateEnd))
                                            {
                                                suspenso = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("IE") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][1]["mod"].Equals("55"))
                                    {
                                        string CNPJ = exitNotes[i][3].ContainsKey("CNPJ") ? exitNotes[i][3]["CNPJ"] : "escon";
                                        string indIEDest = exitNotes[i][3].ContainsKey("indIEDest") ? exitNotes[i][3]["indIEDest"] : "escon";
                                        string IE = exitNotes[i][3].ContainsKey("IE") ? exitNotes[i][3]["IE"] : "escon";

                                        if (contribuintesRaiz.Contains(exitNotes[i][3]["CNPJ"].Substring(0, 8)))
                                        {
                                            posClienteRaiz = contribuintesRaiz.IndexOf(exitNotes[i][3]["CNPJ"].Substring(0, 8));
                                        }

                                        if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                        {
                                            posCliente = contribuintes.IndexOf(exitNotes[i][3]["CNPJ"]);
                                        }

                                        bool existe = false;

                                        if (clientesAll.Contains(exitNotes[i][3]["CNPJ"]))
                                        {
                                            existe = true;
                                        }

                                        if (existe == false)
                                        {
                                            throw new Exception("Há Clientes não Importados");
                                        }
                                    }

                                    for (int k = 0; k < exitNotes[i].Count(); k++)
                                    {

                                        if (exitNotes[i][k].ContainsKey("NCM"))
                                        {
                                            status = false;

                                            for (int j = 0; j < ncms.Count(); j++)
                                            {
                                                int tamanho = ncms[j].Length;

                                                if (ncms[j].Equals(exitNotes[i][k]["NCM"].Substring(0, tamanho)))
                                                {
                                                    status = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("CFOP"))
                                        {
                                            cfop = false;
                                            if (cfopsVenda.Contains(exitNotes[i][k]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }

                                        if (status == true && cfop == true)
                                        {

                                            if (exitNotes[i][k].ContainsKey("vProd") && exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                totalNcm += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                                }

                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                }
                                            }

                                            if (exitNotes[i][k].ContainsKey("vFrete") && exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                totalNcm += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                                }
                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                }
                                            }

                                            if (exitNotes[i][k].ContainsKey("vDesc") && exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                totalVendas -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                totalNcm -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                                }
                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                }
                                            }

                                            if (exitNotes[i][k].ContainsKey("vOutro") && exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                totalNcm += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                                }
                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                }
                                            }

                                            if (exitNotes[i][k].ContainsKey("vSeg") && exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                totalNcm += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();

                                                }
                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                }
                                            }
                                        }
                                        else if (status == false && cfop == true)
                                        {

                                            if (exitNotes[i][k].ContainsKey("vProd") && exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                                }
                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                }
                                            }

                                            if (exitNotes[i][k].ContainsKey("vFrete") && exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                                }
                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                }
                                            }

                                            if (exitNotes[i][k].ContainsKey("vDesc") && exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                totalVendas -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                                }
                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                }
                                            }

                                            if (exitNotes[i][k].ContainsKey("vOutro") && exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                                }
                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                }
                                            }

                                            if (exitNotes[i][k].ContainsKey("vSeg") && exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();

                                                }
                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                }
                                            }
                                        }

                                    }

                                }

                                // Transferência Saida
                                for (int i = exitNotes.Count - 1; i >= 0; i--)
                                {

                                    if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }

                                    bool suspenso = false;

                                    if (exitNotes[i][1].ContainsKey("dhEmi"))
                                    {
                                        foreach (var suspension in suspensions)
                                        {
                                            if (Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) >= Convert.ToDateTime(suspension.DateStart) && Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) < Convert.ToDateTime(suspension.DateEnd))
                                            {
                                                suspenso = true;
                                                break;
                                            }
                                        }
                                    }

                                    int posClienteRaiz = contContribuintesRaiz - 1;

                                    if (exitNotes[i][3].ContainsKey("CNPJ"))
                                    {
                                        if (contribuintesRaiz.Contains(exitNotes[i][3]["CNPJ"].Substring(0, 8)))
                                        {
                                            posClienteRaiz = contribuintesRaiz.IndexOf(exitNotes[i][3]["CNPJ"].Substring(0, 8));
                                        }
                                    }

                                    bool cfop = false;
                                    for (int j = 0; j < exitNotes[i].Count; j++)
                                    {
                                        if (exitNotes[i][j].ContainsKey("CFOP"))
                                        {
                                            cfop = false;
                                            if (cfopsTransf.Contains(exitNotes[i][j]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }

                                        if (cfop == true)
                                        {
                                            if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                                }
                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();

                                                }
                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalTranferencias -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();

                                                }
                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();

                                                }
                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();

                                                }
                                                if (suspenso == true)
                                                {
                                                    totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                }
                                            }
                                        }

                                    }

                                }

                                // Devolução Saida
                                for (int i = exitNotes.Count - 1; i >= 0; i--)
                                {
                                    if (exitNotes[i][3].ContainsKey("CNPJ"))
                                    {
                                        if (!exitNotes[i][3]["CNPJ"].Equals(comp.Document))
                                        {
                                            exitNotes.RemoveAt(i);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }

                                    if (exitNotes[i][1]["finNFe"] != "4")
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }

                                    bool existe = false, status = false, cfop = false;

                                    if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("IE") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][1]["mod"].Equals("55"))
                                    {

                                        if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                        {
                                            existe = true;
                                        }
                                    }

                                    for (int j = 0; j < exitNotes[i].Count(); j++)
                                    {
                                        if (exitNotes[i][j].ContainsKey("NCM"))
                                        {
                                            status = false;

                                            for (int k = 0; k < ncms.Count(); k++)
                                            {
                                                int tamanho = ncms[k].Length;

                                                if (ncms[k].Equals(exitNotes[i][j]["NCM"].Substring(0, tamanho)))
                                                {
                                                    status = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("CFOP"))
                                        {
                                            cfop = false;
                                            if (cfopsDevo.Contains(exitNotes[i][j]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }

                                        if (status == true && cfop == true)
                                        {
                                            if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                totalDevoAnexo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                }
                                            }
                                        }
                                        else if (status == false && cfop == true)
                                        {
                                            if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                }

                                            }

                                            if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                            {

                                                totalDevo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                }
                                            }
                                        }

                                    }

                                }

                                // Devolução Entrada
                                for (int i = entryNotes.Count - 1; i >= 0; i--)
                                {
                                    if (entryNotes[i][3].ContainsKey("CNPJ"))
                                    {
                                        if (!entryNotes[i][3]["CNPJ"].Equals(comp.Document))
                                        {
                                            entryNotes.RemoveAt(i);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        entryNotes.RemoveAt(i);
                                        continue;
                                    }

                                    if (entryNotes[i][1]["finNFe"] != "4")
                                    {
                                        entryNotes.RemoveAt(i);
                                        continue;
                                    }

                                    bool existe = false;
                                    int posClienteRaiz = contContribuintesRaiz - 1;

                                    if (entryNotes[i][3].ContainsKey("CNPJ") && entryNotes[i][3].ContainsKey("IE") && entryNotes[i][3].ContainsKey("indIEDest") && entryNotes[i][1]["mod"].Equals("55"))
                                    {

                                        if (contribuintes.Contains(entryNotes[i][3]["CNPJ"]))
                                        {
                                            existe = true;
                                        }

                                        string CNPJ = entryNotes[i][3].ContainsKey("CNPJ") ? entryNotes[i][3]["CNPJ"] : "escon";
                                        string indIEDest = entryNotes[i][3].ContainsKey("indIEDest") ? entryNotes[i][3]["indIEDest"] : "escon";
                                        string IE = entryNotes[i][3].ContainsKey("IE") ? entryNotes[i][3]["IE"] : "escon";

                                        if (contribuintesRaiz.Contains(entryNotes[i][3]["CNPJ"].Substring(0, 8)))
                                        {
                                            posClienteRaiz = contribuintesRaiz.IndexOf(entryNotes[i][3]["CNPJ"].Substring(0, 8));
                                        }
                                    }

                                    bool status = false, cfop = false;

                                    for (int j = 0; j < entryNotes[i].Count(); j++)
                                    {

                                        if (entryNotes[i][j].ContainsKey("NCM"))
                                        {
                                            status = false;

                                            for (int k = 0; k < ncms.Count(); k++)
                                            {
                                                int tamanho = ncms[k].Length;

                                                if (ncms[k].Equals(entryNotes[i][j]["NCM"].Substring(0, tamanho)))
                                                {
                                                    status = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("CFOP"))
                                        {
                                            cfop = false;
                                            if (cfopsDevo.Contains(entryNotes[i][j]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }

                                        if (status == true && cfop == true)
                                        {
                                            if (entryNotes[i][j].ContainsKey("vProd") && entryNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                                totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                                }
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vProd"])).ToString();
                                                }
                                            }

                                            if (entryNotes[i][j].ContainsKey("vFrete") && entryNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                                totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                                }
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vFrete"])).ToString();
                                                }
                                            }

                                            if (entryNotes[i][j].ContainsKey("vDesc") && entryNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                                totalDevoAnexo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                                }
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(entryNotes[i][j]["vDesc"])).ToString();
                                                }
                                            }

                                            if (entryNotes[i][j].ContainsKey("vOutro") && entryNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                                totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                                }

                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vOutro"])).ToString();
                                                }
                                            }

                                            if (entryNotes[i][j].ContainsKey("vSeg") && entryNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                                totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                                }
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vSeg"])).ToString();
                                                }
                                            }
                                        }
                                        else if (status == false && cfop == true)
                                        {
                                            if (entryNotes[i][j].ContainsKey("vProd") && entryNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                                }

                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vProd"])).ToString();
                                                }

                                            }

                                            if (entryNotes[i][j].ContainsKey("vFrete") && entryNotes[i][j].ContainsKey("cProd"))
                                            {

                                                totalDevo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                                }

                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vFrete"])).ToString();
                                                }

                                            }

                                            if (entryNotes[i][j].ContainsKey("vDesc") && entryNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                                }

                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(entryNotes[i][j]["vDesc"])).ToString();
                                                }
                                            }

                                            if (entryNotes[i][j].ContainsKey("vOutro") && entryNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                                }
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vOutro"])).ToString();
                                                }
                                            }

                                            if (entryNotes[i][j].ContainsKey("vSeg") && entryNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                                if (existe == true)
                                                {
                                                    totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                                }

                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vSeg"])).ToString();
                                                }
                                            }
                                        }
                                    }

                                }

                                decimal totalNcontribuinte = Convert.ToDecimal(resumoCnpjRaiz[contContribuintesRaiz - 1, 1]), baseCalc = totalVendas - totalDevo;
                                decimal totalContribuinte = totalVendas - totalNcontribuinte;
                                decimal baseCalcContribuinte = totalContribuinte - totalDevoContribuinte;
                                decimal totalDevoNContribuinte = totalDevo - totalDevoContribuinte;
                                decimal baseCalcNContribuinte = totalNcontribuinte - totalDevoNContribuinte;

                                totalSaida = baseCalc + totalTranferencias;
                                totalVendas = totalVendas + totalTranferencias;
                                decimal limiteNContribuinte = (baseCalc * (Convert.ToDecimal(comp.VendaCpf))) / 100,
                                    limiteNcm = (baseCalc * Convert.ToDecimal(comp.VendaAnexo)) / 100,
                                    limiteGrupo = (totalSaida * Convert.ToDecimal(comp.VendaMGrupo)) / 100,
                                    limiteTransferencia = (totalEntradas * Convert.ToDecimal(comp.TransferenciaInterExcedente)) / 100;

                                decimal impostoNContribuinte = 0, excedenteNContribuinte = 0, excedenteNcm = 0, impostoNcm = 0, excedenteTranfInter = 0, impostoTransfInter = 0;


                                //CNPJ
                                List<List<string>> gruposExecentes = new List<List<string>>();
                                decimal totalVendaGrupo = 0, totalExcedente = 0, totalDevoGrupo = 0, totalImpostoGrupo = 0;
                                for (int i = 0; i < contContribuintesRaiz - 1; i++)
                                {
                                    var vendaGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 1]);
                                    var devoGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 2]);
                                    var baseCalcGrupo = (vendaGrupo - devoGrupo);
                                    if (baseCalcGrupo > limiteGrupo)
                                    {
                                        List<string> grupoExcedente = new List<string>();
                                        var cnpjGrupo = resumoAllCnpjRaiz[i, 0];
                                        var clientGrupo = _clientService.FindByRaiz(cnpjGrupo);
                                        var nomeGrupo = clientGrupo.Name;
                                        var percentGrupo = Math.Round((baseCalcGrupo / totalSaida) * 100, 2);
                                        var excedenteGrupo = baseCalcGrupo - limiteGrupo;
                                        var impostoGrupo = (excedenteGrupo * Convert.ToDecimal(comp.VendaMGrupoExcedente)) / 100;
                                        totalVendaGrupo += vendaGrupo;
                                        totalDevoGrupo += devoGrupo;
                                        totalExcedente += baseCalcGrupo;
                                        totalImpostoGrupo += impostoGrupo;
                                        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");
                                        grupoExcedente.Add(cnpjGrupo);
                                        grupoExcedente.Add(nomeGrupo);
                                        grupoExcedente.Add(percentGrupo.ToString());
                                        grupoExcedente.Add(Math.Round(Convert.ToDouble(vendaGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                                        grupoExcedente.Add(Math.Round(Convert.ToDouble(limiteGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                                        grupoExcedente.Add(Math.Round(Convert.ToDouble(excedenteGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                                        grupoExcedente.Add(comp.VendaMGrupoExcedente.ToString());
                                        grupoExcedente.Add(Math.Round(Convert.ToDouble(impostoGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                                        grupoExcedente.Add(Math.Round(Convert.ToDouble(baseCalcGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                                        grupoExcedente.Add(Math.Round(Convert.ToDouble(devoGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());

                                        gruposExecentes.Add(grupoExcedente);
                                        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                                    }
                                }

                                decimal baseCalcNcm = totalNcm - totalDevoAnexo;

                                //Anexo II
                                if (baseCalcNcm < limiteNcm)
                                {
                                    excedenteNcm = limiteNcm - baseCalcNcm;
                                    impostoNcm = (excedenteNcm * Convert.ToDecimal(comp.VendaAnexoExcedente)) / 100;
                                }

                                //Não Contribuinte
                                if (baseCalcNContribuinte > limiteNContribuinte && limiteNContribuinte > 0)
                                {
                                    excedenteNContribuinte = baseCalcNContribuinte - limiteNContribuinte;
                                    impostoNContribuinte = (excedenteNContribuinte * Convert.ToDecimal(comp.VendaCpfExcedente)) / 100;
                                }

                                // Transferência inter
                                if (limiteTransferencia < totalTranferenciaInter)
                                {
                                    excedenteTranfInter = totalTranferenciaInter - limiteTransferencia;
                                    impostoTransfInter = (excedenteTranfInter * Convert.ToDecimal(comp.TransferenciaInterExcedente)) / 100;
                                }

                                // Suspensão
                                decimal valorSuspensao = (totalVendasSuspensao * Convert.ToDecimal(comp.Suspension)) / 100;

                                // Percentuais
                                decimal percentualVendaContribuinte = (baseCalcContribuinte * 100) / baseCalc;
                                decimal percentualVendaNContribuinte = (baseCalcNContribuinte * 100) / baseCalc;
                                decimal percentualVendaAnexo = (baseCalcNcm * 100) / baseCalc;
                                decimal percentualGrupo = (totalExcedente * 100) / baseCalc;

                                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                                //Geral
                                ViewBag.Contribuinte = Math.Round(Convert.ToDouble(totalContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.NContribuinte = Math.Round(Convert.ToDouble(totalNcontribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalVenda = Math.Round(Convert.ToDouble(totalVendas.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.VendaAnexo = Math.Round(Convert.ToDouble(totalNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalTransferencia = Math.Round(Convert.ToDouble(totalTranferenciaInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalDevo = Math.Round(Convert.ToDouble(totalDevo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.BaseCalc = Math.Round(Convert.ToDouble(baseCalc.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalDevoAnexo = Math.Round(Convert.ToDouble(totalDevoAnexo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.BaseCalcAnexo = Math.Round(Convert.ToDouble(baseCalcNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalDevoContrib = Math.Round(Convert.ToDouble(totalDevoContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.BaseCalcContrib = Math.Round(Convert.ToDouble(baseCalcContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalDevoNContrib = Math.Round(Convert.ToDouble(totalDevoNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.BaseCalcNContrib = Math.Round(Convert.ToDouble(baseCalcNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                                // Percentuais
                                ViewBag.PercentualVendaContribuinte = Math.Round(Convert.ToDouble(percentualVendaContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.PercentualVendaNContribuinte = Math.Round(Convert.ToDouble(percentualVendaNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.PercentualVendaAnexo = Math.Round(Convert.ToDouble(percentualVendaAnexo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.PercentualVendaGrupo = Math.Round(Convert.ToDouble(percentualGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                                //CNPJ
                                ViewBag.PercentualCNPJ = comp.VendaMGrupo;
                                ViewBag.TotalVendaGrupo = Math.Round(Convert.ToDouble(totalVendaGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalDevoGrupo = Math.Round(Convert.ToDouble(totalDevoGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.TotalExecedente = Math.Round(Convert.ToDouble(totalExcedente.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.Grupo = gruposExecentes;

                                //Anexo II
                                ViewBag.LimiteAnexo = Math.Round(Convert.ToDouble(limiteNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.ExcedenteAnexo = Math.Round(Convert.ToDouble(excedenteNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.PercentualExcedenteAnexo = comp.VendaAnexoExcedente;
                                ViewBag.TotalExcedenteAnexo = Math.Round(Convert.ToDouble(impostoNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                                //Não Contribuinte
                                ViewBag.LimiteNContribuinte = Math.Round(Convert.ToDouble(limiteNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.ExcedenteNContribuinte = Math.Round(Convert.ToDouble(excedenteNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.PercentualExcedenteNContribuinte = comp.VendaCpfExcedente;
                                ViewBag.TotalExcedenteNContribuinte = Math.Round(Convert.ToDouble(impostoNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                                //Tranferência
                                ViewBag.LimiteTransferencia = Math.Round(Convert.ToDouble(limiteTransferencia.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.ExcedenteTransferencia = Math.Round(Convert.ToDouble(excedenteTranfInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.PercentaulTransferencia = comp.TransferenciaInterExcedente;
                                ViewBag.TotalExcedenteTransferencia = Math.Round(Convert.ToDouble(impostoTransfInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                                // Suspensão
                                ViewBag.BaseCalcSuspensao = Math.Round(Convert.ToDouble(totalVendasSuspensao.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                                ViewBag.PercentaulSuspensao = comp.Suspension;
                                ViewBag.TotalSuspensao = Math.Round(Convert.ToDouble(valorSuspensao.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                                //Total Icms
                                ViewBag.TotalIcms = Math.Round(Convert.ToDouble((impostoNcm + impostoNContribuinte + impostoTransfInter + totalImpostoGrupo + valorSuspensao).ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                                totalDarIcms += (impostoNcm + impostoNContribuinte + impostoTransfInter + totalImpostoGrupo + valorSuspensao);
                            }
                        }

                        ViewBag.TotalDarSTCO = Convert.ToDouble(Math.Round(totalDarSTCO, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalDarFecop = Convert.ToDouble(Math.Round(totalDarFecop, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalDarFunef = Convert.ToDouble(Math.Round(totalDarFunef, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalDarIcms = Convert.ToDouble(Math.Round(totalDarIcms, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalDarCotac = Convert.ToDouble(Math.Round(totalDarCotac, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    }
                    else if (typeTaxation >= 2 && typeTaxation <= 5)
                    {
                        decimal totalIcmsFreteIE = 0;
                        foreach(var prod in products)
                        {
                            if (!prod.Note.Iest.Equals(""))
                            {
                                if (Convert.ToDecimal(prod.Diferencial) > 0)
                                {
                                    totalIcmsFreteIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                                }
                            }
                        }
                        ViewBag.TotalFreteIE = Convert.ToDouble(totalIcmsFreteIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        
                        totalIcmsIE = products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.IcmsApurado).Sum();
                        totalIcmsSIE = products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.IcmsApurado).Sum();

                        decimal valorNfe1NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe1RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                        decimal valorNfe2NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe2RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                        decimal valorNfe1NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe1RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                        decimal valorNfe2NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        decimal valorNfe2RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                        icmsStnoteSIE += valorNfe1NormalSIE + valorNfe1RetSIE + valorNfe2NormalSIE + valorNfe2RetSIE;
                        icmsStnoteIE += valorNfe1NormalIE + valorNfe1RetIE + valorNfe2NormalIE + valorNfe2RetIE;

                        decimal gnrePagaIE = 0, gnreNPagaIE = 0, gnrePagaSIE = 0, gnreNPagaSIE = 0;
                        decimal? icmsApIE = 0, icmsApSIE = 0;
                        if (typeTaxation == 2)
                        {
                            gnrePagaIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreAp).Distinct().Sum()), 2);
                            gnreNPagaIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreNAp).Distinct().Sum()), 2);
                            gnrePagaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreAp).Distinct().Sum()), 2);
                            gnreNPagaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreNAp).Distinct().Sum()), 2);
                            icmsApIE = products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.IcmsAp).Distinct().Sum();
                            icmsApSIE = products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.IcmsAp).Distinct().Sum();
                        }
                        else if (typeTaxation == 3)
                        {
                            gnrePagaIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreCo).Distinct().Sum()), 2);
                            gnreNPagaIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreNCo).Distinct().Sum()), 2);
                            gnrePagaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreCo).Distinct().Sum()), 2);
                            gnreNPagaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreNCo).Distinct().Sum()), 2);
                            icmsApIE = products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.IcmsCo).Distinct().Sum();
                            icmsApSIE = products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.IcmsCo).Distinct().Sum();

                        }
                        else if (typeTaxation == 5)
                        {
                            gnrePagaIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreIm).Distinct().Sum()), 2);
                            gnreNPagaIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreNIm).Distinct().Sum()), 2);
                            gnrePagaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreIm).Distinct().Sum()), 2);
                            gnreNPagaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreNIm).Distinct().Sum()), 2);
                            icmsApIE = products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.IcmsIm).Distinct().Sum();
                            icmsApSIE = products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.IcmsIm).Distinct().Sum();
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


                        valorDiefIE = Convert.ToDecimal(totalIcmsIE - icmsStnoteIE - gnrePagaIE + gnreNPagaIE - totalIcmsFreteIE);
                        var valorDiefSIE = Convert.ToDecimal((totalIcmsSIE + totalIcmsFreteIE) - icmsStnoteSIE - gnrePagaSIE + gnreNPagaSIE);

                        ViewBag.ValorDiefIE = Convert.ToDouble(valorDiefIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ValorDiefSIE = Convert.ToDouble(valorDiefSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.IcmsApIE = Convert.ToDouble(icmsApIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsApSIE = Convert.ToDouble(icmsApSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.IcmsPagarIE = Convert.ToDouble(valorDiefIE - icmsApIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsPagarSIE = Convert.ToDouble(valorDiefSIE - icmsApSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    }


                    
                }
                else if (type == 3)
                {
                    var prod = products.Where(_ => _.Pautado.Equals(false));
                    ViewBag.product = prod;
                }
                else if (type == 6)
                {
                    decimal totalDarSTCO = 0, totalDarFecop = 0, totalDarAp = 0, totalDarIm = 0, totalDarIcms = 0, totalDarCotac = 0, totalDarFunef = 0;
                    products = _service.FindByProducts(notes);
                    // Substituição Tributária

                    decimal totalIcmsFreteSTIE = 0, totalFecop1FreteSTIE = 0, totalFecop2FreteSTIE = 0;

                    foreach (var prod in products)
                    {
                        if (!prod.Note.Iest.Equals("") && (prod.TaxationTypeId.Equals(5) || prod.TaxationTypeId.Equals(6)))
                        {
                            if (Convert.ToDecimal(prod.Aliqinterna) > 0)
                            {
                                decimal valorAgreg = 0;
                                if (prod.Mva != null)
                                {
                                    valorAgreg = calculation.ValorAgregadoMva(prod.Freterateado, Convert.ToDecimal(prod.Mva));
                                }
                                if (prod.BCR != null)
                                {
                                    valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(prod.BCR), valorAgreg);
                                }
                                if (prod.Fecop != null)
                                {
                                    if (Convert.ToDecimal(prod.Fecop).Equals(1))
                                    {
                                        totalFecop1FreteSTIE += calculation.valorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                    }
                                    else
                                    {
                                        totalFecop2FreteSTIE += calculation.valorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                    }

                                }

                                totalIcmsFreteSTIE += calculation.valorAgregadoAliqInt(Convert.ToDecimal(prod.Aliqinterna), Convert.ToDecimal(prod.Fecop), valorAgreg) - prod.IcmsCTe;
                            }
                        }
                    }

                    ViewBag.TotalIcmsFreteSTIE = Convert.ToDouble(totalIcmsFreteSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFecopFreteSTIE = Convert.ToDouble(totalFecop1FreteSTIE + totalFecop2FreteSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    decimal? gnrePagaSTIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreSt).Sum()), 2);
                    decimal? gnreNPagaSTIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ =>_.GnreNSt).Sum()), 2);
                    decimal? gnrePagaSTSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreSt).Sum()), 2);
                    decimal? gnreNPagaSTSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNSt).Sum()), 2);

                    decimal? icmsStSTIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.IcmsST).Sum()), 2);
                    decimal? icmsStSTSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.IcmsST).Sum()), 2);
                    decimal? totalApuradoSTIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.TotalICMS).Sum()), 2);
                    decimal? totalApuradoSTSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.TotalICMS).Sum()), 2);
                    decimal? totalDiefSTSIE = (totalApuradoSTSIE + totalIcmsFreteSTIE) - icmsStSTSIE + gnreNPagaSTSIE - gnrePagaSTSIE;
                    decimal? totalDiefSTIE = totalApuradoSTIE - icmsStSTIE + gnreNPagaSTIE - gnrePagaSTIE - totalIcmsFreteSTIE;
                    int? qtdSTSIE = products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Count();
                    int? qtdSTIE = products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Count();

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

                    decimal valorbase1STIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                    decimal valorbase1STSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                    decimal valorbase2STIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                    decimal valorbase2STSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);

                    decimal TotalFecopCalcSTIE = valorbase1STIE + valorbase2STIE;
                    decimal TotalFecopCalcSTSIE = valorbase1STSIE + valorbase2STSIE;
                    ViewBag.TotalFecopCalculadaSTIE = Convert.ToDouble(Math.Round(TotalFecopCalcSTIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFecopCalculadaSTSIE = Convert.ToDouble(Math.Round(TotalFecopCalcSTSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

      
                    decimal valorNfe1NormalSTIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetSTIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe1NormalSTSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetSTSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal valorNfe2NormalSTIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetSTIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalSTSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetSTSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    
                    decimal TotalFecopNfeSTIE = valorNfe1NormalSTIE + valorNfe1RetSTIE + valorNfe2NormalSTIE + valorNfe2RetSTIE;
                    decimal TotalFecopNfeSTSIE = valorNfe1NormalSTSIE + valorNfe1RetSTSIE + valorNfe2NormalSTSIE + valorNfe2RetSTSIE;
                    ViewBag.TotalFecopNfeSTIE = Convert.ToDouble(Math.Round(TotalFecopNfeSTIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFecopNfeSTSIE = Convert.ToDouble(Math.Round(TotalFecopNfeSTSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    
                    decimal gnreNPagaFecopSTIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2);
                    decimal gnreNPagaFecopSTSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2);
                    ViewBag.GNREnPagaFecopSTIE = Convert.ToDouble(gnreNPagaFecopSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GNREnPagaFecopSTSIE = Convert.ToDouble(gnreNPagaFecopSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal gnrePagaFecop1STIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2);
                    decimal gnrePagaFecop1STSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2);
                    decimal gnrePagaFecop2STIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2);
                    decimal gnrePagaFecop2STSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2);
                    ViewBag.GNREPagaFecopSTIE = Convert.ToDouble(gnrePagaFecop2STIE + gnrePagaFecop1STIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GNREPagaFecopSTSIE = Convert.ToDouble(gnrePagaFecop2STSIE + gnrePagaFecop1STSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal totalGnreFecopSTIE = gnrePagaFecop1STIE + gnrePagaFecop2STIE;
                    ViewBag.TotalGnreFecopSTIE = Convert.ToDouble(totalGnreFecopSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    decimal totalGnreFecopSTSIE = gnrePagaFecop1STSIE + gnrePagaFecop2STSIE;
                    ViewBag.TotalGnreFecopSTSIE = Convert.ToDouble(totalGnreFecopSTSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal totalfecopDiefSTIE = TotalFecopCalcSTIE - totalGnreFecopSTIE + gnreNPagaFecopSTIE - TotalFecopNfeSTIE - totalFecop1FreteSTIE - totalFecop2FreteSTIE;
                    decimal totalfecopDiefSTSIE = TotalFecopCalcSTSIE - totalGnreFecopSTSIE + gnreNPagaFecopSTSIE - TotalFecopNfeSTSIE + totalFecop1FreteSTIE + totalFecop2FreteSTIE;
                    ViewBag.TotalFecopDiefSTIE = Convert.ToDouble(Math.Round(totalfecopDiefSTIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFecopDiefSTSIE = Convert.ToDouble(Math.Round(totalfecopDiefSTSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal? icmsFecop1STIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2);
                    decimal? icmsFecop1STSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2);
                    decimal? icmsFecop2STIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2);
                    decimal? icmsFecop2STSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2);
                    ViewBag.IcmsFecopSTIE = Convert.ToDouble(icmsFecop1STIE + icmsFecop2STIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsFecopSTSIE = Convert.ToDouble(icmsFecop1STSIE + icmsFecop2STSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    ViewBag.TotalFinalFecopCalculadaSTIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalfecopDiefSTIE - (icmsFecop1STIE + icmsFecop2STIE)), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFinalFecopCalculadaSTSIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalfecopDiefSTSIE - (icmsFecop1STSIE + icmsFecop2STSIE)), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    //Incentivo
                    if (comp.Incentive == true && !comp.AnnexId.Equals(null))
                    {
                        //Produtos não incentivados
                        var productsNormal = _service.FindByNormal(notes);
                        productsNormal = productsNormal.Where(_ => _.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)).ToList();

                        totalIcmsFreteSTIE = 0;
                        totalFecop1FreteSTIE = 0;
                        totalFecop2FreteSTIE = 0;

                        foreach (var prod in productsNormal)
                        {
                            if (!prod.Note.Iest.Equals("") && (prod.TaxationTypeId.Equals(5) || prod.TaxationTypeId.Equals(6)) && prod.Incentivo.Equals(false))
                            {
                                if (Convert.ToDecimal(prod.Aliqinterna) > 0)
                                {
                                    decimal valorAgreg = 0;
                                    if (prod.Mva != null)
                                    {
                                        valorAgreg = calculation.ValorAgregadoMva(prod.Freterateado, Convert.ToDecimal(prod.Mva));
                                    }
                                    if (prod.BCR != null)
                                    {
                                        valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(prod.BCR), valorAgreg);
                                    }
                                    if (prod.Fecop != null)
                                    {
                                        if (Convert.ToDecimal(prod.Fecop).Equals(1))
                                        {
                                            totalFecop1FreteSTIE += calculation.valorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                        }
                                        else
                                        {
                                            totalFecop2FreteSTIE += calculation.valorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                        }

                                    }

                                    totalIcmsFreteSTIE += calculation.valorAgregadoAliqInt(Convert.ToDecimal(prod.Aliqinterna), Convert.ToDecimal(prod.Fecop), valorAgreg) - prod.IcmsCTe;
                                }
                            }
                        }

                        ViewBag.TotalIcmsFreteSTIE = Convert.ToDouble(totalIcmsFreteSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalFecopFreteSTIE = Convert.ToDouble(totalFecop1FreteSTIE + totalFecop2FreteSTIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                        gnrePagaSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreSt).Distinct().Sum()), 2);
                        gnreNPagaSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreNSt).Distinct().Sum()), 2);
                        gnrePagaSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreSt).Distinct().Sum()), 2);
                        gnreNPagaSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreNSt).Distinct().Sum()), 2);

                        icmsStSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.IcmsST).Sum()), 2);
                        icmsStSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.IcmsST).Sum()), 2);
                        totalApuradoSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.TotalICMS).Sum()), 2);
                        totalApuradoSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.TotalICMS).Sum()), 2);
                        totalDiefSTSIE = (totalApuradoSTSIE + totalIcmsFreteSTIE) - icmsStSTSIE + gnreNPagaSTSIE - gnrePagaSTSIE;
                        totalDiefSTIE = totalApuradoSTIE - icmsStSTIE + gnreNPagaSTIE - gnrePagaSTIE - totalIcmsFreteSTIE;

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

                        totalfecopDiefSTIE = TotalFecopCalcSTIE - totalGnreFecopSTIE +  gnreNPagaFecopSTIE - TotalFecopNfeSTIE - totalFecop1FreteSTIE - totalFecop2FreteSTIE;
                        totalfecopDiefSTSIE = TotalFecopCalcSTSIE - totalGnreFecopSTSIE + gnreNPagaFecopSTSIE - TotalFecopNfeSTSIE + totalFecop1FreteSTIE + totalFecop2FreteSTIE;
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

                        ViewBag.Icms = comp.Icms;
                        ViewBag.Fecop = comp.Fecop;

                        ///decimal baseIcms = productsIncentive.Select(_ => _.Vbasecalc).Sum();
                        decimal baseIcms = Convert.ToDecimal(productsIncentive.Select(_ => _.Vprod).Sum() + productsIncentive.Select(_ => _.Voutro).Sum() +
                        productsIncentive.Select(_ => _.Vseg).Sum() - productsIncentive.Select(_ => _.Vdesc).Sum() + productsIncentive.Select(_ => _.Vfrete).Sum() +
                        productsIncentive.Select(_ => _.Freterateado).Sum() + productsIncentive.Select(_ => _.Vipi).Sum());

                        ViewBag.Base = Convert.ToDouble(Math.Round(baseIcms, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        decimal impostoIcms = Convert.ToDecimal(baseIcms * (Convert.ToDecimal(comp.Icms) / 100));
                        ViewBag.ImpostoIcms = Convert.ToDouble(Math.Round(impostoIcms, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        decimal impostoFecop = Convert.ToDecimal(baseIcms * (Convert.ToDecimal(comp.Fecop) / 100));
                        ViewBag.ImpostoFecop = Convert.ToDouble(Math.Round(impostoFecop, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                
                        //ecimal? impostoGeral = 0;

                        //impostoGeral = products.Where(_ => _.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)).Select(_ => _.TotalICMS).Sum() + products.Where(_ => _.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)).Select(_ => _.TotalFecop).Sum();

                        decimal icmsGeralNormal = IcmsAPagarSTSIE + IcmsAPagarSTIE;
                        decimal icmsGeralIncetivo = Convert.ToDecimal(products.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Incentivo.Equals(true)).Select(_ => _.TotalICMS).Sum());
                        decimal fecopGeralNomal = Convert.ToDecimal(totalfecopDiefSTIE - (icmsFecop1STIE + icmsFecop2STIE)) + Convert.ToDecimal(totalfecopDiefSTSIE - (icmsFecop1STSIE + icmsFecop2STSIE));
                        decimal fecopGeralIncentivo = Convert.ToDecimal(products.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Incentivo.Equals(true)).Select(_ => _.TotalFecop).Sum());
                        decimal impostoGeral = icmsGeralNormal + icmsGeralIncetivo + fecopGeralNomal + fecopGeralIncentivo;

                        ViewBag.IcmsNormal = Convert.ToDouble(Math.Round(icmsGeralNormal, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.FecopNormal = Convert.ToDouble(Math.Round(fecopGeralNomal, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IcmsIncentivo = Convert.ToDouble(Math.Round(icmsGeralIncetivo, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.FecopIncentivo = Convert.ToDouble(Math.Round(fecopGeralIncentivo, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        ViewBag.ImpostoGeral = Convert.ToDouble(Math.Round(Convert.ToDecimal(impostoGeral), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        decimal? basefunef = impostoGeral - impostoIcms;
                        ViewBag.BaseFunef = Convert.ToDouble(Math.Round(Convert.ToDecimal(basefunef), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.Funef = comp.Funef;
                        decimal taxaFunef = Convert.ToDecimal(basefunef * (Convert.ToDecimal(comp.Funef) / 100));
                        ViewBag.TaxaFunef = Convert.ToDouble(Math.Round(taxaFunef, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                       

                        totalDarIcms += impostoIcms;
                        totalDarFecop += impostoFecop;
                        totalDarFunef += taxaFunef;

                        // Icms Excedente
                        List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                        List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();

                        /*List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                        List<List<Dictionary<string, string>>> notesTranferencia = new List<List<Dictionary<string, string>>>();
                        List<List<Dictionary<string, string>>> notesEntradaDevolucao = new List<List<Dictionary<string, string>>>();
                        List<List<Dictionary<string, string>>> notesSaidaDevolucao = new List<List<Dictionary<string, string>>>();
                        List<List<Dictionary<string, string>>> notesTransferenciaEntrada = new List<List<Dictionary<string, string>>>();
                        List<List<Dictionary<string, string>>> notesDevoSaida = new List<List<Dictionary<string, string>>>();
                        List<List<Dictionary<string, string>>> notesDevoEntrada = new List<List<Dictionary<string, string>>>();*/

                        exitNotes = import.Nfe(directoryNfeExit);
                        entryNotes = import.Nfe(directoryNfeEntrada);

                        var ncms = _ncmConvenioService.FindByAnnex(Convert.ToInt32(comp.AnnexId));
                        var clientesAll = _clientService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).Select(_ => _.Document).ToList();
                        var suspensions = _suspensionService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).ToList();

                        /*notesDevoEntrada = import.NfeExit(directoryNfeEntrada, id, type, "devo");
                        notesTransferenciaEntrada = import.NotesTransfer(directoryNfeEntrada, id);
                        notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                        notesTranferencia = import.NfeExit(directoryNfeExit, id, type, "transferencia");
                        notesTransferenciaEntrada = import.NotesTransfer(directoryNfeEntrada, id);
                        notesDevoSaida = import.NfeExit(directoryNfeExit, id, type, "devo");
                        notesDevoEntrada = import.NfeExit(directoryNfeEntrada, id, type, "devo");*/


                        var contribuintes = _clientService.FindByContribuinte(id, "all");
                        var contribuintesRaiz = _clientService.FindByContribuinte(id, "raiz");

                        var cfopsVenda = _companyCfopService.FindByCfopActive(id, "venda", "venda").Select(_ => _.Cfop.Code);
                        var cfopsTransf = _companyCfopService.FindByCfopActive(id, "venda", "transferencia").Select(_ => _.Cfop.Code);
                        var cfopsDevo = _companyCfopService.FindByCfopActive(id, "venda", "devo").Select(_ => _.Cfop.Code);

                        decimal totalVendas = 0, totalNcm = 0, totalTranferencias = 0, totalSaida = 0, totalDevo = 0,
                            totalDevoAnexo = 0, totalDevoContribuinte = 0, totalVendasSuspensao = 0;
                        int contContribuintes = contribuintes.Count();
                        int contContribuintesRaiz = contribuintesRaiz.Count() + 1;

                        string[,] resumoCnpjs = new string[contContribuintes, 2];
                        string[,] resumoCnpjRaiz = new string[contContribuintesRaiz, 2];
                        string[,] resumoAllCnpjRaiz = new string[contContribuintesRaiz - 1, 3];

                        for (int i = 0; i < contContribuintes; i++)
                        {
                            resumoCnpjs[i, 0] = contribuintes[i];
                            resumoCnpjs[i, 1] = "0";
                        }

                        for (int i = 0; i < contContribuintesRaiz; i++)
                        {
                            if (i < contContribuintesRaiz - 1)
                            {
                                resumoCnpjRaiz[i, 0] = contribuintesRaiz[i];
                                resumoAllCnpjRaiz[i, 0] = contribuintesRaiz[i];
                                resumoCnpjRaiz[i, 1] = "0";
                                resumoAllCnpjRaiz[i, 1] = "0";
                                resumoAllCnpjRaiz[i, 2] = "0";
                            }
                            else
                            {
                                resumoCnpjRaiz[i, 0] = "Não contribuinte";
                                resumoCnpjRaiz[i, 1] = "0";
                            }
                        }

                        decimal totalEntradas = 0, totalTranferenciaInter = 0;

                        // Transferência Entrada
                        for (int i = entryNotes.Count - 1; i >= 0; i--)
                        {
                            if (!entryNotes[i][3]["CNPJ"].Equals(comp.Document))
                            {
                                entryNotes.RemoveAt(i);
                                continue;
                            }

                            bool cfop = false;

                            for (int j = 0; j < entryNotes[i].Count; j++)
                            {
                                if (entryNotes[i][j].ContainsKey("CFOP"))
                                {
                                    cfop = false;
                                    if (cfopsTransf.Contains(entryNotes[i][j]["CFOP"]))
                                    {
                                        cfop = true;
                                    }

                                }
                                if (cfop == true)
                                {
                                    if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vProd"))
                                    {
                                        if (entryNotes[i][1]["idDest"].Equals("2"))
                                        {
                                            totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                        }
                                        totalEntradas += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                    }
                                    if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vFrete"))
                                    {
                                        if (entryNotes[i][1]["idDest"].Equals("2"))
                                        {
                                            totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                        }
                                        totalEntradas += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                    }
                                    if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vDesc"))
                                    {
                                        if (entryNotes[i][1]["idDest"].Equals("2"))
                                        {
                                            totalTranferenciaInter -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                        }
                                        totalEntradas -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                    }
                                    if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vOutro"))
                                    {
                                        if (entryNotes[i][1]["idDest"].Equals("2"))
                                        {
                                            totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                        }
                                        totalEntradas += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                    }
                                    if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vSeg"))
                                    {
                                        if (entryNotes[i][1]["idDest"].Equals("2"))
                                        {
                                            totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                        }
                                        totalEntradas += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                    }
                                }
                            }

                        }

                        // Vendas 
                        for (int i = exitNotes.Count - 1; i >= 0; i--)
                        {
                            if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                            {
                                exitNotes.RemoveAt(i);
                                continue;
                            }

                            int posClienteRaiz = contContribuintesRaiz - 1, posCliente = -1;

                            bool status = false, suspenso = false, cfop = false;

                            if (exitNotes[i][1].ContainsKey("dhEmi"))
                            {
                                foreach (var suspension in suspensions)
                                {
                                    if (Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) >= Convert.ToDateTime(suspension.DateStart) && Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) < Convert.ToDateTime(suspension.DateEnd))
                                    {
                                        suspenso = true;
                                        break;
                                    }
                                }
                            }

                            if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("IE") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][1]["mod"].Equals("55"))
                            {
                                string CNPJ = exitNotes[i][3].ContainsKey("CNPJ") ? exitNotes[i][3]["CNPJ"] : "escon";
                                string indIEDest = exitNotes[i][3].ContainsKey("indIEDest") ? exitNotes[i][3]["indIEDest"] : "escon";
                                string IE = exitNotes[i][3].ContainsKey("IE") ? exitNotes[i][3]["IE"] : "escon";

                                if (contribuintesRaiz.Contains(exitNotes[i][3]["CNPJ"].Substring(0, 8)))
                                {
                                    posClienteRaiz = contribuintesRaiz.IndexOf(exitNotes[i][3]["CNPJ"].Substring(0, 8));
                                }

                                if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                {
                                    posCliente = contribuintes.IndexOf(exitNotes[i][3]["CNPJ"]);
                                }

                                bool existe = false;

                                if (clientesAll.Contains(exitNotes[i][3]["CNPJ"]))
                                {
                                    existe = true;
                                }

                                if (existe == false)
                                {
                                    throw new Exception("Há Clientes não Importados");
                                }
                            }

                            for (int k = 0; k < exitNotes[i].Count(); k++)
                            {

                                if (exitNotes[i][k].ContainsKey("NCM"))
                                {
                                    status = false;

                                    for (int j = 0; j < ncms.Count(); j++)
                                    {
                                        int tamanho = ncms[j].Length;

                                        if (ncms[j].Equals(exitNotes[i][k]["NCM"].Substring(0, tamanho)))
                                        {
                                            status = true;
                                            break;
                                        }
                                    }
                                }

                                if (exitNotes[i][k].ContainsKey("CFOP"))
                                {
                                    cfop = false;
                                    if (cfopsVenda.Contains(exitNotes[i][k]["CFOP"]))
                                    {
                                        cfop = true;
                                    }

                                }

                                if (status == true && cfop == true)
                                {

                                    if (exitNotes[i][k].ContainsKey("vProd") && exitNotes[i][k].ContainsKey("cProd"))
                                    {
                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                        totalNcm += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                        resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                        }
                                        if (posCliente >= 0)
                                        {
                                            resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                        }

                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                        }
                                    }

                                    if (exitNotes[i][k].ContainsKey("vFrete") && exitNotes[i][k].ContainsKey("cProd"))
                                    {
                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                        totalNcm += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                        resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                        }
                                        if (posCliente >= 0)
                                        {
                                            resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                        }
                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                        }
                                    }

                                    if (exitNotes[i][k].ContainsKey("vDesc") && exitNotes[i][k].ContainsKey("cProd"))
                                    {
                                        totalVendas -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                        totalNcm -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                        resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                        }
                                        if (posCliente >= 0)
                                        {
                                            resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                        }
                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                        }
                                    }

                                    if (exitNotes[i][k].ContainsKey("vOutro") && exitNotes[i][k].ContainsKey("cProd"))
                                    {
                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                        totalNcm += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                        resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                        }
                                        if (posCliente >= 0)
                                        {
                                            resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                        }
                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                        }
                                    }

                                    if (exitNotes[i][k].ContainsKey("vSeg") && exitNotes[i][k].ContainsKey("cProd"))
                                    {
                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                        totalNcm += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                        resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                        }
                                        if (posCliente >= 0)
                                        {
                                            resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();

                                        }
                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                        }
                                    }
                                }
                                else if (status == false && cfop == true)
                                {

                                    if (exitNotes[i][k].ContainsKey("vProd") && exitNotes[i][k].ContainsKey("cProd"))
                                    {
                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                        resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                        }
                                        if (posCliente >= 0)
                                        {
                                            resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                        }
                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                        }
                                    }

                                    if (exitNotes[i][k].ContainsKey("vFrete") && exitNotes[i][k].ContainsKey("cProd"))
                                    {
                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                        resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                        }
                                        if (posCliente >= 0)
                                        {
                                            resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                        }
                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                        }
                                    }

                                    if (exitNotes[i][k].ContainsKey("vDesc") && exitNotes[i][k].ContainsKey("cProd"))
                                    {
                                        totalVendas -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                        resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                        }
                                        if (posCliente >= 0)
                                        {
                                            resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                        }
                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                        }
                                    }

                                    if (exitNotes[i][k].ContainsKey("vOutro") && exitNotes[i][k].ContainsKey("cProd"))
                                    {
                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                        resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                        }
                                        if (posCliente >= 0)
                                        {
                                            resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                        }
                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                        }
                                    }

                                    if (exitNotes[i][k].ContainsKey("vSeg") && exitNotes[i][k].ContainsKey("cProd"))
                                    {
                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                        resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                        }
                                        if (posCliente >= 0)
                                        {
                                            resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();

                                        }
                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                        }
                                    }
                                }

                            }

                        }

                        // Transferência Saida
                        for (int i = exitNotes.Count - 1; i >= 0; i--)
                        {

                            if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                            {
                                exitNotes.RemoveAt(i);
                                continue;
                            }

                            bool suspenso = false;

                            if (exitNotes[i][1].ContainsKey("dhEmi"))
                            {
                                foreach (var suspension in suspensions)
                                {
                                    if (Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) >= Convert.ToDateTime(suspension.DateStart) && Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) < Convert.ToDateTime(suspension.DateEnd))
                                    {
                                        suspenso = true;
                                        break;
                                    }
                                }
                            }

                            int posClienteRaiz = contContribuintesRaiz - 1;

                            if (exitNotes[i][3].ContainsKey("CNPJ"))
                            {
                                if (contribuintesRaiz.Contains(exitNotes[i][3]["CNPJ"].Substring(0, 8)))
                                {
                                    posClienteRaiz = contribuintesRaiz.IndexOf(exitNotes[i][3]["CNPJ"].Substring(0, 8));
                                }
                            }

                            bool cfop = false;
                            for (int j = 0; j < exitNotes[i].Count; j++)
                            {
                                if (exitNotes[i][j].ContainsKey("CFOP"))
                                {
                                    cfop = false;
                                    if (cfopsTransf.Contains(exitNotes[i][j]["CFOP"]))
                                    {
                                        cfop = true;
                                    }

                                }

                                if (cfop == true)
                                {
                                    if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                        }
                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                        }
                                    }

                                    if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();

                                        }
                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                        }
                                    }

                                    if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalTranferencias -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();

                                        }
                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                        }
                                    }

                                    if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();

                                        }
                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                        }
                                    }

                                    if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();

                                        }
                                        if (suspenso == true)
                                        {
                                            totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                        }
                                    }
                                }

                            }

                        }

                        // Devolução Saida
                        for (int i = exitNotes.Count - 1; i >= 0; i--)
                        {
                            if (exitNotes[i][3].ContainsKey("CNPJ"))
                            {
                                if (!exitNotes[i][3]["CNPJ"].Equals(comp.Document))
                                {
                                    exitNotes.RemoveAt(i);
                                    continue;
                                }
                            }
                            else
                            {
                                exitNotes.RemoveAt(i);
                                continue;
                            }

                            if (exitNotes[i][1]["finNFe"] != "4")
                            {
                                exitNotes.RemoveAt(i);
                                continue;
                            }

                            bool existe = false, status = false, cfop = false;

                            if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("IE") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][1]["mod"].Equals("55"))
                            {

                                if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                {
                                    existe = true;
                                }
                            }

                            for (int j = 0; j < exitNotes[i].Count(); j++)
                            {
                                if (exitNotes[i][j].ContainsKey("NCM"))
                                {
                                    status = false;

                                    for (int k = 0; k < ncms.Count(); k++)
                                    {
                                        int tamanho = ncms[k].Length;

                                        if (ncms[k].Equals(exitNotes[i][j]["NCM"].Substring(0, tamanho)))
                                        {
                                            status = true;
                                            break;
                                        }
                                    }
                                }

                                if (exitNotes[i][j].ContainsKey("CFOP"))
                                {
                                    cfop = false;
                                    if (cfopsDevo.Contains(exitNotes[i][j]["CFOP"]))
                                    {
                                        cfop = true;
                                    }

                                }

                                if (status == true && cfop == true)
                                {
                                    if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                        totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                        }
                                    }

                                    if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                        totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                        }
                                    }

                                    if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                        totalDevoAnexo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                        }
                                    }

                                    if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                        totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                        }
                                    }

                                    if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                        totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                        }
                                    }
                                }
                                else if (status == false && cfop == true)
                                {
                                    if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                        }

                                    }

                                    if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                    {

                                        totalDevo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                        }
                                    }

                                    if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                        }
                                    }

                                    if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                        }
                                    }

                                    if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                        }
                                    }
                                }

                            }

                        }

                        // Devolução Entrada
                        for (int i = entryNotes.Count - 1; i >= 0; i--)
                        {
                            if (entryNotes[i][3].ContainsKey("CNPJ"))
                            {
                                if (!entryNotes[i][3]["CNPJ"].Equals(comp.Document))
                                {
                                    entryNotes.RemoveAt(i);
                                    continue;
                                }
                            }
                            else
                            {
                                entryNotes.RemoveAt(i);
                                continue;
                            }

                            if (entryNotes[i][1]["finNFe"] != "4")
                            {
                                entryNotes.RemoveAt(i);
                                continue;
                            }

                            bool existe = false;
                            int posClienteRaiz = contContribuintesRaiz - 1;

                            if (entryNotes[i][3].ContainsKey("CNPJ") && entryNotes[i][3].ContainsKey("IE") && entryNotes[i][3].ContainsKey("indIEDest") && entryNotes[i][1]["mod"].Equals("55"))
                            {

                                if (contribuintes.Contains(entryNotes[i][3]["CNPJ"]))
                                {
                                    existe = true;
                                }

                                string CNPJ = entryNotes[i][3].ContainsKey("CNPJ") ? entryNotes[i][3]["CNPJ"] : "escon";
                                string indIEDest = entryNotes[i][3].ContainsKey("indIEDest") ? entryNotes[i][3]["indIEDest"] : "escon";
                                string IE = entryNotes[i][3].ContainsKey("IE") ? entryNotes[i][3]["IE"] : "escon";

                                if (contribuintesRaiz.Contains(entryNotes[i][3]["CNPJ"].Substring(0, 8)))
                                {
                                    posClienteRaiz = contribuintesRaiz.IndexOf(entryNotes[i][3]["CNPJ"].Substring(0, 8));
                                }
                            }

                            bool status = false, cfop = false;

                            for (int j = 0; j < entryNotes[i].Count(); j++)
                            {

                                if (entryNotes[i][j].ContainsKey("NCM"))
                                {
                                    status = false;

                                    for (int k = 0; k < ncms.Count(); k++)
                                    {
                                        int tamanho = ncms[k].Length;

                                        if (ncms[k].Equals(entryNotes[i][j]["NCM"].Substring(0, tamanho)))
                                        {
                                            status = true;
                                            break;
                                        }
                                    }
                                }

                                if (entryNotes[i][j].ContainsKey("CFOP"))
                                {
                                    cfop = false;
                                    if (cfopsDevo.Contains(entryNotes[i][j]["CFOP"]))
                                    {
                                        cfop = true;
                                    }

                                }

                                if (status == true && cfop == true)
                                {
                                    if (entryNotes[i][j].ContainsKey("vProd") && entryNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                        totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                        }
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vProd"])).ToString();
                                        }
                                    }

                                    if (entryNotes[i][j].ContainsKey("vFrete") && entryNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                        totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                        }
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vFrete"])).ToString();
                                        }
                                    }

                                    if (entryNotes[i][j].ContainsKey("vDesc") && entryNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                        totalDevoAnexo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                        }
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(entryNotes[i][j]["vDesc"])).ToString();
                                        }
                                    }

                                    if (entryNotes[i][j].ContainsKey("vOutro") && entryNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                        totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                        }

                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vOutro"])).ToString();
                                        }
                                    }

                                    if (entryNotes[i][j].ContainsKey("vSeg") && entryNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                        totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                        }
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vSeg"])).ToString();
                                        }
                                    }
                                }
                                else if (status == false && cfop == true)
                                {
                                    if (entryNotes[i][j].ContainsKey("vProd") && entryNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                        }

                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vProd"])).ToString();
                                        }

                                    }

                                    if (entryNotes[i][j].ContainsKey("vFrete") && entryNotes[i][j].ContainsKey("cProd"))
                                    {

                                        totalDevo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                        }

                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vFrete"])).ToString();
                                        }

                                    }

                                    if (entryNotes[i][j].ContainsKey("vDesc") && entryNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                        }

                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(entryNotes[i][j]["vDesc"])).ToString();
                                        }
                                    }

                                    if (entryNotes[i][j].ContainsKey("vOutro") && entryNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                        }
                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vOutro"])).ToString();
                                        }
                                    }

                                    if (entryNotes[i][j].ContainsKey("vSeg") && entryNotes[i][j].ContainsKey("cProd"))
                                    {
                                        totalDevo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                        if (existe == true)
                                        {
                                            totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                        }

                                        if (posClienteRaiz < contContribuintesRaiz - 1)
                                        {
                                            resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vSeg"])).ToString();
                                        }
                                    }
                                }
                            }

                        }

                        decimal totalNcontribuinte = Convert.ToDecimal(resumoCnpjRaiz[contContribuintesRaiz - 1, 1]), baseCalc = totalVendas - totalDevo;
                        decimal totalContribuinte = totalVendas - totalNcontribuinte;
                        decimal baseCalcContribuinte = totalContribuinte - totalDevoContribuinte;
                        decimal totalDevoNContribuinte = totalDevo - totalDevoContribuinte;
                        decimal baseCalcNContribuinte = totalNcontribuinte - totalDevoNContribuinte;

                        totalSaida = baseCalc + totalTranferencias;
                        totalVendas = totalVendas + totalTranferencias;
                        decimal limiteNContribuinte = (baseCalc * (Convert.ToDecimal(comp.VendaCpf))) / 100,
                            limiteNcm = (baseCalc * Convert.ToDecimal(comp.VendaAnexo)) / 100,
                            limiteGrupo = (totalSaida * Convert.ToDecimal(comp.VendaMGrupo)) / 100,
                            limiteTransferencia = (totalEntradas * Convert.ToDecimal(comp.TransferenciaInterExcedente)) / 100;

                        decimal impostoNContribuinte = 0, excedenteNContribuinte = 0, excedenteNcm = 0, impostoNcm = 0, excedenteTranfInter = 0, impostoTransfInter = 0;


                        //CNPJ
                        List<List<string>> gruposExecentes = new List<List<string>>();
                        decimal totalVendaGrupo = 0, totalExcedente = 0, totalDevoGrupo = 0, totalImpostoGrupo = 0;
                        for (int i = 0; i < contContribuintesRaiz - 1; i++)
                        {
                            var vendaGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 1]);
                            var devoGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 2]);
                            var baseCalcGrupo = (vendaGrupo - devoGrupo);
                            if (baseCalcGrupo > limiteGrupo)
                            {
                                List<string> grupoExcedente = new List<string>();
                                var cnpjGrupo = resumoAllCnpjRaiz[i, 0];
                                var clientGrupo = _clientService.FindByRaiz(cnpjGrupo);
                                var nomeGrupo = clientGrupo.Name;
                                var percentGrupo = Math.Round((baseCalcGrupo / totalSaida) * 100, 2);
                                var excedenteGrupo = baseCalcGrupo - limiteGrupo;
                                var impostoGrupo = (excedenteGrupo * Convert.ToDecimal(comp.VendaMGrupoExcedente)) / 100;
                                totalVendaGrupo += vendaGrupo;
                                totalDevoGrupo += devoGrupo;
                                totalExcedente += baseCalcGrupo;
                                totalImpostoGrupo += impostoGrupo;
                                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");
                                grupoExcedente.Add(cnpjGrupo);
                                grupoExcedente.Add(nomeGrupo);
                                grupoExcedente.Add(percentGrupo.ToString());
                                grupoExcedente.Add(Math.Round(Convert.ToDouble(vendaGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                                grupoExcedente.Add(Math.Round(Convert.ToDouble(limiteGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                                grupoExcedente.Add(Math.Round(Convert.ToDouble(excedenteGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                                grupoExcedente.Add(comp.VendaMGrupoExcedente.ToString());
                                grupoExcedente.Add(Math.Round(Convert.ToDouble(impostoGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                                grupoExcedente.Add(Math.Round(Convert.ToDouble(baseCalcGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                                grupoExcedente.Add(Math.Round(Convert.ToDouble(devoGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());

                                gruposExecentes.Add(grupoExcedente);
                                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                            }
                        }

                        decimal baseCalcNcm = totalNcm - totalDevoAnexo;

                        //Anexo II
                        if (baseCalcNcm < limiteNcm)
                        {
                            excedenteNcm = limiteNcm - baseCalcNcm;
                            impostoNcm = (excedenteNcm * Convert.ToDecimal(comp.VendaAnexoExcedente)) / 100;
                        }

                        //Não Contribuinte
                        if (baseCalcNContribuinte > limiteNContribuinte && limiteNContribuinte > 0)
                        {
                            excedenteNContribuinte = baseCalcNContribuinte - limiteNContribuinte;
                            impostoNContribuinte = (excedenteNContribuinte * Convert.ToDecimal(comp.VendaCpfExcedente)) / 100;
                        }

                        // Transferência inter
                        if (limiteTransferencia < totalTranferenciaInter)
                        {
                            excedenteTranfInter = totalTranferenciaInter - limiteTransferencia;
                            impostoTransfInter = (excedenteTranfInter * Convert.ToDecimal(comp.TransferenciaInterExcedente)) / 100;
                        }

                        // Suspensão
                        decimal valorSuspensao = (totalVendasSuspensao * Convert.ToDecimal(comp.Suspension)) / 100;

                        // Percentuais
                        decimal percentualVendaContribuinte = (baseCalcContribuinte * 100) / baseCalc;
                        decimal percentualVendaNContribuinte = (baseCalcNContribuinte * 100) / baseCalc;
                        decimal percentualVendaAnexo = (baseCalcNcm * 100) / baseCalc;
                        decimal percentualGrupo = (totalExcedente * 100) / baseCalc;

                        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                        //Geral
                        ViewBag.Contribuinte = Math.Round(Convert.ToDouble(totalContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.NContribuinte = Math.Round(Convert.ToDouble(totalNcontribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalVenda = Math.Round(Convert.ToDouble(totalVendas.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.VendaAnexo = Math.Round(Convert.ToDouble(totalNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalTransferencia = Math.Round(Convert.ToDouble(totalTranferenciaInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalDevo = Math.Round(Convert.ToDouble(totalDevo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalc = Math.Round(Convert.ToDouble(baseCalc.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalDevoAnexo = Math.Round(Convert.ToDouble(totalDevoAnexo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcAnexo = Math.Round(Convert.ToDouble(baseCalcNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalDevoContrib = Math.Round(Convert.ToDouble(totalDevoContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcContrib = Math.Round(Convert.ToDouble(baseCalcContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalDevoNContrib = Math.Round(Convert.ToDouble(totalDevoNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcNContrib = Math.Round(Convert.ToDouble(baseCalcNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        // Percentuais
                        ViewBag.PercentualVendaContribuinte = Math.Round(Convert.ToDouble(percentualVendaContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PercentualVendaNContribuinte = Math.Round(Convert.ToDouble(percentualVendaNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PercentualVendaAnexo = Math.Round(Convert.ToDouble(percentualVendaAnexo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PercentualVendaGrupo = Math.Round(Convert.ToDouble(percentualGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //CNPJ
                        ViewBag.PercentualCNPJ = comp.VendaMGrupo;
                        ViewBag.TotalVendaGrupo = Math.Round(Convert.ToDouble(totalVendaGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalDevoGrupo = Math.Round(Convert.ToDouble(totalDevoGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalExecedente = Math.Round(Convert.ToDouble(totalExcedente.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.Grupo = gruposExecentes;

                        //Anexo II
                        ViewBag.LimiteAnexo = Math.Round(Convert.ToDouble(limiteNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ExcedenteAnexo = Math.Round(Convert.ToDouble(excedenteNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PercentualExcedenteAnexo = comp.VendaAnexoExcedente;
                        ViewBag.TotalExcedenteAnexo = Math.Round(Convert.ToDouble(impostoNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //Não Contribuinte
                        ViewBag.LimiteNContribuinte = Math.Round(Convert.ToDouble(limiteNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ExcedenteNContribuinte = Math.Round(Convert.ToDouble(excedenteNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PercentualExcedenteNContribuinte = comp.VendaCpfExcedente;
                        ViewBag.TotalExcedenteNContribuinte = Math.Round(Convert.ToDouble(impostoNContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //Tranferência
                        ViewBag.LimiteTransferencia = Math.Round(Convert.ToDouble(limiteTransferencia.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ExcedenteTransferencia = Math.Round(Convert.ToDouble(excedenteTranfInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PercentaulTransferencia = comp.TransferenciaInterExcedente;
                        ViewBag.TotalExcedenteTransferencia = Math.Round(Convert.ToDouble(impostoTransfInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        // Suspensão
                        ViewBag.BaseCalcSuspensao = Math.Round(Convert.ToDouble(totalVendasSuspensao.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PercentaulSuspensao = comp.Suspension;
                        ViewBag.TotalSuspensao = Math.Round(Convert.ToDouble(valorSuspensao.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                        //Total Icms
                        ViewBag.TotalIcms = Math.Round(Convert.ToDouble((impostoNcm + impostoNContribuinte + impostoTransfInter + totalImpostoGrupo + valorSuspensao).ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        totalDarIcms += (impostoNcm + impostoNContribuinte + impostoTransfInter + totalImpostoGrupo + valorSuspensao);
                    }
                    else if (comp.Incentive == true && comp.AnnexId.Equals(null))
                    {
                        if (arquivo == null || arquivo.Length == 0)
                        {
                            ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                            return View(ViewData);
                        }

                        string nomeArquivo = comp.Document + year + month;

                        if (arquivo.FileName.Contains(".txt"))
                            nomeArquivo += ".txt";
                        else
                            nomeArquivo += ".tmp";

                        string caminho_WebRoot = _appEnvironment.WebRootPath;
                        string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Speds\\";
                        string caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                        string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivo);
                        if (System.IO.File.Exists(caminhoDestinoArquivoOriginal))
                        {
                            System.IO.File.Delete(caminhoDestinoArquivoOriginal);
                        }
                        var stream = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);
                        await arquivo.CopyToAsync(stream);
                        stream.Close();

                        List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                        List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();

                        if (comp.TypeCompany.Equals(true))
                        {
                            var productincentivo = _productIncentivoService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.CompanyId.Equals(id)).ToList();

                            var codeProdIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Code).ToList();
                            var codeProdST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Code).ToList();
                            var codeProdIsento = productincentivo.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Code).ToList();

                            var cestIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Cest).ToList();
                            var cestST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Cest).ToList();
                            var cestIsento = productincentivo.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Cest).ToList();

                            decimal creditosIcms = import.SpedCredito(caminhoDestinoArquivoOriginal, comp.Id),
                                 debitosIcms = 0;

                            /*List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                            List<List<Dictionary<string, string>>> notesVendaSt = new List<List<Dictionary<string, string>>>();
                            List<List<Dictionary<string, string>>> notesSaidaDevoCompra = new List<List<Dictionary<string, string>>>();*/

                            List<List<string>> icmsForaDoEstado = new List<List<string>>();

                            var contribuintes = _clientService.FindByContribuinte(id, "all");
                            var clientesAll = _clientService.FindAll(null).Where(_ => _.CompanyId.Equals(id)).Select(_ => _.Document).ToList();

                            exitNotes = import.Nfe(directoryNfeExit);

                            /*notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                            notesVendaSt = import.NfeExit(directoryNfeExit, id, type, "vendaSt");
                            notesSaidaDevoCompra = import.NfeExit(directoryNfeExit, id, type, "devolucao de compra");*/

                            var cfopsDevoCompra = _companyCfopService.FindByCfopActive(id, "incentivo", "devolucao de compra").Select(_ => _.Cfop.Code);
                            var cfopsVendaST = _companyCfopService.FindByCfopActive(id, "incentivo", "vendaSt").Select(_ => _.Cfop.Code);
                            var cfopsVenda = _companyCfopService.FindByCfopActive(id, "incentivo", "venda").Select(_ => _.Cfop.Code);
                            var cfospDevoVenda = _companyCfopService.FindByCfopActive(id, "entrada", "devolução de venda").Select(_ => _.Cfop.Code);

                            decimal totalVendas = 0, naoContribuinteIncentivo = 0, naoContribuinteNIncetivo = 0, vendaCfopSTNaoContribuinteNIncetivo = 0, NaoContribuiteIsento = 0,
                                naoContriForaDoEstadoIncentivo = 0, naoContriForaDoEstadoNIncentivo = 0, vendaCfopSTNaoContriForaDoEstadoNIncentivo = 0, NaoContribuinteForaDoEstadoIsento = 0,
                                ContribuintesNIncentivo = 0, ContribuintesIncentivoAliqM25 = 0, ContribuintesIncentivo = 0, vendaCfopSTContribuintesNIncentivo = 0,
                                ContribuinteIsento = 0;

                            // Vendas
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                                {
                                    exitNotes.RemoveAt(i);
                                    continue;
                                }

                                int posCliente = -1;

                                if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][3].ContainsKey("IE"))
                                {
                                    string CNPJ = exitNotes[i][3]["CNPJ"];
                                    string indIEDest = exitNotes[i][3]["indIEDest"];
                                    string IE = exitNotes[i][3]["IE"];
                                    if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                    {
                                        posCliente = contribuintes.IndexOf(exitNotes[i][3]["CNPJ"]);
                                    }

                                    bool existe = false;
                                    if (clientesAll.Contains(exitNotes[i][3]["CNPJ"]))
                                    {
                                        existe = true;
                                    }

                                    if (existe == false)
                                    {
                                        throw new Exception("Há Clientes não Importados");
                                    }
                                }

                                int posUf = -1;
                                if (exitNotes[i][3].ContainsKey("UF") && exitNotes[i][1]["idDest"].Equals("2"))
                                {

                                    for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                    {
                                        if (icmsForaDoEstado[j][0].Equals(exitNotes[i][3]["UF"]))
                                        {
                                            posUf = j;
                                        }
                                    }

                                    if (posUf < 0)
                                    {
                                        List<string> uf = new List<string>();
                                        uf.Add(exitNotes[i][3]["UF"]);
                                        uf.Add("0,00");
                                        uf.Add("0,00");
                                        icmsForaDoEstado.Add(uf);
                                    }

                                }

                                bool cfop = false;
                                decimal valorProduto = 0;
                                string cProd = null, cest = null;

                                for (int k = 0; k < exitNotes[i].Count(); k++)
                                {
                                    if (exitNotes[i][k].ContainsKey("cProd"))
                                    {
                                        cProd = exitNotes[i][k]["cProd"];
                                        cest = null;
                                        if (exitNotes[i][k].ContainsKey("CEST"))
                                        {
                                            cest = exitNotes[i][k]["CEST"];
                                            if (cest.Equals(""))
                                            {
                                                cest = null;
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("CFOP"))
                                        {
                                            cfop = false;
                                            if (cfopsVenda.Contains(exitNotes[i][k]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }

                                        if (cfop == true)
                                        {
                                            if (exitNotes[i][k].ContainsKey("vProd"))
                                            {
                                                valorProduto += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                            }

                                            if (exitNotes[i][k].ContainsKey("vFrete"))
                                            {
                                                valorProduto += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                            }

                                            if (exitNotes[i][k].ContainsKey("vDesc"))
                                            {
                                                valorProduto -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                            }

                                            if (exitNotes[i][k].ContainsKey("vOutro"))
                                            {
                                                valorProduto += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                            }

                                            if (exitNotes[i][k].ContainsKey("vSeg"))
                                            {
                                                valorProduto += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                            }
                                        }

                                    }

                                    if (exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                    {
                                        decimal aliquota = 0;
                                        if (exitNotes[i][k].ContainsKey("pICMS"))
                                        {
                                            aliquota = Convert.ToDecimal(exitNotes[i][k]["pICMS"]);
                                            debitosIcms += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }

                                        if (codeProdIncentivado.Contains(cProd) && cestIncentivado.Contains(cest))
                                        {

                                            if (posCliente < 0)
                                            {
                                                naoContribuinteIncentivo += Convert.ToDecimal(valorProduto);

                                                if (exitNotes[i][1]["idDest"].Equals("2"))
                                                {
                                                    naoContriForaDoEstadoIncentivo += Convert.ToDecimal(valorProduto);
                                                    icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(valorProduto)).ToString();
                                                }
                                            }
                                            else
                                            {
                                                if (aliquota <= 25)
                                                {
                                                    ContribuintesIncentivo += Convert.ToDecimal(valorProduto);
                                                }
                                                else
                                                {
                                                    ContribuintesIncentivoAliqM25 += Convert.ToDecimal(valorProduto);
                                                }

                                            }
                                            totalVendas += Convert.ToDecimal(valorProduto);

                                        }
                                        else if (codeProdST.Contains(cProd) && cestST.Contains(cest))
                                        {
                                            if (posCliente < 0)
                                            {
                                                naoContribuinteNIncetivo += Convert.ToDecimal(valorProduto);

                                                if (exitNotes[i][1]["idDest"].Equals("2"))
                                                {
                                                    naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(valorProduto);
                                                    icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(valorProduto)).ToString();
                                                }
                                            }
                                            else
                                            {
                                                ContribuintesNIncentivo += Convert.ToDecimal(valorProduto);
                                            }
                                            totalVendas += Convert.ToDecimal(valorProduto);

                                        }
                                        else if (codeProdIsento.Contains(cProd) && cestIsento.Contains(cest))
                                        {
                                            if (posCliente < 0)
                                            {
                                                NaoContribuiteIsento += Convert.ToDecimal(valorProduto);

                                                if (exitNotes[i][1]["idDest"].Equals("2"))
                                                {
                                                    NaoContribuinteForaDoEstadoIsento += Convert.ToDecimal(valorProduto);
                                                }
                                            }
                                            else
                                            {
                                                ContribuinteIsento += Convert.ToDecimal(valorProduto);
                                            }
                                            totalVendas += Convert.ToDecimal(valorProduto);
                                        }
                                        else
                                        {
                                            throw new Exception("Há Produtos não Tributado");
                                        }
                                        cest = null;
                                        cProd = null;
                                        valorProduto = 0;
                                        aliquota = 0;
                                    }

                                    if (exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                    {
                                        debitosIcms += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                    }
                                }

                            }

                            // Vendas ST
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                int posCliente = -1;

                                if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][3].ContainsKey("IE"))
                                {
                                    string CNPJ = exitNotes[i][3]["CNPJ"];
                                    string indIEDest = exitNotes[i][3]["indIEDest"];
                                    string IE = exitNotes[i][3]["IE"];
                                    if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                    {
                                        posCliente = contribuintes.IndexOf(exitNotes[i][3]["CNPJ"]);
                                    }

                                    bool existe = false;
                                    if (clientesAll.Contains(exitNotes[i][3]["CNPJ"]))
                                    {
                                        existe = true;
                                    }

                                    if (existe == false)
                                    {
                                        throw new Exception("Há Clientes não Importados");
                                    }
                                }

                                int posUf = -1;
                                if (exitNotes[i][3].ContainsKey("UF") && exitNotes[i][1]["idDest"].Equals("2"))
                                {

                                    for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                    {
                                        if (icmsForaDoEstado[j][0].Equals(exitNotes[i][3]["UF"]))
                                        {
                                            posUf = j;
                                        }
                                    }

                                    if (posUf < 0)
                                    {
                                        List<string> uf = new List<string>();
                                        uf.Add(exitNotes[i][3]["UF"]);
                                        uf.Add("0");
                                        uf.Add("0");
                                        icmsForaDoEstado.Add(uf);
                                    }

                                }

                                bool cfop = false;

                                for (int k = 0; k < exitNotes[i].Count(); k++)
                                {
                                    string cest = null;
                                    if (exitNotes[i][k].ContainsKey("CEST"))
                                    {
                                        cest = exitNotes[i][k]["CEST"];
                                        if (cest.Equals(""))
                                        {
                                            cest = null;
                                        }
                                    }

                                    if (exitNotes[i][k].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfopsVendaST.Contains(exitNotes[i][k]["CFOP"]))
                                        {
                                            cfop = true;
                                        }

                                    }

                                    if (cfop == true)
                                    {
                                        if (exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            if (exitNotes[i][k].ContainsKey("vProd"))
                                            {
                                                if (posCliente < 0)
                                                {
                                                    vendaCfopSTNaoContribuinteNIncetivo += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                    if (exitNotes[i][1]["idDest"].Equals("2"))
                                                    {
                                                        vendaCfopSTNaoContriForaDoEstadoNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                        icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();

                                                    }
                                                }
                                                else
                                                {
                                                    vendaCfopSTContribuintesNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                }
                                                totalVendas += Convert.ToDecimal(exitNotes[i][k]["vProd"]);

                                            }

                                            if (exitNotes[i][k].ContainsKey("vFrete"))
                                            {
                                                if (posCliente < 0)
                                                {
                                                    vendaCfopSTNaoContribuinteNIncetivo += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                    if (exitNotes[i][1]["idDest"].Equals("2"))
                                                    {
                                                        vendaCfopSTNaoContriForaDoEstadoNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                        icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();

                                                    }
                                                }
                                                else
                                                {
                                                    vendaCfopSTContribuintesNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                }

                                                totalVendas += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                            }

                                            if (exitNotes[i][k].ContainsKey("vDesc"))
                                            {
                                                if (posCliente < 0)
                                                {
                                                    vendaCfopSTNaoContribuinteNIncetivo -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                    if (exitNotes[i][1]["idDest"].Equals("2"))
                                                    {
                                                        vendaCfopSTNaoContriForaDoEstadoNIncentivo -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                        icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();

                                                    }
                                                }
                                                else
                                                {
                                                    vendaCfopSTContribuintesNIncentivo -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                }

                                                totalVendas -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                            }

                                            if (exitNotes[i][k].ContainsKey("vOutro"))
                                            {
                                                if (posCliente < 0)
                                                {
                                                    vendaCfopSTNaoContribuinteNIncetivo += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                    if (exitNotes[i][1]["idDest"].Equals("2"))
                                                    {
                                                        vendaCfopSTNaoContriForaDoEstadoNIncentivo += Convert.ToDecimal(exitNotes[i][k]["Outro"]);
                                                        icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();

                                                    }
                                                }
                                                else
                                                {
                                                    vendaCfopSTContribuintesNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                }

                                                totalVendas += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                            }

                                            if (exitNotes[i][k].ContainsKey("vSeg"))
                                            {
                                                if (posCliente < 0)
                                                {
                                                    vendaCfopSTNaoContribuinteNIncetivo += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                    if (exitNotes[i][1]["idDest"].Equals("2"))
                                                    {
                                                        vendaCfopSTNaoContriForaDoEstadoNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                        icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();

                                                    }
                                                }
                                                else
                                                {
                                                    vendaCfopSTContribuintesNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                }

                                                totalVendas += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                            }
                                        }



                                    }

                                }
                            }

                            // Devolução de Compra
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                if (exitNotes[i][1]["finNFe"] != "4")
                                {
                                    exitNotes.RemoveAt(i);
                                    continue;
                                }

                                bool cfop = false;
                                for (int k = 0; k < exitNotes[i].Count; k++)
                                {
                                    if (exitNotes[i][k].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfopsDevoCompra.Contains(exitNotes[i][k]["CFOP"]))
                                        {
                                            cfop = true;
                                        }

                                    }

                                    if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                    {
                                        debitosIcms += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                    }
                                }
                            }

                            // Devolução de Venda
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                bool cfop = false;

                                for (int k = 0; k < exitNotes[i].Count(); k++)
                                {
                                    if (exitNotes[i][k].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfospDevoVenda.Contains(exitNotes[i][k]["CFOP"]))
                                        {
                                            cfop = true;
                                        }
                                    }
                                    if (exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                    {
                                        creditosIcms += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                    }
                                }
                            }

                            //// Cálculos dos Produtos  Incentivados

                            //Contribuinte
                            var icmsContribuinteIncentivo = Math.Round(Convert.ToDecimal(comp.Icms) * ContribuintesIncentivo / 100, 2);
                            var icmsContribuinteIncentivoAliqM25 = Math.Round(Convert.ToDecimal(comp.IcmsAliqM25) * ContribuintesIncentivoAliqM25 / 100, 2);

                            //Não Contribuinte
                            var totalVendasNContribuinte = Math.Round(naoContribuinteIncentivo + naoContribuinteNIncetivo + vendaCfopSTNaoContribuinteNIncetivo, 2);
                            var icmsNContribuinteIncentivo = Math.Round(Convert.ToDecimal(comp.IcmsNContribuinte) * naoContribuinteIncentivo / 100, 2);

                            //Não Contribuinte Fora do Estado
                            var totalVendasNContribuinteForaDoEstado = Math.Round(naoContriForaDoEstadoIncentivo + naoContriForaDoEstadoNIncentivo + vendaCfopSTNaoContriForaDoEstadoNIncentivo, 2);
                            var icmsNContribuinteForaDoEstado = Math.Round(Convert.ToDecimal(comp.IcmsNContribuinteFora) * totalVendasNContribuinteForaDoEstado / 100, 2);

                            for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                            {
                                icmsForaDoEstado[j][2] = ((Convert.ToDecimal(comp.IcmsNContribuinteFora) * Convert.ToDecimal(icmsForaDoEstado[j][1])) / 100).ToString();
                            }

                            //// Direfença de débito e crédito
                            var diferenca = debitosIcms - creditosIcms;

                            //Total Icms
                            var totalIcms = icmsContribuinteIncentivo + icmsNContribuinteIncentivo + icmsContribuinteIncentivoAliqM25;

                            //// FUNEF e COTAC
                            var baseCalculo = diferenca - totalIcms;

                            //FUNEF
                            decimal percentualFunef = Convert.ToDecimal(comp.Funef == null ? 0 : comp.Funef);
                            var totalFunef = Math.Round((baseCalculo * percentualFunef) / 100, 2);

                            //COTAC
                            decimal percentualCotac = Convert.ToDecimal(comp.Cotac == null ? 0 : comp.Cotac);
                            var totalCotac = Math.Round((baseCalculo * percentualCotac) / 100, 2);

                            //Total Funef e Cotac
                            var totalFunefCotac = totalFunef + totalCotac;

                            ////Total Imposto
                            var totalImposto = icmsContribuinteIncentivo + icmsNContribuinteIncentivo + totalFunef + totalCotac;


                            ////Total Imposto Geral
                            var totalImpostoGeral = totalImposto + icmsNContribuinteForaDoEstado;

                            //// Cálculos dos Totais
                            var totalVendaContribuinte = Math.Round(ContribuintesIncentivo + ContribuintesNIncentivo + vendaCfopSTContribuintesNIncentivo, 2);
                            var totalIcmsGeralIncentivo = Math.Round(icmsContribuinteIncentivo + icmsNContribuinteIncentivo + icmsNContribuinteForaDoEstado, 2);
                            var totalGeralVendasIncentivo = Math.Round(totalVendaContribuinte + totalVendasNContribuinte + ContribuinteIsento + NaoContribuiteIsento + ContribuintesIncentivoAliqM25, 2);



                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                            //// Produtos Incentivados

                            //Contribuinte
                            ViewBag.VendaContribuinteIncentivo = Convert.ToDouble(ContribuintesIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.PercentualIcmsContrib = Convert.ToDouble(comp.Icms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.ValorVendaContribIncentivo = Convert.ToDouble(icmsContribuinteIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.ContribuinteIsento = Convert.ToDouble(ContribuinteIsento.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.ContribuinteIncentivoAliM25 = Convert.ToDouble(ContribuintesIncentivoAliqM25.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.ValorVendaContribuinteAliM25 = Convert.ToDouble(icmsContribuinteIncentivoAliqM25.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.PercentualIcmsAiqM25Contrib = Convert.ToDouble(comp.IcmsAliqM25.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.VendaSTContribuinte = Convert.ToDouble(vendaCfopSTContribuintesNIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            //Não Contribuinte
                            ViewBag.VendaNContribIncentivo = Convert.ToDouble(naoContribuinteIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalVendaNContribuinte = Convert.ToDouble(totalVendasNContribuinte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.PercentualIcmsNContribuinte = Convert.ToDouble(comp.IcmsNContribuinte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.ValorVendaNContribIncentivo = Convert.ToDouble(icmsNContribuinteIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.NaoContribuinteIsento = Convert.ToDouble(NaoContribuiteIsento.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.VendaSTNContribuinte = Convert.ToDouble(vendaCfopSTNaoContribuinteNIncetivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            //Não Contribuinte Fora do Estado
                            ViewBag.VendaNForaEstadoContribuinteIncetivo = Convert.ToDouble(naoContriForaDoEstadoIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalVendaNContribuinteForaDoEstado = Convert.ToDouble(totalVendasNContribuinteForaDoEstado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.PercentualIcmsNaoContribForaDoEstado = Convert.ToDouble(comp.IcmsNContribuinteFora.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.ValorVendaNContribForaDoEstado = Convert.ToDouble(icmsNContribuinteForaDoEstado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.NaoContribuinteForaDoEstadoIsento = Convert.ToDouble(NaoContribuinteForaDoEstadoIsento.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.VendaSTForaEstadoNContribuinte = Convert.ToDouble(vendaCfopSTNaoContriForaDoEstadoNIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                            {
                                icmsForaDoEstado[j][2] = (Convert.ToDouble(icmsForaDoEstado[j][2].ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "")).ToString();
                            }

                            ViewBag.IcmsForaDoEstado = icmsForaDoEstado;

                            //// Produtos não incentivados

                            //Contribuinte
                            ViewBag.VendaContribuinteNIncentivo = Convert.ToDouble(ContribuintesNIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            //Não Contribuinte
                            ViewBag.VendaNContribuinteNIncentivo = Convert.ToDouble(naoContribuinteNIncetivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            //Não Contribuinte Fora do Estado
                            ViewBag.VendaNContribuinteNIncentivoForaDoEstado = Convert.ToDouble(naoContriForaDoEstadoNIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            //// Crédito e Débito
                            //Crédito
                            ViewBag.Credito = Convert.ToDouble(creditosIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            //Débito
                            ViewBag.Debito = Convert.ToDouble(debitosIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            //Diferença
                            ViewBag.Diferenca = Convert.ToDouble(diferenca.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            ////Total Icms
                            ViewBag.TotalIcms = Convert.ToDouble(totalIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            //// FUNEF e COTAC
                            ViewBag.BaseCalculo = Convert.ToDouble(baseCalculo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            //FUNEF
                            ViewBag.PercentualFunef = Convert.ToDouble(percentualFunef.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalFunef = Convert.ToDouble(totalFunef.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            //COTAC
                            ViewBag.PercentualCotac = Convert.ToDouble(percentualCotac.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalCotac = Convert.ToDouble(totalCotac.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            //Total Funef e Cotac
                            ViewBag.TotalFunefCotac = Convert.ToDouble(totalFunefCotac.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            ////Total Imposto
                            ViewBag.TotalImposto = Convert.ToDouble(totalImposto.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            ////Total Imposto Geral
                            ViewBag.TotalImpostoGeral = Convert.ToDouble(totalImpostoGeral.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            //// Total
                            ViewBag.TotalVendaContibuinte = Convert.ToDouble((totalVendaContribuinte + ContribuinteIsento + ContribuintesIncentivoAliqM25).ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGeralVendaNContibuinte = Convert.ToDouble((totalVendasNContribuinte + NaoContribuiteIsento).ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGeralVendaNContibuinteForaDoEstado = Convert.ToDouble((totalVendasNContribuinteForaDoEstado + NaoContribuinteForaDoEstadoIsento).ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGeralIcmsIncentivo = Convert.ToDouble(totalIcmsGeralIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalGeralVendasIncentivo = Convert.ToDouble(totalGeralVendasIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.Uf = comp.Uf;

                        }
                        else
                        {
                            /*List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                            List<List<Dictionary<string, string>>> notesDevo = new List<List<Dictionary<string, string>>>();*/

                            List<ProductIncentivo> productincentivo = new List<ProductIncentivo>();
                            List<string> codeProdIncentivado = new List<string>();
                            List<string> codeProdST = new List<string>();
                            List<string> codeProdIsento = new List<string>();
                            List<string> cestIncentivado = new List<string>();
                            List<string> cestST = new List<string>();
                            List<string> cestIsento = new List<string>();
                            List<List<string>> percentuaisIncentivado = new List<List<string>>();
                            List<List<string>> percentuaisNIncentivado = new List<List<string>>();


                            decimal creditosIcms = import.SpedCredito(caminhoDestinoArquivoOriginal, comp.Id);

                            /*notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                            notesDevo = import.NfeExit(directoryNfeExit, id, type, "devolucao de compra");*/

                            exitNotes = import.Nfe(directoryNfeExit);

                            var cfopsDevoCompra = _companyCfopService.FindByCfopActive(id, "incentivo", "devolucao de compra").Select(_ => _.Cfop.Code);
                            var cfopsVenda = _companyCfopService.FindByCfopActive(id, "incentivo", "venda").Select(_ => _.Cfop.Code);
                            var cfospDevoVenda = _companyCfopService.FindByCfopActive(id, "entrada", "devolução de venda").Select(_ => _.Cfop.Code);

                            var prodsIncentivo = _productIncentivoService.FindAll(null);

                            decimal vendasIncentivada = 0, vendasNIncentivada = 0, debitoIncetivo = 0, debitoNIncentivo = 0;

                            // Vendas
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                                {
                                    exitNotes.RemoveAt(i);
                                    continue;
                                }

                                if (exitNotes[i][1].ContainsKey("dhEmi"))
                                {
                                    //productincentivo = _productIncentivoService.FindByDate(comp.Id, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));
                                    productincentivo = _productIncentivoService.FindByDate(prodsIncentivo, comp.Id, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));
                                    codeProdIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Code).ToList();
                                    codeProdST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Code).ToList();
                                    codeProdIsento = productincentivo.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Code).ToList();

                                    cestIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Cest).ToList();
                                    cestST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Cest).ToList();
                                    cestIsento = productincentivo.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Cest).ToList();
                                }

                                int status = 3;
                                decimal percent = 0;
                                bool cfop = false;

                                for (int k = 0; k < exitNotes[i].Count(); k++)
                                {
                                    if (exitNotes[i][k].ContainsKey("cProd"))
                                    {
                                        status = 3;
                                        percent = 0;

                                        string cest = null;
                                        if (exitNotes[i][k].ContainsKey("CEST"))
                                        {
                                            cest = exitNotes[i][k]["CEST"];
                                            if (cest.Equals(""))
                                            {
                                                cest = null;
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("CFOP"))
                                        {
                                            cfop = false;
                                            if (cfopsVenda.Contains(exitNotes[i][k]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }

                                        if (cfop == true)
                                        {
                                            if (codeProdIncentivado.Contains(exitNotes[i][k]["cProd"]))
                                            {
                                                if (cestIncentivado.Contains(cest))
                                                {
                                                    status = 1;
                                                    var percentualIncentivado = Convert.ToDecimal(productincentivo.Where(_ => _.Code.Equals(exitNotes[i][k]["cProd"])).ToList().Select(_ => _.Percentual).FirstOrDefault());
                                                    percent = percentualIncentivado;
                                                    if (percentualIncentivado < 100)
                                                    {
                                                        var percentualNIncentivado = 100 - percentualIncentivado;

                                                        if (exitNotes[i][k].ContainsKey("vProd"))
                                                        {
                                                            vendasIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vProd"]) * percentualIncentivado) / 100);
                                                            vendasNIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vProd"]) * percentualNIncentivado) / 100);
                                                        }

                                                        if (exitNotes[i][k].ContainsKey("vFrete"))
                                                        {
                                                            vendasIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vFrete"]) * percentualIncentivado) / 100);
                                                            vendasNIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vFrete"]) * percentualNIncentivado) / 100);
                                                        }

                                                        if (exitNotes[i][k].ContainsKey("vDesc"))
                                                        {
                                                            vendasIncentivada -= ((Convert.ToDecimal(exitNotes[i][k]["vDesc"]) * percentualIncentivado) / 100);
                                                            vendasNIncentivada -= ((Convert.ToDecimal(exitNotes[i][k]["vDesc"]) * percentualNIncentivado) / 100);
                                                        }

                                                        if (exitNotes[i][k].ContainsKey("vOutro"))
                                                        {
                                                            vendasIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vOutro"]) * percentualIncentivado) / 100);
                                                            vendasNIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vOutro"]) * percentualNIncentivado) / 100);
                                                        }

                                                        if (exitNotes[i][k].ContainsKey("vSeg"))
                                                        {
                                                            vendasIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vSeg"]) * percentualIncentivado) / 100);
                                                            vendasNIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vSeg"]) * percentualNIncentivado) / 100);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][k].ContainsKey("vProd"))
                                                        {
                                                            vendasIncentivada += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                        }

                                                        if (exitNotes[i][k].ContainsKey("vFrete"))
                                                        {
                                                            vendasIncentivada += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                        }

                                                        if (exitNotes[i][k].ContainsKey("vDesc"))
                                                        {
                                                            vendasIncentivada -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                        }

                                                        if (exitNotes[i][k].ContainsKey("vOutro"))
                                                        {
                                                            vendasIncentivada += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                        }

                                                        if (exitNotes[i][k].ContainsKey("vSeg"))
                                                        {
                                                            vendasIncentivada += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                        }
                                                    }
                                                }

                                            }
                                            else if (codeProdST.Contains(exitNotes[i][k]["cProd"]))
                                            {
                                                if (cestST.Contains(cest))
                                                {
                                                    if (exitNotes[i][k].ContainsKey("vProd"))
                                                    {
                                                        vendasNIncentivada += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vFrete"))
                                                    {
                                                        vendasNIncentivada += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vDesc"))
                                                    {
                                                        vendasNIncentivada -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vOutro"))
                                                    {
                                                        vendasNIncentivada += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vSeg"))
                                                    {
                                                        vendasNIncentivada += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                    }

                                                    status = 2;
                                                    percent = 0;
                                                }
                                            }
                                            else if (codeProdIsento.Contains(exitNotes[i][k]["cProd"]))
                                            {
                                                if (cestIsento.Contains(cest))
                                                {
                                                    status = 3;
                                                    percent = 0;
                                                }
                                            }
                                            else
                                            {
                                                throw new Exception("Há Produtos não Tributado");
                                            }
                                        }


                                    }

                                    if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                    {
                                        if (status == 1)
                                        {
                                            if (percent < 100)
                                            {
                                                var percentNIncentivado = 100 - percent;

                                                int pos = -1;
                                                for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                {
                                                    if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                    {
                                                        pos = j;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    List<string> percIncentivado = new List<string>();
                                                    percIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100).ToString());
                                                    percIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                    percIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percent) / 100).ToString());
                                                    percentuaisIncentivado.Add(percIncentivado);
                                                }
                                                else
                                                {
                                                    percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100)).ToString();
                                                    percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percent) / 100)).ToString();
                                                }

                                                debitoIncetivo += (((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) / 100) * percent) / 100);

                                                int indice = -1;
                                                for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                {
                                                    if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                    {
                                                        indice = j;
                                                    }
                                                }

                                                if (indice < 0)
                                                {
                                                    List<string> percNIncentivado = new List<string>();
                                                    percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percentNIncentivado) / 100).ToString());
                                                    percNIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                    percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100).ToString());
                                                    percentuaisNIncentivado.Add(percNIncentivado);
                                                }
                                                else
                                                {
                                                    percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percentNIncentivado) / 100)).ToString();
                                                    percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100)).ToString();
                                                }
                                                debitoNIncentivo += ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100);

                                            }
                                            else
                                            {
                                                int pos = -1;
                                                for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                {
                                                    if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                    {
                                                        pos = j;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    List<string> percIncentivado = new List<string>();
                                                    percIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                    percIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                    percIncentivado.Add(exitNotes[i][k]["vICMS"]);
                                                    percentuaisIncentivado.Add(percIncentivado);
                                                }
                                                else
                                                {
                                                    percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                    percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + (Convert.ToDecimal(exitNotes[i][k]["vICMS"]))).ToString();
                                                }

                                                debitoIncetivo += (Convert.ToDecimal(exitNotes[i][k]["vICMS"]));

                                            }
                                        }
                                        else if (status == 2)
                                        {
                                            int pos = -1;
                                            for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                            {
                                                if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                {
                                                    pos = j;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                List<string> percNIncentivado = new List<string>();
                                                percNIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                percNIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                percNIncentivado.Add(exitNotes[i][k]["vICMS"]);
                                                percentuaisNIncentivado.Add(percNIncentivado);
                                            }
                                            else
                                            {
                                                percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vICMS"])).ToString();
                                            }
                                            debitoNIncentivo += (Convert.ToDecimal(exitNotes[i][k]["vICMS"]));

                                        }
                                    }

                                    if (exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                    {
                                        if (status == 1)
                                        {
                                            if (percent < 100)
                                            {
                                                var percentNIncentivado = 100 - percent;

                                                int pos = -1;
                                                for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                {
                                                    if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                    {
                                                        pos = j;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    List<string> percIncentivado = new List<string>();
                                                    percIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percent) / 100).ToString());
                                                    percIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                    percIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vFCP"])) * percent) / 100).ToString());
                                                    percentuaisIncentivado.Add(percIncentivado);
                                                }
                                                else
                                                {
                                                    percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100)).ToString();
                                                    percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percent) / 100)).ToString();
                                                }

                                                debitoIncetivo += ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percent) / 100);

                                                int indice = -1;
                                                for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                {
                                                    if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                    {
                                                        indice = j;
                                                    }
                                                }

                                                if (indice < 0)
                                                {
                                                    List<string> percNIncentivado = new List<string>();
                                                    percNIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percentNIncentivado) / 100).ToString());
                                                    percNIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                    percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100).ToString());
                                                    percentuaisNIncentivado.Add(percNIncentivado);
                                                }
                                                else
                                                {
                                                    percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + (((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percentNIncentivado) / 100)).ToString();
                                                    percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100)).ToString();
                                                }

                                                debitoNIncentivo += ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100);

                                            }
                                            else
                                            {
                                                int pos = -1;
                                                for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                {
                                                    if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                    {
                                                        pos = j;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    List<string> percIncentivado = new List<string>();
                                                    percIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                    percIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                    percIncentivado.Add(exitNotes[i][k]["vFCP"]);
                                                    percentuaisIncentivado.Add(percIncentivado);
                                                }
                                                else
                                                {
                                                    percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                    percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vFCP"])).ToString();
                                                }

                                                debitoIncetivo += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                            }
                                        }
                                        else if (status == 2)
                                        {
                                            int pos = -1;
                                            for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                            {
                                                if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                {
                                                    pos = j;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                List<string> percNIncentivado = new List<string>();
                                                percNIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                percNIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                percNIncentivado.Add(exitNotes[i][k]["vFCP"]);

                                                percentuaisNIncentivado.Add(percNIncentivado);
                                            }
                                            else
                                            {
                                                percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vFCP"])).ToString();
                                            }

                                            debitoNIncentivo += (Convert.ToDecimal(exitNotes[i][k]["vFCP"]));
                                        }
                                    }

                                }
                            }

                            // Devolução de Compra
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                if (exitNotes[i][1]["finNFe"] != "4")
                                {
                                    exitNotes.RemoveAt(i);
                                    continue;
                                }

                                if (exitNotes[i][1].ContainsKey("dhEmi"))
                                {
                                    //productincentivo = _productIncentivoService.FindByDate(comp.Id, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));
                                    productincentivo = _productIncentivoService.FindByDate(prodsIncentivo, comp.Id, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));

                                    codeProdIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Code).ToList();
                                    codeProdST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Code).ToList();
                                    codeProdIsento = productincentivo.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Code).ToList();

                                    cestIncentivado = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Cest).ToList();
                                    cestST = productincentivo.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Cest).ToList();
                                    cestIsento = productincentivo.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Cest).ToList();
                                }

                                int status = 3;
                                decimal percent = 0;

                                bool cfop = false;

                                for (int k = 0; k < exitNotes[i].Count(); k++)
                                {
                                    if (exitNotes[i][k].ContainsKey("cProd"))
                                    {
                                        status = 3;
                                        percent = 0;
                                        string cest = null;
                                        if (exitNotes[i][k].ContainsKey("CEST"))
                                        {
                                            cest = exitNotes[i][k]["CEST"];
                                            if (cest.Equals(""))
                                            {
                                                cest = null;
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("CFOP"))
                                        {
                                            cfop = false;
                                            if (cfopsDevoCompra.Contains(exitNotes[i][k]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }

                                        if (cfop == true)
                                        {
                                            if (codeProdIncentivado.Contains(exitNotes[i][k]["cProd"]))
                                            {
                                                if (cestIncentivado.Contains(cest))
                                                {
                                                    status = 1;
                                                    var percentualIncentivado = Convert.ToDecimal(productincentivo.Where(_ => _.Code.Equals(exitNotes[i][k]["cProd"])).ToList().Select(_ => _.Percentual).FirstOrDefault());
                                                    percent = percentualIncentivado;
                                                }
                                            }
                                            else if (codeProdST.Contains(exitNotes[i][k]["cProd"]))
                                            {
                                                if (cestST.Contains(cest))
                                                {
                                                    status = 2;
                                                    percent = 0;
                                                }
                                            }
                                            else if (codeProdIsento.Contains(exitNotes[i][k]["cProd"]))
                                            {
                                                if (cestIsento.Contains(cest))
                                                {
                                                    status = 3;
                                                    percent = 0;
                                                }
                                            }
                                            else
                                            {
                                                throw new Exception("Há Produtos não Tributado");
                                            }
                                        }

                                    }

                                    if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                    {
                                        if (status == 1)
                                        {
                                            if (percent < 100)
                                            {
                                                var percentNIncentivado = 100 - percent;

                                                int pos = -1;
                                                for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                {
                                                    if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                    {
                                                        pos = j;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    List<string> percIncentivado = new List<string>();
                                                    percIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100).ToString());
                                                    percIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                    percIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percent) / 100).ToString());
                                                    percentuaisIncentivado.Add(percIncentivado);
                                                }
                                                else
                                                {
                                                    percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100)).ToString();
                                                    percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percent) / 100)).ToString();
                                                }

                                                debitoIncetivo += (((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) / 100) * percent) / 100);

                                                int indice = -1;
                                                for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                {
                                                    if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                    {
                                                        indice = j;
                                                    }
                                                }

                                                if (indice < 0)
                                                {
                                                    List<string> percNIncentivado = new List<string>();
                                                    percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percentNIncentivado) / 100).ToString());
                                                    percNIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                    percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100).ToString());
                                                    percentuaisNIncentivado.Add(percNIncentivado);
                                                }
                                                else
                                                {
                                                    percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percentNIncentivado) / 100)).ToString();
                                                    percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100)).ToString();
                                                }
                                                debitoNIncentivo += ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100);

                                            }
                                            else
                                            {
                                                int pos = -1;
                                                for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                {
                                                    if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                    {
                                                        pos = j;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    List<string> percIncentivado = new List<string>();
                                                    percIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                    percIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                    percIncentivado.Add(exitNotes[i][k]["vICMS"]);
                                                    percentuaisIncentivado.Add(percIncentivado);
                                                }
                                                else
                                                {
                                                    percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                    percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + (Convert.ToDecimal(exitNotes[i][k]["vICMS"]))).ToString();
                                                }

                                                debitoIncetivo += (Convert.ToDecimal(exitNotes[i][k]["vICMS"]));

                                            }
                                        }
                                        else if (status == 2)
                                        {
                                            int pos = -1;
                                            for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                            {
                                                if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                {
                                                    pos = j;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                List<string> percNIncentivado = new List<string>();
                                                percNIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                percNIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                percNIncentivado.Add(exitNotes[i][k]["vICMS"]);
                                                percentuaisNIncentivado.Add(percNIncentivado);
                                            }
                                            else
                                            {
                                                percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vICMS"])).ToString();
                                            }
                                            debitoNIncentivo += (Convert.ToDecimal(exitNotes[i][k]["vICMS"]));

                                        }
                                    }

                                    if (exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                    {
                                        if (status == 1)
                                        {
                                            if (percent < 100)
                                            {
                                                var percentNIncentivado = 100 - percent;

                                                int pos = -1;
                                                for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                {
                                                    if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                    {
                                                        pos = j;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    List<string> percIncentivado = new List<string>();
                                                    percIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percent) / 100).ToString());
                                                    percIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                    percIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vFCP"])) * percent) / 100).ToString());
                                                    percentuaisIncentivado.Add(percIncentivado);
                                                }
                                                else
                                                {
                                                    percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100)).ToString();
                                                    percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percent) / 100)).ToString();
                                                }

                                                debitoIncetivo += ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percent) / 100);

                                                int indice = -1;
                                                for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                {
                                                    if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                    {
                                                        indice = j;
                                                    }
                                                }

                                                if (indice < 0)
                                                {
                                                    List<string> percNIncentivado = new List<string>();
                                                    percNIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percentNIncentivado) / 100).ToString());
                                                    percNIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                    percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100).ToString());
                                                    percentuaisNIncentivado.Add(percNIncentivado);
                                                }
                                                else
                                                {
                                                    percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + (((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percentNIncentivado) / 100)).ToString();
                                                    percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100)).ToString();
                                                }

                                                debitoNIncentivo += ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100);

                                            }
                                            else
                                            {
                                                int pos = -1;
                                                for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                {
                                                    if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                    {
                                                        pos = j;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    List<string> percIncentivado = new List<string>();
                                                    percIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                    percIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                    percIncentivado.Add(exitNotes[i][k]["vFCP"]);
                                                    percentuaisIncentivado.Add(percIncentivado);
                                                }
                                                else
                                                {
                                                    percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                    percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vFCP"])).ToString();
                                                }

                                                debitoIncetivo += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                            }
                                        }
                                        else if (status == 2)
                                        {
                                            int pos = -1;
                                            for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                            {
                                                if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                {
                                                    pos = j;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                List<string> percNIncentivado = new List<string>();
                                                percNIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                percNIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                percNIncentivado.Add(exitNotes[i][k]["vFCP"]);

                                                percentuaisNIncentivado.Add(percNIncentivado);
                                            }
                                            else
                                            {
                                                percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vFCP"])).ToString();
                                            }

                                            debitoNIncentivo += (Convert.ToDecimal(exitNotes[i][k]["vFCP"]));
                                        }
                                    }

                                }
                            }

                            // Devolução de Venda
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                bool cfop = false;

                                for (int k = 0; k < exitNotes[i].Count(); k++)
                                {
                                    if (exitNotes[i][k].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfospDevoVenda.Contains(exitNotes[i][k]["CFOP"]))
                                        {
                                            cfop = true;
                                        }
                                    }
                                    if (exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                    {
                                        creditosIcms += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                    }
                                }
                            }

                            var totalVendas = vendasIncentivada + vendasNIncentivada;

                            var difApuNormal = debitoIncetivo - creditosIcms;

                            var percentualCreditoNIncentivado = vendasNIncentivada / totalVendas * 100;
                            var creditoNIncentivado = creditosIcms * percentualCreditoNIncentivado / 100;

                            var difApuNNormal = debitoNIncentivo - creditoNIncentivado;

                            //Funef e Cotac
                            var baseDeCalcFunef = difApuNormal - difApuNNormal;
                            decimal valorFunef = baseDeCalcFunef * Convert.ToDecimal(comp.Funef) / 100;
                            decimal valorCotac = baseDeCalcFunef * Convert.ToDecimal(comp.Cotac) / 100;

                            var totalImposto = difApuNNormal + valorFunef + valorCotac;

                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                            List<List<string>> valoresIncentivo = new List<List<string>>();

                            for (int i = 0; i < percentuaisIncentivado.Count(); i++)
                            {
                                List<string> percentual = new List<string>();
                                percentual.Add((Convert.ToDouble(percentuaisIncentivado[i][0].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "")).ToString());
                                percentual.Add(percentuaisIncentivado[i][1].Replace(".", ","));
                                percentual.Add((Convert.ToDouble(percentuaisIncentivado[i][2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "")).ToString());
                                valoresIncentivo.Add(percentual);
                            }

                            List<List<string>> valoresNIncentivo = new List<List<string>>();

                            for (int i = 0; i < percentuaisNIncentivado.Count(); i++)
                            {
                                List<string> percentual = new List<string>();
                                percentual.Add((Convert.ToDouble(percentuaisNIncentivado[i][0].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "")).ToString());
                                percentual.Add(percentuaisNIncentivado[i][1].Replace(".", ","));
                                percentual.Add((Convert.ToDouble(percentuaisNIncentivado[i][2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "")).ToString());
                                valoresNIncentivo.Add(percentual);
                            }


                            if (valoresIncentivo.Count() < valoresNIncentivo.Count())
                            {
                                int diferenca = valoresNIncentivo.Count() - valoresIncentivo.Count();
                                for (int i = 0; i < diferenca; i++)
                                {
                                    List<string> percentual = new List<string>();
                                    percentual.Add("0,00");
                                    percentual.Add("0,00");
                                    percentual.Add("0,00");
                                    valoresIncentivo.Add(percentual);
                                }
                            }
                            else if (valoresIncentivo.Count() > valoresNIncentivo.Count())
                            {
                                int diferenca = valoresIncentivo.Count() - valoresNIncentivo.Count();
                                for (int i = 0; i < diferenca; i++)
                                {
                                    List<string> percentual = new List<string>();
                                    percentual.Add("0,00");
                                    percentual.Add("0,00");
                                    percentual.Add("0,00");
                                    valoresNIncentivo.Add(percentual);
                                }
                            }

                            //Incentivado
                            ViewBag.ValoresIncentivo = valoresIncentivo;
                            ViewBag.DebitoIncentivo = Convert.ToDouble(debitoIncetivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""); ViewBag.TotalVendas = Convert.ToDouble(totalVendas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalVendasIncentivadas = Convert.ToDouble(vendasIncentivada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            //Não Incentivado
                            ViewBag.ValoresNIncentivo = valoresNIncentivo;
                            ViewBag.PercentualCreditoNIncentivo = Convert.ToDouble(percentualCreditoNIncentivado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.CreditoNIncentivo = Convert.ToDouble(creditoNIncentivado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.DebitoNIncentivo = Convert.ToDouble(debitoNIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""); ViewBag.TotalVendas = Convert.ToDouble(totalVendas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.TotalVendasNIncentivadas = Convert.ToDouble(vendasNIncentivada.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            // Total
                            ViewBag.TotalVendas = Convert.ToDouble(totalVendas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.Credito = Convert.ToDouble(creditosIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""); ViewBag.TotalVendas = Convert.ToDouble(totalVendas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                            //Apuração Normal
                            //Debito - ViewBag.DebitoIncentivo
                            ViewBag.CreditoIncentivo = Convert.ToDouble(creditosIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.DifApuNormal = Convert.ToDouble(difApuNormal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            //Apuração ñ Incentivada
                            //Debito - ViewBag.DebitoNIncetivo
                            //Credito - CreditoNIncentivo
                            ViewBag.DifApuNNormal = Convert.ToDouble(difApuNNormal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            // Funef e COTAC
                            // DifNormal - DifNIncentivada
                            ViewBag.BaseDeCalcFunef = Convert.ToDouble(baseDeCalcFunef.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.PercentFunef = comp.Funef;
                            ViewBag.ValorFunef = Convert.ToDouble(valorFunef.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                            ViewBag.PercentCotac = comp.Cotac;
                            ViewBag.ValorCotac = Convert.ToDouble(valorCotac.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                            // Total De Imposto
                            ViewBag.TotalDeImposto = Convert.ToDouble(totalImposto.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        }

                    }

                    totalDarSTCO += Convert.ToDecimal(totalDiefSTSIE - icmsStnotaSIE);
                    totalDarFecop += Convert.ToDecimal(totalfecopDiefSTSIE - (icmsFecop1STSIE + icmsFecop2STSIE));


                    // Antecipação Parcial
                    decimal totalFreteAPIE = 0;

                    foreach (var prod in products)
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

                    decimal valorNfe1NormalAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalAPSIE = Math.Round(Convert.ToDecimal(products.Where( _ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe1NormalAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal? gnrePagaAPIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreAp).Sum()), 2);
                    decimal? gnreNPagaAPIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNAp).Sum()), 2);
                    decimal? gnrePagaAPSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreAp).Sum()), 2);
                    decimal? gnreNPagaAPSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNAp).Sum()), 2);
                    
                    decimal? icmsStAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalAPIE + valorNfe1RetAPIE + valorNfe2NormalAPIE + valorNfe2RetAPIE;
                    decimal? icmsStAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalAPSIE + valorNfe1RetAPSIE + valorNfe2NormalAPSIE + valorNfe2RetAPSIE;
                    decimal? totalApuradoAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalApuradoAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalDiefAPSIE = (totalApuradoAPSIE + totalFreteAPIE) - icmsStAPSIE + gnreNPagaAPSIE - gnrePagaAPSIE;
                    decimal? totalDiefAPIE = totalApuradoAPIE - icmsStAPIE + gnreNPagaAPIE - gnrePagaAPIE - totalFreteAPIE;
                    int? qtdAPSIE = products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Count();
                    int? qtdAPIE = products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Count();

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

                    totalDarAp += IcmsAPagarAPSIE;


                    // Consumo
                    decimal totalFreteCOIE = 0;

                    foreach (var prod in products)
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

                    decimal valorNfe1NormalCOSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetCOSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalCOSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetCOSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe1NormalCOIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetCOIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalCOIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetCOIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal? gnrePagaCOIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreCo).Sum()), 2);
                    decimal? gnreNPagaCOIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNCo).Sum()), 2);
                    decimal? gnrePagaCOSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreCo).Sum()), 2);
                    decimal? gnreNPagaCOSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNCo).Sum()), 2);
                    
                    decimal? icmsStCOIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalCOIE + valorNfe1RetCOIE + valorNfe2NormalCOIE + valorNfe2RetCOIE;
                    decimal? icmsStCOSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalCOSIE + valorNfe1RetCOSIE + valorNfe2NormalCOSIE + valorNfe2RetCOSIE;
                    decimal? totalApuradoCOIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalApuradoCOSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalDiefCOSIE = (totalApuradoCOSIE + totalFreteCOIE) - icmsStCOSIE + gnreNPagaCOSIE - gnrePagaCOSIE;
                    decimal? totalDiefCOIE = totalApuradoCOIE - icmsStCOIE + gnreNPagaCOIE - gnrePagaCOIE - totalFreteCOIE;
                    int? qtdCOSIE = products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Count();
                    int? qtdCOIE = products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Count();

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

                    totalDarSTCO += IcmsAPagarCOSIE;


                    // Consumo para Revenda
                    decimal totalFreteCORIE = 0;

                    foreach (var prod in products)
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

                    decimal valorNfe1NormalCORSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetCORSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalCORSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetCORSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe1NormalCORIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetCORIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalCORIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetCORIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal? icmsStCORIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalCORIE + valorNfe1RetCORIE + valorNfe2NormalCORIE + valorNfe2RetCORIE;
                    decimal? icmsStCORSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalCORSIE + valorNfe1RetCORSIE + valorNfe2NormalCORSIE + valorNfe2RetCORSIE;
                    decimal? totalApuradoCORIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalApuradoCORSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalDiefCORSIE = (totalApuradoCORSIE + totalFreteCORIE) - icmsStCORSIE;
                    decimal? totalDiefCORIE = totalApuradoCORIE - icmsStCORIE - totalFreteCORIE;
                    int? qtdCORSIE = products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Count();
                    int? qtdCORIE = products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Count();

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

                    totalDarSTCO += IcmsAPagarCORSIE;


                    // Imobilizado
                    decimal totalFreteIMIE = 0;

                    foreach (var prod in products)
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

                    decimal valorNfe1NormalIMSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetIMSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalIMSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetIMSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe1NormalIMIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetIMIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalIMIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetIMIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal? gnrePagaIMIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreIm).Sum()), 2);
                    decimal? gnreNPagaIMIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNIm).Sum()), 2);
                    decimal? gnrePagaIMSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreIm).Sum()), 2);
                    decimal? gnreNPagaIMSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNIm).Sum()), 2);

                    decimal? icmsStIMIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalIMIE + valorNfe1RetIMIE + valorNfe2NormalIMIE + valorNfe2RetIMIE;
                    decimal? icmsStIMSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalIMSIE + valorNfe1RetIMSIE + valorNfe2NormalIMSIE + valorNfe2RetIMSIE;
                    decimal? totalApuradoIMIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalApuradoIMSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsApurado).Sum()), 2);
                    decimal? totalDiefIMSIE = (totalApuradoIMSIE + totalFreteIMIE) - icmsStIMSIE + gnreNPagaIMSIE - gnrePagaIMSIE;
                    decimal? totaDiefIMIE = totalApuradoIMIE - icmsStIMIE + gnreNPagaIMIE - gnrePagaIMIE - totalFreteIMIE;
                    int? qtdIMSIE = products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Count();
                    int? qtdIMIE = products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Count();

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

                    totalDarIm += IcmsAPagarIMSIE;


                    // Antecipação Total
                    decimal totalFreteATIE = 0;

                    foreach (var prod in products)
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

                    decimal totalIcmsFreteATIE = 0, totalFecop1FreteATIE = 0, totalFecop2FreteATIE = 0;

                    foreach (var prod in products)
                    {
                        if (!prod.Note.Iest.Equals("") && prod.TaxationTypeId.Equals(8))
                        {
                            if (Convert.ToDecimal(prod.Aliqinterna) > 0)
                            {
                                decimal valorAgreg = 0;
                                if (prod.Mva != null)
                                {
                                    valorAgreg = calculation.ValorAgregadoMva(prod.Freterateado, Convert.ToDecimal(prod.Mva));
                                }
                                if (prod.BCR != null)
                                {
                                    valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(prod.BCR), valorAgreg);
                                }
                                if (prod.Fecop != null)
                                {
                                    if (Convert.ToDecimal(prod.Fecop).Equals(1))
                                    {
                                        totalFecop1FreteATIE += calculation.valorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                    }
                                    else
                                    {
                                        totalFecop2FreteATIE += calculation.valorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                    }

                                }

                                totalIcmsFreteATIE += calculation.valorAgregadoAliqInt(Convert.ToDecimal(prod.Aliqinterna), Convert.ToDecimal(prod.Fecop), valorAgreg) - prod.IcmsCTe;
                            }
                        }
                    }

                    ViewBag.TotalIcmsFreteATIE = Convert.ToDouble(totalIcmsFreteATIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFecopFreteATIE = Convert.ToDouble(totalFecop1FreteATIE + totalFecop2FreteATIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal? icmsStATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.IcmsST).Sum()), 2);
                    decimal? icmsStATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.IcmsST).Sum()), 2);
                    decimal? totalApuradoATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.TotalICMS).Sum()), 2);
                    decimal? totalApuradoATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.TotalICMS).Sum()), 2);
                    decimal? totalDiefATSIE = (totalApuradoATSIE + totalFreteATIE) - icmsStATSIE;
                    decimal? totalDiefATIE = totalApuradoATIE - icmsStATIE;
                    int? qtdATSIE = products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Count();
                    int? qtdATIE = products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Count();

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

                    decimal valorbase1ATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                    decimal valorbase1ATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                    decimal valorbase2ATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                    decimal valorbase2ATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);

                    decimal TotalFecopCalcATIE = valorbase1ATIE + valorbase2ATIE;
                    decimal TotalFecopCalcATSIE = valorbase1ATSIE + valorbase2ATSIE;
                    ViewBag.TotalFecopCalculadaATIE = Convert.ToDouble(Math.Round(TotalFecopCalcATIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFecopCalculadaATSIE = Convert.ToDouble(Math.Round(TotalFecopCalcATSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal baseNfe1NormalATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                    decimal baseNfe1NormalATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                    decimal baseNfe1RetATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);
                    decimal baseNfe1RetATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);


                    decimal valorNfe1NormalATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe1NormalATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe1RetATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal valorNfe2NormalATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                    decimal valorNfe2NormalATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                    decimal valorNfe2RetATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal TotalFecopNfeATIE = valorNfe1NormalATIE + valorNfe1RetATIE + valorNfe2NormalATIE + valorNfe2RetATIE;
                    decimal TotalFecopNfeATSIE = valorNfe1NormalATSIE + valorNfe1RetATSIE + valorNfe2NormalATSIE + valorNfe2RetATSIE;
                    ViewBag.TotalFecopNfeATIE = Convert.ToDouble(Math.Round(TotalFecopNfeATIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFecopNfeATSIE = Convert.ToDouble(Math.Round(TotalFecopNfeATSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal gnreNPagaFecopATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2);
                    decimal gnreNPagaFecopATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2);
                    ViewBag.GNREnPagaFecopATIE = Convert.ToDouble(gnreNPagaFecopATIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.GNREnPagaFecopATSIE = Convert.ToDouble(gnreNPagaFecopATSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    decimal gnrePagaFecop1ATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2);
                    decimal gnrePagaFecop1ATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2);
                    decimal gnrePagaFecop2ATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2);
                    decimal gnrePagaFecop2ATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2);

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

                    decimal? icmsFecop1ATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2);
                    decimal? icmsFecop1ATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2);
                    decimal? icmsFecop2ATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2);
                    decimal? icmsFecop2ATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2);
                    ViewBag.IcmsFecopATIE = Convert.ToDouble(icmsFecop1ATIE + icmsFecop2ATIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.IcmsFecopATSIE = Convert.ToDouble(icmsFecop1ATSIE + icmsFecop2ATSIE).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    ViewBag.TotalFinalFecopCalculadaATIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalfecopDiefATIE - (icmsFecop1ATIE + icmsFecop2ATIE)), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalFinalFecopCalculadaATSIE = Convert.ToDouble(Math.Round(Convert.ToDecimal(totalfecopDiefATSIE - (icmsFecop1ATSIE + icmsFecop2ATSIE)), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    totalDarSTCO += IcmsAPagarATSIE;
                    totalDarFecop += Convert.ToDecimal(totalfecopDiefATSIE - (icmsFecop1ATSIE + icmsFecop2ATSIE));

                    // Isento
                    int? qtdIsentoIE = products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(7)).Count();
                    int? qtdIsentoSIE = products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(7)).Count();
                    ViewBag.QtdIsentoIE = qtdIsentoIE;
                    ViewBag.QtdIsentoSIE = qtdIsentoSIE;


                    // Somatório Geral
                    ViewBag.TotalDarSTCO = Convert.ToDouble(Math.Round(totalDarSTCO, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDarFecop = Convert.ToDouble(Math.Round(totalDarFecop, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDarIm = Convert.ToDouble(Math.Round(totalDarIm, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDarAp = Convert.ToDouble(Math.Round(totalDarAp, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDarFunef = Convert.ToDouble(Math.Round(totalDarFunef, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDarIcms = Convert.ToDouble(Math.Round(totalDarIcms, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalDarCotac = Convert.ToDouble(Math.Round(totalDarCotac, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                }
                else if (type == 9)
                {
                    products = _service.FindByProductsType(notesS.ToList(), typeTaxation);
                    List<List<string>> fornecedores = new List<List<string>>();
                    decimal icmsStTotal = 0, fecopStTotal = 0, gnrePagaTotal = 0, gnreNPagaTotal = 0, gnrefecopPagaTotal = 0;
                    
                    foreach (var prod in products)
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
                else if(type == 10)
                {

                    var query =  notes.GroupJoin(products.Where(_ => _.Status.Equals(true)).ToList(),
                      n => n,
                      p => p.Note,
                      (n, pCollection) =>
                          new
                          {
                              nNf = n.Nnf,
                              dhemi = n.Dhemi,
                              xNome = n.Xnome,
                              cnpj = n.Cnpj,
                              vNF = n.Vnf,
                              IcmsST = pCollection.Sum(_ => _.IcmsST)
                          }).Where(_ => _.IcmsST > 0);

                    List<List<string>> nnotes = new List<List<string>>();
                    foreach (var item in query.ToList())
                    {
                        List<string> nnote = new List<string>();
                        nnote.Add(item.nNf);
                        nnote.Add(item.dhemi.ToString("dd/MM/yyy"));
                        nnote.Add(item.xNome);
                        nnote.Add(item.cnpj);
                        nnote.Add(Convert.ToDouble(Math.Round(item.vNF, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                        nnote.Add(Convert.ToDouble(Math.Round(Convert.ToDecimal(item.IcmsST), 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                        nnotes.Add(nnote);
                    }
                    ViewBag.NNotes = nnotes;

                }

                ViewBag.IcmsStNoteS = Convert.ToDouble(Math.Round(icmsStnoteSIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                ViewBag.IcmsStNoteI = Convert.ToDouble(Math.Round(icmsStnoteIE, 2)).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                var dar = _darService.FindAll(GetLog(OccorenceLog.Read));

                var darFecop = dar.Where(_ => _.Type.Equals("Fecop")).Select(_ => _.Code).FirstOrDefault();
                var darStCo = dar.Where(_ => _.Type.Equals("ST-CO")).Select(_ => _.Code).FirstOrDefault();
                var darIcms = dar.Where(_ => _.Type.Equals("Icms")).Select(_ => _.Code).FirstOrDefault();
                var darAp = dar.Where(_ => _.Type.Equals("AP")).Select(_ => _.Code).FirstOrDefault();
                var darIm = dar.Where(_ => _.Type.Equals("IM")).Select(_ => _.Code).FirstOrDefault();
                var darFunef = dar.Where(_ => _.Type.Equals("Funef")).Select(_ => _.Code).FirstOrDefault();
                var darCotac = dar.Where(_ => _.Type.Equals("Cotac")).Select(_ => _.Code).FirstOrDefault();

                ViewBag.DarFecop = darFecop;
                ViewBag.DarSTCO = darStCo;
                ViewBag.DarIcms = darIcms;
                ViewBag.DarAp = darAp;
                ViewBag.DarIm = darIm;
                ViewBag.DarFunef = darFunef;
                ViewBag.DarCotac = darCotac;

               
                return View(products);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
        
        public IActionResult Delete(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var prod = _service.FindById(id,GetLog(Model.OccorenceLog.Read));
                _service.Delete(id, GetLog(Model.OccorenceLog.Delete));

                return RedirectToAction("Index", new { noteId = prod.NoteId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}