// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.GenerateFilters;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.Scripting
{
    public class ScriptingCompleterTests
    {
        private readonly IScriptDescriptor scriptDescriptor1 = A.Fake<IScriptDescriptor>();
        private readonly IScriptDescriptor scriptDescriptor2 = A.Fake<IScriptDescriptor>();
        private readonly FilterSchema dataSchema;
        private readonly ScriptingCompleter sut;

        public ScriptingCompleterTests()
        {
            var schema =
                new Schema("simple")
                    .AddString(1, "my-invariant", Partitioning.Invariant)
                    .AddString(2, "my-localized", Partitioning.Language);

            dataSchema = schema.BuildDataSchema(LanguagesConfig.English.ToResolver(), ResolvedComponents.Empty);

            sut = new ScriptingCompleter(new[] { scriptDescriptor1, scriptDescriptor2 });
        }

        [Fact]
        public void Should_calls_descriptors()
        {
            sut.UsageTrigger();

            A.CallTo(() => scriptDescriptor1.Describe(A<AddDescription>._, A<ScriptScope>._))
                .MustHaveHappened();

            A.CallTo(() => scriptDescriptor2.Describe(A<AddDescription>._, A<ScriptScope>._))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_describe_content_script()
        {
            var result = sut.ContentScript(dataSchema);

            AssertCompletion(result,
                PresetUser("ctx.user"),
                new[]
                {
                    "ctx",
                    "ctx.appId",
                    "ctx.appName",
                    "ctx.contentId",
                    "ctx.data",
                    "ctx.data['my-invariant']",
                    "ctx.data['my-invariant'].iv",
                    "ctx.data['my-localized']",
                    "ctx.data['my-localized'].en",
                    "ctx.dataOld",
                    "ctx.dataOld['my-invariant']",
                    "ctx.dataOld['my-invariant'].iv",
                    "ctx.dataOld['my-localized']",
                    "ctx.dataOld['my-localized'].en",
                    "ctx.operation",
                    "ctx.permanent",
                    "ctx.schemaId",
                    "ctx.schemaName",
                    "ctx.status",
                    "ctx.statusOld"
                });
        }

        [Fact]
        public void Should_describe_content_trigger()
        {
            var result = sut.ContentTrigger(dataSchema);

            AssertCompletion(result,
                PresetActor("event.actor"),
                PresetUser("event.user"),
                new[]
                {
                    "event",
                    "event.appId.id",
                    "event.appId.name",
                    "event.created",
                    "event.createdBy",
                    "event.data",
                    "event.data['my-invariant']",
                    "event.data['my-invariant'].iv",
                    "event.data['my-localized']",
                    "event.data['my-localized'].en",
                    "event.dataOld",
                    "event.dataOld['my-invariant']",
                    "event.dataOld['my-invariant'].iv",
                    "event.dataOld['my-localized']",
                    "event.dataOld['my-localized'].en",
                    "event.id",
                    "event.lastModified",
                    "event.lastModifiedBy",
                    "event.name",
                    "event.newStatus",
                    "event.schemaId.id",
                    "event.schemaId.name",
                    "event.status",
                    "event.timestamp",
                    "event.type",
                    "event.version"
                });
        }

        [Fact]
        public void Should_describe_asset_script()
        {
            var result = sut.AssetScript();

            AssertCompletion(result,
                PresetUser("ctx.user"),
                new[]
                {
                    "ctx",
                    "ctx.appId",
                    "ctx.appName",
                    "ctx.asset",
                    "ctx.asset.fileHash",
                    "ctx.asset.fileName",
                    "ctx.asset.fileSize",
                    "ctx.asset.fileVersion",
                    "ctx.asset.isProtected",
                    "ctx.asset.metadata",
                    "ctx.asset.metadata.name",
                    "ctx.asset.mimeType",
                    "ctx.asset.parentId",
                    "ctx.asset.slug",
                    "ctx.asset.tags",
                    "ctx.assetId",
                    "ctx.command",
                    "ctx.command.fileHash",
                    "ctx.command.fileName",
                    "ctx.command.fileSize",
                    "ctx.command.isProtected",
                    "ctx.command.metadata",
                    "ctx.command.metadata.name",
                    "ctx.command.mimeType",
                    "ctx.command.parentId",
                    "ctx.command.parentPath",
                    "ctx.command.permanent",
                    "ctx.command.slug",
                    "ctx.command.tags",
                    "ctx.operation",
                });
        }

        [Fact]
        public void Should_describe_asset_trigger()
        {
            var result = sut.AssetTrigger();

            AssertCompletion(result,
                PresetActor("event.actor"),
                PresetUser("event.user"),
                new[]
                {
                    "event",
                    "event.appId.id",
                    "event.appId.name",
                    "event.assetType",
                    "event.created",
                    "event.createdBy",
                    "event.fileHash",
                    "event.fileName",
                    "event.fileSize",
                    "event.fileVersion",
                    "event.id",
                    "event.isImage",
                    "event.isProtected",
                    "event.lastModified",
                    "event.lastModifiedBy",
                    "event.metadata",
                    "event.metadata.name",
                    "event.mimeType",
                    "event.name",
                    "event.parentId",
                    "event.pixelHeight",
                    "event.pixelWidth",
                    "event.slug",
                    "event.timestamp",
                    "event.type",
                    "event.version"
                });
        }

        [Fact]
        public void Should_describe_comment_trigger()
        {
            var result = sut.CommentTrigger();

            AssertCompletion(result,
                PresetActor("event.actor"),
                PresetUser("event.user"),
                PresetUser("event.mentionedUser"),
                new[]
                {
                    "event",
                    "event.appId.id",
                    "event.appId.name",
                    "event.name",
                    "event.text",
                    "event.timestamp",
                    "event.type",
                    "event.url",
                    "event.version"
                });
        }

        [Fact]
        public void Should_describe_schema_trigger()
        {
            var result = sut.SchemaTrigger();

            AssertCompletion(result,
                PresetActor("event.actor"),
                PresetUser("event.user"),
                new[]
                {
                    "event",
                    "event.appId.id",
                    "event.appId.name",
                    "event.name",
                    "event.schemaId.id",
                    "event.schemaId.name",
                    "event.timestamp",
                    "event.type",
                    "event.version"
                });
        }

        [Fact]
        public void Should_describe_usage_trigger()
        {
            var result = sut.UsageTrigger();

            AssertCompletion(result,
                PresetActor("event.actor"),
                PresetUser("event.user"),
                new[]
                {
                    "event",
                    "event.appId.id",
                    "event.appId.name",
                    "event.callsCurrent",
                    "event.callsLimit",
                    "event.name",
                    "event.timestamp",
                    "event.type",
                    "event.version"
                });
        }

        private static void AssertCompletion(IReadOnlyList<ScriptingValue> result, params string[][] expected)
        {
            var allExpected = expected.SelectMany(x => x).ToArray();

            var paths = result.Select(x => x.Path).ToArray();

            foreach (var value in paths)
            {
                Assert.Contains(value, allExpected);
            }

            foreach (var value in allExpected)
            {
                Assert.Contains(value, paths);
            }
        }

        private static string[] PresetActor(string path)
        {
            return new[]
            {
                $"{path}",
                $"{path}.identifier",
                $"{path}.type"
            };
        }

        private static string[] PresetUser(string path)
        {
            return new[]
            {
                $"{path}",
                $"{path}.claims",
                $"{path}.claims.name",
                $"{path}.email",
                $"{path}.id",
                $"{path}.isClient",
                $"{path}.isUser",
            };
        }
    }
}
