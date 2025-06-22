// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public sealed class NotChangedValidator(IRootField field, ContentData previousData) : IValidator
{
    public void Validate(object? value, ValidationContext context)
    {
        var previousFieldData =
            previousData.GetValueOrDefault(field.Name);

        var newFieldData =
            value as ContentFieldData;

        var partitions = context.Root.App.PartitionResolver()(field.Partitioning);

        foreach (var partition in partitions.AllKeys)
        {
            var previousLanguageValue =
                previousFieldData?.GetValueOrDefault(partition);

            var newLanguageValue =
                newFieldData?.GetValueOrDefault(partition);

            if (!Equals(previousLanguageValue, newLanguageValue))
            {
                var path = context.Path.Enqueue(partition);

                context.AddError(T.Get("contents.validation.createOnly"), path);
            }
        }
    }
}
