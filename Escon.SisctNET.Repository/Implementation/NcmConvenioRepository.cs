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

        public List<NcmConvenio> FindAllInDate(List<NcmConvenio> ncms, DateTime data, Log log = null)
        {
            var result = ncms.Where(_ => (DateTime.Compare(_.DateStart, data) <= 0 && _.DateEnd == null) ||
                                         (DateTime.Compare(_.DateStart, data) <= 0 && DateTime.Compare(Convert.ToDateTime(_.DateEnd), data) >= 0))
                             .ToList();
            AddLog(log);
            return result;
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
                if (n.Ncm == null)
                    n.Ncm = "";

                int contaChar = n.Ncm.Length;
                string substring = ncm;

                if (contaChar < 8 && ncm.Length > contaChar)
                    substring = ncm.Substring(0, contaChar);

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
                if (n.Ncm == null)
                    n.Ncm = "";

                int contaChar = n.Ncm.Length;
                string substring = ncm;

                if (contaChar < 8 && ncm.Length > contaChar)
                    substring = ncm.Substring(0, contaChar);

                string cestTemp = n.Cest;

                if (n.Cest == null || n.Cest == "")
                    cestTemp = null;

                if (comp.Annex.Description.Equals("NENHUM"))
                {
                    if (n.Ncm.Equals(substring) && cestTemp == cestBase)
                        return n;

                    if (n.Ncm.Equals(substring))
                        return n;

                    if (cestBase != "" && cestBase != null && cestTemp != "")
                    {
                        if (cestTemp == cestBase)
                            return n;
                    }
                }
                else
                {
                    if (comp.Annex.Description.Equals("ANEXO ÚNICO") || comp.Annex.Description.Equals("ANEXO CCCXXVI (Art. 791 - A)") || 
                        comp.Annex.Description.Equals("ANEXO VII (Art. 59 Parte 1) - REGIMES ESPECIAIS DE TRIBUTAÇÃO"))
                    {
                        if (n.Ncm.Equals(substring))
                            return n;
                    }
                    else
                    {
                        if (n.Ncm.Equals(substring) && cestTemp == cestBase)
                            return n;

                        if (n.Ncm.Equals(substring))
                            return n;

                        if (cestBase != "" && cestBase != null && cestTemp != "")
                        {
                            if (cestTemp == cestBase)
                                return n;
                        }
                    }

                }

            }

            return null;
        }

        public bool FindByNcmExists(List<NcmConvenio> ncms, string ncm,  string cest, Company comp, Log log = null)
        {
            string cestBase = null;

            if (cest != "")
                cestBase = cest;

            foreach (var n in ncms)
            {
                if (n.Ncm == null)
                    n.Ncm = "";

                int contaChar = n.Ncm.Length;
                string substring = ncm;

                if (contaChar < 8 && ncm.Length > contaChar)
                    substring = ncm.Substring(0, contaChar);

                string cestTemp = n.Cest;

                if (n.Cest == null || n.Cest == "")
                    cestTemp = null;

                if (comp.Annex.Description.Equals("ANEXO ÚNICO") || comp.Annex.Description.Equals("ANEXO CCCXXVI (Art. 791 - A)") || 
                    comp.Annex.Description.Equals("ANEXO VII (Art. 59 Parte 1) - REGIMES ESPECIAIS DE TRIBUTAÇÃO"))
                {
                    if (n.Ncm.Equals(substring))
                        return true;
                }
                else
                {

                    if (n.Ncm.Equals(substring) && cestTemp == cestBase)
                        return true;

                    if (n.Ncm.Equals(substring))
                        return true;
                   
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
                string substring = ncm;

                if (contaChar < 8 && ncm.Length > contaChar)
                    substring = ncm.Substring(0, contaChar);

                if (n.Ncm.Equals(substring) && !contaChar.Equals(0))
                    return true;
            }

            return false;
        }
    }
}
