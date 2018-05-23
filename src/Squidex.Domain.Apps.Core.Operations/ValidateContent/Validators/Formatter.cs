// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public static class Formatter
    {
        public static ErrorFormatter Combine(string field, ErrorFormatter formatter)
        {
            return (innerField, message) =>
            {
                if (!string.IsNullOrWhiteSpace(innerField))
                {
                    formatter($"{field}.{innerField}", message);
                }
                else
                {
                    formatter(field, message);
                }
            };
        }
    }
}
