using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{
    public class CompareController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;
        private readonly IHostingEnvironment _appEnvironment;

        public CompareController(
           ICompanyService companyService,
           IConfigurationService configurationService,
           IHostingEnvironment env)
        {
            _companyService = companyService;
            _configurationService = configurationService;
            _appEnvironment = env;
        }

        public async Task<IActionResult> Index(long id, string year, string month, Model.Opcao opcao, Model.Ordem ordem, IFormFile arquivoSped, IFormFile arquivoExcel)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var ident = Request.Form["ident"];

                var company = _companyService.FindById(id, null);

                SessionManager.SetCompanyInSession(company);
                SessionManager.SetYearInSession(year);
                SessionManager.SetMonthInSession(month);

                var importPeriod = new Period.Month();

                ViewBag.Opcao = opcao.ToString();
                ViewBag.Ordem = ordem.ToString();
                ViewBag.Company = company;
                ViewBag.NumberMonth = importPeriod.NumberMonth(month);

                SessionManager.SetMonthInSession(month);
                SessionManager.SetYearInSession(year);

                var importXml = new Xml.Import();
                var importSped = new Sped.Import();
                var importExcel = new Planilha.Import();
                var importEvento = new Evento.Import();
                var confDBSisctNfe = new Model.Configuration();
                var importDir = new Diretorio.Import();

                string caminhoDestinoArquivoOriginalSped = "";
                string caminho_WebRoot = _appEnvironment.WebRootPath;

                if (arquivoSped != null)
                {
                    string filedirSped = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Speds");

                    if (!Directory.Exists(filedirSped))
                        Directory.CreateDirectory(filedirSped);

                    string nomeArquivoSped = company.Document + "Empresa";

                    if (arquivoSped.FileName.Contains(".txt"))
                        nomeArquivoSped += ".txt";
                    else
                        nomeArquivoSped += ".tmp";

                    string caminhoDestinoArquivoSped = caminho_WebRoot + "\\Uploads\\Speds\\";
                    caminhoDestinoArquivoOriginalSped = caminhoDestinoArquivoSped + nomeArquivoSped;

                    string[] paths_upload_sped = Directory.GetFiles(caminhoDestinoArquivoSped);

                    if (System.IO.File.Exists(caminhoDestinoArquivoOriginalSped))
                        System.IO.File.Delete(caminhoDestinoArquivoOriginalSped);

                    var streamSped = new FileStream(caminhoDestinoArquivoOriginalSped, FileMode.Create);
                    await arquivoSped.CopyToAsync(streamSped);
                    streamSped.Close();
                }

                if (opcao.Equals(Model.Opcao.NFe))
                {
                    List<List<Dictionary<string, string>>> notesValidas = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesNFeCanceladas = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesNFeCanceladasEvento = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesNFCeCanceladas = new List<List<Dictionary<string, string>>>();
                    List<List<Dictionary<string, string>>> notesNFCeCanceladasEvento = new List<List<Dictionary<string, string>>>();

                    List<List<string>> spedNormal = new List<List<string>>();
                    List<List<string>> spedNFeCancelada = new List<List<string>>();
                    List<List<string>> spedNFCeCancelada = new List<List<string>>();

                    string directoryValida = "", directoryNFeCancelada = "", directoryNFCeCancelada = "";

                    if (ident.Equals("0"))
                    {
                        confDBSisctNfe = _configurationService.FindByName("NFe");
                        directoryValida = importDir.Entrada(company, confDBSisctNfe.Value, year, month);
                    }
                    else if (ident.Equals("1"))
                    {
                        confDBSisctNfe = _configurationService.FindByName("NFe Saida");

                        if (ordem.Equals(Model.Ordem.XmlEmpresa) || ordem.Equals(Model.Ordem.SisCTXE) || ordem.Equals(Model.Ordem.SpedXE) || 
                            ordem.Equals(Model.Ordem.EmpresaXFsist) || ordem.Equals(Model.Ordem.DifereValorEmpresa))
                        {
                            directoryValida = importDir.SaidaEmpresa(company, confDBSisctNfe.Value, year, month);
                            directoryNFeCancelada = importDir.NFeCanceladaEmpresa(company, confDBSisctNfe.Value, year, month);
                            directoryNFCeCancelada = importDir.NFCeCanceladaEmpresa(company, confDBSisctNfe.Value, year, month);
                        }
                        else
                        {
                            directoryValida = importDir.SaidaSefaz(company, confDBSisctNfe.Value, year, month);
                            directoryNFeCancelada = importDir.NFeCanceladaSefaz(company, confDBSisctNfe.Value, year, month);
                            directoryNFCeCancelada = importDir.NFCeCanceladaSefaz(company, confDBSisctNfe.Value, year, month);
                        }
                       
                    }
                   

                    if (ordem.Equals(Model.Ordem.DifereValor) || ordem.Equals(Model.Ordem.SisCTXS) || ordem.Equals(Model.Ordem.SisCTXE) || 
                        ordem.Equals(Model.Ordem.DifereIcms) || ordem.Equals(Model.Ordem.DifereValorSefaz) || ordem.Equals(Model.Ordem.DifereValorEmpresa))
                    {
                        spedNormal = importSped.NFeDif(caminhoDestinoArquivoOriginalSped, ident);
                        notesValidas = importXml.NFeResumeEmit(directoryValida);
                    }
                    else if(ordem.Equals(Model.Ordem.XmlSefaz) || ordem.Equals(Model.Ordem.XmlEmpresa) || ordem.Equals(Model.Ordem.SpedXS) || ordem.Equals(Model.Ordem.SpedXE))
                    { 
                        if (ident.Equals("0"))
                        {
                            spedNormal = importSped.NFeAll(caminhoDestinoArquivoOriginalSped);
                        }
                        else if (ident.Equals("1"))
                        {
                            spedNormal = importSped.NFeExitNormal(caminhoDestinoArquivoOriginalSped);
                            spedNFeCancelada = importSped.NFeExitCanceled(caminhoDestinoArquivoOriginalSped,"55");
                            spedNFCeCancelada = importSped.NFeExitCanceled(caminhoDestinoArquivoOriginalSped, "65");
                        }

                        notesValidas = importXml.NFeResumeEmit(directoryValida);
                        notesNFeCanceladas = importXml.NFeResumeEmit(directoryNFeCancelada);
                        notesNFeCanceladasEvento = importEvento.NFeCancelada(directoryNFeCancelada);
                        notesNFCeCanceladas = importXml.NFeResumeEmit(directoryNFCeCancelada);
                        notesNFCeCanceladasEvento = importEvento.NFeCancelada(directoryNFCeCancelada);
                    }

                    List<List<Dictionary<string, string>>> notasValidas = new List<List<Dictionary<string, string>>>();
                    List<List<string>> notasInvalidas = new List<List<string>>();
                    List<List<string>> notas_sped = new List<List<string>>();

                    List<string> keys = new List<string>();

                    List<List<List<string>>> notasCanceladas = new List<List<List<string>>>();
                    List<List<List<string>>> eventos = new List<List<List<string>>>();

                    int qtdValida = 0;
                    int qtdInvalida = 0;

                    if (ordem.Equals(Model.Ordem.XmlSefaz) || ordem.Equals(Model.Ordem.XmlEmpresa))
                    {
                        qtdValida += notesValidas.Count();
                        qtdInvalida += (notesNFeCanceladas.Count() + notesNFeCanceladasEvento.Count() + notesNFCeCanceladas.Count() + notesNFCeCanceladasEvento.Count());

                        //  NFe e NFCe
                        foreach (var note in notesValidas)
                        {
                            string nota_xml = note[0]["chave"];
                            bool nota_encontrada = false;
                            foreach (var nota_sped in spedNormal)
                            {
                                if (nota_xml.Equals(nota_sped[0]))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }

                            foreach (var nota_sped in spedNFeCancelada)
                            {
                                if (nota_xml.Equals(nota_sped[0]))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }

                            foreach (var nota_sped in spedNFCeCancelada)
                            {
                                if (nota_xml.Equals(nota_sped[0]))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }

                            if (nota_encontrada.Equals(false))
                            {
                                notasValidas.Add(note);
                                keys.Add(nota_xml);
                            }
                        }

                        //  NFe
                        foreach (var note in notesNFeCanceladas)
                        {
                            string nota_xml = note[0]["chave"];
                            bool nota_encontrada = false;
                            foreach (var nota_sped in spedNFeCancelada)
                            {
                                if (nota_xml.Equals(nota_sped[0]))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }

                            if (nota_encontrada.Equals(false))
                            {
                                List<string> n = new List<string>();
                                n.Add(note[1]["mod"]);
                                n.Add(note[1]["nNF"]);
                                n.Add(note[0]["chave"]);
                                notasInvalidas.Add(n);
                            }
                        }

                        foreach (var note in notesNFeCanceladasEvento)
                        {
                            string nota_xml = note[0]["chNFe"];
                            bool nota_encontrada = false;
                            foreach (var nota_sped in spedNFeCancelada)
                            {
                                if (nota_xml.Equals(nota_sped[0]))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }

                            if (nota_encontrada.Equals(false))
                            {
                                List<string> n = new List<string>();
                                n.Add(note[0]["chNFe"].Substring(20, 2));
                                n.Add(note[0]["chNFe"].Substring(25, 9));
                                n.Add(note[0]["chNFe"]);
                                notasInvalidas.Add(n);
                            }
                        }

                        //  NFCe
                        foreach (var note in notesNFCeCanceladas)
                        {
                            string nota_xml = note[0]["chave"];
                            bool nota_encontrada = false;
                            foreach (var nota_sped in spedNFCeCancelada)
                            {
                                if (nota_xml.Equals(nota_sped[0]))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }

                            if (nota_encontrada.Equals(false))
                            {
                                List<string> n = new List<string>();
                                n.Add(note[1]["mod"]);
                                n.Add(note[1]["nNF"]);
                                n.Add(note[0]["chave"]);
                                notasInvalidas.Add(n);
                            }
                        }

                        foreach (var note in notesNFCeCanceladasEvento)
                        {
                            string nota_xml = note[0]["chNFe"];
                            bool nota_encontrada = false;
                            foreach (var nota_sped in spedNFCeCancelada)
                            {
                                if (nota_xml.Equals(nota_sped[0]))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }

                            if (nota_encontrada.Equals(false))
                            {
                                List<string> n = new List<string>();
                                n.Add(note[0]["chNFe"].Substring(20, 2));
                                n.Add(note[0]["chNFe"].Substring(25, 9));
                                n.Add(note[0]["chNFe"]);
                                notasInvalidas.Add(n);
                            }
                        }

                    }
                    else if (ordem.Equals(Model.Ordem.SpedXS) || ordem.Equals(Model.Ordem.SpedXE))
                    {
                        if (ident.Equals("0"))
                            spedNormal = importSped.NFeTypeEmission(caminhoDestinoArquivoOriginalSped, ident, "1");

                        qtdValida += spedNormal.Count();
                        qtdInvalida += (spedNFeCancelada.Count() + spedNFCeCancelada.Count());

                        //  NFe e NFCe
                        foreach (var note in spedNormal)
                        {
                            bool nota_encontrada = false;

                            foreach (var notaXml in notesValidas)
                            {
                                string nota_xml = notaXml[0]["chave"];
                                if (note[0].Equals(nota_xml))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }

                            if (nota_encontrada.Equals(false))
                                notas_sped.Add(note);
                        }

                        //  NFe
                        foreach (var note in spedNFeCancelada)
                        {
                            bool nota_encontrada = false;

                            foreach (var notaXml in notesNFeCanceladas)
                            {
                                string nota_xml = notaXml[0]["chave"];
                                if (note[0].Equals(nota_xml))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }

                            if (nota_encontrada.Equals(false))
                            {
                                List<string> n = new List<string>();
                                n.Add(note[1]);
                                n.Add(note[2]);
                                n.Add(note[0]);
                                notasInvalidas.Add(n);
                            }
                        }

                        foreach (var note in spedNFeCancelada)
                        {
                            bool nota_encontrada = false;

                            foreach (var notaXml in notesNFeCanceladasEvento)
                            {
                                string nota_xml = notaXml[0]["chNFe"];
                                if (note[0].Equals(nota_xml))
                                {
                                    nota_encontrada = true;
                                    for (int i = notasInvalidas.Count() - 1; i >= 0; i--)
                                    {
                                        if (notasInvalidas[i][2].Equals(nota_xml))
                                        {
                                            notasInvalidas.RemoveAt(i);
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }

                            if (nota_encontrada.Equals(false))
                            {
                                for (int i = notasInvalidas.Count() - 1; i >= 0; i--)
                                {
                                    if (notasInvalidas[i][2].Equals(note[0]))
                                    {
                                        notasInvalidas.RemoveAt(i);
                                        break;
                                    }
                                }

                                List<string> n = new List<string>();
                                n.Add(note[1]);
                                n.Add(note[2]);
                                n.Add(note[0]);
                                notasInvalidas.Add(n);

                                for (int i = notasInvalidas.Count() - 1; i >= 0; i--)
                                {
                                    bool achou = false;
                                    foreach (var notaXml in notesNFeCanceladas)
                                    {
                                        string nota_xml = notaXml[0]["chave"];
                                        if (notasInvalidas[i][2].Equals(nota_xml))
                                        {
                                            achou = true;
                                            break;
                                        }
                                    }

                                    if (achou == true)
                                    {
                                        notasInvalidas.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                        }

                        //  NFCe
                        foreach (var note in spedNFCeCancelada)
                        {
                            bool nota_encontrada = false;

                            foreach (var notaXml in notesNFCeCanceladas)
                            {
                                string nota_xml = notaXml[0]["chave"];
                                if (note[0].Equals(nota_xml))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }

                            if (nota_encontrada.Equals(false))
                            {
                                List<string> n = new List<string>();
                                n.Add(note[1]);
                                n.Add(note[2]);
                                n.Add(note[0]);
                                notasInvalidas.Add(n);
                            }
                        }

                        foreach (var note in spedNFCeCancelada)
                        {
                            bool nota_encontrada = false;

                            foreach (var notaXml in notesNFCeCanceladasEvento)
                            {
                                string nota_xml = notaXml[0]["chNFe"];
                                if (note[0].Equals(nota_xml))
                                {
                                    nota_encontrada = true;
                                    for (int i = notasInvalidas.Count() - 1; i >= 0; i--)
                                    {
                                        if (notasInvalidas[i][2].Equals(nota_xml))
                                        {
                                            notasInvalidas.RemoveAt(i);
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }

                            if (nota_encontrada.Equals(false))
                            {
                                for (int i = notasInvalidas.Count() - 1; i >= 0; i--)
                                {
                                    if (notasInvalidas[i][2].Equals(note[0]))
                                    {
                                        notasInvalidas.RemoveAt(i);
                                        break;
                                    }
                                }

                                List<string> n = new List<string>();
                                n.Add(note[1]);
                                n.Add(note[2]);
                                n.Add(note[0]);
                                notasInvalidas.Add(n);

                                for (int i = notasInvalidas.Count() - 1; i >= 0; i--)
                                {
                                    bool achou = false;
                                    foreach (var notaXml in notesNFCeCanceladas)
                                    {
                                        string nota_xml = notaXml[0]["chave"];
                                        if (notasInvalidas[i][2].Equals(nota_xml))
                                        {
                                            achou = true;
                                            break;
                                        }
                                    }

                                    if (achou == true)
                                    {
                                        notasInvalidas.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (ordem.Equals(Model.Ordem.DifereValor) || ordem.Equals(Model.Ordem.DifereValorSefaz) || ordem.Equals(Model.Ordem.DifereValorEmpresa))
                    {
                        List<List<string>> registros = new List<List<string>>();
                        
                        foreach (var linha in spedNormal)
                        {
                            List<string> Valores = new List<string>();
                            foreach (var notaXml in notesValidas)
                            {
                                string nota_xml = notaXml[0]["chave"];

                                if (linha[1].Equals(nota_xml))
                                {

                                    string fornecedor = notaXml[2]["xNome"];
                                    string totalXml = notaXml[3]["vNF"];
                                    string totalSped = linha[2].Equals("") ? "0" : linha[2].Replace('.', '*').Replace(',', '.').Replace('*', ',');
                                    string totalDif = (Convert.ToDecimal(totalXml) - Convert.ToDecimal(totalSped)).ToString();
                                    string descXml = notaXml[3]["vDesc"];
                                    string descSped = linha[3].Equals("") ? "0" : linha[3].Replace('.', '*').Replace(',', '.').Replace('*', ',');
                                    string descDif = (Convert.ToDecimal(descXml) - Convert.ToDecimal(descSped)).ToString();
                                    string outDespXml = notaXml[3]["vOutro"];
                                    string outDespSped = linha[6].Equals("") ? "0" : linha[6].Replace('.', '*').Replace(',', '.').Replace('*', ',');
                                    string outDespDif = (Convert.ToDecimal(outDespXml) - Convert.ToDecimal(outDespSped)).ToString();
                                    string segXml = notaXml[3]["vSeg"];
                                    string segSped = linha[5].Equals("") ? "0" : linha[5].Replace('.', '*').Replace(',', '.').Replace('*', ',');
                                    string segDif = (Convert.ToDecimal(segXml) - Convert.ToDecimal(segSped)).ToString();
                                    string freteXml = notaXml[3]["vFrete"];
                                    string freteSped = linha[4].Equals("") ? "0" : linha[4].Replace('.', '*').Replace(',', '.').Replace('*', ',');
                                    string freteDif = (Convert.ToDecimal(freteXml) - Convert.ToDecimal(freteSped)).ToString();

                                    string totalSped2 = linha[9].Equals("") ? "0" : (Convert.ToDecimal(linha[9].Replace('.', '*').Replace(',', '.').Replace('*', ',')) - 
                                        Convert.ToDecimal(descSped) + Convert.ToDecimal(outDespSped) + Convert.ToDecimal(segSped) + Convert.ToDecimal(freteSped)).ToString();
                                    string totalDif2 = (Convert.ToDecimal(totalXml) - Convert.ToDecimal(totalSped2)).ToString();


                                    if (!Convert.ToDecimal(totalDif).Equals(0) || !Convert.ToDecimal(totalDif2).Equals(0) || !Convert.ToDecimal(descDif).Equals(0) ||
                                        !Convert.ToDecimal(outDespDif).Equals(0) || !Convert.ToDecimal(segDif).Equals(0) ||
                                        !Convert.ToDecimal(freteDif).Equals(0))
                                    {
                                        Valores.Add(linha[0]);
                                        Valores.Add(fornecedor);
                                        Valores.Add(totalXml);
                                        if (!Convert.ToDecimal(totalDif).Equals(0))
                                        {
                                            Valores.Add(totalSped);
                                            Valores.Add(totalDif);
                                        }
                                        else
                                        {
                                            Valores.Add(totalSped2);
                                            Valores.Add(totalDif2);
                                        }
                                        Valores.Add(descXml);
                                        Valores.Add(descSped);
                                        Valores.Add(descDif);
                                        Valores.Add(outDespXml);
                                        Valores.Add(outDespSped);
                                        Valores.Add(outDespDif);
                                        Valores.Add(freteXml);
                                        Valores.Add(freteSped);
                                        Valores.Add(freteDif);
                                        Valores.Add(segXml);
                                        Valores.Add(segSped);
                                        Valores.Add(segDif);
                                        registros.Add(Valores);
                                    }
                                }
                            }
                        }
                       
                        ViewBag.Valores = registros;
                    }
                    else if (ordem.Equals(Model.Ordem.SisCTXS) || ordem.Equals(Model.Ordem.SisCTXE))
                    {
                        List<List<string>> registros = new List<List<string>>();
                        decimal valorTotaGeralXml = 0, valorTotalGeralSped = 0;
                        foreach (var linha in spedNormal)
                        {
                            List<string> Valores = new List<string>();
                            foreach (var notaXml in notesValidas)
                            {
                                string nota_xml = notaXml[0]["chave"];
                                if (linha[1].Equals(nota_xml))
                                {
                                    string fornecedor = notaXml[2]["xNome"];
                                    string totalXml = notaXml[3]["vNF"];
                                    string totalSped = linha[2].Equals("") ? "0" : linha[2].Replace('.', '*').Replace(',', '.').Replace('*', ',');
                                    string totalDif = (Convert.ToDecimal(totalXml) - Convert.ToDecimal(totalSped)).ToString();

                                    Valores.Add(linha[0]);
                                    Valores.Add(fornecedor);
                                    Valores.Add(totalXml);
                                    Valores.Add(totalSped);
                                    Valores.Add(totalDif);
                                    registros.Add(Valores);
                                    valorTotaGeralXml += Convert.ToDecimal(totalXml);
                                    valorTotalGeralSped += Convert.ToDecimal(totalSped);
                                }
                            }

                        }
                        ViewBag.Valores = registros;
                        ViewBag.TotalXml = valorTotaGeralXml;
                        ViewBag.TotalSped = valorTotalGeralSped;
                        ViewBag.TotalDif = valorTotaGeralXml - valorTotalGeralSped;
                    }
                    else if (ordem.Equals(Model.Ordem.DifereIcms))
                    {
                        List<List<string>> registros = new List<List<string>>();
                        foreach (var linha in spedNormal)
                        {
                            List<string> Valores = new List<string>();
                            foreach (var notaXml in notesValidas)
                            {
                                string nota_xml = notaXml[0]["chave"];

                                if (linha[1].Equals(nota_xml))
                                {
                                    string fornecedor = notaXml[2]["xNome"];
                                    string totalNota = notaXml[3]["vNF"];

                                    string icmsXml = notaXml[3]["vICMS"];
                                    string icmsSped = linha[10].Equals("") ? "0" : linha[10].Replace('.', '*').Replace(',', '.').Replace('*', ',');
                                    string icmsDif = (Convert.ToDecimal(icmsXml) - Convert.ToDecimal(icmsSped)).ToString();
  

                                    if (!Convert.ToDecimal(icmsDif).Equals(0))
                                    {
                                        Valores.Add(linha[0]);
                                        Valores.Add(fornecedor);
                                        Valores.Add(totalNota);
                                        Valores.Add(icmsXml);
                                        Valores.Add(icmsSped);
                                        Valores.Add(icmsDif);
                                        registros.Add(Valores);
                                    }
                                }
                            }
                        }
                        ViewBag.Valores = registros;
                    }
                    else if (ordem.Equals(Model.Ordem.SefazXFsist) || ordem.Equals(Model.Ordem.EmpresaXFsist))
                    {
                        if (arquivoExcel == null || arquivoExcel.Length == 0)
                        {
                            ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                            return View(ViewData);
                        }

                        string filedirExcel = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Planilha");

                        if (!Directory.Exists(filedirExcel))
                            Directory.CreateDirectory(filedirExcel);

                        string nomeArquivoExcel = company.Document + "Sefaz";

                        if (arquivoExcel.FileName.Contains(".xls") || arquivoExcel.FileName.Contains(".xlsx"))
                            nomeArquivoExcel += ".xls";

                        string caminhoDestinoArquivoExcel = caminho_WebRoot + "\\Uploads\\Planilha\\";
                        string caminhoDestinoArquivoOriginalExcel = caminhoDestinoArquivoExcel + nomeArquivoExcel;

                        string[] paths_upload_excel = Directory.GetFiles(caminhoDestinoArquivoExcel);

                        if (System.IO.File.Exists(caminhoDestinoArquivoOriginalExcel))
                            System.IO.File.Delete(caminhoDestinoArquivoOriginalExcel);

                        var streamExcel = new FileStream(caminhoDestinoArquivoOriginalExcel, FileMode.Create);
                        await arquivoExcel.CopyToAsync(streamExcel);
                        streamExcel.Close();

                        notesValidas = importXml.NFeResumeEmit(directoryValida);
                        var notesPlanilha = importExcel.NotesFsist(caminhoDestinoArquivoOriginalExcel);

                        qtdValida = notesValidas.Count();
                        //  NFe e NFCe
                        foreach (var note in notesValidas)
                        {
                            string nota_xml = note[0]["chave"];
                            bool nota_encontrada = false;
                            foreach (var nota_fsist in notesPlanilha)
                            {
                                if (nota_xml.Equals(nota_fsist[3]))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }

                            if (nota_encontrada.Equals(false))
                                notasValidas.Add(note);
                        }

                    }

                    ViewBag.Notas = notasValidas.OrderBy(_ => _[1]["mod"]).ThenBy(_ => _[1]["nNF"]).ToList();
                    ViewBag.NotasInvalidas = notasInvalidas.OrderBy(_ => _[0]).ThenBy(_ => _[1]).ToList();
                    ViewBag.notas_sped = notas_sped.OrderBy(_ => _[1]).ThenBy(_ => _[2]).ToList();
                    ViewBag.NotasCanceladas = notasCanceladas;
                    ViewBag.Eventos = eventos;
                    ViewBag.QtdValida = qtdValida;
                    ViewBag.QtdInvalida = qtdInvalida;

                    SessionManager.SetKeysInSession(keys);

                }
                else if (opcao.Equals(Model.Opcao.CTe))
                {
                    string directoryCte = "";

                    List<List<string>> sped = new List<List<string>>();

                    if (ident.Equals("0"))
                    {
                        var confDBSisctCte = _configurationService.FindByName("CTe");
                        directoryCte = importDir.Entrada(company, confDBSisctCte.Value, year, month);

                        sped = importSped.CTeType(caminhoDestinoArquivoOriginalSped, "0");
                    }
                    else
                    {
                        var confDBSisctCte = _configurationService.FindByName("CTe Saida");

                        if (ordem.Equals(Model.Ordem.XmlEmpresa))
                            directoryCte = importDir.SaidaEmpresa(company, confDBSisctCte.Value, year, month);
                        else
                            directoryCte = importDir.SaidaSefaz(company, confDBSisctCte.Value, year, month);

                        sped = importSped.CTeType(caminhoDestinoArquivoOriginalSped, "1");
                    }
                    

                    List<List<Dictionary<string, string>>> ctes = new List<List<Dictionary<string, string>>>();

                    ctes = importXml.CTeAll(directoryCte);


                    if (ordem.Equals(Model.Ordem.XmlSefaz) || ordem.Equals(Model.Ordem.XmlEmpresa))
                    {
                        List<List<Dictionary<string, string>>> ctesXml = new List<List<Dictionary<string, string>>>();

                        foreach (var cte in ctes)
                        {
                            for (int i = 0; i < cte.Count; i++)
                            {
                                if (cte[i].ContainsKey("chave"))
                                {
                                    string cte_xml = cte[i]["chave"];
                                    bool cte_encontrado = false;
                                    foreach (var cteSped in sped)
                                    {
                                        if (cte_xml == cteSped[1])
                                        {
                                            cte_encontrado = true;
                                            break;
                                        }
                                    }

                                    if (cte_encontrado == false)
                                        ctesXml.Add(cte);
                                }
                            }
                        }

                        ViewBag.Ctes = ctesXml;
                        ViewBag.QuantidadeCTe = ctes.Count;
                    }
                    else if (ordem.Equals(Model.Ordem.SpedXS) || ordem.Equals(Model.Ordem.SpedXE))
                    {
                        List<List<string>> ctesSped = new List<List<string>>();

                        foreach (var cteSped in sped)
                        {
                            bool cte_encontrado = false;

                            foreach (var cte in ctes)
                            {
                                for (int i = 0; i < cte.Count; i++)
                                {
                                    if (cte[i].ContainsKey("chave"))
                                    {
                                        string cte_xml = cte[i]["chave"];

                                        if (cte_xml == cteSped[1])
                                        {
                                            cte_encontrado = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (cte_encontrado == false)
                                ctesSped.Add(cteSped);
                        }

                        ViewBag.Ctes = ctesSped;
                        ViewBag.QuantidadeCTe = sped.Count;
                    }

                   
                    List<string> modal = new List<string> { "Rod", "Aér", "Aquav", "Ferrov", "Dutov", "Multi" };
                    List<string> tipo = new List<string> { "Normal", "Subcontratação", "Redespacho", "Red. Inter", "Ser. Vin. Multi" };
                    ViewBag.Modal = modal;
                    ViewBag.Tipos = tipo;

                }
                else if (opcao.Equals(Model.Opcao.Planilha))
                {
                    if (arquivoExcel == null || arquivoExcel.Length == 0)
                    {
                        ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
                        return View(ViewData);
                    }

                    string filedirExcel = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "Planilha");

                    if (!Directory.Exists(filedirExcel))
                        Directory.CreateDirectory(filedirExcel);

                    string nomeArquivoExcel = company.Document + "Sefaz";

                    if (arquivoExcel.FileName.Contains(".xls") || arquivoExcel.FileName.Contains(".xlsx"))
                        nomeArquivoExcel += ".xls";

                    string caminhoDestinoArquivoExcel = caminho_WebRoot + "\\Uploads\\Planilha\\";
                    string caminhoDestinoArquivoOriginalExcel = caminhoDestinoArquivoExcel + nomeArquivoExcel;

                    string[] paths_upload_excel = Directory.GetFiles(caminhoDestinoArquivoExcel);

                    if (System.IO.File.Exists(caminhoDestinoArquivoOriginalExcel))
                        System.IO.File.Delete(caminhoDestinoArquivoOriginalExcel);

                    var streamExcel = new FileStream(caminhoDestinoArquivoOriginalExcel, FileMode.Create);
                    await arquivoExcel.CopyToAsync(streamExcel);
                    streamExcel.Close();

                    List<List<string>> notasExcel = new List<List<string>>();
                    List<List<string>> notasFsist = new List<List<string>>();


                    if (ordem.Equals(Model.Ordem.Malha))
                    {
                        var notesSped = importSped.NFeType(caminhoDestinoArquivoOriginalSped, ident);
                        var notesPlanilha = importExcel.Notes(caminhoDestinoArquivoOriginalExcel);
                        
                        foreach (var nPlanilha in notesPlanilha)
                        {
                            bool nota_encontrada = false;

                            foreach (var nSped in notesSped)
                            {
                                if (nSped[0].Equals(nPlanilha[5]))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }

                            if (nota_encontrada.Equals(false) && nPlanilha[5].Length.Equals(44))
                                notasExcel.Add(nPlanilha);
                        }
                    }
                    else if (ordem.Equals(Model.Ordem.FsistXSefaz) || ordem.Equals(Model.Ordem.FsistXEmpresa))
                    {
                        List<List<Dictionary<string, string>>> notesValidas = new List<List<Dictionary<string, string>>>();


                        string directoryValida = "";

                        if (ident.Equals("0"))
                        {
                            confDBSisctNfe = _configurationService.FindByName("NFe");
                            directoryValida = importDir.Entrada(company, confDBSisctNfe.Value, year, month);
                        }
                        else if (ident.Equals("1"))
                        {
                            confDBSisctNfe = _configurationService.FindByName("NFe Saida");

                            if (ordem.Equals(Model.Ordem.FsistXEmpresa))
                            {
                                directoryValida = importDir.SaidaEmpresa(company, confDBSisctNfe.Value, year, month);
     
                            }
                            else
                            {
                                directoryValida = importDir.SaidaSefaz(company, confDBSisctNfe.Value, year, month);
                            }

                        }

                        notesValidas = importXml.NFeResumeEmit(directoryValida);
                        var notesPlanilha = importExcel.NotesFsist(caminhoDestinoArquivoOriginalExcel);


                        //  NFe e NFCe
                        foreach (var note in notesPlanilha)
                        {
                            bool nota_encontrada = false;

                            foreach (var notaXml in notesValidas)
                            {
                                string nota_xml = notaXml[0]["chave"];
                                if (note[3].Equals(nota_xml))
                                {
                                    nota_encontrada = true;
                                    break;
                                }
                            }

                            if (nota_encontrada.Equals(false))
                                notasFsist.Add(note);
                        }

                    }

                    ViewBag.NotasExcel = notasExcel;
                    ViewBag.NotasFsist = notasFsist;
                }

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");

                return View();
                
            }
            catch (ArgumentException aEx)
            {
                return BadRequest(new { erro = 500, message = aEx.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
            
        }

        public IActionResult Move()
        {
            try
            {
                var importXml = new Xml.Import();
                var importDir = new Diretorio.Import();
                var importMonth = new Period.Month();

                var confDBSisctNfe = _configurationService.FindByName("NFe");

                var directory = importDir.Entrada(SessionManager.GetCompanyInSession(), confDBSisctNfe.Value, SessionManager.GetYearInSession(), SessionManager.GetMonthInSession());

                var notasMove = importXml.NFeMove(directory, SessionManager.GetKeysInSession());

                var numberMonth = importMonth.NumberMonth(SessionManager.GetMonthInSession());

                var month = importMonth.NameMonthNext(numberMonth);
                var year = SessionManager.GetYearInSession();

                if (numberMonth == 12)
                    year = (Convert.ToInt32(year) + 1).ToString();

                var newDirectory = importDir.Entrada(SessionManager.GetCompanyInSession(), confDBSisctNfe.Value, year, month);

                if (!Directory.Exists(newDirectory))
                    Directory.CreateDirectory(newDirectory);

                foreach (var nota in notasMove)
                {
                    var temp = nota.Split("\\");
                    var dirtemp = newDirectory + "\\" + temp[temp.Count() - 1];

                    if (System.IO.File.Exists(dirtemp))
                        System.IO.File.Delete(dirtemp);

                    if (System.IO.File.Exists(nota))
                        System.IO.File.Move(nota, dirtemp);
                }

                return Ok(new { code = 200, message = "As notas foram movida para " + month + " de " + year + " com sucesso!" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}