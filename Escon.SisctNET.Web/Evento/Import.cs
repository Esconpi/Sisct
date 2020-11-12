using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Escon.SisctNET.Web.Evento
{
    public class Import
    {
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
                                        case "infEvento":
                                            Dictionary<string, string> infEvento = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name != "infEvento" && reader.Name != "verEvento")
                                            {
                                                if (reader.Name == "cOrgao" || reader.Name == "tpAmb" || reader.Name == "CNPJ" ||
                                                     reader.Name == "chNFe" || reader.Name == "dhEvento" || reader.Name == "tpEvento" ||
                                                     reader.Name == "nSeqEvento" || reader.Name == "verEvento" || reader.Name == "cStat" || 
                                                     reader.Name == "xMotivo" || reader.Name == "CNPJDest" || reader.Name == "dhRegEvento" || 
                                                     reader.Name == "nProt")
                                                {
                                                    if (!infEvento.ContainsKey(reader.Name))
                                                    {
                                                        infEvento.Add(reader.Name, reader.ReadString());
                                                    }
                                                }
                                                reader.Read();
                                            }
                                            nota.Add(infEvento);
                                            break;


                                        case "detEvento":
                                            Dictionary<string, string> det = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name.ToString() != "detEvento" && reader.Name.ToString() != "xJust")
                                            {
                                                if (reader.Name == "descEvento" || reader.Name == "nProt" || reader.Name == "xJust")
                                                {
                                                    if (!det.ContainsKey(reader.Name))
                                                    {
                                                        det.Add(reader.Name, reader.ReadString());
                                                    }
                                                }
                                                reader.Read();
                                            }
                                            nota.Add(det);
                                            break;
                                    }
                                }
                            }
                            reader.Close();
                            sr.Close();
                        }

                        if (nota.Count() > 0)
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

        public List<List<List<string>>> MoveCanceladaSefaz(string directoryNfe, List<List<Dictionary<string, string>>> notesNFeCanceladas,
           List<List<Dictionary<string, string>>> notesNFeCanceladasEvento, List<List<Dictionary<string, string>>> notesNFCeCanceladas,
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
                                        case "infEvento":
                                            reader.Read();
                                            while (reader.Name != "infEvento" && reader.Name != "verEvento")
                                            {
                                                if (reader.Name == "chNFe")
                                                {
                                                    List<string> nn = new List<string>();
                                                    List<string> nnInfo = new List<string>();

                                                    var chave = reader.ReadString();

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

            notes.Add(notes55);
            notes.Add(notes65);
            notes.Add(notesInfo);

            return notes;

        }

        public List<List<List<string>>> MoveCanceladaEmpresa(string directoryNfe, List<List<string>> spedNFeCancelada, List<List<string>> spedNFCeCancelada)
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
                                        case "infEvento":
                                            reader.Read();
                                            while (reader.Name != "infEvento" && reader.Name != "verEvento")
                                            {
                                                if (reader.Name == "chNFe")
                                                {
                                                    List<string> nn = new List<string>();
                                                    List<string> nnInfo = new List<string>();

                                                    var chave = reader.ReadString();

                                                    bool achou = false;

                                                    for (int k = 0; k < notesInfo.Count(); k++)
                                                    {

                                                        if (notesInfo[k][0].Equals(chave))
                                                        {
                                                            achou = true;
                                                            break;
                                                        }
                                                    }

                                                    if(achou == false)
                                                    {
                                                        foreach (var note in spedNFeCancelada)
                                                        {
                                                            if (note[0].Equals(chave))
                                                            {
                                                                nn.Add(arquivo);
                                                                nnInfo.Add(note[0]);
                                                                nnInfo.Add(note[1]);
                                                                nnInfo.Add(note[2]);
                                                                notes55.Add(nn);
                                                                notesInfo.Add(nnInfo);
                                                                break;
                                                            }
                                                        }

                                                        foreach (var note in spedNFCeCancelada)
                                                        {
                                                            if (note[0].Equals(chave))
                                                            {
                                                                nn.Add(arquivo);
                                                                nnInfo.Add(note[0]);
                                                                nnInfo.Add(note[1]);
                                                                nnInfo.Add(note[2]);
                                                                notes65.Add(nn);
                                                                notesInfo.Add(nnInfo);
                                                                break;
                                                            }
                                                        }
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

            notes.Add(notes55);
            notes.Add(notes65);
            notes.Add(notesInfo);

            return notes;
        }
    }
}
