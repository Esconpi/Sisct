using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

                    var result = _service.FindByAllProducts(companyId).TakeLast(1000);
                    return View(result);
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

                return RedirectToAction("Details", new { id = id, count = cont });
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
                    if (comp.TypeCompany.Equals(false))
                    {
                        prod.Percentual = entity.Percentual;
                    }
                    
                    _service.Update(prod, null);
                }
                else
                {
                    var products = _service.FindAll(null).Where(_ => _.CompanyId.Equals(companyId)).ToList();

                    foreach (var p in products)
                    {
                        p.TypeTaxation = Request.Form["taxation"].ToString();
                        p.Active = true;
                        p.Updated = DateTime.Now;
                        if (comp.TypeCompany.Equals(false))
                        {
                            p.Percentual =  entity.Percentual;
                        }

                        _service.Update(p, null);
                    }
                }

                return RedirectToAction("Index", new { id = companyId });
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
                var products = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.CompanyId.Equals(companyId)).Reverse();
                var result = products.Take(count).ToList();
                ViewBag.Count = count;
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
                //ViewBag.TypeCompany = comp.TypeCompany;
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
                    if (comp.TypeCompany.Equals(false))
                    {
                        prod.Percentual = entity.Percentual;
                    }
                    _service.Update(prod, null);
                }
                else
                {
                    var products = _service.FindAll(null).Where(_ => _.CompanyId.Equals(companyId) &&
                    _.Year.Equals(year) && _.Month.Equals(month) && _.Ncm.Equals(prod.Ncm) &&
                    _.Active.Equals(false)).ToList();

                    foreach (var p in products)
                    {
                        p.TypeTaxation = Request.Form["taxation"].ToString();
                        p.Active = true;
                        p.Updated = DateTime.Now;
                        if (comp.TypeCompany.Equals(false))
                        {
                            p.Percentual = entity.Percentual;
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

    }
}