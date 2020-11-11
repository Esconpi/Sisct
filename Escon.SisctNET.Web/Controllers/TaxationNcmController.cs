using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{
    public class TaxationNcmController : ControllerBaseSisctNET
    {
        private readonly INcmService _ncmService;
        private readonly IConfigurationService _configurationService;
        private readonly ICompanyService _companyService;
        private readonly ICstService _cstService;
        private readonly ITaxationNcmService _service;
        private readonly ITypeNcmService _typeNcmService;
        private readonly INatReceitaService _natReceitaService;
        private readonly IHostingEnvironment _appEnvironment;

        public TaxationNcmController(
            INcmService ncmService,
            IConfigurationService configurationService,
            ICompanyService companyService,
            ICstService cstService,
            ITaxationNcmService service,
            ITypeNcmService typeNcmService,
            INatReceitaService natReceitaService,
            IHostingEnvironment env,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "TaxationNcm")
        {
            _ncmService = ncmService;
            _configurationService = configurationService;
            _companyService = companyService;
            _cstService = cstService;
            _service = service;
            _typeNcmService = typeNcmService;
            _natReceitaService = natReceitaService;
            _appEnvironment = env;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        [HttpGet]
        public IActionResult Import()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxationNcm")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var comp = _companyService.FindById(SessionManager.GetCompanyIdInSession(), GetLog(Model.OccorenceLog.Read));
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Import(int companyid, string year, string month,string arquivo)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxationNcm")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                var comp = _companyService.FindById(companyid, GetLog(Model.OccorenceLog.Read));

                var ncmsMonofasicoAll = _service.FindByCompany(comp.Document);
                var ncmsAll = _ncmService.FindAll(null);                

                if (comp.CountingTypeId == null)
                {
                    ViewBag.Erro = 1;
                    return View(comp);
                }

                var confDBSisctNfe = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));

                var importDir = new Diretorio.Import();

                string directoryNfe = "";

                if (arquivo.Equals("xmlE"))
                {
                    directoryNfe = importDir.SaidaEmpresa(comp, confDBSisctNfe.Value, year, month);
                }
                else
                {
                    directoryNfe = importDir.SaidaSefaz(comp, confDBSisctNfe.Value, year, month);
                }

                var importXml = new Xml.Import();
                List<List<string>> ncms = new List<List<string>>();
                ncms = importXml.FindByNcms(directoryNfe);

                List<TaxationNcm> monoAdd = new List<TaxationNcm>();
                List<TaxationNcm> monoUpdate = new List<TaxationNcm>();

                for (int i = 0; i < ncms.Count(); i++)
                {
                    var ncmMonofasicoTemp = ncmsMonofasicoAll.Where(_ => _.CodeProduct.Equals(ncms[i][0]) && _.Ncm.Code.Equals(ncms[i][1])).FirstOrDefault();

                    var ncmTemp = ncmsAll.Where(_ => _.Code.Equals(ncms[i][1])).FirstOrDefault();

                    if (ncmMonofasicoTemp == null && ncmTemp != null) 
                    {
                        TaxationNcm tributacao = new TaxationNcm();

                        tributacao.CompanyId = companyid;
                        tributacao.NcmId = ncmTemp.Id;
                        tributacao.CodeProduct = ncms[i][0];
                        tributacao.Product = ncms[i][2];
                        tributacao.Year = year;
                        tributacao.Month = month;
                        tributacao.Created = DateTime.Now;
                        tributacao.Updated = DateTime.Now;
                        tributacao.Type = "Normal";
                        tributacao.TypeNcmId = 2;

                        monoAdd.Add(tributacao);

                    }

                    if (ncmTemp == null)
                    {
                        ViewBag.Erro = 2;
                        ViewBag.Ncm = ncms[i][1];
                        return View(comp);
                    }

                    if (ncmMonofasicoTemp != null)
                    {
                        ncmMonofasicoTemp.CodeProduct = ncms[i][0];
                        ncmMonofasicoTemp.NcmId = ncmTemp.Id;
                        ncmMonofasicoTemp.Product = ncms[i][2];
                        ncmMonofasicoTemp.Updated = DateTime.Now;

                        monoUpdate.Add(ncmMonofasicoTemp);
                    }
                }

                _service.Create(monoAdd, null);
                _service.Update(monoUpdate, null);

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                return RedirectToAction("Index", new { id = companyid, year = year, month = month});
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Index(int id, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxationNcm")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Comp = comp;
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

        public IActionResult IndexAll(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxationNcm")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Comp = comp;
                SessionManager.SetCompanyIdInSession(id);
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxationNcm")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                List<Model.Cst> list_cstE = _cstService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Ident.Equals(false) && _.Type.Equals(true)).OrderBy(_ => _.Code).ToList();
                foreach (var c in list_cstE)
                {
                    c.Description = c.Code + " - " + c.Description;
                }
                list_cstE.Insert(0, new Model.Cst() { Description = "Nennhum", Id = 0 });
                SelectList cstE = new SelectList(list_cstE, "Id", "Description", null);
                ViewBag.CstEntradaId = cstE;

                List<Model.Cst> list_cstS = _cstService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Ident.Equals(true) && _.Type.Equals(true)).OrderBy(_ => _.Code).ToList();
                foreach (var c in list_cstS)
                {
                    c.Description = c.Code + " - " + c.Description;
                }
                list_cstS.Insert(0, new Model.Cst() { Description = "Nennhum", Id = 0 });
                SelectList cstS = new SelectList(list_cstS, "Id", "Description", null);
                ViewBag.CstSaidaID = cstS;

                List<Model.TypeNcm> list_tipos = _typeNcmService.FindAll(null);
                SelectList listType = new SelectList(list_tipos, "Id", "Name", null);
                ViewBag.ListaTipoNcm = listType;

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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxationNcm")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                var rst = _service.FindById(id, null);

                List<TaxationNcm> tributacoes = new List<TaxationNcm>();

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

                    rst.TypeNcmId = entity.TypeNcmId;
                    rst.Status = true;
                    rst.NatReceita = entity.NatReceita;
                    rst.Pis = entity.Pis;
                    rst.Cofins = entity.Cofins;
                    rst.DateStart = entity.DateStart;
                    rst.Type = Request.Form["type"].ToString();

                    tributacoes.Add(rst);
                    //_service.Update(rst, GetLog(Model.OccorenceLog.Update));

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

                        tributacoes.Add(n);
                        //_service.Update(n, null);
                    }
                }

                _service.Update(tributacoes, null);

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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxationNcm")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try {

                List<Model.Cst> list_cstE = _cstService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Ident.Equals(false) && _.Type.Equals(true)).OrderBy(_ => _.Code).ToList();
                foreach (var c in list_cstE)
                {
                    c.Description = c.Code + " - " + c.Description;
                }
                list_cstE.Insert(0, new Model.Cst() { Description = "Nennhum", Id = 0 });
                SelectList cstE = new SelectList(list_cstE, "Id", "Description", null);
                ViewBag.CstEntradaId = cstE;

                List<Model.Cst> list_cstS = _cstService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Ident.Equals(true) && _.Type.Equals(true)).OrderBy(_ => _.Code).ToList();
                foreach (var c in list_cstS)
                {
                    c.Description = c.Code + " - " + c.Description;
                }
                list_cstS.Insert(0, new Model.Cst() { Description = "Nennhum", Id = 0 });
                SelectList cstS = new SelectList(list_cstS, "Id", "Description", null);
                ViewBag.CstSaidaID = cstS;

                List<Model.TypeNcm> list_tipos = _typeNcmService.FindAll(null);
                SelectList listType = new SelectList(list_tipos, "Id", "Name", null);
                ViewBag.ListaTipoNcm = listType;

                var result = _service.FindById(id,null);

                ViewBag.CompanyId = result.CompanyId;

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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxationNcm")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {


                var rst = _service.FindById(id, null);

                List<TaxationNcm> tributacoes = new List<TaxationNcm>();

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

                    rst.TypeNcmId = entity.TypeNcmId;
                    rst.Status = true;
                    rst.NatReceita = entity.NatReceita;
                    rst.Pis = entity.Pis;
                    rst.Cofins = entity.Cofins;
                    rst.DateStart = entity.DateStart;
                    rst.Type = Request.Form["type"].ToString();

                    tributacoes.Add(rst);
                    //_service.Update(rst, GetLog(Model.OccorenceLog.Update));

                }
                else if (Request.Form["opcao"].ToString() == "2")
                {
                    var ncms = _service.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.CompanyId.Equals(rst.CompanyId) && _.NcmId.Equals(rst.NcmId) && _.DateEnd.Equals(null)).ToList();

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

                        tributacoes.Add(n);
                        //_service.Update(n, null);
                    }
                }

                _service.Update(tributacoes, null);

                /*rst.Updated = DateTime.Now;
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

                rst.TypeNcmId = entity.TypeNcmId;
                rst.Status = true;
                rst.NatReceita = entity.NatReceita;
                rst.Pis = entity.Pis;
                rst.Cofins = entity.Cofins;
                rst.DateStart = entity.DateStart;
                rst.Type = Request.Form["type"].ToString();
                _service.Update(rst, GetLog(Model.OccorenceLog.Update));*/
                return RedirectToAction("IndexALl", new { id = rst.CompanyId});
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Atualize(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxationNcm")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }
            try
            {
                List<Model.Cst> list_cstE = _cstService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Ident.Equals(false) && _.Type.Equals(true)).OrderBy(_ => _.Code).ToList();
                foreach (var c in list_cstE)
                {
                    c.Description = c.Code + " - " + c.Description;
                }
                list_cstE.Insert(0, new Model.Cst() { Description = "Nennhum", Id = 0 });
                SelectList cstE = new SelectList(list_cstE, "Id", "Description", null);
                ViewBag.CstEntradaId = cstE;



                List<Model.Cst> list_cstS = _cstService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Ident.Equals(true) && _.Type.Equals(true)).OrderBy(_ => _.Code).ToList();
                foreach (var c in list_cstS)
                {
                    c.Description = c.Code + " - " + c.Description;
                }
                list_cstS.Insert(0, new Model.Cst() { Description = "Nennhum", Id = 0 });
                SelectList cstS = new SelectList(list_cstS, "Id", "Description", null);
                ViewBag.CstSaidaID = cstS;

                var result = _service.FindById(id, null);

                ViewBag.CompanyId = result.CompanyId;

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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxationNcm")).FirstOrDefault() == null)
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
                entity.TypeNcmId = rst.TypeNcmId;

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
                return RedirectToAction("IndexALl", new { id = rst.CompanyId });
                //return View(rst);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Compare()
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxationNcm")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var comp = _companyService.FindById(SessionManager.GetCompanyIdInSession(), GetLog(Model.OccorenceLog.Read));
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public async Task<IActionResult> Relatory(DateTime inicio, DateTime fim, IFormFile arquivo)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxationNcm")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var comp = _companyService.FindById(SessionManager.GetCompanyIdInSession(), GetLog(Model.OccorenceLog.Read));

                ViewBag.Comp = comp;
                ViewBag.Inicio = inicio;
                ViewBag.Fim = fim;

                if (arquivo == null || arquivo.Length == 0)
                {
                    ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                    return View(ViewData);
                }

                string filedir = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Planilha");

                if (!Directory.Exists(filedir))
                {
                    Directory.CreateDirectory(filedir);
                }

                string nomeArquivo = comp.Document + "Fortes";

                if (arquivo.FileName.Contains(".xls") || arquivo.FileName.Contains(".xlsx"))
                    nomeArquivo += ".xls";

                string caminho_WebRoot = _appEnvironment.WebRootPath;
                string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Planilha\\";
                string caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                string[] paths_upload_ato = Directory.GetFiles(caminhoDestinoArquivo);

                if (System.IO.File.Exists(caminhoDestinoArquivoOriginal))
                {
                    System.IO.File.Delete(caminhoDestinoArquivoOriginal);

                }

                var stream = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);
                await arquivo.CopyToAsync(stream);
                stream.Close();

                var import = new Planilha.Import();

                var ncmsMono = _service.FindByCompany(comp.Document);

                var ncmsSisct = _service.FindByPeriod(ncmsMono,inicio,fim);
                var ncmsFortes = import.Ncms(caminhoDestinoArquivoOriginal);

                var ncmsAll = _ncmService.FindAll(null);
                var natReceitasAll = _natReceitaService.FindAll(null);

                List<List<string>> ncmdivergentes = new List<List<string>>();

                foreach (var nF in ncmsFortes)
                {
                    bool achou = false;
                    int contF = nF[2].Count();
                    foreach (var nS in ncmsSisct)
                    {
                        int contS = nS.CodeProduct.Count();
                        string nSTEmp = "", nFTemp = "", natReceitaTemp = null;
                        int dif = 0;

                        if (contS > contF)
                        {
                            dif = contS - contF;
                            for (int i = 0; i < dif; i++)
                            {
                                nFTemp += "0";
                            }
                            nFTemp += nF[2];
                            nSTEmp = nS.CodeProduct;
                        }
                        else
                        {
                            dif = contF - contS;
                            for (int i = 0; i < dif; i++)
                            {
                                nSTEmp += "0";
                            }
                            nSTEmp += nS.CodeProduct;
                            nFTemp = nF[2];
                        }

                        var natReceita = natReceitasAll.Where(_ => _.CodigoAC.Equals(nF[10])).FirstOrDefault();

                        if (natReceita != null)
                        {
                            natReceitaTemp = natReceita.Code;
                        }

                        if (nSTEmp.Equals(nFTemp) && nS.Ncm.Code.Equals(nF[3]) && nS.CstSaida.Code.Equals(nF[5]) && Convert.ToInt32(nS.NatReceita).Equals(Convert.ToInt32(natReceitaTemp)))
                        {
                            achou = true;
                            break;
                        }
                    }

                    if (achou == false)
                    {
                        Model.TaxationNcm temp = new Model.TaxationNcm();

                        bool contem = false;

                        foreach (var nS in ncmsSisct)
                        {
                            int contS = nS.CodeProduct.Count();
                            string nSTemp = "", nFTemp = "";
                            int dif = 0;

                            if (contS > contF)
                            {
                                dif = contS - contF;
                                for (int i = 0; i < dif; i++)
                                {
                                    nFTemp += "0";
                                }
                                nFTemp += nF[2];
                                nSTemp = nS.CodeProduct;
                            }
                            else
                            {
                                dif = contF - contS;
                                for (int i = 0; i < dif; i++)
                                {
                                    nSTemp += "0";
                                }
                                nSTemp += nS.CodeProduct;
                                nFTemp = nF[2];
                            }

                            if (nSTemp.Equals(nFTemp))
                            {
                                temp = nS;
                                contem = true;
                                break;
                            }
                        }

                        if(contem == true)
                        {
                            List<string> divergente = new List<string>();
                            divergente.Add(temp.CodeProduct);
                            divergente.Add(temp.Product);
                            divergente.Add(temp.Ncm.Code);
                            divergente.Add(temp.Ncm.Description);
                            if (temp.CstSaidaId != null)
                            {
                                divergente.Add(temp.CstSaida.Code);
                            }
                            else
                            {
                                divergente.Add("");
                            }

                            divergente.Add(temp.NatReceita);

                            var tempNcm = ncmsAll.Where(_ => _.Code.Equals(nF[3])).FirstOrDefault();
                            var natReceita = natReceitasAll.Where(_ => _.CodigoAC.Equals(nF[10])).FirstOrDefault();

                            if (tempNcm == null)
                            {
                                ViewBag.Ncm = nF[3];
                                ViewBag.Erro = 2;
                                return View();
                            }

                            divergente.Add(nF[3]);
                            divergente.Add(tempNcm.Description);
                            divergente.Add(nF[5]);

                            if (natReceita != null)
                            {
                                divergente.Add(natReceita.Code);
                            }
                            else
                            {
                                divergente.Add("");
                            }

                            ncmdivergentes.Add(divergente);
                        }
                        else
                        {
                            ViewBag.Erro = 1;
                            return View();
                        }
                    }
                }

                ViewBag.Divergentes = ncmdivergentes;

                return View();
            }
            catch (ArgumentException aEx)
            {
                return BadRequest(new { erro = 500, message = aEx.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Details(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("TaxationNcm")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var rst = _service.FindById(id, null);
                return PartialView(rst);
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

            var ncmsAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession()).OrderBy(_ => _.Ncm.Code).ToList();

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
                              Type = r.Type,
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
                              Type = r.Type,
                              Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")
                          };
                return Ok(new { draw = draw, recordsTotal = ncmsAll.Count(), recordsFiltered = ncmsAll.Count(), data = ncm.Skip(start).Take(lenght) });
            }

        }

        public IActionResult GetAllCompany(int draw, int start)
        {


            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var ncmsAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession()).Where(_ => _.Year.Equals(SessionManager.GetYearInSession()) && _.Month.Equals(SessionManager.GetMonthInSession())).ToList().OrderBy(_ => _.Status).ToList();


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
                              Type = r.Type
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
                              Type = r.Type
                          };
                return Ok(new { draw = draw, recordsTotal = ncmsAll.Count(), recordsFiltered = ncmsAll.Count(), data = ncm.Skip(start).Take(lenght) });
            }

        }

    }
}