using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Escon.SisctNET.Web.Sped
{
    public class Import
    {
        private readonly ICompanyCfopService _companyCfopService;
        private readonly ITaxationNcmService _taxationNcmService;

        public Import(ICompanyCfopService companyCfopService)
        {
            _companyCfopService = companyCfopService;
        }

        public Import(
            ICompanyCfopService companyCfopService,
            ITaxationNcmService taxationNcmService)
        {
            _companyCfopService = companyCfopService;
            _taxationNcmService = taxationNcmService;
        }

        public Import() { }

        public List<string> SpedAll(string directorySped, string directoryNfe)
        {
            List<string> sped = new List<string>();
            List<List<string>> productsAlteration = new List<List<string>>();
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
            List<List<string>> cfops = new List<List<string>>();

            decimal valorNota = 0, ipiNota = 0, descontoNota = 0, outrasDespesasNota = 0, freteNota = 0, seguroNota = 0, icmsRetidoST = 0, valorNotaNF = 0;
            int posC100 = -1;
            string line, tipoOperacao = "", chave = "", emissao = "", numero = "";
            bool diferenca = false;

            var importXml = new Xml.Import(_companyCfopService);

            notes = importXml.Nfe(directoryNfe);

            StreamReader archiveSped = new StreamReader(directorySped);

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

                    if (linha[1].Equals("C100") && tipoOperacao.Equals("0"))
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
                                        valorNotaNF += Convert.ToDecimal(notes[i][j]["vNF"]);
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
                                textoC100 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + linha[5] + "|" + linha[6] + "|" + linha[7] + "|"
                                                 + linha[8] + "|" + linha[9] + "|" + linha[10] + "|" + linha[11] + "|" + valorNota.ToString().Replace(".", ",") + "|" + linha[13] + "|" + "" + "|" + linha[15] + "|"
                                                 + valorNota.ToString().Replace(".", ",") + "|" + linha[17] + "|" + "" + "|" + "" + "|" + "" + "|" + linha[21] + "|" + linha[22] + "|" + "" + "|"
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

                    if (linha[1].Equals("C170") && tipoOperacao.Equals("0") && !emissao.Equals("0"))
                    {
                        string textoC170 = "";

                        if (posC100 >= 0)
                        {
                            if (!descontoNota.Equals(0) || !freteNota.Equals(0) || !seguroNota.Equals(0) || !outrasDespesasNota.Equals(0) || !ipiNota.Equals(0) || !icmsRetidoST.Equals(0) || !valorNota.Equals(valorNotaNF) || diferenca.Equals(true))
                            {
                                decimal valorProduto = 0, vProd = 0;
                                int posCfop = -1, nItem = 1;

                                for (int i = 0; i < cfops.Count(); i++)
                                {
                                    if (linha[11].Equals(cfops[i][0]) && linha[10].Equals(cfops[i][2]))
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
                                    cfops.Add(cfop);
                                    posCfop = cfops.Count() - 1;
                                }

                                for (int i = posC100; i < posC100 + 1; i++)
                                {
                                    for (int j = 0; j < notes[i].Count(); j++)
                                    {

                                        if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            nItem = Convert.ToInt32(notes[i][j]["nItem"]);
                                            vProd = Convert.ToDecimal(notes[i][j]["vProd"]);
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vProd"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                            }
                                        }


                                        if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("pIPI"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(nItem) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vIPI"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vICMSST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(nItem) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vICMSST"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vFCPST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(nItem) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vFCPST"]);
                                            }
                                        }
                                    }

                                }

                                if (valorProduto == 0)
                                {
                                    valorProduto = Convert.ToDecimal(linha[7].Replace(",", "."));
                                }


                                valorProduto = Math.Round(valorProduto, 2);
                                cfops[posCfop][1] = (Math.Round(Convert.ToDecimal(cfops[posCfop][1]) + valorProduto, 2)).ToString();

                                textoC170 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + linha[5] + "|" + linha[6] + "|" + valorProduto.ToString().Replace(".", ",") + "|" + "" + "|"
                                    + linha[9] + "|" + linha[10] + "|" + linha[11] + "|" + linha[12] + "|" + linha[13] + "|" + linha[14] + "|" + linha[15] + "|" + "" + "|" + "" + "|"
                                    + "" + "|" + linha[19] + "|" + linha[20] + "|" + linha[21] + "|" + linha[22] + "|" + "" + "|" + "" + "|" + linha[25] + "|" + linha[26] + "|"
                                    + linha[27] + "|" + linha[28] + "|" + linha[29] + "|" + linha[30] + "|" + linha[31] + "|" + linha[32] + "|" + linha[33] + "|" + linha[34] + "|" + linha[35] + "|"
                                    + linha[36] + "|" + linha[37] + "|" + linha[38] + "|";

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

                    if (linha[1].Equals("C190") && tipoOperacao.Equals("0") && !emissao.Equals("0"))
                    {
                        string textoC190 = "";

                        if (posC100 >= 0)
                        {
                            if (!descontoNota.Equals(0) || !freteNota.Equals(0) || !seguroNota.Equals(0) || !outrasDespesasNota.Equals(0) || !ipiNota.Equals(0) || !icmsRetidoST.Equals(0) || !valorNota.Equals(valorNotaNF) || diferenca.Equals(true))
                            {

                                for (int i = 0; i < cfops.Count(); i++)
                                {
                                    if (cfops[i][0].Equals(linha[3]) && cfops[i][2].Equals(linha[2]))
                                    {
                                        textoC190 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + cfops[i][1].Replace(".", ",") + "|" + linha[6] + "|" + linha[7] + "|"
                                                        + "" + "|" + "" + "|" + linha[10] + "|" + "" + "|" + linha[12] + "|";
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
                        /*if (posC100 >= 0)
                        {
                            decimal valorCfop = 0;
                            string cfop = "", cst = "";
                            decimal valorCfopTemp = 0;

                            for (int i = posC100; i < posC100 + 1; i++)
                            {
                                for (int j = 0; j < notes[i].Count(); j++)
                                {
                                    if (notes[i][j].ContainsKey("cProd"))
                                    {
                                        if (!cst.Equals("") && !cfop.Equals(""))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(cst)) && Convert.ToInt32(linha[3]).Equals(Convert.ToInt32(cfop)))
                                            {
                                                valorCfop += valorCfopTemp;
                                            }
                                        }
                                        cfop = "";
                                        cst = "";
                                        valorCfopTemp = 0;
                                    }

                                    if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                    {
                                        cfop = notes[i][j]["CFOP"];
                                        valorCfopTemp += Convert.ToDecimal(notes[i][j]["vProd"]);
                                    }

                                    if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                    {
                                        valorCfopTemp += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                    }

                                    if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                    {
                                        valorCfopTemp -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                    }

                                    if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                    {
                                        valorCfopTemp += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                    }

                                    if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                    {
                                        valorCfopTemp += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                    }

                                    if (notes[i][j].ContainsKey("pIPI"))
                                    {
                                        valorCfopTemp += Convert.ToDecimal(notes[i][j]["vIPI"]);
                                    }

                                    if (notes[i][j].ContainsKey("vICMSST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        valorCfopTemp += Convert.ToDecimal(notes[i][j]["vICMSST"]);
                                    }

                                    if (notes[i][j].ContainsKey("vFCPST") && notes[i][j].ContainsKey("orig"))
                                    {
                                        valorCfopTemp += Convert.ToDecimal(notes[i][j]["vFCPST"]);
                                    }

                                    if (notes[i][j].ContainsKey("orig"))
                                    {
                                        cst = notes[i][j]["CST"];
                                    }

                                }
                            }

                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(cst)) && Convert.ToInt32(linha[3]).Equals(Convert.ToInt32(cfop)))
                            {
                                valorCfop += valorCfopTemp;
                            }

                            valorCfop = Math.Round(valorCfop, 2);

                            textoC190 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + valorCfop.ToString().Replace(".", ",") + "|" + linha[6] + "|" + linha[7] + "|"
                                                            + "" + "|" + "" + "|" + linha[10] + "|" + "" + "|" + linha[12] + "|";
                            sped.Add(textoC190);
                        }
                        else
                        {
                            textoC190 = "";
                            foreach (var l in linha)
                            {
                                textoC190 += l + "|";
                            }
                            sped.Add(textoC190);
                        }*/

                        textoC190 = "";
                        foreach (var l in linha)
                        {
                            textoC190 += l + "|";
                        }
                        sped.Add(textoC190);
                    }

                    if (!linha[1].Equals("C100") && !linha[1].Equals("C170") && !linha[1].Equals("C190"))
                    {
                        linha = line.TrimEnd('|').Split('|');
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }

                    if (tipoOperacao.Equals("1"))
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

        public List<string> SpedEntry(string directorySped, string directoryNfe)
        {
            List<string> sped = new List<string>();
            List<List<string>> productsAlteration = new List<List<string>>();
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
            List<List<string>> cfops = new List<List<string>>();

            decimal valorNota = 0, ipiNota = 0, descontoNota = 0, outrasDespesasNota = 0, freteNota = 0, seguroNota = 0, icmsRetidoST = 0, valorNotaNF = 0;
            int posC100 = -1;
            string line, tipoOperacao = "", chave = "", emissao = "", numero = "";
            bool diferenca = false;

            var importXml = new Xml.Import(_companyCfopService);

            notes = importXml.Nfe(directoryNfe);

            StreamReader archiveSped = new StreamReader(directorySped);

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

                    /*if (linha[1].Equals("C113"))
                    {
                        tipoOperacao = linha[2];
                    }*/

                    if (linha[1].Equals("C100") && tipoOperacao.Equals("0"))
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
                                        valorNotaNF += Convert.ToDecimal(notes[i][j]["vNF"]);
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
                                textoC100 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + linha[5] + "|" + linha[6] + "|" + linha[7] + "|"
                                                 + linha[8] + "|" + linha[9] + "|" + linha[10] + "|" + linha[11] + "|" + valorNota.ToString().Replace(".", ",") + "|" + linha[13] + "|" + "" + "|" + linha[15] + "|"
                                                 + valorNota.ToString().Replace(".", ",") + "|" + linha[17] + "|" + "" + "|" + "" + "|" + "" + "|" + linha[21] + "|" + linha[22] + "|" + "" + "|"
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

                    if (linha[1].Equals("C170") && tipoOperacao.Equals("0") && !emissao.Equals("0"))
                    {
                        string textoC170 = "";

                        if (posC100 >= 0)
                        {
                            if (!descontoNota.Equals(0) || !freteNota.Equals(0) || !seguroNota.Equals(0) || !outrasDespesasNota.Equals(0) || !ipiNota.Equals(0) || !icmsRetidoST.Equals(0) || !valorNota.Equals(valorNotaNF) || diferenca.Equals(true))
                            {
                                decimal valorProduto = 0, vProd = 0;
                                int posCfop = -1, nItem = 1;

                                for (int i = 0; i < cfops.Count(); i++)
                                {
                                    if (linha[11].Equals(cfops[i][0]) && linha[10].Equals(cfops[i][2]))
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
                                    cfops.Add(cfop);
                                    posCfop = cfops.Count() - 1;
                                }

                                for (int i = posC100; i < posC100 + 1; i++)
                                {
                                    for (int j = 0; j < notes[i].Count(); j++)
                                    {
                                        if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            nItem = Convert.ToInt32(notes[i][j]["nItem"]);
                                            vProd = Convert.ToDecimal(notes[i][j]["vProd"]);

                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vProd"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("pIPI"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(nItem) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vIPI"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vICMSST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(nItem) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vICMSST"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vFCPST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(nItem) && Convert.ToDecimal(linha[7].Replace(",", ".")).Equals(vProd))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vFCPST"]);
                                            }
                                        }
                                    }

                                }

                                if (valorProduto == 0)
                                {
                                    valorProduto = Convert.ToDecimal(linha[7].Replace(",", "."));
                                }

                                valorProduto = Math.Round(valorProduto, 2);

                                cfops[posCfop][1] = (Math.Round(Convert.ToDecimal(cfops[posCfop][1]) + valorProduto, 2)).ToString();

                                textoC170 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + linha[5] + "|" + linha[6] + "|" + valorProduto.ToString().Replace(".", ",") + "|" + "" + "|"
                                    + linha[9] + "|" + linha[10] + "|" + linha[11] + "|" + linha[12] + "|" + linha[13] + "|" + linha[14] + "|" + linha[15] + "|" + "" + "|" + "" + "|"
                                    + "" + "|" + linha[19] + "|" + linha[20] + "|" + linha[21] + "|" + linha[22] + "|" + "" + "|" + "" + "|" + linha[25] + "|" + linha[26] + "|"
                                    + linha[27] + "|" + linha[28] + "|" + linha[29] + "|" + linha[30] + "|" + linha[31] + "|" + linha[32] + "|" + linha[33] + "|" + linha[34] + "|" + linha[35] + "|"
                                    + linha[36] + "|" + linha[37] + "|" + linha[38] + "|";
                                
                                sped.Add(textoC170);

                                decimal dif = 0;
                                if (Convert.ToDecimal(linha[7].Replace(",", ".")) > valorProduto) {
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

                    if (linha[1].Equals("C190") && tipoOperacao.Equals("0") && !emissao.Equals("0"))
                    {
                        string textoC190 = "";

                        if (posC100 >= 0)
                        {
                            if (!descontoNota.Equals(0) || !freteNota.Equals(0) || !seguroNota.Equals(0) || !outrasDespesasNota.Equals(0) || !ipiNota.Equals(0) || !icmsRetidoST.Equals(0) || !valorNota.Equals(valorNotaNF) || diferenca.Equals(true))
                            {

                                for (int i = 0; i < cfops.Count(); i++)
                                {
                                    if (cfops[i][0].Equals(linha[3]) && cfops[i][2].Equals(linha[2]))
                                    {
                                        textoC190 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + cfops[i][1].Replace(".", ",") + "|" + linha[6] + "|" + linha[7] + "|"
                                                        + "" + "|" + "" + "|" + linha[10] + "|" + "" + "|" + linha[12] + "|";
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
                        /* if (posC100 >= 0)
                         {
                             decimal valorCfop = 0;
                             string cfop = "", cst = "";
                             decimal valorCfopTemp = 0;

                             for (int i = posC100; i < posC100 + 1; i++)
                             {
                                 for (int j = 0; j < notes[i].Count(); j++)
                                 {
                                     if (notes[i][j].ContainsKey("cProd"))
                                     {
                                         if (!cst.Equals("") && !cfop.Equals(""))
                                         {
                                             if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(cst)) && Convert.ToInt32(linha[3]).Equals(Convert.ToInt32(cfop)))
                                             {
                                                 valorCfop += valorCfopTemp;
                                             }
                                         }
                                         cfop = "";
                                         cst = "";
                                         valorCfopTemp = 0;
                                     }

                                     if (notes[i][j].ContainsKey("vProd") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                     {
                                         cfop = notes[i][j]["CFOP"];
                                         valorCfopTemp += Convert.ToDecimal(notes[i][j]["vProd"]);
                                     }

                                     if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                     {
                                         valorCfopTemp += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                     }

                                     if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                     {
                                         valorCfopTemp -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                     }

                                     if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                     {
                                         valorCfopTemp += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                     }

                                     if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                     {
                                         valorCfopTemp += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                     }

                                     if (notes[i][j].ContainsKey("pIPI"))
                                     {
                                         valorCfopTemp += Convert.ToDecimal(notes[i][j]["vIPI"]);
                                     }

                                     if (notes[i][j].ContainsKey("vICMSST") && notes[i][j].ContainsKey("orig"))
                                     {
                                         valorCfopTemp += Convert.ToDecimal(notes[i][j]["vICMSST"]);
                                     }

                                     if (notes[i][j].ContainsKey("vFCPST") && notes[i][j].ContainsKey("orig"))
                                     {
                                         valorCfopTemp += Convert.ToDecimal(notes[i][j]["vFCPST"]);
                                     }

                                     if (notes[i][j].ContainsKey("orig"))
                                     {
                                         cst = notes[i][j]["CST"];
                                     }

                                 }
                             }

                             if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(cst)) && Convert.ToInt32(linha[3]).Equals(Convert.ToInt32(cfop)))
                             {
                                 valorCfop += valorCfopTemp;
                             }

                             valorCfop = Math.Round(valorCfop, 2);

                             textoC190 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + valorCfop.ToString().Replace(".", ",") + "|" + linha[6] + "|" + linha[7] + "|"
                                                             + "" + "|" + "" + "|" + linha[10] + "|" + "" + "|" + linha[12] + "|";
                             sped.Add(textoC190);
                         }
                         else
                         {
                             textoC190 = "";
                             foreach (var l in linha)
                             {
                                 textoC190 += l + "|";
                             }
                             sped.Add(textoC190);
                         }*/

                        textoC190 = "";
                        foreach (var l in linha)
                        {
                            textoC190 += l + "|";
                        }
                        sped.Add(textoC190);

                    }

                    if (!linha[1].Equals("C100") && !linha[1].Equals("C170") && !linha[1].Equals("C190") && !linha[1].Equals("D100") && !linha[1].Equals("D190"))
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

                    /*if (linha[1].Equals("C113") && tipoOperacao.Equals("0"))
                    {
                        string texto = "";
                        foreach (var l in linha)
                        {
                            texto += l + "|";
                        }
                        sped.Add(texto);
                    }*/
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

        public decimal SpedCredito(string directorySped, int companyId)
        {
            decimal totalDeCredito = 0;
            StreamReader archiveSped = new StreamReader(directorySped);
            var cfopsCompra = _companyCfopService.FindByCfopActive(companyId, "entrada", "compra").Select(_ => _.Cfop.Code).ToList();
            var cfopsDevo = _companyCfopService.FindByCfopActive(companyId, "entrada", "devolução de venda").Select(_ => _.Cfop.Code).ToList();
            string line, tipo = "";
            try
            {

                var fob = false;
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');
                    /*if (linha[1].Equals("C170") && cfopsDevo.Contains(linha[11]) && linha[10].Equals("000") && !linha[15].Equals(""))
                    {
                            totalDeCredito += Convert.ToDecimal(linha[15]);
                    }*/

                    if (linha[1].Equals("C100"))
                    {
                        tipo = linha[2];
                    }
                    if (tipo == "0")
                    {
                        if (linha[1].Equals("C190") && (cfopsCompra.Contains(linha[3]) || cfopsDevo.Contains(linha[3])) && !linha[7].Equals(""))
                        {
                            totalDeCredito += Convert.ToDecimal(linha[7]);

                            /*if (cfopsDevo.Contains(linha[3]))
                            {
                                if (!linha[4].Equals(""))
                                {
                                    if (Convert.ToDecimal(linha[4]).Equals(17))
                                    {
                                        totalDeCredito += ((1 * Convert.ToDecimal(linha[6])) / 100);
                                    }
                                }
                            }*/

                        }
                        if (linha[1].Equals("D100") && linha[17].Equals("1"))
                        {
                            fob = true;
                        }
                        else if (linha[1].Equals("D100") && !linha[17].Equals("1"))
                        {
                            fob = false;
                        }
                        if (fob.Equals(true) && cfopsCompra.Contains(linha[3]) && linha[1].Equals("D190") && linha[7] != "")
                        {
                            totalDeCredito += Convert.ToDecimal(linha[7]);
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
            return totalDeCredito;
        }
        
        public decimal SpedDevolucao(string directorySped, string company)
        {
            decimal totalDevolucao = 0;
            StreamReader archiveSped = new StreamReader(directorySped);
            var cfopsDevo = _companyCfopService.FindByCfopActive(company, "devolucao").Select(_ => _.Cfop.Code).ToList();
            string line, tipo = "";
            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("C100"))
                    {
                        tipo = linha[2];
                    }

                    if (tipo == "0")
                    {
                        if (linha[1].Equals("C170") && cfopsDevo.Contains(linha[11]) && !linha[7].Equals(""))
                        {
                            totalDevolucao += Convert.ToDecimal(linha[7]);
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
            return totalDevolucao;
        }
        
        public decimal SpedDevolucaoMono(string directorySped, string company, List<Model.TaxationNcm> ncmsMonofasico)
        {
            decimal totalDevolucao = 0;
            StreamReader archiveSped = new StreamReader(directorySped);
            var cfopsDevo = _companyCfopService.FindByCfopActive(company, "devolucao").Select(_ => _.Cfop.Code).ToList();
            string line, tipo = "";

            List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();
            List<string> codeProdMono = new List<string>();

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("C100"))
                    {
                        tipo = linha[2];
                    }

                    if (linha[1].Equals("C100") && tipo == "0")
                    {
                        DateTime dataNota = Convert.ToDateTime(linha[10].Substring(0, 2) + "/" + linha[10].Substring(2, 2) + "/" + linha[10].Substring(4, 2));
                        ncmsTaxation = _taxationNcmService.FindAllInDate(ncmsMonofasico, dataNota).Where(_ => _.Company.CountingTypeId.Equals(2) || _.Company.CountingTypeId.Equals(3)).ToList();
                        codeProdMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.CodeProduct).ToList();
                    }

                    if (linha[1].Equals("C170") && cfopsDevo.Contains(linha[11]) && !linha[7].Equals("") && tipo == "0")
                    {
                        if (codeProdMono.Contains(linha[3]))
                        {
                            totalDevolucao += Convert.ToDecimal(linha[7]);
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
            return totalDevolucao;
        }
    }
}
