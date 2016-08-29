// ==========================================================================
//  ModelField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.Tasks;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace PinkParrot.Core.Schema
{
    public abstract class ModelField
    {
        private readonly Guid id;
        private string name;
        private string hint;
        private string displayName;
        private bool isRequired;
        private bool isDisabled;
        private bool isHidden;

        public Guid Id
        {
            get { return id; }
        }

        public string Name
        {
            get { return name; }
        }

        public string Hint
        {
            get { return hint; }
        }

        public string DisplayName
        {
            get { return displayName; }
        }

        public bool IsRequired
        {
            get { return isRequired; }
        }

        public bool IsHidden
        {
            get { return isHidden; }
        }

        public bool IsDisabled
        {
            get { return isDisabled; }
        }

        protected ModelField(Guid id, string name)
        {
            Guard.NotEmpty(id, nameof(id));
            Guard.ValidSlug(name, nameof(name));

            this.id = id;

            this.name = name;
        }

        public ModelField Configure(PropertiesBag settings, ICollection<string> errors)
        {
            var clone = Clone();

            if (settings.Contains("Name"))
            {
                clone.name = settings["Name"].ToString();

                if (!clone.name.IsSlug())
                {
                    errors.Add("Field name must be a slug");
                }
            }

            if (settings.Contains("Hint"))
            {
                clone.hint = settings["Hint"].ToString()?.Trim() ?? string.Empty;
            }

            if (settings.Contains("DisplayName"))
            {
                clone.displayName = settings["DisplayName"].ToString()?.Trim() ?? string.Empty;
            }

            if (settings.Contains("IsRequired"))
            {
                try
                {
                    clone.isRequired = settings["IsRequired"].ToBoolean(CultureInfo.InvariantCulture);
                }
                catch (InvalidCastException)
                {
                    errors.Add("IsRequired is not a valid boolean");
                }
            }

            clone.ConfigureCore(settings, errors);

            return clone;
        }

        protected virtual void ConfigureCore(PropertiesBag settings, ICollection<string> errors)
        {
        }

        public Task ValidateAsync(PropertyValue property, ICollection<string> errors)
        {
            Guard.NotNull(property, nameof(property));
            
            if (isRequired && property.RawValue == null)
            {
                errors.Add("<Field> is required");
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
            if (isHidden)
            {
                throw new DomainValidationException($"The field '{name} is already hidden.");
            }

            var clone = Clone();

            clone.isHidden = true;

            return Clone();
        }

        public ModelField Show()
        {
            if (!isHidden)
            {
                throw new DomainValidationException($"The field '{name} is already visible.");
            }

            var clone = Clone();

            clone.isHidden = false;

            return Clone();
        }

        public ModelField Disable()
        {
            if (isDisabled)
            {
                throw new DomainValidationException($"The field '{name} is already disabled.");
            }

            var clone = Clone();

            clone.isDisabled = true;

            return clone;
        }

        public ModelField Enable()
        {
            if (!isDisabled)
            {
                throw new DomainValidationException($"The field '{name} is already enabled.");
            }

            var clone = Clone();

            clone.isDisabled = false;

            return clone;
        }

        protected abstract ModelField Clone();
    }
}