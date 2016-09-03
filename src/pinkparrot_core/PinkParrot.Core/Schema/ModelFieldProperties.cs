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
    public abstract class ModelFieldProperties : NamedElementProperties
    {
        public bool IsRequired { get; }

        protected ModelFieldProperties(
            bool isRequired, 
            string name, 
            string label, 
            string hints)
            : base(name, label, hints)
        {
            IsRequired = isRequired;
        }

        public override void Validate(IList<ValidationError> errors)
        {       
            base.Validate(errors);

            ValidateCore(errors);
        }

        protected virtual void ValidateCore(IList<ValidationError> errors)
        {
        }
    }
}