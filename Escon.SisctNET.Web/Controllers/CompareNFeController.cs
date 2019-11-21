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
                var confDBSisctNfe = _configurationService.FindByName("NFe");
                var import = new Import();

                var nome_social = Request.Form["socialname"];
                var year = Request.Form["year"];
                var id = Request.Form["id"];
                var month = Request.Form["month"];
                var company = _companyService.FindById(Convert.ToInt32(id) , null);
                string directoryNfe = confDBSisctNfe.Value + "\\" + company.Document + "\\" + year + "\\" + month;

                List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
                List<string> sped = new List<string>();


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
                sped = import.SpedNfe(caminhoDestinoArquivoOriginal);

                List<List<Dictionary<string, string>>> notas = new List<List<Dictionary<string, string>>>();

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
                
                ViewBag.Notas = notas;
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
