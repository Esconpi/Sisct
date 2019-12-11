using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Escon.SisctNET.Web.Compare
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
                    if (new FileInfo(archivesNfes[i]).Length != 0)
                    {
                        Dictionary<string, string> infNFe = new Dictionary<string, string>();
                        Dictionary<string, string> ide = new Dictionary<string, string>();
                        Dictionary<string, string> emit = new Dictionary<string, string>();
                        Dictionary<string, string> dest = new Dictionary<string, string>();

                        List<Dictionary<string, string>> note = new List<Dictionary<string, string>>();
                        using (XmlReader reader = XmlReader.Create(new StreamReader(archivesNfes[i], Encoding.GetEncoding("ISO-8859-1"))))
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
                                            note.Add(infNFe);
                                            break;


                                        case "ide":
                                            reader.Read();
                                            while (reader.Name.ToString() != "ide" && reader.Name != "NFref")
                                            {
                                                if (reader.Name.ToString() == "dhEmi")
                                                {
                                                    string data = Convert.ToDateTime(reader.ReadString().Substring(0,10)).ToString("dd/MM/yyyy");                                                    
                                                    ide.Add(reader.Name, data);
                                                }
                                                else
                                                {
                                                    ide.Add(reader.Name, reader.ReadString());
                                                }
                                                reader.Read();
                                            }
                                            note.Add(ide);
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
                                            note.Add(emit);
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
                                            note.Add(dest);
                                            break;

                                        case "ICMSTot":
                                            Dictionary<string, string> total = new Dictionary<string, string>();
                                            reader.Read();
                                            while (reader.Name.ToString() != "ICMSTot")
                                            {
                                                total.Add(reader.Name, reader.ReadString());
                                                reader.Read();

                                            }
                                            note.Add(total);
                                            break;
                                    }
                                }
                            }
                            reader.Close();
                        }
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

        public List<string> SpedNfe(string directorySped)
        {
            List<string> sped = new List<string>();
            StreamReader archiveSped = new StreamReader(directorySped);
            try
            {
                string line;
                while ((line = archiveSped.ReadLine()) != null)
                {
                    string[] linha = line.Split('|');
                    if (linha[1] == "C100" && linha[2] == "0")
                    {
                        sped.Add(linha[9]);
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
            StreamReader archiveSped = new StreamReader(directorySped);
            try
            {
                string line;

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

        public List<List<Dictionary<string, string>>> Cte(string directotyCte)
        {
            List<List<Dictionary<string, string>>> ctes = new List<List<Dictionary<string, string>>>();
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string[] archivesCtes = Directory.GetFiles(directotyCte);


                for (int i = 0; i < archivesCtes.Count(); i++)
                {
                    if (new FileInfo(archivesCtes[i]).Length != 0)
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

                                                if (reader.Name != "toma3")
                                                {
                                                    ide.Add(reader.Name, reader.ReadString());
                                                }

                                                if (reader.Name == "toma4")
                                                {
                                                    break;
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
                        ctes.Add(cte);
                    }
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
