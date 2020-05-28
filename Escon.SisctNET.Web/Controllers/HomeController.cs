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
        private readonly IHostingEnvironment _appEnvironment;

        public HomeController(
            ICompanyService service,
            IHostingEnvironment env,
            Service.IFunctionalityService functionalityService, 
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Home")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _appEnvironment = env;
        }


        public IActionResult Index()
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
        public async Task<IActionResult> Sped(int id, IFormFile arquivo)
        {
            try
            {
                var comp = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

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

                string nomeArquivo = comp.Document;

                if (arquivo.FileName.Contains(".txt"))
                    nomeArquivo += ".txt";
                else
                    nomeArquivo += ".tmp";

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
                sped = import.Sped(caminhoDestinoArquivoOriginalUpload);

                
                return RedirectToAction("Download", new { id = id});
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Download(int id)
        {
            try
            {
                var comp = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                return View(comp);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

    }
}
