using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{
    public class SequenceController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly IHostingEnvironment _appEnvironment;

        public SequenceController(
            ICompanyService companyService,
            IConfigurationService configurationService,
            IHostingEnvironment appEnvironment)
        {
            _companyService = companyService;
            _configurationService = configurationService;
            _appEnvironment = appEnvironment;
        }

        public async Task<IActionResult> Index(Model.Archive archive,IFormFile arquivo)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }

            try
            {
                var year = Request.Form["year"];
                var id = Request.Form["id"];
                var month = Request.Form["month"];
                var ident = Request.Form["ident"];

                var company = _companyService.FindById(Convert.ToInt32(id), null);

                ViewBag.Archive = archive.ToString();
                ViewBag.Document = company.Document;
                ViewBag.SocialName = company.SocialName;
                ViewBag.Ano = year;
                ViewBag.Mes = month;

                var importXml = new Xml.Import();
                var importSped = new Sped.Import();
                var confDBSisctNfe = new Model.Configuration();

                if (archive.Equals(Model.Archive.XmlNFe))
                {

                }
                else if (archive.Equals(Model.Archive.XmlCTe))
                {

                }
                else if (archive.Equals(Model.Archive.SpedNFe))
                {
                    if (arquivo == null || arquivo.Length == 0)
                    {
                        ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                        return View(ViewData);
                    }

                    string filedirSped = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Speds");

                    if (!Directory.Exists(filedirSped))
                    {
                        Directory.CreateDirectory(filedirSped);
                    }

                    string nomeArquivoSped = company.Document + year + month;

                    if (arquivo.FileName.Contains(".txt"))
                        nomeArquivoSped += ".txt";
                    else
                        nomeArquivoSped += ".tmp";

                    string caminho_WebRoot = _appEnvironment.WebRootPath;

                    string caminhoDestinoArquivoSped = caminho_WebRoot + "\\Uploads\\Speds\\";
                    string caminhoDestinoArquivoOriginalSped = caminhoDestinoArquivoSped + nomeArquivoSped;

                    string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivoSped);
                    if (System.IO.File.Exists(caminhoDestinoArquivoOriginalSped))
                    {
                        System.IO.File.Delete(caminhoDestinoArquivoOriginalSped);

                    }
                    var streamSped = new FileStream(caminhoDestinoArquivoOriginalSped, FileMode.Create);
                    await arquivo.CopyToAsync(streamSped);
                    streamSped.Close();

                    var sped = importSped.SpedNfe(caminhoDestinoArquivoOriginalSped, ident);
                }

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
