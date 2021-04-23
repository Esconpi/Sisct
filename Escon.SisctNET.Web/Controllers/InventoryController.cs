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
    public class InventoryController : ControllerBaseSisctNET
    {
        private readonly ICompanyService _service;
        private readonly IConfigurationService _configurationService;
        private readonly IHostingEnvironment _appEnvironment;

        public InventoryController(
            ICompanyService service,
            IConfigurationService configurationService,
            IHostingEnvironment env,
            Service.IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Company")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _configurationService = configurationService;
            _appEnvironment = env;
        }

        public IActionResult Index()
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

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
        public IActionResult Inventario(int id)
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
        public async Task<IActionResult> Inventario(int id, DateTime dateInicial, DateTime dateFinal, IFormFile arquivo)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                SessionManager.SetProductsSped(null);

                var comp = _service.FindById(id, null);


                string dirUpload = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Planilha");

                if (!Directory.Exists(dirUpload))
                {
                    Directory.CreateDirectory(dirUpload);
                }

                var importPlanilha = new Planilha.Import();

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

                if (arquivo.FileName.Contains(".xls") || arquivo.FileName.Contains(".xlsx"))
                    nomeArquivo += ".xls";

                string caminho_WebRoot = _appEnvironment.WebRootPath;

                // Arquivo Planilha
                string caminhoDestinoArquivoUpload = caminho_WebRoot + "\\Uploads\\Planilha\\";
                string caminhoDestinoArquivoOriginalUpload = caminhoDestinoArquivoUpload + nomeArquivo;

                string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivoUpload);
                if (System.IO.File.Exists(caminhoDestinoArquivoOriginalUpload))
                {
                    System.IO.File.Delete(caminhoDestinoArquivoOriginalUpload);
                }

                var stream = new FileStream(caminhoDestinoArquivoOriginalUpload, FileMode.Create);
                await arquivo.CopyToAsync(stream);
                stream.Close();

                var planilhaInventario = importPlanilha.Inventario(caminhoDestinoArquivoOriginalUpload).OrderBy(_ => _[0]);

                // Criando Novo Arquivo Sped
                string caminhoDestinoArquivoDownload = caminho_WebRoot + "\\Downloads\\Speds\\";
                string caminhoDestinoArquivoOriginalDownload = caminhoDestinoArquivoDownload + nomeArquivo;

                string[] paths_download_sped = Directory.GetFiles(caminhoDestinoArquivoDownload);
                if (System.IO.File.Exists(caminhoDestinoArquivoOriginalDownload))
                {
                    System.IO.File.Delete(caminhoDestinoArquivoOriginalDownload);
                }
                StreamWriter novoArquivo = new StreamWriter(caminhoDestinoArquivoOriginalDownload);

                string linha0000 = "|0000|015|0|" + dateInicial.ToString("dd/MM/yyyy").Replace("/", "") + "|" + dateFinal.ToString("dd/MM/yyyy").Replace("/", "") +
                                    "|" + comp.SocialName + "|" + comp.Document + "||" + comp.County.State.UF + "|" + comp.Ie + "|" + comp.County.Code + "|||A|1|",
                       linha0001 = "|0001|0|",
                       linha0005 = "|0005|" + comp.FantasyName + "|" + comp.Cep + "|" + comp.Logradouro + "|" + comp.Number + "|" + comp.Complement + "|" +
                                     comp.District + "|" + comp.Phone.Replace("(", "").Replace(")", "") + "|||",
                       linha0100 = "|0100|RIMARIO DE JESUS RODRIGUES|20086180304|3683||64014073|RUA SANTA LUZIA|3040||ILHOTAS|8633037105||ESCONCONTABILIDADERJR@GMAIL.COM|2211001|";


                novoArquivo.WriteLine(linha0000);
                novoArquivo.WriteLine(linha0001);
                novoArquivo.WriteLine(linha0005);
                novoArquivo.WriteLine(linha0100);

                List<string> linhas0190 = new List<string>();
                List<string> linhas0200 = new List<string>();
                List<string> linhasH010 = new List<string>();

                decimal total = 0;
                foreach (var inven in planilhaInventario)
                {
                    bool unidade = false;
                    bool produto = false;

                    foreach (var un in linhas0190)
                    {
                        string[] line = un.Split('|');
                        if (line[2].Equals(inven[3].Trim()))
                        {
                            unidade = true;
                            break;
                        }
                    }

                    if (!unidade)
                    {
                        string u = "|0190|" + inven[3].Trim() + "|DESCR " + inven[3].Trim() + "|";
                        linhas0190.Add(u);
                    }

                    foreach (var prod in linhas0200)
                    {
                        string[] line = prod.Split('|');
                        if (line[2].Equals(inven[0].Trim()))
                        {
                            unidade = true;
                            break;
                        }
                    }

                    if (!produto)
                    {
                        string p = "|0200|" + inven[0].Trim() + "|" + inven[1].Trim() + "|||" + inven[3].Trim() + "|00|" + inven[2] + "||0,00||";
                        linhas0200.Add(p);
                    }

                    decimal totalItem = Math.Round(Convert.ToDecimal(inven[5].ToString().Replace(".", ",")) * Convert.ToDecimal(inven[4].ToString().Replace(".", ",")), 2);
                    total += totalItem;
                    string linha = "|H010|" + inven[0] + "|" + inven[3].Trim() + "|" + inven[4] + "|" + inven[5].ToString().Replace(".", ",") + "|" + totalItem.ToString().Replace(".", ",") +
                            "|0|||||" + totalItem.ToString().Replace(".", ",") + "|";
                    linhasH010.Add(linha);
                }

                foreach (var un in linhas0190)
                {
                    novoArquivo.WriteLine(un);
                }

                foreach (var prod in linhas0200)
                {
                    novoArquivo.WriteLine(prod);
                }

                string linhaE001 = "|H001|0|",
                       linhaH005 = "|H005|" + dateFinal.ToString("dd/MM/yyyy").Replace("/", "") + "|" + total.ToString().Replace(".", ",") + "|01|";


                novoArquivo.WriteLine(linhaE001);
                novoArquivo.WriteLine(linhaH005);

                int qtd = 3;

                foreach (var linha in linhasH010)
                {
                    novoArquivo.WriteLine(linha);
                    qtd += 1;
                }

                string linhaH990 = "|H990|" + qtd + "|";

                novoArquivo.WriteLine(linhaH990);

                novoArquivo.Close();

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                return RedirectToAction("Download", new { id = id });

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Download(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _service.FindById(id, null);
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public FileResult DownloadInventario(int id)
        {
            var comp = _service.FindById(id, null);

            var nomeArquivo = comp.Document + ".txt";

            string caminho_WebRoot = _appEnvironment.WebRootPath;
            string caminhoDestinoArquivoDownload = caminho_WebRoot + "/Downloads/Speds/";
            string caminhoDestinoArquivoOriginalDownload = caminhoDestinoArquivoDownload + nomeArquivo;
            string contentType = "application/text";
            byte[] fileBytes = System.IO.File.ReadAllBytes(caminhoDestinoArquivoOriginalDownload);
            string fileName = "ESCON - Sped Inventario " + comp.SocialName + " " + comp.Document + ".txt";

            return File(fileBytes, contentType, fileName);
        }

        [HttpGet]
        public IActionResult ImportEntry(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _service.FindById(id, null);
                return PartialView(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { requestcode = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ImportExit(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _service.FindById(id, null);
                return PartialView(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { requestcode = 500, message = ex.Message });
            }
        }
        
        [HttpGet]
        public IActionResult Relatory(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _service.FindById(id, null);
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
