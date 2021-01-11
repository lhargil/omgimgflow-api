using System;
using System.Collections.Generic;

namespace API.Models
{
    public class OmgImage
    {
        private OmgImage()
        {
            Tags = new List<OmgImageTag>();
        }
        public OmgImage(string filename) : this()
        {
            Filename = filename;
        }
        public Guid Id { get; private set; }
        public string Filename { get; private set; }
        public string Description { get; set; }
        public ICollection<OmgImageTag> Tags { get; private set; }

        public void AddTag(string tag)
        {
            Tags.Add(new OmgImageTag(tag));
        }

        public void RemoveTag(string tag)
        {
            Tags.Remove(new OmgImageTag(tag));
        }
    }

    public class OmgImageTag
    {
        private OmgImageTag()
        {
            
        }

        public OmgImageTag(string name)
        {
            Name = name;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
    }
}
