using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class AliquotConfazRepository : Repository<Model.AliquotConfaz>, IAliquotConfazRepository
    {
        private readonly ContextDataBase _context;

        public AliquotConfazRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public AliquotConfaz FindByUf(List<AliquotConfaz> aliquotConfazs, DateTime data, string ufOrigem, string ufDestino, long annexId, Log log = null)
        {
            var ufs = aliquotConfazs.Where(_ => _.StateOrigem.UF.Equals(ufOrigem) && _.StateDestino.UF.Equals(ufDestino) && _.AnnexId.Equals(annexId))
                                    .ToList();
            var result = ufs.Where(_ => ((DateTime.Compare(_.DateStart, data) <= 0 && _.DateEnd == null) ||
                                         (DateTime.Compare(_.DateStart, data) <= 0 && DateTime.Compare(Convert.ToDateTime(_.DateEnd), data) >= 0)))
                                       .FirstOrDefault();
            AddLog(log);
            return result;
        }

        public AliquotConfaz FindByAliquot(long stateOrigemId, long stateDestinoId, long annexId, Log log = null)
        {
            var rst = _context.AliquotConfazs.Where(_ => _.StateOrigemId.Equals(stateOrigemId) && _.StateDestinoId.Equals(stateDestinoId) && _.AnnexId.Equals(annexId) && _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<AliquotConfaz> FindByAllState(Log log = null)
        {
            var rst = _context.AliquotConfazs
                .Include(so => so.StateOrigem)
                .Include(sd => sd.StateDestino)
                .ToList();
            AddLog(log);
            return rst;
        }
    }
}
