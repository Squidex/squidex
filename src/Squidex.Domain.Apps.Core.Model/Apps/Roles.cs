// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class Roles : ArrayDictionary<string, Role>
    {
        public static readonly Roles Empty = new Roles();

        private Roles()
        {
        }

        public Roles(KeyValuePair<string, Role>[] items)
            : base(items)
        {
        }

        [Pure]
        public Roles Remove(string name)
        {
            return new Roles(Without(name));
        }

        [Pure]
        public Roles Add(string name)
        {
            var newRole = new Role(name);

            if (ContainsKey(name))
            {
                throw new ArgumentException("Name already exists.", nameof(name));
            }

            return new Roles(With(name, newRole));
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

            return new Roles(With(name, role.Update(permissions)));
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
                }.ToArray());
        }
    }
}
