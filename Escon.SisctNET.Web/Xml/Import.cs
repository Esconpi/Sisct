using Escon.SisctNET.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Escon.SisctNET.Web.Xml
{
    public class Import
    {
        private readonly ICfopService _cfopService;
        private readonly ITaxationNcmService _taxationNcmService;

        public Import(ICfopService cfopService)
        {
            _cfopService = cfopService;
        }

        public Import(
            ICfopService cfopService,
            ITaxationNcmService taxationNcmService)
        {
            _cfopService = cfopService;
            _taxationNcmService = taxationNcmService;
        }

        public Import() { }

        // NFe

        public List<Dictionary<string, string>> NFeClient(string directoryNfe)
        {
            List<Dictionary<string, string>> dets = new List<Dictionary<string, string>>();
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                bool infCte = false;

                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && (arquivo.Contains(".xml") || arquivo.Contains(".XML")) && (!arquivo.Contains(".lnk") || !arquivo.Contains(".lnk")))
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
                                        case "infCte":
                                            infCte = true;
                                            break;

                                        case "infNFe":
                                            infCte = false;
                                            break;

                                        case "dest":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "dest")
                                                {
                                                    if (reader.Name.ToString() != "enderDest")
                                                    {
                                                        dest.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                bool status = false;

                                                string CNPJ = dest.ContainsKey("CNPJ") ? dest["CNPJ"] : "";
                                                string CPF = dest.ContainsKey("CPF") ? dest["CPF"] : "";

                                                for (int e = 0; e < dets.Count(); e++)
                                                {
                                                    if (dets[e].ContainsKey("CNPJ"))
                                                    {
                                                        if (dets[e]["CNPJ"].Equals(CNPJ))
                                                        {
                                                            status = true;
                                                            break;
                                                        }
                                                    }

                                                    if (dets[e].ContainsKey("CPF"))
                                                    {
                                                        if (dets[e]["CPF"].Equals(CPF))
                                                        {
                                                            status = true;
                                                            break;
                                                        }
                                                    }

                                                }
                                                if (dets.Count() == 0 || status == false)
                                                {
                                                    dets.Add(dest);
                                                }
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

            return dets;
        }

        public List<Dictionary<string, string>> NFeProvider(string directoryNfe)
        {
            List<Dictionary<string, string>> dets = new List<Dictionary<string, string>>();
           
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                bool infCte = false;

                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && (arquivo.Contains(".xml") || arquivo.Contains(".XML")) && (!arquivo.Contains(".lnk") || !arquivo.Contains(".lnk")))
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
                                        case "infCte":
                                            infCte = true;
                                            break;

                                        case "infNFe":
                                            infCte = false;
                                            break;

                                        case "emit":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "emit")
                                                {
                                                    if (reader.Name.ToString() != "enderEmit")
                                                    {
                                                        dest.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                bool status = false;

                                                string CNPJ = dest.ContainsKey("CNPJ") ? dest["CNPJ"] : "";
                                                string CPF = dest.ContainsKey("CPF") ? dest["CPF"] : "";

                                                for (int e = 0; e < dets.Count(); e++)
                                                {
                                                    if (dets[e].ContainsKey("CNPJ"))
                                                    {
                                                        if (dets[e]["CNPJ"].Equals(CNPJ))
                                                        {
                                                            status = true;
                                                            break;
                                                        }
                                                    }

                                                    if (dets[e].ContainsKey("CPF"))
                                                    {
                                                        if (dets[e]["CPF"].Equals(CPF))
                                                        {
                                                            status = true;
                                                            break;
                                                        }
                                                    }

                                                }
                                                if (dets.Count() == 0 || status == false)
                                                {
                                                    dets.Add(dest);
                                                }
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

            return dets;
        }

        public List<Dictionary<string, string>> NFeProduct(string directoryNfe)
        {
            List<Dictionary<string, string>> products = new List<Dictionary<string, string>>();
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                bool infCte = false;

                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && (arquivo.Contains(".xml") || arquivo.Contains(".XML")) && (!arquivo.Contains(".lnk") || !arquivo.Contains(".lnk")))
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
                                        case "infCte":
                                            infCte = true;
                                            break;

                                        case "infNFe":
                                            infCte = false;
                                            break;

                                        case "prod":

                                            Dictionary<string, string> prod = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
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

                                                bool status = false;

                                                string CEST = prod.ContainsKey("CEST") ? prod["CEST"] : "";

                                                for (int e = 0; e < products.Count(); e++)
                                                {
                                                    string cestTemp = products[e].ContainsKey("CEST") ? products[e]["CEST"] : "";

                                                    if (products[e]["cProd"].Equals(prod["cProd"]) && products[e]["NCM"].Equals(prod["NCM"]) && CEST.Equals(cestTemp))
                                                    {
                                                        status = true;
                                                        break;
                                                    }
                                                }

                                                if (products.Count == 0 || status.Equals(false))
                                                {
                                                    products.Add(prod);
                                                }
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

            return products;
        }

        public List<List<string>> NFeNCM(string directoryNfe)
        {
            List<List<string>> ncms = new List<List<string>>();
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                bool infCte = false;

                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && (arquivo.Contains(".xml") || arquivo.Contains(".XML")) && (!arquivo.Contains(".lnk") || !arquivo.Contains(".lnk")))
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
                                        case "infCte":
                                            infCte = true;
                                            break;

                                        case "infNFe":
                                            infCte = false;
                                            break;

                                        case "prod":

                                            List<string> prod = new List<string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                string ncm = "", code = "", produto = "";
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

                                                    if (reader.Name == "xProd")
                                                    {
                                                        produto = reader.ReadString();
                                                    }

                                                    reader.Read();

                                                }

                                                for (int e = 0; e < ncms.Count(); e++)
                                                {
                                                    if (ncms[e][0].Equals(code) && ncms[e][1].Equals(ncm))
                                                    {
                                                        status = true;
                                                        break;
                                                    }
                                                }

                                                if (ncms.Count == 0 || status.Equals(false))
                                                {
                                                    prod.Add(code);
                                                    prod.Add(ncm);
                                                    prod.Add(produto);
                                                    ncms.Add(prod);
                                                }
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

        public List<List<Dictionary<string, string>>> NFeAll(string directoryNfe)
        {
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                bool infCte = false;

                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && (arquivo.Contains(".xml") || arquivo.Contains(".XML")) && (!arquivo.Contains(".lnk") || !arquivo.Contains(".lnk")))
                    {
                        Dictionary<string, string> infNFe = new Dictionary<string, string>();
                        Dictionary<string, string> ide = new Dictionary<string, string>();
                        Dictionary<string, string> emit = new Dictionary<string, string>();
                        Dictionary<string, string> dest = new Dictionary<string, string>();
                        List<Dictionary<string, string>> nota = new List<Dictionary<string, string>>();
                        StreamReader sr = new StreamReader(arquivo, Encoding.GetEncoding("ISO-8859-1"));

                        string cStat = "100";
                        using (XmlReader reader = XmlReader.Create(sr))
                        {
                            int nItem = 1;
                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {
                                        case "infCte":
                                            infCte = true;
                                            break;

                                        case "infNFe":
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "Id")
                                                {
                                                    nItem = 1;
                                                    infNFe.Add("chave", reader.Value.Substring(3, 44));
                                                }
                                            }
                                            infCte = false;
                                            nota.Add(infNFe);
                                            break;

                                        case "ide":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name != "ide" && reader.Name != "NFref")
                                                {
                                                    ide.Add(reader.Name, reader.ReadString());
                                                    reader.Read();
                                                }
                                                nota.Add(ide);
                                            }
                                           
                                            break;


                                        case "emit":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "emit")
                                                {
                                                    if (reader.Name.ToString() != "enderEmit")
                                                    {
                                                        emit.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                nota.Add(emit);
                                            }
                                          
                                            break;

                                        case "dest":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "dest")
                                                {
                                                    if (reader.Name.ToString() != "enderDest")
                                                    {
                                                        dest.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                nota.Add(dest);
                                            }
                                          
                                            break;

                                        case "prod":

                                            Dictionary<string, string> prod = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
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
                                            if (!infCte)
                                            {
                                                while (reader.Name != "ICMS")
                                                {
                                                    if (reader.Name == "orig" || reader.Name == "CST" || reader.Name == "modBC" || reader.Name == "vBC" ||
                                                        reader.Name == "pICMS" || reader.Name == "pFCP" || reader.Name == "vFCP" || reader.Name == "vICMS" ||
                                                        reader.Name == "vBCST" || reader.Name == "vICMSST" || reader.Name == "vICMSSTRet" || reader.Name == "vBCFCPST" ||
                                                        reader.Name == "vBCFCPSTRet" || reader.Name == "pFCPST" || reader.Name == "pFCPSTRet" || reader.Name == "vFCPST" ||
                                                        reader.Name == "vFCPSTRet" || reader.Name == "CSOSN" || reader.Name == "pCredSN" || reader.Name == "vCredICMSSN" ||
                                                        reader.Name == "pRedBCEfet" || reader.Name == "vBCSTRet" || reader.Name == "pST" || reader.Name == "vICMSSubstituto" ||
                                                        reader.Name == "pICMSST" || reader.Name == "pMVAST" || reader.Name == "pRedBCST" || reader.Name == "pRedBC")
                                                    {
                                                        icms.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                nota.Add(icms);
                                            }
                                          
                                            break;

                                        case "IPI":
                                            Dictionary<string, string> ipi = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name != "IPI")
                                                {
                                                    if (reader.Name == "cEnq" || reader.Name == "CST" || reader.Name == "vBC" ||
                                                    reader.Name == "pIPI" || reader.Name == "vIPI")
                                                    {
                                                        if (reader.Name == "CST")
                                                        {
                                                            ipi.Add("CSTI", reader.ReadString());
                                                        }
                                                        else
                                                        {
                                                            ipi.Add(reader.Name, reader.ReadString());
                                                        }
                                                    }
                                                    reader.Read();
                                                }
                                                nota.Add(ipi);
                                            }
                                           
                                            break;

                                        case "PIS":
                                            Dictionary<string, string> pis = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name != "PIS")
                                                {
                                                    if (reader.Name != "PISAliq" && reader.Name != "PISQtde" && reader.Name != "PISNT" && reader.Name != "PISOutr")
                                                    {
                                                        if (reader.Name == "CST")
                                                        {
                                                            pis.Add("CSTP", reader.ReadString());
                                                        }
                                                        else
                                                        {
                                                            pis.Add(reader.Name, reader.ReadString());
                                                        }
                                                    }
                                                    reader.Read();
                                                }
                                                nota.Add(pis);
                                            }
                                            
                                            break;

                                        case "COFINS":
                                            Dictionary<string, string> cofins = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name != "COFINS")
                                                {
                                                    if (reader.Name != "COFINSAliq" && reader.Name != "COFINSQtde" && reader.Name != "COFINSNT" && reader.Name != "COFINSOutr")
                                                    {
                                                        if (reader.Name == "CST")
                                                        {
                                                            cofins.Add("CSTC", reader.ReadString());
                                                        }
                                                        else
                                                        {
                                                            cofins.Add(reader.Name, reader.ReadString());
                                                        }
                                                    }
                                                    reader.Read();
                                                }
                                                nota.Add(cofins);
                                            }
                                        
                                            break;

                                        case "ICMSTot":
                                            Dictionary<string, string> total = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "ICMSTot")
                                                {
                                                    total.Add(reader.Name, reader.ReadString());

                                                    reader.Read();

                                                }
                                                nota.Add(total);
                                            }
                                         
                                            break;

                                        case "infProt":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "infProt")
                                                {
                                                    if (reader.Name.ToString() == "cStat")
                                                    {
                                                        cStat = reader.ReadString();
                                                    }
                                                    reader.Read();
                                                }
                                            }
                                          
                                            break;

                                    }
                                }
                            }
                            reader.Close();
                            sr.Close();
                        }                        

                        if (nota.Count() > 0 && (Convert.ToInt32(cStat).Equals(100) || Convert.ToInt32(cStat).Equals(150)))
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

        public List<List<Dictionary<string, string>>> NFeResumeEmit(string directoryNfe)
        {
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                bool infCte = false;

                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];


                    if (new FileInfo(arquivo).Length != 0 && (arquivo.Contains(".xml") || arquivo.Contains(".XML")) && (!arquivo.Contains(".lnk") || !arquivo.Contains(".lnk")))
                    {
                        Dictionary<string, string> infNFe = new Dictionary<string, string>();
                        Dictionary<string, string> ide = new Dictionary<string, string>();
                        Dictionary<string, string> emit = new Dictionary<string, string>();
                        Dictionary<string, string> dest = new Dictionary<string, string>();
                        Dictionary<string, string> infProt = new Dictionary<string, string>();

        
                        List<Dictionary<string, string>> note = new List<Dictionary<string, string>>();

                        StreamReader arq = new StreamReader(archivesNfes[i], Encoding.GetEncoding("ISO-8859-1"));
                        using (XmlReader reader = XmlReader.Create(arq))
                        {
                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {
                                        case "infCte":
                                            infCte = true;
                                            break;

                                        case "infNFe":
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "Id")
                                                {
                                                    infNFe.Add("chave", reader.Value.Substring(3, 44));
                                                }
                                            }
                                            infCte = false;
                                            note.Add(infNFe);
                                            break;


                                        case "ide":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "ide" && reader.Name != "NFref")
                                                {
                                                    if (reader.Name.ToString() == "dhEmi")
                                                    {
                                                        string data = Convert.ToDateTime(reader.ReadString().Substring(0, 10)).ToString("dd/MM/yyyy");
                                                        ide.Add(reader.Name, data);
                                                    }
                                                    else
                                                    {
                                                        ide.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                note.Add(ide);
                                            }
                                           
                                            break;


                                        case "emit":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "emit")
                                                {
                                                    if (reader.Name.ToString() != "enderEmit")
                                                    {
                                                        emit.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                note.Add(emit);
                                            }
                                        
                                            break;

                                        case "ICMSTot":
                                            Dictionary<string, string> total = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "ICMSTot")
                                                {
                                                    total.Add(reader.Name, reader.ReadString());
                                                    reader.Read();
                                                }
                                                note.Add(total);
                                            }
                                           
                                            break;

                                        case "infProt":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "infProt")
                                                {
                                                    if (reader.Name.ToString() != "enderEmit")
                                                    {
                                                        if(!infProt.ContainsKey(reader.Name))
                                                            infProt.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                note.Add(infProt);
                                            }
                                        
                                            break;

                                    }
                                }
                            }
                          
                            reader.Close();
                            arq.Close();
                        }

                        if (note.Count > 0)
                            notes.Add(note);

                    }
                }


            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return notes;
        }

        public List<List<Dictionary<string, string>>> NFeAll(string directoryNfe, string directotyCte, Model.Company company)
        {
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();

            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                List<List<Dictionary<string, string>>> ctes = new List<List<Dictionary<string, string>>>();

                if (directotyCte != "")
                    ctes = CTeAll(directotyCte, company.Document);

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                bool infCte = false;

                foreach (var arquivo in archivesNfes)
                {
                    if (new FileInfo(arquivo).Length != 0 && (arquivo.Contains(".xml") || arquivo.Contains(".XML")) && (!arquivo.Contains(".lnk") || !arquivo.Contains(".lnk")))
                    {
                        Dictionary<string, string> infNFe = new Dictionary<string, string>();
                        Dictionary<string, string> ide = new Dictionary<string, string>();
                        Dictionary<string, string> emit = new Dictionary<string, string>();
                        Dictionary<string, string> dest = new Dictionary<string, string>();

                        List<Dictionary<string, string>> nota = new List<Dictionary<string, string>>();
                        string cStat = "100";

                        StreamReader sr = new StreamReader(arquivo, Encoding.GetEncoding("ISO-8859-1"));
                        using (XmlReader reader = XmlReader.Create(sr))
                        {
                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {
                                        case "infCte":
                                            infCte = true;
                                            break;

                                        case "infNFe":
                                            infCte = false;

                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "Id")
                                                {
                                                    infNFe.Add("chave", reader.Value.Substring(3, 44));

                                                    if(reader.Value.Substring(3, 44) == "35230154069380000220550010000674811357321848")
                                                    {
                                                        var t = "OK";
                                                    }
                                                }
                                            }
                                            nota.Add(infNFe);
                                            break;


                                        case "ide":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name != "ide" && reader.Name != "NFref")
                                                {
                                                    ide.Add(reader.Name, reader.ReadString());
                                                    reader.Read();
                                                }
                                                nota.Add(ide);
                                            }
                                            break;


                                        case "emit":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "emit")
                                                {
                                                    if (reader.Name.ToString() != "enderEmit")
                                                    {
                                                        emit.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                nota.Add(emit);
                                            }
                                            break;

                                        case "dest":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "dest")
                                                {
                                                    if (reader.Name.ToString() != "enderDest")
                                                    {
                                                        dest.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                nota.Add(dest);
                                            }
                                            break;

                                        case "ICMSTot":
                                            Dictionary<string, string> total = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "ICMSTot")
                                                {
                                                    total.Add(reader.Name, reader.ReadString());

                                                    reader.Read();

                                                }
                                                nota.Add(total);
                                            }
                                            break;

                                        case "infProt":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "infProt")
                                                {
                                                    if (reader.Name.ToString() == "cStat")
                                                    {
                                                        cStat = reader.ReadString();
                                                    }
                                                    reader.Read();
                                                }
                                            }
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

                                            decimal total_da_nota = Convert.ToDecimal(nota[4]["vNF"]);
                                            decimal total_dos_produtos = Convert.ToDecimal(nota[4]["vProd"]);
                                            decimal valor_carga = 0, valor_prestado = 0, proporcao = 0,  frete_nota = 0, total_icms_frete = 0, frete_icms = 0, 
                                                proporcao_prod = 0, frete_prod = 0, proporcao_icms = 0, frete_icmsprod = 0;
                                            string nCT_temp = "";
                                            nCT = "";

                                            foreach (var item in ctes)
                                            {
                                                for (int j = 0; j < item.Count; j++)
                                                {
                                                    if (item[j].ContainsKey("vCarga"))
                                                        valor_carga = Convert.ToDecimal(item[j]["vCarga"]);

                                                    if (item[j].ContainsKey("vTPrest"))
                                                        valor_prestado = Convert.ToDecimal(item[j]["vTPrest"]);

                                                    if (item[j].ContainsKey("vICMS"))
                                                        total_icms_frete = Convert.ToDecimal(item[j]["vICMS"]);

                                                    if (item[j].ContainsKey("nCT"))
                                                        nCT_temp = item[j]["nCT"];


                                                    if (item[j].ContainsKey("chave"))
                                                    {
                                                        if (item[j]["chave"] == nota[0]["chave"])
                                                        {
                                                            nCT += nCT_temp + " | ";
                                                            proporcao = ((100 * total_da_nota) / valor_carga) / 100;

                                                            if (proporcao > 1)
                                                                proporcao = 1;

                                                            frete_nota = proporcao * valor_prestado;
                                                            frete_icms = proporcao * total_icms_frete;

                                                            if (frete_nota > 0)
                                                            {
                                                                proporcao_prod = ((100 * Convert.ToDecimal(prod["vProd"])) / total_dos_produtos) / 100;
                                                                frete_prod += proporcao_prod * frete_nota;
                                                            }

                                                            if (frete_icms > 0)
                                                            {
                                                                proporcao_icms = ((100 * Convert.ToDecimal(prod["vProd"])) / total_dos_produtos) / 100;
                                                                frete_icmsprod += proporcao_icms * frete_icms;
                                                            }

                                                            valor_carga = 0;
                                                            valor_prestado = 0;
                                                            total_icms_frete = 0;
                                                            nCT_temp = "";
                                                        }
                                                        else
                                                        {
                                                            if (item[j].Count() == 9)
                                                            {
                                                                valor_carga = 0;
                                                                valor_prestado = 0;
                                                                total_icms_frete = 0;
                                                                nCT_temp = "";
                                                            }
                                                        }
                                                    }
                                                }
                                            }                    

                                            if (prod.ContainsKey("vProd"))
                                                base_calc = Convert.ToDecimal(prod["vProd"]);

                                            if (prod.ContainsKey("vOutro"))
                                                base_calc += Convert.ToDecimal(prod["vOutro"]);

                                            if (prod.ContainsKey("vFrete"))
                                                base_calc += Convert.ToDecimal(prod["vFrete"]);

                                            if (prod.ContainsKey("vSeg"))
                                                base_calc += Convert.ToDecimal(prod["vSeg"]);

                                            if (prod.ContainsKey("vDesc"))
                                                base_calc -= Convert.ToDecimal(prod["vDesc"]);

                                            base_calc += frete_prod;

                                            string CEST = "", NCM = "";

                                            if (prod.ContainsKey("NCM"))
                                                NCM = prod["NCM"];

                                            if (prod.ContainsKey("CEST"))
                                                CEST = prod["CEST"];

                                            prod.Add("frete_prod", frete_prod.ToString());
                                            prod.Add("frete_icms", frete_icmsprod.ToString());
                                            prod.Add("nItem", Convert.ToString(nItem));
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
                                                   reader.Name == "pICMS" || reader.Name == "pFCP" || reader.Name == "vFCP" || reader.Name == "vICMS" ||
                                                   reader.Name == "vBCST" || reader.Name == "vICMSST" || reader.Name == "vICMSSTRet" || reader.Name == "vBCFCPST" ||
                                                   reader.Name == "vBCFCPSTRet" || reader.Name == "pFCPST" || reader.Name == "pFCPSTRet" || reader.Name == "vFCPST" ||
                                                   reader.Name == "vFCPSTRet" || reader.Name == "CSOSN" || reader.Name == "pCredSN" || reader.Name == "vCredICMSSN" ||
                                                   reader.Name == "pRedBCEfet" || reader.Name == "vBCSTRet" || reader.Name == "pST" || reader.Name == "vICMSSubstituto" ||
                                                   reader.Name == "pICMSST" || reader.Name == "pMVAST" || reader.Name == "pRedBCST" || reader.Name == "pRedBC")
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
                                            char[] charsToTrim = { ' ', '|' };
                                            NCte.Add("nCT", nCT.Trim(charsToTrim));
                                            nota.Add(NCte);
                                            break;
                                    }
                                }
                            }
                            reader.Close();
                            sr2.Close();
                        }

                        if (nota.Count() > 0 && (Convert.ToInt32(cStat).Equals(100) || Convert.ToInt32(cStat).Equals(150)))
                        {
                            notes.Add(nota);
                        }
                          
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return notes;
        }

        public List<List<Dictionary<string, string>>> NFeAll(string directoryNfe, List<string> cfops)
        {
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                bool infCte = false;

                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && (arquivo.Contains(".xml") || arquivo.Contains(".XML")) && (!arquivo.Contains(".lnk") || !arquivo.Contains(".lnk")))
                    {
                        bool cfop = true;
                        Dictionary<string, string> infNFe = new Dictionary<string, string>();
                        Dictionary<string, string> ide = new Dictionary<string, string>();
                        Dictionary<string, string> emit = new Dictionary<string, string>();
                        Dictionary<string, string> dest = new Dictionary<string, string>();

                        List<Dictionary<string, string>> nota = new List<Dictionary<string, string>>();
                        string cStat = "100";

                        StreamReader sr = new StreamReader(arquivo, Encoding.GetEncoding("ISO-8859-1"));
                        using (XmlReader reader = XmlReader.Create(sr))
                        {
                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {
                                        case "infCte":
                                            infCte = true;
                                            break;

                                        case "infNFe":
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "Id")
                                                {
                                                    infNFe.Add("chave", reader.Value.Substring(3, 44));
                                                }
                                            }
                                            infCte = false;
                                            nota.Add(infNFe);
                                            break;

                                        case "ide":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name != "ide" && reader.Name != "NFref")
                                                {
                                                    ide.Add(reader.Name, reader.ReadString());
                                                    reader.Read();
                                                }
                                                nota.Add(ide);
                                            }
                                           
                                            break;


                                        case "emit":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "emit")
                                                {
                                                    if (reader.Name.ToString() != "enderEmit")
                                                    {
                                                        emit.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                nota.Add(emit);
                                            }
                                           
                                            break;

                                        case "dest":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "dest")
                                                {
                                                    if (reader.Name.ToString() != "enderDest")
                                                    {
                                                        dest.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                nota.Add(dest);
                                            }
                                         
                                            break;

                                        case "prod":

                                            Dictionary<string, string> prod = new Dictionary<string, string>();
                                            cfop = true;
                                            reader.Read();

                                            if (!infCte)
                                            {
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
                                            if (!infCte)
                                            {
                                                while (reader.Name != "ICMS")
                                                {
                                                    if (reader.Name == "orig" || reader.Name == "CST" || reader.Name == "modBC" || reader.Name == "vBC" ||
                                                        reader.Name == "pICMS" || reader.Name == "pFCP" || reader.Name == "vFCP" || reader.Name == "vICMS" ||
                                                        reader.Name == "vBCST" || reader.Name == "vICMSST" || reader.Name == "vICMSSTRet" || reader.Name == "vBCFCPST" ||
                                                        reader.Name == "vBCFCPSTRet" || reader.Name == "pFCPST" || reader.Name == "pFCPSTRet" || reader.Name == "vFCPST" ||
                                                        reader.Name == "vFCPSTRet" || reader.Name == "CSOSN" || reader.Name == "pCredSN" || reader.Name == "vCredICMSSN" ||
                                                        reader.Name == "pRedBCEfet" || reader.Name == "vBCSTRet" || reader.Name == "pST" || reader.Name == "vICMSSubstituto" ||
                                                        reader.Name == "pICMSST" || reader.Name == "pMVAST" || reader.Name == "pRedBCST" || reader.Name == "pRedBC")
                                                    {
                                                        icms.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                if (cfop == true)
                                                {
                                                    nota.Add(icms);
                                                }
                                            }
                                           
                                            break;

                                        case "IPI":
                                            Dictionary<string, string> ipi = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name != "IPI")
                                                {
                                                    if (reader.Name == "cEnq" || reader.Name == "CST" || reader.Name == "vBC" ||
                                                    reader.Name == "pIPI" || reader.Name == "vIPI")
                                                    {
                                                        if (reader.Name == "CST")
                                                        {
                                                            ipi.Add("CSTI", reader.ReadString());
                                                        }
                                                        else
                                                        {
                                                            ipi.Add(reader.Name, reader.ReadString());
                                                        }

                                                    }
                                                    reader.Read();
                                                }
                                                if (cfop == true)
                                                {
                                                    nota.Add(ipi);
                                                }


                                            }

                                            break;

                                        case "PIS":
                                            Dictionary<string, string> pis = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name != "PIS")
                                                {
                                                    if (reader.Name != "PISAliq" && reader.Name != "PISQtde" && reader.Name != "PISNT" && reader.Name != "PISOutr")
                                                    {
                                                        if (reader.Name == "CST")
                                                        {
                                                            pis.Add("CSTP", reader.ReadString());
                                                        }
                                                        else
                                                        {
                                                            pis.Add(reader.Name, reader.ReadString());
                                                        }
                                                    }
                                                    reader.Read();
                                                }
                                                if (cfop == true)
                                                {
                                                    nota.Add(pis);
                                                }
                                            }
                                           
                                            break;

                                        case "COFINS":
                                            Dictionary<string, string> cofins = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name != "COFINS")
                                                {
                                                    if (reader.Name != "COFINSAliq" && reader.Name != "COFINSQtde" && reader.Name != "COFINSNT" && reader.Name != "COFINSOutr")
                                                    {
                                                        if (reader.Name == "CST")
                                                        {
                                                            cofins.Add("CSTC", reader.ReadString());
                                                        }
                                                        else
                                                        {
                                                            cofins.Add(reader.Name, reader.ReadString());
                                                        }
                                                    }
                                                    reader.Read();
                                                }
                                                if (cfop == true)
                                                {
                                                    nota.Add(cofins);
                                                }
                                            }
                                           
                                            break;

                                        case "ICMSTot":
                                            Dictionary<string, string> total = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "ICMSTot")
                                                {
                                                    total.Add(reader.Name, reader.ReadString());

                                                    reader.Read();

                                                }
                                                nota.Add(total);
                                            }
                                          
                                            break;

                                        case "infProt":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "infProt")
                                                {
                                                    if (reader.Name.ToString() == "cStat")
                                                    {
                                                        cStat = reader.ReadString();
                                                    }
                                                    reader.Read();
                                                }
                                            }
                                           
                                            break;
                                    }
                                }
                            }
                            reader.Close();
                            sr.Close();
                        }

                        if (nota.Count() > 0 && (Convert.ToInt32(cStat).Equals(100) || Convert.ToInt32(cStat).Equals(150)))
                        {
                            notes.Add(nota);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return notes;
        }

        public List<List<Dictionary<string, string>>> NFeCFOP(string directoryNfe, string codeCfop)
        {
            List<List<Dictionary<string, string>>> notes = new List<List<Dictionary<string, string>>>();
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                bool infCte = false;


                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && (arquivo.Contains(".xml") || arquivo.Contains(".XML")) && (!arquivo.Contains(".lnk") || !arquivo.Contains(".lnk")))
                    {
                        bool cfop = true;
                        Dictionary<string, string> infNFe = new Dictionary<string, string>();
                        Dictionary<string, string> ide = new Dictionary<string, string>();
                        Dictionary<string, string> emit = new Dictionary<string, string>();
                        Dictionary<string, string> dest = new Dictionary<string, string>();
                        List<Dictionary<string, string>> nota = new List<Dictionary<string, string>>();
                        string cStat = "100";

                        StreamReader sr = new StreamReader(arquivo, Encoding.GetEncoding("ISO-8859-1"));
                        using (XmlReader reader = XmlReader.Create(sr))
                        {
                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                    switch (reader.Name)
                                    {
                                        case "infCte":
                                            infCte = true;
                                            break;

                                        case "infNFe":
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "Id")
                                                {
                                                    infNFe.Add("chave", reader.Value.Substring(3, 44));
                                                }
                                            }
                                            infCte = false;
                                            nota.Add(infNFe);
                                            break;

                                        case "ide":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name != "ide" && reader.Name != "NFref")
                                                {
                                                    ide.Add(reader.Name, reader.ReadString());
                                                    reader.Read();
                                                }
                                                nota.Add(ide);
                                            }
                                           
                                            break;


                                        case "emit":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "emit")
                                                {
                                                    if (reader.Name.ToString() != "enderEmit")
                                                    {
                                                        emit.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                nota.Add(emit);
                                            }
                                          
                                            break;

                                        case "dest":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "dest")
                                                {
                                                    if (reader.Name.ToString() != "enderDest")
                                                    {
                                                        dest.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }
                                                nota.Add(dest);
                                            }
                                          
                                            break;

                                        case "prod":

                                            Dictionary<string, string> prod = new Dictionary<string, string>();
                                            cfop = true;
                                            reader.Read();
                                            if (!infCte)
                                            {
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
                                                    cfop = false;

                                                if (cfop)
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
                                            if (!infCte)
                                            {
                                                while (reader.Name != "ICMS")
                                                {
                                                    if (reader.Name == "orig" || reader.Name == "CST" || reader.Name == "modBC" || reader.Name == "vBC" ||
                                                        reader.Name == "pICMS" || reader.Name == "pFCP" || reader.Name == "vFCP" || reader.Name == "vICMS" ||
                                                        reader.Name == "vBCST" || reader.Name == "vICMSST" || reader.Name == "vICMSSTRet" || reader.Name == "vBCFCPST" ||
                                                        reader.Name == "vBCFCPSTRet" || reader.Name == "pFCPST" || reader.Name == "pFCPSTRet" || reader.Name == "vFCPST" ||
                                                        reader.Name == "vFCPSTRet" || reader.Name == "CSOSN" || reader.Name == "pCredSN" || reader.Name == "vCredICMSSN" ||
                                                        reader.Name == "pRedBCEfet" || reader.Name == "vBCSTRet" || reader.Name == "pST" || reader.Name == "vICMSSubstituto" ||
                                                        reader.Name == "pICMSST" || reader.Name == "pMVAST" || reader.Name == "pRedBCST" || reader.Name == "pRedBC")
                                                    {
                                                        icms.Add(reader.Name, reader.ReadString());
                                                    }
                                                    reader.Read();
                                                }

                                                if (cfop)
                                                    nota.Add(icms);
                                            }
                                        
                                            break;

                                        case "IPI":
                                            Dictionary<string, string> ipi = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name != "IPI")
                                                {
                                                    if (reader.Name == "cEnq" || reader.Name == "CST" || reader.Name == "vBC" ||
                                                    reader.Name == "pIPI" || reader.Name == "vIPI")
                                                    {
                                                        if (reader.Name == "CST")
                                                        {
                                                            ipi.Add("CSTI", reader.ReadString());
                                                        }
                                                        else
                                                        {
                                                            ipi.Add(reader.Name, reader.ReadString());
                                                        }
                                                    }
                                                    reader.Read();
                                                }

                                                if (cfop)
                                                    nota.Add(ipi);
                                            }

                                            break;

                                        case "PIS":
                                            Dictionary<string, string> pis = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name != "PIS")
                                                {
                                                    if (reader.Name != "PISAliq" && reader.Name != "PISQtde" && reader.Name != "PISNT" && reader.Name != "PISOutr")
                                                    {
                                                        if (reader.Name == "CST")
                                                        {
                                                            pis.Add("CSTP", reader.ReadString());
                                                        }
                                                        else
                                                        {
                                                            pis.Add(reader.Name, reader.ReadString());
                                                        }
                                                    }
                                                    reader.Read();
                                                }

                                                if (cfop)
                                                    nota.Add(pis);
                                            }

                                            break;

                                        case "COFINS":
                                            Dictionary<string, string> cofins = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name != "COFINS")
                                                {
                                                    if (reader.Name != "COFINSAliq" && reader.Name != "COFINSQtde" && reader.Name != "COFINSNT" && reader.Name != "COFINSOutr")
                                                    {
                                                        if (reader.Name == "CST")
                                                        {
                                                            cofins.Add("CSTC", reader.ReadString());
                                                        }
                                                        else
                                                        {
                                                            cofins.Add(reader.Name, reader.ReadString());
                                                        }
                                                    }
                                                    reader.Read();
                                                }

                                                if (cfop)
                                                    nota.Add(cofins);
                                            }
                                       
                                            break;

                                        case "ICMSTot":
                                            Dictionary<string, string> total = new Dictionary<string, string>();
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "ICMSTot")
                                                {
                                                    total.Add(reader.Name, reader.ReadString());

                                                    reader.Read();

                                                }
                                                nota.Add(total);
                                            }
                                           
                                            break;

                                        case "infProt":
                                            reader.Read();
                                            if (!infCte)
                                            {
                                                while (reader.Name.ToString() != "infProt")
                                                {
                                                    if (reader.Name.ToString() == "cStat")
                                                    {
                                                        cStat = reader.ReadString();
                                                    }
                                                    reader.Read();
                                                }
                                            }
                                           
                                            break;
                                    }
                                }
                            }
                            reader.Close();
                            sr.Close();
                        }

                        if(nota.Count() > 0 && (Convert.ToInt32(cStat).Equals(100) || Convert.ToInt32(cStat).Equals(150)))
                        {
                            notes.Add(nota);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            return notes;
        }

        public List<List<List<string>>> NFeCanceladaSefaz(string directoryNfe, List<List<Dictionary<string, string>>> notesNFeCanceladas,
                                                          List<List<Dictionary<string, string>>> notesNFeCanceladasEvento,
                                                          List<List<Dictionary<string, string>>> notesNFCeCanceladas,
                                                          List<List<Dictionary<string, string>>> notesNFCeCanceladasEvento)
        {
            List<List<List<string>>> notes = new List<List<List<string>>>();
            List<List<string>> notes55 = new List<List<string>>();
            List<List<string>> notes65 = new List<List<string>>();
            List<List<string>> notesInfo = new List<List<string>>();

            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && (arquivo.Contains(".xml") || arquivo.Contains(".XML")) && (!arquivo.Contains(".lnk") || !arquivo.Contains(".lnk")))
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
                                        case "infNFe":
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "Id")
                                                {
                                                    string chave = reader.Value.Substring(3, 44);
                                                    List<string> nn = new List<string>();
                                                    List<string> nnInfo = new List<string>();

                                                    bool achou = false;

                                                    for (int k = 0; k < notesInfo.Count(); k++)
                                                    {

                                                        if (notesInfo[k][0].Equals(chave))
                                                        {
                                                            achou = true;
                                                            break;
                                                        }
                                                    }

                                                    if (achou == false)
                                                    {
                                                        foreach (var note in notesNFeCanceladas)
                                                        {
                                                            if (note[0]["chave"].Equals(chave))
                                                            {
                                                                nn.Add(arquivo);
                                                                nnInfo.Add(note[0]["chave"]);
                                                                nnInfo.Add(note[1]["mod"]);
                                                                nnInfo.Add(note[1]["nNF"]);
                                                                notes55.Add(nn);
                                                                notesInfo.Add(nnInfo);
                                                                break;
                                                            }
                                                        }

                                                        foreach (var note in notesNFeCanceladasEvento)
                                                        {
                                                            if (note[0]["chNFe"].Equals(chave))
                                                            {
                                                                nn.Add(arquivo);
                                                                nnInfo.Add(note[0]["chNFe"]);
                                                                nnInfo.Add(note[0]["chNFe"].Substring(20, 2));
                                                                nnInfo.Add(note[0]["chNFe"].Substring(25, 9));
                                                                notes55.Add(nn);
                                                                notesInfo.Add(nnInfo);
                                                                break;
                                                            }
                                                        }

                                                        foreach (var note in notesNFCeCanceladas)
                                                        {
                                                            if (note[0]["chave"].Equals(chave))
                                                            {
                                                                nn.Add(arquivo);
                                                                nnInfo.Add(note[0]["chave"]);
                                                                nnInfo.Add(note[1]["mod"]);
                                                                nnInfo.Add(note[1]["nNF"]);
                                                                notes65.Add(nn);
                                                                notesInfo.Add(nnInfo);
                                                                break;
                                                            }
                                                        }

                                                        foreach (var note in notesNFCeCanceladasEvento)
                                                        {
                                                            if (note[0]["chNFe"].Equals(chave))
                                                            {
                                                                nn.Add(arquivo);
                                                                nnInfo.Add(note[0]["chNFe"]);
                                                                nnInfo.Add(note[0]["chNFe"].Substring(20, 2));
                                                                nnInfo.Add(note[0]["chNFe"].Substring(25, 9));
                                                                notes65.Add(nn);
                                                                notesInfo.Add(nnInfo);
                                                                break;
                                                            }
                                                        }
                                                    }

                                                }
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

            notes.Add(notes55);
            notes.Add(notes65);
            notes.Add(notesInfo);

            return notes;
        }

        public List<List<List<string>>> NFeCanceladaEmpresa(string directoryNfe, List<List<string>> spedNFeCancelada, List<List<string>> spedNFCeCancelada)
        {
            List<List<List<string>>> notes = new List<List<List<string>>>();
            List<List<string>> notes55 = new List<List<string>>();
            List<List<string>> notes65 = new List<List<string>>();
            List<List<string>> notesInfo = new List<List<string>>();

            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && (arquivo.Contains(".xml") || arquivo.Contains(".XML")) && (!arquivo.Contains(".lnk") || !arquivo.Contains(".lnk")))
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
                                        case "infNFe":
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "Id")
                                                {
                                                    string chave = reader.Value.Substring(3, 44);
                                                    List<string> nn = new List<string>();
                                                    List<string> nnInfo = new List<string>();

                                                    bool achou = false;

                                                    for (int k = 0; k < notesInfo.Count(); k++)
                                                    {

                                                        if (notesInfo[k][0].Equals(chave))
                                                        {
                                                            achou = true;
                                                            break;
                                                        }
                                                    }

                                                    if (achou == false)
                                                    {
                                                        foreach (var note in spedNFeCancelada)
                                                        {
                                                            if (note[0].Equals(chave))
                                                            {
                                                                nn.Add(arquivo);
                                                                notes55.Add(nn);

                                                                nnInfo.Add(note[0]);
                                                                nnInfo.Add(note[1]);
                                                                nnInfo.Add(note[2]);
                                                                notesInfo.Add(nnInfo);
                                                                break;
                                                            }
                                                        }

                                                        foreach (var note in spedNFCeCancelada)
                                                        {
                                                            if (note[0].Equals(chave))
                                                            {
                                                                nn.Add(arquivo);
                                                                notes65.Add(nn);

                                                                nnInfo.Add(note[0]);
                                                                nnInfo.Add(note[1]);
                                                                nnInfo.Add(note[2]);
                                                                notesInfo.Add(nnInfo);
                                                                break;
                                                            }
                                                        }
                                                    }

                                                }
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

            notes.Add(notes55);
            notes.Add(notes65);
            notes.Add(notesInfo);

            return notes;
        }

        public List<string> NFeMove(string directoryNfe, List<string> notas)
        {
            List<string> notes = new List<string>();

            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesNfes = Directory.GetFiles(directoryNfe);

                for (int i = 0; i < archivesNfes.Count(); i++)
                {
                    var arquivo = archivesNfes[i];

                    if (new FileInfo(arquivo).Length != 0 && (arquivo.Contains(".xml") || arquivo.Contains(".XML")) && (!arquivo.Contains(".lnk") || !arquivo.Contains(".lnk")))
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
                                        case "infNFe":
                                            while (reader.MoveToNextAttribute())
                                            {
                                                if (reader.Name == "Id")
                                                {
                                                    string chave = reader.Value.Substring(3, 44);
                                                    if (notas.Contains(chave))
                                                    {
                                                        notes.Add(arquivo);

                                                    }
                                                }
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

            return notes;
        }

        // CTe

        public List<List<Dictionary<string, string>>> CTeAll(string directotyCte)
        {
            List<List<Dictionary<string, string>>> ctes = new List<List<Dictionary<string, string>>>();

            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesCtes = Directory.GetFiles(directotyCte);

                for (int i = 0; i < archivesCtes.Count(); i++)
                {
                    var arquivo = archivesCtes[i];

                    if (new FileInfo(arquivo).Length != 0 && (arquivo.Contains(".xml") || arquivo.Contains(".XML")) && (!arquivo.Contains(".lnk") || !arquivo.Contains(".lnk")))
                    {
                        List<Dictionary<string, string>> cte = new List<Dictionary<string, string>>();

                        Dictionary<string, string> infCte = new Dictionary<string, string>();
                        Dictionary<string, string> toma = new Dictionary<string, string>();
                        Dictionary<string, string> emit = new Dictionary<string, string>();
                        Dictionary<string, string> dest = new Dictionary<string, string>();
                        Dictionary<string, string> vPrest = new Dictionary<string, string>();
                        Dictionary<string, string> infCarga = new Dictionary<string, string>();
                        Dictionary<string, string> ide = new Dictionary<string, string>();

                        using (XmlReader reader = XmlReader.Create(new StreamReader(archivesCtes[i], Encoding.UTF8)))
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
                                                    infCte.Add("chave", reader.Value.Substring(3, 44));
                                                }
                                            }
                                            cte.Add(infCte);
                                            break;

                                        case "ide":
                                            reader.Read();
                                            while (reader.Name != "ide")
                                            {
                                                if (reader.Name.Equals("toma4"))
                                                {
                                                    break;
                                                }
                                                if (reader.Name != "toma3")
                                                {
                                                    ide.Add(reader.Name, reader.ReadString());
                                                }

                                                reader.Read();
                                            }
                                            cte.Add(ide);
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
                                            cte.Add(emit);
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
                                            cte.Add(dest);
                                            break;

                                        case "vTPrest":
                                            vPrest.Add(reader.Name, reader.ReadString());
                                            cte.Add(vPrest);
                                            break;
                                    }
                                }

                            }
                            reader.Close();
                        }

                        if (cte.Count() > 0)
                        {
                            ctes.Add(cte);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            return ctes;
        }

        public List<List<Dictionary<string, string>>> CTeAll(string directory, string cnpj)
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
                    var arquivo = archivesCtes[i];

                    if (new FileInfo(arquivo).Length != 0 && (arquivo.Contains(".xml") || arquivo.Contains(".XML")) && (!arquivo.Contains(".lnk") || !arquivo.Contains(".lnk")))
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
                                                if (reader.Name != "")
                                                {
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

                                            while (reader.Name != "infNFe")
                                            {
                                                if (reader.Name != "")
                                                {
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

                        if (cte.Count() > 0)
                        {
                            ctes.Add(cte);
                        }

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

                            if (ctes[i][j]["toma"] != "3")
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

    }
}
