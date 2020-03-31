// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public static class ValidationTestExtensions
    {
        private static readonly NamedId<Guid> AppId = NamedId.Of(Guid.NewGuid(), "my-app");
        private static readonly NamedId<Guid> SchemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private static readonly IValidatorsFactory Factory = new DefaultValidatorsFactory();

        public static Task ValidateAsync(this IValidator validator, object? value, IList<string> errors,
            Schema? schema = null, ValidationMode mode = ValidationMode.Default, Func<ValidationContext, ValidationContext>? updater = null)
        {
            var context = CreateContext(schema, mode, updater);

            return validator.ValidateAsync(value, context, CreateFormatter(errors));
        }

        public static Task ValidateAsync(this IField field, object? value, IList<string> errors,
            Schema? schema = null, ValidationMode mode = ValidationMode.Default, Func<ValidationContext, ValidationContext>? updater = null)
        {
            var context = CreateContext(schema, mode, updater);

            var validators = Factory.CreateValueValidators(context, field, null!);

            return new FieldValidator(validators.ToArray(), field)
                .ValidateAsync(value, context, CreateFormatter(errors));
        }

        public static async Task ValidatePartialAsync(this NamedContentData data, PartitionResolver partitionResolver, IList<ValidationError> errors,
            Schema? schema = null, ValidationMode mode = ValidationMode.Default, Func<ValidationContext, ValidationContext>? updater = null)
        {
            var context = CreateContext(schema, mode, updater);

            var validator = new ContentValidator(partitionResolver, context, Enumerable.Repeat(Factory, 1));

            await validator.ValidateInputPartialAsync(data);

            foreach (var error in validator.Errors)
            {
                errors.Add(error);
            }
        }

        public static async Task ValidateAsync(this NamedContentData data, PartitionResolver partitionResolver, IList<ValidationError> errors,
            Schema? schema = null, ValidationMode mode = ValidationMode.Default, Func<ValidationContext, ValidationContext>? updater = null)
        {
            var context = CreateContext(schema, mode, updater);

            var validator = new ContentValidator(partitionResolver, context, Enumerable.Repeat(Factory, 1));

            await validator.ValidateInputAsync(data);

            foreach (var error in validator.Errors)
            {
                errors.Add(error);
            }
        }

        public static AddError CreateFormatter(IList<string> errors)
        {
            return (field, message) =>
            {
                if (field == null || !field.Any())
                {
                    errors.Add(message);
                }
                else
                {
                    errors.Add($"{field.ToPathString()}: {message}");
                }
            };
        }

        public static ValidationContext CreateContext(Schema? schema = null, ValidationMode mode = ValidationMode.Default, Func<ValidationContext, ValidationContext>? updater = null)
        {
            var context = new ValidationContext(
                AppId,
                SchemaId,
                schema ?? new Schema(SchemaId.Name),
                Guid.NewGuid(),
                mode);

            if (updater != null)
            {
                context = updater(context);
            }

            return context;
        }
    }
}
