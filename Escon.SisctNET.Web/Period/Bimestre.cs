using System.Collections.Generic;

namespace Escon.SisctNET.Web.Period
{
    public class Bimestre
    {
        public List<string> Months(string mes)
        {
            List<string> meses = new List<string>();

            if (mes.Equals("Março"))
            {
                meses.Add("Janeiro");
                meses.Add("Fevereiro");
            }
            else if (mes.Equals("Junho"))
            {
                meses.Add("Abril");
                meses.Add("Maio");
            }
            else if (mes.Equals("Setembro"))
            {
                meses.Add("Julho");
                meses.Add("Agosto");
            }
            else if (mes.Equals("Dezembro"))
            {
                meses.Add("Outubro");
                meses.Add("Novembro");
            }

            return meses;
        }
    }
}
