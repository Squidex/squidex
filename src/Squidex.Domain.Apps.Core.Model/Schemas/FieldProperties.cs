// ==========================================================================
//  FieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

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

        public abstract T Accept<T>(IFieldPropertiesVisitor<T> visitor);

        public abstract Field CreateField(long id, string name, Partitioning partitioning);
    }
}