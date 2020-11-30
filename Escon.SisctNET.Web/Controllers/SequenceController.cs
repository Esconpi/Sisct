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
                var importDir = new Diretorio.Import();

                var confDBSisctNfe = _configurationService.FindByName("NFe Saida");
                var onfDBSisctCte = _configurationService.FindByName("CTe Saida");

                List<List<List<int>>> nfe55 = new List<List<List<int>>>();
                List<List<List<int>>> nfe65 = new List<List<List<int>>>();

                if (archive.Equals(Model.Archive.XmlNFeSefaz) || archive.Equals(Model.Archive.XmlNFeEmpresa))
                {
                    List<List<Dictionary<string, string>>> notesValidas = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesNFeCanceladas = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesNFeCanceladasEvento = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesNFCeCanceladas = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesNFCeCanceladasEvento = new List<List<Dictionary<string, string>>>();


                    string directoryValida = "", directoryNFeCancelada = "", directoryNFCeCancelada = "";
                    if (archive.Equals(Model.Archive.XmlNFeEmpresa))
                    {
                        directoryValida = importDir.SaidaEmpresa(comp, confDBSisctNfe.Value, year, month);
                        directoryNFeCancelada = importDir.NFeCanceladaEmpresa(comp, confDBSisctNfe.Value, year, month);
                        directoryNFCeCancelada = importDir.NFCeCanceladaEmpresa(comp, confDBSisctNfe.Value, year, month);
                    }
                    else
                    {
                        directoryValida = importDir.SaidaSefaz(comp, confDBSisctNfe.Value, year, month);
                        directoryNFeCancelada = importDir.NFeCanceladaSefaz(comp, confDBSisctNfe.Value, year, month);
                        directoryNFCeCancelada = importDir.NFCeCanceladaSefaz(comp, confDBSisctNfe.Value, year, month);
                    }

                    notesValidas = importXml.NfeResume(directoryValida);
                    notesNFeCanceladas = importXml.NfeResume(directoryNFeCancelada);
                    notesNFeCanceladasEvento = importEvento.Nfe(directoryNFeCancelada);
                    notesNFCeCanceladas = importXml.NfeResume(directoryNFCeCancelada);
                    notesNFCeCanceladasEvento = importEvento.Nfe(directoryNFCeCancelada);

                    for (int i = notesValidas.Count - 1; i >= 0; i--)
                    {
                        if (!notesValidas[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notesValidas.RemoveAt(i);
                            continue;
                        }

                        if (notesValidas[i][1]["mod"].Equals("55"))
                        {
                            // Notas Modelo 55

                            int pos = -1;

                            for (int k = 0; k < nfe55.Count(); k++)
                            {
                                for (int e = 0; e < nfe55[k].Count(); e++)
                                {
                                    if (nfe55[k][e][0].Equals(Convert.ToInt32(notesValidas[i][1]["serie"])))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                            }

                            if (pos < 0)
                            {
                                List<List<int>> nfe = new List<List<int>>();
                                nfe55.Add(nfe);
                                pos = nfe55.Count() - 1;
                            }

                            List<int> nota = new List<int>();
                            nota.Add(Convert.ToInt32(notesValidas[i][1]["serie"]));
                            nota.Add(Convert.ToInt32(notesValidas[i][1]["nNF"]));
                            nfe55[pos].Add(nota);                           
                        }
                        else
                        {
                            // Notas Modelo 65

                            int pos = -1;

                            for (int k = 0; k < nfe65.Count(); k++)
                            {
                                for (int e = 0; e < nfe65[k].Count(); e++)
                                {
                                    if (nfe65[k][e][0].Equals(Convert.ToInt32(notesValidas[i][1]["serie"])))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                            }

                            if (pos < 0)
                            {
                                List<List<int>> nfce = new List<List<int>>();
                                nfe65.Add(nfce);
                                pos = nfe65.Count() - 1;
                            }

                            List<int> nota = new List<int>();
                            nota.Add(Convert.ToInt32(notesValidas[i][1]["serie"]));
                            nota.Add(Convert.ToInt32(notesValidas[i][1]["nNF"]));
                            nfe65[pos].Add(nota);
                        }
                    }
                    
                    for (int i = notesNFeCanceladas.Count - 1; i >= 0; i--)
                    {
                        if (!notesNFeCanceladas[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notesNFeCanceladas.RemoveAt(i);
                            continue;
                        }

                        if (notesNFeCanceladas[i][1]["mod"].Equals("55"))
                        {
                            // Notas Modelo 55

                            int pos = -1;

                            for (int k = 0; k < nfe55.Count(); k++)
                            {
                                for (int e = 0; e < nfe55[k].Count(); e++)
                                {
                                    if (nfe55[k][e][0].Equals(Convert.ToInt32(notesNFeCanceladas[i][1]["serie"])))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                            }

                            if (pos < 0)
                            {
                                List<List<int>> nfe = new List<List<int>>();
                                nfe55.Add(nfe);
                                pos = nfe55.Count() - 1;
                            }

                            List<int> nota = new List<int>();
                            nota.Add(Convert.ToInt32(notesNFeCanceladas[i][1]["serie"]));
                            nota.Add(Convert.ToInt32(notesNFeCanceladas[i][1]["nNF"]));
                            nfe55[pos].Add(nota);
                        }
                        else
                        {
                            // Notas Modelo 65

                            int pos = -1;

                            for (int k = 0; k < nfe65.Count(); k++)
                            {
                                for (int e = 0; e < nfe65[k].Count(); e++)
                                {
                                    if (nfe65[k][e][0].Equals(Convert.ToInt32(notesNFeCanceladas[i][1]["serie"])))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                            }

                            if (pos < 0)
                            {
                                List<List<int>> nfce = new List<List<int>>();
                                nfe65.Add(nfce);
                                pos = nfe65.Count() - 1;
                            }

                            List<int> nota = new List<int>();
                            nota.Add(Convert.ToInt32(notesNFeCanceladas[i][1]["serie"]));
                            nota.Add(Convert.ToInt32(notesNFeCanceladas[i][1]["nNF"]));
                            nfe65[pos].Add(nota);
                        }
                    }

                    foreach (var note in notesNFeCanceladasEvento)
                    {
                        if (note[0]["chNFe"].Substring(20, 2).Equals("55"))
                        {
                            // Notas Modelo 55

                            int pos = -1;

                            for (int k = 0; k < nfe55.Count(); k++)
                            {
                                for (int e = 0; e < nfe55[k].Count(); e++)
                                {
                                    if (nfe55[k][e][0].Equals(Convert.ToInt32(note[0]["chNFe"].Substring(22, 3))))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                            }

                            if (pos < 0)
                            {
                                List<List<int>> nfe = new List<List<int>>();
                                nfe55.Add(nfe);
                                pos = nfe55.Count() - 1;
                            }

                            List<int> nota = new List<int>();
                            nota.Add(Convert.ToInt32(note[0]["chNFe"].Substring(22, 3)));
                            nota.Add(Convert.ToInt32(note[0]["chNFe"].Substring(25, 9)));
                            nfe55[pos].Add(nota);
                        }
                        else
                        {
                            // Notas Modelo 65

                            int pos = -1;

                            for (int k = 0; k < nfe65.Count(); k++)
                            {
                                for (int e = 0; e < nfe65[k].Count(); e++)
                                {
                                    if (nfe65[k][e][0].Equals(Convert.ToInt32(note[0]["chNFe"].Substring(22, 3))))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                            }

                            if (pos < 0)
                            {
                                List<List<int>> nfce = new List<List<int>>();
                                nfe65.Add(nfce);
                                pos = nfe65.Count() - 1;
                            }

                            List<int> nota = new List<int>();
                            nota.Add(Convert.ToInt32(note[0]["chNFe"].Substring(22, 3)));
                            nota.Add(Convert.ToInt32(note[0]["chNFe"].Substring(25, 9)));
                            nfe65[pos].Add(nota);
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
                            // Notas Modelo 55

                            int pos = -1;

                            for (int k = 0; k < nfe55.Count(); k++)
                            {
                                for (int e = 0; e < nfe55[k].Count(); e++)
                                {
                                    if (nfe55[k][e][0].Equals(Convert.ToInt32(notesNFCeCanceladas[i][1]["serie"])))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                            }

                            if (pos < 0)
                            {
                                List<List<int>> nfe = new List<List<int>>();
                                nfe55.Add(nfe);
                                pos = nfe55.Count() - 1;
                            }

                            List<int> nota = new List<int>();
                            nota.Add(Convert.ToInt32(notesNFCeCanceladas[i][1]["serie"]));
                            nota.Add(Convert.ToInt32(notesNFCeCanceladas[i][1]["nNF"]));
                            nfe55[pos].Add(nota);
                        }
                        else
                        {
                            // Notas Modelo 65

                            int pos = -1;

                            for (int k = 0; k < nfe65.Count(); k++)
                            {
                                for (int e = 0; e < nfe65[k].Count(); e++)
                                {
                                    if (nfe65[k][e][0].Equals(Convert.ToInt32(notesNFCeCanceladas[i][1]["serie"])))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                            }

                            if (pos < 0)
                            {
                                List<List<int>> nfce = new List<List<int>>();
                                nfe65.Add(nfce);
                                pos = nfe65.Count() - 1;
                            }

                            List<int> nota = new List<int>();
                            nota.Add(Convert.ToInt32(notesNFCeCanceladas[i][1]["serie"]));
                            nota.Add(Convert.ToInt32(notesNFCeCanceladas[i][1]["nNF"]));
                            nfe65[pos].Add(nota);
                        }

                    }

                    foreach (var note in notesNFCeCanceladasEvento)
                    {
                        
                        if (note[0]["chNFe"].Substring(20, 2).Equals("55"))
                        {
                            // Notas Modelo 55

                            int pos = -1;

                            for (int k = 0; k < nfe55.Count(); k++)
                            {
                                for (int e = 0; e < nfe55[k].Count(); e++)
                                {
                                    if (nfe55[k][e][0].Equals(Convert.ToInt32(note[0]["chNFe"].Substring(22, 3))))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                            }

                            if (pos < 0)
                            {
                                List<List<int>> nfe = new List<List<int>>();
                                nfe55.Add(nfe);
                                pos = nfe55.Count() - 1;
                            }

                            List<int> nota = new List<int>();
                            nota.Add(Convert.ToInt32(note[0]["chNFe"].Substring(22, 3)));
                            nota.Add(Convert.ToInt32(note[0]["chNFe"].Substring(25, 9)));
                            nfe55[pos].Add(nota);
                        }
                        else
                        {
                            // Notas Modelo 65

                            int pos = -1;

                            for (int k = 0; k < nfe65.Count(); k++)
                            {
                                for (int e = 0; e < nfe65[k].Count(); e++)
                                {
                                    if (nfe65[k][e][0].Equals(Convert.ToInt32(note[0]["chNFe"].Substring(22, 3))))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                            }

                            if (pos < 0)
                            {
                                List<List<int>> nfce = new List<List<int>>();
                                nfe65.Add(nfce);
                                pos = nfe65.Count() - 1;
                            }

                            List<int> nota = new List<int>();
                            nota.Add(Convert.ToInt32(note[0]["chNFe"].Substring(22, 3)));
                            nota.Add(Convert.ToInt32(note[0]["chNFe"].Substring(25, 9)));
                            nfe65[pos].Add(nota);
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
                            // Notas Modelo 55

                            int pos = -1;

                            for (int k = 0; k < nfe55.Count(); k++)
                            {
                                for (int e = 0; e < nfe55[k].Count(); e++)
                                {
                                    if (nfe55[k][e][0].Equals(Convert.ToInt32(note[6])))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                            }

                            if (pos < 0)
                            {
                                List<List<int>> nfe = new List<List<int>>();
                                nfe55.Add(nfe);
                                pos = nfe55.Count() - 1;
                            }

                            List<int> nota = new List<int>();
                            nota.Add(Convert.ToInt32(note[6]));
                            nota.Add(Convert.ToInt32(note[2]));
                            nfe55[pos].Add(nota);
                        }
                        else
                        {
                            // Notas Modelo 65

                            int pos = -1;

                            for (int k = 0; k < nfe65.Count(); k++)
                            {
                                for (int e = 0; e < nfe65[k].Count(); e++)
                                {
                                    if (nfe65[k][e][0].Equals(Convert.ToInt32(note[6])))
                                    {
                                        pos = k;
                                        break;
                                    }
                                }

                            }

                            if (pos < 0)
                            {
                                List<List<int>> nfce = new List<List<int>>();
                                nfe65.Add(nfce);
                                pos = nfe65.Count() - 1;
                            }

                            List<int> nota = new List<int>();
                            nota.Add(Convert.ToInt32(note[6]));
                            nota.Add(Convert.ToInt32(note[2]));
                            nfe55[pos].Add(nota);
                            nfe65[pos].Add(nota);
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

                List<List<int>> nfe55Fora = new List<List<int>>();
                List<List<int>> nfe65Fora = new List<List<int>>();

                int qtd55 = 0, qtd65 = 0, min55 = 0, max55 = 0, min65 = 0, max65 = 0;

                // Notas Modelo 55
                for (int i = 0; i < nfe55.Count(); i++)
                {
                    var nfe55Ordenada = nfe55[i].ToList().OrderBy(_ => _[1]).ToList();

                    if(i == 0)
                    {
                        min55 = nfe55Ordenada[0][1];
                        max55 = nfe55Ordenada[nfe55Ordenada.Count() - 1][1];
                    }

                    if(nfe55Ordenada[0][1] < min55)
                    {
                        min55 = nfe55Ordenada[0][1];
                    }

                    if (nfe55Ordenada[nfe55Ordenada.Count() - 1][1] > max55)
                    {
                        max55 = nfe55Ordenada[nfe55Ordenada.Count() - 1][1];
                    }

                    qtd55 += nfe55Ordenada.Count();

                    for (int j = 1; j < nfe55Ordenada.Count(); j++)
                    {
                        if ((nfe55Ordenada[j][1] - (nfe55Ordenada[j - 1][1])) > 1)
                        {
                            List<int> notesIntervalo = new List<int>();
                            notesIntervalo.Add(nfe55Ordenada[j][0]);
                            notesIntervalo.Add(nfe55Ordenada[j - 1][1]);
                            notesIntervalo.Add(nfe55Ordenada[j][1]);
                            notesIntervalo.Add((nfe55Ordenada[j][1] - nfe55Ordenada[j - 1][1]) - 1);

                            nfe55Fora.Add(notesIntervalo);
                        }
                    }

                }

                // Notas Modelo 65
                for (int i = 0; i < nfe65.Count(); i++)
                {
                    var nfe65Ordenada = nfe65[i].ToList().OrderBy(_ => _[1]).ToList();

                    if (i == 0)
                    {
                        min65 = nfe65Ordenada[0][1];
                        max65 = nfe65Ordenada[nfe65Ordenada.Count() - 1][1];
                    }

                    if (nfe65Ordenada[0][1] < min65)
                    {
                        min65 = nfe65Ordenada[0][1];
                    }

                    if (nfe65Ordenada[nfe65Ordenada.Count() - 1][1] > max65)
                    {
                        max65 = nfe65Ordenada[nfe65Ordenada.Count() - 1][1];
                    }

                    qtd65 += nfe65Ordenada.Count();

                    for (int j = 1; j < nfe65Ordenada.Count(); j++)
                    {
                        if ((nfe65Ordenada[j][1] - (nfe65Ordenada[j - 1][1])) > 1)
                        {
                            List<int> notesIntervalo = new List<int>();
                            notesIntervalo.Add(nfe65Ordenada[j][0]);
                            notesIntervalo.Add(nfe65Ordenada[j - 1][1]);
                            notesIntervalo.Add(nfe65Ordenada[j][1]);
                            notesIntervalo.Add((nfe65Ordenada[j][1] - nfe65Ordenada[j - 1][1]) - 1);

                            nfe65Fora.Add(notesIntervalo);
                        }
                    }

                }

                ViewBag.Qtd55 = qtd55;
                ViewBag.Qtd65 = qtd65;
                ViewBag.Min55 = min55;
                ViewBag.Max55 = max55;
                ViewBag.Min65 = min65;
                ViewBag.Max65 = max65;
                ViewBag.Notas55 = nfe55Fora.OrderBy(_ => _[0]).ToList();
                ViewBag.Notas65 = nfe65Fora.OrderBy(_ => _[0]).ToList();

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
