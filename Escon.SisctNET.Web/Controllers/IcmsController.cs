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
        private readonly INcmConvenioService _ncmConvenioService;

        public IcmsController(
            ICompanyService companyService,
            IConfigurationService configurationService,
            ICompanyCfopService companyCfopService,
            IClientService clientService,
            INcmConvenioService ncmConvenioService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "NoteExit")
        {
            _companyService = companyService;
            _configurationService = configurationService;
            _companyCfopService = companyCfopService;
            _clientService = clientService;
            _ncmConvenioService = ncmConvenioService;
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

                var NfeExit = _configurationService.FindByName("NFe Saida", GetLog(Model.OccorenceLog.Read));
                var NfeEntrada = _configurationService.FindByName("NFe", GetLog(Model.OccorenceLog.Read));

                var import = new Import(_companyCfopService);

                string directoryNfeExit = NfeExit.Value + "\\" + comp.Document + "\\" + year + "\\" + month;
                string directoryNfeEntrada = NfeEntrada.Value + "\\" + comp.Document + "\\" + year + "\\" + month;

                ViewBag.Type = type;

                if (type.Equals("resumocfop")) {

                    List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

                    notes = import.NfeExit(directoryNfeExit, id, type, "all");

                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i].Count <= 5)
                        {
                            notes.RemoveAt(i);
                        }

                    }
                    var cfops = _companyCfopService.FindByCfopActive(id, type, "all");

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
                        string cpf = "escon";
                        string cnpj = "escon";
                        string indIEDest = "escon";

                        if (notes[i][3].ContainsKey("CPF"))
                        {
                            cpf = notes[i][3]["CPF"];
                        }

                        if (notes[i][3].ContainsKey("CNPJ"))
                        {
                            cnpj = notes[i][3]["CNPJ"];
                        }


                        if (notes[i][3].ContainsKey("indIEDest"))
                        {
                            indIEDest = notes[i][3]["indIEDest"];
                        }

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

                else if (type.Equals("venda"))
                {
                    List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesTranferencia = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesEntrada = new List<List<Dictionary<string, string>>>();

                    notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                    notesTranferencia = import.NfeExit(directoryNfeExit, id, type, "transferencia");
                    notesEntrada = import.NfeExit(directoryNfeEntrada, id, type, "devolução");

                    for (int i = notesVenda.Count - 1; i >= 0; i--)
                    {
                        if (!notesVenda[i][2]["CNPJ"].Equals(comp.Document) || notesVenda[i].Count <= 5)
                        {
                            notesVenda.RemoveAt(i);
                        }

                    }

                    for (int i = notesTranferencia.Count - 1; i >= 0; i--)
                    {
                        if (!notesTranferencia[i][2]["CNPJ"].Equals(comp.Document) || notesTranferencia[i].Count <= 5)
                        {
                            notesTranferencia.RemoveAt(i);
                        }

                    }

                    for(int i = notesEntrada.Count - 1; i >= 0; i--)
                    {
                        if (!notesTranferencia[i][3]["CNPJ"].Equals(comp.Document) || notesEntrada[i].Count <= 5)
                        {
                            notesTranferencia.RemoveAt(i);
                        }
                    }


                    var contribuintes = _clientService.FindByContribuinte(id, "all");
                    var contribuintesRaiz = _clientService.FindByContribuinte(id, "raiz");
                    var ncms = _ncmConvenioService.FindByAnnex(Convert.ToInt32(comp.AnnexId));
                    decimal totalVendas = 0, totalNcm = 0, totalTranferencias = 0, totalSaida = 0;
                    int contContribuintes = contribuintes.Count();
                    int contContribuintesRaiz = contribuintesRaiz.Count() + 1;

                    string[,] resumoCnpjs = new string[contContribuintes, 2];
                    string[,] resumoCnpjRaiz = new string[contContribuintesRaiz, 2];
                    string[,] resumoAllCnpjRaiz = new string[contContribuintesRaiz, 2];

                    for(int i = 0; i < contContribuintes; i++)
                    {
                        resumoCnpjs[i, 0] = contribuintes[i];
                        resumoCnpjs[i, 1] = "0";
                    }

                    for (int i = 0; i < contContribuintesRaiz; i++)
                    {
                        if (i < contContribuintesRaiz - 1)
                        {
                            resumoCnpjRaiz[i , 0] = contribuintesRaiz[i];
                            resumoAllCnpjRaiz[i, 0] = contribuintesRaiz[i];
                            resumoCnpjRaiz[i , 1] = "0";
                            resumoAllCnpjRaiz[i, 1] = "0";
                        }
                        else
                        {
                            resumoCnpjRaiz[i, 0] = "Não contribuinte";
                            resumoAllCnpjRaiz[i, 0] = "Não contribuinte";
                            resumoCnpjRaiz[i, 1] = "0";
                            resumoAllCnpjRaiz[i, 1] = "0";
                        }
                    }

                    for (int i = 0; i < notesVenda.Count(); i++)
                    {
                        int posClienteRaiz = contContribuintesRaiz - 1, posCliente = -1;

                        if (notesVenda[i][3].ContainsKey("CNPJ"))
                        {
                            if (contribuintesRaiz.Contains(notesVenda[i][3]["CNPJ"].Substring(0, 8)))
                            {
                                posClienteRaiz = contribuintesRaiz.IndexOf(notesVenda[i][3]["CNPJ"].Substring(0, 8));
                            }

                            if (contribuintes.Contains(notesVenda[i][3]["CNPJ"]))
                            {
                                posCliente = contribuintes.IndexOf(notesVenda[i][3]["CNPJ"]);
                            }

                        }

                        bool status = false;                          
                        for (int k = 0; k < notesVenda[i].Count(); k++)
                        {

                            if (notesVenda[i][k].ContainsKey("NCM"))
                            {
                                status = false;

                                for (int e = 0; e < ncms.Count; e++)
                                {
                                    int tamanho = ncms[e].Length;
                                    if (ncms[e].Equals(notesVenda[i][k]["NCM"].Substring(0, tamanho)))
                                    {
                                        status = true;
                                        break;
                                    }
                                }
                            }

                            if (status == true)
                            {
                                if (notesVenda[i][k].ContainsKey("vProd") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                    totalNcm += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();

                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posCliente, 1] = (Convert.ToDecimal(resumoCnpjs[posCliente, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    }

                                }

                                if (notesVenda[i][k].ContainsKey("vFrete") && notesVenda[i][k].ContainsKey("cProd"))
                                {

                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                    totalNcm += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();

                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posCliente, 1] = (Convert.ToDecimal(resumoCnpjs[posCliente, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vDesc") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                    totalNcm -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posCliente, 1] = (Convert.ToDecimal(resumoCnpjs[posCliente, 1]) + Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vOutro") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                    totalNcm += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posCliente, 1] = (Convert.ToDecimal(resumoCnpjs[posCliente, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vSeg") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                    totalNcm += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posCliente, 1] = (Convert.ToDecimal(resumoCnpjs[posCliente, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                    }

                                }
                            }
                            else
                            {
                                if (notesVenda[i][k].ContainsKey("vProd") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vFrete") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vDesc") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vOutro") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vSeg") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();

                                    }
                                }
                            }

                            
                        }
                    }


                    for (int i = 0; i < notesTranferencia.Count(); i++)
                    {
                        int posClienteRaiz = contContribuintesRaiz - 1;

                        if (notesTranferencia[i][3].ContainsKey("CNPJ"))
                        {
                            if (contribuintesRaiz.Contains(notesTranferencia[i][3]["CNPJ"].Substring(0, 8)))
                            {
                                posClienteRaiz = contribuintesRaiz.IndexOf(notesTranferencia[i][3]["CNPJ"].Substring(0, 8));
                            }
                        }

                        for (int j = 0; j < notesTranferencia[i].Count; j++)
                        {
                            if (notesVenda[i][j].ContainsKey("vProd") && notesVenda[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias += Convert.ToDecimal(notesVenda[i][j]["vProd"]);
                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][j]["vProd"])).ToString();
                            }

                            if (notesVenda[i][j].ContainsKey("vFrete") && notesVenda[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias += Convert.ToDecimal(notesVenda[i][j]["vFrete"]);
                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][j]["vFrete"])).ToString();
                            }

                            if (notesVenda[i][j].ContainsKey("vDesc") && notesVenda[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias -= Convert.ToDecimal(notesVenda[i][j]["vDesc"]);
                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][j]["vDesc"])).ToString();
                            }

                            if (notesVenda[i][j].ContainsKey("vOutro") && notesVenda[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias += Convert.ToDecimal(notesVenda[i][j]["vOutro"]);
                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][j]["vOutro"])).ToString();
                            }

                            if (notesVenda[i][j].ContainsKey("vSeg") && notesVenda[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias += Convert.ToDecimal(notesVenda[i][j]["vSeg"]);
                                resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][j]["vSeg"])).ToString();
                            }
                        }
                    }
                   
                    decimal totalNcontribuinte = Convert.ToDecimal(resumoCnpjRaiz[contContribuintesRaiz - 1, 1]);
                    decimal totalContribuinte = totalVendas - totalNcontribuinte;

                    decimal limiteContribuinte = (totalVendas * Convert.ToDecimal(comp.VendaContribuinte)) / 100,
                        limiteNcm = (totalVendas * Convert.ToDecimal(comp.VendaAnexo)) / 100,
                        limiteGrupo = (totalVendas * Convert.ToDecimal(comp.VendaMGrupo)) / 100;

                    decimal impostoContribuinte = 0, excedenteContribuinte = 0, excedenteNcm = 0 , impostoNcm = 0;

                    List<List<string>> gruposExecentes = new List<List<string>>();

                    for (int i = 0; i < contContribuintesRaiz; i++)
                    {
                        var totalVendaGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 1]);
                        if (totalVendaGrupo > limiteGrupo)
                        {
                            List<string> grupoExcedente = new List<string>();
                            var cnpjGrupo = resumoAllCnpjRaiz[i, 0];
                            var clientGrupo = _clientService.FindByRaiz(cnpjGrupo);
                            var nomeGrupo = clientGrupo.Name;
                            var percentualGrupo = Math.Round((totalVendaGrupo / totalVendas) * 100, 2);
                            grupoExcedente.Add(cnpjGrupo);
                            grupoExcedente.Add(nomeGrupo);
                            grupoExcedente.Add(percentualGrupo.ToString());
                            grupoExcedente.Add(totalVendaGrupo.ToString());
                            gruposExecentes.Add(grupoExcedente);
                        }
                    }

                    if (totalContribuinte < limiteContribuinte)
                    {
                        excedenteContribuinte = limiteContribuinte - totalContribuinte;
                        impostoContribuinte = (excedenteContribuinte * Convert.ToDecimal(comp.VendaContribuinteExcedente)) / 100;
                    }

                    if (totalNcm < limiteNcm)
                    {
                        excedenteNcm = limiteNcm - totalNcm;
                        impostoNcm = (excedenteNcm * Convert.ToDecimal(comp.VendaAnexoExcedente)) / 100;
                    }
                   

                    ViewBag.Contribuinte = totalContribuinte;
                    ViewBag.NContribuinte = totalNcontribuinte;
                    ViewBag.TotalSaida = totalSaida;

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
