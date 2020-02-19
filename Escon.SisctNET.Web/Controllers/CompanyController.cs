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

        public CompanyController(
            Fortes.IEnterpriseService fortesEnterpriseService,
            ICompanyService service,
            INoteService noteService,
            ITaxationService taxationService,
            IProductNoteService itemService,
            ITaxationTypeService taxationTypeService,
            IConfigurationService configurationService,
            IAnnexService annexService,
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
        }

        [HttpGet]
        public IActionResult Sincronize()
        {
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
            try
            {
                
                var login = SessionManager.GetLoginInSession();
                if (login == null)
                {
                    
                    return RedirectToAction("Index", "Authentication");
                }
                else
                {
                    var result = _service.FindAll(GetLog(Model.OccorenceLog.Read));
                    return View(result);
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
            try
            {
                List<Company> list_matrix = _service.FindAll(GetLog(OccorenceLog.Read));
                list_matrix.Insert(0, new Company() { SocialName = "Nenhuma Matriz selecionada", Id = 0 });

                SelectList matrix = new SelectList(list_matrix, "Id", "SocialName", null);
                ViewBag.Matrix = matrix;

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
            try
            {
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;

                if (entity.CompanyId == 0)
                    entity.CompanyId = null;        

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
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                List<Model.Company> list_matrix = _service.FindAll(GetLog(Model.OccorenceLog.Read));
                list_matrix.Insert(0, new Model.Company() { SocialName = "Nenhuma Matriz selecionada", Id = 0 });

                SelectList matrix = new SelectList(list_matrix, "Id", "SocialName", result.CompanyId);
                ViewBag.Matrix = matrix;

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
            try
            {
                var rst = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                if (entity.CompanyId == 0)
                    rst.CompanyId = null;

                entity.Created = rst.Created;
                entity.Updated = DateTime.Now;

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
            try
            {
                var rst = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                rst.TipoApuracao = entity.TipoApuracao;
                rst.AnnexId = entity.AnnexId;
                rst.Icms = entity.Icms;
                rst.Funef = entity.Funef;
                rst.Cotac = entity.Cotac;
                rst.Transferencia = entity.Transferencia;
                rst.TransferenciaExcedente = entity.TransferenciaExcedente;
                rst.TransferenciaInter = entity.TransferenciaInterExcedente;
                rst.VendaContribuinte = entity.VendaContribuinte;
                rst.VendaContribuinteExcedente = entity.VendaContribuinteExcedente;
                rst.VendaCpf = entity.VendaCpf;
                rst.VendaCpfExcedente = entity.VendaCpfExcedente;
                rst.VendaMGrupo = entity.VendaMGrupo;
                rst.VendaMGrupoExcedente = entity.VendaMGrupoExcedente;
                rst.VendaAnexo = entity.VendaAnexo;
                rst.Fecop = entity.Fecop;
                rst.Suspension = entity.Suspension;


                _service.Update(rst, GetLog(Model.OccorenceLog.Update));

                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Details(int id)
        {
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                ViewBag.Id = result.Id;
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
        public IActionResult Compare(int id, int ident)
        {
            try
            {
                ViewBag.Ident = ident;
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                return PartialView(result);
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

    }
}