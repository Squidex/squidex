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
    internal sealed class AdaptionVisitor : TransformVisitor<ClrValue, AdaptionVisitor.Args>
    {
        private static readonly AdaptionVisitor Instance = new AdaptionVisitor();

        public readonly struct Args
        {
            public readonly DomainId AppId;

            public Args(DomainId appId)
            {
                AppId = appId;
            }
        }

        private AdaptionVisitor()
        {
        }

        public static FilterNode<ClrValue>? AdaptFilter(FilterNode<ClrValue> filter, DomainId appId)
        {
            var args = new Args(appId);

            return filter.Accept(Instance, args);
        }

        public override FilterNode<ClrValue> Visit(CompareFilter<ClrValue> nodeIn, Args args)
        {
            var result = nodeIn;

            var (path, _, value) = nodeIn;

            var clrValue = value.Value;

            if (string.Equals(path[0], "id", StringComparison.OrdinalIgnoreCase))
            {
                path = "_id";

                if (clrValue is List<string> idList)
                {
                    value = idList.Select(x => DomainId.Combine(args.AppId, DomainId.Create(x)).ToString()).ToList();
                }
                else if (clrValue is string id)
                {
                    value = DomainId.Combine(args.AppId, DomainId.Create(id)).ToString();
                }
                else if (clrValue is List<Guid> guidIdList)
                {
                    value = guidIdList.Select(x => DomainId.Combine(args.AppId, DomainId.Create(x)).ToString()).ToList();
                }
                else if (clrValue is Guid guidId)
                {
                    value = DomainId.Combine(args.AppId, DomainId.Create(guidId)).ToString();
                }
            }
            else
            {
                path = Adapt.MapPath(path);

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
