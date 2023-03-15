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
    public class ListingController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly ICfopService _cfopService;
        private readonly IHostingEnvironment _appEnvironment;

        public ListingController(
            ICompanyService companyService,
            IConfigurationService configurationService,
            ICfopService cfopService,
            IHostingEnvironment appEnvironment)
        {
            _companyService = companyService;
            _configurationService = configurationService;
            _cfopService = cfopService;
            _appEnvironment = appEnvironment;
        }

        public async Task<IActionResult> Index(long id, string year, string month, string opcao, string archive, IFormFile arquivoSped)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var ident = Request.Form["ident"].ToString();
                var aliquotIcms = Request.Form["aliquotIcms"].ToString();
                var aliquotFecop = Request.Form["aliquotFecop"].ToString();

                SessionManager.SetMonthInSession(month);
                SessionManager.SetYearInSession(year);

                var comp = _companyService.FindById(id, null);

                var importDir = new Diretorio.Import();
                var importXml = new Xml.Import();
                var importSped = new Sped.Import();

                string caminhoDestinoArquivoOriginalSped = "";
                string caminho_WebRoot = _appEnvironment.WebRootPath;

                List<List<string>> products = new List<List<string>>();
                var cfops = _cfopService.FindAll(null);

                if (ident.Equals("0"))
                {

                    string filedirSped = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Speds");

                    if (!Directory.Exists(filedirSped)) Directory.CreateDirectory(filedirSped);

                    string nomeArquivoSped = comp.Document + "Empresa";

                    if (arquivoSped.FileName.Contains(".txt"))
                        nomeArquivoSped += ".txt";
                    else
                        nomeArquivoSped += ".tmp";



                    string caminhoDestinoArquivoSped = caminho_WebRoot + "\\Uploads\\Speds\\";
                    caminhoDestinoArquivoOriginalSped = caminhoDestinoArquivoSped + nomeArquivoSped;

                    string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivoSped);
                    if (System.IO.File.Exists(caminhoDestinoArquivoOriginalSped))
                    {
                        System.IO.File.Delete(caminhoDestinoArquivoOriginalSped);

                    }
                    var streamSped = new FileStream(caminhoDestinoArquivoOriginalSped, FileMode.Create);
                    await arquivoSped.CopyToAsync(streamSped);
                    streamSped.Close();
                    if (aliquotIcms.Equals(""))
                        products = importSped.NFeProduct(caminhoDestinoArquivoOriginalSped, cfops);
                    else
                    {
                        if(aliquotFecop.Equals(""))
                            products = importSped.NFeProduct(caminhoDestinoArquivoOriginalSped, cfops, Convert.ToDecimal(aliquotIcms));
                    }
                }
                else
                {
                    var confDBSisctNfe = _configurationService.FindByName("NFe Saida");

                    string directoryNfe = "";
                    if (archive.Equals("xmlE"))
                        directoryNfe = importDir.SaidaEmpresa(comp, confDBSisctNfe.Value, year, month);
                    else
                        directoryNfe = importDir.SaidaSefaz(comp, confDBSisctNfe.Value, year, month);

                    List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

                    notes = importXml.NFeAll(directoryNfe);

                    if (opcao.Equals("sEmissao"))
                    {
                        if (aliquotIcms.Equals(""))
                        {
                            for (int i = notes.Count - 1; i >= 0; i--)
                            {
                                if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i][1]["tpNF"].Equals("0"))
                                {
                                    notes.RemoveAt(i);
                                    continue;
                                }

                                string CFOP = "", cProd = "", xProd = "", pICMS = "0", pFCP = "0";
                                decimal vProd = 0, vBC = 0, vICMS = 0, vFCP = 0;
                                int pos = -1;
                                bool status = false;

                                for (int j = 0; j < notes[i].Count(); j++)
                                {
                                    if ((notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM")))
                                    {
                                        if (status)
                                        {
                                            for (int e = 0; e < products.Count(); e++)
                                            {
                                                if (products[e][0].Equals(CFOP) && products[e][2].Equals(cProd) && Convert.ToDecimal(products[e][6]).Equals(Convert.ToDecimal(pICMS)) &&
                                                    Convert.ToDecimal(products[e][8]).Equals(Convert.ToDecimal(pFCP)))
                                                {
                                                    pos = e;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var cfp = cfops.Where(_ => _.Code.Equals(CFOP)).FirstOrDefault();
                                                List<string> cc = new List<string>();
                                                cc.Add(CFOP);
                                                cc.Add(cfp.Description);
                                                cc.Add(cProd);
                                                cc.Add(xProd);
                                                cc.Add(vProd.ToString());
                                                cc.Add(vBC.ToString());
                                                cc.Add(pICMS);
                                                cc.Add(vICMS.ToString());
                                                cc.Add(pFCP);
                                                cc.Add(vFCP.ToString());
                                                products.Add(cc);
                                            }
                                            else
                                            {
                                                products[pos][4] = (Convert.ToDecimal(products[pos][4]) + vProd).ToString();
                                                products[pos][5] = (Convert.ToDecimal(products[pos][5]) + vBC).ToString();
                                                products[pos][7] = (Convert.ToDecimal(products[pos][7]) + vICMS).ToString();
                                                products[pos][9] = (Convert.ToDecimal(products[pos][9]) + vFCP).ToString();
                                            }
                                        }

                                        pos = -1;
                                        status = false;
                                        CFOP = notes[i][j]["CFOP"];
                                        cProd = notes[i][j]["cProd"];
                                        xProd = notes[i][j]["xProd"];
                                        vBC = 0;
                                        vICMS = 0;
                                        vFCP = 0;
                                        pICMS = "0";
                                        pFCP = "0";

                                        if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                        {
                                            vProd = 0;
                                            vProd += Convert.ToDecimal(notes[i][j]["vProd"]);
                                            status = true;
                                        }

                                        if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                            vProd += Convert.ToDecimal(notes[i][j]["vFrete"]);

                                        if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                            vProd -= Convert.ToDecimal(notes[i][j]["vDesc"]);

                                        if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                            vProd += Convert.ToDecimal(notes[i][j]["vOutro"]);

                                        if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                            vProd += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                    }


                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                    {
                                        vBC = Convert.ToDecimal(notes[i][j]["vBC"]);
                                        status = true;
                                    }


                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        pICMS = notes[i][j]["pICMS"];
                                        vICMS = Convert.ToDecimal(notes[i][j]["vICMS"]);
                                        status = true;
                                    }


                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        pFCP = notes[i][j]["pFCP"];
                                        vFCP = Convert.ToDecimal(notes[i][j]["vFCP"]);
                                        status = true;
                                    }

                                    if (notes[i][j].ContainsKey("vNF"))
                                    {
                                        for (int e = 0; e < products.Count(); e++)
                                        {
                                            if (products[e][0].Equals(CFOP) && products[e][2].Equals(cProd) && Convert.ToDecimal(products[e][6]).Equals(Convert.ToDecimal(pICMS)) &&
                                                Convert.ToDecimal(products[e][8]).Equals(Convert.ToDecimal(pFCP)))
                                            {
                                                pos = e;
                                                break;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            var cfp = cfops.Where(_ => _.Code.Equals(CFOP)).FirstOrDefault();
                                            List<string> cc = new List<string>();
                                            cc.Add(CFOP);
                                            cc.Add(cfp.Description);
                                            cc.Add(cProd);
                                            cc.Add(xProd);
                                            cc.Add(vProd.ToString());
                                            cc.Add(vBC.ToString());
                                            cc.Add(pICMS);
                                            cc.Add(vICMS.ToString());
                                            cc.Add(pFCP);
                                            cc.Add(vFCP.ToString());
                                            products.Add(cc);
                                        }
                                        else
                                        {
                                            products[pos][4] = (Convert.ToDecimal(products[pos][4]) + vProd).ToString();
                                            products[pos][5] = (Convert.ToDecimal(products[pos][5]) + vBC).ToString();
                                            products[pos][7] = (Convert.ToDecimal(products[pos][7]) + vICMS).ToString();
                                            products[pos][9] = (Convert.ToDecimal(products[pos][9]) + vFCP).ToString();
                                        }
                                    }

                                }

                            }
                        }
                        else
                        {
                            if (aliquotFecop.Equals(""))
                            {
                                for (int i = notes.Count - 1; i >= 0; i--)
                                {

                                    if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i][1]["tpNF"].Equals("0"))
                                    {
                                        notes.RemoveAt(i);
                                        continue;
                                    }


                                    string CFOP = "", cProd = "", xProd = "", pICMS = "0", pFCP = "0";
                                    decimal vProd = 0, vBC = 0, vICMS = 0, vFCP = 0;
                                    int pos = -1;
                                    bool status = false;

                                    for (int j = 0; j < notes[i].Count(); j++)
                                    {
                                        if ((notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM")))
                                        {
                                            if (status && Convert.ToDecimal(pICMS).Equals(Convert.ToDecimal(aliquotIcms)) && Convert.ToDecimal(pFCP).Equals(0))
                                            {
                                                for (int e = 0; e < products.Count(); e++)
                                                {
                                                    if (products[e][0].Equals(CFOP) && products[e][2].Equals(cProd) && Convert.ToDecimal(products[e][6]).Equals(Convert.ToDecimal(pICMS)) &&
                                                        Convert.ToDecimal(products[e][8]).Equals(Convert.ToDecimal(pFCP)))
                                                    {
                                                        pos = e;
                                                        break;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    var cfp = cfops.Where(_ => _.Code.Equals(CFOP)).FirstOrDefault();
                                                    List<string> cc = new List<string>();
                                                    cc.Add(CFOP);
                                                    cc.Add(cfp.Description);
                                                    cc.Add(cProd);
                                                    cc.Add(xProd);
                                                    cc.Add(vProd.ToString());
                                                    cc.Add(vBC.ToString());
                                                    cc.Add(pICMS);
                                                    cc.Add(vICMS.ToString());
                                                    cc.Add(pFCP);
                                                    cc.Add(vFCP.ToString());
                                                    products.Add(cc);
                                                }
                                                else
                                                {
                                                    products[pos][4] = (Convert.ToDecimal(products[pos][4]) + vProd).ToString();
                                                    products[pos][5] = (Convert.ToDecimal(products[pos][5]) + vBC).ToString();
                                                    products[pos][7] = (Convert.ToDecimal(products[pos][7]) + vICMS).ToString();
                                                    products[pos][9] = (Convert.ToDecimal(products[pos][9]) + vFCP).ToString();
                                                }
                                            }

                                            pos = -1;
                                            status = false;
                                            CFOP = notes[i][j]["CFOP"];
                                            cProd = notes[i][j]["cProd"];
                                            xProd = notes[i][j]["xProd"];
                                            vBC = 0;
                                            vICMS = 0;
                                            vFCP = 0;
                                            pICMS = "0";
                                            pFCP = "0";

                                            if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                            {
                                                vProd = 0;
                                                vProd += Convert.ToDecimal(notes[i][j]["vProd"]);
                                                status = true;
                                            }

                                            if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                                vProd += Convert.ToDecimal(notes[i][j]["vFrete"]);

                                            if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                                vProd -= Convert.ToDecimal(notes[i][j]["vDesc"]);

                                            if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                                vProd += Convert.ToDecimal(notes[i][j]["vOutro"]);

                                            if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                                vProd += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                        }


                                        if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                        {
                                            vBC = Convert.ToDecimal(notes[i][j]["vBC"]);
                                            status = true;
                                        }


                                        if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            pICMS = notes[i][j]["pICMS"];
                                            vICMS = Convert.ToDecimal(notes[i][j]["vICMS"]);
                                            status = true;
                                        }


                                        if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            pFCP = notes[i][j]["pFCP"];
                                            vFCP = Convert.ToDecimal(notes[i][j]["vFCP"]);
                                            status = true;
                                        }

                                        if (notes[i][j].ContainsKey("vNF"))
                                        {
                                            if (Convert.ToDecimal(pICMS).Equals(Convert.ToDecimal(aliquotIcms)) && Convert.ToDecimal(pFCP).Equals(0))
                                            {
                                                for (int e = 0; e < products.Count(); e++)
                                                {
                                                    if (products[e][0].Equals(CFOP) && products[e][2].Equals(cProd) && Convert.ToDecimal(products[e][6]).Equals(Convert.ToDecimal(pICMS)) &&
                                                        Convert.ToDecimal(products[e][8]).Equals(Convert.ToDecimal(pFCP)))
                                                    {
                                                        pos = e;
                                                        break;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    var cfp = cfops.Where(_ => _.Code.Equals(CFOP)).FirstOrDefault();
                                                    List<string> cc = new List<string>();
                                                    cc.Add(CFOP);
                                                    cc.Add(cfp.Description);
                                                    cc.Add(cProd);
                                                    cc.Add(xProd);
                                                    cc.Add(vProd.ToString());
                                                    cc.Add(vBC.ToString());
                                                    cc.Add(pICMS);
                                                    cc.Add(vICMS.ToString());
                                                    cc.Add(pFCP);
                                                    cc.Add(vFCP.ToString());
                                                    products.Add(cc);
                                                }
                                                else
                                                {
                                                    products[pos][4] = (Convert.ToDecimal(products[pos][4]) + vProd).ToString();
                                                    products[pos][5] = (Convert.ToDecimal(products[pos][5]) + vBC).ToString();
                                                    products[pos][7] = (Convert.ToDecimal(products[pos][7]) + vICMS).ToString();
                                                    products[pos][9] = (Convert.ToDecimal(products[pos][9]) + vFCP).ToString();
                                                }
                                            }

                                        }
                                    }

                                }
                            }
                            else
                            {
                                for (int i = notes.Count - 1; i >= 0; i--)
                                {
                                    if (!notes[i][2]["CNPJ"].Equals(comp.Document) || notes[i][1]["tpNF"].Equals("0"))
                                    {
                                        notes.RemoveAt(i);
                                        continue;
                                    }


                                    string CFOP = "", cProd = "", xProd = "", pICMS = "0", pFCP = "0";
                                    decimal vProd = 0, vBC = 0, vICMS = 0, vFCP = 0;
                                    int pos = -1;
                                    bool status = false;

                                    for (int j = 0; j < notes[i].Count(); j++)
                                    {
                                        if ((notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM")))
                                        {
                                            if (status && Convert.ToDecimal(pICMS).Equals(Convert.ToDecimal(aliquotIcms)) && Convert.ToDecimal(pFCP).Equals(Convert.ToDecimal(aliquotFecop)))
                                            {
                                                for (int e = 0; e < products.Count(); e++)
                                                {
                                                    if (products[e][0].Equals(CFOP) && products[e][2].Equals(cProd) && Convert.ToDecimal(products[e][6]).Equals(Convert.ToDecimal(pICMS)) &&
                                                        Convert.ToDecimal(products[e][8]).Equals(Convert.ToDecimal(pFCP)))
                                                    {
                                                        pos = e;
                                                        break;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    var cfp = cfops.Where(_ => _.Code.Equals(CFOP)).FirstOrDefault();
                                                    List<string> cc = new List<string>();
                                                    cc.Add(CFOP);
                                                    cc.Add(cfp.Description);
                                                    cc.Add(cProd);
                                                    cc.Add(xProd);
                                                    cc.Add(vProd.ToString());
                                                    cc.Add(vBC.ToString());
                                                    cc.Add(pICMS);
                                                    cc.Add(vICMS.ToString());
                                                    cc.Add(pFCP);
                                                    cc.Add(vFCP.ToString());
                                                    products.Add(cc);
                                                }
                                                else
                                                {
                                                    products[pos][4] = (Convert.ToDecimal(products[pos][4]) + vProd).ToString();
                                                    products[pos][5] = (Convert.ToDecimal(products[pos][5]) + vBC).ToString();
                                                    products[pos][7] = (Convert.ToDecimal(products[pos][7]) + vICMS).ToString();
                                                    products[pos][9] = (Convert.ToDecimal(products[pos][9]) + vFCP).ToString();
                                                }
                                            }

                                            pos = -1;
                                            status = false;
                                            CFOP = notes[i][j]["CFOP"];
                                            cProd = notes[i][j]["cProd"];
                                            xProd = notes[i][j]["xProd"];
                                            vBC = 0;
                                            vICMS = 0;
                                            vFCP = 0;
                                            pICMS = "0";
                                            pFCP = "0";

                                            if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                            {
                                                vProd = 0;
                                                vProd += Convert.ToDecimal(notes[i][j]["vProd"]);
                                                status = true;
                                            }

                                            if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                                vProd += Convert.ToDecimal(notes[i][j]["vFrete"]);

                                            if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                                vProd -= Convert.ToDecimal(notes[i][j]["vDesc"]);

                                            if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                                vProd += Convert.ToDecimal(notes[i][j]["vOutro"]);

                                            if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                                vProd += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                        }


                                        if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                        {
                                            vBC = Convert.ToDecimal(notes[i][j]["vBC"]);
                                            status = true;
                                        }


                                        if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            pICMS = notes[i][j]["pICMS"];
                                            vICMS = Convert.ToDecimal(notes[i][j]["vICMS"]);
                                            status = true;
                                        }


                                        if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            pFCP = notes[i][j]["pFCP"];
                                            vFCP = Convert.ToDecimal(notes[i][j]["vFCP"]);
                                            status = true;
                                        }

                                        if (notes[i][j].ContainsKey("vNF"))
                                        {
                                            if (Convert.ToDecimal(pICMS).Equals(Convert.ToDecimal(aliquotIcms)) && Convert.ToDecimal(pFCP).Equals(Convert.ToDecimal(aliquotFecop)))
                                            {
                                                for (int e = 0; e < products.Count(); e++)
                                                {
                                                    if (products[e][0].Equals(CFOP) && products[e][2].Equals(cProd) && Convert.ToDecimal(products[e][6]).Equals(Convert.ToDecimal(pICMS)) &&
                                                        Convert.ToDecimal(products[e][8]).Equals(Convert.ToDecimal(pFCP)))
                                                    {
                                                        pos = e;
                                                        break;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    var cfp = cfops.Where(_ => _.Code.Equals(CFOP)).FirstOrDefault();
                                                    List<string> cc = new List<string>();
                                                    cc.Add(CFOP);
                                                    cc.Add(cfp.Description);
                                                    cc.Add(cProd);
                                                    cc.Add(xProd);
                                                    cc.Add(vProd.ToString());
                                                    cc.Add(vBC.ToString());
                                                    cc.Add(pICMS);
                                                    cc.Add(vICMS.ToString());
                                                    cc.Add(pFCP);
                                                    cc.Add(vFCP.ToString());
                                                    products.Add(cc);
                                                }
                                                else
                                                {
                                                    products[pos][4] = (Convert.ToDecimal(products[pos][4]) + vProd).ToString();
                                                    products[pos][5] = (Convert.ToDecimal(products[pos][5]) + vBC).ToString();
                                                    products[pos][7] = (Convert.ToDecimal(products[pos][7]) + vICMS).ToString();
                                                    products[pos][9] = (Convert.ToDecimal(products[pos][9]) + vFCP).ToString();
                                                }
                                            }

                                        }
                                    }

                                }
                            }
                        }
                    }
                    else if (opcao.Equals("cEmissao"))
                    {
                        if (aliquotIcms.Equals(""))
                        {
                            for (int i = notes.Count - 1; i >= 0; i--)
                            {
                                if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                                {
                                    notes.RemoveAt(i);
                                    continue;
                                }

                                string CFOP = "", cProd = "", xProd = "", pICMS = "0", pFCP = "0";
                                decimal vProd = 0, vBC = 0, vICMS = 0, vFCP = 0;
                                int pos = -1;
                                bool status = false;

                                for (int j = 0; j < notes[i].Count(); j++)
                                {
                                    if ((notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM")))
                                    {
                                        if (status)
                                        {
                                            for (int e = 0; e < products.Count(); e++)
                                            {
                                                if (products[e][0].Equals(CFOP) && products[e][2].Equals(cProd) && Convert.ToDecimal(products[e][6]).Equals(Convert.ToDecimal(pICMS)) &&
                                                    Convert.ToDecimal(products[e][8]).Equals(Convert.ToDecimal(pFCP)))
                                                {
                                                    pos = e;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                var cfp = cfops.Where(_ => _.Code.Equals(CFOP)).FirstOrDefault();
                                                List<string> cc = new List<string>();
                                                cc.Add(CFOP);
                                                cc.Add(cfp.Description);
                                                cc.Add(cProd);
                                                cc.Add(xProd);
                                                cc.Add(vProd.ToString());
                                                cc.Add(vBC.ToString());
                                                cc.Add(pICMS);
                                                cc.Add(vICMS.ToString());
                                                cc.Add(pFCP);
                                                cc.Add(vFCP.ToString());
                                                products.Add(cc);
                                            }
                                            else
                                            {
                                                products[pos][4] = (Convert.ToDecimal(products[pos][4]) + vProd).ToString();
                                                products[pos][5] = (Convert.ToDecimal(products[pos][5]) + vBC).ToString();
                                                products[pos][7] = (Convert.ToDecimal(products[pos][7]) + vICMS).ToString();
                                                products[pos][9] = (Convert.ToDecimal(products[pos][9]) + vFCP).ToString();
                                            }
                                        }

                                        pos = -1;
                                        status = false;
                                        CFOP = notes[i][j]["CFOP"];
                                        cProd = notes[i][j]["cProd"];
                                        xProd = notes[i][j]["xProd"];
                                        vBC = 0;
                                        vICMS = 0;
                                        vFCP = 0;
                                        pICMS = "0";
                                        pFCP = "0";

                                        if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                        {
                                            vProd = 0;
                                            vProd += Convert.ToDecimal(notes[i][j]["vProd"]);
                                            status = true;
                                        }

                                        if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                            vProd += Convert.ToDecimal(notes[i][j]["vFrete"]);

                                        if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                            vProd -= Convert.ToDecimal(notes[i][j]["vDesc"]);

                                        if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                            vProd += Convert.ToDecimal(notes[i][j]["vOutro"]);

                                        if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                            vProd += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                    }


                                    if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                    {
                                        vBC = Convert.ToDecimal(notes[i][j]["vBC"]);
                                        status = true;
                                    }


                                    if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        pICMS = notes[i][j]["pICMS"];
                                        vICMS = Convert.ToDecimal(notes[i][j]["vICMS"]);
                                        status = true;
                                    }


                                    if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        pFCP = notes[i][j]["pFCP"];
                                        vFCP = Convert.ToDecimal(notes[i][j]["vFCP"]);
                                        status = true;
                                    }

                                    if (notes[i][j].ContainsKey("vNF"))
                                    {
                                        for (int e = 0; e < products.Count(); e++)
                                        {
                                            if (products[e][0].Equals(CFOP) && products[e][2].Equals(cProd) && Convert.ToDecimal(products[e][6]).Equals(Convert.ToDecimal(pICMS)) &&
                                                Convert.ToDecimal(products[e][8]).Equals(Convert.ToDecimal(pFCP)))
                                            {
                                                pos = e;
                                                break;
                                            }
                                        }

                                        if (pos < 0)
                                        {
                                            var cfp = cfops.Where(_ => _.Code.Equals(CFOP)).FirstOrDefault();
                                            List<string> cc = new List<string>();
                                            cc.Add(CFOP);
                                            cc.Add(cfp.Description);
                                            cc.Add(cProd);
                                            cc.Add(xProd);
                                            cc.Add(vProd.ToString());
                                            cc.Add(vBC.ToString());
                                            cc.Add(pICMS);
                                            cc.Add(vICMS.ToString());
                                            cc.Add(pFCP);
                                            cc.Add(vFCP.ToString());
                                            products.Add(cc);
                                        }
                                        else
                                        {
                                            products[pos][4] = (Convert.ToDecimal(products[pos][4]) + vProd).ToString();
                                            products[pos][5] = (Convert.ToDecimal(products[pos][5]) + vBC).ToString();
                                            products[pos][7] = (Convert.ToDecimal(products[pos][7]) + vICMS).ToString();
                                            products[pos][9] = (Convert.ToDecimal(products[pos][9]) + vFCP).ToString();
                                        }
                                    }

                                }

                            }
                        }
                        else
                        {
                            if (aliquotFecop.Equals(""))
                            {
                                for (int i = notes.Count - 1; i >= 0; i--)
                                {

                                    if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                                    {
                                        notes.RemoveAt(i);
                                        continue;
                                    }


                                    string CFOP = "", cProd = "", xProd = "", pICMS = "0", pFCP = "0";
                                    decimal vProd = 0, vBC = 0, vICMS = 0, vFCP = 0;
                                    int pos = -1;
                                    bool status = false;

                                    for (int j = 0; j < notes[i].Count(); j++)
                                    {
                                        if ((notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM")))
                                        {
                                            if (status && Convert.ToDecimal(pICMS).Equals(Convert.ToDecimal(aliquotIcms)) && Convert.ToDecimal(pFCP).Equals(0))
                                            {
                                                for (int e = 0; e < products.Count(); e++)
                                                {
                                                    if (products[e][0].Equals(CFOP) && products[e][2].Equals(cProd) && Convert.ToDecimal(products[e][6]).Equals(Convert.ToDecimal(pICMS)) &&
                                                        Convert.ToDecimal(products[e][8]).Equals(Convert.ToDecimal(pFCP)))
                                                    {
                                                        pos = e;
                                                        break;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    var cfp = cfops.Where(_ => _.Code.Equals(CFOP)).FirstOrDefault();
                                                    List<string> cc = new List<string>();
                                                    cc.Add(CFOP);
                                                    cc.Add(cfp.Description);
                                                    cc.Add(cProd);
                                                    cc.Add(xProd);
                                                    cc.Add(vProd.ToString());
                                                    cc.Add(vBC.ToString());
                                                    cc.Add(pICMS);
                                                    cc.Add(vICMS.ToString());
                                                    cc.Add(pFCP);
                                                    cc.Add(vFCP.ToString());
                                                    products.Add(cc);
                                                }
                                                else
                                                {
                                                    products[pos][4] = (Convert.ToDecimal(products[pos][4]) + vProd).ToString();
                                                    products[pos][5] = (Convert.ToDecimal(products[pos][5]) + vBC).ToString();
                                                    products[pos][7] = (Convert.ToDecimal(products[pos][7]) + vICMS).ToString();
                                                    products[pos][9] = (Convert.ToDecimal(products[pos][9]) + vFCP).ToString();
                                                }
                                            }

                                            pos = -1;
                                            status = false;
                                            CFOP = notes[i][j]["CFOP"];
                                            cProd = notes[i][j]["cProd"];
                                            xProd = notes[i][j]["xProd"];
                                            vBC = 0;
                                            vICMS = 0;
                                            vFCP = 0;
                                            pICMS = "0";
                                            pFCP = "0";

                                            if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                            {
                                                vProd = 0;
                                                vProd += Convert.ToDecimal(notes[i][j]["vProd"]);
                                                status = true;
                                            }

                                            if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                                vProd += Convert.ToDecimal(notes[i][j]["vFrete"]);

                                            if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                                vProd -= Convert.ToDecimal(notes[i][j]["vDesc"]);

                                            if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                                vProd += Convert.ToDecimal(notes[i][j]["vOutro"]);

                                            if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                                vProd += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                        }


                                        if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                        {
                                            vBC = Convert.ToDecimal(notes[i][j]["vBC"]);
                                            status = true;
                                        }


                                        if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            pICMS = notes[i][j]["pICMS"];
                                            vICMS = Convert.ToDecimal(notes[i][j]["vICMS"]);
                                            status = true;
                                        }


                                        if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            pFCP = notes[i][j]["pFCP"];
                                            vFCP = Convert.ToDecimal(notes[i][j]["vFCP"]);
                                            status = true;
                                        }

                                        if (notes[i][j].ContainsKey("vNF"))
                                        {
                                            if (Convert.ToDecimal(pICMS).Equals(Convert.ToDecimal(aliquotIcms)) && Convert.ToDecimal(pFCP).Equals(0))
                                            {
                                                for (int e = 0; e < products.Count(); e++)
                                                {
                                                    if (products[e][0].Equals(CFOP) && products[e][2].Equals(cProd) && Convert.ToDecimal(products[e][6]).Equals(Convert.ToDecimal(pICMS)) &&
                                                        Convert.ToDecimal(products[e][8]).Equals(Convert.ToDecimal(pFCP)))
                                                    {
                                                        pos = e;
                                                        break;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    var cfp = cfops.Where(_ => _.Code.Equals(CFOP)).FirstOrDefault();
                                                    List<string> cc = new List<string>();
                                                    cc.Add(CFOP);
                                                    cc.Add(cfp.Description);
                                                    cc.Add(cProd);
                                                    cc.Add(xProd);
                                                    cc.Add(vProd.ToString());
                                                    cc.Add(vBC.ToString());
                                                    cc.Add(pICMS);
                                                    cc.Add(vICMS.ToString());
                                                    cc.Add(pFCP);
                                                    cc.Add(vFCP.ToString());
                                                    products.Add(cc);
                                                }
                                                else
                                                {
                                                    products[pos][4] = (Convert.ToDecimal(products[pos][4]) + vProd).ToString();
                                                    products[pos][5] = (Convert.ToDecimal(products[pos][5]) + vBC).ToString();
                                                    products[pos][7] = (Convert.ToDecimal(products[pos][7]) + vICMS).ToString();
                                                    products[pos][9] = (Convert.ToDecimal(products[pos][9]) + vFCP).ToString();
                                                }
                                            }

                                        }
                                    }

                                }
                            }
                            else
                            {
                                for (int i = notes.Count - 1; i >= 0; i--)
                                {
                                    if (!notes[i][2]["CNPJ"].Equals(comp.Document))
                                    {
                                        notes.RemoveAt(i);
                                        continue;
                                    }


                                    string CFOP = "", cProd = "", xProd = "", pICMS = "0", pFCP = "0";
                                    decimal vProd = 0, vBC = 0, vICMS = 0, vFCP = 0;
                                    int pos = -1;
                                    bool status = false;

                                    for (int j = 0; j < notes[i].Count(); j++)
                                    {
                                        if ((notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("NCM")))
                                        {
                                            if (status && Convert.ToDecimal(pICMS).Equals(Convert.ToDecimal(aliquotIcms)) && Convert.ToDecimal(pFCP).Equals(Convert.ToDecimal(aliquotFecop)))
                                            {
                                                for (int e = 0; e < products.Count(); e++)
                                                {
                                                    if (products[e][0].Equals(CFOP) && products[e][2].Equals(cProd) && Convert.ToDecimal(products[e][6]).Equals(Convert.ToDecimal(pICMS)) &&
                                                        Convert.ToDecimal(products[e][8]).Equals(Convert.ToDecimal(pFCP)))
                                                    {
                                                        pos = e;
                                                        break;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    var cfp = cfops.Where(_ => _.Code.Equals(CFOP)).FirstOrDefault();
                                                    List<string> cc = new List<string>();
                                                    cc.Add(CFOP);
                                                    cc.Add(cfp.Description);
                                                    cc.Add(cProd);
                                                    cc.Add(xProd);
                                                    cc.Add(vProd.ToString());
                                                    cc.Add(vBC.ToString());
                                                    cc.Add(pICMS);
                                                    cc.Add(vICMS.ToString());
                                                    cc.Add(pFCP);
                                                    cc.Add(vFCP.ToString());
                                                    products.Add(cc);
                                                }
                                                else
                                                {
                                                    products[pos][4] = (Convert.ToDecimal(products[pos][4]) + vProd).ToString();
                                                    products[pos][5] = (Convert.ToDecimal(products[pos][5]) + vBC).ToString();
                                                    products[pos][7] = (Convert.ToDecimal(products[pos][7]) + vICMS).ToString();
                                                    products[pos][9] = (Convert.ToDecimal(products[pos][9]) + vFCP).ToString();
                                                }
                                            }

                                            pos = -1;
                                            status = false;
                                            CFOP = notes[i][j]["CFOP"];
                                            cProd = notes[i][j]["cProd"];
                                            xProd = notes[i][j]["xProd"];
                                            vBC = 0;
                                            vICMS = 0;
                                            vFCP = 0;
                                            pICMS = "0";
                                            pFCP = "0";

                                            if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd"))
                                            {
                                                vProd = 0;
                                                vProd += Convert.ToDecimal(notes[i][j]["vProd"]);
                                                status = true;
                                            }

                                            if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd"))
                                                vProd += Convert.ToDecimal(notes[i][j]["vFrete"]);

                                            if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd"))
                                                vProd -= Convert.ToDecimal(notes[i][j]["vDesc"]);

                                            if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd"))
                                                vProd += Convert.ToDecimal(notes[i][j]["vOutro"]);

                                            if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd"))
                                                vProd += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                        }


                                        if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                        {
                                            vBC = Convert.ToDecimal(notes[i][j]["vBC"]);
                                            status = true;
                                        }


                                        if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            pICMS = notes[i][j]["pICMS"];
                                            vICMS = Convert.ToDecimal(notes[i][j]["vICMS"]);
                                            status = true;
                                        }


                                        if (notes[i][j].ContainsKey("pFCP") && notes[i][j].ContainsKey("CST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            pFCP = notes[i][j]["pFCP"];
                                            vFCP = Convert.ToDecimal(notes[i][j]["vFCP"]);
                                            status = true;
                                        }

                                        if (notes[i][j].ContainsKey("vNF"))
                                        {
                                            if (Convert.ToDecimal(pICMS).Equals(Convert.ToDecimal(aliquotIcms)) && Convert.ToDecimal(pFCP).Equals(Convert.ToDecimal(aliquotFecop)))
                                            {
                                                for (int e = 0; e < products.Count(); e++)
                                                {
                                                    if (products[e][0].Equals(CFOP) && products[e][2].Equals(cProd) && Convert.ToDecimal(products[e][6]).Equals(Convert.ToDecimal(pICMS)) &&
                                                        Convert.ToDecimal(products[e][8]).Equals(Convert.ToDecimal(pFCP)))
                                                    {
                                                        pos = e;
                                                        break;
                                                    }
                                                }

                                                if (pos < 0)
                                                {
                                                    var cfp = cfops.Where(_ => _.Code.Equals(CFOP)).FirstOrDefault();
                                                    List<string> cc = new List<string>();
                                                    cc.Add(CFOP);
                                                    cc.Add(cfp.Description);
                                                    cc.Add(cProd);
                                                    cc.Add(xProd);
                                                    cc.Add(vProd.ToString());
                                                    cc.Add(vBC.ToString());
                                                    cc.Add(pICMS);
                                                    cc.Add(vICMS.ToString());
                                                    cc.Add(pFCP);
                                                    cc.Add(vFCP.ToString());
                                                    products.Add(cc);
                                                }
                                                else
                                                {
                                                    products[pos][4] = (Convert.ToDecimal(products[pos][4]) + vProd).ToString();
                                                    products[pos][5] = (Convert.ToDecimal(products[pos][5]) + vBC).ToString();
                                                    products[pos][7] = (Convert.ToDecimal(products[pos][7]) + vICMS).ToString();
                                                    products[pos][9] = (Convert.ToDecimal(products[pos][9]) + vFCP).ToString();
                                                }
                                            }

                                        }
                                    }

                                }
                            }
                        }
                    }

                }

                ViewBag.Company = comp;
                ViewBag.AliquotIcms = aliquotIcms;
                ViewBag.AliquotFecop = aliquotFecop;
                ViewBag.Opcao = opcao;
                ViewBag.Archive = archive;
                ViewBag.Products = products.OrderBy(_ => Convert.ToInt32(_[0])).ToList();

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
