using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Escon.SisctNET.Web.Controllers
{
    public class ProductController : ControllerBaseSisctNET
    {
        private readonly IProduct3Service _service;
        private readonly IGroupService _groupService;
        private readonly IHostingEnvironment _appEnvironment;

        public ProductController(
            IProduct3Service service,
            IGroupService groupService,
            IHostingEnvironment env,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Product")
        {
            _service = service;
            _groupService = groupService;
            _appEnvironment = env;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Create()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                List<Model.Group> lista_group = _groupService.FindAll(null);

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
        public IActionResult Create(Model.Product3 entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var item = _service.FindByProduct(entity.Code,entity.GroupId);

                if (item != null)
                {
                    item.Updated = DateTime.Now;
                    item.DateEnd = entity.DateStart.AddDays(-1);
                    _service.Update(item, null);
                }

                //var lastId = _service.FindAll(null).Max(_ => _.Id);
                decimal price = Convert.ToDecimal(Request.Form["price"]);
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;
                entity.DateEnd = null;
                entity.Price = price;
                //entity.Id = lastId + (long)1;

                _service.Create(entity, GetLog(Model.OccorenceLog.Create));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);

                List<Model.Group> lista_group = _groupService.FindAll(null);

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
        public IActionResult Edit(long id, Model.Product3 entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
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
        public IActionResult Atualize(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);

                List<Model.Group> lista_group = _groupService.FindAll(null);

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
        public IActionResult Atualize(long id, Model.Product3 entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                if (result != null)
                {
                    result.Updated = DateTime.Now;
                    result.DateEnd = Convert.ToDateTime(entity.DateStart).AddDays(-1);
                    _service.Update(result, GetLog(Model.OccorenceLog.Update));
                }

                var lastId = _service.FindAll(null).Max(_ => _.Id);
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                List<Model.Group> lista_group = _groupService.FindAll(null);

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
        public async Task<IActionResult> Import(long groupId, IFormFile arquivo, DateTime inicioATo)       
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Product")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                if (arquivo == null || arquivo.Length == 0)
                {
                    ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                    return View(ViewData);
                }

                string filedir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Atos");

                if (!Directory.Exists(filedir)) Directory.CreateDirectory(filedir);

                string nomeArquivo = "Ato";

                if (arquivo.FileName.Contains(".xls") || arquivo.FileName.Contains(".xlsx"))
                    nomeArquivo += ".xls";

                string caminho_WebRoot = _appEnvironment.WebRootPath;
                string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Atos\\";
                string caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                string[] paths_upload_ato = Directory.GetFiles(caminhoDestinoArquivo);

                if (System.IO.File.Exists(caminhoDestinoArquivoOriginal)) System.IO.File.Delete(caminhoDestinoArquivoOriginal);

                var stream = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);
                await arquivo.CopyToAsync(stream);
                stream.Close();

                var import = new Planilha.Import();

                List<List<string>> products = new List<List<string>>();
                products = import.Products(caminhoDestinoArquivoOriginal);

                List<Model.Product3> addProduct = new List<Model.Product3>();
                List<Model.Product3> updateProduct = new List<Model.Product3>();

                var productsGroup = _service.FindByGroup(groupId);

                for (int i = 0; i < products.Count(); i++)
                {
                    var item = productsGroup.Where(_ => _.Code.Equals(products[i][0])).FirstOrDefault();

                    if (item != null)
                    {
                        item.Updated = DateTime.Now;
                        item.DateEnd = inicioATo.AddDays(-1);
                        updateProduct.Add(item);
                    }

                    if (!products[i][0].Equals("") && !products[i][1].Equals("") && !products[i][3].Equals(""))
                    {
                        Model.Product3 product = new Model.Product3();
                        product.Code = products[i][0];
                        product.Description = products[i][1];
                        product.Unity = products[i][2];
                        product.Price = Convert.ToDecimal(products[i][3].Replace(",","*").Replace(".",",").Replace("*","."));
                        product.DateStart = inicioATo;
                        product.GroupId = groupId;
                        product.Created = DateTime.Now;
                        product.Updated = DateTime.Now;
                        addProduct.Add(product);
                    }

                }

                _service.Create(addProduct);
                _service.Update(updateProduct);

                return RedirectToAction("Index");

            }
            catch (IndexOutOfRangeException iEx)
            {
                return BadRequest(new { erro = 500, message = iEx.Message });
            }
            catch (ArgumentException aEx)
            {
                return BadRequest(new { erro = 500, message = aEx.Message });
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

            var productsAll = _service.FindByAllGroup(null).OrderBy(_ => _.Group.AttachmentId).ToList();


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Product3> products = new List<Product3>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Product3> productTemp = new List<Product3>();
                productsAll.ToList().ForEach(s =>
                {
                    s.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Description);
                    productTemp.Add(s);
                });

                var ids = productTemp.Where(c =>
                    c.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Group.Description.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    )
                .Select(s => s.Id).ToList();

                products = productsAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var product = from r in products
                          where ids.ToArray().Contains(r.Id)
                          select new
                          {
                              Id = r.Id.ToString(),
                              Product = r.Code + " - " + r.Description,
                              Group = r.Group.Item + " - " + r.Group.Description,
                              Price = r.Price,
                              Unity = r.Unity,
                              Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                              Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")

                          };

                return Ok(new { draw = draw, recordsTotal = products.Count(), recordsFiltered = products.Count(), data = product.Skip(start).Take(lenght) });

            }
            else
            {

                var product = from r in productsAll
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  Product = r.Code + " - " + r.Description,
                                  Group = r.Group.Item + " - " + r.Group.Description,
                                  Price = r.Price,
                                  Unity = r.Unity,
                                  Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                                  Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")

                              };
                return Ok(new { draw = draw, recordsTotal = productsAll.Count(), recordsFiltered = productsAll.Count(), data = product.Skip(start).Take(lenght) });
            }

        }
    }
}
   