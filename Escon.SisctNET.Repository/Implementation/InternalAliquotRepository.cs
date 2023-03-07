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
            var result = aliquots.Where(_ => ((DateTime.Compare(_.DateStart, data) <= 0 && _.DateEnd == null) ||
                                              (DateTime.Compare(_.DateStart, data) <= 0 && DateTime.Compare(Convert.ToDateTime(_.DateEnd), data) > 0)) &&
                                             _.State.UF.Equals(uf))
                                 .FirstOrDefault();
            AddLog(log);
            return result;
        }
    }
}
