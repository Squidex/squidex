// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json.Linq;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class FieldProperties : NamedElementPropertiesBase
    {
        private bool isRequired;
        private bool isListField;
        private string placeholder;

        public bool IsRequired
        {
            get
            {
                return isRequired;
            }
            set
            {
                ThrowIfFrozen();

                isRequired = value;
            }
        }

        public bool IsListField
        {
            get
            {
                return isListField;
            }
            set
            {
                ThrowIfFrozen();

                isListField = value;
            }
        }

        public string Placeholder
        {
            get
            {
                return placeholder;
            }
            set
            {
                ThrowIfFrozen();

                placeholder = value;
            }
        }

        public abstract JToken GetDefaultValue();

        public abstract T Accept<T>(IFieldPropertiesVisitor<T> visitor);

        public virtual bool ShouldApplyDefaultValue(JToken value)
        {
            return value.IsNull();
        }
    }
}