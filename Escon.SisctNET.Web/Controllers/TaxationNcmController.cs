using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Taxation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class TaxationNcmController : ControllerBaseSisctNET
    {
        private readonly INcmService _ncmService;
        private readonly IConfigurationService _configurationService;
        private readonly ICompanyService _companyService;
        private readonly ICstService _cstService;
        private readonly ITaxationNcmService _service;

        public TaxationNcmController(
            INcmService ncmService,
            IConfigurationService configurationService,
            ICompanyService companyService,
            ICstService cstService,
            ITaxationNcmService service,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "TaxationNcm")
        {
            _ncmService = ncmService;
            _configurationService = configurationService;
            _companyService = companyService;
            _cstService = cstService;
            _service = service;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        [HttpGet]
        public IActionResult Import(int id, string year, string month)
        {
            try
            {
                List<Ncm> ncmsAll = null;
                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                if (comp.CountingType.Name.Equals("Lucro Real"))
                {
                    ncmsAll = _ncmService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.StatusReal.Equals(false)).ToList();
                }
                else
                {
                    ncmsAll = _ncmService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Status.Equals(false)).ToList();
                }
                var confDBSisctNfe = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));
                string directoryNfe = confDBSisctNfe.Value + "\\" + comp.Document + "\\" + year + "\\" + month;
                var import = new Import();
                List<string> ncms = new List<string>();
                ncms = import.FindByNcms(directoryNfe);

                var nncms = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.CompanyId.Equals(id) && _.Year.Equals(year) && _.Month.Equals(month)).ToList();

                foreach (var ncm in ncms)
                {
                    var ncmTemp = ncmsAll.Where(_ => _.Code.Equals(ncm)).FirstOrDefault();
                    var nTemp = nncms.Where(_ => _.Ncm.Code.Equals(ncm)).FirstOrDefault();

                    if (ncmTemp != null && nTemp != null) 
                    {
                        var taxationNcm = new Model.TaxationNcm
                        {
                            CompanyId = id,
                            NcmId = ncmTemp.Id,
                            Year = year,
                            Month = month,
                            Created = DateTime.Now,
                            Updated = DateTime.Now,
                        };

                        _service.Create(taxationNcm, GetLog(Model.OccorenceLog.Create));
                    }
                }
                return RedirectToAction("Index", new { id = id, year = year, month = month });
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Index(int id, string year, string month)
        {
            try
            {
                ViewBag.CompanyId = id;
                ViewBag.Year = year;
                ViewBag.Month = month;
                var result = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.CompanyId.Equals(id) && _.Year.Equals(year) && _.Month.Equals(month)).ToList();
                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Ncm(int companyId, string year, string month, int id)
        {
            try
            {
                List<Model.Cst> list_cstE = _cstService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Ident.Equals(false)).ToList();
                list_cstE.Insert(0, new Model.Cst() { Code = "Nennhum", Id = 0 });
                SelectList cstE = new SelectList(list_cstE, "Id", "Code", null);
                ViewBag.CstEntradaId = cstE;

                List<Model.Cst> list_cstS = _cstService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Ident.Equals(true)).ToList();
                list_cstS.Insert(0, new Model.Cst() { Code = "Nennhum", Id = 0 });
                SelectList cstS = new SelectList(list_cstS, "Id", "Code", null);
                ViewBag.CstSaidaID = cstS;

                var result = _ncmService.FindById(id, GetLog(Model.OccorenceLog.Read));

                if (result.CstSaidaId == null)
                {
                    result.CstSaidaId = 0;
                }
                if (result.CstEntradaId == null)
                {
                    result.CstEntradaId = 0;
                }
                var comp = _companyService.FindById(companyId, GetLog(Model.OccorenceLog.Read));
                ViewBag.Type = comp.CountingType.Name;
                ViewBag.CompanyId = companyId;
                ViewBag.Year = year;
                ViewBag.Month = month;

                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }


        [HttpPost]
        public IActionResult Ncm(int id, Model.Ncm entity)
        {
            try
            {
                var rst = _ncmService.FindById(id, GetLog(Model.OccorenceLog.Read));
                entity.Created = rst.Created;
                entity.Updated = DateTime.Now;
                var type = Request.Form["type"].ToString();
                if(type != "Lucro Real")
                {
                    if (entity.CstEntradaId.Equals(0))
                    {
                        entity.CstEntradaId = null;
                    }
                    if (entity.CstSaidaId.Equals(0))
                    {
                        entity.CstSaidaId = null;
                    }
                    entity.CstEntradaRealId = null;
                    entity.CstSaidaRealId = null;
                    entity.Status = true;
                }
                else
                {
                    if (entity.CstEntradaRealId.Equals(0))
                    {
                        entity.CstEntradaRealId = null;
                    }
                    if (entity.CstSaidaRealId.Equals(0))
                    {
                        entity.CstSaidaRealId = null;
                    }
                    entity.CstEntradaId = null;
                    entity.CstSaidaId = null;
                    entity.StatusReal = true;
                }

                var company = Convert.ToInt32(Request.Form["company"]);
                var year = Request.Form["year"].ToString();
                var month = Request.Form["month"].ToString();
                _ncmService.Update(entity, GetLog(Model.OccorenceLog.Update));

                var ncm = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.CompanyId.Equals(company) && _.Year.Equals(year) && _.Month.Equals(month) && _.NcmId.Equals(id)).FirstOrDefault();
                ncm.Status = true;
                _service.Update(ncm, GetLog(Model.OccorenceLog.Update));

                return RedirectToAction("Index" , new {id = company, year = year, month = month});
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

    }
}