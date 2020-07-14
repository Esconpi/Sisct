using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Taxation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{

    public class HomeController : ControllerBaseSisctNET
    {

        private readonly ICompanyService _service;
        private readonly IConfigurationService _configurationService;
        private readonly IHostingEnvironment _appEnvironment;

        public HomeController(
            ICompanyService service,
            IConfigurationService configurationService,
            IHostingEnvironment env,
            Service.IFunctionalityService functionalityService, 
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Home")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
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
                var login = SessionManager.GetLoginInSession();

                if (login == null)
                {
                    return RedirectToAction("Index", "Authentication");
                }
                else
                {
                    var result = _service.FindByCompanies();
                    SessionManager.SetTipoInSession(0);
                    return View(null);
                }
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

                string directoryNfe = Nfe.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                var import = new Import();

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
                    sped = import.SpedAll(caminhoDestinoArquivoOriginalUpload, directoryNfe);
                }
                else if (option.Equals("entry"))
                {
                    sped = import.SpedEntry(caminhoDestinoArquivoOriginalUpload, directoryNfe);
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

                foreach(var s in sped)
                {
                    novoArquivo.WriteLine(s);
                }

                novoArquivo.Close();

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
                return View(comp);
            }
            catch(Exception ex)
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

    }
}
