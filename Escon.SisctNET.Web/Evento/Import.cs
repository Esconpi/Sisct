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
                        Dictionary<string, string> infEvento = new Dictionary<string, string>();
                        Dictionary<string, string> det = new Dictionary<string, string>();
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
                                            reader.Read();
                                            while (reader.Name != "infEvento" && reader.Name != "verEvento")
                                            {
                                                infEvento.Add(reader.Name, reader.ReadString());
                                                reader.Read();
                                            }
                                            nota.Add(infEvento);
                                            break;


                                        case "detEvento":
                                            reader.Read();
                                            while (reader.Name.ToString() != "detEvento")
                                            {
                                                if (reader.Name.ToString() != "xJust")
                                                {
                                                    det.Add(reader.Name, reader.ReadString());
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
