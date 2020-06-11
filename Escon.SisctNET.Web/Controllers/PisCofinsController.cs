using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Taxation;
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

        public PisCofinsController(
            ITaxationNcmService service,
            ICompanyService companyService,
            IConfigurationService configurationService,
            ICompanyCfopService companyCfopService,
            INcmService ncmService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "NoteExit")
        {
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            _companyCfopService = companyCfopService;
            _ncmService = ncmService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult RelatoryExit(int id, string year, string month, string type)
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

                var import = new Import(_companyCfopService, _service);

                string directoryNfeExit = NfeExit.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                ViewBag.Type = type;

                if (comp.CountingTypeId == null)
                {
                    throw new Exception("Escolha o Tipo da Empresa");
                }

                if (type.Equals("resumoncm"))
                {
                    List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                    List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();
                    List<string> codeProdMono = new List<string>();
                    List<string> codeProdNormal = new List<string>();
                    List<List<string>> resumoNcm = new List<List<string>>();
                    List<int> ncms = new List<int>();

                    var ncmsAll = _ncmService.FindAll(null);

                    notesVenda = import.NfeExit(directoryNfeExit, comp.Id, Convert.ToInt32(comp.CountingTypeId));

                    decimal valorProduto = 0, valorPis = 0, valorCofins = 0; 

                    for (int i = notesVenda.Count - 1; i >= 0; i--)
                    {
                        if (!notesVenda[i][2]["CNPJ"].Equals(comp.Document) || notesVenda[i].Count <= 5)
                        {
                            notesVenda.RemoveAt(i);
                            continue;
                        }

                        if (notesVenda[i][1].ContainsKey("dhEmi"))
                        {
                            if (comp.CountingTypeId.Equals(1))
                            {
                                ncmsTaxation = _service.FindAllInDate(Convert.ToDateTime(notesVenda[i][1]["dhEmi"])).Where(_ => _.Company.CountingTypeId.Equals(1)).ToList();
                            }
                            else
                            {
                                ncmsTaxation = _service.FindAllInDate(Convert.ToDateTime(notesVenda[i][1]["dhEmi"])).Where(_ => _.Company.CountingTypeId.Equals(2) || _.Company.CountingTypeId.Equals(3)).ToList();
                            }
                            codeProdMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.CodeProduct).ToList();
                            codeProdNormal = ncmsTaxation.Where(_ => _.Type.Equals("Normal") || _.Type.Equals("Nenhum")).Select(_ => _.CodeProduct).ToList();
                        }

                        int pos = -1;

                        for (int j = 0; j < notesVenda[i].Count(); j++)
                        {
                            if (notesVenda[i][j].ContainsKey("cProd") && notesVenda[i][j].ContainsKey("NCM"))
                            {

                                if (codeProdMono.Contains(notesVenda[i][j]["cProd"]))
                                {
                                    pos = -1;
                                    for (int k = 0; k < resumoNcm.Count(); k++)
                                    {
                                        if (resumoNcm[k][0].Equals(notesVenda[i][j]["NCM"]))
                                        {
                                            pos = k;
                                        }
                                    }
                                    if (pos < 0)
                                    {
                                        var nn = ncmsAll.Where(_ => _.Code.Equals(notesVenda[i][j]["NCM"])).FirstOrDefault();
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
                                        if (notesVenda[i][j].ContainsKey("vProd"))
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + Convert.ToDecimal(notesVenda[i][j]["vProd"])).ToString();
                                            valorProduto += Convert.ToDecimal(notesVenda[i][j]["vProd"]);
                                        }

                                        if (notesVenda[i][j].ContainsKey("vFrete"))
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + Convert.ToDecimal(notesVenda[i][j]["vFrete"])).ToString();
                                            valorProduto += Convert.ToDecimal(notesVenda[i][j]["vFrete"]);
                                        }

                                        if (notesVenda[i][j].ContainsKey("vDesc"))
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) - Convert.ToDecimal(notesVenda[i][j]["vDesc"])).ToString();
                                            valorProduto -= Convert.ToDecimal(notesVenda[i][j]["vDesc"]);
                                        }

                                        if (notesVenda[i][j].ContainsKey("vOutro"))
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + Convert.ToDecimal(notesVenda[i][j]["vOutro"])).ToString();
                                            valorProduto += Convert.ToDecimal(notesVenda[i][j]["vOutro"]);                                        }

                                        if (notesVenda[i][j].ContainsKey("vSeg"))
                                        {
                                            resumoNcm[pos][4] = (Convert.ToDecimal(resumoNcm[pos][4]) + Convert.ToDecimal(notesVenda[i][j]["vSeg"])).ToString();
                                            valorProduto += Convert.ToDecimal(notesVenda[i][j]["vSeg"]);
                                        }

                                    }

                                }
                                else if (codeProdNormal.Contains(notesVenda[i][j]["cProd"]))
                                {
                                    pos = -1;
                                    continue;
                                }
                                else
                                {
                                    throw new Exception("Há Ncm não Importado");
                                }

                            }


                            if (notesVenda[i][j].ContainsKey("pPIS") && notesVenda[i][j].ContainsKey("CST"))
                            {
                                if(pos >= 0)
                                {
                                    resumoNcm[pos][2] = (Convert.ToDecimal(resumoNcm[pos][2]) + ((Convert.ToDecimal(notesVenda[i][j]["pPIS"]) * Convert.ToDecimal(notesVenda[i][j]["vBC"])) / 100)).ToString();
                                    valorPis += (Convert.ToDecimal(notesVenda[i][j]["pPIS"]) * Convert.ToDecimal(notesVenda[i][j]["vBC"])) / 100;
                                }
                            }

                            if (notesVenda[i][j].ContainsKey("pCOFINS") && notesVenda[i][j].ContainsKey("CST"))
                            {
                                if(pos >= 0)
                                {
                                    resumoNcm[pos][3] = (Convert.ToDecimal(resumoNcm[pos][3]) + ((Convert.ToDecimal(notesVenda[i][j]["pCOFINS"]) * Convert.ToDecimal(notesVenda[i][j]["vBC"])) / 100)).ToString();
                                    valorCofins += (Convert.ToDecimal(notesVenda[i][j]["pCOFINS"]) * Convert.ToDecimal(notesVenda[i][j]["vBC"])) / 100;
                                }
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
                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
