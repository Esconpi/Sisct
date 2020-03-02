using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Escon.SisctNET.Web.Ato
{
    public class Import
    {

        public List<List<string>> Product(string directoryAto)
        {
            List<List<string>> products = new List<List<string>>();
            StreamReader archiveAto = new StreamReader(directoryAto, Encoding.GetEncoding("ISO-8859-1"));
            try
            {
                string line;

                while ((line = archiveAto.ReadLine()) != null)
                {
                    List<string> product = new List<string>();

                    string[] linhas = line.Split(";");
                    foreach (var linha in linhas)
                    {
                        if (!linha.Equals(""))
                        {
                            product.Add(linha);
                        }                        
                    }
                    if (product.Count > 0)
                    {
                        if (product.Count.Equals(5))
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
                archiveAto.Close();
            }
            return products;
        }
    }
}
