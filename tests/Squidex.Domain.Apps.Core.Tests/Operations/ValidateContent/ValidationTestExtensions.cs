﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public static class ValidationTestExtensions
    {
        private static readonly Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> EmptyReferences = Task.FromResult<IReadOnlyList<(Guid SchemaId, Guid Id)>>(new List<(Guid SchemaId, Guid Id)>());
        private static readonly Task<IReadOnlyList<IAssetInfo>> EmptyAssets = Task.FromResult<IReadOnlyList<IAssetInfo>>(new List<IAssetInfo>());

        public static readonly ValidationContext ValidContext = new ValidationContext(Guid.NewGuid(), Guid.NewGuid(),
            (x, y) => EmptyReferences,
            (x) => EmptyReferences,
            (x) => EmptyAssets);

        public static Task ValidateAsync(this IValidator validator, object value, IList<string> errors, ValidationContext context = null)
        {
            return validator.ValidateAsync(value,
                CreateContext(context),
                CreateFormatter(errors));
        }

        public static Task ValidateOptionalAsync(this IValidator validator, object value, IList<string> errors, ValidationContext context = null)
        {
            return validator.ValidateAsync(
                value,
                CreateContext(context).Optional(true),
                CreateFormatter(errors));
        }

        public static Task ValidateAsync(this IField field, object value, IList<string> errors, ValidationContext context = null)
        {
            return new FieldValidator(FieldValueValidatorsFactory.CreateValidators(field).ToArray(), field)
                .ValidateAsync(
                    value,
                    CreateContext(context),
                    CreateFormatter(errors));
        }

        private static AddError CreateFormatter(IList<string> errors)
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

        private static ValidationContext CreateContext(ValidationContext context)
        {
            return context ?? ValidContext;
        }

        public static ValidationContext Assets(params IAssetInfo[] assets)
        {
            var actual = Task.FromResult<IReadOnlyList<IAssetInfo>>(assets.ToList());

            return new ValidationContext(Guid.NewGuid(), Guid.NewGuid(), (x, y) => EmptyReferences, x => EmptyReferences, x => actual);
        }

        public static ValidationContext References(params (Guid Id, Guid SchemaId)[] referencesIds)
        {
            var actual = Task.FromResult<IReadOnlyList<(Guid Id, Guid SchemaId)>>(referencesIds.ToList());

            return new ValidationContext(Guid.NewGuid(), Guid.NewGuid(), (x, y) => actual, x => actual, x => EmptyAssets);
        }
    }
}
