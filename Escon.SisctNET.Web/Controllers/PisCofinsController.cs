using System;
using System.Collections.Generic;
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
                var import = new Period.Trimestre();

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

                    List<List<string>> resumoNcmOrdenado = new List<List<string>>();

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
                    ViewBag.ValorProduto = valorProduto;
                    ViewBag.ValorPis = valorPis;
                    ViewBag.ValorCofins = valorCofins;

                }
                else if (type.Equals("imposto"))
                {
                    ViewBag.PercentualPetroleo = Convert.ToDecimal(comp.IRPJ1);
                    ViewBag.PercentualComercio = Convert.ToDecimal(comp.IRPJ2);
                    ViewBag.PercentualTransporte = Convert.ToDecimal(comp.IRPJ3);
                    ViewBag.PercentualServico = Convert.ToDecimal(comp.IRPJ4);
                    ViewBag.PercentualCsll1 = Convert.ToDecimal(comp.CSLL1);
                    ViewBag.PercentualCsll2 = Convert.ToDecimal(comp.CSLL2);
                    ViewBag.PercentualCPRB = Convert.ToDecimal(comp.CPRB);
                    ViewBag.PercentualIrpjNormal = Convert.ToDecimal(comp.PercentualIRPJ);
                    ViewBag.PercentualCsllNormal = Convert.ToDecimal(comp.PercentualCSLL);
                    ViewBag.PercentualAdicionalIrpj = Convert.ToDecimal(comp.AdicionalIRPJ);

                    var basePisCofins = _baseService.FindByName("Irpj");

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

                        //Comércio
                        ViewBag.FaturamentoComercio = receitaComercio;
                        ViewBag.DevolucaoComercio = devolucaoComercio;

                        //Serviço
                        ViewBag.FaturamentoServico = receitaServico;
                        ViewBag.DevolucaoServico = devolucaoServico;

                        //Petróleo
                        ViewBag.FaturamentoPetroleo = receitaPetroleo;
                        ViewBag.DevolucaoPetroleo = devolucaoPetroleo;

                        //Transporte
                        ViewBag.FaturamentoTransporte = receitaTransporte;
                        ViewBag.DevolucaoTransporte = devolucaoTransporte;

                        //Dados PIS e COFINS
                        ViewBag.DevolucaoNormal = devolucaoNormal;
                        ViewBag.BaseCalcAntesMono = baseCalcAntesMono;
                        ViewBag.BaseCalcMono = receitaMono;
                        ViewBag.DevolucaoMono = devolucaoMono;
                        ViewBag.BaseCalcPisCofins = baseCalcPisCofins;
                        ViewBag.ReducaoIcms = reducaoIcms;

                        //PIS
                        ViewBag.PisApurado = pisApurado;
                        ViewBag.PisRetido = pisRetido;
                        ViewBag.PisAPagar = pisAPagar;

                        //COFINS
                        ViewBag.CofinsApurado = cofinsApurado;
                        ViewBag.CofinsRetido = cofinsRetido;
                        ViewBag.CofinsAPagar = cofinsAPagar;

                        // CSLL E IRPJ
                        ViewBag.BaseCalcCsllIrpjPetroleo = baseCalcCsllIrpjPetroleo;
                        ViewBag.BaseCalcCsllIrpjComercio = baseCalcCsllIrpjComercio;
                        ViewBag.BaseCalcCsllIrpjTransporte = baseCalcCsllIrpjTransporte;
                        ViewBag.BaseCalcCsllIrpjServico = baseCalcCsllIrpjServico;

                        //CSLL
                        ViewBag.CsllApurado = csllApurado;
                        ViewBag.CsllRetido = csllRetido;
                        ViewBag.CsllAPagar = csllAPagar;

                        //IRPJ
                        ViewBag.IrpjApurado = irpjApurado;
                        ViewBag.IrpjRetido = irpjRetido;
                        ViewBag.IrpjAPagar = irpjAPagar;

                        //CPRB
                        ViewBag.CprbAPagar = cprbAPagar;

                    }
                    else
                    {
                        var mesesTrimestre = import.Months(trimestre);

                        List<List<string>> impostosTrimestre = new List<List<string>>();
                        List<List<string>> impostosBimestre = new List<List<string>>();

                        decimal irpj1Total = 0, irpj2Total = 0, irpj3Total = 0, irpj4Total = 0, irpjFonteServico = 0, irpjFonteAF = 0, irpjPagoTotal = 0,
                          csll1Total = 0, csll2Total = 0, csll3Total = 0, csll4Total = 0, csllFonte = 0, csllPagoTotal = 0,
                          capitalIM = 0, bonificacao = 0, receitaAF = 0;

                        decimal receitaPetroleo = 0, receitaComercio = 0, receitaTransporte = 0, receitaServico = 0,
                            devolucaoPetroleo = 0, devolucaoComercio = 0, devolucaoTransporte = 0, devolucaoServico = 0,
                            receitas = 0, receitasMono = 0, devolucoesNormal = 0, baseCalcPisCofinsTotal = 0,
                            baseCalcCsllIrpjTotal1 = 0, baseCalcCsllIrpjTotal2 = 0, baseCalcCsllIrpjTotal3 = 0, baseCalcCsllIrpjTotal4 = 0,
                            pisApuradoTotal = 0, pisRetidoTotal = 0, pisAPagarTotal = 0, cofinsApuradoTotal = 0, cofinsRetidoTotal = 0, cofinsAPagarTotal = 0,
                            csllApuradoTotal = 0, csllRetidoTotal = 0, irpjApuradoTotal = 0, irpjRetidoTotal = 0,
                            cprbAPagarTotal = 0, reducaoIcmsTotal = 0;

                        foreach (var m in mesesTrimestre)
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
                            decimal csllPago = Convert.ToDecimal(imp.CsllPago);

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
                            decimal irpjPago = Convert.ToDecimal(imp.IrpjPago);

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
                            csllPagoTotal += csllPago;

                            //IRPJ
                            irpjApuradoTotal += irpjApurado;
                            irpjRetidoTotal += irpjRetido;
                            irpj1Total += irp1;
                            irpj2Total += irp2;
                            irpj3Total += irp3;
                            irpj4Total += irp4;
                            irpjFonteServico += Convert.ToDecimal(imp.IrpjFonteServico);
                            irpjFonteAF += Convert.ToDecimal(imp.IrpjFonteFinanceira);
                            irpjPagoTotal += irpjPago;

                            //CPRB
                            cprbAPagarTotal += cprbAPagar;

                            List<string> imposto = new List<string>();

                            imposto.Add(imp.MesRef);
                            imposto.Add(imp.Receita1.ToString());
                            imposto.Add(imp.Receita2.ToString());
                            imposto.Add(imp.Receita3.ToString());
                            imposto.Add(imp.Receita4.ToString());
                            imposto.Add((Convert.ToDecimal(imp.Devolucao1Entrada) + Convert.ToDecimal(imp.Devolucao1Saida)).ToString());
                            imposto.Add((Convert.ToDecimal(imp.Devolucao2Entrada) + Convert.ToDecimal(imp.Devolucao2Saida)).ToString());
                            imposto.Add((Convert.ToDecimal(imp.Devolucao3Entrada) + Convert.ToDecimal(imp.Devolucao3Saida)).ToString());
                            imposto.Add((Convert.ToDecimal(imp.Devolucao4Entrada) + Convert.ToDecimal(imp.Devolucao4Saida)).ToString());
                            imposto.Add(receita.ToString());
                            imposto.Add(imp.ReceitaMono.ToString());
                            imposto.Add(devolucaoNormal.ToString());
                            imposto.Add(reducaoIcms.ToString());
                            imposto.Add(baseCalcPisCofins.ToString());
                            imposto.Add(baseCalcCsllIrpj1.ToString());
                            imposto.Add(baseCalcCsllIrpj2.ToString());
                            imposto.Add(baseCalcCsllIrpj3.ToString());
                            imposto.Add(baseCalcCsllIrpj4.ToString());
                            imposto.Add(pisApurado.ToString());
                            imposto.Add(pisRetido.ToString());
                            imposto.Add(pisAPagar.ToString());
                            imposto.Add(cofinsApurado.ToString());
                            imposto.Add(cofinsRetido.ToString());
                            imposto.Add(cofinsAPagar.ToString());
                            imposto.Add(csllApurado.ToString());
                            imposto.Add(csllRetido.ToString());
                            imposto.Add(csllPago.ToString());
                            imposto.Add(irpjApurado.ToString());
                            imposto.Add(irpjRetido.ToString());
                            imposto.Add(irpjPago.ToString());
                            imposto.Add(cprbAPagar.ToString());
                            impostosTrimestre.Add(imposto);
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
                        decimal irpjAPagar = totalIrpj - irpjFonteAF - irpjFonteServico - irpjRetidoTotal - irpjPagoTotal;

                        // CSLL
                        decimal csllTotal = csll1Total + csll2Total + csll3Total + csll4Total;
                        decimal baseCalcCsll = csllTotal + capitalIM + bonificacao + receitaAF;
                        decimal csllNormal = baseCalcCsll * Convert.ToDecimal(comp.PercentualCSLL) / 100;
                        decimal csllAPagar = csllNormal - csllFonte - csllRetidoTotal - csllPagoTotal;

                        ViewBag.Impostos = impostosTrimestre;
                        ViewBag.ReceitaPetroleo = receitaPetroleo;
                        ViewBag.ReceitaComercio = receitaComercio;
                        ViewBag.ReceitaTransporte = receitaTransporte;
                        ViewBag.ReceitaServico = receitaServico;
                        ViewBag.DeolucaoPetroleo = devolucaoPetroleo;
                        ViewBag.DeolucaoComercio = devolucaoComercio;
                        ViewBag.DeolucaoTransporte = devolucaoTransporte;
                        ViewBag.DeolucaoServico = devolucaoServico;
                        ViewBag.Receitas = receitas;
                        ViewBag.ReceitasMono = receitasMono;
                        ViewBag.DevolucaoNormal = devolucoesNormal;
                        ViewBag.BaseCalcPisCofins = baseCalcPisCofinsTotal;
                        ViewBag.BaseCalcCsllIrpj1 = baseCalcCsllIrpjTotal1;
                        ViewBag.BaseCalcCsllIrpj2 = baseCalcCsllIrpjTotal2;
                        ViewBag.BaseCalcCsllIrpj3 = baseCalcCsllIrpjTotal3;
                        ViewBag.BaseCalcCsllIrpj4 = baseCalcCsllIrpjTotal4;
                        ViewBag.PisApurado = pisApuradoTotal;
                        ViewBag.PisRetido = pisRetidoTotal;
                        ViewBag.PisAPagar = pisAPagarTotal;
                        ViewBag.CofinsApurado = cofinsApuradoTotal;
                        ViewBag.CofinsRetido = cofinsRetidoTotal;
                        ViewBag.CofinsAPagar = cofinsAPagarTotal;
                        ViewBag.CsllApurado = csllApuradoTotal;
                        ViewBag.CsllRetido = csllRetidoTotal;
                        ViewBag.IrpjApurado = irpjApuradoTotal;
                        ViewBag.IrpjRetido = irpjRetidoTotal;
                        ViewBag.CprbAPagar = cprbAPagarTotal;
                        ViewBag.ReducaoIcms = reducaoIcmsTotal;


                        // IRPJ e CSLL
                        ViewBag.CapitalIM = capitalIM;
                        ViewBag.Bonificacao = bonificacao;
                        ViewBag.ReceitaAF = receitaAF;

                        // IRPJ
                        ViewBag.Irpj1 = irpj1Total;
                        ViewBag.Irpj2 = irpj2Total;
                        ViewBag.Irpj3 = irpj3Total;
                        ViewBag.Irpj4 = irpj4Total;
                        ViewBag.IrpjTotal = irpjSubTotal;
                        ViewBag.BaseCalcIrpjNormal = baseCalcIrpjNormal;
                        ViewBag.BaseCalcAdicionalIrpj = baseCalcAdcionalIrpj;
                        ViewBag.IrpjNormal = irpjNormal;
                        ViewBag.AdicionalIrpj = adicionalIrpj;
                        ViewBag.TotalIrpj = totalIrpj;
                        ViewBag.IrpjFonteServico = irpjFonteServico;
                        ViewBag.IrpjFonteAF = irpjFonteAF;
                        ViewBag.IrpjAPagar = irpjAPagar;
                        ViewBag.IrpjPago = irpjPagoTotal;

                        // CSLL
                        ViewBag.Csll1 = csll1Total;
                        ViewBag.Csll2 = csll2Total;
                        ViewBag.Csll3 = csll3Total;
                        ViewBag.Csll4 = csll4Total;
                        ViewBag.CsllTotal = csllTotal;
                        ViewBag.BaseCalcCsll = baseCalcCsll;
                        ViewBag.CsllNormal = csllNormal;
                        ViewBag.CsllFonte = csllFonte;
                        ViewBag.CsllAPagar = csllAPagar;
                        ViewBag.CsllPago = csllPagoTotal;

                        List<decimal> values = new List<decimal>();

                        values.Add(Math.Round(baseCalcIrpjNormal, 2));
                        values.Add(Math.Round(irpjAPagar ,2));
                        values.Add(Math.Round(baseCalcCsll, 2));
                        values.Add(Math.Round(csllAPagar, 2));
                        SessionManager.SetValues(values);
                    }
              
                }

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");
                
                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
