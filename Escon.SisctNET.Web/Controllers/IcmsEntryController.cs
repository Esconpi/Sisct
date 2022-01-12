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
    public class IcmsEntryController : ControllerBaseSisctNET
    {
        private readonly ICompanyService _companyService;
        private readonly ICfopService _cfopService;
        private readonly IHostingEnvironment _appEnvironment;

        public IcmsEntryController(
            ICompanyService companyService,
            ICfopService cfopService,
            IHostingEnvironment env,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Company")
        {
            _companyService = companyService;
            _cfopService = cfopService;
            _appEnvironment = env;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public async Task<IActionResult> Relatory(long id, string year, string month, string type, IFormFile arquivo)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var company = _companyService.FindById(id, null);

                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);

                ViewBag.Company = company;
                ViewBag.Type = type;

                var importSped = new Sped.Import();

                string caminhoDestinoArquivoOriginal = "";
                string caminho_WebRoot = _appEnvironment.WebRootPath;

                if (arquivo != null)
                {
                    string filedirSped = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Speds");

                    if (!Directory.Exists(filedirSped))
                        Directory.CreateDirectory(filedirSped);

                    string nomeArquivo = company.Document;

                    if (arquivo.FileName.Contains(".txt"))
                        nomeArquivo += ".txt";
                    else
                        nomeArquivo += ".tmp";

                    string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Speds\\";
                    caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                    string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivo);

                    if (System.IO.File.Exists(caminhoDestinoArquivoOriginal))
                        System.IO.File.Delete(caminhoDestinoArquivoOriginal);

                    var streamSped = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);
                    await arquivo.CopyToAsync(streamSped);
                    streamSped.Close();
                }

                var cfopsAll = _cfopService.FindAll(null);

                if (type.Equals("resumoCfop"))
                {
                    //  Resumo CFOP

                    List<List<string>> cfops = new List<List<string>>();

                    var notes = importSped.NFeC190(caminhoDestinoArquivoOriginal);

                    foreach (var note in notes)
                    {
                        int pos = -1;

                        for (int e = 0; e < cfops.Count(); e++)
                        {
                            if (cfops[e][0].Equals(note[3]))
                            {
                                pos = e;
                                break;
                            }
                        }

                        if (pos < 0)
                        {
                            var cfp = cfopsAll.Where(_ => _.Code.Equals(note[3])).FirstOrDefault();
                            List<string> cc = new List<string>();
                            cc.Add(note[3]);
                            cc.Add(cfp.Description);
                            cc.Add("0");
                            cc.Add("0");
                            cc.Add("0");
                            cfops.Add(cc);
                            pos = cfops.Count() - 1;
                        }

                        decimal vProd = 0, vBase = 0, vIcms = 0;

                        if (note[5] != "")
                            vProd = Convert.ToDecimal(note[5]);

                        if (note[6] != "")
                            vBase = Convert.ToDecimal(note[6]);

                        if (note[7] != "")
                            vIcms = Convert.ToDecimal(note[7]);

                        cfops[pos][2] = (Convert.ToDecimal(cfops[pos][2]) + vProd).ToString();
                        cfops[pos][3] = (Convert.ToDecimal(cfops[pos][3]) + vBase).ToString();
                        cfops[pos][4] = (Convert.ToDecimal(cfops[pos][4]) + vIcms).ToString();
                    }

                    ViewBag.Cfop = cfops.OrderBy(_ => Convert.ToInt32(_[0])).ToList();
                }

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
