using Escon.SisctNET.Model;
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
    public class NoteInventoryEntryController : ControllerBaseSisctNET
    {
        private readonly IProductNoteInventoryEntryService _itemService;
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly ICountyService _countyService;
        private readonly IHostingEnvironment _appEnvironment;

        public NoteInventoryEntryController(
            IProductNoteInventoryEntryService itemService,
            ICompanyService companyService,
            IConfigurationService configurationService,
            ICountyService countyService,
            IHostingEnvironment env,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Note")
        {
            _itemService = itemService;
            _companyService = companyService;
            _configurationService = configurationService;
            _countyService = countyService;
            _appEnvironment = env;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(int id, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);

                ViewBag.Company = comp;

                SessionManager.SetCompanyIdInSession(id);
                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);

                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Import()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(SessionManager.GetCompanyIdInSession(), null);
                return PartialView(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile arquivo)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Note")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                int id = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var company = _companyService.FindById(id, null);
                var municipios = _countyService.FindAll(null);

                var importSped = new Sped.Import();

                string filedirSped = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Speds");

                if (!Directory.Exists(filedirSped))
                    Directory.CreateDirectory(filedirSped);

                string nomeArquivoSped = company.Document + "Empresa";

                if (arquivo.FileName.Contains(".txt"))
                    nomeArquivoSped += ".txt";
                else
                    nomeArquivoSped += ".tmp";

                string caminho_WebRoot = _appEnvironment.WebRootPath;

                string caminhoDestinoArquivoSped = caminho_WebRoot + "\\Uploads\\Speds\\";
                string caminhoDestinoArquivoOriginalSped = caminhoDestinoArquivoSped + nomeArquivoSped;

                string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivoSped);

                if (System.IO.File.Exists(caminhoDestinoArquivoOriginalSped))
                    System.IO.File.Delete(caminhoDestinoArquivoOriginalSped);

                var streamSped = new FileStream(caminhoDestinoArquivoOriginalSped, FileMode.Create);
                await arquivo.CopyToAsync(streamSped);
                streamSped.Close();

                var produtosImportados = _itemService.FindByCompany(id);

                List<Model.ProductNoteInventoryEntry> addProduct = new List<Model.ProductNoteInventoryEntry>();

                var produtos = importSped.NFeProduct(caminhoDestinoArquivoOriginalSped, municipios);

                foreach(var produto in produtos)
                {

                }

                _itemService.Create(addProduct, GetLog(OccorenceLog.Create));

                return RedirectToAction("Index", new { id = id, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult GetAll(int draw, int start)
        {

            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var notasAll = _itemService.FindByNotes(SessionManager.GetCompanyIdInSession(), SessionManager.GetYearInSession(), SessionManager.GetMonthInSession())
                        .OrderBy(_ => Convert.ToInt64(_.Nnf)).ToList();


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<ProductNoteInventoryEntry> notes = new List<ProductNoteInventoryEntry>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<ProductNoteInventoryEntry> notesTemp = new List<ProductNoteInventoryEntry>();
                notasAll.ToList().ForEach(s =>
                {
                    s.Xnome = Helpers.CharacterEspecials.RemoveDiacritics(s.Xnome);
                    notesTemp.Add(s);
                });

                var ids = notesTemp.Where(c =>
                    c.Xnome.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Nnf.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                notes = notasAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var note = from r in notes
                              where ids.ToArray().Contains(r.Id)
                              select new
                              {
                                  Id = r.Chave.ToString(),
                                  Nota = r.Nnf,
                                  Fornecedor = r.Xnome,
                                  Valor = r.Vnf
                              };

                return Ok(new { draw = draw, recordsTotal = notes.Count(), recordsFiltered = notes.Count(), data = note.Skip(start).Take(lenght) });

            }
            else
            {

                var notes = from r in notasAll
                              select new
                              {
                                  Id = r.Chave.ToString(),
                                  Nota = r.Nnf,
                                  Fornecedor = r.Xnome,
                                  Valor = r.Vnf

                              };
                return Ok(new { draw = draw, recordsTotal = notes.Count(), recordsFiltered = notes.Count(), data = notes.Skip(start).Take(lenght) });
            }

        }

    }
}
