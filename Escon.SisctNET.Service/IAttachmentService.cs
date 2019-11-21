using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IAttachmentService : IServiceBase<Model.Attachment>
    {
        void Create(List<Model.Attachment> anexos, Model.Log log = null);

        Model.Attachment FindByDescription(string description, Model.Log log = null);
    }
}
