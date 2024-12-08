using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedResources.Entities
{
    public class DocumentOcr
    {
        public required Guid Id { get; set; }
        public required string Title { get; set; }


        public required string Content { get; set; }

    }
}
