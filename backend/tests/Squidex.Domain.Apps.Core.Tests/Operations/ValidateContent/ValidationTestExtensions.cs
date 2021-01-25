// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Log;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public delegate ValidationContext ValidationUpdater(ValidationContext context);

    public static class ValidationTestExtensions
    {
        private static readonly NamedId<DomainId> AppId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private static readonly NamedId<DomainId> SchemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private static readonly ISemanticLog Log = A.Fake<ISemanticLog>();
        private static readonly IValidatorsFactory Factory = new DefaultValidatorsFactory();

        public static Task ValidateAsync(this IValidator validator, object? value, IList<string> errors,
            Schema? schema = null,
            ValidationMode mode = ValidationMode.Default,
            ValidationUpdater? updater = null,
            ValidationAction action = ValidationAction.Upsert)
        {
            var context = CreateContext(schema, mode, updater, action);

            return validator.ValidateAsync(value, context, CreateFormatter(errors));
        }

        public static Task ValidateAsync(this IField field, object? value, IList<string> errors,
            Schema? schema = null,
            ValidationMode mode = ValidationMode.Default,
            ValidationUpdater? updater = null,
            IValidatorsFactory? factory = null,
            ValidationAction action = ValidationAction.Upsert)
        {
            var context = CreateContext(schema, mode, updater, action);

            var validators = Factories(factory).SelectMany(x => x.CreateValueValidators(context, field, null!)).ToArray();
            var validator = new AggregateValidator(validators, Log);

            return new FieldValidator(validator, field)
                .ValidateAsync(value, context, CreateFormatter(errors));
        }

        public static async Task ValidatePartialAsync(this ContentData data, PartitionResolver partitionResolver, IList<ValidationError> errors,
            Schema? schema = null,
            ValidationMode mode = ValidationMode.Default,
            ValidationUpdater? updater = null,
            IValidatorsFactory? factory = null,
            ValidationAction action = ValidationAction.Upsert)
        {
            var context = CreateContext(schema, mode, updater, action);

            var validator = new ContentValidator(partitionResolver, context, Factories(factory), Log);

            await validator.ValidateInputPartialAsync(data);

            foreach (var error in validator.Errors)
            {
                errors.Add(error);
            }
        }

        public static async Task ValidateAsync(this ContentData data, PartitionResolver partitionResolver, IList<ValidationError> errors,
            Schema? schema = null,
            ValidationMode mode = ValidationMode.Default,
            ValidationUpdater? updater = null,
            IValidatorsFactory? factory = null,
            ValidationAction action = ValidationAction.Upsert)
        {
            var context = CreateContext(schema, mode, updater, action);

            var validator = new ContentValidator(partitionResolver, context, Factories(factory), Log);

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

        private static IEnumerable<IValidatorsFactory> Factories(IValidatorsFactory? factory)
        {
            var result = Enumerable.Repeat(Factory, 1);

            if (factory != null)
            {
                result = result.Union(Enumerable.Repeat(factory, 1));
            }

            return result;
        }

        public static ValidationContext CreateContext(
            Schema? schema = null,
            ValidationMode mode = ValidationMode.Default,
            ValidationUpdater? updater = null,
            ValidationAction action = ValidationAction.Upsert)
        {
            var context = new ValidationContext(
                TestUtils.DefaultSerializer,
                AppId,
                SchemaId,
                schema ?? new Schema(SchemaId.Name),
                DomainId.NewGuid());

            context = context.WithMode(mode).WithAction(action);

            if (updater != null)
            {
                context = updater(context);
            }

            return context;
        }
    }
}
