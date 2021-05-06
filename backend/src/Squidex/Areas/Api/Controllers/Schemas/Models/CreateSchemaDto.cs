// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class CreateSchemaDto : UpsertSchemaDto
    {
        /// <summary>
        /// The name of the schema.
        /// </summary>
        [LocalizedRequired]
        [LocalizedRegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// Set to true to allow a single content item only.
        /// </summary>
        [Obsolete("Use Type property.")]
        public bool IsSingleton { get; set; }

        /// <summary>
        /// The type of the schema.
        /// </summary>
        public SchemaType Type { get; set; }

        public CreateSchema ToCommand()
        {
            var command = ToCommand(this, new CreateSchema());

#pragma warning disable CS0618 // Type or member is obsolete
            if (IsSingleton)
            {
                command.Type = SchemaType.Singleton;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            return command;
        }
    }
}
