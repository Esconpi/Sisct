using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{

    public class CompanyController : ControllerBaseSisctNET
    {
        private readonly ICompanyService _service;
        private readonly INoteService _noteService;
        private readonly ITaxationService _taxationService;
        private readonly IProductNoteService _itemService;
        private readonly Fortes.IEnterpriseService _fortesEnterpriseService;
        private readonly IConfigurationService _configurationService;
        private readonly ITaxationTypeService _taxationTypeService;
        private readonly IAnnexService _annexService;
        private readonly ICountingTypeService _countingTypeService;
        private readonly ICfopService _cfopService;
        private readonly IChapterService _chapterService;
        private readonly ISectionService _sectionService;
        private readonly IEmailResponsibleService _emailResponsibleService;
        private readonly IStateService _stateService;
        private readonly ICountyService _countyService;

        public CompanyController(
            Fortes.IEnterpriseService fortesEnterpriseService,
            ICompanyService service,
            INoteService noteService,
            ITaxationService taxationService,
            IProductNoteService itemService,
            ITaxationTypeService taxationTypeService,
            IConfigurationService configurationService,
            IAnnexService annexService,
            ICountingTypeService countingTypeService,
            ICfopService cfopService,
            IChapterService chapterService,
            ISectionService sectionService,
            IEmailResponsibleService emailResponsibleService,
            IStateService stateService,
            ICountyService countyService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Company")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _noteService = noteService;
            _taxationService = taxationService;
            _itemService = itemService;
            _taxationTypeService = taxationTypeService;
            _fortesEnterpriseService = fortesEnterpriseService;
            _configurationService = configurationService;
            _annexService = annexService;
            _countingTypeService = countingTypeService;
            _cfopService = cfopService;
            _chapterService = chapterService;
            _sectionService = sectionService;
            _emailResponsibleService = emailResponsibleService;
            _stateService = stateService;
            _countyService = countyService;
        }

        [HttpGet]
        public IActionResult Sincronize()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var confDbFortes = _configurationService.FindByName("DataBaseFortes", null);
                var result = _service.FindAll(GetLog(Model.OccorenceLog.Read));

                var counties = _countyService.FindAll(null);
               
                var empFortes = _fortesEnterpriseService.GetCompanies(counties, confDbFortes.Value);

                List<Company> addCompany = new List<Company>();
                List<Company> updateCompany = new List<Company>();

                if (result.Count <= 0)
                {
                    result.Add(new Company() { Id = 0, Code = "0000" });
                    addCompany = empFortes;
                }

                foreach(var emp in empFortes) 
                {
                    var company = result.Where(c => c.Document.Equals(emp.Document) && c.Code.Equals(emp.Code)).FirstOrDefault();

                    if(company == null)
                    {
                        emp.Sped = true;
                        addCompany.Add(emp);
                    }
                    else
                    {
                        company.Code = emp.Code;
                        company.SocialName = emp.SocialName;
                        company.FantasyName = emp.FantasyName;
                        company.Document = emp.Document;
                        company.Ie = emp.Ie;
                        company.IM = emp.IM;
                        company.Logradouro = emp.Logradouro;
                        company.Number = emp.Number;
                        company.Complement = emp.Complement;
                        company.District = emp.District;
                        company.Cep = emp.Cep;
                        company.CountyId = emp.CountyId;
                        company.Phone = emp.Phone;
                        company.Updated = DateTime.Now;
                        updateCompany.Add(company);
                    }
                }
               
                _service.Create(addCompany, GetLog(OccorenceLog.Create));
                _service.Update(updateCompany, GetLog(OccorenceLog.Update));

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var countingType = _countingTypeService.FindAll(null);
                List<CountingType> countingTypes = new List<CountingType>();
                countingTypes.Insert(0, new CountingType() { Id = 0, Name = "Nenhum" });

                foreach (var item in countingType)
                {
                    countingTypes.Add(new CountingType() { Id = item.Id, Name = item.Name });
                }

                SelectList countingsTypes = new SelectList(countingTypes, "Id", "Name", null);
                ViewBag.ListTypes = countingsTypes;

                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var list_city = _countyService.FindAll(null).Where(_ => !_.State.UF.Equals("EXT")).OrderBy(_ => _.State.UF).ThenBy(_ => _.Name).ToList();

                foreach (var c in list_city)
                {
                    c.Name = c.Name + " - " + c.State.UF;
                }

                SelectList citys = new SelectList(list_city, "Id", "Name", null);
                ViewBag.CountyId = citys;

                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create(Company entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {

                if (entity.SocialName == null)
                    entity.SocialName = "";

                if (entity.FantasyName == null)
                    entity.FantasyName = "";

                entity.Sped = true;
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;

                _service.Create(entity, GetLog(Model.OccorenceLog.Create));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);

                var list_city = _countyService.FindAll(null).Where(_ => !_.State.UF.Equals("EXT")).OrderBy(_ => _.State.UF).ThenBy(_ => _.Name).ToList();

                foreach (var c in list_city)
                {
                    c.Name = c.Name + " - " + c.State.UF;
                }

                SelectList citys = new SelectList(list_city, "Id", "Name", null);
                ViewBag.CountyId = citys;


                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.Company entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var rst = _service.FindById(id, null);

                rst.Active = entity.Active;
                rst.Status = entity.Status;
                rst.Incentive = entity.Incentive;
                rst.SocialName = entity.SocialName;
                rst.FantasyName = entity.FantasyName;
                rst.Code = entity.Code;
                rst.Document = entity.Document;
                rst.Ie = entity.Ie;
                rst.IM = entity.IM;
                rst.Logradouro = entity.Logradouro;
                rst.Number = entity.Number;
                rst.Complement = entity.Complement;
                rst.District = entity.District;
                rst.Cep = entity.Cep;
                rst.CountyId = entity.CountyId;
                rst.Phone = entity.Phone;

                if (entity.SocialName == null)
                    rst.SocialName = "";

                if (entity.FantasyName == null)
                    rst.FantasyName = "";

                rst.Updated = DateTime.Now;


                var result = _service.Update(rst, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult EditNew(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);

                List<Annex> list_annex = _annexService.FindAll(null);
                foreach (var annex in list_annex)
                {
                    annex.Description = annex.Description + " - " + annex.Convenio;
                }
                list_annex.Insert(0, new Annex() { Description = "Nennhum anexo selecionado", Id = 0 });
                SelectList annexs = new SelectList(list_annex, "Id", "Description", null);
                ViewBag.AnnexId = annexs;

                List<Chapter> list_chapters = _chapterService.FindAll(null);
                foreach (var chapter in list_chapters)
                {
                    chapter.Name = chapter.Name + " - " + chapter.Description;
                }
                list_chapters.Insert(0, new Chapter() { Name = "Nenhuma capítulo selecionado", Id = 0 });
                SelectList chapters = new SelectList(list_chapters, "Id", "Name", null);
                ViewBag.ChapterId = chapters;

                List<Section> list_sections = _sectionService.FindAll(null);
                foreach (var section in list_sections)
                {
                    section.Name = section.Name + " - " + section.Description;
                }
                list_sections.Insert(0, new Section() { Name = "Nenhuma seção selecionada", Id = 0 });
                SelectList sections = new SelectList(list_sections, "Id", "Name", null);
                ViewBag.SectionId = sections;


                if(result.AnnexId == null)
                    result.AnnexId = 0;

                if (result.ChapterId == null)
                    result.ChapterId = 0;

                if(result.SectionId == null)
                    result.SectionId = 0;

                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult EditNew(long id, Model.Company entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var rst = _service.FindById(id, null);
                rst.TipoApuracao = entity.TipoApuracao;
                rst.TypeCompany = entity.TypeCompany;
                rst.AnnexId = entity.AnnexId.Equals(0) ? null : entity.AnnexId;
                rst.Icms = entity.Icms;
                rst.Funef = entity.Funef;
                rst.Cotac = entity.Cotac;
                rst.Transferencia = entity.Transferencia;
                rst.TransferenciaExcedente = entity.TransferenciaExcedente;
                rst.TransferenciaInter = entity.TransferenciaInter;
                rst.TransferenciaInterExcedente = entity.TransferenciaInterExcedente;
                rst.VendaContribuinte = entity.VendaContribuinte;
                rst.VendaContribuinteExcedente = entity.VendaContribuinteExcedente;
                rst.VendaCpf = entity.VendaCpf;
                rst.VendaCpfExcedente = entity.VendaCpfExcedente;
                rst.VendaMGrupo = entity.VendaMGrupo;
                rst.VendaMGrupoExcedente = entity.VendaMGrupoExcedente;
                rst.VendaAnexo = entity.VendaAnexo;
                rst.VendaAnexoExcedente = entity.VendaAnexoExcedente;
                rst.Fecop = entity.Fecop;
                rst.Suspension = entity.Suspension;
                rst.IcmsNContribuinte = entity.IcmsNContribuinte;
                rst.IcmsNContribuinteFora = entity.IcmsNContribuinteFora;
                rst.IcmsAliqM25 = entity.IcmsAliqM25;
                rst.ChapterId = entity.ChapterId.Equals(0) ? null : entity.ChapterId;
                rst.SectionId = entity.SectionId.Equals(0) ? null : entity.SectionId;
                rst.AliqInterna = entity.AliqInterna;
                rst.IncIInterna = entity.IncIInterna;
                rst.IncIInterestadual = entity.IncIInterestadual;
                rst.IncIIInterna = entity.IncIIInterna;
                rst.IncIIInterestadual = entity.IncIIInterestadual;
                rst.VendaArt781 = entity.VendaArt781;
                rst.VendaArt781Excedente = entity.VendaArt781Excedente;
                rst.Faturamento = entity.Faturamento;
                rst.FaturamentoExcedente = entity.FaturamentoExcedente;
                rst.Updated = DateTime.Now;
                _service.Update(rst, GetLog(Model.OccorenceLog.Update));

                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Delete(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                _service.Delete(id, GetLog(Model.OccorenceLog.Delete));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateActive([FromBody] Model.UpdateActive updateActive)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var entity = _service.FindById(updateActive.Id, null);
                entity.Active = updateActive.Active;

                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return Ok(new { requestcode = 200, message = "ok" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { requestcode = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateIncentive([FromBody] Model.UpdateIncentive updateIncentive)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var entity = _service.FindById(updateIncentive.Id, null);
                entity.Incentive = updateIncentive.Incentive;

                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return Ok(new { requestcode = 200, message = "ok" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { requestcode = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateStatus([FromBody] Model.UpdateStatus updateStatus)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var entity = _service.FindById(updateStatus.Id, null);
                entity.Status = updateStatus.Status;

                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return Ok(new { requestcode = 200, message = "ok" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { requestcode = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Compare(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Listing(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Tax(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpPost]
        public IActionResult Tax(long id, Model.Company entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);
                var companies = _service.FindByCompanies(result.Document);

                List<Model.Company> comps = new List<Company>();

                foreach (var c in companies)
                {


                    //  Empresa Lucro Real
                    c.PercentualCofinsRF = entity.PercentualCofinsRF;
                    c.PercentualCofinsCredito = entity.PercentualCofinsCredito;
                    c.PercentualPisRF = entity.PercentualPisRF;
                    c.PercentualPisCredito = entity.PercentualPisCredito;


                    //  Empresa Lucro Presumido
                    c.IRPJ1 = entity.IRPJ1;
                    c.IRPJ2 = entity.IRPJ2;
                    c.IRPJ3 = entity.IRPJ3;
                    c.IRPJ4 = entity.IRPJ4;
                    c.CSLL1 = entity.CSLL1;
                    c.CSLL2 = entity.CSLL2;
                    c.CPRB = entity.CPRB;
                    c.StatusCPRB = entity.StatusCPRB;
                    c.PercentualIRPJ = entity.PercentualIRPJ;
                    c.PercentualCSLL = entity.PercentualCSLL;
                    c.AdicionalIRPJ = entity.AdicionalIRPJ;


                    //  Empresas
                    c.PercentualCofins = entity.PercentualCofins;
                    c.PercentualPis = entity.PercentualPis;
                    c.Sped = entity.Sped;
                    c.Taxation = entity.Taxation;
                    c.Updated = DateTime.Now;

                    comps.Add(c);
                }
                
                _service.Update(comps);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
       
        [HttpPost]
        public IActionResult UpdateCountingType([FromBody] Model.UpdateCountingType updateCountingType)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var entity = _service.FindById(updateCountingType.CompanyId, GetLog(Model.OccorenceLog.Read));

                if (updateCountingType.CountingTypeId.Equals(0))
                {
                    entity.CountingTypeId = null;
                }
                else
                {
                    entity.CountingTypeId = updateCountingType.CountingTypeId;
                }

                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return Ok(new { requestcode = 200, message = "ok" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetResponsibleByCompanyId(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            var draw = Request.Query["draw"].ToString();

            var result = await _emailResponsibleService.GetByCompanyAsync(id);
            return Ok(new { draw = Convert.ToInt32(draw), recordsTotal = result.Count(), recordsFiltered = result.Count(), data = result });
        }

        [HttpPost]
        public IActionResult PostResponsibleByCompanyId([FromBody] EmailResponsible responsible)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            responsible.Created = DateTime.Now;
            responsible.Updated = DateTime.Now;
            _emailResponsibleService.Create(responsible, GetLog(OccorenceLog.Create));
            return Ok(new { code = "200", message = "ok" });
        }

        [HttpDelete]
        public IActionResult DeleteResponsibleByCompanyId(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            _emailResponsibleService.Delete(id, GetLog(OccorenceLog.Delete));
            return Ok(new { code = "200", message = "ok" });
        }

        public IActionResult GetAllActive(int draw, int start)
        {


            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var companies = _service.FindByCompanies();


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Company> company = new List<Company>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                var ids = companies.Where(c =>
                    c.FantasyName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Document.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.SocialName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                company = companies.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var empresa = from r in company
                           where ids.ToArray().Contains(r.Id)
                           select new
                           {
                               Id = r.Id.ToString(),
                               Code = r.Code,
                               SocialName = r.SocialName,
                               FantasyName = r.FantasyName,
                               Document = r.Document,
                               Status = r.Status,
                               Incentivo = r.Incentive,
                               Anexo = r.AnnexId
                           };

                return Ok(new { draw = draw, recordsTotal = company.Count(), recordsFiltered = company.Count(), data = empresa.Skip(start).Take(lenght) });

            }
            else
            {


                var empresa = from r in companies
                           select new
                           {
                               Id = r.Id.ToString(),
                               Code = r.Code,
                               SocialName = r.SocialName,
                               FantasyName = r.FantasyName,
                               Document = r.Document,
                               Status = r.Status,
                               Incentivo = r.Incentive,
                               Anexo = r.AnnexId
                           };
                return Ok(new { draw = draw, recordsTotal = companies.Count(), recordsFiltered = companies.Count(), data = empresa.Skip(start).Take(lenght) });
            }

        }

        public IActionResult GetAll(int draw, int start)
        {


            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var companies = _service.FindAll(null).OrderBy(_ => _.Document).ToList();


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Company> company = new List<Company>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                var ids = companies.Where(c =>
                    c.FantasyName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Document.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.SocialName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                company = companies.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var empresa = from r in company
                              where ids.ToArray().Contains(r.Id)
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  Code = r.Code,
                                  SocialName = r.SocialName,
                                  FantasyName = r.FantasyName,
                                  Document = r.Document,
                                  Active = r.Active,
                                  Status = r.Status,
                                  Incentivo = r.Incentive,
                                  CountingTypeId = r.CountingTypeId
                              };

                return Ok(new { draw = draw, recordsTotal = company.Count(), recordsFiltered = company.Count(), data = empresa.Skip(start).Take(lenght) });

            }
            else
            {


                var empresa = from r in companies
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  Code = r.Code,
                                  SocialName = r.SocialName,
                                  FantasyName = r.FantasyName,
                                  Document = r.Document,
                                  Active = r.Active,
                                  Status = r.Status,
                                  Incentivo = r.Incentive,
                                  CountingTypeId = r.CountingTypeId
                              };
                return Ok(new { draw = draw, recordsTotal = companies.Count(), recordsFiltered = companies.Count(), data = empresa.Skip(start).Take(lenght) });
            }

        }

    }
}