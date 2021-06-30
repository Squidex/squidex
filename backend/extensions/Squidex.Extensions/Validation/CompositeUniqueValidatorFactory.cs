// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Contents.Repositories;

namespace Squidex.Extensions.Validation
{
    public sealed class CompositeUniqueValidatorFactory : IValidatorsFactory
    {
        private const string Prefix = "unique:";
        private readonly IContentRepository contentRepository;

        public CompositeUniqueValidatorFactory(IContentRepository contentRepository)
        {
            this.contentRepository = contentRepository;
        }

        public IEnumerable<IValidator> CreateContentValidators(ValidatorContext context, ValidatorFactory createFieldValidator)
        {
            foreach (var validatorTag in ValidatorTags(context.Schema.Properties.Tags))
            {
                yield return new CompositeUniqueValidator(validatorTag, contentRepository);
            }
        }

        private static IEnumerable<string> ValidatorTags(IEnumerable<string> tags)
        {
            if (tags == null)
            {
                yield break;
            }

            foreach (var tag in tags)
            {
                if (tag.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase) && tag.Length > Prefix.Length)
                {
                    yield return tag;
                }
            }
        }
    }
}
