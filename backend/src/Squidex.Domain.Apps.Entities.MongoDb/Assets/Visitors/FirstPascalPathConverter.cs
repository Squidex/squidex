// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors
{
    public sealed class FirstPascalPathConverter<TValue> : TransformVisitor<TValue>
    {
        private static readonly FirstPascalPathConverter<TValue> Instance = new FirstPascalPathConverter<TValue>();

        private FirstPascalPathConverter()
        {
        }

        public static FilterNode<TValue>? Transform(FilterNode<TValue> node)
        {
            return node.Accept(Instance);
        }

        public override FilterNode<TValue>? Visit(CompareFilter<TValue> nodeIn)
        {
            return new CompareFilter<TValue>(nodeIn.Path.ToFirstPascalCase(), nodeIn.Operator, nodeIn.Value);
        }
    }
}
