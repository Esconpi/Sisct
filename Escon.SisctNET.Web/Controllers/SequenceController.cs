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

                var comp = _companyService.FindById(Convert.ToInt32(id), null);

                ViewBag.Archive = archive.ToString();
                ViewBag.Document = comp.Document;
                ViewBag.SocialName = comp.SocialName;

                SessionManager.SetMonthInSession(month);
                SessionManager.SetYearInSession(year);

                var importXml = new Xml.Import();
                var importSped = new Sped.Import();
                var importEvento = new Evento.Import();

                var confDBSisctNfe = _configurationService.FindByName("NFe Saida");
                var onfDBSisctCte = _configurationService.FindByName("CTe Saida");

                List<int> nfe55 = new List<int>();
                List<int> nfe65 = new List<int>();

                if (archive.Equals(Model.Archive.XmlNFeSefaz) || archive.Equals(Model.Archive.XmlNFeEmpresa))
                {
                    List<List<Dictionary<string, string>>> notesValidas = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesNFeCanceladas = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesNFCeCanceladas = new List<List<Dictionary<string, string>>>();

                    string directoryValida = "", directoryNFeCancelada = "", directoryNFCeCancelada = "";
                    if (archive.Equals(Model.Archive.XmlNFeEmpresa))
                    {

                        directoryValida = confDBSisctNfe.Value + "\\" + comp.SocialName + "-" + comp.Document + "\\" + year + "\\" + month + "\\" + "EMPRESA";
                        directoryNFeCancelada = confDBSisctNfe.Value + "\\" + comp.SocialName + "-" + comp.Document + "\\" + year + "\\" + month + "\\" + "EMPRESA" + "\\" + "NFe CANCELADA";
                        directoryNFCeCancelada = confDBSisctNfe.Value + "\\" + comp.SocialName + "-" +  comp.Document + "\\" + year + "\\" + month + "\\" + "EMPRESA" + "\\" + "NFCe CANCELADA";
                    }
                    else
                    {
                        directoryValida = confDBSisctNfe.Value + "\\" + comp.SocialName + "-" + comp.Document + "\\" + year + "\\" + month + "\\" + "SEFAZ";
                        directoryNFeCancelada = confDBSisctNfe.Value + "\\" + comp.SocialName + "-" + comp.Document + "\\" + year + "\\" + month + "\\" + "SEFAZ" + "\\" + "NFe CANCELADA";
                        directoryNFCeCancelada = confDBSisctNfe.Value + "\\" + comp.SocialName + "-" + comp.Document + "\\" + year + "\\" + month + "\\" + "SEFAZ" + "\\" + "NFCe CANCELADA";
                    }

                    notesValidas = importXml.NfeResume(directoryValida);
                    notesNFeCanceladas = importEvento.Nfe(directoryNFeCancelada);
                    notesNFCeCanceladas = importXml.NfeResume(directoryNFCeCancelada);

                    for (int i = notesValidas.Count - 1; i >= 0; i--)
                    {
                        if (!notesValidas[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notesValidas.RemoveAt(i);
                            continue;
                        }

                        if (notesValidas[i][1]["mod"].Equals("55"))
                        {
                            nfe55.Add(Convert.ToInt32(notesValidas[i][1]["nNF"]));
                        }
                        else
                        {
                            nfe65.Add(Convert.ToInt32(notesValidas[i][1]["nNF"]));
                        }
                    }

                    foreach (var note in notesNFeCanceladas)
                    {
                        if (note[0]["chNFe"].Substring(20, 2).Equals("55"))
                        {
                            nfe55.Add(Convert.ToInt32(note[0]["chNFe"].Substring(25, 9)));
                        }
                        else
                        {
                            nfe65.Add(Convert.ToInt32(note[0]["chNFe"].Substring(25, 9)));
                        }
                    }

                    for (int i = notesNFCeCanceladas.Count - 1; i >= 0; i--)
                    {
                        if (!notesNFCeCanceladas[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notesNFCeCanceladas.RemoveAt(i);
                            continue;
                        }

                        if (notesNFCeCanceladas[i][1]["mod"].Equals("55"))
                        {
                            nfe55.Add(Convert.ToInt32(notesNFCeCanceladas[i][1]["nNF"]));
                        }
                        else
                        {
                            nfe65.Add(Convert.ToInt32(notesNFCeCanceladas[i][1]["nNF"]));
                        }
                    }
                }
                else if (archive.Equals(Model.Archive.XmlCTeSefaz) || archive.Equals(Model.Archive.XmlCTeEmpresa))
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

                    string nomeArquivoSped = comp.Document + year + month;

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

                    var sped = importSped.SpedNfe(caminhoDestinoArquivoOriginalSped, "1");

                    foreach (var note in sped)
                    {
                        if (note[1].Equals("55"))
                        {
                            nfe55.Add(Convert.ToInt32(note[2]));
                        }
                        else
                        {
                            nfe65.Add(Convert.ToInt32(note[2]));
                        }
                        
                    }

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

                    string nomeArquivoSped = comp.Document + year + month;

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

                    var sped = importSped.SpedCte(caminhoDestinoArquivoOriginalSped, "1");
                }

                nfe55.Sort();
                nfe65.Sort();

                List<List<int>> nfe55Fora = new List<List<int>>();
                List<List<int>> nfe65Fora = new List<List<int>>();

                for (int i = 1; i < nfe55.Count(); i++)
                {
                    if ((nfe55[i] - (nfe55[i - 1])) > 1)
                    {
                        List<int> notesIntervalo = new List<int>();
                        notesIntervalo.Add(nfe55[i - 1]);
                        notesIntervalo.Add(nfe55[i]);
                        notesIntervalo.Add((nfe55[i] - nfe55[i - 1]) - 1);
                        nfe55Fora.Add(notesIntervalo);
                    }
                }

                for (int i = 1; i < nfe65.Count(); i++)
                {
                    if ((nfe65[i] - (nfe65[i - 1])) > 1)
                    {
                        List<int> noteIntervalo = new List<int>();
                        noteIntervalo.Add(nfe65[i - 1]);
                        noteIntervalo.Add(nfe65[i]);
                        noteIntervalo.Add((nfe65[i] - nfe65[i - 1]) -1);
                        nfe65Fora.Add(noteIntervalo);
                    }
                }

                ViewBag.N55 = nfe55;
                ViewBag.N65 = nfe65;
                ViewBag.Notas55 = nfe55Fora;
                ViewBag.Notas65 = nfe65Fora;



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
