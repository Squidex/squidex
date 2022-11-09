// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Text;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.ValidateContent;

internal sealed class DefaultFieldValueValidatorsFactory : IFieldVisitor<IEnumerable<IValidator>, DefaultFieldValueValidatorsFactory.Args>
{
    private static readonly DefaultFieldValueValidatorsFactory Instance = new DefaultFieldValueValidatorsFactory();

    public record struct Args(ValidationContext Context, ValidatorFactory Factory);

    private DefaultFieldValueValidatorsFactory()
    {
    }

    public static IEnumerable<IValidator> CreateValidators(ValidationContext context, IField field, ValidatorFactory factory)
    {
        var args = new Args(context, factory);

        return field.Accept(Instance, args);
    }

    public IEnumerable<IValidator> Visit(IArrayField field, Args args)
    {
        var properties = field.Properties;

        var isRequired = IsRequired(properties, args.Context);

        if (isRequired || properties.MinItems != null || properties.MaxItems != null)
        {
            yield return new CollectionValidator(isRequired, properties.MinItems, properties.MaxItems);
        }

        if (properties.UniqueFields?.Count > 0)
        {
            yield return new UniqueObjectValuesValidator(properties.UniqueFields);
        }

        var nestedValidators = new Dictionary<string, (bool IsOptional, IValidator Validator)>(field.Fields.Count);

        foreach (var nestedField in field.Fields)
        {
            nestedValidators[nestedField.Name] = (false, args.Factory(nestedField));
        }

        yield return new CollectionItemValidator(new ObjectValidator<JsonValue>(nestedValidators, false, "field"));
    }

    public IEnumerable<IValidator> Visit(IField<AssetsFieldProperties> field, Args args)
    {
        yield break;
    }

    public IEnumerable<IValidator> Visit(IField<BooleanFieldProperties> field, Args args)
    {
        var properties = field.Properties;

        var isRequired = IsRequired(properties, args.Context);

        if (isRequired)
        {
            yield return new RequiredValidator();
        }
    }

    public IEnumerable<IValidator> Visit(IField<ComponentFieldProperties> field, Args args)
    {
        var properties = field.Properties;

        var isRequired = IsRequired(properties, args.Context);

        if (isRequired)
        {
            yield return new RequiredValidator();
        }

        yield return ComponentValidator(args.Factory);
    }

    public IEnumerable<IValidator> Visit(IField<ComponentsFieldProperties> field, Args args)
    {
        var properties = field.Properties;

        var isRequired = IsRequired(properties, args.Context);

        if (isRequired || properties.MinItems != null || properties.MaxItems != null)
        {
            yield return new CollectionValidator(isRequired, properties.MinItems, properties.MaxItems);
        }

        if (properties.UniqueFields?.Count > 0)
        {
            yield return new UniqueObjectValuesValidator(properties.UniqueFields);
        }

        yield return new CollectionItemValidator(ComponentValidator(args.Factory));
    }

    public IEnumerable<IValidator> Visit(IField<DateTimeFieldProperties> field, Args args)
    {
        var properties = field.Properties;

        var isRequired = IsRequired(properties, args.Context);

        if (isRequired)
        {
            yield return new RequiredValidator();
        }

        if (properties.MinValue != null || properties.MaxValue != null)
        {
            yield return new RangeValidator<Instant>(properties.MinValue, properties.MaxValue);
        }
    }

    public IEnumerable<IValidator> Visit(IField<GeolocationFieldProperties> field, Args args)
    {
        var properties = field.Properties;

        var isRequired = IsRequired(properties, args.Context);

        if (isRequired)
        {
            yield return new RequiredValidator();
        }
    }

    public IEnumerable<IValidator> Visit(IField<JsonFieldProperties> field, Args args)
    {
        var properties = field.Properties;

        var isRequired = IsRequired(properties, args.Context);

        if (isRequired)
        {
            yield return new RequiredValidator();
        }
    }

    public IEnumerable<IValidator> Visit(IField<NumberFieldProperties> field, Args args)
    {
        var properties = field.Properties;

        var isRequired = IsRequired(properties, args.Context);

        if (isRequired)
        {
            yield return new RequiredValidator();
        }

        if (properties.MinValue != null || properties.MaxValue != null)
        {
            yield return new RangeValidator<double>(properties.MinValue, properties.MaxValue);
        }

        if (properties.AllowedValues != null)
        {
            yield return new AllowedValuesValidator<double>(properties.AllowedValues);
        }
    }

    public IEnumerable<IValidator> Visit(IField<ReferencesFieldProperties> field, Args args)
    {
        yield break;
    }

    public IEnumerable<IValidator> Visit(IField<StringFieldProperties> field, Args args)
    {
        var properties = field.Properties;

        var isRequired = IsRequired(properties, args.Context);

        if (isRequired)
        {
            yield return new RequiredStringValidator(true);
        }

        if (properties.MinLength != null || properties.MaxLength != null)
        {
            yield return new StringLengthValidator(properties.MinLength, properties.MaxLength);
        }

        if (properties.MinCharacters != null ||
            properties.MaxCharacters != null ||
            properties.MinWords != null ||
            properties.MaxWords != null)
        {
            Func<string, string>? transform = null;

            switch (properties.ContentType)
            {
                case StringContentType.Markdown:
                    transform = MarkdownExtensions.Markdown2Text;
                    break;
                case StringContentType.Html:
                    transform = HtmlExtensions.Html2Text;
                    break;
            }

            yield return new StringTextValidator(transform,
               properties.MinCharacters,
               properties.MaxCharacters,
               properties.MinWords,
               properties.MaxWords);
        }

        if (!string.IsNullOrWhiteSpace(properties.Pattern))
        {
            yield return new PatternValidator(properties.Pattern, properties.PatternMessage);
        }

        if (properties.AllowedValues != null)
        {
            yield return new AllowedValuesValidator<string>(properties.AllowedValues);
        }
    }

    public IEnumerable<IValidator> Visit(IField<TagsFieldProperties> field, Args args)
    {
        var properties = field.Properties;

        var isRequired = IsRequired(properties, args.Context);

        if (isRequired || properties.MinItems != null || properties.MaxItems != null)
        {
            yield return new CollectionValidator(isRequired, properties.MinItems, properties.MaxItems);
        }

        if (properties.AllowedValues != null)
        {
            yield return new CollectionItemValidator(new AllowedValuesValidator<string>(properties.AllowedValues));
        }

        yield return new CollectionItemValidator(new RequiredStringValidator(true));
    }

    public IEnumerable<IValidator> Visit(IField<UIFieldProperties> field, Args args)
    {
        if (field is INestedField)
        {
            yield return NoValueValidator.Instance;
        }
    }

    private static bool IsRequired(FieldProperties properties, ValidationContext context)
    {
        var isRequired = properties.IsRequired;

        if (context.Action == ValidationAction.Publish)
        {
            isRequired = isRequired || properties.IsRequiredOnPublish;
        }

        return isRequired;
    }

    private static IValidator ComponentValidator(ValidatorFactory factory)
    {
        return new ComponentValidator(schema =>
        {
            var nestedValidators = new Dictionary<string, (bool IsOptional, IValidator Validator)>(schema.Fields.Count);

            foreach (var nestedField in schema.Fields)
            {
                nestedValidators[nestedField.Name] = (false, factory(nestedField));
            }

            return new ObjectValidator<JsonValue>(nestedValidators, false, "field");
        });
    }
}
