using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Routing;

namespace Escon.SisctNET.Web.Taxation
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
        public List<List<string>> FindByNcms(string directoryNfe)
        {
            List<List<string>> ncms = new List<List<string>>();
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

                                            List<string> prod = new List<string>();
                                            reader.Read();
                                            string ncm = "", code = "";
                                            bool status = false;
                                            while (reader.Name.ToString() != "prod")
                                            {
                                                if (reader.Name == "cProd")
                                                {
                                                    code = reader.ReadString();
                                                }
                                                if (reader.Name == "NCM")
                                                {
                                                    ncm = reader.ReadString();
                                                }

                                                reader.Read();

                                            }

                                            
                                            for(int e = 0; e < ncms.Count(); e++)
                                            {
                                                if (ncms[e][0].Equals(code) && ncms[e][1].Equals(ncm))
                                                {
                                                    status = true;
                                                }
                                            }

                                            if (ncms.Count == 0 || status.Equals(false))
                                            {
                                                prod.Add(code);
                                                prod.Add(ncm);
                                                ncms.Add(prod);
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

        public decimal SpedCredito(string directorySped, int companyId)
        {
            decimal totalDeCredito = 0;
            StreamReader archiveSped = new StreamReader(directorySped);
            var cfopsCompra = _companyCfopService.FindByCfopActive(companyId, "entrada", "compra").Select(_ => _.Cfop.Code).ToList();
            var cfopsDevo = _companyCfopService.FindByCfopActive(companyId, "entrada", "devolução de venda").Select(_ => _.Cfop.Code).ToList();
            string line;
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
                    else if(linha[1].Equals("D100") && !linha[17].Equals("1"))
                    {
                        fob = false;
                    }
                    if (fob.Equals(true) && cfopsCompra.Contains(linha[3]) && linha[1].Equals("D190") && linha[7] != "")
                    {
                        totalDeCredito += Convert.ToDecimal(linha[7]);
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

        public List<List<Dictionary<string, string>>> NfeExit(string directoryNfe, int companyId, string type, string typeCfop)
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

        public List<Dictionary<string, string>> NfeExitProducts(string directoryNfe)
        {
            List<Dictionary<string, string>> products = new List<Dictionary<string, string>>();
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

                                            products.Add(prod);
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

            return products;
        }

        public List<string> Sped(string directorySped,string directoryNfe)
        {
            List<string> sped = new List<string>();
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
            List<List<string>> cfops = new List<List<string>>();

            decimal valorNota = 0, ipiNota = 0, descontoNota = 0, outrasDespesasNota = 0, freteNota = 0, seguroNota = 0, icmsRetidoST = 0, valorNotaNF = 0;
            int posC100 = -1;
            string line, tipoOperacao = "", chave = "" , emissao = "";
            bool diferenca = false;

            notes = Nfe(directoryNfe);
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

                        if(!linha[16].Equals(""))
                        {
                            valorNota = Convert.ToDecimal(linha[16].Replace(",","."));
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

                        if(posC100 >= 0)
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

                        if(posC100 >= 0)
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

                        if(posC100 >= 0)
                        {
                            if (!descontoNota.Equals(0) || !freteNota.Equals(0) || !seguroNota.Equals(0) || !outrasDespesasNota.Equals(0) || !ipiNota.Equals(0) || !icmsRetidoST.Equals(0) || !valorNota.Equals(valorNotaNF) || diferenca.Equals(true))
                            {
                                decimal valorProduto = 0;
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
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vProd"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vFrete") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vFrete"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vDesc") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])))
                                            {
                                                valorProduto -= Convert.ToDecimal(notes[i][j]["vDesc"]);
                                            }
                                        }


                                        if (notes[i][j].ContainsKey("vOutro") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vOutro"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vSeg") && notes[i][j].ContainsKey("cProd") && notes[i][j].ContainsKey("uCom") && notes[i][j].ContainsKey("qCom"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(Convert.ToInt32(notes[i][j]["nItem"])))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vSeg"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("pIPI"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(nItem))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vIPI"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vICMSST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(nItem))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vICMSST"]);
                                            }
                                        }

                                        if (notes[i][j].ContainsKey("vFCPST") && notes[i][j].ContainsKey("orig"))
                                        {
                                            if (Convert.ToInt32(linha[2]).Equals(nItem))
                                            {
                                                valorProduto += Convert.ToDecimal(notes[i][j]["vFCPST"]);
                                            }
                                        }
                                    }

                                }

                                valorProduto = Math.Round(valorProduto, 2);
                                cfops[posCfop][1] = (Math.Round(Convert.ToDecimal(cfops[posCfop][1]) + valorProduto, 2)).ToString();
                                textoC170 += "|" + linha[1] + "|" + linha[2] + "|" + linha[3] + "|" + linha[4] + "|" + linha[5] + "|" + linha[6] + "|" + valorProduto.ToString().Replace(".", ",") + "|" + "" + "|"
                                    + linha[9] + "|" + linha[10] + "|" + linha[11] + "|" + linha[12] + "|" + linha[13] + "|" + linha[14] + "|" + linha[15] + "|" + "" + "|" + "" + "|"
                                    + "" + "|" + linha[19] + "|" + linha[20] + "|" + linha[21] + "|" + linha[22] + "|" + "" + "|" + "" + "|" + linha[25] + "|" + linha[26] + "|"
                                    + linha[27] + "|" + linha[28] + "|" + linha[29] + "|" + linha[30] + "|" + linha[31] + "|" + linha[32] + "|" + linha[33] + "|" + linha[34] + "|" + linha[35] + "|"
                                    + linha[36] + "|" + linha[37] + "|" + linha[38] + "|";
                                sped.Add(textoC170);
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
                        if (posC100 >= 0)
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
                        }
                       

                    }

                    if (!linha[1].Equals("C100") && !linha[1].Equals("C170") && !linha[1].Equals("C190"))
                    {
                        linha = line.TrimEnd('|').Split('|');
                        string texto = "";
                        foreach(var l in linha)
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
            return sped;
        }

        public List<List<Dictionary<string, string>>> Nfe(string directoryNfe)
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
                            int nItem = 1;
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
                                                    nItem = 1;
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
                                            prod.Add("nItem", nItem.ToString());
                                            nItem++;
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

        public List<List<Dictionary<string, string>>> NfeExit(string directoryNfe, string codeCfop)
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

                                            if (!prod["CFOP"].Equals(codeCfop))
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

        public List<List<Dictionary<string, string>>> NfeExit(string directoryNfe,int companyId, int typeCompany)
        {
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
            try
            {
                var cfops = _companyCfopService.FindByCfopActive(companyId, "resumoncm", "venda").Select(_ => _.Cfop.Code);
                var prods = _taxationNcmService.FindMono(typeCompany).Select(_ => _.CodeProduct);

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);


                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && arquivo.Contains(".xml"))
                    {
                        bool cfop = true;
                        bool produto = true;
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
                                            produto = true;
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

                                            if (!cfops.Contains(prod["CFOP"]) && !prods.Contains(prod["cProd"]))
                                            {
                                                cfop = false;
                                                produto = false;
                                            }

                                            if (cfop == true && produto == true)
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
                                            if (cfop == true && produto == true)
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
                                            if (cfop == true && produto == true)
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
                                            if (cfop == true && produto == true)
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
                                            if (cfop == true && produto == true)
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

    }
}
