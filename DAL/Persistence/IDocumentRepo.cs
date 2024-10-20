using DAL.DTO;
using DAL.Entities;

namespace DAL.Persistence
{
    public interface IDocumentRepo
    {
        List<Document> Get();
        DocumentDTO Create(CreateDocumentDTO documentDto);
        DocumentDTO Read(int id);
        void Update(DocumentDTO documentDto);
        void Delete(int id);
    }
}
