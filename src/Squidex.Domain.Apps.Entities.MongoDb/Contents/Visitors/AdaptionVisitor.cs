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
    internal sealed class AdaptionVisitor : TransformVisitor
    {
        private readonly Func<IReadOnlyList<string>, IReadOnlyList<string>> pathConverter;

        public AdaptionVisitor(Func<IReadOnlyList<string>, IReadOnlyList<string>> pathConverter)
        {
            this.pathConverter = pathConverter;
        }

        public override FilterNode Visit(FilterComparison nodeIn)
        {
            FilterComparison result;

            var value = nodeIn.Rhs.Value;

            if (value is Instant &&
                !string.Equals(nodeIn.Lhs[0], "mt", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(nodeIn.Lhs[0], "ct", StringComparison.OrdinalIgnoreCase))
            {
                result = new FilterComparison(pathConverter(nodeIn.Lhs), nodeIn.Operator, new FilterValue(value.ToString()));
            }
            else
            {
                result = new FilterComparison(pathConverter(nodeIn.Lhs), nodeIn.Operator, nodeIn.Rhs);
            }

            if (result.Lhs.Count == 1 && result.Lhs[0] == "_id" && result.Rhs.Value is List<Guid> guidList)
            {
                result = new FilterComparison(nodeIn.Lhs, nodeIn.Operator, new FilterValue(guidList.Select(x => x.ToString()).ToList()));
            }

            return result;
        }
    }
}
