﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Ato;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Escon.SisctNET.Web.Controllers
{
    public class ProductController : ControllerBaseSisctNET
    {
        private readonly IProduct2Service _service;
        private readonly IGroupService _groupService;
        private readonly IHostingEnvironment _appEnvironment;

        public ProductController(
            IProduct2Service service,
            IGroupService groupService,
            IHostingEnvironment env,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Product2")
        {
            _service = service;
            _groupService = groupService;
            _appEnvironment = env;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault() == null)
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

                    return View(null);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Create()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                List<Model.Group> lista_group = _groupService.FindAll(GetLog(Model.OccorenceLog.Read));

                foreach (var g in lista_group)
                {
                    g.Description = g.Item + " - " + g.Description;
                }

                lista_group.Insert(0, new Model.Group() { Description = "Nennhum item selecionado", Id = 0 });
                SelectList groups = new SelectList(lista_group, "Id", "Description", null);
                ViewBag.GroupId = groups;

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpPost]
        public IActionResult Create(Model.Product2 entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                var lastId = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Max(_ => _.Id);
                decimal price = Convert.ToDecimal(Request.Form["price"]);
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;
                entity.DateEnd = null;
                entity.Price = price;
                entity.Id = lastId + 1;

                _service.Create(entity, GetLog(Model.OccorenceLog.Create));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                List<Model.Group> lista_group = _groupService.FindAll(GetLog(Model.OccorenceLog.Read));

                foreach (var g in lista_group)
                {
                    g.Description = g.Item + " - " + g.Description;
                }

                ViewBag.GroupId = new SelectList(lista_group, "Id", "Description", null);

                if (result.DateEnd != null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(result);
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(int id, Model.Product2 entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                decimal price = Convert.ToDecimal(Request.Form["price"]);
                entity.Created = result.Created;
                entity.Updated = DateTime.Now;
                entity.Price = price;
                _service.Update(entity, GetLog(Model.OccorenceLog.Update));

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Atualize(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                List<Model.Group> lista_group = _groupService.FindAll(GetLog(Model.OccorenceLog.Read));

                foreach (var g in lista_group)
                {
                    g.Description = g.Item + " - " + g.Description;
                }

                ViewBag.GroupId = new SelectList(lista_group, "Id", "Description", null);

                if (result.DateEnd != null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(result);
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Atualize(int id, Model.Product2 entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                var result = _service.FindById(id, null);
                if (result != null)
                {
                    result.Updated = DateTime.Now;
                    result.DateEnd = Convert.ToDateTime(entity.DateStart).AddDays(-1);
                    _service.Update(result, GetLog(Model.OccorenceLog.Update));
                }

                var lastId = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Max(_ => _.Id);
                decimal price = Convert.ToDecimal(Request.Form["price"]);
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;
                entity.DateEnd = null;
                entity.Price = price;
                entity.Description = result.Description;
                entity.GroupId = result.GroupId;
                entity.Unity = result.Unity;
                entity.Code = result.Code;
                entity.Id = lastId + 1;

                _service.Create(entity, GetLog(Model.OccorenceLog.Create));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
        
        [HttpGet]
        public IActionResult Import()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                List<Model.Group> lista_group = _groupService.FindAll(GetLog(Model.OccorenceLog.Read));

                foreach (var g in lista_group)
                {
                    g.Description = g.Item + " - " + g.Description;
                }

                ViewBag.GroupId = new SelectList(lista_group, "Id", "Description", null);
                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Import(int groupId,IFormFile arquivo, Model.Product2 entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                if (arquivo == null || arquivo.Length == 0)
                {
                    ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                    return View(ViewData);
                }

                string filedir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Atos");

                if (!Directory.Exists(filedir))
                {
                    Directory.CreateDirectory(filedir);
                }

                string nomeArquivo = "ato";

                if (arquivo.FileName.Contains(".csv"))
                    nomeArquivo += ".csv";
                else
                    nomeArquivo += ".tmp";

                string caminho_WebRoot = _appEnvironment.WebRootPath;
                string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Atos\\";
                string caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                string[] paths_upload_ato = Directory.GetFiles(caminhoDestinoArquivo);

                if (System.IO.File.Exists(caminhoDestinoArquivoOriginal))
                {
                    System.IO.File.Delete(caminhoDestinoArquivoOriginal);

                }

                var stream = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);
                await arquivo.CopyToAsync(stream);
                stream.Close();

                var import = new Import();

                List<List<string>> products = new List<List<string>>();
                products = import.Product(caminhoDestinoArquivoOriginal);

                List<Model.Product2> addProduct = new List<Model.Product2>();
                List<Model.Product2> updateProduct = new List<Model.Product2>();

                for (int i = 0; i < products.Count(); i++)
                {
                    var item = _service.FindByProduct(products[i][0], groupId);

                    if (item != null)
                    {
                        item.Updated = DateTime.Now;
                        item.DateEnd = Convert.ToDateTime(products[i][4]).AddDays(-1);
                        updateProduct.Add(item);
                        //_service.Update(item, GetLog(Model.OccorenceLog.Update));
                    }

                    if (!products[i][0].Equals("") && !products[i][1].Equals("") && !products[i][3].Equals(""))
                    {
                        Model.Product2 product = new Model.Product2();
                        product.Code = products[i][0];
                        product.Description = products[i][1];
                        product.Unity = products[i][2];
                        product.Price = Convert.ToDecimal(products[i][3]);
                        product.DateStart = Convert.ToDateTime(products[i][4]);
                        product.GroupId = groupId;
                        product.Created = DateTime.Now;
                        product.Updated = DateTime.Now;
                        addProduct.Add(product);
                        //_service.Create(entity: product, GetLog(Model.OccorenceLog.Create));
                    }

                }

                _service.Create(addProduct);
                _service.Update(updateProduct);

                return RedirectToAction("Index");

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

            var productsAll = _service.FindAll(null).OrderBy(_ => _.GroupId);


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Product2> products = new List<Product2>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Product2> productTemp = new List<Product2>();
                productsAll.ToList().ForEach(s =>
                {
                    s.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Description);
                    s.Code = s.Code;
                    s.Price = s.Price;
                    s.Group.Description = s.Group.Description;
                    s.Unity = s.Unity;
                    s.DateStart = s.DateStart;
                    s.DateEnd = s.DateEnd;
                    productTemp.Add(s);
                });

                var ids = productTemp.Where(c =>
                    c.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Group.Description.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    )
                .Select(s => s.Id).ToList();

                products = productsAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var ncm = from r in products
                          where ids.ToArray().Contains(r.Id)
                          select new
                          {
                              Id = r.Id.ToString(),
                              Code = r.Code,
                              Description = r.Description,
                              GroupName = r.Group.Item + " - " + r.Group.Description,
                              Price = r.Price.ToString().Replace(".", ","),
                              Unity = r.Unity,
                              Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                              Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")

                          };

                return Ok(new { draw = draw, recordsTotal = products.Count(), recordsFiltered = products.Count(), data = ncm.Skip(start).Take(lenght) });

            }
            else
            {

                var product = from r in productsAll
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  Code = r.Code,
                                  Description = r.Description,
                                  GroupName = r.Group.Item + " - " + r.Group.Description,
                                  Price = r.Price.ToString().Replace(".", ","),
                                  Unity = r.Unity,
                                  Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                                  Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")

                              };
                return Ok(new { draw = draw, recordsTotal = productsAll.Count(), recordsFiltered = productsAll.Count(), data = product.Skip(start).Take(lenght) });
            }

        }
    }
}
