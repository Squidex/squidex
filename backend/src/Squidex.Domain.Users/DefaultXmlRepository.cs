// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Users;

public sealed class DefaultXmlRepository : IXmlRepository
{
    private readonly ISnapshotStore<State> store;

    [CollectionName("Identity_Xml")]
    public sealed class State
    {
        public string Xml { get; set; }

        public State()
        {
        }

        public State(XElement xml)
        {
            Xml = xml.ToString();
        }

        public XElement ToXml()
        {
            return XElement.Parse(Xml);
        }
    }

    public DefaultXmlRepository(ISnapshotStore<State> store)
    {
        this.store = store;
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        return GetAllElementsAsync().Result;
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        var state = new State(element);

        store.WriteAsync(new SnapshotWriteJob<State>(DomainId.Create(friendlyName), state, 0));
    }

    private async Task<IReadOnlyCollection<XElement>> GetAllElementsAsync()
    {
        return await store.ReadAllAsync().Select(x => x.Value.ToXml()).ToListAsync();
    }
}
