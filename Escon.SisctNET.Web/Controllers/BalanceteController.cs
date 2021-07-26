using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{
    public class BalanceteController : ControllerBaseSisctNET
    {
        private readonly ICompanyService _companyService;
        private readonly Fortes.ISDOService _sDOService;
        private readonly IConfigurationService _configurationService;
        private readonly IAccountPlanService _accountPlanService;
        private readonly IHostingEnvironment _appEnvironment;

        public BalanceteController(
            ICompanyService companyService,
            Fortes.ISDOService sDOService,
            IConfigurationService configurationService,
            IAccountPlanService accountPlanService,
            IHostingEnvironment env,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Company")
        {
            _companyService = companyService;
            _sDOService = sDOService;
            _configurationService = configurationService;
            _accountPlanService = accountPlanService;
            _appEnvironment = env;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public async Task<IActionResult> Relatory(long companyId, DateTime inicio, DateTime fim, IFormFile arquivoExcel)
        {

            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _companyService.FindById(companyId, null);
                var accs = _accountPlanService.FindByCompanyActive(comp.Code);
                //var confDbFortes = _configurationService.FindByName("DataBaseFortes", null);

                var importExcel = new Planilha.Import();

                if (arquivoExcel == null || arquivoExcel.Length == 0)
                {
                    ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                    return View(ViewData);
                }

                string filedirExcel = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Planilha");

                if (!Directory.Exists(filedirExcel))
                    Directory.CreateDirectory(filedirExcel);

                string nomeArquivoExcel = comp.Document + "Sefaz";

                if (arquivoExcel.FileName.Contains(".xls") || arquivoExcel.FileName.Contains(".xlsx"))
                    nomeArquivoExcel += ".xls";

                string caminho_WebRoot = _appEnvironment.WebRootPath;
                string caminhoDestinoArquivoExcel = caminho_WebRoot + "\\Uploads\\Planilha\\";
                string caminhoDestinoArquivoOriginalExcel = caminhoDestinoArquivoExcel + nomeArquivoExcel;

                string[] paths_upload_excel = Directory.GetFiles(caminhoDestinoArquivoExcel);

                if (System.IO.File.Exists(caminhoDestinoArquivoOriginalExcel))
                    System.IO.File.Delete(caminhoDestinoArquivoOriginalExcel);

                var streamExcel = new FileStream(caminhoDestinoArquivoOriginalExcel, FileMode.Create);
                await arquivoExcel.CopyToAsync(streamExcel);
                streamExcel.Close();

                var balancete = importExcel.Balancete(caminhoDestinoArquivoOriginalExcel, accs);

                ViewBag.Company = comp;
                ViewBag.Inicio = inicio;
                ViewBag.Fim = fim;
                ViewBag.DisponibilidadeFinanceira = balancete[0];
                ViewBag.DespesasOperacionais = balancete[1];
                ViewBag.EstoqueMercadoria = balancete[2];

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
