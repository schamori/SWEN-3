using SharedResources.Entities;

namespace DAL.Persistence
{
    public interface IDocumentRepo
    {
        List<DocumentDAL> Get();
        DocumentDAL Create(DocumentDAL documentDto);
        DocumentDAL Read(Guid id);
        DocumentDAL Update(DocumentDAL documentDto);
        void Delete(Guid id);
    }
}
