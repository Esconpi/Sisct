using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

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
        }

        [HttpGet]
        public IActionResult Sincronize()
        {
            if (!SessionManager.GetCompanyInSession().Equals(11))
            {
                return Unauthorized();
            }

            try
            {
                var confDbFortes = _configurationService.FindByName("DataBaseFortes", GetLog(Model.OccorenceLog.Read));
                var result = _service.FindAll(GetLog(Model.OccorenceLog.Read));
                if(result.Count <= 0)
                {
                    result.Add(new Company() { Id = 0, Code = "0000" });
                }
                var lastCode = result.Max(m => Convert.ToInt32(m.Code));

                var empFortes = _fortesEnterpriseService.GetCompanies(lastCode, confDbFortes.Value);
               
                _service.Create(empFortes, GetLog(OccorenceLog.Create));


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
            if (!SessionManager.GetCompanyInSession().Equals(11))
            {
                return Unauthorized();
            }
            try
            {
                
                var login = SessionManager.GetLoginInSession();
                if (login == null)
                {
                    
                    return RedirectToAction("Index", "Authentication");
                }
                else
                {
                    var countingType = _countingTypeService.FindAll(GetLog(Model.OccorenceLog.Read));
                    List<CountingType> countingTypes = new List<CountingType>();
                    countingTypes.Insert(0, new CountingType() { Id = 0, Name = "Nenhum" });

                    foreach (var item in countingType)
                    {
                        countingTypes.Add(new CountingType() { Id = item.Id, Name = item.Name });
                    }

                    SelectList countingsTypes = new SelectList(countingTypes, "Id", "Name", null);
                    ViewBag.ListTypes = countingsTypes;

                    var result = _service.FindAll(GetLog(Model.OccorenceLog.Read));
                    return View(null);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!SessionManager.GetCompanyInSession().Equals(11))
            {
                return Unauthorized();
            }

            try
            {
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
            if (!SessionManager.GetCompanyInSession().Equals(11))
            {
                return Unauthorized();
            }

            try
            {
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created; 

                var result = _service.Create(entity, GetLog(Model.OccorenceLog.Create));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!SessionManager.GetCompanyInSession().Equals(11))
            {
                return Unauthorized();
            }

            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(int id, Model.Company entity)
        {
            if (!SessionManager.GetCompanyInSession().Equals(11))
            {
                return Unauthorized();
            }

            try
            {
                var rst = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                rst.Updated = DateTime.Now;
                rst.Active = entity.Active;
                rst.Status = entity.Status;
                rst.Incentive = entity.Incentive;
                rst.SocialName = entity.SocialName;
                rst.FantasyName = entity.FantasyName;
                rst.Code = entity.Code;
                rst.Document = entity.Document;
                rst.Ie = entity.Ie;
                rst.Uf = entity.Uf;

                var result = _service.Update(rst, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult EditNew(int id)
        {
            if (!SessionManager.GetCompanyInSession().Equals(11))
            {
                return Unauthorized();
            }

            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                ViewBag.Id = result.Id;

                List<Annex> list_annex = _annexService.FindAll(GetLog(Model.OccorenceLog.Read));
                list_annex.Insert(0, new Annex() { Description = "Nennhum anexo selecionado", Id = 0 });
                SelectList annexs = new SelectList(list_annex, "Id", "Description", null);
                ViewBag.AnnexId = annexs;

                if(result.AnnexId == null)
                {
                    result.AnnexId = 0;
                }
                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult EditNew(int id, Model.Company entity)
        {
            if (!SessionManager.GetCompanyInSession().Equals(11))
            {
                return Unauthorized();
            }

            try
            {
                var rst = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
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

                _service.Update(rst, GetLog(Model.OccorenceLog.Update));

                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Details(int id )
        {
            if (!SessionManager.GetCompanyInSession().Equals(11))
            {
                return Unauthorized();
            }

            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                ViewBag.Id = result.Id;
                ViewBag.Type = SessionManager.GetTipoInSession();
                if(result.Incentive == false)
                {
                    if (SessionManager.GetTipoInSession() == 0)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Index", "HomeExit");
                    }
                    
                }
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!SessionManager.GetCompanyInSession().Equals(11))
            {
                return Unauthorized();
            }

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
            if (!SessionManager.GetCompanyInSession().Equals(11))
            {
                return Unauthorized();
            }

            try
            {
                var entity = _service.FindById(updateActive.Id, GetLog(Model.OccorenceLog.Read));
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
            if (!SessionManager.GetCompanyInSession().Equals(11))
            {
                return Unauthorized();
            }

            try
            {
                var entity = _service.FindById(updateIncentive.Id, GetLog(Model.OccorenceLog.Read));
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
            if (!SessionManager.GetCompanyInSession().Equals(11))
            {
                return Unauthorized();
            }

            try
            {
                var entity = _service.FindById(updateStatus.Id, GetLog(Model.OccorenceLog.Read));
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
        public IActionResult Compare(int id)
        {
            try
            {
                ViewBag.Ident = SessionManager.GetTipoInSession();
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Taxation(int id)
        {
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
            
        }

        [HttpGet]
        public IActionResult TaxationNcm(int id)
        {
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Relatory(int id)
        {
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
            
        }
        [HttpGet]
        public IActionResult RelatoryExit(int id)
        {
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                List<Cfop> list_cfop = _cfopService.FindAll(null);
                foreach (var cfop in list_cfop)
                {
                    cfop.Description = cfop.Code +" - " + cfop.Description;
                }
                list_cfop.Insert(0, new Cfop() { Description = "Nennhum item selecionado", Id = 0 });
                SelectList cfops = new SelectList(list_cfop, "Id", "Description", null);
                ViewBag.CfopId = cfops;
                ViewBag.TypeCompany = result.TypeCompany;
                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Ncm(int id)
        {
            try
            {
                var result = _taxationService.FindByCompany(id);
                var company = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                ViewBag.Company = company.FantasyName;
                ViewBag.Document = company.Document;
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult TaxationProduct(int id)
        {
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpPost]
        public IActionResult UpdateCountingType([FromBody] Model.UpdateCountingType updateCountingType)
        {

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

        public IActionResult GetAllActive(int draw, int start)
        {


            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var companies = _service.FindByCompanies();


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Company> company = new List<Company>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Company> companyTemp = new List<Company>();
                companies.ToList().ForEach(s =>
                {
                    s.SocialName = Helpers.CharacterEspecials.RemoveDiacritics(s.SocialName);
                    s.FantasyName = Helpers.CharacterEspecials.RemoveDiacritics(s.FantasyName);
                    companyTemp.Add(s);
                });

                var ids = companyTemp.Where(c =>
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
                               Incentivo = r.Incentive
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
                               Incentivo = r.Incentive
                           };
                return Ok(new { draw = draw, recordsTotal = companies.Count(), recordsFiltered = companies.Count(), data = empresa.Skip(start).Take(lenght) });
            }

        }

        public IActionResult GetAll(int draw, int start)
        {


            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var companies = _service.FindAll(null); ;


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Company> company = new List<Company>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Company> companyTemp = new List<Company>();
                companies.ToList().ForEach(s =>
                {
                    s.SocialName = Helpers.CharacterEspecials.RemoveDiacritics(s.SocialName);
                    s.FantasyName = Helpers.CharacterEspecials.RemoveDiacritics(s.FantasyName);
                    companyTemp.Add(s);
                });

                var ids = companyTemp.Where(c =>
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