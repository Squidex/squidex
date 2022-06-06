// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class ComponentInterfaceGraphType : InterfaceGraphType<JsonObject>
    {
        public ComponentInterfaceGraphType()
        {
            // The name is used for equal comparison. Therefore it is important to treat it as readonly.
            Name = "Component";

            AddField(ContentFields.SchemaId);

            Description = "The structure of all content types.";
        }
    }
}
