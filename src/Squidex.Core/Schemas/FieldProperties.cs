// ==========================================================================
//  FieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Core.Schemas
{
    public abstract class FieldProperties : NamedElementProperties, IValidatable
    {
        private bool isRequired;
        private bool isLocalizable;
        private bool isListField;
        private string placeholder;

        public bool IsRequired
        {
            get { return isRequired; }
            set
            {
                ThrowIfFrozen();

                isRequired = value;
            }
        }

        public bool IsLocalizable
        {
            get { return isLocalizable; }
            set
            {
                ThrowIfFrozen();

                isLocalizable = value;
            }
        }

        public bool IsListField
        {
            get { return isListField; }
            set
            {
                ThrowIfFrozen();

                isListField = value;
            }
        }

        public string Placeholder
        {
            get { return placeholder; }
            set
            {
                ThrowIfFrozen();

                placeholder = value;
            }
        }

        public void Validate(IList<ValidationError> errors)
        {
            foreach (var error in ValidateCore())
            {
                errors.Add(error);
            }
        }

        protected abstract IEnumerable<ValidationError> ValidateCore();
    }
}