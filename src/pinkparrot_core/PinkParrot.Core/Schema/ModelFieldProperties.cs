// ==========================================================================
//  ModelFieldProperties.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using PinkParrot.Infrastructure;

namespace PinkParrot.Core.Schema
{
    public abstract class ModelFieldProperties : NamedElementProperties, IModelFieldProperties
    {
        public bool IsRequired { get; }

        protected ModelFieldProperties(string label,  string hints, bool isRequired)
            : base(label, hints)
        {
            IsRequired = isRequired;
        }

        public void Validate(IList<ValidationError> errors)
        {
            ValidateCore(errors);
        }

        protected virtual void ValidateCore(IList<ValidationError> errors)
        {
        }
    }
}