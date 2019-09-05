﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

#pragma warning disable SA1028, IDE0004 // Code must not contain trailing whitespace

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class ContentValidator
    {
        private readonly Schema schema;
        private readonly PartitionResolver partitionResolver;
        private readonly ValidationContext context;
        private readonly ConcurrentBag<ValidationError> errors = new ConcurrentBag<ValidationError>();

        public IReadOnlyCollection<ValidationError> Errors
        {
            get { return errors; }
        }

        public ContentValidator(Schema schema, PartitionResolver partitionResolver, ValidationContext context)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            this.schema = schema;
            this.context = context;
            this.partitionResolver = partitionResolver;
        }

        private void AddError(IEnumerable<string> path, string message)
        {
            var pathString = path.ToPathString();

            errors.Add(new ValidationError(message, pathString));
        }

        public Task ValidatePartialAsync(NamedContentData data)
        {
            Guard.NotNull(data, nameof(data));

            var validator = CreateSchemaValidator(true);

            return validator.ValidateAsync(data, context, AddError);
        }

        public Task ValidateAsync(NamedContentData data)
        {
            Guard.NotNull(data, nameof(data));

            var validator = CreateSchemaValidator(false);

            return validator.ValidateAsync(data, context, AddError);
        }

        private IValidator CreateSchemaValidator(bool isPartial)
        {
            var fieldsValidators = new Dictionary<string, (bool IsOptional, IValidator Validator)>(schema.Fields.Count);

            foreach (var field in schema.Fields)
            {
                fieldsValidators[field.Name] = (!field.RawProperties.IsRequired, CreateFieldValidator(field, isPartial));
            }

            return new ObjectValidator<ContentFieldData>(fieldsValidators, isPartial, "field");
        }

        private IValidator CreateFieldValidator(IRootField field, bool isPartial)
        {
            var partitioning = partitionResolver(field.Partitioning);

            var fieldValidator = field.CreateValidator();
            var fieldsValidators = new Dictionary<string, (bool IsOptional, IValidator Validator)>();

            foreach (var partition in partitioning)
            {
                fieldsValidators[partition.Key] = (partition.IsOptional, fieldValidator);
            }

            return new AggregateValidator(
                field.CreateBagValidator()
                    .Union(Enumerable.Repeat(
                        new ObjectValidator<IJsonValue>(fieldsValidators, isPartial, TypeName(field)), 1)));
        }

        private static string TypeName(IRootField field)
        {
            var isLanguage = field.Partitioning.Equals(Partitioning.Language);

            return isLanguage ? "language" : "invariant value";
        }
    }
}
