using System;
using System.Collections.Generic;
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

        public PisCofinsController(
            ITaxationNcmService service,
            ICompanyService companyService,
            IConfigurationService configurationService,
            ICompanyCfopService companyCfopService,
            INcmService ncmService,
            ITaxService taxService,
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
                    ViewBag.PercentualCPRB = Convert.ToDecimal(comp.CPRB).ToString().Replace(".", ",");

                    if (trimestre == "Nenhum")
                    {
                        var imp = _taxService.FindByMonth(id, month, year);

                        if(imp == null)
                        {
                            throw new Exception("Os dados para calcular PIS/COFINS não foram importados");
                        }

                        decimal receitaPetroleo = Convert.ToDecimal(imp.Receita1),receitaComercio = Convert.ToDecimal(imp.Receita2), receitaTransporte = Convert.ToDecimal(imp.Receita3),
                            receitaServico = Convert.ToDecimal(imp.Receita4), receitaMono = Convert.ToDecimal(imp.ReceitaMono),
                            devolucaoPetroleo = Convert.ToDecimal(imp.Devolucao1Entrada) + Convert.ToDecimal(imp.Devolucao1Entrada),
                            devolucaoComercio = Convert.ToDecimal(imp.Devolucao2Entrada) + Convert.ToDecimal(imp.Devolucao2Entrada),
                            devolucaoTransporte = Convert.ToDecimal(imp.Devolucao3Entrada) + Convert.ToDecimal(imp.Devolucao3Entrada),
                            devolucaoServico = Convert.ToDecimal(imp.Devolucao4Entrada) + Convert.ToDecimal(imp.Devolucao4Entrada),
                            devolucaoMono = devolucaoServico = Convert.ToDecimal(imp.DevolucaoMonoEntrada) + Convert.ToDecimal(imp.DevolucaoMonoSaida);

                        decimal baseCalcAntesMono = receitaComercio + receitaServico + receitaPetroleo;
                        decimal baseCalcPisCofins = baseCalcAntesMono - receitaMono;

                        //PIS
                        decimal pisApurado = (baseCalcPisCofins * Convert.ToDecimal(comp.PercentualPis)) / 100;
                        decimal pisRetido = (devolucaoMono * Convert.ToDecimal(comp.PercentualPis)) / 100;
                        decimal pisAPagar = pisApurado - pisRetido;

                        //COFINS
                        decimal cofinsApurado = (baseCalcPisCofins * Convert.ToDecimal(comp.PercentualCofins)) / 100;
                        decimal cofinsRetido = (devolucaoMono * Convert.ToDecimal(comp.PercentualCofins)) / 100;
                        decimal cofinsAPagar = cofinsApurado - cofinsRetido;

                        //CSLL
                        decimal percentualCsll1 = (Convert.ToDecimal(comp.CSLL1) * Convert.ToDecimal(comp.PercentualCSLL)) / 100;
                        decimal percentualCsll2 = (Convert.ToDecimal(comp.CSLL2) * Convert.ToDecimal(comp.PercentualCSLL)) / 100;
                        decimal csllApurado = ((receitaComercio - devolucaoComercio) * percentualCsll1 / 100) + ((receitaPetroleo - devolucaoPetroleo) * percentualCsll1 / 100) +
                           ((receitaTransporte - devolucaoTransporte) * percentualCsll1 / 100)  + ((receitaServico - devolucaoServico) * percentualCsll2 / 100);
                        decimal csllRetido = 0;
                        decimal csllAPagar = csllApurado - csllRetido;

                        //IRPJ
                        decimal percentualIrpj1 = (Convert.ToDecimal(comp.IRPJ1) * Convert.ToDecimal(comp.PercentualIRPJ)) / 100;
                        decimal percentualIrpj2 = (Convert.ToDecimal(comp.IRPJ2) * Convert.ToDecimal(comp.PercentualIRPJ)) / 100;
                        decimal percentualIrpj3 = (Convert.ToDecimal(comp.IRPJ3) * Convert.ToDecimal(comp.PercentualIRPJ)) / 100;
                        decimal percentualIrpj4 = (Convert.ToDecimal(comp.IRPJ4) * Convert.ToDecimal(comp.PercentualIRPJ)) / 100;
                        decimal irpjApurado = ((receitaPetroleo - devolucaoPetroleo) * percentualIrpj1 / 100) + ((receitaComercio - devolucaoComercio) * percentualIrpj2 / 100) +
                            ((receitaTransporte - devolucaoTransporte) * percentualIrpj3 / 100)  + ((receitaServico - devolucaoServico) * percentualIrpj4 / 100);
                        decimal irpjRetido = 0;

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
                        ViewBag.BaseCalcAntesMono = Convert.ToDouble(baseCalcAntesMono.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcMono = Convert.ToDouble(receitaMono.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CreditoDevMono = Convert.ToDouble(devolucaoMono.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.BaseCalcPisCofins = Convert.ToDouble(baseCalcPisCofins.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //PIS
                        ViewBag.PisApurado = Convert.ToDouble(pisApurado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PisRetido = Convert.ToDouble(pisRetido.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.PisAPagar = Convert.ToDouble(pisAPagar.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                        //COFINS
                        ViewBag.CofinsApurado = Convert.ToDouble(cofinsApurado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CofinsRetido = Convert.ToDouble(cofinsRetido.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                        ViewBag.CofinsAPagar = Convert.ToDouble(cofinsAPagar.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

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
