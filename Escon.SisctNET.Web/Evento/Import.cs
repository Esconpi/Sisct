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
    }
}
