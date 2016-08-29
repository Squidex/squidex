// ==========================================================================
//  ModelSchemaMetadata.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;

namespace PinkParrot.Core.Schema
{
    public sealed class ModelSchemaMetadata
    {
        private string name;
        private string displayName;
        private string hint;
        private string itemTitle;

        public string Name
        {
            get { return name; }
        }

        public string DisplayName
        {
            get { return displayName; }
        }

        public string Hint
        {
            get { return hint; }
        }

        public string ItemTitle
        {
            get { return itemTitle; }
        }

        public ModelSchemaMetadata(string name)
        {
            Guard.ValidSlug(name, nameof(name));

            this.name = name;
        }

        public ModelSchemaMetadata Configure(string newName, PropertiesBag properties)
        {
            Guard.NotNull(properties, nameof(properties));

            var clone = (ModelSchemaMetadata) MemberwiseClone();

            if (newName != null)
            {
                if (!newName.IsSlug())
                {
                    throw new DomainValidationException("Cannot update the schema.", $"'{newName}' is not a valid slug.");
                }

                clone.name = newName;
            }

            if (properties.Contains("Hint"))
            {
                clone.hint = properties["Hint"].ToString()?.Trim();
            }

            if (properties.Contains("DisplayName"))
            {
                clone.displayName = properties["DisplayName"].ToString()?.Trim();
            }

            if (properties.Contains("ItemTitle"))
            {
                clone.itemTitle = properties["ItemTitle"].ToString()?.Trim();
            }

            return clone;
        }
    }
}