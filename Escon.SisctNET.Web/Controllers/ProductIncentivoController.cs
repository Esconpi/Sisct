using System;
using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Escon.SisctNET.Web.Controllers
{
    public class ProductIncentivoController : ControllerBaseSisctNET
    {
        private readonly IProductIncentivoService _service;
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly ICstService _cstService;

        public ProductIncentivoController(
            IProductIncentivoService service,
            ICompanyService companyService,
            IConfigurationService configurationService,
            ICstService cstService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "ProductNote")
        {
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _cstService = cstService;
        }

        public IActionResult IndexAll(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Company = comp;
                SessionManager.SetCompanyIdInSession(id);

                return View(null);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Import()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _companyService.FindById(SessionManager.GetCompanyIdInSession(), null);
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Import(long companyid, string year, string month, string arquivo)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(companyid, null);
                var confDBSisctNfe = _configurationService.FindByName("NFe Saida", null);
                var confDateNfe = _configurationService.FindByName("Data Inicio", null);

                var importXml = new Xml.Import();
                var importDir = new Diretorio.Import();

                string directoryNfe = "",arqui = "";

                if (arquivo.Equals("xmlE"))
                {
                    directoryNfe = importDir.SaidaEmpresa(comp, confDBSisctNfe.Value, year, month);
                    arqui = "XML EMPRESA";
                }
                else
                {
                    directoryNfe = importDir.SaidaSefaz(comp, confDBSisctNfe.Value, year, month);
                    arqui = "XML SEFAZ";
                }

                List<Dictionary<string, string>> products = new List<Dictionary<string, string>>();

                products = importXml.NFeProduct(directoryNfe);

                var productsAll = _service.FindByAllProducts(comp.Document);

                List<Model.ProductIncentivo> addProducts = new List<Model.ProductIncentivo>();


                foreach (var prod in products)
                {
                    string cProd = "", ncm = "", cest = "";
                    if (prod.ContainsKey("cProd"))
                    {
                        cProd = prod["cProd"];
                    }

                    if (prod.ContainsKey("NCM"))
                    {
                        ncm = prod["NCM"];
                    }

                    if (prod.ContainsKey("CEST"))
                    {
                        cest = prod["CEST"];
                        if (cest.Equals(""))
                        {
                            cest = "";
                        }
                    }

                    var prodImport = productsAll.Where(_ => _.Code.Equals(cProd) && _.Ncm.Equals(ncm) && _.Cest.Equals(cest)).FirstOrDefault();
                    if (prodImport == null)
                    {
                        Model.ProductIncentivo product = new Model.ProductIncentivo();
                        product.Code = prod["cProd"];
                        product.Ncm = prod["NCM"];
                        product.Name = prod["xProd"];
                        product.Cest = cest;
                        product.TypeTaxation = "";
                        product.Active = false;
                        product.CompanyId = companyid;
                        product.Month = month;
                        product.Year = year;
                        product.Arquivo = arqui;
                        product.DateStart = Convert.ToDateTime(confDateNfe.Value);
                        product.Created = DateTime.Now;
                        product.Updated = DateTime.Now;

                        addProducts.Add(product);
                    }
                }

                _service.Create(addProducts, GetLog(OccorenceLog.Create));

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                return RedirectToAction("Index", new { companyId = companyid, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Details(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);

                if (result.DateEnd != null)
                {
                    return RedirectToAction("Index", new { id = result.CompanyId });
                }
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.ProductIncentivo entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var prod = _service.FindById(id, null);
                long companyId = prod.CompanyId;
                var year = prod.Year;
                var month = prod.Month;

                var comp = _companyService.FindById(companyId, null);

                var ncmRaiz = Request.Form["NcmRaiz"].ToString().Replace(".","");

                List<Model.ProductIncentivo> updateProducts = new List<Model.ProductIncentivo>();

                if (Request.Form["type"].ToString() == "1")
                {
                    prod.TypeTaxation = entity.TypeTaxation;
                    prod.Bcr = entity.Bcr;
                    prod.PercentualBcr = entity.PercentualBcr;
                    prod.PercentualInciso = entity.PercentualInciso;
                    prod.DateStart = entity.DateStart;
                    prod.Active = true;
                    prod.Updated = DateTime.Now;
                    if (comp.TypeCompany.Equals(false) && prod.TypeTaxation.Equals("Incentivado"))
                    {
                        prod.Percentual = entity.Percentual;
                    }

                    updateProducts.Add(prod);
                }
                else if(Request.Form["type"].ToString() == "2")
                {
                    if (ncmRaiz == "")
                    {
                        var products = _service.FindAll(null)
                            .Where(_ => _.CompanyId.Equals(companyId) && _.Ncm.Equals(prod.Ncm) && _.DateEnd.Equals(null))
                            .ToList();

                        foreach (var p in products)
                        {
                            p.TypeTaxation = entity.TypeTaxation;
                            p.Bcr = entity.Bcr;
                            p.PercentualBcr = entity.PercentualBcr;
                            p.PercentualInciso = entity.PercentualInciso;
                            p.DateStart = entity.DateStart;
                            p.Active = true;
                            p.Updated = DateTime.Now;
                            if (comp.TypeCompany.Equals(false) && p.TypeTaxation.Equals("Incentivado"))
                            {
                                p.Percentual = entity.Percentual;
                                p.DateStart = entity.DateStart;
                            }
                            updateProducts.Add(p);
                        }

                    }
                    else
                    {
                        var products = _service.FindAll(null).Where(_ => _.CompanyId.Equals(companyId)).ToList();
                        products = _service.FindByProducts(products, ncmRaiz);
                        foreach (var p in products)
                        {
                            p.TypeTaxation = entity.TypeTaxation;
                            p.Bcr = entity.Bcr;
                            p.PercentualBcr = entity.PercentualBcr;
                            p.PercentualInciso = entity.PercentualInciso;
                            p.DateStart = entity.DateStart;
                            p.Active = true;
                            p.Updated = DateTime.Now;
                            if (comp.TypeCompany.Equals(false) && p.TypeTaxation.Equals("Incentivado"))
                            {
                                p.Percentual = entity.Percentual;
                                p.DateStart = entity.DateStart;
                            }
                            updateProducts.Add(p);
                        }
                    }
                }
                else if (Request.Form["type"].ToString() == "3")
                {
                    var products = _service.FindAll(null).Where(_ => _.CompanyId.Equals(companyId) && _.DateEnd.Equals(null)).ToList();

                    foreach (var p in products)
                    {
                        p.TypeTaxation = entity.TypeTaxation;
                        p.Bcr = entity.Bcr;
                        p.PercentualBcr = entity.PercentualBcr;
                        p.PercentualInciso = entity.PercentualInciso;
                        p.DateStart = entity.DateStart;
                        p.Active = true;
                        p.Updated = DateTime.Now;
                        if (comp.TypeCompany.Equals(false) && p.TypeTaxation.Equals("Incentivado"))
                        {
                            p.Percentual = entity.Percentual;
                        }

                        updateProducts.Add(p);
                    }
                }

                _service.Update(updateProducts, GetLog(OccorenceLog.Update));

                return RedirectToAction("IndexAll", new { id = companyId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Index(long companyId, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(companyId, null);
                ViewBag.Company = comp;
                SessionManager.SetCompanyIdInSession(companyId);
                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);
                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Product(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var prod = _service.FindById(id, null);

                return View(prod);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Product(long id, string year, string month, Model.ProductIncentivo entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var prod = _service.FindById(id, null);
                var companyId = prod.CompanyId;

                var comp = _companyService.FindById(prod.CompanyId, null);

                var ncmRaiz = Request.Form["NcmRaiz"].ToString().Replace(".", "");

                List<Model.ProductIncentivo> updateProducts = new List<Model.ProductIncentivo>();

                if (Request.Form["type"].ToString() == "1")
                {
                    prod.TypeTaxation = entity.TypeTaxation;
                    prod.Bcr = entity.Bcr;
                    prod.PercentualBcr = entity.PercentualBcr;
                    prod.PercentualInciso = entity.PercentualInciso;
                    prod.DateStart = entity.DateStart;
                    prod.Active = true;
                    prod.Updated = DateTime.Now;
                    if (comp.TypeCompany.Equals(false) && prod.TypeTaxation.Equals("Incentivado"))
                    {
                        prod.Percentual = entity.Percentual;
                        prod.DateStart = entity.DateStart;
                    }
                    updateProducts.Add(prod);
                }
                else if (Request.Form["type"].ToString() == "2")
                {
                    if(ncmRaiz == "")
                    {
                        var products = _service.FindAll(null).Where(_ => _.CompanyId.Equals(companyId) && _.Year.Equals(year) && _.Month.Equals(month) && _.Ncm.Equals(prod.Ncm)).ToList();

                        foreach (var p in products)
                        {
                            p.TypeTaxation = entity.TypeTaxation;
                            p.Bcr = entity.Bcr;
                            p.PercentualBcr = entity.PercentualBcr;
                            p.PercentualInciso = entity.PercentualInciso;
                            p.DateStart = entity.DateStart;
                            p.Active = true;
                            p.Updated = DateTime.Now;
                            if (comp.TypeCompany.Equals(false) && p.TypeTaxation.Equals("Incentivado"))
                            {
                                p.Percentual = entity.Percentual;
                                p.DateStart = entity.DateStart;
                            }
                            updateProducts.Add(p);
                        }

                    }
                    else
                    {
                        var products = _service.FindAll(null).Where(_ => _.CompanyId.Equals(companyId) && _.Year.Equals(year) && _.Month.Equals(month)).ToList();
                        products = _service.FindByProducts(products, ncmRaiz);
                        foreach (var p in products)
                        {
                            p.TypeTaxation = entity.TypeTaxation;
                            p.Bcr = entity.Bcr;
                            p.PercentualBcr = entity.PercentualBcr;
                            p.PercentualInciso = entity.PercentualInciso;
                            p.DateStart = entity.DateStart;
                            p.Active = true;
                            p.Updated = DateTime.Now;
                            if (comp.TypeCompany.Equals(false) && p.TypeTaxation.Equals("Incentivado"))
                            {
                                p.Percentual = entity.Percentual;
                                p.DateStart = entity.DateStart;
                            }
                            updateProducts.Add(p);
                        }
                    }
                }
                else if (Request.Form["type"].ToString() == "3")
                {
                    var products = _service.FindAll(null).Where(_ => _.CompanyId.Equals(companyId) &&
                    _.Year.Equals(year) && _.Month.Equals(month)).ToList();

                    foreach (var p in products)
                    {
                        p.TypeTaxation = entity.TypeTaxation;
                        p.Bcr = entity.Bcr;
                        p.PercentualBcr = entity.PercentualBcr;
                        p.PercentualInciso = entity.PercentualInciso;
                        p.DateStart = entity.DateStart;
                        p.Active = true;
                        p.Updated = DateTime.Now;
                        if (comp.TypeCompany.Equals(false) && p.TypeTaxation.Equals("Incentivado"))
                        {
                            p.Percentual = entity.Percentual;
                            p.DateStart = entity.DateStart;
                        }
                        updateProducts.Add(p);
                    }
                }
                
                _service.Update(updateProducts, GetLog(OccorenceLog.Update));
                
                return RedirectToAction("Index", new { companyId = companyId, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Atualize(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);

                if (result.DateEnd != null)
                {
                    return RedirectToAction("Index", new { id = result.CompanyId });
                }
                ViewBag.CompanyId = result.CompanyId;
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Atualize(long id, Model.ProductIncentivo entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);

                List<Model.ProductIncentivo> createProducts = new List<Model.ProductIncentivo>();
                List<Model.ProductIncentivo> updateProducts = new List<Model.ProductIncentivo>();

                if (Request.Form["type"].ToString() == "1")
                {
                    result.DateEnd = Convert.ToDateTime(entity.DateStart).AddDays(-1);
                    result.Updated = DateTime.Now;

                    updateProducts.Add(result);

                    ProductIncentivo prod = new ProductIncentivo();

                    prod.Arquivo = result.Arquivo;
                    prod.Code = result.Code;
                    prod.Ncm = result.Ncm;
                    prod.Name = result.Name;
                    prod.Cest = result.Cest;
                    prod.Active = result.Active;
                    prod.CompanyId = result.CompanyId;
                    prod.Month = result.Month;
                    prod.Year = result.Year;
                    prod.DateStart = entity.DateStart;
                    prod.DateEnd = null;
                    prod.TypeTaxation = result.TypeTaxation;
                    prod.Percentual = entity.Percentual;
                    prod.PercentualBcr = entity.PercentualBcr;
                    prod.PercentualInciso = entity.PercentualInciso;
                    prod.CstId = result.CstId;
                    prod.Bcr = result.Bcr;
                    prod.Created = DateTime.Now;
                    prod.Updated = prod.Created;

                    updateProducts.Add(result);
                    createProducts.Add(prod);
                }
                else if (Request.Form["type"].ToString() == "2")
                {

                    var products = _service.FindAll(null)
                        .Where(_ => _.CompanyId.Equals(result.CompanyId) && _.Ncm.Equals(result.Ncm) && _.DateEnd.Equals(null) && _.DateStart < entity.DateStart)
                        .ToList();

                    foreach (var p in products)
                    {
                        p.DateEnd = Convert.ToDateTime(entity.DateStart).AddDays(-1);
                        p.Updated = DateTime.Now;

                        updateProducts.Add(p);

                        ProductIncentivo prod = new ProductIncentivo();

                        prod.Arquivo = p.Arquivo;
                        prod.Code = p.Code;
                        prod.Ncm = p.Ncm;
                        prod.Name = p.Name;
                        prod.Cest = p.Cest;
                        prod.Active = p.Active;
                        prod.CompanyId = p.CompanyId;
                        prod.Month = p.Month;
                        prod.Year = p.Year;
                        prod.DateStart = entity.DateStart;
                        prod.DateEnd = null;
                        prod.TypeTaxation = p.TypeTaxation;
                        prod.Percentual = entity.Percentual;
                        prod.PercentualBcr = entity.PercentualBcr;
                        prod.PercentualInciso = entity.PercentualInciso;
                        prod.CstId = p.CstId;
                        prod.Bcr = p.Bcr;
                        prod.Created = DateTime.Now;
                        prod.Updated = prod.Created;

                        createProducts.Add(prod);
                    }

                }
                else if (Request.Form["type"].ToString() == "3")
                {
                    var products = _service.FindAll(null).Where(_ => _.CompanyId.Equals(result.CompanyId) && _.DateEnd.Equals(null) && _.DateStart < entity.DateStart).ToList();

                    foreach (var p in products)
                    {
                        p.DateEnd = Convert.ToDateTime(entity.DateStart).AddDays(-1);
                        p.Updated = DateTime.Now;

                        updateProducts.Add(p);

                        ProductIncentivo prod = new ProductIncentivo();

                        prod.Arquivo = p.Arquivo;
                        prod.Code = p.Code;
                        prod.Ncm = p.Ncm;
                        prod.Name = p.Name;
                        prod.Cest = p.Cest;
                        prod.Active = p.Active;
                        prod.CompanyId = p.CompanyId;
                        prod.Month = p.Month;
                        prod.Year = p.Year;
                        prod.DateStart = entity.DateStart;
                        prod.DateEnd = null;
                        prod.TypeTaxation = p.TypeTaxation;
                        prod.Percentual = entity.Percentual;
                        prod.PercentualBcr = entity.PercentualBcr;
                        prod.PercentualInciso = entity.PercentualInciso;
                        prod.CstId = p.CstId;
                        prod.Bcr = p.Bcr;
                        prod.Created = DateTime.Now;
                        prod.Updated = prod.Created;

                        createProducts.Add(prod);
                    }
                }

                _service.Update(updateProducts, GetLog(OccorenceLog.Update));
                _service.Create(createProducts, GetLog(OccorenceLog.Create));

                return RedirectToAction("IndexAll", new { id = result.CompanyId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Lista()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _companyService.FindById(SessionManager.GetCompanyIdInSession(), null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Listagem(string tipo)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                long companyid = SessionManager.GetCompanyIdInSession();

                List<ProductIncentivo> products = new List<ProductIncentivo>();
                if (tipo.Equals("Incentivado/Normal"))
                {
                    products = _service.FindByAllProducts(companyid).Where(_ => _.TypeTaxation.Equals("Incentivado/Normal")).ToList();
                }
                else if (tipo.Equals("Incentivado/ST"))
                {
                    products = _service.FindByAllProducts(companyid).Where(_ => _.TypeTaxation.Equals("Incentivado/ST")).ToList();
                }
                else if (tipo.Equals("ST"))
                {
                    products = _service.FindByAllProducts(companyid).Where(_ => _.TypeTaxation.Equals("ST")).ToList();
                }
                else if (tipo.Equals("Isento"))
                {
                    products = _service.FindByAllProducts(companyid).Where(_ => _.TypeTaxation.Equals("Isento")).ToList();
                }
                else if (tipo.Equals("Normal"))
                {
                    products = _service.FindByAllProducts(companyid).Where(_ => _.TypeTaxation.Equals("Normal")).ToList();
                }
                var comp = _companyService.FindById(companyid, null);
   
                ViewBag.Tipo = tipo;
                ViewBag.Company = comp;
                return PartialView(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Delete(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("ProductNote")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var product = _service.FindById(id, null);
                var comp = _companyService.FindById(product.CompanyId, null);
                _service.Delete(id, GetLog(OccorenceLog.Delete));
                return RedirectToAction("Index", new { id = comp.Id});
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult GetAllCompany(int draw, int start)
        {
            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var produtosAll = _service.FindByAllProducts(SessionManager.GetCompanyIdInSession())
                .OrderBy(_ => _.DateEnd)
                .ThenBy(_ => _.Active)
                .ToList();

            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<ProductIncentivo> produtos = new List<ProductIncentivo>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<ProductIncentivo> productTemp = new List<ProductIncentivo>();
                produtosAll.ToList().ForEach(s =>
                {
                    s.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.Name);
                    productTemp.Add(s);
                });

                var ids = productTemp.Where(c =>
                    c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Ncm.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                produtos = produtosAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var product = from r in produtos
                              where ids.ToArray().Contains(r.Id)
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  Product = r.Code + " - " + r.Name,
                                  Ncm = r.Ncm,
                                  Ccest = r.Cest,
                                  Active = r.Active,
                                  TipoTaxation = r.TypeTaxation,
                                  Percentual = r.Company.ChapterId == 4 ? r.PercentualInciso : r.Percentual,
                                  Inicio = Convert.ToDateTime(r.DateStart).ToString("dd/MM/yyyy"),
                                  Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy"),

                               };

                return Ok(new { draw = draw, recordsTotal = produtos.Count(), recordsFiltered = produtos.Count(), data = product.Skip(start).Take(lenght) });

            }
            else
            {


                var product = from r in produtosAll
                           select new
                           {
                               Id = r.Id.ToString(),
                               Product = r.Code + " - " + r.Name,
                               Ncm = r.Ncm,
                               Ccest = r.Cest,
                               Active = r.Active,
                               TipoTaxation = r.TypeTaxation,
                               Percentual = r.Company.ChapterId == 4 ? r.PercentualInciso : r.Percentual,
                               Inicio = Convert.ToDateTime(r.DateStart).ToString("dd/MM/yyyy"),
                               Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy"),

                           };
                return Ok(new { draw = draw, recordsTotal = produtosAll.Count(), recordsFiltered = produtosAll.Count(), data = product.Skip(start).Take(lenght) });
            }

        }

        public IActionResult GetAll(int draw, int start)
        {

            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var produtosAll = _service.FindByProducts(SessionManager.GetCompanyIdInSession(),SessionManager.GetYearInSession(),SessionManager.GetMonthInSession())
                .OrderBy(_ => _.Active)
                .ToList();

            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<ProductIncentivo> produtos = new List<ProductIncentivo>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<ProductIncentivo> productTemp = new List<ProductIncentivo>();
                produtosAll.ToList().ForEach(s =>
                {
                    s.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.Name);
                    productTemp.Add(s);
                });

                var ids = productTemp.Where(c =>
                    c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Ncm.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                produtos = produtosAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var product = from r in produtos
                              where ids.ToArray().Contains(r.Id)
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  Product = r.Code + " - " + r.Name,
                                  Ncm = r.Ncm,
                                  Ccest = r.Cest,
                                  Active = r.Active,
                                  TipoTaxation = r.TypeTaxation,
                                  Inicio = Convert.ToDateTime(r.DateStart).ToString("dd/MM/yyyy"),
                                  Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy"),

                              };

                return Ok(new { draw = draw, recordsTotal = produtos.Count(), recordsFiltered = produtos.Count(), data = product.Skip(start).Take(lenght) });

            }
            else
            {


                var product = from r in produtosAll
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  Product = r.Code + " - " + r.Name,
                                  Ncm = r.Ncm,
                                  Ccest = r.Cest,
                                  Active = r.Active,
                                  TipoTaxation = r.TypeTaxation,
                                  Inicio = Convert.ToDateTime(r.DateStart).ToString("dd/MM/yyyy"),
                                  Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy"),

                              };
                return Ok(new { draw = draw, recordsTotal = produtosAll.Count(), recordsFiltered = produtosAll.Count(), data = product.Skip(start).Take(lenght) });
            }

        }
    }
}