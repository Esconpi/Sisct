using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Taxation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Escon.SisctNET.Web.Controllers
{
    public class RuralProducerController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly ITypeClientService _typeClientService;
        private readonly IConfigurationService _configurationService;
        private readonly IHostingEnvironment _appEnvironment;

        public RuralProducerController(
            ITypeClientService typeClientService,
            ICompanyService companyService,
            IConfigurationService configurationService,
            IHostingEnvironment env)
        {
            _typeClientService = typeClientService;
            _companyService = companyService;
            _configurationService = configurationService;
            _appEnvironment = env;
        }


        public async Task<IActionResult> Index(IFormFile arquivo)
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
                    var year = Request.Form["year"];
                    var id = Request.Form["id"];
                    var month = Request.Form["month"];

                    var import = new Import();

                    var company = _companyService.FindById(Convert.ToInt32(id), null);

                    if (arquivo == null || arquivo.Length == 0)
                    {
                        ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                        return View(ViewData);
                    }

                    string nomeArquivo = company.Document + year + month;

                    if (arquivo.FileName.Contains(".txt"))
                        nomeArquivo += ".txt";
                    else
                        nomeArquivo += ".tmp";

                    string caminho_WebRoot = _appEnvironment.WebRootPath;
                    string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Speds\\";
                    string caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                    string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivo);
                    if (System.IO.File.Exists(caminhoDestinoArquivoOriginal))
                    {
                        System.IO.File.Delete(caminhoDestinoArquivoOriginal);

                    }
                    var stream = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);
                    await arquivo.CopyToAsync(stream);
                    stream.Close();

                    //var rstNotes = 
                }

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}