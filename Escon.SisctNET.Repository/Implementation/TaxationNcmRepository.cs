using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public void Create(List<TaxationNcm> taxationNcms, Log log = null)
        {
            foreach (var t in taxationNcms)
            {
                _context.TaxationNcms.Add(t);
            }

            AddLog(log);
            _context.SaveChanges();
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

        public List<TaxationNcm> FindAllInDate(List<TaxationNcm> ncms, DateTime dateProd, Log log = null)
        {
            List<TaxationNcm> ncmsAll = new List<TaxationNcm>();

            foreach (var ncm in ncms)
            {
                var dataInicial = DateTime.Compare(Convert.ToDateTime(ncm.DateStart), dateProd);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(ncm.DateEnd), dateProd);

                if ((dataInicial <= 0 || ncm.DateStart == null) && ncm.DateEnd == null)
                {
                    ncmsAll.Add(ncm);
                    continue;
                }
                else if ((dataInicial <= 0 || ncm.DateStart == null) && dataFinal > 0)
                {
                    ncmsAll.Add(ncm);
                    continue;
                }
            }

            return ncmsAll;
        }

        public List<TaxationNcm> FindByCompany(string company, Log log = null)
        {
            var rst = _context.TaxationNcms.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8))).ToList();
            AddLog(log);
            return rst;
        }

        public List<TaxationNcm> FindByCompany(int company, Log log = null)
        {
            var rst = _context.TaxationNcms.Where(_ => _.CompanyId.Equals(company)).ToList();
            AddLog(log);
            return rst;
        }

        public List<TaxationNcm> FindByCompany(int company, string year, string month, Log log = null)
        {
            var rst = _context.TaxationNcms.Where(_ => _.CompanyId.Equals(company) && _.Year.Equals(year) && _.Month.Equals(month)).ToList();
            AddLog(log);
            return rst;
        }

        public List<TaxationNcm> FindByCompany(Log log = null)
        {
            var rst = _context.TaxationNcms.Where(_ => !_.Company.Taxation).ToList();
            AddLog(log);
            return rst;
        }

        public List<TaxationNcm> FindByPeriod(List<TaxationNcm> taxationNcms, DateTime inicio, DateTime fim, Log log = null)
        {
            List<TaxationNcm> ncms = new List<TaxationNcm>();

            foreach (var ncm in taxationNcms)
            {
                if(ncm.DateEnd == null && (fim >= ncm.DateStart || ncm.DateStart == null))
                {
                    ncms.Add(ncm);
                }
                else
                {
                    if (inicio >= ncm.DateStart && fim <= ncm.DateEnd)
                    {
                        ncms.Add(ncm);
                    }
                }

            }

            return ncms;
        }

        public List<TaxationNcm> FindMono(int typeCompany, Log log = null)
        {
            List<TaxationNcm> ncms = new List<TaxationNcm>();

            if (typeCompany.Equals(1))
            {
                ncms = _context.TaxationNcms.Where(_ => _.Company.CountingTypeId.Equals(1) && _.Type.Equals("Monofásico")).ToList();
            }
            else
            {
                ncms = _context.TaxationNcms.Where(_ => (_.Company.CountingTypeId.Equals(2) || _.Company.CountingTypeId.Equals(3)) && _.Type.Equals("Monofásico")).ToList();
            }
            return ncms;
        }

        public void Update(List<TaxationNcm> taxationNcms, Log log = null)
        {
            foreach (var t in taxationNcms)
            {
                _context.TaxationNcms.Update(t);
            }

            AddLog(log);
            _context.SaveChanges();
        }
    }
}
