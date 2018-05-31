// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class NestedField : Cloneable<NestedField>, INestedField
    {
        private readonly long fieldId;
        private readonly string fieldName;
        private bool isDisabled;
        private bool isHidden;
        private bool isLocked;

        public long Id
        {
            get { return fieldId; }
        }

        public string Name
        {
            get { return fieldName; }
        }

        public bool IsLocked
        {
            get { return isLocked; }
        }

        public bool IsHidden
        {
            get { return isHidden; }
        }

        public bool IsDisabled
        {
            get { return isDisabled; }
        }

        public abstract FieldProperties RawProperties { get; }

        protected NestedField(long id, string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.GreaterThan(id, 0, nameof(id));

            fieldId = id;
            fieldName = name;
        }

        [Pure]
        public NestedField Lock()
        {
            return Clone(clone =>
            {
                clone.isLocked = true;
            });
        }

        [Pure]
        public NestedField Hide()
        {
            return Clone(clone =>
            {
                clone.isHidden = true;
            });
        }

        [Pure]
        public NestedField Show()
        {
            return Clone(clone =>
            {
                clone.isHidden = false;
            });
        }

        [Pure]
        public NestedField Disable()
        {
            return Clone(clone =>
            {
                clone.isDisabled = true;
            });
        }

        [Pure]
        public NestedField Enable()
        {
            return Clone(clone =>
            {
                clone.isDisabled = false;
            });
        }

        public abstract T Accept<T>(IFieldVisitor<T> visitor);

        public abstract NestedField Update(FieldProperties newProperties);
    }
}