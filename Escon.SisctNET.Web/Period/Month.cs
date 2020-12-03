
using System.Collections.Generic;

namespace Escon.SisctNET.Web.Period
{
    public class Month
    {
        public int NumberMonth(string mes)
        {
            if (mes.Equals("Janeiro"))
            {
                return 1;
            }
            else if (mes.Equals("Fevereiro"))
            {
                return 2;
            }
            else if (mes.Equals("Março"))
            {
                return 3;
            }
            else if (mes.Equals("Abril"))
            {
                return 4;
            }
            else if (mes.Equals("Maio"))
            {
                return 5;
            }
            else if (mes.Equals("Junho"))
            {
                return 6;
            }
            else if (mes.Equals("Julho"))
            {
                return 7;
            }
            else if (mes.Equals("Agosto"))
            {
                return 8;
            }
            else if (mes.Equals("Setembro"))
            {
                return 9;
            }
            else if (mes.Equals("Outubro"))
            {
                return 10;
            }
            else if (mes.Equals("Novembro"))
            {
                return 11;
            }
            else
            {
                return 12;
            }
        }
        
        public string NameMonth(int numero)
        {

            if (numero.Equals(1))
            {
                return "Janeiro";
            }
            else if (numero.Equals(2))
            {
                return "Fevereiro";
            }
            else if (numero.Equals(3))
            {
                return "Março";
            }
            else if (numero.Equals(4))
            {
                return "Abril";
            }
            else if (numero.Equals(5))
            {
                return "Maio";
            }
            else if (numero.Equals(6))
            {
                return "Junho";
            }
            else if (numero.Equals(7))
            {
                return "Julho";
            }
            else if (numero.Equals(8))
            {
                return "Agosto";
            }
            else if (numero.Equals(9))
            {
                return "Setembro";
            }
            else if (numero.Equals(10))
            {
                return "Outubro";
            }
            else if (numero.Equals(11))
            {
                return "Novembro";
            }
            else
            {
                return "Dezembro";
            }
        }

        public string NameMonthPrevious(int numero)
        {
            if (numero.Equals(1))
            {
                numero = 12;
            }
            else
            {
                numero = numero - 1;
            }

            if (numero.Equals(1))
            {
                return "Janeiro";
            }
            else if (numero.Equals(2))
            {
                return "Fevereiro";
            }
            else if (numero.Equals(3))
            {
                return "Março";
            }
            else if (numero.Equals(4))
            {
                return "Abril";
            }
            else if (numero.Equals(5))
            {
                return "Maio";
            }
            else if (numero.Equals(6))
            {
                return "Junho";
            }
            else if (numero.Equals(7))
            {
                return "Julho";
            }
            else if (numero.Equals(8))
            {
                return "Agosto";
            }
            else if (numero.Equals(9))
            {
                return "Setembro";
            }
            else if (numero.Equals(10))
            {
                return "Outubro";
            }
            else if (numero.Equals(11))
            {
                return "Novembro";
            }
            else
            {
                return "Dezembro";
            }
        }

        public string NameMonthNext(int numero)
        {
            if (numero.Equals(12))
            {
                numero = 1;
            }
            else
            {
                numero = numero + 1;
            }

            if (numero.Equals(1))
            {
                return "Janeiro";
            }
            else if (numero.Equals(2))
            {
                return "Fevereiro";
            }
            else if (numero.Equals(3))
            {
                return "Março";
            }
            else if (numero.Equals(4))
            {
                return "Abril";
            }
            else if (numero.Equals(5))
            {
                return "Maio";
            }
            else if (numero.Equals(6))
            {
                return "Junho";
            }
            else if (numero.Equals(7))
            {
                return "Julho";
            }
            else if (numero.Equals(8))
            {
                return "Agosto";
            }
            else if (numero.Equals(9))
            {
                return "Setembro";
            }
            else if (numero.Equals(10))
            {
                return "Outubro";
            }
            else if (numero.Equals(11))
            {
                return "Novembro";
            }
            else
            {
                return "Dezembro";
            }
        }
        
        public List<string> Months(string inicio, string fim)
        {
            List<string> meses = new List<string>();

            if(inicio.Equals("Janeiro"))
            {
                if (fim.Equals("Janeiro"))
                {
                    meses.Add("Janeiro");
                }
                else if (fim.Equals("Fevereiro"))
                {
                    meses.Add("Janeiro");
                    meses.Add("Fevereiro");
                }
                else if (fim.Equals("Março"))
                {
                    meses.Add("Janeiro");
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                }
                else if (fim.Equals("Abril"))
                {
                    meses.Add("Janeiro");
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                }
                else if (fim.Equals("Maio"))
                {
                    meses.Add("Janeiro");
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                }
                else if (fim.Equals("Junho"))
                {
                    meses.Add("Janeiro");
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                }
                else if (fim.Equals("Julho"))
                {
                    meses.Add("Janeiro");
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                }
                else if (fim.Equals("Agosto"))
                {
                    meses.Add("Janeiro");
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                }
                else if (fim.Equals("Setembro"))
                {
                    meses.Add("Janeiro");
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                }
                else if (fim.Equals("Outubro"))
                {
                    meses.Add("Janeiro");
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                }
                else if (fim.Equals("Novembro"))
                {
                    meses.Add("Janeiro");
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                }
                else if (fim.Equals("Dezembro"))
                {
                    meses.Add("Janeiro");
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                    meses.Add("Dezembro");
                }

            }
            else if (inicio.Equals("Fevereiro"))
            {
                if (fim.Equals("Fevereiro"))
                {
                    meses.Add("Fevereiro");
                }
                else if (fim.Equals("Março"))
                {
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                }
                else if (fim.Equals("Abril"))
                {
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                }
                else if (fim.Equals("Maio"))
                {
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                }
                else if (fim.Equals("Junho"))
                {
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                }
                else if (fim.Equals("Julho"))
                {
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                }
                else if (fim.Equals("Agosto"))
                {
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                }
                else if (fim.Equals("Setembro"))
                {
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                }
                else if (fim.Equals("Outubro"))
                {
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                }
                else if (fim.Equals("Novembro"))
                {
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                }
                else if (fim.Equals("Dezembro"))
                {
                    meses.Add("Fevereiro");
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                    meses.Add("Dezembro");
                }

            }
            else if (inicio.Equals("Março"))
            {
                if (fim.Equals("Março"))
                {
                    meses.Add("Março");
                }
                else if (fim.Equals("Abril"))
                {
                    meses.Add("Março");
                    meses.Add("Abril");
                }
                else if (fim.Equals("Maio"))
                {
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                }
                else if (fim.Equals("Junho"))
                {
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                }
                else if (fim.Equals("Julho"))
                {
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                }
                else if (fim.Equals("Agosto"))
                {
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                }
                else if (fim.Equals("Setembro"))
                {
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                }
                else if (fim.Equals("Outubro"))
                {
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                }
                else if (fim.Equals("Novembro"))
                {
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                }
                else if (fim.Equals("Dezembro"))
                {
                    meses.Add("Março");
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                    meses.Add("Dezembro");
                }
            }
            else if (inicio.Equals("Abril"))
            {
                if (fim.Equals("Abril"))
                {
                    meses.Add("Abril");
                }
                else if (fim.Equals("Maio"))
                {
                    meses.Add("Abril");
                    meses.Add("Maio");
                }
                else if (fim.Equals("Junho"))
                {
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                }
                else if (fim.Equals("Julho"))
                {
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                }
                else if (fim.Equals("Agosto"))
                {
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                }
                else if (fim.Equals("Setembro"))
                {
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                }
                else if (fim.Equals("Outubro"))
                {
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                }
                else if (fim.Equals("Novembro"))
                {
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                }
                else if (fim.Equals("Dezembro"))
                {
                    meses.Add("Abril");
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                    meses.Add("Dezembro");
                }
            }
            else if (inicio.Equals("Maio"))
            {
                if (fim.Equals("Maio"))
                {
                    meses.Add("Maio");
                }
                else if (fim.Equals("Junho"))
                {
                    meses.Add("Maio");
                    meses.Add("Junho");
                }
                else if (fim.Equals("Julho"))
                {
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                }
                else if (fim.Equals("Agosto"))
                {
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                }
                else if (fim.Equals("Setembro"))
                {
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                }
                else if (fim.Equals("Outubro"))
                {
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                }
                else if (fim.Equals("Novembro"))
                {
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                }
                else if (fim.Equals("Dezembro"))
                {
                    meses.Add("Maio");
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                    meses.Add("Dezembro");
                }
            }
            else if (inicio.Equals("Junho"))
            {
                if (fim.Equals("Junho"))
                {
                    meses.Add("Junho");
                }
                else if (fim.Equals("Julho"))
                {
                    meses.Add("Junho");
                    meses.Add("Julho");
                }
                else if (fim.Equals("Agosto"))
                {
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                }
                else if (fim.Equals("Setembro"))
                {
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                }
                else if (fim.Equals("Outubro"))
                {
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                }
                else if (fim.Equals("Novembro"))
                {
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                }
                else if (fim.Equals("Dezembro"))
                {
                    meses.Add("Junho");
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                    meses.Add("Dezembro");
                }
            }
            else if (inicio.Equals("Julho"))
            {
                if (fim.Equals("Julho"))
                {
                    meses.Add("Julho");
                }
                else if (fim.Equals("Agosto"))
                {
                    meses.Add("Julho");
                    meses.Add("Agosto");
                }
                else if (fim.Equals("Setembro"))
                {
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                }
                else if (fim.Equals("Outubro"))
                {
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                }
                else if (fim.Equals("Novembro"))
                {
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                }
                else if (fim.Equals("Dezembro"))
                {
                    meses.Add("Julho");
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                    meses.Add("Dezembro");
                }
            }
            else if (inicio.Equals("Agosto"))
            {
                if (fim.Equals("Agosto"))
                {
                    meses.Add("Agosto");
                }
                else if (fim.Equals("Setembro"))
                {
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                }
                else if (fim.Equals("Outubro"))
                {
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                }
                else if (fim.Equals("Novembro"))
                {
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                }
                else if (fim.Equals("Dezembro"))
                {
                    meses.Add("Agosto");
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                    meses.Add("Dezembro");
                }
            }
            else if (inicio.Equals("Setembro"))
            {
                if (fim.Equals("Setembro"))
                {
                    meses.Add("Setembro");
                }
                else if (fim.Equals("Outubro"))
                {
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                }
                else if (fim.Equals("Novembro"))
                {
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                }
                else if (fim.Equals("Dezembro"))
                {
                    meses.Add("Setembro");
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                    meses.Add("Dezembro");
                }
            }
            else if (inicio.Equals("Outubro"))
            {
                if (fim.Equals("Outubro"))
                {
                    meses.Add("Outubro");
                }
                else if (fim.Equals("Novembro"))
                {
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                }
                else if (fim.Equals("Dezembro"))
                {
                    meses.Add("Outubro");
                    meses.Add("Novembro");
                    meses.Add("Dezembro");
                }
            }
            else if (inicio.Equals("Novembro"))
            {
                if (fim.Equals("Novembro"))
                {
                    meses.Add("Novembro");
                }
                else if (fim.Equals("Dezembro"))
                {
                    meses.Add("Novembro");
                    meses.Add("Dezembro");
                }
            }
            else if (inicio.Equals("Dezembro"))
            {
                if (fim.Equals("Dezembro"))
                {
                    meses.Add("Dezembro");
                }
            }

            return meses;
        }
    }
}
