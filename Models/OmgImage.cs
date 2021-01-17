﻿using System;
using System.Collections.Generic;

namespace API.Models
{
    public class OmgImage
    {
        private OmgImage()
        {
            Tags = new List<OmgImageTag>();
        }
        public OmgImage(string filename, string title) : this()
        {
            Filename = filename;
            Title = title;
        }
        public Guid Id { get; private set; }
        public string Filename { get; private set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<OmgImageTag> Tags { get; private set; }

        public void AddTag(string tag)
        {
            Tags.Add(new OmgImageTag(tag));
        }

        public void RemoveTag(Predicate<OmgImageTag> filterExpression)
        {
            Tags.RemoveAll(filterExpression);
        }
        public void RemoveTag(string tagToRemove)
        {
            RemoveTag(tag => tag.Name == tagToRemove);
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
