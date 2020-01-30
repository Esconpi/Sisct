using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Taxation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class IcmsController : ControllerBaseSisctNET
    {
    
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly ICompanyCfopService _companyCfopService;

        public IcmsController(
            ICompanyService companyService,
            IConfigurationService configurationService,
            ICompanyCfopService companyCfopService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "NoteExit")
        {
            _companyService = companyService;
            _configurationService = configurationService;
            _companyCfopService = companyCfopService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult RelatoryExit(int id, string year, string month)
        {
            try
            {

                var comp = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));

                ViewBag.Year = year;
                ViewBag.Month = month;
                ViewBag.SocialName = comp.SocialName;
                ViewBag.Document = comp.Document;

                var confDBSisctNfe = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));

                var import = new Import(_companyCfopService);

                string directoryNfe = confDBSisctNfe.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

                notes = import.NfeExit(directoryNfe,id);


                for (int i = notes.Count - 1; i >= 0; i--)
                {
                    if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i].Count <= 5)
                    {
                        notes.RemoveAt(i);
                    }
                    
                }

                var cfops = _companyCfopService.FindByCfopActive(id);

                int cont = cfops.Count();
                string[,] cfop = new string[cont, 5];

                for(int i = 0; i < cont; i++)
                {
                    cfop[i, 0] = cfops[i].Cfop.Code;
                    cfop[i, 1] = "0";
                    cfop[i, 2] = "0";
                    cfop[i, 3] = "0";
                    cfop[i, 4] = cfops[i].Cfop.Description;
                }

                for(int i = 0; i < notes.Count(); i++)
                {

                    int pos = -1;

                    for(int j = 0; j < notes[i].Count(); j++)
                    {
                        if (notes[i][j].ContainsKey("CFOP"))
                        {
                            for (int k = 0; k < cont; k++)
                            {
                                if (cfop[k, 0].Equals(notes[i][j]["CFOP"]))
                                {
                                    pos = k;
                                }
                            }
                            
                        }

                        if (pos >= 0)
                        {

                            if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                            {
                                cfop[pos, 1] = (Convert.ToDecimal(cfop[pos, 1]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                            }

                            if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                            {
                                cfop[pos, 1] = (Convert.ToDecimal(cfop[pos, 1]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                            }

                            if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                            {
                                cfop[pos, 1] = (Convert.ToDecimal(cfop[pos, 1]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                            }

                            if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                            {
                                cfop[pos, 1] = (Convert.ToDecimal(cfop[pos, 1]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                            }
                            if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                            {
                                cfop[pos, 1] = (Convert.ToDecimal(cfop[pos, 1]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                            }
                            
                            if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                            {
                                cfop[pos, 2] = (Convert.ToDecimal(cfop[pos, 2]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                            }

                            if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                            {
                                cfop[pos, 3] = (Convert.ToDecimal(cfop[pos, 3]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"]))/100)).ToString();   
                            }

                        }

                    }
                }


                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");


                for (int i = 0; i < cont; i++)
                {
                    cfop[i, 1] = Convert.ToDouble(cfop[i, 1].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    cfop[i, 2] = Convert.ToDouble(cfop[i, 2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    cfop[i, 3] = Convert.ToDouble(cfop[i, 3].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                }

                ViewBag.Cfop = cfop;
                ViewBag.Cont = cont;

                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
