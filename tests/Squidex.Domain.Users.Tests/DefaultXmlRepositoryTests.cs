// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using FakeItEasy;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Users
{
    public class DefaultXmlRepositoryTests
    {
        private readonly ISnapshotStore<DefaultXmlRepository.State, string> store = A.Fake<ISnapshotStore<DefaultXmlRepository.State, string>>();
        private readonly DefaultXmlRepository sut;

        public DefaultXmlRepositoryTests()
        {
            sut = new DefaultXmlRepository(store);
        }

        [Fact]
        public void Should_write_new_item_to_store_with_friendly_name()
        {
            sut.StoreElement(new XElement("a"), "friendly-name");

            A.CallTo(() => store.WriteAsync("friendly-name", A<DefaultXmlRepository.State>.That.Matches(x => x.Xml == "<a />"), EtagVersion.Any, EtagVersion.Any))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_return_items_from_store()
        {
            A.CallTo(() => store.ReadAllAsync(A<Func<DefaultXmlRepository.State, long, Task>>.Ignored))
                .Invokes((Func<DefaultXmlRepository.State, long, Task> callback) =>
                {
                    callback(new DefaultXmlRepository.State { Xml = "<a />" }, EtagVersion.Any);
                    callback(new DefaultXmlRepository.State { Xml = "<b />" }, EtagVersion.Any);
                    callback(new DefaultXmlRepository.State { Xml = "<c />" }, EtagVersion.Any);
                });

            var result = sut.GetAllElements().ToList();

            Assert.Equal("<a />", result[0].ToString());
            Assert.Equal("<b />", result[1].ToString());
            Assert.Equal("<c />", result[2].ToString());
        }
    }
}
