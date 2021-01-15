using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class PhotoDto
    {
        
        private PhotoDto()
        {
            Tags = new List<string>();
        }

        public PhotoDto(Guid id, string filename, string description): this()
        {
            Id = id;
            Filename = filename;
            Description = description;
        }

        public Guid Id { get; private set; }
        public string Filename { get; private set; }
        public string Description { get; private set; }
        public List<string> Tags { get; private set; }

        public void AddTag(string tagName)
        {
            Tags.Add(tagName);
        }
    }
}
