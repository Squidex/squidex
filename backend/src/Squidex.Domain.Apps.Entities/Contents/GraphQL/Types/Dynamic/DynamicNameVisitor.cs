// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Dynamic
{
    internal sealed class DynamicNameVisitor : BaseSchemaNodeVisitor
    {
        private readonly ReservedNames typeNames;

        public DynamicNameVisitor(ReservedNames typeNames)
        {
            this.typeNames = typeNames;
        }

        public override void VisitObject(IObjectGraphType type, ISchema schema)
        {
            // The normal types are already conflict free. Therefore we only fix dynamic types.
            if (!IsDynamic(type))
            {
                return;
            }

            type.Name = typeNames[type.Name];
        }

        public static IGraphType MarkDynamic(IGraphType source)
        {
            return source.WithMetadata(nameof(IsDynamic), true);
        }

        public static bool IsDynamic(IGraphType source)
        {
            return source.GetMetadata<bool>(nameof(IsDynamic));
        }
    }
}
