using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Escon.SisctNET.Service;

namespace Escon.SisctNET.Web.Taxation
{
    public class Import
    {
        private readonly ICompanyCfopService _companyCfopService;

        public Import(ICompanyCfopService companyCfopService)
        {
            _companyCfopService = companyCfopService;
        }

        public Import() { }

        public List<List<Dictionary<string, string>>> Nfe(string directoryNfe, string directotyCte)
        {
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                string[] archivesNfes = Directory.GetFiles(directoryNfe);
                foreach (var arquivo in archivesNfes) 
                {                         
                    //Task.Factory.StartNew(() => {
                    if (new FileInfo(arquivo).Length != 0 && arquivo.Contains(".xml"))
                    {
                        Dictionary<string, string> infNFe = new Dictionary<string, string>();
                        Dictionary<string, string> ide = new Dictionary<string, string>();
                        Dictionary<string, string> emit = new Dictionary<string, string>();
                        Dictionary<string, string> dest = new Dictionary<string, string>();

                        List<Dictionary<string, string>> nota = new List<Dictionary<string, string>>();
                        StreamReader sr = new StreamReader(arquivo, Encoding.GetEncoding("ISO-8859-1"));
                        using (XmlReader reader = XmlReader.Create(sr))
                        {
                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {
                                        case "infNFe":
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "Id")
                                                {
                                                    infNFe.Add("chave", reader.Value.Substring(3, 44));
                                                }
                                            }
                                            nota.Add(infNFe);
                                            break;

                                        case "ide":
                                            reader.Read();
                                            while (reader.Name != "ide" && reader.Name != "NFref")
                                            {
                                                ide.Add(reader.Name, reader.ReadString());
                                                reader.Read();
                                            }
                                            nota.Add(ide);
                                            break;


                                        case "emit":
                                            reader.Read();
                                            while (reader.Name.ToString() != "emit")
                                            {
                                                if (reader.Name.ToString() != "enderEmit")
                                                {
                                                    emit.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            nota.Add(emit);
                                            break;

                                        case "dest":
                                            reader.Read();
                                            while (reader.Name.ToString() != "dest")
                                            {
                                                if (reader.Name.ToString() != "enderDest")
                                                {
                                                    dest.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            nota.Add(dest);
                                            break;

                                        case "ICMSTot":
                                            Dictionary<string, string> total = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name.ToString() != "ICMSTot")
                                            {
                                                total.Add(reader.Name, reader.ReadString());
                                                    
                                                reader.Read();

                                            }
                                            nota.Add(total);
                                            break;
                                    }
                                }
                            }
                        reader.Close();
                        sr.Close();
                        }

                        StreamReader sr2 = new StreamReader(arquivo, Encoding.GetEncoding("UTF-8"));
                        using (XmlReader reader = XmlReader.Create(sr2))
                        {
                            decimal base_calc = 0;
                            //var code = nota[2]["CNPJ"];
                            string nCT = "";
                            int nItem = 1;

                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {
                                        case "prod":

                                            if (nItem > 1)
                                            {
                                                Dictionary<string, string> baseCalc = new Dictionary<string, string>();
                                                baseCalc.Add("baseCalc", base_calc.ToString());
                                                nota.Add(baseCalc);
                                                base_calc = 0;
                                                //code = nota[2]["CNPJ"];
                                            }
                                            Dictionary<string, string> prod = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name.ToString() != "prod")
                                            {
                                                if (reader.Name == "cProd" || reader.Name == "cEAN" || reader.Name == "xProd" ||
                                                    reader.Name == "NCM" || reader.Name == "CEST" || reader.Name == "indEscala" ||
                                                    reader.Name == "CNPJFab" || reader.Name == "cBenef" || reader.Name == "EXTIPI" ||
                                                    reader.Name == "CFOP" || reader.Name == "uCom" || reader.Name == "qCom" ||
                                                    reader.Name == "vUnCom" || reader.Name == "vProd" || reader.Name == "cEANTrib" ||
                                                    reader.Name == "uTrib" || reader.Name == "qTrib" || reader.Name == "vUnTrib" ||
                                                    reader.Name == "vFrete" || reader.Name == "vSeg" || reader.Name == "vDesc" ||
                                                    reader.Name == "vOutro" || reader.Name == "intTot" || reader.Name == "xPed" ||
                                                    reader.Name == "nItemPed" || reader.Name == "vTotTrib" || reader.Name == "Nfci" ||
                                                    reader.Name == "nRECOPI")
                                                {

                                                    prod.Add(reader.Name, reader.ReadString());
                                                }

                                                reader.Read();

                                            }

                                            decimal valor_carga = 0;
                                            decimal valor_prestado = 0;
                                            decimal total_da_nota = Convert.ToDecimal(nota[4]["vNF"]);
                                            decimal total_dos_produtos = Convert.ToDecimal(nota[4]["vProd"]);
                                            decimal proporcao = 0;
                                            decimal frete_nota = 0;
                                            decimal total_icms_frete = 0;
                                            decimal frete_icms = 0;
                                            string nCT_temp = "";

                                            List<List<Dictionary<string, string>>> ctes = new List<List<Dictionary<string, string>>>();
                                            ctes = Cte(directotyCte, nota[3]["CNPJ"]);
                                            foreach (var item in ctes)
                                            {
                                                for (int j = 0; j < item.Count; j++)
                                                {
                                                    if (item[j].ContainsKey("vCarga"))
                                                    {
                                                        valor_carga = Convert.ToDecimal(item[j]["vCarga"]);
                                                    }
                                                    if (item[j].ContainsKey("vTPrest"))
                                                    {
                                                        valor_prestado = Convert.ToDecimal(item[j]["vTPrest"]);
                                                    }
                                                    if (item[j].ContainsKey("vICMS"))
                                                    {
                                                        total_icms_frete = Convert.ToDecimal(item[j]["vICMS"]);

                                                    }
                                                    if (item[j].ContainsKey("nCT"))
                                                    {
                                                        nCT_temp = item[j]["nCT"];
                                                    }
                                                    if (item[j].ContainsKey("chave"))
                                                    {
                                                        if (item[j]["chave"] == nota[0]["chave"])
                                                        {
                                                            nCT = nCT_temp;
                                                            proporcao = ((100 * total_da_nota) / valor_carga) / 100;
                                                            frete_nota = proporcao * valor_prestado;
                                                            frete_icms = proporcao * total_icms_frete;
                                                        }
                                                    }
                                                }
                                            }


                                            decimal proporcao_prod = 0;
                                            decimal frete_prod = 0;
                                            decimal proporcao_icms = 0;
                                            decimal frete_icmsprod = 0;
                                            if (frete_nota > 0)
                                            {
                                                proporcao_prod = ((100 * Convert.ToDecimal(prod["vProd"])) / total_dos_produtos) / 100;
                                                frete_prod = proporcao_prod * frete_nota;
                                            }

                                            if (frete_icms > 0)
                                            {
                                                proporcao_icms = ((100 * Convert.ToDecimal(prod["vProd"])) / total_dos_produtos) / 100;
                                                frete_icmsprod = proporcao_icms * frete_icms;
                                            }


                                            if (prod.ContainsKey("vProd"))
                                            {
                                                base_calc = Convert.ToDecimal(prod["vProd"]);
                                            }

                                            if (prod.ContainsKey("vOutro"))
                                            {
                                                base_calc += Convert.ToDecimal(prod["vOutro"]);
                                            }

                                            if (prod.ContainsKey("vFrete"))
                                            {
                                                base_calc += Convert.ToDecimal(prod["vFrete"]);
                                            }

                                            if (prod.ContainsKey("vSeg"))
                                            {
                                                base_calc += Convert.ToDecimal(prod["vSeg"]);
                                            }

                                            if (prod.ContainsKey("vDesc"))
                                            {
                                                base_calc -= Convert.ToDecimal(prod["vDesc"]);
                                            }

                                            base_calc += frete_prod;
                                            string CEST = "";
                                            string NCM = "";

                                            if (prod.ContainsKey("NCM"))
                                            {
                                                NCM = prod["NCM"];
                                            }

                                            if (prod.ContainsKey("CEST"))
                                            {
                                                CEST = prod["CEST"];
                                            }

                                            prod.Add("frete_prod", frete_prod.ToString());
                                            prod.Add("frete_icms", frete_icmsprod.ToString());
                                            prod.Add("nItem", Convert.ToString(nItem));
                                            nItem++;
                                            //code += (prod["cProd"] + NCM + CEST);
                                            nota.Add(prod);

                                            break;

                                        case "ICMS00":
                                        case "ICMS10":
                                        case "ICMS20":
                                        case "ICMS30":
                                        case "ICMS40":
                                        case "ICMS51":
                                        case "ICMS60":
                                        case "ICMS70":
                                        case "ICMS90":
                                        case "ICMSPart":
                                        case "ICMSST":
                                        case "ICMSSN101":
                                        case "ICMSSN102":
                                        case "ICMSSN201":
                                        case "ICMSSN202":
                                        case "ICMSSN500":
                                        case "ICMSSN900":
                                            Dictionary<string, string> icms = new Dictionary<string, string>();
                                            while (reader.Name != "ICMS")
                                            {
                                                if (reader.Name == "orig" || reader.Name == "CST" || reader.Name == "modBC" || reader.Name == "vBC" ||
                                                    reader.Name == "pICMS" || reader.Name == "vICMS" || reader.Name == "vBCST" || reader.Name == "vICMSST" ||
                                                    reader.Name == "vICMSSTRet" || reader.Name == "vBCFCPST" || reader.Name == "vBCFCPSTRet" || reader.Name == "pFCPST" ||
                                                    reader.Name == "pFCPSTRet" || reader.Name == "vFCPST" || reader.Name == "vFCPSTRet")
                                                {
                                                    icms.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            nota.Add(icms);
                                            break;

                                        case "IPI":
                                            Dictionary<string, string> ipi = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name != "IPI")
                                            {
                                                if (reader.Name == "cEnq" || reader.Name == "CST" || reader.Name == "vBC" ||
                                                reader.Name == "pIPI" || reader.Name == "vIPI")
                                                {
                                                    ipi.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }

                                            if (ipi.ContainsKey("vIPI"))
                                            {
                                                base_calc += Convert.ToDecimal(ipi["vIPI"]);
                                            }
                                            nota.Add(ipi);
                                            break;


                                        case "PIS":
                                            Dictionary<string, string> pis = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name != "PIS")
                                            {
                                                if (reader.Name != "PISAliq" && reader.Name != "PISQtde" && reader.Name != "PISNT" && reader.Name != "PISOutr")
                                                {
                                                    pis.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            nota.Add(pis);
                                            break;

                                        case "COFINS":
                                            Dictionary<string, string> cofins = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name != "COFINS")
                                            {
                                                if (reader.Name != "COFINSAliq" && reader.Name != "COFINSQtde" && reader.Name != "COFINSNT" && reader.Name != "COFINSOutr")
                                                {
                                                    cofins.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            nota.Add(cofins);
                                            break;

                                        case "total":

                                            Dictionary<string, string> baseCal = new Dictionary<string, string>();
                                            baseCal.Add("baseCalc", base_calc.ToString());
                                            nota.Add(baseCal);
                                            Dictionary<string, string> NCte = new Dictionary<string, string>();
                                            NCte.Add("nCT", nCT);
                                            nota.Add(NCte);
                                            break;
                                    }
                                }
                            }
                        reader.Close();
                        sr2.Close();
                        }
                        notes.Add(nota);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            return notes;
        }

        public List<List<Dictionary<string, string>>> Cte(string directory, string cnpj)
        {
            List<List<Dictionary<string, string>>> ctes = new List<List<Dictionary<string, string>>>();        

            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                string[] archivesCtes = Directory.GetFiles(directory);

                for (int i = 0; i < archivesCtes.Count(); i++)
                {
                    if (new FileInfo(archivesCtes[i]).Length != 0)
                    {
                        List<Dictionary<string, string>> cte = new List<Dictionary<string, string>>();

                        Dictionary<string, string> infCte = new Dictionary<string, string>();
                        Dictionary<string, string> toma = new Dictionary<string, string>();
                        Dictionary<string, string> emit = new Dictionary<string, string>();
                        Dictionary<string, string> dest = new Dictionary<string, string>();
                        Dictionary<string, string> receb = new Dictionary<string, string>();
                        Dictionary<string, string> vPrest = new Dictionary<string, string>();
                        Dictionary<string, string> infCarga = new Dictionary<string, string>();
                        Dictionary<string, string> ide = new Dictionary<string, string>();
                        StreamReader ct = new StreamReader(archivesCtes[i], Encoding.GetEncoding("ISO-8859-1"));
                        using (XmlReader reader = XmlReader.Create(ct))
                        {

                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {
                                        case "infCte":
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "Id")
                                                {
                                                    infCte.Add("chave_cte", reader.Value.Substring(3, 44));
                                                }
                                            }
                                            cte.Add(infCte);
                                            break;

                                        case "ide":
                                            reader.Read();
                                            while (reader.Name != "toma4" && reader.Name != "toma3")
                                            {
                                                if (reader.Name != "") {
                                                    ide.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            cte.Add(ide);
                                            break;

                                        case "toma":
                                            toma.Add(reader.Name, reader.ReadString());
                                            cte.Add(toma);
                                            break;

                                        case "emit":
                                            reader.Read();
                                            while (reader.Name.ToString() != "emit")
                                            {
                                                if (reader.Name.ToString() != "enderEmit" && reader.Name != "")
                                                {
                                                    emit.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            cte.Add(emit);
                                            break;

                                        case "dest":
                                            reader.Read();
                                            while (reader.Name.ToString() != "dest")
                                            {
                                                if (reader.Name.ToString() != "enderDest" && reader.Name != "")
                                                {
                                                    dest.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            cte.Add(dest);
                                            break;

                                        case "receb":
                                            reader.Read();
                                            while (reader.Name.Equals(""))
                                            {
                                                reader.Read();
                                            }
                                            receb.Add("cnpjreceb", reader.ReadString());
                                            cte.Add(receb);
                                            break;

                                        case "vTPrest":
                                            vPrest.Add(reader.Name, reader.ReadString());
                                            cte.Add(vPrest);
                                            break;

                                        case "vCarga":
                                            infCarga.Add(reader.Name, reader.ReadString());
                                            cte.Add(infCarga);
                                            break;

                                        case "ICMS00":
                                        case "ICMS20":
                                        case "ICMS45":
                                        case "ICMS60":
                                        case "ICMS90":
                                        case "ICMSOutraUF":
                                        case "ICMSSN":
                                            Dictionary<string, string> icms = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name != "ICMS")
                                            {

                                                if (reader.NodeType.ToString() != "EndElement" && reader.Name != "")
                                                {
                                                    icms.Add(reader.Name, reader.ReadString());
                                                }

                                                reader.Read();
                                            }
                                            cte.Add(icms);
                                            break;

                                        case "infNFe":
                                            reader.Read();

                                            while (reader.Name != "infNFe") {
                                                if (reader.Name != "") {
                                                    Dictionary<string, string> infNFe = new Dictionary<string, string>();
                                                    infNFe.Add(reader.Name, reader.ReadString());
                                                    cte.Add(infNFe);
                                                }
                                                reader.Read();
                                            }
                                            
                                            break;

                                        case "infCteComp":
                                            reader.Read();
                                            Dictionary<string, string> chCte = new Dictionary<string, string>();
                                            chCte.Add(reader.Name, reader.ReadString());
                                            cte.Add(chCte);
                                            break;
                                    }
                                }

                            }
                            reader.Close();
                            ct.Close();
                        }
                        ctes.Add(cte);
                    }
                }

                double valor_comp = 0;
                double valor_icms_comp = 0;
                foreach (var item in ctes)
                {
                    for (int i = 0; i < item.Count; i++)
                    {
                        if (item[i].ContainsKey("vTPrest"))
                        {
                            valor_comp = Convert.ToDouble(item[i]["vTPrest"]);
                        }
                        if (item[i].ContainsKey("vICMS"))
                        {
                            valor_icms_comp = Convert.ToDouble(item[i]["vICMS"]);
                        }
                        if (item[i].ContainsKey("chCTe"))
                        {
                            foreach (var item2 in ctes)
                            {
                                bool cte_complementar = false;
                                for (int j = 0; j < item2.Count; j++)
                                {
                                    if (item2[j].ContainsKey("chave_cte"))
                                    {
                                        if (item[i]["chCTe"] == item2[j]["chave_cte"])
                                        {
                                            cte_complementar = true;
                                        }
                                    }
                                    if (item2[j].ContainsKey("vTPrest") && cte_complementar == true)
                                    {
                                        double valorDaNota = Convert.ToDouble(item2[j]["vTPrest"]);
                                        double valorTotal = valor_comp + valorDaNota;
                                        item2[j]["vTPrest"] = valorTotal.ToString();
                                    }
                                    if (item2[j].ContainsKey("vICMS") && cte_complementar == true)
                                    {
                                        double valor_icms = Convert.ToDouble(item2[j]["vICMS"]);
                                        double valorTotalIcms = valor_icms_comp + valor_icms;
                                        item2[j]["vICMS"] = valorTotalIcms.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
                string recebedor = "";
                for (int i = ctes.Count - 1; i >= 0; i--)
                {
                    for (int j = 0; j < ctes[i].Count(); j++)
                    {
                        if (ctes[i][j].ContainsKey("cnpjreceb"))
                        {
                            recebedor = ctes[i][j]["cnpjreceb"];
                            break;
                        }
                    }
                    for (int j = 0; j < ctes[i].Count(); j++)
                    {
                        if (ctes[i][j].ContainsKey("toma"))
                        {
                            
                            if(ctes[i][j]["toma"] != "3")
                            {
                                if (ctes[i][j]["toma"] == "2")
                                {
                                    if (cnpj != recebedor)
                                    {
                                        ctes.RemoveAt(i);
                                    }    
                                }
                                else
                                {
                                    ctes.RemoveAt(i);
                                }
                            }
                            break;
                        }
                    }
                    recebedor = "";
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
       
            return ctes;
        }

        public List<List<Dictionary<string,string>>> NfeExit(string directoryNfe, int companyId, string type, string typeCfop)
        {
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                var cfops = _companyCfopService.FindByCfopActive(companyId, type, typeCfop).Select(_ => _.Cfop.Code);

                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && arquivo.Contains(".xml"))
                    {
                        bool cfop = true;
                        Dictionary<string, string> infNFe = new Dictionary<string, string>();
                        Dictionary<string, string> ide = new Dictionary<string, string>();
                        Dictionary<string, string> emit = new Dictionary<string, string>();
                        Dictionary<string, string> dest = new Dictionary<string, string>();
                        List<Dictionary<string, string>> nota = new List<Dictionary<string, string>>();
                        StreamReader sr = new StreamReader(arquivo, Encoding.GetEncoding("ISO-8859-1"));
                        using (XmlReader reader = XmlReader.Create(sr))
                        {
                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {
                                        case "infNFe":
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "Id")
                                                {
                                                    infNFe.Add("chave", reader.Value.Substring(3, 44));
                                                }
                                            }
                                            nota.Add(infNFe);
                                            break;

                                        case "ide":
                                            reader.Read();
                                            while (reader.Name != "ide" && reader.Name != "NFref")
                                            {
                                                ide.Add(reader.Name, reader.ReadString());
                                                reader.Read();
                                            }
                                            nota.Add(ide);
                                            break;


                                        case "emit":
                                            reader.Read();
                                            while (reader.Name.ToString() != "emit")
                                            {
                                                if (reader.Name.ToString() != "enderEmit")
                                                {
                                                    emit.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            nota.Add(emit);
                                            break;

                                        case "dest":
                                            reader.Read();
                                            while (reader.Name.ToString() != "dest")
                                            {
                                                if (reader.Name.ToString() != "enderDest")
                                                {
                                                    dest.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            nota.Add(dest);
                                            break;

                                        case "prod":

                                            Dictionary<string, string> prod = new Dictionary<string, string>();
                                            cfop = true;
                                            reader.Read();
                                            while (reader.Name.ToString() != "prod")
                                            {
                                                if (reader.Name == "cProd" || reader.Name == "cEAN" || reader.Name == "xProd" ||
                                                    reader.Name == "NCM" || reader.Name == "CEST" || reader.Name == "indEscala" ||
                                                    reader.Name == "CNPJFab" || reader.Name == "cBenef" || reader.Name == "EXTIPI" ||
                                                    reader.Name == "CFOP" || reader.Name == "uCom" || reader.Name == "qCom" ||
                                                    reader.Name == "vUnCom" || reader.Name == "vProd" || reader.Name == "cEANTrib" ||
                                                    reader.Name == "uTrib" || reader.Name == "qTrib" || reader.Name == "vUnTrib" ||
                                                    reader.Name == "vFrete" || reader.Name == "vSeg" || reader.Name == "vDesc" ||
                                                    reader.Name == "vOutro" || reader.Name == "intTot" || reader.Name == "xPed" ||
                                                    reader.Name == "nItemPed" || reader.Name == "vTotTrib" || reader.Name == "Nfci" ||
                                                    reader.Name == "nRECOPI")
                                                {

                                                    prod.Add(reader.Name, reader.ReadString());
                                                }

                                                reader.Read();

                                            }

                                            if (!cfops.Contains(prod["CFOP"]))
                                            {
                                                cfop = false;
                                            }

                                            if (cfop == true)
                                            {
                                                nota.Add(prod);
                                            }

                                            break;

                                        case "ICMS00":
                                        case "ICMS10":
                                        case "ICMS20":
                                        case "ICMS30":
                                        case "ICMS40":
                                        case "ICMS51":
                                        case "ICMS60":
                                        case "ICMS70":
                                        case "ICMS90":
                                        case "ICMSPart":
                                        case "ICMSST":
                                        case "ICMSSN101":
                                        case "ICMSSN102":
                                        case "ICMSSN201":
                                        case "ICMSSN202":
                                        case "ICMSSN500":
                                        case "ICMSSN900":
                                            Dictionary<string, string> icms = new Dictionary<string, string>();
                                            while (reader.Name != "ICMS")
                                            {
                                                if (reader.Name == "orig" || reader.Name == "CST" || reader.Name == "modBC" || reader.Name == "vBC" ||
                                                    reader.Name == "pICMS" || reader.Name == "pFCP" || reader.Name == "vICMS" || reader.Name == "vBCST" || reader.Name == "vICMSST" ||
                                                    reader.Name == "vICMSSTRet" || reader.Name == "vBCFCPST" || reader.Name == "vBCFCPSTRet" || reader.Name == "pFCPST" ||
                                                    reader.Name == "pFCPSTRet" || reader.Name == "vFCPST" || reader.Name == "vFCPSTRet")
                                                {
                                                    icms.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            if (cfop == true)
                                            {
                                                nota.Add(icms);
                                            }

                                            break;

                                        case "IPI":
                                            Dictionary<string, string> ipi = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name != "IPI")
                                            {
                                                if (reader.Name == "cEnq" || reader.Name == "CST" || reader.Name == "vBC" ||
                                                reader.Name == "pIPI" || reader.Name == "vIPI")
                                                {
                                                    ipi.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            if (cfop == true)
                                            {
                                                nota.Add(ipi);
                                            }

                                            break;

                                        case "PIS":
                                            Dictionary<string, string> pis = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name != "PIS")
                                            {
                                                if (reader.Name != "PISAliq" && reader.Name != "PISQtde" && reader.Name != "PISNT" && reader.Name != "PISOutr")
                                                {
                                                    pis.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            if (cfop == true)
                                            {
                                                nota.Add(pis);
                                            }

                                            break;

                                        case "COFINS":
                                            Dictionary<string, string> cofins = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name != "COFINS")
                                            {
                                                if (reader.Name != "COFINSAliq" && reader.Name != "COFINSQtde" && reader.Name != "COFINSNT" && reader.Name != "COFINSOutr")
                                                {
                                                    cofins.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            if (cfop == true)
                                            {
                                                nota.Add(cofins);
                                            }

                                            break;

                                        case "ICMSTot":
                                            Dictionary<string, string> total = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name.ToString() != "ICMSTot")
                                            {
                                                total.Add(reader.Name, reader.ReadString());

                                                reader.Read();

                                            }
                                            nota.Add(total);
                                            break;
                                    }
                                }
                            }
                            reader.Close();
                            sr.Close();
                        }
                        notes.Add(nota);
                        
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return notes;
        }

        public List<Dictionary<string, string>> Client(string directoryNfe)
        {
            List<Dictionary<string, string>> dets = new List<Dictionary<string, string>>();
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);


                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && arquivo.Contains(".xml"))
                    {
                        Dictionary<string, string> dest = new Dictionary<string, string>();
                        
                        StreamReader sr = new StreamReader(arquivo, Encoding.GetEncoding("ISO-8859-1"));
                        using (XmlReader reader = XmlReader.Create(sr))
                        {
                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {
                                        case "dest":
                                            reader.Read();
                                            while (reader.Name.ToString() != "dest")
                                            {
                                                if (reader.Name.ToString() != "enderDest")
                                                {
                                                    dest.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            dets.Add(dest);
                                            break;
                                    }
                                }
                            }
                            reader.Close();
                            sr.Close();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return dets;
        }

        public List<List<Dictionary<string, string>>> NotesRelatoryIcms(string directoryNfe)
        {
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);


                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && arquivo.Contains(".xml"))
                    {                       
                        List<Dictionary<string, string>> nota = new List<Dictionary<string, string>>();

                        StreamReader sr = new StreamReader(arquivo, Encoding.GetEncoding("ISO-8859-1"));
                        using (XmlReader reader = XmlReader.Create(sr))
                        {
                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {
                                        case "dest":
                                            Dictionary<string, string> dest = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name.ToString() != "dest")
                                            {
                                                if (reader.Name.ToString() != "enderDest")
                                                {
                                                    dest.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            nota.Add(dest);
                                            break;
                                        case "ICMSTot":
                                            Dictionary<string, string> total = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name.ToString() != "ICMSTot")
                                            {
                                                total.Add(reader.Name, reader.ReadString());

                                                reader.Read();

                                            }
                                            nota.Add(total);
                                            break;
                                    }
                                }
                            }
                            reader.Close();
                            sr.Close();
                        }
                        notes.Add(nota);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return notes;
        }

        public List<List<Dictionary<string, string>>> NotesTransfer(string directoryNfe, int companyId)
        {
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && arquivo.Contains(".xml"))
                    {
                        Dictionary<string, string> infNFe = new Dictionary<string, string>();
                        Dictionary<string, string> ide = new Dictionary<string, string>();
                        Dictionary<string, string> emit = new Dictionary<string, string>();
                        Dictionary<string, string> dest = new Dictionary<string, string>();
                        List<Dictionary<string, string>> nota = new List<Dictionary<string, string>>();
                        StreamReader sr = new StreamReader(arquivo, Encoding.GetEncoding("ISO-8859-1"));
                        using (XmlReader reader = XmlReader.Create(sr))
                        {
                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {
                                        case "infNFe":
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "Id")
                                                {
                                                    infNFe.Add("chave", reader.Value.Substring(3, 44));
                                                }
                                            }
                                            nota.Add(infNFe);
                                            break;

                                        case "ide":
                                            reader.Read();
                                            while (reader.Name != "ide" && reader.Name != "NFref")
                                            {
                                                ide.Add(reader.Name, reader.ReadString());
                                                reader.Read();
                                            }
                                            nota.Add(ide);
                                            break;


                                        case "emit":
                                            reader.Read();
                                            while (reader.Name.ToString() != "emit")
                                            {
                                                if (reader.Name.ToString() != "enderEmit")
                                                {
                                                    emit.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            nota.Add(emit);
                                            break;

                                        case "dest":
                                            reader.Read();
                                            while (reader.Name.ToString() != "dest")
                                            {
                                                if (reader.Name.ToString() != "enderDest")
                                                {
                                                    dest.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            nota.Add(dest);
                                            break;

                                        case "prod":

                                            Dictionary<string, string> prod = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name.ToString() != "prod")
                                            {
                                                if (reader.Name == "cProd" || reader.Name == "cEAN" || reader.Name == "xProd" ||
                                                    reader.Name == "NCM" || reader.Name == "CEST" || reader.Name == "indEscala" ||
                                                    reader.Name == "CNPJFab" || reader.Name == "cBenef" || reader.Name == "EXTIPI" ||
                                                    reader.Name == "CFOP" || reader.Name == "uCom" || reader.Name == "qCom" ||
                                                    reader.Name == "vUnCom" || reader.Name == "vProd" || reader.Name == "cEANTrib" ||
                                                    reader.Name == "uTrib" || reader.Name == "qTrib" || reader.Name == "vUnTrib" ||
                                                    reader.Name == "vFrete" || reader.Name == "vSeg" || reader.Name == "vDesc" ||
                                                    reader.Name == "vOutro" || reader.Name == "intTot" || reader.Name == "xPed" ||
                                                    reader.Name == "nItemPed" || reader.Name == "vTotTrib" || reader.Name == "Nfci" ||
                                                    reader.Name == "nRECOPI")
                                                {

                                                    prod.Add(reader.Name, reader.ReadString());
                                                }

                                                reader.Read();

                                            }
                                            nota.Add(prod);
                                            break;

                                        case "ICMS00":
                                        case "ICMS10":
                                        case "ICMS20":
                                        case "ICMS30":
                                        case "ICMS40":
                                        case "ICMS51":
                                        case "ICMS60":
                                        case "ICMS70":
                                        case "ICMS90":
                                        case "ICMSPart":
                                        case "ICMSST":
                                        case "ICMSSN101":
                                        case "ICMSSN102":
                                        case "ICMSSN201":
                                        case "ICMSSN202":
                                        case "ICMSSN500":
                                        case "ICMSSN900":
                                            Dictionary<string, string> icms = new Dictionary<string, string>();
                                            while (reader.Name != "ICMS")
                                            {
                                                if (reader.Name == "orig" || reader.Name == "CST" || reader.Name == "modBC" || reader.Name == "vBC" ||
                                                    reader.Name == "pICMS" || reader.Name == "pFCP" || reader.Name == "vICMS" || reader.Name == "vBCST" || reader.Name == "vICMSST" ||
                                                    reader.Name == "vICMSSTRet" || reader.Name == "vBCFCPST" || reader.Name == "vBCFCPSTRet" || reader.Name == "pFCPST" ||
                                                    reader.Name == "pFCPSTRet" || reader.Name == "vFCPST" || reader.Name == "vFCPSTRet")
                                                {
                                                    icms.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            break;

                                        case "IPI":
                                            Dictionary<string, string> ipi = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name != "IPI")
                                            {
                                                if (reader.Name == "cEnq" || reader.Name == "CST" || reader.Name == "vBC" ||
                                                reader.Name == "pIPI" || reader.Name == "vIPI")
                                                {
                                                    ipi.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }

                                            break;

                                        case "PIS":
                                            Dictionary<string, string> pis = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name != "PIS")
                                            {
                                                if (reader.Name != "PISAliq" && reader.Name != "PISQtde" && reader.Name != "PISNT" && reader.Name != "PISOutr")
                                                {
                                                    pis.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }

                                            break;

                                        case "COFINS":
                                            Dictionary<string, string> cofins = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name != "COFINS")
                                            {
                                                if (reader.Name != "COFINSAliq" && reader.Name != "COFINSQtde" && reader.Name != "COFINSNT" && reader.Name != "COFINSOutr")
                                                {
                                                    cofins.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }

                                            break;

                                        case "ICMSTot":
                                            Dictionary<string, string> total = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name.ToString() != "ICMSTot")
                                            {
                                                total.Add(reader.Name, reader.ReadString());

                                                reader.Read();

                                            }
                                            nota.Add(total);
                                            break;
                                    }
                                }
                            }
                            reader.Close();
                            sr.Close();
                        }
                        notes.Add(nota);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return notes;
        }

        public List<string> FindByNcms(string directoryNfe)
        {
            List<string> ncms = new List<string>();
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && arquivo.Contains(".xml"))
                    {
                        StreamReader sr = new StreamReader(arquivo, Encoding.GetEncoding("ISO-8859-1"));
                        using (XmlReader reader = XmlReader.Create(sr))
                        {
                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {

                                        case "prod":

                                            Dictionary<string, string> prod = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name.ToString() != "prod")
                                            {
                                                if (reader.Name == "NCM")
                                                {
                                                    bool status = false;
                                                    string ncmTemp = reader.ReadString();
                                                    foreach (var ncm in ncms)
                                                    {
                                                        if (ncm.Equals(ncmTemp))
                                                        {
                                                            status = true;
                                                            break;
                                                        }
                                                    }
                                                    if(status == false)
                                                    {
                                                        ncms.Add(ncmTemp);
                                                    }
                                                   
                                                    
                                                }

                                                reader.Read();

                                            }
                                            break;

                                    }
                                }
                            }
                            reader.Close();
                            sr.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return ncms;
        }


    }
}
