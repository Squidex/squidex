// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class ReorderFieldsDto
    {
        /// <summary>
        /// The field ids in the target order.
        /// </summary>
        [LocalizedRequired]
        public long[] FieldIds { get; set; }

        public ReorderFields ToCommand(long? parentId = null)
        {
            return new ReorderFields { ParentFieldId = parentId, FieldIds = FieldIds };
        }
    }
}
