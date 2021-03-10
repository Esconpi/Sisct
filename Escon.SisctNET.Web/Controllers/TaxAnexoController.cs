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
    public class TaxAnexoController : ControllerBaseSisctNET
    {
        private readonly ITaxAnexoService _service;
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly INcmConvenioService _ncmConvenioService;
        private readonly ICompanyCfopService _companyCfopService;
        private readonly ICompraAnexoService _compraAnexoService;
        private readonly IDevoClienteService _devoClienteService;
        private readonly IDevoFornecedorService _devoFornecedorService;
        private readonly IVendaAnexoService _vendaAnexoService;
        private readonly INoteService _noteService;
        private readonly IProductNoteService _productNoteService;
        private readonly IHostingEnvironment _appEnvironment;

        public TaxAnexoController(
            ITaxAnexoService service,
            ICompanyService companyService,
            IConfigurationService configurationService,
            INcmConvenioService ncmConvenioService,
            ICompanyCfopService companyCfopService,
            ICompraAnexoService compraAnexoService,
            IDevoClienteService devoClienteService,
            IDevoFornecedorService devoFornecedorService,
            IVendaAnexoService vendaAnexoService,
            INoteService noteService,
            IProductNoteService productNoteService,
            IHostingEnvironment env,
            IFunctionalityService functionalityService,
             IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Tax")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _ncmConvenioService = ncmConvenioService;
            _companyCfopService = companyCfopService;
            _compraAnexoService = compraAnexoService;
            _devoClienteService = devoClienteService;
            _devoFornecedorService = devoFornecedorService;
            _vendaAnexoService = vendaAnexoService;
            _noteService = noteService;
            _productNoteService = productNoteService;
            _appEnvironment = env;
        }

        public IActionResult Index(int id, string year, string month)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var company = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                ViewBag.Company = company;

                SessionManager.SetCompanyIdInSession(id);
                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);

                var result = _service.FindByMonth(id,month,year);

                List<Model.VendaAnexo> vendas = new List<Model.VendaAnexo>();
                List<Model.DevoFornecedor> devoFornecedors = new List<Model.DevoFornecedor>();
                List<Model.CompraAnexo> compras = new List<Model.CompraAnexo>();
                List<Model.DevoCliente> devoClientes = new List<Model.DevoCliente>();

                if (result != null)
                {
                    vendas = _vendaAnexoService.FindByVendasTax(result.Id).OrderBy(_ => _.Aliquota).ToList();
                    devoFornecedors = _devoFornecedorService.FindByDevoTax(result.Id).OrderBy(_ => _.Aliquota).ToList();
                    compras = _compraAnexoService.FindByComprasTax(result.Id).OrderBy(_ => _.Aliquota).ToList();
                    devoClientes = _devoClienteService.FindByDevoTax(result.Id).OrderBy(_ => _.Aliquota).ToList();
                }


                ViewBag.VendasInternas = vendas;
                ViewBag.DevoFornecedorInternas = devoFornecedors;
                ViewBag.ComprasInternas = compras;
                ViewBag.DevoClienteInternas = devoClientes;

                return View(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Import()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var company = _companyService.FindById(SessionManager.GetCompanyIdInSession(), GetLog(Model.OccorenceLog.Read));
                return View(company);

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Import(string type, IFormFile arqchive)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Tax")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                int companyid = SessionManager.GetCompanyIdInSession();
                string year = SessionManager.GetYearInSession();
                string month = SessionManager.GetMonthInSession();

                var comp = _companyService.FindById(companyid, GetLog(Model.OccorenceLog.Read));

                var NfeExit = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));

                var importDir = new Diretorio.Import();

                string directoryNfeExit = "",arqui = "";

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

                var importXml = new Xml.Import(_companyCfopService);
                var importSped = new Sped.Import(_companyCfopService, _ncmConvenioService);

                List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                List<List<Dictionary<string, string>>> entryNotes = new List<List<Dictionary<string, string>>>();


                //  Entrada
                var cfopsCompra = _companyCfopService.FindByCfopCompra(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();
                var cfopsCompraST = _companyCfopService.FindByCfopCompraST(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();
                var cfopsBoniCompra = _companyCfopService.FindByCfopBonificacaoCompra(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsDevoCompra = _companyCfopService.FindByCfopDevoCompra(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();
                var cfopsDevoCompraST = _companyCfopService.FindByCfopDevoCompraST(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();

                //  Saida
                var cfopsVenda = _companyCfopService.FindByCfopVenda(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsVendaST = _companyCfopService.FindByCfopVendaST(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsBoniVenda = _companyCfopService.FindByCfopBonificacaoVenda(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsDevoVenda = _companyCfopService.FindByCfopDevoVenda(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();
                var cfopsDevoVendaST = _companyCfopService.FindByCfopDevoVendaST(comp.Document).Select(_ => _.Cfop.Code).Distinct().ToList();

                //  Transferencia
                var cfopsTransf = _companyCfopService.FindByCfopTransferencia(comp.Document).Select(_ => _.Cfop.Code).ToList();
                var cfopsTransfST = _companyCfopService.FindByCfopTransferenciaST(comp.Document).Select(_ => _.Cfop.Code).ToList();

                var ncmConvenio = _ncmConvenioService.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId));

                var imp = _service.FindByMonth(companyid, month, year);

                Model.TaxAnexo taxAnexo = new Model.TaxAnexo();


                if (imp != null)
                {
                    if (arqui != "")
                    {
                        imp.Arquivo = arqui;
                    }
                    imp.Updated = DateTime.Now;
                }
                else
                {
                    if (arqui != "")
                    {
                        taxAnexo.Arquivo = arqui;
                    }
                    taxAnexo.CompanyId = companyid;
                    taxAnexo.MesRef = month;
                    taxAnexo.AnoRef = year;
                    taxAnexo.Created = DateTime.Now;
                    taxAnexo.Updated = taxAnexo.Created;
                }

                if (type.Equals("sped"))
                {
                    string filedir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Speds");

                    if (!Directory.Exists(filedir))
                        Directory.CreateDirectory(filedir);

                    string caminho_WebRoot = _appEnvironment.WebRootPath;
                    string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Speds\\";

                    string nomeArquivo = comp.Document + year + month + ".txt";

                    string caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                    string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivo);

                    if (System.IO.File.Exists(caminhoDestinoArquivoOriginal))
                        System.IO.File.Delete(caminhoDestinoArquivoOriginal);

                    var stream = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);

                    await arqchive.CopyToAsync(stream);

                    stream.Close();

                    if (comp.AnnexId.Equals(1))
                    {
                        var notes = _noteService.FindByNotes(companyid, year, month);
                        var products = _productNoteService.FindByProductsType(notes, Model.TypeTaxation.Nenhum).Where(_ => _.TaxationTypeId.Equals(1) && _.TaxationTypeId.Equals(3) && _.Incentivo.Equals(false)).ToList();

                        decimal baseCalcCompraInterestadual4 = products.Where(_ => _.Picms.Equals(4)).Sum(_ => _.Vbasecalc),
                                baseCalcCompraInterestadual7 = products.Where(_ => _.Picms.Equals(7)).Sum(_ => _.Vbasecalc), 
                                baseCalcCompraInterestadual12 = products.Where(_ => _.Picms.Equals(12)).Sum(_ => _.Vbasecalc),
                                icmsCompraInterestadual4 = products.Where(_ => _.Picms.Equals(4)).Sum(_ => _.Vicms),
                                icmsCompraInterestadual7 = products.Where(_ => _.Picms.Equals(7)).Sum(_ => _.Vicms),
                                icmsCompraInterestadual12 = products.Where(_ => _.Picms.Equals(12)).Sum(_ => _.Vicms);

                        var entradasInterna = importSped.NFeInternal(caminhoDestinoArquivoOriginal, cfopsCompra, cfopsBoniCompra, cfopsTransf, cfopsDevoVenda, ncmConvenio);
                        var devolucoesInterestadual = importSped.NFeDevolution(caminhoDestinoArquivoOriginal, cfopsDevoVenda, cfopsDevoVendaST, ncmConvenio);

                        if (imp != null)
                        {
                            imp.BaseCompra4 = baseCalcCompraInterestadual4;
                            imp.BaseCompra7 = baseCalcCompraInterestadual7;
                            imp.BaseCompra12 = baseCalcCompraInterestadual12;
                            imp.IcmsCompra4 = icmsCompraInterestadual4;
                            imp.IcmsCompra7 = icmsCompraInterestadual7;
                            imp.IcmsCompra12 = icmsCompraInterestadual12;

                            imp.BaseDevoCliente4 = Convert.ToDecimal(devolucoesInterestadual[0][0]);
                            imp.BaseDevoCliente12 = Convert.ToDecimal(devolucoesInterestadual[1][0]);
                            imp.IcmsDevoCliente4 = Convert.ToDecimal(devolucoesInterestadual[0][1]);
                            imp.IcmsDevoCliente12 = Convert.ToDecimal(devolucoesInterestadual[1][1]);

                            _service.Update(imp, null);

                            var compras = _compraAnexoService.FindByComprasTax(imp.Id);
                            var devoClientes = _devoClienteService.FindByDevoTax(imp.Id);

                            List<Model.CompraAnexo> compraAnexosAdd = new List<Model.CompraAnexo>();
                            List<Model.CompraAnexo> compraAnexosUpdate = new List<Model.CompraAnexo>();

                            foreach(var entrada in entradasInterna[0])
                            {
                                var cc = compras.Where(_ => _.Aliquota.Equals(Convert.ToDecimal(entrada[1]))).FirstOrDefault();

                                if(cc == null)
                                {
                                    Model.CompraAnexo compra = new Model.CompraAnexo();
                                    compra.TaxAnexoId = imp.Id;
                                    compra.Base = Convert.ToDecimal(entrada[0]);
                                    compra.Aliquota = Convert.ToDecimal(entrada[1]);
                                    compra.Icms = Convert.ToDecimal(entrada[2]);
                                    compra.Created = DateTime.Now;
                                    compra.Updated = compra.Created;
                                    compraAnexosAdd.Add(compra);
                                }
                                else
                                {
                                    cc.Base = Convert.ToDecimal(entrada[0]);
                                    cc.Aliquota = Convert.ToDecimal(entrada[1]);
                                    cc.Icms = Convert.ToDecimal(entrada[2]);
                                    cc.Updated = DateTime.Now;
                                    compraAnexosUpdate.Add(cc);
                                }
                            }

                            _compraAnexoService.Create(compraAnexosAdd);
                            _compraAnexoService.Update(compraAnexosUpdate);

                            List<Model.DevoCliente> devoClientesAdd = new List<Model.DevoCliente>();
                            List<Model.DevoCliente> devoClientesUpdate = new List<Model.DevoCliente>();

                            foreach(var entrada in entradasInterna[1])
                            {
                                var dd = devoClientes.Where(_ => _.Aliquota.Equals(Convert.ToDecimal(entrada[1]))).FirstOrDefault();

                                if(dd == null)
                                {
                                    Model.DevoCliente devoCliente = new Model.DevoCliente();
                                    devoCliente.TaxAnexoId = imp.Id;
                                    devoCliente.Base = Convert.ToDecimal(entrada[0]);
                                    devoCliente.Aliquota = Convert.ToDecimal(entrada[1]);
                                    devoCliente.Icms = Convert.ToDecimal(entrada[2]);
                                    devoCliente.Created = DateTime.Now;
                                    devoCliente.Updated = devoCliente.Created;
                                    devoClientesAdd.Add(devoCliente);
                                }
                                else
                                {
                                    dd.Base = Convert.ToDecimal(entrada[0]);
                                    dd.Aliquota = Convert.ToDecimal(entrada[1]);
                                    dd.Icms = Convert.ToDecimal(entrada[2]);
                                    dd.Updated = DateTime.Now;
                                    devoClientesUpdate.Add(dd);
                                }
                            }

                            _devoClienteService.Create(devoClientesAdd);
                            _devoClienteService.Update(devoClientesUpdate);
                        }
                        else
                        {
                            taxAnexo.BaseCompra4 = baseCalcCompraInterestadual4;
                            taxAnexo.BaseCompra7 = baseCalcCompraInterestadual7;
                            taxAnexo.BaseCompra12 = baseCalcCompraInterestadual12;
                            taxAnexo.IcmsCompra4 = icmsCompraInterestadual4;
                            taxAnexo.IcmsCompra7 = icmsCompraInterestadual7;
                            taxAnexo.IcmsCompra12 = icmsCompraInterestadual12;

                            taxAnexo.BaseDevoCliente4 = Convert.ToDecimal(devolucoesInterestadual[0][0]);
                            taxAnexo.BaseDevoCliente12 = Convert.ToDecimal(devolucoesInterestadual[1][0]);
                            taxAnexo.IcmsDevoCliente4 = Convert.ToDecimal(devolucoesInterestadual[0][1]);
                            taxAnexo.IcmsDevoCliente12 = Convert.ToDecimal(devolucoesInterestadual[1][1]);

                            _service.Create(taxAnexo, null);

                            //imp = _service.FindByMonth(companyid, month, year);

                            var compras = _compraAnexoService.FindByComprasTax(imp.Id);
                            var devoClientes = _devoClienteService.FindByDevoTax(imp.Id);

                            List<Model.CompraAnexo> compraAnexosAdd = new List<Model.CompraAnexo>();
                            List<Model.CompraAnexo> compraAnexosUpdate = new List<Model.CompraAnexo>();

                            foreach (var entrada in entradasInterna[0])
                            {
                                var cc = compras.Where(_ => _.Aliquota.Equals(Convert.ToDecimal(entrada[1]))).FirstOrDefault();

                                if (cc == null)
                                {
                                    Model.CompraAnexo compra = new Model.CompraAnexo();
                                    compra.TaxAnexoId = imp.Id;
                                    compra.Base = Convert.ToDecimal(entrada[0]);
                                    compra.Aliquota = Convert.ToDecimal(entrada[1]);
                                    compra.Icms = Convert.ToDecimal(entrada[2]);
                                    compra.Created = DateTime.Now;
                                    compra.Updated = compra.Created;
                                    compraAnexosAdd.Add(compra);
                                }
                                else
                                {
                                    cc.Base = Convert.ToDecimal(entrada[0]);
                                    cc.Aliquota = Convert.ToDecimal(entrada[1]);
                                    cc.Icms = Convert.ToDecimal(entrada[2]);
                                    cc.Updated = DateTime.Now;
                                    compraAnexosUpdate.Add(cc);
                                }
                            }

                            _compraAnexoService.Create(compraAnexosAdd);
                            _compraAnexoService.Update(compraAnexosUpdate);

                            List<Model.DevoCliente> devoClientesAdd = new List<Model.DevoCliente>();
                            List<Model.DevoCliente> devoClientesUpdate = new List<Model.DevoCliente>();

                            foreach (var entrada in entradasInterna[1])
                            {
                                var dd = devoClientes.Where(_ => _.Aliquota.Equals(Convert.ToDecimal(entrada[1]))).FirstOrDefault();

                                if (dd == null)
                                {
                                    Model.DevoCliente devoCliente = new Model.DevoCliente();
                                    devoCliente.TaxAnexoId = imp.Id;
                                    devoCliente.Base = Convert.ToDecimal(entrada[0]);
                                    devoCliente.Aliquota = Convert.ToDecimal(entrada[1]);
                                    devoCliente.Icms = Convert.ToDecimal(entrada[2]);
                                    devoCliente.Created = DateTime.Now;
                                    devoCliente.Updated = devoCliente.Created;
                                    devoClientesAdd.Add(devoCliente);
                                }
                                else
                                {
                                    dd.Base = Convert.ToDecimal(entrada[0]);
                                    dd.Aliquota = Convert.ToDecimal(entrada[1]);
                                    dd.Icms = Convert.ToDecimal(entrada[2]);
                                    dd.Updated = DateTime.Now;
                                    devoClientesUpdate.Add(dd);
                                }
                            }

                            _devoClienteService.Create(devoClientesAdd);
                            _devoClienteService.Update(devoClientesUpdate);
                        }
                    }
                }
                else
                {
                    if (comp.AnnexId.Equals(1))
                    {
                        exitNotes = importXml.NFeAll(directoryNfeExit);

                        decimal baseCalcVendaInterestadual4 = 0, baseCalcVendaInterestadual7 = 0, baseCalcVendaInterestadual12 = 0,
                           icmsVendaInterestadual4 = 0, icmsVendaInterestadual7 = 0, icmsVendaInterestadual12 = 0,
                           baseCalcDevoFornecedorInterestadual4 = 0, baseCalcDevoFornecedorInterestadual7 = 0, baseCalcDevoFornecedorInterestadual12 = 0,
                           icmsDevoFornecedorInterestadual4 = 0, icmsDevoFornecedorInterestadual7 = 0, icmsDevoFornecedorInterestadual12 = 0;

                        List<List<string>> devoFornecedorInterna = new List<List<string>>();
                        List<List<string>> vendaInterna = new List<List<string>>();

                        // Vendas
                        for (int i = exitNotes.Count - 1; i >= 0; i--)
                        {
                            if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document))
                            {
                                exitNotes.RemoveAt(i);
                                continue;
                            }

                            bool ncm = false, cfop = false;

                            for (int j = 0; j < exitNotes[i].Count(); j++)
                            {
                                if (exitNotes[i][j].ContainsKey("CFOP"))
                                {
                                    cfop = false;

                                    if (cfopsVenda.Contains(exitNotes[i][j]["CFOP"]) || cfopsTransf.Contains(exitNotes[i][j]["CFOP"]))
                                        cfop = true;
                                }

                                if (exitNotes[i][j].ContainsKey("NCM"))
                                    ncm = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, exitNotes[i][j]["NCM"]);

                                if (exitNotes[i][j].ContainsKey("pICMS") && !exitNotes[i][j].ContainsKey("pFCP") && exitNotes[i][j].ContainsKey("CST") &&
                                    exitNotes[i][j].ContainsKey("orig") && exitNotes[i][1]["finNFe"] != "4" && !ncm && cfop)
                                {
                                    if (exitNotes[i][1]["idDest"].Equals("1"))
                                    {
                                        int pos = -1;
                                        for (int k = 0; k < vendaInterna.Count(); k++)
                                        {
                                            if (vendaInterna[k][1].Equals(exitNotes[i][j]["pICMS"]))
                                            {
                                                pos = k;
                                                break;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> cc = new List<string>();
                                            cc.Add(exitNotes[i][j]["vBC"]);
                                            cc.Add(exitNotes[i][j]["pICMS"]);
                                            cc.Add(exitNotes[i][j]["vICMS"]);
                                            vendaInterna.Add(cc);
                                        }
                                        else
                                        {
                                            vendaInterna[pos][0] = (Convert.ToDecimal(vendaInterna[pos][0]) + Convert.ToDecimal(exitNotes[i][j]["vBC"])).ToString();
                                            vendaInterna[pos][2] = (Convert.ToDecimal(vendaInterna[pos][2]) + Convert.ToDecimal(exitNotes[i][j]["vICMS"])).ToString();
                                        }

                                    }
                                    else
                                    {
                                        if (Convert.ToDecimal(exitNotes[i][j]["pICMS"]).Equals(4))
                                        {
                                            baseCalcVendaInterestadual4 += Convert.ToDecimal(exitNotes[i][j]["vBC"]);
                                            icmsVendaInterestadual4 += Convert.ToDecimal(exitNotes[i][j]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][j]["pICMS"]).Equals(7))
                                        {
                                            baseCalcVendaInterestadual7 += Convert.ToDecimal(exitNotes[i][j]["vBC"]);
                                            icmsVendaInterestadual7 += Convert.ToDecimal(exitNotes[i][j]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][j]["pICMS"]).Equals(12))
                                        {
                                            baseCalcVendaInterestadual12 += Convert.ToDecimal(exitNotes[i][j]["vBC"]);
                                            icmsVendaInterestadual12 += Convert.ToDecimal(exitNotes[i][j]["vICMS"]);
                                        }
                                    }
                                }

                                if (exitNotes[i][j].ContainsKey("pICMS") && exitNotes[i][j].ContainsKey("pFCP") && exitNotes[i][j].ContainsKey("CST") && 
                                    exitNotes[i][j].ContainsKey("orig") && exitNotes[i][1]["finNFe"] != "4" && !ncm && cfop)
                                {
                                    if (exitNotes[i][1]["idDest"].Equals("1"))
                                    {
                                        int pos = -1;
                                        for (int k = 0; k < vendaInterna.Count(); k++)
                                        {
                                            if (vendaInterna[k][1].Equals((Math.Round(Convert.ToDecimal(exitNotes[i][j]["pICMS"]) + Convert.ToDecimal(exitNotes[i][j]["pFCP"]), 2)).ToString()))
                                            {
                                                pos = k;
                                                break;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> cc = new List<string>();
                                            cc.Add(exitNotes[i][j]["vBC"]);
                                            cc.Add((Math.Round(Convert.ToDecimal(exitNotes[i][j]["pICMS"]) + Convert.ToDecimal(exitNotes[i][j]["pFCP"]), 2)).ToString());
                                            cc.Add(exitNotes[i][j]["vICMS"]);
                                            vendaInterna.Add(cc);
                                        }
                                        else
                                        {
                                            vendaInterna[pos][0] = (Convert.ToDecimal(vendaInterna[pos][0]) + Convert.ToDecimal(exitNotes[i][j]["vBC"])).ToString();
                                            vendaInterna[pos][2] = (Convert.ToDecimal(vendaInterna[pos][2]) + (Convert.ToDecimal(exitNotes[i][j]["vICMS"]) + Convert.ToDecimal(exitNotes[i][j]["vFCP"]))).ToString();
                                        }

                                    }
                                    else
                                    {
                                        if (Convert.ToDecimal(exitNotes[i][j]["pICMS"]).Equals(4))
                                        {
                                            baseCalcVendaInterestadual4 += Convert.ToDecimal(exitNotes[i][j]["vBC"]);
                                            icmsVendaInterestadual4 += Convert.ToDecimal(exitNotes[i][j]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][j]["pICMS"]).Equals(7))
                                        {
                                            baseCalcVendaInterestadual7 += Convert.ToDecimal(exitNotes[i][j]["vBC"]);
                                            icmsVendaInterestadual7 += Convert.ToDecimal(exitNotes[i][j]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][j]["pICMS"]).Equals(12))
                                        {
                                            baseCalcVendaInterestadual12 += Convert.ToDecimal(exitNotes[i][j]["vBC"]);
                                            icmsVendaInterestadual12 += Convert.ToDecimal(exitNotes[i][j]["vICMS"]);
                                        }
                                    }
                                }
                            }
                        }

                        // Devolução a Fornecedor
                        for (int i = exitNotes.Count - 1; i >= 0; i--)
                        {
                            if (exitNotes[i][1]["finNFe"] != "4" || exitNotes[i][1]["tpNF"] == "0")
                            {
                                exitNotes.RemoveAt(i);
                                continue;
                            }

                            bool ncm = false;

                            for (int k = 0; k < exitNotes[i].Count(); k++)
                            {
                                if (exitNotes[i][k].ContainsKey("NCM"))
                                    ncm = _ncmConvenioService.FindByNcmAnnex(ncmConvenio, exitNotes[i][k]["NCM"]);

                                if (exitNotes[i][k].ContainsKey("pICMS") && !exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && !ncm)
                                {
                                    if (exitNotes[i][1]["idDest"].Equals("1"))
                                    {
                                        int pos = -1;
                                        for (int j = 0; j < devoFornecedorInterna.Count(); j++)
                                        {
                                            if (devoFornecedorInterna[j][1].Equals(exitNotes[i][k]["pICMS"]))
                                            {
                                                pos = j;
                                                break;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> cc = new List<string>();
                                            cc.Add(exitNotes[i][k]["vBC"]);
                                            cc.Add(exitNotes[i][k]["pICMS"]);
                                            cc.Add(exitNotes[i][k]["vICMS"]);
                                            devoFornecedorInterna.Add(cc);
                                        }
                                        else
                                        {
                                            devoFornecedorInterna[pos][0] = (Convert.ToDecimal(devoFornecedorInterna[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                            devoFornecedorInterna[pos][2] = (Convert.ToDecimal(devoFornecedorInterna[pos][2]) + (Convert.ToDecimal(exitNotes[i][k]["vICMS"]))).ToString();
                                        }

                                    }
                                    else
                                    {
                                        if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(4))
                                        {
                                            baseCalcDevoFornecedorInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsDevoFornecedorInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(7))
                                        {
                                            baseCalcDevoFornecedorInterestadual7 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsDevoFornecedorInterestadual7 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(12))
                                        {
                                            baseCalcDevoFornecedorInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsDevoFornecedorInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                    }
                                }

                                if (exitNotes[i][k].ContainsKey("pICMS") && exitNotes[i][k].ContainsKey("pFCP") && exitNotes[i][k].ContainsKey("CST") && exitNotes[i][k].ContainsKey("orig") && !ncm)
                                {
                                    if (exitNotes[i][1]["idDest"].Equals("1"))
                                    {
                                        int pos = -1;
                                        for (int j = 0; j < devoFornecedorInterna.Count(); j++)
                                        {
                                            if (devoFornecedorInterna[j][1].Equals((Math.Round(Convert.ToDecimal(exitNotes[i][k]["pICMS"]) + Convert.ToDecimal(exitNotes[i][k]["pFCP"]), 2)).ToString()))
                                            {
                                                pos = j;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            List<string> cc = new List<string>();
                                            cc.Add(exitNotes[i][k]["vBC"]);
                                            cc.Add((Math.Round(Convert.ToDecimal(exitNotes[i][k]["pICMS"]) + Convert.ToDecimal(exitNotes[i][k]["pFCP"]), 2)).ToString());
                                            cc.Add(exitNotes[i][k]["vICMS"]);
                                            devoFornecedorInterna.Add(cc);
                                        }
                                        else
                                        {
                                            devoFornecedorInterna[pos][0] = (Convert.ToDecimal(devoFornecedorInterna[pos][0]) + Convert.ToDecimal(exitNotes[i][k]["vBC"])).ToString();
                                            devoFornecedorInterna[pos][2] = (Convert.ToDecimal(devoFornecedorInterna[pos][2]) + (Convert.ToDecimal(exitNotes[i][k]["vICMS"]) + Convert.ToDecimal(exitNotes[i][k]["vFCP"]))).ToString();
                                        }

                                    }
                                    else
                                    {
                                        if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(4))
                                        {
                                            baseCalcDevoFornecedorInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsDevoFornecedorInterestadual4 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(7))
                                        {
                                            baseCalcDevoFornecedorInterestadual7 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsDevoFornecedorInterestadual7 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                        else if (Convert.ToDecimal(exitNotes[i][k]["pICMS"]).Equals(12))
                                        {
                                            baseCalcDevoFornecedorInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vBC"]);
                                            icmsDevoFornecedorInterestadual12 += Convert.ToDecimal(exitNotes[i][k]["vICMS"]);
                                        }
                                    }
                                }
                            }
                        }


                        if (imp != null)
                        {
                            imp.BaseVenda4 = baseCalcVendaInterestadual4;
                            imp.BaseVenda7 = baseCalcVendaInterestadual7;
                            imp.BaseVenda12 = baseCalcVendaInterestadual12;
                            imp.IcmsVenda4 = icmsVendaInterestadual4;
                            imp.IcmsVenda7 = icmsVendaInterestadual7;
                            imp.IcmsVenda12 = icmsVendaInterestadual12;

                            imp.BaseDevoFornecedor4 = baseCalcDevoFornecedorInterestadual4;
                            imp.BaseDevoFornecedor7 = baseCalcDevoFornecedorInterestadual7;
                            imp.BaseDevoFornecedor12 = baseCalcDevoFornecedorInterestadual12;
                            imp.IcmsDevoFornecedor4 = icmsDevoFornecedorInterestadual4;
                            imp.IcmsDevoFornecedor7 = icmsDevoFornecedorInterestadual7;
                            imp.IcmsDevoFornecedor12 = icmsDevoFornecedorInterestadual12;

                            _service.Update(imp, null);

                            var vendas = _vendaAnexoService.FindByVendasTax(imp.Id);
                            var devoFornecedors = _devoFornecedorService.FindByDevoTax(imp.Id);

                            List<Model.VendaAnexo> vendaAnexosAdd = new List<Model.VendaAnexo>();
                            List<Model.VendaAnexo> vendaAnexosUpdate = new List<Model.VendaAnexo>();

                            List<Model.DevoFornecedor> devoFornecedorsAdd = new List<Model.DevoFornecedor>();
                            List<Model.DevoFornecedor> devoFornecedorsUpdate = new List<Model.DevoFornecedor>();

                            for (int i = 0; i < vendaInterna.Count(); i++)
                            {
                                
                                var venda = vendas.Where(_ => _.Aliquota.Equals(Convert.ToDecimal(vendaInterna[i][1]))).FirstOrDefault();

                                if (venda == null)
                                {
                                    Model.VendaAnexo vendaAnexo = new Model.VendaAnexo();
                                    vendaAnexo.TaxAnexoId = imp.Id;
                                    vendaAnexo.Base = Convert.ToDecimal(vendaInterna[i][0]);
                                    vendaAnexo.Aliquota = Convert.ToDecimal(vendaInterna[i][1]);
                                    vendaAnexo.Icms = Convert.ToDecimal(vendaInterna[i][2]);
                                    vendaAnexo.Created = DateTime.Now;
                                    vendaAnexo.Updated = vendaAnexo.Created;
                                    vendaAnexosAdd.Add(vendaAnexo);
                                }
                                else
                                {
                                    venda.Base = Convert.ToDecimal(vendaInterna[i][0]);
                                    venda.Aliquota = Convert.ToDecimal(vendaInterna[i][1]);
                                    venda.Icms = Convert.ToDecimal(vendaInterna[i][2]);
                                    venda.Updated = DateTime.Now;
                                    vendaAnexosUpdate.Add(venda);
                                }
                            }

                            _vendaAnexoService.Create(vendaAnexosAdd);
                            _vendaAnexoService.Update(vendaAnexosUpdate);

                            for (int i = 0; i < devoFornecedorInterna.Count(); i++)
                            {
                                var devoForne = devoFornecedors.Where(_ => _.Aliquota.Equals(Convert.ToDecimal(devoFornecedorInterna[i][1]))).FirstOrDefault();

                                if (devoForne == null)
                                {
                                    Model.DevoFornecedor devoFornecedor = new Model.DevoFornecedor();
                                    devoFornecedor.TaxAnexoId = imp.Id;
                                    devoFornecedor.Base = Convert.ToDecimal(devoFornecedorInterna[i][0]);
                                    devoFornecedor.Aliquota = Convert.ToDecimal(devoFornecedorInterna[i][1]);
                                    devoFornecedor.Icms = Convert.ToDecimal(devoFornecedorInterna[i][2]);
                                    devoFornecedor.Created = DateTime.Now;
                                    devoFornecedor.Updated = devoFornecedor.Created;
                                    devoFornecedorsAdd.Add(devoFornecedor);
                                }
                                else
                                {
                                    devoForne.Base = Convert.ToDecimal(devoFornecedorInterna[i][0]);
                                    devoForne.Aliquota = Convert.ToDecimal(devoFornecedorInterna[i][1]);
                                    devoForne.Icms = Convert.ToDecimal(devoFornecedorInterna[i][2]);
                                    devoForne.Updated = DateTime.Now;
                                    devoFornecedorsUpdate.Add(devoForne);
                                }
                            }

                            _devoFornecedorService.Create(devoFornecedorsAdd);
                            _devoFornecedorService.Update(devoFornecedorsUpdate);

                        }
                        else
                        {
                            taxAnexo.BaseVenda4 = baseCalcVendaInterestadual4;
                            taxAnexo.BaseVenda7 = baseCalcVendaInterestadual7;
                            taxAnexo.BaseVenda12 = baseCalcVendaInterestadual12;
                            taxAnexo.IcmsVenda4 = icmsVendaInterestadual4;
                            taxAnexo.IcmsVenda7 = icmsVendaInterestadual7;
                            taxAnexo.IcmsVenda12 = icmsVendaInterestadual12;

                            taxAnexo.BaseDevoFornecedor4 = baseCalcDevoFornecedorInterestadual4;
                            taxAnexo.BaseDevoFornecedor7 = baseCalcDevoFornecedorInterestadual7;
                            taxAnexo.BaseDevoFornecedor12 = baseCalcDevoFornecedorInterestadual12;
                            taxAnexo.IcmsDevoFornecedor4 = icmsDevoFornecedorInterestadual4;
                            taxAnexo.IcmsDevoFornecedor7 = icmsDevoFornecedorInterestadual7;
                            taxAnexo.IcmsDevoFornecedor12 = icmsDevoFornecedorInterestadual12;

                            _service.Create(taxAnexo, null);

                            imp = _service.FindByMonth(companyid, month, year);

                            List<Model.VendaAnexo> vendaAnexos = new List<Model.VendaAnexo>();
                            List<Model.DevoFornecedor> devoFornecedors = new List<Model.DevoFornecedor>();

                            for (int i = 0; i < vendaInterna.Count(); i++)
                            {
                                Model.VendaAnexo vendaAnexo = new Model.VendaAnexo();
                                vendaAnexo.TaxAnexoId = imp.Id;
                                vendaAnexo.Base = Convert.ToDecimal(vendaInterna[i][0]);
                                vendaAnexo.Aliquota = Convert.ToDecimal(vendaInterna[i][1]);
                                vendaAnexo.Icms = Convert.ToDecimal(vendaInterna[i][2]);
                                vendaAnexo.Created = DateTime.Now;
                                vendaAnexo.Updated = vendaAnexo.Created;
                                vendaAnexos.Add(vendaAnexo);
                            }

                            for (int i = 0; i < devoFornecedorInterna.Count(); i++)
                            {
                                Model.DevoFornecedor devoFornecedor = new Model.DevoFornecedor();
                                devoFornecedor.TaxAnexoId = imp.Id;
                                devoFornecedor.Base = Convert.ToDecimal(devoFornecedorInterna[i][0]);
                                devoFornecedor.Aliquota = Convert.ToDecimal(devoFornecedorInterna[i][1]);
                                devoFornecedor.Icms = Convert.ToDecimal(devoFornecedorInterna[i][2]);
                                devoFornecedor.Created = DateTime.Now;
                                devoFornecedor.Updated = devoFornecedor.Created;
                                devoFornecedors.Add(devoFornecedor);
                            }

                            _vendaAnexoService.Create(vendaAnexos);
                            _devoFornecedorService.Create(devoFornecedors);
                        }
                    }
                    
                }

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                return RedirectToAction("Index", new { id = companyid, year = year, month = month });

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

    }
}
