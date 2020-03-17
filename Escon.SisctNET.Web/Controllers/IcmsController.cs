using Escon.SisctNET.Service;
using Escon.SisctNET.Web.Taxation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{
    public class IcmsController : ControllerBaseSisctNET
    {
    
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly ICompanyCfopService _companyCfopService;
        private readonly IClientService _clientService;
        private readonly INcmConvenioService _ncmConvenioService;
        private readonly IDarService _darService;
        private readonly IHostingEnvironment _appEnvironment;
        private readonly IProductIncentivoService _productIncentivoService;
        public IcmsController(
            ICompanyService companyService,
            IConfigurationService configurationService,
            ICompanyCfopService companyCfopService,
            IClientService clientService,
            INcmConvenioService ncmConvenioService,
            IDarService darService,
            IFunctionalityService functionalityService,
            IProductIncentivoService productIncentivoService,
            IHostingEnvironment env,

            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "NoteExit")
        {
            _companyService = companyService;
            _configurationService = configurationService;
            _companyCfopService = companyCfopService;
            _clientService = clientService;
            _ncmConvenioService = ncmConvenioService;
            _darService = darService;
            _productIncentivoService = productIncentivoService;
            _appEnvironment = env;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public async Task<IActionResult> RelatoryExit(int id, string year, string month, string type, IFormFile arquivo)
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
                    //notes = import.NfeExit(directoryNfeEntrada, id, type, "all");
                    for (int i = notes.Count - 1; i >= 0; i--)
                    {
                        if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i].Count <= 5)
                        //if (notes[i].Count <= 5)
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
                    List<List<Dictionary<string, string>>> notesEntradaDevolucao = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesSaidaDevolucao = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesTransferenciaEntrada = new List<List<Dictionary<string, string>>>();

                    notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                    notesTranferencia = import.NfeExit(directoryNfeExit, id, type, "transferencia");
                    notesSaidaDevolucao = import.NfeExit(directoryNfeExit, id, type, "devolução");
                    notesEntradaDevolucao = import.NfeExit(directoryNfeEntrada, id, type, "devolução");
                    notesTransferenciaEntrada = import.NotesTransfer(directoryNfeEntrada, id);
                    var cfopstransf = _companyCfopService.FindByCfopActive(id, type, "transferencia").Select(_ => _.Cfop.Code).ToList();

                    decimal totalEntradas = 0, totalTranferenciaInter = 0;
                    for (int i = notesTransferenciaEntrada.Count - 1; i >= 0; i--)
                    {
                        if (!notesTransferenciaEntrada[i][3]["CNPJ"].Equals(comp.Document) && !notesTransferenciaEntrada[i][2]["CNPJ"].Equals(comp.Document))
                        {
                            notesTransferenciaEntrada.RemoveAt(i);
                        }
                        else
                        {
                            if (cfopstransf.Contains(notesTransferenciaEntrada[i][4]["CFOP"]) && notesTransferenciaEntrada[i][1]["idDest"].Equals("2"))
                            {
                                totalTranferenciaInter += Convert.ToDecimal(notesTransferenciaEntrada[i][notesTransferenciaEntrada[i].Count - 1]["vNF"]);
                            }
                            totalEntradas += Convert.ToDecimal(notesTransferenciaEntrada[i][notesTransferenciaEntrada[i].Count - 1]["vNF"]);
                        }
                    }

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

                    decimal totalDevolucao = 0;

                    for (int i = notesEntradaDevolucao.Count - 1; i >= 0; i--)
                    {

                        if (!notesEntradaDevolucao[i][3]["CNPJ"].Equals(comp.Document) || notesEntradaDevolucao[i].Count <= 5)
                        {
                            notesEntradaDevolucao.RemoveAt(i);
                            continue;
                        }
                        totalDevolucao += Convert.ToDecimal(notesEntradaDevolucao[i][notesEntradaDevolucao[i].Count() - 1]["vNF"]);
                    }

                    for (int i = notesSaidaDevolucao.Count - 1; i >= 0; i--)
                    {

                        if (notesSaidaDevolucao[i].Count <= 5)
                        {
                            notesSaidaDevolucao.RemoveAt(i);
                            continue;
                        }
                        totalDevolucao += Convert.ToDecimal(notesSaidaDevolucao[i][notesSaidaDevolucao[i].Count() - 1]["vNF"]);
                    }

                    var contribuintes = _clientService.FindByContribuinte(id, "all");
                    var contribuintesRaiz = _clientService.FindByContribuinte(id, "raiz");
                    var ncms = _ncmConvenioService.FindByAnnex(Convert.ToInt32(comp.AnnexId));
                    decimal totalVendas = -totalDevolucao, totalNcm = 0, totalTranferencias = 0, totalSaida = 0;
                    int contContribuintes = contribuintes.Count();
                    int contContribuintesRaiz = contribuintesRaiz.Count() + 1;

                    string[,] resumoCnpjs = new string[contContribuintes, 2];
                    string[,] resumoCnpjRaiz = new string[contContribuintesRaiz, 2];
                    string[,] resumoAllCnpjRaiz = new string[contContribuintesRaiz-1, 2];

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
                            resumoCnpjRaiz[i, 1] = "0";
                        }
                    }

                    for (int i = 0; i < notesVenda.Count(); i++)
                    {
                        int posClienteRaiz = contContribuintesRaiz - 1, posCliente = -1;

                        bool status = false;

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
                        
                        for (int k = 0; k < notesVenda[i].Count(); k++)
                        {

                            if (notesVenda[i][k].ContainsKey("NCM"))
                            {
                                status = false;

                                for(int j = 0; j < ncms.Count(); j++)
                                {
                                    int tamanho = ncms[j].Length;

                                    if (ncms[j].Equals(notesVenda[i][k]["NCM"].Substring(0, tamanho)))
                                    {
                                        status = true;
                                        break;
                                    }
                                }
                            }

                            if(status == true)
                            {

                                if (notesVenda[i][k].ContainsKey("vProd") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                    totalNcm += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vFrete") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                    totalNcm += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vDesc") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                    totalNcm -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vOutro") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                    totalNcm += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vSeg") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                    totalNcm += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();

                                    }
                                }
                            }
                            else
                            {

                                if (notesVenda[i][k].ContainsKey("vProd") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vProd"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vFrete") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vFrete"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vDesc") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) - Convert.ToDecimal(notesVenda[i][k]["vDesc"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vOutro") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    }
                                    if (posCliente >= 0)
                                    {
                                        resumoCnpjs[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjs[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vOutro"])).ToString();
                                    }
                                }

                                if (notesVenda[i][k].ContainsKey("vSeg") && notesVenda[i][k].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                    resumoCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                    if (posClienteRaiz < contContribuintesRaiz - 1)
                                    {
                                        resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesVenda[i][k]["vSeg"])).ToString();
                                    }
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
                            if (notesTranferencia[i][j].ContainsKey("vProd") && notesTranferencia[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias += Convert.ToDecimal(notesTranferencia[i][j]["vProd"]);
                                if (posClienteRaiz < contContribuintesRaiz - 1) { 
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesTranferencia[i][j]["vProd"])).ToString();
                                }
                            }

                            if (notesTranferencia[i][j].ContainsKey("vFrete") && notesTranferencia[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias += Convert.ToDecimal(notesTranferencia[i][j]["vFrete"]);
                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                {
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesTranferencia[i][j]["vFrete"])).ToString();

                                }
                            }
                            if (notesTranferencia[i][j].ContainsKey("vDesc") && notesTranferencia[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias -= Convert.ToDecimal(notesTranferencia[i][j]["vDesc"]);
                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                {
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) - Convert.ToDecimal(notesTranferencia[i][j]["vDesc"])).ToString();

                                }
                            }

                            if (notesTranferencia[i][j].ContainsKey("vOutro") && notesTranferencia[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias += Convert.ToDecimal(notesTranferencia[i][j]["vOutro"]);
                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                {
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesTranferencia[i][j]["vOutro"])).ToString();

                                }
                            }

                            if (notesTranferencia[i][j].ContainsKey("vSeg") && notesTranferencia[i][j].ContainsKey("cProd"))
                            {
                                totalTranferencias += Convert.ToDecimal(notesTranferencia[i][j]["vSeg"]);
                                if (posClienteRaiz < contContribuintesRaiz - 1)
                                {
                                    resumoAllCnpjRaiz[posClienteRaiz, 1] = (Convert.ToDecimal(resumoAllCnpjRaiz[posClienteRaiz, 1]) + Convert.ToDecimal(notesTranferencia[i][j]["vSeg"])).ToString();

                                }
                            }
                        }
                    }
                   
                    decimal totalNcontribuinte = Convert.ToDecimal(resumoCnpjRaiz[contContribuintesRaiz - 1, 1]);
                    decimal totalContribuinte = totalVendas - totalNcontribuinte;
                    totalSaida = totalVendas + totalTranferencias;
                    decimal limiteContribuinte = (totalVendas * Convert.ToDecimal(comp.VendaContribuinte)) / 100,
                        limiteNcm = (totalVendas * Convert.ToDecimal(comp.VendaAnexo)) / 100,
                        limiteGrupo = (totalSaida * Convert.ToDecimal(comp.VendaMGrupo)) / 100,
                        limiteTransferencia = (totalEntradas * Convert.ToDecimal(comp.TransferenciaInterExcedente)) / 100;

                    decimal impostoContribuinte = 0, excedenteContribuinte = 0, excedenteNcm = 0 , impostoNcm = 0, excedenteTranfInter = 0, impostoTransfInter = 0;
                    

                    //CNPJ
                    List<List<string>> gruposExecentes = new List<List<string>>();
                    decimal totalExcedente = 0, totalImpostoGrupo = 0;
                    for (int i = 0; i < contContribuintesRaiz - 1; i++)
                    {
                        var totalVendaGrupo = Convert.ToDecimal(resumoAllCnpjRaiz[i, 1]);
                        if (totalVendaGrupo > limiteGrupo)
                        {
                            List<string> grupoExcedente = new List<string>();
                            var cnpjGrupo = resumoAllCnpjRaiz[i, 0];
                            var clientGrupo = _clientService.FindByRaiz(cnpjGrupo);
                            var nomeGrupo = clientGrupo.Name;
                            var percentualGrupo = Math.Round((totalVendaGrupo / totalSaida) * 100, 2);
                            var excedenteGrupo = totalVendaGrupo - limiteGrupo;
                            var impostoGrupo = (excedenteGrupo * Convert.ToDecimal(comp.VendaMGrupoExcedente)) / 100;
                            totalExcedente += totalVendaGrupo;
                            totalImpostoGrupo += impostoGrupo;
                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");
                            grupoExcedente.Add(cnpjGrupo);
                            grupoExcedente.Add(nomeGrupo);
                            grupoExcedente.Add(percentualGrupo.ToString());
                            grupoExcedente.Add(Math.Round(Convert.ToDouble(totalVendaGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                            grupoExcedente.Add(Math.Round(Convert.ToDouble(limiteGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                            grupoExcedente.Add(Math.Round(Convert.ToDouble(excedenteGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                            grupoExcedente.Add(comp.VendaMGrupoExcedente.ToString());
                            grupoExcedente.Add(Math.Round(Convert.ToDouble(impostoGrupo.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "").ToString());
                            gruposExecentes.Add(grupoExcedente);
                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                        }
                    }


                    //Anexo II
                    if (totalNcm < limiteNcm)
                    {
                        excedenteNcm = limiteNcm - totalNcm;
                        impostoNcm = (excedenteNcm * Convert.ToDecimal(comp.VendaAnexoExcedente)) / 100;
                    }

                    //Contribuinte
                    if (totalContribuinte < limiteContribuinte)
                    {
                        excedenteContribuinte = limiteContribuinte - totalContribuinte;
                        impostoContribuinte = (excedenteContribuinte * Convert.ToDecimal(comp.VendaContribuinteExcedente)) / 100;
                    }

                    // Transferência inter
                    if (limiteTransferencia < totalTranferenciaInter)
                    {
                        excedenteTranfInter = totalTranferenciaInter - limiteTransferencia;
                        impostoTransfInter = (excedenteTranfInter * Convert.ToDecimal(comp.TransferenciaInterExcedente)) / 100;
                    }

                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                    //Geral
                    ViewBag.Contribuinte = Math.Round(Convert.ToDouble(totalContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.NContribuinte = Math.Round(Convert.ToDouble(totalNcontribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalSaida = Math.Round(Convert.ToDouble(totalSaida.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.VendaAnexo = Math.Round(Convert.ToDouble(totalNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalTransferencia = Math.Round(Convert.ToDouble(totalTranferenciaInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //CNPJ
                    ViewBag.PercentualCNPJ = comp.VendaMGrupo;
                    ViewBag.TotalExecedente = Math.Round(Convert.ToDouble(totalExcedente.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.Grupo = gruposExecentes;

                    //Anexo II
                    ViewBag.LimiteAnexo = Math.Round(Convert.ToDouble(limiteNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ExcedenteAnexo = Math.Round(Convert.ToDouble(excedenteNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualExcedenteAnexo = comp.VendaAnexoExcedente;
                    ViewBag.TotalExcedenteAnexo = Math.Round(Convert.ToDouble(impostoNcm.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //Contribuinte
                    ViewBag.LimiteContribuinte = Math.Round(Convert.ToDouble(limiteContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ExcedenteContribuinte = Math.Round(Convert.ToDouble(excedenteContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualExcedenteContribuinte = comp.VendaContribuinteExcedente;
                    ViewBag.TotalExcedenteContribuinte = Math.Round(Convert.ToDouble(impostoContribuinte.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //Tranferência
                    ViewBag.LimiteTransferencia = Math.Round(Convert.ToDouble(limiteTransferencia.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ExcedenteTransferencia = Math.Round(Convert.ToDouble(excedenteTranfInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentaulTransferencia = comp.TransferenciaInterExcedente;
                    ViewBag.TotalExcedenteTransferencia = Math.Round(Convert.ToDouble(impostoTransfInter.ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //Total Icms
                    ViewBag.TotalIcms = Math.Round(Convert.ToDouble((impostoNcm + impostoContribuinte + impostoTransfInter + totalImpostoGrupo).ToString().Replace(".", ",")), 2).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");


                    //Dar
                    var dar = _darService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Type.Equals("Icms")).FirstOrDefault();
                    ViewBag.Dar = dar.Code;
                }
                else if (type.Equals("anexo"))
                {
                    List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();                    
                    notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                    var ncms = _ncmConvenioService.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId));

                    var contNcm = ncms.Count();
                    string[,] resumoAnexo = new string[contNcm, 4];

                    for (int i = 0; i < contNcm; i++)
                    {
                        resumoAnexo[i, 0] = ncms[i].Ncm;
                        resumoAnexo[i, 1] = ncms[i].Description;
                        resumoAnexo[i, 2] = "0";
                        resumoAnexo[i, 3] = "0";
                    }

                    for (int i = notesVenda.Count - 1; i >= 0; i--)
                    {
                        if (!notesVenda[i][2]["CNPJ"].Equals(comp.Document) || notesVenda[i].Count <= 5)
                        {
                            notesVenda.RemoveAt(i);
                        }

                    }

                    decimal totalVendas = 0;
                    bool status = false;

                    for (int i = 0; i < notesVenda.Count(); i++)
                    {
                        int pos = 0;
                        for (int j = 0; j < notesVenda[i].Count(); j++)
                        {
                            if (notesVenda[i][j].ContainsKey("NCM"))
                            {
                                status = false;

                                for (int k = 0; k < ncms.Count; k++)
                                {
                                    int tamanho = ncms[k].Ncm.Length;
                                    if (ncms[k].Ncm.Equals(notesVenda[i][j]["NCM"].Substring(0, tamanho)))
                                    {
                                        status = true;
                                        pos = k;
                                        break;
                                    }
                                }
                            }

                            if (status == true)
                            {
                                if (notesVenda[i][j].ContainsKey("vProd") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vProd"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) + Convert.ToDecimal(notesVenda[i][j]["vProd"])).ToString();
 
                                }

                                if (notesVenda[i][j].ContainsKey("vFrete") && notesVenda[i][j].ContainsKey("cProd"))
                                {

                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vFrete"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) + Convert.ToDecimal(notesVenda[i][j]["vFrete"])).ToString();

                                }

                                if (notesVenda[i][j].ContainsKey("vDesc") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas -= Convert.ToDecimal(notesVenda[i][j]["vDesc"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) - Convert.ToDecimal(notesVenda[i][j]["vDesc"])).ToString();

                                }

                                if (notesVenda[i][j].ContainsKey("vOutro") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vOutro"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) + Convert.ToDecimal(notesVenda[i][j]["vOutro"])).ToString();

                                }

                                if (notesVenda[i][j].ContainsKey("vSeg") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vSeg"]);
                                    resumoAnexo[pos, 2] = (Convert.ToDecimal(resumoAnexo[pos, 2]) + Convert.ToDecimal(notesVenda[i][j]["vSeg"])).ToString();
                                }
                            }
                            else
                            {
                                if (notesVenda[i][j].ContainsKey("vProd") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vProd"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vFrete") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vFrete"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vDesc") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas -= Convert.ToDecimal(notesVenda[i][j]["vDesc"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vOutro") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vOutro"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vSeg") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vSeg"]);
                                }
                            }
                        }
                       
                    }

                    decimal percentualTotal = 0;
                    decimal valorTotalNcm = 0;
                    for (int i = 0; i < contNcm; i++)
                    {
                        if (resumoAnexo[i, 2] != "0")
                        {
                            resumoAnexo[i, 3] = ((Convert.ToDecimal(resumoAnexo[i, 2]) * 100) / totalVendas).ToString();
                            valorTotalNcm += Convert.ToDecimal(resumoAnexo[i, 2]);
                            percentualTotal += Convert.ToDecimal(resumoAnexo[i, 3]);
                        }
                    }
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                    List<List<string>> ncm_list = new List<List<string>>();

                    for (int i = 0; i < contNcm; i++)
                    {
                        if (resumoAnexo[i, 2] != "0")
                        {
                            List<string> ncm = new List<string>();
                            ncm.Add(resumoAnexo[i, 0]);
                            ncm.Add(resumoAnexo[i, 1]);

                            ncm.Add(Convert.ToDouble(resumoAnexo[i, 2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            ncm.Add(Convert.ToDouble(resumoAnexo[i, 3].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            ncm_list.Add(ncm);
                        }
                    }

                    ViewBag.TotalNcm = valorTotalNcm.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualTotal = percentualTotal.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalVendas = totalVendas.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""); ;
                    ViewBag.ResumoNcm = ncm_list;

                }
                else if (type.Equals("foraAnexo"))
                {
                    List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                    notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                    var ncms = _ncmConvenioService.FindByNcmAnnex(Convert.ToInt32(comp.AnnexId));

                    for (int i = notesVenda.Count - 1; i >= 0; i--)
                    {
                        if (!notesVenda[i][2]["CNPJ"].Equals(comp.Document) || notesVenda[i].Count <= 5)
                        {
                            notesVenda.RemoveAt(i);
                        }

                    }
                    
                    List<List<string>> ncmsForaAnexo = new List<List<string>>();
                    bool status = false;
                    decimal totalVendas = 0;
                    for (int i = 0; i < notesVenda.Count(); i++)
                    {
                        string ncm = "";
                        for (int j = 0; j < notesVenda[i].Count(); j++)
                        {
                            if (notesVenda[i][j].ContainsKey("NCM"))
                            {
                                status = false;
                                ncm = "";
                                for (int k = 0; k < ncms.Count; k++)
                                {
                                    int tamanho = ncms[k].Ncm.Length;
                                    if (ncms[k].Ncm.Equals(notesVenda[i][j]["NCM"].Substring(0, tamanho)))
                                    {
                                        status = true;
                                        break;
                                    }
                                }

                                if(status == false)
                                {
                                    ncm = notesVenda[i][j]["NCM"];
                                }
                            }

                            if (status == false)
                            {
                                List<string> ncmForaAnexo = new List<string>();
                                int pos = -1; 
                                for(int k = 0; k < ncmsForaAnexo.Count(); k++)
                                {
                                    if (ncmsForaAnexo[k][0].Equals(ncm))
                                    {
                                        pos = k;
                                    }
                                }
                                
                                if(pos < 0)
                                {
                                    ncmForaAnexo.Add(ncm);
                                    ncmForaAnexo.Add("0");
                                    ncmForaAnexo.Add("0");
                                    ncmsForaAnexo.Add(ncmForaAnexo);
                                    var x = ncmsForaAnexo.IndexOf(ncmForaAnexo);
                                    pos = ncmsForaAnexo.Count() - 1;
                                }
                                if (notesVenda[i][j].ContainsKey("vProd") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) + Convert.ToDecimal(notesVenda[i][j]["vProd"])).ToString();
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vProd"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vFrete") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) + Convert.ToDecimal(notesVenda[i][j]["vFrete"])).ToString();
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vFrete"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vDesc") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) - Convert.ToDecimal(notesVenda[i][j]["vDesc"])).ToString();
                                    totalVendas -= Convert.ToDecimal(notesVenda[i][j]["vDesc"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vOutro") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) + Convert.ToDecimal(notesVenda[i][j]["vOutro"])).ToString();
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vOutro"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vSeg") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    ncmsForaAnexo[pos][1] = (Convert.ToDecimal(ncmsForaAnexo[pos][1]) + Convert.ToDecimal(notesVenda[i][j]["vSeg"])).ToString();
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vSeg"]);
                                }
                            }
                            else
                            {
                                if (notesVenda[i][j].ContainsKey("vProd") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vProd"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vFrete") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vFrete"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vDesc") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas -= Convert.ToDecimal(notesVenda[i][j]["vDesc"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vOutro") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vOutro"]);
                                }

                                if (notesVenda[i][j].ContainsKey("vSeg") && notesVenda[i][j].ContainsKey("cProd"))
                                {
                                    totalVendas += Convert.ToDecimal(notesVenda[i][j]["vSeg"]);
                                }
                            }

                        }
                    }

                    decimal valorTotalNcm = 0, percentualTotal = 0;

                    foreach (var ncm in ncmsForaAnexo)
                    {
                        if (!ncm[1].Equals("0"))
                        {
                            ncm[2] = ((Convert.ToDecimal(ncm[1]) * 100) / totalVendas).ToString();
                            valorTotalNcm += Convert.ToDecimal(ncm[1]);
                            percentualTotal += Convert.ToDecimal(ncm[2]);

                        }
                    }

                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");
                    List<List<string>> ncmsForaAnexoList = new List<List<string>>();

                    foreach (var ncm in ncmsForaAnexo)
                    {
                        if (!ncm[1].Equals("0"))
                        {
                            List<string> ncmFora = new List<string>();
                            ncmFora.Add(ncm[0]);
                            ncmFora.Add(Convert.ToDouble(ncm[1].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            ncmFora.Add(Convert.ToDouble(ncm[2].Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""));
                            ncmsForaAnexoList.Add(ncmFora);

                        }
                    }

                    ViewBag.Ncm = ncmsForaAnexoList;
                    ViewBag.PercentualTotal = percentualTotal.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.TotalVendas = totalVendas.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", ""); 
                    ViewBag.TotalNcm = valorTotalNcm.ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                }
                else if (type.Equals("incentivo"))
                {
                    var productincentivo = _productIncentivoService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.CompanyId.Equals(id)).ToList();
                    var codeProdI = productincentivo.Where(_ => _.TypeTaxation.Equals("Incentivado")).Select(_ => _.Code).ToList();
                    var codeProdNI = productincentivo.Where(_ => _.TypeTaxation.Equals("Não Incentivado") && _.TypeTaxation.Equals("Isento")).Select(_ => _.Code).ToList();

                    if (arquivo == null || arquivo.Length == 0)
                    {
                        ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                        return View(ViewData);
                    }

                    string nomeArquivo = comp.Document + year + month;

                    if (arquivo.FileName.Contains(".txt"))
                        nomeArquivo += ".txt";
                    else
                        nomeArquivo += ".tmp";

                    string caminho_WebRoot = _appEnvironment.WebRootPath;
                    string caminhoDestinoArquivo = caminho_WebRoot + "\\Uploads\\Speds\\";
                    string caminhoDestinoArquivoOriginal = caminhoDestinoArquivo + nomeArquivo;

                    string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivo);
                    if (System.IO.File.Exists(caminhoDestinoArquivoOriginal))
                    {
                        System.IO.File.Delete(caminhoDestinoArquivoOriginal);
                    }
                    var stream = new FileStream(caminhoDestinoArquivoOriginal, FileMode.Create);
                    await arquivo.CopyToAsync(stream);
                    stream.Close();
                    decimal creditosIcms = import.SpedCredito(caminhoDestinoArquivoOriginal, comp.Id),
                         debitosIcms = 0;

                    List<List<Dictionary<string, string>>> notesVenda = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesVendaSt = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesEntradaVenda = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesEntradaDevo = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesSaidaDevo = new List<List<Dictionary<string, string>>>();
                    //List<List<Dictionary<string, string>>> notesSaidaDevolucao = new List<List<Dictionary<string, string>>>();
                    var contribuintes = _clientService.FindByContribuinte(id, "all");
                    notesVenda = import.NfeExit(directoryNfeExit, id, type, "venda");
                    notesVendaSt = import.NfeExit(directoryNfeExit, id, type, "vendaSt");
                    notesSaidaDevo = import.NfeExit(directoryNfeExit, id, type, "devolução de saida");

                    decimal totalVendas = 0, naoContriForaDoEstadoIncentivo = 0, ContribuintesIncentivo = 0,
                        naoContriDentroDoEstadoIncentivo = 0, naoContriForaDoEstadoNIncentivo = 0, 
                        ContribuintesNIncentivo = 0, naoContribuinteDentroDoEstadoNIncentivo = 0, naoContriForaDoEstado = 0;

                    for (int i = notesVenda.Count - 1; i >= 0; i--)
                    {
                        if (!notesVenda[i][2]["CNPJ"].Equals(comp.Document) || notesVenda[i].Count <= 5)
                        {
                            notesVenda.RemoveAt(i);
                            continue;
                        }

                        int posCliente = -1;

                        if (notesVenda[i][3].ContainsKey("CNPJ"))
                        {
                            if (contribuintes.Contains(notesVenda[i][3]["CNPJ"]))
                            {
                                posCliente = contribuintes.IndexOf(notesVenda[i][3]["CNPJ"]);
                            }

                        }

                        for (int k = 0; k < notesVenda[i].Count(); k++)
                        {
                            if (notesVenda[i][k].ContainsKey("cProd"))
                            {
                                if (codeProdI.Contains(notesVenda[i][k]["cProd"]))
                                {
                                    if (notesVenda[i][k].ContainsKey("vProd"))
                                    {
                                        if (posCliente < 0)
                                        {
                                            if (notesVenda[i][1]["idDest"].Equals("2"))
                                            {
                                                naoContriForaDoEstadoIncentivo += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                            }
                                            else
                                            {
                                                naoContriDentroDoEstadoIncentivo += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                            }
                                        }
                                        else
                                        {
                                            ContribuintesIncentivo += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                        }
                                        totalVendas += Convert.ToDecimal(notesVenda[i][k]["vProd"]);

                                    }

                                    if (notesVenda[i][k].ContainsKey("vFrete"))
                                    {
                                        if (posCliente < 0)
                                        {
                                            if (notesVenda[i][1]["idDest"].Equals("2"))
                                            {
                                                naoContriForaDoEstadoIncentivo += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                            }
                                            else
                                            {
                                                naoContriDentroDoEstadoIncentivo += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                            }
                                        }
                                        else
                                        {
                                            ContribuintesIncentivo += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                        }
                                        totalVendas += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);

                                    }

                                    if (notesVenda[i][k].ContainsKey("vDesc"))
                                    {
                                        if (posCliente < 0)
                                        {
                                            if (notesVenda[i][1]["idDest"].Equals("2"))
                                            {
                                                naoContriForaDoEstadoIncentivo -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                            }
                                            else
                                            {
                                                naoContriDentroDoEstadoIncentivo -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                            }
                                        }
                                        else
                                        {
                                            ContribuintesIncentivo -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                        }
                                        totalVendas -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);

                                    }

                                    if (notesVenda[i][k].ContainsKey("vOutro"))
                                    {
                                        if (posCliente < 0)
                                        {
                                            if (notesVenda[i][1]["idDest"].Equals("2"))
                                            {
                                                naoContriForaDoEstadoIncentivo += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                            }
                                            else
                                            {
                                                naoContriDentroDoEstadoIncentivo += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                            }
                                        }
                                        else
                                        {
                                            ContribuintesIncentivo += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                        }
                                        totalVendas += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);

                                    }

                                    if (notesVenda[i][k].ContainsKey("vSeg"))
                                    {
                                        if (posCliente < 0)
                                        {
                                            if (notesVenda[i][1]["idDest"].Equals("2"))
                                            {
                                                naoContriForaDoEstadoIncentivo += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                            }
                                            else
                                            {
                                                naoContriDentroDoEstadoIncentivo += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                            }
                                        }
                                        else
                                        {
                                            ContribuintesIncentivo += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                        }
                                        totalVendas += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);

                                    }
                                }
                                else if(codeProdNI.Contains(notesVenda[i][k]["cProd"]))
                                {
                                    if (notesVenda[i][k].ContainsKey("vProd"))
                                    {
                                        if (posCliente < 0)
                                        {
                                            if (!notesVenda[i][1]["idDest"].Equals("2"))
                                            {
                                                naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                            }
                                            else
                                            {
                                                naoContribuinteDentroDoEstadoNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                            }
                                        }
                                        else
                                        {
                                            ContribuintesNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vProd"]);
                                        }
                                        totalVendas += Convert.ToDecimal(notesVenda[i][k]["vProd"]);

                                    }

                                    if (notesVenda[i][k].ContainsKey("vFrete"))
                                    {
                                        if (posCliente < 0)
                                        {
                                            if (notesVenda[i][1]["idDest"].Equals("2"))
                                            {
                                                naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                            }
                                            else
                                            {
                                                naoContribuinteDentroDoEstadoNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                            }
                                        }
                                        else
                                        {
                                            ContribuintesNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);
                                        }
                                        totalVendas += Convert.ToDecimal(notesVenda[i][k]["vFrete"]);

                                    }

                                    if (notesVenda[i][k].ContainsKey("vDesc"))
                                    {
                                        if (posCliente < 0)
                                        {
                                            if (notesVenda[i][1]["idDest"].Equals("2"))
                                            {
                                                naoContriForaDoEstadoNIncentivo -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                            }
                                            else
                                            {
                                                naoContribuinteDentroDoEstadoNIncentivo -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                            }
                                        }
                                        else
                                        {
                                            ContribuintesNIncentivo -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);
                                        }
                                        totalVendas -= Convert.ToDecimal(notesVenda[i][k]["vDesc"]);

                                    }

                                    if (notesVenda[i][k].ContainsKey("vOutro"))
                                    {
                                        if (posCliente < 0)
                                        {
                                            if (notesVenda[i][1]["idDest"].Equals("2"))
                                            {
                                                naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                            }
                                            else
                                            {
                                                naoContribuinteDentroDoEstadoNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                            }
                                        }
                                        else
                                        {
                                            ContribuintesNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);
                                        }
                                        totalVendas += Convert.ToDecimal(notesVenda[i][k]["vOutro"]);

                                    }

                                    if (notesVenda[i][k].ContainsKey("vSeg"))
                                    {
                                        if (posCliente < 0)
                                        {
                                            if (notesVenda[i][1]["idDest"].Equals("2"))
                                            {
                                                naoContriForaDoEstadoNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                            }
                                            else
                                            {
                                                naoContribuinteDentroDoEstadoNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                            }
                                        }
                                        else
                                        {
                                            ContribuintesNIncentivo += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);
                                        }
                                        totalVendas += Convert.ToDecimal(notesVenda[i][k]["vSeg"]);

                                    }
                                }

                            }
                            if (notesVenda[i][k].ContainsKey("pICMS") && notesVenda[i][k].ContainsKey("CST") && notesVenda[i][k].ContainsKey("orig"))
                            {
                                debitosIcms += (Convert.ToDecimal(notesVenda[i][k]["pICMS"]) * Convert.ToDecimal(notesVenda[i][k]["vBC"])) / 100;
                            }
                        }

                    }
                    for (int i = notesVendaSt.Count - 1; i >= 0; i--)
                    {
                        if ((!notesVendaSt[i][2]["CNPJ"].Equals(comp.Document) || notesVendaSt[i].Count <= 5))
                        {
                            notesVendaSt.RemoveAt(i);
                            continue;
                        }
                        else if (notesVendaSt[i][3].ContainsKey("CNPJ"))
                        {
                            if (contribuintes.Contains(notesVendaSt[i][3]["CNPJ"]))
                            {
                                notesVendaSt.RemoveAt(i);
                            }
                            continue;
                        }

                        for (int k = 0; k < notesVendaSt[i].Count(); k++)
                        {
                            if (notesVendaSt[i][k].ContainsKey("vProd") && notesVendaSt[i][k].ContainsKey("cProd"))
                            {
                                naoContriForaDoEstadoIncentivo += Convert.ToDecimal(notesVendaSt[i][k]["vProd"]);
                                if (notesVendaSt[i][1]["idDest"].Equals("2"))
                                {
                                    naoContriForaDoEstado += Convert.ToDecimal(notesVendaSt[i][k]["vProd"]);
                                }
                               
                                totalVendas += Convert.ToDecimal(notesVendaSt[i][k]["vProd"]);

                            }

                            if (notesVendaSt[i][k].ContainsKey("vFrete") && notesVendaSt[i][k].ContainsKey("cProd"))
                            {
                                naoContriForaDoEstadoIncentivo += Convert.ToDecimal(notesVendaSt[i][k]["vFrete"]);

                                if (notesVendaSt[i][1]["idDest"].Equals("2"))
                                {
                                    naoContriForaDoEstado += Convert.ToDecimal(notesVendaSt[i][k]["vFrete"]);
                                }
                                totalVendas += Convert.ToDecimal(notesVendaSt[i][k]["vFrete"]);

                            }

                            if (notesVendaSt[i][k].ContainsKey("vDesc") && notesVendaSt[i][k].ContainsKey("cProd"))
                            {
                                naoContriForaDoEstadoIncentivo -= Convert.ToDecimal(notesVendaSt[i][k]["vDesc"]);
                                if (notesVendaSt[i][1]["idDest"].Equals("2"))
                                {
                                    naoContriForaDoEstado -= Convert.ToDecimal(notesVendaSt[i][k]["vDesc"]);
                                }
                                
                                totalVendas -= Convert.ToDecimal(notesVendaSt[i][k]["vDesc"]);

                            }

                            if (notesVendaSt[i][k].ContainsKey("vOutro") && notesVendaSt[i][k].ContainsKey("cProd"))
                            {
                                naoContriForaDoEstadoIncentivo += Convert.ToDecimal(notesVendaSt[i][k]["vOutro"]);
                                if (notesVendaSt[i][1]["idDest"].Equals("2"))
                                {
                                    naoContriForaDoEstado += Convert.ToDecimal(notesVendaSt[i][k]["Outro"]);
                                }
                           
                                totalVendas += Convert.ToDecimal(notesVendaSt[i][k]["vOutro"]);

                            }

                            if (notesVendaSt[i][k].ContainsKey("vSeg") && notesVendaSt[i][k].ContainsKey("cProd"))
                            {
                                naoContriForaDoEstadoIncentivo += Convert.ToDecimal(notesVendaSt[i][k]["vSeg"]);

                                if (notesVendaSt[i][1]["idDest"].Equals("2"))
                                {
                                    naoContriForaDoEstado += Convert.ToDecimal(notesVendaSt[i][k]["vSeg"]);
                                }
                              
                                totalVendas += Convert.ToDecimal(notesVendaSt[i][k]["vSeg"]);
                            }

                        }


                    }

                    for (int i = notesSaidaDevo.Count - 1; i >= 0; i--)
                    {
                        if (notesSaidaDevo[i][2].ContainsKey("CNPJ"))
                        {
                            if (!notesSaidaDevo[i][2]["CNPJ"].Equals(comp.Document) || notesSaidaDevo[i].Count <= 5)
                            {
                                notesSaidaDevo.RemoveAt(i);
                                continue;
                            }
                        }
                        for (int k = 0; k < notesSaidaDevo[i].Count; k++)
                        {
                            if (notesSaidaDevo[i][k].ContainsKey("pICMS") && notesSaidaDevo[i][k].ContainsKey("CST") && notesSaidaDevo[i][k].ContainsKey("orig"))
                            {
                                creditosIcms += (Convert.ToDecimal(notesSaidaDevo[i][k]["pICMS"]) * Convert.ToDecimal(notesSaidaDevo[i][k]["vBC"])) / 100;
                            }
                        }
                    }


                    //// Cálculos de Incentivo
                    
                    //Contribuinte
                    var icmsContribuinteIncentivo = Math.Round(Convert.ToDecimal(comp.Icms) * ContribuintesIncentivo/ 100, 2);
                    double valorVendaContriIncentivo = (Convert.ToDouble(ContribuintesIncentivo) * Convert.ToDouble(comp.Icms)) / 100;

                    //Não Contribuinte Fora do Estado
                    var icmsNContribuinteForaDoEstado = Math.Round(Convert.ToDecimal(comp.IcmsNContribuinteFora) * naoContriForaDoEstado / 100, 2);

                    //Não Contribuinte Dentro do Estado
                    var icmsNContribuinteDentroDoEstado = Math.Round(Convert.ToDecimal(comp.IcmsNContribuinte) * naoContriDentroDoEstadoIncentivo / 100, 2);

                    //// FUNEF
                    var BCFunef = debitosIcms - creditosIcms - icmsContribuinteIncentivo;
                    var vFunef = BCFunef * Convert.ToDecimal(comp.Funef) / 100;



                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                    //// Valores do Incentivo

                    //Contribuinte
                    ViewBag.VendaContribuinteIncentivo = Convert.ToDouble(ContribuintesIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualIcmsContrib = Convert.ToDouble(comp.Icms.ToString().Replace(".",",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.ValorVendaContribIncentivo = Convert.ToDouble(valorVendaContriIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //Não Contribuinte Dentro do Estado
                    ViewBag.VendaNDentroEstadoContribIncentivo = Convert.ToDouble(naoContriDentroDoEstadoIncentivo.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualIcmsNaoContribDentroDoEstado = Convert.ToDouble(comp.IcmsNContribuinte.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

                    //Não Contribuinte Fora do Estado
                    ViewBag.VendaNForaEstadoContribIncentivo = Convert.ToDouble(naoContriForaDoEstado.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");
                    ViewBag.PercentualIcmsNaoContribForaDoEstado = Convert.ToDouble(comp.IcmsNContribuinteFora.ToString().Replace(".", ",")).ToString("C2", CultureInfo.CurrentCulture).Replace("R$", "");

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