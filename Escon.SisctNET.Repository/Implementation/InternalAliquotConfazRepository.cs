﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class InternalAliquotConfazRepository : Repository<Model.InternalAliquotConfaz>, IInternalAliquotConfazRepository
    {
        private readonly ContextDataBase _context;

        public InternalAliquotConfazRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public InternalAliquotConfaz FindByAliquot(long stateId, long annexId, Log log = null)
        {
            var rst = _context.InternalAliquotConfazs.Where(_ => _.StateId.Equals(stateId) && _.AnnexId.Equals(annexId) && _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<InternalAliquotConfaz> FindByAllState(Log log = null)
        {
            var rst = _context.InternalAliquotConfazs
               .Include(s => s.State)
               .ToList();
            AddLog(log);
            return rst;
        }

        public InternalAliquotConfaz FindByUf(List<InternalAliquotConfaz> internalAliquotConfazs, DateTime data, string uf, long annexId, Log log = null)
        {
            var ufs = internalAliquotConfazs.Where(_ => _.State.UF.Equals(uf) && _.AnnexId.Equals(annexId)).ToList();
            var result = ufs.Where(_ => ((DateTime.Compare(_.DateStart, data) <= 0 && _.DateEnd == null) ||
                                         (DateTime.Compare(_.DateStart, data) <= 0 && DateTime.Compare(Convert.ToDateTime(_.DateEnd), data) >= 0)))
                             .FirstOrDefault();
            AddLog(log);
            return result;
        }
    }
}
