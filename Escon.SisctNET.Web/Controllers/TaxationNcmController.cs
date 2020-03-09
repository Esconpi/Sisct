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
        private readonly INcmService _service;
        private readonly IConfigurationService _configurationService;
        private readonly ICompanyService _companyService;
        private readonly ICstService _cstService;

        public TaxationNcmController(
            INcmService service,
            IConfigurationService configurationService,
            ICompanyService companyService,
             ICstService cstService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Ncm")
        {
            _service = service;
            _configurationService = configurationService;
            _companyService = companyService;
            _cstService = cstService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(int id, string year, string month)
        {
            try
            {
               
                List<Ncm> ncmsAll = null;
                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                if (comp.CountingType.Name.Equals("Lucro Real"))
                {
                    ncmsAll = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.StatusReal.Equals(false)).ToList();
                }
                else
                {
                    ncmsAll = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Status.Equals(false)).ToList();
                }
                var confDBSisctNfe = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));
                string directoryNfe = confDBSisctNfe.Value + "\\" + comp.Document + "\\" + year + "\\" + month;
                var import = new Import();
                List<string> ncms = new List<string>();
                ncms = import.FindByNcms(directoryNfe);
                List<Ncm> result = new List<Ncm>();
                foreach(var ncm in ncms)
                {
                    var ncmTemp = ncmsAll.Where(_ => _.Code.Equals(ncm));
                    if (ncmTemp != null)
                    {
                        result.AddRange(ncmTemp);
                    }
                }
                ViewBag.CompanyId = id;
                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Ncm(int companyId,int id)
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

                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                if (result.CstSaidaId == null)
                {
                    result.CstSaidaId = 0;
                }
                if (result.CstEntradaId == null)
                {
                    result.CstEntradaId = 0;
                }
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
                var rst = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                entity.Created = rst.Created;
                entity.Updated = DateTime.Now;
                if (entity.CstEntradaId.Equals(0))
                {
                    entity.CstEntradaId = null;
                }
                if (entity.CstSaidaId.Equals(0))
                {
                    entity.CstSaidaId = null;
                }
                var result = _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

    }
}