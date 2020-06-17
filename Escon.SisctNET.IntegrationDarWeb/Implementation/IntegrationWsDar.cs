using Escon.SisctNET.Model.DarWebWs;
using IntegrationDarService;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Escon.SisctNET.IntegrationDarWeb.Implementation
{
    public class IntegrationWsDar : IIntegrationWsDar
    {
        private readonly DocumentoArrecadacaoWSClient wsClient;
        private readonly IConfiguration _configuration;

        public IntegrationWsDar(IConfiguration configuration)
        {
            _configuration = configuration;

            var urlBase = _configuration["Sefaz:Url"];

            wsClient = new DocumentoArrecadacaoWSClient(DocumentoArrecadacaoWSClient.EndpointConfiguration.DocumentoArrecadacaoWSPort, urlBase);
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
    }
}