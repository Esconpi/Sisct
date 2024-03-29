﻿using Escon.SisctNET.Model.DarWebWs;
using IntegrationDarService;
using System.Threading.Tasks;

namespace Escon.SisctNET.IntegrationDarWeb
{
    public interface IIntegrationWsDar
    {
        Task<ResponseBarCodeDarService> GetBarCodeAsync(solicitarCodigoBarrasRequest request);

        Task<ResponseGetDarIcms> RequestDarIcmsAsync(solicitarDarIcmsRequest request);

        Task<ResponseBarCodeDarService> GetBarCodePdfAsync(solicitarCodigoBarrasPDFRequest request);

        Task<ResponseGetBilletPeriodReference> GetDocumentsByPeriodReferenceAsync(RequestGetBilletPeriodReference request);
    }
}