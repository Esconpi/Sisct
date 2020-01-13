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
    public class CompareNFeController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly IHostingEnvironment _appEnvironment;

        public CompareNFeController(
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
                var confDBSisctNfe = _configurationService.FindByName("NFe Entrada");
                var import = new Import();

                var nome_social = Request.Form["socialname"];
                var year = Request.Form["year"];
                var id = Request.Form["id"];
                var month = Request.Form["month"];
                var ordem = Request.Form["ordem"];
                var company = _companyService.FindById(Convert.ToInt32(id) , null);
                string directoryNfe = confDBSisctNfe.Value + "\\" + company.Document + "\\" + year + "\\" + month;

                List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
                List<string> sped = new List<string>();
                List<List<string>> SpedDif = new List<List<string>>();

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
                    sped = import.SpedNfe(caminhoDestinoArquivoOriginal);
                }
                List<List<Dictionary<string, string>>> notas = new List<List<Dictionary<string, string>>>();
                List<string> notas_sped = new List<string>();
                if (ordem.Equals("xml"))
                {
                    foreach (var note in notes)
                    {
                        for (int i = 0; i < note.Count; i++)
                        {
                            if (note[i].ContainsKey("CNPJ"))
                            {
                                if (note[i]["CNPJ"] != company.Document)
                                {
                                    for (int j = 0; j < note.Count; j++)
                                    {
                                        if (note[j].ContainsKey("chave"))
                                        {
                                            string nota_xml = note[j]["chave"];

                                            bool nota_encontrada = false;

                                            for (int k = 0; k < sped.Count(); k++)
                                            {
                                                if (nota_xml == sped[k])
                                                {
                                                    nota_encontrada = true;
                                                    break;
                                                }
                                            }
                                            if (nota_encontrada == false)
                                            {
                                                notas.Add(note);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    ViewBag.ordem = "1";
                }
                else if (ordem.Equals("sped"))
                {
                    foreach(var note in sped)
                    {
                        bool nota_encontrada = false;
                        foreach (var notaXml in notes)
                        {
                            string nota_xml = notaXml[0]["chave"];
                            if (note.Equals(nota_xml))
                            {
                                nota_encontrada = true;
                                break;
                            }
                        }
                        if (nota_encontrada.Equals(false))
                        {
                            notas_sped.Add(note);
                        }
                    }

                    ViewBag.ordem = "2";
                }
                else if (ordem.Equals("difereValor"))
                {
                    List<string> ValoresRelatorio = new List<string>();
                    foreach(var linha in SpedDif)
                    {
                        foreach(var notaXml in notes)
                        {
                            string nota_xml = notaXml[0]["chave"];
                            if (linha[1].Equals(nota_xml))
                            {
                                string fornecedor = notaXml[2]["xFant"];
                                string totalXml = notaXml[4]["vNF"];
                                string totalSped = linha[2].Equals("") ? "0" : linha[2];
                                string totalDif = (Math.Abs(Convert.ToDecimal(totalXml) - Convert.ToDecimal(totalSped))).ToString();
                                string descXml = notaXml[4]["VDesc"];
                                string descSped = linha[3].Equals("") ? "0" : linha[3];
                                string descDif = (Math.Abs(Convert.ToDecimal(descXml) - Convert.ToDecimal(descSped))).ToString();
                                string outDespXml = notaXml[4]["vOutro"];
                                string outDespSped = linha[6].Equals("") ? "0" : linha[6];
                                string outDespDif = (Math.Abs(Convert.ToDecimal(outDespXml) - Convert.ToDecimal(outDespSped))).ToString();
                                string segXml = notaXml[4]["vSeg"];
                                string segSped = linha[5].Equals("") ? "0" : linha[5];
                                string segDif = (Math.Abs(Convert.ToDecimal(segXml) - Convert.ToDecimal(segSped))).ToString();
                                string freteXml = notaXml[4]["vFrete"];
                                string freteSped = linha[4].Equals("") ? "0" : linha[4];
                                string freteDif = (Math.Abs(Convert.ToDecimal(freteXml) - Convert.ToDecimal(freteSped))).ToString();

                                if (!Convert.ToDecimal(totalDif).Equals(0) && !Convert.ToDecimal(descDif).Equals(0) && 
                                    !Convert.ToDecimal(outDespDif).Equals(0) && !Convert.ToDecimal(segDif).Equals(0) &&
                                    !Convert.ToDecimal(freteDif).Equals(0))
                                {
                                    ValoresRelatorio.Add(nota_xml);
                                    ValoresRelatorio.Add(fornecedor);
                                    ValoresRelatorio.Add(totalSped);
                                    ValoresRelatorio.Add(totalXml);
                                    ValoresRelatorio.Add(totalDif);
                                    ValoresRelatorio.Add(descSped);
                                    ValoresRelatorio.Add(descXml);
                                    ValoresRelatorio.Add(descDif);
                                    ValoresRelatorio.Add(outDespSped);
                                    ValoresRelatorio.Add(outDespXml);
                                    ValoresRelatorio.Add(outDespDif);
                                    ValoresRelatorio.Add(freteSped);
                                    ValoresRelatorio.Add(freteXml);
                                    ValoresRelatorio.Add(freteDif);
                                    ValoresRelatorio.Add(segSped);
                                    ValoresRelatorio.Add(segXml);
                                    ValoresRelatorio.Add(segDif);
                                }
                            }
                        }
                    }

                    ViewBag.ordem = "3";
                }

                ViewBag.Notas = notas;
                ViewBag.notas_sped = notas_sped;
                ViewBag.Document = company.Document;
                ViewBag.SocialName = company.SocialName;
                ViewBag.Ano = year;
                ViewBag.Mes = month;

                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
            
        }
    }
}
