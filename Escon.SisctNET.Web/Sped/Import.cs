using DocumentFormat.OpenXml.EMMA;
using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Escon.SisctNET.Web.Sped
{
    public class Import
    {
        private readonly ICfopService _cfopService;
        private readonly ITaxationNcmService _taxationNcmService;
        private readonly INcmConvenioService _ncmConvenioService;

        public Import(ICfopService cfopService)
        {
            _cfopService = cfopService;
        }

        public Import(
            ICfopService cfopService,
            ITaxationNcmService taxationNcmService
            )
        {
            _cfopService = cfopService;
            _taxationNcmService = taxationNcmService;
        }

        public Import(
            ICfopService cfopService,
            INcmConvenioService ncmConvenioService
            )
        {
            _cfopService = cfopService;
            _ncmConvenioService = ncmConvenioService;
        }

        public Import() { }


        // NFe

        public decimal NFeCredit(string directorySped, List<string> cfopsDevo, List<string> cfopsCompra, List<string> cfopsBonifi,
                                 List<string> cfopsCompraST, List<string> cfopsTransf, List<string> cfopsTransfST, List<string> cfopsDevoST)
        {
            decimal totalDeCredito = 0;

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line, tipoOperacao = "";

            try
            {

                bool fob = false, cfop = false;
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("C100"))
                    {
                        tipoOperacao = linha[2];
                        cfop = false;
                    }

                    if (linha[1].Equals("C190") && (cfopsCompra.Contains(linha[3]) || cfopsDevo.Contains(linha[3]) ||
                        cfopsBonifi.Contains(linha[3]) || cfopsCompraST.Contains(linha[3]) || cfopsTransf.Contains(linha[3])
                        || cfopsTransfST.Contains(linha[3]) || cfopsDevoST.Contains(linha[3])) && !linha[7].Equals("") && tipoOperacao == "0")
                    {
                        totalDeCredito += Convert.ToDecimal(linha[7]);
                        cfop = true;
                    }

                    //if (linha[1].Equals("C191") && cfop == true && tipoOperacao == "0")
                        //totalDeCredito += Convert.ToDecimal(linha[2]);

                    if (linha[1].Equals("D100"))
                        tipoOperacao = linha[2];

                    if (tipoOperacao == "0")
                    {
                        if (linha[1].Equals("D100") && linha[17].Equals("1"))
                            fob = true;
                        else if (linha[1].Equals("D100") && !linha[17].Equals("1"))
                            fob = false;

                        if (fob.Equals(true) && cfopsCompra.Contains(linha[3]) && linha[1].Equals("D190") && linha[7] != "")
                            totalDeCredito += Convert.ToDecimal(linha[7]);

                    }

                    if (linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }

            }

            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }
            return totalDeCredito;
        }

        public List<decimal> NFeLPresumido(string directorySped, List<string> cfopsDevo, List<string> cfopsDevoST, List<string> cfopsBoniCompra, List<Model.TaxationNcm> taxationNcms, 
                                           Model.Company company)
        {
            List<decimal> lucroPresumido = new List<decimal>();

            var codeProd1 = taxationNcms.Where(_ => _.TypeNcm.Name.Equals("Combustível"))
                .Select(_ => _.CodeProduct)
                .ToList();
            var codeProd2 = taxationNcms.Where(_ => _.TypeNcm.Name.Equals("Comércio"))
                .Select(_ => _.CodeProduct)
                .ToList();
            var codeProd3 = taxationNcms.Where(_ => _.TypeNcm.Name.Equals("Transporte"))
                .Select(_ => _.CodeProduct)
                .ToList();
            var codeProd4 = taxationNcms.Where(_ => _.TypeNcmId.Equals("Serviço"))
                .Select(_ => _.CodeProduct)
                .ToList();

            var ncm1 = taxationNcms.Where(_ => _.TypeNcm.Name.Equals("Combustível"))
                .Select(_ => _.Ncm.Code)
                .ToList();
            var ncm2 = taxationNcms.Where(_ => _.TypeNcmId.Equals("Comércio"))
                .Select(_ => _.Ncm.Code)
                .ToList();
            var ncm3 = taxationNcms.Where(_ => _.TypeNcm.Name.Equals("Transporte"))
                .Select(_ => _.Ncm.Code)
                .ToList();
            var ncm4 = taxationNcms.Where(_ => _.TypeNcmId.Equals("Serviço"))
                .Select(_ => _.Ncm.Code)
                .ToList();

            List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();

            decimal devolucaoComercio = 0, devolucaoServico = 0, devolucaoPetroleo = 0, devolucaoTransporte = 0, devolucaoNormal = 0, bonificacao = 0;

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            var notes = NFeC100C170(directorySped);

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("0200"))
                    {
                        string tipo = "", emisao = "";

                        foreach (var note in notes)
                        {
                            if (note[1].Equals("C100"))
                            {
                                tipo = note[2];
                                emisao = note[3];
                            }

                            if (note[1].Equals("C100") && tipo == "0" && emisao == "1")
                            {
                                DateTime dataNota = Convert.ToDateTime(note[10].Substring(0, 2) + "/" + note[10].Substring(2, 2) + "/" + note[10].Substring(4, 4));
                                ncmsTaxation = _taxationNcmService.FindAllInDate(taxationNcms, dataNota);
                            }

                            if (note[1].Equals("C170") && tipo == "0" && emisao == "1" && note[3].Equals(linha[2]))
                            {
                                if ((cfopsDevo.Contains(note[11]) || cfopsDevoST.Contains(note[11])) && !note[7].Equals(""))
                                {
                                    Model.TaxationNcm ehMono = null;

                                    decimal desconto = 0;

                                    if (!note[8].Equals(""))
                                        desconto = Convert.ToDecimal(note[8]);

                                    if (company.Taxation == "Produto")
                                    {
                                        if (codeProd1.Contains(linha[3]) && ncm1.Contains(linha[8]))
                                        {
                                            // Devolução Pretoleo
                                            devolucaoPetroleo += (Convert.ToDecimal(note[7]) - desconto);
                                        }
                                        else if (codeProd2.Contains(linha[3]) && ncm2.Contains(linha[8]))
                                        {
                                            // Devolução Comercio
                                            devolucaoComercio += (Convert.ToDecimal(note[7]) - desconto);
                                        }
                                        else if (codeProd3.Contains(linha[3]) && ncm3.Contains(linha[8]))
                                        {
                                            // Devolução Transporte
                                            devolucaoTransporte += (Convert.ToDecimal(note[7]) - desconto);
                                        }
                                        else if (codeProd4.Contains(linha[3]) && ncm4.Contains(linha[8]))
                                        {
                                            // Devolução Serviço
                                            devolucaoServico += (Convert.ToDecimal(note[7]) - desconto);
                                        }

                                        var prod = linha[2];

                                        foreach (var n in ncmsTaxation)
                                        {
                                            int qtd = n.CodeProduct.Length;

                                            string code = prod.Substring(prod.Length - qtd);

                                            ehMono = ncmsTaxation.Where(_ => _.CodeProduct.Equals(code) && _.Ncm.Code.Equals(linha[8]) && (_.TaxationTypeNcm.Description.Equals("Monofásico") || 
                                                                            _.TaxationTypeNcm.Description.Equals("Aliquota Zero") ||  _.TaxationTypeNcm.Description.Equals("S. Tributária") || 
                                                                            _.TaxationTypeNcm.Description.Equals("Isento"))).FirstOrDefault();

                                            if (ehMono != null)
                                                break;

                                        }
                                    }
                                    else
                                    {
                                        if (ncm1.Contains(linha[8]))
                                        {
                                            // Devolução Pretoleo
                                            devolucaoPetroleo += (Convert.ToDecimal(note[7]) - desconto);
                                        }
                                        else if (ncm2.Contains(linha[8]))
                                        {
                                            // Devolução Comercio
                                            devolucaoComercio += (Convert.ToDecimal(note[7]) - desconto);
                                        }
                                        else if (ncm3.Contains(linha[8]))
                                        {
                                            // Devolução Transporte
                                            devolucaoTransporte += (Convert.ToDecimal(note[7]) - desconto);
                                        }
                                        else if (ncm4.Contains(linha[8]))
                                        {
                                            // Devolução Serviço
                                            devolucaoServico += (Convert.ToDecimal(note[7]) - desconto);
                                        }

                                        ehMono = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(linha[8]) && (_.TaxationTypeNcm.Description.Equals("Monofásico") ||
                                                _.TaxationTypeNcm.Description.Equals("Aliquota Zero") || _.TaxationTypeNcm.Description.Equals("S. Tributária") ||
                                                _.TaxationTypeNcm.Description.Equals("Isento"))).FirstOrDefault();
                                    }

                                    if (ehMono == null)
                                    {
                                        // Devolução Normal
                                        devolucaoNormal += (Convert.ToDecimal(note[7]) - desconto);
                                    }
                                }

                                if (cfopsBoniCompra.Contains(note[11]) && !note[7].Equals("")){
                                    // Bonificação
                                    bonificacao += Convert.ToDecimal(note[7]);
                                }
                            }
                        
                        }
                    }

                    if (linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            lucroPresumido.Add(devolucaoPetroleo);
            lucroPresumido.Add(devolucaoComercio);
            lucroPresumido.Add(devolucaoTransporte);
            lucroPresumido.Add(devolucaoServico);
            lucroPresumido.Add(devolucaoNormal);
            lucroPresumido.Add(bonificacao);

            return lucroPresumido;
        }

        public List<decimal> NFeEntry(string directorySped, List<string> cfopsCompra, List<string> cfopsBonifi, List<string> cfopsCompraST,
                                     List<string> cfopsTransf, List<string> cfopsTransfST, Model.Company company)
        {
            List<decimal> entradas = new List<decimal>();

            decimal compra = 0, transferencia = 0, transferenciaInter = 0;

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line, tipoOperacao = "", chave = "";

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("C100"))
                    {
                        tipoOperacao = linha[2];
                        chave = linha[9];
                        var t = chave.Substring(0, 2);
                    }

                    if (linha[1].Equals("C170") && tipoOperacao == "0")
                    {
                        if ((cfopsCompra.Contains(linha[11]) || cfopsBonifi.Contains(linha[11]) || cfopsCompraST.Contains(linha[11])) && !linha[7].Equals(""))
                        {
                            compra += Convert.ToDecimal(linha[7]);
                        }

                        if ((cfopsTransf.Contains(linha[11]) || cfopsTransfST.Contains(linha[11])) && !linha[7].Equals(""))
                        {
                            transferencia += Convert.ToDecimal(linha[7]);

                            if (!chave.Substring(0, 2).Equals(company.County.State.Code))
                                transferenciaInter += Convert.ToDecimal(linha[7]);
                        }
                    }

                    if (linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            entradas.Add(compra);
            entradas.Add(transferencia);
            entradas.Add(transferenciaInter);

            return entradas;
        }
        
        public List<decimal> NFeLReal(string directorySped, List<string> cfopsCompra, List<string> cfopsBonifi, List<string> cfopsCompraST,
                                      List<string> cfopsTransf, List<string> cfopsTransfST, List<string> cfopsDevo, List<string> cfopsDevoST,
                                      List<Model.TaxationNcm> taxationNcms, Model.Company company)
        {
            List<decimal> entradas = new List<decimal>();

            decimal compra = 0, devolucao = 0;

            List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            var notes = NFeC100C170(directorySped);

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("0200"))
                    {
                        string tipo = "", emisao = "";

                        foreach (var note in notes)
                        {
                            if (note[1].Equals("C100"))
                            {
                                tipo = note[2];
                                emisao = note[3];
                            }

                            if (note[1].Equals("C100") && tipo == "0" && emisao == "1")
                            {
                                DateTime dataNota = Convert.ToDateTime(note[10].Substring(0, 2) + "/" + note[10].Substring(2, 2) + "/" + note[10].Substring(4, 4));
                                ncmsTaxation = _taxationNcmService.FindAllInDate(taxationNcms, dataNota);
                            }

                            if (note[1].Equals("C170") && tipo == "0" && emisao == "1" && note[3].Equals(linha[2]))
                            {
                                Model.TaxationNcm ehMono = null;

                                if (company.Taxation == "Produto")
                                {
                                    // Tributação Produto/NCM
                                    var prod = linha[2];
                                    foreach (var n in ncmsTaxation)
                                    {
                                        int qtd = n.CodeProduct.Length;

                                        string code = prod.Substring(prod.Length - qtd);

                                        ehMono = ncmsTaxation.Where(_ => _.CodeProduct.Equals(code) && _.Ncm.Code.Equals(linha[8]) && (_.TaxationTypeNcm.Description.Equals("Monofásico") ||
                                                _.TaxationTypeNcm.Description.Equals("Aliquota Zero") || _.TaxationTypeNcm.Description.Equals("S. Tributária") ||
                                                _.TaxationTypeNcm.Description.Equals("Isento"))).FirstOrDefault();

                                        if (ehMono != null)
                                        {
                                            break;
                                        }

                                    }
                                }
                                else
                                {
                                    // Tributação por NCM
                                    ehMono = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(linha[8]) && (_.TaxationTypeNcm.Description.Equals("Monofásico") ||
                                                _.TaxationTypeNcm.Description.Equals("Aliquota Zero") || _.TaxationTypeNcm.Description.Equals("S. Tributária") ||
                                                _.TaxationTypeNcm.Description.Equals("Isento"))).FirstOrDefault();
                                }

                                if ((cfopsCompra.Contains(note[11]) || cfopsBonifi.Contains(note[11]) || cfopsCompraST.Contains(note[11])
                                    || cfopsTransf.Contains(note[11]) || cfopsTransfST.Contains(note[11])) && !note[7].Equals(""))
                                {
                                    
                                    if (ehMono == null)
                                    {
                                        // Compra Normal
                                        compra += Convert.ToDecimal(note[7]);
                                    }
                                }

                                if ((cfopsDevo.Contains(note[11]) || cfopsDevoST.Contains(note[11])) && !note[7].Equals(""))
                                {
                                    if (ehMono == null)
                                    {
                                        // Devolução Normal
                                        devolucao += Convert.ToDecimal(note[7]);
                                    }

                                }
                            }
                        }
                    }

                    if (linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            entradas.Add(compra);
            entradas.Add(devolucao);

            return entradas;
        }

        public List<decimal> NFeDevolution(string directorySped, List<string> cfopsDevo, List<Model.TaxationNcm> taxationNcms,
                                           Model.Company company)
        {
            List<decimal> devolucoes = new List<decimal>();

            List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();

            decimal devoNormalNormal = 0, devoNormalMono = 0, devoNormalST = 0, devoNormalAliqZero = 0, devoNormalIsento = 0, devoNormalOutras = 0,
                          devoSTNormal = 0, devoSTMono = 0, devoSTST = 0, devoSTAliqZero = 0, devoSTIsento = 0, devoSTOutras = 0;

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            var notes = NFeC100C170C190(directorySped);

            decimal p = 0;
            string line, tipo = "", orig = "";

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("0200"))
                    {
                        string tipoTemp = "", emisao = "";

                        foreach (var note in notes)
                        {
                            if (note[1].Equals("C100"))
                            {
                                tipoTemp = note[2];
                                emisao = note[3];
                            }

                            if (note[1].Equals("C100") && tipoTemp == "0" && emisao == "1")
                            {
                                DateTime dataNota = Convert.ToDateTime(note[10].Substring(0, 2) + "/" + note[10].Substring(2, 2) + "/" + note[10].Substring(4, 4));
                                ncmsTaxation = _taxationNcmService.FindAllInDate(taxationNcms, dataNota);
                            }

                            if (note[1].Equals("C170") && tipoTemp == "0" && emisao == "1" && note[3].Equals(linha[2]))
                            {
                                if (cfopsDevo.Contains(note[11]) && !note[7].Equals(""))
                                {
                                    p += Convert.ToDecimal(note[7]);

                                    Model.TaxationNcm ehNormal = null;
                                    Model.TaxationNcm ehMono = null;
                                    Model.TaxationNcm ehST = null;
                                    Model.TaxationNcm ehAliqZero = null;
                                    Model.TaxationNcm ehIsento = null;
                                    Model.TaxationNcm ehOutras = null;

                                    if (company.Taxation == "Produto")
                                    {
                                        var prod = linha[2];

                                        foreach (var n in ncmsTaxation)
                                        {
                                            int qtd = n.CodeProduct.Length;

                                            string code = prod.Substring(prod.Length - qtd);

                                            ehNormal = ncmsTaxation.Where(_ => _.CodeProduct.Equals(code) && _.Ncm.Code.Equals(linha[8]) && _.TaxationTypeNcm.Description.Equals("Normal")).FirstOrDefault();
                                            ehMono = ncmsTaxation.Where(_ => _.CodeProduct.Equals(code) && _.Ncm.Code.Equals(linha[8]) && _.TaxationTypeNcm.Description.Equals("Monofásico")).FirstOrDefault();
                                            ehAliqZero = ncmsTaxation.Where(_ => _.CodeProduct.Equals(code) && _.Ncm.Code.Equals(linha[8]) && _.TaxationTypeNcm.Description.Equals("Aliquota Zero")).FirstOrDefault();
                                            ehST = ncmsTaxation.Where(_ => _.CodeProduct.Equals(code) && _.Ncm.Code.Equals(linha[8]) && _.TaxationTypeNcm.Description.Equals("S. Tributária")).FirstOrDefault();
                                            ehIsento = ncmsTaxation.Where(_ => _.CodeProduct.Equals(code) && _.Ncm.Code.Equals(linha[8]) && _.TaxationTypeNcm.Description.Equals("Isento")).FirstOrDefault();
                                            ehOutras = ncmsTaxation.Where(_ => _.CodeProduct.Equals(code) && _.Ncm.Code.Equals(linha[8]) && _.TaxationTypeNcm.Description.Equals("Outras")).FirstOrDefault();

                                            if (ehNormal != null || ehMono != null || ehST != null || ehAliqZero != null || ehIsento != null || ehOutras != null)
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        ehNormal = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(linha[8]) && _.TaxationTypeNcm.Description.Equals("Normal")).FirstOrDefault();
                                        ehMono = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(linha[8]) && _.TaxationTypeNcm.Description.Equals("Monofásico")).FirstOrDefault();
                                        ehAliqZero = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(linha[8]) && _.TaxationTypeNcm.Description.Equals("Aliquota Zero")).FirstOrDefault();
                                        ehST = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(linha[8]) && _.TaxationTypeNcm.Description.Equals("S. Tributária")).FirstOrDefault();
                                        ehIsento = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(linha[8]) && _.TaxationTypeNcm.Description.Equals("Isento")).FirstOrDefault();
                                        ehOutras = ncmsTaxation.Where(_ => _.Ncm.Code.Equals(linha[8]) && _.TaxationTypeNcm.Description.Equals("Outras")).FirstOrDefault();
                                    }

                                    if (ehNormal != null)
                                    {
                                        if (note[11].Substring(0, 1) == "2")
                                        {
                                            // Devolução Normal
                                            devoNormalNormal += Convert.ToDecimal(note[7]);
                                        }
                                        else
                                        {
                                            if (note[10] == "101" || linha[8] == "102" || linha[8] == "300")
                                            {
                                                // Devolução ST
                                                devoNormalNormal += Convert.ToDecimal(note[7]);
                                            }
                                            else if (note[10] == "500")
                                            {
                                                // Devolução Normal
                                                devoSTNormal += Convert.ToDecimal(note[7]);
                                            }
                                            else
                                            {
                                                // Devolução ST
                                                devoNormalNormal += Convert.ToDecimal(note[7]);
                                            }
                                        }
                                        
                                    }
                                    else if(ehMono != null)
                                    {
                                        if (note[11].Substring(0, 1) == "2")
                                        {
                                            // Devolução Normal
                                            devoNormalMono += Convert.ToDecimal(note[7]);
                                        }
                                        else
                                        {
                                            if (note[10] == "500")
                                            {
                                                // Devolução ST Monofásica
                                                devoSTMono += Convert.ToDecimal(note[7]);
                                            }
                                            else if (note[10] == "101" || linha[8] == "102" || linha[8] == "300")
                                            {
                                                // Devolução Normal Monofásica
                                                devoNormalMono += Convert.ToDecimal(note[7]);
                                            }
                                            else
                                            {
                                                // Devolução Normal Monofásica
                                                devoNormalMono += Convert.ToDecimal(note[7]);
                                            }
                                        }
                                     
                                    }
                                    else if (ehST != null)
                                    {
                                        if (note[11].Substring(0, 1) == "6")
                                        {
                                            // Devolução Normal ST
                                            devoNormalST += Convert.ToDecimal(note[7]);
                                        }
                                        else
                                        {
                                            if (note[10] == "500")
                                            {
                                                // Devolução ST ST
                                                devoSTST += Convert.ToDecimal(note[7]);
                                            }
                                            else if (note[10] == "101" || linha[8] == "102" || linha[8] == "300")
                                            {
                                                // Devolução Normal ST
                                                devoNormalST += Convert.ToDecimal(note[7]);
                                            }
                                            else
                                            {
                                                // Devolução Normal ST
                                                devoNormalST += Convert.ToDecimal(note[7]);
                                            }
                                        }

                                    }
                                    else if (ehAliqZero != null)
                                    {
                                        if (note[11].Substring(0, 1) == "2")
                                        {
                                            // Devolução Normal Aliq. Zero
                                            devoNormalAliqZero += Convert.ToDecimal(note[7]);
                                        }
                                        else
                                        {
                                            if (note[10] == "500")
                                            {
                                                // Devolução ST Aliq. Zero
                                                devoSTAliqZero += Convert.ToDecimal(note[7]);
                                            }
                                            else if (note[10] == "101" || linha[8] == "102" || linha[8] == "300")
                                            {
                                                // Devolução Normal Aliq. Zero
                                                devoNormalAliqZero += Convert.ToDecimal(note[7]);
                                            }
                                            else
                                            {
                                                // Devolução Normal Aliq. Zero
                                                devoNormalAliqZero += Convert.ToDecimal(note[7]);
                                            }
                                        }

                                    }
                                    else if (ehIsento != null)
                                    {
                                        if (note[11].Substring(0, 1) == "2")
                                        {
                                            // Devolução Normal Isento
                                            devoNormalIsento += Convert.ToDecimal(note[7]);
                                        }
                                        else
                                        {
                                            if (note[10] == "500")
                                            {
                                                // Devolução ST Isento
                                                devoSTIsento += Convert.ToDecimal(note[7]);
                                            }
                                            else if (note[10] == "101" || linha[8] == "102" || linha[8] == "300")
                                            {
                                                // Devolução Normal Isento
                                                devoNormalIsento += Convert.ToDecimal(note[7]);
                                            }
                                            else
                                            {
                                                // Devolução Normal Isento
                                                devoNormalIsento += Convert.ToDecimal(note[7]);
                                            }
                                        }

                                    }
                                    else if (ehOutras != null)
                                    {
                                        if (note[11].Substring(0, 1) == "2")
                                        {
                                            // Devolução Normal Outras
                                            devoNormalOutras += Convert.ToDecimal(note[7]);
                                        }
                                        else
                                        {
                                            if (note[10] == "500")
                                            {
                                                // Devolução ST Outras
                                                devoSTOutras += Convert.ToDecimal(note[7]);
                                            }
                                            else if (note[10] == "101" || linha[8] == "102" || linha[8] == "300")
                                            {
                                                // Devolução Normal Outras
                                                devoNormalOutras += Convert.ToDecimal(note[7]);
                                            }
                                            else
                                            {
                                                // Devolução Normal Outras
                                                devoNormalOutras += Convert.ToDecimal(note[7]);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        if (note[11].Substring(0, 1) == "2")
                                        {
                                            // Devolução Normal
                                            devoNormalNormal += Convert.ToDecimal(note[7]);
                                        }
                                        else
                                        {
                                            if (note[10] == "101" || linha[8] == "102" || linha[8] == "300")
                                            {
                                                // Devolução ST
                                                devoNormalNormal += Convert.ToDecimal(note[7]);
                                            }
                                            else if (note[10] == "500")
                                            {
                                                // Devolução Normal
                                                devoSTNormal += Convert.ToDecimal(note[7]);
                                            }
                                            else
                                            {
                                                // Devolução ST
                                                devoNormalNormal += Convert.ToDecimal(note[7]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (linha[1].Equals("C100")){
                        tipo = linha[2];
                        orig = linha[3];
                    }

                    if (linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            devolucoes.Add(devoNormalNormal);
            devolucoes.Add(devoSTNormal);
            devolucoes.Add(devoSTMono);
            devolucoes.Add(devoNormalMono);
            devolucoes.Add(devoNormalST);
            devolucoes.Add(devoNormalAliqZero);
            devolucoes.Add(devoNormalIsento);
            devolucoes.Add(devoSTST);
            devolucoes.Add(devoSTAliqZero);
            devolucoes.Add(devoSTIsento);
            devolucoes.Add(devoNormalOutras);
            devolucoes.Add(devoSTOutras);

            return devolucoes;
        }

        public List<string> NFeEntry(string directorySped)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            List<string> sped = new List<string>();

            string line, tipoOperacao = "";

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("C100"))
                    {
                        tipoOperacao = linha[2];
                    }

                    if (linha[1].Equals("D100"))
                    {
                        tipoOperacao = linha[2];
                    }

                    if (linha[1].Equals("C100") && tipoOperacao.Equals("0"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("C170") && tipoOperacao.Equals("0"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("C190") && tipoOperacao.Equals("0"))
                    {
                        string textoC190 = "";
                        foreach (var l in linha)
                        {
                            textoC190 += l + "|";
                        }
                        sped.Add(textoC190);

                    }

                    if (!linha[1].Equals("C100") && !linha[1].Equals("C170") && !linha[1].Equals("C190") &&
                        !linha[1].Equals("D100") && !linha[1].Equals("D190"))
                    {
                        linha = line.TrimEnd('|').Split('|');
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("D100") && tipoOperacao.Equals("0"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("D190") && tipoOperacao.Equals("0"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }
            return sped;
        }

        public List<string> NFeAll(string directorySped, string directoryNfe)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            List<string> sped = new List<string>();
            List<List<string>> productsAlteration = new List<List<string>>();
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
            List<List<string>> cfops = new List<List<string>>();

            decimal valorNota = 0, ipiNota = 0, descontoNota = 0, outrasDespesasNota = 0, freteNota = 0, seguroNota = 0, icmsRetidoST = 0,
                valorNotaNF = 0, vBCNF = 0, vICMSNF = 0, vBCNota = 0, vICMSNota = 0;
            int posC100 = -1;
            string line, tipoOperacao = "", chave = "", emissao = "", numero = "";
            bool diferenca = false;

            var importXml = new Xml.Import(_cfopService);

            notes = importXml.NFeAll(directoryNfe);

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("C100"))
                    {
                        tipoOperacao = linha[2];
                        chave = linha[9];
                        emissao = linha[3];
                        numero = linha[8];
                    }

                    if (linha[1].Equals("C100") && tipoOperacao.Equals("0") && !emissao.Equals("0"))
                    {
                        string textoC100 = "";
                        valorNota = 0;
                        ipiNota = 0;
                        descontoNota = 0;
                        outrasDespesasNota = 0;
                        freteNota = 0;
                        seguroNota = 0;
                        icmsRetidoST = 0;
                        valorNotaNF = 0;
                        vBCNF = 0;
                        vICMSNF = 0;
                        vBCNota = 0;
                        vICMSNota = 0;
                        posC100 = -1;
                        cfops.Clear();

                        if (!linha[16].Equals(""))
                        {
                            valorNota = Convert.ToDecimal(linha[16].Replace(",", "."));
                        }

                        if (!linha[14].Equals(""))
                        {
                            descontoNota = Convert.ToDecimal(linha[14].Replace(",", "."));
                        }

                        if (!linha[18].Equals(""))
                        {
                            freteNota = Convert.ToDecimal(linha[18].Replace(",", "."));
                        }

                        if (!linha[19].Equals(""))
                        {
                            seguroNota = Convert.ToDecimal(linha[19].Replace(",", "."));
                        }

                        if (!linha[20].Equals(""))
                        {
                            outrasDespesasNota = Convert.ToDecimal(linha[20].Replace(",", "."));
                        }

                        if (!linha[21].Equals(""))
                        {
                            vBCNota = Convert.ToDecimal(linha[21].Replace(",", "."));
                        }

                        if (!linha[22].Equals(""))
                        {
                            vICMSNota = Convert.ToDecimal(linha[22].Replace(",", "."));
                        }

                        if (!linha[24].Equals(""))
                        {
                            icmsRetidoST = Convert.ToDecimal(linha[24].Replace(",", "."));
                        }

                        if (!linha[25].Equals(""))
                        {
                            ipiNota = Convert.ToDecimal(linha[25].Replace(",", "."));
                        }

                        for (int i = 0; i < notes.Count(); i++)
                        {
                            if (chave.Equals(notes[i][0]["chave"]))
                            {
                                posC100 = i;
                                break;
                            }
                        }

                        if (posC100 >= 0)
                        {
                            for (int i = posC100; i < posC100 + 1; i++)
                            {
                                for (int j = 0; j < notes[i].Count(); j++)
                                {
                                    if (notes[i][j].ContainsKey("vNF"))
                                    {
                                        valorNotaNF = Convert.ToDecimal(notes[i][j]["vNF"]);

                                        if (notes[i][j].ContainsKey("vBC"))
                                        {
                                            vBCNF = Convert.ToDecimal(notes[i][j]["vBC"]);
                                        }

                                        if (notes[i][j].ContainsKey("vICMS"))
                                        {
                                            vICMSNF = Convert.ToDecimal(notes[i][j]["vICMS"]);
                                        }

                                    }
                                }
                            }
                        }


                        if (posC100 >= 0)
                        {
                            if (!descontoNota.Equals(0) || !freteNota.Equals(0) || !seguroNota.Equals(0) || !outrasDespesasNota.Equals(0) || !ipiNota.Equals(0) || !icmsRetidoST.Equals(0) || !valorNota.Equals(valorNotaNF))
                            {
                                diferenca = true;
                                valorNota = Math.Round(valorNotaNF, 2);

                                if (notes[posC100][1]["finNFe"].Equals("2"))
                                {
                                    vBCNota = Math.Round(vBCNF, 2);
                                    vICMSNota = Math.Round(vICMSNF, 2);
                                }

                                // Linha C100
                                textoC100 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + linha[5] + "|" + linha[6] + "|" + linha[7] + "|"
                                                 + linha[8] + "|" + linha[9] + "|" + linha[10] + "|" + linha[11] + "|" + valorNota.ToString().Replace(".", ",") + "|" + linha[13] + "|" + "" + "|" + linha[15] + "|"
                                                 + valorNota.ToString().Replace(".", ",") + "|" + linha[17] + "|" + "" + "|" + "" + "|" + "" + "|" + vBCNota.ToString().Replace(".", ",") + "|" + vICMSNota.ToString().Replace(".", ",") + "|" + "" + "|"
                                                 + "" + "|" + "" + "|" + linha[26] + "|" + linha[27] + "|" + linha[28] + "|" + linha[29] + "|";
                                sped.Add(textoC100);
                            }
                            else
                            {
                                foreach (var l in linha)
                                {
                                    textoC100 += l + "|";
                                }
                                sped.Add(textoC100);
                            }
                        }
                        else
                        {
                            foreach (var l in linha)
                            {
                                textoC100 += l + "|";
                            }
                            sped.Add(textoC100);
                        }

                    }

                    if (linha[1].Equals("C100") && tipoOperacao.Equals("0") && emissao.Equals("0"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("C100") && tipoOperacao.Equals("1"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("C170") && tipoOperacao.Equals("0") && !emissao.Equals("0"))
                    {
                        string textoC170 = "";

                        decimal valorProdutoTemp = Convert.ToDecimal(linha[7].Replace(",", "."));

                        if (linha[8] != "")
                        {
                            valorProdutoTemp -= Convert.ToDecimal(linha[8].Replace(",", "."));
                        }

                        if (linha[24] != "")
                        {
                            valorProdutoTemp += Convert.ToDecimal(linha[24].Replace(",", "."));
                        }

                        if (posC100 >= 0)
                        {
                            if (!descontoNota.Equals(0) || !freteNota.Equals(0) || !seguroNota.Equals(0) || !outrasDespesasNota.Equals(0) || !ipiNota.Equals(0)
                                || !icmsRetidoST.Equals(0) || !valorNota.Equals(valorNotaNF) || diferenca.Equals(true) || !linha[7].Equals(linha[13]))
                            {
                                decimal valorProduto = 0, vProd = 0, vFCPST = 0, vOutro = 0, vSeg = 0, vFrete = 0, vDesc = 0, vBC = 0, vICMS = 0, pICMS = 0, vIPI = 0, vICMSST = 0,
                                    vItem = Convert.ToDecimal(linha[7].Replace(",", ".")), vIpiItem = 0, vDescItem = 0;

                                if (linha[24] != "")
                                {
                                    vIpiItem = Convert.ToDecimal(linha[24].Replace(",", "."));
                                }

                                if (linha[8] != "")
                                {
                                    vDescItem = Convert.ToDecimal(linha[8].Replace(",", "."));
                                }


                                int posCfop = -1, nItem = 0;

                                for (int i = 0; i < cfops.Count(); i++)
                                {
                                    if (linha[11].Equals(cfops[i][0]) && linha[10].Equals(cfops[i][2]) && linha[14].Equals(cfops[i][3]))
                                    {
                                        posCfop = i;
                                    }
                                }

                                if (posCfop < 0)
                                {
                                    List<string> cfop = new List<string>();
                                    cfop.Add(linha[11]);
                                    cfop.Add("0");
                                    cfop.Add(linha[10]);
                                    cfop.Add(linha[14]);
                                    cfop.Add("0");
                                    cfop.Add("0");
                                    cfop.Add("0");
                                    cfops.Add(cfop);
                                    posCfop = cfops.Count() - 1;
                                }

                                int cont = 0;

                                for (int i = posC100; i < posC100 + 1; i++)
                                {
                                    for (int j = 0; j < notes[i].Count(); j++)
                                    {

                                        if (notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            if (cont > 0)
                                            {
                                                if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                   (vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro) ||
                                                   vItem.Equals(vProd - vDesc + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc) ||
                                                   vItem.Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd + vIPI + vICMSST) ||
                                                   vItem.Equals(vProd + vIPI + vFCPST) ||
                                                   vItem.Equals(vProd + vIPI) ||
                                                   vItem.Equals(vProd + vICMSST) ||
                                                   vItem.Equals(vProd + vFCPST) ||
                                                   vItem.Equals(vProd)))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;

                                                }
                                                else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                    ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd)))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                    ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd)))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if (vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                       vItem.Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vIPI) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro) ||
                                                       vItem.Equals(vProd - vDesc + vIPI) ||
                                                       vItem.Equals(vProd - vDesc + vICMSST) ||
                                                       vItem.Equals(vProd - vDesc + vFCPST) ||
                                                       vItem.Equals(vProd - vDesc) ||
                                                       vItem.Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                       vItem.Equals(vProd + vIPI + vICMSST) ||
                                                       vItem.Equals(vProd + vIPI + vFCPST) ||
                                                       vItem.Equals(vProd + vIPI) ||
                                                       vItem.Equals(vProd + vICMSST) ||
                                                       vItem.Equals(vProd + vFCPST) ||
                                                       vItem.Equals(vProd))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                            }

                                            nItem = Convert.ToInt32(notes[i][j]["nItem"]);
                                            vProd = 0;
                                            vOutro = 0;
                                            vSeg = 0;
                                            vFrete = 0;
                                            vDesc = 0;
                                            vFCPST = 0;
                                            vBC = 0;
                                            vICMS = 0;
                                            pICMS = 0;
                                            vIPI = 0;
                                            vICMSST = 0;

                                            cont++;

                                            if (notes[i][j].ContainsKey("vProd"))
                                            {
                                                vProd += Convert.ToDecimal(notes[i][j]["vProd"]);
                                            }

                                            if (notes[i][j].ContainsKey("vFrete"))
                                            {
                                                vFrete = Convert.ToDecimal(notes[i][j]["vFrete"]);
                                            }

                                            if (notes[i][j].ContainsKey("vDesc"))
                                            {
                                                vDesc = Convert.ToDecimal(notes[i][j]["vDesc"]);
                                            }

                                            if (notes[i][j].ContainsKey("vOutro"))
                                            {
                                                vOutro = Convert.ToDecimal(notes[i][j]["vOutro"]);
                                            }

                                            if (notes[i][j].ContainsKey("vSeg"))
                                            {
                                                vSeg = Convert.ToDecimal(notes[i][j]["vSeg"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("pIPI"))
                                        {
                                            vIPI = Convert.ToDecimal(notes[i][j]["vIPI"]);
                                        }

                                        if (notes[i][j].ContainsKey("vICMSST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            vICMSST = Convert.ToDecimal(notes[i][j]["vICMSST"]);
                                        }

                                        if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                        {
                                            vBC = Convert.ToDecimal(notes[i][j]["vBC"]);
                                        }

                                        if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("orig"))
                                        {
                                            pICMS = Convert.ToDecimal(notes[i][j]["pICMS"]);
                                        }

                                        if (notes[i][j].ContainsKey("vICMS") && notes[i][j].ContainsKey("orig"))
                                        {
                                            vICMS = Convert.ToDecimal(notes[i][j]["vICMS"]);

                                            if (notes[i][1]["finNFe"].Equals("2"))
                                            {
                                                if (Convert.ToInt32(linha[2]).Equals(nItem))
                                                {
                                                    break;
                                                }
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vFCPST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            vFCPST = Convert.ToDecimal(notes[i][j]["vFCPST"]);
                                        }


                                        if (notes[i][j].ContainsKey("vNF"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                   (vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro) ||
                                                   vItem.Equals(vProd - vDesc + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc) ||
                                                   vItem.Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd + vIPI + vICMSST) ||
                                                   vItem.Equals(vProd + vIPI + vFCPST) ||
                                                   vItem.Equals(vProd + vIPI) ||
                                                   vItem.Equals(vProd + vICMSST) ||
                                                   vItem.Equals(vProd + vFCPST) ||
                                                   vItem.Equals(vProd)))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;

                                            }
                                            else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd)))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd)))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if (vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro) ||
                                                   vItem.Equals(vProd - vDesc + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc) ||
                                                   vItem.Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd + vIPI + vICMSST) ||
                                                   vItem.Equals(vProd + vIPI + vFCPST) ||
                                                   vItem.Equals(vProd + vIPI) ||
                                                   vItem.Equals(vProd + vICMSST) ||
                                                   vItem.Equals(vProd + vFCPST) ||
                                                   vItem.Equals(vProd))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                        }
                                    }

                                }

                                if (valorProduto == 0)
                                {
                                    valorProduto = Convert.ToDecimal(linha[7].Replace(",", "."));

                                    if (linha[8] != "")
                                    {
                                        valorProduto -= Convert.ToDecimal(linha[8].Replace(",", "."));
                                    }

                                    if (linha[24] != "")
                                    {
                                        valorProduto += Convert.ToDecimal(linha[24].Replace(",", "."));
                                    }
                                }

                                string pICMSTemp = "";

                                if (notes[posC100][1]["finNFe"].Equals("2"))
                                {
                                    valorProduto = 0;
                                    vBC = Math.Round(vBC, 2);
                                    if (pICMS > 0)
                                    {
                                        pICMSTemp = Math.Round(pICMS, 2).ToString().Replace(".", ",");
                                    }
                                    vICMS = Math.Round(vICMS, 2);
                                }
                                else
                                {
                                    if (linha[13] == "")
                                    {
                                        vBC = Math.Round(Convert.ToDecimal(0), 2);
                                    }
                                    else
                                    {
                                        vBC = Math.Round(Convert.ToDecimal(linha[13].Replace(",", ".")), 2);
                                    }

                                    pICMSTemp = linha[14];

                                    if (linha[15] == "")
                                    {
                                        vICMS = Math.Round(Convert.ToDecimal(0), 2);
                                    }
                                    else
                                    {
                                        vICMS = Math.Round(Convert.ToDecimal(linha[15].Replace(",", ".")), 2);
                                    }
                                }

                                valorProduto = Math.Round(valorProduto, 2);
                                cfops[posCfop][1] = (Math.Round(Convert.ToDecimal(cfops[posCfop][1]) + valorProduto, 2)).ToString();
                                cfops[posCfop][4] = (Math.Round(Convert.ToDecimal(cfops[posCfop][4]) + vBC, 2)).ToString();
                                cfops[posCfop][5] = pICMSTemp;
                                cfops[posCfop][6] = (Math.Round(Convert.ToDecimal(cfops[posCfop][6]) + vICMS, 2)).ToString();

                                textoC170 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + linha[5] + "|" + linha[6] + "|" + valorProduto.ToString().Replace(".", ",") + "|" + "" + "|"
                                    + linha[9] + "|" + linha[10] + "|" + linha[11] + "|" + linha[12] + "|" + vBC.ToString().Replace(".", ",") + "|" + pICMSTemp + "|"
                                    + vICMS.ToString().Replace(".", ",") + "|" + "" + "|" + "" + "|" + "" + "|" + linha[19] + "|" + linha[20] + "|" + linha[21] + "|" + linha[22] + "|" + "" + "|" + "" + "|"
                                    + linha[25] + "|" + linha[26] + "|" + linha[27] + "|" + linha[28] + "|" + linha[29] + "|" + linha[30] + "|" + linha[31] + "|" + linha[32] + "|" + linha[33] + "|"
                                    + linha[34] + "|" + linha[35] + "|" + linha[36] + "|" + linha[37] + "|" + linha[38] + "|";

                                sped.Add(textoC170);

                                decimal dif = 0;
                                if (Convert.ToDecimal(linha[7].Replace(",", ".")) > valorProduto)
                                {
                                    dif = Convert.ToDecimal(linha[7].Replace(",", ".")) - valorProduto;
                                }
                                else
                                {
                                    dif = valorProduto - Convert.ToDecimal(linha[7].Replace(",", "."));
                                }

                                List<string> prodAlteration = new List<string>();
                                prodAlteration.Add(numero);
                                prodAlteration.Add(chave);
                                prodAlteration.Add(valorNota.ToString().Replace(".", ","));
                                prodAlteration.Add(linha[2]);
                                prodAlteration.Add(linha[3]);
                                prodAlteration.Add(linha[4]);
                                prodAlteration.Add(linha[7]);
                                prodAlteration.Add(valorProduto.ToString().Replace(".", ","));
                                prodAlteration.Add(dif.ToString());
                                productsAlteration.Add(prodAlteration);

                            }
                            else
                            {
                                foreach (var l in linha)
                                {
                                    textoC170 += l + "|";
                                }
                                sped.Add(textoC170);
                            }
                        }
                        else
                        {
                            foreach (var l in linha)
                            {
                                textoC170 += l + "|";
                            }
                            sped.Add(textoC170);
                        }
                    }

                    if (linha[1].Equals("C170") && tipoOperacao.Equals("0") && emissao.Equals("0"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("C170") && tipoOperacao.Equals("1"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("C190") && tipoOperacao.Equals("0") && !emissao.Equals("0"))
                    {
                        string textoC190 = "";

                        if (posC100 >= 0)
                        {
                            if (!descontoNota.Equals(0) || !freteNota.Equals(0) || !seguroNota.Equals(0) || !outrasDespesasNota.Equals(0) || !ipiNota.Equals(0) || !icmsRetidoST.Equals(0) ||
                                !valorNota.Equals(valorNotaNF) || diferenca.Equals(true) || !linha[5].Equals(linha[6]))
                            {

                                for (int i = 0; i < cfops.Count(); i++)
                                {
                                    if (cfops[i][0].Equals(linha[3]) && cfops[i][2].Equals(linha[2]) && cfops[i][3].Equals(linha[4]))
                                    {
                                        if (notes[posC100][1]["finNFe"].Equals("2"))
                                        {
                                            textoC190 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + cfops[i][5] + "|" + cfops[i][1].Replace(".", ",") + "|"
                                             + cfops[i][4].Replace(".", ",") + "|" + cfops[i][6].Replace(".", ",") + "|" + "" + "|" + "" + "|" + linha[10] + "|" + "" + "|" + linha[12] + "|";
                                        }
                                        else
                                        {
                                            textoC190 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + cfops[i][1].Replace(".", ",") + "|"
                                            + linha[6] + "|" + linha[7] + "|" + "" + "|" + "" + "|" + linha[10] + "|" + "" + "|" + linha[12] + "|";
                                        }

                                        sped.Add(textoC190);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var l in linha)
                                {
                                    textoC190 += l + "|";
                                }
                                sped.Add(textoC190);
                            }
                        }
                        else
                        {
                            foreach (var l in linha)
                            {
                                textoC190 += l + "|";
                            }
                            sped.Add(textoC190);
                        }
                        diferenca = false;
                    }

                    if (linha[1].Equals("C190") && tipoOperacao.Equals("0") && emissao.Equals("0"))
                    {
                        string textoC190 = "";

                        foreach (var l in linha)
                        {
                            textoC190 += l + "|";
                        }
                        sped.Add(textoC190);
                    }

                    if (linha[1].Equals("C190") && tipoOperacao.Equals("1"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (!linha[1].Equals("C100") && !linha[1].Equals("C170") && !linha[1].Equals("C190") &&
                        !linha[1].Equals("D100") && !linha[1].Equals("D190"))
                    {
                        linha = line.TrimEnd('|').Split('|');
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("D100"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("D190"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            SessionManager.SetProductsSped(productsAlteration);
            return sped;
        }

        public List<string> NFeEntry(string directorySped, string directoryNfe)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            List<string> sped = new List<string>();
            List<List<string>> productsAlteration = new List<List<string>>();
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
            List<List<string>> cfops = new List<List<string>>();

            decimal valorNota = 0, ipiNota = 0, descontoNota = 0, outrasDespesasNota = 0, freteNota = 0, seguroNota = 0, icmsRetidoST = 0,
                valorNotaNF = 0, vBCNF = 0, vICMSNF = 0, vBCNota = 0, vICMSNota = 0;
            int posC100 = -1;
            string line, tipoOperacao = "", chave = "", emissao = "", numero = "";
            bool diferenca = false;

            var importXml = new Xml.Import(_cfopService);

            notes = importXml.NFeAll(directoryNfe);

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("C100"))
                    {
                        tipoOperacao = linha[2];
                        chave = linha[9];
                        emissao = linha[3];
                        numero = linha[8];
                    }

                    if (linha[1].Equals("D100"))
                    {
                        tipoOperacao = linha[2];
                    }

                    if (linha[1].Equals("C100") && tipoOperacao.Equals("0") && !emissao.Equals("0"))
                    {
                        string textoC100 = "";
                        valorNota = 0;
                        ipiNota = 0;
                        descontoNota = 0;
                        outrasDespesasNota = 0;
                        freteNota = 0;
                        seguroNota = 0;
                        icmsRetidoST = 0;
                        valorNotaNF = 0;
                        vBCNF = 0;
                        vICMSNF = 0;
                        vBCNota = 0;
                        vICMSNota = 0;
                        posC100 = -1;
                        cfops.Clear();

                        if (!linha[16].Equals(""))
                        {
                            valorNota = Convert.ToDecimal(linha[16].Replace(",", "."));
                        }

                        if (!linha[14].Equals(""))
                        {
                            descontoNota = Convert.ToDecimal(linha[14].Replace(",", "."));
                        }

                        if (!linha[18].Equals(""))
                        {
                            freteNota = Convert.ToDecimal(linha[18].Replace(",", "."));
                        }

                        if (!linha[19].Equals(""))
                        {
                            seguroNota = Convert.ToDecimal(linha[19].Replace(",", "."));
                        }

                        if (!linha[20].Equals(""))
                        {
                            outrasDespesasNota = Convert.ToDecimal(linha[20].Replace(",", "."));
                        }

                        if (!linha[21].Equals(""))
                        {
                            vBCNota = Convert.ToDecimal(linha[21].Replace(",", "."));
                        }

                        if (!linha[22].Equals(""))
                        {
                            vICMSNota = Convert.ToDecimal(linha[22].Replace(",", "."));
                        }

                        if (!linha[24].Equals(""))
                        {
                            icmsRetidoST = Convert.ToDecimal(linha[24].Replace(",", "."));
                        }

                        if (!linha[25].Equals(""))
                        {
                            ipiNota = Convert.ToDecimal(linha[25].Replace(",", "."));
                        }

                        for (int i = 0; i < notes.Count(); i++)
                        {
                            if (chave.Equals(notes[i][0]["chave"]))
                            {
                                posC100 = i;
                                break;
                            }
                        }

                        if (posC100 >= 0)
                        {
                            for (int i = posC100; i < posC100 + 1; i++)
                            {
                                for (int j = 0; j < notes[i].Count(); j++)
                                {
                                    if (notes[i][j].ContainsKey("vNF"))
                                    {
                                        valorNotaNF = Convert.ToDecimal(notes[i][j]["vNF"]);

                                        if (notes[i][j].ContainsKey("vBC"))
                                        {
                                            vBCNF = Convert.ToDecimal(notes[i][j]["vBC"]);
                                        }

                                        if (notes[i][j].ContainsKey("vICMS"))
                                        {
                                            vICMSNF = Convert.ToDecimal(notes[i][j]["vICMS"]);
                                        }

                                    }
                                }
                            }
                        }


                        if (posC100 >= 0)
                        {
                            if (!descontoNota.Equals(0) || !freteNota.Equals(0) || !seguroNota.Equals(0) || !outrasDespesasNota.Equals(0) || !ipiNota.Equals(0) || !icmsRetidoST.Equals(0) || !valorNota.Equals(valorNotaNF))
                            {
                                diferenca = true;
                                valorNota = Math.Round(valorNotaNF, 2);

                                if (notes[posC100][1]["finNFe"].Equals("2"))
                                {
                                    vBCNota = Math.Round(vBCNF, 2);
                                    vICMSNota = Math.Round(vICMSNF, 2);
                                }


                                textoC100 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + linha[5] + "|" + linha[6] + "|" + linha[7] + "|"
                                                 + linha[8] + "|" + linha[9] + "|" + linha[10] + "|" + linha[11] + "|" + valorNota.ToString().Replace(".", ",") + "|" + linha[13] + "|" + "" + "|" + linha[15] + "|"
                                                 + valorNota.ToString().Replace(".", ",") + "|" + linha[17] + "|" + "" + "|" + "" + "|" + "" + "|" + vBCNota.ToString().Replace(".", ",") + "|" + vICMSNota.ToString().Replace(".", ",") + "|" + "" + "|"
                                                 + "" + "|" + "" + "|" + linha[26] + "|" + linha[27] + "|" + linha[28] + "|" + linha[29] + "|";
                                sped.Add(textoC100);
                            }
                            else
                            {
                                foreach (var l in linha)
                                {
                                    textoC100 += l + "|";
                                }
                                sped.Add(textoC100);
                            }
                        }
                        else
                        {
                            foreach (var l in linha)
                            {
                                textoC100 += l + "|";
                            }
                            sped.Add(textoC100);
                        }

                    }

                    if (linha[1].Equals("C100") && tipoOperacao.Equals("0") && emissao.Equals("0"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("C170") && tipoOperacao.Equals("0") && !emissao.Equals("0"))
                    {
                        string textoC170 = "";

                        decimal valorProdutoTemp = Convert.ToDecimal(linha[7].Replace(",", "."));

                        if (linha[8] != "")
                        {
                            valorProdutoTemp -= Convert.ToDecimal(linha[8].Replace(",", "."));
                        }

                        if (linha[24] != "")
                        {
                            valorProdutoTemp += Convert.ToDecimal(linha[24].Replace(",", "."));
                        }

                        if (posC100 >= 0)
                        {
                            if (!descontoNota.Equals(0) || !freteNota.Equals(0) || !seguroNota.Equals(0) || !outrasDespesasNota.Equals(0) || !ipiNota.Equals(0)
                                || !icmsRetidoST.Equals(0) || !valorNota.Equals(valorNotaNF) || diferenca.Equals(true) || !linha[7].Equals(linha[13]))
                            {
                                decimal valorProduto = 0, vProd = 0, vFCPST = 0, vOutro = 0, vSeg = 0, vFrete = 0, vDesc = 0, vBC = 0, vICMS = 0, pICMS = 0, vIPI = 0, vICMSST = 0,
                                    vItem = Convert.ToDecimal(linha[7].Replace(",", ".")), vIpiItem = 0, vDescItem = 0;

                                if (linha[24] != "")
                                {
                                    vIpiItem = Convert.ToDecimal(linha[24].Replace(",", "."));
                                }

                                if (linha[8] != "")
                                {
                                    vDescItem = Convert.ToDecimal(linha[8].Replace(",", "."));
                                }


                                int posCfop = -1, nItem = 0;

                                for (int i = 0; i < cfops.Count(); i++)
                                {
                                    if (linha[11].Equals(cfops[i][0]) && linha[10].Equals(cfops[i][2]) && linha[14].Equals(cfops[i][3]))
                                    {
                                        posCfop = i;
                                    }
                                }

                                if (posCfop < 0)
                                {
                                    List<string> cfop = new List<string>();
                                    cfop.Add(linha[11]);
                                    cfop.Add("0");
                                    cfop.Add(linha[10]);
                                    cfop.Add(linha[14]);
                                    cfop.Add("0");
                                    cfop.Add("0");
                                    cfop.Add("0");
                                    cfops.Add(cfop);
                                    posCfop = cfops.Count() - 1;
                                }

                                int cont = 0;

                                for (int i = posC100; i < posC100 + 1; i++)
                                {
                                    for (int j = 0; j < notes[i].Count(); j++)
                                    {

                                        if (notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            if (cont > 0)
                                            {
                                                if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                   (vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro) ||
                                                   vItem.Equals(vProd - vDesc + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc) ||
                                                   vItem.Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd + vIPI + vICMSST) ||
                                                   vItem.Equals(vProd + vIPI + vFCPST) ||
                                                   vItem.Equals(vProd + vIPI) ||
                                                   vItem.Equals(vProd + vICMSST) ||
                                                   vItem.Equals(vProd + vFCPST) ||
                                                   vItem.Equals(vProd)))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;

                                                }
                                                else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                    ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd)))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                    ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd)))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if (vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                       vItem.Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vIPI) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                       vItem.Equals(vProd - vDesc + vOutro) ||
                                                       vItem.Equals(vProd - vDesc + vIPI) ||
                                                       vItem.Equals(vProd - vDesc + vICMSST) ||
                                                       vItem.Equals(vProd - vDesc + vFCPST) ||
                                                       vItem.Equals(vProd - vDesc) ||
                                                       vItem.Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                       vItem.Equals(vProd + vIPI + vICMSST) ||
                                                       vItem.Equals(vProd + vIPI + vFCPST) ||
                                                       vItem.Equals(vProd + vIPI) ||
                                                       vItem.Equals(vProd + vICMSST) ||
                                                       vItem.Equals(vProd + vFCPST) ||
                                                       vItem.Equals(vProd))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                            }

                                            nItem = Convert.ToInt32(notes[i][j]["nItem"]);
                                            vProd = 0;
                                            vOutro = 0;
                                            vSeg = 0;
                                            vFrete = 0;
                                            vDesc = 0;
                                            vFCPST = 0;
                                            vBC = 0;
                                            vICMS = 0;
                                            pICMS = 0;
                                            vIPI = 0;
                                            vICMSST = 0;

                                            cont++;

                                            if (notes[i][j].ContainsKey("vProd"))
                                            {
                                                vProd += Convert.ToDecimal(notes[i][j]["vProd"]);
                                            }

                                            if (notes[i][j].ContainsKey("vFrete"))
                                            {
                                                vFrete = Convert.ToDecimal(notes[i][j]["vFrete"]);

                                            }

                                            if (notes[i][j].ContainsKey("vDesc"))
                                            {
                                                vDesc = Convert.ToDecimal(notes[i][j]["vDesc"]);
                                            }

                                            if (notes[i][j].ContainsKey("vOutro"))
                                            {
                                                vOutro = Convert.ToDecimal(notes[i][j]["vOutro"]);
                                            }

                                            if (notes[i][j].ContainsKey("vSeg"))
                                            {
                                                vSeg = Convert.ToDecimal(notes[i][j]["vSeg"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("pIPI"))
                                        {
                                            vIPI = Convert.ToDecimal(notes[i][j]["vIPI"]);
                                        }

                                        if (notes[i][j].ContainsKey("vICMSST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            vICMSST = Convert.ToDecimal(notes[i][j]["vICMSST"]);
                                        }

                                        if (notes[i][j].ContainsKey("vBC") && notes[i][j].ContainsKey("orig"))
                                        {
                                            vBC = Convert.ToDecimal(notes[i][j]["vBC"]);
                                        }

                                        if (notes[i][j].ContainsKey("pICMS") && notes[i][j].ContainsKey("orig"))
                                        {
                                            pICMS = Convert.ToDecimal(notes[i][j]["pICMS"]);
                                        }

                                        if (notes[i][j].ContainsKey("vICMS") && notes[i][j].ContainsKey("orig"))
                                        {
                                            vICMS = Convert.ToDecimal(notes[i][j]["vICMS"]);

                                            if (notes[i][1]["finNFe"].Equals("2"))
                                            {
                                                if (Convert.ToInt32(linha[2]).Equals(nItem))
                                                {
                                                    break;
                                                }
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vFCPST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            vFCPST = Convert.ToDecimal(notes[i][j]["vFCPST"]);
                                        }


                                        if (notes[i][j].ContainsKey("vNF"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                   (vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro) ||
                                                   vItem.Equals(vProd - vDesc + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc) ||
                                                   vItem.Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd + vIPI + vICMSST) ||
                                                   vItem.Equals(vProd + vIPI + vFCPST) ||
                                                   vItem.Equals(vProd + vIPI) ||
                                                   vItem.Equals(vProd + vICMSST) ||
                                                   vItem.Equals(vProd + vFCPST) ||
                                                   vItem.Equals(vProd)))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;

                                            }
                                            else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd)))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd)))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if (vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc + vOutro) ||
                                                   vItem.Equals(vProd - vDesc + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vICMSST) ||
                                                   vItem.Equals(vProd - vDesc + vFCPST) ||
                                                   vItem.Equals(vProd - vDesc) ||
                                                   vItem.Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                   vItem.Equals(vProd + vIPI + vICMSST) ||
                                                   vItem.Equals(vProd + vIPI + vFCPST) ||
                                                   vItem.Equals(vProd + vIPI) ||
                                                   vItem.Equals(vProd + vICMSST) ||
                                                   vItem.Equals(vProd + vFCPST) ||
                                                   vItem.Equals(vProd))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                        }

                                    }

                                }

                                if (valorProduto == 0)
                                {
                                    valorProduto = Convert.ToDecimal(linha[7].Replace(",", "."));

                                    if (linha[8] != "")
                                    {
                                        valorProduto -= Convert.ToDecimal(linha[8].Replace(",", "."));
                                    }

                                    if (linha[24] != "")
                                    {
                                        valorProduto += Convert.ToDecimal(linha[24].Replace(",", "."));
                                    }
                                }

                                string pICMSTemp = "";

                                if (notes[posC100][1]["finNFe"].Equals("2"))
                                {
                                    valorProduto = 0;
                                    vBC = Math.Round(vBC, 2);
                                    if (pICMS > 0)
                                    {
                                        pICMSTemp = Math.Round(pICMS, 2).ToString().Replace(".", ",");
                                    }
                                    vICMS = Math.Round(vICMS, 2);
                                }
                                else
                                {
                                    if (linha[13] == "")
                                    {
                                        vBC = Math.Round(Convert.ToDecimal(0), 2);
                                    }
                                    else
                                    {
                                        vBC = Math.Round(Convert.ToDecimal(linha[13].Replace(",", ".")), 2);
                                    }

                                    pICMSTemp = linha[14];

                                    if (linha[15] == "")
                                    {
                                        vICMS = Math.Round(Convert.ToDecimal(0), 2);
                                    }
                                    else
                                    {
                                        vICMS = Math.Round(Convert.ToDecimal(linha[15].Replace(",", ".")), 2);
                                    }
                                }

                                valorProduto = Math.Round(valorProduto, 2);
                                cfops[posCfop][1] = (Math.Round(Convert.ToDecimal(cfops[posCfop][1]) + valorProduto, 2)).ToString();
                                cfops[posCfop][4] = (Math.Round(Convert.ToDecimal(cfops[posCfop][4]) + vBC, 2)).ToString();
                                cfops[posCfop][5] = pICMSTemp;
                                cfops[posCfop][6] = (Math.Round(Convert.ToDecimal(cfops[posCfop][6]) + vICMS, 2)).ToString();

                                textoC170 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + linha[5] + "|" + linha[6] + "|" + valorProduto.ToString().Replace(".", ",") + "|" + "" + "|"
                                    + linha[9] + "|" + linha[10] + "|" + linha[11] + "|" + linha[12] + "|" + vBC.ToString().Replace(".", ",") + "|" + pICMSTemp + "|"
                                    + vICMS.ToString().Replace(".", ",") + "|" + "" + "|" + "" + "|" + "" + "|" + linha[19] + "|" + linha[20] + "|" + linha[21] + "|" + linha[22] + "|" + "" + "|" + "" + "|"
                                    + linha[25] + "|" + linha[26] + "|" + linha[27] + "|" + linha[28] + "|" + linha[29] + "|" + linha[30] + "|" + linha[31] + "|" + linha[32] + "|" + linha[33] + "|"
                                    + linha[34] + "|" + linha[35] + "|" + linha[36] + "|" + linha[37] + "|" + linha[38] + "|";

                                sped.Add(textoC170);

                                decimal dif = 0;
                                if (Convert.ToDecimal(linha[7].Replace(",", ".")) > valorProduto)
                                {
                                    dif = Convert.ToDecimal(linha[7].Replace(",", ".")) - valorProduto;
                                }
                                else
                                {
                                    dif = valorProduto - Convert.ToDecimal(linha[7].Replace(",", "."));
                                }

                                List<string> prodAlteration = new List<string>();
                                prodAlteration.Add(numero);
                                prodAlteration.Add(chave);
                                prodAlteration.Add(valorNota.ToString().Replace(".", ","));
                                prodAlteration.Add(linha[2]);
                                prodAlteration.Add(linha[3]);
                                prodAlteration.Add(linha[4]);
                                prodAlteration.Add(linha[7]);
                                prodAlteration.Add(valorProduto.ToString().Replace(".", ","));
                                prodAlteration.Add(dif.ToString());
                                productsAlteration.Add(prodAlteration);

                            }
                            else
                            {
                                foreach (var l in linha)
                                {
                                    textoC170 += l + "|";
                                }
                                sped.Add(textoC170);
                            }
                        }
                        else
                        {
                            foreach (var l in linha)
                            {
                                textoC170 += l + "|";
                            }
                            sped.Add(textoC170);
                        }
                    }

                    if (linha[1].Equals("C170") && tipoOperacao.Equals("0") && emissao.Equals("0"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("C190") && tipoOperacao.Equals("0") && !emissao.Equals("0"))
                    {
                        string textoC190 = "";

                        if (posC100 >= 0)
                        {
                            if (!descontoNota.Equals(0) || !freteNota.Equals(0) || !seguroNota.Equals(0) || !outrasDespesasNota.Equals(0) || !ipiNota.Equals(0) || !icmsRetidoST.Equals(0) ||
                                !valorNota.Equals(valorNotaNF) || diferenca.Equals(true) || !linha[5].Equals(linha[6]))
                            {

                                for (int i = 0; i < cfops.Count(); i++)
                                {
                                    if (cfops[i][0].Equals(linha[3]) && cfops[i][2].Equals(linha[2]) && cfops[i][3].Equals(linha[4]))
                                    {
                                        if (notes[posC100][1]["finNFe"].Equals("2"))
                                        {
                                            textoC190 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + cfops[i][5] + "|" + cfops[i][1].Replace(".", ",") + "|"
                                             + cfops[i][4].Replace(".", ",") + "|" + cfops[i][6].Replace(".", ",") + "|" + "" + "|" + "" + "|" + linha[10] + "|" + "" + "|" + linha[12] + "|";
                                        }
                                        else
                                        {
                                            textoC190 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + cfops[i][1].Replace(".", ",") + "|"
                                            + linha[6] + "|" + linha[7] + "|" + "" + "|" + "" + "|" + linha[10] + "|" + "" + "|" + linha[12] + "|";
                                        }

                                        sped.Add(textoC190);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var l in linha)
                                {
                                    textoC190 += l + "|";
                                }
                                sped.Add(textoC190);
                            }
                        }
                        else
                        {
                            foreach (var l in linha)
                            {
                                textoC190 += l + "|";
                            }
                            sped.Add(textoC190);
                        }
                        diferenca = false;
                    }

                    if (linha[1].Equals("C190") && tipoOperacao.Equals("0") && emissao.Equals("0"))
                    {
                        string textoC190 = "";

                        foreach (var l in linha)
                        {
                            textoC190 += l + "|";
                        }
                        sped.Add(textoC190);

                    }

                    if (!linha[1].Equals("C100") && !linha[1].Equals("C170") && !linha[1].Equals("C190") &&
                        !linha[1].Equals("D100") && !linha[1].Equals("D190"))
                    {
                        linha = line.TrimEnd('|').Split('|');
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("D100") && tipoOperacao.Equals("0"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (linha[1].Equals("D190") && tipoOperacao.Equals("0"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            SessionManager.SetProductsSped(productsAlteration);
            return sped;
        }

        public List<List<string>> NFeAll(string directorySped)
        {
            List<List<string>> spedNfe = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');
                    if (linha[1] == "C100")
                    {
                        List<string> sped = new List<string>();
                        sped.Add(linha[9]);
                        sped.Add(linha[5]);
                        sped.Add(linha[8]);
                        sped.Add(linha[12]);
                        sped.Add(linha[3]);
                        sped.Add(linha[2]);
                        spedNfe.Add(sped);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }
            return spedNfe;
        }

        public List<List<string>> NFeExitNormal(string directorySped)
        {
            List<List<string>> spedNf = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');
                    if (linha[1] == "C100" && (linha[2] == "1" || (linha[2] == "0" && linha[3] == "0")) && linha[6] != "05" && linha[6] != "03" && linha[6] != "02")
                    {
                        List<string> sped = new List<string>();
                        sped.Add(linha[9]);
                        sped.Add(linha[5]);
                        sped.Add(linha[8]);
                        sped.Add(linha[12]);
                        spedNf.Add(sped);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }
            return spedNf;
        }

        public List<List<string>> NFeDif(string directorySped, string tipo)
        {
            List<List<string>> sped = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    List<string> linhaSped = new List<string>();
                    string[] linha = line.Split('|');


                    if (tipo == "0")
                    {
                        if (linha[1] == "C100" && linha[2] == "0")
                        {
                            linhaSped.Add(linha[8]);
                            linhaSped.Add(linha[9]);
                            linhaSped.Add(linha[12]);
                            linhaSped.Add(linha[14]);
                            linhaSped.Add(linha[18]);
                            linhaSped.Add(linha[19]);
                            linhaSped.Add(linha[20]);
                            linhaSped.Add(linha[24]);
                            linhaSped.Add(linha[25]);
                            linhaSped.Add(linha[16]);
                            linhaSped.Add(linha[22]);
                            sped.Add(linhaSped);
                        }
                    }
                    else
                    {
                        if (linha[1] == "C100" && (linha[2] == "1" || (linha[2] == "0" && linha[3] == "0")) && linha[6] != "05" && linha[6] != "03" && linha[6] != "02")
                        {
                            linhaSped.Add(linha[8]);
                            linhaSped.Add(linha[9]);
                            linhaSped.Add(linha[12]);
                            linhaSped.Add(linha[14]);
                            linhaSped.Add(linha[18]);
                            linhaSped.Add(linha[19]);
                            linhaSped.Add(linha[20]);
                            linhaSped.Add(linha[24]);
                            linhaSped.Add(linha[25]);
                            linhaSped.Add(linha[16]);
                            linhaSped.Add(linha[22]);
                            sped.Add(linhaSped);
                        }
                    }                   
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }
            return sped;

        }

        public List<List<string>> NFeNCM(string directorySped)
        {
            List<List<string>> products = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("0200"))
                    {
                        List<string> product = new List<string>();
                        product.Add(linha[2]);
                        product.Add(linha[3]);
                        product.Add(linha[8]);
                        products.Add(product);
                    }

                    if (linha[1].Equals("C100") || linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            return products;
        }

        public List<List<string>> NFeC100C170(string directorySped)
        {
            List<List<string>> products = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');
                    if (linha[1] == "C100")
                    {
                        products.Add(linha.ToList());
                    }

                    if (linha[1].Equals("C170"))
                        products.Add(linha.ToList());

                    if (linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            return products;
        }

        public List<List<string>> NFeC100C170C190(string directorySped)
        {
            List<List<string>> products = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line, tipo = "";

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');
                    if (linha[1] == "C100")
                    {
                        tipo = linha[2];

                        if (tipo == "0")
                            products.Add(linha.ToList());
                    }

                    if (linha[1].Equals("C170") && tipo == "0")
                        products.Add(linha.ToList());

                    if (linha[1].Equals("C190") && tipo == "0")
                        products.Add(linha.ToList());

                    if (linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            return products;
        }

        public List<List<string>> NFeC190(string directorySped)
        {
            List<List<string>> cfops = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line, tipo = "";

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1] == "C100")
                    {
                        tipo = linha[2];
                    }

                    if (linha[1].Equals("C190") && tipo == "0")
                        cfops.Add(linha.ToList());

                    if (linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            return cfops;
        }

        public List<List<string>> NFe0150(string directorySped)
        {
            List<List<string>> produtos = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("0150"))
                        produtos.Add(linha.ToList());

                    if (linha[1].Equals("E001") || linha[1].Equals("H001") || linha[1].Equals("0200"))
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            return produtos;
        }

        public List<List<string>> NFeExitCanceled(string directorySped, string modelo)
        {
            List<List<string>> spedNf = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');
                    if (linha[1] == "C100" && (linha[2] == "1" || (linha[2] == "0" && linha[3] == "0")) && (linha[6] == "05" || linha[6] == "03" || linha[6] == "02") && linha[5] == modelo)
                    {
                        List<string> sped = new List<string>();
                        sped.Add(linha[9]);
                        sped.Add(linha[5]);
                        sped.Add(linha[8]);
                        sped.Add(linha[12]);
                        spedNf.Add(sped);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }
            return spedNf;
        }

        public List<List<string>> NFeType(string directorySped, string tipo)
        {
            List<List<string>> spedNfe = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');
                    if (linha[1] == "C100" && linha[2].Equals(tipo))
                    {
                        List<string> sped = new List<string>();
                        sped.Add(linha[9]);
                        sped.Add(linha[5]);
                        sped.Add(linha[8]);
                        sped.Add(linha[12]);
                        sped.Add(linha[3]);
                        sped.Add(linha[2]);
                        sped.Add(linha[7]);
                        spedNfe.Add(sped);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }
            return spedNfe;
        }

        public List<List<string>> NFeProduct(string directorySped, List<Model.County> counties)
        {
            List<List<string>> produtos = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            var fornecdores = NFe0150(directorySped);
            var notes = NFeC100C170(directorySped);

            string line, chave = "", nNF = "", dhemi = "", vNF = "", xNome = "", cnpj = "", ie = "", uf = "";

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("0200"))
                    {
                        foreach (var note in notes)
                        {
                            if (note[1].Equals("C100"))
                            {
                                chave = note[9];
                                nNF = note[8];
                                vNF = note[12];
                                dhemi = note[10].Substring(0,2) + "/" + note[10].Substring(2, 2) + "/" + note[10].Substring(4, 4);

                                foreach(var fornecedor in fornecdores)
                                {
                                    if (fornecedor[2].Equals(note[4])){
                                        xNome = fornecedor[3];
                                        cnpj = fornecedor[5];
                                        ie = fornecedor[7];
                                        uf = counties.Where(_ => _.Code.Equals(fornecedor[8])).FirstOrDefault().State.UF;
                                        break;
                                    }
                                }
                            }

                            if (note[1].Equals("C170"))
                            {
                                if (linha[2].Equals(note[3]))
                                {
                                    List<string> produto = new List<string>();

                                    produto.Add(chave);
                                    produto.Add(nNF);
                                    produto.Add(dhemi);
                                    produto.Add(xNome);
                                    produto.Add(cnpj);
                                    produto.Add(ie);
                                    produto.Add(uf);
                                    produto.Add(vNF);

                                    produto.Add(linha[2]);
                                    produto.Add(linha[3]);
                                    produto.Add(linha[8]);
                                    produto.Add(linha[13]);
                                    produto.Add(note[11]);
                                    produto.Add(note[2]);
                                    produto.Add(note[5]);
                                    produto.Add(note[6]);
                                    produto.Add(note[7]);
                                    if(note[8] == "")
                                        produto.Add("0");
                                    else
                                        produto.Add(note[8]);
                                    if(note[24] == "")
                                        produto.Add("0");
                                    else
                                        produto.Add(note[24]);

                                    produtos.Add(produto);
                                }

                            }
                        }

                    }

                    if (linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }


            return produtos;
        }
       
        public List<List<string>> NFeProduct(string directorySped, List<Model.Cfop> cfops)
        {
            List<List<string>> products = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;
            string tipoOperacao = "", CFOP = "", cProd = "", xProd = "", pICMS = "0", pFCP = "0";
            decimal vProd = 0, vBC = 0, vICMS = 0, vFCP = 0;
            bool status = false;
            int pos = -1;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("C100"))
                    {
                        tipoOperacao = linha[2];

                        if (tipoOperacao.Equals(0) && status)
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
                        
                        status = false;
                        pICMS = "0";
                        pFCP = "0";
                        pos = -1;
                    }

                    if(linha[1].Equals("C170") && tipoOperacao.Equals("0"))
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

                        CFOP = linha[11];
                        cProd = linha[3];
                        xProd = linha[4];

                        if (linha[7] != "")
                            vProd = Convert.ToDecimal(linha[7]);

                        if (linha[13] != "")
                            vBC = Convert.ToDecimal(linha[13]);

                        if (linha[14] != "")
                            pICMS = linha[14];

                        if (linha[15] != "")
                            vICMS = Convert.ToDecimal(linha[15]);

                        status = true;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            return products;
        }

        public List<List<string>> NFeTypeEmission(string directorySped, string tipo, string emissao)
        {
            List<List<string>> spedNfe = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');
                    if (linha[1] == "C100" && linha[2].Equals(tipo) && linha[3].Equals(emissao))
                    {
                        List<string> sped = new List<string>();
                        sped.Add(linha[9]);
                        sped.Add(linha[5]);
                        sped.Add(linha[8]);
                        sped.Add(linha[12]);
                        sped.Add(linha[3]);
                        sped.Add(linha[2]);
                        sped.Add(linha[7]);
                        spedNfe.Add(sped);
                    }

                    if (linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }
            return spedNfe;
        }

        public List<List<string>> NFeProduct(string directorySped, List<Model.Cfop> cfops, decimal percentualIcms)
        {
            List<List<string>> products = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;
            string tipoOperacao = "", CFOP = "", cProd = "", xProd = "", pICMS = "0", pFCP = "0";
            decimal vProd = 0, vBC = 0, vICMS = 0, vFCP = 0;
            bool status = false;
            int pos = -1;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("C100"))
                    {
                        tipoOperacao = linha[2];
                        if (tipoOperacao.Equals(0) && status && Convert.ToDecimal(pICMS).Equals(percentualIcms))
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

                        status = false;
                        pICMS = "0";
                        pFCP = "0";
                        pos = -1;
                    }


                    if (linha[1].Equals("C170") && tipoOperacao.Equals("0"))
                    {
                        if (status && Convert.ToDecimal(pICMS).Equals(percentualIcms))
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

                        CFOP = linha[11];
                        cProd = linha[3];
                        xProd = linha[4];

                        if (linha[7] != "")
                            vProd = Convert.ToDecimal(linha[7]);

                        if (linha[13] != "")
                            vBC = Convert.ToDecimal(linha[13]);

                        if (linha[14] != "")
                            pICMS = linha[14];

                        if (linha[15] != "")
                            vICMS = Convert.ToDecimal(linha[15]);

                        status = true;
                    }

                    if (linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            return products;
        }

        public List<List<string>> NFeDevolution(string directorySped, List<string> cfopsDevo, List<string> cfopsDevoST, 
            List<Model.NcmConvenio> ncmConvenio, Company comp)
        {
            List<List<string>> spedDevo = new List<List<string>>();

            List<string> devo4 = new List<string>();
            devo4.Add("0");
            devo4.Add("0");
            spedDevo.Add(devo4);
            List<string> devo12 = new List<string>();
            devo12.Add("0");
            devo12.Add("0");
            spedDevo.Add(devo12);

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            var notes = NFeC100C170(directorySped);

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("0200"))
                    {
                        bool ncm = false;

                        foreach (var note in notes)
                        {
                            if (note[1].Equals("C100"))
                            {
                                DateTime dataNota = Convert.ToDateTime(note[10].Substring(0, 2) + "/" + note[10].Substring(2, 2) + "/" + note[10].Substring(4, 4));
                                var ncmConvenioTemp = _ncmConvenioService.FindAllInDate(ncmConvenio, dataNota);
                                ncm = _ncmConvenioService.FindByNcmExists(ncmConvenioTemp, linha[8], linha[13], comp);

                            }

                            if (note[1].Equals("C170") && !note[13].Equals("") && !note[14].Equals("") && 
                                !note[15].Equals("") && (cfopsDevo.Contains(note[11]) || cfopsDevoST.Contains(note[11])) &&
                                !ncm)
                            {
                                if (linha[2].Equals(note[3]))
                                {
                                    int inicio = Convert.ToInt32(note[11].Substring(0, 1));

                                    if (inicio != 1)
                                    {
                                        if (Convert.ToDecimal(note[14]).Equals(4))
                                        {
                                            spedDevo[0][0] = (Convert.ToDecimal(spedDevo[0][0]) + Convert.ToDecimal(note[13])).ToString();
                                            spedDevo[0][1] = (Convert.ToDecimal(spedDevo[0][1]) + Convert.ToDecimal(note[15])).ToString();
                                        }

                                        if (Convert.ToDecimal(note[14]).Equals(12))
                                        {
                                            spedDevo[1][0] = (Convert.ToDecimal(spedDevo[1][0]) + Convert.ToDecimal(note[13])).ToString();
                                            spedDevo[1][1] = (Convert.ToDecimal(spedDevo[1][1]) + Convert.ToDecimal(note[15])).ToString();
                                        }

                                    }
                                }

                            }
                        }

                    }

                    if (linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            return spedDevo;
        }

        public List<List<List<string>>> NFeInternal(string directorySped, List<string> cfopsCompra, List<string> cfopsBonifi, List<string> cfopsTransf,
                                                    List<string> cfopsDevo, List<Model.NcmConvenio> ncmConvenio, Company comp)
        {
            List<List<List<string>>> spedInterna = new List<List<List<string>>>();

            List<List<string>> spedCompra = new List<List<string>>();
            List<List<string>> spedDevo = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            var notes = NFeC100C170(directorySped);

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("0200"))
                    {
                        bool ncm = false;

                        foreach (var note in notes)
                        {
                            if (note[1].Equals("C100"))
                            {
                                DateTime dataNota = Convert.ToDateTime(note[10].Substring(0, 2) + "/" + note[10].Substring(2, 2) + "/" + note[10].Substring(4, 4));
                                var ncmConvenioTemp = _ncmConvenioService.FindAllInDate(ncmConvenio, dataNota);
                                ncm = _ncmConvenioService.FindByNcmExists(ncmConvenioTemp, linha[8], linha[13], comp);

                            }

                            if (note[1].Equals("C170") && !note[13].Equals("") && !note[14].Equals("") && !note[15].Equals("") &&
                                (cfopsCompra.Contains(note[11]) || cfopsBonifi.Contains(note[11]) || cfopsTransf.Contains(note[11]) ||
                                cfopsDevo.Contains(note[11])) && !ncm)
                            {
                                if (linha[2].Equals(note[3]))
                                {
                                    int inicio = Convert.ToInt32(note[11].Substring(0, 1));

                                    if (inicio == 1)
                                    {
                                        if (cfopsCompra.Contains(note[11]) || cfopsBonifi.Contains(note[11]) || cfopsTransf.Contains(note[11]))
                                        {
                                            int pos = -1;

                                            for (int k = 0; k < spedCompra.Count(); k++)
                                            {
                                                if (spedCompra[k][1].Equals(note[14]))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                List<string> sped = new List<string>();
                                                sped.Add(note[13]);
                                                sped.Add(note[14]);
                                                sped.Add(note[15]);
                                                spedCompra.Add(sped);
                                            }
                                            else
                                            {
                                                spedCompra[pos][0] = (Convert.ToDecimal(spedCompra[pos][0]) + Convert.ToDecimal(note[13])).ToString();
                                                spedCompra[pos][2] = (Convert.ToDecimal(spedCompra[pos][2]) + Convert.ToDecimal(note[15])).ToString();
                                            }
                                        }

                                        if (cfopsDevo.Contains(note[11]))
                                        {
                                            int pos = -1;

                                            for (int k = 0; k < spedDevo.Count(); k++)
                                            {
                                                if (spedDevo[k][1].Equals(note[14]))
                                                {
                                                    pos = k;
                                                    break;
                                                }
                                            }

                                            if (pos < 0)
                                            {
                                                List<string> sped = new List<string>();
                                                sped.Add(note[13]);
                                                sped.Add(note[14]);
                                                sped.Add(note[15]);
                                                spedDevo.Add(sped);
                                            }
                                            else
                                            {
                                                spedDevo[pos][0] = (Convert.ToDecimal(spedDevo[pos][0]) + Convert.ToDecimal(note[13])).ToString();
                                                spedDevo[pos][2] = (Convert.ToDecimal(spedDevo[pos][2]) + Convert.ToDecimal(note[15])).ToString();
                                            }
                                        }

                                    }
                                }

                            }
                        }
                    }

                    if (linha[1].Equals("E001") || linha[1].Equals("H001"))
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }

            spedInterna.Add(spedCompra);
            spedInterna.Add(spedDevo);

            return spedInterna;
        }


        // CTe

        public List<string> CTeAll(string directorySped)
        {
            List<string> sped = new List<string>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');
                    if (linha[1] == "D100")
                        sped.Add(linha[10]);
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }
            return sped;
        }

        public List<List<string>> CTeType(string directorySped, string tipo)
        {
            List<List<string>> spedNfe = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');
                    if (linha[1] == "D100" && linha[2].Equals(tipo))
                    {
                        List<string> sped = new List<string>();
                        sped.Add(linha[9]);
                        sped.Add(linha[10]);
                        sped.Add(linha[11]);
                        sped.Add(linha[15]);
                        spedNfe.Add(sped);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                archiveSped.Close();
            }
            return spedNfe;
        }

    }
}
