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

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
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

            var path = pathConverter(nodeIn.Path);

            var value = nodeIn.Value.Value;

            if (value is Instant &&
                !string.Equals(path[0], "mt", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(path[0], "ct", StringComparison.OrdinalIgnoreCase))
            {
                result = new CompareFilter<ClrValue>(path, nodeIn.Operator, value.ToString());
            }
            else
            {
                result = new CompareFilter<ClrValue>(path, nodeIn.Operator, nodeIn.Value);
            }

            if (value is List<Guid> guidList)
            {
                result = new CompareFilter<ClrValue>(path, nodeIn.Operator, guidList.Select(x => x.ToString()).ToList());
            }
            else if (value is Guid guid)
            {
                result = new CompareFilter<ClrValue>(path, nodeIn.Operator, guid.ToString());
            }

            return result;
        }
    }
}
