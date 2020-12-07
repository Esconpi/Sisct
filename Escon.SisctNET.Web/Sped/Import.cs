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
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US"); 

            List<string> sped = new List<string>();
            List<List<string>> productsAlteration = new List<List<string>>();
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
            List<List<string>> cfops = new List<List<string>>();

            decimal valorNota = 0, ipiNota = 0, descontoNota = 0, outrasDespesasNota = 0, freteNota = 0, seguroNota = 0, icmsRetidoST = 0,
                valorNotaNF = 0 , vBCNF = 0, vICMSNF = 0, vBCNota = 0, vICMSNota = 0;
            int posC100 = -1;
            string line, tipoOperacao = "", chave = "", emissao = "", numero = "";
            bool diferenca = false;

            var importXml = new Xml.Import(_companyCfopService);

            notes = importXml.Nfe(directoryNfe);

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
                                                    vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    vItem.Equals(vProd - vDesc + vOutro) ||
                                                    vItem.Equals(vProd - vDesc) ||
                                                    vItem.Equals(vProd)))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;

                                                }
                                                else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                    ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                                    (vItem + vIpiItem).Equals(vProd)))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                    ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd)))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if (vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                    vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                    vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    vItem.Equals(vProd - vDesc + vOutro) ||
                                                    vItem.Equals(vProd - vDesc) ||
                                                    vItem.Equals(vProd))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                   (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                   (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                   (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                   (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                   (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                                   (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                                   (vItem + vIpiItem).Equals(vProd))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                   (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                   (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                   (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                   (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                   (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                                   (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
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
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                   vItem.Equals(vProd - vDesc + vOutro) ||
                                                   vItem.Equals(vProd - vDesc) ||
                                                   vItem.Equals(vProd)))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;

                                            }
                                            else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                                (vItem + vIpiItem).Equals(vProd)))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd)))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if (vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                vItem.Equals(vProd - vDesc + vOutro) ||
                                                vItem.Equals(vProd - vDesc) ||
                                                vItem.Equals(vProd))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                               (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                               (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                               (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                               (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                               (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                               (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                               (vItem + vIpiItem).Equals(vProd))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                               (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                               (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                               (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                               (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                               (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                               (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
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

        public List<string> SpedEntry(string directorySped, string directoryNfe)
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

            var importXml = new Xml.Import(_companyCfopService);

            notes = importXml.Nfe(directoryNfe);

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
                                                    vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    vItem.Equals(vProd - vDesc + vOutro) ||
                                                    vItem.Equals(vProd - vDesc) ||
                                                    vItem.Equals(vProd)))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;

                                                }
                                                else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                    ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                                    (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                                    (vItem + vIpiItem).Equals(vProd)))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                    ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
                                                    (vItem + vIpiItem - vDescItem).Equals(vProd)))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if (vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                    vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                    vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                    vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                    vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                    vItem.Equals(vProd - vDesc + vOutro) ||
                                                    vItem.Equals(vProd - vDesc) ||
                                                    vItem.Equals(vProd))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                   (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                   (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                   (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                   (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                   (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                                   (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                                   (vItem + vIpiItem).Equals(vProd))
                                                {
                                                    valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                    break;
                                                }
                                                else if ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                   (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                   (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                   (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                   (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                   (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                                   (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
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
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                   vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                   vItem.Equals(vProd - vDesc + vOutro) ||
                                                   vItem.Equals(vProd - vDesc) ||
                                                   vItem.Equals(vProd)))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;

                                            }
                                            else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                                (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                                (vItem + vIpiItem).Equals(vProd)))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if (Convert.ToInt32(linha[2]).Equals(nItem) &&
                                                ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
                                                (vItem + vIpiItem - vDescItem).Equals(vProd)))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if (vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                                vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                                vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                                vItem.Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                                vItem.Equals(vProd - vDesc + vOutro + vFrete) ||
                                                vItem.Equals(vProd - vDesc + vOutro) ||
                                                vItem.Equals(vProd - vDesc) ||
                                                vItem.Equals(vProd))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if ((vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                               (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                               (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                               (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                               (vItem + vIpiItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                               (vItem + vIpiItem).Equals(vProd - vDesc + vOutro) ||
                                               (vItem + vIpiItem).Equals(vProd - vDesc) ||
                                               (vItem + vIpiItem).Equals(vProd))
                                            {
                                                valorProduto = vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST;
                                                break;
                                            }
                                            else if ((vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST + vFCPST) ||
                                               (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI + vICMSST) ||
                                               (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg + vIPI) ||
                                               (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete + vSeg) ||
                                               (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro + vFrete) ||
                                               (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc + vOutro) ||
                                               (vItem + vIpiItem - vDescItem).Equals(vProd - vDesc) ||
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
                                    if(pICMS > 0)
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

        public List<string> SpedEntry(string directorySped)
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

        public decimal SpedCredito(string directorySped, List<string> cfopsDevo, List<string> cfopsCompra,
                                    List<string> cfopsBonifi, List<string> cfopsCompraST, List<string> cfopsTransf,
                                    List<string> cfopsTransfST)
        {
            decimal totalDeCredito = 0;

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line, tipo = "";

            try
            {

                bool fob = false, cfop = false;
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("C100"))
                    {
                        tipo = linha[2];
                        cfop = false;
                    }

                    if (linha[1].Equals("C190") && (cfopsCompra.Contains(linha[3]) || cfopsDevo.Contains(linha[3]) || 
                        cfopsBonifi.Contains(linha[3]) || cfopsCompraST.Contains(linha[3]) || cfopsTransf.Contains(linha[3])
                        || cfopsTransfST.Contains(linha[3])) && !linha[7].Equals("") && tipo == "0")
                    {
                        totalDeCredito += Convert.ToDecimal(linha[7]);
                        cfop = true;

                    }

                    if (linha[1].Equals("C191") && cfop == true && tipo == "0")
                    {
                        totalDeCredito += Convert.ToDecimal(linha[2]);
                    }

                    if (linha[1].Equals("D100"))
                    {
                        tipo = linha[2];
                    }

                    if (tipo == "0")
                    {
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

        public List<decimal> SpedDevolucao(string directorySped, List<string> cfopsDevo, List<Model.TaxationNcm> taxationNcms)
        {
            List<decimal> Devolucoes = new List<decimal>();
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

            var codeProd1 = taxationNcms.Where(_ => _.TypeNcmId.Equals(1)).Select(_ => _.CodeProduct).ToList();
            var codeProd2 = taxationNcms.Where(_ => _.TypeNcmId.Equals(2)).Select(_ => _.CodeProduct).ToList();
            var codeProd3 = taxationNcms.Where(_ => _.TypeNcmId.Equals(3)).Select(_ => _.CodeProduct).ToList();
            var codeProd4 = taxationNcms.Where(_ => _.TypeNcmId.Equals(4)).Select(_ => _.CodeProduct).ToList();

            var ncm1 = taxationNcms.Where(_ => _.TypeNcmId.Equals(1)).Select(_ => _.Ncm.Code).ToList();
            var ncm2 = taxationNcms.Where(_ => _.TypeNcmId.Equals(2)).Select(_ => _.Ncm.Code).ToList();
            var ncm3 = taxationNcms.Where(_ => _.TypeNcmId.Equals(3)).Select(_ => _.Ncm.Code).ToList();
            var ncm4 = taxationNcms.Where(_ => _.TypeNcmId.Equals(4)).Select(_ => _.Ncm.Code).ToList();

            List<TaxationNcm> ncmsTaxation = new List<TaxationNcm>();
            List<string> codeProdMono = new List<string>();
            List<string> ncmMono = new List<string>();

            decimal devolucaoComercio = 0, devolucaoServico = 0, devolucaoPetroleo = 0, devolucaoTransporte = 0, devolucaoNormal = 0;

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("0200"))
                    {
                        StreamReader archiveSpedTemp = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));
                        string lineTemp, tipo = "";
                        try
                        {
                            while ((lineTemp = archiveSpedTemp.ReadLine()) != null)
                            {
                                string[] linhaTemp = lineTemp.Split('|');

                                if (linhaTemp[1].Equals("C100"))
                                {
                                    tipo = linhaTemp[2];
                                }

                                if (linhaTemp[1].Equals("C100") && tipo == "0")
                                {
                                    DateTime dataNota = Convert.ToDateTime(linhaTemp[10].Substring(2, 2) + "/" + linhaTemp[10].Substring(0, 2) + "/" + linhaTemp[10].Substring(4, 4));
                                    ncmsTaxation = _taxationNcmService.FindAllInDate(taxationNcms, dataNota);
                                    codeProdMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.CodeProduct).ToList();
                                    ncmMono = ncmsTaxation.Where(_ => _.Type.Equals("Monofásico")).Select(_ => _.Ncm.Code).ToList();
                                }

                                if (linhaTemp[1].Equals("C170") && tipo == "0" && linhaTemp[3].Equals(linha[2]))
                                {
                                    if (cfopsDevo.Contains(linhaTemp[11]) && !linhaTemp[7].Equals(""))
                                    {
                                        if (codeProd1.Contains(linha[3]) && ncm1.Contains(linha[8]))
                                        {
                                            devolucaoPetroleo += Convert.ToDecimal(linha[7]);
                                        }
                                        else if (codeProd2.Contains(linha[3]) && ncm2.Contains(linha[8]))
                                        {
                                            devolucaoComercio += Convert.ToDecimal(linha[7]);
                                        }
                                        else if (codeProd3.Contains(linha[3]) && ncm3.Contains(linha[8]))
                                        {
                                            devolucaoTransporte += Convert.ToDecimal(linha[7]);
                                        }
                                        else if (codeProd4.Contains(linha[3]) && ncm4.Contains(linha[8]))
                                        {
                                            devolucaoServico += Convert.ToDecimal(linha[7]);
                                        }

                                        if (!codeProdMono.Contains(linha[3]) && !ncmMono.Contains(linha[8]))
                                        {
                                            devolucaoNormal += Convert.ToDecimal(linha[7]);
                                        }
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
                            archiveSpedTemp.Close();
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

            Devolucoes.Add(devolucaoPetroleo);
            Devolucoes.Add(devolucaoComercio);
            Devolucoes.Add(devolucaoTransporte);
            Devolucoes.Add(devolucaoServico);
            Devolucoes.Add(devolucaoNormal);

            return Devolucoes;
        }

        public List<List<string>> SpedNfe(string directorySped)
        {
            List<List<string>> spedNfe = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');
                    //if (linha[1] == "C100" && (linha[2] == "1" || linha[2] == "0"))
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

        public List<List<string>> SpedNfeSaida(string directorySped)
        {
            List<List<string>> spedNf = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');
                    if (linha[1] == "C100" && (linha[2] == "1" || (linha[2] == "0" && linha[3] == "0")))
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

        public List<List<string>> SpedNfeSaidaNormal(string directorySped)
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
        
        public List<List<string>> SpedNfeSaidaCancelada(string directorySped, string modelo)
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

        public List<List<string>> SpedDif(string directorySped)
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
                        sped.Add(linhaSped);
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

        public List<string> SpedCte(string directorySped)
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
                    {
                        sped.Add(linha[10]);
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

        public List<List<string>> SpedNfe(string directorySped, string tipo)
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

        public List<List<string>> SpedNfe(string directorySped, string tipo, string emissao)
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

        public List<List<string>> SpedCte(string directorySped, string tipo)
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

        public List<List<List<string>>> SpedInterna(string directorySped, List<string> cfopsCompra, List<string> cfopsBonifi,
                                                    List<string> cfopsTransf, List<string> cfopsDevo, List<string> ncms)
        {
            List<List<List<string>>> spedInterna = new List<List<List<string>>>();

            List<List<string>> spedCompra = new List<List<string>>();
            List<List<string>> spedDevo = new List<List<string>>();

            StreamReader archiveSped = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("0200"))
                    {
                        bool ncm = false;

                        for (int j = 0; j < ncms.Count(); j++)
                        {
                            int tamanho = ncms[j].Length;

                            if (ncms[j].Equals(linha[8].Substring(0, tamanho)))
                            {
                                ncm = true;
                                break;
                            }
                        }

                        if (ncm == false)
                        {
                            StreamReader archiveSpedTemp = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));
                            string lineTemp, tipo = "";

                            try
                            {
                                while ((lineTemp = archiveSpedTemp.ReadLine()) != null)
                                {
                                    string[] linhaTemp = lineTemp.Split('|');

                                    if (linhaTemp[1].Equals("C100"))
                                    {
                                        tipo = linhaTemp[2];
                                    }

                                    if (linhaTemp[1].Equals("C170") && tipo == "0" && !linhaTemp[13].Equals("") && !linhaTemp[14].Equals("") && !linhaTemp[15].Equals("") &&
                                        (cfopsCompra.Contains(linhaTemp[11]) || cfopsBonifi.Contains(linhaTemp[11]) || cfopsTransf.Contains(linhaTemp[11]) ||
                                        cfopsDevo.Contains(linhaTemp[11])))
                                    {
                                        if (linha[2].Equals(linhaTemp[3]))
                                        {
                                            int inicio = Convert.ToInt32(linhaTemp[11].Substring(0, 1));

                                            if (inicio == 1)
                                            {
                                                if (cfopsCompra.Contains(linhaTemp[11]) || cfopsBonifi.Contains(linhaTemp[11]) || cfopsTransf.Contains(linhaTemp[11]))
                                                {
                                                    int pos = -1;

                                                    for (int k = 0; k < spedCompra.Count(); k++)
                                                    {
                                                        if (spedCompra[k][1].Equals(linhaTemp[14]))
                                                        {
                                                            pos = k;
                                                            break;
                                                        }
                                                    }

                                                    if (pos < 0)
                                                    {
                                                        List<string> sped = new List<string>();
                                                        sped.Add(linhaTemp[13]);
                                                        sped.Add(linhaTemp[14]);
                                                        sped.Add(linhaTemp[15]);
                                                        spedCompra.Add(sped);
                                                    }
                                                    else
                                                    {
                                                        spedCompra[pos][0] = (Convert.ToDecimal(spedCompra[pos][0]) + Convert.ToDecimal(linhaTemp[13])).ToString();
                                                        spedCompra[pos][2] = (Convert.ToDecimal(spedCompra[pos][2]) + Convert.ToDecimal(linhaTemp[15])).ToString();
                                                    }
                                                }

                                                if (cfopsDevo.Contains(linhaTemp[11]))
                                                {
                                                    int pos = -1;

                                                    for (int k = 0; k < spedDevo.Count(); k++)
                                                    {
                                                        if (spedDevo[k][1].Equals(linhaTemp[14]))
                                                        {
                                                            pos = k;
                                                            break;
                                                        }
                                                    }

                                                    if (pos < 0)
                                                    {
                                                        List<string> sped = new List<string>();
                                                        sped.Add(linhaTemp[13]);
                                                        sped.Add(linhaTemp[14]);
                                                        sped.Add(linhaTemp[15]);
                                                        spedDevo.Add(sped);
                                                    }
                                                    else
                                                    {
                                                        spedDevo[pos][0] = (Convert.ToDecimal(spedDevo[pos][0]) + Convert.ToDecimal(linhaTemp[13])).ToString();
                                                        spedDevo[pos][2] = (Convert.ToDecimal(spedDevo[pos][2]) + Convert.ToDecimal(linhaTemp[15])).ToString();
                                                    }
                                                }

                                            }
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
                                archiveSpedTemp.Close();
                            }
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

            spedInterna.Add(spedCompra);
            spedInterna.Add(spedDevo);

            return spedInterna;
        }

        public List<List<string>> SpedDevolucao(string directorySped, List<string> cfopsDevo, List<string> ncms)
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

            string line;

            try
            {
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');

                    if (linha[1].Equals("0200"))
                    {
                        bool ncm = false;

                        for (int j = 0; j < ncms.Count(); j++)
                        {
                            int tamanho = ncms[j].Length;

                            if (ncms[j].Equals(linha[8].Substring(0, tamanho)))
                            {
                                ncm = true;
                                break;
                            }
                        }

                        if (ncm == false)
                        {
                            StreamReader archiveSpedTemp = new StreamReader(directorySped, Encoding.GetEncoding("ISO-8859-1"));
                            string lineTemp, tipo = "";

                            try
                            {
                                while ((lineTemp = archiveSpedTemp.ReadLine()) != null)
                                {
                                    string[] linhaTemp = lineTemp.Split('|');

                                    if (linhaTemp[1].Equals("C100"))
                                    {
                                        tipo = linhaTemp[2];
                                    }

                                    if (linhaTemp[1].Equals("C170") && tipo == "0" && !linhaTemp[13].Equals("") && !linhaTemp[14].Equals("") && !linhaTemp[15].Equals("") &&
                                        cfopsDevo.Contains(linhaTemp[11]))
                                    {
                                        if (linha[2].Equals(linhaTemp[3]))
                                        {
                                            int inicio = Convert.ToInt32(linhaTemp[11].Substring(0, 1));

                                            if (inicio != 1)
                                            {

                                                if (Convert.ToDecimal(linhaTemp[14]).Equals(4))
                                                {
                                                    spedDevo[0][0] = (Convert.ToDecimal(spedDevo[0][0]) + Convert.ToDecimal(linhaTemp[13])).ToString();
                                                    spedDevo[0][1] = (Convert.ToDecimal(spedDevo[0][1]) + Convert.ToDecimal(linhaTemp[15])).ToString();
                                                }

                                                if (Convert.ToDecimal(linhaTemp[14]).Equals(12))
                                                {
                                                    spedDevo[1][0] = (Convert.ToDecimal(spedDevo[1][0]) + Convert.ToDecimal(linhaTemp[13])).ToString();
                                                    spedDevo[1][1] = (Convert.ToDecimal(spedDevo[1][1]) + Convert.ToDecimal(linhaTemp[15])).ToString();
                                                }

                                            }
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
                                archiveSpedTemp.Close();
                            }
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

            return spedDevo;
        }
    }
}
