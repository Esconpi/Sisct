using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Compare;
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
    public class CompareCTeController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly IHostingEnvironment _appEnvironment;

        public CompareCTeController(
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
                var confDBSisctCte = _configurationService.FindByName("CTe");
                var import = new Import();

                var nome_social = Request.Form["socialname"];
                var year = Request.Form["year"];
                var id = Request.Form["id"];
                var month = Request.Form["month"];
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
                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }
    }
}