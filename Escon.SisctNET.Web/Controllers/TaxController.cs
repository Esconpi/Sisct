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
    public class TaxController : ControllerBaseSisctNET
    {
        private readonly ITaxService _service;
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly ICompanyCfopService _companyCfopService;
        private readonly ITaxationNcmService _taxationNcmService;
        private readonly INcmConvenioService _ncmConvenioService;
        private readonly IClientService _clientService;
        private readonly ISuspensionService _suspensionService;
        private readonly IGrupoService _grupoService;
        private readonly IProductNoteService _itemService;
        private readonly IProductIncentivoService _productIncentivoService;
        private readonly IHostingEnvironment _appEnvironment;

        public TaxController(
            ITaxService service,
            ICompanyService companyService,
            IConfigurationService configurationService,
            ICompanyCfopService companyCfopService,
            ITaxationNcmService taxationNcmService,
            INcmConvenioService ncmConvenioService,
            IClientService clientService,
            ISuspensionService suspensionService,
            IGrupoService grupoService,
            IProductNoteService itemService,
            IProductIncentivoService productIncentivoService,
            IHostingEnvironment env,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Tax")
        {
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _companyCfopService = companyCfopService;
            _taxationNcmService = taxationNcmService;
            _ncmConvenioService = ncmConvenioService;
            _clientService = clientService;
            _suspensionService = suspensionService;
            _grupoService = grupoService;
            _itemService = itemService;
            _productIncentivoService = productIncentivoService;
            _appEnvironment = env;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(long id,string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);

                ViewBag.PercentualPetroleo = Convert.ToDecimal(comp.IRPJ1).ToString().Replace(".", ",");
                ViewBag.PercentualComercio = Convert.ToDecimal(comp.IRPJ2).ToString().Replace(".", ",");
                ViewBag.PercentualTransporte = Convert.ToDecimal(comp.IRPJ3).ToString().Replace(".", ",");
                ViewBag.PercentualServico = Convert.ToDecimal(comp.IRPJ4).ToString().Replace(".", ",");

                ViewBag.Company = comp;

                SessionManager.SetCompanyIdInSession(id);
                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);
            
                var result = _service.FindByMonth(id, month, year, null);

                decimal vendasGrupo = 0, devolucaoGrupo = 0;

                List<Grupo> grupos = new List<Grupo>();

                if(result != null)
                {
                    grupos = _grupoService.FindByGrupos(result.Id);

                    vendasGrupo = Convert.ToDecimal(grupos.Sum(_ => _.Vendas));
                    devolucaoGrupo = Convert.ToDecimal(grupos.Sum(_ => _.Devolucao));
                }

                ViewBag.Grupos = grupos;
                ViewBag.VendasGrupo = vendasGrupo;
                ViewBag.DevolucoesGrupo = devolucaoGrupo;
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Receita()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(SessionManager.GetCompanyIdInSession(), null);
                var tax = _service.FindByMonth(SessionManager.GetCompanyIdInSession(), SessionManager.GetMonthInSession(), SessionManager.GetYearInSession());
                ViewBag.Tax = tax;
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Receita(Model.Tax entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                long companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var imp = _service.FindByMonth(companyid, month, year);

                if (imp == null)
                {
                    entity.CompanyId = companyid;
                    entity.MesRef = month;
                    entity.AnoRef = year;
                    entity.Created = DateTime.Now;
                    entity.Updated = entity.Created;

                    _service.Create(entity, GetLog(OccorenceLog.Create));
                }
                else
                {
                    imp.Bonificacao = entity.Bonificacao;
                    imp.CapitalIM = entity.CapitalIM;
                    imp.ReceitaAF = entity.ReceitaAF;
                    imp.OutrasReceitas = entity.OutrasReceitas;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, GetLog(OccorenceLog.Update));
                }

                return RedirectToAction("Index", new { id = companyid, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Despesa()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(SessionManager.GetCompanyIdInSession(), null);
                var tax = _service.FindByMonth(SessionManager.GetCompanyIdInSession(), SessionManager.GetMonthInSession(), SessionManager.GetYearInSession());
                ViewBag.Tax = tax;
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Despesa(Model.Tax entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                long companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var imp = _service.FindByMonth(companyid, month, year);

                if (imp == null)
                {

                    entity.CompanyId = companyid;
                    entity.MesRef = month;
                    entity.AnoRef = year;
                    entity.Created = DateTime.Now;
                    entity.Updated = entity.Created;

                    _service.Create(entity, GetLog(OccorenceLog.Create));
                }
                else
                {
                    imp.Energia = entity.Energia;
                    imp.AluguelPredio = entity.AluguelPredio;
                    imp.AluguelME = entity.AluguelME;
                    imp.DespesasF = entity.DespesasF;
                    imp.DespesasME = entity.DespesasME;
                    imp.DespesasA = entity.DespesasA;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, GetLog(OccorenceLog.Update));
                }

                return RedirectToAction("Index", new { id = companyid, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Retention()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(SessionManager.GetCompanyIdInSession(), null);
                var tax = _service.FindByMonth(SessionManager.GetCompanyIdInSession(), SessionManager.GetMonthInSession(), SessionManager.GetYearInSession());
                ViewBag.Tax = tax;
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Retention(Model.Tax entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                long companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var imp = _service.FindByMonth(companyid, month, year);

                if (imp == null)
                {
                    entity.CompanyId = companyid;
                    entity.MesRef = month;
                    entity.AnoRef = year;
                    entity.Created = DateTime.Now;
                    entity.Updated = entity.Created;

                    _service.Create(entity, GetLog(OccorenceLog.Create));
                }
                else
                {
                    imp.PisRetido = entity.PisRetido;
                    imp.CofinsRetido = entity.CofinsRetido;
                    imp.CsllRetido = entity.CsllRetido;
                    imp.CsllFonte = entity.CsllFonte;
                    imp.IrpjRetido = entity.IrpjRetido;
                    imp.IrpjFonteServico = entity.IrpjFonteServico;
                    imp.IrpjFonteFinanceira = entity.IrpjFonteFinanceira;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, GetLog(OccorenceLog.Update));
                }

                return RedirectToAction("Index", new { id = companyid, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Reduction()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(SessionManager.GetCompanyIdInSession(), null);
                var tax = _service.FindByMonth(SessionManager.GetCompanyIdInSession(), SessionManager.GetMonthInSession(), SessionManager.GetYearInSession());
                ViewBag.Tax = tax;
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Reduction(Model.Tax entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                long companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();

                string month = SessionManager.GetMonthInSession();
                var imp = _service.FindByMonth(companyid, month, year);

                if (imp == null)
                {
                    entity.CompanyId = companyid;
                    entity.MesRef = month;
                    entity.AnoRef = year;
                    entity.Created = DateTime.Now;
                    entity.Updated = entity.Created;

                    _service.Create(entity, GetLog(OccorenceLog.Create));
                }
                else
                {
                    imp.ReducaoIcms = entity.ReducaoIcms;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, GetLog(OccorenceLog.Update));
                }

                return RedirectToAction("Index", new { id = companyid, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Loss()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(SessionManager.GetCompanyIdInSession(), null);
                var tax = _service.FindByMonth(SessionManager.GetCompanyIdInSession(), SessionManager.GetMonthInSession(), SessionManager.GetYearInSession());
                ViewBag.Tax = tax;
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Loss(Model.Tax entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                long companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();
                var imp = _service.FindByMonth(companyid, month, year);

                if (imp == null)
                {
                    entity.CompanyId = companyid;
                    entity.MesRef = month;
                    entity.AnoRef = year;
                    entity.Created = DateTime.Now;
                    entity.Updated = entity.Created;

                    _service.Create(entity, GetLog(OccorenceLog.Create));
                }
                else
                {
                    imp.Perda = entity.Perda;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, GetLog(OccorenceLog.Update));
                }

                return RedirectToAction("Index", new { id = companyid, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Pag()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(SessionManager.GetCompanyIdInSession(), null);
                var importPeriodo = new Period.Bimestre();
                var meses = importPeriodo.Months(SessionManager.GetMonthInSession());
                ViewBag.Meses = meses;
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Pag(string mesRef, Model.Tax entity) 
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                long companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var imp = _service.FindByMonth(companyid, mesRef, year);

                if (imp == null)
                {
                    entity.CompanyId = companyid;
                    entity.MesRef = month;
                    entity.AnoRef = year;
                    entity.Created = DateTime.Now;
                    entity.Updated = entity.Created;

                    _service.Create(entity, GetLog(OccorenceLog.Create));
                }
                else
                {
                    imp.IrpjPago = entity.IrpjPago;
                    imp.CsllPago = entity.CsllPago;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, GetLog(OccorenceLog.Update));
                }

                return RedirectToAction("Index", new { id = companyid, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Service()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(SessionManager.GetCompanyIdInSession(), null);
                var tax = _service.FindByMonth(SessionManager.GetCompanyIdInSession(), SessionManager.GetMonthInSession(), SessionManager.GetYearInSession());
                ViewBag.Tax = tax;
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Service(Model.Tax entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                long companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();
                var imp = _service.FindByMonth(companyid, month, year);

                if (imp == null)
                {
                    entity.CompanyId = companyid;
                    entity.MesRef = month;
                    entity.AnoRef = year;
                    entity.Created = DateTime.Now;
                    entity.Updated = entity.Created;

                    _service.Create(entity, GetLog(OccorenceLog.Create));
                }
                else
                {
                    imp.Receita4 = entity.Receita4;
                    imp.Devolucao4 = entity.Devolucao4;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, GetLog(OccorenceLog.Update));
                }

                return RedirectToAction("Index", new { id = companyid, year = year, month = month });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Import()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var comp = _companyService.FindById(SessionManager.GetCompanyIdInSession(), null);
                return View(comp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Import(string imposto, string type, List<IFormFile> arquivo)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                long companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var comp = _companyService.FindById(companyid, null);

                if (comp.CountingTypeId == null)
                {
                    ViewBag.Erro = 1;
                    return View(comp);
                }

                var NfeExit = _configurationService.FindByName("NFe Saida", null);
                var NfeEntry = _configurationService.FindByName("NFe", null);

                var importDir = new Diretorio.Import();
                var calculation = new Tax.Calculation();
                var importMes = new Period.Month();

                var mes = importMes.NumberMonth(month);

                DateTime data = Convert.ToDateTime("01" + "/" + mes + "/" + year);

                string directoryNfeExit = "", arqui = "";

                if (type.Equals("xmlE"))
                {
                    directoryNfeExit = importDir.SaidaEmpresa(comp, NfeExit.Value, year, month);
                    arqui = "XML EMPRESA";
                }
                else
                {
                    directoryNfeExit = importDir.SaidaSefaz(comp, NfeExit.Value, year, month);
                    arqui = "XML SEFAZ";
                }

                string directoryNfeEntry = importDir.Entrada(comp, NfeEntry.Value, year, month);

                List<string> caminhos = new List<string>();
                 
                var importXml = new Xml.Import(_companyCfopService, _taxationNcmService);
                var importSped = new Sped.Import(_companyCfopService, _taxationNcmService);

                Model.Tax tax = new Model.Tax();

                List<Model.Grupo> addGrupos = new List<Grupo>();

                var imp = _service.FindByMonth(companyid,month,year);

                if(imp != null)
                {
                    if(arqui != "")
                        imp.Arquivo = arqui;

                    imp.Updated = DateTime.Now;
                }
                else
                {
                    if (arqui != "")
                    {
                        tax.Arquivo = arqui;
                    }

                    tax.CompanyId = companyid;
                    tax.MesRef = month;
                    tax.AnoRef = year;
                    tax.Created = DateTime.Now;
                    tax.Updated = tax.Created;
                }

                if (type.Equals("sped"))
                {
                    string filedir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Speds");

                    if (!Directory.Exists(filedir)) 
                        Directory.CreateDirectory(filedir);

                    string caminho_WebRoot = _appEnvironment.WebRootPath;
                    string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Speds\\";

                    int indice = 1;

                    foreach (var a in arquivo)
                    {

                        string nomeArquivo = comp.Document + indice.ToString() + year + month + ".txt";

                        string caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                        string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivo);

                        if (System.IO.File.Exists(caminhoDestinoArquivoOriginal))
                            System.IO.File.Delete(caminhoDestinoArquivoOriginal);

                        var stream = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);

                        await a.CopyToAsync(stream);

                        stream.Close();

                        caminhos.Add(caminhoDestinoArquivoOriginal);

                        indice++;
                    }
                }

                var cfopAll = _companyCfopService.FindByCompany(comp.Document);

                //  Entrada
                var cfopsCompra = _companyCfopService.FindByCfopCompra(cfopAll)
                    .Select(_ => _.Cfop.Code)
                    .Distinct()
                    .ToList();
                var cfopsCompraST = _companyCfopService.FindByCfopCompraST(cfopAll)
                    .Select(_ => _.Cfop.Code)
                    .Distinct()
                    .ToList();
                var cfopsBoniCompra = _companyCfopService.FindByCfopBonificacaoCompra(cfopAll)
                    .Select(_ => _.Cfop.Code)
                    .Distinct()
                    .ToList();
                var cfopsDevoCompra = _companyCfopService.FindByCfopDevoCompra(cfopAll)
                    .Select(_ => _.Cfop.Code)
                    .Distinct()
                    .ToList();
                var cfopsDevoCompraST = _companyCfopService.FindByCfopDevoCompraST(cfopAll)
                    .Select(_ => _.Cfop.Code)
                    .Distinct()
                    .ToList();

                //  Saida
                var cfopsVenda = _companyCfopService.FindByCfopVenda(cfopAll)
                    .Select(_ => _.Cfop.Code)
                    .Distinct()
                    .ToList();
                var cfopsVendaIM = _companyCfopService.FindByCfopVendaIM(cfopAll)
                    .Select(_ => _.Cfop.Code)
                    .Distinct()
                    .ToList();
                var cfopsVendaST = _companyCfopService.FindByCfopVendaST(cfopAll)
                    .Select(_ => _.Cfop.Code)
                    .Distinct()
                    .ToList();
                var cfopsBoniVenda = _companyCfopService.FindByCfopBonificacaoVenda(cfopAll)
                    .Select(_ => _.Cfop.Code)
                    .Distinct()
                    .ToList();
                var cfopsDevoVenda = _companyCfopService.FindByCfopDevoVenda(cfopAll)
                    .Select(_ => _.Cfop.Code)
                    .Distinct()
                    .ToList();
                var cfopsDevoVendaST = _companyCfopService.FindByCfopDevoVendaST(cfopAll)
                    .Select(_ => _.Cfop.Code)
                    .Distinct()
                    .ToList();

                //  Transferencia
                var cfopsTransf = _companyCfopService.FindByCfopTransferencia(cfopAll)
                    .Select(_ => _.Cfop.Code)
                    .Distinct()
                    .ToList();
                var cfopsTransfST = _companyCfopService.FindByCfopTransferenciaST(cfopAll)
                    .Select(_ => _.Cfop.Code)
                    .Distinct()
                    .ToList();

                var cfopsPerda = _companyCfopService.FindByCfopCompraPerda(cfopAll)
                    .Select(_ => _.Cfop.Code)
                    .Distinct()
                    .ToList();

                var ncmsConvenio = _ncmConvenioService.FindByAnnex(Convert.ToInt64(comp.AnnexId));

                List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();

                if (imposto.Equals("icms"))
                {
                    if (type.Equals("sped"))
                    {
                        if (comp.Incentive && comp.TipoApuracao)
                        {
                            decimal compra = 0, tranferencia = 0, transferenciaInter = 0;
                            foreach (var cc in caminhos)
                            {
                                var entradas = importSped.NFeEntry(cc, cfopsCompra, cfopsBoniCompra, cfopsCompraST, cfopsTransf, cfopsTransfST, comp);
                                compra += entradas[0];
                                tranferencia += entradas[1];
                                transferenciaInter += entradas[2];
                            }

                            if (imp != null)
                            {
                                imp.Compras = compra;
                                imp.TransferenciaEntrada = tranferencia;
                                imp.TransferenciaInter = transferenciaInter;
                                imp.Icms = true;
                            }
                            else
                            {
                                tax.Compras = compra;
                                tax.TransferenciaEntrada = tranferencia;
                                tax.TransferenciaInter = transferenciaInter;
                                tax.Icms = true;
                            }
                        }

                        if (comp.Incentive && !comp.TipoApuracao)
                        {
                            decimal credito = 0;

                            foreach (var cc in caminhos)
                            {
                                credito = importSped.NFeCredit(cc, cfopsDevoVenda, cfopsCompra, cfopsBoniCompra, cfopsCompraST, cfopsTransf, cfopsTransfST, cfopsDevoVendaST);
                            }

                            if (imp != null)
                            {
                                imp.Credito = credito;
                                imp.Icms = true;
                            }
                            else
                            {
                                tax.Credito = credito;
                                tax.Icms = true;
                            }
                        }
                    }
                    else if (type.Equals("xmlS") || type.Equals("xmlE"))
                    {
                        if (comp.Incentive && comp.TipoApuracao)
                        {
                            exitNotes = importXml.NFeAll(directoryNfeExit);
                            entryNotes = importXml.NFeAll(directoryNfeEntry);

                            var clientesAll = _clientService.FindByCompany(companyid).Select(_ => _.Document).ToList();
                            var suspensions = _suspensionService.FindByCompany(companyid);
                            var contribuintes = _clientService.FindByContribuinte(companyid, "all");
                            var contribuintesRaiz = _clientService.FindByContribuinte(companyid, "raiz");

                            decimal totalVendas = 0, totalVendasIncisoI = 0, totalVendasIncisoII = 0, totalNcm = 0, totalTranferenciaSaida = 0, baseCalc = 0, totalDevoCompra = 0,
                                    totalDevoVenda = 0, totalDevoVendaIncisoI = 0, totalDevoVendaIncisoII = 0, totalDevoNcm = 0, totalDevoContribuinte = 0, totalVendasSuspensao = 0;

                            int contContribuintes = contribuintes.Count(), contContribuintesRaiz = contribuintesRaiz.Count() + 1;

                            string[,] resumoCnpjs = new string[contContribuintes, 2];
                            string[,] resumoCnpjRaiz = new string[contContribuintesRaiz, 2];
                            string[,] resumoAllCnpjRaiz = new string[contContribuintesRaiz - 1, 3];

                            for (int i = 0; i < contContribuintes; i++)
                            {
                                resumoCnpjs[i, 0] = contribuintes[i];
                                resumoCnpjs[i, 1] = "0";
                            }

                            for (int i = 0; i < contContribuintesRaiz; i++)
                            {
                                if (i < contContribuintesRaiz - 1)
                                {
                                    resumoCnpjRaiz[i, 0] = contribuintesRaiz[i];
                                    resumoAllCnpjRaiz[i, 0] = contribuintesRaiz[i];
                                    resumoCnpjRaiz[i, 1] = "0";
                                    resumoAllCnpjRaiz[i, 1] = "0";
                                    resumoAllCnpjRaiz[i, 2] = "0";
                                }
                                else
                                {
                                    resumoCnpjRaiz[i, 0] = "Não contribuinte";
                                    resumoCnpjRaiz[i, 1] = "0";
                                }
                            }

                            // Notas Suspensãp
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {

                                if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                                {
                                    exitNotes.RemoveAt(i);
                                    continue;
                                }
                                bool suspenso = false, cfop = false;

                                if (exitNotes[i][1].ContainsKey("dhEmi"))
                                {
                                    foreach (var suspension in suspensions)
                                    {
                                        if (Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) >= Convert.ToDateTime(suspension.DateStart) && Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) < Convert.ToDateTime(suspension.DateEnd))
                                        {
                                            suspenso = true;
                                            break;
                                        }
                                    }
                                }

                                for (int j = 0; j < exitNotes[i].Count(); j++)
                                {
                                    if (exitNotes[i][j].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfopsVenda.Contains(exitNotes[i][j]["CFOP"]) || cfopsVendaST.Contains(exitNotes[i][j]["CFOP"]) || 
                                            cfopsBoniVenda.Contains(exitNotes[i][j]["CFOP"]) || cfopsTransf.Contains(exitNotes[i][j]["CFOP"]) ||  
                                            cfopsTransfST.Contains(exitNotes[i][j]["CFOP"]))
                                        {
                                            cfop = true;
                                        }

                                    }

                                    if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd") && suspenso && cfop)
                                        totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                    if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd") && suspenso && cfop)
                                        totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                    if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd") && suspenso && cfop)
                                        totalVendasSuspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);

                                    if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd") && suspenso && cfop)
                                        totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);

                                    if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd") && suspenso && cfop)
                                        totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);

                                }

                            }

                            // Transferência Compra
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                int posClienteRaiz = contContribuintesRaiz - 1;

                                if (exitNotes[i][3].ContainsKey("CNPJ"))
                                    if (contribuintesRaiz.Contains(exitNotes[i][3]["CNPJ"].Substring(0, 8)))
                                        posClienteRaiz = contribuintesRaiz.IndexOf(exitNotes[i][3]["CNPJ"].Substring(0, 8));


                                bool cfop = false;
                                for (int j = 0; j < exitNotes[i].Count; j++)
                                {
                                    if (exitNotes[i][j].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfopsTransf.Contains(exitNotes[i][j]["CFOP"]) || cfopsTransfST.Contains(exitNotes[i][j]["CFOP"]))
                                        {
                                            cfop = true;
                                        }

                                    }

                                    if (cfop && exitNotes[i][1]["tpNF"] != "1")
                                    {
                                        if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferenciaSaida += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                        }

                                        if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferenciaSaida += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();
                                        }

                                        if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferenciaSaida -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();
                                        }

                                        if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferenciaSaida += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();
                                        }

                                        if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferenciaSaida += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();
                                        }
                                    }

                                }

                            }

                            // Devolução Saida
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                bool cfop = false;


                                for (int j = 0; j < exitNotes[i].Count(); j++)
                                {

                                    if (exitNotes[i][j].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfopsDevoCompra.Contains(exitNotes[i][j]["CFOP"]) || cfopsDevoCompraST.Contains(exitNotes[i][j]["CFOP"]) && exitNotes[i][1]["tpNF"] != "1")
                                        {
                                            cfop = true;
                                        }

                                    }

                                    if (cfop)
                                    {
                                        if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                            totalDevoCompra += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                        if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                            totalDevoCompra += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                        if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                            totalDevoCompra -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);

                                        if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                            totalDevoCompra += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);

                                        if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                            totalDevoCompra += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                    }
                                }

                            }

                            if (comp.ChapterId.Equals((long)4))
                            {
                                var ncmInciso = _productIncentivoService.FindByAllProducts(companyid);

                                var incisoI = ncmInciso.Where(_ => _.PercentualInciso != null).OrderBy(_ => _.PercentualInciso).Select(_ => _.PercentualInciso).Distinct().FirstOrDefault();
                                var incisoII = ncmInciso.Where(_ => _.PercentualInciso != null).OrderBy(_ => _.PercentualInciso).Select(_ => _.PercentualInciso).Distinct().LastOrDefault();

                                var ncmIncisoI = ncmInciso.Where(_ => _.PercentualInciso.Equals(incisoI)).ToList();
                                var ncmIncisoII = ncmInciso.Where(_ => _.PercentualInciso.Equals(incisoII)).ToList();

                                // Vendas 
                                for (int i = exitNotes.Count - 1; i >= 0; i--)
                                {
                                    int posClienteRaiz = contContribuintesRaiz - 1, posCliente = -1;

                                    bool ncm = false, ncmI = false, ncmII = false, cfop = false;

                                    if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("IE") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][1]["mod"].Equals("55"))
                                    {
                                        string CNPJ = exitNotes[i][3].ContainsKey("CNPJ") ? exitNotes[i][3]["CNPJ"] : "escon";
                                        string indIEDest = exitNotes[i][3].ContainsKey("indIEDest") ? exitNotes[i][3]["indIEDest"] : "escon";
                                        string IE = exitNotes[i][3].ContainsKey("IE") ? exitNotes[i][3]["IE"] : "escon";

                                        if (contribuintesRaiz.Contains(exitNotes[i][3]["CNPJ"].Substring(0, 8)))
                                            posClienteRaiz = contribuintesRaiz.IndexOf(exitNotes[i][3]["CNPJ"].Substring(0, 8));

                                        if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                            posCliente = contribuintes.IndexOf(exitNotes[i][3]["CNPJ"]);

                                        bool client = false;

                                        if (clientesAll.Contains(exitNotes[i][3]["CNPJ"]))
                                            client = true;

                                        if (!client)
                                        {
                                            ViewBag.Erro = 2;
                                            return View(comp);
                                        }
                                    }

                                    for (int j = 0; j < exitNotes[i].Count(); j++)
                                    {

                                        if (exitNotes[i][j].ContainsKey("NCM"))
                                        {
                                            ncm = false;
                                            ncmI = false;
                                            ncmII = false;

                                            var nn = ncmInciso.Where(_ => _.Ncm.Equals(exitNotes[i][j]["NCM"])).FirstOrDefault();
                                            
                                            if(nn == null)
                                            {
                                                ViewBag.Erro = 3;
                                                return View();
                                            }

                                            var nIncentivo = ncmInciso.Where(_ => _.TypeTaxation.Equals("Incentivado/Regime") && _.Code.Equals(exitNotes[i][j]["cProd"]) &&
                                                _.Ncm.Equals(exitNotes[i][j]["NCM"])).FirstOrDefault();

                                            var nIncentivoIncisoI = ncmIncisoI.Where(_ => _.TypeTaxation.Equals("Incentivado/Regime") && _.Code.Equals(exitNotes[i][j]["cProd"]) &&
                                                _.Ncm.Equals(exitNotes[i][j]["NCM"])).FirstOrDefault();

                                            var nIncentivoIncisoII = ncmIncisoII.Where(_ => _.TypeTaxation.Equals("Incentivado/Regime") && _.Code.Equals(exitNotes[i][j]["cProd"]) &&
                                                _.Ncm.Equals(exitNotes[i][j]["NCM"])).FirstOrDefault();

                                            if (nIncentivo != null)
                                                ncm = true;

                                            if (nIncentivoIncisoI != null)
                                                ncmI = true;


                                            if (nIncentivoIncisoII != null)
                                                ncmII = true;

                                        }

                                        if (exitNotes[i][j].ContainsKey("CFOP"))
                                        {
                                            cfop = false;

                                            if (cfopsVenda.Contains(exitNotes[i][j]["CFOP"]) || cfopsVendaST.Contains(exitNotes[i][j]["CFOP"]) || cfopsBoniVenda.Contains(exitNotes[i][j]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }

                                        if (cfop)
                                        {
                                            if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();

                                                if (ncm)
                                                    totalNcm += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                                if (posCliente >= 0)
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                                if (ncmI)
                                                    totalVendasIncisoI += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                if (ncmII)
                                                    totalVendasIncisoII += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();

                                                if (ncm)
                                                    totalNcm += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();
                                                if (posCliente >= 0)
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();
                                                if (ncmI)
                                                    totalVendasIncisoI += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                if (ncmII)
                                                    totalVendasIncisoII += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalVendas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();

                                                if (ncm)
                                                    totalNcm -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();
                                                if (posCliente >= 0)
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();
                                                if (ncmI)
                                                    totalVendasIncisoI -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                if (ncmII)
                                                    totalVendasIncisoII -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();
                                                
                                                if (ncm)
                                                    totalNcm += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();
                                                if (posCliente >= 0)
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();
                                                if (ncmI)
                                                    totalVendasIncisoI += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                if (ncmII)
                                                    totalVendasIncisoII += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();

                                                if (ncm)
                                                    totalNcm += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();
                                                if (posCliente >= 0)
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();
                                                if (ncmI)
                                                    totalVendasIncisoI += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                if (ncmII)
                                                    totalVendasIncisoII += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            }
                                        }
                                    }

                                }

                                // Devolução Saida Emissão Própria
                                for (int i = exitNotes.Count - 1; i >= 0; i--)
                                {

                                    if (exitNotes[i][1]["finNFe"] != "4" || exitNotes[i][1]["tpNF"] == "1")
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }

                                    bool contribuinte = false, ncm = false, ncmI = false, ncmII = false, cfop = false;

                                    if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("IE") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][1]["mod"].Equals("55"))
                                        if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                            contribuinte = true;

                                    for (int j = 0; j < exitNotes[i].Count(); j++)
                                    {
                                        if (exitNotes[i][j].ContainsKey("NCM"))
                                        {
                                            ncm = false;
                                            ncmI = false;
                                            ncmII = false;

                                            var nn = ncmInciso.Where(_ => _.Ncm.Equals(exitNotes[i][j]["NCM"])).FirstOrDefault();

                                            if (nn == null)
                                            {
                                                ViewBag.Erro = 3;
                                                return View();
                                            }

                                            var nIncentivo = ncmInciso.Where(_ => _.TypeTaxation.Equals("Incentivado/Regime") && _.Code.Equals(exitNotes[i][j]["cProd"])
                                                && _.Ncm.Equals(exitNotes[i][j]["NCM"])).FirstOrDefault();

                                            var nIncentivoIncisoI = ncmIncisoI.Where(_ => _.TypeTaxation.Equals("Incentivado/Regime") && _.Code.Equals(exitNotes[i][j]["cProd"])
                                                && _.Ncm.Equals(exitNotes[i][j]["NCM"])).FirstOrDefault();

                                            var nIncentivoIncisoII = ncmIncisoII.Where(_ => _.TypeTaxation.Equals("Incentivado/Regime") && _.Code.Equals(exitNotes[i][j]["cProd"]) 
                                                && _.Ncm.Equals(exitNotes[i][j]["NCM"])).FirstOrDefault();

                                            if (nIncentivo != null)
                                                ncm = true;

                                            if (nIncentivoIncisoI != null)
                                                ncmI = true;


                                            if (nIncentivoIncisoII != null)
                                                ncmII = true;
                                        }

                                        if (exitNotes[i][j].ContainsKey("CFOP"))
                                        {
                                            cfop = false;
                                            if (cfopsDevoVenda.Contains(exitNotes[i][j]["CFOP"]) || cfopsDevoVendaST.Contains(exitNotes[i][j]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }

                                        if (cfop)
                                        {
                                            if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevoVenda += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                if(ncm)
                                                    totalDevoNcm += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                if (contribuinte)
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                if (ncmI)
                                                    totalDevoVendaIncisoI += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                if (ncmII)
                                                    totalDevoVendaIncisoII += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                            }

                                            if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevoVenda += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                if (ncm)
                                                    totalDevoNcm += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                if (contribuinte)
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                if (ncmI)
                                                    totalDevoVendaIncisoI += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                if (ncmII)
                                                    totalDevoVendaIncisoII += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevoVenda -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                if (ncm)
                                                    totalDevoNcm -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                if (contribuinte)
                                                    totalDevoContribuinte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                if (ncmI)
                                                    totalDevoVendaIncisoI -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                if (ncmII)
                                                    totalDevoVendaIncisoII -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevoVenda += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                if (ncm)
                                                    totalDevoNcm += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                if (contribuinte)
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                if (ncmI)
                                                    totalDevoVendaIncisoI += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                if (ncmII)
                                                    totalDevoVendaIncisoII += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevoVenda += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                if (ncm)
                                                    totalDevoNcm += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                if (contribuinte)
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                if (ncmI)
                                                    totalDevoVendaIncisoI += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                if (ncmII)
                                                    totalDevoVendaIncisoII += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            }
                                        }
                                    }

                                }

                                // Devolução Entrada
                                for (int i = entryNotes.Count - 1; i >= 0; i--)
                                {
                                    if (entryNotes[i][1]["finNFe"] != "4" || entryNotes[i][1]["tpNF"] == "0")
                                    {
                                        entryNotes.RemoveAt(i);
                                        continue;
                                    }
                                    else if (!entryNotes[i][3]["CNPJ"].Equals(comp.Document))
                                    {
                                        entryNotes.RemoveAt(i);
                                        continue;
                                    }

                                    bool contribuinte = false, ncm = false, ncmI = false, ncmII = false;
                                    int posClienteRaiz = contContribuintesRaiz - 1;

                                    if (entryNotes[i][2].ContainsKey("CNPJ") && entryNotes[i][3].ContainsKey("IE") && entryNotes[i][2].ContainsKey("indIEDest") && entryNotes[i][1]["mod"].Equals("55"))
                                    {

                                        if (contribuintes.Contains(entryNotes[i][2]["CNPJ"]))
                                            contribuinte = true;

                                        string CNPJ = entryNotes[i][2].ContainsKey("CNPJ") ? entryNotes[i][2]["CNPJ"] : "escon";
                                        string indIEDest = entryNotes[i][2].ContainsKey("indIEDest") ? entryNotes[i][2]["indIEDest"] : "escon";
                                        string IE = entryNotes[i][2].ContainsKey("IE") ? entryNotes[i][2]["IE"] : "escon";

                                        if (contribuintesRaiz.Contains(entryNotes[i][2]["CNPJ"].Substring(0, 8)))
                                            posClienteRaiz = contribuintesRaiz.IndexOf(entryNotes[i][2]["CNPJ"].Substring(0, 8));
                                    }

                                    for (int j = 0; j < entryNotes[i].Count(); j++)
                                    {

                                        if (entryNotes[i][j].ContainsKey("NCM"))
                                        {
                                            ncm = false;
                                            ncmI = false;
                                            ncmII = false;

                                            var nn = ncmInciso.Where(_ => _.Ncm.Equals(entryNotes[i][j]["NCM"])).FirstOrDefault();

                                            if (nn == null)
                                            {
                                                ViewBag.Erro = 3;
                                                return View();
                                            }

                                            var nIncentivo = ncmInciso.Where(_ => _.TypeTaxation.Equals("Incentivado/Regime")  && _.Code.Equals(entryNotes[i][j]["cProd"])
                                                && _.Ncm.Equals(entryNotes[i][j]["NCM"])).FirstOrDefault();

                                            var nIncentivoIncisoI = ncmIncisoI.Where(_ => _.TypeTaxation.Equals("Incentivado/Regime") && _.Code.Equals(entryNotes[i][j]["cProd"]) 
                                                && _.Ncm.Equals(entryNotes[i][j]["NCM"])).FirstOrDefault();

                                            var nIncentivoIncisoII = ncmIncisoII.Where(_ => _.TypeTaxation.Equals("Incentivado/Regime") && _.Code.Equals(entryNotes[i][j]["cProd"]) 
                                                && _.Ncm.Equals(entryNotes[i][j]["NCM"])).FirstOrDefault();

                                            if (nIncentivo != null)
                                                ncm = true;

                                            if (nIncentivoIncisoI != null)
                                                ncmI = true;


                                            if (nIncentivoIncisoII != null)
                                                ncmII = true;
                                        }

                                        if (entryNotes[i][j].ContainsKey("vProd") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevoVenda += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            if (ncm)
                                                totalDevoNcm += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            if (contribuinte)
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vProd"])).ToString();
                                            if (ncmI)
                                                totalDevoVendaIncisoI += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            if (ncmII)
                                                totalDevoVendaIncisoII += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                        }

                                        if (entryNotes[i][j].ContainsKey("vFrete") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevoVenda += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            if (ncm)
                                                totalDevoNcm += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            if (contribuinte)
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vFrete"])).ToString();
                                            if (ncmI)
                                                totalDevoVendaIncisoI += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            if (ncmII)
                                                totalDevoVendaIncisoII += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                        }

                                        if (entryNotes[i][j].ContainsKey("vDesc") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevoVenda -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            if (ncm)
                                                totalDevoNcm -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            if (contribuinte)
                                                totalDevoContribuinte -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(entryNotes[i][j]["vDesc"])).ToString();
                                            if (ncmI)
                                                totalDevoVendaIncisoI -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            if (ncmII)
                                                totalDevoVendaIncisoII -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                        }

                                        if (entryNotes[i][j].ContainsKey("vOutro") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevoVenda += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            if (ncm)
                                                totalDevoNcm += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            if (contribuinte)
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vOutro"])).ToString();
                                            if (ncmI)
                                                totalDevoVendaIncisoI += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            if (ncmII)
                                                totalDevoVendaIncisoII += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);

                                        }

                                        if (entryNotes[i][j].ContainsKey("vSeg") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevoVenda += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            if (ncm)
                                                totalDevoNcm += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            if (contribuinte)
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(entryNotes[i][j]["vSeg"])).ToString();
                                            if (ncmI)
                                                totalDevoVendaIncisoI += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            if (ncmII)
                                                totalDevoVendaIncisoII += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                        }
                                    }

                                }
                            }
                            else
                            {
                                var ncmConvenio = _ncmConvenioService.FindByNcmAnnex((long)comp.AnnexId);

                                // Vendas 
                                for (int i = exitNotes.Count - 1; i >= 0; i--)
                                {
                                    int posClienteRaiz = contContribuintesRaiz - 1, posCliente = -1;

                                    bool ncm = false, cfop = false;

                                    if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("IE") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][1]["mod"].Equals("55"))
                                    {
                                        string CNPJ = exitNotes[i][3].ContainsKey("CNPJ") ? exitNotes[i][3]["CNPJ"] : "escon";
                                        string indIEDest = exitNotes[i][3].ContainsKey("indIEDest") ? exitNotes[i][3]["indIEDest"] : "escon";
                                        string IE = exitNotes[i][3].ContainsKey("IE") ? exitNotes[i][3]["IE"] : "escon";

                                        if (contribuintesRaiz.Contains(exitNotes[i][3]["CNPJ"].Substring(0, 8)))
                                            posClienteRaiz = contribuintesRaiz.IndexOf(exitNotes[i][3]["CNPJ"].Substring(0, 8));

                                        if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                            posCliente = contribuintes.IndexOf(exitNotes[i][3]["CNPJ"]);

                                        bool client = false;

                                        if (clientesAll.Contains(exitNotes[i][3]["CNPJ"]))
                                            client = true;

                                        if (!client)
                                        {
                                            ViewBag.Erro = 2;
                                            return View(comp);
                                        }
                                    }

                                    for (int j = 0; j < exitNotes[i].Count(); j++)
                                    {

                                        if (exitNotes[i][j].ContainsKey("NCM"))
                                        {
                                            string CEST = exitNotes[i][j].ContainsKey("CEST") ? exitNotes[i][j]["CEST"] : "";

                                            ncm = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, exitNotes[i][j]["NCM"], CEST, comp);
                                        }

                                        if (exitNotes[i][j].ContainsKey("CFOP"))
                                        {
                                            cfop = false;
                                            if (cfopsVenda.Contains(exitNotes[i][j]["CFOP"]) || cfopsVendaST.Contains(exitNotes[i][j]["CFOP"]) ||  
                                                cfopsBoniVenda.Contains(exitNotes[i][j]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }

                                        if (cfop)
                                        {
                                            if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                                if (ncm)
                                                    totalNcm += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();
                                                if (ncm)
                                                    totalNcm += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalVendas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();
                                                if (ncm)
                                                    totalNcm -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();
                                                if (ncm)
                                                    totalNcm += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();
                                                }
                                            }

                                            if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalVendas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();
                                                if (ncm)
                                                    totalNcm += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                                {
                                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();
                                                }
                                                if (posCliente >= 0)
                                                {
                                                    resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();

                                                }
                                            }
                                        }
                                    }

                                }

                                // Devolução Saida Emissão Própria
                                for (int i = exitNotes.Count - 1; i >= 0; i--)
                                {

                                    if (exitNotes[i][1]["finNFe"] != "4" || exitNotes[i][1]["tpNF"] == "1")
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }

                                    bool contribuinte = false, ncm = false, cfop = false;

                                    if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("IE") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][1]["mod"].Equals("55"))
                                    {

                                        if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                            contribuinte = true;
                                    }

                                    for (int j = 0; j < exitNotes[i].Count(); j++)
                                    {
                                        if (exitNotes[i][j].ContainsKey("NCM"))
                                        {
                                            string CEST = exitNotes[i][j].ContainsKey("CEST") ? exitNotes[i][j]["CEST"] : "";

                                            ncm = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, exitNotes[i][j]["NCM"], CEST, comp);
                                        }

                                        if (exitNotes[i][j].ContainsKey("CFOP"))
                                        {
                                            cfop = false;
                                            if (cfopsDevoVenda.Contains(exitNotes[i][j]["CFOP"]) || cfopsDevoVendaST.Contains(exitNotes[i][j]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }

                                        if (cfop)
                                        {
                                            if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevoVenda += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                if(ncm)
                                                    totalDevoNcm += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                if (contribuinte)
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevoVenda += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                if (ncm)
                                                    totalDevoNcm += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                if (contribuinte)
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevoVenda -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                if (ncm)
                                                    totalDevoNcm -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                if (contribuinte)
                                                    totalDevoContribuinte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevoVenda += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                if (ncm)
                                                    totalDevoNcm += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                if (contribuinte)
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                            {
                                                totalDevoVenda += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                if (ncm)
                                                    totalDevoNcm += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                if (contribuinte)
                                                    totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            }
                                        }
                                        
                                    }

                                }

                                // Devolução Entrada
                                for (int i = entryNotes.Count - 1; i >= 0; i--)
                                {
                                    if (entryNotes[i][1]["finNFe"] != "4" || entryNotes[i][1]["tpNF"] == "0")
                                    {
                                        entryNotes.RemoveAt(i);
                                        continue;
                                    }

                                    bool contribuinte = false;
                                    int posClienteRaiz = contContribuintesRaiz - 1;

                                    if (entryNotes[i][2].ContainsKey("CNPJ") && entryNotes[i][3].ContainsKey("IE") && entryNotes[i][2].ContainsKey("indIEDest") && entryNotes[i][1]["mod"].Equals("55"))
                                    {

                                        if (contribuintes.Contains(entryNotes[i][2]["CNPJ"]))
                                            contribuinte = true;

                                        string CNPJ = entryNotes[i][2].ContainsKey("CNPJ") ? entryNotes[i][2]["CNPJ"] : "escon";
                                        string indIEDest = entryNotes[i][2].ContainsKey("indIEDest") ? entryNotes[i][2]["indIEDest"] : "escon";
                                        string IE = entryNotes[i][2].ContainsKey("IE") ? entryNotes[i][2]["IE"] : "escon";

                                        if (contribuintesRaiz.Contains(entryNotes[i][2]["CNPJ"].Substring(0, 8)))
                                            posClienteRaiz = contribuintesRaiz.IndexOf(entryNotes[i][2]["CNPJ"].Substring(0, 8));
                                    }

                                    bool ncm = false;

                                    for (int j = 0; j < entryNotes[i].Count(); j++)
                                    {

                                        if (entryNotes[i][j].ContainsKey("NCM"))
                                        {
                                            string CEST = entryNotes[i][j].ContainsKey("CEST") ? entryNotes[i][j]["CEST"] : "";

                                            ncm = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, entryNotes[i][j]["NCM"], CEST, comp);
                                        }

                                        if (entryNotes[i][j].ContainsKey("vProd") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevoVenda += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            if (ncm)
                                                totalDevoNcm += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            if (contribuinte)
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                        }

                                        if (entryNotes[i][j].ContainsKey("vFrete") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevoVenda += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            if (ncm)
                                                totalDevoNcm += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            if (contribuinte)
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();
                                        }

                                        if (entryNotes[i][j].ContainsKey("vDesc") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevoVenda -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            if (ncm)
                                                totalDevoNcm -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            if (contribuinte)
                                                totalDevoContribuinte -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();
                                        }

                                        if (entryNotes[i][j].ContainsKey("vOutro") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevoVenda += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            if (ncm)
                                                totalDevoNcm += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            if (contribuinte)
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);

                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();
                                        }

                                        if (entryNotes[i][j].ContainsKey("vSeg") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevoVenda += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            if (ncm)
                                                totalDevoNcm += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            if (contribuinte)
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();

                                        }
                                    }

                                }
                            }

                            baseCalc = totalVendas - totalDevoVenda + totalTranferenciaSaida;
                            totalVendas += totalTranferenciaSaida;

                            decimal totalNcontribuinte = Convert.ToDecimal(resumoCnpjRaiz[contContribuintesRaiz - 1, 1]),
                                    totalContribuinte = totalVendas - totalNcontribuinte,
                                    baseCalcContribuinte = totalContribuinte - totalDevoContribuinte,
                                    totalDevoNContribuinte = totalDevoVenda - totalDevoContribuinte,
                                    baseCalcNContribuinte = totalNcontribuinte - totalDevoNContribuinte,
                                    limiteGrupo = (baseCalc * Convert.ToDecimal(comp.VendaMGrupo)) / 100;

                            if (imp != null)
                            {
                                imp.Vendas = totalVendas;
                                imp.VendasIncisoI = totalVendasIncisoI;
                                imp.VendasIncisoII = totalVendasIncisoII;
                                imp.VendasNContribuinte = totalNcontribuinte;
                                imp.VendasNcm = totalNcm;
                                imp.TransferenciaSaida = totalTranferenciaSaida;
                                imp.DevolucaoVendas = totalDevoVenda;
                                imp.DevolucaoVendasIncisoI = totalDevoVendaIncisoI;
                                imp.DevolucaoVendasIncisoII = totalDevoVendaIncisoII;
                                imp.DevolucaoCompras = totalDevoCompra;
                                imp.DevolucaoNContribuinte = totalDevoNContribuinte;
                                imp.DevolucaoNcm = totalDevoNcm;
                                imp.Suspensao = totalVendasSuspensao;
                                imp.Icms = true;

                                var grupoTemp = _grupoService.FindByGrupos(imp.Id);

                                if (grupoTemp != null)
                                {
                                    List<Model.Grupo> updateGrupos = new List<Grupo>();

                                    for (int i = 0; i < contContribuintesRaiz - 1; i++)
                                    {
                                        var vendaGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 1]);
                                        var devoGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 2]);
                                        var baseCalcGrupo = (vendaGrupo - devoGrupo);
                                        if (baseCalcGrupo > limiteGrupo && vendaGrupo > 0 && limiteGrupo > 0)
                                        {
                                            List<string> grupoExcedente = new List<string>();
                                            var cnpjGrupo = resumoAllCnpjRaiz[i, 0];
                                            var clientGrupo = _clientService.FindByRaiz(cnpjGrupo);
                                            var nomeGrupo = clientGrupo.Name;
                                            var percentGrupo = Math.Round((baseCalcGrupo * 100) / baseCalc, 2);

                                            var gg = grupoTemp.Where(_ => _.Cnpj.Equals(cnpjGrupo)).FirstOrDefault();

                                            if (gg != null)
                                            {
                                                gg.Cnpj = cnpjGrupo;
                                                gg.Nome = nomeGrupo;
                                                gg.Vendas = vendaGrupo;
                                                gg.Devolucao = devoGrupo;
                                                gg.BaseCalculo = baseCalcGrupo;
                                                gg.Percentual = percentGrupo;
                                                gg.TaxId = imp.Id;
                                                gg.Updated = DateTime.Now;

                                                updateGrupos.Add(gg);
                                            }
                                            else
                                            {
                                                Model.Grupo grupo = new Model.Grupo();
                                                grupo.Cnpj = cnpjGrupo;
                                                grupo.Nome = nomeGrupo;
                                                grupo.Vendas = vendaGrupo;
                                                grupo.Devolucao = devoGrupo;
                                                grupo.BaseCalculo = baseCalcGrupo;
                                                grupo.Percentual = percentGrupo;
                                                grupo.TaxId = imp.Id;
                                                grupo.Created = DateTime.Now;
                                                grupo.Updated = grupo.Created;

                                                addGrupos.Add(grupo);
                                            }


                                        }
                                    }

                                    _grupoService.Create(addGrupos, GetLog(OccorenceLog.Create));
                                    _grupoService.Update(updateGrupos, GetLog(OccorenceLog.Update));
                                }
                                else
                                {
                                    for (int i = 0; i < contContribuintesRaiz - 1; i++)
                                    {
                                        var vendaGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 1]);
                                        var devoGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 2]);
                                        var baseCalcGrupo = (vendaGrupo - devoGrupo);
                                        if (baseCalcGrupo > limiteGrupo && vendaGrupo > 0 && limiteGrupo > 0)
                                        {
                                            List<string> grupoExcedente = new List<string>();
                                            var cnpjGrupo = resumoAllCnpjRaiz[i, 0];
                                            var clientGrupo = _clientService.FindByRaiz(cnpjGrupo);
                                            var nomeGrupo = clientGrupo.Name;
                                            var percentGrupo = Math.Round((baseCalcGrupo * 100) / baseCalc, 2);

                                            Model.Grupo grupo = new Model.Grupo();
                                            grupo.Cnpj = cnpjGrupo;
                                            grupo.Nome = nomeGrupo;
                                            grupo.Vendas = vendaGrupo;
                                            grupo.Devolucao = devoGrupo;
                                            grupo.BaseCalculo = baseCalcGrupo;
                                            grupo.Percentual = percentGrupo;
                                            grupo.TaxId = imp.Id;
                                            grupo.Created = DateTime.Now;
                                            grupo.Updated = grupo.Created;

                                            addGrupos.Add(grupo);

                                        }
                                    }
                                    _grupoService.Create(addGrupos, GetLog(OccorenceLog.Create));
                                }
                            }
                            else
                            {
                                tax.Vendas = totalVendas;
                                tax.VendasIncisoI = totalVendasIncisoI;
                                tax.VendasIncisoII = totalVendasIncisoII;
                                tax.VendasNContribuinte = totalNcontribuinte;
                                tax.VendasNcm = totalNcm;
                                tax.TransferenciaSaida = totalTranferenciaSaida;
                                tax.DevolucaoVendas = totalDevoCompra;
                                tax.DevolucaoVendasIncisoI = totalDevoVendaIncisoI;
                                tax.DevolucaoVendasIncisoII = totalDevoVendaIncisoII;
                                tax.DevolucaoNContribuinte = totalDevoNContribuinte;
                                tax.DevolucaoNcm = totalDevoNcm;
                                tax.Suspensao = totalVendasSuspensao;
                                tax.Icms = true;

                                for (int i = 0; i < contContribuintesRaiz - 1; i++)
                                {
                                    var vendaGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 1]);
                                    var devoGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 2]);
                                    var baseCalcGrupo = (vendaGrupo - devoGrupo);
                                    if (baseCalcGrupo > limiteGrupo && vendaGrupo > 0 && limiteGrupo > 0)
                                    {
                                        List<string> grupoExcedente = new List<string>();
                                        var cnpjGrupo = resumoAllCnpjRaiz[i, 0];
                                        var clientGrupo = _clientService.FindByRaiz(cnpjGrupo);
                                        var nomeGrupo = clientGrupo.Name;
                                        var percentGrupo = Math.Round((baseCalcGrupo * 100) / baseCalc, 2);

                                        Model.Grupo grupo = new Model.Grupo();
                                        grupo.Cnpj = cnpjGrupo;
                                        grupo.Nome = nomeGrupo;
                                        grupo.Vendas = vendaGrupo;
                                        grupo.Devolucao = devoGrupo;
                                        grupo.BaseCalculo = baseCalcGrupo;
                                        grupo.Percentual = percentGrupo;
                                        grupo.Created = DateTime.Now;
                                        grupo.Updated = grupo.Created;

                                        addGrupos.Add(grupo);
                                    }
                                }
                            }

                        }
                        
                        if(comp.Incentive && !comp.TipoApuracao)
                        {
                            if (!comp.AnnexId.Equals((long)3))
                            {
                                //  Incentivo de Produto e Indústria
                                var prodsAll = _productIncentivoService.FindByAllProducts(companyid);
                                List<Model.ProductIncentivo> prodsTemp = new List<ProductIncentivo>();
                                List<string> codeProdIncentivado = new List<string>();
                                List<string> codeProdST = new List<string>();
                                List<string> codeProdIsento = new List<string>();
                                List<string> cestIncentivado = new List<string>();
                                List<string> cestST = new List<string>();
                                List<string> cestIsento = new List<string>();

                                var codeProd = prodsAll.Select(_ => _.Code).ToList();
                                var cestProd = prodsAll.Select(_ => _.Cest).ToList();

                                if (comp.TypeCompany.Equals(true))
                                {
                                    //  Incentivo Produto

                                    List<List<string>> icmsForaDoEstado = new List<List<string>>();

                                    var contribuintes = _clientService.FindByContribuinte(companyid, "all");
                                    var clientesAll = _clientService.FindByCompany(companyid).Select(_ => _.Document).ToList();

                                    exitNotes = importXml.NFeAll(directoryNfeExit);

                                    decimal totalVendas = 0, naoContribuinteIncentivo = 0, naoContribuinteNIncetivo = 0, vendaCfopSTNaoContribuinteNIncetivo = 0,
                                        naoContribuiteIsento = 0, naoContriForaDoEstadoIncentivo = 0, naoContriForaDoEstadoNIncentivo = 0,
                                        vendaCfopSTNaoContriForaDoEstadoNIncentivo = 0, naoContribuinteForaDoEstadoIsento = 0, contribuintesNIncentivo = 0,
                                        contribuintesIncentivoAliqM25 = 0, contribuintesIncentivo = 0, vendaCfopSTContribuintesNIncentivo = 0,
                                        contribuinteIsento = 0, debitosIcms = 0;

                                    // Vendas
                                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                                    {
                                        if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                                        {
                                            exitNotes.RemoveAt(i);
                                            continue;
                                        }

                                        if (exitNotes[i][1].ContainsKey("dhEmi"))
                                        {
                                            prodsTemp = _productIncentivoService.FindByDate(prodsAll, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));

                                            codeProdIncentivado = prodsTemp.Where(_ => _.TypeTaxation.Equals("Incentivado/Normal") || _.TypeTaxation.Equals("Incentivado/ST") || _.TypeTaxation.Equals("Normal")).Select(_ => _.Code).ToList();
                                            codeProdST = prodsTemp.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Code).ToList();
                                            codeProdIsento = prodsTemp.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Code).ToList();

                                            cestIncentivado = prodsTemp.Where(_ => _.TypeTaxation.Equals("Incentivado/Normal") || _.TypeTaxation.Equals("Incentivado/ST") || _.TypeTaxation.Equals("Normal")).Select(_ => _.Cest).ToList();
                                            cestST = prodsTemp.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Cest).ToList();
                                            cestIsento = prodsTemp.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Cest).ToList();
                                        }

                                        int posCliente = -1;

                                        if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][3].ContainsKey("IE"))
                                        {
                                            string CNPJ = exitNotes[i][3]["CNPJ"];
                                            string indIEDest = exitNotes[i][3]["indIEDest"];
                                            string IE = exitNotes[i][3]["IE"];
                                            if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                                posCliente = contribuintes.IndexOf(exitNotes[i][3]["CNPJ"]);

                                            bool existe = false;
                                            if (clientesAll.Contains(exitNotes[i][3]["CNPJ"]))
                                                existe = true;

                                            if (!existe)
                                            {
                                                ViewBag.Erro = 2;
                                                return View(comp);
                                            }
                                        }

                                        int posUf = -1;
                                        if (exitNotes[i][1]["idDest"].Equals("2"))
                                        {
                                            var UF = exitNotes[i][3].ContainsKey("UF") ? exitNotes[i][3]["UF"] : "";

                                            for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                            {
                                                if (icmsForaDoEstado[j][0].Equals(UF))
                                                {
                                                    posUf = j;
                                                    break;
                                                }
                                            }

                                            if (posUf < 0)
                                            {
                                                List<string> uf = new List<string>();
                                                uf.Add(UF);
                                                uf.Add("0,00");
                                                uf.Add("0,00");
                                                icmsForaDoEstado.Add(uf);
                                            }

                                        }

                                        bool cfop = false;
                                        decimal valorProduto = 0;
                                        string cProd = "", cest = "";

                                        for (int k = 0; k < exitNotes[i].Count(); k++)
                                        {
                                            if (exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                cProd = exitNotes[i][k]["cProd"];
                                                cest = "";
                                                if (exitNotes[i][k].ContainsKey("CEST"))
                                                    cest = exitNotes[i][k]["CEST"];

                                                if (exitNotes[i][k].ContainsKey("CFOP"))
                                                {
                                                    cfop = false;
                                                    if (cfopsVenda.Contains(exitNotes[i][k]["CFOP"]) || cfopsBoniVenda.Contains(exitNotes[i][k]["CFOP"]) ||
                                                        cfopsTransf.Contains(exitNotes[i][k]["CFOP"]))
                                                    {
                                                        cfop = true;
                                                    }

                                                }

                                                if (cfop)
                                                {
                                                    if (exitNotes[i][k].ContainsKey("vProd"))
                                                        valorProduto = Convert.ToDecimal(exitNotes[i][k]["vProd"]);

                                                    if (exitNotes[i][k].ContainsKey("vFrete"))
                                                        valorProduto += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);

                                                    if (exitNotes[i][k].ContainsKey("vDesc"))
                                                        valorProduto -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);

                                                    if (exitNotes[i][k].ContainsKey("vOutro"))
                                                        valorProduto += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);

                                                    if (exitNotes[i][k].ContainsKey("vSeg"))
                                                        valorProduto += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                }

                                            }

                                            if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("orig") && cfop)
                                            {
                                                decimal aliquota = 0;
                                                if (exitNotes[i][k].ContainsKey("pICMS"))
                                                {
                                                    aliquota = Convert.ToDecimal(exitNotes[i][k]["pICMS"]);
                                                    debitosIcms += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                                }

                                                if(codeProd.Contains(cProd) && cestProd.Contains(cest))
                                                {
                                                    if (codeProdIncentivado.Contains(cProd) && cestIncentivado.Contains(cest))
                                                    {

                                                        if (posCliente < 0)
                                                        {
                                                            naoContribuinteIncentivo += Convert.ToDecimal(valorProduto);

                                                            if (exitNotes[i][1]["idDest"].Equals("2") && posUf >= 0)
                                                            {
                                                                naoContriForaDoEstadoIncentivo += Convert.ToDecimal(valorProduto);
                                                                icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(valorProduto)).ToString();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (aliquota <= 25)
                                                                contribuintesIncentivo += Convert.ToDecimal(valorProduto);
                                                            else
                                                                contribuintesIncentivoAliqM25 += Convert.ToDecimal(valorProduto);

                                                        }
                                                        totalVendas += Convert.ToDecimal(valorProduto);

                                                    }
                                                    else if (codeProdST.Contains(cProd) && cestST.Contains(cest))
                                                    {
                                                        if (posCliente < 0)
                                                        {
                                                            naoContribuinteNIncetivo += Convert.ToDecimal(valorProduto);

                                                            if (exitNotes[i][1]["idDest"].Equals("2") && posUf >= 0)
                                                            {
                                                                naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(valorProduto);
                                                                icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(valorProduto)).ToString();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            contribuintesNIncentivo += Convert.ToDecimal(valorProduto);
                                                        }
                                                        totalVendas += Convert.ToDecimal(valorProduto);

                                                    }
                                                    else if (codeProdIsento.Contains(cProd) && cestIsento.Contains(cest))
                                                    {
                                                        if (posCliente < 0)
                                                        {
                                                            naoContribuiteIsento += Convert.ToDecimal(valorProduto);

                                                            if (exitNotes[i][1]["idDest"].Equals("2") && posUf >= 0)
                                                            {
                                                                naoContribuinteForaDoEstadoIsento += Convert.ToDecimal(valorProduto);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            contribuinteIsento += Convert.ToDecimal(valorProduto);
                                                        }
                                                        totalVendas += Convert.ToDecimal(valorProduto);
                                                    }
                                                }
                                                else
                                                {
                                                    ViewBag.Erro = 3;
                                                    return View(comp);
                                                }
                                                cest = "";
                                                cProd = "";
                                                valorProduto = 0;
                                                aliquota = 0;
                                            }

                                            if (exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("orig") && cfop)
                                                debitosIcms += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                        }

                                    }

                                    // Vendas ST
                                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                                    {
                                        int posCliente = -1;

                                        if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][3].ContainsKey("IE"))
                                        {
                                            string CNPJ = exitNotes[i][3]["CNPJ"];
                                            string indIEDest = exitNotes[i][3]["indIEDest"];
                                            string IE = exitNotes[i][3]["IE"];
                                            if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                                posCliente = contribuintes.IndexOf(exitNotes[i][3]["CNPJ"]);

                                            bool existe = false;
                                            if (clientesAll.Contains(exitNotes[i][3]["CNPJ"]))
                                                existe = true;

                                            if (!existe)
                                            {
                                                ViewBag.Erro = 2;
                                                return View(comp);
                                            }
                                        }

                                        int posUf = -1;
                                        if (exitNotes[i][1]["idDest"].Equals("2"))
                                        {
                                            var UF = exitNotes[i][3].ContainsKey("UF") ? exitNotes[i][3]["UF"] : "";

                                            for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                            {
                                                if (icmsForaDoEstado[j][0].Equals(UF))
                                                {
                                                    posUf = j;
                                                    break;
                                                }
                                            }

                                            if (posUf < 0)
                                            {
                                                List<string> uf = new List<string>();
                                                uf.Add(UF);
                                                uf.Add("0");
                                                uf.Add("0");
                                                icmsForaDoEstado.Add(uf);
                                            }

                                        }

                                        bool cfop = false;

                                        for (int k = 0; k < exitNotes[i].Count(); k++)
                                        {
                                            string cest = "";
                                            if (exitNotes[i][k].ContainsKey("CEST"))
                                                cest = exitNotes[i][k]["CEST"];

                                            if (exitNotes[i][k].ContainsKey("CFOP"))
                                            {
                                                cfop = false;
                                                if (cfopsVendaST.Contains(exitNotes[i][k]["CFOP"]) || cfopsTransfST.Contains(exitNotes[i][k]["CFOP"]))
                                                {
                                                    cfop = true;
                                                }

                                            }

                                            if (cfop)
                                            {
                                                if (exitNotes[i][k].ContainsKey("cProd"))
                                                {
                                                    if (exitNotes[i][k].ContainsKey("vProd"))
                                                    {
                                                        if (posCliente < 0)
                                                        {
                                                            vendaCfopSTNaoContribuinteNIncetivo += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                                            {
                                                                vendaCfopSTNaoContriForaDoEstadoNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                                icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            vendaCfopSTContribuintesNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                        }
                                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vProd"]);

                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vFrete"))
                                                    {
                                                        if (posCliente < 0)
                                                        {
                                                            vendaCfopSTNaoContribuinteNIncetivo += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                                            {
                                                                vendaCfopSTNaoContriForaDoEstadoNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                                icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();

                                                            }
                                                        }
                                                        else
                                                        {
                                                            vendaCfopSTContribuintesNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                        }

                                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vDesc"))
                                                    {
                                                        if (posCliente < 0)
                                                        {
                                                            vendaCfopSTNaoContribuinteNIncetivo -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                                            {
                                                                vendaCfopSTNaoContriForaDoEstadoNIncentivo -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                                icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();

                                                            }
                                                        }
                                                        else
                                                        {
                                                            vendaCfopSTContribuintesNIncentivo -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                        }

                                                        totalVendas -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vOutro"))
                                                    {
                                                        if (posCliente < 0)
                                                        {
                                                            vendaCfopSTNaoContribuinteNIncetivo += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                                            {
                                                                vendaCfopSTNaoContriForaDoEstadoNIncentivo += Convert.ToDecimal(exitNotes[i][k]["Outro"]);
                                                                icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();

                                                            }
                                                        }
                                                        else
                                                        {
                                                            vendaCfopSTContribuintesNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                        }

                                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vSeg"))
                                                    {
                                                        if (posCliente < 0)
                                                        {
                                                            vendaCfopSTNaoContribuinteNIncetivo += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                                            {
                                                                vendaCfopSTNaoContriForaDoEstadoNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                                icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();

                                                            }
                                                        }
                                                        else
                                                        {
                                                            vendaCfopSTContribuintesNIncentivo += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                        }

                                                        totalVendas += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                    }
                                                }
                                            }

                                        }
                                    }

                                    // Devolução de Compra
                                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                                    {
                                        if (exitNotes[i][1]["finNFe"] != "4" || exitNotes[i][1]["tpNF"] == "0")
                                        {
                                            exitNotes.RemoveAt(i);
                                            continue;
                                        }

                                        bool cfop = false;
                                        for (int k = 0; k < exitNotes[i].Count; k++)
                                        {
                                            if (exitNotes[i][k].ContainsKey("CFOP"))
                                            {
                                                cfop = false;
                                                if (cfopsDevoCompra.Contains(exitNotes[i][k]["CFOP"]) || cfopsDevoCompraST.Contains(exitNotes[i][k]["CFOP"]))
                                                {
                                                    cfop = true;
                                                }

                                            }

                                            if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("orig") && cfop)
                                                debitosIcms += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                    }


                                    if (imp != null)
                                    {
                                        imp.Debito = debitosIcms;
                                        imp.VendasContribuinte1 = contribuintesIncentivo;
                                        imp.VendasContribuinte2 = contribuintesIncentivoAliqM25;
                                        imp.ReceitaNormal1 = contribuintesNIncentivo;
                                        imp.ReceitaST1 = vendaCfopSTContribuintesNIncentivo;
                                        imp.VendasNContribuinte = naoContribuinteIncentivo;
                                        imp.ReceitaNormal2 = naoContribuinteNIncetivo;
                                        imp.ReceitaST2 = vendaCfopSTNaoContribuinteNIncetivo;
                                        imp.VendasNContribuinteFora = naoContriForaDoEstadoIncentivo;
                                        imp.ReceitaNormal3 = naoContriForaDoEstadoNIncentivo;
                                        imp.ReceitaST3 = vendaCfopSTNaoContriForaDoEstadoNIncentivo;
                                        imp.Icms = true;

                                        var grupoTemp = _grupoService.FindByGrupos(imp.Id);

                                        if (grupoTemp != null)
                                        {
                                            List<Model.Grupo> updateGrupos = new List<Grupo>();

                                            for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                            {

                                                decimal baseDifal = Convert.ToDecimal(icmsForaDoEstado[j][1]),
                                                        icms = (Convert.ToDecimal(comp.IcmsNContribuinteFora) * baseDifal) / 100;

                                                if (baseDifal > 0)
                                                {
                                                    var gg = grupoTemp.Where(_ => _.Uf.Equals(icmsForaDoEstado[j][0])).FirstOrDefault();

                                                    if (gg != null)
                                                    {
                                                        gg.BaseDifal = baseDifal;
                                                        gg.Icms = icms;
                                                        gg.Updated = DateTime.Now;
                                                        updateGrupos.Add(gg);
                                                    }
                                                    else
                                                    {
                                                        Model.Grupo grupo = new Model.Grupo();
                                                        grupo.Uf = icmsForaDoEstado[j][0];
                                                        grupo.BaseDifal = baseDifal;
                                                        grupo.Icms = icms;
                                                        grupo.TaxId = imp.Id;
                                                        grupo.Created = DateTime.Now;
                                                        grupo.Updated = grupo.Created;
                                                        addGrupos.Add(grupo);
                                                    }

                                                }
                                            }

                                            _grupoService.Create(addGrupos, null);
                                            _grupoService.Update(updateGrupos, null);
                                        }
                                        else
                                        {
                                            for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                            {
                                                decimal baseDifal = Convert.ToDecimal(icmsForaDoEstado[j][1]),
                                                        icms = (Convert.ToDecimal(comp.IcmsNContribuinteFora) * baseDifal) / 100;

                                                if (baseDifal > 0)
                                                {
                                                    Model.Grupo grupo = new Model.Grupo();
                                                    grupo.Uf = icmsForaDoEstado[j][0];
                                                    grupo.BaseDifal = baseDifal;
                                                    grupo.Icms = icms;
                                                    grupo.TaxId = imp.Id;
                                                    grupo.Created = DateTime.Now;
                                                    grupo.Updated = grupo.Created;
                                                    addGrupos.Add(grupo);
                                                }
                                            }

                                        }

                                    }
                                    else
                                    {
                                        tax.Debito = debitosIcms;
                                        tax.VendasContribuinte1 = contribuintesIncentivo;
                                        tax.VendasContribuinte2 = contribuintesIncentivoAliqM25;
                                        tax.ReceitaNormal1 = contribuintesNIncentivo;
                                        tax.ReceitaST1 = vendaCfopSTContribuintesNIncentivo;
                                        tax.VendasNContribuinte = naoContribuinteIncentivo;
                                        tax.ReceitaNormal2 = naoContribuinteNIncetivo;
                                        tax.ReceitaST2 = vendaCfopSTNaoContribuinteNIncetivo;
                                        tax.VendasNContribuinteFora = naoContriForaDoEstadoIncentivo;
                                        tax.ReceitaNormal3 = naoContriForaDoEstadoNIncentivo;
                                        tax.ReceitaST3 = vendaCfopSTNaoContriForaDoEstadoNIncentivo;
                                        tax.Icms = true;

                                        for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                        {
                                            decimal baseDifal = Convert.ToDecimal(icmsForaDoEstado[j][1]),
                                                    icms = (Convert.ToDecimal(comp.IcmsNContribuinteFora) * baseDifal) / 100;

                                            if (baseDifal > 0)
                                            {
                                                Model.Grupo grupo = new Model.Grupo();
                                                grupo.Uf = icmsForaDoEstado[j][0];
                                                grupo.BaseDifal = baseDifal;
                                                grupo.Icms = icms;
                                                grupo.Created = DateTime.Now;
                                                grupo.Updated = grupo.Created;
                                                addGrupos.Add(grupo);
                                            }
                                        }
                                    }
                                
                                }
                                else
                                {
                                    //  Incentivo Indústria

                                    List<List<string>> percentuaisIncentivado = new List<List<string>>();
                                    List<List<string>> percentuaisNIncentivado = new List<List<string>>();

                                    exitNotes = importXml.NFeAll(directoryNfeExit);

                                    decimal vendasIncentivada = 0, vendasNIncentivada = 0, debitoIncetivo = 0, debitoNIncentivo = 0;

                                    // Saida
                                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                                    {
                                        if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                                        {
                                            exitNotes.RemoveAt(i);
                                            continue;
                                        }

                                        if (exitNotes[i][1].ContainsKey("dhEmi"))
                                        {
                                            prodsTemp = _productIncentivoService.FindByDate(prodsAll, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));

                                            codeProdIncentivado = prodsTemp.Where(_ => _.TypeTaxation.Equals("Incentivado/Normal") || _.TypeTaxation.Equals("Incentivado/ST")).Select(_ => _.Code).ToList();
                                            codeProdST = prodsTemp.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Code).ToList();
                                            codeProdIsento = prodsTemp.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Code).ToList();

                                            cestIncentivado = prodsTemp.Where(_ => _.TypeTaxation.Equals("Incentivado/Normal") || _.TypeTaxation.Equals("Incentivado/ST")).Select(_ => _.Cest).ToList();
                                            cestST = prodsTemp.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Cest).ToList();
                                            cestIsento = prodsTemp.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Cest).ToList();
                                        }

                                        int status = 3;
                                        decimal percent = 0;
                                        bool cfop = false;
                                        decimal vProd = 0;

                                        for (int k = 0; k < exitNotes[i].Count(); k++)
                                        {
                                            if (exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                status = 1;
                                                percent = 0;

                                                string cest = "";
                                                if (exitNotes[i][k].ContainsKey("CEST"))
                                                    cest = exitNotes[i][k]["CEST"];

                                                if (exitNotes[i][k].ContainsKey("CFOP"))
                                                {
                                                    cfop = false;

                                                    if (cfopsVenda.Contains(exitNotes[i][k]["CFOP"]) || cfopsVendaST.Contains(exitNotes[i][k]["CFOP"]) ||
                                                        cfopsBoniVenda.Contains(exitNotes[i][k]["CFOP"]) || cfopsDevoCompra.Contains(exitNotes[i][k]["CFOP"]) ||
                                                        cfopsDevoCompraST.Contains(exitNotes[i][k]["CFOP"]) || cfopsTransf.Contains(exitNotes[i][k]["CFOP"]) ||
                                                        cfopsTransfST.Contains(exitNotes[i][k]["CFOP"]) || cfopsPerda.Contains(exitNotes[i][k]["CFOP"]))
                                                    {
                                                        cfop = true;
                                                    }

                                                }

                                                if (cfop == true)
                                                {
                                                    if (exitNotes[i][k].ContainsKey("vProd"))
                                                        vProd = Convert.ToDecimal(exitNotes[i][k]["vProd"]);

                                                    if (exitNotes[i][k].ContainsKey("vFrete"))
                                                        vProd += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);

                                                    if (exitNotes[i][k].ContainsKey("vDesc"))
                                                        vProd -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);

                                                    if (exitNotes[i][k].ContainsKey("vOutro"))
                                                        vProd += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);

                                                    if (exitNotes[i][k].ContainsKey("vSeg"))
                                                        vProd += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);

                                                    if (codeProd.Contains(exitNotes[i][k]["cProd"]) && cestProd.Contains(cest))
                                                    {
                                                        if (codeProdIncentivado.Contains(exitNotes[i][k]["cProd"]) && cestIncentivado.Contains(cest))
                                                        {
                                                            status = 1;
                                                            var percentualIncentivado = Convert.ToDecimal(prodsTemp.Where(_ => _.Code.Equals(exitNotes[i][k]["cProd"])).ToList().Select(_ => _.Percentual).FirstOrDefault());
                                                            percent = percentualIncentivado;

                                                            if (percent < 100)
                                                            {
                                                                var percentNIncentivado = 100 - percent;

                                                                vendasIncentivada += (vProd * percent) / 100;
                                                                vendasNIncentivada += (vProd * percentNIncentivado) / 100;
                                                            }
                                                            else
                                                            {
                                                                vendasIncentivada += vProd;
                                                            }
                                                        }
                                                        else if (codeProdST.Contains(exitNotes[i][k]["cProd"]) && cestST.Contains(cest))
                                                        {
                                                            status = 2;
                                                            percent = 0;
                                                            vendasNIncentivada += vProd;
                                                        }
                                                        else if (codeProdIsento.Contains(exitNotes[i][k]["cProd"]) && cestIsento.Contains(cest))
                                                        {
                                                            status = 3;
                                                            percent = 0;
                                                            vendasNIncentivada += vProd;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ViewBag.Erro = 3;
                                                        return View(comp);
                                                    }
                                                }

                                            }

                                            if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                            {
                                                if (status == 1)
                                                {
                                                    if (percent < 100)
                                                    {
                                                        var percentNIncentivado = 100 - percent;

                                                        int pos = -1;
                                                        for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                            {
                                                                pos = j;
                                                                break;
                                                            }
                                                        }

                                                        if (pos < 0)
                                                        {
                                                            List<string> percIncentivado = new List<string>();
                                                            percIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100).ToString());
                                                            percIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                            percIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percent) / 100).ToString());
                                                            percentuaisIncentivado.Add(percIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100)).ToString();
                                                            percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percent) / 100)).ToString();
                                                        }

                                                        debitoIncetivo += ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percent) / 100);

                                                        int indice = -1;
                                                        for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                            {
                                                                indice = j;
                                                                break;
                                                            }
                                                        }

                                                        if (indice < 0)
                                                        {
                                                            List<string> percNIncentivado = new List<string>();
                                                            percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percentNIncentivado) / 100).ToString());
                                                            percNIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                            percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100).ToString());
                                                            percentuaisNIncentivado.Add(percNIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percentNIncentivado) / 100)).ToString();
                                                            percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100)).ToString();
                                                        }

                                                        debitoNIncentivo += ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100);
                                                    }
                                                    else
                                                    {
                                                        int pos = -1;
                                                        for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                            {
                                                                pos = j;
                                                                break;
                                                            }
                                                        }

                                                        if (pos < 0)
                                                        {
                                                            List<string> percIncentivado = new List<string>();
                                                            percIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                            percIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                            percIncentivado.Add(exitNotes[i][k]["vICMS"]);
                                                            percentuaisIncentivado.Add(percIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                            percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + (Convert.ToDecimal(exitNotes[i][k]["vICMS"]))).ToString();
                                                        }

                                                        debitoIncetivo += (Convert.ToDecimal(exitNotes[i][k]["vICMS"]));
                                                    }
                                                }
                                                else if (status == 2)
                                                {
                                                    int pos = -1;
                                                    for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                    {
                                                        if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                        {
                                                            pos = j;
                                                            break;
                                                        }
                                                    }

                                                    if (pos < 0)
                                                    {
                                                        List<string> percNIncentivado = new List<string>();
                                                        percNIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                        percNIncentivado.Add(exitNotes[i][k]["pICMS"]);
                                                        percNIncentivado.Add(exitNotes[i][k]["vICMS"]);
                                                        percentuaisNIncentivado.Add(percNIncentivado);
                                                    }
                                                    else
                                                    {
                                                        percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                        percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vICMS"])).ToString();
                                                    }

                                                    debitoNIncentivo += (Convert.ToDecimal(exitNotes[i][k]["vICMS"]));
                                                }
                                            }

                                            if (exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                            {
                                                if (status == 1)
                                                {
                                                    if (percent < 100)
                                                    {
                                                        var percentNIncentivado = 100 - percent;

                                                        int pos = -1;
                                                        for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                            {
                                                                pos = j;
                                                                break;
                                                            }
                                                        }

                                                        if (pos < 0)
                                                        {
                                                            List<string> percIncentivado = new List<string>();
                                                            percIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percent) / 100).ToString());
                                                            percIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                            percIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vFCP"])) * percent) / 100).ToString());
                                                            percentuaisIncentivado.Add(percIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + ((Convert.ToDecimal(exitNotes[i][k]["vBC"]) * percent) / 100)).ToString();
                                                            percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percent) / 100)).ToString();
                                                        }

                                                       
                                                        debitoIncetivo += ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percent) / 100);

                                                        int indice = -1;
                                                        for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                            {
                                                                indice = j;
                                                                break;
                                                            }
                                                        }

                                                        if (indice < 0)
                                                        {
                                                            List<string> percNIncentivado = new List<string>();
                                                            percNIncentivado.Add((((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percentNIncentivado) / 100).ToString());
                                                            percNIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                            percNIncentivado.Add(((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100).ToString());
                                                            percentuaisNIncentivado.Add(percNIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + (((Convert.ToDecimal(exitNotes[i][k]["vBC"])) * percentNIncentivado) / 100)).ToString();
                                                            percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100)).ToString();
                                                        }

                                                        debitoNIncentivo += ((Convert.ToDecimal(exitNotes[i][k]["vFCP"]) * percentNIncentivado) / 100);
                                                    }
                                                    else
                                                    {
                                                        int pos = -1;
                                                        for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                            {
                                                                pos = j;
                                                                break;
                                                            }
                                                        }

                                                        if (pos < 0)
                                                        {
                                                            List<string> percIncentivado = new List<string>();
                                                            percIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                            percIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                            percIncentivado.Add(exitNotes[i][k]["vFCP"]);
                                                            percentuaisIncentivado.Add(percIncentivado);
                                                        }
                                                        else
                                                        {
                                                            percentuaisIncentivado[pos][0] = (Convert.ToDecimal(percentuaisIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                            percentuaisIncentivado[pos][2] = (Convert.ToDecimal(percentuaisIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vFCP"])).ToString();
                                                        }

                                                        debitoIncetivo += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                                    }
                                                }
                                                else if (status == 2)
                                                {
                                                    int pos = -1;
                                                    for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                    {
                                                        if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pFCP"]))
                                                        {
                                                            pos = j;
                                                            break;
                                                        }
                                                    }

                                                    if (pos < 0)
                                                    {
                                                        List<string> percNIncentivado = new List<string>();
                                                        percNIncentivado.Add(exitNotes[i][k]["vBC"]);
                                                        percNIncentivado.Add(exitNotes[i][k]["pFCP"]);
                                                        percNIncentivado.Add(exitNotes[i][k]["vFCP"]);

                                                        percentuaisNIncentivado.Add(percNIncentivado);
                                                    }
                                                    else
                                                    {
                                                        percentuaisNIncentivado[pos][0] = (Convert.ToDecimal(percentuaisNIncentivado[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                                        percentuaisNIncentivado[pos][2] = (Convert.ToDecimal(percentuaisNIncentivado[pos][2]) + Convert.ToDecimal(exitNotes[i][k]["vFCP"])).ToString();
                                                    }

                                                    debitoNIncentivo += (Convert.ToDecimal(exitNotes[i][k]["vFCP"]));
                                                }
                                            }

                                        }
                                    }

                                    List<List<string>> valoresIncentivo = new List<List<string>>();

                                    for (int i = 0; i < percentuaisIncentivado.Count(); i++)
                                    {
                                        List<string> percentual = new List<string>();
                                        percentual.Add(percentuaisIncentivado[i][0]);
                                        percentual.Add(percentuaisIncentivado[i][1]);
                                        percentual.Add(percentuaisIncentivado[i][2]);
                                        valoresIncentivo.Add(percentual);
                                    }

                                    List<List<string>> valoresNIncentivo = new List<List<string>>();

                                    for (int i = 0; i < percentuaisNIncentivado.Count(); i++)
                                    {
                                        List<string> percentual = new List<string>();
                                        percentual.Add(percentuaisNIncentivado[i][0]);
                                        percentual.Add(percentuaisNIncentivado[i][1]);
                                        percentual.Add(percentuaisNIncentivado[i][2]);
                                        valoresNIncentivo.Add(percentual);
                                    }

                                    if (valoresIncentivo.Count() < valoresNIncentivo.Count())
                                    {
                                        int diferenca = valoresNIncentivo.Count() - valoresIncentivo.Count();
                                        for (int i = 0; i < diferenca; i++)
                                        {
                                            List<string> percentual = new List<string>();
                                            percentual.Add("0.00");
                                            percentual.Add("0.00");
                                            percentual.Add("0.00");
                                            valoresIncentivo.Add(percentual);
                                        }
                                    }
                                    else if (valoresIncentivo.Count() > valoresNIncentivo.Count())
                                    {
                                        int diferenca = valoresIncentivo.Count() - valoresNIncentivo.Count();
                                        for (int i = 0; i < diferenca; i++)
                                        {
                                            List<string> percentual = new List<string>();
                                            percentual.Add("0.00");
                                            percentual.Add("0.00");
                                            percentual.Add("0.00");
                                            valoresNIncentivo.Add(percentual);
                                        }
                                    }

                                    if (imp != null)
                                    {
                                        imp.VendasIncentivada = vendasIncentivada;
                                        imp.VendasNIncentivada = vendasNIncentivada;
                                        imp.Icms = true;

                                        var grupoTemp = _grupoService.FindByGrupos(imp.Id);

                                        List<Model.Grupo> updateGrupos = new List<Model.Grupo>();

                                        if(grupoTemp != null)
                                        {
                                            for (int i = 0; i < valoresIncentivo.Count(); i++)
                                            {
                                                var gg = grupoTemp.Where(_ => _.Percentual.Equals(Convert.ToDecimal(valoresIncentivo[i][1]))).FirstOrDefault();

                                                if (gg != null)
                                                {
                                                    gg.BaseCalculo = Convert.ToDecimal(valoresIncentivo[i][0]);
                                                    gg.Percentual = Convert.ToDecimal(valoresIncentivo[i][1]);
                                                    gg.Icms = Convert.ToDecimal(valoresIncentivo[i][2]);
                                                    gg.BaseCalculoNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][0]);
                                                    gg.PercentualNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][1]);
                                                    gg.IcmsNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][2]);
                                                    gg.Updated = DateTime.Now;
                                                    updateGrupos.Add(gg);
                                                }
                                                else
                                                {
                                                    Model.Grupo grupo = new Model.Grupo();
                                                    grupo.BaseCalculo = Convert.ToDecimal(valoresIncentivo[i][0]);
                                                    grupo.Percentual = Convert.ToDecimal(valoresIncentivo[i][1]);
                                                    grupo.Icms = Convert.ToDecimal(valoresIncentivo[i][2]);
                                                    grupo.BaseCalculoNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][0]);
                                                    grupo.PercentualNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][1]);
                                                    grupo.IcmsNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][2]);
                                                    grupo.TaxId = imp.Id;
                                                    grupo.Created = DateTime.Now;
                                                    grupo.Updated = grupo.Created;

                                                    if (grupo.BaseCalculo > 0 || grupo.BaseCalculoNIncentivo > 0)
                                                        addGrupos.Add(grupo);
                                                }
                                            }
                                            _grupoService.Create(addGrupos, GetLog(OccorenceLog.Create));
                                            _grupoService.Update(updateGrupos, GetLog(OccorenceLog.Update));
                                        }
                                        else
                                        {
                                            for (int i = 0; i < valoresIncentivo.Count(); i++)
                                            {
                                                Model.Grupo grupo = new Model.Grupo();
                                                grupo.BaseCalculo = Convert.ToDecimal(valoresIncentivo[i][0]);
                                                grupo.Percentual = Convert.ToDecimal(valoresIncentivo[i][1]);
                                                grupo.Icms = Convert.ToDecimal(valoresIncentivo[i][2]);
                                                grupo.BaseCalculoNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][0]);
                                                grupo.PercentualNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][1]);
                                                grupo.IcmsNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][2]);
                                                grupo.TaxId = imp.Id;
                                                grupo.Created = DateTime.Now;
                                                grupo.Updated = grupo.Created;

                                                if (grupo.BaseCalculo > 0 || grupo.BaseCalculoNIncentivo > 0)
                                                    addGrupos.Add(grupo);
                                            }
                                            _grupoService.Create(addGrupos, GetLog(OccorenceLog.Create));
                                        }
                                    }
                                    else
                                    {
                                        tax.VendasIncentivada = vendasIncentivada;
                                        tax.VendasNIncentivada = vendasNIncentivada;
                                        tax.Icms = true;

                                        for (int i = 0; i < valoresIncentivo.Count(); i++)
                                        {
                                            Model.Grupo grupo = new Model.Grupo();
                                            grupo.BaseCalculo = Convert.ToDecimal(valoresIncentivo[i][0]);
                                            grupo.Percentual = Convert.ToDecimal(valoresIncentivo[i][1]);
                                            grupo.Icms = Convert.ToDecimal(valoresIncentivo[i][2]);
                                            grupo.BaseCalculoNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][0]);
                                            grupo.PercentualNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][1]);
                                            grupo.IcmsNIncentivo = Convert.ToDecimal(valoresNIncentivo[i][2]);
                                            grupo.Created = DateTime.Now;
                                            grupo.Updated = grupo.Created;
                                            addGrupos.Add(grupo);
                                        }
                                    }

                                }
                            }
                            else if (comp.AnnexId.Equals((long)3))
                            {
                                //  Incentivo de Medicamentos

                                var clientesAll = _clientService.FindByCompany(companyid);
                                var nContribuintes = clientesAll.Where(_ => _.TypeClientId.Equals((long)2)).Select(_ => _.Document).ToList();
                                var suspensions = _suspensionService.FindByCompany(companyid);

                                cfopsVenda.AddRange(cfopsBoniVenda);
                                cfopsVenda.AddRange(cfopsVendaST);

                                if (comp.SectionId.Equals((long)2))
                                {
                                    exitNotes = importXml.NFeAll(directoryNfeExit);

                                    decimal vendasInternasElencadas = 0, vendasInterestadualElencadas = 0, vendasInternasDeselencadas = 0, vendasInterestadualDeselencadas = 0,
                                        internasElencadas = 0, interestadualElencadas = 0, internasElencadasPortaria = 0, interestadualElencadasPortaria = 0,
                                        internasDeselencadas = 0, interestadualDeselencadas = 0, internasDeselencadasPortaria = 0, interestadualDeselencadasPortaria = 0,
                                        suspensao = 0, vendasClienteCredenciado = 0, vendas = 0;

                                    //  Vendas
                                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                                    {
                                        if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document) || exitNotes[i][1]["finNFe"] == "4")
                                        {
                                            exitNotes.RemoveAt(i);
                                            continue;
                                        }

                                        bool clenteCredenciado = false, ncm = false, cfop = false, suspenso = false;

                                        if (exitNotes[i][1].ContainsKey("dhEmi"))
                                        {
                                            foreach (var suspension in suspensions)
                                            {
                                                if (Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) >= Convert.ToDateTime(suspension.DateStart) && Convert.ToDateTime(exitNotes[i][1]["dhEmi"]) < Convert.ToDateTime(suspension.DateEnd))
                                                {
                                                    suspenso = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (exitNotes[i][3].ContainsKey("CNPJ"))
                                        {
                                            if (nContribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                                clenteCredenciado = true;

                                            bool existe = false;

                                            if (clientesAll.Select(_ => _.Document).Contains(exitNotes[i][3]["CNPJ"]))
                                                existe = true;

                                            if (!existe)
                                            {
                                                ViewBag.Erro = 2;
                                                return View(comp);
                                            }
                                        }

                                        for (int j = 0; j < exitNotes[i].Count; j++)
                                        {
                                            if (exitNotes[i][j].ContainsKey("NCM"))
                                                ncm = _ncmConvenioService.FindByNcmAnnex(ncmsConvenio, exitNotes[i][j]["NCM"].ToString());

                                            if (exitNotes[i][j].ContainsKey("CFOP"))
                                            {
                                                cfop = false;
                                                if (cfopsVenda.Contains(exitNotes[i][j]["CFOP"]) || cfopsVendaST.Contains(exitNotes[i][j]["CFOP"]) ||
                                                    cfopsBoniVenda.Contains(exitNotes[i][j]["CFOP"]))
                                                {
                                                    cfop = true;
                                                }
                                            }

                                            if (cfop)
                                            {
                                                if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    vendas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                    if (clenteCredenciado)
                                                        vendasClienteCredenciado += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                        vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    else
                                                        vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        internasElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        if (ncm)
                                                            internasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }
                                                    else
                                                    {
                                                        interestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        if (ncm)
                                                            interestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }
                                                    if (suspenso)
                                                        suspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                }

                                                if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    vendas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                                    if (clenteCredenciado)
                                                        vendasClienteCredenciado += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                        vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    else
                                                        vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        internasElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (ncm)
                                                            internasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }
                                                    else
                                                    {
                                                        interestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (ncm)
                                                            interestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }

                                                    if (suspenso)
                                                        suspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);


                                                }

                                                if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    vendas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    if (clenteCredenciado)
                                                        vendasClienteCredenciado -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                        vendasInternasElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    else
                                                        vendasInterestadualElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        internasElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (ncm)
                                                            internasElencadasPortaria -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }
                                                    else
                                                    {
                                                        interestadualElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (ncm)
                                                            interestadualElencadasPortaria -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }

                                                    if (suspenso)
                                                        suspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                }

                                                if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    vendas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    if (clenteCredenciado)
                                                        vendasClienteCredenciado += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                        vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    else
                                                        vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        internasElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (ncm)
                                                            internasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }
                                                    else
                                                    {
                                                        interestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (ncm)
                                                            interestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }

                                                    if (suspenso)
                                                        suspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                }

                                                if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    vendas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);

                                                    if (clenteCredenciado)
                                                        vendasClienteCredenciado += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                        vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    else
                                                        vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        internasElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (ncm)
                                                            internasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }
                                                    else
                                                    {
                                                        interestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (ncm)
                                                            interestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }

                                                    if (suspenso)
                                                        suspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                }
                                            }
                                        }

                                    }

                                    if (imp != null)
                                    {
                                        imp.VendasClientes = vendasClienteCredenciado;
                                        imp.VendasInternas1 = vendasInternasElencadas;
                                        imp.VendasInterestadual1 = vendasInterestadualElencadas;
                                        imp.SaidaInterna1 = internasElencadas;
                                        imp.SaidaInterestadual1 = interestadualElencadas;
                                        imp.SaidaPortInterna1 = internasElencadasPortaria;
                                        imp.SaidaPortInterestadual1 = interestadualElencadasPortaria;

                                        imp.VendasInternas2 = vendasInternasDeselencadas;
                                        imp.VendasInterestadual2 = vendasInterestadualDeselencadas;
                                        imp.SaidaInterna2 = internasDeselencadas;
                                        imp.SaidaInterestadual2 = interestadualDeselencadas;
                                        imp.SaidaPortInterna2 = internasDeselencadasPortaria;
                                        imp.SaidaPortInterestadual2 = interestadualDeselencadasPortaria;
                                        imp.Suspensao = suspensao;
                                        imp.Icms = true;
                                    }
                                    else
                                    {
                                        tax.VendasClientes = vendasClienteCredenciado;
                                        tax.VendasInternas1 = vendasInternasElencadas;
                                        tax.VendasInterestadual1 = vendasInterestadualElencadas;
                                        tax.SaidaInterna1 = internasElencadas;
                                        tax.SaidaInterestadual1 = interestadualElencadas;
                                        tax.SaidaPortInterna1 = internasElencadasPortaria;
                                        tax.SaidaPortInterestadual1 = interestadualElencadasPortaria;

                                        tax.VendasInternas2 = vendasInternasDeselencadas;
                                        tax.VendasInterestadual2 = vendasInterestadualDeselencadas;
                                        tax.SaidaInterna2 = internasDeselencadas;
                                        tax.SaidaInterestadual2 = interestadualDeselencadas;
                                        tax.SaidaPortInterna2 = internasDeselencadasPortaria;
                                        tax.SaidaPortInterestadual2 = interestadualDeselencadasPortaria;
                                        tax.Suspensao = suspensao;
                                        tax.Icms = true;
                                    }
                                }
                            }
                            else if (comp.AnnexId.Equals((long)4))
                            {

                            }
                        }
                    }
                }
                else if (imposto.Equals("pisCofins"))
                {
                    var companies = _companyService.FindByCompanies(comp.Document);

                    List<TaxationNcm> ncmsCompany = new List<TaxationNcm>();

                    ncmsCompany = _taxationNcmService.FindByCompany(comp.Document);

                    if (type.Equals("sped") && comp.Sped)
                    {
                        // Empresa Lucro Real
                        decimal compraLR = 0, devolucaoLR = 0;

                        // Empresa Lucro Presumido
                        decimal devolucaoComercio = 0, devolucaoServico = 0, devolucaoPetroleo = 0, devolucaoTransporte = 0, devolucaoNormal = 0, bonificacao = 0;

                        //  Empresa do Simples
                        decimal devoNormalNormal = 0, devoNormalMono = 0, devoNormalST = 0, devoNormalAliqZero = 0, devoNormalIsento = 0, devoNormalOutras = 0,
                            devoSTNormal = 0, devoSTMono = 0, devoSTST = 0, devoSTAliqZero = 0, devoSTIsento = 0, devoSTOutras = 0;

                        foreach (var cc in caminhos)
                        {
                            if (comp.CountingTypeId.Equals((long)1))
                            {
                                // Empresa Lucro Real
                                var entradas = importSped.NFeLReal(cc, cfopsCompra, cfopsBoniCompra, cfopsCompraST, cfopsTransf, cfopsTransfST, cfopsDevoVenda, cfopsDevoVendaST, ncmsCompany, comp);
                                compraLR += entradas[0];
                                devolucaoLR += entradas[1];

                            }
                            else if (comp.CountingTypeId.Equals((long)2))
                            {
                                // Empresa Lucro Presumido
                                var lucroPresumido = importSped.NFeLPresumido(cc, cfopsDevoVenda, cfopsDevoVendaST, cfopsBoniCompra, ncmsCompany, comp);
                                devolucaoPetroleo += lucroPresumido[0];
                                devolucaoComercio += lucroPresumido[1];
                                devolucaoTransporte += lucroPresumido[2];
                                devolucaoServico += lucroPresumido[3];
                                devolucaoNormal += lucroPresumido[4];
                                bonificacao += lucroPresumido[5];

                            }
                            else if (comp.CountingTypeId.Equals((long)3))
                            {
                                // Empresa do Simples
                                cfopsDevoVenda.AddRange(cfopsDevoVendaST);

                                var sped = importSped.NFeDevolution(cc, cfopsDevoVenda, ncmsCompany, comp);

                                devoNormalNormal += sped[0];
                                devoSTNormal += sped[1];
                                devoSTMono += sped[2];
                                devoNormalMono += sped[3];
                                devoNormalST += sped[4];
                                devoNormalAliqZero += sped[5];
                                devoNormalIsento += sped[6];
                                devoSTST += sped[7];
                                devoSTAliqZero += sped[8];
                                devoSTIsento += sped[9];
                                devoNormalOutras += sped[10];
                                devoSTOutras += sped[11];
                            }
                        }

                        if (imp != null)
                        {
                            imp.PisCofins = true;

                            //  Empresa Lucro Real
                            imp.Compra = compraLR;
                            imp.DevolucaoVenda = devolucaoLR;

                            // Empresa Lucro Presumido
                            imp.Devolucao1 = devolucaoPetroleo;
                            imp.Devolucao2 = devolucaoComercio;
                            imp.Devolucao3 = devolucaoTransporte;
                            imp.Devolucao4 = devolucaoServico;
                            imp.DevolucaoNormal = devolucaoNormal;
                            imp.Bonificacao = bonificacao;

                            //  Empresa do Simples
                            imp.DevoNormalNormal = devoNormalNormal;
                            imp.DevoSTNormal = devoSTNormal;
                            imp.DevoNormalMonofasico = devoNormalMono;
                            imp.DevoSTMonofasico = devoSTMono;
                            imp.DevoNormalST = devoNormalST;
                            imp.DevoNormalAliqZero = devoNormalAliqZero;
                            imp.DevoNormalIsento = devoNormalIsento;
                            imp.DevoSTST = devoSTST;
                            imp.DevoSTAliqZero = devoSTAliqZero;
                            imp.DevoSTIsento = devoSTIsento;
                            imp.DevoSTOutras = devoSTOutras;
                            imp.DevoNormalOutras = devoNormalOutras;
                        }
                        else
                        {
                            tax.PisCofins = true;

                            //  Empresa Lucro Real
                            tax.Compra = compraLR;
                            tax.DevolucaoVenda = devolucaoLR;

                            // Empresa Lucro Presumido
                            tax.Devolucao1 = devolucaoPetroleo;
                            tax.Devolucao2 = devolucaoComercio;
                            tax.Devolucao3 = devolucaoTransporte;
                            tax.Devolucao4 = devolucaoServico;
                            tax.DevolucaoNormal = devolucaoNormal;
                            tax.Bonificacao = bonificacao;

                            //  Empresa do Simples
                            tax.DevoNormalNormal = devoNormalNormal;
                            tax.DevoSTNormal = devoSTNormal;
                            tax.DevoNormalMonofasico = devoNormalMono;
                            tax.DevoSTMonofasico = devoSTMono;
                            tax.DevoNormalST = devoNormalST;
                            tax.DevoNormalAliqZero = devoNormalAliqZero;
                            tax.DevoNormalIsento = devoNormalIsento;
                            tax.DevoSTST = devoSTST;
                            tax.DevoSTAliqZero = devoSTAliqZero;
                            tax.DevoSTIsento = devoSTIsento;
                            tax.DevoSTOutras = devoSTOutras;
                            tax.DevoNormalOutras = devoNormalOutras;
                        }
                    }
                    else if (type.Equals("xmlS") || type.Equals("xmlE"))
                    {
                        List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();

                        var codeProdAll = ncmsCompany.Select(_ => _.CodeProduct)
                            .ToList();
                        var ncmAll = ncmsCompany.Select(_ => _.Ncm.Code)
                            .ToList();

                        var codeProd1 = ncmsCompany.Where(_ => _.TypeNcmId.Equals((long)1))
                            .Select(_ => _.CodeProduct)
                            .ToList();
                        var codeProd2 = ncmsCompany.Where(_ => _.TypeNcmId.Equals((long)2))
                            .Select(_ => _.CodeProduct)
                            .ToList();
                        var codeProd3 = ncmsCompany.Where(_ => _.TypeNcmId.Equals((long)3))
                            .Select(_ => _.CodeProduct)
                            .ToList();
                        var codeProd4 = ncmsCompany.Where(_ => _.TypeNcmId.Equals((long)4))
                            .Select(_ => _.CodeProduct)
                            .ToList();

                        var ncm1 = ncmsCompany.Where(_ => _.TypeNcmId.Equals((long)1))
                            .Select(_ => _.Ncm.Code)
                            .ToList();
                        var ncm2 = ncmsCompany.Where(_ => _.TypeNcmId.Equals((long)2))
                            .Select(_ => _.Ncm.Code)
                            .ToList();
                        var ncm3 = ncmsCompany.Where(_ => _.TypeNcmId.Equals((long)3))
                            .Select(_ => _.Ncm.Code)
                            .ToList();
                        var ncm4 = ncmsCompany.Where(_ => _.TypeNcmId.Equals((long)4))
                            .Select(_ => _.Ncm.Code)
                            .ToList();

                        //  Empresa Lucro Real
                        decimal vendaLR = 0, devolucaoLR = 0;

                        //  Empresa Lucro Presumido
                        decimal receitaComercio = 0, devolucaoComercio = 0, receitaServico = 0, devolucaoServico = 0, receitaPetroleo = 0, devolucaoPetroleo = 0,
                            receitaTransporte = 0, devolucaoTransporte = 0, receitaMono = 0, devolucaoNormal = 0;

                        //  Empresa do Simples
                        decimal vendasNormalNormal = 0, vendasNormalMono = 0, vendasNormalST = 0, vendasNormalAliqZero = 0, vendasNormalIsento = 0, vendasSTNormal = 0, 
                            vendasSTMono = 0, vendasSTST = 0, vendasSTAliqZero = 0, vendasSTIsento = 0, vendasNormalOutras = 0, vendasSTOutras = 0;

                        decimal devoNormalNormal = 0, devoNormalMono = 0, devoNormalST = 0, devoNormalAliqZero = 0, devoNormalIsento = 0, devoNormalOutras = 0,
                            devoSTNormal = 0, devoSTMono = 0, devoSTST = 0, devoSTAliqZero = 0, devoSTIsento = 0, devoSTOutras = 0;

                        //  Importação dos Dados
                        foreach (var c in companies)
                        {

                            if (type.Equals("xmlE"))
                                directoryNfeExit = importDir.SaidaEmpresa(c, NfeExit.Value, year, month);
                            else
                                directoryNfeExit = importDir.SaidaSefaz(c, NfeExit.Value, year, month);

                            directoryNfeEntry = importDir.Entrada(c, NfeExit.Value, year, month);

                            exitNotes = importXml.NFeAll(directoryNfeExit);

                            if (!c.Sped)
                                entryNotes = importXml.NFeAll(directoryNfeExit);

                            if (comp.CountingTypeId.Equals((long)1))
                            {
                                //  Empresa Lucro Real

                                // Receitas
                                for (int i = exitNotes.Count - 1; i >= 0; i--)
                                {
                                    if (!exitNotes[i][2]["CNPJ"].Equals(c.Document))
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }


                                    if (exitNotes[i][1].ContainsKey("dhEmi"))
                                        ncmsTaxation = _taxationNcmService.FindAllInDate(ncmsCompany, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));

                                    bool cfop = false;
                                    string NCM = "", cProd = "";

                                    for (int j = 0; j < exitNotes[i].Count; j++)
                                    {
                                        if (exitNotes[i][j].ContainsKey("CFOP"))
                                        {
                                            cfop = false;

                                            if (cfopsVenda.Contains(exitNotes[i][j]["CFOP"]) || cfopsVendaST.Contains(exitNotes[i][j]["CFOP"]) ||
                                                cfopsTransf.Contains(exitNotes[i][j]["CFOP"]) || cfopsTransfST.Contains(exitNotes[i][j]["CFOP"]) ||
                                                cfopsBoniVenda.Contains(exitNotes[i][j]["CFOP"]))
                                            {
                                                cfop = true;
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("NCM") && cfop && exitNotes[i][1]["finNFe"] != "4")
                                        {
                                            NCM = exitNotes[i][j]["NCM"];
                                            cProd = exitNotes[i][j]["cProd"];


                                            if (comp.Taxation == "Produto")
                                            {
                                                if (codeProdAll.Contains(cProd) && ncmAll.Contains(NCM))
                                                {
                                                    var ehMono = ncmsTaxation.Where(_ => _.CodeProduct.Equals(cProd) && _.Ncm.Code.Equals(NCM) &&
                                                                                    (_.TaxationTypeNcmId.Equals((long)2) || _.TaxationTypeNcmId.Equals((long)3) || _.TaxationTypeNcmId.Equals((long)4) ||
                                                                                    _.TaxationTypeNcmId.Equals((long)5)))
                                                                            .FirstOrDefault();

                                                    if (exitNotes[i][j].ContainsKey("vProd"))
                                                    {
                                                        if (ehMono == null)
                                                            vendaLR += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vFrete"))
                                                    {
                                                        if (ehMono == null)
                                                            vendaLR += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vDesc"))
                                                    {
                                                        if (ehMono == null)
                                                            vendaLR -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vOutro"))
                                                    {
                                                        if (ehMono == null)
                                                            vendaLR += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vSeg"))
                                                    {
                                                        if (ehMono == null)
                                                            vendaLR += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }
                                                }
                                                else
                                                {
                                                    ViewBag.Erro = 4;
                                                    return View(comp);
                                                }
                                            }
                                            else
                                            {
                                                if (ncmAll.Contains(NCM))
                                                {
                                                    var ehMono = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(NCM) && (_.TaxationTypeNcmId.Equals((long)2) ||
                                                                                        _.TaxationTypeNcmId.Equals((long)3) || _.TaxationTypeNcmId.Equals((long)4) || _.TaxationTypeNcmId.Equals((long)5)))
                                                                             .FirstOrDefault();

                                                    if (exitNotes[i][j].ContainsKey("vProd"))
                                                    {
                                                        if (ehMono == null)
                                                            vendaLR += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vFrete"))
                                                    {
                                                        if (ehMono == null)
                                                            vendaLR += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vDesc"))
                                                    {
                                                        if (ehMono == null)
                                                            vendaLR -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vOutro"))
                                                    {
                                                        if (ehMono == null)
                                                            vendaLR += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vSeg"))
                                                    {
                                                        if (ehMono == null)
                                                            vendaLR += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }

                                                }
                                                else
                                                {
                                                    ViewBag.Erro = 4;
                                                    return View(comp);
                                                }
                                            }
                                        }
                                    }
                                }

                                // Devoluções
                                for (int i = exitNotes.Count - 1; i >= 0; i--)
                                {
                                    if (exitNotes[i][1]["finNFe"] != "4")
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }

                                    if (exitNotes[i][1].ContainsKey("dhEmi"))
                                        ncmsTaxation = _taxationNcmService.FindAllInDate(ncmsCompany, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));

                                    bool cfop = false;
                                    string NCM = "", cProd = "";

                                    for (int j = 0; j < exitNotes[i].Count; j++)
                                    {
                                        if (exitNotes[i][j].ContainsKey("CFOP"))
                                        {
                                            cfop = false;

                                            if (cfopsDevoCompra.Contains(exitNotes[i][j]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }

                                        if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("NCM") && cfop)
                                        {
                                            NCM = exitNotes[i][j]["NCM"];
                                            cProd = exitNotes[i][j]["cProd"];

                                            if (comp.Taxation == "Produto")
                                            {
                                                if (codeProdAll.Contains(cProd) && ncmAll.Contains(NCM))
                                                {
                                                    var ehMono = ncmsTaxation.Where(_ => _.CodeProduct.Equals(cProd) && _.Ncm.Code.Equals(NCM) &&
                                                                                    (_.TaxationTypeNcmId.Equals((long)2) || _.TaxationTypeNcmId.Equals((long)3) || _.TaxationTypeNcmId.Equals((long)4) ||
                                                                                    _.TaxationTypeNcmId.Equals((long)5)))
                                                                            .FirstOrDefault();

                                                    if (exitNotes[i][j].ContainsKey("vProd"))
                                                    {
                                                        if (ehMono == null)
                                                            devolucaoLR += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vFrete"))
                                                    {
                                                        if (ehMono == null)
                                                            devolucaoLR += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vDesc"))
                                                    {
                                                        if (ehMono == null)
                                                            devolucaoLR -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vOutro"))
                                                    {
                                                        if (ehMono == null)
                                                            devolucaoLR += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vSeg"))
                                                    {
                                                        if (ehMono == null)
                                                            devolucaoLR += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }
                                                }
                                                else
                                                {
                                                    ViewBag.Erro = 4;
                                                    return View(comp);
                                                }
                                            }
                                            else
                                            {
                                                if (ncmAll.Contains(NCM))
                                                {
                                                    var ehMono = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(NCM) && (_.TaxationTypeNcmId.Equals((long)2) ||
                                                                                        _.TaxationTypeNcmId.Equals((long)3) || _.TaxationTypeNcmId.Equals((long)4) || _.TaxationTypeNcmId.Equals((long)5)))
                                                                             .FirstOrDefault();

                                                    if (exitNotes[i][j].ContainsKey("vProd"))
                                                    {
                                                        if (ehMono == null)
                                                            devolucaoLR += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vFrete"))
                                                    {
                                                        if (ehMono == null)
                                                            devolucaoLR += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vDesc"))
                                                    {
                                                        if (ehMono == null)
                                                            devolucaoLR -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vOutro"))
                                                    {
                                                        if (ehMono == null)
                                                            devolucaoLR += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vSeg"))
                                                    {
                                                        if (ehMono == null)
                                                            devolucaoLR += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }

                                                }
                                                else
                                                {
                                                    ViewBag.Erro = 4;
                                                    return View(comp);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (comp.CountingTypeId.Equals((long)2))
                            {
                                //  Empresa Lucro Presumido

                                // Receitas
                                for (int i = exitNotes.Count - 1; i >= 0; i--)
                                {
                                    if (!exitNotes[i][2]["CNPJ"].Equals(c.Document))
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }

                                    if (exitNotes[i][1].ContainsKey("dhEmi"))
                                        ncmsTaxation = _taxationNcmService.FindAllInDate(ncmsCompany, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));

                                    bool cfop = false;
                                    string NCM = "", cProd = "";

                                    for (int j = 0; j < exitNotes[i].Count; j++)
                                    {
                                        if (exitNotes[i][j].ContainsKey("CFOP"))
                                        {
                                            cfop = false;

                                            if (cfopsVenda.Contains(exitNotes[i][j]["CFOP"]) || cfopsVendaST.Contains(exitNotes[i][j]["CFOP"]))
                                            {
                                                cfop = true;
                                            }

                                        }

                                        if (exitNotes[i][1]["finNFe"] != "4" && cfop)
                                        {
                                            if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("NCM"))
                                            {
                                                NCM = exitNotes[i][j]["NCM"];
                                                cProd = exitNotes[i][j]["cProd"];

                                               
                                                if (comp.Taxation == "Produto")
                                                {
                                                    if (codeProdAll.Contains(cProd) && ncmAll.Contains(NCM))
                                                    {
                                                        var ehMono = ncmsTaxation.Where(_ => _.CodeProduct.Equals(cProd) && _.Ncm.Code.Equals(NCM) &&
                                                                                        (_.TaxationTypeNcmId.Equals((long)2) || _.TaxationTypeNcmId.Equals((long)3) || _.TaxationTypeNcmId.Equals((long)4) ||
                                                                                        _.TaxationTypeNcmId.Equals((long)5)))
                                                                                .FirstOrDefault();

                                                        if (exitNotes[i][j].ContainsKey("vProd"))
                                                        {
                                                            if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                                receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                                receitaComercio += Convert.ToDecimal(NCM);
                                                            else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                                receitaTransporte += Convert.ToDecimal(NCM);
                                                            else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                                receitaServico += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                            if (ehMono != null)
                                                                receitaMono += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }

                                                        if (exitNotes[i][j].ContainsKey("vFrete"))
                                                        {
                                                            if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                                receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                                receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                                receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                                receitaServico += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                                            if (ehMono != null)
                                                                receitaMono += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }

                                                        if (exitNotes[i][j].ContainsKey("vDesc"))
                                                        {
                                                            if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                                receitaPetroleo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                                receitaComercio -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                                receitaTransporte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                                receitaServico -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);

                                                            if (ehMono != null)
                                                                receitaMono -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }

                                                        if (exitNotes[i][j].ContainsKey("vOutro"))
                                                        {
                                                            if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                                receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                                receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                                receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                                receitaServico += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);

                                                            if (ehMono != null)
                                                                receitaMono += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }

                                                        if (exitNotes[i][j].ContainsKey("vSeg"))
                                                        {
                                                            if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                                receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                                receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                                receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                                receitaServico += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);

                                                            if (ehMono != null)
                                                                receitaMono += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ViewBag.Erro = 4;
                                                        return View(comp);
                                                    }
                                                }
                                                else
                                                {
                                                    if (ncmAll.Contains(NCM))
                                                    {
                                                        var ehMono = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(NCM) && (_.TaxationTypeNcmId.Equals((long)2) ||
                                                                                            _.TaxationTypeNcmId.Equals((long)3) || _.TaxationTypeNcmId.Equals((long)4) || _.TaxationTypeNcmId.Equals((long)5)))
                                                                                 .FirstOrDefault();

                                                        if (exitNotes[i][j].ContainsKey("vProd"))
                                                        {
                                                            if (ncm1.Contains(NCM))
                                                                receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            else if (ncm2.Contains(NCM))
                                                                receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            else if (ncm3.Contains(NCM))
                                                                receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                            else if (ncm4.Contains(NCM))
                                                                receitaServico += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                            if (ehMono != null)
                                                                receitaMono += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }

                                                        if (exitNotes[i][j].ContainsKey("vFrete"))
                                                        {
                                                            if (ncm1.Contains(NCM))
                                                                receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            else if (ncm2.Contains(NCM))
                                                                receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            else if (ncm3.Contains(NCM))
                                                                receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                            else if (ncm4.Contains(NCM))
                                                                receitaServico += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                                            if (ehMono != null)
                                                                receitaMono += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }

                                                        if (exitNotes[i][j].ContainsKey("vDesc"))
                                                        {
                                                            if (ncm1.Contains(NCM))
                                                                receitaPetroleo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            else if (ncm2.Contains(NCM))
                                                                receitaComercio -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            else if (ncm3.Contains(NCM))
                                                                receitaTransporte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                            else if (ncm4.Contains(NCM))
                                                                receitaServico -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);

                                                            if (ehMono != null)
                                                                receitaMono -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }

                                                        if (exitNotes[i][j].ContainsKey("vOutro"))
                                                        {
                                                            if (ncm1.Contains(NCM))
                                                                receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            else if (ncm2.Contains(NCM))
                                                                receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            else if (ncm3.Contains(NCM))
                                                                receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                            else if (ncm4.Contains(NCM))
                                                                receitaServico += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);

                                                            if (ehMono != null)
                                                                receitaMono += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }

                                                        if (exitNotes[i][j].ContainsKey("vSeg"))
                                                        {
                                                            if (ncm1.Contains(NCM))
                                                                receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            else if (ncm2.Contains(NCM))
                                                                receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            else if (ncm3.Contains(NCM))
                                                                receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                            else if (ncm4.Contains(NCM))
                                                                receitaServico += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);

                                                            if (ehMono != null)
                                                                receitaMono += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }

                                                    }
                                                    else
                                                    {
                                                        ViewBag.Erro = 4;
                                                        return View(comp);
                                                    }
                                                }
                                            }
                                            /*
                                            if (comp.Taxation == "Produto")
                                            {
                                                var ehMono = ncmsTaxation.Where(_ => _.CodeProduct.Equals(cProd) && _.Ncm.Code.Equals(NCM) &&
                                                                                (_.TaxationTypeNcmId.Equals((long)2) || _.TaxationTypeNcmId.Equals((long)3) || _.TaxationTypeNcmId.Equals((long)4) ||
                                                                                _.TaxationTypeNcmId.Equals((long)5)))
                                                                        .FirstOrDefault();

                                                if (exitNotes[i][j].ContainsKey("vFCP") && exitNotes[i][j].ContainsKey("orig"))
                                                {
                                                    if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                        receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                                    else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                        receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                                    else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                        receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                                    else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                        receitaServico += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);

                                                    if (ehMono != null)
                                                        receitaMono += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                                }

                                                if (exitNotes[i][j].ContainsKey("pIPI"))
                                                {
                                                    if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                        receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                                    else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                        receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                                    else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                        receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                                    else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                        receitaServico += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);

                                                    if (ehMono != null)
                                                        receitaMono += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                                }
                                            }
                                            else
                                            {
                                                if (ncmAll.Contains(NCM))
                                                {
                                                    var ehMono = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(NCM) && (_.TaxationTypeNcmId.Equals((long)2) ||
                                                                                        _.TaxationTypeNcmId.Equals((long)3) || _.TaxationTypeNcmId.Equals((long)4) || _.TaxationTypeNcmId.Equals((long)5)))
                                                                             .FirstOrDefault();

                                                    if (exitNotes[i][j].ContainsKey("vFCP") && exitNotes[i][j].ContainsKey("orig"))
                                                    {
                                                        if (ncm1.Contains(NCM))
                                                            receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                                        else if (ncm2.Contains(NCM))
                                                            receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                                        else if (ncm3.Contains(NCM))
                                                            receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                                        else if (ncm4.Contains(NCM))
                                                            receitaServico += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);

                                                        if (ehMono != null)
                                                            receitaMono += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("pIPI"))
                                                    {
                                                        if (ncm1.Contains(NCM))
                                                            receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                                        else if (ncm2.Contains(NCM))
                                                            receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                                        else if (ncm3.Contains(NCM))
                                                            receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                                        else if (ncm4.Contains(NCM))
                                                            receitaServico += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);

                                                        if (ehMono != null)
                                                            receitaMono += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                                    }
                                                }
                                                else
                                                {
                                                    ViewBag.Erro = 4;
                                                    return View(comp);
                                                }
                                            }
                                            */
                                        }
                                    }
                                }

                                // Devoluções de Vendas
                                for (int i = exitNotes.Count - 1; i >= 0; i--)
                                {
                                    if (exitNotes[i][1]["finNFe"] != "4" || exitNotes[i][1]["tpNF"].Equals("1"))
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }

                                    if (exitNotes[i][1].ContainsKey("dhEmi"))
                                        ncmsTaxation = _taxationNcmService.FindAllInDate(ncmsCompany, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));

                                    string NCM = "", cProd = "";

                                    for (int j = 0; j < exitNotes[i].Count; j++)
                                    {
                                        if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("NCM"))
                                        {
                                            NCM = exitNotes[i][j]["NCM"];
                                            cProd = exitNotes[i][j]["cProd"];

                                            if (comp.Taxation == "Produto")
                                            {
                                                if (codeProdAll.Contains(cProd) && ncmAll.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    var ehMono = ncmsTaxation.Where(_ => _.CodeProduct.Equals(cProd) && _.Ncm.Code.Equals(exitNotes[i][j]["NCM"]) &&
                                                                (_.TaxationTypeNcmId.Equals((long)2) || _.TaxationTypeNcmId.Equals((long)3) || _.TaxationTypeNcmId.Equals((long)4) ||
                                                                _.TaxationTypeNcmId.Equals((long)5))).FirstOrDefault();

                                                    if (exitNotes[i][j].ContainsKey("vProd"))
                                                    {
                                                        if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vFrete"))
                                                    {
                                                        if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vDesc"))
                                                    {
                                                        if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                            devolucaoPetroleo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                            devolucaoComercio -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                            devolucaoTransporte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                            devolucaoServico -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vOutro"))
                                                    {
                                                        if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vSeg"))
                                                    {
                                                        if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }

                                                }
                                                else
                                                {
                                                    ViewBag.Erro = 4;
                                                    return View(comp);
                                                }
                                            }
                                            else
                                            {
                                                if (ncmAll.Contains(exitNotes[i][j]["NCM"]))
                                                {
                                                    var ehMono = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(exitNotes[i][j]["NCM"]) && (_.TaxationTypeNcmId.Equals((long)2) || _.TaxationTypeNcmId.Equals((long)3) ||
                                                                                        _.TaxationTypeNcmId.Equals((long)4) || _.TaxationTypeNcmId.Equals((long)5))).FirstOrDefault();

                                                    if (exitNotes[i][j].ContainsKey("vProd"))
                                                    {
                                                        if (ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        else if (ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        else if (ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        else if (ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vFrete"))
                                                    {
                                                        if (ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        else if (ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        else if (ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        else if (ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vDesc"))
                                                    {
                                                        if (ncm1.Contains(NCM))
                                                            devolucaoPetroleo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        else if (ncm2.Contains(NCM))
                                                            devolucaoComercio -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        else if (ncm3.Contains(NCM))
                                                            devolucaoTransporte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        else if (ncm4.Contains(NCM))
                                                            devolucaoServico -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vOutro"))
                                                    {
                                                        if (ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        else if (ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        else if (ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        else if (ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }

                                                    if (exitNotes[i][j].ContainsKey("vSeg"))
                                                    {
                                                        if (ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        else if (ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        else if (ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        else if (ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }

                                                }
                                                else
                                                {
                                                    ViewBag.Erro = 4;
                                                    return View(comp);
                                                }
                                            }
                                        }
                                        /*
                                        if (comp.Taxation == "Produto")
                                        {
                                            var ehMono = ncmsTaxation.Where(_ => _.CodeProduct.Equals(cProd) && _.Ncm.Code.Equals(NCM) &&
                                                        (_.TaxationTypeNcmId.Equals((long)2) || _.TaxationTypeNcmId.Equals((long)3) || _.TaxationTypeNcmId.Equals((long)4) ||
                                                        _.TaxationTypeNcmId.Equals((long)5))).FirstOrDefault();

                                            if (exitNotes[i][j].ContainsKey("vFCP") && exitNotes[i][j].ContainsKey("orig"))
                                            {
                                                if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                    devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                                else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                    devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                                else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                    devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                                else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                    devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);

                                                if (ehMono == null)
                                                    devolucaoNormal += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("pIPI"))
                                            {
                                                if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                    devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                                else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                    devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                                else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                    devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                                else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                    devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);

                                                if (ehMono == null)
                                                    devolucaoNormal += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                            }
                                        }
                                        else
                                        {
                                            var ehMono = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(NCM) && (_.TaxationTypeNcmId.Equals((long)2) || _.TaxationTypeNcmId.Equals((long)3) ||
                                                                                _.TaxationTypeNcmId.Equals((long)4) || _.TaxationTypeNcmId.Equals((long)5))).FirstOrDefault();

                                            if (exitNotes[i][j].ContainsKey("vFCP") && exitNotes[i][j].ContainsKey("orig"))
                                            {
                                                if (ncm1.Contains(NCM))
                                                    devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                                else if (ncm2.Contains(NCM))
                                                    devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                                else if (ncm3.Contains(NCM))
                                                    devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                                else if (ncm4.Contains(NCM))
                                                    devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);

                                                if (ehMono == null)
                                                    devolucaoNormal += Convert.ToDecimal(exitNotes[i][j]["vFCP"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("pIPI"))
                                            {
                                                if (ncm1.Contains(NCM))
                                                    devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                                else if (ncm2.Contains(NCM))
                                                    devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                                else if (ncm3.Contains(NCM))
                                                    devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                                else if (ncm4.Contains(NCM))
                                                    devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);

                                                if (ehMono == null)
                                                    devolucaoNormal += Convert.ToDecimal(exitNotes[i][j]["vIPI"]);
                                            }
                                        }
                                        */
                                    }
                                }

                                if (!comp.Sped)
                                {
                                    // Devoluções de Vendas
                                    for (int i = entryNotes.Count - 1; i >= 0; i--)
                                    {
                                        if (entryNotes[i][1]["finNFe"] != "4" || !entryNotes[i][3]["CNPJ"].Equals(c.Document) || exitNotes[i][1]["tpNF"] == "0")
                                        {
                                            entryNotes.RemoveAt(i);
                                            continue;
                                        }

                                        if (entryNotes[i][1].ContainsKey("dhEmi"))
                                            ncmsTaxation = _taxationNcmService.FindAllInDate(ncmsCompany, Convert.ToDateTime(entryNotes[i][1]["dhEmi"]));

                                        string NCM = "", cProd = "";

                                        for (int j = 0; j < entryNotes[i].Count; j++)
                                        {
                                            if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("NCM"))
                                            {
                                                NCM = entryNotes[i][j]["NCM"];
                                                cProd = entryNotes[i][j]["cProd"];

                                                if (comp.Taxation == "Produto")
                                                {
                                                    var ehMono = ncmsTaxation.Where(_ => _.CodeProduct.Equals(cProd) && _.Ncm.Code.Equals(NCM) &&
                                                                  (_.TaxationTypeNcmId.Equals((long)2) || _.TaxationTypeNcmId.Equals((long)3) || _.TaxationTypeNcmId.Equals((long)4) ||
                                                                  _.TaxationTypeNcmId.Equals((long)5))).FirstOrDefault();

                                                    if (entryNotes[i][j].ContainsKey("vProd"))
                                                    {
                                                        if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                                        else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                                        else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                                        else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vProd"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                                    }

                                                    if (entryNotes[i][j].ContainsKey("vFrete"))
                                                    {
                                                        if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                                        else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                                        else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                                        else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                                    }

                                                    if (entryNotes[i][j].ContainsKey("vDesc"))
                                                    {
                                                        if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                            devolucaoPetroleo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                                        else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                            devolucaoComercio -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                                        else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                            devolucaoTransporte -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                                        else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                            devolucaoServico -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                                    }

                                                    if (entryNotes[i][j].ContainsKey("vOutro"))
                                                    {
                                                        if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                                        else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                                        else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                                        else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                                    }

                                                    if (entryNotes[i][j].ContainsKey("vSeg"))
                                                    {
                                                        if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                                        else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                                        else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                                        else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                                    }
                                                }
                                                else
                                                {
                                                    var ehMono = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(NCM) && (_.TaxationTypeNcmId.Equals((long)2) || 
                                                                                        _.TaxationTypeNcmId.Equals((long)3) || _.TaxationTypeNcmId.Equals((long)4) || _.TaxationTypeNcmId.Equals((long)5)))
                                                                            .FirstOrDefault();

                                                    if (entryNotes[i][j].ContainsKey("vProd"))
                                                    {
                                                        if (ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                                        else if (ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                                        else if (ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                                        else if (ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vProd"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                                    }

                                                    if (entryNotes[i][j].ContainsKey("vFrete"))
                                                    {
                                                        if (ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                                        else if (ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                                        else if (ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                                        else if (ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                                    }

                                                    if (entryNotes[i][j].ContainsKey("vDesc"))
                                                    {
                                                        if (ncm1.Contains(NCM))
                                                            devolucaoPetroleo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                                        else if (ncm2.Contains(NCM))
                                                            devolucaoComercio -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                                        else if (ncm3.Contains(NCM))
                                                            devolucaoTransporte -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                                        else if (ncm4.Contains(NCM))
                                                            devolucaoServico -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                                    }

                                                    if (entryNotes[i][j].ContainsKey("vOutro"))
                                                    {
                                                        if (ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                                        else if (ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                                        else if (ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                                        else if (ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                                    }

                                                    if (entryNotes[i][j].ContainsKey("vSeg"))
                                                    {
                                                        if (ncm1.Contains(NCM))
                                                            devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                                        else if (ncm2.Contains(NCM))
                                                            devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                                        else if (ncm3.Contains(NCM))
                                                            devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                                        else if (ncm4.Contains(NCM))
                                                            devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);

                                                        if (ehMono == null)
                                                            devolucaoNormal += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                                    }
                                                }
                                               
                                            }
                                            /*
                                            if (comp.Taxation == "Produto")
                                            {
                                                var ehMono = ncmsTaxation.Where(_ => _.CodeProduct.Equals(cProd) && _.Ncm.Code.Equals(NCM) &&
                                                              (_.TaxationTypeNcmId.Equals((long)2) || _.TaxationTypeNcmId.Equals((long)3) || _.TaxationTypeNcmId.Equals((long)4) ||
                                                              _.TaxationTypeNcmId.Equals((long)5))).FirstOrDefault();

                                                if (entryNotes[i][j].ContainsKey("vFCP") && entryNotes[i][j].ContainsKey("orig"))
                                                {
                                                    if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                        devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vFCP"]);
                                                    else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                        devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vFCP"]);
                                                    else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                        devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vFCP"]);
                                                    else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                        devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vFCP"]);

                                                    if (ehMono == null)
                                                        devolucaoNormal += Convert.ToDecimal(entryNotes[i][j]["vFCP"]);
                                                }

                                                if (entryNotes[i][j].ContainsKey("pIPI"))
                                                {
                                                    if (codeProd1.Contains(cProd) && ncm1.Contains(NCM))
                                                        devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vIPI"]);
                                                    else if (codeProd2.Contains(cProd) && ncm2.Contains(NCM))
                                                        devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vIPI"]);
                                                    else if (codeProd3.Contains(cProd) && ncm3.Contains(NCM))
                                                        devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vIPI"]);
                                                    else if (codeProd4.Contains(cProd) && ncm4.Contains(NCM))
                                                        devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vIPI"]);

                                                    if (ehMono == null)
                                                        devolucaoNormal += Convert.ToDecimal(entryNotes[i][j]["vIPI"]);
                                                }
                                            }
                                            else
                                            {
                                                var ehMono = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(NCM) && (_.TaxationTypeNcmId.Equals((long)2) ||
                                                                                    _.TaxationTypeNcmId.Equals((long)3) || _.TaxationTypeNcmId.Equals((long)4) || _.TaxationTypeNcmId.Equals((long)5)))
                                                                        .FirstOrDefault();

                                                if (entryNotes[i][j].ContainsKey("vFCP") && entryNotes[i][j].ContainsKey("orig"))
                                                {
                                                    if (ncm1.Contains(NCM))
                                                        devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vFCP"]);
                                                    else if (ncm2.Contains(NCM))
                                                        devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vFCP"]);
                                                    else if (ncm3.Contains(NCM))
                                                        devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vFCP"]);
                                                    else if (ncm4.Contains(NCM))
                                                        devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vFCP"]);

                                                    if (ehMono == null)
                                                        devolucaoNormal += Convert.ToDecimal(entryNotes[i][j]["vFCP"]);
                                                }

                                                if (entryNotes[i][j].ContainsKey("pIPI"))
                                                {
                                                    if (ncm1.Contains(NCM))
                                                        devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vIPI"]);
                                                    else if (ncm2.Contains(NCM))
                                                        devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vIPI"]);
                                                    else if (ncm3.Contains(NCM))
                                                        devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vIPI"]);
                                                    else if (ncm4.Contains(NCM))
                                                        devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vIPI"]);

                                                    if (ehMono == null)
                                                        devolucaoNormal += Convert.ToDecimal(entryNotes[i][j]["vIPI"]);
                                                }
                                            }
                                            */
                                        }
                                    }
                                }
                               
                            }
                            else if (comp.CountingTypeId.Equals((long)3))
                            {
                                //  Empresa do Simples

                                //   Vendas
                                List<string> codeProdMono = new List<string>();
                                List<string> codeProdNormal = new List<string>();
                                List<string> codeProdST = new List<string>();
                                List<string> codeProdAliqZero = new List<string>();
                                List<string> codeProdIsento = new List<string>();
                                List<string> codeProdOutras = new List<string>();
                                List<string> ncmMono = new List<string>();
                                List<string> ncmNormal = new List<string>();
                                List<string> ncmST = new List<string>();
                                List<string> ncmAliqZero = new List<string>();
                                List<string> ncmIsento = new List<string>();
                                List<string> ncmOutras = new List<string>();

                                cfopsVenda.AddRange(cfopsVendaST);
                                cfopsVenda.AddRange(cfopsTransf);
                                cfopsVenda.AddRange(cfopsTransfST);

                                exitNotes = importXml.NFeAll(directoryNfeExit, cfopsVenda);
                                // Vendas
                                for (int i = exitNotes.Count - 1; i >= 0; i--)
                                {
                                    if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }

                                    if (exitNotes[i][1].ContainsKey("dhEmi"))
                                    {
                                        ncmsTaxation = _taxationNcmService.FindAllInDate(ncmsCompany, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));

                                        codeProdMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)2)).Select(_ => _.CodeProduct).ToList();
                                        codeProdNormal = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)1)).Select(_ => _.CodeProduct).ToList();
                                        codeProdST = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)4)).Select(_ => _.CodeProduct).ToList();
                                        codeProdAliqZero = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)3)).Select(_ => _.CodeProduct).ToList();
                                        codeProdIsento = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)5)).Select(_ => _.CodeProduct).ToList();
                                        codeProdOutras = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)6)).Select(_ => _.CodeProduct).ToList();

                                        ncmMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)2)).Select(_ => _.Ncm.Code).ToList();
                                        ncmNormal = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)1)).Select(_ => _.Ncm.Code).ToList();
                                        ncmST = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)4)).Select(_ => _.Ncm.Code).ToList();
                                        ncmAliqZero = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)3)).Select(_ => _.Ncm.Code).ToList();
                                        ncmIsento = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)5)).Select(_ => _.Ncm.Code).ToList();
                                        ncmOutras = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)6)).Select(_ => _.Ncm.Code).ToList();

                                    }

                                    string NCM = "", cProd = "", CFOP = "";
                                    decimal vProd = 0;

                                    for (int j = 0; j < exitNotes[i].Count(); j++)
                                    {
                                        if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("NCM"))
                                        {
                                            cProd = exitNotes[i][j]["cProd"];
                                            NCM = exitNotes[i][j]["NCM"];
                                            CFOP = exitNotes[i][j]["CFOP"];

                                            if (exitNotes[i][j].ContainsKey("vProd"))
                                            {
                                                vProd = 0;
                                                vProd += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("vFrete"))
                                                vProd += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                            if (exitNotes[i][j].ContainsKey("vDesc"))
                                                vProd -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);

                                            if (exitNotes[i][j].ContainsKey("vOutro"))
                                                vProd += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);

                                            if (exitNotes[i][j].ContainsKey("vSeg"))
                                                vProd += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);

                                        }

                                        if (exitNotes[i][j].ContainsKey("CSOSN"))
                                        {
                                            if (comp.Taxation == "Produto")
                                            {
                                                if (codeProdMono.Contains(cProd) && ncmMono.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0,1) == "6")
                                                    {
                                                        vendasNormalMono += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            vendasSTMono += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            vendasNormalMono += Convert.ToDecimal(vProd);
                                                    }

                                                   
                                                }
                                                else if (codeProdNormal.Contains(cProd) && ncmNormal.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        vendasNormalNormal += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            vendasSTNormal += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            vendasNormalNormal += Convert.ToDecimal(vProd);
                                                    }
                                                   
                                                }
                                                else if (codeProdST.Contains(cProd) && ncmST.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        vendasNormalST += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            vendasSTST += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            vendasNormalST += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (codeProdAliqZero.Contains(cProd) && ncmAliqZero.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        vendasNormalAliqZero += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            vendasSTAliqZero += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            vendasNormalAliqZero += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (codeProdIsento.Contains(cProd) && ncmIsento.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        vendasNormalIsento += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            vendasSTIsento += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            vendasNormalIsento += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (codeProdOutras.Contains(cProd) && ncmOutras.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        vendasNormalOutras += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            vendasSTOutras += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            vendasNormalOutras += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else
                                                {
                                                    ViewBag.NCM = NCM;
                                                    ViewBag.Erro = 4;
                                                    return View(comp);
                                                }
                                            }
                                            else
                                            {
                                                if (ncmMono.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        vendasNormalMono += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            vendasSTMono += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            vendasNormalMono += Convert.ToDecimal(vProd);
                                                    }
                                                
                                                }
                                                else if (ncmNormal.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        vendasNormalNormal += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            vendasSTNormal += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            vendasNormalNormal += Convert.ToDecimal(vProd);
                                                    }
                                                 
                                                }
                                                else if (ncmST.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        vendasNormalST += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            vendasSTST += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            vendasNormalST += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (ncmAliqZero.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        vendasNormalAliqZero += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            vendasSTAliqZero += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            vendasNormalAliqZero += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (ncmIsento.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        vendasNormalIsento += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            vendasSTIsento += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            vendasNormalIsento += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (ncmOutras.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        vendasNormalOutras += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            vendasSTOutras += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            vendasNormalOutras += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else
                                                {
                                                    ViewBag.NCM = NCM;
                                                    ViewBag.Erro = 4;
                                                    return View(comp);
                                                }
                                            }

                                        }
                                    }
                                }

                                
                                // Devoluções de Vendas
                                for (int i = exitNotes.Count - 1; i >= 0; i--)
                                {
                                    if (exitNotes[i][1]["finNFe"] != "4" || exitNotes[i][1]["tpNF"].Equals("1"))
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }

                                    if (exitNotes[i][1].ContainsKey("dhEmi"))
                                    {
                                        ncmsTaxation = _taxationNcmService.FindAllInDate(ncmsCompany, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));

                                        codeProdMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)2)).Select(_ => _.CodeProduct).ToList();
                                        codeProdNormal = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)1)).Select(_ => _.CodeProduct).ToList();
                                        codeProdST = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)4)).Select(_ => _.CodeProduct).ToList();
                                        codeProdAliqZero = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)3)).Select(_ => _.CodeProduct).ToList();
                                        codeProdIsento = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)5)).Select(_ => _.CodeProduct).ToList();
                                        codeProdOutras = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)6)).Select(_ => _.CodeProduct).ToList();

                                        ncmMono = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)2)).Select(_ => _.Ncm.Code).ToList();
                                        ncmNormal = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)1)).Select(_ => _.Ncm.Code).ToList();
                                        ncmST = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)4)).Select(_ => _.Ncm.Code).ToList();
                                        ncmAliqZero = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)3)).Select(_ => _.Ncm.Code).ToList();
                                        ncmIsento = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)5)).Select(_ => _.Ncm.Code).ToList();
                                        ncmOutras = ncmsTaxation.Where(_ => _.TaxationTypeNcmId.Equals((long)6)).Select(_ => _.Ncm.Code).ToList();
                                    }

                                    string NCM = "", cProd = "", CFOP = "";
                                    decimal vProd = 0;

                                    for (int j = 0; j < exitNotes[i].Count; j++)
                                    {
                                        if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("NCM"))
                                        {
                                            cProd = exitNotes[i][j]["cProd"];
                                            NCM = exitNotes[i][j]["NCM"];
                                            CFOP = exitNotes[i][j]["CFOP"];

                                            if (exitNotes[i][j].ContainsKey("vProd"))
                                            {
                                                vProd = 0;
                                                vProd += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            }

                                            if (exitNotes[i][j].ContainsKey("vFrete"))
                                                vProd += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                            if (exitNotes[i][j].ContainsKey("vDesc"))
                                                vProd -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);

                                            if (exitNotes[i][j].ContainsKey("vOutro"))
                                                vProd += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);

                                            if (exitNotes[i][j].ContainsKey("vSeg"))
                                                vProd += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);

                                        }

                                        if (exitNotes[i][j].ContainsKey("CSOSN"))
                                        {
                                            if (comp.Taxation == "Produto")
                                            {
                                                if (codeProdMono.Contains(cProd) && ncmMono.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        devoNormalMono += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            devoSTMono += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            devoNormalMono += Convert.ToDecimal(vProd);
                                                    }


                                                }
                                                else if (codeProdNormal.Contains(cProd) && ncmNormal.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        devoNormalNormal += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            devoSTNormal += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            devoNormalNormal += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (codeProdST.Contains(cProd) && ncmST.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        devoNormalST += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            devoSTST += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            devoNormalST += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (codeProdAliqZero.Contains(cProd) && ncmAliqZero.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        devoNormalAliqZero += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            devoSTAliqZero += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            devoNormalAliqZero += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (codeProdIsento.Contains(cProd) && ncmIsento.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        devoNormalIsento += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            devoSTIsento += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            devoNormalIsento += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (codeProdOutras.Contains(cProd) && ncmOutras.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        devoNormalOutras += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            devoSTOutras += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            devoNormalOutras += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else
                                                {
                                                    ViewBag.NCM = NCM;
                                                    ViewBag.Erro = 4;
                                                    return View(comp);
                                                }
                                            }
                                            else
                                            {
                                                if (ncmMono.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        devoNormalMono += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            devoSTMono += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            devoNormalMono += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (ncmNormal.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        devoNormalNormal += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            devoSTNormal += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            devoNormalNormal += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (ncmST.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        devoNormalST += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            devoSTST += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            devoNormalST += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (ncmAliqZero.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        devoNormalAliqZero += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            devoSTAliqZero += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            devoNormalAliqZero += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (ncmIsento.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        devoNormalIsento += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            devoSTIsento += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            devoNormalIsento += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else if (ncmOutras.Contains(NCM))
                                                {
                                                    if (CFOP.Substring(0, 1) == "6")
                                                    {
                                                        devoNormalOutras += Convert.ToDecimal(vProd);
                                                    }
                                                    else
                                                    {
                                                        if (exitNotes[i][j]["CSOSN"] == "500")
                                                            devoSTOutras += Convert.ToDecimal(vProd);
                                                        else if (exitNotes[i][j]["CSOSN"] == "101" || exitNotes[i][j]["CSOSN"] == "102" || exitNotes[i][j]["CSOSN"] == "300")
                                                            devoNormalOutras += Convert.ToDecimal(vProd);
                                                    }

                                                }
                                                else
                                                {
                                                    ViewBag.NCM = NCM;
                                                    ViewBag.Erro = 4;
                                                    return View(comp);
                                                }
                                            }

                                        }
                                    }
                                }

                            }
                        }

                        if (imp != null)
                        {
                            imp.PisCofins = true;

                            //  Empresa Lucro Real
                            imp.Venda = vendaLR;
                            imp.DevolucaoVendaP = devolucaoLR;


                            //  Empresa Lucro Presumido
                            imp.Receita1 = receitaPetroleo;
                            imp.Receita2 = receitaComercio;
                            imp.Receita3 = receitaTransporte;
                            //imp.Receita4 = receitaServico;
                            imp.ReceitaMono = receitaMono;

                            imp.Devolucao1P = devolucaoPetroleo;
                            imp.Devolucao2P = devolucaoComercio;
                            imp.Devolucao3P = devolucaoTransporte;
                            //imp.Devolucao4P = devolucaoServico;
                            imp.DevolucaoNormalP = devolucaoNormal;


                            //  Empresa do Simples
                            imp.VendaNormalNormal = vendasNormalNormal;
                            imp.VendaSTNormal = vendasSTNormal;
                            imp.VendaNormalMonofasico = vendasNormalMono;
                            imp.VendaSTMonofasico = vendasSTMono;
                            imp.VendaNormalST = vendasNormalST;
                            imp.VendaNormalAliqZero = vendasNormalAliqZero;
                            imp.VendaNormalIsento = vendasNormalIsento;
                            imp.VendaSTST = vendasSTST;
                            imp.VendaSTAliqZero = vendasSTAliqZero;
                            imp.VendaSTIsento = vendasSTIsento;
                            imp.VendaNormalOutras = vendasNormalOutras;
                            imp.VendaSTOutras = vendasSTOutras;

                            imp.DevoNormalNormalP = devoNormalNormal;
                            imp.DevoSTNormalP = devoSTNormal;
                            imp.DevoNormalMonofasicoP = devoNormalMono;
                            imp.DevoSTMonofasicoP = devoSTMono;
                            imp.DevoNormalSTP = devoNormalST;
                            imp.DevoNormalAliqZeroP = devoNormalAliqZero;
                            imp.DevoNormalIsentoP = devoNormalIsento;
                            imp.DevoSTSTP = devoSTST;
                            imp.DevoSTAliqZeroP = devoSTAliqZero;
                            imp.DevoSTIsentoP = devoSTIsento;
                            imp.DevoSTOutrasP = devoSTOutras;
                            imp.DevoNormalOutrasP = devoNormalOutras;

                        }
                        else
                        {
                            tax.PisCofins = true;

                            //  Empresa Lucro Real
                            tax.Venda = vendaLR;
                            tax.DevolucaoVendaP = devolucaoLR;

                            //  Empresa Lucro Presumido
                            tax.Receita1 = receitaPetroleo;
                            tax.Receita2 = receitaComercio;
                            tax.Receita3 = receitaTransporte;
                            //tax.Receita4 = receitaServico;
                            tax.ReceitaMono = receitaMono;

                            tax.Devolucao1P = devolucaoPetroleo;
                            tax.Devolucao2P = devolucaoComercio;
                            tax.Devolucao3P = devolucaoTransporte;
                           //tax.Devolucao4P = devolucaoServico;
                            tax.DevolucaoNormalP = devolucaoNormal;

                            //  Empresa do Simples
                            tax.VendaNormalNormal = vendasNormalNormal;
                            tax.VendaSTNormal = vendasSTNormal;
                            tax.VendaNormalMonofasico = vendasNormalMono;
                            tax.VendaSTMonofasico = vendasSTMono;
                            tax.VendaNormalST = vendasNormalST;
                            tax.VendaNormalAliqZero = vendasNormalAliqZero;
                            tax.VendaNormalIsento = vendasNormalIsento;
                            tax.VendaSTST = vendasSTST;
                            tax.VendaSTAliqZero = vendasSTAliqZero;
                            tax.VendaSTIsento = vendasSTIsento;
                            tax.VendaNormalOutras = vendasNormalOutras;
                            tax.VendaSTOutras = vendasSTOutras;

                            tax.DevoNormalNormalP = devoNormalNormal;
                            tax.DevoSTNormalP = devoSTNormal;
                            tax.DevoNormalMonofasicoP = devoNormalMono;
                            tax.DevoSTMonofasicoP = devoSTMono;
                            tax.DevoNormalSTP = devoNormalST;
                            tax.DevoNormalAliqZeroP = devoNormalAliqZero;
                            tax.DevoNormalIsentoP = devoNormalIsento;
                            tax.DevoSTSTP = devoSTST;
                            tax.DevoSTAliqZeroP = devoSTAliqZero;
                            tax.DevoSTIsentoP = devoSTIsento;
                            tax.DevoSTOutrasP = devoSTOutras;
                            tax.DevoNormalOutrasP = devoNormalOutras;

                        }
                    }
                }

                if (imp != null)
                {
                    _service.Update(imp, GetLog(OccorenceLog.Update));
                }
                else
                {
                    _service.Create(tax, GetLog(OccorenceLog.Create));
                    imp = _service.FindByMonth(companyid, month, year);

                    foreach(var g in addGrupos)
                    {
                        g.TaxId = imp.Id;
                    }
                    _grupoService.Create(addGrupos, GetLog(OccorenceLog.Create));
                }

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                return RedirectToAction("Index", new { id = companyid, year = year,  month = month});
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    
    }
}
