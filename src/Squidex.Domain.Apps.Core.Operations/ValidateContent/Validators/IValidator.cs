// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public delegate void AddError(string field, string message);

    public delegate AddError CombineFields(string field, AddError formatter);

    public interface IValidator
    {
        Task ValidateAsync(object value, ValidationContext context, AddError addError);
    }
}
