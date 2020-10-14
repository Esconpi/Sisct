using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace Escon.SisctNET.Web.Planilha
{
    public class Import
    {
        public List<List<string>> Product(string directoryPlanilha)
        {
            List<List<string>> products = new List<List<string>>();

            OleDbConnection connect = new OleDbConnection(@"Provider = Microsoft.ACE.OLEDB.12.0; Data Source = " + directoryPlanilha + "; " + "Extended Properties = 'Excel 12.0 Xml;HDR=NO';");
            string commandoSql = "Select * from [Plan1$]";
            OleDbCommand comando = new OleDbCommand(commandoSql, connect);

            try
            {
                connect.Open();
                OleDbDataReader rd = comando.ExecuteReader();

                while (rd.Read())
                {
                    List<string> product = new List<string>();
                    product.Add(rd[0].ToString());
                    product.Add(rd[1].ToString());
                    product.Add(rd[2].ToString());
                    product.Add(rd[3].ToString());
                    products.Add(product);
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                connect.Close();
            }

            /*StreamReader archive = new StreamReader(directoryPlanilha, Encoding.GetEncoding("ISO-8859-1"));

            string line;

            try
            {
                while ((line = archive.ReadLine()) != null)
                {
                    List<string> product = new List<string>();

                    string[] linhas = line.Split(";");
                    foreach (var linha in linhas)
                    {
                        if (!linha.Equals(""))
                        {
                            product.Add(linha);
                        }
                        //product.Add(linha);
                    }
                    if (product.Count > 0)
                    {
                        if (product.Count.Equals(4))
                        {
                            var ultimo = products[products.Count - 1];
                            ultimo[1] = (ultimo[1] + " " + product[0]);
                            product.RemoveAt(0);
                            ultimo.AddRange(product);
                            products[products.Count - 1] = ultimo;
                        }
                        else
                        {
                            products.Add(product);
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
                archive.Close();
            }*/
            return products;
        }
        
        public List<List<string>> Notes(string directoryPlanilha)
        {
            List<List<string>> notes = new List<List<string>>();

            OleDbConnection connect = new OleDbConnection(@"Provider = Microsoft.ACE.OLEDB.12.0; Data Source = " + directoryPlanilha + "; " + "Extended Properties = 'Excel 12.0 Xml;HDR=NO';");
            string commandoSql = "Select * from [Plan1$]";
            OleDbCommand comando = new OleDbCommand(commandoSql, connect);

            try
            {

                connect.Open();
                OleDbDataReader rd = comando.ExecuteReader();

                while (rd.Read())
                {
                    List<string> note = new List<string>();
                    note.Add(rd[0].ToString());
                    note.Add(rd[1].ToString());
                    note.Add(rd[2].ToString());
                    note.Add(rd[3].ToString());
                    note.Add(rd[4].ToString());
                    note.Add(rd[5].ToString());
                    notes.Add(note);
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                connect.Close();
            }
            return notes;
        }
    
    }
}
