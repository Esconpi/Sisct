﻿using Escon.SisctNET.Model;
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
            : base(functionalityService, "Company")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _emailResponsibleService = emailResponsibleService;
            _configurationService = configurationService;
            _appEnvironment = env;
        }


        public IActionResult Index()
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                SessionManager.SetTipoInSession(0);
                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Sped(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _service.FindById(id, null);
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { requestcode = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Sped(long id,string month, string year,string option, IFormFile arquivo)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                SessionManager.SetProductsSped(null);

                var comp = _service.FindById(id, null);

                var Nfe = _configurationService.FindByName("NFe", null);

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
                    sped = importSped.NFeAll(caminhoDestinoArquivoOriginalUpload, directoryNfe);
                }
                else if (option.Equals("entry"))
                {
                    sped = importSped.NFeEntry(caminhoDestinoArquivoOriginalUpload, directoryNfe);
                }
                else if (option.Equals("entryOriginal"))
                {
                    sped = importSped.NFeEntry(caminhoDestinoArquivoOriginalUpload);
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

        public IActionResult Download(long id, string year, string month)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _service.FindById(id, null);
                ViewBag.Year = year;
                ViewBag.Month = month;
                List<List<string>> products = new List<List<string>>();

                if(SessionManager.GetProductsSped() != null)
                {
                    products = SessionManager.GetProductsSped().OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                }

                ViewBag.Produtos = products;
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public FileResult DownloadSped(long id, string year, string month)
        {
            var comp = _service.FindById(id, null);

            var nomeArquivo = comp.Document + year + month + ".txt";

            string caminho_WebRoot = _appEnvironment.WebRootPath;
            string caminhoDestinoArquivoDownload = caminho_WebRoot + "/Downloads/Speds/";
            string caminhoDestinoArquivoOriginalDownload = caminhoDestinoArquivoDownload + nomeArquivo;
            string contentType = "application/text";
            byte[] fileBytes = System.IO.File.ReadAllBytes(caminhoDestinoArquivoOriginalDownload);
            string fileName = "ESCON - Sped Fiscal " + comp.SocialName + " " + comp.Document + " " + month + "/" + year + ".txt";

            return File(fileBytes, contentType, fileName);
        }

        public IActionResult Icms(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Taxation(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Relatory(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
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
        public async Task<IActionResult> GetResponsibleByCompanyId(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            var draw = Request.Query["draw"].ToString();

            var result = await _emailResponsibleService.GetByCompanyAsync(id);
            return Ok(new { draw = Convert.ToInt32(draw), recordsTotal = result.Count(), recordsFiltered = result.Count(), data = result });
        }

        [HttpPost]
        public IActionResult PostResponsibleByCompanyId([FromBody] EmailResponsible responsible)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            responsible.Created = DateTime.Now;
            responsible.Updated = DateTime.Now;
            _emailResponsibleService.Create(responsible, GetLog(OccorenceLog.Create));
            return Ok(new { code="200", message="ok" });
        }

        [HttpDelete]
        public IActionResult DeleteResponsibleByCompanyId(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            _emailResponsibleService.Delete(id, GetLog(OccorenceLog.Delete));
            return Ok(new { code = "200", message = "ok" });
        }
    }
}