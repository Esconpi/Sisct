namespace Escon.SisctNET.Web.Diretorio
{
    public class Import
    {
        public string Entrada(Model.Company comp,string dirRaiz,string year, string month)
        {
            return dirRaiz + "\\" + comp.SocialName.Trim() + "-" + comp.Document.Trim() + "\\" + year + "\\" + month;
        }

        public string SaidaSefaz(Model.Company comp, string dirRaiz, string year, string month)
        {
           return dirRaiz + "\\" + comp.SocialName.Trim() + "-" + comp.Document.Trim() + "\\" + year + "\\" + month + "\\" + "SEFAZ";
        }

        public string NFeCanceladaSefaz(Model.Company comp, string dirRaiz, string year, string month)
        {
            return dirRaiz + "\\" + comp.SocialName.Trim() + "-" + comp.Document.Trim() + "\\" + year + "\\" + month + "\\" + "SEFAZ" + "\\" + "NFe CANCELADA";
        }

        public string NFCeCanceladaSefaz(Model.Company comp, string dirRaiz, string year, string month)
        {
            return dirRaiz + "\\" + comp.SocialName.Trim() + "-" + comp.Document.Trim() + "\\" + year + "\\" + month + "\\" + "SEFAZ" + "\\" + "NFCe CANCELADA";
        }

        public string SaidaEmpresa(Model.Company comp, string dirRaiz, string year, string month)
        {
            return dirRaiz + "\\" + comp.SocialName.Trim() + "-" + comp.Document.Trim() + "\\" + year + "\\" + month + "\\" + "EMPRESA";
        }

        public string NFeCanceladaEmpresa(Model.Company comp, string dirRaiz, string year, string month)
        {
            return dirRaiz + "\\" + comp.SocialName.Trim() + "-" + comp.Document.Trim() + "\\" + year + "\\" + month + "\\" + "EMPRESA" + "\\" + "NFe CANCELADA";
        }

        public string NFCeCanceladaEmpresa(Model.Company comp, string dirRaiz, string year, string month)
        {
            return dirRaiz + "\\" + comp.SocialName.Trim() + "-" + comp.Document.Trim() + "\\" + year + "\\" + month + "\\" + "EMPRESA" + "\\" + "NFCe CANCELADA";
        }

    }
}
