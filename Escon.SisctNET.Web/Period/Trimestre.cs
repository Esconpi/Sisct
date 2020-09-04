using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Web.Period
{
    public class Trimestre
    {
        public List<string> Months(string trimestre)
        {
            List<string> meses = new List<string>();

            if (trimestre.Equals("Primeiro"))
            {
                meses.Add("Janeiro");
                meses.Add("Fevereiro");
                meses.Add("Março");
            }
            else if(trimestre.Equals("Segundo"))
            {
                meses.Add("Abril");
                meses.Add("Maio");
                meses.Add("Junho");
            }
            else if (trimestre.Equals("Terceiro"))
            {
                meses.Add("Julho");
                meses.Add("Agosto");
                meses.Add("Setembro");
            }
            else if (trimestre.Equals("Quarto"))
            {
                meses.Add("Outubro");
                meses.Add("Novembro");
                meses.Add("Dezembro");
            }

            return meses;

        }
    }
}
