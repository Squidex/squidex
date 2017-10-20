// ==========================================================================
//  Field.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas.Validators;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public abstract class Field : CloneableBase
    {
        private readonly Lazy<List<IValidator>> validators;
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

        public IReadOnlyList<IValidator> Validators
        {
            get { return validators.Value; }
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

            validators = new Lazy<List<IValidator>>(() => new List<IValidator>(CreateValidators()));
        }

        protected abstract Field UpdateInternal(FieldProperties newProperties);

        protected abstract IEnumerable<IValidator> CreateValidators();

        public abstract object ConvertValue(JToken value);

        public Field Lock()
        {
            return Clone<Field>(clone =>
            {
                clone.isLocked = true;
            });
        }

        public Field Hide()
        {
            return Clone<Field>(clone =>
            {
                clone.isHidden = true;
            });
        }

        public Field Show()
        {
            return Clone<Field>(clone =>
            {
                clone.isHidden = false;
            });
        }

        public Field Disable()
        {
            return Clone<Field>(clone =>
            {
                clone.isDisabled = true;
            });
        }

        public Field Enable()
        {
            return Clone<Field>(clone =>
            {
                clone.isDisabled = false;
            });
        }

        public Field Update(FieldProperties newProperties)
        {
            return UpdateInternal(newProperties);
        }

        public abstract T Accept<T>(IFieldVisitor<T> visitor);
    }
}