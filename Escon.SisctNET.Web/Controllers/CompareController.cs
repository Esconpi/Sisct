using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Compare;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                    var opcao = Request.Form["opcao"];
                    var nome_social = Request.Form["socialname"];
                    var year = Request.Form["year"];
                    var id = Request.Form["id"];
                    var month = Request.Form["month"];
                    ViewBag.opcao = opcao;

                    if (opcao.Equals("1"))
                    {
                        var confDBSisctNfe = new Model.Configuration();
                        var import = new Import();
                        var ordem = Request.Form["ordem"];
                        var ident = Request.Form["ident"];

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
                        var company = _companyService.FindById(Convert.ToInt32(id), null);
                        string directoryNfe = confDBSisctNfe.Value + "\\" + company.Document + "\\" + year + "\\" + month;

                        notes = import.Nfe(directoryNfe);

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
                        if (ordem.Equals("difereValor"))
                        {
                            SpedDif = import.SpedDif(caminhoDestinoArquivoOriginal);
                        }
                        else
                        {
                            if (ident.Equals("0"))
                            {
                                sped = import.SpedNfe(caminhoDestinoArquivoOriginal);
                            }
                            else if (ident.Equals("1"))
                            {
                                sped = import.SpedNfeSaida(caminhoDestinoArquivoOriginal);
                            }
                        }
                        List<List<Dictionary<string, string>>> notas = new List<List<Dictionary<string, string>>>();
                        List<List<string>> notas_sped = new List<List<string>>();
                        if (ordem.Equals("xml"))
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
                            
                            ViewBag.ordem = "1";
                        }
                        else if (ordem.Equals("sped"))
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
                                if (nota_encontrada.Equals(false) && !cnpj_chave.Equals(company.Document))
                                {
                                    notas_sped.Add(note);
                                }
                            }

                            ViewBag.ordem = "2";
                        }
                        else if (ordem.Equals("difereValor"))
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
                                        string totalXml = notaXml[4]["vNF"];
                                        string totalSped = linha[2].Equals("") ? "0" : linha[2].Replace(",", ".");
                                        string totalDif = (Math.Abs(Convert.ToDecimal(totalXml) - Convert.ToDecimal(totalSped))).ToString();
                                        string descXml = notaXml[4]["vDesc"];
                                        string descSped = linha[3].Equals("") ? "0" : linha[3].Replace(",", ".");
                                        string descDif = (Math.Abs(Convert.ToDecimal(descXml) - Convert.ToDecimal(descSped))).ToString();
                                        string outDespXml = notaXml[4]["vOutro"];
                                        string outDespSped = linha[6].Equals("") ? "0" : linha[6].Replace(",", ".");
                                        string outDespDif = (Math.Abs(Convert.ToDecimal(outDespXml) - Convert.ToDecimal(outDespSped))).ToString();
                                        string segXml = notaXml[4]["vSeg"];
                                        string segSped = linha[5].Equals("") ? "0" : linha[5].Replace(",", ".");
                                        string segDif = (Math.Abs(Convert.ToDecimal(segXml) - Convert.ToDecimal(segSped))).ToString();
                                        string freteXml = notaXml[4]["vFrete"];
                                        string freteSped = linha[4].Equals("") ? "0" : linha[4].Replace(",", ".");
                                        string freteDif = (Math.Abs(Convert.ToDecimal(freteXml) - Convert.ToDecimal(freteSped))).ToString();

                                        if ((!totalDif.Equals("0.00") && !totalDif.Equals("0")) || (!descDif.Equals("0.00") && !descDif.Equals("0")) ||
                                            (!outDespDif.Equals("0.00") && !outDespDif.Equals("0")) || (!segDif.Equals("0.00") &&
                                            !segDif.Equals("0")) || (!freteDif.Equals("0.00") && !freteDif.Equals("0")))
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
                            ViewBag.ordem = "3";
                        }

                        ViewBag.Notas = notas;
                        ViewBag.notas_sped = notas_sped;
                        ViewBag.Document = company.Document;
                        ViewBag.SocialName = company.SocialName;
                        ViewBag.Ano = year;
                        ViewBag.Mes = month;

                    }
                    else if (opcao.Equals("2"))
                    {
                        var confDBSisctCte = _configurationService.FindByName("CTe");
                        var import = new Import();
                        var company = _companyService.FindById(Convert.ToInt32(id), null);
                        string directoryCte = confDBSisctCte.Value + "\\" + company.Document + "\\" + year + "\\" + month;

                        List<List<Dictionary<string, string>>> ctes = new List<List<Dictionary<string, string>>>();
                        List<string> sped = new List<string>();


                        ctes = import.Cte(directoryCte);

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

                        sped = import.SpedCte(caminhoDestinoArquivoOriginal);

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
                        ViewBag.Document = company.Document;
                        ViewBag.SocialName = company.SocialName;
                        ViewBag.Ano = year;
                        ViewBag.Mes = month;
                        
                    }
                    return View();
                }
               
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
            
        }
    }
}
