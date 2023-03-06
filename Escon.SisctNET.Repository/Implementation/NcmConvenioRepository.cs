using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
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

        public List<NcmConvenio> FindAllInDate(List<NcmConvenio> ncms, DateTime dateNCM, Log log = null)
        {
            List<NcmConvenio> ncmTemp = new List<NcmConvenio>();

            foreach (var ncm in ncms)
            {
                var dataInicial = DateTime.Compare(ncm.DateStart, dateNCM);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(ncm.DateEnd), dateNCM);

                if (dataInicial <= 0 && ncm.DateEnd == null)
                {
                    ncmTemp.Add(ncm);
                    continue;
                }
                else if (dataInicial <= 0 && dataFinal > 0)
                {
                    ncmTemp.Add(ncm);
                    continue;
                }
            }

            return ncmTemp;
        }

        public List<NcmConvenio> FindByAnnex(long annexId, Log log = null)
        {
            var result = _context.NcmConvenios.Where(_ => _.AnnexId.Equals(annexId)).ToList();
            AddLog(log);
            return result;
        }

        public List<NcmConvenio> FindByAnnex(Log log = null)
        {
            var result = _context.NcmConvenios.Include(_ => _.Annex).ToList();
            AddLog(log);
            return result;
        }

        public List<NcmConvenio> FindByNcmAnnex(long annexId, Log log = null)
        {
            var result = _context.NcmConvenios.Where(_ => _.AnnexId.Equals(annexId)).ToList();
            AddLog(log);
            return result;
        }

        public NcmConvenio FindByNcmAnnex(List<NcmConvenio> ncms, string ncm, Log log = null)
        {
            foreach (var n in ncms)
            {
                int contaChar = n.Ncm.Length;
                string substring = "";

                if (contaChar < 8 && ncm.Length > contaChar)
                    substring = ncm.Substring(0, contaChar);
                else
                    substring = ncm;

                if (n.Ncm.Equals(substring) && !contaChar.Equals(0))
                    return n;
            }

            return null;
        }

        public NcmConvenio FindByNcmAnnex(List<NcmConvenio> ncms, string ncm, string cest, Company comp, Log log = null)
        {
            string cestBase = null;

            if (cest != "")
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

                if (comp.AnnexId.Equals((long)3) || comp.AnnexId.Equals((long)4))
                {
                    if (n.Ncm.Equals(substring))
                        return n;
                }
                else
                {

                    if (n.Ncm.Equals(substring) && cestTemp == cestBase)
                        return n;

                    if (substring != "" && (n.Ncm != "" || n.Ncm != null))
                    {
                        if (n.Ncm.Equals(substring))
                            return n;
                    }


                    if (cestBase != "" && cestBase != null && cestTemp != "")
                    {
                        if (cestTemp == cestBase)
                            return n;
                    }
                }

            }

            return null;
        }

        public bool FindByNcmExists(List<NcmConvenio> ncms, string ncm,  string cest, Company comp, Log log = null)
        {
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

                if (comp.AnnexId.Equals((long)3) || comp.AnnexId.Equals((long)4))
                {
                    if (n.Ncm.Equals(substring))
                        return true;
                }
                else
                {

                    if (n.Ncm.Equals(substring) && cestTemp == cestBase)
                        return true;

                    if (substring != "" && (n.Ncm != "" || n.Ncm != null))
                    {
                        if (n.Ncm.Equals(substring))
                            return true;
                    }

                   
                    if (cestBase != "" && cestBase != null && cestTemp != "")
                    {
                        if (cestTemp == cestBase)
                            return true;
                    }
                }
               
            }

            return false;
        }

        public bool FindByNcmExists(List<NcmConvenio> ncms, string ncm, Log log = null)
        {
            foreach (var n in ncms)
            {
                int contaChar = n.Ncm.Length;
                string substring = "";

                if (contaChar < 8 && ncm.Length > contaChar)
                    substring = ncm.Substring(0, contaChar);
                else
                    substring = ncm;

                if (n.Ncm.Equals(substring) && !contaChar.Equals(0))
                    return true;
            }

            return false;
        }
    }
}
