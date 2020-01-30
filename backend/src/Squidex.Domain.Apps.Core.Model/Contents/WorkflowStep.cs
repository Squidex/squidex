// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

#pragma warning disable IDE0051 // Remove unused private members

namespace Squidex.Domain.Apps.Core.Contents
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class WorkflowStep
    {
        private static readonly IReadOnlyDictionary<Status, WorkflowTransition> EmptyTransitions = new Dictionary<Status, WorkflowTransition>();

        [IgnoreDuringEquals]
        public IReadOnlyDictionary<Status, WorkflowTransition> Transitions { get; }

        public string? Color { get; }

        public NoUpdate? NoUpdate { get; }

        public WorkflowStep(IReadOnlyDictionary<Status, WorkflowTransition>? transitions = null, string? color = null, NoUpdate? noUpdate = null)
        {
            Transitions = transitions ?? EmptyTransitions;

            Color = color;

            NoUpdate = noUpdate;
        }

        [CustomEqualsInternal]
        private bool CustomEquals(WorkflowStep other)
        {
            return Transitions.EqualsDictionary(other.Transitions);
        }

        [CustomGetHashCode]
        private int CustomHashCode()
        {
            return Transitions.DictionaryHashCode();
        }
    }
}
