using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{
    public class IcmsEntryController : ControllerBaseSisctNET
    {
        private readonly ICompanyService _companyService;
        private readonly IHostingEnvironment _appEnvironment;

        public IcmsEntryController(
            ICompanyService companyService,
            IFunctionalityService functionalityService,
            IHostingEnvironment env,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Company")
        {
            _companyService = companyService;
            _appEnvironment = env;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public async Task<IActionResult> Relatory(long id, string year, string month, IFormFile arquivo)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var company = _companyService.FindById(id, null);

                string caminhoDestinoArquivoOriginal = "";
                string caminho_WebRoot = _appEnvironment.WebRootPath;

                if (arquivo != null)
                {
                    string filedirSped = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Speds");

                    if (!Directory.Exists(filedirSped))
                        Directory.CreateDirectory(filedirSped);

                    string nomeArquivo = company.Document;

                    if (arquivo.FileName.Contains(".txt"))
                        nomeArquivo += ".txt";
                    else
                        nomeArquivo += ".tmp";

                    string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Speds\\";
                    caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                    string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivo);

                    if (System.IO.File.Exists(caminhoDestinoArquivoOriginal))
                        System.IO.File.Delete(caminhoDestinoArquivoOriginal);

                    var streamSped = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);
                    await arquivo.CopyToAsync(streamSped);
                    streamSped.Close();
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
