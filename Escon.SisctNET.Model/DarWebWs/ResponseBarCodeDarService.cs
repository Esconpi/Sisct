namespace Escon.SisctNET.Model.DarWebWs
{
    public class ResponseBarCodeDarService
    {
        /// <summary>
        /// Retorna o valor ERRO ou SUCESSO.
        /// </summary>
        public string MessageType { get; set; }

        /// <summary>
        /// Mensagem de retorno da operação. Podendo ser uma mensagem de sucesso, caso o DAR tenha sido inserido com sucesso, ou uma mensagem de erro caso contrário
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Número que deve ser usado para a geração da imagem do código barras. O tipo de código de barras aceito é I25
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// Código de Barras com os respectivos dígitos verificadores separados por traço (-)
        /// </summary>
        public string DigitableLine { get; set; }

        /// <summary>
        /// Número único do documento gerado que vai dentro do código de barras e é responsável por localizar o pagamento do documento.
        /// </summary>
        public string ControlNumber { get; set; }

        /// <summary>
        /// Número do documento que foi enviado no momento da solicitação
        /// </summary>
        public string DocumentNumber { get; set; }

    }
}