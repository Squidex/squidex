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
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Core.Operations.EventSynchronization
{
    public static class AssertHelper
    {
        public static void ShouldHaveSameEvents(this IEnumerable<IEvent> events, params IEvent[] others)
        {
            var source = events.ToArray();

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

            ((object)lhs).Should().BeEquivalentTo(rhs, o => o.IncludingAllRuntimeProperties().Excluding(x => x.SelectedMemberPath == "Properties.IsFrozen"));
        }

        public static void ShouldBeSameEventType(this IEvent lhs, IEvent rhs)
        {
            lhs.Should().BeOfType(rhs.GetType());
        }

        public static void ShouldBeEquivalent<T>(this J<T> lhs, T rhs)
        {
            lhs.Value.Should().BeEquivalentTo(rhs, o => o.IncludingProperties());
        }
    }
}
