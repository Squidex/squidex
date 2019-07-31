// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Visitors
{
    internal sealed class AdaptionVisitor : TransformVisitor<ClrValue>
    {
        private readonly Func<PropertyPath, PropertyPath> pathConverter;

        public AdaptionVisitor(Func<PropertyPath, PropertyPath> pathConverter)
        {
            this.pathConverter = pathConverter;
        }

        public override FilterNode<ClrValue> Visit(CompareFilter<ClrValue> nodeIn)
        {
            CompareFilter<ClrValue> result;

            var value = nodeIn.Value.Value;

            if (value is Instant &&
                !string.Equals(nodeIn.Path[0], "mt", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(nodeIn.Path[0], "ct", StringComparison.OrdinalIgnoreCase))
            {
                result = new CompareFilter<ClrValue>(pathConverter(nodeIn.Path), nodeIn.Operator, value.ToString());
            }
            else
            {
                result = new CompareFilter<ClrValue>(pathConverter(nodeIn.Path), nodeIn.Operator, nodeIn.Value);
            }

            if (result.Path.Count == 1 && result.Path[0] == "_id" && result.Value.Value is List<Guid> guidList)
            {
                result = new CompareFilter<ClrValue>(nodeIn.Path, nodeIn.Operator, guidList.Select(x => x.ToString()).ToList());
            }

            return result;
        }
    }
}
