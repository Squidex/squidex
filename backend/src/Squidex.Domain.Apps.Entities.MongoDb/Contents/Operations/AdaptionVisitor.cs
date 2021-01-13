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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class AdaptionVisitor : TransformVisitor<ClrValue>
    {
        private readonly Func<PropertyPath, PropertyPath> pathConverter;
        private readonly DomainId appId;

        public AdaptionVisitor(Func<PropertyPath, PropertyPath> pathConverter, DomainId appId)
        {
            this.pathConverter = pathConverter;

            this.appId = appId;
        }

        public override FilterNode<ClrValue> Visit(CompareFilter<ClrValue> nodeIn)
        {
            var result = nodeIn;

            var (path, op, value) = nodeIn;

            var clrValue = value.Value;

            if (string.Equals(path[0], "id", StringComparison.OrdinalIgnoreCase))
            {
                path = "_id";

                if (clrValue is List<string> idList)
                {
                    value = idList.Select(x => DomainId.Combine(appId, DomainId.Create(x)).ToString()).ToList();
                }
                else if (clrValue is string id)
                {
                    value = DomainId.Combine(appId, DomainId.Create(id)).ToString();
                }
                else if (clrValue is List<Guid> guidIdList)
                {
                    value = guidIdList.Select(x => DomainId.Combine(appId, DomainId.Create(x)).ToString()).ToList();
                }
                else if (clrValue is Guid guidId)
                {
                    value = DomainId.Combine(appId, DomainId.Create(guidId)).ToString();
                }
            }
            else
            {
                path = pathConverter(path);

                if (clrValue is List<Guid> guidList)
                {
                    value = guidList.Select(x => x.ToString()).ToList();
                }
                else if (clrValue is Guid guid)
                {
                    value = guid.ToString();
                }
                else if (clrValue is Instant &&
                    !string.Equals(path[0], "mt", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(path[0], "ct", StringComparison.OrdinalIgnoreCase))
                {
                    value = clrValue.ToString();
                }
            }

            return result with { Path = path, Value = value };
        }
    }
}
