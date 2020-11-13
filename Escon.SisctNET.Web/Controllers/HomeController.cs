using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{

    public class HomeController : ControllerBaseSisctNET
    {

        private readonly ICompanyService _service;
        private readonly IEmailResponsibleService _emailResponsibleService;
        private readonly IConfigurationService _configurationService;
        private readonly IHostingEnvironment _appEnvironment;

        public HomeController(
            ICompanyService service,
            IEmailResponsibleService emailResponsibleService,
            IConfigurationService configurationService,
            IHostingEnvironment env,
            Service.IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Home")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _emailResponsibleService = emailResponsibleService;
            _configurationService = configurationService;
            _appEnvironment = env;
        }


        public IActionResult Index()
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }
            try
            {
                var result = _service.FindByCompanies();
                SessionManager.SetTipoInSession(0);
                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Sped(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }

            try
            {
                var comp = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { requestcode = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Sped(int id,string month, string year,string option, IFormFile arquivo)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }

            try
            {
                var comp = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                var Nfe = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                var importSped = new Sped.Import();
                var importDir = new Diretorio.Import();

                string directoryNfe = importDir.Entrada(comp,Nfe.Value,year,month) ;
                

                string dirUpload = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Speds");

                if (!Directory.Exists(dirUpload))
                {
                    Directory.CreateDirectory(dirUpload);
                }

                string dirDownload = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Downloads", "Speds");

                if (!Directory.Exists(dirDownload))
                {
                    Directory.CreateDirectory(dirDownload);
                }

                if (arquivo == null || arquivo.Length == 0)
                {
                    ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                    return View(ViewData);
                }

                string nomeArquivo = comp.Document + year + month;


                nomeArquivo += ".txt";


                // Arquivo Sped Original
                string caminho_WebRoot = _appEnvironment.WebRootPath;
                string caminhoDestinoArquivoUpload = caminho_WebRoot + "\\Uploads\\Speds\\";
                string caminhoDestinoArquivoOriginalUpload = caminhoDestinoArquivoUpload + nomeArquivo;

                string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivoUpload);
                if (System.IO.File.Exists(caminhoDestinoArquivoOriginalUpload))
                {
                    System.IO.File.Delete(caminhoDestinoArquivoOriginalUpload);
                }

                var stream = new FileStream(caminhoDestinoArquivoOriginalUpload, FileMode.Create);
                await arquivo.CopyToAsync(stream);
                stream.Close();

                List<string> sped = new List<string>();

                if (option.Equals("all"))
                {
                    sped = importSped.SpedAll(caminhoDestinoArquivoOriginalUpload, directoryNfe);
                }
                else if (option.Equals("entry"))
                {
                    sped = importSped.SpedEntry(caminhoDestinoArquivoOriginalUpload, directoryNfe);
                }
                

                // Criando Novo Arquivo Sped
                string caminhoDestinoArquivoDownload = caminho_WebRoot + "\\Downloads\\Speds\\";
                string caminhoDestinoArquivoOriginalDownload = caminhoDestinoArquivoDownload + nomeArquivo;

                string[] paths_download_sped = Directory.GetFiles(caminhoDestinoArquivoDownload);
                if (System.IO.File.Exists(caminhoDestinoArquivoOriginalDownload))
                {
                    System.IO.File.Delete(caminhoDestinoArquivoOriginalDownload);
                }
                StreamWriter novoArquivo = new StreamWriter(caminhoDestinoArquivoOriginalDownload);

                foreach (var s in sped)
                {
                    novoArquivo.WriteLine(s);
                }

                novoArquivo.Close();

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                return RedirectToAction("Download", new { id = id, year = year, month = month});

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Download(int id, string year, string month)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }

            try
            {
                var comp = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                ViewBag.Year = year;
                ViewBag.Month = month;
                ViewBag.Produtos = SessionManager.GetProductsSped().OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public FileResult DownloadSped(int id, string year, string month)
        {

            var comp = _service.FindById(id, null);

            var nomeArquivo = comp.Document + year + month + ".txt";

            string caminho_WebRoot = _appEnvironment.WebRootPath;
            string caminhoDestinoArquivoDownload = caminho_WebRoot + "/Downloads/Speds/";
            string caminhoDestinoArquivoOriginalDownload = caminhoDestinoArquivoDownload + nomeArquivo;
            string contentType = "application/text";
            byte[] fileBytes = System.IO.File.ReadAllBytes(caminhoDestinoArquivoOriginalDownload);
            string fileName = "ESCON - Sped Fiscal " + comp.Document + " " + month + "/" + year + ".txt";

            return File(fileBytes, contentType, fileName);
        }

        [HttpGet]
        public IActionResult Taxation(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Relatory(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                ViewBag.Incentive = result.Incentive;
                ViewBag.TypeIncentive = result.TipoApuracao;
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetResponsibleByCompanyId(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }

            var draw = Request.Query["draw"].ToString();

            var result = await _emailResponsibleService.GetByCompanyAsync(id);
            return Ok(new { draw = Convert.ToInt32(draw), recordsTotal = result.Count(), recordsFiltered = result.Count(), data = result });
        }

        [HttpPost]
        public IActionResult PostResponsibleByCompanyId([FromBody] EmailResponsible responsible)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }
            responsible.Created = DateTime.Now;
            responsible.Updated = DateTime.Now;
            _emailResponsibleService.Create(responsible, GetLog(OccorenceLog.Create));
            return Ok(new { code="200", message="ok" });
        }

        [HttpDelete]
        public IActionResult DeleteResponsibleByCompanyId(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }

            _emailResponsibleService.Delete(id, GetLog(OccorenceLog.Delete));
            return Ok(new { code = "200", message = "ok" });
        }
    }
}