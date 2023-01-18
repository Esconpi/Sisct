using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class InternalAliquotRepository : Repository<Model.InternalAliquot>, IInternalAliquotRepository
    {
        private readonly ContextDataBase _context;

        public InternalAliquotRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public InternalAliquot FindByAliquot(long stateId, Log log = null)
        {
            var rst = _context.InternalAliquots.Where(_ => _.StateId.Equals(stateId) && _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<InternalAliquot> FindByAllState(Log log = null)
        {
            var rst = _context.InternalAliquots
                .Include(s => s.State)
                .ToList();
            AddLog(log);
            return rst;
        }

        public InternalAliquot FindByUf(List<InternalAliquot> aliquots, DateTime data, string uf, Log log = null)
        {
            InternalAliquot result = null;

            var statesAll = aliquots.Where(_ => _.State.UF.Equals(uf)).ToList();

            foreach (var t in statesAll)
            {
                var dataInicial = DateTime.Compare(t.DateStart, data);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(t.DateEnd), data);

                if (dataInicial <= 0 && t.DateEnd == null)
                {
                    result = t;
                    break;
                }
                else if (dataInicial <= 0 && dataFinal > 0)
                {
                    result = t;
                    break;
                }

            }
            AddLog(log);
            return result;
        }
    }
}
