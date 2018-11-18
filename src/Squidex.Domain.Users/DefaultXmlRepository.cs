// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Users
{
    public sealed class DefaultXmlRepository : IXmlRepository
    {
        private readonly ISnapshotStore<State, string> store;

        [CollectionName("XmlRepository")]
        public sealed class State
        {
            public string Xml { get; set; }
        }

        public DefaultXmlRepository(ISnapshotStore<State, string> store)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            var result = new List<XElement>();

            store.ReadAllAsync((state, version) =>
            {
                result.Add(XElement.Parse(state.Xml));

                return TaskHelper.Done;
            }).Wait();

            return result;
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            store.WriteAsync(friendlyName, new State { Xml = element.ToString() }, EtagVersion.Any, EtagVersion.Any).Wait();
        }
    }
}
