using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace Escon.SisctNET.Web.Controllers
{
    public class CompareController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly IHostingEnvironment _appEnvironment;

        public CompareController(
           ICompanyService companyService,
           IConfigurationService configurationService,
           IHostingEnvironment env)
        {
            _companyService = companyService;
            _configurationService = configurationService;
            _appEnvironment = env;
        }

        [HttpPost]
        public async Task<IActionResult> Index(Model.Opcao opcao, Model.Ordem ordem, IFormFile arquivoSped, IFormFile arquivoExcel)
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

                ViewBag.Opcao = opcao.ToString();
                ViewBag.Ordem = ordem.ToString();
                ViewBag.Document = company.Document;
                ViewBag.SocialName = company.SocialName;
                ViewBag.Ano = year;
                ViewBag.Mes = month;

                var importXml = new Xml.Import();
                var importSped = new Sped.Import();
                var imporrExcel = new Planilha.Import();
                var confDBSisctNfe = new Model.Configuration();

                if (arquivoSped == null || arquivoSped.Length == 0)
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

                if (arquivoSped.FileName.Contains(".txt"))
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
                await arquivoSped.CopyToAsync(streamSped);
                streamSped.Close();

                if (opcao.Equals(Model.Opcao.NFe))
                {

                    List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
                    List<List<string>> sped = new List<List<string>>();
                    List<List<string>> SpedDif = new List<List<string>>();

                    if (ident.Equals("0"))
                    {
                        confDBSisctNfe = _configurationService.FindByName("NFe");
                    }
                    else if (ident.Equals("1"))
                    {
                        confDBSisctNfe = _configurationService.FindByName("NFe Saida");
                    }

                    string directoryNfe = confDBSisctNfe.Value + "\\" + company.Document + "\\" + year + "\\" + month;

                    notes = importXml.NfeResume(directoryNfe);
                        
                    if (ordem.Equals(Model.Ordem.DifereValor) || ordem.Equals(Model.Ordem.SisCT))
                    {
                        SpedDif = importSped.SpedDif(caminhoDestinoArquivoOriginalSped);
                    }
                    else
                    {
                        if (ident.Equals("0"))
                        {
                            //sped = importSped.SpedNfe(caminhoDestinoArquivoOriginalSped);
                            sped = importSped.SpedNfe(caminhoDestinoArquivoOriginalSped, ident);
                        }
                        else if (ident.Equals("1"))
                        {
                            sped = importSped.SpedNfeSaida(caminhoDestinoArquivoOriginalSped);
                        }
                    }

                    List<List<Dictionary<string, string>>> notas = new List<List<Dictionary<string, string>>>();
                    List<List<string>> notas_sped = new List<List<string>>();

                    if (ordem.Equals(Model.Ordem.Xml))
                    {
                        foreach (var note in notes)
                        {
                            string nota_xml = note[0]["chave"];
                            bool nota_encontrada = false;
                            foreach (var nota_sped in sped)
                            {
                                if (nota_xml.Equals(nota_sped[0]))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }
                            if (nota_encontrada.Equals(false))
                            {
                                notas.Add(note);
                            }
                        }
                            
                    }
                    else if (ordem.Equals(Model.Ordem.Sped))
                    {
                        foreach (var note in sped)
                        {
                            bool nota_encontrada = false;

                            foreach (var notaXml in notes)
                            {
                                string nota_xml = notaXml[0]["chave"];
                                if (note[0].Equals(nota_xml))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }
                            string cnpj_chave = !note[0].Length.Equals(0) ? note[0].Substring(6, 14) : "";
                            if (nota_encontrada.Equals(false))
                            {
                                notas_sped.Add(note);
                            }

                            /*if (nota_encontrada.Equals(false))
                            {
                                if (note[4].Equals("0") && note[5].Equals("0"))
                                {
                                    notas_sped.Add(note);
                                }
                            }*/
                        }

                    }
                    else if (ordem.Equals(Model.Ordem.DifereValor))
                    {
                        List<List<string>> registros = new List<List<string>>();
                        foreach (var linha in SpedDif)
                        {
                            List<string> Valores = new List<string>();
                            foreach (var notaXml in notes)
                            {
                                string nota_xml = notaXml[0]["chave"];
                                if (linha[1].Equals(nota_xml))
                                {
                                    string fornecedor = notaXml[2]["xNome"];
                                    string totalXml = notaXml[3]["vNF"];
                                    string totalSped = linha[2].Equals("") ? "0" : linha[2].Replace('.', '*').Replace(',', '.').Replace('*', ',');
                                    string totalDif = (Convert.ToDecimal(totalXml) - Convert.ToDecimal(totalSped)).ToString();                                       
                                    string descXml = notaXml[3]["vDesc"];
                                    string descSped = linha[3].Equals("") ? "0" : linha[3].Replace('.', '*').Replace(',', '.').Replace('*', ',');
                                    string descDif = (Convert.ToDecimal(descXml) - Convert.ToDecimal(descSped)).ToString();
                                    string outDespXml = notaXml[3]["vOutro"];
                                    string outDespSped = linha[6].Equals("") ? "0" : linha[6].Replace('.', '*').Replace(',', '.').Replace('*', ',');
                                    string outDespDif = (Convert.ToDecimal(outDespXml) - Convert.ToDecimal(outDespSped)).ToString();
                                    string segXml = notaXml[3]["vSeg"];
                                    string segSped = linha[5].Equals("") ? "0" : linha[5].Replace('.', '*').Replace(',', '.').Replace('*', ',');
                                    string segDif = (Convert.ToDecimal(segXml) - Convert.ToDecimal(segSped)).ToString();
                                    string freteXml = notaXml[3]["vFrete"];
                                    string freteSped = linha[4].Equals("") ? "0" : linha[4].Replace('.', '*').Replace(',', '.').Replace('*', ',');
                                    string freteDif = (Convert.ToDecimal(freteXml) - Convert.ToDecimal(freteSped)).ToString();

                                    if (!Convert.ToDecimal(totalDif).Equals(0) || !Convert.ToDecimal(descDif).Equals(0) ||
                                        !Convert.ToDecimal(outDespDif).Equals(0) || !Convert.ToDecimal(segDif).Equals(0) || 
                                        !Convert.ToDecimal(freteDif).Equals(0))
                                    {
                                        Valores.Add(linha[0]);
                                        Valores.Add(fornecedor);
                                        Valores.Add(totalXml);
                                        Valores.Add(totalSped);
                                        Valores.Add(totalDif);
                                        Valores.Add(descXml);
                                        Valores.Add(descSped);
                                        Valores.Add(descDif);
                                        Valores.Add(outDespXml);
                                        Valores.Add(outDespSped);
                                        Valores.Add(outDespDif);
                                        Valores.Add(freteXml);
                                        Valores.Add(freteSped);
                                        Valores.Add(freteDif);
                                        Valores.Add(segXml);
                                        Valores.Add(segSped);
                                        Valores.Add(segDif);
                                        registros.Add(Valores);
                                    }
                                }
                            }

                        }
                        ViewBag.Valores = registros;
                    }
                    else if (ordem.Equals(Model.Ordem.SisCT))
                    {
                        List<List<string>> registros = new List<List<string>>();
                        decimal valorTotaGeralXml = 0, valorTotalGeralSped = 0;
                        foreach (var linha in SpedDif)
                        {
                            List<string> Valores = new List<string>();
                            foreach (var notaXml in notes)
                            {
                                string nota_xml = notaXml[0]["chave"];
                                if (linha[1].Equals(nota_xml))
                                {
                                    string fornecedor = notaXml[2]["xNome"];
                                    string totalXml = notaXml[3]["vNF"];
                                    string totalSped = linha[2].Equals("") ? "0" : linha[2].Replace('.', '*').Replace(',', '.').Replace('*', ',');
                                    string totalDif = (Convert.ToDecimal(totalXml) - Convert.ToDecimal(totalSped)).ToString();

                                    Valores.Add(linha[0]);
                                    Valores.Add(fornecedor);
                                    Valores.Add(totalXml);
                                    Valores.Add(totalSped);
                                    Valores.Add(totalDif);
                                    registros.Add(Valores);
                                    valorTotaGeralXml += Convert.ToDecimal(totalXml);
                                    valorTotalGeralSped += Convert.ToDecimal(totalSped);
                                }
                            }

                        }
                        ViewBag.Valores = registros;
                        ViewBag.TotalXml = valorTotaGeralXml;
                        ViewBag.TotalSped = valorTotalGeralSped;
                        ViewBag.TotalDif = valorTotaGeralXml - valorTotalGeralSped;
                    }

                    ViewBag.Notas = notas;
                    ViewBag.notas_sped = notas_sped;
                       
                }
                else if (opcao.Equals(Model.Opcao.CTe))
                {
                    var confDBSisctCte = _configurationService.FindByName("CTe");
                        
                    string directoryCte = confDBSisctCte.Value + "\\" + company.Document + "\\" + year + "\\" + month;

                    List<List<Dictionary<string, string>>> ctes = new List<List<Dictionary<string, string>>>();
                    List<string> sped = new List<string>();

                    ctes = importXml.Cte(directoryCte);

                    sped = importSped.SpedCte(caminhoDestinoArquivoOriginalSped);

                    List<List<Dictionary<string, string>>> ctes_nao_encontrados = new List<List<Dictionary<string, string>>>();

                    foreach (var cte in ctes)
                    {
                        for (int i = 0; i < cte.Count; i++)
                        {
                            if (cte[i].ContainsKey("chave"))
                            {
                                string cte_xml = cte[i]["chave"];
                                bool cte_encontrado = false;
                                for (int k = 0; k < sped.Count(); k++)
                                {
                                    if (cte_xml == sped[k])
                                    {
                                        cte_encontrado = true;
                                        break;
                                    }
                                }

                                if (cte_encontrado == false)
                                {
                                    ctes_nao_encontrados.Add(cte);
                                }
                            }
                        }
                    }

                    List<string> modal = new List<string> { "Rod", "Aér", "Aquav", "Ferrov", "Dutov", "Multi" };
                    List<string> tipo = new List<string> { "Normal", "Subcontratação", "Redespacho", "Red. Inter", "Ser. Vin. Multi" };
                    ViewBag.Modal = modal;
                    ViewBag.Tipos = tipo;
                    ViewBag.Ctes = ctes_nao_encontrados;
                        
                }
                else if (opcao.Equals(Model.Opcao.Planilha))
                {
                    if (arquivoExcel == null || arquivoExcel.Length == 0)
                    {
                        ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                        return View(ViewData);
                    }

                    string filedirExcel = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Planilha");

                    if (!Directory.Exists(filedirExcel))
                    {
                        Directory.CreateDirectory(filedirExcel);
                    }

                    string nomeArquivoExcel = company.Document + year + month;

                    if (arquivoExcel.FileName.Contains(".xls") || arquivoExcel.FileName.Contains(".xlsx"))
                        nomeArquivoExcel += ".xlsx";

                    string caminhoDestinoArquivoExcel = caminho_WebRoot + "\\Uploads\\Planilha\\";
                    string caminhoDestinoArquivoOriginalExcel = caminhoDestinoArquivoExcel + nomeArquivoExcel;

                    string[] paths_upload_excel = Directory.GetFiles(caminhoDestinoArquivoExcel);
                    if (System.IO.File.Exists(caminhoDestinoArquivoOriginalExcel))
                    {
                        System.IO.File.Delete(caminhoDestinoArquivoOriginalExcel);

                    }
                    var streamExcel = new FileStream(caminhoDestinoArquivoOriginalExcel, FileMode.Create);
                    await arquivoExcel.CopyToAsync(streamExcel);
                    streamExcel.Close();

                    List<List<string>> notasExcel = new List<List<string>>();

                    if (ordem.Equals(Model.Ordem.Malha))
                    {
                        var notesPlanilha = imporrExcel.Notes(caminhoDestinoArquivoOriginalExcel);
                        var notesSped = importSped.SpedNfe(caminhoDestinoArquivoOriginalSped, ident);

                        foreach (var nPlanilha in notesPlanilha)
                        {
                            bool nota_encontrada = false;

                            foreach (var nSped in notesSped)
                            {
                                if (nSped[0].Equals(nPlanilha[5]))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }

                            string cnpj_chave = nPlanilha[5].Length >= 14 ? nPlanilha[5].Substring(6, 14) : "";
                            if (nota_encontrada.Equals(false) && nPlanilha[5].Length.Equals(44))
                            {
                                notasExcel.Add(nPlanilha);
                            }

                        }
                    }
                        
                    ViewBag.NotasExcel = notasExcel;
                    ViewBag.NotasExcelCount = notasExcel.Count();
                }

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
