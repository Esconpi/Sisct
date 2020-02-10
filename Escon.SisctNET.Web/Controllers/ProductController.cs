using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Ato;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{
    public class ProductController : ControllerBaseSisctNET
    {
        private readonly IProductService _service;
        private readonly IGroupService _groupService;
        private readonly IHostingEnvironment _appEnvironment;
        public ProductController(
            IProductService service,
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
        public IActionResult Index(int page = 1)
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
                    var rst = _service.FindAll(GetLog(Model.OccorenceLog.Read));

                    int contaPage = rst.Count() / 2000;
                    if (rst.Count() % 2000 > 0)
                    {
                        contaPage++;
                    }
                    int final = page * 2000;
                    int inicio = final - 2000;
                    var result = rst.Where(_ => _.Id > inicio && _.Id <= final).ToList();

                    ViewBag.ContaPage = contaPage;

                    return View(result);
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
            try
            {
                List<Model.Group> lista_group = _groupService.FindAll(GetLog(Model.OccorenceLog.Read));
                lista_group.Insert(0, new Model.Group() { Description = "Nennhum item selecionado", Id = 0 });
                SelectList groups = new SelectList(lista_group, "Id", "Description", null);
                ViewBag.GroupId = groups;

                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
           
        }

        [HttpPost]
        public IActionResult Create(Model.Product entity)
        {
            try
            {
                var result = _service.FindByProduct(entity.Code, entity.GroupId,entity.Description);
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
                entity.Id = lastId+1;

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
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                ViewBag.GroupId = new SelectList(_groupService.FindAll(GetLog(Model.OccorenceLog.Read)), "Id", "Description", null);

                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(int id, Model.Product entity)
        {
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
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Import()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile arquivo, Model.Product entity)
        {
            try
            {
                if (arquivo == null || arquivo.Length == 0)
                {
                    ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                    return View(ViewData);
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


                for (int i = 0; i < products.Count(); i++)
                {
                    var item = _service.FindByProduct(products[i][0], Convert.ToInt32(products[i][5]), products[i][1]);

                    if (item != null)
                    {
                        item.Updated = DateTime.Now;
                        item.DateEnd = Convert.ToDateTime(products[i][4]).AddDays(-1);
                        _service.Update(item, GetLog(Model.OccorenceLog.Update));
                    }

                    var id = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Max(_ => _.Id);
                    var product = new Model.Product
                    {
                        Id = id + 1,
                        Code = products[i][0],
                        Description = products[i][1],
                        Unity = products[i][2],
                        Price = Convert.ToDecimal(products[i][3]),
                        DateStart = Convert.ToDateTime(products[i][4]),
                        GroupId = Convert.ToInt32(products[i][5]),
                        Created = DateTime.Now,
                        Updated = DateTime.Now
                    };
                    _service.Create(entity: product, GetLog(Model.OccorenceLog.Create));

                }

                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}