using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
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
            var rst = _context.TaxationNcms
                .Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)))
                .Include(n => n.Ncm)
                .Include(t => t.TaxationTypeNcm)
                .ToList();
            AddLog(log);
            return rst;
        }

        public List<TaxationNcm> FindByCompany(long company, Log log = null)
        {
            var rst = _context.TaxationNcms
                .Where(_ => _.CompanyId.Equals(company))
                .Include(n => n.Ncm)
                .Include(t => t.TaxationTypeNcm)
                .ToList();
            AddLog(log);
            return rst;
        }

        public List<TaxationNcm> FindByCompany(long company, string year, string month, Log log = null)
        {
            var rst = _context.TaxationNcms
                .Where(_ => _.CompanyId.Equals(company) && _.Year.Equals(year) && _.Month.Equals(month))
                .Include(n => n.Ncm)
                .Include(t => t.TaxationTypeNcm)
                .ToList();
            AddLog(log);
            return rst;
        }

        public List<TaxationNcm> FindByGeneral(Log log = null)
        {
            var rst = _context.TaxationNcms
                .Include(n => n.Ncm)
                .Include(t => t.TaxationTypeNcm)
                .ToList();
            AddLog(log);
            return rst;
        }

        public List<TaxationNcm> FindByNcms(List<TaxationNcm> ncms, string ncmRaiz, Log log = null)
        {
            List<TaxationNcm> result = new List<TaxationNcm>();
            ncmRaiz = ncmRaiz.Replace(".", "");

            int contaChar = ncmRaiz.Length;
            foreach (var n in ncms)
            {
                string substring = "";
                if (contaChar < 8 && n.Ncm.Code.Length > contaChar)
                {
                    substring = n.Ncm.Code.Substring(0, contaChar);
                }
                else
                {
                    substring = n.Ncm.Code;
                }

                if (ncmRaiz.Equals(substring))
                {
                    result.Add(n);
                }
            }

            return result;
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

        public List<TaxationNcm> FindMono(long typeCompany, Log log = null)
        {
            List<TaxationNcm> ncms = new List<TaxationNcm>();

            if (typeCompany.Equals(1))
            {
                ncms = _context.TaxationNcms
                    .Where(_ => _.Company.CountingTypeId.Equals(1) && (_.TaxationTypeNcmId.Equals(2) || _.TaxationTypeNcmId.Equals(3) || _.TaxationTypeNcmId.Equals(4)))
                    .Include(n => n.Ncm)
                    .Include(t => t.TaxationTypeNcm)
                    .ToList();
            }
            else
            {
                ncms = _context.TaxationNcms
                    .Where(_ => (_.Company.CountingTypeId.Equals(2) || _.Company.CountingTypeId.Equals(3)) && (_.TaxationTypeNcmId.Equals(2) || _.TaxationTypeNcmId.Equals(3) || _.TaxationTypeNcmId.Equals(4)))
                    .Include(n => n.Ncm)
                    .Include(t => t.TaxationTypeNcm)
                    .ToList();
            }
            return ncms;
        }
    }
}
