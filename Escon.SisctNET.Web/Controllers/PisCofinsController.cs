using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Escon.SisctNET.Web.Controllers
{
    public class PisCofinsController : ControllerBaseSisctNET
    {
        private readonly ITaxationNcmService _service;
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly ICompanyCfopService _companyCfopService;
        private readonly INcmService _ncmService;
        private readonly ITaxService _taxService;
        private readonly IBaseService _baseService;

        public PisCofinsController(
            ITaxationNcmService service,
            ICompanyService companyService,
            IConfigurationService configurationService,
            ICompanyCfopService companyCfopService,
            INcmService ncmService,
            ITaxService taxService,
            IBaseService baseService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "NoteExit")
        {
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _companyCfopService = companyCfopService;
            _ncmService = ncmService;
            _taxService = taxService;
            _baseService = baseService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult RelatoryExit(int id, string year, string month,string trimestre, string type)
        {
            if (SessionManager.GetLoginInSession().Equals(null))
            {
                return Unauthorized();
            }

            try
            {
                var comp = _companyService.FindById(id, null);

                ViewBag.Year = year;
                ViewBag.Month = month;
                ViewBag.SocialName = comp.SocialName;
                ViewBag.Document = comp.Document;

                var NfeExit = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));
                var NfeEntry = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                var importXml = new Xml.Import(_companyCfopService, _service);
                var importSped = new Sped.Import(_companyCfopService, _service);
                var importPeriod = new Period.Trimestre();

                string directoryNfeExit = NfeExit.Value + "\\" + comp.Document + "\\" + year + "\\" + month;
                string directoryNfeEntry = NfeEntry.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                ViewBag.Type = type;
                ViewBag.Trimestre = trimestre;

                if (comp.CountingTypeId == null)
                {
                    throw new Exception("Escolha o Tipo da Empresa");
                }

                if (type.Equals("resumoncm"))
                {

                    List<List<Dictionary<string, string>>> exitNotes = new List<List<Dictionary<string, string>>>();
                    List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();
                    List<TaxationNcm> ncmsMonofasico = new List<TaxationNcm>();
                    List<string> codeProdMono = new List<string>();
                    List<string> codeProdNormal = new List<string>();
                    List<string> ncmMono = new List<string>();
                    List<string> ncmNormal = new List<string>();
                    List<List<string>> resumoNcm = new List<List<string>>();
                    List<int> ncms = new List<int>();

                    var ncmsAll = _ncmService.FindAll(null);

                    ncmsMonofasico = _service.FindAll(null).Where(_ => _.Company.Document.Substring(0, 8).Equals(comp.Document.Substring(0, 8))).ToList();

                    //notesVenda = import.NfeExit(directoryNfeExit, comp.Id, Convert.ToInt32(comp.CountingTypeId));
                    exitNotes = importXml.Nfe(directoryNfeExit);

                    decimal valorProduto = 0, valorPis = 0, valorCofins = 0; 

                    for (int i = exitNotes.Count - 1; i >= 0; i--)
                    {
                        if (!exitNotes[i][2]["CNPJ"].Equals(comp.Document) || exitNotes[i][1]["finNFe"] == "4")
                        {
                            exitNotes.RemoveAt(i);
                            continue;
                        }

                        if (exitNotes[i][1].ContainsKey("dhEmi"))
                        {
                            ncmsTaxation = _service.FindAllInDate(ncmsMonofasico, Convert.ToDateTime(exitNotes[i][1]["dhEmi"]));
                           
                            codeProdMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.CodeProduct).ToList();
                            codeProdNormal = ncmsTaxation.Where(_ => _.Type.Equals("Normal") || _.Type.Equals("Nenhum")).Select(_ => _.CodeProduct).ToList();
                            ncmMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.Ncm.Code).ToList();
                            ncmNormal = ncmsTaxation.Where(_ => _.Type.Equals("Normal") || _.Type.Equals("Nenhum")).Select(_ => _.Ncm.Code).ToList();
                        }

                        int pos = -1;

                        for (int j = 0; j < exitNotes[i].Count(); j++)
                        {
                            if (exitNotes[i][j].ContainsKey("cProd") && exitNotes[i][j].ContainsKey("NCM"))
                            {

                                if (codeProdMono.Contains(exitNotes[i][j]["cProd"]) && ncmMono.Contains(exitNotes[i][j]["NCM"]))
                                {
                                    pos = -1;
                                    for (int k = 0; k < resumoNcm.Count(); k++)
                                    {
                                        if (resumoNcm[k][0].Equals(exitNotes[i][j]["NCM"]))
                                        {
                                            pos = k;
                                        }
                                    }

                                    if (pos < 0)
                                    {
                                        var nn = ncmsAll.Where(_ => _.Code.Equals(exitNotes[i][j]["NCM"])).FirstOrDefault();
                                        List<string> ncmTemp = new List<string>();
                                        ncmTemp.Add(nn.Code);
                                        ncmTemp.Add(nn.Description);
                                        ncmTemp.Add("0");
                                        ncmTemp.Add("0");
                                        ncmTemp.Add("0");
                                        resumoNcm.Add(ncmTemp);
                                        ncms.Add(Convert.ToInt32(nn.Code));
                                        pos = resumoNcm.Count() - 1;
                                    }

                                    if(pos >= 0)
                                    {
                                        if (exitNotes[i][j].ContainsKey("vProd"))
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + Convert.ToDecimal(exitNotes[i][j]["vProd"])).ToString();
                                            valorProduto += Convert.ToDecimal(exitNotes[i][j]["vProd"]);
                                        }

                                        if (exitNotes[i][j].ContainsKey("vFrete"))
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + Convert.ToDecimal(exitNotes[i][j]["vFrete"])).ToString();
                                            valorProduto += Convert.ToDecimal(exitNotes[i][j]["vFrete"]);
                                        }

                                        if (exitNotes[i][j].ContainsKey("vDesc"))
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) - Convert.ToDecimal(exitNotes[i][j]["vDesc"])).ToString();
                                            valorProduto -= Convert.ToDecimal(exitNotes[i][j]["vDesc"]);
                                        }

                                        if (exitNotes[i][j].ContainsKey("vOutro"))
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + Convert.ToDecimal(exitNotes[i][j]["vOutro"])).ToString();
                                            valorProduto += Convert.ToDecimal(exitNotes[i][j]["vOutro"]);                                        }

                                        if (exitNotes[i][j].ContainsKey("vSeg"))
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + Convert.ToDecimal(exitNotes[i][j]["vSeg"])).ToString();
                                            valorProduto += Convert.ToDecimal(exitNotes[i][j]["vSeg"]);
                                        }

                                    }

                                }
                                else if (codeProdNormal.Contains(exitNotes[i][j]["cProd"]) && ncmNormal.Contains(exitNotes[i][j]["NCM"]))
                                {
                                    pos = -1;
                                    continue;
                                }
                                else
                                {
                                    throw new Exception("Há Ncm não Tributado");
                                }

                            }


                            if (exitNotes[i][j].ContainsKey("pPIS") && exitNotes[i][j].ContainsKey("CST") && pos >= 0)
                            {
                                resumoNcm[pos][2] = (Convert.ToDecimal(resumoNcm[pos][2]) + ((Convert.ToDecimal(exitNotes[i][j]["pPIS"]) * Convert.ToDecimal(exitNotes[i][j]["vBC"])) / 100)).ToString();
                                valorPis += (Convert.ToDecimal(exitNotes[i][j]["pPIS"]) * Convert.ToDecimal(exitNotes[i][j]["vBC"])) / 100;
                            }

                            if (exitNotes[i][j].ContainsKey("pCOFINS") && exitNotes[i][j].ContainsKey("CST") && pos >= 0)
                            {
                                resumoNcm[pos][3] = (Convert.ToDecimal(resumoNcm[pos][3]) + ((Convert.ToDecimal(exitNotes[i][j]["pCOFINS"]) * Convert.ToDecimal(exitNotes[i][j]["vBC"])) / 100)).ToString();
                                valorCofins += (Convert.ToDecimal(exitNotes[i][j]["pCOFINS"]) * Convert.ToDecimal(exitNotes[i][j]["vBC"])) / 100;
                            }
                        }

                    }

                    ncms.Sort();

                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                    List<List<string>> resumoNcmOrdenado = new List<List<string>>();


                    for (int i = 0; i < resumoNcm.Count; i++)
                    {
                        resumoNcm[i][2] = (Convert.ToDouble(resumoNcm[i][2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "")).ToString();
                        resumoNcm[i][3] = (Convert.ToDouble(resumoNcm[i][3].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "")).ToString();
                        resumoNcm[i][4] = (Convert.ToDouble(resumoNcm[i][4].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "")).ToString();

                    }

                    for (int i = 0; i < ncms.Count(); i++)
                    {
                        int pos = 0;
                        for (int j = 0; i < resumoNcm.Count(); j++)
                        {
                            if (ncms[i] == Convert.ToInt32(resumoNcm[j][0]))
                            {
                                pos = j;
                                break;
                            }
                        }

                        List<string> cc = new List<string>();
                        cc.Add(resumoNcm[pos][0]);
                        cc.Add(resumoNcm[pos][1]);
                        cc.Add(resumoNcm[pos][2]);
                        cc.Add(resumoNcm[pos][3]);
                        cc.Add(resumoNcm[pos][4]);
                        resumoNcmOrdenado.Add(cc);
                    }

                    ViewBag.Ncm = resumoNcmOrdenado;
                    ViewBag.ValorProduto = Convert.ToDouble(valorProduto.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorPis = Convert.ToDouble(valorPis.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorCofins = Convert.ToDouble(valorCofins.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                }
                else if (type.Equals("imposto"))
                {
                    ViewBag.PercentualPetroleo = Convert.ToDecimal(comp.IRPJ1).ToString().Replace(".", ",");
                    ViewBag.PercentualComercio = Convert.ToDecimal(comp.IRPJ2).ToString().Replace(".", ",");
                    ViewBag.PercentualTransporte = Convert.ToDecimal(comp.IRPJ3).ToString().Replace(".", ",");
                    ViewBag.PercentualServico = Convert.ToDecimal(comp.IRPJ4).ToString().Replace(".", ",");
                    ViewBag.PercentualCsll1 = Convert.ToDecimal(comp.CSLL1).ToString().Replace(".", ",");
                    ViewBag.PercentualCsll2 = Convert.ToDecimal(comp.CSLL2).ToString().Replace(".", ",");
                    ViewBag.PercentualCPRB = Convert.ToDecimal(comp.CPRB).ToString().Replace(".", ",");
                    ViewBag.PercentualIrpjNormal = Convert.ToDecimal(comp.PercentualIRPJ).ToString().Replace(".", ",");
                    ViewBag.PercentualCsllNormal = Convert.ToDecimal(comp.PercentualCSLL).ToString().Replace(".", ",");
                    ViewBag.PercentualAdicionalIrpj = Convert.ToDecimal(comp.AdicionalIRPJ).ToString().Replace(".", ",");

                    var basePisCofins = _baseService.FindByName("PisCofins");

                    if (trimestre == "Nenhum")
                    {
                        var imp = _taxService.FindByMonth(id, month, year);

                        if(imp == null)
                        {
                            throw new Exception("Os dados para calcular PIS/COFINS não foram importados");
                        }

                        decimal receitaPetroleo = Convert.ToDecimal(imp.Receita1),receitaComercio = Convert.ToDecimal(imp.Receita2), receitaTransporte = Convert.ToDecimal(imp.Receita3),
                            receitaServico = Convert.ToDecimal(imp.Receita4), receitaMono = Convert.ToDecimal(imp.ReceitaMono),
                            devolucaoPetroleo = Convert.ToDecimal(imp.Devolucao1Entrada) + Convert.ToDecimal(imp.Devolucao1Saida),
                            devolucaoComercio = Convert.ToDecimal(imp.Devolucao2Entrada) + Convert.ToDecimal(imp.Devolucao2Saida),
                            devolucaoTransporte = Convert.ToDecimal(imp.Devolucao3Entrada) + Convert.ToDecimal(imp.Devolucao3Saida),
                            devolucaoServico = Convert.ToDecimal(imp.Devolucao4Entrada) + Convert.ToDecimal(imp.Devolucao4Saida),
                            devolucaoMono = Convert.ToDecimal(imp.DevolucaoMonoEntrada) + Convert.ToDecimal(imp.DevolucaoMonoSaida),
                            reducaoIcms = Convert.ToDecimal(imp.ReducaoIcms);


                        decimal baseCalcAntesMono = receitaComercio + receitaServico + receitaPetroleo + receitaTransporte;
                        decimal devolucaoNormal = (devolucaoPetroleo + devolucaoComercio + devolucaoTransporte + devolucaoServico) - devolucaoMono;
                        decimal baseCalcPisCofins = baseCalcAntesMono - receitaMono - devolucaoNormal - reducaoIcms;

                        //PIS
                        decimal pisApurado = (baseCalcPisCofins * Convert.ToDecimal(comp.PercentualPis)) / 100;
                        decimal pisRetido = Convert.ToDecimal(imp.PisRetido);
                        decimal pisAPagar = pisApurado - pisRetido;

                        //COFINS
                        decimal cofinsApurado = (baseCalcPisCofins * Convert.ToDecimal(comp.PercentualCofins)) / 100;
                        decimal cofinsRetido = Convert.ToDecimal(imp.CofinsRetido);
                        decimal cofinsAPagar = cofinsApurado - cofinsRetido;

                        // CSLL E IRPJ
                        decimal baseCalcCsllIrpjPetroleo = receitaPetroleo - devolucaoPetroleo;
                        decimal baseCalcCsllIrpjComercio = receitaComercio - devolucaoComercio;
                        decimal baseCalcCsllIrpjTransporte = receitaTransporte - devolucaoTransporte;
                        decimal baseCalcCsllIrpjServico = receitaServico - devolucaoServico;

                        //CSLL
                        decimal percentualCsll1 = Convert.ToDecimal(comp.CSLL1);
                        decimal percentualCsll2 = Convert.ToDecimal(comp.CSLL2);
                        decimal csll1 = (baseCalcCsllIrpjPetroleo * percentualCsll1 / 100);
                        decimal csll2 = (baseCalcCsllIrpjComercio * percentualCsll1 / 100);
                        decimal csll3 = (baseCalcCsllIrpjTransporte * percentualCsll1 / 100);
                        decimal csll4 = (baseCalcCsllIrpjServico * percentualCsll2 / 100);
                        decimal csllApurado = (csll1  + csll2 + csll3 + csll4) * Convert.ToDecimal(comp.PercentualCSLL) / 100;
                        decimal csllRetido = Convert.ToDecimal(imp.CsllRetido);
                        decimal csllAPagar = csllApurado - csllRetido;

                        //IRPJ
                        decimal percentualIrpj1 = Convert.ToDecimal(comp.IRPJ1);
                        decimal percentualIrpj2 = Convert.ToDecimal(comp.IRPJ2);
                        decimal percentualIrpj3 = Convert.ToDecimal(comp.IRPJ3);
                        decimal percentualIrpj4 = Convert.ToDecimal(comp.IRPJ4);
                        decimal irp1 = (baseCalcCsllIrpjPetroleo * percentualIrpj1 / 100);
                        decimal irp2 = (baseCalcCsllIrpjComercio * percentualIrpj2 / 100);
                        decimal irp3 = (baseCalcCsllIrpjTransporte * percentualIrpj3 / 100);
                        decimal irp4 = (baseCalcCsllIrpjServico * percentualIrpj4 / 100);
                        decimal irpjApurado = ((irp1 + irp2 + irp3 + irp4) * Convert.ToDecimal(comp.PercentualIRPJ)) / 100;
                        decimal irpjRetido = Convert.ToDecimal(imp.IrpjRetido);
                        decimal irpjAPagar = irpjApurado - irpjRetido;

                        //CPRB
                        decimal cprbAPagar = (baseCalcAntesMono * Convert.ToDecimal(comp.CPRB)) / 100;

                        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                        //Comércio
                        ViewBag.FaturamentoComercio = Convert.ToDouble(receitaComercio.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DevolucaoComercio = Convert.ToDouble(devolucaoComercio.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //Serviço
                        ViewBag.FaturamentoServico = Convert.ToDouble(receitaServico.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DevolucaoServico = Convert.ToDouble(devolucaoServico.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //Petróleo
                        ViewBag.FaturamentoPetroleo = Convert.ToDouble(receitaPetroleo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DevolucaoPetroleo = Convert.ToDouble(devolucaoPetroleo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //Transporte
                        ViewBag.FaturamentoTransporte = Convert.ToDouble(receitaTransporte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DevolucaoTransporte = Convert.ToDouble(devolucaoTransporte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //Dados PIS e COFINS
                        ViewBag.DevolucaoNormal = Convert.ToDouble(devolucaoNormal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcAntesMono = Convert.ToDouble(baseCalcAntesMono.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcMono = Convert.ToDouble(receitaMono.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CreditoDevMono = Convert.ToDouble(devolucaoMono.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcPisCofins = Convert.ToDouble(baseCalcPisCofins.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ReducaoIcms = Convert.ToDouble(reducaoIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //PIS
                        ViewBag.PisApurado = Convert.ToDouble(pisApurado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PisRetido = Convert.ToDouble(pisRetido.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PisAPagar = Convert.ToDouble(pisAPagar.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //COFINS
                        ViewBag.CofinsApurado = Convert.ToDouble(cofinsApurado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CofinsRetido = Convert.ToDouble(cofinsRetido.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CofinsAPagar = Convert.ToDouble(cofinsAPagar.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        // CSLL E IRPJ
                        ViewBag.BaseCalcCsllIrpjPetroleo = Convert.ToDouble(baseCalcCsllIrpjPetroleo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcCsllIrpjComercio = Convert.ToDouble(baseCalcCsllIrpjComercio.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcCsllIrpjTransporte = Convert.ToDouble(baseCalcCsllIrpjTransporte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcCsllIrpjServico = Convert.ToDouble(baseCalcCsllIrpjServico.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //CSLL
                        ViewBag.CsllApurado = Convert.ToDouble(csllApurado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CsllRetido = Convert.ToDouble(csllRetido.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CsllAPagar = Convert.ToDouble(csllAPagar.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //IRPJ
                        ViewBag.IrpjApurado = Convert.ToDouble(irpjApurado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IrpjRetido = Convert.ToDouble(irpjRetido.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IrpjAPagar = Convert.ToDouble(irpjAPagar.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //CPRB
                        ViewBag.CprbAPagar = Convert.ToDouble(cprbAPagar.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    }
                    else
                    {
                        var meses = importPeriod.Months(trimestre);
                        List<List<string>> impostos = new List<List<string>>();

                        decimal irpj1Total = 0, irpj2Total = 0, irpj3Total = 0, irpj4Total = 0, irpjFonteServico = 0, irpjFonteAF = 0,
                          csll1Total = 0, csll2Total = 0, csll3Total = 0, csll4Total = 0, csllFonte = 0,
                          capitalIM = 0, bonificacao = 0, receitaAF = 0;

                        decimal receitaPetroleo = 0, receitaComercio = 0, receitaTransporte = 0, receitaServico = 0,
                            devolucaoPetroleo = 0, devolucaoComercio = 0, devolucaoTransporte = 0, devolucaoServico = 0,
                            receitas = 0, receitasMono = 0, devolucoesNormal = 0, baseCalcPisCofinsTotal = 0,
                            baseCalcCsllIrpjTotal1 = 0, baseCalcCsllIrpjTotal2 = 0, baseCalcCsllIrpjTotal3 = 0, baseCalcCsllIrpjTotal4 = 0,
                            pisApuradoTotal = 0, pisRetidoTotal = 0, pisAPagarTotal = 0, cofinsApuradoTotal = 0, cofinsRetidoTotal = 0, cofinsAPagarTotal = 0,
                            csllApuradoTotal = 0, csllRetidoTotal = 0, irpjApuradoTotal = 0, irpjRetidoTotal = 0,
                            cprbAPagarTotal = 0, reducaoIcmsTotal = 0;

                        foreach (var m in meses)
                        {
                            var imp = _taxService.FindByMonth(id, m, year);

                            if (imp == null)
                            {
                                continue;
                            }

                            decimal receita1 = Convert.ToDecimal(imp.Receita1);
                            decimal receita2 = Convert.ToDecimal(imp.Receita2);
                            decimal receita3 = Convert.ToDecimal(imp.Receita3);
                            decimal receita4 = Convert.ToDecimal(imp.Receita4);
                            decimal receita = receita1 + receita2 + receita3 + receita4;
                            decimal devolucao1 = (Convert.ToDecimal(imp.Devolucao1Entrada) + Convert.ToDecimal(imp.Devolucao1Saida));
                            decimal devolucao2 = (Convert.ToDecimal(imp.Devolucao2Entrada) + Convert.ToDecimal(imp.Devolucao2Saida));
                            decimal devolucao3 = (Convert.ToDecimal(imp.Devolucao3Entrada) + Convert.ToDecimal(imp.Devolucao3Saida));
                            decimal devolucao4 = (Convert.ToDecimal(imp.Devolucao4Entrada) + Convert.ToDecimal(imp.Devolucao4Saida));
                            decimal devolucoes = devolucao1 + devolucao2 + devolucao3 + devolucao4;
                            decimal devolucaoNormal = devolucoes - (Convert.ToDecimal(imp.DevolucaoMonoEntrada) + Convert.ToDecimal(imp.DevolucaoMonoSaida));
                            decimal reducaoIcms = Convert.ToDecimal(imp.ReducaoIcms);

                            // PIS E COFINS
                            decimal baseCalcPisCofins = receita - Convert.ToDecimal(imp.ReceitaMono) - devolucaoNormal - reducaoIcms;
                            decimal pisApurado = (baseCalcPisCofins * Convert.ToDecimal(comp.PercentualPis)) / 100;
                            decimal pisRetido = Convert.ToDecimal(imp.PisRetido);
                            decimal pisAPagar = pisApurado - pisRetido;
                            decimal cofinsApurado = (baseCalcPisCofins * Convert.ToDecimal(comp.PercentualCofins)) / 100;
                            decimal cofinsRetido = Convert.ToDecimal(imp.CofinsRetido);
                            decimal cofinsAPagar = cofinsApurado - cofinsRetido;

                            // CSLL E IRPJ
                            decimal baseCalcCsllIrpj1 = receita1 - devolucao1;
                            decimal baseCalcCsllIrpj2 = receita2 - devolucao2;
                            decimal baseCalcCsllIrpj3 = receita3 - devolucao3;
                            decimal baseCalcCsllIrpj4 = receita4 - devolucao4;

                            //CSLL
                            decimal percentualCsll1 = Convert.ToDecimal(comp.CSLL1);
                            decimal percentualCsll2 = Convert.ToDecimal(comp.CSLL2);
                            decimal csll1 = (baseCalcCsllIrpj1 * percentualCsll1 / 100);
                            decimal csll2 = (baseCalcCsllIrpj2 * percentualCsll1 / 100);
                            decimal csll3 = (baseCalcCsllIrpj3 * percentualCsll1 / 100);
                            decimal csll4 = (baseCalcCsllIrpj4 * percentualCsll2 / 100);
                            decimal csllApurado = ((csll1 + csll2 + csll3 + csll4) * Convert.ToDecimal(comp.PercentualCSLL)) / 100;
                            decimal csllRetido = Convert.ToDecimal(imp.CsllRetido);

                            //IRPJ
                            decimal percentualIrpj1 = Convert.ToDecimal(comp.IRPJ1);
                            decimal percentualIrpj2 = Convert.ToDecimal(comp.IRPJ2);
                            decimal percentualIrpj3 = Convert.ToDecimal(comp.IRPJ3);
                            decimal percentualIrpj4 = Convert.ToDecimal(comp.IRPJ4);
                            decimal irp1 = (baseCalcCsllIrpj1 * percentualIrpj1 / 100);
                            decimal irp2 = (baseCalcCsllIrpj2 * percentualIrpj2 / 100);
                            decimal irp3 = (baseCalcCsllIrpj3 * percentualIrpj3 / 100);
                            decimal irp4 = (baseCalcCsllIrpj4 * percentualIrpj4 / 100);
                            decimal irpjApurado = ((irp1 + irp2 + irp3 + irp4) * Convert.ToDecimal(comp.PercentualIRPJ)) / 100;
                            decimal irpjRetido = Convert.ToDecimal(imp.IrpjRetido);

                            //CPRB
                            decimal cprbAPagar = (baseCalcPisCofins * Convert.ToDecimal(comp.CPRB)) / 100;

                            receitaPetroleo += Convert.ToDecimal(imp.Receita1);
                            receitaComercio += Convert.ToDecimal(imp.Receita2);
                            receitaTransporte += Convert.ToDecimal(imp.Receita3);
                            receitaServico += Convert.ToDecimal(imp.Receita4);
                            devolucaoPetroleo += (Convert.ToDecimal(imp.Devolucao1Entrada) + Convert.ToDecimal(imp.Devolucao1Saida));
                            devolucaoComercio += (Convert.ToDecimal(imp.Devolucao2Entrada) + Convert.ToDecimal(imp.Devolucao2Saida));
                            devolucaoTransporte += (Convert.ToDecimal(imp.Devolucao3Entrada) + Convert.ToDecimal(imp.Devolucao3Saida));
                            devolucaoServico += (Convert.ToDecimal(imp.Devolucao4Entrada) + Convert.ToDecimal(imp.Devolucao4Saida));
                            receitas += receita;
                            receitasMono += Convert.ToDecimal(imp.ReceitaMono);
                            devolucoesNormal += devolucaoNormal;
                            reducaoIcmsTotal += reducaoIcms;

                            // PIS E COFINS
                            baseCalcPisCofinsTotal += baseCalcPisCofins;
                            pisApuradoTotal += pisApurado;
                            pisRetidoTotal += pisRetido;
                            pisAPagarTotal += pisAPagar;
                            cofinsApuradoTotal += cofinsApurado;
                            cofinsRetidoTotal += cofinsRetido;
                            cofinsAPagarTotal += cofinsAPagar;

                            // CSLL E IRPJ
                            baseCalcCsllIrpjTotal1 += baseCalcCsllIrpj1;
                            baseCalcCsllIrpjTotal2 += baseCalcCsllIrpj2;
                            baseCalcCsllIrpjTotal3 += baseCalcCsllIrpj3;
                            baseCalcCsllIrpjTotal4 += baseCalcCsllIrpj4;
                            capitalIM += Convert.ToDecimal(imp.CapitalIM);
                            bonificacao += Convert.ToDecimal(imp.Bonificacao);
                            receitaAF += Convert.ToDecimal(imp.ReceitaAF);

                            // CSLL
                            csllApuradoTotal += csllApurado;
                            csllRetidoTotal += csllRetido;
                            csll1Total += csll1;
                            csll2Total += csll2;
                            csll3Total += csll3;
                            csll4Total += csll4;
                            csllFonte += Convert.ToDecimal(imp.CsllFonte);

                            //IRPJ
                            irpjApuradoTotal += irpjApurado;
                            irpjRetidoTotal += irpjRetido;
                            irpj1Total += irp1;
                            irpj2Total += irp2;
                            irpj3Total += irp3;
                            irpj4Total += irp4;
                            irpjFonteServico += Convert.ToDecimal(imp.IrpjFonteServico);
                            irpjFonteAF += Convert.ToDecimal(imp.IrpjFonteFinanceira);

                            //CPRB
                            cprbAPagarTotal += cprbAPagar;

                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                            List<string> imposto = new List<string>();

                            imposto.Add(imp.MesRef);
                            imposto.Add(Convert.ToDecimal(imp.Receita1.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(imp.Receita2.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(imp.Receita3.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(imp.Receita4.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(Convert.ToDecimal(imp.Devolucao1Entrada) + Convert.ToDecimal(imp.Devolucao1Saida).ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(Convert.ToDecimal(imp.Devolucao2Entrada) + Convert.ToDecimal(imp.Devolucao2Saida).ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(Convert.ToDecimal(imp.Devolucao3Entrada) + Convert.ToDecimal(imp.Devolucao3Saida).ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(Convert.ToDecimal(imp.Devolucao4Entrada) + Convert.ToDecimal(imp.Devolucao4Saida).ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(receita.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(imp.ReceitaMono.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(devolucaoNormal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(reducaoIcms.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(baseCalcPisCofins.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(baseCalcCsllIrpj1.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(baseCalcCsllIrpj2.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(baseCalcCsllIrpj3.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(baseCalcCsllIrpj4.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(pisApurado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(pisRetido.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(pisAPagar.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(cofinsApurado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(cofinsRetido.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(cofinsAPagar.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(csllApurado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(csllRetido.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(irpjApurado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(irpjRetido.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            imposto.Add(Convert.ToDecimal(cprbAPagar.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            impostos.Add(imposto);

                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                        }

                        // IRPJ
                        decimal irpjSubTotal = irpj1Total + irpj2Total + irpj3Total + irpj4Total;
                        decimal baseCalcIrpjNormal = irpjSubTotal + capitalIM + bonificacao + receitaAF;
                        decimal irpjNormal = baseCalcIrpjNormal * Convert.ToDecimal(comp.PercentualIRPJ) / 100;
                        decimal baseCalcAdcionalIrpj = 0;
                        decimal limite = Convert.ToDecimal(basePisCofins.Value);
                        decimal difImposto = baseCalcIrpjNormal - limite;
                        if (difImposto > 0)
                        {
                            baseCalcAdcionalIrpj = difImposto;
                        }
                        decimal adicionalIrpj = (baseCalcAdcionalIrpj * Convert.ToDecimal(comp.AdicionalIRPJ)) / 100;
                        decimal totalIrpj = irpjNormal + adicionalIrpj;
                        decimal irpjAPagar = totalIrpj - irpjFonteAF - irpjFonteServico - irpjRetidoTotal;

                        // CSLL
                        decimal csllTotal = csll1Total + csll2Total + csll3Total + csll4Total;
                        decimal baseCalcCsll = csllTotal + capitalIM + bonificacao + receitaAF;
                        decimal csllNormal = baseCalcCsll * Convert.ToDecimal(comp.PercentualCSLL) / 100;
                        decimal csllAPagar = csllNormal - csllFonte - csllRetidoTotal;

                        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                        ViewBag.Impostos = impostos;
                        ViewBag.ReceitaPetroleo = Convert.ToDouble(receitaPetroleo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ReceitaComercio = Convert.ToDouble(receitaComercio.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ReceitaTransporte = Convert.ToDouble(receitaTransporte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ReceitaServico = Convert.ToDouble(receitaServico.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DeolucaoPetroleo = Convert.ToDouble(devolucaoPetroleo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DeolucaoComercio = Convert.ToDouble(devolucaoComercio.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DeolucaoTransporte = Convert.ToDouble(devolucaoTransporte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DeolucaoServico = Convert.ToDouble(devolucaoServico.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.Receitas = Convert.ToDouble(receitas.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ReceitasMono = Convert.ToDouble(receitasMono.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.DevolucaoNormal = Convert.ToDouble(devolucoesNormal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcPisCofins = Convert.ToDouble(baseCalcPisCofinsTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcCsllIrpj1 = Convert.ToDouble(baseCalcCsllIrpjTotal1.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcCsllIrpj2 = Convert.ToDouble(baseCalcCsllIrpjTotal2.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcCsllIrpj3 = Convert.ToDouble(baseCalcCsllIrpjTotal3.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcCsllIrpj4 = Convert.ToDouble(baseCalcCsllIrpjTotal4.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PisApurado = Convert.ToDouble(pisApuradoTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PisRetido = Convert.ToDouble(pisRetidoTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PisAPagar = Convert.ToDouble(pisAPagarTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CofinsApurado = Convert.ToDouble(cofinsApuradoTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CofinsRetido = Convert.ToDouble(cofinsRetidoTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CofinsAPagar = Convert.ToDouble(cofinsAPagarTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CsllApurado = Convert.ToDouble(csllApuradoTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CsllRetido = Convert.ToDouble(csllRetidoTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IrpjApurado = Convert.ToDouble(irpjApuradoTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IrpjRetido = Convert.ToDouble(irpjRetidoTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CprbAPagar = Convert.ToDouble(cprbAPagarTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ReducaoIcms = Convert.ToDouble(reducaoIcmsTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                        // IRPJ e CSLL
                        ViewBag.CapitalIM = Convert.ToDouble(capitalIM.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.Bonificacao = Convert.ToDouble(bonificacao.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.ReceitaAF = Convert.ToDouble(receitaAF.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        // IRPJ
                        ViewBag.Irpj1 = Convert.ToDouble(irpj1Total.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.Irpj2 = Convert.ToDouble(irpj2Total.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.Irpj3 = Convert.ToDouble(irpj3Total.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.Irpj4 = Convert.ToDouble(irpj4Total.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IrpjTotal = Convert.ToDouble(irpjSubTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcIrpjNormal = Convert.ToDouble(baseCalcIrpjNormal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcAdicionalIrpj = Convert.ToDouble(baseCalcAdcionalIrpj.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IrpjNormal = Convert.ToDouble(irpjNormal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.AdicionalIrpj = Convert.ToDouble(adicionalIrpj.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.TotalIrpj = Convert.ToDouble(totalIrpj.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IrpjFonteServico = Convert.ToDouble(irpjFonteServico.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IrpjFonteAF = Convert.ToDouble(irpjFonteAF.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.IrpjAPagar = Convert.ToDouble(irpjAPagar.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        // CSLL
                        ViewBag.Csll1 = Convert.ToDouble(csll1Total.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.Csll2 = Convert.ToDouble(csll2Total.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.Csll3 = Convert.ToDouble(csll3Total.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.Csll4 = Convert.ToDouble(csll4Total.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CsllTotal = Convert.ToDouble(csllTotal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcCsll = Convert.ToDouble(baseCalcCsll.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CsllNormal = Convert.ToDouble(csllNormal.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CsllFonte = Convert.ToDouble(csllFonte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CsllAPagar = Convert.ToDouble(csllAPagar.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        List<decimal> values = new List<decimal>();

                        values.Add(Math.Round(baseCalcIrpjNormal, 2));
                        values.Add(Math.Round(irpjAPagar ,2));
                        values.Add(Math.Round(baseCalcCsll, 2));
                        values.Add(Math.Round(csllAPagar, 2));
                        SessionManager.SetValues(values);
                    }
              
                }

                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
