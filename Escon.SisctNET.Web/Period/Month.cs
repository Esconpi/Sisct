
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
    }
}
