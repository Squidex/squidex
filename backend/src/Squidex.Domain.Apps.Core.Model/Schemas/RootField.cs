// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class RootField : FieldBase, IRootField
    {
        public Partitioning Partitioning { get; }

        public bool IsLocked { get; private set; }

        public bool IsHidden { get; private set; }

        public bool IsDisabled { get; private set; }

        public abstract FieldProperties RawProperties { get; }

        protected RootField(long id, string name, Partitioning partitioning, IFieldSettings? settings = null)
            : base(id, name)
        {
            Guard.NotNull(partitioning, nameof(partitioning));

            Partitioning = partitioning;

            if (settings != null)
            {
                IsLocked = settings.IsLocked;
                IsHidden = settings.IsHidden;
                IsDisabled = settings.IsDisabled;
            }
        }

        [Pure]
        public RootField Lock()
        {
            if (IsLocked)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.IsLocked = true;
            });
        }

        [Pure]
        public RootField Hide()
        {
            if (IsHidden)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.IsHidden = true;
            });
        }

        [Pure]
        public RootField Show()
        {
            if (!IsHidden)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.IsHidden = false;
            });
        }

        [Pure]
        public RootField Disable()
        {
            if (IsDisabled)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.IsDisabled = true;
            });
        }

        [Pure]
        public RootField Enable()
        {
            if (!IsDisabled)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.IsDisabled = false;
            });
        }

        public abstract T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, TArgs args);

        public abstract RootField Update(FieldProperties newProperties);

        protected RootField Clone(Action<RootField> updater)
        {
            var clone = (RootField)MemberwiseClone();

            updater(clone);

            return clone;
        }
    }
}
