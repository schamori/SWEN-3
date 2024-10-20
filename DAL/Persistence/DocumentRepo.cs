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
        private readonly IMapper _mapper;

        public DocumentRepo(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<Document> Get()
        {

            return _context.Documents.ToList();
        }

        public DocumentDTO Create(CreateDocumentDTO documentDto)
        {
            var documentEntity = _mapper.Map<Document>(documentDto);

            documentEntity.CreatedAt = DateTime.UtcNow;
            documentEntity.UpdatedAt = DateTime.UtcNow;
            _context.Documents.Add(documentEntity);
            _context.SaveChanges();

            // Mapping des gespeicherten Dokumentes zurück zu DocumentDTO
            return _mapper.Map<DocumentDTO>(documentEntity);
        }


        public DocumentDTO Read(int id)
        {
            var documentEntity = _context.Documents.FirstOrDefault(d => d.Id == id);

            if (documentEntity == null)
            {
                return null;
            }

            return _mapper.Map<DocumentDTO>(documentEntity);
        }

        public void Update(DocumentDTO documentDto)
        {
            var documentEntity = _context.Documents.FirstOrDefault(d => d.Id == documentDto.Id);

            if (documentEntity != null)
            {
                _mapper.Map(documentDto, documentEntity);
                _context.Entry(documentEntity).State = EntityState.Modified;
                _context.SaveChanges();
            }
        }

        // Delete operation
        public void Delete(int id)
        {
            // Find the document entity by id
            var documentEntity = _context.Documents.FirstOrDefault(d => d.Id == id);

            if (documentEntity != null)
            {
                _context.Documents.Remove(documentEntity);
                _context.SaveChanges();
            }
        }
    }
}
