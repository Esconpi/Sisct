using System.Collections.Generic;

namespace Escon.SisctNET.Web.Period
{
    public class Bimestre
    {
        public List<string> Months(string mesTrimestre)
        {
            List<string> meses = new List<string>();

            if (mesTrimestre.Equals("Março"))
            {
                meses.Add("Janeiro");
                meses.Add("Fevereiro");
            }
            else if (mesTrimestre.Equals("Junho"))
            {
                meses.Add("Abril");
                meses.Add("Maio");
            }
            else if (mesTrimestre.Equals("Setembro"))
            {
                meses.Add("Julho");
                meses.Add("Agosto");
            }
            else if (mesTrimestre.Equals("Dezembro"))
            {
                meses.Add("Outubro");
                meses.Add("Novembro");
            }

            return meses;
        }
    }
}
