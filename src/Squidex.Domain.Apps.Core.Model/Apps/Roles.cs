// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class Roles : DictionaryWrapper<string, Role>
    {
        public static readonly Roles Empty = new Roles();

        private Roles()
            : base(ImmutableDictionary<string, Role>.Empty)
        {
        }

        public Roles(ImmutableDictionary<string, Role> inner)
            : base(inner)
        {
        }

        [Pure]
        public Roles Add(string name)
        {
            var newRole = new Role(name);

            return new Roles(Inner.Add(name, newRole));
        }

        [Pure]
        public Roles Remove(string name)
        {
            return new Roles(Inner.Remove(name));
        }

        [Pure]
        public Roles Update(string name, params string[] permissions)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNull(permissions, nameof(permissions));

            if (!TryGetValue(name, out var role))
            {
                return this;
            }

            return new Roles(Inner.SetItem(name, role.Update(permissions)));
        }

        public static Roles CreateDefaults(string app)
        {
            return new Roles(
                new Dictionary<string, Role>
                {
                    [Role.Developer] = Role.CreateDeveloper(app),
                    [Role.Editor] = Role.CreateEditor(app),
                    [Role.Owner] = Role.CreateOwner(app),
                    [Role.Reader] = Role.CreateReader(app)
                }.ToImmutableDictionary());
        }
    }
}
