using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IAttachmentRepository : IRepository<Model.Attachment>
    {
        void Create(List<Model.Attachment> attachments, Model.Log log = null);

        Model.Attachment FindByDescription(string description, Model.Log log = null);
    }
}
