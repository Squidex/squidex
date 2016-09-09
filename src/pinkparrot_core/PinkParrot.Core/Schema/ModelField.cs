// ==========================================================================
//  ModelField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.Tasks;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace PinkParrot.Core.Schema
{
    public abstract class ModelField
    {
        private readonly long id;
        private bool isDisabled;
        private bool isHidden;

        public long Id
        {
            get { return id; }
        }

        public abstract ModelFieldProperties RawProperties { get; }

        public abstract string Name { get; }

        public abstract string Label { get; }

        public abstract string Hints { get; }

        public abstract bool IsRequired { get; }

        public bool IsHidden
        {
            get { return isHidden; }
        }

        public bool IsDisabled
        {
            get { return isDisabled; }
        }

        protected ModelField(long id)
        {
            Guard.GreaterThan(id, 0, nameof(id));

            this.id = id;
        }

        public abstract ModelField Configure(ModelFieldProperties newProperties);

        public Task ValidateAsync(PropertyValue property, ICollection<string> errors)
        {
            Guard.NotNull(property, nameof(property));
            
            if (IsRequired && property.RawValue == null)
            {
                errors.Add("Field is required");
            }

            if (property.RawValue == null)
            {
                return TaskHelper.Done;
            }

            return ValidateCoreAsync(property, errors);
        }

        protected virtual Task ValidateCoreAsync(PropertyValue property, ICollection<string> errors)
        {
            return TaskHelper.Done;
        }

        public ModelField Hide()
        {
            return Update<ModelField>(clone => clone.isHidden = true);
        }

        public ModelField Show()
        {
            return Update<ModelField>(clone => clone.isHidden = false);
        }

        public ModelField Disable()
        {
            return Update<ModelField>(clone => clone.isDisabled = true);
        }

        public ModelField Enable()
        {
            return Update<ModelField>(clone => clone.isDisabled = false);
        }

        protected T Update<T>(Action<T> updater) where T : ModelField
        {
            var clone = (T)Clone();

            updater(clone);

            return clone;
        }

        protected abstract ModelField Clone();
    }
}