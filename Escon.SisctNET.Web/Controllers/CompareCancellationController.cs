using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{
    public class CompareCancellationController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly IHostingEnvironment _appEnvironment;

        public CompareCancellationController(
           ICompanyService companyService,
           IConfigurationService configurationService,
           IHostingEnvironment env)
        {
            _companyService = companyService;
            _configurationService = configurationService;
            _appEnvironment = env;
        }

        [HttpPost]
        public async Task<IActionResult> Index(int id, string year, string month, Model.OrdemCancellation ordem, IFormFile arquivoSped)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                
                var period = Request.Form["period"];
                var inicio = Request.Form["inicio"];
                var fim = Request.Form["fim"];

                var company = _companyService.FindById(id, null);

                ViewBag.Ordem = ordem.ToString();
                ViewBag.Company = company;
                ViewBag.Period = period;
                ViewBag.Inicio = inicio;
                ViewBag.Fim = fim;

                SessionManager.SetMonthInSession(month);
                SessionManager.SetYearInSession(year);

                var importXml = new Xml.Import();
                var importSped = new Sped.Import();
                var importExcel = new Planilha.Import();
                var importEvento = new Evento.Import();
                var importDir = new Diretorio.Import();
                var importMonth = new Period.Month();

                var confDBSisctNfe = _configurationService.FindByName("NFe Saida");

                List<List<Dictionary<string, string>>> notesValidas = new List<List<Dictionary<string, string>>>();
                List<List<Dictionary<string, string>>> notesNFeCanceladas = new List<List<Dictionary<string, string>>>();
                List<List<Dictionary<string, string>>> notesNFeCanceladasEvento = new List<List<Dictionary<string, string>>>();
                List<List<Dictionary<string, string>>> notesNFCeCanceladas = new List<List<Dictionary<string, string>>>();
                List<List<Dictionary<string, string>>> notesNFCeCanceladasEvento = new List<List<Dictionary<string, string>>>();

                List<List<List<string>>> notasCanceladas = new List<List<List<string>>>();
                List<List<List<string>>> eventos = new List<List<List<string>>>();

                List<List<string>> spedNormal = new List<List<string>>();
                List<List<string>> spedNFeCancelada = new List<List<string>>();
                List<List<string>> spedNFCeCancelada = new List<List<string>>();
                List<List<string>> produtos = new List<List<string>>();

                List<string> dirtoriosNFe = new List<string>();
                List<string> dirtoriosNFCe = new List<string>();

                string caminhoDestinoArquivoOriginalSped = "";
                string caminho_WebRoot = _appEnvironment.WebRootPath;

                if (arquivoSped != null)
                {
                    string filedirSped = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Speds");

                    if (!Directory.Exists(filedirSped))
                        Directory.CreateDirectory(filedirSped);

                    string nomeArquivoSped = company.Document + "Empresa";

                    if (arquivoSped.FileName.Contains(".txt"))
                        nomeArquivoSped += ".txt";
                    else
                        nomeArquivoSped += ".tmp";

                    string caminhoDestinoArquivoSped = caminho_WebRoot + "\\Uploads\\Speds\\";
                    caminhoDestinoArquivoOriginalSped = caminhoDestinoArquivoSped + nomeArquivoSped;

                    string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivoSped);

                    if (System.IO.File.Exists(caminhoDestinoArquivoOriginalSped))
                        System.IO.File.Delete(caminhoDestinoArquivoOriginalSped);

                    var streamSped = new FileStream(caminhoDestinoArquivoOriginalSped, FileMode.Create);
                    await arquivoSped.CopyToAsync(streamSped);
                    streamSped.Close();

                }

                string directoryValida = "", directoryNFeCancelada = "", directoryNFCeCancelada = "";

                if (ordem.Equals(Model.OrdemCancellation.VerificarSefaz))
                {
                    directoryValida = importDir.SaidaSefaz(company, confDBSisctNfe.Value, year, month);
                    directoryNFeCancelada = importDir.NFeCanceladaSefaz(company, confDBSisctNfe.Value, year, month);
                    directoryNFCeCancelada = importDir.NFCeCanceladaSefaz(company, confDBSisctNfe.Value, year, month);
                    
                }
                else if (ordem.Equals(Model.OrdemCancellation.VerificarEmpresa))
                {
                    directoryValida = importDir.SaidaEmpresa(company, confDBSisctNfe.Value, year, month);
                }
                else if(ordem.Equals(Model.OrdemCancellation.NotasSefaz))
                {
                    directoryNFeCancelada = importDir.NFeCanceladaSefaz(company, confDBSisctNfe.Value, year, month);
                    directoryNFCeCancelada = importDir.NFCeCanceladaSefaz(company, confDBSisctNfe.Value, year, month);

                    if (period.Equals("periodo"))
                    {
                        var meses = importMonth.Months(inicio, fim);

                        foreach(var mes in meses)
                        {
                            directoryNFeCancelada = importDir.NFeCanceladaSefaz(company, confDBSisctNfe.Value, year, mes);
                            dirtoriosNFe.Add(directoryNFeCancelada);
                            directoryNFCeCancelada = importDir.NFCeCanceladaSefaz(company, confDBSisctNfe.Value, year, mes);
                            dirtoriosNFCe.Add(directoryNFCeCancelada);
                        }
                    }
                }
                else if(ordem.Equals(Model.OrdemCancellation.NotasEmpresa))
                {
                    directoryNFeCancelada = importDir.NFeCanceladaEmpresa(company, confDBSisctNfe.Value, year, month);
                    directoryNFCeCancelada = importDir.NFCeCanceladaEmpresa(company, confDBSisctNfe.Value, year, month);

                    if (period.Equals("periodo"))
                    {
                        var meses = importMonth.Months(inicio, fim);

                        foreach (var mes in meses)
                        {
                            directoryNFeCancelada = importDir.NFeCanceladaEmpresa(company, confDBSisctNfe.Value, year, month);
                            dirtoriosNFe.Add(directoryNFeCancelada);
                            directoryNFCeCancelada = importDir.NFCeCanceladaEmpresa(company, confDBSisctNfe.Value, year, month);
                            dirtoriosNFCe.Add(directoryNFCeCancelada);
                        }
                    }
                }

                if (ordem.Equals(Model.OrdemCancellation.VerificarSefaz))
                {
                    notesValidas = importXml.NFeResumeEmit(directoryValida);
                    notesNFeCanceladas = importXml.NFeResumeEmit(directoryNFeCancelada);
                    notesNFeCanceladasEvento = importEvento.Nfe(directoryNFeCancelada);
                    notesNFCeCanceladas = importXml.NFeResumeEmit(directoryNFCeCancelada);
                    notesNFCeCanceladasEvento = importEvento.Nfe(directoryNFCeCancelada);
                }
                else if (ordem.Equals(Model.OrdemCancellation.VerificarEmpresa))
                {
                    notesValidas = importXml.NFeResumeEmit(directoryValida);
                    spedNFeCancelada = importSped.NFeExitCanceled(caminhoDestinoArquivoOriginalSped, "55");
                    spedNFCeCancelada = importSped.NFeExitCanceled(caminhoDestinoArquivoOriginalSped, "65");
                }
                else
                {
                    if (period.Equals("periodo"))
                    {
                        for(int i = 0; i < dirtoriosNFe.Count(); i++)
                        {
                            List<List<Dictionary<string, string>>> notesNFeCanceladasTemp = new List<List<Dictionary<string, string>>>();
                            List<List<Dictionary<string, string>>> notesNFeCanceladasEventoTemp = new List<List<Dictionary<string, string>>>();

                            notesNFeCanceladasEventoTemp = importEvento.Nfe(dirtoriosNFe[i]);
                            notesNFeCanceladasEvento.AddRange(notesNFeCanceladasEventoTemp);
                            notesNFeCanceladasTemp = importXml.NFeAll(dirtoriosNFe[i]);
                            notesNFeCanceladas.AddRange(notesNFeCanceladasTemp);
                           
                        }

                        for(int i = 0; i < dirtoriosNFCe.Count(); i++)
                        {
                            List<List<Dictionary<string, string>>> notesNFCeCanceladasTemp = new List<List<Dictionary<string, string>>>();
                            List<List<Dictionary<string, string>>> notesNFCeCanceladasEventoTemp = new List<List<Dictionary<string, string>>>();
                           
                            notesNFCeCanceladasTemp = importXml.NFeAll(dirtoriosNFCe[i]);
                            notesNFCeCanceladas.AddRange(notesNFCeCanceladasTemp);
                            notesNFCeCanceladasEventoTemp = importEvento.Nfe(dirtoriosNFCe[i]);
                            notesNFCeCanceladasEvento.AddRange(notesNFCeCanceladasEventoTemp);
                        }
                    }
                    else
                    {
                        notesNFeCanceladas = importXml.NFeAll(directoryNFeCancelada);
                        notesNFeCanceladasEvento = importEvento.Nfe(directoryNFeCancelada);
                        notesNFCeCanceladas = importXml.NFeAll(directoryNFCeCancelada);
                        notesNFCeCanceladasEvento = importEvento.Nfe(directoryNFCeCancelada);
                    }
                }


                decimal totalNotas = 0, totalProdutos = 0;

                if (ordem.Equals(Model.OrdemCancellation.VerificarSefaz))
                {
                    notasCanceladas = importXml.NFeCanceladaSefaz(directoryValida, notesNFeCanceladas, notesNFeCanceladasEvento, notesNFCeCanceladas, notesNFCeCanceladasEvento);
                    eventos = importEvento.MoveCanceladaSefaz(directoryValida, notesNFeCanceladas, notesNFeCanceladasEvento, notesNFCeCanceladas, notesNFCeCanceladasEvento);

                    if (!Directory.Exists(directoryNFeCancelada))
                        Directory.CreateDirectory(directoryNFeCancelada);

                    for (int i = 0; i < notasCanceladas[0].Count(); i++)
                    {
                        var temp = notasCanceladas[0][i][0].Split("\\");
                        var dirtemp = directoryNFeCancelada + "\\" + temp[temp.Count() - 1];

                        if (System.IO.File.Exists(dirtemp))
                            System.IO.File.Delete(dirtemp);

                        if (System.IO.File.Exists(notasCanceladas[0][i][0]))
                            System.IO.File.Move(notasCanceladas[0][i][0], dirtemp);

                    }

                    for (int i = 0; i < eventos[0].Count(); i++)
                    {
                        var temp = eventos[0][i][0].Split("\\");
                        var dirtemp = directoryNFeCancelada + "\\" + temp[temp.Count() - 1];

                        if (System.IO.File.Exists(dirtemp))
                            System.IO.File.Delete(dirtemp);

                        if (System.IO.File.Exists(eventos[0][i][0]))
                            System.IO.File.Move(eventos[0][i][0], dirtemp);

                    }

                    if (!Directory.Exists(directoryNFCeCancelada))
                        Directory.CreateDirectory(directoryNFCeCancelada);

                    for (int i = 0; i < notasCanceladas[1].Count(); i++)
                    {
                        var temp = notasCanceladas[1][i][0].Split("\\");
                        var dirtemp = directoryNFCeCancelada + "\\" + temp[temp.Count() - 1];

                        if (System.IO.File.Exists(dirtemp))
                            System.IO.File.Delete(dirtemp);

                        if (System.IO.File.Exists(notasCanceladas[1][i][0]))
                            System.IO.File.Move(notasCanceladas[1][i][0], dirtemp);

                    }

                    for (int i = 0; i < eventos[1].Count(); i++)
                    {
                        var temp = eventos[1][i][0].Split("\\");
                        var dirtemp = directoryNFCeCancelada + "\\" + temp[temp.Count() - 1];

                        if (System.IO.File.Exists(dirtemp))
                            System.IO.File.Delete(dirtemp);

                        if (System.IO.File.Exists(eventos[1][i][0]))
                            System.IO.File.Move(eventos[1][i][0], dirtemp);
                    }
                }
                else if (ordem.Equals(Model.OrdemCancellation.VerificarEmpresa))
                {
                    notasCanceladas = importXml.NFeCanceladaEmpresa(directoryValida, spedNFeCancelada, spedNFCeCancelada);
                    eventos = importEvento.MoveCanceladaEmpresa(directoryValida, spedNFeCancelada, spedNFCeCancelada);

                    if (!Directory.Exists(directoryNFeCancelada))
                        Directory.CreateDirectory(directoryNFeCancelada);

                    for (int i = 0; i < notasCanceladas[0].Count(); i++)
                    {
                        var temp = notasCanceladas[0][i][0].Split("\\");
                        var dirtemp = directoryNFeCancelada + "\\" + temp[temp.Count() - 1];

                        if (System.IO.File.Exists(dirtemp))
                            System.IO.File.Delete(dirtemp);

                        if (System.IO.File.Exists(notasCanceladas[0][i][0]))
                            System.IO.File.Move(notasCanceladas[0][i][0], dirtemp);
                    }

                    for (int i = 0; i < eventos[0].Count(); i++)
                    {
                        var temp = eventos[0][i][0].Split("\\");
                        var dirtemp = directoryNFeCancelada + "\\" + temp[temp.Count() - 1];

                        if (System.IO.File.Exists(dirtemp))
                            System.IO.File.Delete(dirtemp);

                        if (System.IO.File.Exists(eventos[0][i][0]))
                            System.IO.File.Move(eventos[0][i][0], dirtemp);
                    }

                    if (!Directory.Exists(directoryNFCeCancelada))
                        Directory.CreateDirectory(directoryNFCeCancelada);

                    for (int i = 0; i < notasCanceladas[1].Count(); i++)
                    {
                        var temp = notasCanceladas[1][i][0].Split("\\");
                        var dirtemp = directoryNFCeCancelada + "\\" + temp[temp.Count() - 1];

                        if (System.IO.File.Exists(dirtemp))
                            System.IO.File.Delete(dirtemp);

                        if (System.IO.File.Exists(notasCanceladas[1][i][0]))
                            System.IO.File.Move(notasCanceladas[1][i][0], dirtemp);
                    }

                    for (int i = 0; i < eventos[1].Count(); i++)
                    {
                        var temp = eventos[1][i][0].Split("\\");
                        var dirtemp = directoryNFCeCancelada + "\\" + temp[temp.Count() - 1];

                        if (System.IO.File.Exists(dirtemp))
                            System.IO.File.Delete(dirtemp);

                        if (System.IO.File.Exists(eventos[1][i][0]))
                            System.IO.File.Move(eventos[1][i][0], dirtemp);
                    }
                }
                else
                {
                    for (int i = notesNFeCanceladas.Count - 1; i >= 0; i--)
                    {
                        if (!notesNFeCanceladas[i][2]["CNPJ"].Equals(company.Document))
                        {
                            notesNFeCanceladas.RemoveAt(i);
                            continue;
                        }

                        int pos = -1;

                        for (int j = 0; j < notesNFeCanceladas[i].Count(); j++)
                        {
                            if (notesNFeCanceladas[i][j].ContainsKey("cProd"))
                            {
                                for (int e = 0; e < produtos.Count(); e++)
                                {
                                    if (produtos[e][0].Equals(notesNFeCanceladas[i][j]["cProd"]))
                                    {
                                        pos = e;
                                        break;
                                    }
                                }

                                if (pos < 0)
                                {
                                    List<string> produto = new List<string>();
                                    produto.Add(notesNFeCanceladas[i][j]["cProd"]);
                                    produto.Add(notesNFeCanceladas[i][j]["xProd"]);
                                    produto.Add("0");

                                    produtos.Add(produto);

                                    pos = produtos.Count() - 1;
                                }
                            }

                            if (notesNFeCanceladas[i][j].ContainsKey("vProd") && notesNFeCanceladas[i][j].ContainsKey("cProd"))
                            {
                                produtos[pos][2] = (Convert.ToDecimal(produtos[pos][2]) + Convert.ToDecimal(notesNFeCanceladas[i][j]["vProd"])).ToString();

                                totalProdutos += Convert.ToDecimal(notesNFeCanceladas[i][j]["vProd"]);
                            }

                            if (notesNFeCanceladas[i][j].ContainsKey("vFrete") && notesNFeCanceladas[i][j].ContainsKey("cProd"))
                            {
                                produtos[pos][2] = (Convert.ToDecimal(produtos[pos][2]) + Convert.ToDecimal(notesNFeCanceladas[i][j]["vFrete"])).ToString();

                                totalProdutos += Convert.ToDecimal(notesNFeCanceladas[i][j]["vFrete"]);
                            }

                            if (notesNFeCanceladas[i][j].ContainsKey("vDesc") && notesNFeCanceladas[i][j].ContainsKey("cProd"))
                            {
                                produtos[pos][2] = (Convert.ToDecimal(produtos[pos][2]) - Convert.ToDecimal(notesNFeCanceladas[i][j]["vDesc"])).ToString();

                                totalProdutos -= Convert.ToDecimal(notesNFeCanceladas[i][j]["vDesc"]);

                            }

                            if (notesNFeCanceladas[i][j].ContainsKey("vOutro") && notesNFeCanceladas[i][j].ContainsKey("cProd"))
                            {
                                produtos[pos][2] = (Convert.ToDecimal(produtos[pos][2]) + Convert.ToDecimal(notesNFeCanceladas[i][j]["vOutro"])).ToString();

                                totalProdutos += Convert.ToDecimal(notesNFeCanceladas[i][j]["vOutro"]);

                            }

                            if (notesNFeCanceladas[i][j].ContainsKey("vSeg") && notesNFeCanceladas[i][j].ContainsKey("cProd"))
                            {
                                produtos[pos][2] = (Convert.ToDecimal(produtos[pos][2]) + Convert.ToDecimal(notesNFeCanceladas[i][j]["vSeg"])).ToString();

                                totalProdutos += Convert.ToDecimal(notesNFeCanceladas[i][j]["vSeg"]);

                            }

                            if (notesNFeCanceladas[i][j].ContainsKey("vNF"))
                            {
                                totalNotas += Convert.ToDecimal(notesNFeCanceladas[i][j]["vNF"]);
                            }

                        }

                    }

                    for (int i = notesNFCeCanceladas.Count - 1; i >= 0; i--)
                    {
                        if (!notesNFCeCanceladas[i][2]["CNPJ"].Equals(company.Document))
                        {
                            notesNFCeCanceladas.RemoveAt(i);
                            continue;
                        }

                        int pos = -1;

                        for (int j = 0; j < notesNFCeCanceladas[i].Count(); j++)
                        {
                            if (notesNFCeCanceladas[i][j].ContainsKey("cProd"))
                            {
                                for(int e = 0; e < produtos.Count(); e++)
                                {
                                    if (produtos[e][0].Equals(notesNFCeCanceladas[i][j]["cProd"]))
                                    {
                                        pos = e;
                                        break;
                                    }
                                }

                                if(pos < 0)
                                {
                                    List<string> produto = new List<string>();
                                    produto.Add(notesNFCeCanceladas[i][j]["cProd"]);
                                    produto.Add(notesNFCeCanceladas[i][j]["xProd"]);
                                    produto.Add("0");

                                    produtos.Add(produto);

                                    pos = produtos.Count() - 1;
                                }
                            }

                            if (notesNFCeCanceladas[i][j].ContainsKey("vProd") && notesNFCeCanceladas[i][j].ContainsKey("cProd"))
                            {
                                produtos[pos][2] = (Convert.ToDecimal(produtos[pos][2]) + Convert.ToDecimal(notesNFCeCanceladas[i][j]["vProd"])).ToString();

                                totalProdutos += Convert.ToDecimal(notesNFCeCanceladas[i][j]["vProd"]);
                            }

                            if (notesNFCeCanceladas[i][j].ContainsKey("vFrete") && notesNFCeCanceladas[i][j].ContainsKey("cProd"))
                            {
                                produtos[pos][2] = (Convert.ToDecimal(produtos[pos][2]) + Convert.ToDecimal(notesNFCeCanceladas[i][j]["vFrete"])).ToString();

                                totalProdutos += Convert.ToDecimal(notesNFCeCanceladas[i][j]["vFrete"]);
                            }

                            if (notesNFCeCanceladas[i][j].ContainsKey("vDesc") && notesNFCeCanceladas[i][j].ContainsKey("cProd"))
                            {
                                produtos[pos][2] = (Convert.ToDecimal(produtos[pos][2]) - Convert.ToDecimal(notesNFCeCanceladas[i][j]["vDesc"])).ToString();

                                totalProdutos -= Convert.ToDecimal(notesNFCeCanceladas[i][j]["vDesc"]);

                            }

                            if (notesNFCeCanceladas[i][j].ContainsKey("vOutro") && notesNFCeCanceladas[i][j].ContainsKey("cProd"))
                            {
                                produtos[pos][2] = (Convert.ToDecimal(produtos[pos][2]) + Convert.ToDecimal(notesNFCeCanceladas[i][j]["vOutro"])).ToString();

                                totalProdutos += Convert.ToDecimal(notesNFCeCanceladas[i][j]["vOutro"]);

                            }

                            if (notesNFCeCanceladas[i][j].ContainsKey("vSeg") && notesNFCeCanceladas[i][j].ContainsKey("cProd"))
                            {
                                produtos[pos][2] = (Convert.ToDecimal(produtos[pos][2]) + Convert.ToDecimal(notesNFCeCanceladas[i][j]["vSeg"])).ToString();

                                totalProdutos += Convert.ToDecimal(notesNFCeCanceladas[i][j]["vSeg"]);

                            }

                            if (notesNFCeCanceladas[i][j].ContainsKey("vNF"))
                            {
                                totalNotas += Convert.ToDecimal(notesNFCeCanceladas[i][j]["vNF"]);
                            }

                        }

                    }
                }

                ViewBag.TotalNotas = totalNotas;
                ViewBag.TotalProutos = totalProdutos;

                // Notas
                ViewBag.NotasCanceladas = notasCanceladas;
                ViewBag.NFeCanceladas = notesNFeCanceladas.OrderBy(_ => Convert.ToInt32(_[1]["nNF"])).ToList();
                ViewBag.NFCeCanceladas = notesNFCeCanceladas.OrderBy(_ => Convert.ToInt32(_[1]["nNF"])).ToList();

                // Eventos
                ViewBag.Eventos = eventos;
                ViewBag.NFeCanceladasEvento = notesNFeCanceladasEvento.OrderBy(_ => _[0]["chNFe"]).ToList();
                ViewBag.NFCeCanceladasEvento = notesNFCeCanceladasEvento.OrderBy(_ => _[0]["chNFe"]).ToList();

                // Produtos
                ViewBag.Produtos = produtos.OrderBy(_ => Convert.ToInt32(_[0])).ToList();

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
