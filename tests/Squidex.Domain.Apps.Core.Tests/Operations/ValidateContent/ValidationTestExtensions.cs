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
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;

namespace Squidex.Domain.Apps.Core.Operations.ValidateContent
{
    public static class ValidationTestExtensions
    {
        private static readonly Task<IReadOnlyList<Guid>> ValidReferences = Task.FromResult<IReadOnlyList<Guid>>(new List<Guid>());
        private static readonly Task<IReadOnlyList<IAssetInfo>> ValidAssets = Task.FromResult<IReadOnlyList<IAssetInfo>>(new List<IAssetInfo>());

        public static readonly ValidationContext ValidContext = new ValidationContext((x, y) => ValidReferences, x => ValidAssets);

        public static Task ValidateAsync(this IValidator validator, object value, IList<string> errors, ValidationContext context = null)
        {
            return validator.ValidateAsync(value, context ?? ValidContext, errors.Add);
        }

        public static Task ValidateOptionalAsync(this IValidator validator, object value, IList<string> errors, ValidationContext context = null)
        {
            return validator.ValidateAsync(value, (context ?? ValidContext).Optional(true), errors.Add);
        }

        public static Task ValidateAsync(this Field field, JToken value, IList<string> errors, ValidationContext context = null)
        {
            return field.ValidateAsync(value, context ?? ValidContext, errors.Add);
        }

        public static Task ValidateOptionalAsync(this Field field, JToken value, IList<string> errors, ValidationContext context = null)
        {
            return field.ValidateAsync(value, (context ?? ValidContext).Optional(true), errors.Add);
        }

        public static ValidationContext Assets(params IAssetInfo[] assets)
        {
            var actual = Task.FromResult<IReadOnlyList<IAssetInfo>>(assets.ToList());

            return new ValidationContext((x, y) => ValidReferences, x => actual);
        }

        public static ValidationContext InvalidReferences(Guid referencesIds)
        {
            var actual = Task.FromResult<IReadOnlyList<Guid>>(new List<Guid> { referencesIds });

            return new ValidationContext((x, y) => actual, x => ValidAssets);
        }
    }
}
