using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Taxation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Escon.SisctNET.Web.Controllers
{
    public class ProductIncentivoController : ControllerBaseSisctNET
    {
        private readonly IProductIncentivoService _service;
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;

        public ProductIncentivoController(
            IProductIncentivoService service,
            ICompanyService companyService,
            IConfigurationService configurationService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "ProductIncentivo")
        {
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
        }

        public IActionResult Index(int companyId)
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
                    var comp = _companyService.FindById(companyId, null);
                    ViewBag.Id = comp.Id;
                    ViewBag.SocialName = comp.SocialName;
                    ViewBag.Document = comp.Document;
                    ViewBag.Status = comp.Status;
                    ViewBag.TypeCompany = comp.TypeCompany;

                    SessionManager.SetCompanyIdInSession(companyId);

                    return View(null);
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Import(int id)
        {
            try
            {
                var result = _companyService.FindById(id, null);
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }


        [HttpPost]
        public IActionResult Import(int id, string year, string month)
        {
            try
            {
                var comp = _companyService.FindById(id, null);
                var confDBSisctNfe = _configurationService.FindByName("NFe Saida", null);

                int cont = 0;
                var import = new Import();

                string directoryNfe = confDBSisctNfe.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                List<Dictionary<string, string>> products = new List<Dictionary<string, string>>();

                products = import.NfeExitProducts(directoryNfe);

                foreach (var prod in products)
                {
                    var prodImport = _service.FindByProduct(id, prod["cProd"], prod["NCM"]);
                    if (prodImport == null)
                    {
                        var product = new Model.ProductIncentivo
                        {
                            Code = prod["cProd"],
                            Ncm = prod["NCM"],
                            Name = prod["xProd"],
                            TypeTaxation = "",
                            Active = false,
                            CompanyId = id,
                            Month = month,
                            Year = year,
                            Created = DateTime.Now,
                            Updated = DateTime.Now
                        };
                        _service.Create(entity: product, null);
                        cont++;
                    }
                }

                return RedirectToAction("Details", new { companyId = id, count = cont });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                var result = _service.FindById(id, null);
                var comp = _companyService.FindById(result.CompanyId, null);
                ViewBag.TypeCompany = comp.TypeCompany;
                ViewBag.CompanyId = comp.Id;
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(int id, Model.ProductIncentivo entity)
        {
            try
            {
                var prod = _service.FindById(id, null);
                var companyId = prod.CompanyId;
                var year = prod.Year;
                var month = prod.Month;

                var comp = _companyService.FindById(companyId, null);

                if (Request.Form["type"].ToString() == "1")
                {
                    prod.TypeTaxation = Request.Form["taxation"].ToString();
                    prod.Active = true;
                    prod.Updated = DateTime.Now;
                    if (comp.TypeCompany.Equals(false) && prod.TypeTaxation.Equals("Incentivado"))
                    {
                        prod.Percentual = entity.Percentual;
                        prod.DateStart = entity.DateStart;
                    }
                    
                    _service.Update(prod, null);
                }
                else if(Request.Form["type"].ToString() == "2")
                {
                    var products = _service.FindAll(null).Where(_ => _.CompanyId.Equals(companyId) && _.Ncm.Equals(prod.Ncm)).ToList();

                    foreach (var p in products)
                    {
                        p.TypeTaxation = Request.Form["taxation"].ToString();
                        p.Active = true;
                        p.Updated = DateTime.Now;
                        if (comp.TypeCompany.Equals(false) && p.TypeTaxation.Equals("Incentivado"))
                        {
                            p.Percentual =  entity.Percentual;
                            p.DateStart = entity.DateStart;
                        }

                        _service.Update(p, null);
                    }
                }
                else if (Request.Form["type"].ToString() == "3")
                {
                    var products = _service.FindAll(null).Where(_ => _.CompanyId.Equals(companyId)).ToList();

                    foreach (var p in products)
                    {
                        p.TypeTaxation = Request.Form["taxation"].ToString();
                        p.Active = true;
                        p.Updated = DateTime.Now;
                        if (comp.TypeCompany.Equals(false) && p.TypeTaxation.Equals("Incentivado"))
                        {
                            p.Percentual = entity.Percentual;
                            p.DateStart = entity.DateStart;
                        }

                        _service.Update(p, null);
                    }
                }

                return RedirectToAction("Index", new { companyId = companyId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Details(int companyId, int count)
        {
            try
            {
                ViewBag.CompanyId = companyId;
                var comp = _companyService.FindById(companyId, null);
                var products = _service.FindAll(null).Where(_ => _.CompanyId.Equals(companyId)).Reverse();
                var result = products.Take(count).ToList();
                ViewBag.Count = count;
                ViewBag.TypeCompany = comp.TypeCompany;
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Product(int id, int count)
        {
            try
            {
                var prod = _service.FindById(id, null);
                var comp = _companyService.FindById(prod.CompanyId, null);

                ViewBag.CompanyId = prod.CompanyId;
                ViewBag.Month = prod.Month;
                ViewBag.Year = prod.Year;
                ViewBag.TypeCompany = comp.TypeCompany;
                ViewBag.Count = count;
                return View(prod);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Product(int id, int count, Model.ProductIncentivo entity)
        {
            try
            {
                var prod = _service.FindById(id, null);
                var companyId = prod.CompanyId;
                var year = prod.Year;
                var month = prod.Month;

                var comp = _companyService.FindById(prod.CompanyId, null);

                if (Request.Form["type"].ToString() == "1")
                {
                    prod.TypeTaxation = Request.Form["taxation"].ToString();
                    prod.Active = true;
                    prod.Updated = DateTime.Now;
                    if (comp.TypeCompany.Equals(false) && prod.TypeTaxation.Equals("Incentivado"))
                    {
                        prod.Percentual = entity.Percentual;
                        prod.DateStart = entity.DateStart;
                    }
                    _service.Update(prod, null);
                }
                else if (Request.Form["type"].ToString() == "2")
                {
                    var products = _service.FindAll(null).Where(_ => _.CompanyId.Equals(companyId) &&
                    _.Year.Equals(year) && _.Month.Equals(month) && _.Ncm.Equals(prod.Ncm)).ToList();

                    foreach (var p in products)
                    {
                        p.TypeTaxation = Request.Form["taxation"].ToString();
                        p.Active = true;
                        p.Updated = DateTime.Now;
                        if (comp.TypeCompany.Equals(false) && p.TypeTaxation.Equals("Incentivado"))
                        {
                            p.Percentual = entity.Percentual;
                            p.DateStart = entity.DateStart;
                        }
                        _service.Update(p, null);
                    }
                }
                else if (Request.Form["type"].ToString() == "3")
                {
                    var products = _service.FindAll(null).Where(_ => _.CompanyId.Equals(companyId) &&
                    _.Year.Equals(year) && _.Month.Equals(month)).ToList();

                    foreach (var p in products)
                    {
                        p.TypeTaxation = Request.Form["taxation"].ToString();
                        p.Active = true;
                        p.Updated = DateTime.Now;
                        if (comp.TypeCompany.Equals(false) && p.TypeTaxation.Equals("Incentivado"))
                        {
                            p.Percentual = entity.Percentual;
                            p.DateStart = entity.DateStart;
                        }
                        _service.Update(p, null);
                    }
                }

                return RedirectToAction("Details", new { companyId = companyId, count = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Atualize(int id)
        {
            try
            {
                var result = _service.FindById(id, null);
                ViewBag.CompanyId = result.CompanyId;
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Atualize(int id, Model.ProductIncentivo entity)
        {
            try
            {
                var result = _service.FindById(id, null);
                result.Percentual = entity.Percentual;
                result.DateEnd = Convert.ToDateTime(entity.DateStart).AddDays(-1);
                result.Updated = DateTime.Now;
                _service.Update(result, null);

                entity.Code = result.Code;
                entity.Ncm = result.Ncm;
                entity.Name = result.Name;
                entity.Active = result.Active;
                entity.CompanyId = result.CompanyId;
                entity.Month = result.Month;
                entity.Year = result.Year;
                _service.Create(entity, null);


                return RedirectToAction("Index", new { companyId = result.CompanyId });
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

            var produtosAll = _service.FindByAllProducts(SessionManager.GetCompanyIdInSession());

            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<ProductIncentivo> produtos = new List<ProductIncentivo>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<ProductIncentivo> productTemp = new List<ProductIncentivo>();
                produtosAll.ToList().ForEach(s =>
                {
                    s.Code = s.Code;
                    s.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.Name);
                    s.Ncm = s.Ncm;
                    s.Active = s.Active;
                    s.TypeTaxation = s.TypeTaxation;
                    s.DateStart = s.DateStart;
                    s.DateEnd = s.DateEnd;
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
                               Code = r.Code,
                               Name = r.Name,
                               Nncm = r.Ncm,
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
                               Code = r.Code,
                               Name = r.Name,
                               Nncm = r.Ncm,
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