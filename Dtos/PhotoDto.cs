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

        public PhotoDto(Guid id, string filename, string title, string description, int width, int height): this()
        {
            Id = id;
            Filename = filename;
            Title = title;
            Description = description;
            Width = width;
            Height = height;
        }

        public Guid Id { get; private set; }
        public string Filename { get; private set; }
        public string Title { get; set; }
        public string Description { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<string> Tags { get; private set; }

        public void AddTag(string tagName)
        {
            Tags.Add(tagName);
        }
    }
}
