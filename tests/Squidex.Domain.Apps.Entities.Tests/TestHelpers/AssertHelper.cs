// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.TestHelpers
{
    public static class AssertHelper
    {
        public static void ShouldHaveSameEvents(this IEnumerable<Envelope<IEvent>> events, params IEvent[] others)
        {
            var source = events.Select(x => x.Payload).ToArray();

            source.Should().HaveSameCount(others);

            for (var i = 0; i < source.Length; i++)
            {
                var lhs = source[i];
                var rhs = others[i];

                lhs.ShouldBeSameEvent(rhs);
            }
        }

        public static void ShouldBeSameEvent(this IEvent lhs, IEvent rhs)
        {
            lhs.Should().BeOfType(rhs.GetType());

            ((object)lhs).ShouldBeEquivalentTo(rhs, o => o.IncludingAllDeclaredProperties());
        }

        public static void ShouldBeSameEventType(this IEvent lhs, IEvent rhs)
        {
            lhs.Should().BeOfType(rhs.GetType());
        }

        public static void ShouldBeEquivalent<T>(this T result, T value)
        {
            result.ShouldBeEquivalentTo(value, o => o.IncludingAllDeclaredProperties());
        }
    }
}
