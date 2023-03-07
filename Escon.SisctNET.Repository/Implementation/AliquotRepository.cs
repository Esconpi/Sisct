using System;
using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class AliquotRepository : Repository<Model.Aliquot>, IAliquotRepository
    {
        private readonly ContextDataBase _context;

        public AliquotRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public Aliquot FindByAliquot(long stateOrigemId, long stateDestinoId, Log log = null)
        {
            var rst = _context.Aliquots.Where(_ => _.StateOrigemId.Equals(stateOrigemId) && _.StateDestinoId.Equals(stateDestinoId) && _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<Aliquot> FindByAllState(Log log = null)
        {
            var rst = _context.Aliquots
                .Include(so => so.StateOrigem)
                .Include(sd =>  sd.StateDestino)
                .ToList();
            AddLog(log);
            return rst;
        }

        public Aliquot FindByUf(string uf, Log log = null)
        {
            var rst = _context.Aliquots.Where(_ => _.StateOrigem.UF.Equals(uf)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public Aliquot FindByUf(List<Aliquot> aliquots, DateTime data, string ufOrigem, string ufDestino, Log log = null)
        {
            var result = aliquots.Where(_ => ((DateTime.Compare(_.DateStart, data) <= 0 && _.DateEnd == null) ||
                                                   (DateTime.Compare(_.DateStart, data) <= 0 && DateTime.Compare(Convert.ToDateTime(_.DateEnd), data) > 0)) &&
                                                  _.StateOrigem.UF.Equals(ufOrigem) && _.StateDestino.UF.Equals(ufDestino))
                                      .FirstOrDefault();
            AddLog(log);
            return result;
        }

        public Aliquot FindByUf(string ufOrigem, string ufDestino, DateTime data, Log log = null)
        {
            var rst = _context.Aliquots
                .Where(_ => _.StateOrigem.UF.Equals(ufOrigem) && _.StateDestino.UF.Equals(ufDestino) && 
                ((DateTime.Compare(_.DateStart, data) <= 0 && _.DateEnd == null) || (DateTime.Compare(_.DateStart, data) <= 0 && 
                DateTime.Compare(Convert.ToDateTime(_.DateEnd), data) > 0)))
                .FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
