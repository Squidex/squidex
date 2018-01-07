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
    public abstract class Field : Cloneable<Field>
    {
        private readonly long fieldId;
        private readonly Partitioning partitioning;
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

        public Partitioning Partitioning
        {
            get { return partitioning; }
        }

        public abstract FieldProperties RawProperties { get; }

        protected Field(long id, string name, Partitioning partitioning)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNull(partitioning, nameof(partitioning));
            Guard.GreaterThan(id, 0, nameof(id));

            fieldId = id;
            fieldName = name;

            this.partitioning = partitioning;
        }

        [Pure]
        public Field Lock()
        {
            return Clone<Field>(clone =>
            {
                clone.isLocked = true;
            });
        }

        [Pure]
        public Field Hide()
        {
            return Clone(clone =>
            {
                clone.isHidden = true;
            });
        }

        [Pure]
        public Field Show()
        {
            return Clone(clone =>
            {
                clone.isHidden = false;
            });
        }

        [Pure]
        public Field Disable()
        {
            return Clone(clone =>
            {
                clone.isDisabled = true;
            });
        }

        [Pure]
        public Field Enable()
        {
            return Clone(clone =>
            {
                clone.isDisabled = false;
            });
        }

        public abstract Field Update(FieldProperties newProperties);

        public abstract T Accept<T>(IFieldVisitor<T> visitor);
    }
}