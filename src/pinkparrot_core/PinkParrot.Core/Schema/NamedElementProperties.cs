// ==========================================================================
//  NamedElementProperties.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using PinkParrot.Infrastructure;

namespace PinkParrot.Core.Schema
{
    public abstract class NamedElementProperties
    {
        private readonly string name;
        private readonly string label;
        private readonly string hints;

        public string Name
        {
            get { return name; }
        }

        public string Label
        {
            get { return string.IsNullOrWhiteSpace(label) ? name : label; }
        }

        public string Hints
        {
            get { return hints; }
        }

        protected NamedElementProperties(
            string name, 
            string label, 
            string hints)
        {
            this.name = name;
            this.label = label;
            this.hints = hints;
        }

        public virtual void Validate(IList<ValidationError> errors)
        {
            Guard.NotNull(errors, nameof(errors));

            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add(new ValidationError("Name cannot be empty.", "Name"));
            }
            else if (!Name.IsSlug())
            {
                errors.Add(new ValidationError("Name must be a valid slug.", "Name"));
            }
        }
    }
}