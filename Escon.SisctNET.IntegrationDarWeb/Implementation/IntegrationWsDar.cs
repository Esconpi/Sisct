using ConsultaDocumentoArrecadacaoDarWeb;
using Escon.SisctNET.Model.DarWebWs;
using IntegrationDarService;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Escon.SisctNET.IntegrationDarWeb.Implementation
{
    public class IntegrationWsDar : IIntegrationWsDar
    {
        private readonly DocumentoArrecadacaoWSClient wsClient;
        private readonly ConsultaDocumentoArrecadacaoWSClient wsClientRead;
        private readonly IConfiguration _configuration;

        public IntegrationWsDar(IConfiguration configuration)
        {
            _configuration = configuration;

            var urlBase = _configuration["Sefaz:UrlBase"];
            var urlBaseRead = _configuration["Sefaz:UrlBaseRead"];

            wsClient = new DocumentoArrecadacaoWSClient(DocumentoArrecadacaoWSClient.EndpointConfiguration.DocumentoArrecadacaoWSPort, urlBase);
            wsClientRead = new ConsultaDocumentoArrecadacaoWSClient(ConsultaDocumentoArrecadacaoWSClient.EndpointConfiguration.ConsultaDocumentoArrecadacaoWSPort, urlBaseRead);
        }

        public async Task<ResponseBarCodeDarService> GetBarCodeAsync(solicitarCodigoBarrasRequest request)
        {
            var response = await wsClient.solicitarCodigoBarrasAsync(
                request.cpfCnpjIE,
                request.numeroDocumento,
                request.valorTotal,
                request.dataVencimento,
                request.periodoReferencia,
                request.codigoOrgao,
                request.codigoReceita,
                request.tokenAcesso);

            ResponseBarCodeDarService codigoBarrasResponse = new ResponseBarCodeDarService()
            {
                BarCode = response.@return.codigoBarras,
                ControlNumber = response.@return.numeroControle,
                DigitableLine = response.@return.linhaDigitavel,
                DocumentNumber = response.@return.numeroDocumento,
                Message = response.@return.mensagemRetorno1,
                MessageType = response.@return.tipoRetorno
            };

            return codigoBarrasResponse;
        }

        public async Task<ResponseBarCodeDarService> GetBarCodePdfAsync(solicitarCodigoBarrasPDFRequest request)
        {
            try
            {
                //TODO - Voltar no método quando o WS estiver finalizado
                var response = await wsClient.solicitarCodigoBarrasPDFAsync(
                    request.cpfCnpjIE,
                    request.numeroDocumento,
                    request.valorTotal,
                    request.dataVencimento,
                    request.periodoReferencia,
                    request.codigoOrgao,
                    request.codigoReceita,
                    request.tokenAcesso);

                ResponseBarCodeDarService codigoBarrasResponse = new ResponseBarCodeDarService()
                {
                    BarCode = response.@return.codigoBarras,
                    ControlNumber = response.@return.numeroControle,
                    DigitableLine = response.@return.linhaDigitavel,
                    DocumentNumber = response.@return.numeroDocumento,
                    Message = response.@return.mensagemRetorno1,
                    MessageType = response.@return.tipoRetorno,
                    Base64 = response.@return.boletoBytes
                };

                return codigoBarrasResponse;
            }
            catch (System.Exception ex)
            {
                return new ResponseBarCodeDarService();
            }
        }

        public async Task<ResponseGetBilletPeriodReference> GetDocumentsByPeriodReferenceAsync(RequestGetBilletPeriodReference request)
        {
            try
            {
                var response = await wsClientRead.consultarDocumentosEmitidosPorPeriodoReferenciaAsync(request.ReferencePeriod, request.CodeOrgan, request.AccessToken);

                var result = new ResponseGetBilletPeriodReference();
                result.TypeReturn = response.@return.tipoRetorno;
                result.MessageReturn = response.@return.mensagemRetorno1;

                foreach (var bl in response.@return.boleto)
                {
                    result.Billets.Add(new ResponseGetBilletPeriodReferenceBillet()
                    {
                        BarCode = bl.codigoBarras,
                        ControlNumber = bl.numeroControle,
                        DocumentNumber = bl.numeroDocumento,
                        PaidOut = bl.pago.Equals("false") ? false : true
                    });
                }

                return result;
            }
            catch (System.Exception ex)
            {
                return new ResponseGetBilletPeriodReference() { TypeReturn = "ERRO", MessageReturn = ex.Message };
            }
        }

        public async Task<ResponseGetDarIcms> RequestDarIcmsAsync(solicitarDarIcmsRequest request)
        {
            try
            {
                ResponseGetDarIcms resultQuery = null;

                var response = await wsClient.solicitarDarIcmsAsync(request.codigoReceita, request.inscricao, request.substitoTributo, request.periodoReferencia, request.dataVencimento, request.dataPagamento, request.taxaEmissao, request.valorTotal, request.numeroDocumento, request.codigoOrgao, request.tokenAcesso);
                if (!response.@return.tipoRetorno.Equals("SUCESSO"))
                    throw new System.Exception("Houve uma falha na consulta do SEFAZ: " + response.@return.mensagemRetorno1);

                resultQuery = new ResponseGetDarIcms()
                {
                    BoletoBytes = response.@return.boletoBytes,
                    CodigoBarras = response.@return.codigoBarras,
                    LinhaDigitavel = response.@return.linhaDigitavel,
                    MensagemRetorno = response.@return.mensagemRetorno1,
                    NumeroControle = response.@return.numeroControle,
                    NumeroDocumento = response.@return.numeroDocumento,
                    TipoRetorno = response.@return.tipoRetorno
                };

                return resultQuery;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
    }
}