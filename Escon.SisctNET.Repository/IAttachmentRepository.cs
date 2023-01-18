namespace Escon.SisctNET.Repository
{
    public interface IAttachmentRepository : IRepository<Model.Attachment>
    {
        Model.Attachment FindByDescription(string description, Model.Log log = null);
    }
}
