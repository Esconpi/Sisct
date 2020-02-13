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
        private readonly IClientService _clientService;

        public IcmsController(
            ICompanyService companyService,
            IConfigurationService configurationService,
            ICompanyCfopService companyCfopService,
            IClientService clientService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "NoteExit")
        {
            _companyService = companyService;
            _configurationService = configurationService;
            _companyCfopService = companyCfopService;
            _clientService = clientService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult RelatoryExit(int id, string year, string month, string type)
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

                notes = import.NfeExit(directoryNfe, id);

                for (int i = notes.Count - 1; i >= 0; i--)
                {
                    if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i].Count <= 5)
                    {
                        notes.RemoveAt(i);
                    }

                }
                
                if (type.Equals("resumocfop")) {
                    
                    var cfops = _companyCfopService.FindByCfopActive(id);

                    int cont = cfops.Count();
                    string[,] cfopGeral = new string[cont, 6];
                    string[,] cfopCpf = new string[cont, 6];
                    string[,] cfopSCpf = new string[cont, 6];
                    string[,] cfopCnpjIE = new string[cont, 6];
                    string[,] cfopSCnpj = new string[cont, 6];
                    string[,] cfopSCnpjCpf = new string[cont, 6];
                    string[,] cfopCnpjSIE = new string[cont, 6];
                    
                    for (int i = 0; i < cont; i++)
                    {
                        cfopGeral[i, 0] = cfops[i].Cfop.Code;
                        cfopCpf[i, 0] = cfops[i].Cfop.Code;
                        cfopSCpf[i, 0] = cfops[i].Cfop.Code;
                        cfopCnpjIE[i, 0] = cfops[i].Cfop.Code;
                        cfopSCnpj[i, 0] = cfops[i].Cfop.Code;
                        cfopSCnpjCpf[i, 0] = cfops[i].Cfop.Code;
                        cfopCnpjSIE[i, 0] = cfops[i].Cfop.Code;
                        cfopGeral[i, 1] = "0";
                        cfopCpf[i, 1] = "0";
                        cfopSCpf[i, 1] = "0";
                        cfopCnpjIE[i, 1] = "0";
                        cfopSCnpj[i, 1] = "0";
                        cfopSCnpjCpf[i, 1] = "0";
                        cfopCnpjSIE[i, 1] = "0";
                        cfopGeral[i, 2] = "0";
                        cfopCpf[i, 2] = "0";
                        cfopSCpf[i, 2] = "0";
                        cfopCnpjIE[i, 2] = "0";
                        cfopSCnpj[i, 2] = "0";
                        cfopSCnpjCpf[i, 2] = "0";
                        cfopCnpjSIE[i, 2] = "0";
                        cfopGeral[i, 3] = "0";
                        cfopCpf[i, 3] = "0";
                        cfopSCpf[i, 3] = "0";
                        cfopCnpjIE[i, 3] = "0";
                        cfopCnpjIE[i, 3] = "0";
                        cfopSCnpj[i, 3] = "0";
                        cfopSCnpjCpf[i, 3] = "0";
                        cfopCnpjSIE[i, 3] = "0";
                        cfopGeral[i, 4] = cfops[i].Cfop.Description;
                        cfopCpf[i, 4] = cfops[i].Cfop.Description;
                        cfopSCpf[i, 4] = cfops[i].Cfop.Description;
                        cfopCnpjIE[i, 4] = cfops[i].Cfop.Description;
                        cfopSCnpj[i, 4] = cfops[i].Cfop.Description;
                        cfopSCnpjCpf[i, 4] = cfops[i].Cfop.Description;
                        cfopCnpjSIE[i, 4] = cfops[i].Cfop.Description;
                        cfopGeral[i, 5] = "0";
                        cfopCpf[i, 5] = "0";
                        cfopSCpf[i, 5] = "0";
                        cfopCnpjIE[i, 5] = "0";
                        cfopSCnpj[i, 5] = "0";
                        cfopSCnpjCpf[i, 5] = "0";
                        cfopCnpjSIE[i, 5] = "0";
                    }

                    for (int i = 0; i < notes.Count(); i++)
                    {

                        int pos = -1;
                        string cpf = notes[i][3].ContainsKey("CPF") ? notes[i][3]["CPF"] : "escon";
                        string cnpj = notes[i][3].ContainsKey("CNPJ") ? notes[i][3]["CNPJ"] : "escon";
                        string indIEDest = notes[i][3].ContainsKey("indIEDest") ? notes[i][3]["indIEDest"] : "escon";

                        for (int j = 0; j < notes[i].Count(); j++)
                        {
                            if (notes[i][j].ContainsKey("CFOP"))
                            {
                                for (int k = 0; k < cont; k++)
                                {
                                    if (cfopCpf[k, 0].Equals(notes[i][j]["CFOP"]))
                                    {
                                        pos = k;
                                    }
                                }

                            }

                            if (pos >= 0)
                            {
                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopGeral[pos, 1] = (Convert.ToDecimal(cfopGeral[pos, 1]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopGeral[pos, 1] = (Convert.ToDecimal(cfopGeral[pos, 1]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopGeral[pos, 1] = (Convert.ToDecimal(cfopGeral[pos, 1]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopGeral[pos, 1] = (Convert.ToDecimal(cfopGeral[pos, 1]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                {
                                    cfopGeral[pos, 1] = (Convert.ToDecimal(cfopGeral[pos, 1]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopGeral[pos, 2] = (Convert.ToDecimal(cfopGeral[pos, 2]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                }

                                if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopGeral[pos, 3] = (Convert.ToDecimal(cfopGeral[pos, 3]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                }

                                if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                {
                                    cfopGeral[pos, 5] = (Convert.ToDecimal(cfopGeral[pos, 5]) + ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                }

                                if (cpf != "escon" && cpf != "")
                                {
                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCpf[pos, 1] = (Convert.ToDecimal(cfopCpf[pos, 1]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCpf[pos, 1] = (Convert.ToDecimal(cfopCpf[pos, 1]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCpf[pos, 1] = (Convert.ToDecimal(cfopCpf[pos, 1]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCpf[pos, 1] = (Convert.ToDecimal(cfopCpf[pos, 1]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCpf[pos, 1] = (Convert.ToDecimal(cfopCpf[pos, 1]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopCpf[pos, 2] = (Convert.ToDecimal(cfopCpf[pos, 2]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopCpf[pos, 3] = (Convert.ToDecimal(cfopCpf[pos, 3]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopCpf[pos, 5] = (Convert.ToDecimal(cfopCpf[pos, 5]) + ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                    }

                                }
                                else if ((cpf == "escon" || cpf == "") && cnpj == "escon")
                                {
                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCpf[pos, 1] = (Convert.ToDecimal(cfopSCpf[pos, 1]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCpf[pos, 1] = (Convert.ToDecimal(cfopSCpf[pos, 1]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCpf[pos, 1] = (Convert.ToDecimal(cfopSCpf[pos, 1]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCpf[pos, 1] = (Convert.ToDecimal(cfopSCpf[pos, 1]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCpf[pos, 1] = (Convert.ToDecimal(cfopSCpf[pos, 1]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopSCpf[pos, 2] = (Convert.ToDecimal(cfopSCpf[pos, 2]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopSCpf[pos, 3] = (Convert.ToDecimal(cfopSCpf[pos, 3]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopSCpf[pos, 5] = (Convert.ToDecimal(cfopSCpf[pos, 5]) + ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                    }
                                }
                                else if (cnpj != "escon" && cnpj != "" && indIEDest == "1")
                                {
                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCnpjIE[pos, 1] = (Convert.ToDecimal(cfopCnpjIE[pos, 1]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCnpjIE[pos, 1] = (Convert.ToDecimal(cfopCnpjIE[pos, 1]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCnpjIE[pos, 1] = (Convert.ToDecimal(cfopCnpjIE[pos, 1]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCnpjIE[pos, 1] = (Convert.ToDecimal(cfopCnpjIE[pos, 1]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCnpjIE[pos, 1] = (Convert.ToDecimal(cfopCnpjIE[pos, 1]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopCnpjIE[pos, 2] = (Convert.ToDecimal(cfopCnpjIE[pos, 2]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopCnpjIE[pos, 3] = (Convert.ToDecimal(cfopCnpjIE[pos, 3]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopCnpjIE[pos, 5] = (Convert.ToDecimal(cfopCnpjIE[pos, 5]) + ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                    }
                                }
                                else if (cnpj != "escon" && cnpj != "" && indIEDest != "1")
                                {
                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCnpjSIE[pos, 1] = (Convert.ToDecimal(cfopCnpjSIE[pos, 1]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCnpjSIE[pos, 1] = (Convert.ToDecimal(cfopCnpjSIE[pos, 1]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCnpjSIE[pos, 1] = (Convert.ToDecimal(cfopCnpjSIE[pos, 1]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCnpjSIE[pos, 1] = (Convert.ToDecimal(cfopCnpjSIE[pos, 1]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopCnpjSIE[pos, 1] = (Convert.ToDecimal(cfopCnpjSIE[pos, 1]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopCnpjSIE[pos, 2] = (Convert.ToDecimal(cfopCnpjSIE[pos, 2]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopCnpjSIE[pos, 3] = (Convert.ToDecimal(cfopCnpjSIE[pos, 3]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopCnpjSIE[pos, 5] = (Convert.ToDecimal(cfopCnpjSIE[pos, 5]) + ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                    }
                                }
                                else if ((cnpj == "escon" || cnpj == "") && cpf == "escon")
                                {
                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCnpj[pos, 1] = (Convert.ToDecimal(cfopSCnpj[pos, 1]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCnpj[pos, 1] = (Convert.ToDecimal(cfopSCnpj[pos, 1]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCnpj[pos, 1] = (Convert.ToDecimal(cfopSCnpj[pos, 1]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCnpj[pos, 1] = (Convert.ToDecimal(cfopSCnpj[pos, 1]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCnpj[pos, 1] = (Convert.ToDecimal(cfopSCnpj[pos, 1]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopSCnpj[pos, 2] = (Convert.ToDecimal(cfopSCnpj[pos, 2]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopSCnpj[pos, 3] = (Convert.ToDecimal(cfopSCnpj[pos, 3]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopSCnpj[pos, 5] = (Convert.ToDecimal(cfopSCnpj[pos, 5]) + ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                    }
                                }
                                else if (cnpj == "escon" && cpf == "escon")
                                {
                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCnpjCpf[pos, 1] = (Convert.ToDecimal(cfopSCnpjCpf[pos, 1]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCnpjCpf[pos, 1] = (Convert.ToDecimal(cfopSCnpjCpf[pos, 1]) + Convert.ToDecimal(notes[i][j]["vFrete"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCnpjCpf[pos, 1] = (Convert.ToDecimal(cfopSCnpjCpf[pos, 1]) - Convert.ToDecimal(notes[i][j]["vDesc"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCnpjCpf[pos, 1] = (Convert.ToDecimal(cfopSCnpjCpf[pos, 1]) + Convert.ToDecimal(notes[i][j]["vOutro"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                    {
                                        cfopSCnpjCpf[pos, 1] = (Convert.ToDecimal(cfopSCnpjCpf[pos, 1]) + Convert.ToDecimal(notes[i][j]["vSeg"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopSCnpjCpf[pos, 2] = (Convert.ToDecimal(cfopSCnpjCpf[pos, 2]) + Convert.ToDecimal(notes[i][j]["vBC"])).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopSCnpjCpf[pos, 3] = (Convert.ToDecimal(cfopSCnpjCpf[pos, 3]) + ((Convert.ToDecimal(notes[i][j]["pICMS"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                    }

                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        cfopSCnpjCpf[pos, 5] = (Convert.ToDecimal(cfopSCnpjCpf[pos, 5]) + ((Convert.ToDecimal(notes[i][j]["pFCP"]) * Convert.ToDecimal(notes[i][j]["vBC"])) / 100)).ToString();
                                    }
                                }

                            }

                        }
                    }


                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");
                    List<List<string>> cfop_list = new List<List<string>>();

                    for (int i = 0; i < cont; i++)
                    {
                        string t = cfopGeral[i, 1];
                        if (cfopGeral[i, 1] != "0")
                        {
                            List<string> cfop = new List<string>();
                            cfop.Add(cfopGeral[i, 0]);
                            cfop.Add(cfopGeral[i, 4]);

                            cfop.Add(Convert.ToDouble(cfopGeral[i, 1].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopGeral[i, 2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopGeral[i, 3].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopGeral[i, 5].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));

                            cfop.Add(Convert.ToDouble(cfopCpf[i, 1].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopCpf[i, 2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopCpf[i, 3].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopCpf[i, 5].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));

                            cfop.Add(Convert.ToDouble(cfopSCpf[i, 1].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopSCpf[i, 2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopSCpf[i, 3].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopSCpf[i, 5].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));

                            cfop.Add(Convert.ToDouble(cfopCnpjIE[i, 1].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopCnpjIE[i, 2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopCnpjIE[i, 3].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopCnpjIE[i, 5].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));

                            cfop.Add(Convert.ToDouble(cfopCnpjSIE[i, 1].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopCnpjSIE[i, 2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopCnpjSIE[i, 3].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopCnpjSIE[i, 5].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));

                            cfop.Add(Convert.ToDouble(cfopSCnpj[i, 1].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopSCnpj[i, 2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopSCnpj[i, 3].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopSCnpj[i, 5].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));

                            cfop.Add(Convert.ToDouble(cfopSCnpjCpf[i, 1].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopSCnpjCpf[i, 2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopSCnpjCpf[i, 3].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            cfop.Add(Convert.ToDouble(cfopSCnpjCpf[i, 5].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));


                            cfop_list.Add(cfop);
                        }
                    }

                    ViewBag.Cfop = cfop_list;
                }

                else if (type.Equals("icmsexcedente"))
                {
                    /* var notes = import.NotesRelatoryIcms(directoryNfe);
                     decimal total = 0, totalContribuinte = 0, totalNContribuinte = 0;


                     foreach (var note in notes)
                     {
                         if (note[0].ContainsKey("CNPJ") && note[0].ContainsKey("indIEDest") && note[0].ContainsKey("IE"))
                         {
                             if (note[0]["indIEDest"].Equals("1"))
                             {
                                 totalContribuinte += Convert.ToDecimal(note[1]["vNF"]);
                                 total += Convert.ToDecimal(note[1]["vNF"]);
                             }
                         }
                         else if (note.Count.Equals(1))
                         {
                             totalNContribuinte += Convert.ToDecimal(note[0]["vNF"]);
                             total += Convert.ToDecimal(note[0]["vNF"]);
                         }
                         else
                         {
                             totalNContribuinte += Convert.ToDecimal(note[1]["vNF"]);
                             total += Convert.ToDecimal(note[1]["vNF"]);
                         }

                     }*/

                    var contribuintes = _clientService.FindByContribuinte(id);
                    var cfops = _companyCfopService.FindByCfopActive(id);

                    int contContribuintes = contribuintes.Count() + 1;
                    int contCfops = cfops.Count();

                    string[,,] resumoGeral = new string[contContribuintes,contCfops, 3];

                    for (int i = 0; i < contContribuintes; i++)
                    {
                        for (int j = 0; j < contCfops; j++)
                        {
                            if (i < contContribuintes -1)
                            {
                                resumoGeral[i, j, 0] = contribuintes[i].Document;
                                resumoGeral[i, j, 1] = cfops[j].Cfop.Code;
                                resumoGeral[i, j, 2] = "0";
                            }
                            else
                            {
                                resumoGeral[i, j, 0] = "Não contribuinte";
                                resumoGeral[i, j, 1] = cfops[j].Cfop.Code;
                                resumoGeral[i, j, 2] = "0";
                            }
                        }
                    }

                    for (int i = 0; i < notes.Count(); i++)
                    {
                        int posCliente = contContribuintes;

                        if (notes[i][3].ContainsKey("CNPJ"))
                        {
                            for (int j = 0; j < contContribuintes; j++)
                            {
                                if (resumoGeral[j , 0, 0].Equals(notes[i][3]["CNPJ"]))
                                {
                                    posCliente = j;
                                }
                            }
                        }

                        int posCfop = -1;

                        for (int k = 0; k < notes[i].Count(); k++)
                        {
                            if (notes[i][k].ContainsKey("CFOP"))
                            {
                                for (int c = 0; c < contContribuintes; c++)
                                {
                                    if (resumoGeral[c, 1, 0].Equals(notes[i][c]["CFOP"]))
                                    {
                                        posCfop = c;
                                    }
                                }

                            }

                            if (posCfop >= 0)
                            {
                                if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                {
                                    resumoGeral[posCliente,posCfop, 2] = (Convert.ToDecimal(resumoGeral[posCliente,posCfop, 2]) + Convert.ToDecimal(notes[i][j]["vProd"])).ToString();
                                }
                            }
                        }
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
