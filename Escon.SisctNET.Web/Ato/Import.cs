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

                    string[] linha = line.Split(";");
                    foreach (var l in linha)
                    {
                        if (!l.Equals(""))
                        {
                            product.Add(l);
                        }
                        
                    }
                    if (product.Count > 0)
                    {
                        products.Add(product);
                        
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
