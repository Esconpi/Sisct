using Escon.SisctNET.IntegrationDarWeb;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.DarWebWs;
using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Email;
using Escon.SisctNET.Web.Taxation;
using Escon.SisctNET.Web.ViewsModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{
    public class ProductNote : ControllerBaseSisctNET
    {
        private readonly IProductNoteService _service;
        private readonly ITaxationTypeService _taxationTypeService;
        private readonly IProductService _productService;
        private readonly IProduct1Service _product1Service;
        private readonly IProduct2Service _product2Service;
        private readonly INoteService _noteService;
        private readonly INcmService _ncmService;
        private readonly IAliquotService _aliquotService;
        private readonly ITaxationService _taxationService;
        private readonly ICompanyService _companyService;
        private readonly IDarService _darService;
        private readonly IDarDocumentService _darDocumentService;
        private readonly IConfigurationService _configurationService;
        private readonly ICompanyCfopService _companyCfopService;
        private readonly ISuspensionService _suspensionService;
        private readonly IClientService _clientService;
        private readonly INcmConvenioService _ncmConvenioService;
        private readonly IProductIncentivoService _productIncentivoService;
        private readonly ITaxService _taxService;
        private readonly IGrupoService _grupoService;
        private readonly INotificationService _notificationService;
        private readonly IIntegrationWsDar _integrationWsDar;
        private readonly IEmailService _serviceEmail;
        private readonly IEmailConfiguration _emailConfiguration;
        private readonly IEmailResponsibleService _emailResponsibleService;
        private readonly ITaxAnexoService _taxAnexoService;
        private readonly ICompraAnexoService _compraAnexoService;
        private readonly IDevoClienteService _devoClienteService;
        private readonly IDevoFornecedorService _devoFornecedorService;
        private readonly IVendaAnexoService _vendaAnexoService;
        private readonly ICreditBalanceService _creditBalanceService;
        private readonly IConfiguration _configuration;
        private readonly ITaxProducerService _taxProducerService;

        public ProductNote(
            IConfiguration configuration,
            IProductNoteService service,
            INoteService noteService,
            INcmService ncmService,
            IProductService productService,
            IProduct1Service product1Service,
            IProduct2Service product2Service,
            ITaxationTypeService taxationTypeService,
            IAliquotService aliquotService,
            ITaxationService taxationService,
            ICompanyService companyService,
            IDarService darService,
            IDarDocumentService darDocumentService,
            ICompanyCfopService companyCfopService,
            ISuspensionService suspensionService,
            IClientService clientService,
            INcmConvenioService ncmConvenioService,
            IProductIncentivoService productIncentivoService,
            ITaxService taxService,
            IGrupoService grupoService,
            INotificationService notificationService,
            IFunctionalityService functionalityService,
            IIntegrationWsDar integrationWsDar,
            IConfigurationService configurationService,
            IEmailService serviceEmail,
            IEmailConfiguration emailConfiguration,
            IEmailResponsibleService emailResponsibleService,
            ITaxAnexoService taxAnexoService,
            ICompraAnexoService compraAnexoService,
            IDevoClienteService devoClienteService,
            IDevoFornecedorService devoFornecedorService,
            IVendaAnexoService vendaAnexoService,
            ICreditBalanceService creditBalanceService,
            ITaxProducerService taxProducerService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "ProductNote")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _noteService = noteService;
            _ncmService = ncmService;
            _taxationTypeService = taxationTypeService;
            _productService = productService;
            _product1Service = product1Service;
            _product2Service = product2Service;
            _aliquotService = aliquotService;
            _taxationService = taxationService;
            _companyService = companyService;
            _darService = darService;
            _companyCfopService = companyCfopService;
            _suspensionService = suspensionService;
            _clientService = clientService;
            _ncmConvenioService = ncmConvenioService;
            _productIncentivoService = productIncentivoService;
            _taxService = taxService;
            _grupoService = grupoService;
            _notificationService = notificationService;
            _integrationWsDar = integrationWsDar;
            _configurationService = configurationService;
            _darDocumentService = darDocumentService;
            _serviceEmail = serviceEmail;
            _emailConfiguration = emailConfiguration;
            _emailResponsibleService = emailResponsibleService;
            _taxAnexoService = taxAnexoService;
            _compraAnexoService = compraAnexoService;
            _devoClienteService = devoClienteService;
            _devoFornecedorService = devoFornecedorService;
            _vendaAnexoService = vendaAnexoService;
            _creditBalanceService = creditBalanceService;
            _configuration = configuration;
            _taxProducerService = taxProducerService;
        }

        public IActionResult Index(int noteId)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindByNote(noteId, GetLog(OccorenceLog.Read))
                    .OrderBy(_ => _.Status)
                    .ToList();
                var note = _noteService.FindById(noteId, GetLog(OccorenceLog.Read));

                ViewBag.Note = note;

                return View(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Product(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var product = _service.FindByProduct(id);
                var ncm = _ncmService.FindByCode(product.Ncm);

                string description = "";

                if(product.Ncm.Length == 8)
                {
                    if (ncm == null)
                    {
                        ViewBag.Erro = 1;
                        return View(product);
                    }
                    description = ncm.Description;
                }
                else
                {
                    description = "Sem NCM";
                }
                              
                ViewBag.DescriptionNCM = description;

                List<TaxationType> list_taxation = _taxationTypeService.FindAll(GetLog(OccorenceLog.Read));

                list_taxation.Insert(0, new TaxationType() { Description = "Nennhum item selecionado", Id = 0 });


                SelectList taxationtypes = new SelectList(list_taxation, "Id", "Description", null);
                ViewBag.TaxationTypeId = taxationtypes;

                if (Convert.ToDateTime(product.Note.Dhemi.ToString("dd/MM/yyyy")) < Convert.ToDateTime("10/02/2020"))
                {
                    List<Product> list_product = _productService.FindAllInDate(product.Note.Dhemi);
                    foreach (var prod in list_product)
                    {
                        prod.Description = prod.Code + " - " + prod.Price + " - " + prod.Description;
                    }
                    list_product.Insert(0, new Product() { Description = "Nennhum item selecionado", Id = 0 });
                    SelectList products = new SelectList(list_product, "Id", "Description", null);
                    ViewBag.ProductId = products;
                }
                else if (Convert.ToDateTime(product.Note.Dhemi.ToString("dd/MM/yyyy")) >= Convert.ToDateTime("10/02/2020") &&
                        Convert.ToDateTime(product.Note.Dhemi.ToString("dd/MM/yyyy")) < Convert.ToDateTime("14/09/2020"))
                {
                    List<Product1> list_product1 = _product1Service.FindAllInDate1(product.Note.Dhemi);
                    foreach (var prod in list_product1)
                    {
                        prod.Description = prod.Code + " - " + prod.Price + " - " + prod.Description;
                    }
                    list_product1.Insert(0, new Product1() { Description = "Nennhum item selecionado", Id = 0 });
                    SelectList products1 = new SelectList(list_product1, "Id", "Description", null);
                    ViewBag.ProductId = products1;
                }
                else if (Convert.ToDateTime(product.Note.Dhemi.ToString("dd/MM/yyyy")) >= Convert.ToDateTime("14/09/2020"))
                {
                    List<Product2> list_product2 = _product2Service.FindAllInDate2(product.Note.Dhemi);
                    foreach (var prod in list_product2)
                    {
                        prod.Description = prod.Code + " - " + prod.Price + " - " + prod.Description;
                    }
                    list_product2.Insert(0, new Product2() { Description = "Nennhum item selecionado", Id = 0 });
                    SelectList products2 = new SelectList(list_product2, "Id", "Description", null);
                    ViewBag.ProductId = products2;
                }

                if (product.TaxationTypeId == null)
                {
                    product.TaxationTypeId = 0;
                }
                if (product.ProductId == null)
                {
                    product.ProductId = 0;
                }

                if (product.Product1Id == null)
                {
                    product.Product1Id = 0;
                }

                if (product.Product2Id == null)
                {
                    product.Product2Id = 0;
                }
                return View(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Product(int id, Model.ProductNote entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var prod = _service.FindByProduct(id);
                var calculation = new Taxation.Calculation();

                int taxationType = Convert.ToInt32(entity.TaxationTypeId);

                decimal ? mva = entity.Mva, fecop = entity.Fecop, bcr = entity.BCR, quantPauta = entity.Qpauta, inciso = entity.PercentualInciso;
                decimal aliqInterna = Convert.ToDecimal(entity.AliqInterna), valorAgreg = 0, dif = 0,valorFecop = 0;

                if (bcr != null)
                    bcr = Convert.ToDecimal(bcr);

                DateTime dateStart = Convert.ToDateTime(entity.DateStart);

                var notes = _noteService.FindByUf(Convert.ToInt32(prod.Note.Company.Id), prod.Note.AnoRef, prod.Note.MesRef, prod.Note.Uf);

                var products = _service.FindByNcmUfAliq(notes, prod.Ncm, prod.Picms, prod.Cest);

                var taxedtype = _taxationTypeService.FindById(taxationType, GetLog(OccorenceLog.Read));

                List<Model.ProductNote> updateProducts = new List<Model.ProductNote>();

                if (entity.Pautado == true)
                {
                    Product product = null;
                    Product1 product1 = null;
                    Product2 product2 = null;

                    decimal precoPauta = 0;

                    if (Convert.ToDateTime(prod.Note.Dhemi.ToString("dd/MM/yyyy")) < Convert.ToDateTime("10/02/2020"))
                    {
                        product = _productService.FindById(Convert.ToInt32(entity.ProductId), GetLog(OccorenceLog.Read));
                        precoPauta = Convert.ToDecimal(product.Price);
                    }
                    else if (Convert.ToDateTime(prod.Note.Dhemi.ToString("dd/MM/yyyy")) >= Convert.ToDateTime("10/02/2020") && 
                            Convert.ToDateTime(prod.Note.Dhemi.ToString("dd/MM/yyyy")) < Convert.ToDateTime("14/09/2020"))
                    {
                        product1 = _product1Service.FindById(Convert.ToInt32(entity.Product1Id), GetLog(OccorenceLog.Read));
                        precoPauta = Convert.ToDecimal(product1.Price);
                    }
                    else if (Convert.ToDateTime(prod.Note.Dhemi.ToString("dd/MM/yyyy")) >= Convert.ToDateTime("14/09/2020"))
                    {
                        product2 = _product2Service.FindById(Convert.ToInt32(entity.Product2Id), GetLog(OccorenceLog.Read));
                        precoPauta = Convert.ToDecimal(product2.Price);
                    }

                    decimal baseCalc = 0;
                    decimal valorIcms =calculation.ValorIcms(prod.IcmsCTe, prod.Vicms);

                    decimal Vbasecalc = calculation.BaseCalc(Convert.ToDecimal(prod.Vprod), Convert.ToDecimal(prod.Vfrete), Convert.ToDecimal(prod.Vseg),
                                                             Convert.ToDecimal(prod.Voutro), Convert.ToDecimal(prod.Vdesc), Convert.ToDecimal(prod.Vipi),
                                                             Convert.ToDecimal(prod.Freterateado));


                    if (taxedtype.Type == "ST")
                    {
                        decimal totalIcmsPauta = 0;
                        decimal totalIcms = 0;
                        baseCalc = calculation.BaseCalc(Vbasecalc, prod.Vdesc);
                        decimal quantParaCalc = 0;
                        quantParaCalc = Convert.ToDecimal(prod.Qcom);
                        if (quantPauta != null)
                        {
                            prod.Qpauta = Convert.ToDecimal(quantPauta);
                            quantParaCalc = Convert.ToDecimal(quantPauta);
                        }
                        // Primeiro PP feito pela tabela
                        decimal vAgre = calculation.ValorAgregadoPautaAto(Convert.ToDecimal(quantParaCalc), precoPauta);

                        // Segundo PP feito com os dados do produto
                        decimal vAgre2 = calculation.ValorAgregadoPautaProd(baseCalc, quantParaCalc);

                        if (vAgre2 > vAgre)
                        {
                            vAgre = vAgre2;
                        }

                        if (fecop != null)
                        {
                            prod.Fecop = Convert.ToDecimal(fecop);
                            valorFecop = calculation.ValorFecop(Convert.ToDecimal(fecop), vAgre);
                        }

                        decimal valorAgreAliqInt = calculation.ValorAgregadoAliqInt(aliqInterna, Convert.ToDecimal(fecop), vAgre);
                        decimal icmsPauta = calculation.TotalIcms(valorAgreAliqInt, valorIcms);
                        totalIcmsPauta = calculation.TotalIcmsPauta(icmsPauta, valorFecop);

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

                        if (bcr != null)
                        {
                            valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(bcr), valorAgreg);
                            prod.ValorBCR = valorAgreg;
                            prod.BCR = Convert.ToDecimal(bcr);
                            valorIcms = 0;
                        }
                        else
                        {
                            prod.ValorBCR = null;
                            prod.BCR = null;
                        }

                        if (fecop != null)
                        {
                            prod.Fecop = Convert.ToDecimal(fecop);
                            valorFecop = calculation.ValorFecop(Convert.ToDecimal(fecop), valorAgreg);
                            prod.TotalFecop = valorFecop;
                        }
                        else
                        {
                            prod.Fecop = null;
                            prod.TotalFecop = null;
                        }
                        prod.AliqInterna = aliqInterna;
                        decimal valorAgre_AliqInt = calculation.ValorAgregadoAliqInt(aliqInterna, Convert.ToDecimal(prod.Fecop), valorAgreg);
                        prod.ValorAC = valorAgre_AliqInt;
                        totalIcms = calculation.TotalIcms(valorAgre_AliqInt, valorIcms);

                        //decimal total = Convert.ToDecimal(entity.TotalICMS) + valorFecop;

                        if (totalIcms > totalIcmsPauta)
                            prod.TotalICMS = totalIcms;
                        else
                            prod.TotalICMS = totalIcmsPauta;

                    }

                    prod.Pautado = true;

                    if (product != null)
                    {
                        prod.ProductId = product.Id;

                        if (product.Group.Active.Equals(true))
                            prod.Incentivo = true;
                    }

                    if (product1 != null)
                    {
                        prod.Product1Id = product1.Id;

                        if (product1.Group.Active.Equals(true))
                            prod.Incentivo = true;
                    }

                    if (product2 != null)
                    {
                        prod.Product2Id = product2.Id;

                        if (product2.Group.Active.Equals(true))
                            prod.Incentivo = true;
                    }

                    /*
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

                    if (product2 != null)
                    {
                        if (product2.Group.Active.Equals(true))
                        {
                            prod.Incentivo = true;
                        }
                    }

                    */

                    prod.TaxationTypeId = taxationType;
                    prod.Status = true;
                    prod.Vbasecalc = baseCalc;
                    prod.Incentivo = true;
                    prod.DateStart = dateStart;
                    prod.Produto = "Especial";
                    prod.PercentualInciso = inciso;
                    prod.Updated = DateTime.Now;


                    updateProducts.Add(prod);
                }
                else
                {
                    if (Request.Form["produto"].ToString() == "2")
                    {
                        decimal baseCalc = 0;
                        decimal valorIcms = calculation.ValorIcms(prod.IcmsCTe, prod.Vicms);
                        decimal Vbasecalc = calculation.BaseCalc(Convert.ToDecimal(prod.Vprod), Convert.ToDecimal(prod.Vfrete), Convert.ToDecimal(prod.Vseg),
                                                                 Convert.ToDecimal(prod.Voutro), Convert.ToDecimal(prod.Vdesc), Convert.ToDecimal(prod.Vipi),
                                                                 Convert.ToDecimal(prod.Freterateado));

                        if (taxedtype.Type == "ST")
                        {
                            decimal totalIcms = 0;
                            baseCalc = calculation.BaseCalc(Vbasecalc, prod.Vdesc);

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

                            if (bcr != null)
                            {
                                valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(bcr), valorAgreg);
                                prod.ValorBCR = valorAgreg;
                                prod.BCR = Convert.ToDecimal(bcr);
                                valorIcms = 0;
                            }
                            else
                            {
                                prod.ValorBCR = null;
                                prod.BCR = null;
                            }

                            if (fecop != null)
                            {
                                prod.Fecop = Convert.ToDecimal(fecop);
                                valorFecop = calculation.ValorFecop(Convert.ToDecimal(fecop), valorAgreg);
                                prod.TotalFecop = valorFecop;
                            }
                            else
                            {
                                prod.Fecop = Convert.ToDecimal(0);
                                valorFecop = calculation.ValorFecop(Convert.ToDecimal(0), valorAgreg);
                                prod.TotalFecop = valorFecop;
                            }
                            prod.AliqInterna = aliqInterna;
                            decimal valorAgre_AliqInt = calculation.ValorAgregadoAliqInt(aliqInterna, Convert.ToDecimal(prod.Fecop), valorAgreg);
                            prod.ValorAC = valorAgre_AliqInt;
                            totalIcms = calculation.TotalIcms(valorAgre_AliqInt, valorIcms);
                            decimal total = calculation.TotalIcmsPauta(Convert.ToDecimal(entity.TotalICMS), valorFecop);

                            prod.TotalICMS = totalIcms;

                        }
                        else if (taxedtype.Type == "Normal")
                        {
                            dif = calculation.DiferencialAliq(aliqInterna, prod.Picms);
                            prod.AliqInterna = aliqInterna;
                            baseCalc = Vbasecalc;
                            if (prod.Picms != 4)
                            {
                                var aliqSimples = _aliquotService.FindByUf(prod.Note.Uf);
                                dif = calculation.DiferencialAliq(aliqInterna, aliqSimples.Aliquota);
                                prod.Picms = Convert.ToDecimal(aliqSimples.Aliquota);
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
                            decimal icmsApu = calculation.IcmsApurado(dif, baseCalc);
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
                        else if (taxedtype.Type == "NT")
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

                        prod.TaxationTypeId = taxationType;
                        prod.Status = true;
                        prod.Vbasecalc = baseCalc;
                        prod.ProductId = null;
                        prod.Product1Id = null;
                        prod.Product2Id = null;
                        prod.Pautado = false;
                        prod.DateStart = dateStart;
                        prod.PercentualInciso = inciso;

                        if (prod.Note.Company.Incentive.Equals(true) && prod.Note.Company.AnnexId.Equals(2))
                            prod.Incentivo = false;

                        prod.Qpauta = null;
                        prod.Produto = "Especial";
                        prod.Updated = DateTime.Now;

                        updateProducts.Add(prod);
                    }
                    else
                    {
                        foreach (var item in products)
                        {
                            decimal baseCalc = 0;
                            decimal valorIcms = calculation.ValorIcms(item.IcmsCTe, item.Vicms);
                            decimal Vbasecalc = calculation.BaseCalc(Convert.ToDecimal(item.Vprod), Convert.ToDecimal(item.Vfrete), Convert.ToDecimal(item.Vseg),
                                                                     Convert.ToDecimal(item.Voutro), Convert.ToDecimal(item.Vdesc), Convert.ToDecimal(item.Vipi),
                                                                     Convert.ToDecimal(item.Freterateado));
                            if (taxedtype.Type == "ST")
                            {
                                decimal totalIcms = 0;
                                baseCalc = calculation.BaseCalc(Vbasecalc, item.Vdesc);

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

                                if (bcr != null)
                                {
                                    valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(bcr), valorAgreg);
                                    item.ValorBCR = valorAgreg;
                                    item.BCR = Convert.ToDecimal(bcr);
                                    valorIcms = 0;
                                }
                                else
                                {
                                    item.ValorBCR = null;
                                    item.BCR = null;
                                }

                                if (fecop != null)
                                {
                                    item.Fecop = Convert.ToDecimal(fecop);
                                    valorFecop = calculation.ValorFecop(Convert.ToDecimal(fecop), valorAgreg);
                                    item.TotalFecop = valorFecop;
                                }
                                else
                                {
                                    item.Fecop = Convert.ToDecimal(0);
                                    valorFecop = calculation.ValorFecop(Convert.ToDecimal(0), valorAgreg);
                                    item.TotalFecop = valorFecop;
                                }
                                item.AliqInterna = aliqInterna;
                                decimal valorAgre_AliqInt = calculation.ValorAgregadoAliqInt(aliqInterna, Convert.ToDecimal(item.Fecop), valorAgreg);
                                item.ValorAC = valorAgre_AliqInt;
                                totalIcms = calculation.TotalIcms(valorAgre_AliqInt, valorIcms);
                                item.TotalICMS = totalIcms;

                            }
                            else if (taxedtype.Type == "Normal")
                            {
                                dif = calculation.DiferencialAliq(aliqInterna, item.Picms);
                                item.AliqInterna = aliqInterna;
                                baseCalc = Vbasecalc;
                                if (item.Picms != 4)
                                {
                                    var aliq_simples = _aliquotService.FindByUf(item.Note.Uf);
                                    dif = calculation.DiferencialAliq(aliqInterna, aliq_simples.Aliquota);
                                    item.Picms = Convert.ToDecimal(aliq_simples.Aliquota);
                                }
                                item.Diferencial = dif;
                                decimal icmsApu = calculation.IcmsApurado(dif, baseCalc);
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
                            else if (taxedtype.Type == "NT")
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

                            item.TaxationTypeId = taxationType;
                            item.Status = true;
                            item.Vbasecalc = baseCalc;
                            item.ProductId = null;
                            item.Product1Id = null;
                            item.Product2Id = null;
                            item.Pautado = false;
                            item.DateStart = dateStart;
                            item.PercentualInciso = inciso;

                            if (prod.Note.Company.Incentive.Equals(true) && prod.Note.Company.AnnexId.Equals(2))
                                item.Incentivo = false;

                            item.Qpauta = null;
                            item.Produto = "Normal";
                            item.Updated = DateTime.Now;


                            updateProducts.Add(item);
                        }
                    }
                }

                _service.Update(updateProducts, GetLog(OccorenceLog.Update));

                List<Note> updateNote = new List<Note>();

                foreach (var product in products)
                {
                    bool status = false;
                    var nota = _noteService.FindById(Convert.ToInt32(product.NoteId), GetLog(OccorenceLog.Read));

                    var productTaxation = _service.FindByTaxation(Convert.ToInt32(nota.Id));

                    if (productTaxation.Count == 0)
                        status = true;

                    nota.Status = status;
                    nota.Updated = DateTime.Now;

                    if (nota.Status)
                        updateNote.Add(nota);
                }

                _noteService.Update(updateNote);

                if (Request.Form["produto"].ToString() == "1" && entity.Pautado == false)
                {
                    string aliquot = prod.Picms.ToString();

                    var ncm = _ncmService.FindByCode(prod.Ncm);

                    if (prod.Picms != 4)
                    {
                        var state = _aliquotService.FindByUf(prod.Note.Uf);
                        aliquot = state.Aliquota.ToString();
                    }

                    string code = calculation.Code(prod.Note.Company.Document, prod.Ncm, prod.Note.Uf, aliquot);

                    var taxationcm = _taxationService.FindByNcm(code, prod.Cest);

                    if (taxationcm != null)
                    {
                        taxationcm.DateEnd = dateStart.AddDays(-1);
                        _taxationService.Update(taxationcm, GetLog(OccorenceLog.Update));
                    }

                    Model.Taxation taxation = new Model.Taxation();

                    taxation.CompanyId = Convert.ToInt32(prod.Note.CompanyId);
                    taxation.Code = code;
                    taxation.Cest = prod.Cest;
                    taxation.AliqInterna = aliqInterna;
                    taxation.PercentualInciso = inciso;
                    taxation.Diferencial = dif;
                    taxation.MVA = mva;
                    taxation.BCR = bcr;
                    taxation.Fecop = fecop;
                    taxation.TaxationTypeId = taxationType;
                    taxation.NcmId = ncm == null ? 16427 : ncm.Id;
                    taxation.Picms = prod.Picms;
                    taxation.Uf = prod.Note.Uf;
                    taxation.DateStart = dateStart;
                    taxation.DateEnd = null;
                    taxation.Created = DateTime.Now;
                    taxation.Updated = DateTime.Now;

                    _taxationService.Create(taxation, GetLog(OccorenceLog.Create));

                }       

                return RedirectToAction("Index", new { noteId = prod.NoteId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Delete(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var prod = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                _service.Delete(id, GetLog(Model.OccorenceLog.Delete));

                return RedirectToAction("Index", new { noteId = prod.NoteId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Relatory(int id, string year, string month, Model.TypeTaxation typeTaxation, Model.Type type, string nota)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {

                SessionManager.SetCompanyIdInSession(id);
                SessionManager.SetMonthInSession(month);
                SessionManager.SetYearInSession(year);

                var calculation = new Calculation();
                var importXml = new Xml.Import();
                var importDir = new Diretorio.Import();

                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));

                ViewBag.Company = comp;
                ViewBag.TypeTaxation = typeTaxation.ToString();
                ViewBag.Type = type.ToString();
                ViewBag.PeriodReferenceDarWs = $"{year}{GetIntMonth(month).ToString("00")}";
                
                var confDBSisctNfe = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                string directoryNfe = importDir.Entrada(comp, confDBSisctNfe.Value, year, month);

                List<List<Dictionary<string, string>>> notesEntry = new List<List<Dictionary<string, string>>>();

                if(!type.Equals(Model.Type.IcmsProdutor))
                    notesEntry = importXml.NFeAll(directoryNfe);

                for (int i = notesEntry.Count - 1; i >= 0; i--)
                {
                    if (notesEntry[i][1]["finNFe"] == "4")
                    {
                        notesEntry.RemoveAt(i);
                        continue;
                    }
                    else if (!notesEntry[i][3]["CNPJ"].Equals(comp.Document))
                    {
                        notesEntry.RemoveAt(i);
                        continue;
                    }
                    else if (notesEntry[i][1]["idDest"] == "1" && comp.Status)
                    {
                        if (notesEntry[i][2]["UF"] == notesEntry[i][3]["UF"])
                        {
                            notesEntry.RemoveAt(i);
                            continue;
                        }
                    }

                    var notaImport = _noteService.FindByNote(notesEntry[i][0]["chave"]);

                    if(notaImport == null)
                    {
                        ViewBag.Erro = 5;
                        return View(null);
                    }
                }

                var notes = _noteService.FindByNotes(id, year, month);

                var prodsAll = _service.FindByProductsType(notes, Model.TypeTaxation.Nenhum);

                var products = _service.FindByProductsType(notes, typeTaxation)
                    .OrderBy(_ => _.Note.Iest)
                    .ThenBy(_ => Convert.ToInt32(_.Note.Nnf))
                    .ToList();

                var notasTaxation = products
                    .Select(_ => _.Note)
                    .Distinct()
                    .OrderBy(_ => _.Iest)
                    .ThenBy(_ => Convert.ToInt32(_.Nnf))
                    .ToList();

                var notas = products
                    .Select(_ => Convert.ToInt32(_.NoteId))
                    .Distinct()
                    .ToList();

                var total = _service.FindByTotal(notas);
                var notesS = notes.Where(_ => _.Iest == "").ToList();
                var notesI = notes.Where(_ => _.Iest != "").ToList();

                decimal icmsStnoteSIE = Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum()),
                        icmsStnoteIE = Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum());

                var imp = _taxService.FindByMonth(id, month, year, "Icms");
                var impAnexo = _taxAnexoService.FindByMonth(id, month, year);
                var impProdutor = _taxProducerService.FindByTaxs(id, month, year);

                var importMes = new Period.Month();

                if (type.Equals(Model.Type.Produto) || type.Equals(Model.Type.Nota) || type.Equals(Model.Type.AgrupadoA) ||
                    type.Equals(Model.Type.AgrupadoS) || type.Equals(Model.Type.ProdutoI) || type.Equals(Model.Type.ProdutoNI))
                {                    

                    if (!type.Equals(Model.Type.Nota))
                    {
                        if (prodsAll.Where(_ => _.Status.Equals(false)).ToList().Count() > 0)
                        {
                            ViewBag.Erro = 4;
                            return View(null);
                        }
                    }

                    if (type.Equals(Model.Type.Nota))
                    {
                        notes = notes.Where(_ => _.Nnf.Equals(nota)).ToList();
                        total = notes.Select(_ => _.Vnf).Sum();
                        notesS = notes.Where(_ => _.Iest == "").ToList();
                        notesI = notes.Where(_ => _.Iest != "").ToList();
                        icmsStnoteSIE = _service.FindBySubscription(notesS.ToList(), typeTaxation);
                        icmsStnoteIE = _service.FindBySubscription(notesI.ToList(), typeTaxation);
                        products = _service.FindByProductsType(notes, typeTaxation)
                            .OrderBy(_ => _.Note.Iest)
                            .ThenBy(_ => Convert.ToInt32(_.Note.Nnf))
                            .ToList(); 

                        prodsAll = _service.FindByProductsType(notes, Model.TypeTaxation.Nenhum);

                        ViewBag.NotaNumber = nota;

                        if (prodsAll.Where(_ => _.Status.Equals(false)).ToList().Count() > 0)
                        {
                            ViewBag.Erro = 4;
                            return View(null);
                        }
                    }

                    if (type.Equals(Model.Type.AgrupadoA))
                    {
                        ViewBag.NotasTaxation = notasTaxation;
                        ViewBag.Products = products;
                    }

                    if (type.Equals(Model.Type.AgrupadoS))
                    {
                        List<List<string>> notasAgrup = new List<List<string>>();
                        for (int i = 0; i < notasTaxation.Count; i++)
                        {
                            decimal vProdTotal = Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vprod).Sum() +
                                                              products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vfrete).Sum() +
                                                              products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vseg).Sum() +
                                                              products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Voutro).Sum() -
                                                              products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vdesc).Sum() +
                                                              products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vipi).Sum()),
                                    freteTotal = Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Freterateado).Sum()),
                                    vDesc = Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vdesc).Sum()),
                                    bcIcms = Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Valoragregado).Sum()),
                                    bcr = Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.ValorBCR).Sum()),
                                    vAC = Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.ValorAC).Sum()),
                                    nfecte = Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.Vicms).Sum()) +
                                             Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.IcmsCTe).Sum()),
                                    cte = Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.IcmsCTe).Sum()),
                                    icms = Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.IcmsST).Sum()),
                                    icmsTotal = Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.TotalICMS).Sum()),
                                    fecopTotal = Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.TotalFecop).Sum()),
                                    icmsApurado = Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.IcmsApurado).Sum()),
                                    fecopST = Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.VfcpST).Sum()) +
                                              Convert.ToDecimal(products.Where(_ => _.NoteId.Equals(notasTaxation[i].Id)).Select(_ => _.VfcpSTRet).Sum());

                            List<string> notesAgrup = new List<string>();
                            notesAgrup.Add(notasTaxation[i].Nnf.ToString());
                            notesAgrup.Add(notasTaxation[i].Xnome.ToString());
                            notesAgrup.Add(notasTaxation[i].Dhemi.ToString("dd/MM"));
                            notesAgrup.Add(notasTaxation[i].Vnf.ToString());
                            notesAgrup.Add(vProdTotal.ToString());
                            notesAgrup.Add(notasTaxation[i].Nct.ToString());
                            notesAgrup.Add(freteTotal.ToString());
                            notesAgrup.Add(vDesc.ToString());
                            notesAgrup.Add(bcIcms.ToString());
                            notesAgrup.Add(bcr.ToString());
                            notesAgrup.Add(vAC.ToString());
                            notesAgrup.Add(nfecte.ToString());
                            notesAgrup.Add(cte.ToString());
                            notesAgrup.Add(icms.ToString());
                            notesAgrup.Add(icmsTotal.ToString());
                            notesAgrup.Add(fecopTotal.ToString());
                            notesAgrup.Add(icmsApurado.ToString());
                            notesAgrup.Add(notasTaxation[i].Uf);
                            notesAgrup.Add(fecopST.ToString());
                            notesAgrup.Add(notasTaxation[i].Iest);

                            notasAgrup.Add(notesAgrup);

                        }

                        ViewBag.NotasTaxation = notasAgrup;
                        ViewBag.Registro = notasAgrup.Count();
                    }
                   
                    if (type.Equals(Model.Type.ProdutoI))
                        products = products.Where(_ => _.Incentivo.Equals(true)).ToList();

                    if (type.Equals(Model.Type.ProdutoNI))
                        products = products.Where(_ => _.Incentivo.Equals(false)).ToList();

                    int registro = products.Count();
                    decimal vProd = Convert.ToDecimal(products.Select(_ => _.Vprod).Sum() + Convert.ToDecimal(products.Select(_ => _.Voutro).Sum()) +
                                    Convert.ToDecimal(products.Select(_ => _.Vseg).Sum()) - Convert.ToDecimal(products.Select(_ => _.Vdesc).Sum()) +
                                    Convert.ToDecimal(products.Select(_ => _.Vfrete).Sum()) + Convert.ToDecimal(products.Select(_ => _.Vipi).Sum())),
                            freterateado = Convert.ToDecimal(products.Select(_ => _.Freterateado).Sum()),
                            baseCalculo = Convert.ToDecimal(products.Select(_ => _.Vprod).Sum() + Convert.ToDecimal(products.Select(_ => _.Voutro).Sum()) +
                                              Convert.ToDecimal(products.Select(_ => _.Vseg).Sum()) + Convert.ToDecimal(products.Select(_ => _.Vfrete).Sum()) +
                                              Convert.ToDecimal(products.Select(_ => _.Freterateado).Sum()) + Convert.ToDecimal(products.Select(_ => _.Vipi).Sum())),
                            baseCalcIcms = Convert.ToDecimal(products.Select(_ => _.Valoragregado).Sum()),
                            baseCalcBCR = Convert.ToDecimal(products.Select(_ => _.ValorBCR).Sum()),
                            totalAC = Convert.ToDecimal(products.Select(_ => _.ValorAC).Sum()),
                            totalIcmsNFeCTe = Convert.ToDecimal(products.Select(_ => _.Vicms).Sum()) + Convert.ToDecimal(products.Select(_ => _.IcmsCTe).Sum()),
                            totalGeralIcmsST = Convert.ToDecimal(products.Select(_ => _.IcmsST).Sum()),
                            totalFecopST = Convert.ToDecimal(products.Select(_ => _.VfcpST).Sum()) + Convert.ToDecimal(products.Select(_ => _.VfcpSTRet).Sum()),
                            totalGeralIcms = Convert.ToDecimal(products.Select(_ => _.TotalICMS).Sum()),
                            totalFecop = Convert.ToDecimal(products.Select(_ => _.TotalFecop).Sum()),
                            icmsGeralStIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum()), 2),
                            icmsGeralStSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum()), 2),
                            totalIcmsIE = 0, totalIcmsSIE = 0, totalIcmsFreteIE = 0, gnrePagaIE = 0, gnrePagaSIE = 0, gnreNPagaSIE = 0, gnreNPagaIE = 0,
                            valorDiefIE = 0, valorDiefSIE = 0, totalIcmsPagoIE = 0, totalIcmsPagoSIE = 0, totalIcmsPagarIE = 0, totalIcmsPagarSIE = 0;


                    if (typeTaxation.Equals(Model.TypeTaxation.ST) || typeTaxation.Equals(Model.TypeTaxation.AT))
                    {
                        decimal totalIcmsMvaIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Pautado.Equals(false) && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2),
                                totalIcmsPautaIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Pautado.Equals(true) && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2),
                                totalIcmsMvaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Pautado.Equals(false) && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2),
                                totalIcmsPautaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Pautado.Equals(true) && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2),
                                totalFecop1FreteIE = 0, totalFecop2FreteIE = 0, base1FecopFreteIE = 0, base2FecopFreteIE = 0,
                                totalDarSTCO = 0, totalDarFecop = 0, totalDarIcms = 0, totalDarCotac = 0, totalDarFunef = 0;

                        foreach (var prod in products)
                        {
                            if (!prod.Note.Iest.Equals(""))
                            {
                                if (Convert.ToDecimal(prod.AliqInterna) > 0)
                                {
                                    decimal valorAgreg = 0;

                                    if (prod.Mva != null)
                                        valorAgreg = calculation.ValorAgregadoMva(prod.Freterateado, Convert.ToDecimal(prod.Mva));

                                    if (prod.BCR != null)
                                        valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(prod.BCR), valorAgreg);

                                    if (prod.Fecop != null)
                                    {
                                        if (Convert.ToDecimal(prod.Fecop).Equals(1))
                                        {
                                            base1FecopFreteIE += valorAgreg;
                                            totalFecop1FreteIE += calculation.ValorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                        }
                                        else
                                        {
                                            base2FecopFreteIE += valorAgreg;
                                            totalFecop2FreteIE += calculation.ValorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                        }

                                    }

                                    totalIcmsFreteIE += calculation.ValorAgregadoAliqInt(Convert.ToDecimal(prod.AliqInterna), Convert.ToDecimal(prod.Fecop), valorAgreg) - prod.IcmsCTe;
                                }
                            }
                        }

                       
                        if (typeTaxation.Equals(Model.TypeTaxation.ST))
                        {
                            gnreNPagaSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNSt).Sum()), 2);
                            gnrePagaSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreSt).Sum()), 2);
                            gnreNPagaIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNSt).Sum()), 2);
                            gnrePagaIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreSt).Sum()), 2);
                        }

                        decimal base1SIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2),
                                base1IE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2),
                                base1fecopIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.Fecop1).Sum()), 2),
                                base1fecopSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.Fecop1).Sum()), 2),
                                valorbase1IE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2),
                                valorbase1SIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);

                        base1SIE += Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);
                        base1IE += Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);
                        
                        decimal base2IE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2),
                                base2SIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2),
                                base2fecopIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.Fecop2).Sum()), 2),
                                base2fecopSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.Fecop2).Sum()), 2),
                                valorbase2IE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2),
                                valorbase2SIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);

                        base2IE += Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);
                        base2SIE += Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);

                        decimal totalBaseFecopIE = base1fecopIE + base2fecopIE,
                                totalBaseFecopSIE = base1fecopSIE + base2fecopSIE,
                                baseNfe1NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2),
                                baseNfe1NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2),
                                baseNfe1RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2),
                                baseNfe1RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2),
                                valorNfe1NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                                valorNfe1RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                                valorNfe1NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                                valorNfe1RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                                baseNfe2NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2),
                                baseNfe2RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2),
                                baseNfe2NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2),
                                baseNfe2RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2),
                                valorNfe2NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                                valorNfe2RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),
                                valorNfe2NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                                valorNfe2RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),
                                gnreNPagaFecopIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreFecop).Sum()), 2),
                                gnreNPagaFecopSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreFecop).Sum()), 2),
                                gnrePagaFecop1IE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.FecopGnre1).Sum()), 2),
                                gnrePagaFecop1SIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.FecopGnre1).Sum()), 2),
                                gnrePagaFecop2IE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.FecopGnre2).Sum()), 2),
                                gnrePagaFecop2SIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.FecopGnre2).Sum()), 2),
                                totalGnreFecopIE = gnrePagaFecop1IE + gnrePagaFecop2IE,
                                totalGnreFecopSIE = gnrePagaFecop1SIE + gnrePagaFecop2SIE,
                                totalFecopCalcIE = valorbase1IE + valorbase2IE,
                                totalFecopCalcSIE = valorbase1SIE + valorbase2SIE,
                                totalFecopNfeIE = valorNfe1NormalIE + valorNfe1RetIE + valorNfe2NormalIE + valorNfe2RetIE,
                                totalFecopNfeSIE = valorNfe1NormalSIE + valorNfe1RetSIE + valorNfe2NormalSIE + valorNfe2RetSIE;

                        totalIcmsIE = Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum());
                        totalIcmsSIE = Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum());

                        totalGeralIcms = totalIcmsIE + totalIcmsSIE;

                        // Valores do Icms
                        valorDiefIE = Convert.ToDecimal(totalIcmsIE - icmsGeralStIE - gnrePagaIE + gnreNPagaIE - totalIcmsFreteIE);
                        valorDiefSIE = Convert.ToDecimal(totalIcmsSIE - icmsGeralStSIE - gnrePagaSIE + gnreNPagaSIE + totalIcmsFreteIE);

                        totalIcmsPagoIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsSt).Sum()), 2);
                        totalIcmsPagoSIE = Math.Round(Convert.ToDecimal(notasTaxation.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsSt).Sum()), 2);

                        totalIcmsPagarIE = Math.Round(valorDiefIE - totalIcmsPagoIE, 2);
                        totalIcmsPagarSIE = Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2);

                        // Valores da Fecop
                        decimal difvalor1IE = valorbase1IE - valorNfe1NormalIE - valorNfe1RetIE - gnrePagaFecop1IE - totalFecop1FreteIE - totalFecop2FreteIE,
                                difvalor1SIE = valorbase1SIE - valorNfe1NormalSIE - valorNfe1RetSIE - gnrePagaFecop1SIE + totalFecop1FreteIE + totalFecop2FreteIE,
                                difvalor2IE = valorbase2IE - valorNfe2NormalIE - valorNfe2RetIE - gnrePagaFecop2IE - totalFecop2FreteIE,
                                difvalor2SIE = valorbase2SIE - valorNfe2NormalSIE - valorNfe2RetSIE - gnrePagaFecop2SIE + totalFecop2FreteIE,
                                diftotalIE = difvalor1IE + difvalor2IE, diftotalSIE = difvalor1SIE + difvalor2SIE, totalfecop1IE = difvalor1IE - base1fecopIE,
                                totalfecop1SIE = difvalor1SIE - base1fecopSIE,totalfecop2IE = difvalor2IE - base2fecopIE, totalfecop2SIE = difvalor2SIE - base2fecopSIE;

                        //  Relatorio das Empresas Incentivadas
                        if (comp.Incentive && (comp.AnnexId != null || comp.ChapterId == 4) && typeTaxation.Equals(Model.TypeTaxation.ST))
                        {
                            var productsAll = _service.FindByProductsType(notes, typeTaxation);

                            //Produtos não incentivados
                            var productsNormal = productsAll.Where(_ => _.Incentivo.Equals(false)).ToList();
                            decimal totalIcmsNormalIE = Convert.ToDecimal(productsNormal.Where(_ => _.TaxationType.Type.Equals("ST") && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()),
                                    totalIcmsNormalSIE = Convert.ToDecimal(productsNormal.Where(_ => _.TaxationType.Type.Equals("ST") && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum());

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
                                    if (Convert.ToDecimal(prod.AliqInterna) > 0)
                                    {
                                        decimal valorAgreg = 0;

                                        if (prod.Mva != null)
                                            valorAgreg = calculation.ValorAgregadoMva(prod.Freterateado, Convert.ToDecimal(prod.Mva));
                                        if (prod.BCR != null)
                                            valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(prod.BCR), valorAgreg);

                                        if (prod.Fecop != null)
                                        {
                                            if (Convert.ToDecimal(prod.Fecop).Equals(1))
                                            {
                                                base1FecopFreteIE += valorAgreg;
                                                totalFecop1FreteIE += calculation.ValorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                            }
                                            else
                                            {
                                                base2FecopFreteIE += valorAgreg;
                                                totalFecop2FreteIE += calculation.ValorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                            }
                                        }

                                        totalIcmsFreteIE += calculation.ValorAgregadoAliqInt(Convert.ToDecimal(prod.AliqInterna), Convert.ToDecimal(prod.Fecop), valorAgreg) - prod.IcmsCTe;
                                    }
                                }
                            }

                            icmsGeralStIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum()), 2);
                            icmsGeralStSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum()), 2);


                            gnreNPagaSIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNSt).Distinct().Sum()), 2);
                            gnrePagaSIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreSt).Distinct().Sum()), 2);
                            gnreNPagaIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNSt).Distinct().Sum()), 2);
                            gnrePagaIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreSt).Distinct().Sum()), 2);


                            valorDiefIE = Convert.ToDecimal(totalIcmsNormalIE - icmsGeralStIE - gnrePagaIE + gnreNPagaIE - totalIcmsFreteIE);
                            valorDiefSIE = Convert.ToDecimal(totalIcmsNormalSIE - icmsGeralStSIE - gnrePagaSIE + gnreNPagaSIE + totalIcmsFreteIE);

                            totalIcmsMvaIE = Convert.ToDecimal(productsNormal.Where(_ => _.Pautado.Equals(false) && _.TaxationType.Type.Equals("ST") && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum());
                            totalIcmsPautaIE = Convert.ToDecimal(productsNormal.Where(_ => _.Pautado.Equals(true) && _.TaxationType.Type.Equals("ST") && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum());
                            totalIcmsMvaSIE = Convert.ToDecimal(productsNormal.Where(_ => _.Pautado.Equals(false) && _.TaxationType.Type.Equals("ST") && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum());
                            totalIcmsPautaSIE = Convert.ToDecimal(productsNormal.Where(_ => _.Pautado.Equals(true) && _.TaxationType.Type.Equals("ST") && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum());


                            totalIcmsPagoIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsSt).Sum()), 2);
                            totalIcmsPagoSIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsSt).Sum()), 2);

                            totalIcmsPagarIE = Math.Round(valorDiefIE - totalIcmsPagoIE, 2);
                            totalIcmsPagarSIE = Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2);

                            if (valorDiefSIE - totalIcmsPagoSIE > 0)
                                totalDarSTCO += Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2);

                            if (valorDiefIE - totalIcmsPagoIE > 0)
                                totalDarSTCO += Math.Round(valorDiefIE - totalIcmsPagoIE, 2);

                            base1SIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2);
                            base1SIE += Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);
                            base1IE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2);
                            base1IE += Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);

                            base1fecopIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.Fecop1).Sum()), 2);
                            base1fecopSIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.Fecop1).Sum()), 2);

                            valorbase1IE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                            valorbase1SIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);

                            base2IE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2);
                            base2IE += Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);
                            base2SIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2);
                            base2SIE += Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);

                            base2fecopIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.Fecop2).Sum()), 2);
                            base2fecopSIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.Fecop2).Sum()), 2);

                            valorbase2IE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                            valorbase2SIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);

                            totalBaseFecopIE = base1fecopIE + base2fecopIE;
                            totalBaseFecopSIE = base1fecopSIE + base2fecopSIE;

                            baseNfe1NormalIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                            baseNfe1NormalSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2);
                            baseNfe1RetIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);
                            baseNfe1RetSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2);

                            valorNfe1NormalIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                            valorNfe1RetIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                            valorNfe1NormalSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                            valorNfe1RetSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);

                            baseNfe2NormalIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2);
                            baseNfe2RetIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2);
                            baseNfe2NormalSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2);
                            baseNfe2RetSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2);

                            valorNfe2NormalIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                            valorNfe2RetIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                            valorNfe2NormalSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                            valorNfe2RetSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                            gnreNPagaFecopIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreFecop).Sum()), 2);
                            gnreNPagaFecopSIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreFecop).Sum()), 2);

                            gnrePagaFecop1IE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.FecopGnre1).Sum()), 2);
                            gnrePagaFecop1SIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.FecopGnre1).Sum()), 2);

                            gnrePagaFecop2IE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => !_.Iest.Equals("")).Select(_ => _.FecopGnre2).Sum()), 2);
                            gnrePagaFecop2SIE = Math.Round(Convert.ToDecimal(notasTaxationNormal.Where(_ => _.Iest.Equals("")).Select(_ => _.FecopGnre2).Sum()), 2);

                            totalGnreFecopIE = gnrePagaFecop1IE + gnrePagaFecop2IE;
                            totalGnreFecopSIE = gnrePagaFecop1SIE + gnrePagaFecop2SIE;

                            totalFecopCalcIE = valorbase1IE + valorbase2IE;
                            totalFecopCalcSIE = valorbase1SIE + valorbase2SIE;

                            totalFecopNfeIE = valorNfe1NormalIE + valorNfe1RetIE + valorNfe2NormalIE + valorNfe2RetIE;
                            totalFecopNfeSIE = valorNfe1NormalSIE + valorNfe1RetSIE + valorNfe2NormalSIE + valorNfe2RetSIE;

                            difvalor1IE = valorbase1IE - valorNfe1NormalIE - valorNfe1RetIE - gnrePagaFecop1IE - totalFecop1FreteIE - totalFecop2FreteIE;
                            difvalor1SIE = valorbase1SIE - valorNfe1NormalSIE - valorNfe1RetSIE - gnrePagaFecop1SIE + totalFecop1FreteIE + totalFecop2FreteIE;

                            difvalor2IE = valorbase2IE - valorNfe2NormalIE - valorNfe2RetIE - gnrePagaFecop2IE - totalFecop2FreteIE;
                            difvalor2SIE = valorbase2SIE - valorNfe2NormalSIE - valorNfe2RetSIE - gnrePagaFecop2SIE + totalFecop2FreteIE;

                            diftotalIE = difvalor1IE + difvalor2IE;
                            diftotalSIE = difvalor1SIE + difvalor2SIE;

                            totalfecop1IE = difvalor1IE - base1fecopIE;
                            totalfecop1SIE = difvalor1SIE - base1fecopSIE;

                            totalfecop2IE = difvalor2IE - base2fecopIE;
                            totalfecop2SIE = difvalor2SIE - base2fecopSIE;

                            if (totalfecop1SIE + totalfecop2SIE > 0)
                                totalDarFecop += Math.Round(totalfecop1SIE + totalfecop2SIE, 2);

                            if (totalfecop1IE + totalfecop2IE > 0)
                                totalDarFecop += Math.Round(totalfecop1IE + totalfecop2IE, 2);

                            //Produto incentivados
                            var productsIncentivado = productsAll.Where(_ => _.Incentivo.Equals(true)).ToList();

                            decimal? impostoGeral = 0, totalIcmsNormal = 0, totalFecopNormal = 0, totalIcmsIncentivo = 0, totalFecopIncentivo = 0;

                            if (type.Equals(Model.Type.ProdutoNI))
                            {
                                registro = productsNormal.Count();
                                total = productsNormal.Select(_ => _.Note.Vnf).Distinct().Sum();
                                vProd = Convert.ToDecimal(productsNormal.Select(_ => _.Vprod).Sum() + Convert.ToDecimal(productsNormal.Select(_ => _.Voutro).Sum()) +
                                        Convert.ToDecimal(productsNormal.Select(_ => _.Vseg).Sum()) - Convert.ToDecimal(productsNormal.Select(_ => _.Vdesc).Sum()) +
                                        Convert.ToDecimal(productsNormal.Select(_ => _.Vfrete).Sum()) + Convert.ToDecimal(productsNormal.Select(_ => _.Vipi).Sum()));
                                freterateado = Convert.ToDecimal(productsNormal.Select(_ => _.Freterateado).Sum());
                                baseCalculo = Convert.ToDecimal(productsNormal.Select(_ => _.Vprod).Sum() + productsNormal.Select(_ => _.Voutro).Sum() +
                                                productsNormal.Select(_ => _.Vseg).Sum() + productsNormal.Select(_ => _.Vfrete).Sum() +
                                                productsNormal.Select(_ => _.Freterateado).Sum() + productsNormal.Select(_ => _.Vipi).Sum());
                                baseCalcIcms = Convert.ToDecimal(productsNormal.Select(_ => _.Valoragregado).Sum());
                                baseCalcBCR = Convert.ToDecimal(productsNormal.Select(_ => _.ValorBCR).Sum());
                                totalAC = Convert.ToDecimal(productsNormal.Select(_ => _.ValorAC).Sum());
                                totalIcmsNFeCTe = Convert.ToDecimal(productsNormal.Select(_ => _.Vicms).Sum()) + Convert.ToDecimal(productsNormal.Select(_ => _.IcmsCTe).Sum());
                                totalGeralIcmsST = Convert.ToDecimal(productsNormal.Select(_ => _.IcmsST).Sum());
                                totalFecopST = Convert.ToDecimal(productsNormal.Select(_ => _.VfcpST).Sum()) + Convert.ToDecimal(productsNormal.Select(_ => _.VfcpSTRet).Sum());
                                totalGeralIcms = Convert.ToDecimal(productsNormal.Select(_ => _.TotalICMS).Sum());
                                totalFecop = Convert.ToDecimal(productsNormal.Select(_ => _.TotalFecop).Sum());
                            }

                            if (type.Equals(Model.Type.ProdutoI))
                            {
                                registro = productsIncentivado.Count();
                                total = productsIncentivado.Select(_ => _.Note.Vnf).Distinct().Sum();
                                vProd = Convert.ToDecimal(productsIncentivado.Select(_ => _.Vprod).Sum() + Convert.ToDecimal(productsIncentivado.Select(_ => _.Voutro).Sum()) +
                                        Convert.ToDecimal(productsIncentivado.Select(_ => _.Vseg).Sum()) - Convert.ToDecimal(productsIncentivado.Select(_ => _.Vdesc).Sum()) +
                                        Convert.ToDecimal(productsIncentivado.Select(_ => _.Vfrete).Sum()) + Convert.ToDecimal(productsIncentivado.Select(_ => _.Vipi).Sum()));
                                freterateado = Convert.ToDecimal(productsIncentivado.Select(_ => _.Freterateado).Sum());
                                baseCalculo = Convert.ToDecimal(productsIncentivado.Select(_ => _.Vprod).Sum() + productsIncentivado.Select(_ => _.Voutro).Sum() +
                                                productsIncentivado.Select(_ => _.Vseg).Sum() + productsIncentivado.Select(_ => _.Vfrete).Sum() +
                                                productsIncentivado.Select(_ => _.Freterateado).Sum() + productsIncentivado.Select(_ => _.Vipi).Sum());
                                baseCalcIcms = Convert.ToDecimal(productsIncentivado.Select(_ => _.Valoragregado).Sum());
                                baseCalcBCR = Convert.ToDecimal(productsIncentivado.Select(_ => _.ValorBCR).Sum());
                                totalAC = Convert.ToDecimal(productsIncentivado.Select(_ => _.ValorAC).Sum());
                                totalIcmsNFeCTe = Convert.ToDecimal(productsIncentivado.Select(_ => _.Vicms).Sum()) + Convert.ToDecimal(productsIncentivado.Select(_ => _.IcmsCTe).Sum());
                                totalGeralIcmsST = Convert.ToDecimal(productsIncentivado.Select(_ => _.IcmsST).Sum());
                                totalFecopST = Convert.ToDecimal(productsIncentivado.Select(_ => _.VfcpST).Sum()) + Convert.ToDecimal(productsIncentivado.Select(_ => _.VfcpSTRet).Sum());
                                totalGeralIcms = Convert.ToDecimal(productsIncentivado.Select(_ => _.TotalICMS).Sum());
                                totalFecop = Convert.ToDecimal(productsIncentivado.Select(_ => _.TotalFecop).Sum());
                            }

                            totalIcmsNormal = Convert.ToDecimal(productsNormal.Select(_ => _.TotalICMS).Sum());
                            totalIcmsIncentivo = Convert.ToDecimal(productsIncentivado.Select(_ => _.TotalICMS).Sum());
                            totalFecopNormal = Convert.ToDecimal(productsNormal.Select(_ => _.TotalFecop).Sum());
                            totalFecopIncentivo = Convert.ToDecimal(productsIncentivado.Select(_ => _.TotalFecop).Sum());

                            impostoGeral = totalIcmsNormal + totalIcmsIncentivo + totalFecopNormal + totalFecopIncentivo;

                            ViewBag.Icms = Convert.ToDecimal(comp.Icms);
                            ViewBag.Fecop = Convert.ToDecimal(comp.Fecop);
                            ViewBag.AliqInterna = Convert.ToDecimal(comp.AliqInterna);

                            //  Elencadas
                            ViewBag.IncIInterna = comp.IncIInterna;
                            ViewBag.IncIInterestadual = comp.IncIInterestadual;

                            //  Deselencadas
                            ViewBag.IncIIInterna = comp.IncIIInterna;
                            ViewBag.IncIIInterestadual = comp.IncIIInterestadual;

                            decimal totalImpostoIncentivo = 0, impostoIcms = 0, impostoFecop = 0, baseIcms = 0, icmsAnexoCCCXVI = 0;

                            if (comp.ChapterId.Equals(4))
                            {
                                decimal percentualCaputI = Convert.ToDecimal(productsIncentivado.OrderBy(_ => _.PercentualInciso).Select(_ => _.PercentualInciso).Distinct().FirstOrDefault()),
                                        percentualCaputII = Convert.ToDecimal(productsIncentivado.OrderBy(_ => _.PercentualInciso).Select(_ => _.PercentualInciso).Distinct().LastOrDefault()),
                                        baseCaputI = Convert.ToDecimal(productsIncentivado.Where(_ => _.PercentualInciso.Equals(percentualCaputI)).Select(_ => _.Vprod).Sum() + 
                                                                       productsIncentivado.Where(_ => _.PercentualInciso.Equals(percentualCaputI)).Select(_ => _.Voutro).Sum() +
                                                                       productsIncentivado.Where(_ => _.PercentualInciso.Equals(percentualCaputI)).Select(_ => _.Vseg).Sum() -
                                                                       productsIncentivado.Where(_ => _.PercentualInciso.Equals(percentualCaputI)).Select(_ => _.Vdesc).Sum() +
                                                                       productsIncentivado.Where(_ => _.PercentualInciso.Equals(percentualCaputI)).Select(_ => _.Vfrete).Sum() +
                                                                       productsIncentivado.Where(_ => _.PercentualInciso.Equals(percentualCaputI)).Select(_ => _.Freterateado).Sum() +
                                                                       productsIncentivado.Where(_ => _.PercentualInciso.Equals(percentualCaputI)).Select(_ => _.Vipi).Sum()),
                                        baseCaputII = Convert.ToDecimal(productsIncentivado.Where(_ => _.PercentualInciso.Equals(percentualCaputII)).Select(_ => _.Vprod).Sum() +
                                                                       productsIncentivado.Where(_ => _.PercentualInciso.Equals(percentualCaputII)).Select(_ => _.Voutro).Sum() +
                                                                       productsIncentivado.Where(_ => _.PercentualInciso.Equals(percentualCaputII)).Select(_ => _.Vseg).Sum() -
                                                                       productsIncentivado.Where(_ => _.PercentualInciso.Equals(percentualCaputII)).Select(_ => _.Vdesc).Sum() +
                                                                       productsIncentivado.Where(_ => _.PercentualInciso.Equals(percentualCaputII)).Select(_ => _.Vfrete).Sum() +
                                                                       productsIncentivado.Where(_ => _.PercentualInciso.Equals(percentualCaputII)).Select(_ => _.Freterateado).Sum() +
                                                                       productsIncentivado.Where(_ => _.PercentualInciso.Equals(percentualCaputII)).Select(_ => _.Vipi).Sum()),
                                        impostoIcmsCaputI = Math.Round(Convert.ToDecimal(baseCaputI * percentualCaputI / 100), 2),
                                        impostoIcmsCaputII = Math.Round(Convert.ToDecimal(baseCaputII * percentualCaputII / 100), 2);

                                baseIcms = baseCaputI + baseCaputII;
                                impostoIcms = impostoIcmsCaputI + impostoIcmsCaputII;
                                impostoFecop = Math.Round(Convert.ToDecimal(baseIcms * (comp.Fecop / 100)), 2);

                                ViewBag.PercentualCaputI = percentualCaputI;
                                ViewBag.PercentualCaputII = percentualCaputII;
                                ViewBag.BaseCaputI = baseCaputI;
                                ViewBag.BaseCaputII = baseCaputII;
                                ViewBag.ImpostoIcmsCaputI = impostoIcmsCaputI;
                                ViewBag.ImpostoIcmsCaputII = impostoIcmsCaputII;
                            }

                            if (comp.AnnexId.Equals(3) && !comp.ChapterId.Equals(4))
                            {
                                if (imp == null)
                                {
                                    ViewBag.Erro = 1;
                                    return View(null);
                                }

                                if (comp.SectionId.Equals(2))
                                {
                                    decimal vendasInternasElencadas = Convert.ToDecimal(imp.VendasInternas1), vendasInterestadualElencadas = Convert.ToDecimal(imp.VendasInterestadual1),
                                    vendasInternasDeselencadas = Convert.ToDecimal(imp.VendasInternas2), vendasInterestadualDeselencadas = Convert.ToDecimal(imp.VendasInterestadual2),
                                    InternasElencadas = Convert.ToDecimal(imp.SaidaInterna1), InterestadualElencadas = Convert.ToDecimal(imp.SaidaInterestadual1),
                                    InternasElencadasPortaria = Convert.ToDecimal(imp.SaidaPortInterna1), InterestadualElencadasPortaria = Convert.ToDecimal(imp.SaidaPortInterestadual1),
                                    InternasDeselencadas = Convert.ToDecimal(imp.SaidaInterna2), InterestadualDeselencadas = Convert.ToDecimal(imp.SaidaInterestadual2),
                                    InternasDeselencadasPortaria = Convert.ToDecimal(imp.SaidaPortInterna2),
                                    InterestadualDeselencadasPortaria = Convert.ToDecimal(imp.SaidaPortInterestadual2),
                                    suspensao = Convert.ToDecimal(imp.Suspensao), vendasClienteCredenciado = Convert.ToDecimal(imp.VendasClientes),
                                    vendas = vendasInternasElencadas + vendasInterestadualElencadas + vendasInternasDeselencadas + vendasInterestadualDeselencadas;


                                    //  Elencadas
                                    // Internas
                                    decimal icmsInternaElencada = (InternasElencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                                    decimal fecopInternaElencada = Math.Round((InternasElencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100, 2);
                                    decimal totalInternasElencada = icmsInternaElencada;
                                    decimal icmsPresumidoInternaElencada = (InternasElencadasPortaria * Convert.ToDecimal(comp.IncIInterna)) / 100;
                                    decimal totalIcmsInternaElencada = Math.Round(totalInternasElencada - icmsPresumidoInternaElencada, 2);

                                    totalDarFecop += fecopInternaElencada;
                                    totalDarIcms += totalIcmsInternaElencada;

                                    impostoIcms += Math.Round(totalIcmsInternaElencada, 2);
                                    impostoFecop += fecopInternaElencada;

                                    // Interestadual
                                    decimal icmsInterestadualElencada = (InterestadualElencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                                    decimal fecopInterestadualElencada = Math.Round((InterestadualElencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100, 2);
                                    decimal totalInterestadualElencada = icmsInterestadualElencada;
                                    decimal icmsPresumidoInterestadualElencada = (InterestadualElencadasPortaria * Convert.ToDecimal(comp.IncIInterestadual)) / 100;
                                    decimal totalIcmsInterestadualElencada = Math.Round(totalInterestadualElencada - icmsPresumidoInterestadualElencada, 2);

                                    totalDarFecop += fecopInterestadualElencada;
                                    totalDarIcms += totalIcmsInterestadualElencada;

                                    impostoIcms += Math.Round(totalIcmsInterestadualElencada, 2);
                                    impostoFecop += fecopInterestadualElencada;

                                    //  Deselencadas
                                    //  Internas
                                    decimal icmsInternaDeselencada = (InternasDeselencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                                    decimal fecopInternaDeselencada = Math.Round((InternasDeselencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100, 2);
                                    decimal totalInternasDeselencada = icmsInternaDeselencada;
                                    decimal icmsPresumidoInternaDeselencada = (InternasDeselencadasPortaria * Convert.ToDecimal(comp.IncIIInterna)) / 100;
                                    decimal totalIcmsInternaDeselencada = Math.Round(totalInternasDeselencada - icmsPresumidoInternaDeselencada, 2);

                                    totalDarFecop += fecopInternaDeselencada;
                                    totalDarIcms += totalIcmsInternaDeselencada;

                                    impostoIcms += Math.Round(totalIcmsInternaDeselencada, 2);
                                    impostoFecop += fecopInternaDeselencada;

                                    // Interestadual
                                    decimal icmsInterestadualDeselencada = (InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                                    decimal fecopInterestadualDeselencada = Math.Round((InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100, 2);
                                    decimal totalInterestadualDeselencada = icmsInterestadualDeselencada;
                                    decimal icmsPresumidoInterestadualDeselencada = (InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.IncIIInterestadual)) / 100;
                                    decimal totalIcmsInterestadualDeselencada = Math.Round(totalInterestadualDeselencada - icmsPresumidoInterestadualDeselencada, 2);

                                    totalDarFecop += fecopInterestadualDeselencada;
                                    totalDarIcms += totalIcmsInterestadualDeselencada;

                                    impostoIcms += Math.Round(totalIcmsInterestadualDeselencada, 2);
                                    impostoFecop += fecopInterestadualDeselencada;

                                    //  Percentual
                                    decimal percentualVendas = (vendasClienteCredenciado * 100) / vendas;

                                    var notifi = _notificationService.FindByCurrentMonth(id, month, year);

                                    if (percentualVendas < Convert.ToDecimal(comp.VendaArt781))
                                    {
                                        if (notifi != null)
                                        {
                                            Model.Notification nn = new Notification();
                                            nn.Description = "Venda p/ Cliente Credenciado no Art. 781 menor que " + comp.VendaArt781.ToString() + " %";
                                            nn.Percentual = percentualVendas;
                                            nn.MesRef = month;
                                            nn.AnoRef = year;
                                            nn.CompanyId = id;
                                            nn.Created = DateTime.Now;
                                            nn.Updated = nn.Created;
                                            _notificationService.Create(nn, GetLog(Model.OccorenceLog.Create));
                                        }
                                        else
                                        {
                                            notifi.Percentual = percentualVendas;
                                            notifi.Updated = DateTime.Now;
                                            _notificationService.Update(notifi, GetLog(Model.OccorenceLog.Update));
                                        }
                                    }
                                    else
                                    {
                                        if (notifi != null)
                                        {
                                            _notificationService.Delete(notifi.Id, GetLog(Model.OccorenceLog.Delete));
                                        }
                                    }

                                    //  Suspensão
                                    decimal totalSuspensao = Math.Round((suspensao * Convert.ToDecimal(comp.Suspension)) / 100, 2);
                                    totalDarIcms += totalSuspensao;


                                    //  Elencadas
                                    // Internas
                                    ViewBag.VendasInternasElencadas = vendasInternasElencadas;
                                    ViewBag.InternasElencadas = InternasElencadas;
                                    ViewBag.InternasElencadasPortaria = InternasElencadasPortaria;
                                    ViewBag.IcmsInternasElencadas = icmsInternaElencada;
                                    ViewBag.FecopInternasElencadas = fecopInternaElencada;
                                    ViewBag.TotalInternasElencadas = totalInternasElencada;
                                    ViewBag.IcmsPresumidoInternasElencadas = icmsPresumidoInternaElencada;
                                    ViewBag.TotalIcmsInternasElencadas = totalIcmsInternaElencada;

                                    // Interestadual
                                    ViewBag.VendasInterestadualElencadas = vendasInterestadualElencadas;
                                    ViewBag.InterestadualElencadas = InterestadualElencadas;
                                    ViewBag.InterestadualElencadasPortaria = InterestadualElencadasPortaria;
                                    ViewBag.IcmsInterestadualElencadas = icmsInterestadualElencada;
                                    ViewBag.FecopInterestadualElencadas = fecopInterestadualElencada;
                                    ViewBag.TotalInterestadualElencadas = totalInterestadualElencada;
                                    ViewBag.IcmsPresumidoInterestadualElencadas = icmsPresumidoInterestadualElencada;
                                    ViewBag.TotalIcmsInterestadualElencadas = totalIcmsInterestadualElencada;

                                    //  Deselencadas
                                    //  Internas
                                    ViewBag.VendasInternasDeselencadas = vendasInternasDeselencadas;
                                    ViewBag.InternasDeselencadas = InternasDeselencadas;
                                    ViewBag.InternasDeselencadasPortaria = InternasDeselencadasPortaria;
                                    ViewBag.IcmsInternasElencadas = icmsInternaElencada;
                                    ViewBag.IcmsInternasDeselencadas = icmsInternaDeselencada;
                                    ViewBag.FecopInternasDeselencadas = fecopInternaDeselencada;
                                    ViewBag.TotalInternasDeselencadas = totalInternasDeselencada;
                                    ViewBag.IcmsPresumidoInternasDeselencadas = icmsPresumidoInternaDeselencada;
                                    ViewBag.TotalIcmsInternasDeselencadas = totalIcmsInternaDeselencada;


                                    // Interestadual
                                    ViewBag.VendasInterestadualDeselencadas = vendasInterestadualDeselencadas;
                                    ViewBag.InterestadualDeselencadas = InterestadualDeselencadas;
                                    ViewBag.InterestadualDeselencadasPortaria = InterestadualDeselencadasPortaria;
                                    ViewBag.IcmsInterestadualDeselencadas = icmsInterestadualDeselencada;
                                    ViewBag.FecopInterestadualDeselencadas = fecopInterestadualDeselencada;
                                    ViewBag.TotalInterestadualDeselencadas = totalInterestadualDeselencada;
                                    ViewBag.IcmsPresumidoInterestadualDeselencadas = icmsPresumidoInterestadualDeselencada;
                                    ViewBag.TotalIcmsInterestadualDeselencadas = totalIcmsInterestadualDeselencada;


                                    //  Percentual
                                    ViewBag.PercentualVendas = percentualVendas;

                                    //  Suspensão
                                    ViewBag.PercentualSuspensao = Convert.ToDecimal(comp.Suspension);
                                    ViewBag.Suspensao = suspensao;
                                    ViewBag.TotalSuspensao = totalSuspensao;

                                }

                            }

                            if (comp.AnnexId.Equals(1) && !comp.ChapterId.Equals(4))
                            {
                                if (impAnexo == null)
                                {
                                    ViewBag.Erro = 2;
                                    return View(null);
                                }

                                var produtosAP = _service.FindByProductsType(notes, Model.TypeTaxation.AP);

                                decimal totalFreteAPIE = 0;

                                foreach (var prod in products)
                                {
                                    if (!prod.Note.Iest.Equals("") && prod.TaxationTypeId.Equals(1))
                                        if (Convert.ToDecimal(prod.Diferencial) > 0)
                                            totalFreteAPIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                                }

                                decimal valorNfe1NormalAPSIE = Math.Round(Convert.ToDecimal(produtosAP.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                                        valorNfe1RetAPSIE = Math.Round(Convert.ToDecimal(produtosAP.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                                        valorNfe2NormalAPSIE = Math.Round(Convert.ToDecimal(produtosAP.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                                        valorNfe2RetAPSIE = Math.Round(Convert.ToDecimal(produtosAP.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),
                                        valorNfe1NormalAPIE = Math.Round(Convert.ToDecimal(produtosAP.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                                        valorNfe1RetAPIE = Math.Round(Convert.ToDecimal(produtosAP.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                                        valorNfe2NormalAPIE = Math.Round(Convert.ToDecimal(produtosAP.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                                        valorNfe2RetAPIE = Math.Round(Convert.ToDecimal(produtosAP.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                                decimal? gnrePagaAPIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreAp).Sum()), 2),
                                         gnreNPagaAPIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNAp).Sum()), 2),
                                         gnrePagaAPSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreAp).Sum()), 2),
                                         gnreNPagaAPSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNAp).Sum()), 2),
                                         icmsStAPIE = Math.Round(Convert.ToDecimal(produtosAP.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalAPIE + valorNfe1RetAPIE + valorNfe2NormalAPIE + valorNfe2RetAPIE,
                                         icmsStAPSIE = Math.Round(Convert.ToDecimal(produtosAP.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalAPSIE + valorNfe1RetAPSIE + valorNfe2NormalAPSIE + valorNfe2RetAPSIE,
                                         totalApuradoAPIE = Math.Round(Convert.ToDecimal(produtosAP.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsApurado).Sum()), 2),
                                         totalApuradoAPSIE = Math.Round(Convert.ToDecimal(produtosAP.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsApurado).Sum()), 2),
                                         totalDiefAPSIE = Convert.ToDecimal((totalApuradoAPSIE + totalFreteAPIE) - icmsStAPSIE + gnreNPagaAPSIE - gnrePagaAPSIE),
                                         totalDiefAPIE = Convert.ToDecimal(totalApuradoAPIE + gnreNPagaAPIE - icmsStAPIE - gnrePagaAPIE - totalFreteAPIE);

                                decimal icmsAPnotaIE = Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsAp).Sum()),
                                        icmsAPnotaSIE = Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsAp).Sum());

                                decimal icmsAPagarAPSIE = Convert.ToDecimal(totalDiefAPSIE - icmsAPnotaSIE),
                                        icmsAPagarAPIE = Convert.ToDecimal(totalDiefAPIE - icmsAPnotaIE);

                                decimal icmsAPAPagar = 0;

                                if (icmsAPagarAPSIE > 0)
                                    icmsAPAPagar += icmsAPagarAPSIE;

                                if (icmsAPagarAPIE > 0)
                                    icmsAPAPagar += icmsAPagarAPIE;

                                var vendas = _vendaAnexoService.FindByVendasTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();
                                var devoFornecedors = _devoFornecedorService.FindByDevoTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();
                                var compras = _compraAnexoService.FindByComprasTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();
                                var devoClientes = _devoClienteService.FindByDevoTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();

                                ViewBag.VendasInternas = vendas;
                                ViewBag.DevoFornecedorInternas = devoFornecedors;
                                ViewBag.ComprasInternas = compras;
                                ViewBag.DevoClienteInternas = devoClientes;
                                ViewBag.TaxAnexo = impAnexo;

                                var mesAtual = importMes.NumberMonth(month);
                                var mesAnterior = importMes.NameMonthPrevious(mesAtual);
                                decimal saldoCredorAnterior = 0;

                                string ano = year;

                                if (mesAtual.Equals(1))
                                    ano = (Convert.ToInt32(year) - 1).ToString();

                                var creditLast = _creditBalanceService.FindByLastMonth(id, mesAnterior, ano);

                                if (creditLast != null)
                                    saldoCredorAnterior = Convert.ToDecimal(creditLast.Saldo);

                                //  Total
                                // A
                                decimal icmsTotalA = Convert.ToDecimal(compras.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsCompra4) +
                                                    Convert.ToDecimal(impAnexo.IcmsCompra7) + Convert.ToDecimal(impAnexo.IcmsCompra12);
                                icmsTotalA -= (Convert.ToDecimal(devoFornecedors.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsDevoFornecedor4) +
                                                Convert.ToDecimal(impAnexo.IcmsDevoFornecedor7) + Convert.ToDecimal(impAnexo.IcmsDevoFornecedor12));
                                ViewBag.IcmsTotalA = icmsTotalA;

                                // D
                                decimal icmsTotalD = Convert.ToDecimal(vendas.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsVenda4) +
                                                     Convert.ToDecimal(impAnexo.IcmsVenda7) + Convert.ToDecimal(impAnexo.IcmsVenda12);
                                icmsTotalD -= (Convert.ToDecimal(devoClientes.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsDevoCliente4) +
                                                Convert.ToDecimal(impAnexo.IcmsDevoCliente12));
                                ViewBag.IcmsTotalD = icmsTotalD;


                                // Icms Anexo
                                icmsAnexoCCCXVI = icmsTotalD - icmsTotalA - icmsAPAPagar - saldoCredorAnterior;

                                if (icmsAnexoCCCXVI < 0)
                                    icmsAnexoCCCXVI = 0;

                                impostoGeral += icmsAnexoCCCXVI;

                                baseIcms = Convert.ToDecimal(productsIncentivado.Select(_ => _.Vprod).Sum() + productsIncentivado.Select(_ => _.Voutro).Sum() +
                                                productsIncentivado.Select(_ => _.Vseg).Sum() - productsIncentivado.Select(_ => _.Vdesc).Sum() + productsIncentivado.Select(_ => _.Vfrete).Sum() +
                                                productsIncentivado.Select(_ => _.Freterateado).Sum() + productsIncentivado.Select(_ => _.Vipi).Sum());
                                impostoIcms = Math.Round(Convert.ToDecimal(baseIcms * (comp.Icms / 100)), 2);

                                totalDarIcms += Math.Round(impostoIcms, 2);
                            }

                            if (!comp.AnnexId.Equals(3) && !comp.AnnexId.Equals(1) && !comp.ChapterId.Equals(4))
                            {
                                baseIcms = Convert.ToDecimal(productsIncentivado.Select(_ => _.Vprod).Sum() + productsIncentivado.Select(_ => _.Voutro).Sum() +
                                                productsIncentivado.Select(_ => _.Vseg).Sum() - productsIncentivado.Select(_ => _.Vdesc).Sum() + productsIncentivado.Select(_ => _.Vfrete).Sum() +
                                                productsIncentivado.Select(_ => _.Freterateado).Sum() + productsIncentivado.Select(_ => _.Vipi).Sum());
                                impostoIcms = Math.Round(Convert.ToDecimal(baseIcms * (comp.Icms / 100)), 2);
                                impostoFecop = Math.Round(Convert.ToDecimal(baseIcms * (comp.Fecop / 100)), 2);

                                totalDarFecop += Math.Round(impostoFecop, 2);
                                totalDarIcms += Math.Round(impostoIcms, 2);
                            }


                            decimal basefunef = Convert.ToDecimal(impostoGeral - impostoIcms),
                                    taxaFunef = 0;

                            ViewBag.IcmsAnexoCCCXVI = icmsAnexoCCCXVI;
                            ViewBag.IcmsNormal = totalIcmsNormal;
                            ViewBag.IcmsIncentivo = totalIcmsIncentivo;
                            ViewBag.FecopNormal = totalFecopNormal;
                            ViewBag.FecopIncentivo = totalFecopIncentivo;
                            ViewBag.ImpostoGeral = impostoGeral;

                            ViewBag.Base = baseIcms;
                            ViewBag.ImpostoFecop = impostoFecop;
                            ViewBag.ImpostoIcms = impostoIcms;
                            ViewBag.BaseFunef = Convert.ToDecimal(basefunef);
                            ViewBag.Funef = comp.Funef;
                          
                            if (basefunef > 0)
                                taxaFunef = Convert.ToDecimal(basefunef * (Convert.ToDecimal(comp.Funef) / 100));

                            ViewBag.TaxaFunef = taxaFunef;

                            totalDarFunef += Math.Round(taxaFunef, 2);

                            totalImpostoIncentivo = impostoIcms + impostoFecop + taxaFunef;

                            if (typeTaxation.Equals(Model.TypeTaxation.ST) && !type.Equals(Model.Type.ProdutoI) && !type.Equals(Model.Type.ProdutoNI))
                                ViewBag.TotalImpostoIncentivo = totalImpostoIncentivo + (valorDiefSIE - totalIcmsPagoSIE) + (totalfecop1SIE + totalfecop2SIE);
                            else if (typeTaxation.Equals(Model.TypeTaxation.ST) && type.Equals(Model.Type.ProdutoI))
                                ViewBag.TotalImpostoIncentivo = totalImpostoIncentivo;

                            if (!type.Equals(Model.Type.ProdutoI) && !type.Equals(Model.Type.ProdutoNI) && (comp.AnnexId != 3 || comp.ChapterId == 4) && !type.Equals(Model.Type.Nota))
                            {
                                if (imp == null)
                                {
                                    ViewBag.Erro = 1;
                                    return View(null);
                                }

                                var grupos = _grupoService.FindByGrupos(imp.Id);

                                decimal totalTranferenciaSaida = Convert.ToDecimal(imp.TransferenciaSaida), totalTranferenciaEntrada = Convert.ToDecimal(imp.TransferenciaEntrada),
                                        totalTranferenciaInter = Convert.ToDecimal(imp.TransferenciaInter), totalVendasSuspensao = Convert.ToDecimal(imp.Suspensao),
                                        totalCompra = Convert.ToDecimal(imp.Compras) + totalTranferenciaEntrada, totalDevoCompra = Convert.ToDecimal(imp.DevolucaoCompras),
                                        totalVenda = Convert.ToDecimal(imp.Vendas) + totalTranferenciaSaida, totalDevoVenda = Convert.ToDecimal(imp.DevolucaoVendas),
                                        totalVendaGrupo = Convert.ToDecimal(grupos.Sum(_ => _.Vendas)), totalDevoGrupo = Convert.ToDecimal(grupos.Sum(_ => _.Devolucao)),
                                        totalNcm = Convert.ToDecimal(imp.VendasNcm), totalDevoNcm = Convert.ToDecimal(imp.DevolucaoNcm),
                                        totalIncisoI = Convert.ToDecimal(imp.VendasIncisoI), totalDevoIncisoI = Convert.ToDecimal(imp.DevolucaoVendasIncisoI),
                                        totalIncisoII = Convert.ToDecimal(imp.VendasIncisoII), totalDevoIncisoII = Convert.ToDecimal(imp.DevolucaoVendasIncisoII),
                                        totalNcontribuinte = Convert.ToDecimal(imp.VendasNContribuinte), totalDevoNContribuinte = Convert.ToDecimal(imp.DevolucaoNContribuinte),
                                        totalContribuinte = totalVenda - totalNcontribuinte, totalDevoContribuinte = totalDevoVenda - totalDevoNContribuinte,
                                        baseCalcCompra = totalCompra - totalDevoCompra,
                                        baseCalcVenda = totalVenda - totalDevoVenda,
                                        baseCalcGrupo = totalVendaGrupo - totalDevoGrupo,
                                        baseCalcNcm = totalNcm - totalDevoNcm,
                                        baseCalcIncisoI = totalIncisoI - totalDevoIncisoI,
                                        baseCalcIncisoII = totalIncisoII - totalDevoIncisoII,
                                        baseCalcNContribuinte = totalNcontribuinte - totalDevoNContribuinte,
                                        baseCalcContribuinte = totalContribuinte - totalDevoContribuinte,
                                        limiteGrupo = (baseCalcVenda * Convert.ToDecimal(comp.VendaMGrupo)) / 100,
                                        limiteNcm = (baseCalcVenda * Convert.ToDecimal(comp.VendaAnexo)) / 100,
                                        limiteNContribuinte = (baseCalcVenda * (Convert.ToDecimal(comp.VendaCpf))) / 100,
                                        limiteContribuinte = (baseCalcVenda * (Convert.ToDecimal(comp.VendaContribuinte))) / 100,
                                        limiteTransferencia = (baseCalcVenda * Convert.ToDecimal(comp.Transferencia)) / 100,
                                        limiteTransferenciaInter = (totalTranferenciaEntrada * Convert.ToDecimal(comp.TransferenciaInter)) / 100;


                                if (comp.ChapterId == 4)
                                    limiteNcm = (baseCalcVenda * Convert.ToDecimal(comp.Faturamento)) / 100;

                                decimal excedenteGrupo = 0, impostoGrupo = 0, excedenteNcm = 0, impostoNcm = 0, impostoNContribuinte = 0, excedenteNContribuinte = 0,
                                        impostoContribuinte = 0, excedenteContribuinte = 0, excedenteTranf = 0, impostoTransf = 0, excedenteTranfInter = 0, impostoTransfInter = 0;

                                //  CNPJ
                                List<List<string>> gruposExecentes = new List<List<string>>();

                                if (baseCalcGrupo > limiteGrupo)
                                {
                                    excedenteGrupo = baseCalcGrupo - limiteGrupo;
                                    impostoGrupo = Math.Round((excedenteGrupo * Convert.ToDecimal(comp.VendaMGrupoExcedente)) / 100, 2);
                                }

                                foreach (var g in grupos)
                                {
                                    List<string> grupoExcedente = new List<string>();
                                    grupoExcedente.Add(g.Cnpj);
                                    grupoExcedente.Add(g.Nome);
                                    grupoExcedente.Add(Math.Round(Convert.ToDecimal(g.BaseCalculo), 2).ToString());
                                    grupoExcedente.Add(g.Percentual.ToString());

                                    gruposExecentes.Add(grupoExcedente);
                                }

                                //  Anexo II ou Inciso I e II
                                if (baseCalcNcm < limiteNcm && (comp.AnnexId == 1 || comp.ChapterId == 4))
                                {
                                    excedenteNcm = limiteNcm - baseCalcNcm;

                                    if (comp.AnnexId == 1)
                                        impostoNcm = Math.Round((excedenteNcm * Convert.ToDecimal(comp.VendaAnexoExcedente)) / 100, 2);
                                    else
                                        impostoNcm = Math.Round((excedenteNcm * Convert.ToDecimal(comp.FaturamentoExcedente)) / 100, 2);
                                }

                                //  Contribuinte
                                if (baseCalcContribuinte < limiteContribuinte && comp.ChapterId == 4)
                                {
                                    excedenteContribuinte = baseCalcContribuinte - limiteContribuinte;
                                    impostoContribuinte = Math.Round((excedenteContribuinte * Convert.ToDecimal(comp.VendaContribuinteExcedente)) / 100, 2);
                                }

                                //  Não Contribuinte
                                if (baseCalcNContribuinte > limiteNContribuinte && comp.ChapterId != 4)
                                {
                                    excedenteNContribuinte = baseCalcNContribuinte - limiteNContribuinte;
                                    impostoNContribuinte = Math.Round((excedenteNContribuinte * Convert.ToDecimal(comp.VendaCpfExcedente)) / 100, 2);
                                }

                                //  Transferência
                                if (totalTranferenciaSaida > limiteTransferencia && comp.ChapterId == 4)
                                {
                                    excedenteTranf = totalTranferenciaSaida - limiteTransferencia;
                                    impostoTransf = Math.Round((excedenteTranf * Convert.ToDecimal(comp.TransferenciaExcedente)) / 100, 2);
                                }

                                //  Transferência inter
                                if (totalTranferenciaInter > limiteTransferenciaInter)
                                {
                                    excedenteTranfInter = totalTranferenciaInter - limiteTransferenciaInter;
                                    impostoTransfInter = Math.Round((excedenteTranfInter * Convert.ToDecimal(comp.TransferenciaInterExcedente)) / 100, 2);
                                }

                                //  Suspensão
                                decimal valorSuspensao = Math.Round((totalVendasSuspensao * Convert.ToDecimal(comp.Suspension)) / 100, 2);

                                //  Percentuais
                                decimal percentualVendaContribuinte = (baseCalcContribuinte * 100) / baseCalcVenda,
                                        percentualVendaNContribuinte = (baseCalcNContribuinte * 100) / baseCalcVenda,
                                        percentualVendaNcm = (baseCalcNcm * 100) / baseCalcVenda,
                                        percentualVendaIncisoI = (baseCalcIncisoI * 100) / baseCalcVenda,
                                        percentualVendaIncisoII = (baseCalcIncisoII * 100) / baseCalcVenda,
                                        percentualGrupo = (baseCalcGrupo * 100) / baseCalcVenda;


                                //  Geral
                                ViewBag.TotalVenda = totalVenda;
                                ViewBag.TotalDevo = totalDevoVenda;
                                ViewBag.BaseCalc = baseCalcVenda;

                                //  CNPJ
                                ViewBag.TotalVendaGrupo = totalVendaGrupo;
                                ViewBag.TotalDevoGrupo = totalDevoGrupo;
                                ViewBag.TotalBaseCalcuGrupo = baseCalcGrupo;
                                ViewBag.PercentualVendaGrupo = percentualGrupo;
                                ViewBag.ExcedenteGrupo = excedenteGrupo;
                                ViewBag.TotalExcedenteGrupo = impostoGrupo;
                                ViewBag.LimiteGrupo = limiteGrupo;
                                ViewBag.Grupo = gruposExecentes;

                                //  Anexo II ou Inciso I e II
                                ViewBag.VendaNcm = totalNcm;
                                ViewBag.TotalDevoNcm = totalDevoNcm;
                                ViewBag.BaseCalcNcm = baseCalcNcm;
                                ViewBag.PercentualVendaNcm = percentualVendaNcm;
                                ViewBag.LimiteNcm = limiteNcm;
                                ViewBag.ExcedenteNcm = excedenteNcm;
                                ViewBag.TotalExcedenteNcm = impostoNcm;

                                // Inciso I e II
                                ViewBag.VendasIncisoI = totalIncisoI;
                                ViewBag.VendasIncisoII = totalIncisoII;
                                ViewBag.TotalDevoIncisoI = totalDevoIncisoI;
                                ViewBag.TotalDevoIncisoII = totalDevoIncisoII;
                                ViewBag.BaseCalcIncisoI = baseCalcIncisoI;
                                ViewBag.BaseCalcIncisoII = baseCalcIncisoII;
                                ViewBag.PercentualVendaIncisoI = percentualVendaIncisoI;
                                ViewBag.PercentualVendaIncisoII = percentualVendaIncisoII;

                                //  Contribuinte
                                ViewBag.Contribuinte = totalContribuinte;
                                ViewBag.TotalDevoContribuite = totalDevoContribuinte;
                                ViewBag.BaseCalcContribuinte = baseCalcContribuinte;
                                ViewBag.PercentualVendaContribuinte = percentualVendaContribuinte;
                                ViewBag.LimiteContribuinte = limiteContribuinte;
                                ViewBag.ExcedenteContribuinte = excedenteContribuinte;
                                ViewBag.TotalExcedenteContribuinte = impostoContribuinte;

                                //  Não Contribuinte
                                ViewBag.NContribuinte = totalNcontribuinte;
                                ViewBag.TotalDevoNContribuinte = totalDevoNContribuinte;
                                ViewBag.BaseCalcNContribuinte = baseCalcNContribuinte;
                                ViewBag.PercentualVendaNContribuinte = percentualVendaNContribuinte;
                                ViewBag.LimiteNContribuinte = limiteNContribuinte;
                                ViewBag.ExcedenteNContribuinte = excedenteNContribuinte;
                                ViewBag.TotalExcedenteNContribuinte = impostoNContribuinte;

                                //  Tranferência
                                ViewBag.TotalTransferencia = totalTranferenciaSaida;
                                ViewBag.LimiteTransferencia = limiteTransferencia;
                                ViewBag.ExcedenteTransferencia = excedenteTranf;
                                ViewBag.TotalExcedenteTransferencia = impostoTransf;

                                //  Tranferência Interestadual
                                ViewBag.TotalTransferenciaInter = totalTranferenciaInter;
                                ViewBag.LimiteTransferenciaInter = limiteTransferenciaInter;
                                ViewBag.ExcedenteTransferenciaInter = excedenteTranfInter;
                                ViewBag.TotalExcedenteTransferenciaInter = impostoTransfInter;

                                //  Suspensão
                                ViewBag.BaseCalcSuspensao = totalVendasSuspensao;
                                ViewBag.TotalSuspensao = valorSuspensao;


                                //Total Icms
                                ViewBag.TotalIcmsExcedente = Math.Round(impostoNcm + impostoContribuinte + impostoNContribuinte + impostoTransf + impostoTransfInter + impostoGrupo, 2);

                                totalDarIcms += Math.Round(impostoNcm + impostoContribuinte + impostoNContribuinte + impostoTransf + impostoTransfInter + impostoGrupo + valorSuspensao, 2);
                            }
                        }


                        //      Resumo dos Impostos
                        
                        //  ICMS
                        ViewBag.TotalICMSPautaSIE = totalIcmsPautaSIE;
                        ViewBag.TotalICMSMvaSIE = totalIcmsMvaSIE;
                        ViewBag.TotalICMSPautaIE = totalIcmsPautaIE;
                        ViewBag.TotalICMSMvaIE = totalIcmsMvaIE;
                        ViewBag.TotalICMSSTNota = totalIcmsIE - (totalIcmsPautaSIE + totalIcmsPautaIE);

                        //  FECOP
                        ViewBag.Base1SIE = base1SIE;
                        ViewBag.Base1IE = base1IE;
                        ViewBag.Base2IE = base2IE;
                        ViewBag.Base2SIE = base2SIE;
                        ViewBag.Valorbase1IE = valorbase1IE;
                        ViewBag.Valorbase1SIE = valorbase1SIE;
                        ViewBag.Valorbase2IE = valorbase2IE;
                        ViewBag.Valorbase2SIE = valorbase2SIE;
                        ViewBag.TotalFecopCalculadaIE = totalFecopCalcIE;
                        ViewBag.TotalFecopCalculadaSIE = totalFecopCalcSIE;
                        ViewBag.BaseFecop1FreteIE = base1FecopFreteIE;
                        ViewBag.BaseFecop2FreteIE = base2FecopFreteIE;
                        ViewBag.TotalFecop1FreteIE = totalFecop1FreteIE;
                        ViewBag.TotalFecop2FreteIE = totalFecop2FreteIE;
                        ViewBag.TotalFecopFreteIE = totalFecop1FreteIE + totalFecop2FreteIE;
                        ViewBag.GNRENPagaFecopIE = gnreNPagaFecopIE;
                        ViewBag.GNRENPagaFecopSIE = gnreNPagaFecopSIE;
                        ViewBag.GNREPagaFecop1IE = gnrePagaFecop1IE;
                        ViewBag.GNREPagaFecop1SIE = gnrePagaFecop1SIE;
                        ViewBag.GNREPagaFecop2IE = gnrePagaFecop2IE;
                        ViewBag.GNREPagaFecop2SIE = gnrePagaFecop2SIE;
                        ViewBag.TotalGnreFecopIE = totalGnreFecopIE;
                        ViewBag.TotalGnreFecopSIE = totalGnreFecopSIE;
                        ViewBag.FecopNfe1IE = baseNfe1NormalIE + baseNfe1RetIE;
                        ViewBag.FecopNfe1SIE = baseNfe1NormalSIE + baseNfe1RetSIE;
                        ViewBag.FecopNfe2IE = baseNfe2NormalIE + baseNfe2RetIE;
                        ViewBag.FecopNfe2SIE = baseNfe2NormalSIE + baseNfe2RetSIE;
                        ViewBag.ValorNfe1IE = valorNfe1NormalIE + valorNfe1RetIE;
                        ViewBag.ValorNfe1SIE = valorNfe1NormalSIE + valorNfe1RetSIE;
                        ViewBag.ValorNfe2IE = valorNfe2NormalIE + valorNfe2RetIE;
                        ViewBag.ValorNfe2SIE = valorNfe2NormalSIE + valorNfe2RetSIE;
                        ViewBag.TotalFecopNfeIE = totalFecopNfeIE;
                        ViewBag.TotalFecopNfeSIE = totalFecopNfeSIE;
                        ViewBag.DiefBase1IE = base1IE - baseNfe1NormalIE - baseNfe1RetIE - base1FecopFreteIE;
                        ViewBag.DiefBase1SIE = base1SIE - baseNfe1NormalSIE - baseNfe1RetSIE + base1FecopFreteIE;
                        ViewBag.DiefBase2IE = base2IE - baseNfe2NormalIE - baseNfe2RetIE - base2FecopFreteIE;
                        ViewBag.DiefBase2SIE = base2SIE - baseNfe2NormalSIE - baseNfe2RetSIE + base2FecopFreteIE;
                        ViewBag.DiefValor1IE = difvalor1IE;
                        ViewBag.DiefValor1SIE = difvalor1SIE;
                        ViewBag.DiefValor2IE = difvalor2IE;
                        ViewBag.DiefValor2SIE = difvalor2SIE;
                        ViewBag.DiefTotalIE = diftotalIE;
                        ViewBag.DiefTotalSIE = diftotalSIE;
                        ViewBag.Base1FecopPagoIE = base1fecopIE;
                        ViewBag.Base1FecopPagoSIE = base1fecopSIE;
                        ViewBag.Base2FecopPagoIE = base2fecopIE;
                        ViewBag.Base2FecopPagoSIE = base2fecopSIE;
                        ViewBag.TotalBaseFecopPagoIE = totalBaseFecopIE;
                        ViewBag.TotalBaseFecopPagoSIE = totalBaseFecopSIE;
                        ViewBag.TotalFecop1IE = totalfecop1IE;
                        ViewBag.TotalFecop1SIE = totalfecop1SIE;
                        ViewBag.TotalFecop2IE = totalfecop2IE;
                        ViewBag.TotalFecop2SIE = totalfecop2IE;
                        ViewBag.TotalFinalFecopCalculadaIE = Math.Round(totalfecop1IE + totalfecop2IE, 2);
                        ViewBag.TotalFinalFecopCalculadaSIE = Math.Round(totalfecop1SIE + totalfecop2SIE, 2);


                        //  Valores total DAR
                        ViewBag.TotalDarSTCO = totalDarSTCO;
                        ViewBag.TotalDarFecop = totalDarFecop;
                        ViewBag.TotalDarFunef = totalDarFunef;
                        ViewBag.TotalDarIcms = totalDarIcms;
                        ViewBag.TotalDarCotac = totalDarCotac;

                    }
                    else if (typeTaxation.Equals(Model.TypeTaxation.AP) || typeTaxation.Equals(Model.TypeTaxation.CO) ||
                            typeTaxation.Equals(Model.TypeTaxation.COR) || typeTaxation.Equals(Model.TypeTaxation.IM))
                    {
                            baseCalculo = Convert.ToDecimal(products.Select(_ => _.Vprod).Sum() + products.Select(_ => _.Voutro).Sum() +
                                          Convert.ToDecimal(products.Select(_ => _.Vseg).Sum()) - Convert.ToDecimal(products.Select(_ => _.Vdesc).Sum()) +
                                          Convert.ToDecimal(products.Select(_ => _.Vfrete).Sum()) + Convert.ToDecimal(products.Select(_ => _.Freterateado).Sum()) +
                                          Convert.ToDecimal(products.Select(_ => _.Vipi).Sum()));


                            foreach (var prod in products)
                            {
                                if (!prod.Note.Iest.Equals(""))
                                    if (Convert.ToDecimal(prod.Diferencial) > 0)
                                        totalIcmsFreteIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                            }

                            totalIcmsIE = Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.IcmsApurado).Sum());
                            totalIcmsSIE = Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.IcmsApurado).Sum());

                            decimal valorNfe1NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                                    valorNfe1RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                                    valorNfe2NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                                    valorNfe2RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),
                                    valorNfe1NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                                    valorNfe1RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                                    valorNfe2NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                                    valorNfe2RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                            
                            icmsStnoteSIE += valorNfe1NormalSIE + valorNfe1RetSIE + valorNfe2NormalSIE + valorNfe2RetSIE;
                            icmsStnoteIE += valorNfe1NormalIE + valorNfe1RetIE + valorNfe2NormalIE + valorNfe2RetIE;

                            totalGeralIcmsST = icmsStnoteSIE + icmsStnoteIE;

                            if (typeTaxation.Equals(Model.TypeTaxation.AP))
                            {
                                gnrePagaIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreAp).Distinct().Sum()), 2);
                                gnreNPagaIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreNAp).Distinct().Sum()), 2);
                                gnrePagaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreAp).Distinct().Sum()), 2);
                                gnreNPagaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreNAp).Distinct().Sum()), 2);
                                totalIcmsPagoIE = Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.IcmsAp).Distinct().Sum());
                                totalIcmsPagoSIE = Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.IcmsAp).Distinct().Sum());
                            }
                            else if (typeTaxation.Equals(Model.TypeTaxation.CO))
                            {
                                gnrePagaIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreCo).Distinct().Sum()), 2);
                                gnreNPagaIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreNCo).Distinct().Sum()), 2);
                                gnrePagaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreCo).Distinct().Sum()), 2);
                                gnreNPagaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreNCo).Distinct().Sum()), 2);
                                totalIcmsPagoIE = Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.IcmsCo).Distinct().Sum());
                                totalIcmsPagoSIE = Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.IcmsCo).Distinct().Sum());

                            }
                            else if (typeTaxation.Equals(Model.TypeTaxation.IM))
                            {
                                gnrePagaIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreIm).Distinct().Sum()), 2);
                                gnreNPagaIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.GnreNIm).Distinct().Sum()), 2);
                                gnrePagaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreIm).Distinct().Sum()), 2);
                                gnreNPagaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.GnreNIm).Distinct().Sum()), 2);
                                totalIcmsPagoIE = Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.Note.IcmsIm).Distinct().Sum());
                                totalIcmsPagoSIE = Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.Note.IcmsIm).Distinct().Sum());
                            }

                            totalGeralIcms = totalIcmsIE + totalIcmsSIE;

                            valorDiefIE = Convert.ToDecimal(totalIcmsIE - icmsStnoteIE - gnrePagaIE + gnreNPagaIE - totalIcmsFreteIE);
                            valorDiefSIE = Convert.ToDecimal((totalIcmsSIE + totalIcmsFreteIE) - icmsStnoteSIE - gnrePagaSIE + gnreNPagaSIE);

                            icmsGeralStIE = icmsStnoteIE;
                            icmsGeralStSIE = icmsStnoteSIE;

                            totalIcmsPagarIE = Math.Round(valorDiefIE - totalIcmsPagoIE, 2);
                            totalIcmsPagarSIE = Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2);
                         }

                    // Resumo dos produtos
                    ViewBag.Notes = notes;
                    ViewBag.Registro = registro;
                    ViewBag.TotalNotas = total;
                    ViewBag.ValorProd = vProd;
                    ViewBag.TotalFrete = freterateado;
                    ViewBag.TotalBC = baseCalculo;
                    ViewBag.TotalBCICMS = baseCalcIcms;
                    ViewBag.TotalBCR = baseCalcBCR;
                    ViewBag.TotalAC = totalAC;
                    ViewBag.TotalICMSNfeCte = totalIcmsNFeCTe;
                    ViewBag.TotalICMSST = totalGeralIcmsST;
                    ViewBag.FecopST = totalFecopST;
                    ViewBag.TotalICMSGeral = totalGeralIcms;
                    ViewBag.TotalICMS = totalGeralIcms;
                    ViewBag.TotalFecop = totalFecop;


                    //      Resumo dos Impostos

                    //  ICMS
                    ViewBag.TotalICMSIE = totalIcmsIE;
                    ViewBag.TotalICMSSIE = totalIcmsSIE;
                    ViewBag.TotalIcmsFreteIE = totalIcmsFreteIE;
                    ViewBag.TotalGNREPagaIE = gnrePagaIE;
                    ViewBag.TotalGNREPagaSIE = gnrePagaSIE;
                    ViewBag.TotalGNREnPagaIE = gnreNPagaIE;
                    ViewBag.TotalGNREnPagaSIE = gnreNPagaSIE;
                    ViewBag.IcmsGeralSTIE = icmsGeralStIE;
                    ViewBag.IcmsGeralSTSIE = icmsGeralStSIE;
                    ViewBag.ValorDiefIE = valorDiefIE;
                    ViewBag.ValorDiefSIE = valorDiefSIE;
                    ViewBag.IcmsPagoIE = totalIcmsPagoIE;
                    ViewBag.IcmsPagoSIE = totalIcmsPagoSIE;
                    ViewBag.IcmsPagarIE = totalIcmsPagarIE;
                    ViewBag.IcmsPagarSIE = totalIcmsPagarSIE;
                  
                }
                else if (type.Equals(Model.Type.ProdutoFP))
                {
                    products = _service.FindByProductsType(notes, Model.TypeTaxation.Nenhum)
                        .Where(_ => _.Pautado.Equals(false))
                        .ToList();
                }
                else if (type.Equals(Model.Type.Geral))
                {
                    if (prodsAll.Where(_ => _.Status.Equals(false)).ToList().Count() > 0)
                    {
                        ViewBag.Erro = 4;
                        return View(null);
                    }

                    decimal totalApuradoST = 0, totalApuradoCO = 0, totalApuradoFecop = 0, totalApuradoAP = 0, totalApuradoIM = 0,
                        totalRecolhidoST = 0, totalRecolhidoAP = 0, totalRecolhidoCO = 0, totalRecolhidoFecop = 0, totalRecolhidoIM = 0,
                        totalDarST = 0, totalDarFecop = 0, totalDarAp = 0, totalDarIm = 0, totalDarCO = 0,
                        totalDarIcms = 0, totalDarCotac = 0, totalDarFunef = 0;

                    products = prodsAll;

                    // Antecipação Parcial
                    decimal totalFreteAPIE = 0;

                    foreach (var prod in products)
                    {
                        if (!prod.Note.Iest.Equals("") && prod.TaxationTypeId.Equals(1))
                            if (Convert.ToDecimal(prod.Diferencial) > 0)
                                totalFreteAPIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                    }

                    ViewBag.TotalFreteAPIE = totalFreteAPIE;

                    decimal valorNfe1NormalAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe1RetAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe2NormalAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe2RetAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe1NormalAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe1RetAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe2NormalAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe2RetAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(1) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal? gnrePagaAPIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreAp).Sum()), 2),
                             gnreNPagaAPIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNAp).Sum()), 2),
                             gnrePagaAPSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreAp).Sum()), 2),
                             gnreNPagaAPSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNAp).Sum()), 2),
                             icmsStAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalAPIE + valorNfe1RetAPIE + valorNfe2NormalAPIE + valorNfe2RetAPIE,
                             icmsStAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalAPSIE + valorNfe1RetAPSIE + valorNfe2NormalAPSIE + valorNfe2RetAPSIE,
                             totalApuradoAPIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsApurado).Sum()), 2),
                             totalApuradoAPSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(1)).Select(_ => _.IcmsApurado).Sum()), 2),
                             totalDiefAPSIE = Convert.ToDecimal((totalApuradoAPSIE + totalFreteAPIE) - icmsStAPSIE + gnreNPagaAPSIE - gnrePagaAPSIE),
                             totalDiefAPIE = Convert.ToDecimal(totalApuradoAPIE + gnreNPagaAPIE - icmsStAPIE - gnrePagaAPIE - totalFreteAPIE);

                    ViewBag.TotatlApuradoAPIE = totalApuradoAPIE;
                    ViewBag.TotatlApuradoAPSIE = totalApuradoAPSIE;
                    ViewBag.TotalIcmsPagoAPIE = icmsStAPIE;
                    ViewBag.TotalIcmsPagoAPSIE = icmsStAPSIE;

                    ViewBag.GnrePagaAPSIE = gnrePagaAPSIE;
                    ViewBag.GnrePagaAPIE = gnrePagaAPIE;
                    ViewBag.GnreNPagaAPSIE = gnreNPagaAPSIE;
                    ViewBag.GnreNPagaAPIE = gnreNPagaAPIE;

                    ViewBag.TotalDiefAPSIE = totalDiefAPSIE;
                    ViewBag.TotalDiefAPIE = totalDiefAPIE;

                    decimal icmsAPnotaIE = Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsAp).Sum()),
                            icmsAPnotaSIE = Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsAp).Sum());
                    ViewBag.IcmsAPIE = icmsAPnotaIE;
                    ViewBag.IcmsAPSIE = icmsAPnotaSIE;

                    decimal icmsAPagarAPSIE = Convert.ToDecimal(totalDiefAPSIE - icmsAPnotaSIE),
                            icmsAPagarAPIE = Convert.ToDecimal(totalDiefAPIE - icmsAPnotaIE);
                    ViewBag.IcmsAPagarAPSIE = icmsAPagarAPSIE;
                    ViewBag.IcmsAPagarAPIE = icmsAPagarAPIE;

                    if (Convert.ToDecimal(totalDiefAPSIE) > 0)
                        totalApuradoAP += Math.Round(Convert.ToDecimal(totalDiefAPSIE), 2);

                    if (Convert.ToDecimal(totalDiefAPIE) > 0)
                        totalApuradoAP += Math.Round(Convert.ToDecimal(totalDiefAPIE), 2);

                    totalRecolhidoAP += Math.Round(icmsAPnotaSIE + icmsAPnotaIE, 2);

                    if (icmsAPagarAPSIE > 0)
                        totalDarAp += Math.Round(icmsAPagarAPSIE, 2);

                    if (icmsAPagarAPIE > 0)
                        totalDarAp += Math.Round(icmsAPagarAPIE, 2);

                    // Substituição Tributária

                    decimal totalIcmsFreteSTIE = 0, totalFecop1FreteSTIE = 0, totalFecop2FreteSTIE = 0;

                    foreach (var prod in products)
                    {
                        if (!prod.Note.Iest.Equals("") && (prod.TaxationTypeId.Equals(5) || prod.TaxationTypeId.Equals(6)))
                        {
                            if (Convert.ToDecimal(prod.AliqInterna) > 0)
                            {
                                decimal valorAgreg = 0;
                                if (prod.Mva != null)
                                    valorAgreg = calculation.ValorAgregadoMva(prod.Freterateado, Convert.ToDecimal(prod.Mva));
                                if (prod.BCR != null)
                                    valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(prod.BCR), valorAgreg);
                                if (prod.Fecop != null)
                                {
                                    if (Convert.ToDecimal(prod.Fecop).Equals(1))
                                        totalFecop1FreteSTIE += calculation.ValorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                    else
                                        totalFecop2FreteSTIE += calculation.ValorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);

                                }

                                totalIcmsFreteSTIE += calculation.ValorAgregadoAliqInt(Convert.ToDecimal(prod.AliqInterna), Convert.ToDecimal(prod.Fecop), valorAgreg) - prod.IcmsCTe;
                            }
                        }
                    }

                    decimal? gnrePagaSTIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreSt).Sum()), 2),
                            gnreNPagaSTIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNSt).Sum()), 2),
                            gnrePagaSTSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreSt).Sum()), 2),
                            gnreNPagaSTSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNSt).Sum()), 2),
                            icmsStSTIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.IcmsST).Sum()), 2),
                            icmsStSTSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.IcmsST).Sum()), 2),
                            totalApuradoSTIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.TotalICMS).Sum()), 2),
                            totalApuradoSTSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.TotalICMS).Sum()), 2),
                            totalDiefSTSIE = (totalApuradoSTSIE + totalIcmsFreteSTIE) - icmsStSTSIE + gnreNPagaSTSIE - gnrePagaSTSIE,
                            totalDiefSTIE = totalApuradoSTIE - icmsStSTIE + gnreNPagaSTIE - gnrePagaSTIE - totalIcmsFreteSTIE,
                            icmsSTPagoIE = Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsSt).Sum()),
                            icmsSTPagoSIE = Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsSt).Sum());


                    decimal icmsAPagarSTSIE = Math.Round(Convert.ToDecimal(totalDiefSTSIE - icmsSTPagoSIE), 2),
                            icmsAPagarSTIE = Math.Round(Convert.ToDecimal(totalDiefSTIE - icmsSTPagoIE), 2),
                            valorbase1STIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2),
                            valorbase1STSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2),
                            valorbase2STIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2),
                            valorbase2STSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2),
                            totalFecopCalcSTIE = valorbase1STIE + valorbase2STIE, totalFecopCalcSTSIE = valorbase1STSIE + valorbase2STSIE;

                    decimal valorNfe1NormalSTIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) &&  _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe1RetSTIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe1NormalSTSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe1RetSTSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal valorNfe2NormalSTIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe2RetSTIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe2NormalSTSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe2RetSTSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),
                            totalFecopNfeSTIE = valorNfe1NormalSTIE + valorNfe1RetSTIE + valorNfe2NormalSTIE + valorNfe2RetSTIE,
                            totalFecopNfeSTSIE = valorNfe1NormalSTSIE + valorNfe1RetSTSIE + valorNfe2NormalSTSIE + valorNfe2RetSTSIE;

                    decimal gnreNPagaFecopSTIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2),
                            gnreNPagaFecopSTSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2),
                            gnrePagaFecop1STIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2),
                            gnrePagaFecop1STSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2),
                            gnrePagaFecop2STIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2),
                            gnrePagaFecop2STSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2),
                            totalGnreFecopSTIE = gnrePagaFecop1STIE + gnrePagaFecop2STIE, totalGnreFecopSTSIE = gnrePagaFecop1STSIE + gnrePagaFecop2STSIE,
                            totalfecopDiefSTIE = totalFecopCalcSTIE - totalGnreFecopSTIE + gnreNPagaFecopSTIE - totalFecopNfeSTIE - totalFecop1FreteSTIE - totalFecop2FreteSTIE,
                            totalfecopDiefSTSIE = totalFecopCalcSTSIE - totalGnreFecopSTSIE + gnreNPagaFecopSTSIE - totalFecopNfeSTSIE + totalFecop1FreteSTIE + totalFecop2FreteSTIE;


                    decimal? icmsFecop1STIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2),
                             icmsFecop1STSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2),
                             icmsFecop2STIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2),
                             icmsFecop2STSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2);


                    //Incentivo
                    if (comp.Incentive && (!comp.AnnexId.Equals(null) || comp.ChapterId.Equals(4)))
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
                                if (Convert.ToDecimal(prod.AliqInterna) > 0)
                                {
                                    decimal valorAgreg = 0;
                                    if (prod.Mva != null)
                                        valorAgreg = calculation.ValorAgregadoMva(prod.Freterateado, Convert.ToDecimal(prod.Mva));
                                    if (prod.BCR != null)
                                        valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(prod.BCR), valorAgreg);
                                    if (prod.Fecop != null)
                                    {
                                        if (Convert.ToDecimal(prod.Fecop).Equals(1))
                                            totalFecop1FreteSTIE += calculation.ValorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                        else
                                            totalFecop2FreteSTIE += calculation.ValorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);

                                    }

                                    totalIcmsFreteSTIE += calculation.ValorAgregadoAliqInt(Convert.ToDecimal(prod.AliqInterna), Convert.ToDecimal(prod.Fecop), valorAgreg) - prod.IcmsCTe;
                                }
                            }
                        }

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

                        icmsSTPagoIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.IcmsSt).Distinct().Sum()), 2);
                        icmsSTPagoSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.IcmsSt).Distinct().Sum()), 2);

                        icmsAPagarSTSIE = Convert.ToDecimal(totalDiefSTSIE - icmsSTPagoSIE);
                        icmsAPagarSTIE = Convert.ToDecimal(totalDiefSTIE - icmsSTPagoIE);

                        valorbase1STIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                        valorbase1STSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2);
                        valorbase2STIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);
                        valorbase2STSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2);

                        totalFecopCalcSTIE = valorbase1STIE + valorbase2STIE;
                        totalFecopCalcSTSIE = valorbase1STSIE + valorbase2STSIE;

                        valorNfe1NormalSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        valorNfe1RetSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);
                        valorNfe1NormalSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2);
                        valorNfe1RetSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2);

                        valorNfe2NormalSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        valorNfe2RetSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);
                        valorNfe2NormalSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2);
                        valorNfe2RetSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                        totalFecopNfeSTIE = valorNfe1NormalSTIE + valorNfe1RetSTIE + valorNfe2NormalSTIE + valorNfe2RetSTIE;
                        totalFecopNfeSTSIE = valorNfe1NormalSTSIE + valorNfe1RetSTSIE + valorNfe2NormalSTSIE + valorNfe2RetSTSIE;

                        gnreNPagaFecopSTIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2);
                        gnreNPagaFecopSTSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2);


                        gnrePagaFecop1STIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2);
                        gnrePagaFecop1STSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2);
                        gnrePagaFecop2STIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2);
                        gnrePagaFecop2STSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2);

                        totalGnreFecopSTIE = gnrePagaFecop1STIE + gnrePagaFecop2STIE;
                        totalGnreFecopSTSIE = gnrePagaFecop1STSIE + gnrePagaFecop2STSIE;

                        totalfecopDiefSTIE = totalFecopCalcSTIE - totalGnreFecopSTIE + gnreNPagaFecopSTIE - totalFecopNfeSTIE - totalFecop1FreteSTIE - totalFecop2FreteSTIE;
                        totalfecopDiefSTSIE = totalFecopCalcSTSIE - totalGnreFecopSTSIE + gnreNPagaFecopSTSIE - totalFecopNfeSTSIE + totalFecop1FreteSTIE + totalFecop2FreteSTIE;

                        icmsFecop1STIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2);
                        icmsFecop1STSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2);
                        icmsFecop2STIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => !_.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2);
                        icmsFecop2STSIE = Math.Round(Convert.ToDecimal(productsNormal.Where(_ => _.Note.Iest.Equals("") && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2);

                        //Produto incentivados
                        var productsIncentivado = _service.FindByIncentive(notes);
                        productsIncentivado = productsIncentivado.Where(_ => _.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)).ToList();

                        ViewBag.Icms = Convert.ToDecimal(comp.Icms);
                        ViewBag.Fecop = Convert.ToDecimal(comp.Fecop);

                        decimal baseIcms = 0,impostoIcms = 0, impostoFecop = 0, icmsAnexoCCCXVI = 0;

                        if (imp == null)
                        {
                            ViewBag.Erro = 1;
                            return View(null);
                        }

                        if (comp.ChapterId.Equals(4))
                        {
                            decimal percentualCaputI = Convert.ToDecimal(productsIncentivado.OrderBy(_ => _.PercentualInciso).Select(_ => _.PercentualInciso).Distinct().FirstOrDefault()),
                                    percentualCaputII = Convert.ToDecimal(productsIncentivado.OrderBy(_ => _.PercentualInciso).Select(_ => _.PercentualInciso).Distinct().LastOrDefault()),
                                    baseCaputI = Convert.ToDecimal(productsIncentivado.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.PercentualInciso.Equals(percentualCaputI)).Select(_ => _.Vprod).Sum() +
                                                                   productsIncentivado.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.PercentualInciso.Equals(percentualCaputI)).Select(_ => _.Voutro).Sum() +
                                                                   productsIncentivado.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.PercentualInciso.Equals(percentualCaputI)).Select(_ => _.Vseg).Sum() -
                                                                   productsIncentivado.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.PercentualInciso.Equals(percentualCaputI)).Select(_ => _.Vdesc).Sum() +
                                                                   productsIncentivado.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.PercentualInciso.Equals(percentualCaputI)).Select(_ => _.Vfrete).Sum() +
                                                                   productsIncentivado.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.PercentualInciso.Equals(percentualCaputI)).Select(_ => _.Freterateado).Sum() +
                                                                   productsIncentivado.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.PercentualInciso.Equals(percentualCaputI)).Select(_ => _.Vipi).Sum()),
                                    baseCaputII = Convert.ToDecimal(productsIncentivado.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.PercentualInciso.Equals(percentualCaputII)).Select(_ => _.Vprod).Sum() +
                                                                   productsIncentivado.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.PercentualInciso.Equals(percentualCaputII)).Select(_ => _.Voutro).Sum() +
                                                                   productsIncentivado.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.PercentualInciso.Equals(percentualCaputII)).Select(_ => _.Vseg).Sum() -
                                                                   productsIncentivado.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.PercentualInciso.Equals(percentualCaputII)).Select(_ => _.Vdesc).Sum() +
                                                                   productsIncentivado.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.PercentualInciso.Equals(percentualCaputII)).Select(_ => _.Vfrete).Sum() +
                                                                   productsIncentivado.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.PercentualInciso.Equals(percentualCaputII)).Select(_ => _.Freterateado).Sum() +
                                                                   productsIncentivado.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.PercentualInciso.Equals(percentualCaputII)).Select(_ => _.Vipi).Sum()),
                                    impostoIcmsCaputI = Math.Round(Convert.ToDecimal(baseCaputI * percentualCaputI / 100), 2),
                                    impostoIcmsCaputII = Math.Round(Convert.ToDecimal(baseCaputII * percentualCaputII / 100), 2);

                            baseIcms = baseCaputI + baseCaputII;
                            impostoIcms = impostoIcmsCaputI + impostoIcmsCaputII;
                            impostoFecop = Math.Round(Convert.ToDecimal(baseIcms * (comp.Fecop / 100)), 2);

                            ViewBag.PercentualCaputI = percentualCaputI;
                            ViewBag.PercentualCaputII = percentualCaputII;
                            ViewBag.BaseCaputI = baseCaputI;
                            ViewBag.BaseCaputII = baseCaputII;
                            ViewBag.ImpostoIcmsCaputI = impostoIcmsCaputI;
                            ViewBag.ImpostoIcmsCaputII = impostoIcmsCaputII;
                        }
                        
                        if (comp.AnnexId.Equals(3) && !comp.ChapterId.Equals(4))
                        {
                            if (comp.SectionId.Equals(2))
                            {
                                decimal vendasInternasElencadas = Convert.ToDecimal(imp.VendasInternas1), vendasInterestadualElencadas = Convert.ToDecimal(imp.VendasInterestadual1),
                                vendasInternasDeselencadas = Convert.ToDecimal(imp.VendasInternas2), vendasInterestadualDeselencadas = Convert.ToDecimal(imp.VendasInterestadual2),
                                InternasElencadas = Convert.ToDecimal(imp.SaidaInterna1), InterestadualElencadas = Convert.ToDecimal(imp.SaidaInterestadual1),
                                InternasElencadasPortaria = Convert.ToDecimal(imp.SaidaPortInterna1), InterestadualElencadasPortaria = Convert.ToDecimal(imp.SaidaPortInterestadual1),
                                InternasDeselencadas = Convert.ToDecimal(imp.SaidaInterna2), InterestadualDeselencadas = Convert.ToDecimal(imp.SaidaInterestadual2),
                                InternasDeselencadasPortaria = Convert.ToDecimal(imp.SaidaPortInterna2),
                                InterestadualDeselencadasPortaria = Convert.ToDecimal(imp.SaidaPortInterestadual2),
                                suspensao = Convert.ToDecimal(imp.Suspensao), vendasClienteCredenciado = Convert.ToDecimal(imp.VendasClientes),
                                vendas = vendasInternasElencadas + vendasInterestadualElencadas + vendasInternasDeselencadas + vendasInterestadualDeselencadas;


                                //  Elencadas
                                // Internas
                                decimal icmsInternaElencada = (InternasElencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                                decimal fecopInternaElencada = Math.Round((InternasElencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100, 2);
                                decimal totalInternasElencada = icmsInternaElencada;
                                decimal icmsPresumidoInternaElencada = (InternasElencadasPortaria * Convert.ToDecimal(comp.IncIInterna)) / 100;
                                decimal totalIcmsInternaElencada = Math.Round(totalInternasElencada - icmsPresumidoInternaElencada, 2);

                                impostoIcms += Math.Round(totalIcmsInternaElencada, 2);
                                impostoFecop += Math.Round(fecopInternaElencada, 2);

                                // Interestadual
                                decimal icmsInterestadualElencada = (InterestadualElencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                                decimal fecopInterestadualElencada = Math.Round((InterestadualElencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100, 2);
                                decimal totalInterestadualElencada = icmsInterestadualElencada;
                                decimal icmsPresumidoInterestadualElencada = (InterestadualElencadasPortaria * Convert.ToDecimal(comp.IncIInterestadual)) / 100;
                                decimal totalIcmsInterestadualElencada = Math.Round(totalInterestadualElencada - icmsPresumidoInterestadualElencada, 2);

                                impostoIcms += totalIcmsInterestadualElencada;
                                impostoFecop += fecopInterestadualElencada;

                                //  Deselencadas
                                //  Internas
                                decimal icmsInternaDeselencada = (InternasDeselencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                                decimal fecopInternaDeselencada = Math.Round((InternasDeselencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100, 2);
                                decimal totalInternasDeselencada = icmsInternaDeselencada;
                                decimal icmsPresumidoInternaDeselencada = (InternasDeselencadasPortaria * Convert.ToDecimal(comp.IncIIInterna)) / 100;
                                decimal totalIcmsInternaDeselencada = Math.Round(totalInternasDeselencada - icmsPresumidoInternaDeselencada, 2);

                                impostoIcms += totalIcmsInternaDeselencada;
                                impostoFecop += fecopInternaDeselencada;

                                // Interestadual
                                decimal icmsInterestadualDeselencada = (InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.AliqInterna)) / 100;
                                decimal fecopInterestadualDeselencada = Math.Round((InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.Fecop)) / 100, 2);
                                decimal totalInterestadualDeselencada = icmsInterestadualDeselencada;
                                decimal icmsPresumidoInterestadualDeselencada = (InterestadualDeselencadasPortaria * Convert.ToDecimal(comp.IncIIInterestadual)) / 100;
                                decimal totalIcmsInterestadualDeselencada = Math.Round(totalInterestadualDeselencada - icmsPresumidoInterestadualDeselencada, 2);

                                impostoIcms += totalIcmsInterestadualDeselencada;
                                impostoFecop += fecopInterestadualDeselencada;

                                //  Percentual
                                decimal percentualVendas = (vendasClienteCredenciado * 100) / vendas;

                                var notifi = _notificationService.FindByCurrentMonth(id, month, year);

                                if (percentualVendas < Convert.ToDecimal(comp.VendaArt781))
                                {
                                    // Venda p/ Cliente Credenciado no Art. 781
                                    if (notifi != null)
                                    {
                                        Model.Notification nn = new Notification();
                                        nn.Description = "Venda p/ Cliente Credenciado no Art. 781 menor que " + comp.VendaArt781.ToString() + " %";
                                        nn.Percentual = percentualVendas;
                                        nn.MesRef = month;
                                        nn.AnoRef = year;
                                        nn.CompanyId = id;
                                        nn.Created = DateTime.Now;
                                        nn.Updated = nn.Created;
                                        _notificationService.Create(nn, GetLog(Model.OccorenceLog.Create));
                                    }
                                    else
                                    {
                                        notifi.Percentual = percentualVendas;
                                        notifi.Updated = DateTime.Now;
                                        _notificationService.Update(notifi, GetLog(Model.OccorenceLog.Update));
                                    }
                                }
                                else
                                {
                                    if (notifi != null)
                                    {
                                        _notificationService.Delete(notifi.Id, GetLog(Model.OccorenceLog.Delete));
                                    }
                                }

                                //  Suspensão
                                decimal totalSuspensao = Math.Round((suspensao * Convert.ToDecimal(comp.Suspension)) / 100, 2);

                                totalDarIcms += totalSuspensao;


                                //  Elencadas
                                // Internas
                                ViewBag.VendasInternasElencadas = vendasInternasElencadas;
                                ViewBag.InternasElencadas = InternasElencadas;
                                ViewBag.InternasElencadasPortaria = InternasElencadasPortaria;
                                ViewBag.IcmsInternasElencadas = icmsInternaElencada;
                                ViewBag.FecopInternasElencadas = fecopInternaElencada;
                                ViewBag.TotalInternasElencadas = totalInternasElencada;
                                ViewBag.IcmsPresumidoInternasElencadas = icmsPresumidoInternaElencada;
                                ViewBag.TotalIcmsInternasElencadas = totalIcmsInternaElencada;

                                // Interestadual
                                ViewBag.VendasInterestadualElencadas = vendasInterestadualElencadas;
                                ViewBag.InterestadualElencadas = InterestadualElencadas;
                                ViewBag.InterestadualElencadasPortaria = InterestadualElencadasPortaria;
                                ViewBag.IcmsInterestadualElencadas = icmsInterestadualElencada;
                                ViewBag.FecopInterestadualElencadas = fecopInterestadualElencada;
                                ViewBag.TotalInterestadualElencadas = totalInterestadualElencada;
                                ViewBag.IcmsPresumidoInterestadualElencadas = icmsPresumidoInterestadualElencada;
                                ViewBag.TotalIcmsInterestadualElencadas = totalIcmsInterestadualElencada;

                                //  Deselencadas
                                //  Internas
                                ViewBag.VendasInternasDeselencadas = vendasInternasDeselencadas;
                                ViewBag.InternasDeselencadas = InternasDeselencadas;
                                ViewBag.InternasDeselencadasPortaria = InternasDeselencadasPortaria;
                                ViewBag.IcmsInternasElencadas = icmsInternaElencada;
                                ViewBag.IcmsInternasDeselencadas = icmsInternaDeselencada;
                                ViewBag.FecopInternasDeselencadas = fecopInternaDeselencada;
                                ViewBag.TotalInternasDeselencadas = totalInternasDeselencada;
                                ViewBag.IcmsPresumidoInternasDeselencadas = icmsPresumidoInternaDeselencada;
                                ViewBag.TotalIcmsInternasDeselencadas = totalIcmsInternaDeselencada;


                                // Interestadual
                                ViewBag.VendasInterestadualDeselencadas = vendasInterestadualDeselencadas;
                                ViewBag.InterestadualDeselencadas = InterestadualDeselencadas;
                                ViewBag.InterestadualDeselencadasPortaria = InterestadualDeselencadasPortaria;
                                ViewBag.IcmsInterestadualDeselencadas = icmsInterestadualDeselencada;
                                ViewBag.FecopInterestadualDeselencadas = fecopInterestadualDeselencada;
                                ViewBag.TotalInterestadualDeselencadas = totalInterestadualDeselencada;
                                ViewBag.IcmsPresumidoInterestadualDeselencadas = icmsPresumidoInterestadualDeselencada;
                                ViewBag.TotalIcmsInterestadualDeselencadas = totalIcmsInterestadualDeselencada;


                                //  Percentual
                                ViewBag.PercentualVendas = percentualVendas;

                                //  Suspensão
                                ViewBag.PercentualSuspensao = Convert.ToDecimal(comp.Suspension);
                                ViewBag.Suspensao = suspensao;
                                ViewBag.TotalSuspensao = totalSuspensao;

                            }

                            ViewBag.AliqInterna = Convert.ToDecimal(comp.AliqInterna);
                            ViewBag.Fecop = Convert.ToDecimal(comp.Fecop);

                            //  Elencadas
                            ViewBag.IncIInterna = Convert.ToDecimal(comp.IncIInterna);
                            ViewBag.IncIInterestadual = Convert.ToDecimal(comp.IncIInterestadual);

                            //  Deselencadas
                            ViewBag.IncIIInterna = Convert.ToDecimal(comp.IncIIInterna);
                            ViewBag.IncIIInterestadual = Convert.ToDecimal(comp.IncIIInterestadual);

                        }

                        if (comp.AnnexId.Equals(1) && !comp.ChapterId.Equals(4))
                        {
                            if (impAnexo == null)
                            {
                                ViewBag.Erro = 2;
                                return View(null);
                            }

                            baseIcms = Convert.ToDecimal(productsIncentivado.Select(_ => _.Vprod).Sum() + productsIncentivado.Select(_ => _.Voutro).Sum() +
                            productsIncentivado.Select(_ => _.Vseg).Sum() - productsIncentivado.Select(_ => _.Vdesc).Sum() + productsIncentivado.Select(_ => _.Vfrete).Sum() +
                            productsIncentivado.Select(_ => _.Freterateado).Sum() + productsIncentivado.Select(_ => _.Vipi).Sum());
                            impostoIcms = Convert.ToDecimal(baseIcms * (Convert.ToDecimal(comp.Icms) / 100));

                            decimal icmsAPAPagarTemp = 0;

                            if (icmsAPagarAPSIE > 0)
                                icmsAPAPagarTemp += icmsAPagarAPSIE;

                            if (icmsAPagarAPIE > 0)
                                icmsAPAPagarTemp += icmsAPagarAPIE;

                            var vendas = _vendaAnexoService.FindByVendasTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();
                            var devoFornecedors = _devoFornecedorService.FindByDevoTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();
                            var compras = _compraAnexoService.FindByComprasTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();
                            var devoClientes = _devoClienteService.FindByDevoTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();

                            ViewBag.VendasInternas = vendas;
                            ViewBag.DevoFornecedorInternas = devoFornecedors;
                            ViewBag.ComprasInternas = compras;
                            ViewBag.DevoClienteInternas = devoClientes;
                            ViewBag.TaxAnexo = impAnexo;

                            var mesAtual = importMes.NumberMonth(month);
                            var mesAnterior = importMes.NameMonthPrevious(mesAtual);
                            decimal saldoCredorAnterior = 0;

                            string ano = year;

                            if (mesAtual.Equals(1))
                                ano = (Convert.ToInt32(year) - 1).ToString();

                            var creditLast = _creditBalanceService.FindByLastMonth(id, mesAnterior, ano);

                            if (creditLast != null)
                                saldoCredorAnterior = Convert.ToDecimal(creditLast.Saldo);

                            //  Total
                            // A
                            decimal icmsTotalA = Convert.ToDecimal(compras.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsCompra4) +
                                                Convert.ToDecimal(impAnexo.IcmsCompra7) + Convert.ToDecimal(impAnexo.IcmsCompra12);
                            icmsTotalA -= (Convert.ToDecimal(devoFornecedors.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsDevoFornecedor4) +
                                            Convert.ToDecimal(impAnexo.IcmsDevoFornecedor7) + Convert.ToDecimal(impAnexo.IcmsDevoFornecedor12));
                            ViewBag.IcmsTotalA = icmsTotalA;

                            // D
                            decimal icmsTotalD = Convert.ToDecimal(vendas.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsVenda4) +
                                                 Convert.ToDecimal(impAnexo.IcmsVenda7) + Convert.ToDecimal(impAnexo.IcmsVenda12);
                            icmsTotalD -= (Convert.ToDecimal(devoClientes.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsDevoCliente4) +
                                            Convert.ToDecimal(impAnexo.IcmsDevoCliente12));
                            ViewBag.IcmsTotalD = icmsTotalD;


                            // Icms Anexo
                            icmsAnexoCCCXVI = icmsTotalD - icmsTotalA - icmsAPAPagarTemp - saldoCredorAnterior;

                            if (icmsAnexoCCCXVI < 0)
                                icmsAnexoCCCXVI = 0;

                            baseIcms = Convert.ToDecimal(productsIncentivado.Select(_ => _.Vprod).Sum() + productsIncentivado.Select(_ => _.Voutro).Sum() +
                                            productsIncentivado.Select(_ => _.Vseg).Sum() - productsIncentivado.Select(_ => _.Vdesc).Sum() + productsIncentivado.Select(_ => _.Vfrete).Sum() +
                                            productsIncentivado.Select(_ => _.Freterateado).Sum() + productsIncentivado.Select(_ => _.Vipi).Sum());
                            impostoIcms = Math.Round(Convert.ToDecimal(baseIcms * (comp.Icms / 100)), 2);
                        }
                       
                        if (!comp.AnnexId.Equals(3) && !comp.AnnexId.Equals(1) && !comp.ChapterId.Equals(4))
                        {
                            baseIcms = Convert.ToDecimal(productsIncentivado.Select(_ => _.Vprod).Sum() + productsIncentivado.Select(_ => _.Voutro).Sum() +
                            productsIncentivado.Select(_ => _.Vseg).Sum() - productsIncentivado.Select(_ => _.Vdesc).Sum() + productsIncentivado.Select(_ => _.Vfrete).Sum() +
                            productsIncentivado.Select(_ => _.Freterateado).Sum() + productsIncentivado.Select(_ => _.Vipi).Sum());
                            impostoIcms = Convert.ToDecimal(baseIcms * (Convert.ToDecimal(comp.Icms) / 100));
                            impostoFecop = Convert.ToDecimal(baseIcms * (Convert.ToDecimal(comp.Fecop) / 100));
                        }
                        
                        totalDarFecop += Math.Round(impostoFecop, 2);
                        totalApuradoFecop += Math.Round(impostoFecop, 2);

                        decimal icmsGeralNormal = Convert.ToDecimal(totalApuradoSTIE) + Convert.ToDecimal(totalApuradoSTSIE);
                        decimal icmsGeralIncetivo = Convert.ToDecimal(products.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Incentivo.Equals(true)).Select(_ => _.TotalICMS).Sum());
                        decimal fecopGeralNomal = Convert.ToDecimal(totalFecopCalcSTIE) + Convert.ToDecimal(totalFecopCalcSTSIE);
                        decimal fecopGeralIncentivo = Convert.ToDecimal(products.Where(_ => (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)) && _.Incentivo.Equals(true)).Select(_ => _.TotalFecop).Sum());
                        decimal impostoGeral = icmsGeralNormal + icmsGeralIncetivo + fecopGeralNomal + fecopGeralIncentivo + icmsAnexoCCCXVI;

                        ViewBag.Base = baseIcms;
                        ViewBag.IcmsAnexoCCCXVI = icmsAnexoCCCXVI;
                        ViewBag.IcmsNormal = icmsGeralNormal;
                        ViewBag.FecopNormal = fecopGeralNomal;
                        ViewBag.IcmsIncentivo = icmsGeralIncetivo;
                        ViewBag.FecopIncentivo = fecopGeralIncentivo;
                        ViewBag.ImpostoGeral = impostoGeral;
                        ViewBag.ImpostoFecop = impostoFecop;
                        ViewBag.ImpostoIcms = impostoIcms;



                        decimal? basefunef = impostoGeral - impostoIcms;
                        ViewBag.BaseFunef = Convert.ToDecimal(basefunef);
                        ViewBag.Funef = comp.Funef;
                        decimal taxaFunef = 0;

                        if (basefunef > 0)
                            taxaFunef = Convert.ToDecimal(basefunef * (Convert.ToDecimal(comp.Funef) / 100));

                        ViewBag.TaxaFunef = taxaFunef;

                        totalDarIcms += Math.Round(impostoIcms, 2);
                        totalDarFunef += Math.Round(taxaFunef, 2);

                        if (!comp.AnnexId.Equals(3))
                        {
                            // Icms Excedente
                            var grupos = _grupoService.FindByGrupos(imp.Id);


                            decimal totalTranferenciaSaida = Convert.ToDecimal(imp.TransferenciaSaida), totalTranferenciaEntrada = Convert.ToDecimal(imp.TransferenciaEntrada),
                                    totalTranferenciaInter = Convert.ToDecimal(imp.TransferenciaInter), totalVendasSuspensao = Convert.ToDecimal(imp.Suspensao),
                                    totalCompra = Convert.ToDecimal(imp.Compras) + totalTranferenciaEntrada, totalDevoCompra = Convert.ToDecimal(imp.DevolucaoCompras),
                                    totalVenda = Convert.ToDecimal(imp.Vendas) + totalTranferenciaSaida, totalDevoVenda = Convert.ToDecimal(imp.DevolucaoVendas),
                                    totalVendaGrupo = Convert.ToDecimal(grupos.Sum(_ => _.Vendas)), totalDevoGrupo = Convert.ToDecimal(grupos.Sum(_ => _.Devolucao)),
                                    totalNcm = Convert.ToDecimal(imp.VendasNcm), totalDevoNcm = Convert.ToDecimal(imp.DevolucaoNcm),
                                    totalIncisoI = Convert.ToDecimal(imp.VendasIncisoI), totalDevoIncisoI = Convert.ToDecimal(imp.DevolucaoVendasIncisoI),
                                    totalIncisoII = Convert.ToDecimal(imp.VendasIncisoII), totalDevoIncisoII = Convert.ToDecimal(imp.DevolucaoVendasIncisoII),
                                    totalNcontribuinte = Convert.ToDecimal(imp.VendasNContribuinte), totalDevoNContribuinte = Convert.ToDecimal(imp.DevolucaoNContribuinte),
                                    totalContribuinte = totalVenda - totalNcontribuinte, totalDevoContribuinte = totalDevoVenda - totalDevoNContribuinte,
                                    baseCalcCompra = totalCompra - totalDevoCompra,
                                    baseCalcVenda = totalVenda - totalDevoVenda,
                                    baseCalcGrupo = totalVendaGrupo - totalDevoGrupo,
                                    baseCalcNcm = totalNcm - totalDevoNcm,
                                    baseCalcIncisoI = totalIncisoI - totalDevoIncisoI,
                                    baseCalcIncisoII = totalIncisoII - totalDevoIncisoII,
                                    baseCalcNContribuinte = totalNcontribuinte - totalDevoNContribuinte,
                                    baseCalcContribuinte = totalContribuinte - totalDevoContribuinte,
                                    limiteGrupo = (baseCalcVenda * Convert.ToDecimal(comp.VendaMGrupo)) / 100,
                                    limiteNcm = (baseCalcVenda * Convert.ToDecimal(comp.VendaAnexo)) / 100,
                                    limiteNContribuinte = (baseCalcVenda * (Convert.ToDecimal(comp.VendaCpf))) / 100,
                                    limiteContribuinte = (baseCalcVenda * (Convert.ToDecimal(comp.VendaContribuinte))) / 100,
                                    limiteTransferencia = (baseCalcVenda * Convert.ToDecimal(comp.Transferencia)) / 100,
                                    limiteTransferenciaInter = (totalTranferenciaEntrada * Convert.ToDecimal(comp.TransferenciaInter)) / 100;

                            if (comp.ChapterId == 4)
                                limiteNcm = (baseCalcVenda * Convert.ToDecimal(comp.Faturamento)) / 100;

                            decimal excedenteGrupo = 0, impostoGrupo = 0, excedenteNcm = 0, impostoNcm = 0, impostoNContribuinte = 0, excedenteNContribuinte = 0,
                                    impostoContribuinte = 0, excedenteContribuinte = 0, excedenteTranf = 0, impostoTransf = 0, excedenteTranfInter = 0, impostoTransfInter = 0;


                            //  CNPJ
                            List<List<string>> gruposExecentes = new List<List<string>>();

                            if (baseCalcGrupo > limiteGrupo)
                            {
                                excedenteGrupo = baseCalcGrupo - limiteGrupo;
                                impostoGrupo = Math.Round((excedenteGrupo * Convert.ToDecimal(comp.VendaMGrupoExcedente)) / 100, 2);
                            }

                            foreach (var g in grupos)
                            {
                                List<string> grupoExcedente = new List<string>();
                                grupoExcedente.Add(g.Cnpj);
                                grupoExcedente.Add(g.Nome);
                                grupoExcedente.Add(Math.Round(Convert.ToDecimal(g.BaseCalculo), 2).ToString());
                                grupoExcedente.Add(g.Percentual.ToString());

                                gruposExecentes.Add(grupoExcedente);
                            }

                            //  Anexo II ou Inciso I e II
                            if (baseCalcNcm < limiteNcm && (comp.AnnexId == 1 || comp.ChapterId == 4))
                            {
                                excedenteNcm = limiteNcm - baseCalcNcm;

                                if (comp.AnnexId == 1)
                                    impostoNcm = Math.Round((excedenteNcm * Convert.ToDecimal(comp.VendaAnexoExcedente)) / 100, 2);
                                else
                                    impostoNcm = Math.Round((excedenteNcm * Convert.ToDecimal(comp.FaturamentoExcedente)) / 100, 2);
                            }

                            //  Contribuinte
                            if (baseCalcContribuinte < limiteContribuinte && comp.ChapterId == 4)
                            {
                                excedenteContribuinte = baseCalcContribuinte - limiteContribuinte;
                                impostoContribuinte = Math.Round((excedenteContribuinte * Convert.ToDecimal(comp.VendaContribuinteExcedente)) / 100, 2);
                            }

                            //  Não Contribuinte
                            if (baseCalcNContribuinte > limiteNContribuinte && comp.ChapterId != 4)
                            {
                                excedenteNContribuinte = baseCalcNContribuinte - limiteNContribuinte;
                                impostoNContribuinte = Math.Round((excedenteNContribuinte * Convert.ToDecimal(comp.VendaCpfExcedente)) / 100, 2);
                            }

                            //  Transferência
                            if (totalTranferenciaSaida > limiteTransferencia && comp.ChapterId == 4)
                            {
                                excedenteTranf = totalTranferenciaSaida - limiteTransferencia;
                                impostoTransf = Math.Round((excedenteTranf * Convert.ToDecimal(comp.TransferenciaExcedente)) / 100, 2);
                            }

                            //  Transferência inter
                            if (totalTranferenciaInter > limiteTransferenciaInter)
                            {
                                excedenteTranfInter = totalTranferenciaInter - limiteTransferenciaInter;
                                impostoTransfInter = Math.Round((excedenteTranfInter * Convert.ToDecimal(comp.TransferenciaInterExcedente)) / 100, 2);
                            }

                            //  Suspensão
                            decimal valorSuspensao = Math.Round((totalVendasSuspensao * Convert.ToDecimal(comp.Suspension)) / 100, 2);

                            //  Percentuais
                            decimal percentualVendaContribuinte = (baseCalcContribuinte * 100) / baseCalcVenda,
                                    percentualVendaNContribuinte = (baseCalcNContribuinte * 100) / baseCalcVenda,
                                    percentualVendaNcm = (baseCalcNcm * 100) / baseCalcVenda,
                                    percentualVendaIncisoI = (baseCalcIncisoI * 100) / baseCalcVenda,
                                    percentualVendaIncisoII = (baseCalcIncisoII * 100) / baseCalcVenda,
                                    percentualGrupo = (baseCalcGrupo * 100) / baseCalcVenda;

                            //  Geral
                            ViewBag.TotalVenda = totalVenda;
                            ViewBag.TotalDevo = totalDevoVenda;
                            ViewBag.BaseCalc = baseCalcVenda;

                            //  CNPJ
                            ViewBag.TotalVendaGrupo = totalVendaGrupo;
                            ViewBag.TotalDevoGrupo = totalDevoGrupo;
                            ViewBag.TotalBaseCalcuGrupo = baseCalcGrupo;
                            ViewBag.PercentualVendaGrupo = percentualGrupo;
                            ViewBag.ExcedenteGrupo = excedenteGrupo;
                            ViewBag.TotalExcedenteGrupo = impostoGrupo;
                            ViewBag.LimiteGrupo = limiteGrupo;
                            ViewBag.Grupo = gruposExecentes;

                            //  Anexo II ou Inciso I e II
                            ViewBag.VendaNcm = totalNcm;
                            ViewBag.TotalDevoNcm = totalDevoNcm;
                            ViewBag.BaseCalcNcm = baseCalcNcm;
                            ViewBag.PercentualVendaNcm = percentualVendaNcm;
                            ViewBag.LimiteNcm = limiteNcm;
                            ViewBag.ExcedenteNcm = excedenteNcm;
                            ViewBag.TotalExcedenteNcm = impostoNcm;

                            // Inciso I e II
                            ViewBag.VendasIncisoI = totalIncisoI;
                            ViewBag.VendasIncisoII = totalIncisoII;
                            ViewBag.TotalDevoIncisoI = totalDevoIncisoI;
                            ViewBag.TotalDevoIncisoII = totalDevoIncisoII;
                            ViewBag.BaseCalcIncisoI = baseCalcIncisoI;
                            ViewBag.BaseCalcIncisoII = baseCalcIncisoII;
                            ViewBag.PercentualVendaIncisoI = percentualVendaIncisoI;
                            ViewBag.PercentualVendaIncisoII = percentualVendaIncisoII;

                            //  Contribuinte
                            ViewBag.Contribuinte = totalContribuinte;
                            ViewBag.TotalDevoContribuite = totalDevoContribuinte;
                            ViewBag.BaseCalcContribuinte = baseCalcContribuinte;
                            ViewBag.PercentualVendaContribuinte = percentualVendaContribuinte;
                            ViewBag.LimiteContribuinte = limiteContribuinte;
                            ViewBag.ExcedenteContribuinte = excedenteContribuinte;
                            ViewBag.TotalExcedenteContribuinte = impostoContribuinte;

                            //  Não Contribuinte
                            ViewBag.NContribuinte = totalNcontribuinte;
                            ViewBag.TotalDevoNContribuinte = totalDevoNContribuinte;
                            ViewBag.BaseCalcNContribuinte = baseCalcNContribuinte;
                            ViewBag.PercentualVendaNContribuinte = percentualVendaNContribuinte;
                            ViewBag.LimiteNContribuinte = limiteNContribuinte;
                            ViewBag.ExcedenteNContribuinte = excedenteNContribuinte;
                            ViewBag.TotalExcedenteNContribuinte = impostoNContribuinte;

                            //  Tranferência
                            ViewBag.TotalTransferencia = totalTranferenciaSaida;
                            ViewBag.LimiteTransferencia = limiteTransferencia;
                            ViewBag.ExcedenteTransferencia = excedenteTranf;
                            ViewBag.TotalExcedenteTransferencia = impostoTransf;

                            //  Tranferência Interestadual
                            ViewBag.TotalTransferenciaInter = totalTranferenciaInter;
                            ViewBag.LimiteTransferenciaInter = limiteTransferenciaInter;
                            ViewBag.ExcedenteTransferenciaInter = excedenteTranfInter;
                            ViewBag.TotalExcedenteTransferenciaInter = impostoTransfInter;

                            //  Suspensão
                            ViewBag.BaseCalcSuspensao = totalVendasSuspensao;
                            ViewBag.TotalSuspensao = valorSuspensao;


                            //Total Icms Excedente
                            ViewBag.TotalIcmsExcedente = Math.Round(impostoNcm + impostoContribuinte + impostoNContribuinte + impostoTransf + impostoTransfInter + impostoGrupo, 2);

                            totalDarIcms += Math.Round(impostoNcm + impostoContribuinte + impostoNContribuinte + impostoTransf + impostoTransfInter + impostoGrupo + valorSuspensao, 2);
                        }

                    }
                    
                    if (comp.Incentive && comp.AnnexId.Equals(null))
                    {
                        if (imp == null)
                        {
                            ViewBag.Erro = 1;
                            return View(null);
                        }

                        var grupos = _grupoService.FindByGrupos(imp.Id);

                        if (comp.TypeCompany)
                        {
                            decimal creditosIcms = Convert.ToDecimal(imp.Credito), debitosIcms = Convert.ToDecimal(imp.Debito);

                            decimal naoContribuinteIncentivo = Convert.ToDecimal(imp.VendasNContribuinte), naoContriForaDoEstadoIncentivo = Convert.ToDecimal(imp.VendasNContribuinteFora),
                                vendaCfopSTContribuintesNIncentivo = Convert.ToDecimal(imp.ReceitaST1), ContribuinteIsento = Convert.ToDecimal(imp.ReceitaIsento1),
                                ContribuintesIncentivo = Convert.ToDecimal(imp.VendasContribuinte1), ContribuintesNIncentivo = Convert.ToDecimal(imp.ReceitaNormal1),
                                ContribuintesIncentivoAliqM25 = Convert.ToDecimal(imp.VendasContribuinte2), naoContribuinteNIncetivo = Convert.ToDecimal(imp.ReceitaNormal2),
                                vendaCfopSTNaoContribuinteNIncetivo = Convert.ToDecimal(imp.ReceitaST2), NaoContribuiteIsento = Convert.ToDecimal(imp.ReceitaIsento2),
                                naoContriForaDoEstadoNIncentivo = Convert.ToDecimal(imp.ReceitaNormal3), vendaCfopSTNaoContriForaDoEstadoNIncentivo = Convert.ToDecimal(imp.ReceitaST3),
                                NaoContribuinteForaDoEstadoIsento = Convert.ToDecimal(imp.ReceitaIsento3);


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

                            //// Direfença de débito e crédito
                            var diferenca = debitosIcms - creditosIcms;

                            //Total Icms
                            var totalIcms = icmsContribuinteIncentivo + icmsNContribuinteIncentivo + icmsContribuinteIncentivoAliqM25;

                            totalDarIcms += totalIcms;

                            //// FUNEF e COTAC
                            var baseCalculo = diferenca - totalIcms;

                            //FUNEF
                            decimal percentualFunef = Convert.ToDecimal(comp.Funef == null ? 0 : comp.Funef);
                            var totalFunef = Math.Round((baseCalculo * percentualFunef) / 100, 2);

                            totalDarFunef += Math.Round(totalFunef, 2);

                            //COTAC
                            decimal percentualCotac = Convert.ToDecimal(comp.Cotac == null ? 0 : comp.Cotac);
                            var totalCotac = Math.Round((baseCalculo * percentualCotac) / 100, 2);

                            totalDarCotac += Math.Round(totalCotac, 2);

                            //Total Funef e Cotac
                            var totalFunefCotac = totalFunef + totalCotac;

                            ////Total Imposto
                            var totalImposto = Math.Round(icmsContribuinteIncentivo + icmsNContribuinteIncentivo + totalFunef + totalCotac, 2);


                            ////Total Imposto Geral
                            var totalImpostoGeral = Math.Round(totalImposto + icmsNContribuinteForaDoEstado, 2);

                            //// Cálculos dos Totais
                            var totalVendaContribuinte = Math.Round(ContribuintesIncentivo + ContribuintesNIncentivo + vendaCfopSTContribuintesNIncentivo, 2);
                            var totalIcmsGeralIncentivo = Math.Round(icmsContribuinteIncentivo + icmsNContribuinteIncentivo + icmsNContribuinteForaDoEstado, 2);
                            var totalGeralVendasIncentivo = Math.Round(totalVendaContribuinte + totalVendasNContribuinte + ContribuinteIsento + NaoContribuiteIsento + ContribuintesIncentivoAliqM25, 2);


                            //// Produtos Incentivados

                            //Contribuinte
                            ViewBag.VendaContribuinteIncentivo = ContribuintesIncentivo;
                            ViewBag.PercentualIcmsContrib = Convert.ToDecimal(comp.Icms);
                            ViewBag.ValorVendaContribIncentivo = icmsContribuinteIncentivo;
                            ViewBag.ContribuinteIsento = ContribuinteIsento;
                            ViewBag.ContribuinteIncentivoAliM25 = ContribuintesIncentivoAliqM25;
                            ViewBag.ValorVendaContribuinteAliM25 = icmsContribuinteIncentivoAliqM25;
                            ViewBag.PercentualIcmsAiqM25Contrib = Convert.ToDecimal(comp.IcmsAliqM25);
                            ViewBag.VendaSTContribuinte = vendaCfopSTContribuintesNIncentivo;


                            //Não Contribuinte
                            ViewBag.VendaNContribIncentivo = naoContribuinteIncentivo;
                            ViewBag.TotalVendaNContribuinte = totalVendasNContribuinte;
                            ViewBag.PercentualIcmsNContribuinte = Convert.ToDecimal(comp.IcmsNContribuinte);
                            ViewBag.ValorVendaNContribIncentivo = icmsNContribuinteIncentivo;
                            ViewBag.NaoContribuinteIsento = NaoContribuiteIsento;
                            ViewBag.VendaSTNContribuinte = vendaCfopSTNaoContribuinteNIncetivo;

                            //Não Contribuinte Fora do Estado
                            ViewBag.VendaNForaEstadoContribuinteIncetivo = naoContriForaDoEstadoIncentivo;
                            ViewBag.TotalVendaNContribuinteForaDoEstado = totalVendasNContribuinteForaDoEstado;
                            ViewBag.PercentualIcmsNaoContribForaDoEstado = Convert.ToDecimal(comp.IcmsNContribuinteFora);
                            ViewBag.ValorVendaNContribForaDoEstado = icmsNContribuinteForaDoEstado;
                            ViewBag.NaoContribuinteForaDoEstadoIsento = NaoContribuinteForaDoEstadoIsento;
                            ViewBag.VendaSTForaEstadoNContribuinte = vendaCfopSTNaoContriForaDoEstadoNIncentivo;

                            List<List<string>> icmsForaDoEstado = new List<List<string>>();

                            foreach (var g in grupos)
                            {
                                List<string> icmsFora = new List<string>();
                                icmsFora.Add(g.Uf);
                                icmsFora.Add(g.Icms.ToString());
                                icmsForaDoEstado.Add(icmsFora);

                            }

                            ViewBag.IcmsForaDoEstado = icmsForaDoEstado;

                            //// Produtos não incentivados

                            //Contribuinte
                            ViewBag.VendaContribuinteNIncentivo = ContribuintesNIncentivo;

                            //Não Contribuinte
                            ViewBag.VendaNContribuinteNIncentivo = naoContribuinteNIncetivo;

                            //Não Contribuinte Fora do Estado
                            ViewBag.VendaNContribuinteNIncentivoForaDoEstado = naoContriForaDoEstadoNIncentivo;

                            //// Crédito e Débito
                            //Crédito
                            ViewBag.Credito = creditosIcms;

                            //Débito
                            ViewBag.Debito = debitosIcms;

                            //Diferença
                            ViewBag.Diferenca = diferenca;


                            ////Total Icms
                            ViewBag.TotalIcms = totalIcms;


                            //// FUNEF e COTAC
                            ViewBag.BaseCalculo = baseCalculo;

                            //FUNEF
                            ViewBag.PercentualFunef = percentualFunef;
                            ViewBag.TotalFunef = totalFunef;

                            //COTAC
                            ViewBag.PercentualCotac = percentualCotac;
                            ViewBag.TotalCotac = totalCotac;

                            //Total Funef e Cotac
                            ViewBag.TotalFunefCotac = totalFunefCotac;

                            ////Total Imposto
                            ViewBag.TotalImposto = totalImposto;

                            ////Total Imposto Geral
                            ViewBag.TotalImpostoGeral = totalImpostoGeral;

                            //// Total
                            ViewBag.TotalVendaContibuinte = totalVendaContribuinte + ContribuinteIsento + ContribuintesIncentivoAliqM25;
                            ViewBag.TotalGeralVendaNContibuinte = totalVendasNContribuinte + NaoContribuiteIsento;
                            ViewBag.TotalGeralVendaNContibuinteForaDoEstado = totalVendasNContribuinteForaDoEstado + NaoContribuinteForaDoEstadoIsento;
                            ViewBag.TotalGeralIcmsIncentivo = totalIcmsGeralIncentivo;
                            ViewBag.TotalGeralVendasIncentivo = totalGeralVendasIncentivo;
                            ViewBag.Uf = comp.County.State.UF;

                        }
                        else
                        {

                            decimal creditosIcms = creditosIcms = Convert.ToDecimal(imp.Credito);

                            decimal vendasIncentivada = Convert.ToDecimal(imp.VendasIncentivada), vendasNIncentivada = Convert.ToDecimal(imp.VendasNIncentivada),
                                debitoIncetivo = Convert.ToDecimal(grupos.Sum(_ => _.Icms)), debitoNIncentivo = Convert.ToDecimal(grupos.Sum(_ => _.IcmsNIncentivo));


                            var totalVendas = vendasIncentivada + vendasNIncentivada;

                            var difApuNormal = debitoIncetivo - creditosIcms;

                            var percentualCreditoNIncentivado = vendasNIncentivada / totalVendas * 100;
                            var creditoNIncentivado = creditosIcms * percentualCreditoNIncentivado / 100;

                            var difApuNNormal = debitoNIncentivo - creditoNIncentivado;

                            totalDarIcms += difApuNNormal;

                            //Funef e Cotac
                            var baseDeCalcFunef = difApuNormal - difApuNNormal;
                            decimal valorFunef = baseDeCalcFunef * Convert.ToDecimal(comp.Funef) / 100;
                            decimal valorCotac = baseDeCalcFunef * Convert.ToDecimal(comp.Cotac) / 100;

                            totalDarFunef += Math.Round(valorFunef, 2);
                            totalDarCotac += Math.Round(valorCotac, 2);

                            var totalImposto = Math.Round(difApuNNormal + valorFunef + valorCotac, 2);

                            List<List<string>> valoresIncentivo = new List<List<string>>();
                            List<List<string>> valoresNIncentivo = new List<List<string>>();

                            foreach (var g in grupos)
                            {
                                List<string> percentualIncentivo = new List<string>();
                                percentualIncentivo.Add(g.BaseCalculo.ToString());
                                percentualIncentivo.Add(g.Percentual.ToString());
                                percentualIncentivo.Add(g.Icms.ToString());
                                valoresIncentivo.Add(percentualIncentivo);

                                List<string> percentualNIncentivo = new List<string>();
                                percentualNIncentivo.Add(g.BaseCalculoNIncentivo.ToString());
                                percentualNIncentivo.Add(g.PercentualNIncentivo.ToString());
                                percentualNIncentivo.Add(g.IcmsNIncentivo.ToString());
                                valoresNIncentivo.Add(percentualNIncentivo);
                            }


                            //Incentivado
                            ViewBag.ValoresIncentivo = valoresIncentivo;
                            ViewBag.DebitoIncentivo = debitoIncetivo;
                            ViewBag.TotalVendas = totalVendas;
                            ViewBag.TotalVendasIncentivadas = vendasIncentivada;


                            //Não Incentivado
                            ViewBag.ValoresNIncentivo = valoresNIncentivo;
                            ViewBag.PercentualCreditoNIncentivo = percentualCreditoNIncentivado;
                            ViewBag.CreditoNIncentivo = creditoNIncentivado;
                            ViewBag.DebitoNIncentivo = debitoNIncentivo;
                            ViewBag.TotalVendas = totalVendas;
                            ViewBag.TotalVendasNIncentivadas = vendasNIncentivada;


                            // Total
                            ViewBag.TotalVendas = totalVendas;
                            ViewBag.Credito = creditosIcms;
                            ViewBag.TotalVendas = totalVendas;


                            //Apuração Normal
                            //Debito - ViewBag.DebitoIncentivo
                            ViewBag.CreditoIncentivo = creditosIcms;
                            ViewBag.DifApuNormal = difApuNormal;

                            //Apuração ñ Incentivada
                            //Debito - ViewBag.DebitoNIncetivo
                            //Credito - CreditoNIncentivo
                            ViewBag.DifApuNNormal = difApuNNormal;

                            // Funef e COTAC
                            // DifNormal - DifNIncentivada
                            ViewBag.BaseDeCalcFunef = baseDeCalcFunef;
                            ViewBag.PercentFunef = Convert.ToDecimal(comp.Funef);
                            ViewBag.ValorFunef = valorFunef;
                            ViewBag.PercentCotac = Convert.ToDecimal(comp.Cotac);
                            ViewBag.ValorCotac = valorCotac;

                            // Total De Imposto
                            ViewBag.TotalDeImposto = totalImposto;
                        }

                    }

                    //      Resumo dos Imposto Substituição Tributaria

                    //  ICMS
                    ViewBag.TotatlApuradoSTIE = totalApuradoSTIE;
                    ViewBag.TotatlApuradoSTSIE = totalApuradoSTSIE;
                    ViewBag.TotalIcmsFreteSTIE = totalIcmsFreteSTIE;
                    ViewBag.TotalFecopFreteSTIE = totalFecop1FreteSTIE + totalFecop2FreteSTIE;
                    ViewBag.GnrePagaSTSIE = gnrePagaSTSIE;
                    ViewBag.GnrePagaSTIE = gnrePagaSTIE;
                    ViewBag.GnreNPagaSTSIE = gnreNPagaSTSIE;
                    ViewBag.GnreNPagaSTIE = gnreNPagaSTIE;
                    ViewBag.TotalIcmsSTIE = icmsStSTIE;
                    ViewBag.TotalIcmsSTSIE = icmsStSTSIE;
                    ViewBag.TotalDiefSTSIE = totalDiefSTSIE;
                    ViewBag.TotalDiefSTIE = totalDiefSTIE;
                    ViewBag.IcmsPagoSTIE = icmsSTPagoIE;
                    ViewBag.IcmsPagoSTSIE = icmsSTPagoSIE;
                    ViewBag.IcmsAPagarSTSIE = icmsAPagarSTSIE;
                    ViewBag.IcmsAPagarSTIE = icmsAPagarSTIE;

                    //  FECOP
                    ViewBag.TotalFecopCalculadaSTIE = totalFecopCalcSTIE;
                    ViewBag.TotalFecopCalculadaSTSIE = totalFecopCalcSTSIE;
                    ViewBag.GNREPagaFecopSTIE = gnrePagaFecop2STIE + gnrePagaFecop1STIE;
                    ViewBag.GNREPagaFecopSTSIE = gnrePagaFecop2STSIE + gnrePagaFecop1STSIE;
                    ViewBag.GNRENPagaFecopSTIE = gnreNPagaFecopSTIE;
                    ViewBag.GNRENPagaFecopSTSIE = gnreNPagaFecopSTSIE;
                    ViewBag.TotalFecopNfeSTIE = totalFecopNfeSTIE;
                    ViewBag.TotalFecopNfeSTSIE = totalFecopNfeSTSIE;
                    ViewBag.TotalFecopDiefSTIE = totalfecopDiefSTIE;
                    ViewBag.TotalFecopDiefSTSIE = totalfecopDiefSTSIE;
                    ViewBag.IcmsFecopSTIE = icmsFecop1STIE + icmsFecop2STIE;
                    ViewBag.IcmsFecopSTSIE = icmsFecop1STSIE + icmsFecop2STSIE;
                    ViewBag.TotalFinalFecopCalculadaSTIE = Convert.ToDecimal(totalfecopDiefSTIE - (icmsFecop1STIE + icmsFecop2STIE));
                    ViewBag.TotalFinalFecopCalculadaSTSIE = Convert.ToDecimal(totalfecopDiefSTSIE - (icmsFecop1STSIE + icmsFecop2STSIE));

                    ViewBag.TotalGnreFecopSTIE = totalGnreFecopSTIE;
                    ViewBag.TotalGnreFecopSTSIE = totalGnreFecopSTSIE;



                    if (Convert.ToDecimal(totalDiefSTSIE) > 0)
                        totalApuradoST += Math.Round(Convert.ToDecimal(totalDiefSTSIE), 2);

                    if (Convert.ToDecimal(totalDiefSTIE) > 0)
                        totalApuradoST += Math.Round(Convert.ToDecimal(totalDiefSTIE), 2);

                    totalRecolhidoST += Math.Round(Convert.ToDecimal(icmsSTPagoSIE + icmsSTPagoIE), 2);

                    if (Convert.ToDecimal(totalDiefSTSIE - icmsSTPagoSIE) > 0)
                        totalDarST += Math.Round(Convert.ToDecimal(totalDiefSTSIE - icmsSTPagoSIE), 2);

                    if (Convert.ToDecimal(totalDiefSTIE - icmsSTPagoIE) > 0)
                        totalDarST += Math.Round(Convert.ToDecimal(totalDiefSTIE - icmsSTPagoIE), 2);

                    if (totalfecopDiefSTSIE > 0)
                        totalApuradoFecop += Math.Round(totalfecopDiefSTSIE, 2);

                    if (totalfecopDiefSTIE > 0)
                        totalApuradoFecop += Math.Round(totalfecopDiefSTIE, 2);

                    totalRecolhidoFecop += Math.Round(Convert.ToDecimal(icmsFecop1STSIE + icmsFecop2STSIE + icmsFecop1STIE + icmsFecop2STIE));

                    if (Convert.ToDecimal(totalfecopDiefSTSIE - (icmsFecop1STSIE + icmsFecop2STSIE)) > 0)
                        totalDarFecop += Math.Round(Convert.ToDecimal(totalfecopDiefSTSIE - (icmsFecop1STSIE + icmsFecop2STSIE)), 2);

                    if (Convert.ToDecimal(totalfecopDiefSTIE - (icmsFecop1STIE + icmsFecop2STIE)) > 0)
                        totalDarFecop += Math.Round(Convert.ToDecimal(totalfecopDiefSTIE - (icmsFecop1STIE + icmsFecop2STIE)), 2);

                    // Consumo
                    decimal totalFreteCOIE = 0;

                    foreach (var prod in products)
                    {
                        if (!prod.Note.Iest.Equals("") && prod.TaxationTypeId.Equals(2))
                            if (Convert.ToDecimal(prod.Diferencial) > 0)
                                totalFreteCOIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                    }

                    ViewBag.TotalFreteCOIE = totalFreteCOIE;

                    decimal valorNfe1NormalCOSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe1RetCOSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe2NormalCOSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe2RetCOSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe1NormalCOIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe1RetCOIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe2NormalCOIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe2RetCOIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(2) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal? gnrePagaCOIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreCo).Sum()), 2),
                             gnreNPagaCOIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNCo).Sum()), 2),
                             gnrePagaCOSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreCo).Sum()), 2),
                             gnreNPagaCOSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNCo).Sum()), 2),
                             icmsStCOIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalCOIE + valorNfe1RetCOIE + valorNfe2NormalCOIE + valorNfe2RetCOIE,
                             icmsStCOSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalCOSIE + valorNfe1RetCOSIE + valorNfe2NormalCOSIE + valorNfe2RetCOSIE,
                             totalApuradoCOIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsApurado).Sum()), 2),
                             totalApuradoCOSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(2)).Select(_ => _.IcmsApurado).Sum()), 2),
                             totalDiefCOSIE = (totalApuradoCOSIE + totalFreteCOIE) - icmsStCOSIE + gnreNPagaCOSIE - gnrePagaCOSIE,
                             totalDiefCOIE = totalApuradoCOIE - icmsStCOIE + gnreNPagaCOIE - gnrePagaCOIE - totalFreteCOIE;

                    ViewBag.TotatlApuradoCOIE = totalApuradoCOIE;
                    ViewBag.TotatlApuradoCOSIE = totalApuradoCOSIE;
                    ViewBag.TotalIcmsPagoCOIE = icmsStCOIE;
                    ViewBag.TotalIcmsPagoCOSIE = icmsStCOSIE;

                    ViewBag.GnrePagaCOSIE = gnrePagaCOSIE;
                    ViewBag.GnrePagaCOIE = gnrePagaCOIE;
                    ViewBag.GnreNPagaCOSIE = gnreNPagaCOSIE;
                    ViewBag.GnreNPagaCOIE = gnreNPagaCOIE;

                    ViewBag.TotalDiefCOSIE = totalDiefCOSIE;
                    ViewBag.TotalDiefCOIE = totalDiefCOIE;

                    decimal icmsCOnotaIE = Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsCo).Sum()),
                            icmsCOnotaSIE = Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsCo).Sum());
                    ViewBag.IcmsCOIE = icmsCOnotaIE;
                    ViewBag.IcmsCOSIE = icmsCOnotaSIE;

                    decimal icmsAPagarCOSIE = Convert.ToDecimal(totalDiefCOSIE - icmsCOnotaSIE),
                            icmsAPagarCOIE = Convert.ToDecimal(totalDiefCOIE - icmsCOnotaIE);
                    ViewBag.IcmsAPagarCOSIE = icmsAPagarCOSIE;
                    ViewBag.IcmsAPagarCOIE = icmsAPagarCOIE;

                    if (totalDiefCOSIE > 0)
                        totalApuradoCO += Math.Round(Convert.ToDecimal(totalDiefCOSIE), 2);

                    if (totalDiefCOIE > 0)
                        totalApuradoCO += Math.Round(Convert.ToDecimal(totalDiefCOIE), 2);

                    totalRecolhidoCO += Math.Round(icmsCOnotaSIE + icmsCOnotaIE, 2);

                    if (icmsAPagarCOSIE > 0)
                        totalDarCO += Math.Round(icmsAPagarCOSIE, 2);

                    if (icmsAPagarCOIE > 0)
                        totalDarCO += Math.Round(icmsAPagarCOIE, 2);


                    // Consumo para Revenda
                    decimal totalFreteCORIE = 0;

                    foreach (var prod in products)
                    {
                        if (!prod.Note.Iest.Equals("") && prod.TaxationTypeId.Equals(4))
                            if (Convert.ToDecimal(prod.Diferencial) > 0)
                                totalFreteCORIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                    }

                    ViewBag.TotalFreteCORIE = totalFreteCORIE;

                    decimal valorNfe1NormalCORSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe1RetCORSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe2NormalCORSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe2RetCORSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe1NormalCORIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe1RetCORIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe2NormalCORIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe2RetCORIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(4) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal? icmsStCORIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalCORIE + valorNfe1RetCORIE + valorNfe2NormalCORIE + valorNfe2RetCORIE,
                             icmsStCORSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalCORSIE + valorNfe1RetCORSIE + valorNfe2NormalCORSIE + valorNfe2RetCORSIE,
                             totalApuradoCORIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsApurado).Sum()), 2),
                             totalApuradoCORSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(4)).Select(_ => _.IcmsApurado).Sum()), 2),
                             totalDiefCORSIE = (totalApuradoCORSIE + totalFreteCORIE) - icmsStCORSIE,
                             totalDiefCORIE = totalApuradoCORIE - icmsStCORIE - totalFreteCORIE;

                    ViewBag.TotatlApuradoCORIE = totalApuradoCORIE;
                    ViewBag.TotatlApuradoCORSIE = totalApuradoCORSIE;
                    ViewBag.TotalIcmsPagoCORIE = icmsStCORIE;
                    ViewBag.TotalIcmsPagoCORSIE = icmsStCORSIE;

                    ViewBag.TotalDiefCORSIE = totalDiefCORSIE;
                    ViewBag.TotalDiefCORIE = totalDiefCORIE;

                    decimal icmsAPagarCORSIE = Convert.ToDecimal(totalDiefCORSIE),
                            icmsAPagarCORIE = Convert.ToDecimal(totalDiefCORIE);
                    ViewBag.IcmsAPagarCORSIE = icmsAPagarCORSIE;
                    ViewBag.IcmsAPagarCORIE = icmsAPagarCORIE;

                    if (totalDiefCORSIE > 0)
                        totalApuradoCO += Math.Round(Convert.ToDecimal(totalDiefCORSIE), 2);

                    if (totalDiefCORIE > 0)
                        totalApuradoCO += Math.Round(Convert.ToDecimal(totalDiefCORIE), 2);

                    if (icmsAPagarCORSIE > 0)
                        totalDarCO += Math.Round(icmsAPagarCORSIE, 2);

                    if (icmsAPagarCORIE > 0)
                        totalDarCO += Math.Round(icmsAPagarCORIE, 2);

                    // Imobilizado
                    decimal totalFreteIMIE = 0;

                    foreach (var prod in products)
                    {
                        if (!prod.Note.Iest.Equals("") && prod.TaxationTypeId.Equals(3))
                            if (Convert.ToDecimal(prod.Diferencial) > 0)
                                totalFreteIMIE += Convert.ToDecimal((prod.Freterateado * prod.Diferencial) / 100);
                    }

                    ViewBag.TotalFreteIMIE = totalFreteIMIE;

                    decimal valorNfe1NormalIMSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe1RetIMSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe2NormalIMSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe2RetIMSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe1NormalIMIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe1RetIMIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe2NormalIMIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe2RetIMIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.TaxationTypeId.Equals(3) && !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2);

                    decimal? gnrePagaIMIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreIm).Sum()), 2),
                             gnreNPagaIMIE = Math.Round(Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.GnreNIm).Sum()), 2),
                             gnrePagaIMSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreIm).Sum()), 2),
                             gnreNPagaIMSIE = Math.Round(Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.GnreNIm).Sum()), 2),
                             icmsStIMIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalIMIE + valorNfe1RetIMIE + valorNfe2NormalIMIE + valorNfe2RetIMIE,
                             icmsStIMSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsST).Sum()), 2) + valorNfe1NormalIMSIE + valorNfe1RetIMSIE + valorNfe2NormalIMSIE + valorNfe2RetIMSIE,
                             totalApuradoIMIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsApurado).Sum()), 2),
                             totalApuradoIMSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(3)).Select(_ => _.IcmsApurado).Sum()), 2),
                             totalDiefIMSIE = (totalApuradoIMSIE + totalFreteIMIE) - icmsStIMSIE + gnreNPagaIMSIE - gnrePagaIMSIE,
                             totaDiefIMIE = totalApuradoIMIE - icmsStIMIE + gnreNPagaIMIE - gnrePagaIMIE - totalFreteIMIE;
 
                    ViewBag.TotatlApuradoIMIE = totalApuradoIMIE;
                    ViewBag.TotatlApuradoIMSIE = totalApuradoIMSIE;
                    ViewBag.TotalIcmsPagoIMIE = icmsStIMIE;
                    ViewBag.TotalIcmsPagoIMSIE = icmsStIMSIE;

                    ViewBag.GnrePagaIMSIE = gnrePagaIMSIE;
                    ViewBag.GnrePagaIMIE = gnrePagaIMIE;
                    ViewBag.GnreNPagaIMSIE = gnreNPagaIMSIE;
                    ViewBag.GnreNPagaIMIE = gnreNPagaIMIE;

                    ViewBag.TotalDiefIMSIE = totalDiefIMSIE;
                    ViewBag.TotalDiefIMIE = totaDiefIMIE;

                    decimal icmsIMnotaIE = Convert.ToDecimal(notes.Where(_ => !_.Iest.Equals("")).Select(_ => _.IcmsIm).Sum()),
                            icmsIMnotaSIE = Convert.ToDecimal(notes.Where(_ => _.Iest.Equals("")).Select(_ => _.IcmsIm).Sum());
                    ViewBag.IcmsIMIE = icmsIMnotaIE;
                    ViewBag.IcmsIMSIE = icmsIMnotaSIE;

                    decimal icmsAPagarIMSIE = Convert.ToDecimal(totalDiefIMSIE - icmsIMnotaSIE),
                            icmsAPagarIMIE = Convert.ToDecimal(totaDiefIMIE - icmsIMnotaIE);
                    ViewBag.IcmsAPagarIMSIE = icmsAPagarIMSIE;
                    ViewBag.IcmsAPagarIMIE = icmsAPagarIMIE;

                    if (Convert.ToDecimal(totalDiefIMSIE) > 0)
                        totalApuradoIM += Math.Round(Convert.ToDecimal(totalDiefIMSIE), 2);

                    if (Convert.ToDecimal(totaDiefIMIE) > 0)
                        totalApuradoIM += Math.Round(Convert.ToDecimal(totaDiefIMIE), 2);

                    totalRecolhidoIM += Math.Round(icmsIMnotaSIE + icmsIMnotaIE, 2);

                    if (icmsAPagarIMSIE > 0)
                        totalDarIm += Math.Round(icmsAPagarIMSIE, 2);

                    if (icmsAPagarIMIE > 0)
                        totalDarIm += Math.Round(icmsAPagarIMIE, 2);


                    // Antecipação Total
    
                    decimal totalIcmsFreteATIE = 0, totalFecop1FreteATIE = 0, totalFecop2FreteATIE = 0;

                    foreach (var prod in products)
                    {
                        if (!prod.Note.Iest.Equals("") && prod.TaxationTypeId.Equals(8))
                        {
                            if (Convert.ToDecimal(prod.AliqInterna) > 0)
                            {
                                decimal valorAgreg = 0;
                                if (prod.Mva != null)
                                    valorAgreg = calculation.ValorAgregadoMva(prod.Freterateado, Convert.ToDecimal(prod.Mva));
                                if (prod.BCR != null)
                                    valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(prod.BCR), valorAgreg);
                                if (prod.Fecop != null)
                                {
                                    if (Convert.ToDecimal(prod.Fecop).Equals(1))
                                        totalFecop1FreteATIE += calculation.ValorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                    else
                                        totalFecop2FreteATIE += calculation.ValorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);

                                }

                                totalIcmsFreteATIE += calculation.ValorAgregadoAliqInt(Convert.ToDecimal(prod.AliqInterna), Convert.ToDecimal(prod.Fecop), valorAgreg) - prod.IcmsCTe;
                            }
                        }
                    }

                    ViewBag.TotalIcmsFreteATIE = totalIcmsFreteATIE;
                    ViewBag.TotalFecopFreteATIE = totalFecop1FreteATIE + totalFecop2FreteATIE;

                    decimal? icmsStATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.IcmsST).Sum()), 2),
                             icmsStATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.IcmsST).Sum()), 2),
                             totalApuradoATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.TotalICMS).Sum()), 2),
                             totalApuradoATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.TotalICMS).Sum()), 2),
                             totalDiefATSIE = (totalApuradoATSIE + totalIcmsFreteATIE),
                             totalDiefATIE = totalApuradoATIE;

                    ViewBag.TotatlApuradoATIE = totalApuradoATIE;
                    ViewBag.TotatlApuradoATSIE = totalApuradoATSIE;
                    ViewBag.TotalIcmsPagoATIE = icmsStATIE;
                    ViewBag.TotalIcmsPagoATSIE = icmsStATSIE;

                    ViewBag.TotalDiefATSIE = totalDiefATSIE;
                    ViewBag.TotalDiefATIE = totalDiefATIE;

                    decimal icmsAPagarATSIE = Convert.ToDecimal(totalDiefATSIE - icmsStATSIE),
                            icmsAPagarATIE = Convert.ToDecimal(totalDiefATIE - icmsStATIE);
                    ViewBag.IcmsAPagarATSIE = icmsAPagarATSIE;
                    ViewBag.IcmsAPagarATIE = icmsAPagarATIE;

                    decimal valorbase1ATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2),
                            valorbase1ATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2),
                            valorbase2ATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2),
                            valorbase2ATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2),
                            totalFecopCalcATIE = valorbase1ATIE + valorbase2ATIE,
                            totalFecopCalcATSIE = valorbase1ATSIE + valorbase2ATSIE;
                    ViewBag.TotalFecopCalculadaATIE = totalFecopCalcATIE;
                    ViewBag.TotalFecopCalculadaATSIE = totalFecopCalcATSIE;

                    decimal baseNfe1NormalATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2),
                            baseNfe1NormalATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2),
                            baseNfe1RetATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2),
                            baseNfe1RetATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2),
                            valorNfe1NormalATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe1RetATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe1NormalATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe1RetATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe2NormalATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe2RetATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),
                            valorNfe2NormalATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                            valorNfe2RetATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8) && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),
                            totalFecopNfeATIE = valorNfe1NormalATIE + valorNfe1RetATIE + valorNfe2NormalATIE + valorNfe2RetATIE,
                            totalFecopNfeATSIE = valorNfe1NormalATSIE + valorNfe1RetATSIE + valorNfe2NormalATSIE + valorNfe2RetATSIE;
                    ViewBag.TotalFecopNfeATIE = totalFecopNfeATIE;
                    ViewBag.TotalFecopNfeATSIE = totalFecopNfeATSIE;

                    decimal gnreNPagaFecopATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2),
                            gnreNPagaFecopATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.GnreFecop).Distinct().Sum()), 2);
                    ViewBag.GNREnPagaFecopATIE = gnreNPagaFecopATIE;
                    ViewBag.GNREnPagaFecopATSIE = gnreNPagaFecopATSIE;

                    decimal gnrePagaFecop1ATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2),
                            gnrePagaFecop1ATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.FecopGnre1).Distinct().Sum()), 2),
                            gnrePagaFecop2ATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2),
                            gnrePagaFecop2ATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.FecopGnre2).Distinct().Sum()), 2);

                    ViewBag.GNREPagaFecopATIE = gnrePagaFecop2ATIE + gnrePagaFecop1ATIE;
                    ViewBag.GNREPagaFecopATSIE = gnrePagaFecop2ATSIE + gnrePagaFecop1ATSIE;

                    decimal totalGnreFecopATIE = gnrePagaFecop1ATIE + gnrePagaFecop2ATIE;
                    ViewBag.TotalGnreFecopATIE = totalGnreFecopATIE;
                    decimal totalGnreFecopATSIE = gnrePagaFecop1ATSIE + gnrePagaFecop2ATSIE;
                    ViewBag.TotalGnreFecopATSIE = totalGnreFecopATSIE;

                    decimal totalfecopDiefATIE = totalFecopCalcATIE - totalGnreFecopATIE + (gnrePagaFecop2ATIE + gnrePagaFecop1ATIE) - totalFecopNfeATIE,
                            totalfecopDiefATSIE = totalFecopCalcATSIE - totalGnreFecopATSIE + (gnrePagaFecop2ATSIE + gnrePagaFecop1ATSIE) - totalFecopNfeATSIE;
                    ViewBag.TotalFecopDiefATIE = totalfecopDiefATIE;
                    ViewBag.TotalFecopDiefATSIE = totalfecopDiefATSIE;

                    decimal? icmsFecop1ATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2),
                             icmsFecop1ATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.Fecop1).Distinct().Sum()), 2),
                             icmsFecop2ATIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2),
                             icmsFecop2ATSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.TaxationTypeId.Equals(8)).Select(_ => _.Note.Fecop2).Distinct().Sum()), 2);
                    ViewBag.IcmsFecopATIE = icmsFecop1ATIE + icmsFecop2ATIE;
                    ViewBag.IcmsFecopATSIE = icmsFecop1ATSIE + icmsFecop2ATSIE;

                    ViewBag.TotalFinalFecopCalculadaATIE = Convert.ToDecimal(totalfecopDiefATIE - (icmsFecop1ATIE + icmsFecop2ATIE));
                    ViewBag.TotalFinalFecopCalculadaATSIE = Convert.ToDecimal(totalfecopDiefATSIE - (icmsFecop1ATSIE + icmsFecop2ATSIE));

                    if (totalDiefATSIE > 0)
                        totalApuradoST += Math.Round(Convert.ToDecimal(totalDiefATSIE), 2);

                    if (totalDiefATIE > 0)
                        totalApuradoST += Math.Round(Convert.ToDecimal(totalDiefATIE), 2);

                    totalRecolhidoST += Math.Round(Convert.ToDecimal(icmsStATSIE + icmsStATIE), 2);

                    if (icmsAPagarATSIE > 0)
                        totalDarST += Math.Round(icmsAPagarATSIE, 2);

                    if (icmsAPagarATIE > 0)
                        totalDarST += Math.Round(icmsAPagarATIE, 2);

                    if (totalfecopDiefATSIE > 0)
                        totalApuradoFecop += Math.Round(totalfecopDiefATSIE, 2);

                    if (totalfecopDiefATIE > 0)
                        totalApuradoFecop += Math.Round(totalfecopDiefATIE, 2);

                    totalRecolhidoFecop += Math.Round(Convert.ToDecimal(icmsFecop1ATSIE + icmsFecop2ATSIE + icmsFecop1ATIE + icmsFecop2ATIE), 2);

                    if (Convert.ToDecimal(totalfecopDiefATSIE - (icmsFecop1ATSIE + icmsFecop2ATSIE)) > 0)
                        totalDarFecop += Math.Round(Convert.ToDecimal(totalfecopDiefATSIE - (icmsFecop1ATSIE + icmsFecop2ATSIE)), 2);

                    if (Convert.ToDecimal(totalfecopDiefATIE - (icmsFecop1ATIE + icmsFecop2ATIE)) > 0)
                        totalDarFecop += Convert.ToDecimal(totalfecopDiefATIE - (icmsFecop1ATIE + icmsFecop2ATIE));

                    //  ANEXO CCCXVI
                    if (comp.Incentive.Equals(true) && Convert.ToInt32(comp.AnnexId) == 1)
                    {
                        if (impAnexo == null)
                        {
                            ViewBag.Erro = 2;
                            return View(null);
                        }

                        var mesAtual = importMes.NumberMonth(month);
                        var mesAnterior = importMes.NameMonthPrevious(mesAtual);
                        decimal saldoCredorAnterior = 0;

                        string ano = year;

                        if (mesAtual.Equals(1))
                            ano = (Convert.ToInt32(year) - 1).ToString();

                        var creditLast = _creditBalanceService.FindByLastMonth(id, mesAnterior, ano);

                        if (creditLast != null)
                            saldoCredorAnterior = Convert.ToDecimal(creditLast.Saldo);

                        var vendas = _vendaAnexoService.FindByVendasTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();
                        var devoFornecedors = _devoFornecedorService.FindByDevoTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();
                        var compras = _compraAnexoService.FindByComprasTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();
                        var devoClientes = _devoClienteService.FindByDevoTax(impAnexo.Id).OrderBy(_ => _.Aliquota).ToList();


                        decimal baseCalcFecop = Convert.ToDecimal(vendas.Where(_ => Convert.ToDecimal(_.Aliquota).Equals(18)).Sum(_ => _.Base)),
                                valorFecop = baseCalcFecop * 1 / 100;

                        //  Total
                        // A
                        decimal icmsTotalA = Convert.ToDecimal(compras.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsCompra4) +
                                            Convert.ToDecimal(impAnexo.IcmsCompra7) + Convert.ToDecimal(impAnexo.IcmsCompra12);
                        icmsTotalA -= (Convert.ToDecimal(devoFornecedors.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsDevoFornecedor4) +
                                        Convert.ToDecimal(impAnexo.IcmsDevoFornecedor7) + Convert.ToDecimal(impAnexo.IcmsDevoFornecedor12));
                        ViewBag.IcmsTotalA = icmsTotalA;

                        // D
                        decimal icmsTotalD = Convert.ToDecimal(vendas.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsVenda4) +
                                             Convert.ToDecimal(impAnexo.IcmsVenda7) + Convert.ToDecimal(impAnexo.IcmsVenda12);
                        icmsTotalD -= (Convert.ToDecimal(devoClientes.Sum(_ => _.Icms)) + Convert.ToDecimal(impAnexo.IcmsDevoCliente4) +
                                        Convert.ToDecimal(impAnexo.IcmsDevoCliente12));
                        ViewBag.IcmsTotalD = icmsTotalD;

                        decimal icmsAPAPagar = 0;

                        if (icmsAPagarAPSIE > 0)
                            icmsAPAPagar += icmsAPagarAPSIE;

                        if (icmsAPagarAPIE > 0)
                            icmsAPAPagar += icmsAPagarAPIE;

                        // Saldo Devedor
                        decimal saldoDevedor = icmsTotalD - icmsTotalA - icmsAPAPagar - saldoCredorAnterior;

                        if (saldoDevedor > 0)
                        {
                            totalApuradoFecop += Math.Round(valorFecop, 2);
                            totalDarFecop += Math.Round(valorFecop, 2);
                            totalDarIcms += Math.Round(saldoDevedor - valorFecop, 2);

                            ViewBag.IcmsAnexo = Math.Round(saldoDevedor - valorFecop, 2);
                            ViewBag.FecopAnexo = Math.Round(valorFecop, 2);
                        }
                        else
                        {
                            ViewBag.IcmsAnexo = 0;
                            ViewBag.FecopAnexo = 0;
                        }
                    }


                    //  Somatório Geral

                    // ICMS APURADO
                    ViewBag.TotalAPuradoST = totalApuradoST;
                    ViewBag.TotalAPuradoFecop = totalApuradoFecop;
                    ViewBag.TotalAPuradoAP = totalApuradoAP;
                    ViewBag.TotalAPuradoCO = totalApuradoCO;
                    ViewBag.TotalAPuradoIM = totalApuradoIM;

                    // ICMS RECOLHIDO
                    ViewBag.TotalRecolhidoST = totalRecolhidoST;
                    ViewBag.TotalRecolhidoFecop = totalRecolhidoFecop;
                    ViewBag.TotalRecolhidoAP = totalRecolhidoAP;
                    ViewBag.TotalRecolhidoCO = totalRecolhidoCO;
                    ViewBag.TotalRecolhidoIM = totalRecolhidoIM;

                    // ICMS A RECOLHER
                    ViewBag.TotalDarST = totalDarST;
                    ViewBag.TotalDarFecop = totalDarFecop;
                    ViewBag.TotalDarIM = totalDarIm;
                    ViewBag.TotalDarAP = totalDarAp;
                    ViewBag.TotalDarFunef = totalDarFunef;
                    ViewBag.TotalDarIcms = totalDarIcms;
                    ViewBag.TotalDarCotac = totalDarCotac;
                    ViewBag.TotalDarCO = totalDarCO;

                }
                else if (type.Equals(Model.Type.GNRE))
                {
                    products = _service.FindByProductsType(notesS, typeTaxation);
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
                                forenecedor.Add(prod.Note.Dhemi.ToString("dd/MM/yyyy"));
                                forenecedor.Add(prod.Note.Nnf);
                                forenecedor.Add(prod.Note.Xnome);
                                forenecedor.Add("0.00");
                                if (typeTaxation.Equals(1))
                                {
                                    forenecedor.Add(Convert.ToDecimal(prod.Note.GnreSt).ToString());
                                    forenecedor.Add(Convert.ToDecimal(prod.Note.GnreNSt).ToString());
                                    gnrePagaTotal += Convert.ToDecimal(prod.Note.GnreSt);
                                    gnreNPagaTotal += Convert.ToDecimal(prod.Note.GnreNSt);
                                }
                                else if (typeTaxation.Equals(2))
                                {
                                    forenecedor.Add(Convert.ToDecimal(prod.Note.GnreAp).ToString());
                                    forenecedor.Add(Convert.ToDecimal(prod.Note.GnreNAp).ToString());
                                    gnrePagaTotal += Convert.ToDecimal(prod.Note.GnreAp);
                                    gnreNPagaTotal += Convert.ToDecimal(prod.Note.GnreNAp);
                                }
                                else if (typeTaxation.Equals(3))
                                {
                                    forenecedor.Add(Convert.ToDecimal(prod.Note.GnreCo).ToString());
                                    forenecedor.Add(Convert.ToDecimal(prod.Note.GnreNCo).ToString());
                                    gnrePagaTotal += Convert.ToDecimal(prod.Note.GnreCo);
                                    gnreNPagaTotal += Convert.ToDecimal(prod.Note.GnreNCo);
                                }
                                else if (typeTaxation.Equals(5))
                                {
                                    forenecedor.Add(Convert.ToDecimal(prod.Note.GnreIm).ToString());
                                    forenecedor.Add(Convert.ToDecimal(prod.Note.GnreNIm).ToString());
                                    gnrePagaTotal += Convert.ToDecimal(prod.Note.GnreIm);
                                    gnreNPagaTotal += Convert.ToDecimal(prod.Note.GnreNIm);
                                }
                                else
                                {
                                    forenecedor.Add("0.00");
                                    forenecedor.Add("0.00");
                                    gnrePagaTotal += 0;
                                    gnreNPagaTotal += 0;
                                }
                                forenecedor.Add("0.00");
                                forenecedor.Add((Convert.ToDecimal(prod.Note.FecopGnre1) + Convert.ToDecimal(prod.Note.FecopGnre2)).ToString());
                                gnrefecopPagaTotal += Convert.ToDecimal(Convert.ToDecimal(prod.Note.FecopGnre1) + Convert.ToDecimal(prod.Note.FecopGnre2));
                                fornecedores.Add(forenecedor);
                                pos = fornecedores.Count() - 1;
                            }

                            fornecedores[pos][4] = (Convert.ToDecimal(fornecedores[pos][4]) + Convert.ToDecimal(prod.IcmsST)).ToString();
                            fornecedores[pos][7] = (Convert.ToDecimal(fornecedores[pos][7]) + Convert.ToDecimal(prod.VfcpST + prod.VfcpSTRet)).ToString();
                            icmsStTotal += Convert.ToDecimal(prod.IcmsST);
                            fecopStTotal += (Convert.ToDecimal(prod.VfcpST + Convert.ToDecimal(prod.VfcpSTRet)));
                        }

                    }

                    List<List<string>> fornecedoresFinal = new List<List<string>>();
                    for (int i = 0; i < fornecedores.Count(); i++)
                    {
                        if (Convert.ToDecimal(fornecedores[i][4]) > 0 || Convert.ToDecimal(fornecedores[i][7]) > 0)
                            fornecedoresFinal.Add(fornecedores[i]);
                    }
                    ViewBag.Fornecedores = fornecedoresFinal;
                    ViewBag.IcmsStTotal = icmsStTotal;
                    ViewBag.FecopStTotal = fecopStTotal;
                    ViewBag.GnrePagaTotal = gnrePagaTotal;
                    ViewBag.GnreNPagaTotal = gnreNPagaTotal;
                    ViewBag.GnreFecopPagaTotal = gnrefecopPagaTotal;

                }
                else if (type.Equals(Model.Type.IcmsST))
                {
                    var query = notes.GroupJoin(products.Where(_ => _.Status.Equals(true)).ToList(),
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
                        nnote.Add(item.vNF.ToString());
                        nnote.Add(item.IcmsST.ToString());
                        nnotes.Add(nnote);
                    }
                    ViewBag.NNotes = nnotes;

                }
                else if (type.Equals(Model.Type.ProdutoFI))
                {
                    products = _service.FindByProductsType(notes, Model.TypeTaxation.Nenhum)
                        .Where(_ => !_.TaxationType.Type.Equals("ST") && _.Incentivo.Equals(false))
                        .ToList();
                }
                else if (type.Equals(Model.Type.IcmsProdutor))
                {
                    if(impProdutor.Count() == 0 || impProdutor == null)
                    {
                        ViewBag.Erro = 3;
                        return View(null);
                    }

                    var icms = impProdutor.Sum(_ => _.Icms);
                    var vNF = impProdutor.Sum(_ => _.Vnf);
                    var baseCalc = impProdutor.Sum(_ => _.Vbasecalc);
                    ViewBag.Notas = impProdutor.OrderBy(_ => Convert.ToInt32(_.Nnf));
                    ViewBag.Vnf = vNF;
                    ViewBag.BaseCalc = baseCalc;
                    ViewBag.Icms = icms;
                }

                var dars = _darService.FindAll(GetLog(OccorenceLog.Read));

                // Código DAR Fecop
                var darFecop = dars.Where(_ => _.Type.Equals("Fecop"))
                    .FirstOrDefault();

                // Código DAR Substituição Tributária e Consumo
                var darStCo = dars.Where(_ => _.Type.Equals("ST-CO"))
                    .FirstOrDefault();

                // Código DAR Icms Normal
                var darIcms = dars.Where(_ => _.Type.Equals("Icms"))
                    .FirstOrDefault();

                // Código DAR Antecipação Parcial
                var darAp = dars.Where(_ => _.Type.Equals("AP"))
                    .FirstOrDefault();

                // Código DAR Imobilizado
                var darIm = dars.Where(_ => _.Type.Equals("IM"))
                    .FirstOrDefault();

                // Código DAR Funef
                var darFunef = dars.Where(_ => _.Type.Equals("Funef"))
                    .FirstOrDefault();
                
                // Código DAR Cotac
                var darCotac = dars.Where(_ => _.Type.Equals("Cotac"))
                    .FirstOrDefault();

                ViewBag.DarFecop = darFecop;
                ViewBag.DarSTCO = darStCo;
                ViewBag.DarIcms = darIcms;
                ViewBag.DarAP = darAp;
                ViewBag.DarIM = darIm;
                ViewBag.DarFunef = darFunef;
                ViewBag.DarCotac = darCotac;

                return View(products);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateBillet([FromBody] RequestBarCode requestBarCode)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            var messageResponse = new List<object>();

            List<EmailAddress> emailto = new List<EmailAddress>();
            foreach (var to in await _emailResponsibleService.GetByCompanyAsync(SessionManager.GetCompanyIdInSession()))
                emailto.Add(new EmailAddress() { Address = to.Email, Name = "" });

            if (emailto.Count <= 0)
            {
                messageResponse.Add(new { code = 500, recipedesc = "Email", recipecode = "Não cadastrado", message = "Essa empresa não possui destinatários cadastrados. Por favor, faça o cadastro dos destinatários dos boletos para esta empresa" });
                return Ok(new { code = 200, response = messageResponse });
            }

            var accessToken = _configurationService.FindByName("TokenAccessDarWs", null);
            if (accessToken == null) return BadRequest(new { code = 400, message = "O token de acesso não foi encontrado na base de dados" });

            var organCode = _configurationService.FindByName("CodigoOrgaoDarWs", null);
            if (organCode == null) return BadRequest(new { code = 400, message = "O código do orgão não foi encontrado na base de dados" });

            //O vencimento do boleto deve ser até dia 15 de todo mês, caso seja feriado ou final de semana, voltar para o dia útil anterior
            var dueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15);
            if (dueDate.DayOfWeek == DayOfWeek.Sunday)
                dueDate = dueDate.AddDays(-2);
            else if (dueDate.DayOfWeek == DayOfWeek.Saturday)
                dueDate = dueDate.AddDays(-1);

            if (requestBarCode.Vencimento.HasValue)
                dueDate = requestBarCode.Vencimento.Value;

            if (organCode == null) return BadRequest(new { code = 400, message = "A date de vencimento para o boleto não foi encontrado na base de dados" });

            var recipeCode = requestBarCode.RecipeCodeValues.GroupBy(x => new { x.RecipeCode, x.St });

            var dar = _darService.FindAll(GetLog(OccorenceLog.Read));

            var ie = _companyService.FindByDocument(requestBarCode.CpfCnpjIE);
            var recipeCodeValuesRecipesImplemented = _configuration["Sefaz:RecipesImplemented"].Split(',').ToList();

            foreach (var item in recipeCode)
            {
                try
                {
                    var hasValue = false;
                    var darCodes = requestBarCode.RecipeCodeValues.Where(x => x.RecipeCode.Equals(item.Key.RecipeCode));

                    foreach (var darC in darCodes)
                    {
                        if (!darC.Value.Equals("0"))
                        {
                            hasValue = true;
                            break;
                        }
                    }

                    if (!hasValue)
                        continue;

                    var substitoTributo = "0";
                    var substitoTributoLst = requestBarCode.RecipeCodeValues.Where(x => x.RecipeCode == item.Key.RecipeCode);
                    if (substitoTributoLst.Count() > 1)
                    {
                        substitoTributo = substitoTributoLst.First(x => x.Processed == 0).St.ToString();
                        substitoTributoLst.First(x => x.Processed == 0).Processed = 1;
                    }
                    else
                        substitoTributo = substitoTributoLst.First().St.ToString();
                    var st = int.Parse(substitoTributo);
                    var valueTotal = requestBarCode.RecipeCodeValues
                        .Where(x => x.RecipeCode.Equals(item.Key.RecipeCode) && !string.IsNullOrEmpty(x.Value) && x.St == st)
                        .Sum(x => Convert.ToDecimal(x.Value))
                        .ToString();

                    if (valueTotal == null || valueTotal == "0")
                        continue;

                    ResponseGetDarIcms response = new ResponseGetDarIcms();
                    string fileName = null;
                    string fileOutput = null;

                    if (!recipeCodeValuesRecipesImplemented.Contains(item.Key.RecipeCode))
                    {
                        var varResp = await _integrationWsDar.GetBarCodeAsync(new IntegrationDarService.solicitarCodigoBarrasRequest()
                        {
                            codigoOrgao = organCode.Value,
                            codigoReceita = item.Key.RecipeCode,
                            cpfCnpjIE = ie.Ie,
                            dataVencimento = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15).ToString("dd/MM/yyyy"),
                            numeroDocumento = requestBarCode.PeriodoReferencia,
                            periodoReferencia = requestBarCode.PeriodoReferencia,
                            tokenAcesso = accessToken.Value,
                            valorTotal = valueTotal
                        });

                        if (varResp.MessageType.ToLowerInvariant().Equals("erro"))
                        {
                            messageResponse.Add(new { code = 500, recipedesc = dar.FirstOrDefault(x => x.Code.Equals(item.Key.RecipeCode)).Description, recipecode = item.Key.RecipeCode, message = varResp.Message });
                            continue;
                        }

                        response.MensagemRetorno = varResp.Message;
                        response.BoletoBytes = null;
                        response.CodigoBarras = varResp.BarCode;
                        response.LinhaDigitavel = varResp.DigitableLine;
                        response.NumeroControle = varResp.ControlNumber;
                        response.NumeroDocumento = varResp.DocumentNumber;
                        response.TipoRetorno = varResp.MessageType;
                    }
                    else
                    {
                        response = await _integrationWsDar.RequestDarIcmsAsync(new IntegrationDarService.solicitarDarIcmsRequest()
                        {
                            codigoOrgao = organCode.Value,
                            codigoReceita = item.Key.RecipeCode,
                            dataPagamento = dueDate.ToString("dd/MM/yyyy"),
                            dataVencimento = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15).ToString("dd/MM/yyyy"),
                            inscricao = ie.Ie,
                            numeroDocumento = requestBarCode.PeriodoReferencia,
                            periodoReferencia = requestBarCode.PeriodoReferencia,
                            substitoTributo = substitoTributo,
                            taxaEmissao = "0",
                            tokenAcesso = accessToken.Value,
                            valorTotal = valueTotal
                        });

                        if (response.TipoRetorno.ToLowerInvariant().Equals("erro"))
                        {
                            messageResponse.Add(new { code = 500, recipedesc = dar.FirstOrDefault(x => x.Code.Equals(item.Key.RecipeCode)).Description, recipecode = item.Key.RecipeCode, message = response.MensagemRetorno });
                            continue;
                        }

                        var dirOutput = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot/Billets");
                        if (!System.IO.Directory.Exists(dirOutput))
                            System.IO.Directory.CreateDirectory(dirOutput);

                        fileName = $"{requestBarCode.CpfCnpjIE}-{requestBarCode.PeriodoReferencia}-{item.Key.RecipeCode}-{DateTime.Now.ToString("ddMMyyyy-HHmmss")}.pdf";
                        fileOutput = System.IO.Path.Combine(dirOutput, fileName);

                        System.IO.File.WriteAllBytes(fileOutput, Convert.FromBase64String(response.BoletoBytes));
                    }

                    //Cancelar caso já existe o documento na base de dados
                    var darDc = await _darDocumentService
                    .GetByCompanyAndPeriodReferenceAndDarAsync(
                        SessionManager.GetCompanyIdInSession(),
                        Convert.ToInt32(requestBarCode.PeriodoReferencia),
                        Convert.ToInt32(dar.FirstOrDefault(x => x.Code.Equals(item.Key.RecipeCode)).Id)
                    );

                    //Caso exista o DAR e ele esteja pago, não será mais possível editar
                    if (darDc != null && darDc.PaidOut)
                        continue;

                    //Caso exista o DAR, ele será cancelado e um novo será criado 
                    if (darDc != null)
                    {
                        darDc.Canceled = true;
                        _darDocumentService.Update(darDc, GetLog(OccorenceLog.Update));
                    }

                    //Gera novo Dar
                    var darDoc = _darDocumentService.Create(new DarDocument()
                    {
                        BarCode = response.CodigoBarras,
                        ControlNumber = int.Parse(response.NumeroControle),
                        Message = response.MensagemRetorno,
                        Created = DateTime.Now,
                        DigitableLine = response.LinhaDigitavel,
                        DocumentNumber = long.Parse($"{response.NumeroDocumento}{item.Key.RecipeCode}"),
                        MessageType = response.TipoRetorno,
                        Updated = DateTime.Now,
                        CompanyId = SessionManager.GetCompanyIdInSession(),
                        DarId = dar.FirstOrDefault(x => x.Code.Equals(item.Key.RecipeCode)).Id,
                        PaidOut = false,
                        PeriodReference = Convert.ToInt32(requestBarCode.PeriodoReferencia),
                        DueDate = dueDate,
                        BilletPath = fileName,
                        Canceled = false,
                        Value = Convert.ToDecimal(valueTotal)
                    }, null);

                    //Enviar Email
                    var subject = $"{ie.Document} - {requestBarCode.PeriodoReferencia} - Boleto ESCONPI {dar.FirstOrDefault(x => x.Code.Equals(item.Key.RecipeCode)).Description}";
                    var body = $@"Boleto de {dar.FirstOrDefault(x => x.Code.Equals(item.Key.RecipeCode)).Code} - {dar.FirstOrDefault(x => x.Code.Equals(item.Key.RecipeCode)).Description} 
                                  referente ao período {requestBarCode.PeriodoReferencia} com data de vencimento para {dueDate.ToString("dd/MM/yyyy")}";

                    var emailFrom = _emailConfiguration.SmtpUsername;

                    EmailMessage email = new EmailMessage()
                    {
                        FromAddresses = new List<EmailAddress>() { new EmailAddress() { Address = _emailConfiguration.SmtpUsername, Name = "Sistems SisCT - ESCONPI" } },
                        Subject = subject,
                        ToAddresses = emailto
                    };

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        email.Content = body;
                        _serviceEmail.Send(email, new string[] { fileOutput });
                    }
                    else
                    {
                        var contentBillet = System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DocumentModel", "billet-model.html"));
                        body += "<br/><br/>" + string.Format(contentBillet, response.LinhaDigitavel, response.CodigoBarras, new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15).ToString("dd/MM/yyyy"), valueTotal);

                        email.Content = body;
                        _serviceEmail.Send(email, null);
                    }

                    if (darDoc.Id <= 0) return BadRequest(new { code = 500, message = "falha ao tentar gravar dados de resposta do ws." });

                    var recipe = dar.FirstOrDefault(x => x.Code.Equals(item.Key.RecipeCode));
                    var recipedesc = recipe.Description;

                    messageResponse.Add(new { code = 200, recipecode = item.Key.RecipeCode, recipedesc, barcode = response.CodigoBarras, line = response.LinhaDigitavel, download = fileName });
                }
                catch (Exception ex)
                {
                    messageResponse.Add(new { code = 500, recipedesc = dar.FirstOrDefault(x => x.Code.Equals(item.Key.RecipeCode)).Description, recipecode = item.Key.RecipeCode, message = ex.Message });
                }
            }

            return Ok(new { code = 200, response = messageResponse });
        }

        [HttpGet]
        public async Task<IActionResult> GetDocumentsDar([FromQuery] int companyId, [FromQuery] int periodReference)
        {
            var messageResponse = new List<object>();

            var dar = _darService.FindAll(GetLog(OccorenceLog.Read));
            var documents = await _darDocumentService.GetByCompanyAndPeriodReferenceAsync(companyId, periodReference, false);
            foreach (var dc in documents)
            {
                var dr = dar.FirstOrDefault(x => x.Id.Equals(dc.DarId));
                messageResponse.Add(new { code = 200, recipecode = dr.Code, recipedesc = dr.Description, barcode = dc.BarCode, line = dc.DigitableLine, download = dc.BilletPath });
            }

            return Ok(new { code = 200, response = messageResponse });
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