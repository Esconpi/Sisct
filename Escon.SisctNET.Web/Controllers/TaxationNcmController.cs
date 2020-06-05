﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Taxation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
            if (!SessionManager.GetTaxationNcmInSession().Equals(26))
            {
                return Unauthorized();
            }
            try
            {
                var ncmsMonofasicoAll = _service.FindAll(null);
                var ncmsAll = _ncmService.FindAll(null);

                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));

                if (comp.CountingTypeId == null)
                {
                    throw new Exception("Escolha o Tipo da Empresa");
                }

                var confDBSisctNfe = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));
                


                string directoryNfe = confDBSisctNfe.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                var import = new Import();
                List<List<string>> ncms = new List<List<string>>();
                ncms = import.FindByNcms(directoryNfe);

                TaxationNcm ncmMonofasicoTemp = new TaxationNcm();

                for (int i = 0; i < ncms.Count(); i++)
                {
                    if (comp.CountingTypeId.Equals(1))
                    {
                        ncmMonofasicoTemp = ncmsMonofasicoAll.Where(_ => _.CodeProduct.Equals(ncms[i][0]) && _.Ncm.Code.Equals(ncms[i][1]) && _.Company.CountingTypeId.Equals(1)).FirstOrDefault();
                    }
                    else
                    {
                        ncmMonofasicoTemp = ncmsMonofasicoAll.Where(_ => _.CodeProduct.Equals(ncms[i][0]) && _.Ncm.Code.Equals(ncms[i][1]) && (_.Company.CountingTypeId.Equals(2) || _.Company.CountingTypeId.Equals(3))).FirstOrDefault();
                    }
                    var ncmTemp = ncmsAll.Where(_ => _.Code.Equals(ncms[i][1])).FirstOrDefault();
                    if (ncmMonofasicoTemp == null && ncmTemp != null) 
                    {
                        var taxationNcm = new Model.TaxationNcm
                        {
                            CompanyId = id,
                            NcmId = ncmTemp.Id,
                            CodeProduct = ncms[i][0],
                            Year = year,
                            Month = month,
                            Created = DateTime.Now,
                            Updated = DateTime.Now,
                            Type = "Nenhum"
                        };

                        _service.Create(entity:taxationNcm, GetLog(Model.OccorenceLog.Create));
                    }
                    if (ncmTemp == null)
                    {
                        string message = "O NCM " + ncms[i][1] + " não estar cadastrado";
                        throw new Exception(message);
                    }
                }
                return RedirectToAction("Index", new { id = id, year = year, month = month});
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Index(int id, string year, string month)
        {
            if (!SessionManager.GetTaxationNcmInSession().Equals(26))
            {
                return Unauthorized();
            }
            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Name = comp.SocialName;
                ViewBag.Document = comp.Document;
                ViewBag.CompanyId = id;
                ViewBag.Year = year;
                ViewBag.Month = month;
                SessionManager.SetCompanyIdInSession(id);
                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);
                var result = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.CompanyId.Equals(id) && _.Year.Equals(year) && _.Month.Equals(month)).ToList();
                return View(null);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult IndexAll()
        {
            if (!SessionManager.GetTaxationNcmInSession().Equals(26))
            {
                return Unauthorized();
            }
            try
            {
                return View(null);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Ncm(int id)
        {
            if (!SessionManager.GetTaxationNcmInSession().Equals(26))
            {
                return Unauthorized();
            }
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

                var result = _service.FindById(id, null);

                if (result.CstSaidaId == null)
                {
                    result.CstSaidaId = 0;
                }

                if (result.CstEntradaId == null)
                {
                    result.CstEntradaId = 0;
                }
                ViewBag.CompanyId = result.CompanyId;
                ViewBag.Year = result.Year;
                ViewBag.Month = result.Month;

                return View(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }


        [HttpPost]
        public IActionResult Ncm(int id, Model.TaxationNcm entity)
        {
            if (!SessionManager.GetTaxationNcmInSession().Equals(26))
            {
                return Unauthorized();
            }
            try
            {
                var rst = _service.FindById(id, null);

                if (Request.Form["opcao"].ToString() == "1")
                {
                    rst.Updated = DateTime.Now;

                    if (entity.CstEntradaId.Equals(0))
                    {
                        rst.CstEntradaId = null;
                    }
                    else
                    {
                        rst.CstEntradaId = entity.CstEntradaId;
                    }

                    if (entity.CstSaidaId.Equals(0))
                    {
                        rst.CstSaidaId = null;
                    }
                    else
                    {
                        rst.CstSaidaId = entity.CstSaidaId;
                    }

                    rst.Status = true;
                    rst.NatReceita = entity.NatReceita;
                    rst.Pis = entity.Pis;
                    rst.Cofins = entity.Cofins;
                    rst.DateStart = entity.DateStart;
                    rst.Type = Request.Form["type"].ToString();


                    _service.Update(rst, GetLog(Model.OccorenceLog.Update));

                }
                else if(Request.Form["opcao"].ToString() == "2")
                {
                    var ncms = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.CompanyId.Equals(rst.CompanyId) && _.Year.Equals(rst.Year) && _.Month.Equals(rst.Month) && _.NcmId.Equals(rst.NcmId)).ToList();

                    foreach (var n in ncms)
                    {
                        n.Updated = DateTime.Now;

                        if (entity.CstEntradaId.Equals(0))
                        {
                            n.CstEntradaId = null;
                        }
                        else
                        {
                            n.CstEntradaId = entity.CstEntradaId;
                        }

                        if (entity.CstSaidaId.Equals(0))
                        {
                            n.CstSaidaId = null;
                        }
                        else
                        {
                            n.CstSaidaId = entity.CstSaidaId;
                        }

                        n.Status = true;
                        n.NatReceita = entity.NatReceita;
                        n.Pis = entity.Pis;
                        n.Cofins = entity.Cofins;
                        n.DateStart = entity.DateStart;
                        n.Type = Request.Form["type"].ToString();
                        _service.Update(n, null);
                    }
                }

               

                return RedirectToAction("Index" , new {id = rst.CompanyId, year = rst.Year, month = rst.Month});
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!SessionManager.GetTaxationNcmInSession().Equals(26))
            {
                return Unauthorized();
            }
            try {
                List<Model.Cst> list_cstE = _cstService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Ident.Equals(false)).ToList();
                list_cstE.Insert(0, new Model.Cst() { Code = "Nennhum", Id = 0 });
                SelectList cstE = new SelectList(list_cstE, "Id", "Code", null);
                ViewBag.CstEntradaId = cstE;

                List<Model.Cst> list_cstS = _cstService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Ident.Equals(true)).ToList();
                list_cstS.Insert(0, new Model.Cst() { Code = "Nennhum", Id = 0 });
                SelectList cstS = new SelectList(list_cstS, "Id", "Code", null);
                ViewBag.CstSaidaID = cstS;

                var result = _service.FindById(id,null);

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
        public IActionResult Edit(int id, Model.TaxationNcm entity)
        {
            if (!SessionManager.GetTaxationNcmInSession().Equals(26))
            {
                return Unauthorized();
            }
            try
            {
                var rst = _service.FindById(id, null);
                rst.Updated = DateTime.Now;
                var type = Request.Form["type"].ToString();

                if (entity.CstEntradaId.Equals(0))
                {
                    rst.CstEntradaId = null;
                }
                else
                {
                    rst.CstEntradaId = entity.CstEntradaId;
                }
                if (entity.CstSaidaId.Equals(0))
                {
                    rst.CstSaidaId = null;
                }
                else
                {
                    rst.CstSaidaId = entity.CstSaidaId;
                }

                rst.Status = true;
                rst.NatReceita = entity.NatReceita;
                rst.Pis = entity.Pis;
                rst.Cofins = entity.Cofins;
                rst.DateStart = entity.DateStart;
                rst.Type = Request.Form["type"].ToString();
                _service.Update(rst, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("IndexALl");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Atualize(int id)
        {
            if (!SessionManager.GetTaxationNcmInSession().Equals(26))
            {
                return Unauthorized();
            }
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
                var result = _service.FindById(id, null);

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
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Atualize(int id, Model.TaxationNcm entity)
        {
            if (!SessionManager.GetTaxationNcmInSession().Equals(26))
            {
                return Unauthorized();
            }
            try
            {
                var rst = _service.FindById(id, null);
                rst.Updated = DateTime.Now;
                rst.DateEnd = Convert.ToDateTime(entity.DateStart).AddDays(-1);
                _service.Update(rst, GetLog(Model.OccorenceLog.Update));

                var lastId = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Max(_ => _.Id);
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;
                entity.DateEnd = null;
                entity.CompanyId = rst.CompanyId;
                entity.NcmId = rst.NcmId;

                if (entity.CstEntradaId.Equals(0))
                {
                    entity.CstEntradaId = null;
                }

                if (entity.CstSaidaId.Equals(0))
                {
                    entity.CstSaidaId = null;
                }

                entity.Status = true;
                entity.CodeProduct = rst.CodeProduct;
                entity.Id = lastId + 1;
                entity.Type = Request.Form["type"].ToString();

                _service.Create(entity, GetLog(Model.OccorenceLog.Create));

                return View(rst);
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

            var ncmsAll = _service.FindAll(GetLog(Model.OccorenceLog.Read)).OrderBy(_ => _.Ncm.Code).Where(_ => _.Status.Equals(true) && _.Type.Equals("Monofásico")).ToList();


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<TaxationNcm> ncms = new List<TaxationNcm>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<TaxationNcm> ncmTemp = new List<TaxationNcm>();
                ncmsAll.ToList().ForEach(s =>
                {
                    s.Ncm.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Ncm.Description);
                    s.Ncm.Code = s.Ncm.Code;
                    ncmTemp.Add(s);
                });

                var ids = ncmTemp.Where(c =>
                    c.Ncm.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Ncm.Code.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                ncms = ncmsAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var ncm = from r in ncms
                          where ids.ToArray().Contains(r.Id)
                          select new
                          {
                              Id = r.Id.ToString(),
                              CodeProd = r.CodeProduct,
                              Code = r.Ncm.Code,
                              Description = r.Ncm.Description,
                              Inicio = Convert.ToDateTime(r.DateStart).ToString("dd/MM/yyyy"),
                              Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")

                          };

                return Ok(new { draw = draw, recordsTotal = ncms.Count(), recordsFiltered = ncms.Count(), data = ncm.Skip(start).Take(lenght) });

            }
            else
            {


                var ncm = from r in ncmsAll
                          select new
                          {
                              Id = r.Id.ToString(),
                              CodeProd = r.CodeProduct,
                              Code = r.Ncm.Code,
                              Description = r.Ncm.Description,
                              Inicio = Convert.ToDateTime(r.DateStart).ToString("dd/MM/yyyy"),
                              Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")

                          };
                return Ok(new { draw = draw, recordsTotal = ncmsAll.Count(), recordsFiltered = ncmsAll.Count(), data = ncm.Skip(start).Take(lenght) });
            }

        }

        public IActionResult GetAllCompany(int draw, int start)
        {


            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var ncmsAll = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.CompanyId.Equals(SessionManager.GetCompanyIdInSession()) && _.Year.Equals(SessionManager.GetYearInSession()) && _.Month.Equals(SessionManager.GetMonthInSession())).ToList().OrderBy(_ => _.Status).ToList();


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<TaxationNcm> ncms = new List<TaxationNcm>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<TaxationNcm> ncmTemp = new List<TaxationNcm>();
                ncmsAll.ToList().ForEach(s =>
                {
                    s.Ncm.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Ncm.Description);
                    s.Ncm.Code = s.Ncm.Code;
                    ncmTemp.Add(s);
                });

                var ids = ncmTemp.Where(c =>
                    c.Ncm.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Ncm.Code.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                ncms = ncmsAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var ncm = from r in ncms
                          where ids.ToArray().Contains(r.Id)
                          select new
                          {
                              Id = r.Id.ToString(),
                              CodeProd = r.CodeProduct,
                              Code = r.Ncm.Code,
                              Description = r.Ncm.Description,
                              Status = r.Status

                          };

                return Ok(new { draw = draw, recordsTotal = ncms.Count(), recordsFiltered = ncms.Count(), data = ncm.Skip(start).Take(lenght) });

            }
            else
            {


                var ncm = from r in ncmsAll
                          select new
                          {
                              Id = r.Id.ToString(),
                              CodeProd = r.CodeProduct,
                              Code = r.Ncm.Code,
                              Description = r.Ncm.Description,
                              Status = r.Status

                          };
                return Ok(new { draw = draw, recordsTotal = ncmsAll.Count(), recordsFiltered = ncmsAll.Count(), data = ncm.Skip(start).Take(lenght) });
            }

        }

    }
}