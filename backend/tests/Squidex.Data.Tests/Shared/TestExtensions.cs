// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions.Execution;
using FluentAssertions.Numeric;
using NodaTime;

namespace Squidex.Shared;

public static class TestExtensions
{
    public static ComparableTypeAssertions<Instant> BeCloseTo(this ComparableTypeAssertions<Instant> assertions, Instant expected, Duration precision,
        string because = "", params object[] becauseArgs)
    {
        if (assertions.Subject is not Instant instant)
        {
            throw new InvalidOperationException("Not an instant value.");
        }

        var difference = instant - expected;

        var absoluteTicks = Math.Abs(difference.TotalTicks);
        var absoluteDiff = Duration.FromTicks(absoluteTicks);

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(absoluteDiff <= precision)
            .FailWith("Expected {context:instant} to be within {0} of {1}{reason}, but it differed by {2}.",
                precision, expected, difference);

        return assertions;
    }
}
