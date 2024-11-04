using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedResources.DTO;
using SharedResources.Entities;

namespace BL.Services
{
    public interface IDocumentServices
    {
        DocumentBl GetDocumentById(Guid id);
        DocumentBl CreateDocument(DocumentBl documentDto);
        List<DocumentBl> GetDocuments();
        //DocumentDTO UpdateDocument(DocumentBl documentDto);
        bool DeleteDocument(Guid id);
        void SendToQueue(string filePath, Guid documentId);
    }
  

}
