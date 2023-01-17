﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxationRepository : Repository<Model.Taxation>, ITaxationRepository
    {
        private readonly ContextDataBase _context;
        

        public TaxationRepository(ContextDataBase context, IConfiguration configuration) 
            : base(context, configuration)
        {
            _context = context;
        }

        public void Create(List<Taxation> taxations, Log log = null)
        {
            foreach (var taxation in taxations)
            {
                _context.Taxations.Add(taxation);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public Taxation FindByCode(string code, string cest, DateTime data, Log log = null)
        {
            string dataFomart = data.ToString("yyyy-MM-dd");
            Taxation result = null;
            var taxations = _context.Taxations.Where(_ => _.Code.Equals(code) && _.Cest.Equals(cest));
            
            foreach(var t in taxations)
            {
                var dataInicial = DateTime.Compare(t.DateStart, data);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(t.DateEnd), data);

                if (dataInicial <= 0 && t.DateEnd == null)
                {
                    result = t;
                    break;
                }else if (dataInicial <= 0 && dataFinal > 0 )
                {
                    result = t;
                    break;
                }
                
            }
            AddLog(log);
            return result;
        }

        public Taxation FindByCode(List<Taxation> taxations, string code, string cest, DateTime data, Log log = null)
        {
            string dataFomart = data.ToString("yyyy-MM-dd");
            Taxation result = null;
            var taxationsAll = taxations.Where(_ => _.Code.Equals(code) && _.Cest.Equals(cest));

            foreach (var t in taxationsAll)
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

        public List<Taxation> FindByCompany(long companyId, Log log = null)
        {
            var rst = _context.Taxations
               .Where(_ => _.CompanyId.Equals(companyId))
               .Include(n => n.Ncm)
               .ToList();
            AddLog(log);
            return rst;
        }

        public List<Taxation> FindByCompanyActive(long companyId, Log log = null)
        {
            var rst = _context.Taxations
                .Where(_ => _.CompanyId.Equals(companyId) && (Convert.ToDateTime(_.DateStart) < Convert.ToDateTime(_.DateEnd) || _.DateEnd.Equals(null)))
                .Include(n => n.Ncm)
                .ToList();
            AddLog(log);
            return rst;
        }

        public Taxation FindByNcm(string code, string cest, Log log = null)
        {
            var rst = _context.Taxations.Where(_ => _.Code.Equals(code) && _.Cest.Equals(cest) && _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public void Update(List<Taxation> taxations, Log log = null)
        {
            foreach (var taxation in taxations)
            {
                _context.Taxations.Update(taxation);
            }

            AddLog(log);
            _context.SaveChanges();
        }
    }
}
