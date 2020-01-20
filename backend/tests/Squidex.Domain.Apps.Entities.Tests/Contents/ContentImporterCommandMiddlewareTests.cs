// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentImporterCommandMiddlewareTests
    {
        private readonly IServiceProvider serviceProvider = A.Fake<IServiceProvider>();
        private readonly ICommandBus commandBus = A.Dummy<ICommandBus>();
        private readonly ContentImporterCommandMiddleware sut;

        public ContentImporterCommandMiddlewareTests()
        {
            sut = new ContentImporterCommandMiddleware(serviceProvider);
        }

        [Fact]
        public async Task Should_do_nothing_if_datas_is_null()
        {
            var command = new CreateContents();

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.True(context.PlainResult is ImportResult);

            A.CallTo(() => serviceProvider.GetService(A<Type>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_do_nothing_if_datas_is_empty()
        {
            var command = new CreateContents { Datas = new List<NamedContentData>() };

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.True(context.PlainResult is ImportResult);

            A.CallTo(() => serviceProvider.GetService(A<Type>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_import_data()
        {
            var data1 = CreateData(1);
            var data2 = CreateData(2);

            var domainObject = A.Fake<ContentDomainObject>();

            A.CallTo(() => serviceProvider.GetService(typeof(ContentDomainObject)))
                .Returns(domainObject);

            var command = new CreateContents
            {
                Datas = new List<NamedContentData>
                {
                    data1,
                    data2
                }
            };

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            var result = context.Result<ImportResult>();

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result.Count(x => x.ContentId.HasValue && x.Exception == null));

            A.CallTo(() => domainObject.Setup(A<Guid>.Ignored))
                .MustHaveHappenedTwiceExactly();

            A.CallTo(() => domainObject.ExecuteAsync(A<CreateContent>.Ignored))
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_skip_exception()
        {
            var data1 = CreateData(1);
            var data2 = CreateData(2);

            var domainObject = A.Fake<ContentDomainObject>();

            var exception = new InvalidOperationException();

            A.CallTo(() => serviceProvider.GetService(typeof(ContentDomainObject)))
                .Returns(domainObject);

            A.CallTo(() => domainObject.ExecuteAsync(A<CreateContent>.That.Matches(x => x.Data == data1)))
                .Throws(exception);

            var command = new CreateContents
            {
                Datas = new List<NamedContentData>
                {
                    data1,
                    data2
                }
            };

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            var result = context.Result<ImportResult>();

            Assert.Equal(2, result.Count);
            Assert.Equal(1, result.Count(x => x.ContentId.HasValue && x.Exception == null));
            Assert.Equal(1, result.Count(x => !x.ContentId.HasValue && x.Exception == exception));
        }

        private static NamedContentData CreateData(int value)
        {
            return new NamedContentData()
                .AddField("value",
                    new ContentFieldData()
                        .AddJsonValue("iv", JsonValue.Create(value)));
        }
    }
}
