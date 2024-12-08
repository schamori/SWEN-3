using NPaperless.SearchLibrary;
using SharedResources.Entities;

namespace ElasticSearch;

public interface ISearchIndex
{
    void AddDocumentAsync(DocumentOcr doc);
    IEnumerable<DocumentOcr> SearchDocumentAsync(string searchTerm);
}


