using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class EstoqueController : ControllerBaseSisctNET
    {
        private readonly IEstoqueService _service;
        private readonly ICompanyService _companyService;
        private readonly IProductNoteInventoryEntryService _productNoteInventoryEntryService;
        private readonly IProductNoteInventoryExitService _productNoteInventoryExitService;

        public EstoqueController(
            IEstoqueService service,
            ICompanyService companyService,
            IProductNoteInventoryEntryService productNoteInventoryEntryService,
            IProductNoteInventoryExitService productNoteInventoryExitService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Company")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _companyService = companyService;
            _productNoteInventoryEntryService = productNoteInventoryEntryService;
            _productNoteInventoryExitService = productNoteInventoryExitService;
        }

        public IActionResult Index(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                SessionManager.SetCompanyIdInSession(id);
                var company = _companyService.FindById(id, null);
                ViewBag.Company = company;
                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Relatory(int id, string year, string inicio, string fim, string type)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Company = comp;
                ViewBag.Inicio = inicio;
                ViewBag.Fim = fim;
                ViewBag.Tipo = type;

                SessionManager.SetYearInSession(year);

                var importPeriod = new Period.Month();

                var entradas = _productNoteInventoryEntryService.FindByNotes(id, year);
                var saidas = _productNoteInventoryExitService.FindByNotes(id, year);

                var meses = importPeriod.Months(inicio, fim);

                foreach(var mes in meses)
                {
                    for(int i = 1; i <= 31; i++)
                    {

                    }
                }

                return View();
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

            var estoqueAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession(), null).OrderByDescending(_ => _.Id).ToList();


            var estoque = from r in estoqueAll
                             select new
                             {
                                 Id = r.Id.ToString(),
                                 Quantity = r.Quantity,
                                 Value = r.Value,
                                 Total = r.Total

                             };
            return Ok(new { draw = draw, recordsTotal = estoqueAll.Count(), recordsFiltered = estoqueAll.Count(), data = estoque.Skip(start).Take(lenght) });

        }
    }
}
