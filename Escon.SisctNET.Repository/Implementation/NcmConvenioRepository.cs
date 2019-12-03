using System;
using System.Collections.Generic;
using System.Text;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class NcmConvenioRepository : Repository<Model.NcmConvenio>, INcmConvenioRepository
    {
        private readonly ContextDataBase _context;

        public NcmConvenioRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
