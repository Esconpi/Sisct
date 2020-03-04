using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class NcmConvenioRepository : Repository<Model.NcmConvenio>, INcmConvenioRepository
    {
        private readonly ContextDataBase _context;

        public NcmConvenioRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<string> FindByAnnex(int annexId, Log log = null)
        {
            var result = _context.NcmConvenios.Where(_ => _.AnnexId.Equals(annexId)).Select(_ => _.Ncm).ToList();
            AddLog(log);
            return result;
        }

        public List<NcmConvenio> FindByNcmAnnex(int annexId, Log log = null)
        {
            var result = _context.NcmConvenios.Where(_ => _.AnnexId.Equals(annexId)).ToList();
            AddLog(log);
            return result;
        }
    }
}
