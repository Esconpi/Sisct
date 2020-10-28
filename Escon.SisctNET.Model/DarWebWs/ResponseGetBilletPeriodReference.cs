using System.Collections.Generic;

namespace Escon.SisctNET.Model.DarWebWs
{
    public class ResponseGetBilletPeriodReference
    {
        public ResponseGetBilletPeriodReference()
        {
            Billets = new List<ResponseGetBilletPeriodReferenceBillet>();
        }

        public string TypeReturn { get; set; }

        public string MessageReturn { get; set; }

        public List<ResponseGetBilletPeriodReferenceBillet> Billets { get; set; }

    }

    public class ResponseGetBilletPeriodReferenceBillet
    {
        public string BarCode { get; set; }

        public string ControlNumber { get; set; }

        public string DocumentNumber { get; set; }

        public bool PaidOut { get; set; }
    }

    public class ResponseGetDarIcms
    {
        public string MensagemRetorno { get; set; }

        public string TipoRetorno { get; set; }

        public string BoletoBytes { get; set; }

        public string CodigoBarras { get; set; }

        public string LinhaDigitavel { get; set; }

        public string NumeroControle { get; set; }

        public string NumeroDocumento { get; set; }
    }
}
