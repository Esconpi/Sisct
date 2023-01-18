using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class AttachmentRepository : Repository<Attachment>, IAttachmentRepository
    {
        private readonly ContextDataBase _context;

        public AttachmentRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public Attachment FindByDescription(string description, Log log = null)
        {
            var rst = _context.Attachments.Where(_ => _.Description.Equals(description)).FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
