// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class UpdateSchemaDto
    {
        /// <summary>
        /// Optional label for the editor.
        /// </summary>
        [LocalizedStringLength(100)]
        public string? Label { get; set; }

        /// <summary>
        /// Hints to describe the schema.
        /// </summary>
        [LocalizedStringLength(1000)]
        public string? Hints { get; set; }

        /// <summary>
        /// Tags for automation processes.
        /// </summary>
        public ReadOnlyCollection<string>? Tags { get; set; }

        public UpdateSchema ToCommand()
        {
            var properties = SimpleMapper.Map(this, new SchemaProperties());

            return new UpdateSchema { Properties = properties };
        }
    }
}
