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

        public List<string> FindByAnnex(long annexId, Log log = null)
        {
            var result = _context.NcmConvenios.Where(_ => _.AnnexId.Equals(annexId)).Select(_ => _.Ncm).ToList();
            AddLog(log);
            return result;
        }

        public List<NcmConvenio> FindByNcmAnnex(long annexId, Log log = null)
        {
            var result = _context.NcmConvenios.Where(_ => _.AnnexId.Equals(annexId)).ToList();
            AddLog(log);
            return result;
        }

        public bool FindByNcmAnnex(List<NcmConvenio> ncms, string ncm,  string cest, Log log = null)
        {
            bool ncmIncentivo = false;
            string cestBase = null;

            if(cest != "")
                cestBase = cest;

            foreach (var n in ncms)
            {
                int contaChar = n.Ncm.Length;
                string substring = "";

                if (contaChar < 8 && ncm.Length > contaChar)
                    substring = ncm.Substring(0, contaChar);
                else
                    substring = ncm;

                string cestTemp = n.Cest;

                if (n.Cest == null || n.Cest == "")
                    cestTemp = null;

                if (n.Ncm.Equals(substring) && cestTemp == cestBase)
                {
                    ncmIncentivo = true;
                    break;
                }
            }

            return ncmIncentivo;
        }
    }
}
