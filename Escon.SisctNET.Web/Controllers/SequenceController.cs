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

                if (ident.Equals("0"))
                {
                    confDBSisctNfe = _configurationService.FindByName("NFe");
                }
                else if (ident.Equals("1"))
                {
                    confDBSisctNfe = _configurationService.FindByName("NFe Saida");
                }

                string directoryNfe = confDBSisctNfe.Value + "\\" + company.Document + "\\" + year + "\\" + month;

                List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
                List<int> notes55 = new List<int>();
                List<int> notes65 = new List<int>();
                List<List<int>> notes55Fora = new List<List<int>>();
                List<List<int>> notes65Fora = new List<List<int>>();

                if (archive.Equals(Model.Archive.XmlNFe))
                {
                    notes = importXml.Nfe(directoryNfe);
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

                    foreach (var note in sped)
                    {
                        if (note[1].Equals("55"))
                        {
                            notes55.Add(Convert.ToInt32(note[2]));
                        }
                        else
                        {
                            notes65.Add(Convert.ToInt32(note[2]));
                        }
                        
                    }

                    notes55.Sort();
                    notes65.Sort();

                    for (int i = 1; i < notes55.Count() - 1; i++)
                    {
                        if ((notes55[i] - (notes55[i - 1] + 1)) > 1)
                        {
                            List<int> notesIntervalo = new List<int>();
                            notesIntervalo.Add(notes55[i - 1]);
                            notesIntervalo.Add(notes55[i]);
                            notesIntervalo.Add(notes55[i] - notes55[i - 1]);
                            notes55Fora.Add(notesIntervalo);
                        }
                    }

                    for (int i = 1; i < notes65.Count() - 1; i++)
                    {
                        if ((notes65[i] - (notes65[i - 1] + 1)) > 1)
                        {
                            List<int> noteIntervalo = new List<int>();
                            noteIntervalo.Add(notes65[i - 1]);
                            noteIntervalo.Add(notes65[i]);
                            noteIntervalo.Add(notes65[i] - notes65[i - 1]);
                            notes65Fora.Add(noteIntervalo);
                        }
                    }


                    ViewBag.Notas55 = notes55Fora;
                    ViewBag.Notas65 = notes65Fora;

                    /*var p55 = notes55[0];
                    var u55 = notes55[notes55.Count() - 1];

                    var p65 = notes65[0];
                    var u65 = notes65[notes65.Count() - 1];*/

                }
                else if (archive.Equals(Model.Archive.SpedCTe)){

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

                    var sped = importSped.SpedCte(caminhoDestinoArquivoOriginalSped, ident);
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
