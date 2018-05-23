// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public delegate void ErrorFormatter(string field, string message);

    public interface IValidator
    {
        Task ValidateAsync(object value, ValidationContext context, ErrorFormatter addError);
    }
}
