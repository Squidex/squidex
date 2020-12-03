// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ValidationResult
    {
        public ValidationError[] Errors { get; set; }
    }
}
