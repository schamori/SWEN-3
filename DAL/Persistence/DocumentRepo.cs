using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DAL.Entities;
using DAL.DTO;
using Microsoft.EntityFrameworkCore;

namespace DAL.Persistence
{
    public class DocumentRepo : IDocumentRepo
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepo(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
        }

        public List<DocumentDAL> Get()
        {

            return _context.Documents.ToList();
        }

        public DocumentDAL Create(DocumentDAL document)
        {

            document.CreatedAt = DateTime.UtcNow;
            document.UpdatedAt = DateTime.UtcNow;
            _context.Documents.Add(document);
            _context.SaveChanges();

            return document;
        }


        public DocumentDAL Read(Guid id)
        {
            var documentEntity = _context.Documents.FirstOrDefault(d => d.Id == id);

            if (documentEntity == null)
            {
                return null;
            }

            return documentEntity;
        }

        public void Update(DocumentDAL document)
        {
            var foundDocument = _context.Documents.FirstOrDefault(d => d.Id == document.Id);

            if (foundDocument != null)
            {
                _context.Entry(foundDocument).State = EntityState.Modified;
                _context.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            var documentEntity = _context.Documents.FirstOrDefault(d => d.Id == id);

            if (documentEntity != null)
            {
                _context.Documents.Remove(documentEntity);
                _context.SaveChanges();
            }
        }
    }
}
