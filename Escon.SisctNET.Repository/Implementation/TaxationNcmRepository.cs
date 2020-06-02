using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxationNcmRepository : Repository<Model.TaxationNcm> , ITaxationNcmRepository
    {
        private readonly ContextDataBase _context;

        public TaxationNcmRepository(ContextDataBase context, IConfiguration configuration)
          : base(context, configuration)
        {
            _context = context;
        }

        public List<TaxationNcm> FindAllInDate(DateTime dateProd, Log log = null)
        {
            List<TaxationNcm> ncms = new List<TaxationNcm>();

            var ncmsMono = _context.TaxationNcms;

            foreach (var ncm in ncmsMono)
            {
                var dataInicial = DateTime.Compare(Convert.ToDateTime(ncm.DateStart), dateProd);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(ncm.DateEnd), dateProd);

                if ((dataInicial <= 0 || ncm.DateStart == null ) && ncm.DateEnd == null)
                {
                    ncms.Add(ncm);
                    continue;
                }
                else if ((dataInicial <= 0 || ncm.DateStart == null) && dataFinal > 0)
                {
                    ncms.Add(ncm);
                    continue;
                }
            }

            return ncms;
        }
    }
}
