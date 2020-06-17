using Escon.SisctNET.Model.DarWebWs;
using IntegrationDarService;
using System.Threading.Tasks;

namespace Escon.SisctNET.IntegrationDarWeb
{
    public interface IIntegrationWsDar
    {
        Task<ResponseBarCodeDarService> GetBarCodeAsync(solicitarCodigoBarrasRequest request);
    }
}