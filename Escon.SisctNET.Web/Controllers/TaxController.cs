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

        public IActionResult Index(int id,string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                var comp = _companyService.FindById(id, null);

                ViewBag.PercentualPetroleo = Convert.ToDecimal(comp.IRPJ1).ToString().Replace(".", ",");
                ViewBag.PercentualComercio = Convert.ToDecimal(comp.IRPJ2).ToString().Replace(".", ",");
                ViewBag.PercentualTransporte = Convert.ToDecimal(comp.IRPJ3).ToString().Replace(".", ",");
                ViewBag.PercentualServico = Convert.ToDecimal(comp.IRPJ4).ToString().Replace(".", ",");

                ViewBag.Comp = comp;

                SessionManager.SetCompanyIdInSession(id);
                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);
            
                var result = _service.FindByMonth(id, month, year);

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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

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
        public IActionResult Receita(Model.Tax entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                int companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var imp = _service.FindByMonth(companyid, month, year);

                if (imp != null)
                {
                    imp.Bonificacao = entity.Bonificacao;
                    imp.CapitalIM = entity.CapitalIM;
                    imp.ReceitaAF = entity.ReceitaAF;
                    imp.OutrasReceitas = entity.OutrasReceitas;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, null);
                }
                else
                {
                    entity.CompanyId = companyid;
                    entity.MesRef = month;
                    entity.AnoRef = year;
                    entity.Created = DateTime.Now;
                    entity.Updated = entity.Created;

                    _service.Create(entity, null);
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

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
        public IActionResult Despesa(Model.Tax entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                int companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var imp = _service.FindByMonth(companyid, month, year);

                if (imp != null)
                {
                    imp.Energia = entity.Energia;
                    imp.AluguelPredio = entity.AluguelPredio;
                    imp.AluguelME = entity.AluguelME;
                    imp.DespesasF = entity.DespesasF;
                    imp.DespesasME = entity.DespesasME;
                    imp.DespesasA = entity.DespesasA;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, null);
                }
                else
                {
                    entity.CompanyId = companyid;
                    entity.MesRef = month;
                    entity.AnoRef = year;
                    entity.Created = DateTime.Now;
                    entity.Updated = entity.Created;

                    _service.Create(entity, null);
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

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
        public IActionResult Retention(Model.Tax entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                int companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var imp = _service.FindByMonth(companyid, month, year);

                if (imp != null)
                {
                    imp.PisRetido = entity.PisRetido;
                    imp.CofinsRetido = entity.CofinsRetido;
                    imp.CsllRetido = entity.CsllRetido;
                    imp.CsllFonte = entity.CsllFonte;
                    imp.IrpjRetido = entity.IrpjRetido;
                    imp.IrpjFonteServico = entity.IrpjFonteServico;
                    imp.IrpjFonteFinanceira = entity.IrpjFonteFinanceira;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, null);
                }
                else
                {
                    entity.CompanyId = companyid;
                    entity.MesRef = month;
                    entity.AnoRef = year;
                    entity.Created = DateTime.Now;
                    entity.Updated = entity.Created;

                    _service.Create(entity, null);
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

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
        public IActionResult Reduction(Model.Tax entity)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                int companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();

                string month = SessionManager.GetMonthInSession();
                var imp = _service.FindByMonth(companyid, month, year);

                if (imp != null)
                {
                    imp.ReducaoIcms = entity.ReducaoIcms;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, null);
                }
                else
                {
                    entity.CompanyId = companyid;
                    entity.MesRef = month;
                    entity.AnoRef = year;
                    entity.ReducaoIcms = entity.ReducaoIcms;
                    entity.Created = DateTime.Now;
                    entity.Updated = entity.Created;

                    _service.Create(entity, null);
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                int companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var imp = _service.FindByMonth(companyid, mesRef, year);

                if (imp != null)
                {
                    imp.IrpjPago = entity.IrpjPago;
                    imp.CsllPago = entity.CsllPago;
                    imp.Updated = DateTime.Now;

                    _service.Update(imp, null);
                }
                else
                {
                    entity.CompanyId = companyid;
                    entity.MesRef = month;
                    entity.AnoRef = year;
                    entity.Created = DateTime.Now;
                    entity.Updated = entity.Created;

                    _service.Create(entity, null);
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
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

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
        public async Task<IActionResult> Import(string imposto,string type, List<IFormFile> arquivo)
        {
            if (SessionManager.GetAccessesInSession() == null || SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                int companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var comp = _companyService.FindById(companyid, null);

                if (comp.CountingTypeId == null)
                {
                    ViewBag.Erro = 1;
                    return View(comp);
                }

                var NfeExit = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));
                var NfeEntry = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                string directoryNfeExit = "";

                if (type.Equals("xmlE"))
                {
                    directoryNfeExit = NfeExit.Value + "\\" + comp.Document + "\\" + year + "\\" + month + "\\" + "EMPRESA";
                }
                else
                {
                    directoryNfeExit = NfeExit.Value + "\\" + comp.Document + "\\" + year + "\\" + month + "\\" + "SEFAZ";
                }
                
                string directoryNfeEntry = NfeEntry.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                List<string> caminhos = new List<string>();
                 
                var importXml = new Xml.Import(_companyCfopService, _taxationNcmService);
                var importSped = new Sped.Import(_companyCfopService, _taxationNcmService);

                Model.Tax tax = new Model.Tax();

                List<Model.Grupo> addGrupos = new List<Grupo>();

                var imp = _service.FindByMonth(companyid,month,year);

                string arqu = "";

                if (type.Equals("xmlE"))
                {
                    arqu = "XML EMPRESA";
                }
                else if (type.Equals("xmlS"))
                {
                    arqu = "XML SEFAZ";
                }

                if(imp != null)
                {
                    if(arqu != "")
                    {
                        imp.Arquivo = arqu;
                    }

                    imp.Updated = DateTime.Now;
                }
                else
                {
                    if (arqu != "")
                    {
                        tax.Arquivo = arqu;
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
                    {
                        Directory.CreateDirectory(filedir);
                    }

                    string caminho_WebRoot = _appEnvironment.WebRootPath;
                    string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Speds\\";

                    int indice = 1;

                    foreach (var a in arquivo)
                    {

                        string nomeArquivo = comp.Document + indice.ToString() + year + month + ".txt";

                        string caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                        string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivo);

                        if (System.IO.File.Exists(caminhoDestinoArquivoOriginal))
                        {
                            System.IO.File.Delete(caminhoDestinoArquivoOriginal);
                        }

                        var stream = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);

                        await a.CopyToAsync(stream);

                        stream.Close();

                        caminhos.Add(caminhoDestinoArquivoOriginal);

                        indice++;
                    }
                }

                var cfopsDevoCompra = _companyCfopService.FindByCfopDevoCompra(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();
                var cfopsDevoVenda = _companyCfopService.FindByCfopDevoVenda(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();
                var cfopsCompra = _companyCfopService.FindByCfopCompra(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();
                var cfopsCompraST = _companyCfopService.FindByCfopCompraST(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();
                var cfopsVendaST = _companyCfopService.FindByCfopVendaST(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsVenda = _companyCfopService.FindByCfopVenda(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsTransf = _companyCfopService.FindByCfopTransferencia(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsTransfST = _companyCfopService.FindByCfopTransferenciaST(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsBoniVenda = _companyCfopService.FindByCfopBonificacaoVenda(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsBoniCompra = _companyCfopService.FindByCfopBonificacaoCompra(comp.Document).Select(_ => _.Cfop.Code).ToList();

                if (imposto.Equals("icms"))
                {
                    if (type.Equals("sped"))
                    {
                        decimal credito = 0;

                        foreach (var cc in caminhos)
                        {
                            credito = importSped.SpedCredito(cc, cfopsDevoVenda, cfopsCompra, cfopsBoniCompra, cfopsCompraST, cfopsTransf, cfopsTransfST);
                        }

                        if (imp != null)
                        {
                            imp.Credito = credito;
                        }
                        else
                        {
                            tax.Credito = credito;
                        }
                    }
                    else if (type.Equals("xmlS") || type.Equals("xmlE"))
                    {
                        List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                        List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();

                        if (comp.TipoApuracao.Equals(true))
                        {
                            exitNotes = importXml.Nfe(directoryNfeExit);
                            entryNotes = importXml.Nfe(directoryNfeEntry);

                            var ncms = _ncmConvenioService.FindByAnnex(Convert.ToInt32(comp.AnnexId));
                            var clientesAll = _clientService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid)).Select(_ => _.Document).ToList();
                            var suspensions = _suspensionService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid)).ToList();


                            var contribuintes = _clientService.FindByContribuinte(companyid, "all");
                            var contribuintesRaiz = _clientService.FindByContribuinte(companyid, "raiz");

                            decimal totalVendas = 0, totalNcm = 0, totalTranferencias = 0, totalSaida = 0, totalDevo = 0,
                                totalDevoAnexo = 0, totalDevoContribuinte = 0, totalVendasSuspensao = 0, totalTranferenciaInter = 0;
                            int contContribuintes = contribuintes.Count();
                            int contContribuintesRaiz = contribuintesRaiz.Count() + 1;

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


                            // Transferência Entrada
                            for (int i = entryNotes.Count - 1; i >= 0; i--)
                            {
                                if (!entryNotes[i][3]["CNPJ"].Equals(comp.Document))
                                {
                                    entryNotes.RemoveAt(i);
                                    continue;
                                }

                                bool cfop = false;

                                for (int j = 0; j < entryNotes[i].Count; j++)
                                {
                                    if (entryNotes[i][j].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfopsTransf.Contains(entryNotes[i][j]["CFOP"]) || cfopsTransfST.Contains(entryNotes[i][j]["CFOP"]))
                                        {
                                            cfop = true;
                                        }

                                    }
                                    if (cfop == true)
                                    {
                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vProd"))
                                        {
                                            if (entryNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            }
                                            totalTranferencias += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                        }
                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vFrete"))
                                        {
                                            if (entryNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            }
                                            totalTranferencias += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                        }
                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vDesc"))
                                        {
                                            if (entryNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            }
                                            totalTranferencias -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                        }
                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vOutro"))
                                        {
                                            if (entryNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            }
                                            totalTranferencias += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                        }
                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vSeg"))
                                        {
                                            if (entryNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            }
                                            totalTranferencias += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                        }
                                    }
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
                                bool suspenso = false,cfop = false; 

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

                                for (int k = 0; k < exitNotes[i].Count(); k++)
                                {
                                    if (exitNotes[i][k].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfopsVenda.Contains(exitNotes[i][k]["CFOP"]) || cfopsVendaST.Contains(exitNotes[i][k]["CFOP"]) ||
                                            cfopsBoniVenda.Contains(exitNotes[i][k]["CFOP"]) || cfopsTransf.Contains(exitNotes[i][k]["CFOP"]) ||
                                            cfopsTransfST.Contains(exitNotes[i][k]["CFOP"]))
                                        {
                                            cfop = true;
                                        }

                                    }

                                    if (exitNotes[i][k].ContainsKey("vProd") && exitNotes[i][k].ContainsKey("cProd") && suspenso == true && cfop == true)
                                    {
                                        totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                    }

                                    if (exitNotes[i][k].ContainsKey("vFrete") && exitNotes[i][k].ContainsKey("cProd") && suspenso == true && cfop == true)
                                    {

                                        totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);

                                    }

                                    if (exitNotes[i][k].ContainsKey("vDesc") && exitNotes[i][k].ContainsKey("cProd") && suspenso == true && cfop == true)
                                    {
                                        totalVendasSuspensao -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);

                                    }

                                    if (exitNotes[i][k].ContainsKey("vOutro") && exitNotes[i][k].ContainsKey("cProd") && suspenso == true && cfop == true)
                                    {
                                        totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);

                                    }

                                    if (exitNotes[i][k].ContainsKey("vSeg") && exitNotes[i][k].ContainsKey("cProd") && suspenso == true && cfop == true)
                                    {
                                        totalVendasSuspensao += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                    }

                                }

                            }

                            // Vendas 
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                int posClienteRaiz = contContribuintesRaiz - 1, posCliente = -1;

                                bool status = false, cfop = false;
                               
                                if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("IE") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][1]["mod"].Equals("55"))
                                {
                                    string CNPJ = exitNotes[i][3].ContainsKey("CNPJ") ? exitNotes[i][3]["CNPJ"] : "escon";
                                    string indIEDest = exitNotes[i][3].ContainsKey("indIEDest") ? exitNotes[i][3]["indIEDest"] : "escon";
                                    string IE = exitNotes[i][3].ContainsKey("IE") ? exitNotes[i][3]["IE"] : "escon";

                                    if (contribuintesRaiz.Contains(exitNotes[i][3]["CNPJ"].Substring(0, 8)))
                                    {
                                        posClienteRaiz = contribuintesRaiz.IndexOf(exitNotes[i][3]["CNPJ"].Substring(0, 8));
                                    }

                                    if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                    {
                                        posCliente = contribuintes.IndexOf(exitNotes[i][3]["CNPJ"]);
                                    }

                                    bool existe = false;

                                    if (clientesAll.Contains(exitNotes[i][3]["CNPJ"]))
                                    {
                                        existe = true;
                                    }

                                    if (existe == false)
                                    {
                                        ViewBag.Erro = 2;
                                        return View(comp);
                                    }
                                }

                                for (int k = 0; k < exitNotes[i].Count(); k++)
                                {

                                    if (exitNotes[i][k].ContainsKey("NCM"))
                                    {
                                        status = false;

                                        for (int j = 0; j < ncms.Count(); j++)
                                        {
                                            int tamanho = ncms[j].Length;

                                            if (ncms[j].Equals(exitNotes[i][k]["NCM"].Substring(0, tamanho)))
                                            {
                                                status = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (exitNotes[i][k].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfopsVenda.Contains(exitNotes[i][k]["CFOP"]) || cfopsVendaST.Contains(exitNotes[i][k]["CFOP"]) ||
                                            cfopsBoniVenda.Contains(exitNotes[i][k]["CFOP"]))
                                        {
                                            cfop = true;
                                        }

                                    }

                                    if (status == true && cfop == true)
                                    {

                                        if (exitNotes[i][k].ContainsKey("vProd") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                            totalNcm += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vFrete") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                            totalNcm += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vDesc") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                            totalNcm -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vOutro") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                            totalNcm += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vSeg") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                            totalNcm += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();

                                            }
                                        }
                                    }
                                    else if (status == false && cfop == true)
                                    {

                                        if (exitNotes[i][k].ContainsKey("vProd") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vProd"])).ToString();
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vFrete") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vFrete"])).ToString();
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vDesc") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][k]["vDesc"])).ToString();
                                            }
                                        }

                                        if (exitNotes[i][k].ContainsKey("vOutro") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vOutro"])).ToString();
                                            }

                                        }

                                        if (exitNotes[i][k].ContainsKey("vSeg") && exitNotes[i][k].ContainsKey("cProd"))
                                        {
                                            totalVendas += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                            resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();
                                            }
                                            if (posCliente >= 0)
                                            {
                                                resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][k]["vSeg"])).ToString();

                                            }
                                        }
                                    }

                                }

                            }

                            // Transferência Saida
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {
                                int posClienteRaiz = contContribuintesRaiz - 1;

                                if (exitNotes[i][3].ContainsKey("CNPJ"))
                                {
                                    if (contribuintesRaiz.Contains(exitNotes[i][3]["CNPJ"].Substring(0, 8)))
                                    {
                                        posClienteRaiz = contribuintesRaiz.IndexOf(exitNotes[i][3]["CNPJ"].Substring(0, 8));
                                    }
                                }

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

                                    if (cfop == true)
                                    {
                                        if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                            }
                                         
                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();

                                            }

                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferencias -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();

                                            }

                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();

                                            }
                                           
                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalTranferencias += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();

                                            }
                                           
                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                            {
                                                totalTranferenciaInter += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            }
                                        }
                                    }

                                }

                            }

                            // Devolução Saida
                            for (int i = exitNotes.Count - 1; i >= 0; i--)
                            {

                                if (exitNotes[i][1]["finNFe"] != "4" || exitNotes[i][1]["tpNF"] == "1")
                                {
                                    exitNotes.RemoveAt(i);
                                    continue;
                                }

                                bool existe = false, status = false, cfop = false;

                                if (exitNotes[i][3].ContainsKey("CNPJ") && exitNotes[i][3].ContainsKey("IE") && exitNotes[i][3].ContainsKey("indIEDest") && exitNotes[i][1]["mod"].Equals("55"))
                                {

                                    if (contribuintes.Contains(exitNotes[i][3]["CNPJ"]))
                                    {
                                        existe = true;
                                    }
                                }

                                for (int j = 0; j < exitNotes[i].Count(); j++)
                                {
                                    if (exitNotes[i][j].ContainsKey("NCM"))
                                    {
                                        status = false;

                                        for (int k = 0; k < ncms.Count(); k++)
                                        {
                                            int tamanho = ncms[k].Length;

                                            if (ncms[k].Equals(exitNotes[i][j]["NCM"].Substring(0, tamanho)))
                                            {
                                                status = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (exitNotes[i][j].ContainsKey("CFOP"))
                                    {
                                        cfop = false;
                                        if (cfopsDevoVenda.Contains(exitNotes[i][j]["CFOP"]))
                                        {
                                            cfop = true;
                                        }

                                    }

                                    if (status == true && cfop == true)
                                    {
                                        if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            totalDevoAnexo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            totalDevoAnexo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            }
                                        }
                                    }
                                    else if (status == false && cfop == true)
                                    {
                                        if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                            }

                                        }

                                        if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                        {

                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                            }
                                        }

                                        if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                            }
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

                                bool existe = false;
                                int posClienteRaiz = contContribuintesRaiz - 1;

                                if (entryNotes[i][2].ContainsKey("CNPJ") && entryNotes[i][3].ContainsKey("IE") && entryNotes[i][2].ContainsKey("indIEDest") && entryNotes[i][1]["mod"].Equals("55"))
                                {

                                    if (contribuintes.Contains(entryNotes[i][2]["CNPJ"]))
                                    {
                                        existe = true;
                                    }

                                    string CNPJ = entryNotes[i][2].ContainsKey("CNPJ") ? entryNotes[i][2]["CNPJ"] : "escon";
                                    string indIEDest = entryNotes[i][2].ContainsKey("indIEDest") ? entryNotes[i][2]["indIEDest"] : "escon";
                                    string IE = entryNotes[i][2].ContainsKey("IE") ? entryNotes[i][2]["IE"] : "escon";

                                    if (contribuintesRaiz.Contains(entryNotes[i][2]["CNPJ"].Substring(0, 8)))
                                    {
                                        posClienteRaiz = contribuintesRaiz.IndexOf(entryNotes[i][2]["CNPJ"].Substring(0, 8));
                                    }
                                }

                                bool status = false;

                                for (int j = 0; j < entryNotes[i].Count(); j++)
                                {

                                    if (entryNotes[i][j].ContainsKey("NCM"))
                                    {
                                        status = false;

                                        for (int k = 0; k < ncms.Count(); k++)
                                        {
                                            int tamanho = ncms[k].Length;

                                            if (ncms[k].Equals(entryNotes[i][j]["NCM"].Substring(0, tamanho)))
                                            {
                                                status = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (status == true)
                                    {
                                        if (entryNotes[i][j].ContainsKey("vProd") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            }
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("vFrete") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            }
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("vDesc") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            totalDevoAnexo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            }
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("vOutro") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            }

                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("vSeg") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            totalDevoAnexo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            }
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();
                                            }
                                        }
                                    }
                                    else if (status == false)
                                    {
                                        if (entryNotes[i][j].ContainsKey("vProd") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            }

                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                            }

                                        }

                                        if (entryNotes[i][j].ContainsKey("vFrete") && entryNotes[i][j].ContainsKey("cProd"))
                                        {

                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            }

                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();
                                            }

                                        }

                                        if (entryNotes[i][j].ContainsKey("vDesc") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            }

                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("vOutro") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            }
                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("vSeg") && entryNotes[i][j].ContainsKey("cProd"))
                                        {
                                            totalDevo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            if (existe == true)
                                            {
                                                totalDevoContribuinte += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            }

                                            if (posClienteRaiz < contContribuintesRaiz - 1)
                                            {
                                                resumoAllCnpjRaiz[posClienteRaiz, 2] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();
                                            }
                                        }
                                    }
                                }

                            }

                            decimal totalNcontribuinte = Convert.ToDecimal(resumoCnpjRaiz[contContribuintesRaiz - 1, 1]), baseCalc = totalVendas - totalDevo;
                            decimal totalContribuinte = totalVendas - totalNcontribuinte;
                            decimal baseCalcContribuinte = totalContribuinte - totalDevoContribuinte;
                            decimal totalDevoNContribuinte = totalDevo - totalDevoContribuinte;
                            decimal baseCalcNContribuinte = totalNcontribuinte - totalDevoNContribuinte;

                            totalSaida = baseCalc + totalTranferencias;
                            totalVendas = totalVendas + totalTranferencias;

                            decimal limiteGrupo = (totalSaida * Convert.ToDecimal(comp.VendaMGrupo)) / 100;

                            if (imp != null)
                            {
                                imp.Vendas = totalVendas;
                                imp.VendasNContribuinte = totalNcontribuinte;
                                imp.VendasNcm = totalNcm;
                                imp.TransferenciaInter = totalTranferenciaInter;
                                imp.Transferencia = totalTranferencias;
                                imp.Devolucao = totalDevo;
                                imp.DevolucaoNContribuinte = totalDevoNContribuinte;
                                imp.DevolucaoNcm = totalDevoAnexo;
                                imp.Suspensao = totalVendasSuspensao;

                                var grupoTemp = _grupoService.FindByGrupos(imp.Id);

                                if (grupoTemp != null)
                                {
                                    List<Model.Grupo> updateGrupos = new List<Grupo>();

                                    for (int i = 0; i < contContribuintesRaiz - 1; i++)
                                    {
                                        var vendaGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 1]);
                                        var devoGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 2]);
                                        var baseCalcGrupo = (vendaGrupo - devoGrupo);
                                        if (baseCalcGrupo > limiteGrupo)
                                        {
                                            List<string> grupoExcedente = new List<string>();
                                            var cnpjGrupo = resumoAllCnpjRaiz[i, 0];
                                            var clientGrupo = _clientService.FindByRaiz(cnpjGrupo);
                                            var nomeGrupo = clientGrupo.Name;
                                            var percentGrupo = Math.Round((baseCalcGrupo / totalSaida) * 100, 2);



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

                                    _grupoService.Create(addGrupos, null);
                                    _grupoService.Update(updateGrupos, null);
                                }
                                else
                                {
                                    for (int i = 0; i < contContribuintesRaiz - 1; i++)
                                    {
                                        var vendaGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 1]);
                                        var devoGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 2]);
                                        var baseCalcGrupo = (vendaGrupo - devoGrupo);
                                        if (baseCalcGrupo > limiteGrupo)
                                        {
                                            List<string> grupoExcedente = new List<string>();
                                            var cnpjGrupo = resumoAllCnpjRaiz[i, 0];
                                            var clientGrupo = _clientService.FindByRaiz(cnpjGrupo);
                                            var nomeGrupo = clientGrupo.Name;
                                            var percentGrupo = Math.Round((baseCalcGrupo / totalSaida) * 100, 2);

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
                            }
                            else
                            {
                                tax.Vendas = totalVendas;
                                tax.VendasNContribuinte = totalNcontribuinte;
                                tax.VendasNcm = totalNcm;
                                tax.TransferenciaInter = totalTranferenciaInter;
                                tax.Transferencia = totalTranferencias;
                                tax.Devolucao = totalDevo;
                                tax.DevolucaoNContribuinte = totalDevoNContribuinte;
                                tax.DevolucaoNcm = totalDevoAnexo;
                                tax.Suspensao = totalVendasSuspensao;

                                for (int i = 0; i < contContribuintesRaiz - 1; i++)
                                {
                                    var vendaGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 1]);
                                    var devoGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 2]);
                                    var baseCalcGrupo = (vendaGrupo - devoGrupo);
                                    if (baseCalcGrupo > limiteGrupo)
                                    {
                                        List<string> grupoExcedente = new List<string>();
                                        var cnpjGrupo = resumoAllCnpjRaiz[i, 0];
                                        var clientGrupo = _clientService.FindByRaiz(cnpjGrupo);
                                        var nomeGrupo = clientGrupo.Name;
                                        var percentGrupo = Math.Round((baseCalcGrupo / totalSaida) * 100, 2);

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
                        else
                        {
                            if (!comp.AnnexId.Equals(3))
                            {
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

                                    List<List<string>> icmsForaDoEstado = new List<List<string>>();

                                    var contribuintes = _clientService.FindByContribuinte(companyid, "all");
                                    var clientesAll = _clientService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid)).Select(_ => _.Document).ToList();

                                    exitNotes = importXml.Nfe(directoryNfeExit);

                                    decimal totalVendas = 0, naoContribuinteIncentivo = 0, naoContribuinteNIncetivo = 0, vendaCfopSTNaoContribuinteNIncetivo = 0, NaoContribuiteIsento = 0,
                                        naoContriForaDoEstadoIncentivo = 0, naoContriForaDoEstadoNIncentivo = 0, vendaCfopSTNaoContriForaDoEstadoNIncentivo = 0, NaoContribuinteForaDoEstadoIsento = 0,
                                        ContribuintesNIncentivo = 0, ContribuintesIncentivoAliqM25 = 0, ContribuintesIncentivo = 0, vendaCfopSTContribuintesNIncentivo = 0,
                                        ContribuinteIsento = 0, debitosIcms = 0;

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

                                            codeProdIncentivado = prodsTemp.Where(_ => _.TypeTaxation.Equals("Incentivado/Normal") || _.TypeTaxation.Equals("Incentivado/ST")).Select(_ => _.Code).ToList();
                                            codeProdST = prodsTemp.Where(_ => _.TypeTaxation.Equals("ST")).Select(_ => _.Code).ToList();
                                            codeProdIsento = prodsTemp.Where(_ => _.TypeTaxation.Equals("Isento")).Select(_ => _.Code).ToList();

                                            cestIncentivado = prodsTemp.Where(_ => _.TypeTaxation.Equals("Incentivado/Normal") || _.TypeTaxation.Equals("Incentivado/ST")).Select(_ => _.Cest).ToList();
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
                                            {
                                                posCliente = contribuintes.IndexOf(exitNotes[i][3]["CNPJ"]);
                                            }

                                            bool existe = false;
                                            if (clientesAll.Contains(exitNotes[i][3]["CNPJ"]))
                                            {
                                                existe = true;
                                            }

                                            if (existe == false)
                                            {
                                                ViewBag.Erro = 2;
                                                return View(comp);
                                            }
                                        }

                                        int posUf = -1;
                                        if (exitNotes[i][3].ContainsKey("UF") && exitNotes[i][1]["idDest"].Equals("2"))
                                        {

                                            for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                            {
                                                if (icmsForaDoEstado[j][0].Equals(exitNotes[i][3]["UF"]))
                                                {
                                                    posUf = j;
                                                }
                                            }

                                            if (posUf < 0)
                                            {
                                                List<string> uf = new List<string>();
                                                uf.Add(exitNotes[i][3]["UF"]);
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
                                                {
                                                    cest = exitNotes[i][k]["CEST"];
                                                }

                                                if (exitNotes[i][k].ContainsKey("CFOP"))
                                                {
                                                    cfop = false;
                                                    if (cfopsVenda.Contains(exitNotes[i][k]["CFOP"]) || cfopsBoniVenda.Contains(exitNotes[i][k]["CFOP"]))
                                                    {
                                                        cfop = true;
                                                    }

                                                }

                                                if (cfop == true)
                                                {
                                                    if (exitNotes[i][k].ContainsKey("vProd"))
                                                    {
                                                        valorProduto += Convert.ToDecimal(exitNotes[i][k]["vProd"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vFrete"))
                                                    {
                                                        valorProduto += Convert.ToDecimal(exitNotes[i][k]["vFrete"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vDesc"))
                                                    {
                                                        valorProduto -= Convert.ToDecimal(exitNotes[i][k]["vDesc"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vOutro"))
                                                    {
                                                        valorProduto += Convert.ToDecimal(exitNotes[i][k]["vOutro"]);
                                                    }

                                                    if (exitNotes[i][k].ContainsKey("vSeg"))
                                                    {
                                                        valorProduto += Convert.ToDecimal(exitNotes[i][k]["vSeg"]);
                                                    }
                                                }

                                            }

                                            if (exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
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

                                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                                            {
                                                                naoContriForaDoEstadoIncentivo += Convert.ToDecimal(valorProduto);
                                                                icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(valorProduto)).ToString();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (aliquota <= 25)
                                                            {
                                                                ContribuintesIncentivo += Convert.ToDecimal(valorProduto);
                                                            }
                                                            else
                                                            {
                                                                ContribuintesIncentivoAliqM25 += Convert.ToDecimal(valorProduto);
                                                            }

                                                        }
                                                        totalVendas += Convert.ToDecimal(valorProduto);

                                                    }
                                                    else if (codeProdST.Contains(cProd) && cestST.Contains(cest))
                                                    {
                                                        if (posCliente < 0)
                                                        {
                                                            naoContribuinteNIncetivo += Convert.ToDecimal(valorProduto);

                                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                                            {
                                                                naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(valorProduto);
                                                                icmsForaDoEstado[posUf][1] = (Convert.ToDecimal(icmsForaDoEstado[posUf][1]) + Convert.ToDecimal(valorProduto)).ToString();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            ContribuintesNIncentivo += Convert.ToDecimal(valorProduto);
                                                        }
                                                        totalVendas += Convert.ToDecimal(valorProduto);

                                                    }
                                                    else if (codeProdIsento.Contains(cProd) && cestIsento.Contains(cest))
                                                    {
                                                        if (posCliente < 0)
                                                        {
                                                            NaoContribuiteIsento += Convert.ToDecimal(valorProduto);

                                                            if (exitNotes[i][1]["idDest"].Equals("2"))
                                                            {
                                                                NaoContribuinteForaDoEstadoIsento += Convert.ToDecimal(valorProduto);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            ContribuinteIsento += Convert.ToDecimal(valorProduto);
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

                                            if (exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                            {
                                                debitosIcms += Convert.ToDecimal(exitNotes[i][k]["vFCP"]);
                                            }
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
                                            {
                                                posCliente = contribuintes.IndexOf(exitNotes[i][3]["CNPJ"]);
                                            }

                                            bool existe = false;
                                            if (clientesAll.Contains(exitNotes[i][3]["CNPJ"]))
                                            {
                                                existe = true;
                                            }

                                            if (existe == false)
                                            {
                                                ViewBag.Erro = 2;
                                                return View(comp);
                                            }
                                        }

                                        int posUf = -1;
                                        if (exitNotes[i][3].ContainsKey("UF") && exitNotes[i][1]["idDest"].Equals("2"))
                                        {

                                            for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                            {
                                                if (icmsForaDoEstado[j][0].Equals(exitNotes[i][3]["UF"]))
                                                {
                                                    posUf = j;
                                                }
                                            }

                                            if (posUf < 0)
                                            {
                                                List<string> uf = new List<string>();
                                                uf.Add(exitNotes[i][3]["UF"]);
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
                                            {
                                                cest = exitNotes[i][k]["CEST"];
                                            }

                                            if (exitNotes[i][k].ContainsKey("CFOP"))
                                            {
                                                cfop = false;
                                                if (cfopsVendaST.Contains(exitNotes[i][k]["CFOP"]))
                                                {
                                                    cfop = true;
                                                }

                                            }

                                            if (cfop == true)
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
                                                if (cfopsDevoCompra.Contains(exitNotes[i][k]["CFOP"]))
                                                {
                                                    cfop = true;
                                                }

                                            }

                                            if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
                                            {
                                                debitosIcms += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                            }
                                        }
                                    }


                                    if (imp != null)
                                    {
                                        imp.Debito = debitosIcms;
                                        imp.VendasContribuinte1 = ContribuintesIncentivo;
                                        imp.VendasContribuinte2 = ContribuintesIncentivoAliqM25;
                                        imp.ReceitaNormal1 = ContribuintesNIncentivo;
                                        imp.ReceitaST1 = vendaCfopSTContribuintesNIncentivo;
                                        imp.VendasNContribuinte = naoContribuinteIncentivo;
                                        imp.ReceitaNormal2 = naoContribuinteNIncetivo;
                                        imp.ReceitaST2 = vendaCfopSTNaoContribuinteNIncetivo;
                                        imp.VendasNContribuinteFora = naoContriForaDoEstadoIncentivo;
                                        imp.ReceitaNormal3 = naoContriForaDoEstadoNIncentivo;
                                        imp.ReceitaST3 = vendaCfopSTNaoContriForaDoEstadoNIncentivo;

                                        var grupoTemp = _grupoService.FindByGrupos(imp.Id);

                                        if (grupoTemp != null)
                                        {
                                            List<Model.Grupo> updateGrupos = new List<Grupo>();

                                            for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                            {

                                                decimal icms = (Convert.ToDecimal(comp.IcmsNContribuinteFora) * Convert.ToDecimal(icmsForaDoEstado[j][1])) / 100;
                                                if (icms > 0)
                                                {
                                                    var gg = grupoTemp.Where(_ => _.Uf.Equals(icmsForaDoEstado[j][0])).FirstOrDefault();

                                                    if (gg != null)
                                                    {
                                                        gg.Icms = icms;
                                                        gg.Updated = DateTime.Now;
                                                        updateGrupos.Add(gg);
                                                    }
                                                    else
                                                    {
                                                        Model.Grupo grupo = new Model.Grupo();
                                                        grupo.Uf = icmsForaDoEstado[j][0];
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

                                                decimal icms = (Convert.ToDecimal(comp.IcmsNContribuinteFora) * Convert.ToDecimal(icmsForaDoEstado[j][1])) / 100;
                                                if (icms > 0)
                                                {
                                                    Model.Grupo grupo = new Model.Grupo();
                                                    grupo.Uf = icmsForaDoEstado[j][0];
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
                                        tax.VendasContribuinte1 = ContribuintesIncentivo;
                                        tax.VendasContribuinte2 = ContribuintesIncentivoAliqM25;
                                        tax.ReceitaNormal1 = ContribuintesNIncentivo;
                                        tax.ReceitaST1 = vendaCfopSTContribuintesNIncentivo;
                                        tax.VendasNContribuinte = naoContribuinteIncentivo;
                                        tax.ReceitaNormal2 = naoContribuinteNIncetivo;
                                        tax.ReceitaST2 = vendaCfopSTNaoContribuinteNIncetivo;
                                        tax.VendasNContribuinteFora = naoContriForaDoEstadoIncentivo;
                                        tax.ReceitaNormal3 = naoContriForaDoEstadoNIncentivo;
                                        tax.ReceitaST3 = vendaCfopSTNaoContriForaDoEstadoNIncentivo;

                                        for (int j = 0; j < icmsForaDoEstado.Count(); j++)
                                        {

                                            decimal icms = (Convert.ToDecimal(comp.IcmsNContribuinteFora) * Convert.ToDecimal(icmsForaDoEstado[j][1])) / 100;
                                            if (icms > 0)
                                            {
                                                Model.Grupo grupo = new Model.Grupo();
                                                grupo.Uf = icmsForaDoEstado[j][0];
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
                                    List<List<string>> percentuaisIncentivado = new List<List<string>>();
                                    List<List<string>> percentuaisNIncentivado = new List<List<string>>();

                                    exitNotes = importXml.Nfe(directoryNfeExit);

                                    decimal vendasIncentivada = 0, vendasNIncentivada = 0, debitoIncetivo = 0, debitoNIncentivo = 0;

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
                                            //productincentivo = _productIncentivoService.FindByDate(comp.Id, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));
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

                                        for (int k = 0; k < exitNotes[i].Count(); k++)
                                        {
                                            if (exitNotes[i][k].ContainsKey("cProd"))
                                            {
                                                status = 3;
                                                percent = 0;

                                                string cest = "";
                                                if (exitNotes[i][k].ContainsKey("CEST"))
                                                {
                                                    cest = exitNotes[i][k]["CEST"];
                                                }

                                                if (exitNotes[i][k].ContainsKey("CFOP"))
                                                {
                                                    cfop = false;
                                                    if (cfopsVenda.Contains(exitNotes[i][k]["CFOP"]) || cfopsBoniVenda.Contains(exitNotes[i][k]["CFOP"]) ||
                                                        cfopsDevoCompra.Contains(exitNotes[i][k]["CFOP"]) || cfopsVendaST.Contains(exitNotes[i][k]["CFOP"]))
                                                    {
                                                        cfop = true;
                                                    }

                                                }

                                                if (cfop == true)
                                                {
                                                    if (codeProd.Contains(exitNotes[i][k]["cProd"]) && cestProd.Contains(cest))
                                                    {
                                                        if (codeProdIncentivado.Contains(exitNotes[i][k]["cProd"]) && cestIncentivado.Contains(cest))
                                                        {
                                                            status = 1;
                                                            var percentualIncentivado = Convert.ToDecimal(prodsTemp.Where(_ => _.Code.Equals(exitNotes[i][k]["cProd"])).ToList().Select(_ => _.Percentual).FirstOrDefault());
                                                            percent = percentualIncentivado;

                                                        }
                                                        else if (codeProdST.Contains(exitNotes[i][k]["cProd"]) && cestST.Contains(cest))
                                                        {
                                                            status = 2;
                                                            percent = 0;
                                                        }
                                                        else if (codeProdIsento.Contains(exitNotes[i][k]["cProd"]) && cestIsento.Contains(cest))
                                                        {
                                                            status = 3;
                                                            percent = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ViewBag.Erro = 3;
                                                        return View(comp);
                                                    }
                                                }


                                            }

                                            if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
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

                                                        debitoIncetivo += (((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) / 100) * percent) / 100);
                                                        vendasIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percent) / 100);

                                                        int indice = -1;
                                                        for (int j = 0; j < percentuaisNIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisNIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                            {
                                                                indice = j;
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
                                                        vendasNIncentivada += ((Convert.ToDecimal(exitNotes[i][k]["vICMS"]) * percentNIncentivado) / 100);

                                                    }
                                                    else
                                                    {
                                                        int pos = -1;
                                                        for (int j = 0; j < percentuaisIncentivado.Count(); j++)
                                                        {
                                                            if (percentuaisIncentivado[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                                            {
                                                                pos = j;
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
                                                        vendasIncentivada += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
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
                                                    vendasNIncentivada += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                                }
                                            }

                                            if (exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && cfop == true)
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
                                                    addGrupos.Add(grupo);
                                                }
                                            }
                                            _grupoService.Create(addGrupos, null);
                                            _grupoService.Update(updateGrupos, null);
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
                                                addGrupos.Add(grupo);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        tax.VendasIncentivada = vendasIncentivada;
                                        tax.VendasNIncentivada = vendasNIncentivada;

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
                            else if (comp.AnnexId.Equals(3))
                            {
                                var clientesAll = _clientService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid)).ToList();
                                var nContribuintes = clientesAll.Where(_ => _.TypeClientId.Equals(2)).Select(_ => _.Document).ToList();
                                var suspensions = _suspensionService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid)).ToList();

                                var cfopVenda = _companyCfopService.FindAll(null).Where(_ => _.CompanyId.Equals(companyid) && _.Active.Equals(true) && (_.CfopTypeId.Equals(1) || _.CfopTypeId.Equals(4) || _.CfopTypeId.Equals(5))).Select(_ => _.Cfop.Code).ToList();

                                if (comp.SectionId.Equals(2))
                                {
                                    exitNotes = importXml.Nfe(directoryNfeExit);

                                    decimal vendasInternasElencadas = 0, vendasInterestadualElencadas = 0, vendasInternasDeselencadas = 0, vendasInterestadualDeselencadas = 0,
                                        InternasElencadas = 0, InterestadualElencadas = 0, InternasElencadasPortaria = 0, InterestadualElencadasPortaria = 0,
                                        InternasDeselencadas = 0, InterestadualDeselencadas = 0, InternasDeselencadasPortaria = 0, InterestadualDeselencadasPortaria = 0,
                                        suspensao = 0, vendasClienteCredenciado = 0, vendas = 0;

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
                                            {
                                                clenteCredenciado = true;
                                            }

                                            bool existe = false;

                                            if (clientesAll.Select(_ => _.Document).Contains(exitNotes[i][3]["CNPJ"]))
                                            {
                                                existe = true;
                                            }

                                            if (existe == false)
                                            {
                                                ViewBag.Erro = 2;
                                                return View(comp);
                                            }
                                        }

                                        for (int j = 0; j < exitNotes[i].Count; j++)
                                        {
                                            if (exitNotes[i][j].ContainsKey("NCM"))
                                            {
                                                ncm = _itemService.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId), exitNotes[i][j]["NCM"].ToString());
                                            }

                                            if (exitNotes[i][j].ContainsKey("CFOP"))
                                            {
                                                cfop = false;
                                                if (cfopVenda.Contains(exitNotes[i][j]["CFOP"]) || cfopsVendaST.Contains(exitNotes[i][j]["CFOP"]) ||
                                                    cfopsBoniVenda.Contains(exitNotes[i][j]["CFOP"]))
                                                {
                                                    cfop = true;
                                                }
                                            }

                                            if (clenteCredenciado == true)
                                            {
                                                if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasClienteCredenciado += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }

                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                            
                                                        }
                                                    }
                                                    if (suspenso == true)
                                                    {
                                                        suspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasClienteCredenciado += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                    }

                                                    if (suspenso == true)
                                                    {
                                                        suspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasClienteCredenciado -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        vendas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                    }

                                                    if (suspenso == true)
                                                    {
                                                        suspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasClienteCredenciado += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                    }

                                                    if (suspenso == true)
                                                    {
                                                        suspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendasClienteCredenciado += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualElencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualElencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                    }

                                                    if (suspenso == true)
                                                    {
                                                        suspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);

                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                        }
                                                    }

                                                    if (suspenso == true)
                                                    {
                                                        suspensao += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }

                                                    if (suspenso == true)
                                                    {
                                                        suspensao += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualDeselencadas -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualDeselencadasPortaria -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                        }
                                                    }

                                                    if (suspenso == true)
                                                    {
                                                        suspensao -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                        }
                                                    }

                                                    if (suspenso == true)
                                                    {
                                                        suspensao += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][j].ContainsKey("cProd"))
                                                {
                                                    if (cfop == true)
                                                    {
                                                        vendas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (exitNotes[i][1]["idDest"] == "1")
                                                        {
                                                            vendasInternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                        else
                                                        {
                                                            vendasInterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                    }

                                                    if (exitNotes[i][1]["idDest"] == "1")
                                                    {
                                                        InternasDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (ncm == true)
                                                        {
                                                            InternasDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        InterestadualDeselencadas += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        if (ncm == true)
                                                        {
                                                            InterestadualDeselencadasPortaria += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                        }
                                                    }

                                                    if (suspenso == true)
                                                    {
                                                        suspensao += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }
                                                }
                                            }
                                        }

                                    }

                                    if (imp != null)
                                    {
                                        imp.VendasClientes = vendasClienteCredenciado;
                                        imp.VendasInternas1 = vendasInternasElencadas;
                                        imp.VendasInterestadual1 = vendasInterestadualElencadas;
                                        imp.SaidaInterna1 = InternasElencadas;
                                        imp.SaidaInterestadual1 = InterestadualElencadas;
                                        imp.SaidaPortInterna1 = InternasElencadasPortaria;
                                        imp.SaidaPortInterestadual1 = InterestadualElencadasPortaria;

                                        imp.VendasInternas2 = vendasInternasDeselencadas;
                                        imp.VendasInterestadual2 = vendasInterestadualDeselencadas;
                                        imp.SaidaInterna2 = InternasDeselencadas;
                                        imp.SaidaInterestadual2 = InterestadualDeselencadas;
                                        imp.SaidaPortInterna2 = InternasDeselencadasPortaria;
                                        imp.SaidaPortInterestadual2 = InterestadualDeselencadasPortaria;
                                        imp.Suspensao = suspensao;
                                    }
                                    else
                                    {
                                        tax.VendasClientes = vendasClienteCredenciado;
                                        tax.VendasInternas1 = vendasInternasElencadas;
                                        tax.VendasInterestadual1 = vendasInterestadualElencadas;
                                        tax.SaidaInterna1 = InternasElencadas;
                                        tax.SaidaInterestadual1 = InterestadualElencadas;
                                        tax.SaidaPortInterna1 = InternasElencadasPortaria;
                                        tax.SaidaPortInterestadual1 = InterestadualElencadasPortaria;

                                        tax.VendasInternas2 = vendasInternasDeselencadas;
                                        tax.VendasInterestadual2 = vendasInterestadualDeselencadas;
                                        tax.SaidaInterna2 = InternasDeselencadas;
                                        tax.SaidaInterestadual2 = InterestadualDeselencadas;
                                        tax.SaidaPortInterna2 = InternasDeselencadasPortaria;
                                        tax.SaidaPortInterestadual2 = InterestadualDeselencadasPortaria;
                                        tax.Suspensao = suspensao;
                                    }
                                }
                            }
                        }

                    }
                }
                else if (imposto.Equals("pisCofins"))
                {
                    var companies = _companyService.FindByCompanies(comp.Document);
                    var ncmsCompany = _taxationNcmService.FindByCompany(comp.Document);
                    
                    if (type.Equals("sped") && comp.Sped.Equals(true))
                    {
                        decimal  devolucaoComercio = 0, devolucaoServico = 0, devolucaoPetroleo = 0, devolucaoTransporte = 0, devolucaoNormal = 0;

                        foreach (var cc in caminhos)
                        {
                            var devolucoes = importSped.SpedDevolucao(cc, cfopsDevoCompra, ncmsCompany);
                            devolucaoPetroleo += devolucoes[0];
                            devolucaoComercio += devolucoes[1];
                            devolucaoTransporte += devolucoes[2];
                            devolucaoServico += devolucoes[3];
                            devolucaoNormal += devolucoes[4];
                        }


                        if (imp != null)
                        {
                            imp.Devolucao1Entrada = devolucaoPetroleo;
                            imp.Devolucao2Entrada = devolucaoComercio;
                            imp.Devolucao3Entrada = devolucaoTransporte;
                            imp.Devolucao4Entrada = devolucaoServico;
                            imp.DevolucaoNormalEntrada = devolucaoNormal;
                        }
                        else
                        {
                            tax.Devolucao1Entrada = devolucaoPetroleo;
                            tax.Devolucao2Entrada = devolucaoComercio;
                            tax.Devolucao3Entrada = devolucaoTransporte;
                            tax.Devolucao4Entrada = devolucaoServico;
                            tax.DevolucaoNormalEntrada = devolucaoNormal;
                        }
  

                    }
                    else if (type.Equals("xmlS") || type.Equals("xmlE"))
                    {
                        List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();
                        List<string> codeProdMono = new List<string>();
                        List<string> ncmMono = new List<string>();

                        var codeProdAll = ncmsCompany.Select(_ => _.CodeProduct).ToList();
                        var ncmAll = ncmsCompany.Select(_ => _.Ncm.Code).ToList();

                        var codeProd1 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(1)).Select(_ => _.CodeProduct).ToList();
                        var codeProd2 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(2)).Select(_ => _.CodeProduct).ToList();
                        var codeProd3 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(3)).Select(_ => _.CodeProduct).ToList();
                        var codeProd4 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(4)).Select(_ => _.CodeProduct).ToList();

                        var ncm1 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(1)).Select(_ => _.Ncm.Code).ToList();
                        var ncm2 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(2)).Select(_ => _.Ncm.Code).ToList();
                        var ncm3 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(3)).Select(_ => _.Ncm.Code).ToList();
                        var ncm4 = ncmsCompany.Where(_ => _.TypeNcmId.Equals(4)).Select(_ => _.Ncm.Code).ToList();

                        decimal receitaComercio = 0, devolucaoComercio = 0, receitaServico = 0, devolucaoServico = 0, receitaPetroleo = 0, devolucaoPetroleo = 0,
                            receitaTransporte = 0, devolucaoTransporte = 0, receitaMono = 0, devolucaoNoraml = 0;

                        foreach (var c in companies)
                        {
                            List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                            List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();
                            
                            directoryNfeExit = NfeExit.Value + "\\" + c.Document + "\\" + year + "\\" + month;
                            directoryNfeEntry = NfeEntry.Value + "\\" + c.Document + "\\" + year + "\\" + month;

                            exitNotes = importXml.Nfe(directoryNfeExit);

                            if (c.Sped.Equals(false))
                            {
                                entryNotes = importXml.Nfe(directoryNfeExit);
                            }

                            if (comp.CountingTypeId.Equals(1))
                            {

                            }
                            else if (comp.CountingTypeId.Equals(2))
                            {
                                // Receitas
                                for (int i = exitNotes.Count - 1; i >= 0; i--)
                                {
                                    if (!exitNotes[i][2]["CNPJ"].Equals(c.Document))
                                    {
                                        exitNotes.RemoveAt(i);
                                        continue;
                                    }

                                    if (exitNotes[i][1].ContainsKey("dhEmi"))
                                    {

                                        ncmsTaxation = _taxationNcmService.FindAllInDate(ncmsCompany, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));

                                        codeProdMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.CodeProduct).ToList();
                                        ncmMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.Ncm.Code).ToList();
                                    }

                                    bool cfop = false;

                                    for (int j = 0; j < exitNotes[i].Count; j++)
                                    {

                                        if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("NCM"))
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

                                            if (codeProdAll.Contains(exitNotes[i][j]["cProd"]) && ncmAll.Contains(exitNotes[i][j]["NCM"]))
                                            {
                                                if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vProd") && exitNotes[i][1]["finNFe"] != "4" && cfop == true)
                                                {
                                                    if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }
                                                    else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }
                                                    else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }
                                                    else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaServico += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }

                                                    if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaMono += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vFrete") && exitNotes[i][1]["finNFe"] != "4" && cfop == true)
                                                {

                                                    if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }
                                                    else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }
                                                    else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }
                                                    else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaServico += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }

                                                    if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaMono += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vDesc") && exitNotes[i][1]["finNFe"] != "4" && cfop == true)
                                                {
                                                    if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaPetroleo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }
                                                    else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaComercio -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }
                                                    else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaTransporte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }
                                                    else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaServico -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }

                                                    if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaMono -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vOutro") && exitNotes[i][1]["finNFe"] != "4" && cfop == true)
                                                {
                                                    if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }
                                                    else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }
                                                    else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }
                                                    else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaServico += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }

                                                    if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaMono += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vSeg") && exitNotes[i][1]["finNFe"] != "4" && cfop == true)
                                                {
                                                    if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaPetroleo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }
                                                    else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaComercio += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }
                                                    else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaTransporte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }
                                                    else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaServico += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }

                                                    if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        receitaMono += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }
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

                                        ncmsTaxation = _taxationNcmService.FindAllInDate(ncmsCompany, Convert.ToDateTime(exitNotes[i][1]["dhEmi"])).Where(_ => _.Company.CountingTypeId.Equals(2) || _.Company.CountingTypeId.Equals(3)).ToList();

                                        codeProdMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.CodeProduct).ToList();
                                        ncmMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.Ncm.Code).ToList();
                                    }

                                    for (int j = 0; j < exitNotes[i].Count; j++)
                                    {
                                        if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("NCM"))
                                        {
                                            if (codeProdAll.Contains(exitNotes[i][j]["cProd"]) && ncmAll.Contains(exitNotes[i][j]["NCM"]))
                                            {
                                                if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vProd"))
                                                {
                                                    if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }
                                                    else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }
                                                    else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }
                                                    else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }

                                                    if (!codeProdMono.Contains(exitNotes[i][j]["cProd"]) && !ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoNoraml += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vFrete"))
                                                {
                                                    if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }
                                                    else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }
                                                    else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }
                                                    else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }

                                                    if (!codeProdMono.Contains(exitNotes[i][j]["cProd"]) && !ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoNoraml += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vDesc"))
                                                {
                                                    if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoPetroleo -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }
                                                    else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoComercio -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }
                                                    else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoTransporte -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }
                                                    else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoServico -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }

                                                    if (!codeProdMono.Contains(exitNotes[i][j]["cProd"]) && !ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoNoraml -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vOutro"))
                                                {
                                                    if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }
                                                    else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }
                                                    else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }
                                                    else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }

                                                    if (!codeProdMono.Contains(exitNotes[i][j]["cProd"]) && !ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoNoraml += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);
                                                    }
                                                }

                                                if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("vSeg"))
                                                {
                                                    if (codeProd1.Contains(exitNotes[i][j]["cProd"]) && ncm1.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoPetroleo += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }
                                                    else if (codeProd2.Contains(exitNotes[i][j]["cProd"]) && ncm2.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoComercio += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }
                                                    else if (codeProd3.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoTransporte += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }
                                                    else if (codeProd4.Contains(exitNotes[i][j]["cProd"]) && ncm3.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoServico += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }

                                                    if (!codeProdMono.Contains(exitNotes[i][j]["cProd"]) && !ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                                    {
                                                        devolucaoNoraml += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                                    }
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

                                // Devoluções de Vendas
                                for (int i = entryNotes.Count - 1; i >= 0; i--)
                                {
                                    if (entryNotes[i][1]["finNFe"] != "4" || !entryNotes[i][3]["CNPJ"].Equals(c.Document) || exitNotes[i][1]["tpNF"] == "0")
                                    {
                                        entryNotes.RemoveAt(i);
                                        continue;
                                    }

                                    if (entryNotes[i][1].ContainsKey("dhEmi"))
                                    {

                                        ncmsTaxation = _taxationNcmService.FindAllInDate(ncmsCompany, Convert.ToDateTime(entryNotes[i][1]["dhEmi"]));

                                        codeProdMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.CodeProduct).ToList();
                                        ncmMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.Ncm.Code).ToList();
                                    }

                                    for (int j = 0; j < entryNotes[i].Count; j++)
                                    {
                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vProd"))
                                        {
                                            if (codeProd1.Contains(entryNotes[i][j]["cProd"]) && ncm1.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            }
                                            else if (codeProd2.Contains(entryNotes[i][j]["cProd"]) && ncm2.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            }
                                            else if (codeProd3.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            }
                                            else if (codeProd4.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            }

                                            if (!codeProdMono.Contains(entryNotes[i][j]["cProd"]) && !ncmMono.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoNoraml += Convert.ToDecimal(entryNotes[i][j]["vProd"]);
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vFrete"))
                                        {
                                            if (codeProd1.Contains(entryNotes[i][j]["cProd"]) && ncm1.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            }
                                            else if (codeProd2.Contains(entryNotes[i][j]["cProd"]) && ncm2.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            }
                                            else if (codeProd3.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            }
                                            else if (codeProd4.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            }

                                            if (!codeProdMono.Contains(entryNotes[i][j]["cProd"]) && !ncmMono.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoNoraml += Convert.ToDecimal(entryNotes[i][j]["vFrete"]);
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vDesc"))
                                        {
                                            if (codeProd1.Contains(entryNotes[i][j]["cProd"]) && ncm1.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoPetroleo -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            }
                                            else if (codeProd2.Contains(entryNotes[i][j]["cProd"]) && ncm2.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoComercio -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            }
                                            else if (codeProd3.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoTransporte -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            }
                                            else if (codeProd4.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoServico -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            }

                                            if (!codeProdMono.Contains(entryNotes[i][j]["cProd"]) && !ncmMono.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoNoraml -= Convert.ToDecimal(entryNotes[i][j]["vDesc"]);
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vOutro"))
                                        {
                                            if (codeProd1.Contains(entryNotes[i][j]["cProd"]) && ncm1.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            }
                                            else if (codeProd2.Contains(entryNotes[i][j]["cProd"]) && ncm2.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            }
                                            else if (codeProd3.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            }
                                            else if (codeProd4.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            }

                                            if (!codeProdMono.Contains(entryNotes[i][j]["cProd"]) && !ncmMono.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoNoraml += Convert.ToDecimal(entryNotes[i][j]["vOutro"]);
                                            }
                                        }

                                        if (entryNotes[i][j].ContainsKey("cProd") && entryNotes[i][j].ContainsKey("vSeg"))
                                        {
                                            if (codeProd1.Contains(entryNotes[i][j]["cProd"]) && ncm1.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoPetroleo += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            }
                                            else if (codeProd2.Contains(entryNotes[i][j]["cProd"]) && ncm2.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoComercio += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            }
                                            else if (codeProd3.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoTransporte += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            }
                                            else if (codeProd4.Contains(entryNotes[i][j]["cProd"]) && ncm3.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoServico += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            }

                                            if (!codeProdMono.Contains(entryNotes[i][j]["cProd"]) && !ncmMono.Contains(entryNotes[i][j]["NCM"]))
                                            {
                                                devolucaoNoraml += Convert.ToDecimal(entryNotes[i][j]["vSeg"]);
                                            }
                                        }

                                    }
                                }
                            }

                          
                        }


                        if (imp != null)
                        {
                            imp.Receita1 = receitaPetroleo;
                            imp.Receita2 = receitaComercio;
                            imp.Receita3 = receitaTransporte;
                            imp.Receita4 = receitaServico;
                            imp.ReceitaMono = receitaMono;
                            imp.Devolucao1Saida = devolucaoPetroleo;
                            imp.Devolucao2Saida = devolucaoComercio;
                            imp.Devolucao3Saida = devolucaoTransporte;
                            imp.Devolucao4Saida = devolucaoServico;
                            imp.DevolucaoNormalSaida = devolucaoNoraml;
                        }
                        else
                        {
                            tax.Receita1 = receitaPetroleo;
                            tax.Receita2 = receitaComercio;
                            tax.Receita3 = receitaTransporte;
                            tax.Receita4 = receitaServico;
                            tax.ReceitaMono = receitaMono;
                            tax.Devolucao1Saida = devolucaoPetroleo;
                            tax.Devolucao2Saida = devolucaoComercio;
                            tax.Devolucao3Saida = devolucaoTransporte;
                            tax.Devolucao4Saida = devolucaoServico;
                            tax.DevolucaoNormalSaida = devolucaoNoraml;
                        }
                    }
                }

                if (imp != null)
                {
                    _service.Update(imp, null);
                }
                else
                {
                    _service.Create(tax, null);
                    imp = _service.FindByMonth(companyid, month, year);

                    foreach(var g in addGrupos)
                    {
                        g.TaxId = imp.Id;
                    }
                    _grupoService.Create(addGrupos, null);
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
