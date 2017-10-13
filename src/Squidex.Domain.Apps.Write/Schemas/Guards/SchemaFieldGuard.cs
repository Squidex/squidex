// ==========================================================================
//  SchemaFieldGuard.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Schemas.Guards
{
    public static class SchemaFieldGuard
    {
        public static void GuardValidSchemaFieldName(string name)
        {
            if (!name.IsSlug())
            {
                var error = new ValidationError("Name must be a valid slug", "Name");

                throw new ValidationException("Cannot add a new field.", error);
            }
        }

        public static void GuardCanDelete(Schema schema, long fieldId)
        {
            var field = GetFieldOrThrow(schema, fieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is locked.");
            }
        }

        public static void GuardCanHide(Schema schema, long fieldId)
        {
            var field = GetFieldOrThrow(schema, fieldId);

            if (field.IsHidden)
            {
                throw new DomainException("Schema field is already hidden.");
            }
        }

        public static void GuardCanShow(Schema schema, long fieldId)
        {
            var field = GetFieldOrThrow(schema, fieldId);

            if (!field.IsHidden)
            {
                throw new DomainException("Schema field is already visible.");
            }
        }

        public static void GuardCanDisable(Schema schema, long fieldId)
        {
            var field = GetFieldOrThrow(schema, fieldId);

            if (field.IsDisabled)
            {
                throw new DomainException("Schema field is already disabled.");
            }
        }

        public static void GuardCanEnable(Schema schema, long fieldId)
        {
            var field = GetFieldOrThrow(schema, fieldId);

            if (!field.IsDisabled)
            {
                throw new DomainException("Schema field is already enabled.");
            }
        }

        public static void GuardCanLock(Schema schema, long fieldId)
        {
            var field = GetFieldOrThrow(schema, fieldId);

            if (field.IsLocked)
            {
                throw new DomainException("Schema field is already locked.");
            }
        }

        private static Field GetFieldOrThrow(Schema schema, long fieldId)
        {
            if (!schema.FieldsById.TryGetValue(fieldId, out var field))
            {
                throw new DomainObjectNotFoundException(fieldId.ToString(), "Fields", typeof(Field));
            }

            return field;
        }

        private static IEnumerable<ValidationError> ValidateProperties(FieldProperties properties)
        {
            switch (properties)
            {
                case AssetsFieldProperties a:
                {
                    if (a.MaxItems.HasValue && a.MinItems.HasValue && a.MinItems.Value >= a.MaxItems.Value)
                    {
                        yield return new ValidationError("Max items must be greater than min items",
                            nameof(a.MinItems),
                            nameof(a.MaxItems));
                    }

                    break;
                }

                case BooleanFieldProperties b:
                {
                    if (!b.Editor.IsEnumValue())
                    {
                        yield return new ValidationError("Editor is not a valid value",
                            nameof(b.Editor));
                    }

                    break;
                }

                case GeolocationFieldProperties g:
                {
                    if (!g.Editor.IsEnumValue())
                    {
                        yield return new ValidationError("Editor is not a valid value",
                            nameof(g.Editor));
                    }

                    break;
                }

                case ReferencesFieldProperties r:
                {
                    if (r.MaxItems.HasValue && r.MinItems.HasValue && r.MinItems.Value >= r.MaxItems.Value)
                    {
                        yield return new ValidationError("Max items must be greater than min items",
                            nameof(r.MinItems),
                            nameof(r.MaxItems));
                    }

                    break;
                }

                case DateTimeFieldProperties d:
                {
                    if (!d.Editor.IsEnumValue())
                    {
                        yield return new ValidationError("Editor is not a valid value", 
                            nameof(d.Editor));
                    }

                    if (d.DefaultValue.HasValue && d.MinValue.HasValue && d.DefaultValue.Value < d.MinValue.Value)
                    {
                        yield return new ValidationError("Default value must be greater than min value",
                            nameof(d.DefaultValue));
                    }

                    if (d.DefaultValue.HasValue && d.MaxValue.HasValue && d.DefaultValue.Value > d.MaxValue.Value)
                    {
                        yield return new ValidationError("Default value must be less than max value",
                            nameof(d.DefaultValue));
                    }

                    if (d.MaxValue.HasValue && d.MinValue.HasValue && d.MinValue.Value >= d.MaxValue.Value)
                    {
                        yield return new ValidationError("Max value must be greater than min value",
                            nameof(d.MinValue),
                            nameof(d.MaxValue));
                    }

                        if (d.CalculatedDefaultValue.HasValue)
                    {
                        if (!d.CalculatedDefaultValue.Value.IsEnumValue())
                        {
                            yield return new ValidationError("Calculated default value is not valid",
                                nameof(d.CalculatedDefaultValue));
                        }

                        if (d.DefaultValue.HasValue)
                        {
                            yield return new ValidationError("Calculated default value and default value cannot be used together",
                                nameof(d.CalculatedDefaultValue),
                                nameof(d.DefaultValue));
                        }
                    }

                    break;
                }

                case NumberFieldProperties n:
                {
                    if (!n.Editor.IsEnumValue())
                    {
                        yield return new ValidationError("Editor is not a valid value", 
                            nameof(n.Editor));
                    }

                    if ((n.Editor == NumberFieldEditor.Radio || n.Editor == NumberFieldEditor.Dropdown) && (n.AllowedValues == null || n.AllowedValues.Count == 0))
                    {
                        yield return new ValidationError("Radio buttons or dropdown list need allowed values",
                            nameof(n.AllowedValues));
                    }

                    if (n.DefaultValue.HasValue && n.MinValue.HasValue && n.DefaultValue.Value < n.MinValue.Value)
                    {
                        yield return new ValidationError("Default value must be greater than min value",
                            nameof(n.DefaultValue));
                    }

                    if (n.DefaultValue.HasValue && n.MaxValue.HasValue && n.DefaultValue.Value > n.MaxValue.Value)
                    {
                        yield return new ValidationError("Default value must be less than max value",
                            nameof(n.DefaultValue));
                    }

                    if (n.MaxValue.HasValue && n.MinValue.HasValue && n.MinValue.Value >= n.MaxValue.Value)
                    {
                        yield return new ValidationError("Max value must be greater than min value",
                            nameof(n.MinValue),
                            nameof(n.MaxValue));
                    }

                        if (n.AllowedValues != null && n.AllowedValues.Count > 0 && (n.MinValue.HasValue || n.MaxValue.HasValue))
                    {
                        yield return new ValidationError("Either allowed values or min and max value can be defined",
                            nameof(n.AllowedValues),
                            nameof(n.MinValue),
                            nameof(n.MaxValue));
                    }

                    break;
                }

                case StringFieldProperties s:
                {
                    if (!s.Editor.IsEnumValue())
                    {
                        yield return new ValidationError("Editor is not a valid value",
                            nameof(s.Editor));
                    }

                    if ((s.Editor == StringFieldEditor.Radio || s.Editor == StringFieldEditor.Dropdown) && (s.AllowedValues == null || s.AllowedValues.Count == 0))
                    {
                        yield return new ValidationError("Radio buttons or dropdown list need allowed values",
                            nameof(s.AllowedValues));
                    }

                    if (s.Pattern != null && !s.Pattern.IsValidRegex())
                    {
                        yield return new ValidationError("Pattern is not a valid expression",
                            nameof(s.Pattern));
                    }

                    if (s.MaxLength.HasValue && s.MinLength.HasValue && s.MinLength.Value >= s.MaxLength.Value)
                    {
                        yield return new ValidationError("Max length must be greater than min length",
                            nameof(s.MinLength),
                            nameof(s.MaxLength));
                    }

                    if (s.AllowedValues != null && s.AllowedValues.Count > 0 && (s.MinLength.HasValue || s.MaxLength.HasValue))
                    {
                        yield return new ValidationError("Either allowed values or min and max length can be defined",
                            nameof(s.AllowedValues),
                            nameof(s.MinLength),
                            nameof(s.MaxLength));
                    }

                    break;
                }
            }
        }
    }
}
