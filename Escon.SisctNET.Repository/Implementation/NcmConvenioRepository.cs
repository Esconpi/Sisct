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

        public bool FindByNcmAnnex(List<NcmConvenio> ncms, int Annex, string ncm, Log log = null)
        {
            var ncmsAll = ncms.Where(_ => _.AnnexId.Equals(Annex)).Select(_ => _.Ncm);
            bool ncmIncentivo = false;
            foreach (var n in ncmsAll)
            {
                int contaChar = n.Length;
                string substring = "";
                if (contaChar < 8 && ncm.Length > contaChar)
                {
                    substring = ncm.Substring(0, contaChar);
                }
                else
                {
                    substring = ncm;
                }

                if (n.Equals(substring))
                {
                    ncmIncentivo = true;
                    break;
                }
            }
            return ncmIncentivo;
        }
    }
}
