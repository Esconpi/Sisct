namespace Escon.SisctNET.Service
{
    public interface IAttachmentService : IServiceBase<Model.Attachment>
    {
        Model.Attachment FindByDescription(string description, Model.Log log = null);
    }
}
